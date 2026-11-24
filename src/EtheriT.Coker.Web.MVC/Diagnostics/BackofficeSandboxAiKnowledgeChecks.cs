using System.Text.Json;
using EtheriT.Coker.Application.Shared.Ai;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Services.Ai;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiCanvasKnowledgeChecks(BackofficeSandbox box)
    {
        // Everything is under this scenario's existing temporary root. No application
        // assets, production cache, customer DB, HTTP request or external model is used.
        var env = box.Services.GetRequiredService<IWebHostEnvironment>();
        var project = Path.Combine(env.ContentRootPath, "knowledge-fixture");
        var shared = Path.Combine(project, "wwwroot", "Shared");
        Directory.CreateDirectory(Path.Combine(shared, "js"));
        var sourcePath = Path.Combine(shared, "js", "Widget.js");
        var deployedPath = Path.Combine(shared, "widgets.min.js");
        const string source = "function initialize() {\n const selector = '.current_layout';\n const layouts = { '.wide_layout': { slidesPerView: 4 } };\n}";
        await File.WriteAllTextAsync(sourcePath, source);
        await File.WriteAllTextAsync(deployedPath, source);
        await File.WriteAllTextAsync(Path.Combine(project, "bundles.json"), JsonSerializer.Serialize(new {
            scripts = new[] { new { output = "Shared/widgets.min.js", input = new[] { "wwwroot/Shared/js/Widget.js" } } }, styles = Array.Empty<object>()
        }));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["VirtualDirectory:Shared"] = shared
        }).Build();
        await AiMemoryStoreChecks(box);
        var knowledge = new CanvasKnowledgeService(env, config, box.Services.GetRequiredService<IAiMemoryStore>());
        CanvasAiRequest Request(long site = 41, string person = "7", string css = ".site_a { color:red }") => new() {
            WebsiteId = site, MemoryUserId = person, PermissionScope = "Page/Index", ReplyLocale = "zh-TW",
            Html = "<div id='widget' class='current_layout' data-source='keep'><div class='swiper'><span>保留</span></div></div>",
            Css = css, Resources = ["/Shared/widgets.min.js"], SelectedId = "widget"
        };
        var first = Request();
        var prepared = await knowledge.PrepareAsync(first, false, default);
        var original = prepared.Sources.Single(s => s.Path.Replace('\\', '/') == "js/Widget.js");
        Require(first.KnowledgeClasses.Contains("wide_layout") && original.Status == "needs-analysis" && original.Evidence.Length > 0,
            "Initial discovery must follow the actual bundle input and expose versioned source evidence rather than a hardcoded component table.");
        var cited = original.Evidence[0];
        await knowledge.RememberAsync(first, JsonSerializer.Serialize(new { sourceEvidence = new[] {
            new { key = original.Key, start = cited.Start, end = cited.End, facts = new[] { new { className = "wide_layout", parameter = "slidesPerView", value = "4" } } }
        } }), default);
        var next = await knowledge.PrepareAsync(Request(), false, default);
        Require(next.Sources.Single(s => s.Key == original.Key).Facts.Single().Value == "4" &&
            next.Sources.Single(s => s.Key == original.Key).Status == "source-cited-not-runtime-verified",
            "Validated source citations and literal analysis facts must be reusable without being labeled runtime test success.");
        Require(CanvasAiService.TrySourceBackedClassEdit(first, first.Html.Replace("current_layout", "wide_layout")) &&
            !CanvasAiService.TrySourceBackedClassEdit(first, first.Html.Replace("current_layout", "invented_layout")) &&
            !CanvasAiService.TrySourceBackedClassEdit(first, first.Html.Replace("current_layout", "wide_layout").Replace("保留", "刪改")) &&
            !CanvasAiService.TrySourceBackedClassEdit(first, first.Html.Replace("data-source='keep'", "data-source='other'").Replace("current_layout", "wide_layout")),
            "Source-backed class edits preserve custom metadata, every node and text; invented classes or unrelated mutations must not use the safe class-only exception.");
        await knowledge.RememberAsync(first, JsonSerializer.Serialize(new { sourceEvidence = new[] {
            new { key = original.Key, start = cited.Start, end = cited.End, facts = new[] { new { className = "wide_layout", parameter = "private_customer_request", value = "999" } } }
        } }), default);
        Require((await knowledge.PrepareAsync(Request(), false, default)).Sources.Single(s => s.Key == original.Key).Facts.Length == 1,
            "Model prose or uncited customer information cannot be promoted to shared analysis facts.");
        await File.WriteAllTextAsync(sourcePath, source.Replace("wide_layout", "compact_layout"));
        var changed = Request();
        var updated = await knowledge.PrepareAsync(changed, false, default);
        Require(updated.Sources.Single(s => s.Key == original.Key).SourceHash != original.SourceHash &&
            updated.Sources.Single(s => s.Key == original.Key).Facts.Length == 0 && !updated.Sources.Single(s => s.Key == original.Key).Classes.Contains("wide_layout") &&
            !updated.Sources.Single(s => s.Key == original.Key).Classes.Contains("compact_layout"),
            "Changed sources must invalidate old analysis automatically; a class absent from the deployed runtime bundle is not an executable source choice.");
        await File.WriteAllTextAsync(deployedPath, source.Replace("wide_layout", "compact_layout"));
        var deployed = Request(); await knowledge.PrepareAsync(deployed, false, default);
        Require(deployed.KnowledgeClasses.Contains("compact_layout"), "Updated deployed assets must be discovered on the next request without rebuilding a backend class table.");
        File.Delete(sourcePath);
        var removed = await knowledge.PrepareAsync(Request(), false, default);
        Require(removed.Sources.All(s => s.Path.Replace('\\', '/') != "js/Widget.js") && removed.Unavailable.Length > 0,
            "Deleted bundle inputs must not reuse their former analysis; deployed runtime evidence remains separately attributable.");
        File.Delete(deployedPath);
        var missing = Request(); var missingContext = await knowledge.PrepareAsync(missing, true, default);
        Require(missingContext.Sources.All(s => s.Scope != "system") && !missing.KnowledgeClasses.Contains("compact_layout") && missingContext.Unavailable.Contains("/Shared/widgets.min.js"),
            "Deleting a deployed asset must remove its cached capabilities from the next request.");
        var outside = Request(); outside.Resources = ["https://example.invalid/secret.js", "/Shared/../secrets.js", "/Shared/%2e%2e/secrets.js", "/upload/other-site/private.js"];
        Require((await knowledge.PrepareAsync(outside, true, default)).Sources.All(s => s.Scope != "system"),
            "Discovery must not fetch external URLs or read traversal/upload paths, including another site's resources.");
        CanvasAiRequest Personal(long site, string person, string css) => new() {
            WebsiteId = site, MemoryUserId = person, PermissionScope = "Page/Index", ReplyLocale = "zh-TW", Css = css,
            Html = "<div class='current_layout'></div>"
        };
        var firstPerson = Personal(101, "11", ".site_one { color: red; }"); firstPerson.ReplyLocale = "ja-JP";
        await knowledge.RememberReplyAsync(firstPerson, default);
        var same = Personal(101, "11", ".site_one { color: red; }"); await knowledge.PrepareAsync(same, false, default);
        var another = Personal(101, "12", ".site_two { color: blue; }"); var anotherContext = await knowledge.PrepareAsync(another, false, default);
        var anotherSite = Personal(102, "11", ".other_site { color: green; }"); await knowledge.PrepareAsync(anotherSite, false, default);
        Require(same.ConversationLocale == "ja-JP" && another.ConversationLocale == "" && anotherSite.ConversationLocale == "" &&
            anotherContext.SiteClasses.Contains("site_two") && !anotherContext.SiteClasses.Contains("site_one") &&
            !anotherSite.KnowledgeClasses.Contains("site_one"),
            "Personal reply habits remain private to the server-resolved user/site, and website classes are retrieved only from that exact authorized current snapshot.");
        var siteEvidence = (await knowledge.PrepareAsync(same, false, default)).Sources.Single(s => s.Scope == "website");
        await knowledge.RememberAsync(same, JsonSerializer.Serialize(new { sourceEvidence = new[] {
            new { key = siteEvidence.Key, start = siteEvidence.Evidence[0].Start, end = siteEvidence.Evidence[0].End,
                facts = new[] { new { className = "site_one", parameter = "color", value = "red" } } }
        } }), default);
        var sameWebsite = Personal(101, "12", ".site_one { color: red; }");
        var sharedSiteAnalysis = await knowledge.PrepareAsync(sameWebsite, false, default);
        var isolatedSiteAnalysis = await knowledge.PrepareAsync(Personal(102, "12", ".site_one { color: red; }"), false, default);
        Require(sharedSiteAnalysis.Sources.Single(s => s.Scope == "website").Facts.Single().Value == "red" &&
            isolatedSiteAnalysis.Sources.Single(s => s.Scope == "website").Facts.Length == 0 && sameWebsite.ConversationLocale == "",
            "Website source analysis can be shared only for the same authorized site/function/fingerprint; personal language preferences are never shared with that site's other users.");
        var serialized = JsonSerializer.Serialize(same);
        Require(!serialized.Contains("MemoryUserId") && !serialized.Contains("PermissionScope") && !serialized.Contains("KnowledgeClasses"),
            "Clients cannot supply identity, permission scope or validated source choices through the canvas request contract.");
        const string forged = """{"MemoryUserId":"other-person","PermissionScope":"other-function","KnowledgeClasses":["invented"],"SourceQueries":["private"],"KnowledgeExpanded":true,"MemoryVersions":{"forged":"00000000-0000-0000-0000-000000000001"}}""";
        var mvcInput = Newtonsoft.Json.JsonConvert.DeserializeObject<CanvasAiRequest>(forged)!;
        var systemInput = JsonSerializer.Deserialize<CanvasAiRequest>(forged)!;
        Require(new[] { mvcInput, systemInput }.All(i => i.MemoryUserId == "" && i.PermissionScope == "" && i.KnowledgeClasses.Count == 0 && i.SourceQueries.Length == 0 && !i.KnowledgeExpanded && i.MemoryVersions.Count == 0),
            "Both the MVC Newtonsoft formatter and System.Text.Json must ignore forged server-only identity, discovery state and execution choices.");
        await using var inspection = box.Services.CreateAsyncScope();
        var db = inspection.ServiceProvider.GetRequiredService<CokerDbContext>();
        Require(await db.AiMemoryEntries.AsNoTracking().Where(e => e.ScopeKind == 0)
            .AllAsync(e => !e.Payload.Contains("private_customer_request")), "Uncited model/customer content must never enter shared memory.");
    }
}
