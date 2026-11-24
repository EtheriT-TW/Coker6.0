using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.ArticleImports;

// 目錄管理「匯入資料」：選目錄 → 上傳 Excel 勾選欄位 → 背景 AI 整理 → 預覽修改 → 確認建立文章。
// 所有查詢都以目前登入站台（LoginUserData.GetWebsiteId）過濾。
public sealed partial class ArticleImportAppService(
    CokerDbContext db,
    LoginUserData loginUserData,
    BackgroundTaskService tasks,
    ArticleTemplateAppService templates,
    ArticleImportFileCleaner cleaner)
{
    // 所有文章目錄；沒有目錄標籤或沒有樣板的也列出，但附上不能選的原因。
    public async Task<IReadOnlyList<ArticleImportDirectoryDto>> GetDirectoriesAsync()
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var directories = await db.Directory.AsNoTracking()
            .Where(d => d.FK_WebsiteId == websiteId && !d.IsDeleted && d.Type == (int)DirectoryTypeEnum.文章)
            .OrderBy(d => d.SortBy).ThenBy(d => d.Id)
            .Select(d => new
            {
                d.Id,
                d.Title,
                d.ArticleTemplateKey,
                HasTags = db.Tag_Associates.Any(t => !t.IsDeleted && t.Type == TagAssociateTypeEnum.目錄
                    && t.FK_AId == d.Id && t.Tag != null && !t.Tag.IsDeleted && t.Tag.FK_WebsiteId == websiteId)
            })
            .ToListAsync();

        return directories.Select(d =>
        {
            var hasTemplate = ArticleTemplateCatalog.Exists(d.ArticleTemplateKey);
            var reason = !d.HasTags ? "目錄沒有設定標籤"
                : !hasTemplate ? "目錄沒有設定文章樣板"
                : null;
            return new ArticleImportDirectoryDto(
                d.Id,
                d.Title ?? "",
                hasTemplate ? ArticleTemplateCatalog.Get(d.ArticleTemplateKey!).Key : null,
                hasTemplate ? ArticleTemplateCatalog.Get(d.ArticleTemplateKey!).Name : null,
                reason);
        }).ToList();
    }

    // 上傳後先列出欄位讓使用者勾選；這一步不存任何資料。
    public ArticleImportColumnsDto ReadColumns(Stream stream, string fileName)
    {
        var sheet = ReadSheet(stream, fileName);
        var columns = sheet.Columns.Select(column => new ArticleImportColumnDto(
                column,
                Sample(sheet.Rows.Select(r => r.Values.GetValueOrDefault(column)).FirstOrDefault(v => !string.IsNullOrEmpty(v))),
                ArticleImportExcelReader.IsSelectedByDefault(column)))
            .ToList();
        return new ArticleImportColumnsDto(SafeFileName(fileName), sheet.Rows.Count, columns);
    }

    // 建立匯入批次與每列資料（只存勾選的欄位、略過空列），並建立背景任務；呼叫端負責排程 Hangfire。
    public async Task<ArticleImportStartResultDto> StartAsync(
        long directoryId, Stream stream, string fileName, IReadOnlyCollection<string>? selectedColumns)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var userId = await loginUserData.GetUserId();
        var directory = (await GetDirectoriesAsync()).FirstOrDefault(d => d.Id == directoryId)
            ?? throw new InvalidOperationException("找不到文章目錄。");
        if (directory.DisabledReason != null)
            throw new InvalidOperationException($"這個目錄不能匯入：{directory.DisabledReason}。");

        var sheet = ReadSheet(stream, fileName);
        var selected = (selectedColumns ?? []).Where(c => sheet.Columns.Contains(c)).ToHashSet(StringComparer.Ordinal);
        if (selected.Count == 0)
            throw new InvalidOperationException("請至少勾選一個要匯入的欄位。");

        var sources = sheet.Rows
            .Select(r => new
            {
                r.RowNumber,
                Values = r.Values.Where(v => selected.Contains(v.Key)).ToDictionary(v => v.Key, v => v.Value)
            })
            .Where(r => r.Values.Count > 0)
            .ToList();
        if (sources.Count == 0)
            throw new InvalidOperationException("勾選的欄位沒有任何資料。");

        var task = await tasks.CreateArticleImportTaskAsync(websiteId, userId);
        try
        {
            var batch = new ArticleImportBatch
            {
                FK_WebsiteId = websiteId,
                FK_DirectoryId = directory.Id,
                FK_BackgroundTaskId = task.Id,
                TemplateKey = directory.TemplateKey!,
                SourceFileName = SafeFileName(fileName),
                ColumnsJson = JsonConvert.SerializeObject(sheet.Columns.Where(c => selected.Contains(c)))
            };
            db.ArticleImportBatches.Add(batch);
            loginUserData.setOptionParameter(batch, userId);
            foreach (var source in sources)
            {
                var row = new ArticleImportRow
                {
                    Batch = batch,
                    RowNumber = source.RowNumber,
                    Status = ArticleImportRowStatusEnum.Pending,
                    SourceJson = JsonConvert.SerializeObject(source.Values)
                };
                db.ArticleImportRows.Add(row);
                loginUserData.setOptionParameter(row, userId);
            }
            await db.SaveChangesAsync();
            return new ArticleImportStartResultDto(batch.Id, task.Id, sources.Count);
        }
        catch (Exception ex)
        {
            await tasks.FailAsync(task.Id, ex.Message);
            throw;
        }
    }

    // 目前這個站「還沒結束」的匯入（整理中、等待確認、建立文章中），畫面進入匯入頁時直接接續。
    public async Task<long?> GetCurrentBatchIdAsync()
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var now = DateTime.Now;
        var scanning = new[]
        {
            BackgroundTaskStatusEnum.Queued, BackgroundTaskStatusEnum.Running,
            BackgroundTaskStatusEnum.CancelRequested, BackgroundTaskStatusEnum.AwaitingConfirmation
        };
        var writing = new[] { BackgroundTaskStatusEnum.Queued, BackgroundTaskStatusEnum.Running };
        return await db.ArticleImportBatches.AsNoTracking()
            .Where(b => b.FK_WebsiteId == websiteId && !b.IsDeleted)
            .Where(b => db.BackgroundTasks.Any(t => t.Id == b.FK_BackgroundTaskId && scanning.Contains(t.Status)
                    && (t.ExpireTime == null || t.ExpireTime > now))
                || db.BackgroundTasks.Any(t => t.Id == b.FK_ConfirmTaskId && writing.Contains(t.Status)))
            .OrderByDescending(b => b.Id)
            .Select(b => (long?)b.Id)
            .FirstOrDefaultAsync();
    }

    // 預覽頁按確認：取消勾選的列標成 Excluded，其餘成功的列交給背景任務建立文章。
    public async Task<(long BatchId, long TaskId)> ConfirmAsync(ArticleImportConfirmDto input)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var userId = await loginUserData.GetUserId();
        var batch = await FindBatchAsync(websiteId, input.BatchId, tracking: true);
        await EnsureAwaitingAsync(batch);
        await templates.FindArticleDirectoryAsync(websiteId, batch.FK_DirectoryId);

        var excluded = (input.ExcludedRowIds ?? []).ToHashSet();
        var rows = await db.ArticleImportRows
            .Where(r => r.FK_ArticleImportBatchId == batch.Id && !r.IsDeleted)
            .ToListAsync();
        foreach (var row in rows.Where(r => excluded.Contains(r.Id)
            && r.Status is ArticleImportRowStatusEnum.Ready or ArticleImportRowStatusEnum.Failed))
        {
            row.Status = ArticleImportRowStatusEnum.Excluded;
            loginUserData.setOptionParameter(row, userId);
        }
        if (!rows.Any(r => r.Status == ArticleImportRowStatusEnum.Ready))
            throw new InvalidOperationException("沒有可匯入的資料，請至少保留一筆整理成功的資料，或取消這次匯入。");

        var task = await tasks.CreateArticleImportConfirmTaskAsync(websiteId, userId, batch.FK_BackgroundTaskId);
        try
        {
            batch.FK_ConfirmTaskId = task.Id;
            loginUserData.setOptionParameter(batch, userId);
            await db.SaveChangesAsync();
            return (batch.Id, task.Id);
        }
        catch (Exception ex)
        {
            await tasks.FailAsync(task.Id, ex.Message);
            throw;
        }
    }

    // 取消整批：等待確認或排隊中立即結束並刪除下載的圖片；執行中則要求背景任務停止（由它自己清理）。
    public async Task<bool> CancelAsync(long batchId)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var userId = await loginUserData.GetUserId();
        var batch = await FindBatchAsync(websiteId, batchId, tracking: false);
        if (batch.FK_ConfirmTaskId.HasValue)
            throw new InvalidOperationException("這批資料已經確認匯入，無法取消。");

        var finished = await tasks.CancelArticleImportAsync(batch.FK_BackgroundTaskId, websiteId);
        if (!finished) return false;

        var rows = await db.ArticleImportRows
            .Where(r => r.FK_ArticleImportBatchId == batch.Id && !r.IsDeleted && r.Status != ArticleImportRowStatusEnum.Imported)
            .ToListAsync();
        foreach (var row in rows)
        {
            row.Status = ArticleImportRowStatusEnum.Excluded;
            loginUserData.setOptionParameter(row, userId);
        }
        await db.SaveChangesAsync();
        await cleaner.RecycleUnusedAsync(batch.Id);
        return true;
    }

    private async Task<ArticleImportBatch> FindBatchAsync(long websiteId, long batchId, bool tracking)
    {
        var query = tracking ? db.ArticleImportBatches : db.ArticleImportBatches.AsNoTracking();
        return await query.FirstOrDefaultAsync(b => b.Id == batchId && b.FK_WebsiteId == websiteId && !b.IsDeleted)
            ?? throw new KeyNotFoundException("找不到匯入批次。");
    }

    private async Task EnsureAwaitingAsync(ArticleImportBatch batch)
    {
        var status = await db.BackgroundTasks.AsNoTracking()
            .Where(t => t.Id == batch.FK_BackgroundTaskId)
            .Select(t => (BackgroundTaskStatusEnum?)t.Status)
            .FirstOrDefaultAsync();
        if (status != BackgroundTaskStatusEnum.AwaitingConfirmation || batch.FK_ConfirmTaskId.HasValue)
            throw new InvalidOperationException("這批匯入資料目前不在等待確認狀態，請重新整理頁面。");
    }

    private static ArticleImportSheet ReadSheet(Stream stream, string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("請上傳 .xlsx 格式的 Excel 檔案。");
        return ArticleImportExcelReader.Read(stream);
    }

    private static string SafeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/'));
        return name.Length > 255 ? name[^255..] : name;
    }

    private static string Sample(string? value)
    {
        var text = (value ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
        return text.Length > 60 ? text[..60] + "…" : text;
    }
}