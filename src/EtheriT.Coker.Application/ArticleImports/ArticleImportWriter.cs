using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.ArticleImports;

// 匯入的背景第二段（使用者在預覽頁按確認後）：每列建立或更新一篇文章，再用樣板產生內容。
// 新文章：Title＝pageTitle、Description＝簡介前 100 字、顯示且永久上架、NodeDate＝匯入日，掛上目錄所有標籤。
// 同目錄有同標題文章：覆蓋欄位資料與簡介後重新套用樣板（不改標籤）。
public sealed class ArticleImportWriter(
    CokerDbContext db,
    LoginUserData loginUserData,
    ArticleTemplateAppService templates,
    ArticleImportFileCleaner cleaner)
{
    public const int DescriptionLength = 100;
    private const string HistorySource = "背景匯入文章資料";

    public async Task<ArticleImportConfirmResultDto> WriteAsync(long batchId, Func<int, string, Task> reportProgress)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var userId = await loginUserData.GetUserId();
        var batch = await db.ArticleImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && b.FK_WebsiteId == websiteId && !b.IsDeleted)
            ?? throw new InvalidOperationException("找不到匯入批次。");
        if (!ArticleTemplateCatalog.Exists(batch.TemplateKey))
            throw new InvalidOperationException($"文章樣板「{batch.TemplateKey}」已不存在。");

        // 匯入當下的樣板快照；途中改了目錄樣板也照這批的樣板產生內容。
        var template = ArticleTemplateCatalog.Get(batch.TemplateKey);
        var directory = await templates.FindArticleDirectoryAsync(websiteId, batch.FK_DirectoryId);
        var tagIds = await templates.DirectoryTagIds(websiteId, directory.Id, TagAssociateTypeEnum.目錄).Distinct().ToListAsync();
        if (tagIds.Count == 0)
            throw new InvalidOperationException("目錄沒有設定標籤，文章建立後不會出現在目錄裡，請先到目錄設定標籤。");

        var orgName = await loginUserData.GetWebsiteOrgName(websiteId);
        var matches = ArticleImportTitleMatcher.Build(await templates.LoadDirectoryArticlesAsync(websiteId, directory.Id));
        var rowIds = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batch.Id && !r.IsDeleted && r.Status == ArticleImportRowStatusEnum.Ready)
            .OrderBy(r => r.RowNumber)
            .Select(r => r.Id)
            .ToListAsync();

        var created = 0;
        var updated = 0;
        var failures = new List<ArticleImportRowFailureDto>();
        for (var i = 0; i < rowIds.Count; i++)
        {
            var row = await db.ArticleImportRows.FirstAsync(r => r.Id == rowIds[i]);
            var values = ArticleFieldValues.Parse(row.FieldDataJson);
            var title = ArticleFieldValues.First(values, template.PageTitleKey);
            long? newArticleId = null;
            try
            {
                if (title.Length == 0) throw new InvalidOperationException("沒有標題（必填）。");
                var isUpdate = matches.TryGetValue(ArticleImportTitleMatcher.Key(title), out var match);
                if (isUpdate && !match!.CanEdit)
                    throw new InvalidOperationException("同標題的文章在有權限設定的目錄裡，您沒有編輯權限。");

                if (!isUpdate)
                    newArticleId = await CreateArticleAsync(websiteId, userId, title, row.FieldDataJson!, Description(template, values), tagIds);
                var articleId = newArticleId
                    ?? await UpdateArticleAsync(websiteId, match!.Id, row.FieldDataJson!, Description(template, values));
                await templates.ApplyToArticleAsync(websiteId, articleId, template, orgName, HistorySource);

                // 同一批裡後面同名的列，更新剛建立的這篇。
                if (!isUpdate) matches[ArticleImportTitleMatcher.Key(title)] = new ArticleTemplateAppService.DirectoryArticle(articleId, title, true, true);
                if (isUpdate) updated++; else created++;
                await SetRowAsync(row.Id, userId, ArticleImportRowStatusEnum.Imported, articleId, null);
            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                // 新文章已建立但產生內容失敗：刪掉這篇空白文章，避免前台出現沒有內容的文章。
                if (newArticleId.HasValue) await DiscardArticleAsync(newArticleId.Value, userId);
                failures.Add(new ArticleImportRowFailureDto(row.RowNumber, title, ex.Message));
                await SetRowAsync(row.Id, userId, ArticleImportRowStatusEnum.Failed, null, ex.Message);
            }

            await reportProgress(5 + 85 * (i + 1) / rowIds.Count, $"正在建立文章（{i + 1}/{rowIds.Count}）");
        }

        await reportProgress(95, "正在清理未使用的圖片");
        await cleaner.RecycleUnusedAsync(batch.Id);
        var excluded = await db.ArticleImportRows.CountAsync(r =>
            r.FK_ArticleImportBatchId == batch.Id && !r.IsDeleted && r.Status == ArticleImportRowStatusEnum.Excluded);
        return new ArticleImportConfirmResultDto(batch.Id, created, updated, excluded, failures);
    }

    private async Task<long> CreateArticleAsync(
        long websiteId, long userId, string title, string fieldDataJson, string description, IReadOnlyList<long> tagIds)
    {
        var article = new Core.Models.Article
        {
            FK_WebsiteId = websiteId,
            Title = title,
            Description = description,
            Visible = true,
            permanent = true,
            RemovedFromShelves = false,
            NodeDate = DateTime.Today,
            FieldDataJson = fieldDataJson
        };
        db.Article.Add(article);
        loginUserData.setOptionParameter(article, userId);
        await db.SaveChangesAsync();

        foreach (var tagId in tagIds)
        {
            var associate = new Tag_Associate { FK_AId = article.Id, FK_TId = tagId, Type = TagAssociateTypeEnum.文章 };
            db.Tag_Associates.Add(associate);
            loginUserData.setOptionParameter(associate, userId);
        }
        await db.SaveChangesAsync();
        return article.Id;
    }

    private async Task<long> UpdateArticleAsync(long websiteId, long articleId, string fieldDataJson, string description)
    {
        var article = await db.Article.FirstOrDefaultAsync(a => a.Id == articleId && a.FK_WebsiteId == websiteId && !a.IsDeleted)
            ?? throw new InvalidOperationException("要更新的文章已不存在。");
        article.FieldDataJson = fieldDataJson;
        article.Description = description;
        await loginUserData.SaveChanges(article);
        return article.Id;
    }

    private async Task DiscardArticleAsync(long articleId, long userId)
    {
        var now = DateTime.Now;
        var article = await db.Article.FirstAsync(a => a.Id == articleId);
        article.IsDeleted = true;
        article.DeletionTime = now;
        article.DeleterUserId = userId;
        var associates = await db.Tag_Associates
            .Where(t => t.FK_AId == articleId && t.Type == TagAssociateTypeEnum.文章 && !t.IsDeleted)
            .ToListAsync();
        foreach (var associate in associates)
        {
            associate.IsDeleted = true;
            associate.DeletionTime = now;
            associate.DeleterUserId = userId;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task SetRowAsync(long rowId, long userId, ArticleImportRowStatusEnum status, long? articleId, string? error)
    {
        var row = await db.ArticleImportRows.FirstAsync(r => r.Id == rowId);
        row.Status = status;
        if (articleId.HasValue) row.FK_ArticleId = articleId;
        row.Error = error == null ? null : error.Length > 1000 ? error[..1000] : error;
        row.LastModificationTime = DateTime.Now;
        row.LastModifierUserId = userId;
        await db.SaveChangesAsync();
    }

    // 簡介：標籤含「簡介」的多行文字欄位優先，沒有就用第一個有值的多行文字欄位；取前 100 字、換行改空白。
    internal static string Description(ArticleTemplate template, IReadOnlyDictionary<string, List<string>> values)
    {
        var textareas = template.Fields.Where(f => f.Type == ArticleFieldTypes.Textarea).ToList();
        var text = textareas
            .OrderBy(f => f.Label.Contains("簡介", StringComparison.Ordinal) ? 0 : 1)
            .Select(f => ArticleFieldValues.First(values, f.Key))
            .FirstOrDefault(v => v.Length > 0) ?? "";
        text = string.Join(" ", text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length > DescriptionLength ? text[..DescriptionLength] : text;
    }
}

// 用標題找同目錄既有文章：去掉前後空白後完全相同才算（不做模糊比對）。
// 同標題有多篇時，優先選有欄位資料（之前匯入的）、再選最早建立的。
public static class ArticleImportTitleMatcher
{
    public static string Key(string title) => title.Trim();

    internal static Dictionary<string, ArticleTemplateAppService.DirectoryArticle> Build(
        IEnumerable<ArticleTemplateAppService.DirectoryArticle> articles)
        => articles
            .Where(a => Key(a.Title).Length > 0)
            .GroupBy(a => Key(a.Title), StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.HasData).ThenBy(a => a.Id).First(),
                StringComparer.Ordinal);
}