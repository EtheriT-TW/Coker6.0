using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.ArticleImports;

// 匯入的背景第一段：每 5 列一組，先下載這幾列的 Drive 圖片，再請 AI 把資料填進樣板欄位。
// 結果存回 ArticleImportRows（Ready／Failed），等使用者在預覽頁確認。
public sealed class ArticleImportProcessor(
    CokerDbContext db,
    IArticleImportImageStore images,
    ArticleImportAiFiller filler)
{
    public const int MaxImagesPerRow = 12;

    public async Task<ArticleImportScanResultDto> ProcessAsync(
        long websiteId,
        long userId,
        long batchId,
        Func<int, string, Task> reportProgress,
        Func<Task<bool>> isCancelRequested,
        CancellationToken token)
    {
        var batch = await db.ArticleImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && b.FK_WebsiteId == websiteId && !b.IsDeleted, token)
            ?? throw new InvalidOperationException("找不到匯入批次。");
        if (!ArticleTemplateCatalog.Exists(batch.TemplateKey))
            throw new InvalidOperationException($"文章樣板「{batch.TemplateKey}」已不存在。");
        var template = ArticleTemplateCatalog.Get(batch.TemplateKey);

        var pending = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batchId && !r.IsDeleted && r.Status == ArticleImportRowStatusEnum.Pending)
            .OrderBy(r => r.RowNumber)
            .Select(r => r.Id)
            .ToListAsync(token);

        var done = 0;
        foreach (var chunk in pending.Chunk(ArticleImportAiFiller.ChunkSize))
        {
            if (await isCancelRequested())
                throw new OperationCanceledException("已取消文章資料匯入。");

            var rows = await db.ArticleImportRows
                .Where(r => chunk.Contains(r.Id))
                .OrderBy(r => r.RowNumber)
                .ToListAsync(token);

            var aiRows = new List<ArticleImportAiRow>();
            foreach (var row in rows)
            {
                aiRows.Add(await PrepareRowAsync(row, token));
                Touch(row, userId);
                // 每列下載完就存，取消或失敗時才找得到要刪的檔案。
                await db.SaveChangesAsync(token);
            }

            var results = await filler.FillAsync(template, aiRows, token);
            foreach (var (row, result) in rows.Zip(results))
            {
                if (result.Fields != null)
                {
                    row.Status = ArticleImportRowStatusEnum.Ready;
                    row.FieldDataJson = ArticleFieldValues.Serialize(result.Fields);
                    row.Error = null;
                }
                else
                {
                    row.Status = ArticleImportRowStatusEnum.Failed;
                    row.Error = Cut(result.Error ?? "AI 處理失敗。", 1000);
                }
                Touch(row, userId);
            }
            await db.SaveChangesAsync(token);
            db.ChangeTracker.Clear();

            done += rows.Count;
            await reportProgress(5 + 90 * done / pending.Count, $"AI 整理資料中（{done}/{pending.Count} 列）");
        }

        var counts = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batchId && !r.IsDeleted)
            .GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(token);
        return new ArticleImportScanResultDto(
            batchId,
            counts.Where(c => c.Key == ArticleImportRowStatusEnum.Ready).Sum(c => c.Count),
            counts.Where(c => c.Key == ArticleImportRowStatusEnum.Failed).Sum(c => c.Count));
    }

    // 下載這列所有 Drive 圖片，原始資料裡的連結換成 image:N 代號（下載失敗的換成說明文字）。
    private async Task<ArticleImportAiRow> PrepareRowAsync(ArticleImportRow row, CancellationToken token)
    {
        var source = ParseSource(row.SourceJson);
        var records = new List<ArticleImportImageRecord>();
        var tokens = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (column, value) in source)
        {
            foreach (var fileId in ArticleImportDriveLinks.FindFileIds(value))
            {
                if (tokens.ContainsKey(fileId)) continue;
                if (records.Count >= MaxImagesPerRow)
                {
                    tokens[fileId] = "（圖片太多，未下載）";
                    continue;
                }

                var name = $"image:{records.Count + 1}";
                try
                {
                    var stored = await images.ImportAsync(fileId, token);
                    records.Add(new ArticleImportImageRecord(name, column, stored.FileName, stored.FileId, stored.Path, stored.Created, null));
                    tokens[fileId] = name;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // 存檔失敗時上傳服務可能留下未存的資料（例如新增到一半的檔案紀錄），清掉避免跟著這列一起存。
                    foreach (var entry in db.ChangeTracker.Entries()
                        .Where(e => e.Entity is not ArticleImportRow
                            && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                        .ToList())
                        entry.State = EntityState.Detached;
                    records.Add(new ArticleImportImageRecord(name, column, $"drive-{fileId}", null, null, false, Cut(ex.Message, 200)));
                    tokens[fileId] = "（圖片無法下載）";
                }
            }
        }

        row.DownloadedFilesJson = records.Count == 0 ? null : ArticleImportRowImages.Serialize(records);
        var data = source.ToDictionary(
            e => e.Key,
            e => ArticleImportDriveLinks.Replace(e.Value, id => tokens.GetValueOrDefault(id, "（圖片）")),
            StringComparer.Ordinal);
        var available = records
            .Where(r => r.Error == null && !string.IsNullOrEmpty(r.Path))
            .Select(r => new ArticleImportAiImage(r.Token, r.Column, r.FileName, r.Path!))
            .ToList();
        return new ArticleImportAiRow(row.RowNumber, data, available);
    }

    internal static Dictionary<string, string> ParseSource(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string?>>(json);
            return (parsed ?? []).Where(e => !string.IsNullOrWhiteSpace(e.Value))
                .ToDictionary(e => e.Key, e => e.Value!, StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static void Touch(ArticleImportRow row, long userId)
    {
        row.LastModificationTime = DateTime.Now;
        row.LastModifierUserId = userId;
    }

    private static string Cut(string value, int max) => value.Length > max ? value[..max] : value;
}