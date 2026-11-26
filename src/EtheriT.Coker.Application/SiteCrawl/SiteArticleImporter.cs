using System.Web;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.SiteCrawl;

// 文章匯入流程（使用者指定）：建標籤 → 建目錄並綁標籤 → 建文章並綁標籤 → 列表頁改用「文章匯入目錄」元件顯示該目錄。
// 確認畫面標成商品的列表走同一個流程（使用者 2026-11-17 指定），只是建在商品管理的商品列表、目錄類型是商品、
// 列表頁改用「商品匯入目錄」元件（商品專屬的部分在 SiteArticleImporter.Products.cs）。
public sealed partial class SiteArticleImporter(SiteCrawlHtml html, SiteLayoutScanner scanner, SiteGalleryBuilder gallery, IArticleAppService articles,
    IProductAppService products, IWebMenuApplication webMenu, IHtmlSanitizer sanitizer, IWebsiteCacheStateAppService cacheState,
    CokerDbContext db, LoginUserData user, IOptions<SiteCrawlOptions> options, ILogger<SiteArticleImporter> logger)
{
    internal const string PurposeCode = "article-import-directory";
    private const string CatalogFrameMarker = "//*[contains(concat(' ',normalize-space(@class),' '),' catalog_frame ')]";
    private const string TitleMarker = "//*[@data-edit-key='pageTitle'] | //*[@data-import-role='page-title']";
    private const int MaxTagLength = 50;
    private const int MaxDirectoryTitleLength = 60;
    private static readonly TimeSpan FetchDelay = TimeSpan.FromMilliseconds(300);

    // 文章與商品的差別：目錄類型、列表頁用的元件用途與目錄區塊標記。
    private sealed record ImportKind(string Name, DirectoryTypeEnum DirectoryType, string PurposeCode, string PurposeName,
        string DirectoryMarker, string FallbackMarker);

    private static readonly ImportKind ArticleKind = new("文章", DirectoryTypeEnum.文章, PurposeCode, "文章匯入目錄",
        "//*[@data-import-role='article-directory']", "//*[@data-import-role='product-directory'] | " + CatalogFrameMarker);

    private sealed record ListPage(SiteLayoutScanner.LayoutPage Page, ImportedSiteMenu? Menu, string TagName, bool IsProduct);
    private sealed record Membership(HashSet<long> TagIds, DateTime? Date, string? ImageUrl, string? Description, Uri ListUrl,
        Uri ArticleUrl);

    // 回傳已改用目錄元件的選單，內容匯入就不再把這些列表頁存成快照。
    internal async Task<HashSet<long>> ImportAsync(SiteLayoutSelection layout, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var handledMenus = new HashSet<long>();
        var menus = result.ImportedMenus.Where(SiteLayoutScanner.CanHoldContent)
            .GroupBy(menu => SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!)))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var websiteId = await user.GetWebsiteId();
        // 分類頁（列的是子分類，子分類都建成選單）沒有文章或商品可放，改用選單目錄列出子分類。
        // 分類介紹裡可能連到文章／商品，所以等文章、商品都建好（記下新站網址）後才寫，見下方兩個出口。
        var categoryPages = layout.Pages.Where(SiteCategoryLists.IsCategoryPage).ToList();
        // 使用者取消勾選的頁面上的列表不匯入（同一列表的分頁用 ListKey 對回）。
        var lists = layout.Pages.Where(page => page.List != null && !SiteCategoryLists.IsCategoryPage(page)
                && !result.ExcludedPageKeys.Contains(SiteListDetector.ListKey(page.Url)))
            .Select(page =>
            {
                var menu = menus.GetValueOrDefault(SiteLayoutScanner.PageKey(page.Url));
                return new ListPage(page, menu, Truncate(menu?.SourceTitle.Trim() ?? page.List!.Name, MaxTagLength),
                    IsProductList(page, result));
            }).ToList();
        if (lists.Count == 0)
        {
            await BindCategoryPagesAsync(categoryPages, menus, websiteId, handledMenus, result, checkpoint, token);
            await cacheState.TouchByWebsiteIdAsync(websiteId, WebsiteCacheKeys.DirectoryContent);
            return handledMenus;
        }
        var detailPages = layout.Pages
            .Where(page => page.Article != null && page.Content != null && !menus.ContainsKey(SiteLayoutScanner.PageKey(page.Url)))
            .GroupBy(page => page.Article!.Key)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var host = layout.Pages[0].Url.IdnHost;
        // 商品先匯入：商品列表裡的詳細頁不再當文章。
        // 先把商品、文章都建好（記下新站網址），再寫內文：商品內文連到文章、文章連到商品時才改寫得到。
        var productKeys = new HashSet<string>(StringComparer.Ordinal);
        var productLists = lists.Where(list => list.IsProduct).ToList();
        PreparedLists? products = null;
        Dictionary<string, SiteLayoutScanner.LayoutPage> productPages = [];
        Dictionary<string, long> productIds = [];
        if (productLists.Count > 0)
        {
            products = await PrepareAsync(ProductKind, productLists, menus, [], websiteId, result, token);
            productKeys.UnionWith(products.Members.Keys);
            productPages = detailPages.Where(x => productKeys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            await FetchListedAsync(ProductKind, productPages, products.Members, host, result, checkpoint, token);
            productIds = await CreateProductsAsync(productPages, products.Members, websiteId, result, checkpoint, token);
        }
        var articleLists = lists.Where(list => !list.IsProduct).ToList();
        PreparedLists? articles = null;
        Dictionary<string, SiteLayoutScanner.LayoutPage> articlePages = [];
        Dictionary<string, long> articleIds = [];
        if (articleLists.Count > 0)
        {
            articles = await PrepareAsync(ArticleKind, articleLists, menus, productKeys, websiteId, result, token);
            articlePages = detailPages.Where(x => !productKeys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            await FetchListedAsync(ArticleKind, articlePages, articles.Members, host, result, checkpoint, token);
            articleIds = await CreateArticlesAsync(articlePages, articles.Members, websiteId, result, checkpoint, token);
        }
        if (products != null)
        {
            await FillProductsAsync(productPages, productIds, products.Members, websiteId, result, checkpoint, token);
            await BindListPagesAsync(ProductKind, productLists, products.Directories, websiteId, handledMenus, result, checkpoint, token);
        }
        if (articles != null)
        {
            await FillArticlesAsync(articlePages, articleIds, articles.Members, result, websiteId, checkpoint, token);
            await BindListPagesAsync(ArticleKind, articleLists, articles.Directories, websiteId, handledMenus, result, checkpoint, token);
        }
        await BindCategoryPagesAsync(categoryPages, menus, websiteId, handledMenus, result, checkpoint, token);
        await cacheState.TouchByWebsiteIdAsync(websiteId, WebsiteCacheKeys.DirectoryContent);
        return handledMenus;
    }

    private static bool IsProductList(SiteLayoutScanner.LayoutPage page, SiteLayoutResult result) =>
        result.ProductListKeys.Contains(SiteListDetector.ListKey(page.Url));

    private sealed record PreparedLists(Dictionary<string, Membership> Members,
        Dictionary<string, Core.Models.Directory> Directories);

    // 建標籤 → 建目錄並綁標籤，再整理每個項目屬於哪些列表（標籤）。excluded：已當成另一種類型匯入的項目。
    private async Task<PreparedLists> PrepareAsync(ImportKind kind, List<ListPage> lists,
        Dictionary<string, ImportedSiteMenu> menus, HashSet<string> excluded, long websiteId,
        SiteLayoutResult result, CancellationToken token)
    {
        var tags = await EnsureTagsAsync(lists.Select(list => list.TagName), websiteId, result, token);
        var directories = await EnsureDirectoriesAsync(kind, tags, websiteId, result, token);
        var membership = BuildMembership(lists, tags);
        // 只有 sub_id 的頁面已經是選單（主選單、左欄子選單）時保留選單，不再另建一篇。
        foreach (var key in membership.Where(x => excluded.Contains(x.Key) || SiteArticleDetector.IsSubPageKey(x.Key)
                     && menus.ContainsKey(SiteLayoutScanner.PageKey(x.Value.ArticleUrl))).Select(x => x.Key).ToList())
            membership.Remove(key);
        return new PreparedLists(membership, directories);
    }

    private async Task<Dictionary<string, Core.Models.Tag>> EnsureTagsAsync(IEnumerable<string> names, long websiteId,
        SiteLayoutResult result, CancellationToken token)
    {
        var wanted = names.Distinct(StringComparer.Ordinal).ToList();
        var tags = (await db.Tags.Where(tag => tag.FK_WebsiteId == websiteId && !tag.IsDeleted && wanted.Contains(tag.Title))
                .ToListAsync(token))
            .GroupBy(tag => tag.Title, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        foreach (var name in wanted.Where(name => !tags.ContainsKey(name)))
        {
            var tag = new Core.Models.Tag { FK_WebsiteId = websiteId, Title = name };
            db.Tags.Add(tag);
            await user.SaveChanges(tag);
            tags[name] = tag;
            result.TagsCreated++;
        }
        return tags;
    }

    private async Task<Dictionary<string, Core.Models.Directory>> EnsureDirectoriesAsync(ImportKind kind,
        Dictionary<string, Core.Models.Tag> tags, long websiteId, SiteLayoutResult result, CancellationToken token)
    {
        var directories = new Dictionary<string, Core.Models.Directory>(StringComparer.Ordinal);
        foreach (var (name, tag) in tags)
        {
            var title = Truncate(name, MaxDirectoryTitleLength);
            var directory = await db.Directory.FirstOrDefaultAsync(x => x.FK_WebsiteId == websiteId && !x.IsDeleted
                && x.Type == (int)kind.DirectoryType && x.Title == title, token);
            if (directory == null)
            {
                directory = new Core.Models.Directory
                {
                    FK_WebsiteId = websiteId,
                    Title = title,
                    Type = (int)kind.DirectoryType,
                    Visible = true
                };
                db.Directory.Add(directory);
                await user.SaveChanges(directory);
                result.DirectoriesCreated++;
            }
            if (!await db.Tag_Associates.AnyAsync(x => !x.IsDeleted && x.Type == TagAssociateTypeEnum.目錄
                    && x.FK_AId == directory.Id && x.FK_TId == tag.Id, token))
            {
                var binding = new Tag_Associate { FK_TId = tag.Id, FK_AId = directory.Id, Type = TagAssociateTypeEnum.目錄 };
                db.Tag_Associates.Add(binding);
                await user.SaveChanges(binding);
            }
            directories[name] = directory;
        }
        return directories;
    }

    private static Dictionary<string, Membership> BuildMembership(IEnumerable<ListPage> lists, Dictionary<string, Core.Models.Tag> tags)
    {
        var membership = new Dictionary<string, Membership>(StringComparer.Ordinal);
        foreach (var list in lists)
        {
            var tagId = tags[list.TagName].Id;
            foreach (var item in list.Page.List!.Items)
            {
                var current = membership.GetValueOrDefault(item.Key) ?? new Membership([], null, null, null, list.Page.Url, item.Url);
                current.TagIds.Add(tagId);
                membership[item.Key] = current with
                {
                    Date = current.Date ?? item.Date,
                    ImageUrl = current.ImageUrl ?? item.ImageUrl,
                    Description = current.Description ?? item.Description
                };
            }
        }
        return membership;
    }

    // 列表上有、但掃描時沒抓到詳細頁的項目（掃描刻意跳過，或碰到頁數上限），在這裡逐篇補抓，讓目錄內容和舊站列表一致。
    // 舊站型錄商品的詳細頁和文章詳細頁結構相同（#cont@menu_id＋#list_ctrl），同樣用文章詳細頁的判斷。
    private async Task FetchListedAsync(ImportKind kind, Dictionary<string, SiteLayoutScanner.LayoutPage> pages,
        Dictionary<string, Membership> membership, string host, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var limit = options.Value.MaxArticles;
        await using var renderer = scanner.OpenRenderer(host, result);
        foreach (var (key, member) in membership.Where(x => !pages.ContainsKey(x.Key)).ToList())
        {
            if (limit > 0 && pages.Count >= limit)
            {
                result.ArticleLimitReached = true;
                return;
            }
            await checkpoint();
            try
            {
                var page = await scanner.FetchArticleAsync(member.ArticleUrl, key, host, renderer, token);
                if (page == null)
                {
                    // 只有 sub_id 的項目是子分類列表時本來就不當文章，不算錯誤。
                    if (!SiteArticleDetector.IsSubPageKey(key))
                        result.AddError($"{member.ArticleUrl.PathAndQuery} 不是{kind.Name}詳細頁，沒有匯入。");
                    continue;
                }
                pages[key] = page;
                result.ArticlesFetched++;
            }
            catch (Exception ex) when (!token.IsCancellationRequested &&
                ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
            {
                result.AddError($"無法抓取{kind.Name} {member.ArticleUrl.PathAndQuery}。{ex.Message}");
            }
            await Task.Delay(FetchDelay, token);
        }
    }

    // 第一輪只建文章（取得新站 Id），並記下新站網址：第二輪寫內文時，連到其他文章／商品的連結才改寫得到
    // （之前邊建邊寫，連到還沒建的文章會留在舊站）。
    private async Task<Dictionary<string, long>> CreateArticlesAsync(Dictionary<string, SiteLayoutScanner.LayoutPage> pages,
        Dictionary<string, Membership> membership, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        // 重新匯入時用標題找回原本的文章（比照選單同名覆蓋）；同一次匯入內同標題的不同文章不會互相覆蓋。
        var byTitle = (await db.Article.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && x.Title != null)
                .Select(x => new { x.Id, x.Title }).ToListAsync(token))
            .GroupBy(x => x.Title!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(x => x.Id).ToList(), StringComparer.Ordinal);
        var claimed = new HashSet<long>();
        var ids = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var (key, page) in pages)
        {
            await checkpoint();
            try
            {
                var member = membership.GetValueOrDefault(key);
                var id = await SaveArticleAsync(page.Article!, member, byTitle, claimed, websiteId, result, token);
                ids[key] = id;
                SiteCrawlHtml.RegisterDetail(result, page.Url, member?.ListUrl, isProduct: false, id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                logger.LogError(ex, "Site crawl article save failed for {Url}", page.Url);
                result.AddError($"文章「{page.Article!.Title}」匯入失敗，請查看 MVC 日誌。");
            }
        }
        return ids;
    }

    private async Task FillArticlesAsync(Dictionary<string, SiteLayoutScanner.LayoutPage> pages, Dictionary<string, long> ids,
        Dictionary<string, Membership> membership, SiteLayoutResult result, long websiteId,
        Func<Task> checkpoint, CancellationToken token)
    {
        var tagIds = membership.Values.SelectMany(x => x.TagIds).Distinct().ToList();
        var bound = (await db.Tag_Associates.Where(x => !x.IsDeleted && x.Type == TagAssociateTypeEnum.文章 && tagIds.Contains(x.FK_TId))
                .Select(x => new { x.FK_AId, x.FK_TId }).ToListAsync(token))
            .Select(x => (x.FK_AId, x.FK_TId)).ToHashSet();

        foreach (var (key, page) in pages)
        {
            if (!ids.TryGetValue(key, out var id)) continue;
            await checkpoint();
            try
            {
                var member = membership.GetValueOrDefault(key);
                foreach (var tagId in member?.TagIds ?? [])
                    if (bound.Add((id, tagId)))
                    {
                        var binding = new Tag_Associate { FK_TId = tagId, FK_AId = id, Type = TagAssociateTypeEnum.文章 };
                        db.Tag_Associates.Add(binding);
                        await user.SaveChanges(binding);
                    }
                if (member?.ImageUrl != null)
                    await html.BindImageAsync(member.ImageUrl, member.ListUrl, id, FileBindTypeEnum.文章管理, result, checkpoint, token);
                await ImportArticleContentAsync(id, page, websiteId, result, checkpoint, token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                logger.LogError(ex, "Site crawl article import failed for {Url}", page.Url);
                result.AddError($"文章「{page.Article!.Title}」匯入失敗，請查看 MVC 日誌。");
            }
        }
    }

    private async Task<long> SaveArticleAsync(SiteArticleInfo info, Membership? member,
        Dictionary<string, List<long>> byTitle, HashSet<long> claimed, long websiteId,
        SiteLayoutResult result, CancellationToken token)
    {
        var existingId = byTitle.GetValueOrDefault(info.Title)?.FirstOrDefault(id => !claimed.Contains(id));
        if (existingId is > 0)
        {
            claimed.Add(existingId.Value);
            var existing = await db.Article.FirstAsync(x => x.Id == existingId.Value, token);
            var changed = false;
            if (member?.Date != null && existing.NodeDate != member.Date)
            {
                existing.NodeDate = member.Date;
                changed = true;
            }
            if (member?.Description != null && existing.Description != member.Description)
            {
                existing.Description = member.Description;
                changed = true;
            }
            if (changed) await user.SaveChanges(existing);
            result.ArticlesUpdated++;
            return existing.Id;
        }

        var article = new Core.Models.Article
        {
            FK_WebsiteId = websiteId,
            Title = info.Title,
            NodeDate = member?.Date,
            Description = member?.Description,
            Visible = true,
            permanent = true,
            RemovedFromShelves = false
        };
        db.Article.Add(article);
        await user.SaveChanges(article);
        claimed.Add(article.Id);
        result.ArticlesCreated++;
        return article.Id;
    }

    private async Task ImportArticleContentAsync(long id, SiteLayoutScanner.LayoutPage page, long websiteId,
        SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var content = SiteContentImporter.PrepareContent(page.Content!);
        result.ContentSource = SiteLayoutResult.SourceName(page.Article?.Title ?? page.Title, page.Url, "文章");
        var built = await gallery.BuildAsync(content, page.Url, websiteId, isArticle: true, page.SubTitle, result, checkpoint, token);
        if (built == null) return;

        var response = await articles.ImportConten(new ArticleSaveContenDto
        {
            Id = checked((int)id),
            SaveHtml = built.Html,
            SaveCss = built.Css
        }, manualEdit: false);
        if (built.UsedTemplate && response.Success) result.ArticleTemplatesApplied++;
        if (!response.Success)
        {
            logger.LogWarning("Site crawl article content save failed for {ArticleId}: {Reason}", id, response.Error);
            result.AddError($"文章「{page.Article!.Title}」的內容儲存失敗，請查看 MVC 日誌。");
        }
    }

    private async Task BindListPagesAsync(ImportKind kind, List<ListPage> lists, Dictionary<string, Core.Models.Directory> directories,
        long websiteId, HashSet<long> handledMenus, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var menuLists = lists.Where(list => list.Menu != null && !result.HandledMenuIds.Contains(list.Menu.Id)).GroupBy(list => list.Menu!.Id).Select(group => group.First()).ToList();
        if (menuLists.Count == 0) return;
        var template = await gallery.FindTemplateAsync(websiteId, kind.PurposeCode, token);
        if (template == null)
        {
            result.AddError($"沒有勾選「{kind.PurposeName}」用途的元件，{kind.Name}列表頁改存成匯入當下的快照。");
            return;
        }
        var templateHtml = SiteImportTemplates.DecodeComponentHtml(template.Html);
        if (!HasDirectoryMarker(kind, templateHtml))
        {
            result.AddError($"「{kind.PurposeName}」元件裡找不到目錄區塊（data-import-role 目錄標記或 catalog_frame），{kind.Name}列表頁改存成快照。");
            return;
        }

        foreach (var list in menuLists)
        {
            await checkpoint();
            var menu = list.Menu!;
            var (introHtml, introCss) = await BuildListIntroAsync(list.Page, result, checkpoint, token);
            var response = await webMenu.importConten(new MenuSaveContenDto
            {
                Id = checked((int)menu.Id),
                SaveHtml = introHtml + BuildListPageHtml(kind, templateHtml, directories[list.TagName], menu.Title),
                SaveCss = introCss + (template.Css ?? "")
            });
            if (!response.Success)
            {
                logger.LogWarning("Site crawl list page bind failed for menu {MenuId}: {Reason}", menu.Id, response.Error);
                result.AddError($"選單「{menu.Title}」改用目錄元件失敗，請查看 MVC 日誌。");
                continue;
            }
            handledMenus.Add(menu.Id);
            result.ListPagesBound++;
        }
    }

    // 分類頁：上方的分類介紹（例 乙太網路光電轉換器 的橫幅圖與標題）＋選單目錄。
    private async Task BindCategoryPagesAsync(List<SiteLayoutScanner.LayoutPage> pages, Dictionary<string, ImportedSiteMenu> menus,
        long websiteId, HashSet<long> handledMenus, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var bound = pages
            .Select(page => (Page: page, Menu: menus.GetValueOrDefault(SiteLayoutScanner.PageKey(page.Url))))
            .Where(x => x.Menu != null && !result.HandledMenuIds.Contains(x.Menu.Id))
            .DistinctBy(x => x.Menu!.Id);
        foreach (var (page, found) in bound)
        {
            await checkpoint();
            var menu = found!;
            var directory = await SiteCategoryLists.EnsureMenuDirectoryAsync(db, user, websiteId, menu.Id, menu.Title, result, token);
            var (introHtml, introCss) = await BuildListIntroAsync(page, result, checkpoint, token);
            var response = await webMenu.importConten(new MenuSaveContenDto
            {
                Id = checked((int)menu.Id),
                SaveHtml = introHtml + SiteImportTemplates.BuildMenuDirectory(directory.Id, directory.Title ?? menu.Title),
                SaveCss = introCss
            });
            if (!response.Success)
            {
                logger.LogWarning("Site crawl category page bind failed for menu {MenuId}: {Reason}", menu.Id, response.Error);
                result.AddError($"選單「{menu.Title}」改用選單目錄失敗，請查看 MVC 日誌。");
                continue;
            }
            handledMenus.Add(menu.Id);
            result.HandledMenuIds.Add(menu.Id);
        }
    }

    // 列表上方的分類共用內容（例 房型價格表）放在目錄前面，跟舊站順序一致。
    private async Task<(string Html, string Css)> BuildListIntroAsync(SiteLayoutScanner.LayoutPage page,
        SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        if (page.Content == null || SiteImportTemplates.ExtractListIntro(page.Content) is not { } intro) return ("", "");
        result.ContentSource = SiteLayoutResult.SourceName(page.SubTitle ?? page.Title, page.Url, "列表頁");
        var wrapper = HtmlNode.CreateNode("<div></div>");
        foreach (var child in intro.ChildNodes.ToArray()) wrapper.AppendChild(child.CloneNode(true));
        var css = await html.CleanAsync(wrapper, page.Url, result, allowIframes: true, checkpoint, token);
        if (!SiteContentImporter.HasContent(wrapper)) return ("", "");
        var cleanCss = sanitizer.SanitizePublicCss(css);
        return (sanitizer.SanitizePublicHtml(wrapper.InnerHtml), cleanCss.Length == 0 ? "" : cleanCss + "\n");
    }

    private static bool HasDirectoryMarker(ImportKind kind, string templateHtml)
    {
        var document = new HtmlDocument();
        document.LoadHtml(templateHtml);
        return FindDirectoryNode(kind, document) != null;
    }

    private static HtmlNode? FindDirectoryNode(ImportKind kind, HtmlDocument document) =>
        document.DocumentNode.SelectSingleNode(kind.DirectoryMarker) ?? document.DocumentNode.SelectSingleNode(kind.FallbackMarker);

    private static string BuildListPageHtml(ImportKind kind, string templateHtml, Core.Models.Directory directory, string pageTitle)
    {
        var document = new HtmlDocument();
        document.LoadHtml(templateHtml);
        var node = FindDirectoryNode(kind, document)!;
        node.SetAttributeValue("data-dirid", directory.Id.ToString());
        node.SetAttributeValue("data-diridname", directory.Title ?? "");
        if (document.DocumentNode.SelectSingleNode(TitleMarker) is { } title)
            (title.SelectSingleNode(".//span") ?? title).InnerHtml = HttpUtility.HtmlEncode(pageTitle);
        return SiteImportTemplates.BodyHtml(document);
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];
}
