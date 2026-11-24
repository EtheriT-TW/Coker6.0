using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.FileManagement;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.ArticleTemplate;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.ArticleTemplates;

// 目錄文章樣板：查詢套用影響範圍、把目錄的樣板套用到目錄內的文章。
// 只處理有欄位資料（FieldDataJson，資料匯入產生）的文章；手動建立的文章不動。
public sealed class ArticleTemplateAppService(
    CokerDbContext db,
    LoginUserData loginUserData,
    IPermissionsAppService permissionsAppService,
    StringHandler stringHandler,
    IHtmlProcessor htmlProcessor,
    IHtmlSanitizeService htmlSanitizeService,
    IFileReferenceWriter fileReferenceWriter)
{
    internal sealed record DirectoryArticle(long Id, string Title, bool HasData, bool CanEdit);

    public IReadOnlyList<ArticleTemplateInfoDto> GetTemplates() => ArticleTemplateCatalog.List();

    // templateKey 空白＝目錄目前的樣板；fromTemplateKey 有值時列出「換過去會不顯示」的欄位。
    public async Task<ArticleTemplateChangeInfoDto> GetChangeInfoAsync(
        long directoryId, string? templateKey, string? fromTemplateKey)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var directory = await FindArticleDirectoryAsync(websiteId, directoryId);
        var key = string.IsNullOrWhiteSpace(templateKey) ? directory.ArticleTemplateKey : templateKey.Trim();
        if (!ArticleTemplateCatalog.Exists(key))
            throw new InvalidOperationException("請先選擇文章樣板。");

        var template = ArticleTemplateCatalog.Get(key!);
        var articles = await LoadDirectoryArticlesAsync(websiteId, directory.Id);
        var missing = ArticleTemplateCatalog.Exists(fromTemplateKey)
                && !string.Equals(ArticleTemplateCatalog.Get(fromTemplateKey!).Key, template.Key, StringComparison.Ordinal)
            ? ArticleTemplateCatalog.MissingFields(fromTemplateKey!, template.Key).Select(f => f.Label).ToList()
            : [];

        return new ArticleTemplateChangeInfoDto(
            template.Key,
            template.Name,
            articles.Count,
            articles.Count(a => a.HasData && a.CanEdit),
            articles.Count(a => !a.HasData),
            articles.Count(a => a.HasData && !a.CanEdit),
            missing);
    }

    // 背景任務呼叫：用目錄目前的樣板重新產生每篇文章的草稿與發布內容。
    public async Task<ArticleTemplateApplyResultDto> ApplyToDirectoryAsync(
        long directoryId, Func<int, string, Task>? reportProgress = null)
    {
        var websiteId = await loginUserData.GetWebsiteId();
        var directory = await FindArticleDirectoryAsync(websiteId, directoryId);
        if (!ArticleTemplateCatalog.Exists(directory.ArticleTemplateKey))
            throw new InvalidOperationException("目錄尚未設定文章樣板。");

        var template = ArticleTemplateCatalog.Get(directory.ArticleTemplateKey!);
        var orgName = await loginUserData.GetWebsiteOrgName(websiteId);
        var articles = await LoadDirectoryArticlesAsync(websiteId, directory.Id);
        var targets = articles.Where(a => a.HasData && a.CanEdit).ToList();
        var failures = new List<ArticleTemplateApplyFailureDto>();
        var applied = 0;

        for (var i = 0; i < targets.Count; i++)
        {
            try
            {
                await ApplyToArticleAsync(websiteId, targets[i].Id, template, orgName);
                applied++;
            }
            catch (Exception ex)
            {
                // 單篇失敗不中斷，清掉這篇的追蹤狀態後繼續下一篇。
                db.ChangeTracker.Clear();
                failures.Add(new ArticleTemplateApplyFailureDto(targets[i].Id, targets[i].Title, ex.Message));
            }

            if (reportProgress != null)
                await reportProgress(10 + 85 * (i + 1) / targets.Count, $"正在套用文章樣板（{i + 1}/{targets.Count}）");
        }

        return new ArticleTemplateApplyResultDto(
            template.Key,
            applied,
            articles.Count(a => !a.HasData),
            articles.Count(a => a.HasData && !a.CanEdit),
            failures);
    }

    // historySource：還原歷程顯示的來源說明（AuditLogs.BrowserInfo）。
    internal async Task ApplyToArticleAsync(
        long websiteId, long articleId, ArticleTemplate template, string orgName, string historySource = "背景套用文章樣板")
    {
        var userId = await loginUserData.GetUserId();
        var article = await db.Article.FirstOrDefaultAsync(a => a.Id == articleId && a.FK_WebsiteId == websiteId)
            ?? throw new InvalidOperationException("文章不存在。");
        var tagTitles = await db.Tag_Associates.AsNoTracking()
            .Where(t => !t.IsDeleted && t.Type == TagAssociateTypeEnum.文章 && t.FK_AId == articleId
                && t.Tag != null && !t.Tag.IsDeleted && t.Tag.FK_WebsiteId == websiteId)
            .Select(t => t.Tag!.Title)
            .Distinct()
            .OrderBy(title => title)
            .ToListAsync();
        var html = ArticleTemplateRenderer.Render(
            template, ArticleFieldValues.Parse(article.FieldDataJson), orgName, tagTitles);

        // 草稿（編輯器載入的內容）
        article.SaveHtml = stringHandler.HtmlEncode(html);
        article.SaveCss = template.Css;

        // 發布內容：比照 ArticleAppService.ImportConten（上傳路徑換成前台、清理後存檔）
        var sanitized = await htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
        {
            WebsiteId = websiteId,
            SourceType = HtmlSanitizeSourceType.文章,
            SourceId = article.Id,
            ContentKey = "Published",
            SanitizePolicy = "PublicHtml",
            Html = stringHandler.ResolveFrontUploadPath(html, orgName),
            Css = stringHandler.ResolveFrontUploadPath(template.Css, orgName),
            Force = true
        });
        article.Css = sanitized.Css;
        article.PageText = htmlProcessor.text(sanitized.Html);
        article.Html = stringHandler.HtmlEncode(sanitized.Html);
        article.LastModificationTime = DateTime.Now;
        article.LastModifierUserId = userId;
        await loginUserData.SaveChanges(article);

        await fileReferenceWriter.ReplaceSourceReferencesAsync(
            websiteId,
            new FileReferenceSource("Article", article.Id, "Draft", new Dictionary<string, string?>
            {
                ["SaveHtml"] = article.SaveHtml,
                ["SaveCss"] = article.SaveCss,
                ["DataJson"] = article.DataJson
            }),
            userId);
        await fileReferenceWriter.ReplaceSourceReferencesAsync(
            websiteId,
            new FileReferenceSource("Article", article.Id, "Published", new Dictionary<string, string?>
            {
                ["Html"] = article.Html,
                ["Css"] = article.Css,
                ["Description"] = article.Description,
                ["Subtitle"] = article.Subtitle
            }),
            userId);

        await WriteHistoryAsync(websiteId, userId, article, historySource);
    }

    // 還原歷程讀 AuditLogs 的 Article／ImportConten，Parameters 要是 {"Id":..,"SaveHtml":..,"SaveCss":..}。
    // 背景任務沒有 HttpContext，LoginUserData.SetLogs 會記成 BackgroundJob，歷程查不到，所以這裡自己寫。
    private async Task WriteHistoryAsync(long websiteId, long userId, Core.Models.Article article, string historySource)
    {
        var user = await loginUserData.GetUser();
        db.AuditLogs.Add(new Core.Models.AuditLog
        {
            FK_WebsiteId = websiteId,
            UserId = userId,
            ClientName = user.UserName,
            ClientIpAddress = "",
            BrowserInfo = historySource,
            ExecutionTime = DateTime.Now,
            ServiceName = "Article",
            MethodName = "ImportConten",
            Parameters = JsonConvert.SerializeObject(new ArticleSaveContenDto
            {
                Id = (int)article.Id,
                SaveHtml = article.SaveHtml,
                SaveCss = article.SaveCss
            }),
            ReturnValue = JsonConvert.SerializeObject(new ResponseMessageDto { Success = true })
        });
        await db.SaveChangesAsync();
    }

    internal async Task<Core.Models.Directory> FindArticleDirectoryAsync(long websiteId, long directoryId)
        => await db.Directory.AsNoTracking().FirstOrDefaultAsync(d =>
                d.Id == directoryId
                && d.FK_WebsiteId == websiteId
                && d.Type == (int)DirectoryTypeEnum.文章)
            ?? throw new InvalidOperationException("找不到文章目錄。");

    // 「在目錄裡」＝有任一目錄標籤，且沒有任一排除標籤（與前台目錄顯示一致）。
    // 可編輯＝目錄沒有設定權限、或是管理者、或是文章建立者（與目錄文章列表的 CanEdit 一致）。
    internal async Task<List<DirectoryArticle>> LoadDirectoryArticlesAsync(long websiteId, long directoryId)
    {
        var userId = await loginUserData.GetUserId();
        var isPowerUser = await permissionsAppService.IsPowerUserPermissions();
        var isRestricted = await db.PermissionDetail.AsNoTracking().AnyAsync(p =>
            p.FK_WebsiteId == websiteId
            && p.FK_TargetId == directoryId
            && p.Type == (int)PermissionDetailsTypeEnum.目錄
            && p.IsGranted);

        var tagIds = DirectoryTagIds(websiteId, directoryId, TagAssociateTypeEnum.目錄);
        var excludedTagIds = DirectoryTagIds(websiteId, directoryId, TagAssociateTypeEnum.目錄拒絕);
        var rows = await db.Article.AsNoTracking()
            .Where(a => a.FK_WebsiteId == websiteId)
            .Where(a => db.Tag_Associates.Any(t => !t.IsDeleted && t.Type == TagAssociateTypeEnum.文章
                && t.FK_AId == a.Id && tagIds.Contains(t.FK_TId)))
            .Where(a => !db.Tag_Associates.Any(t => !t.IsDeleted && t.Type == TagAssociateTypeEnum.文章
                && t.FK_AId == a.Id && excludedTagIds.Contains(t.FK_TId)))
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.CreatorUserId,
                HasData = a.FieldDataJson != null && a.FieldDataJson != ""
            })
            .ToListAsync();

        return rows
            .Select(r => new DirectoryArticle(
                r.Id,
                r.Title ?? "",
                r.HasData,
                !isRestricted || isPowerUser || r.CreatorUserId == userId))
            .ToList();
    }

    internal IQueryable<long> DirectoryTagIds(long websiteId, long directoryId, TagAssociateTypeEnum type)
        => db.Tag_Associates
            .Where(t => !t.IsDeleted && t.Type == type && t.FK_AId == directoryId)
            .Where(t => t.Tag != null && !t.Tag.IsDeleted && t.Tag.FK_WebsiteId == websiteId)
            .Select(t => t.FK_TId);
}