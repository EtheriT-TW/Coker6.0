using EtheriT.Coker.Web.MVC.Services.Ai;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiCanvasEdits(BackofficeSandbox box)
    {
        var completeHtml = "<div id='brands'>" + new string('牌', 15000) + "</div>";
        var latestHistory = new[] { new { role = "user", content = "修改為四個一排" + new string('前', 6000) }, new { role = "assistant", content = "請問哪一組？" } };
        var oversizedReferences = new
        {
            Html = completeHtml, Prompt = "第一組改成四個一排", Css = new string('c', 8000),
            History = latestHistory, AllowedSelectors = new[] { "#brands" },
            StyleContext = Enumerable.Range(0, 40).Select(i => new { selector = "#item-" + i, properties = new { width = new string('w', 280) } }).ToArray(),
            SelectedTarget = new { Text = new string('牌', 15000) }
        };
        var fitted = CanvasAiService.SerializeModelContext(oversizedReferences);
        using (var fittedDoc = JsonDocument.Parse(fitted))
        {
            var root = fittedDoc.RootElement;
            Require(fitted.Length <= 32000 && root.GetProperty("Html").GetString() == completeHtml &&
                root.GetProperty("Prompt").GetString() == oversizedReferences.Prompt &&
                root.GetProperty("History").GetArrayLength() == 2 &&
                root.GetProperty("History")[0].GetProperty("content").GetString() == latestHistory[0].content &&
                root.GetProperty("History")[1].GetProperty("content").GetString() == "請問哪一組？" &&
                root.GetProperty("AllowedSelectors")[0].GetString() == "#brands" &&
                root.GetProperty("CssTruncated").GetBoolean() &&
                root.GetProperty("StyleContext").GetArrayLength() is > 0 and < 40,
                "Combined reference overflow must fit valid model JSON without truncating Chinese HTML, the current request, selectors or pending clarification history.");
        }
        var compactChinese = CanvasAiService.SerializeModelContext(new { Html = completeHtml, Prompt = "四個一排" });
        Require(compactChinese.Length < 16000, "Chinese HTML must not inflate to six-character Unicode escapes in model-message text.");
        var essentialOverflowRejected = false;
        try { CanvasAiService.SerializeModelContext(new { Html = new string('x', 33000), Prompt = "四個一排" }); }
        catch (EtheriT.Coker.Application.Shared.Ai.AiInputException) { essentialOverflowRejected = true; }
        Require(essentialOverflowRejected, "Essential content overflow must be rejected, never silently truncated or permitted beyond the gateway limit.");
        var layoutRequest = new CanvasAiRequest
        {
            Controller = "PageManagement", Action = "Edit", Scope = "selected", Prompt = "幫我改為四個一排",
            Html = "<div id='brands'><div>品牌一</div><div>品牌二</div></div>", Css = "", History = "[]", StyleContext = "[]",
            AutoDetect = true, PreserveContent = true
        };
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest) == null,
            "A selected-container four-column request must pass input validation before calling the model.");
        layoutRequest.StyleContext = new string(' ', 16000);
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest) == null, "The style reference boundary must remain accepted.");
        layoutRequest.StyleContext += " ";
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest)!.Contains("樣式參考"), "Oversized computed styles must identify the actual failing field.");
        layoutRequest.StyleContext = "[]";
        layoutRequest.Html = new string('x', 64001);
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest)!.Contains("內容太長"), "Oversized HTML must identify the content limit.");
        layoutRequest.Html = "<div></div>";
        layoutRequest.Css = new string('x', 32001);
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest)!.Contains("樣式太多"), "Oversized CSS must identify the stylesheet limit.");
        layoutRequest.Css = "";
        layoutRequest.History = new string('x', 8001);
        Require(Controllers.api.CanvasAiController.ValidatePayload(layoutRequest)!.Contains("對話內容"), "Oversized history must identify the conversation limit.");
        var rules = new List<CanvasAiStyle>
        {
            new() { Selector = ".target", Properties = new() { ["color"] = "red" } },
            new() { Selector = "h2 > span.target", Properties = new() { ["color"] = "#336589" } }
        };
        const string fragment = "<h2>品牌<span class='target'>入口</span></h2>";
        Require(CanvasAiStyleNormalizer.TryNormalize(rules, fragment, out var html, out var normalized) && normalized.Count == 1 &&
            normalized[0].Selector.StartsWith("#ai-style-") && normalized[0].Properties["color"] == "#336589",
            "Class, tag and child selectors must resolve within the fragment and retain CSS specificity.");
        var doc = new HtmlDocument(); doc.LoadHtml(html);
        Require(doc.DocumentNode.InnerText == "品牌入口" && doc.DocumentNode.SelectSingleNode("//span").GetAttributeValue("class", "") == "target",
            "Resolving CSS selectors must preserve original words and classes.");
        Require(!CanvasAiStyleNormalizer.TryNormalize([new() { Selector = ".outside", Properties = new() { ["color"] = "red" } }], fragment, out _, out _),
            "A selector outside the authorized fragment must not be converted into a global rule.");
        Require(CanvasAiStyleNormalizer.TryNormalize([new() { Selector = "h2, span.target", Properties = new() { ["color"] = "red" } }], fragment, out _, out normalized) && normalized.Count == 2,
            "Grouped selectors must map each local target to its own scoped rule.");

        await AiCanvasPlannerChecks(box);
    }
}
