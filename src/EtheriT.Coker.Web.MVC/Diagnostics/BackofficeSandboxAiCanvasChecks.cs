using EtheriT.Coker.Web.MVC.Services.Ai;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiCanvasEdits(BackofficeSandbox box)
    {
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

        var settings = box.Services.GetRequiredService<WebsiteAiSettingsService>();
        var current = await settings.GetAsync(default);
        await settings.SaveAsync(new() { WebsiteId = 999000001, Enabled = true, Provider = "claude", Model = "fixture-model", Revision = current.Revision }, default);
        var response = """{"html":"","styles":[{"selector":".target","properties":{"color":"#336589"}}],"summary":"done","editMode":"style"}""";
        var http = new ModelListFixture((_, _) => ModelResponse(JsonSerializer.Serialize(new
        {
            stop_reason = "end_turn", content = new[] { new { type = "text", text = response } }
        })));
        var ai = new AiClient(http, NullLogger<AiClient>.Instance);
        var availability = box.Services.GetRequiredService<CanvasAiAvailability>();
        var service = new CanvasAiService(new CanvasAiClient(ai, http, availability), box.Services.GetRequiredService<IConfiguration>(), settings,
            box.Services.GetRequiredService<WebsiteAiAvailability>());
        CanvasAiRequest Request(string prompt, string content) => new()
        {
            Scope = "selected", AutoDetect = true, Prompt = prompt, Html = content, StyleContext = "{}", WebsiteLocale = "zh-tw"
        };
        var result = await service.Generate(Request("調整這個區塊的顏色", fragment), default);
        Require(result.Status == 200 && result.HtmlChanged && result.Styles.Count == 1 && result.Styles[0].Selector.StartsWith("#ai-style-"),
            "The real canvas pipeline must apply a valid class selector instead of rejecting it for not being an ID.");
        response = """{"html":"","styles":[{"selector":".target","properties":{"color":"url(https://invalid)"}}],"summary":"done","editMode":"style"}""";
        Require((await service.Generate(Request("調整這個區塊的顏色", fragment), default)).Status == 422,
            "Selector normalization must not bypass CSS property/value validation.");
        var before = http.Calls;
        result = await service.Generate(Request("把入口兩個字換成連結\n顏色變成 #336589", "<h2 id='title'>品牌入口</h2>"), default);
        doc.LoadHtml(result.Html ?? "");
        var link = doc.DocumentNode.SelectSingleNode("//a");
        Require(result.Status == 200 && http.Calls == before && result.HtmlChanged && doc.DocumentNode.InnerText == "品牌入口" &&
            link != null && link.InnerText == "入口" && link.Attributes["href"] == null && result.Styles.Single().Properties["color"] == "#336589",
            "A phrase link with explicit color must preserve other words, allow a deferred URL and require no model call.");
        result = await service.Generate(Request("協助我把入口兩個字變大\n修改為連結", "<h2 id='title'>品牌入口</h2>"), default);
        doc.LoadHtml(result.Html ?? "");
        link = doc.DocumentNode.SelectSingleNode("//a");
        Require(result.Status == 200 && http.Calls == before && result.HtmlChanged && doc.DocumentNode.InnerText == "品牌入口" &&
            link != null && link.InnerText == "入口" && link.Attributes["href"] == null && result.Styles.Single().Properties["font-size"] == "calc(1em + 4px)",
            "The user's compound resize-and-link request must preserve the complete heading without a model rewrite.");
        result = await service.Generate(Request("協助我把入口兩個字變大", "<h2 id='title'>品牌入口</h2>"), default);
        doc.LoadHtml(result.Html ?? "");
        Require(result.Status == 200 && http.Calls == before && doc.DocumentNode.InnerText == "品牌入口" &&
            doc.DocumentNode.SelectSingleNode("//span").InnerText == "入口", "Phrase resizing must wrap only the target and never discard its siblings.");
        response = """{"html":"<h2 id='title'><a id='link'>入口</a></h2>","styles":[{"selector":"#link","properties":{"font-size":"28px"}}],"summary":"done","editMode":"content"}""";
        result = await service.Generate(Request("請讓入口看起來更大並加上連結", "<h2 id='title'>品牌入口</h2>"), default);
        Require(result.Status == 422 && result.Html == null,
            "A model content-mode response must not delete untargeted text for a presentation/link request.");
        response = """{"html":"<h2 id='title'>品牌<a id='link'>入口</a></h2>","styles":[{"selector":"#link","properties":{"font-size":"28px"}}],"summary":"done","editMode":"content"}""";
        result = await service.Generate(Request("請讓入口看起來更大並加上連結", "<h2 id='title'>品牌入口</h2>"), default);
        Require(result.Status == 200, "A complete model content response preserving all text must still be allowed to add links.");
    }
}
