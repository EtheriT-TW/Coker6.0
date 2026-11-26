using EtheriT.Coker.Web.MVC.Services.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiCanvasPlannerChecks(BackofficeSandbox box)
    {
        await AiCanvasKnowledgeChecks(box);
        Require(Controllers.api.CanvasAiController.ResolveReplyLocale("zh-tw", "ja-JP", "ja-JP") == "zh-TW" &&
            Controllers.api.CanvasAiController.ResolveReplyLocale("", "ja-JP;q=0.2,zh-TW;q=0.9,en;q=0", "vi-VN") == "zh-TW" &&
            Controllers.api.CanvasAiController.ResolveReplyLocale("zh-TW;ignore-instructions", "vi-VN", "ja-JP") == "vi-VN" &&
            Controllers.api.CanvasAiController.ResolveReplyLocale("", "*;q=0", "ja-JP") == "ja-JP",
            "Reply locale must prefer validated browser language, then weighted Accept-Language, with website locale only as a missing-browser fallback.");
        Require(Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Development", null) &&
            Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Local", System.Net.IPAddress.Loopback) &&
            Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Local", System.Net.IPAddress.IPv6Loopback) &&
            !Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Local", System.Net.IPAddress.Parse("192.0.2.1")) &&
            !Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Local", null) &&
            !Controllers.api.CanvasAiController.AllowsPlanDiagnostics("Production", System.Net.IPAddress.Loopback),
            "Diagnostics must support the repository's Local loopback launch profile without exposing data in Production or to remote Local clients.");
        var settings = box.Services.GetRequiredService<WebsiteAiSettingsService>();
        var current = await settings.GetAsync(default);
        await settings.SaveAsync(new() { WebsiteId = 999000001, Enabled = true, Provider = "claude", Model = "fixture-model", Revision = current.Revision }, default);
        const string original = "<div id='gallery' class='imageList' data-edit-template='template'><a id='item' href='/brand' data-link-type='link'><img id='logo' src='/logo.png' data-src='/logo.png'></a><a id='item2' href='/other'><img id='logo2' src='/logo.png'></a></div>";
        const string stylePlan = """{"editMode":"style","html":"","styles":[{"selector":"#gallery","properties":{"display":"grid","grid-template-columns":"repeat(5, minmax(0, 1fr))"}},{"selector":"#item","properties":{"width":"100%"}}],"textStyles":[],"summary":"已調整圖片排列","clarification":""}""";
        var contexts = new List<JsonElement>();
        var systems = new List<string>();
        Func<int, HttpResponseMessage> respond = _ => Reply(stylePlan);
        var http = new ModelListFixture((request, call) => {
            using var body = JsonDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            contexts.Add(JsonDocument.Parse(body.RootElement.GetProperty("messages")[0].GetProperty("content")[0].GetProperty("text").GetString()!).RootElement.Clone());
            systems.Add(body.RootElement.GetProperty("system").GetString()!);
            return respond(call);
        });
        static HttpResponseMessage Reply(string plan) => ModelResponse(JsonSerializer.Serialize(new {
            stop_reason = "end_turn", content = new[] { new { type = "text", text = plan } }
        }));
        var availability = box.Services.GetRequiredService<CanvasAiAvailability>();
        var service = new CanvasAiService(new CanvasAiClient(new AiClient(http, NullLogger<AiClient>.Instance), http, availability),
            box.Services.GetRequiredService<IConfiguration>(), settings, box.Services.GetRequiredService<WebsiteAiAvailability>());
        CanvasAiRequest Request(string prompt, string html = original) => new() {
            Scope = "selected", AutoDetect = true, SelectedId = "logo", Prompt = prompt, Html = html,
            WebsiteLocale = "zh-tw", ReplyLocale = "zh-tw", BrowserLocale = "zh-TW", StyleContext = "[]", History = "[]"
        };
        foreach (var prompt in new[] { "五個一排", "5个一排", "1行に5個並べてください", "Xếp 5 hình trên mỗi hàng", "Make the logos five per row", "窄一點，再均分看看" })
        {
            var before = http.Calls;
            var input = Request(prompt);
            var result = await service.Generate(input, default);
            Require(result.Status == 200 && http.Calls == before + 2 && input.ModelCalls == 2 && input.PreserveContent &&
                !result.HtmlChanged && result.Html == original && result.Styles[0].Properties["grid-template-columns"] == "repeat(5, minmax(0, 1fr))" &&
                contexts.Last().GetProperty("Prompt").GetString() == prompt && contexts.Last().GetProperty("SelectedId").GetString() == "logo" &&
                contexts.Last().GetProperty("WebsiteLocale").GetString() == "zh-tw",
                "Every language and unfamiliar wording must reach the same AI planner unchanged; scoped operations preserve the entire custom gallery.");
        }
        var traceEntries = new List<CanvasAiTraceEvent>();
        var traced = Request("調整排列");
        traced.TraceObserver = (entry, _) => { traceEntries.Add(entry); return Task.CompletedTask; };
        var traceCalls = http.Calls;
        respond = _ => {
            Require(traceEntries.Last().Stage == "model-request", "Progress must be emitted before awaiting the provider, not replayed after completion.");
            return Reply(stylePlan);
        };
        var tracedResult = await service.Generate(traced, default);
        Require(tracedResult.Status == 200 && http.Calls == traceCalls + 2 &&
            traceEntries.First().Stage == "settings" && traceEntries.Any(entry => entry.Stage == "resources") &&
            traceEntries.Count(entry => entry.Stage == "model-request") == 2 &&
            traceEntries.Count(entry => entry.Stage == "model-response") == 2 &&
            traceEntries.Any(entry => entry.Stage == "validation") &&
            traceEntries.Zip(traceEntries.Skip(1)).All(pair => pair.First.ElapsedMs <= pair.Second.ElapsedMs) &&
            !JsonSerializer.Serialize(traced).Contains("TraceObserver") && !Newtonsoft.Json.JsonConvert.SerializeObject(traced).Contains("TraceObserver"),
            "Request-scoped progress must preserve operations and call count, record ordered real events, and be impossible to supply via either HTTP formatter.");
        await traced.ReportAsync("fixture", "bounded", new string('x', 70000), default);
        Require(traceEntries.Last().Detail!.Length < 64100 && traceEntries.Last().Detail!.Contains("僅顯示"), "Detailed progress must be bounded and visibly identify clipping.");
        var traceContext = JsonSerializer.Serialize(new { Html = new string('x', 30000), Prompt = "四個一排", SelectedId = "gallery", ValidationFeedback = "Read actual source." });
        var traceProjection = CanvasAiTrace.DescribeRequest("plan", new string('s', 16000), traceContext);
        var traceRetry = CanvasAiTrace.DescribeRequest("plan", new string('s', 16000),
            JsonSerializer.Serialize(new { Html = new string('x', 30000), Prompt = "四個一排", SelectedId = "gallery", ValidationFeedback = "Use its supported initializer." }),
            new string('s', 16000), traceContext);
        Require(traceProjection.Length < 20000 && traceProjection.Contains("原長度") && traceProjection.Contains("四個一排") &&
            traceRetry.Contains("沿用前一輪") && traceRetry.Contains("Use its supported initializer.") &&
            !traceRetry.Contains(new string('x', 100)) && !traceRetry.Contains(new string('s', 100)) &&
            JsonDocument.Parse(traceContext).RootElement.GetProperty("Html").GetString()!.Length == 30000,
            "Trace requests must be readable bounded projections, show changed retry feedback and reference unchanged fields without altering actual model inputs.");
        var start = http.Calls;
        respond = call => Reply(call == start + 1
            ? stylePlan.Replace("已調整圖片排列", "将当前布局调整为每行四个项目。")
            : """{"editMode":"style","html":"","styles":[{"selector":"#outside","properties":{"color":"red"}}],"summary":"將目前版面調整為每排四個項目。"}""");
        var localizedRequest = Request("四個一排");
        var localized = await service.Generate(localizedRequest, default);
        Require(localized.Status == 200 && http.Calls == start + 2 && localizedRequest.ModelCalls == 2 &&
            localized.Message == "將目前版面調整為每排四個項目。" && localized.Styles[0].Selector == "#gallery" &&
            localized.Styles.All(r => r.Selector != "#outside") &&
            contexts.Last().GetProperty("UiMessageReview").GetProperty("Locale").GetString() == "zh-tw" &&
            contexts.Last().GetProperty("History").GetArrayLength() == 0,
            "A dedicated final locale pass must use the selected UI locale, excluding old dialogue and freezing all previously validated edit operations.");
        start = http.Calls;
        respond = call => Reply(call == start + 1 ? stylePlan : "not json");
        var languageFailure = await service.Generate(Request("調整排列"), default);
        Require(languageFailure.Status == 422 && languageFailure.Html == null && http.Calls == start + 2,
            "A failed message-localization pass must not return an applicable plan or the unreviewed model summary.");
        start = http.Calls;
        foreach (var alias in new[] { "#container", "#layout-container" })
        {
            var aliasStart = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new { editMode = "style", html = "", styles = new[] {
                new { selector = alias, properties = new Dictionary<string, string> { ["display"] = "grid", ["grid-template-columns"] = "repeat(4, minmax(0, 1fr))" } },
                new { selector = alias + " > a", properties = new Dictionary<string, string> { ["width"] = "100%" } }
            }, summary = "已調整為四欄" }));
            var aliasResult = await service.Generate(Request("四個一排"), default);
            Require(aliasResult.Status == 200 && aliasResult.Html == original && !aliasResult.HtmlChanged && http.Calls == aliasStart + 2 &&
                aliasResult.Styles.Count == 3 && aliasResult.Styles[0].Selector == "#gallery" &&
                contexts.Last().GetProperty("TargetAliases").GetProperty(alias).GetString() == "#gallery",
                "Observed #container/#layout-container model targets must resolve only through declared snapshot aliases to the actual root and its children.");
        }
        Require(!CanvasAiService.BuildTargetAliases("<div id='root'><div id='container'></div></div>", "root").ContainsKey("#container") &&
            !CanvasAiService.BuildTargetAliases("<div id='a'></div><div id='b'></div>", "a").ContainsKey("#container"),
            "Aliases must never override a real ID or guess a root for a multi-root fragment.");
        start = http.Calls;
        foreach (var (selector, reason) in new[] { ("#missing", "不在目前選取區塊"), ("#item:hover", "定位方式尚不支援") })
        {
            var failureStart = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new { editMode = "style", html = "", styles = new[] {
                new { selector, properties = new Dictionary<string, string> { ["width"] = "25%" } }
            } }));
            var diagnostic = await service.Generate(Request("平均排列"), default);
            Require(diagnostic.Status == 422 && !diagnostic.NeedsClarification && diagnostic.Html == null &&
                http.Calls == failureStart + 3 && diagnostic.Message.Contains(reason),
                "Unmatched targets and unsupported selector syntax must report distinct useful reasons after bounded replanning.");
        }
        var diagnosticInput = Request("測試診斷");
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#unknown-target","properties":{"width":"25%"}}]}""");
        await service.Generate(diagnosticInput, default);
        Require(diagnosticInput.PlanDiagnostics.Count == 3 && diagnosticInput.PlanDiagnostics.All(d =>
            d.Category == "targets-unmatched" && d.ProposedSelectors.Single() == "#unknown-target" && d.AvailableIds.Contains("gallery")) &&
            !JsonSerializer.Serialize(diagnosticInput).Contains("PlanDiagnostics"),
            "Bounded internal diagnostics must retain actual failed selectors and current IDs without entering request serialization or storing HTML/prompt/provider bodies.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":":scope > a","properties":{"width":"25%"}}],"summary":"已調整排列"}""");
        var scoped = await service.Generate(Request("調整排列"), default);
        Require(scoped.Status == 200 && scoped.Styles.Count == 2 && scoped.Styles.All(r => r.Selector is "#item" or "#item2") && http.Calls == start + 2,
            "A fragment-scoped direct child selector must resolve only to its two existing children without replacing custom markup.");
        foreach (var selector in new[] { "body #gallery > a", ".page-wrapper .imageList > a", "#page-outside #gallery > a" })
        {
            var pathStart = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new { editMode = "style", html = "", styles = new[] {
                new { selector, properties = new Dictionary<string, string> { ["width"] = "25%" } }
            }, summary = "已調整排列" }));
            var local = await service.Generate(Request("四個一排"), default);
            Require(local.Status == 200 && !local.HtmlChanged && local.Html == original && http.Calls == pathStart + 2 &&
                local.Styles.Count == 2 && local.Styles.All(r => r.Selector is "#item" or "#item2"),
                "Page-level ancestor prefixes must be rebased to the unique current fragment root/ID without affecting external elements or replacing metadata.");
        }
        start = http.Calls;
        respond = call => Reply(call == start + 1
            ? """{"editMode":"style","html":"","styles":[{"selector":"#outside","properties":{"width":"100%"}}]}""" : stylePlan);
        var repaired = Request("同じ幅で並べて");
        var repairedResult = await service.Generate(repaired, default);
        Require(repairedResult.Status == 200 && http.Calls == start + 3 && repaired.ModelCalls == 3 &&
            contexts.Last().GetProperty("ValidationFeedback").GetString()!.Length > 0 &&
            contexts.Last().GetProperty("Html").GetString() == original && contexts.Last().GetProperty("PlanningAttempt").GetInt32() == 2,
            "An out-of-scope plan must be returned to AI with validation feedback and the unchanged original, then apply only the corrected plan.");
        start = http.Calls;
        respond = _ => Reply(stylePlan);
        var noop = Request("調整效果");
        noop.StyleContext = """[{"selector":"#gallery","properties":{"display":"grid","grid-template-columns":"repeat(5, minmax(0, 1fr))"}},{"selector":"#item","properties":{"width":"100%"}}]""";
        Require((await service.Generate(noop, default)).Status == 422 && http.Calls == start + 3,
            "Repeated operations identical to observed state must be replanned then rejected, never reported as successful edits.");
        start = http.Calls;
        respond = call => Reply(call == start + 1 ? "not json" : call == start + 2
            ? """{"editMode":"style","html":"","styles":[{"selector":"#gallery","properties":{"color":"url(https://invalid)"}}]}""" : stylePlan);
        var third = Request("調整排列");
        Require((await service.Generate(third, default)).Status == 200 && http.Calls == start + 4 && third.ModelCalls == 4,
            "Malformed JSON and unsafe CSS must be corrected within a bounded three-call planning budget.");
        Require(contexts.Last().GetProperty("ValidationFailures").GetArrayLength() == 2,
            "Replanning must retain all prior validation failures rather than oscillating between errors.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#gallery","properties":{"display":"flex","flex-wrap":"wrap"}},{"selector":"#item","properties":{"width":"calc((100% - 3 * 16px) / 4)"}}],"summary":"已調整排列"}""");
        var arithmetic = await service.Generate(Request("平均排列"), default);
        Require(arithmetic.Status == 200 && http.Calls == start + 2 && arithmetic.Styles.Last().Properties["width"] == "calc((100% - 3 * 16px) / 4)",
            "Valid CSS multiplication for equal-width items and existing gaps must not be rejected by the value whitelist.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[],"summary":"已完成"}""");
        var emptyPlan = await service.Generate(Request("4個一排"), default);
        Require(emptyPlan.Status == 422 && emptyPlan.Html == null && http.Calls == start + 3 && emptyPlan.Message.Contains("沒有提供實際修改") &&
            !emptyPlan.Message.Contains("補充"), "Empty plans must be identified without asking the customer to explain an already sufficient request.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"content","html":"<div id='gallery'></div>","styles":[],"summary":"done"}""");
        var rejected = await service.Generate(Request("請美化這組圖片"), default);
        Require(rejected.Status == 422 && rejected.Html == null && http.Calls == start + 3,
            "Repeated destructive content proposals must stop at three calls without returning applicable HTML.");
        start = http.Calls;
        const string unnecessaryQuestion = """{"editMode":"style","html":"","styles":[],"clarification":"你希望这些元素如何排列？每个元素的宽度和间距如何？请提供更详细的信息。"}""";
        respond = call => Reply(call == start + 1 ? unnecessaryQuestion : stylePlan.Replace("repeat(5,", "repeat(4,"));
        var inferred = Request("變成4個一排");
        inferred.StyleContext = """[{"selector":"#gallery","properties":{"display":"grid","column-gap":"16px","row-gap":"20px"}},{"selector":"#item","properties":{"margin-left":"0px","width":"180px"}}]""";
        var inferredResult = await service.Generate(inferred, default);
        Require(inferredResult.Status == 200 && !inferredResult.NeedsClarification && inferredResult.Html == original &&
            inferredResult.Message == "已調整圖片排列" && http.Calls == start + 3 &&
            inferredResult.Styles[0].Properties["grid-template-columns"] == "repeat(4, minmax(0, 1fr))" &&
            contexts.Last().GetProperty("ValidationFeedback").GetString()!.Contains("Resolve unspecified widths") &&
            contexts.Last().GetProperty("ReviewProposal").GetString() == unnecessaryQuestion &&
            contexts.Last().GetProperty("StyleContext")[0].GetProperty("properties").GetProperty("column-gap").GetString() == "16px" &&
            systems.Last().Contains("Authoritative response locale: zh-tw"),
            "A technical Simplified-Chinese draft question must be reviewed internally with website locale and actual spacing before returning an executable localized plan.");
        start = http.Calls;
        var bootstrapBrands = "<div id='gallery' class='row row-cols-lg-6 g-3'>" + string.Concat(Enumerable.Range(0, 12)
            .Select(i => $"<div id='brand-{i}' class='col-6 col-lg-2'><img id='brand-image-{i}' src='/brands/{i}.png'></div>")) + "</div>";
        var gridStart = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#container","properties":{"display":"grid","grid-template-columns":"repeat(4, minmax(0, 1fr))"}}],"summary":"已調整四欄"}""");
        var gridRequest = Request("四個一排", bootstrapBrands);
        gridRequest.SelectedId = "brand-image-0";
        var gridResult = await service.Generate(gridRequest, default);
        Require(gridResult.Status == 200 && gridResult.Html == bootstrapBrands && !gridResult.HtmlChanged && http.Calls == gridStart + 2 &&
            gridResult.Styles.Count == 13 && gridResult.Styles.Skip(1).All(r => r.Properties["width"] == "auto" &&
                r.Properties["max-width"] == "100%" && r.Properties["min-width"] == "0"),
            "The observed root-only Grid plan must release inherited Bootstrap column widths on all 12 children instead of shrinking logos to one-sixth of a grid cell.");
        gridStart = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#container","properties":{"grid-template-columns":"repeat(4, minmax(0, 1fr))"}}],"summary":"已調整四欄"}""");
        var incompleteGrid = Request("換成四個一排", bootstrapBrands);
        incompleteGrid.SelectedId = "brand-image-0";
        var completedGrid = await service.Generate(incompleteGrid, default);
        Require(completedGrid.Status == 200 && http.Calls == gridStart + 2 && completedGrid.Styles.Count == 13 &&
            completedGrid.Styles[0].Properties["display"] == "grid" && completedGrid.Html == bootstrapBrands,
            "The actually observed track-only plan must enable Grid and release Bootstrap child widths instead of reporting success while remaining six-column Flex.");
        gridStart = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#container","properties":{"display":"flex","grid-template-columns":"repeat(4, 1fr)"}}],"summary":"完成"}""");
        var conflictingGrid = Request("修改排列", bootstrapBrands);
        conflictingGrid.SelectedId = "brand-image-0";
        Require((await service.Generate(conflictingGrid, default)).Status == 422 && http.Calls == gridStart + 3 &&
            conflictingGrid.PlanDiagnostics.All(d => d.Reason.Contains("non-Grid display")),
            "Explicitly contradictory Flex/Grid plans must be replanned and the concrete safe reason must remain in development diagnostics.");
        start = http.Calls;
        const string wrongCapacityQuestion = """{"editMode":"style","html":"","styles":[],"clarification":"目前容器寬度和元素數量不足，您要保留寬度還是自動調整？"}""";
        const string bootstrapPlan = """{"editMode":"style","html":"","styles":[{"selector":"#gallery > div","properties":{"width":"25%","max-width":"25%","flex-basis":"25%"}}],"summary":"已調整為每排四個，保留原有容器與間距"}""";
        respond = call => Reply(call == start + 1 ? wrongCapacityQuestion : bootstrapPlan);
        var structureRequest = Request("調整為四個一排", bootstrapBrands);
        structureRequest.SelectedId = "brand-image-0";
        var structureResult = await service.Generate(structureRequest, default);
        var structure = contexts.Last().GetProperty("EditorStructure");
        Require(structureResult.Status == 200 && structureResult.Html == bootstrapBrands && !structureResult.HtmlChanged &&
            structureResult.Styles.Count == 12 && http.Calls == start + 3 &&
            structure.GetProperty("Framework").GetString() == "Bootstrap 5.3" &&
            structure.GetProperty("Containers")[0].GetProperty("Kind").GetString() == "bootstrap-row" &&
            structure.GetProperty("Containers")[0].GetProperty("ItemCount").GetInt32() == 12 &&
            structure.GetProperty("Containers")[0].GetProperty("Items")[0].GetProperty("Classes")[1].GetString() == "col-lg-2" &&
            contexts.Last().GetProperty("ValidationFeedback").GetString()!.Contains("Current computed width is editable"),
            "Bootstrap rows must expose actual child counts and responsive classes; a false width/capacity question must be reviewed into scoped child width edits with the original markup preserved.");
        start = http.Calls;
        const string ambiguousGroups = "<section id='groups'><div id='group-a'><img id='photo-a' src='/a.png'></div><div id='group-b'><img id='photo-b' src='/b.png'></div></section>";
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[],"clarification":"要調整上方還是下方圖片？","clarificationTargets":["group-a","group-b"]}""");
        var ambiguousRequest = Request("改一下", ambiguousGroups);
        ambiguousRequest.SelectedId = "groups";
        var question = await service.Generate(ambiguousRequest, default);
        Require(question.NeedsClarification && question.Html == null && http.Calls == start + 3 && question.Message == "要調整上方還是下方圖片？",
            "Only a still-necessary customer-visible choice may be returned after the draft-question review, with no edits applied.");
        start = http.Calls;
        const string repeatedQuestion = """{"editMode":"style","html":"","styles":[],"clarification":"請問項目是圖片還是文字，及希望如何對齊？"}""";
        respond = call => Reply(call <= start + 2 ? repeatedQuestion : stylePlan);
        var currentRequest = Request("修改為三個一排");
        currentRequest.History = """[{"role":"assistant","content":"請確認是文字還是圖片，需不需要置中？"}]""";
        var currentResult = await service.Generate(currentRequest, default);
        Require(currentResult.Status == 200 && !currentResult.NeedsClarification && http.Calls == start + 4 &&
            contexts.Last().GetProperty("ClarificationRejection").GetString()!.Contains("CURRENT") &&
            contexts.Last().GetProperty("Html").GetString() == original,
            "Repeated questions about currently visible item types/alignment must not reach the customer; the planner must use the current snapshot despite old assistant questions.");
        start = http.Calls;
        respond = _ => Reply(repeatedQuestion);
        var noQuestion = await service.Generate(Request("三個一排"), default);
        Require(noQuestion.Status == 422 && !noQuestion.NeedsClarification && noQuestion.Html == null && http.Calls == start + 3,
            "A repeated ungrounded question must not be presented as a necessary customer decision after retry exhaustion.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[],"clarification":"哪一組？","clarificationTargets":["group-a","group-b"]}""");
        var alreadySelected = Request("調整", ambiguousGroups);
        alreadySelected.SelectedId = "photo-a";
        Require(!(await service.Generate(alreadySelected, default)).NeedsClarification && http.Calls == start + 3,
            "Selecting an image in one candidate group resolves that target choice without another question.");
        start = http.Calls;
        respond = _ => Reply(stylePlan.Replace("已調整圖片排列", "画像の配置を調整しました"));
        var japanese = Request("变成五个一排");
        japanese.WebsiteLocale = "ja-JP";
        japanese.ReplyLocale = "ja-JP";
        var japaneseResult = await service.Generate(japanese, default);
        Require(japaneseResult.Status == 200 && japaneseResult.Message == "画像の配置を調整しました" && http.Calls == start + 2 &&
            systems.Last().Contains("Authoritative response locale: ja-JP") && contexts.Last().GetProperty("WebsiteLocale").GetString() == "ja-JP",
            "When a fixture supplies no language decision, final UI review must retain the validated fallback reply locale independently of website HTML.");
        start = http.Calls;
        const string japaneseHtml = "<h2 id='title'>ようこそ</h2>";
        respond = call => Reply(call <= start + 2
            ? JsonSerializer.Serialize(new { editMode = "content", html = japaneseHtml, styles = Array.Empty<object>(), summary = "見出しを更新しました" })
            : """{"editMode":"content","html":"<h2 id='title'>歡迎光臨</h2>","styles":[],"summary":"已更新日文標題。"}""");
        var mixedLocales = Request("請改成日文歡迎標題", "<h2 id='title'>旧見出し</h2>");
        mixedLocales.SelectedId = "title";
        mixedLocales.WebsiteLocale = "ja-JP";
        mixedLocales.ReplyLocale = "zh-TW";
        var mixedResult = await service.Generate(mixedLocales, default);
        Require(mixedResult.Status == 200 && mixedResult.Html == japaneseHtml && mixedResult.Message == "已更新日文標題。" &&
            http.Calls == start + 3 && contexts.Last().GetProperty("WebsiteLocale").GetString() == "ja-JP" &&
            contexts.Last().GetProperty("ReplyLocale").GetString() == "zh-TW" &&
            contexts.Last().GetProperty("UiMessageReview").GetProperty("Locale").GetString() == "zh-TW" &&
            contexts.Last().GetProperty("ApprovedPlan").GetProperty("html").GetString() == "" &&
            !contexts.Last().TryGetProperty("EditorStructure", out _) && !contexts.Last().TryGetProperty("ResourceKnowledge", out _),
            "A Taiwan browser maintaining a Japanese site must receive a Taiwan reply while approved HTML remains Japanese; UI localization cannot replace the HTML with Chinese.");
        start = http.Calls;
        respond = _ => new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("fixture") };
        Require((await service.Generate(Request("調整"), default)).Status == 429 && http.Calls == start + 1,
            "Quota/transport failures must not be treated as replannable edits.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"content","html":"<h2 id='title'>新的標題</h2>","styles":[],"summary":"已修改標題"}""");
        var contentRequest = Request("把標題改成新的標題", "<h2 id='title'>原標題</h2>");
        contentRequest.SelectedId = "title";
        var content = await service.Generate(contentRequest, default);
        Require(content.Status == 200 && !contentRequest.PreserveContent && http.Calls == start + 3 &&
            contexts.Last().GetProperty("ValidationFeedback").GetString()!.Contains("Review"),
            "Content changes must receive an additional AI review against the original request before returning replacement content.");
        start = http.Calls;
        const string pendingLink = "<a id='label'>品牌入口</a>";
        respond = _ => Reply(JsonSerializer.Serialize(new {
            editMode = "content", html = pendingLink, styles = Array.Empty<object>(),
            summary = "已改成連結，尚未設定目的網址。", clarification = ""
        }));
        var linkRequest = Request("調整為連結", "<span id='label'>品牌入口</span>");
        linkRequest.SelectedId = "label";
        var linkResult = await service.Generate(linkRequest, default);
        Require(linkResult.Status == 200 && linkResult.Html == pendingLink &&
            !linkRequest.PreserveContent && http.Calls == start + 3 &&
            systems.Last().Contains("WITHOUT an href attribute"),
            "A selected Span can become an editable link without inventing a destination, retaining its ID and text after content review.");
        start = http.Calls;
        respond = _ => Reply(stylePlan[..^1] + ",\"replyLocale\":\"ja-JP\",\"replyLanguageBasis\":\"input\"}");
        var languageSwitch = Request("画像を一行に五つ並べてください");
        var languageSwitchResult = await service.Generate(languageSwitch, default);
        Require(languageSwitchResult.Status == 200 && languageSwitch.ReplyLocale == "ja-JP" && http.Calls == start + 2 &&
            contexts.Last().GetProperty("UiMessageReview").GetProperty("Locale").GetString() == "ja-JP" &&
            contexts.Last().GetProperty("WebsiteLocale").GetString() == "zh-tw",
            "An AI-classified substantive input language can override the browser default for UI review without changing the website content locale.");
        start = http.Calls;
        respond = _ => Reply(stylePlan[..^1] + ",\"replyLocale\":\"ja-JP\",\"replyLanguageBasis\":\"continuity\"}");
        var shortReply = Request("OK"); shortReply.ConversationLocale = "ja-JP";
        Require((await service.Generate(shortReply, default)).Status == 200 && shortReply.ReplyLocale == "ja-JP" &&
            contexts.Last().GetProperty("ConversationLocale").GetString() == "ja-JP",
            "Ambiguous short replies carry the current conversation locale to AI rather than forcing a browser-language reset.");
        foreach (var (locale, basis, expectedLocale) in new[] {
            ("vi-VN", "explicit", "vi-VN"), ("zh", "input", "zh-TW"), ("ja-JP;ignore", "explicit", "zh-tw")
        })
        {
            // Join two JSON objects without changing the executable plan's fields.
            respond = _ => Reply(stylePlan[..^1] + "," + JsonSerializer.Serialize(new { replyLocale = locale, replyLanguageBasis = basis })[1..]);
            var localeRequest = Request("請依指定語言回覆");
            Require((await service.Generate(localeRequest, default)).Status == 200 && localeRequest.ReplyLocale == expectedLocale,
                "Explicit locale choices are accepted, neutral same-language detection preserves regional usage, and malformed locale labels cannot enter system instructions.");
        }
        start = http.Calls;
        const string directoryHtml = "<div id='directory' class='catalog_frame' data-dirid='12'><div id='cards' class='catalog type4'><div id='hidden-template' class='templatecontent'><a class='template col'>商品</a></div></div></div>";
        const string directoryPlan = """{"editMode":"style","html":"","styles":[{"selector":"#cards","properties":{"display":"grid","grid-template-columns":"repeat(4, minmax(0, 1fr))"}}],"summary":"已調整目錄卡片排列。"}""";
        respond = call => Reply(call == start + 1 ? directoryPlan.Replace("#cards", "#directory") : directoryPlan);
        var directoryRequest = Request("調整為四欄位", directoryHtml); directoryRequest.SelectedId = "directory";
        var directoryResult = await service.Generate(directoryRequest, default);
        Require(directoryResult.Status == 200 && directoryResult.Html == directoryHtml && http.Calls == start + 3 &&
            directoryResult.Styles.Any(r => r.Selector == "#cards > .template" && r.Properties["width"] == "auto") &&
            directoryResult.Styles.All(r => r.Selector != "#hidden-template") &&
            directoryRequest.PlanDiagnostics.Any(d => d.Reason.Contains("outer frame")),
            "A directory outer-frame grid must be replanned onto its actual catalog; scoped runtime-card widths persist without rewriting data metadata or styling the hidden template.");
        Require(!CanvasAiStyleNormalizer.IsDirectoryItemsSelector("#cards > .template", "<div id='cards' class='catalog'></div>") &&
            !CanvasAiStyleNormalizer.IsDirectoryItemsSelector("#cards > .template, body", directoryHtml),
            "Runtime selectors must remain restricted to the exact direct-card selector of an existing directory catalog.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"content","html":"","styles":[],"summary":"","clarification":"這個連結要前往哪個網址？","missingInput":"link-destination","clarificationTargets":["label"]}""");
        var missingDestination = Request("設定目的網址", "<a id='label'>品牌入口</a>"); missingDestination.SelectedId = "label";
        Require((await service.Generate(missingDestination, default)).NeedsClarification && http.Calls == start + 3,
            "A grounded missing destination can be requested without fabricating competing layout targets or inventing a URL.");
        start = http.Calls;
        respond = call => Reply(JsonSerializer.Serialize(new {
            editMode = "content", html = pendingLink, styles = Array.Empty<object>(),
            summary = "已建立連結，請在設定視窗完成目的網址。", clarification = "",
            settingsActions = new[] { new { kind = "link", targetId = call <= start + 2 ? "label" : "outside" } }
        }));
        var linkHandoff = Request("調整為連結", "<span id='label'>品牌入口</span>");
        linkHandoff.SelectedId = "label"; linkHandoff.AvailableSettings = ["link"];
        var linkHandoffResult = await service.Generate(linkHandoff, default);
        Require(linkHandoffResult.Status == 200 && linkHandoffResult.Html == pendingLink && http.Calls == start + 3 &&
            linkHandoffResult.SettingsActions.Single() == new CanvasAiSettingsAction("link", "label") &&
            contexts.Last().GetProperty("AvailableSettings")[0].GetString() == "link",
            "A safe link structure edit can hand off to the existing link dialog; final UI review cannot replace the validated settings target.");
        foreach (var (kind, target, markup) in new[] {
            ("link", "label", pendingLink), ("directory", "directory", directoryHtml)
        })
        {
            start = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new {
                editMode = "style", html = "", styles = Array.Empty<object>(), summary = "",
                clarification = "要使用哪個設定？請在設定視窗選擇。",
                settingsActions = new[] { new { kind, targetId = target } }
            }));
            var handoff = Request("請協助設定", markup); handoff.SelectedId = target; handoff.AvailableSettings = [kind];
            var handoffResult = await service.Generate(handoff, default);
            Require(handoffResult.NeedsClarification && handoffResult.Html == null && http.Calls == start + 3 &&
                handoffResult.SettingsActions.Single() == new CanvasAiSettingsAction(kind, target),
                "Existing link/directory settings can resolve missing customer choices through a grounded dialog action without rewriting HTML or inventing competing targets.");
        }
        foreach (var (kind, target, available) in new[] {
            ("link", "outside", new[] { "link" }), ("directory", "label", new[] { "directory" }),
            ("execute-script", "label", new[] { "link" }), ("link", "label", Array.Empty<string>())
        })
        {
            start = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new {
                editMode = "style", html = "", styles = Array.Empty<object>(),
                clarification = "要使用哪個設定？", settingsActions = new[] { new { kind, targetId = target } }
            }));
            var unsafeHandoff = Request("設定", pendingLink); unsafeHandoff.SelectedId = "label"; unsafeHandoff.AvailableSettings = available;
            var unsafeResult = await service.Generate(unsafeHandoff, default);
            Require(!unsafeResult.NeedsClarification && unsafeResult.SettingsActions.Count == 0 && unsafeResult.Html == null &&
                http.Calls == start + 3, "Unavailable, arbitrary, incompatible or out-of-boundary settings actions must be rejected without opening any UI.");
        }
        start = http.Calls;
        const string carouselHtml = "<div id='directory' class='catalog_frame' data-dirid='12'><div id='carousel' class='six_swiper'><div id='viewport' class='swiper'><div id='cards' class='catalog swiper-wrapper'><div id='template' class='templatecontent'><div id='card' class='template swiper-slide'>商品</div></div></div></div></div></div>";
        var classPlan = JsonSerializer.Serialize(new {
            editMode = "content", html = carouselHtml.Replace("six_swiper", "four_swiper"), summary = "已調整輪播版型。", styles = Array.Empty<object>(),
            sourceEvidence = new[] { new { key = "fixture-source", start = 1, end = 1 } }
        });
        respond = call => Reply(call == start + 1 ? """{"editMode":"style","html":"","styles":[{"selector":"#catalog","properties":{"grid-template-columns":"repeat(4,1fr)"}}],"summary":"完成"}""" : classPlan);
        var carouselRequest = Request("調整為四欄位", carouselHtml); carouselRequest.SelectedId = "directory";
        carouselRequest.KnowledgeClasses.Add("four_swiper");
        carouselRequest.KnowledgeSources = [new("fixture-source", "/Shared/widgets.min.js", "js/Widget.js", "source-hash", "runtime-hash",
            ["four_swiper"], [], [new(1, 1, "$('.four_swiper').each(function(){ slidesPerView: 4 });")], "source-cited")];
        var carouselResult = await service.Generate(carouselRequest, default);
        Require(carouselResult.Status == 200 && http.Calls == start + 4 && carouselResult.Html == carouselHtml.Replace("six_swiper", "four_swiper") &&
            carouselResult.Styles.Count == 0 && carouselRequest.PlanDiagnostics.Any(d => d.Reason.Contains("JS-driven carousel")),
            "A carousel Grid proposal must be replanned into a source-cited class-only edit, preserving all data-source and template metadata without a fixed backend count table.");
        start = http.Calls;
        var firstPassDelta = JsonSerializer.Serialize(new {
            editMode = "content", html = "", styles = Array.Empty<object>(), summary = "已切換既有配置。",
            classChanges = new[] { new { targetId = "carousel", remove = new[] { "six_swiper" }, add = new[] { "four_swiper" } } },
            sourceEvidence = new[] { new { key = "not-a-current-source", start = 9999, end = 10000 } }
        });
        respond = _ => Reply(firstPassDelta);
        var firstPassRequest = Request("四個一排", carouselHtml); firstPassRequest.SelectedId = "directory";
        var citationLines = Enumerable.Repeat("// initializer fixture", 900).ToArray();
        citationLines[500] = "$('.four_swiper').each(function(){ const slidesPerView = 4; });";
        citationLines[585] = "$('.six_swiper').each(function(){ const slidesPerView = 6; });";
        var generatedRanges = CanvasKnowledgeService.SelectSourceEvidence(string.Join('\n', citationLines),
            new HashSet<string>(StringComparer.Ordinal) { "six_swiper" }, [], true,
            new HashSet<string>(StringComparer.Ordinal) { "six_swiper" });
        firstPassRequest.KnowledgeClasses.Add("four_swiper");
        firstPassRequest.KnowledgeSources = [new("generated-source", "/Shared/runtime.js", "runtime.js", "source-hash", "runtime-hash",
            ["four_swiper", "six_swiper"], [], generatedRanges, "needs-analysis")];
        var firstPassResult = await service.Generate(firstPassRequest, default);
        Require(firstPassResult.Status == 200 && http.Calls == start + 3 && firstPassRequest.PlanDiagnostics.Count == 0 &&
            firstPassResult.Html == carouselHtml.Replace("six_swiper", "four_swiper") &&
            contexts[^3].GetProperty("ComponentExecution").GetProperty("InitializerTargets")[0].GetProperty("Id").GetString() == "carousel" &&
            contexts[^3].GetProperty("ComponentExecution").GetProperty("RuntimeStyleTargets").EnumerateArray().Any(selector => selector.GetString() == "#carousel") &&
            contexts[^3].GetProperty("ComponentExecution").GetProperty("RuntimeStyleTargets").EnumerateArray().Any(selector => selector.GetString() == "#cards > .template") &&
            systems[^3].Contains("Do not regenerate HTML for this operation"),
            "The first planning request must identify the actual initialization owner and allow a compact source-backed delta without regenerating directory HTML or failed repair rounds.");
        Require(generatedRanges.All(range => range.End - range.Start <= CanvasSourceRange.MaxLineSpan) &&
            contexts[^2].GetProperty("ReviewProposal").GetString()!.Contains("generated-source") &&
            !contexts[^2].GetProperty("ReviewProposal").GetString()!.Contains("not-a-current-source"),
            "Nonempty but invented model citations must not suppress automatic citations from actual source excerpts; only current citations survive content review.");
        start = http.Calls;
        var missingExcerptRequest = Request("四個一排", carouselHtml); missingExcerptRequest.SelectedId = "directory";
        missingExcerptRequest.KnowledgeClasses.Add("four_swiper");
        missingExcerptRequest.KnowledgeSources = firstPassRequest.KnowledgeSources.Select(source => source with { Evidence = [] }).ToList();
        var missingExcerptResult = await service.Generate(missingExcerptRequest, default);
        Require(missingExcerptResult.Status == 422 && missingExcerptResult.Html == null && http.Calls == start + 3 &&
            missingExcerptRequest.PlanDiagnostics.Any(entry => entry.Reason.Contains("four_swiper: sources=1, matching excerpts=0")),
            "A known class with no supporting source excerpt must still be rejected and explain what evidence is missing, rather than accepting a fabricated citation.");
        Require(!CanvasAiService.TrySourceBackedClassEdit(firstPassRequest,
            carouselHtml.Replace("six_swiper", "four_swiper").Replace("data-dirid='12'", "data-dirid='13'"), out var metadataFailure) && metadataFailure.Contains("Non-class attributes") &&
            !CanvasAiService.TrySourceBackedClassEdit(firstPassRequest, carouselHtml.Replace("six_swiper", "undeployed_class"), out var unavailableFailure) && unavailableFailure.Contains("undeployed"),
            "A non-class mutation must be distinguished from an unavailable deployed class instead of being mislabeled as missing source citations.");
        foreach (var approveReview in new[] { true, false })
        {
            start = http.Calls;
            var invalidGrid = """{"editMode":"style","html":"","styles":[{"selector":"#carousel","properties":{"grid-template-columns":"repeat(4,1fr)"}}],"summary":"完成"}""";
            var lateDelta = JsonSerializer.Serialize(new {
                editMode = "content", html = "", styles = Array.Empty<object>(), summary = "已切換輪播版型。",
                classChanges = new[] { new { targetId = "carousel", remove = new[] { "six_swiper" }, add = new[] { "four_swiper" } } }
            });
            respond = call => Reply(call <= start + 2 ? invalidGrid : call == start + 3 ? lateDelta : approveReview ? classPlan : invalidGrid);
            var lateRequest = Request("四個一排", carouselHtml); lateRequest.SelectedId = "directory";
            lateRequest.KnowledgeClasses.Add("four_swiper"); lateRequest.KnowledgeSources = carouselRequest.KnowledgeSources;
            var lateEvents = new List<CanvasAiTraceEvent>();
            lateRequest.TraceObserver = (entry, _) => { lateEvents.Add(entry); return Task.CompletedTask; };
            var lateResult = await service.Generate(lateRequest, default);
            Require(lateEvents.Any(entry => entry.Stage == "review-required") &&
                lateEvents.Any(entry => entry.Stage == "validation" && entry.Message.Contains("仍需內容複核")) &&
                (approveReview
                    ? lateResult.Status == 200 && lateResult.Html == carouselHtml.Replace("six_swiper", "four_swiper") && http.Calls == start + 5
                    : lateResult.Status == 422 && lateResult.Html == null && http.Calls == start + 4),
                "A first valid third-round class delta must receive exactly one final review and UI localization; a rejected review must stop without applying or another repair loop.");
        }
        foreach (var selector in new[] { "#directory", "#carousel", "#cards" })
        foreach (var properties in new[] {
            new Dictionary<string, string> { ["flex-wrap"] = "wrap", ["justify-content"] = "space-between" },
            new Dictionary<string, string> { ["display"] = "flex", ["justify-self"] = "stretch", ["aspect-ratio"] = "auto" }
        })
        {
            start = http.Calls;
            respond = _ => Reply(JsonSerializer.Serialize(new {
                editMode = "style", html = "", summary = "已調整為四個一排。", styles = new[] {
                    new { selector, properties }
                }
            }));
            var ineffectiveFlex = Request("四個一排", carouselHtml.Replace("catalog swiper-wrapper", "catalog").Replace("template swiper-slide", "template")); ineffectiveFlex.SelectedId = "directory";
            var ineffectiveResult = await service.Generate(ineffectiveFlex, default);
            Require(ineffectiveResult.Status == 422 && ineffectiveResult.Html == null && http.Calls == start + 3 &&
                ineffectiveFlex.PlanDiagnostics.All(entry => entry.Reason.Contains("flex-wrap/justify-content does not configure")),
                "Flex declarations on the initialized directory frame, owner or slide wrapper must not be reported as a successful slides-per-view change.");
        }
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#gallery","properties":{"flex-wrap":"wrap","justify-content":"space-between"}}],"summary":"已調整一般版面。"}""");
        var ordinaryFlexResult = await service.Generate(Request("調整一般圖片區塊排列"), default);
        Require(ordinaryFlexResult.Status == 200 && http.Calls == start + 2, "Ordinary non-initialized layouts must retain their existing Flex styling capability.");
        foreach (var includeAssertion in new[] { false, true })
        {
            start = http.Calls;
            var assertedPlan = System.Text.Json.Nodes.JsonNode.Parse(firstPassDelta)!.AsObject();
            if (includeAssertion) assertedPlan["runtimeAssertions"] = JsonSerializer.SerializeToNode(new[] { new { targetId = "carousel", value = "4", minWidth = 992 } });
            respond = call => Reply(call == start + 1 ? """{"kind":"columns","targetId":"carousel","value":"4"}""" : assertedPlan.ToJsonString());
            var observed = Request("四個一排", carouselHtml); observed.SelectedId = "directory";
            observed.RuntimeContext = """[{"targetId":"carousel","slidesPerView":6}]""";
            observed.KnowledgeClasses.Add("four_swiper"); observed.KnowledgeSources = firstPassRequest.KnowledgeSources;
            var assertedResult = await service.Generate(observed, default);
            Require(http.Calls == start + 4 && observed.ModelCalls == 4 && observed.ExecutionGoal?.Value == "4" && (includeAssertion
                ? assertedResult.Status == 200 && assertedResult.RuntimeAssertions.Single().Value == "4"
                : assertedResult.Status == 422 && assertedResult.Html == null),
                "Observed runtime proposals need scoped executable assertions; valid assertions must survive review/localization without replacing approved operations.");
        }
        foreach (var prompt in new[] { "四個一排", "1行に4個", "Mỗi hàng 4 mục" })
        {
            start = http.Calls;
            respond = call => Reply(call == start + 1 ? """{"kind":"columns","targetId":"carousel","value":"4"}""" :
                """{"html":"","editMode":"style","summary":"完成四欄","styles":[{"selector":"#carousel","properties":{"font-size":"18px"}}],"runtimeAssertions":[{"targetId":"carousel","value":"4","minWidth":1024}]}""");
            var goalRequest = Request(prompt, carouselHtml); goalRequest.SelectedId = "directory";
            goalRequest.RuntimeContext = """[{"targetId":"carousel","slidesPerView":6}]""";
            var goalResult = await service.Generate(goalRequest, default);
            Require(goalResult.Status == 422 && goalResult.Html == null && http.Calls == start + 4 && goalRequest.ExecutionGoal?.TargetId == "carousel",
                "A separately classified column goal cannot be satisfied by font CSS in any user language; no applicable result may escape.");
        }
        foreach (var selector in new[] { "#directory", "#cards > .template" })
        {
            start = http.Calls;
            var deltaPlan = JsonSerializer.Serialize(new {
                editMode = "content", html = "", styles = Array.Empty<object>(), summary = "已切換現有輪播版型。",
                classChanges = new[] { new { targetId = "carousel", remove = new[] { "six_swiper" }, add = new[] { "four_swiper" } } },
                sourceEvidence = new[] { new { key = "fixture-source", start = 1, end = 1 } }
            });
            respond = call => Reply(call == start + 1 ? JsonSerializer.Serialize(new {
                editMode = "style", html = "", styles = new[] { new { selector, properties = new Dictionary<string, string> { ["grid-template-columns"] = "repeat(4,1fr)" } } }, summary = "完成"
            }) : deltaPlan);
            var deltaRequest = Request("四個一排", carouselHtml); deltaRequest.SelectedId = "directory";
            deltaRequest.KnowledgeClasses.Add("four_swiper"); deltaRequest.KnowledgeSources = carouselRequest.KnowledgeSources;
            var deltaResult = await service.Generate(deltaRequest, default);
            Require(deltaResult.Status == 200 && http.Calls == start + 4 && deltaResult.Styles.Count == 0 &&
                CanvasAiService.TrySourceBackedClassEdit(deltaRequest, deltaResult.Html!) && deltaRequest.PlanDiagnostics[0].Reason.Contains("#carousel"),
                "Both outer-frame and runtime-card retries must identify the same initializer and accept reviewed source-backed token deltas without rebuilding directory markup.");
            var invalidDelta = deltaPlan.Replace("four_swiper", "unavailable_variant");
            var rejectedDelta = false;
            try { CanvasAiService.MaterializeClassChanges(deltaRequest, invalidDelta); }
            catch (FormatException) { rejectedDelta = true; }
            Require(rejectedDelta && deltaRequest.Html == carouselHtml, "Unknown class choices must be rejected before materialization without mutating the original request.");
        }
        start = http.Calls;
        respond = call => Reply(call == start + 1 ? """{"editMode":"style","html":"","styles":[{"selector":"#cards > .template","properties":{"grid-template-columns":"repeat(4,1fr)"}}],"summary":"完成"}""" : classPlan);
        var cardRuleRequest = Request("四個一排", carouselHtml); cardRuleRequest.SelectedId = "directory";
        cardRuleRequest.KnowledgeClasses.Add("four_swiper"); cardRuleRequest.KnowledgeSources = carouselRequest.KnowledgeSources;
        var cardRuleResult = await service.Generate(cardRuleRequest, default);
        Require(cardRuleResult.Status == 200 && http.Calls == start + 4 && cardRuleResult.Styles.Count == 0 &&
            cardRuleResult.Html == carouselHtml.Replace("six_swiper", "four_swiper") && cardRuleRequest.PlanDiagnostics.Any(d => d.Reason.Contains("JS-driven carousel")),
            "Scoped runtime-card Grid selectors must not bypass carousel protection; replan against the real source-backed initialization owner.");
        start = http.Calls;
        var largeCarousel = carouselHtml.Replace("商品", "<!--" + new string('x', 12000) + "-->商品");
        respond = _ => Reply(JsonSerializer.Serialize(new {
            editMode = "content", html = largeCarousel.Replace("six_swiper", "four_swiper"), summary = "已切換既有版型。", styles = Array.Empty<object>(),
            sourceEvidence = new[] { new { key = "fixture-source", start = 1, end = 1 } }
        }));
        var largeRequest = Request("調整輪播", largeCarousel); largeRequest.SelectedId = "directory";
        largeRequest.KnowledgeClasses.Add("four_swiper"); largeRequest.KnowledgeSources = carouselRequest.KnowledgeSources;
        var largeResult = await service.Generate(largeRequest, default);
        Require(largeResult.Status == 200 && http.Calls == start + 3 &&
            contexts[^2].GetProperty("ReviewProposal").GetString()!.Contains("ClassChangesToReview") &&
            contexts.Last().GetProperty("Html").ValueKind == JsonValueKind.Null &&
            contexts.Last().GetProperty("ApprovedPlan").GetProperty("html").GetString() == "" &&
            contexts.Last().GetRawText().Length < 6000 &&
            !contexts.Last().TryGetProperty("ResourceKnowledge", out _) && largeResult.Html!.Contains(new string('x', 12000)),
            "Large class-only directory edits must review compact deltas, keep all approved HTML locally and send only message metadata to UI review.");
        start = http.Calls;
        respond = _ => Reply("""{"editMode":"style","html":"","styles":[{"selector":"#gallery","properties":{"--coker-swiper-columns":"4"}}],"summary":"完成"}""");
        Require((await service.Generate(Request("調整"), default)).Status == 422 && http.Calls == start + 3,
            "Swiper-specific configuration must never be applied to an ordinary gallery.");
        start = http.Calls;
        const string systemLink = "<a id='label' class='brand-section-title' href='' data-link-type='link' data-link-value=''>品牌入口</a>";
        var originalLabel = Request("變更為連結", "<span id='label'>熱門商品 <strong id='emphasis'>精選</strong></span>"); originalLabel.SelectedId = "label";
        const string retainedLabel = "<a id='label'>熱門商品 <strong id='emphasis'>精選</strong></a>";
        Require(CanvasAiService.PreservesLinkConversionText(originalLabel, retainedLabel) &&
            !CanvasAiService.PreservesLinkConversionText(originalLabel, "<a id='label'>連結目的地</a>") &&
            !CanvasAiService.PreservesLinkConversionText(originalLabel, retainedLabel.Replace("id='label'", "id='other'")) &&
            !CanvasAiService.PreservesLinkConversionText(originalLabel, retainedLabel.Replace("<a id='label'>", "<a id='label' data-text=''>")),
            "New links must retain the selected ID and original nested wording and cannot carry a blank or conflicting display-text attribute.");
        respond = call => Reply(JsonSerializer.Serialize(new {
            editMode = "content", html = call == start + 1 ? "<a id='other'>連結目的地</a>" : retainedLabel,
            summary = "請設定連結目的地。", styles = Array.Empty<object>()
        }));
        var retainedResult = await service.Generate(originalLabel, default);
        Require(retainedResult.Status == 200 && retainedResult.Html == retainedLabel && http.Calls == start + 4 &&
            originalLabel.PlanDiagnostics.Any(d => d.Category == "link-text"),
            "A destructive link placeholder must be rejected and corrected within the existing planning budget before content review and UI localization.");
        start = http.Calls;
        respond = _ => Reply(JsonSerializer.Serialize(new { editMode = "content", html = systemLink, summary = "請設定目的網址。", styles = Array.Empty<object>() }));
        var metadataRequest = Request("調整為連結", "<span id='label' class='brand-section-title'>品牌入口</span>"); metadataRequest.SelectedId = "label";
        Require((await service.Generate(metadataRequest, default)).Status == 200 && http.Calls == start + 3,
            "The editor's native pending-link metadata and empty destination must be supported without accepting arbitrary data attributes.");
        start = http.Calls;
        respond = _ => Reply(JsonSerializer.Serialize(new { editMode = "content", html = systemLink.Replace("data-link-value=''", "data-link-value='javascript:alert(1)'"), summary = "完成", styles = Array.Empty<object>() }));
        Require((await service.Generate(metadataRequest, default)).Status == 422 && http.Calls == start + 3,
            "Pending-link compatibility must not admit executable destinations hidden in system metadata.");
        Require(systems.All(s => s.Contains("user's language") && s.Contains("ValidationFeedback")),
            "The selected editor must consistently use the language-independent planning contract.");
    }
}
