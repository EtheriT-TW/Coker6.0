using System.Text.RegularExpressions;
using AutoMapper;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

// IsAlias：同一頁的另一個網址（例如主選單帶 tagid、首頁連結沒帶），只用來把連結改寫到原選單，不再匯入內容。
internal sealed record ImportedSiteMenu(long Id, long? ParentId, string Title, string SourceTitle, string? SourceUrl, string RouterName,
    PageTypeEnum PageType, bool IsAlias = false);

public sealed class SiteMenuImporter(SiteCrawlHttp http, IWebMenuApplication webMenu,
    CokerDbContext db, LoginUserData user, IMapper mapper, ISiteCrawlBrowserFactory browsers, ILogger<SiteMenuImporter> logger)
{
    private const string HomeTitle = "首頁";
    private const string HomeRouterName = "Home";
    private static readonly HashSet<string> HomeTitles = new(StringComparer.OrdinalIgnoreCase) { HomeTitle, "Home" };
    internal async Task<Uri> ImportAsync(Uri root, long websiteId, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        (Uri entry, IReadOnlyList<SiteMenuLink> links) navigation;
        try
        {
            navigation = await ReadNavigationAsync(root, result, checkpoint, token);
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
        {
            result.AddError($"無法讀取來源導覽。{ex.Message}");
            throw;
        }
        result.ScanComplete = true;
        var (entry, links) = navigation;
        if (links.Count == 0)
        {
            result.AddError("入口頁沒有辨識到主導覽選單。" + result.FirstPageDiagnostic);
            return entry;
        }
        links = SiteMenuSelection.Apply(links, result);
        var state = await LoadStateAsync(entry.IdnHost, websiteId, result, checkpoint, hiddenByDefault: false, token);
        // 主選單有「首頁」就把它設成首頁；沒有的話另建一個關閉狀態的首頁選單，內容用舊站入口頁。
        state.HomeLink = FindHomeLink(links, entry);
        if (state.HomeLink == null)
        {
            state.HomeLink = new SiteMenuLink(HomeTitle, entry, false, []);
            state.HomeHidden = true;
            links = [state.HomeLink, .. links];
        }
        await CreateChildrenAsync(links, state, parentId: null, rootId: null);
        AddHomeAliases(result, [root, new Uri(root, "/"), entry, .. result.EntryRedirects]);
        return entry;
    }

    // 舊站根網址（例 superiorlink 的 /）、使用者輸入的網址、入口跳轉經過的網址都是舊站首頁，
    // 記成首頁選單的別名，內文與頁尾連到它們時才會改寫成新站首頁（2026-11-18 footer_logo 連到 / 留在舊站）。
    // 已經是其他選單的網址不當別名。
    private static void AddHomeAliases(SiteLayoutResult result, IEnumerable<Uri> urls)
    {
        if (result.ImportedMenus.FirstOrDefault(menu => menu.PageType == PageTypeEnum.首頁 && !menu.IsAlias) is not { } home)
            return;
        foreach (var url in urls)
        {
            if (url.Scheme is not ("http" or "https") || SiteCrawlHtml.FindMenuRoute(result, url) != null) continue;
            result.ImportedMenus.Add(home with { SourceUrl = url.AbsoluteUri, IsAlias = true });
        }
    }

    internal static SiteMenuLink? FindHomeLink(IEnumerable<SiteMenuLink> links, Uri entry)
    {
        var entryKey = SiteLayoutScanner.PageKey(entry);
        foreach (var link in links)
        {
            if (HomeTitles.Contains(link.Title.Trim())
                || (link.Url != null && SiteLayoutScanner.PageKey(link.Url) == entryKey))
                return link;
            if (FindHomeLink(link.Children, entry) is { } child) return child;
        }
        return null;
    }

    // 主選單項目沒有下拉、但該頁左欄有分類子選單（例 房型介紹 → 野兔雙人房、花鹿三追一…），把子頁建成它底下的子選單。
    // 左欄列的若有其他已匯入的選單，代表這頁只是同層兄弟之一（例 最新消息底下各頁），不處理；同一組子頁只歸給第一個選單。
    // 左欄可以包含這頁自己：舊站主選單常直接連到第一個子頁（例 飯店設施 → 泡湯游泳 的頁面），那個子頁建成連到上層的選單。
    internal async Task ImportSectionMenusAsync(SiteLayoutSelection layout, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var pages = layout.Pages.GroupBy(page => SiteLayoutScanner.PageKey(page.Url))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var menuKeys = result.ImportedMenus.Where(menu => menu.SourceUrl != null)
            .Select(menu => SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!))).ToHashSet(StringComparer.Ordinal);
        var parentIds = result.ImportedMenus.Where(menu => menu.ParentId != null)
            .Select(menu => menu.ParentId!.Value).ToHashSet();
        var claimed = new HashSet<string>(StringComparer.Ordinal);
        var sections = new List<(ImportedSiteMenu Menu, SiteLayoutScanner.LayoutPage Page, IReadOnlyList<SiteMenuLink> Links)>();
        var ownKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var menu in result.ImportedMenus.Where(menu => SiteLayoutScanner.CanHoldContent(menu)
                     && menu.PageType == PageTypeEnum.一般頁面 && !parentIds.Contains(menu.Id)).DistinctBy(menu => menu.Id))
        {
            if (!pages.TryGetValue(SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!)), out var page)
                || page.SectionLinks is not { Count: > 0 } links)
                continue;
            var ownKey = SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!));
            var keys = links.Select(link => SiteLayoutScanner.PageKey(link.Url!)).ToList();
            // 使用者取消勾選的主選單也算「其他選單」：左欄列出它們代表這頁只是兄弟之一，不能把它們建成這頁的子選單。
            if (keys.Any(key => key != ownKey && (menuKeys.Contains(key) || result.ExcludedPageKeys.Contains(key)))
                || keys.Any(claimed.Contains)) continue;
            // 左欄列出自己、而這個選單已在主選單下拉裡（有上層），左欄是上層的兄弟清單，不是它自己的子頁
            // （例 月份壽星慶生會：兄弟「同天入住」在左欄是展開按鈕、沒有網址，上一行比對不到，2026-11-16 實測）。
            if (menu.ParentId != null && keys.Contains(ownKey)) continue;
            // 左欄只有這頁自己（例 留言板：唯一一項是「=有急事請電話洽詢…」，就是留言板本身），沒有子頁可建。
            if (keys.All(key => key == ownKey)) continue;
            claimed.UnionWith(keys);
            if (keys.Contains(ownKey)) ownKeys.Add(ownKey);
            sections.Add((menu, page, links));
        }
        if (sections.Count == 0) return;

        var state = await LoadStateAsync(layout.Pages[0].Url.IdnHost, websiteId, result, checkpoint, hiddenByDefault: false, token);
        state.LinkToMenu.UnionWith(ownKeys);
        foreach (var (menu, page, allLinks) in sections)
        {
            await checkpoint();
            // 確認畫面上對左欄子頁的勾選與路徑名稱；全部取消勾選時這頁維持原樣，不建子選單也不改選單目錄。
            var links = result.MenuSelectionKeys.TryGetValue(menu.Id, out var parentKey)
                ? SiteMenuSelection.ApplySection(allLinks, parentKey, SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!)), result)
                : allLinks;
            if (links.Count == 0) continue;
            // 子頁放進主選單下拉；舊站分類首頁是空白頁時（例 房型介紹），點進去改顯示選單目錄列出子頁（使用者 2026-11-13 決定）。
            // 分類首頁自己有內容時保留內容。
            var useDirectory = page.Content == null
                || !SiteContentImporter.HasContent(SiteContentImporter.PrepareContent(page.Content));
            var before = result.ImportedMenus.Count;
            await CreateChildrenAsync(links, state, menu.Id, RootOf(menu, result) ?? menu.Id);
            result.SectionMenus += result.ImportedMenus.Count - before;
            if (useDirectory) await BindMenuDirectoryAsync(menu, websiteId, result, token);
        }
    }

    private static long? RootOf(ImportedSiteMenu menu, SiteLayoutResult result)
    {
        var current = menu;
        for (var depth = 0; current.ParentId is { } parentId && depth < 10; depth++)
            current = result.ImportedMenus.FirstOrDefault(x => x.Id == parentId) ?? current with { ParentId = null };
        return current.Id == menu.Id ? null : current.Id;
    }

    private async Task BindMenuDirectoryAsync(ImportedSiteMenu menu, long websiteId, SiteLayoutResult result, CancellationToken token)
    {
        var directory = await SiteCategoryLists.EnsureMenuDirectoryAsync(db, user, websiteId, menu.Id, menu.Title, result, token);
        var response = await webMenu.importConten(new MenuSaveContenDto
        {
            Id = checked((int)menu.Id),
            SaveHtml = SiteImportTemplates.BuildMenuDirectory(directory.Id, directory.Title ?? menu.Title),
            SaveCss = ""
        });
        if (!response.Success)
        {
            logger.LogWarning("Site crawl menu directory bind failed for menu {MenuId}: {Reason}", menu.Id, response.Error);
            result.AddError($"選單「{menu.Title}」改用選單目錄失敗，請查看 MVC 日誌。");
            return;
        }
        result.HandledMenuIds.Add(menu.Id);
    }

    // 主選單沒有、但從選單頁內文連得到的獨立頁面（例如首頁「地區類別專區」的各地區），建成第一層選單，Visible 預設關閉。
    // 前台用 RemovedFromShelves 判斷能不能開，Visible 只管選單顯示，所以隱藏選單的頁面仍可從連結進入。
    // 頁面名稱和既有選單相同時視為同一頁的另一個網址，不另建選單，只讓連結改寫到原選單。
    internal async Task ImportHiddenPagesAsync(SiteLayoutSelection layout, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var pages = layout.Pages.GroupBy(page => SiteLayoutScanner.PageKey(page.Url))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var menuKeys = result.ImportedMenus.Where(menu => menu.SourceUrl != null)
            .Select(menu => SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!))).ToHashSet(StringComparer.Ordinal);
        var menusByTitle = result.ImportedMenus.Where(SiteLayoutScanner.CanHoldContent)
            .GroupBy(menu => menu.SourceTitle.Trim(), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var pending = new Queue<string>(menuKeys.Where(pages.ContainsKey));
        var visited = new HashSet<string>(pending, StringComparer.Ordinal);
        var hidden = new List<SiteLayoutScanner.LayoutPage>();
        while (pending.TryDequeue(out var key))
        {
            foreach (var target in pages[key].ContentLinks ?? new HashSet<string>())
            {
                if (result.ExcludedPageKeys.Contains(target) || !visited.Add(target)
                    || !pages.TryGetValue(target, out var page)) continue;
                if (page.Article != null || page.Content == null || string.IsNullOrWhiteSpace(page.Title)) continue;
                if (menusByTitle.TryGetValue(page.Title.Trim(), out var same))
                {
                    result.ImportedMenus.Add(same with { SourceUrl = page.Url.AbsoluteUri, IsAlias = true });
                    continue;
                }
                hidden.Add(page);
                pending.Enqueue(target);
            }
        }
        if (hidden.Count == 0) return;

        var host = layout.Pages[0].Url.IdnHost;
        var state = await LoadStateAsync(host, websiteId, result, checkpoint, hiddenByDefault: true, token);
        var before = result.ImportedMenus.Count;
        // 子分類在確認畫面上的勾選與路徑名稱（使用者 2026-11-18 要求子分類也能改路徑名稱）。
        var tree = SiteMenuSelection.ApplyHidden(BuildHiddenTree(hidden, layout.Pages), result);
        await CreateChildrenAsync(tree, state, parentId: null, rootId: null);
        result.HiddenMenus += result.ImportedMenus.Count - before;
    }

    private const int MaxHiddenDepth = 10;

    // 依舊站麵包屑放到對應上層：上層名稱和既有選單相同就放進去，沒有就建一個隱藏的空白一般頁面當上層。
    // 舊站麵包屑只有兩層（superiorlink 2026-11-17：10/100M乙太網路 的麵包屑是 HOME > 產品資訊，實際在 乙太網路光電轉換器 底下），
    // 所以頁面出現在另一個隱藏頁的子分類列表裡時，改放到那一頁底下；順序照子分類列表。
    private static List<SiteMenuLink> BuildHiddenTree(IReadOnlyList<SiteLayoutScanner.LayoutPage> hidden,
        IEnumerable<SiteLayoutScanner.LayoutPage> pages)
    {
        var listed = SiteCategoryLists.ListedCategories(pages);
        var byKey = hidden.GroupBy(page => SiteLayoutScanner.PageKey(page.Url))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var nodes = new Dictionary<string, MenuNode>(StringComparer.Ordinal);
        var placing = new HashSet<string>(StringComparer.Ordinal);
        var roots = new List<MenuNode>();
        foreach (var key in byKey.Keys) Place(key, 0);
        return roots.Select(node => node.ToLink()).ToList();

        MenuNode? Place(string key, int depth)
        {
            if (nodes.TryGetValue(key, out var placed)) return placed;
            if (!placing.Add(key)) return null;
            var page = byKey[key];
            var category = listed.GetValueOrDefault(key);
            var parent = category != null && byKey.ContainsKey(category.ParentKey) && depth < MaxHiddenDepth
                ? Place(category.ParentKey, depth + 1) : null;
            var level = parent?.Children ?? BreadcrumbLevel(roots, page.Breadcrumb);
            var title = page.Title!.Trim();
            var node = level.FirstOrDefault(item => item.Title == title);
            if (node == null) level.Add(node = new MenuNode(title, page.Url));
            node.Url ??= page.Url;
            if (category != null)
            {
                node.IsCategory = true;
                node.Order = category.Index;
            }
            nodes[key] = node;
            placing.Remove(key);
            return node;
        }
    }

    private static List<MenuNode> BreadcrumbLevel(List<MenuNode> roots, IReadOnlyList<string>? breadcrumb)
    {
        var level = roots;
        foreach (var name in breadcrumb ?? [])
        {
            var parent = level.FirstOrDefault(node => node.Title == name);
            if (parent == null) level.Add(parent = new MenuNode(name, null));
            level = parent.Children;
        }
        return level;
    }

    private sealed class MenuNode(string title, Uri? url)
    {
        internal string Title { get; } = title;
        internal Uri? Url { get; set; } = url;
        internal bool IsCategory { get; set; }
        internal int? Order { get; set; }
        internal List<MenuNode> Children { get; } = [];
        // OrderBy 是穩定排序：子分類列表上的照列表順序，其餘維持找到的順序排在後面。
        internal SiteMenuLink ToLink() => new(Title, Url, false,
            Children.OrderBy(child => child.Order ?? int.MaxValue).Select(child => child.ToLink()).ToList(), IsCategory);
    }

    private async Task<ImportState> LoadStateAsync(string host, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, bool hiddenByDefault, CancellationToken token)
    {
        var existing = await db.WebMenus.AsNoTracking()
            .Where(menu => menu.FK_WebsiteId == websiteId && !menu.IsDeleted)
            .OrderBy(menu => menu.Id)
            .ToListAsync(token);
        var state = new ImportState(host, result, checkpoint) { HiddenByDefault = hiddenByDefault };
        foreach (var menu in existing)
        {
            var siblings = state.GetSiblings(menu.FK_TopNodeId);
            if (menu.Title is { } title) siblings.Existing.TryAdd(title.Trim(), menu);
            siblings.LastOrder = Math.Max(siblings.LastOrder, menu.SerNO);
            state.Routes.Add(menu.RouterName);
            // 之前匯入、仍是關閉狀態的隱藏選單，位置不對時可以搬到麵包屑指定的上層，避免重新匯入多出一份。
            if (menu.PageType == PageTypeEnum.首頁 && (state.ExistingHome == null || menu.Visible && !state.ExistingHome.Visible))
                state.ExistingHome = menu;
            if (hiddenByDefault && !menu.Visible && menu.Title is { } hiddenTitle
                && menu.RouterName?.StartsWith("import-", StringComparison.Ordinal) == true)
                state.Movable.TryAdd(hiddenTitle.Trim(), menu);
        }
        return state;
    }

    internal async Task<(Uri Entry, IReadOnlyList<SiteMenuLink> Links)> ReadNavigationAsync(
        Uri root, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var entry = root;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var redirect = 0; redirect <= 5; redirect++)
        {
            await checkpoint();
            if (!seen.Add(entry.AbsoluteUri)) throw new InvalidOperationException("選單入口發生重複跳轉。");
            var response = await http.GetAsync(entry, root.IdnHost, false, 2 * 1024 * 1024, token);
            await using var content = response.Content;
            var bytes = content.Length;
            var document = SiteLayoutScanner.ReadDocument(content, response.Charset);
            var target = SiteLayoutScanner.FindEntryTarget(document, response.Url);
            if (target == null)
            {
                result.PagesScanned = 1;
                result.FirstPageDiagnostic = SiteLayoutScanner.DescribePage(response.Url, document, bytes,
                    document.DocumentNode.SelectNodes("//a[@href]")?.Count ?? 0);
                var links = SiteMenuExtractor.Extract(document, response.Url);
                if (links.Count > 0) return (response.Url, links);
                await using var renderer = new SiteCrawlRenderSession(browsers, root.IdnHost, result, logger);
                var rendered = await renderer.TryRenderAsync(response.Url, false, token);
                return (response.Url, rendered == null ? links : SiteMenuExtractor.Extract(rendered, response.Url));
            }
            result.EntryRedirects.Add(entry);
            entry = target;
        }
        throw new InvalidOperationException("選單入口跳轉超過上限。");
    }

    private async Task CreateChildrenAsync(IReadOnlyList<SiteMenuLink> links, ImportState state, long? parentId, long? rootId)
    {
        var siblings = state.GetSiblings(parentId);
        foreach (var link in links)
        {
            await state.Checkpoint();
            if (MakeMenu(link, state, siblings, parentId, rootId) is not { Dto: var dto, HasSource: var hasSource }
                || siblings.SavedTitles.Contains(dto.Title!)) continue;
            var id = await SaveAsync(dto, state, link, hasSource);
            if (id.HasValue) siblings.SavedTitles.Add(dto.Title!);
            if (id.HasValue && link.Children.Count > 0)
                await CreateChildrenAsync(link.Children, state, id, rootId ?? id);
        }
    }

    private static string MenuTitle(SiteMenuLink link) => link.Title[..Math.Min(link.Title.Length, 100)].TrimEnd();

    // 自動產生路徑名稱用的字根：舊站網址路徑轉小寫英數（例 /tw/index.asp → tw-index-asp），前面再加 import-。
    private static string RouteStem(Uri? url)
    {
        if (url == null) return "menu";
        var stem = Regex.Replace(Uri.UnescapeDataString(url.AbsolutePath.Trim('/')).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return stem.Length == 0 ? "home" : stem;
    }

    // 偵測時推算每個主選單項目留空時會用的路徑名稱（確認畫面的灰字預設值），規則和 MakeMenu 相同：
    // 首頁固定 Home；外站連結沒有；同層同名的既有選單沿用它的路徑名稱；其餘依序保留 import-{字根}（重複時加 -2、-3）。
    // 和上方選單同一頁又沒有子選單的項目，匯入時不建選單（Duplicate）。
    internal async Task<Dictionary<string, SiteMenuRoute>> SuggestRoutesAsync(IReadOnlyList<SiteMenuLink> links, Uri entry,
        long websiteId, CancellationToken token)
    {
        var existing = await db.WebMenus.AsNoTracking()
            .Where(menu => menu.FK_WebsiteId == websiteId && !menu.IsDeleted)
            .OrderBy(menu => menu.Id)
            .Select(menu => new { menu.Id, menu.FK_TopNodeId, menu.Title, menu.RouterName })
            .ToListAsync(token);
        var routes = new HashSet<string>(existing.Select(menu => menu.RouterName).Where(route => !string.IsNullOrEmpty(route)),
            StringComparer.OrdinalIgnoreCase);
        var children = new Dictionary<long, Dictionary<string, (long Id, string? RouterName)>>();
        foreach (var menu in existing.Where(menu => menu.Title != null))
        {
            if (!children.TryGetValue(menu.FK_TopNodeId ?? 0, out var siblings))
                children[menu.FK_TopNodeId ?? 0] = siblings = new Dictionary<string, (long, string?)>(StringComparer.Ordinal);
            siblings.TryAdd(menu.Title!.Trim(), (menu.Id, menu.RouterName));
        }

        var host = entry.IdnHost;
        var home = FindHomeLink(links, entry);
        var importedUrls = new HashSet<string>(StringComparer.Ordinal);
        var result = new Dictionary<string, SiteMenuRoute>(StringComparer.Ordinal);
        Walk(links, null, 0, null);
        return result;

        // parentId：既有上層選單的 Id（第一層是 0）；上層是新建的就是 null，底下不可能有既有選單。
        // parentPage：上層的頁面（PageKey）。左欄子頁就是上層那一頁時，匯入成連到上層的選單，沒有自己的路徑名稱；
        // 左欄子頁、子分類另外建（ImportSectionMenusAsync／ImportHiddenPagesAsync 用新的狀態），不和主選單比對重複網址。
        void Walk(IReadOnlyList<SiteMenuLink> level, string? parentKey, long? parentId, string? parentPage)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var link in level)
            {
                var key = SiteMenuSelection.Key(parentKey, link.Title);
                if (!seen.Add(key)) continue;
                var url = link.Url;
                var isLocal = url != null && SiteCrawlHttp.IsAllowed(url, host);
                var pageKey = isLocal ? SiteLayoutScanner.PageKey(url!) : null;
                if (link.IsSection && pageKey != null && pageKey == parentPage)
                {
                    result[key] = new SiteMenuRoute(null, false, false, LinkToParent: true);
                    continue;
                }
                var urlKey = isLocal && !link.IsSection && !link.IsCategory ? SiteLayoutScanner.Normalize(url!).AbsoluteUri : null;
                if (urlKey != null && importedUrls.Contains(urlKey))
                {
                    if (link.Children.Count == 0)
                    {
                        result[key] = new SiteMenuRoute(null, false, true);
                        continue;
                    }
                    url = null;
                }
                if (urlKey != null) importedUrls.Add(urlKey);
                (long Id, string? RouterName) found = default;
                var hasExisting = parentId is { } pid && children.TryGetValue(pid, out var siblings)
                    && siblings.TryGetValue(MenuTitle(link), out found);
                if (ReferenceEquals(link, home))
                    result[key] = new SiteMenuRoute(HomeRouterName, hasExisting, false);
                else if (url != null && !isLocal)
                    result[key] = new SiteMenuRoute(null, hasExisting, false);
                else if (hasExisting && !string.IsNullOrWhiteSpace(found.RouterName))
                    result[key] = new SiteMenuRoute(found.RouterName, true, false);
                else
                    result[key] = new SiteMenuRoute(Reserve(routes, "import-" + RouteStem(url), 50, index => $"-{index + 1}"),
                        false, false);
                Walk(link.Children, key, hasExisting ? found.Id : null, url == null ? null : pageKey);
            }
        }
    }

    // 偵測時讀主選單各頁的左欄子頁（#left_main_menu）與子分類樹，和匯入掃描同一套讀法：原始碼看起來要 JS 載入時才開瀏覽器。
    // 左欄子頁只讀沒有下拉的頁面（有下拉的不會再接左欄子頁）；子分類也從左欄子頁往下找（分類頁可能在左欄裡）。
    internal async Task<SitePageExtras> ReadPageExtrasAsync(IReadOnlyList<SiteMenuLink> links, Uri entry,
        Action<string> addError, Func<Task> checkpoint, CancellationToken token)
    {
        var host = entry.IdnHost;
        var home = FindHomeLink(links, entry);
        var targets = Leaves(links).Where(link => !ReferenceEquals(link, home) && link.Url != null
                && SiteCrawlHttp.IsAllowed(link.Url, host))
            .Select(link => link.Url!).DistinctBy(SiteLayoutScanner.PageKey).ToList();
        var sections = new Dictionary<string, IReadOnlyList<SiteMenuLink>>(StringComparer.Ordinal);
        var categories = new Dictionary<string, IReadOnlyList<SiteMenuLink>>(StringComparer.Ordinal);
        var explored = new HashSet<string>(StringComparer.Ordinal);
        var scratch = new SiteLayoutResult();
        await using (var renderer = new SiteCrawlRenderSession(browsers, host, scratch, logger))
        {
            var sectionUrls = new List<Uri>();
            foreach (var url in targets)
            {
                explored.Add(SiteLayoutScanner.PageKey(url));
                if (await ReadPageAsync(url, renderer) is not { } page) continue;
                var section = SitePageParts.ReadSectionMenu(page.Document, page.Url, host);
                if (section.Count > 0)
                {
                    sections.TryAdd(SiteLayoutScanner.PageKey(url), section);
                    sectionUrls.AddRange(section.Select(link => link.Url!));
                }
                await AddCategoriesAsync(url, page, renderer);
            }
            foreach (var url in sectionUrls.DistinctBy(SiteLayoutScanner.PageKey))
            {
                if (!explored.Add(SiteLayoutScanner.PageKey(url))) continue;
                if (await ReadPageAsync(url, renderer) is { } page) await AddCategoriesAsync(url, page, renderer);
            }
        }
        foreach (var error in scratch.Errors) addError(error);
        return new SitePageExtras(sections, categories);

        async Task AddCategoriesAsync(Uri url, (Uri Url, HtmlDocument Document) page, SiteCrawlRenderSession renderer)
        {
            var found = await ReadCategoriesAsync(page.Url, page.Document, renderer, 0);
            if (found.Count > 0) categories.TryAdd(SiteLayoutScanner.PageKey(url), found);
        }

        async Task<(Uri Url, HtmlDocument Document)?> ReadPageAsync(Uri url, SiteCrawlRenderSession renderer)
        {
            await checkpoint();
            try
            {
                var reply = await http.GetAsync(url, host, false, 2 * 1024 * 1024, token);
                await using var content = reply.Content;
                var page = SiteLayoutScanner.ReadDocument(content, reply.Charset);
                var rendered = SiteCrawlRenderSession.NeedsRender(page)
                    ? await renderer.TryRenderAsync(reply.Url, SiteCrawlRenderSession.NeedsScroll(page), token) : null;
                return (reply.Url, rendered ?? page);
            }
            catch (Exception ex) when (!token.IsCancellationRequested &&
                ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
            {
                // 讀不到的頁面就不列它的子頁；正式匯入時會再抓一次。
                return null;
            }
        }

        // 子分類的判斷和 SiteLayoutScanner.ResolveSubPages 相同：列表上只有 sub_id 的卡片，目標頁本身也是列表才算子分類；
        // 名稱用目標頁的頁面名稱（隱藏選單也是用它命名），子分類頁再往下找下一層。
        async Task<IReadOnlyList<SiteMenuLink>> ReadCategoriesAsync(Uri pageUrl, HtmlDocument document,
            SiteCrawlRenderSession renderer, int depth)
        {
            if (depth >= MaxHiddenDepth) return [];
            var items = SiteArticleDetector.ReadList(document, pageUrl, host)?.Items
                .Where(item => SiteArticleDetector.IsSubPageKey(item.Key)) ?? [];
            var children = new List<SiteMenuLink>();
            foreach (var item in items)
            {
                if (!explored.Add(SiteLayoutScanner.PageKey(item.Url))) continue;
                if (await ReadPageAsync(item.Url, renderer) is not { } target
                    || SiteArticleDetector.ReadList(target.Document, target.Url, host) == null
                    || SiteArticleDetector.PageName(target.Document) is not { } title || string.IsNullOrWhiteSpace(title))
                    continue;
                var grandChildren = await ReadCategoriesAsync(target.Url, target.Document, renderer, depth + 1);
                children.Add(new SiteMenuLink(title.Trim(), item.Url, false, grandChildren, IsCategory: true));
            }
            return children;
        }
    }

    private static IEnumerable<SiteMenuLink> Leaves(IEnumerable<SiteMenuLink> links) =>
        links.SelectMany(link => link.Children.Count == 0 ? [link] : Leaves(link.Children));

    // hasSource：這個選單有對應的舊站頁面可匯入內容（沒網址的下拉上層、重複網址的上層都沒有）。
    private (MenuItemDto Dto, bool HasSource)? MakeMenu(SiteMenuLink link, ImportState state, SiblingState siblings, long? parentId, long? rootId)
    {
        var url = link.Url;
        var isLocal = url != null && SiteCrawlHttp.IsAllowed(url, state.Host);
        var key = isLocal ? SiteLayoutScanner.Normalize(url!).AbsoluteUri : null;
        var duplicate = key != null && state.ImportedUrls.Contains(key);
        if (duplicate && link.Children.Count == 0) return null;
        if (duplicate) url = null;
        if (!isLocal && url?.AbsoluteUri.Length > 255)
        {
            state.Result.AddError($"選單「{link.Title}」的外站網址超過 255 字元。");
            if (link.Children.Count == 0) return null;
            url = null;
        }
        // 外站連結（例 線上訂房）存成一般頁面＋連結、不填路徑名稱：前台主選單不列「跳頁」類型。
        // 和上層同一頁的子頁（例 泡湯游泳＝飯店設施的頁面）也一樣，連結指向上層選單，內容只存一份。
        // url 可能已在上面被清成 null（和前面選單同一頁、但有子選單的上層），這時沒有要連過去的頁面，直接跳過，
        // 否則 PageKey(null) 會讓整個匯入失敗。
        var sameAsMenu = isLocal && url != null && state.LinkToMenu.Contains(SiteLayoutScanner.PageKey(url))
            ? SiteCrawlHtml.FindMenuRoute(state.Result, url) : null;
        var external = url != null && (!isLocal || sameAsMenu != null);
        var stem = RouteStem(url);
        var title = MenuTitle(link);
        siblings.Existing.TryGetValue(title, out var existing);
        var moved = false;
        var isHome = ReferenceEquals(link, state.HomeLink);
        if (existing == null && state.HiddenByDefault && state.Movable.Remove(title, out var orphan))
        {
            existing = orphan;
            moved = true;
        }
        // 每個網站只能有一個首頁：網站已有首頁選單時直接沿用它，不另建第二個（另建會被首頁重複的檢查擋下）。
        if (isHome && existing?.PageType != PageTypeEnum.首頁 && state.ExistingHome is { } currentHome)
        {
            existing = currentHome;
            moved = currentHome.FK_TopNodeId != parentId;
        }
        var dto = existing == null ? new MenuItemDto
        {
            VisibleHeader = true,
            VisibleFooter = true,
            VisibleTitle = true,
            SerNO = siblings.LastOrder == int.MaxValue ? int.MaxValue : ++siblings.LastOrder,
            FK_TopNodeId = parentId,
            FK_RootNodeId = rootId
        } : mapper.Map<MenuItemDto>(existing);

        dto.Title = title;
        if (moved)
        {
            dto.FK_TopNodeId = parentId;
            dto.FK_RootNodeId = rootId;
            dto.SerNO = siblings.LastOrder == int.MaxValue ? int.MaxValue : ++siblings.LastOrder;
        }
        if (!external && !isHome && link.RouterName is { } wanted
            && !string.Equals(dto.RouterName, wanted, StringComparison.OrdinalIgnoreCase))
        {
            // 使用者在確認畫面指定的路徑名稱；被其他選單用掉時不自動加流水號，沿用原本或自動產生的名稱並列出錯誤。
            if (state.Routes.Add(wanted)) dto.RouterName = wanted;
            else state.Result.AddError($"選單「{title}」的路徑名稱「{wanted}」已被其他選單使用，改用原本或自動產生的名稱。");
        }
        // 首頁下面固定改成 Home，不能先保留 import-…：否則名字被佔掉，偵測時推算給下一個選單的 import-…（例 import-tw-index-asp）
        // 匯入時就會被判成「已被其他選單使用」（2026-11-18 實測）。
        if (string.IsNullOrWhiteSpace(dto.RouterName) && !external && !isHome)
            dto.RouterName = Reserve(state.Routes, "import-" + stem, 50, index => $"-{index + 1}");
        if (isHome)
        {
            // 前台用 RouterName「Home」找首頁（後台選首頁類型時也固定填 Home）。
            dto.PageType = PageTypeEnum.首頁;
            dto.RouterName = HomeRouterName;
            dto.LinkUrl = null;
            dto.Target = false;
        }
        else if (existing == null || existing.PageType is PageTypeEnum.一般頁面 or PageTypeEnum.跳頁 or PageTypeEnum.結構頁面)
        {
            // 匯入只用一般頁面與首頁（使用者 2026-11-17 決定）：沒網址的下拉上層也是一般頁面，
            // 內容空白時前台主選單連結是 #，和結構頁面一樣點不進去。
            dto.PageType = PageTypeEnum.一般頁面;
            dto.LinkUrl = external ? sameAsMenu ?? url!.AbsoluteUri : null;
            dto.Target = external && sameAsMenu == null && link.OpensNewTab;
            if (external) dto.RouterName = "";
        }
        // 隱藏選單（含主選單沒有、另建的首頁）第一次建立時關閉；重新匯入時保留使用者後來的設定。
        dto.Visible = state.HiddenByDefault || isHome && state.HomeHidden ? existing?.Visible ?? false : true;
        dto.IsFromShelves = true;
        dto.ShowToMenu = true;
        // 子分類（舊站分類頁的子分類列表）：前台選單目錄只列 Visible 的子選單，所以要開啟，但不顯示於主選單（舊站主選單也沒有）。
        // 已經是「不顯示於主選單」的表示之前就這樣匯入過，保留使用者後來對 Visible 的設定。
        if (link.IsCategory)
        {
            dto.Visible = existing is { ShowToMenu: false } ? existing.Visible : true;
            dto.ShowToMenu = false;
        }
        return (dto, url != null);
    }

    private async Task<long?> SaveAsync(MenuItemDto dto, ImportState state, SiteMenuLink link, bool hasSource)
    {
        var response = await webMenu.CreateOrEdit(dto);
        var updated = dto.Id > 0;
        var id = dto.Id;
        if (updated ? !response.Success : !long.TryParse(response.Message, out id) || id <= 0)
        {
            logger.LogWarning("Site crawl menu save failed: {Reason}", response.Error);
            state.Result.AddError($"選單「{dto.Title}」儲存失敗，請查看 MVC 日誌。");
            return null;
        }
        if (!response.Success)
            state.Result.AddError($"選單「{dto.Title}」已建立，但儲存後處理失敗。");
        if (link.Url != null && SiteCrawlHttp.IsAllowed(link.Url, state.Host))
            state.ImportedUrls.Add(SiteLayoutScanner.Normalize(link.Url!).AbsoluteUri);
        if (link.SelectionKey != null) state.Result.MenuSelectionKeys[id] = link.SelectionKey;
        // 沒有舊站頁面的上層與外站連結沒有要匯入的內容，SourceUrl 留空，後續的內容／文章匯入就不會碰它們。
        state.Result.ImportedMenus.Add(new ImportedSiteMenu(id, dto.FK_TopNodeId, dto.Title!, link.Title,
            !hasSource || !string.IsNullOrEmpty(dto.LinkUrl) ? null : link.Url?.AbsoluteUri,
            dto.RouterName ?? "", dto.PageType));
        if (updated) state.Result.MenusUpdated++;
        else state.Result.MenusCreated++;
        return id;
    }

    private static string Reserve(HashSet<string> used, string value, int maxLength, Func<int, string> suffix)
    {
        for (var index = 0; ; index++)
        {
            var tail = index == 0 ? "" : suffix(index);
            var candidate = value[..Math.Min(value.Length, maxLength - tail.Length)] + tail;
            if (used.Add(candidate)) return candidate;
        }
    }

    private sealed class SiblingState
    {
        internal Dictionary<string, WebMenu> Existing { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> SavedTitles { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal int LastOrder { get; set; } = 499;
    }

    private sealed class ImportState(string host, SiteLayoutResult result, Func<Task> checkpoint)
    {
        internal string Host { get; } = host;
        internal SiteLayoutResult Result { get; } = result;
        internal Func<Task> Checkpoint { get; } = checkpoint;
        internal bool HiddenByDefault { get; init; }
        internal SiteMenuLink? HomeLink { get; set; }
        internal bool HomeHidden { get; set; }
        internal WebMenu? ExistingHome { get; set; }
        internal Dictionary<string, WebMenu> Movable { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> Routes { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> ImportedUrls { get; } = new(StringComparer.Ordinal);
        internal HashSet<string> LinkToMenu { get; } = new(StringComparer.Ordinal);
        private Dictionary<long, SiblingState> Siblings { get; } = [];

        internal SiblingState GetSiblings(long? parentId)
        {
            var key = parentId ?? 0;
            if (!Siblings.TryGetValue(key, out var siblings)) Siblings.Add(key, siblings = new SiblingState());
            return siblings;
        }
    }
}
