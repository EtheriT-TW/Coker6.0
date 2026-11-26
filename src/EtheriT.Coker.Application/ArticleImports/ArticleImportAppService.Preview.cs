using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.ArticleImports;

// 預覽頁：列出每列 AI 結果、圖片、失敗原因，標示會更新的同名文章；可逐列修改欄位。
public sealed partial class ArticleImportAppService
{
    public async Task<ArticleImportBatchDto> GetBatchAsync(long batchId)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var batch = await FindBatchAsync(websiteId, batchId, tracking: false);
        var template = ArticleTemplateCatalog.Exists(batch.TemplateKey) ? ArticleTemplateCatalog.Get(batch.TemplateKey) : null;
        var directoryTitle = await db.Directory.AsNoTracking()
            .Where(d => d.Id == batch.FK_DirectoryId && d.FK_WebsiteId == websiteId)
            .Select(d => d.Title)
            .FirstOrDefaultAsync();
        var taskRecords = await db.BackgroundTasks.AsNoTracking()
            .Where(t => t.FK_WebsiteId == websiteId && (t.Id == batch.FK_BackgroundTaskId || t.Id == batch.FK_ConfirmTaskId))
            .ToListAsync();
        var rows = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batch.Id && !r.IsDeleted)
            .OrderBy(r => r.RowNumber)
            .ToListAsync();
        var rowDtos = await ToRowDtosAsync(websiteId, batch, template, rows);

        var scanTask = taskRecords.FirstOrDefault(t => t.Id == batch.FK_BackgroundTaskId)
            ?? throw new KeyNotFoundException("找不到匯入任務。");
        var confirmTask = taskRecords.FirstOrDefault(t => t.Id == batch.FK_ConfirmTaskId);
        return new ArticleImportBatchDto(
            batch.Id,
            batch.FK_DirectoryId,
            directoryTitle ?? "（目錄已刪除）",
            batch.TemplateKey,
            template?.Name ?? batch.TemplateKey,
            batch.SourceFileName,
            batch.CreationTime,
            ToTaskDto(scanTask),
            confirmTask == null ? null : ToTaskDto(confirmTask),
            template == null ? [] : ArticleImportFieldRules.GetFields(template),
            rowDtos);
    }

    public async Task<ArticleImportRowDetailDto> GetRowAsync(long rowId)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var row = await db.ArticleImportRows.AsNoTracking()
            .Include(r => r.Batch)
            .FirstOrDefaultAsync(r => r.Id == rowId && !r.IsDeleted && r.Batch!.FK_WebsiteId == websiteId && !r.Batch.IsDeleted)
            ?? throw new KeyNotFoundException("找不到這一列資料。");
        var template = ArticleTemplateCatalog.Exists(row.Batch!.TemplateKey) ? ArticleTemplateCatalog.Get(row.Batch.TemplateKey) : null;
        var dto = (await ToRowDtosAsync(websiteId, row.Batch, template, [row])).Single();
        return new ArticleImportRowDetailDto(dto, ArticleImportProcessor.ParseSource(row.SourceJson));
    }

    // 預覽頁修改一列：驗證規則同 AI 結果，圖片只能選這列下載的檔案；存檔後這列變成「成功」。
    public async Task<ArticleImportRowDetailDto> UpdateRowAsync(ArticleImportRowUpdateDto input)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var userId = await loginUserData.GetUserId();
        var row = await db.ArticleImportRows
            .Include(r => r.Batch)
            .FirstOrDefaultAsync(r => r.Id == input.RowId && !r.IsDeleted && r.Batch!.FK_WebsiteId == websiteId && !r.Batch.IsDeleted)
            ?? throw new KeyNotFoundException("找不到這一列資料。");
        await EnsureAwaitingAsync(row.Batch!);
        if (row.Status is not (ArticleImportRowStatusEnum.Ready or ArticleImportRowStatusEnum.Failed))
            throw new InvalidOperationException("這一列目前不能修改。");
        if (!ArticleTemplateCatalog.Exists(row.Batch!.TemplateKey))
            throw new InvalidOperationException($"文章樣板「{row.Batch.TemplateKey}」已不存在。");

        var template = ArticleTemplateCatalog.Get(row.Batch.TemplateKey);
        var images = ArticleImportRowImages.Choices(ArticleImportRowImages.Parse(row.DownloadedFilesJson));
        var paths = images.Select(i => i.Path).ToHashSet(StringComparer.Ordinal);
        var (fields, error) = ArticleImportFieldRules.Normalize(
            template, input.Fields, value => paths.Contains(value) ? value : null, images, byUser: true);
        if (error != null) throw new InvalidOperationException(error);

        row.FieldDataJson = ArticleFieldValues.Serialize(fields);
        row.Status = ArticleImportRowStatusEnum.Ready;
        row.Error = null;
        loginUserData.setOptionParameter(row, userId);
        await db.SaveChangesAsync();
        return await GetRowAsync(row.Id);
    }

    private async Task<List<ArticleImportRowDto>> ToRowDtosAsync(
        long websiteId, ArticleImportBatch batch, ArticleTemplate? template, IReadOnlyList<ArticleImportRow> rows)
    {
        var titleKey = template?.PageTitleKey ?? "";
        var parsed = rows.Select(r => new
        {
            Row = r,
            Fields = SafeParse(r.FieldDataJson),
        }).Select(x => new { x.Row, x.Fields, Title = ArticleFieldValues.First(x.Fields, titleKey) }).ToList();

        // 只有還沒寫入的列需要比對同名文章；整批同名的列數用來提示「批次內重複」。
        var needMatch = parsed.Any(x => x.Row.Status is ArticleImportRowStatusEnum.Ready or ArticleImportRowStatusEnum.Failed);
        var matches = needMatch
            ? ArticleImportTitleMatcher.Build(await templates.LoadDirectoryArticlesAsync(websiteId, batch.FK_DirectoryId))
            : new Dictionary<string, ArticleTemplateAppService.DirectoryArticle>(StringComparer.Ordinal);
        var batchTitles = needMatch
            ? await BatchTitleCountsAsync(batch.Id, titleKey)
            : new Dictionary<string, int>(StringComparer.Ordinal);

        return parsed.Select(x =>
        {
            var images = ArticleImportRowImages.Parse(x.Row.DownloadedFilesJson);
            if (x.Row.Status == ArticleImportRowStatusEnum.Ready)
                ArticleImportFieldRules.SetDefaultCover(x.Fields, ArticleImportRowImages.Choices(images));
            var key = ArticleImportTitleMatcher.Key(x.Title);
            var pending = x.Row.Status is ArticleImportRowStatusEnum.Ready or ArticleImportRowStatusEnum.Failed;
            var match = pending && key.Length > 0 && matches.TryGetValue(key, out var found) ? found : null;
            return new ArticleImportRowDto(
                x.Row.Id,
                x.Row.RowNumber,
                x.Row.Status,
                x.Title,
                x.Fields,
                images.Select(ArticleImportRowImages.ToDto).ToList(),
                x.Row.Error,
                match?.Id ?? (x.Row.Status == ArticleImportRowStatusEnum.Imported ? x.Row.FK_ArticleId : null),
                match?.CanEdit ?? true,
                pending && key.Length > 0 && batchTitles.GetValueOrDefault(key) > 1);
        }).ToList();
    }

    // 同一批（未取消、未失敗寫入）裡每個標題出現幾次。
    private async Task<Dictionary<string, int>> BatchTitleCountsAsync(long batchId, string titleKey)
    {
        var jsons = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batchId && !r.IsDeleted
                && (r.Status == ArticleImportRowStatusEnum.Ready || r.Status == ArticleImportRowStatusEnum.Failed))
            .Select(r => r.FieldDataJson)
            .ToListAsync();
        return jsons
            .Select(j => ArticleImportTitleMatcher.Key(ArticleFieldValues.First(SafeParse(j), titleKey)))
            .Where(t => t.Length > 0)
            .GroupBy(t => t, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
    }

    private static Dictionary<string, List<string>> SafeParse(string? json)
    {
        try
        {
            return ArticleFieldValues.Parse(json);
        }
        catch (InvalidOperationException)
        {
            return new Dictionary<string, List<string>>(StringComparer.Ordinal);
        }
    }

    private static ArticleImportTaskDto ToTaskDto(BackgroundTaskRecord task)
        => new(task.Id, task.Status, task.Progress, task.Message, task.Error);
}