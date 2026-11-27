using System.Net;
using System.Text.Json;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.SiteCrawl;
using EtheriT.Coker.Core.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task SiteCrawlRemovals(BackofficeSandbox sandbox, CancellationToken token)
    {
        Require(SiteCrawlRemovalService.IsDeletedStatus(HttpStatusCode.NotFound)
            && SiteCrawlRemovalService.IsDeletedStatus(HttpStatusCode.Gone)
            && !SiteCrawlRemovalService.IsDeletedStatus(HttpStatusCode.OK)
            && !SiteCrawlRemovalService.IsDeletedStatus(HttpStatusCode.Forbidden)
            && !SiteCrawlRemovalService.IsDeletedStatus(HttpStatusCode.Redirect)
            && !SiteCrawlRemovalService.IsDeletedStatus(null), "Only 404 and 410 may confirm a deleted page.");
        Require(SiteCrawlRemovalService.IsSectionRemoval(1, false)
            && !SiteCrawlRemovalService.IsSectionRemoval(0, false)
            && !SiteCrawlRemovalService.IsSectionRemoval(1, true), "An empty or matching section incorrectly confirmed removal.");

        var config = JObject.Parse("""{"custom":4,"sliders":[{"DesktopImage":"/gone.png","Enabled":true},{"DesktopImage":"/still.png","Enabled":true}]}""");
        Require(SiteCrawlRemovalService.DisableSlider(config, "/gone.png")
            && (bool?)config["sliders"]?[0]?["Enabled"] == false
            && (bool?)config["sliders"]?[1]?["Enabled"] == true
            && (int?)config["custom"] == 4
            && !SiteCrawlRemovalService.DisableSlider(config, "/missing.png"), "Slider disable changed the wrong item or discarded configuration.");
        var duplicate = JObject.Parse("""{"Sliders":[{"DesktopImage":"/duplicate.png","Enabled":true},{"DesktopImage":"/duplicate.png","Enabled":true}]}""");
        Require(!SiteCrawlRemovalService.DisableSlider(duplicate, "/duplicate.png")
            && (bool?)duplicate["Sliders"]?[0]?["Enabled"] == true, "Ambiguous image paths must not change the slider.");

        var root = sandbox.Services.GetRequiredService<IUploadPathResolver>().GetRootPath("diagnostics");
        var paths = new UploadPathResolver(Options.Create(new VirtualDirectory { UploadRoots = new() { ["fixture"] = root } }));
        System.IO.Directory.CreateDirectory(Path.Combine(root, "first"));
        System.IO.Directory.CreateDirectory(Path.Combine(root, "second"));
        var store = new SiteCrawlPendingStore(paths);
        var article = new SiteCrawlRemovalItem("1", "Article", "已刪除", "https://example.invalid/article", 5, null);
        var marquee = new SiteCrawlRemovalItem("2", "Marquee", "沒掃到", null, 6, null);
        await store.WriteRemovalsAsync("first", new(21, [article], [marquee]));
        var batch = await store.ReadRemovalsAsync("first", 21, token);
        Require(batch.Removed.SequenceEqual([article]) && batch.NotScanned.SequenceEqual([marquee])
            && (await store.ReadRemovalsAsync("first", 22, token)).Removed.Length == 0
            && (await store.ReadRemovalsAsync("second", 21, token)).Removed.Length == 0,
            "Removal evidence was mixed across statuses, tasks, or sites.");
    }

    private static async Task SiteCrawlConflicts(BackofficeSandbox sandbox, CancellationToken token)
    {
        var root = sandbox.Services.GetRequiredService<IUploadPathResolver>().GetRootPath("diagnostics");
        var paths = new UploadPathResolver(Options.Create(new VirtualDirectory { UploadRoots = new() { ["fixture"] = root } }));
        System.IO.Directory.CreateDirectory(Path.Combine(root, "first"));
        System.IO.Directory.CreateDirectory(Path.Combine(root, "second"));
        var store = new SiteCrawlPendingStore(paths);
        var item = new SiteCrawlPendingItem(21, SiteCrawlConflictKind.Menu, 5, "Menu|example.invalid/page",
            "https://example.invalid/page", "/first/page", "匯入頁面", "source", "<p>來源</p>", "",
            SiteCrawlFingerprint.ComputeWritten("<p>新站</p>", ""), SiteCrawlFingerprint.ComputeWritten("<p>草稿</p>", ""));
        await store.WriteAsync("first", item);
        var saved = (await store.ReadAsync("first", 21, token)).Single();
        Require(saved == item && saved.Id == "Menu-5", "The staged conflict did not preserve the original payload and fingerprints.");
        Require((await store.ReadAsync("first", 22, token)).Count == 0
            && (await store.ReadAsync("second", 21, token)).Count == 0, "A different task or site could see the pending item.");
        Require(saved.Matches("<p>新站</p>", "", "<p>草稿</p>", "")
            && !saved.Matches("<p>再改過的發布內容</p>", "", "<p>草稿</p>", "")
            && !saved.Matches("<p>新站</p>", "", "<p>草稿</p>", "a{color:red}"),
            "Published or draft changes after import did not invalidate the pending overwrite.");

        var statePath = Path.Combine(root, "first", "_crawl", "import-state.json");
        var originalHash = SiteCrawlFingerprint.ComputeWritten("<p>匯入原稿</p>", "");
        var source = new Uri(item.SourceUrl);
        var stateStore = new SiteCrawlStateStore(paths);
        var initialState = await stateStore.LoadAsync("first", source, token);
        await initialState.RecordTargetAsync(item.StateKey, item.NewId, source, item.Title, created: true);
        var contents = System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(statePath, token))!;
        var initialItem = contents["items"]![item.StateKey]!;
        initialItem["writtenHash"] = originalHash;
        initialItem["writtenDraftHash"] = originalHash;
        initialItem["hasVerifiedContentBaseline"] = true;
        initialItem["sourceHash"] = "first-source";
        await File.WriteAllTextAsync(statePath, contents.ToJsonString(), token);
        var updated = await stateStore.LoadAsync("first", source, token);
        updated.RememberConflict(item.StateKey, item.NewId, source, item.Title, "second-source");
        await updated.SaveAsync();
        using (var savedState = JsonDocument.Parse(await File.ReadAllTextAsync(statePath, token)))
        {
            var entry = savedState.RootElement.GetProperty("items").GetProperty(item.StateKey);
            var observedSource = entry.GetProperty("sourceHash").GetString();
            var savedHash = entry.GetProperty("writtenHash").GetString();
            var savedDraftHash = entry.GetProperty("writtenDraftHash").GetString();
            Require(observedSource == "second-source" && savedHash == originalHash && savedDraftHash == originalHash
                && entry.GetProperty("hasVerifiedContentBaseline").GetBoolean(),
                "Observing a conflict replaced the last written baseline.");
            Require(SiteCrawlReimport.Decide("second-source", observedSource, savedHash, "<p>手改</p>", "", false,
                    "<p>手改</p>", "", savedDraftHash) == SiteCrawlImportDecision.Unchanged
                && SiteCrawlReimport.Decide("third-source", observedSource, savedHash, "<p>手改</p>", "", false,
                    "<p>手改</p>", "", savedDraftHash) == SiteCrawlImportDecision.Conflict,
                "Keeping a conflict should skip the same source and protect the edit at the next source change.");
        }

        var site = await sandbox.Db.Websites.SingleAsync(token);
        sandbox.Db.WebMenus.AddRange(
            new WebMenu { Id = 5, FK_WebsiteId = site.Id, RouterName = "新路徑" },
            new WebMenu { Id = 6, FK_WebsiteId = site.Id + 1, RouterName = "other-site" });
        await sandbox.Db.SaveChangesAsync(token);

        var pending = sandbox.Services.GetRequiredService<SiteCrawlPendingStore>();
        await pending.WriteAsync(site.OrgName, item);
        await pending.WriteAsync(site.OrgName, item with { NewId = 6 });
        await pending.WriteAsync(site.OrgName, item with { Kind = SiteCrawlConflictKind.Article, NewId = 7 });
        await pending.WriteAsync(site.OrgName, item with { Kind = SiteCrawlConflictKind.Product, NewId = 8 });
        await pending.WriteAsync(site.OrgName, item with { Kind = SiteCrawlConflictKind.Footer, NewId = 9 });
        var service = sandbox.Services.GetRequiredService<SiteCrawlConflictService>();
        var links = (await service.ListAsync(21, token)).ToDictionary(x => x.Id, x => x.NewUrl);
        var currentRoute = Uri.EscapeDataString("新路徑");
        Require(links["Menu-5"] == $"https://example.invalid/diagnostics/{currentRoute}"
            && links["Menu-6"] == null
            && links["Article-7"] == "https://example.invalid/diagnostics/search/article/7"
            && links["Product-8"] == "https://example.invalid/diagnostics/search/product/8"
            && links["Footer-9"] == "https://example.invalid/diagnostics/Home",
            "Conflict links must use the current site's public routes and the mapped target, not the stored route.");

        site.DefaultUrl = "https://example.invalid/public/diagnostics/";
        await sandbox.Db.SaveChangesAsync(token);
        var menu = (await service.ListAsync(21, token)).Single(x => x.Id == "Menu-5");
        Require(menu.NewUrl == $"https://example.invalid/public/diagnostics/{currentRoute}",
            "A website prefix was duplicated or the public base path was lost.");
    }

    private static async Task SiteCrawlState(BackofficeSandbox sandbox, CancellationToken token)
    {
        var root = sandbox.Services.GetRequiredService<IUploadPathResolver>().GetRootPath("diagnostics");
        var paths = new UploadPathResolver(Options.Create(new VirtualDirectory { UploadRoots = new() { ["fixture"] = root } }));
        var firstRoot = System.IO.Directory.CreateDirectory(Path.Combine(root, "first")).FullName;
        var secondRoot = System.IO.Directory.CreateDirectory(Path.Combine(root, "second")).FullName;
        var firstPath = Path.Combine(firstRoot, "_crawl", "import-state.json");
        var secondPath = Path.Combine(secondRoot, "_crawl", "import-state.json");
        var store = new SiteCrawlStateStore(paths);
        var source = new Uri("https://www.example.invalid/old");
        var sameSource = new Uri("https://example.invalid/old");
        var otherSource = new Uri("https://other.invalid/old");
        const string menuKey = "Menu|www.example.invalid/old";
        const string detailKey = "Article|example.invalid|1";

        var first = await store.LoadAsync("first", source, token);
        await first.RecordTargetAsync(menuKey, 12, source, "頁面", created: true);
        Require(File.Exists(firstPath) && !File.Exists(firstPath + ".tmp"), "The new ID was not immediately persisted.");
        using (var json = JsonDocument.Parse(await File.ReadAllTextAsync(firstPath, token)))
        {
            var item = json.RootElement.GetProperty("items").GetProperty(menuKey);
            Require(item.GetProperty("sourceUrl").GetString() == source.AbsoluteUri
                && item.GetProperty("sourceTitle").GetString() == "頁面"
                && item.GetProperty("sourceHash").ValueKind == JsonValueKind.Null,
                "The source identity or the incomplete fingerprint state was lost.");
        }
        await first.RecordTargetAsync(detailKey, 34, source, "文章", created: false);
        using (var json = JsonDocument.Parse(await File.ReadAllTextAsync(firstPath, token)))
            Require(!json.RootElement.GetProperty("items").TryGetProperty(detailKey, out _), "An existing ID was not batched.");
        await first.SaveAsync();
        Require(File.Exists(firstPath + ".bak"), "The previous JSON was not retained.");
        var original = await File.ReadAllTextAsync(firstPath, token);

        var rejected = false;
        try { await store.LoadAsync("first", otherSource, token); }
        catch (SiteCrawlSourceMismatchException ex) { rejected = ex.Message.Contains("_crawl", StringComparison.Ordinal); }
        Require(rejected && original == await File.ReadAllTextAsync(firstPath, token), "The other source replaced the first site's records.");
        var second = await store.LoadAsync("second", otherSource, token);
        await second.RecordTargetAsync(menuKey, 56, otherSource, "第二站", created: true);
        Require(File.Exists(secondPath) && original == await File.ReadAllTextAsync(firstPath, token), "The site paths were not isolated.");

        await File.WriteAllTextAsync(firstPath, "{", token);
        var recovered = await store.LoadAsync("first", sameSource, token);
        await recovered.RecordTargetAsync(detailKey, 78, sameSource, "恢復", created: true);
        using (var json = JsonDocument.Parse(await File.ReadAllTextAsync(firstPath, token)))
        {
            var items = json.RootElement.GetProperty("items");
            Require(items.GetProperty(menuKey).GetProperty("newId").GetInt64() == 12
                && items.GetProperty(detailKey).GetProperty("newId").GetInt64() == 78, "Backup recovery lost a persisted ID.");
        }
        using (var backup = JsonDocument.Parse(await File.ReadAllTextAsync(firstPath + ".bak", token)))
            Require(backup.RootElement.GetProperty("items").TryGetProperty(menuKey, out _), "The corrupt primary replaced the backup.");
        await File.WriteAllTextAsync(firstPath, "{", token);
        await File.WriteAllTextAsync(firstPath + ".bak", "{}", token);
        var fresh = await store.LoadAsync("first", otherSource, token);
        await fresh.RecordTargetAsync(detailKey, 90, otherSource, "重建", created: true);
        using (var json = JsonDocument.Parse(await File.ReadAllTextAsync(firstPath, token)))
            Require(!json.RootElement.GetProperty("items").TryGetProperty(menuKey, out _), "Invalid state was carried into a new import.");

        using var redirectClient = new HttpClient(new SiteCrawlRedirectFixture(sameSource, source));
        var crawler = new SiteCrawlHttp(new SiteCrawlHttpFixture(redirectClient));
        var redirected = await crawler.GetAsync(sameSource, sameSource.IdnHost, false, 1024, token);
        using var redirectedContent = redirected.Content;
        Require(redirected.Url == source && redirectedContent.Length > 0
            && SiteCrawlHttp.IsAllowed(sameSource, source.IdnHost)
            && !SiteCrawlHttp.IsAllowed(otherSource, source.IdnHost)
            && !SiteCrawlHttp.IsAllowed(new Uri("https://news.example.invalid/old"), source.IdnHost)
            && !SiteCrawlHttp.IsAllowed(new Uri("https://www.example.invalid:444/old"), source.IdnHost),
            "The www redirect must be followed without allowing other hosts or ports.");

        var page = HtmlNode.CreateNode("<div><p>內容</p><span id='list_ctrl'>雜訊</span></div>");
        var samePage = HtmlNode.CreateNode("<div> <!--忽略--> <p> 內容 </p> </div>");
        var date = new DateTime(2026, 1, 2);
        var hash = SiteCrawlFingerprint.ComputeSource(page, "標題", date, "摘要", "https://example.invalid/image.png");
        Require(hash.Length == 64
            && hash == SiteCrawlFingerprint.ComputeSource(samePage, "標題", date, "摘要", "https://example.invalid/image.png")
            && hash != SiteCrawlFingerprint.ComputeSource(page, "新標題", date, "摘要", "https://example.invalid/image.png")
            && hash != SiteCrawlFingerprint.ComputeSource(page, "標題", date.AddDays(1), "摘要", "https://example.invalid/image.png")
            && hash != SiteCrawlFingerprint.ComputeSource(page, "標題", date, "新摘要", "https://example.invalid/image.png")
            && hash != SiteCrawlFingerprint.ComputeSource(page, "標題", date, "摘要", "https://example.invalid/other.png"),
            "The source fingerprint did not normalize markup or detect a metadata change.");
        Require(SiteCrawlFingerprint.ComputeWritten("&lt;p&gt;內容&lt;/p&gt;", "p{color:red}")
            != SiteCrawlFingerprint.ComputeWritten("&lt;p&gt;內容&lt;/p&gt;", "p{color:blue}"),
            "A CSS-only change did not change the stored content fingerprint.");

        var firstList = HtmlNode.CreateNode("""
            <div><p>分類介紹</p><form><input type="hidden" name="token" value="134274"></form>
            <div id="moduleCont"><a href="/page-a">甲</a><a href="/page-b">乙</a></div></div>
            """);
        var sameList = HtmlNode.CreateNode("""
            <div><p>分類介紹</p><form><input type="hidden" name="token" value="471293"></form>
            <div id="moduleCont"><a href="/page-b">乙</a><a href="/page-a">甲</a></div></div>
            """);
        var changedIntro = sameList.CloneNode(true);
        changedIntro.SelectSingleNode("./p").InnerHtml = "新介紹";
        var listHash = SiteCrawlFingerprint.ComputeListSource(firstList, "商品", "product", out var intro);
        Require(listHash == SiteCrawlFingerprint.ComputeListSource(sameList, "商品", "product", out _)
            && listHash != SiteCrawlFingerprint.ComputeListSource(changedIntro, "商品", "product", out _)
            && listHash != SiteCrawlFingerprint.ComputeListSource(firstList, "商品", "article", out _)
            && intro?.SelectSingleNode(".//*[@id='moduleCont']") == null && intro?.InnerText.Contains("分類介紹") == true,
            "A transient token or listing order changed the directory fingerprint, or a real introduction change was ignored.");
        var tokenA = HtmlNode.CreateNode("<div><input type='hidden' name='token' value='1'><input type='text' name='query' value='A'></div>");
        var tokenB = HtmlNode.CreateNode("<div><input type='hidden' name='token' value='2'><input type='text' name='query' value='A'></div>");
        var tokenC = HtmlNode.CreateNode("<div><input type='hidden' name='token' value='2'><input type='text' name='query' value='B'></div>");
        Require(SiteCrawlFingerprint.ComputeSource(tokenA, "快照") == SiteCrawlFingerprint.ComputeSource(tokenB, "快照")
            && SiteCrawlFingerprint.ComputeSource(tokenB, "快照") != SiteCrawlFingerprint.ComputeSource(tokenC, "快照"),
            "Snapshot fingerprints must ignore the hidden search token and keep regular input changes.");
    }

    private sealed class SiteCrawlHttpFixture(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class SiteCrawlRedirectFixture(Uri from, Uri to) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri == from)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.MovedPermanently) { Headers = { Location = to } });
            if (request.RequestUri == to)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<p>來源頁面</p>", System.Text.Encoding.UTF8, "text/html")
                });
            throw new InvalidOperationException("The crawler requested a URL outside the fixture.");
        }
    }

    private static void SiteCrawlReimportRules()
    {
        const string html = "<p>匯入內容</p>";
        const string css = "p{color:red}";
        var written = SiteCrawlFingerprint.ComputeWritten(html, css);
        Require(SiteCrawlReimport.Decide("new", "old", written, html, css, false, requiresManualReview: true)
            == SiteCrawlImportDecision.Conflict, "A legacy fingerprint was trusted on changed source content.");
        Require(SiteCrawlReimport.Decide("old", "old", written, "手動改過", css, false) == SiteCrawlImportDecision.Unchanged,
            "An unchanged source would overwrite a destination-only edit.");
        Require(SiteCrawlReimport.Decide("new", "old", written, html, css, false) == SiteCrawlImportDecision.Write,
            "An untouched destination did not accept a changed source.");
        Require(SiteCrawlReimport.Decide("new", "old", written, html, "p{color:blue}", false) == SiteCrawlImportDecision.Conflict,
            "A CSS-only destination edit did not cause a conflict.");
        Require(SiteCrawlReimport.Decide("new", null, null, html, css, true) == SiteCrawlImportDecision.Write,
            "The first import did not accept a same-title page.");
        Require(SiteCrawlReimport.Decide("new", null, null, html, css, false) == SiteCrawlImportDecision.Conflict
            && SiteCrawlReimport.Decide("new", null, null, null, css, false) == SiteCrawlImportDecision.Conflict,
            "A nonempty unmapped or interrupted target could be overwritten.");
        Require(SiteCrawlReimport.Decide("new", null, null, null, "", false) == SiteCrawlImportDecision.Write,
            "An empty later target could not be filled.");
        var importedAt = new DateTimeOffset(2026, 11, 26, 9, 55, 0, TimeSpan.FromHours(8));
        var reusedId = SiteCrawlReimport.WasRecreated(importedAt.AddDays(1).LocalDateTime, importedAt);
        Require(reusedId && SiteCrawlReimport.Decide("old", null, null, null, null, false) == SiteCrawlImportDecision.Write
            && !SiteCrawlReimport.WasRecreated(importedAt.AddDays(-1).LocalDateTime, importedAt)
            && SiteCrawlReimport.Decide("old", "old", written, null, null, false) == SiteCrawlImportDecision.Unchanged,
            "A recreated empty menu must be restored without repopulating an existing menu cleared by an editor.");
        const string draft = "<p>上次匯入的草稿</p>";
        const string manualDraft = "<p>手動修改的草稿</p>";
        var writtenDraft = SiteCrawlFingerprint.ComputeWritten(draft, css);
        Require(SiteCrawlReimport.Decide("new", "old", written, html, css, false, manualDraft, css, writtenDraft)
            == SiteCrawlImportDecision.Conflict, "A draft-only edit could be overwritten while published content matched.");
        Require(SiteCrawlReimport.Decide("old", "old", written, html, css, false, manualDraft, css, writtenDraft)
            == SiteCrawlImportDecision.Unchanged, "An unchanged source did not skip a manually edited draft.");
        Require(SiteCrawlReimport.Decide("new", "old", written, html, css, false, draft, css, writtenDraft)
            == SiteCrawlImportDecision.Write, "A matching published and draft fingerprint did not accept a changed source.");
        Require(SiteCrawlReimport.Decide("new", "old", written, html, css, false, draft, css) == SiteCrawlImportDecision.Conflict
            && SiteCrawlReimport.Decide("new", null, null, null, null, false, manualDraft) == SiteCrawlImportDecision.Conflict,
            "A missing draft baseline or draft-only content allowed a blind overwrite.");
        Require(SiteCrawlReimport.ShouldUpdate("舊站新標題", "舊站上次標題", "新站上次寫入標題", "新站上次寫入標題")
            && !SiteCrawlReimport.ShouldUpdate("舊站上次標題", "舊站上次標題", "新站上次寫入標題", "新站上次寫入標題")
            && !SiteCrawlReimport.ShouldUpdate("舊站新標題", "舊站上次標題", "人工標題", "新站上次寫入標題"),
            "The title rules did not distinguish the last observed source from the last imported title.");
        var old = new DateTime(2026, 1, 2);
        Require(SiteCrawlReimport.ShouldUpdate<DateTime?>(null, old, old, old)
            && !SiteCrawlReimport.ShouldUpdate(old.AddDays(1), old, old.AddDays(2), old)
            && !SiteCrawlReimport.ShouldUpdate("新摘要", "舊摘要", "人工摘要", "舊摘要"),
            "A source field removal or a manually edited field was handled incorrectly.");
    }
}
