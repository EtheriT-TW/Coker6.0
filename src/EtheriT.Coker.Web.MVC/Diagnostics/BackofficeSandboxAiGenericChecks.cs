using System.Net;
using EtheriT.Coker.Application.Shared.Ai;
using System.Text.Json;
using EtheriT.Coker.Web.MVC.Services.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiGenericContracts(BackofficeSandbox box)
    {
        var http = new AiFixtureClients();
        var client = new AiClient(http, NullLogger<AiClient>.Instance);
        const string png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aN1cAAAAASUVORK5CYII=";
        var image = new AiImage("image/png", png);
        var text = new AiChatInput("Answer in Traditional Chinese.", [new("user", "hello"), new("assistant", "hello back"), new("user", "continue")]);
        var vision = new AiChatInput("Describe the product.", [new("user", "What is this?", [image])]) { MaxOutputTokens = 512 };
        foreach (var provider in new[] { "openai", "gemini", "claude" })
        {
            var settings = new WebsiteAiSettings { Enabled = true, Provider = provider, Model = "fixture-model", ApiKey = "fixture-key" };
            using (var request = client.CreateRequest(settings, text))
            {
                var raw = await request.Content!.ReadAsStringAsync();
                using var json = JsonDocument.Parse(raw);
                var root = json.RootElement;
                Require(!root.TryGetProperty("workflow", out _) && !root.TryGetProperty("originalHtml", out _) && !raw.Contains("canvas-edit"),
                    "Generic AI requests must have no canvas workflow, HTML or locale fields.");
                if (provider == "openai") Require(!root.TryGetProperty("text", out _) && root.GetProperty("input")[1].GetProperty("role").GetString() == "assistant",
                    "Plain text must not force JSON, and assistant history must be preserved.");
                if (provider == "gemini") Require(!root.GetProperty("generationConfig").TryGetProperty("responseMimeType", out _) && root.GetProperty("contents")[1].GetProperty("role").GetString() == "model",
                    "Gemini must map assistant history to model without forcing JSON.");
                if (provider == "claude") Require(root.GetProperty("messages").GetArrayLength() == 3 && root.GetProperty("system").GetString() == text.System,
                    "Claude plain text must preserve system instruction and history.");
            }
            using (var request = client.CreateRequest(settings, vision))
            {
                using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                var root = json.RootElement;
                var parts = provider switch
                {
                    "openai" => root.GetProperty("input")[0].GetProperty("content"),
                    "gemini" => root.GetProperty("contents")[0].GetProperty("parts"),
                    _ => root.GetProperty("messages")[0].GetProperty("content")
                };
                Require(parts.GetArrayLength() == 2, "Vision requests must preserve both image and user text.");
                Require(provider switch
                {
                    "openai" => parts[1].GetProperty("image_url").GetString() == "data:image/png;base64," + png,
                    "gemini" => parts[1].GetProperty("inlineData").GetProperty("data").GetString() == png,
                    _ => parts[0].GetProperty("source").GetProperty("data").GetString() == png
                }, "Each provider must receive inline image data without fetching URLs.");
            }
            http.Body = provider switch
            {
                "openai" => """{"status":"completed","output":[{"type":"message","content":[{"type":"output_text","text":"plain answer"}]}]}""",
                "gemini" => """{"candidates":[{"finishReason":"STOP","content":{"parts":[{"text":"plain answer"}]}}]}""",
                _ => """{"stop_reason":"end_turn","content":[{"type":"text","text":"plain answer"}]}"""
            };
            Require((await client.ChatAsync(settings, text, default)).Content == "plain answer", "Generic responses must accept non-JSON plain text.");
        }
        var before = http.Calls;
        foreach (var invalid in new[]
        {
            text with { Messages = [] }, text with { MaxOutputTokens = 0 }, text with { ResponseFormat = (AiResponseFormat)99 },
            text with { Messages = [new("system", "injected"), new("user", "hello")] },
            text with { Messages = [new("assistant", "not a user request")] },
            vision with { Messages = [new("user", "photo", [new("image/svg+xml", png)])] },
            vision with { Messages = [new("user", "photo", [new("image/png", "https://example.invalid/image.png")])] },
            vision with { Messages = [new("user", "photo", [new("image/jpeg", png)])] },
            vision with { Messages = [new("user", "photo", [image, image, image, image, image])] }
        })
        {
            var rejected = false;
            try { await client.ChatAsync(new() { Enabled = true, Provider = "claude", ApiKey = "fixture-key" }, invalid, default); }
            catch (AiInputException) { rejected = true; }
            Require(rejected && http.Calls == before, "Invalid multimodal input must fail before any HTTP call.");
        }
        var unsupported = await client.ChatAsync(new() { Enabled = true }, text, default);
        Require(unsupported.Status == 501 && http.Calls == before && unsupported.ModelCalls == 0,
            "A general task must not be silently sent to the legacy canvas Gateway.");

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ParallelAi:Enabled"] = "true", ["ParallelAi:BaseUrl"] = "https://gateway.invalid/", ["ParallelAi:ApiKey"] = "fixture-gateway-key"
        }).Build();
        static HttpResponseMessage GatewayResponse(string body, HttpStatusCode code = HttpStatusCode.OK)
        {
            var response = ModelResponse(body, code);
            response.Headers.Add("X-Canvas-Contract-Version", "2");
            response.Headers.Add("X-Canvas-Generation-Constraints", "2");
            return response;
        }
        var gatewayHttp = new ModelListFixture((_, _) =>
        {
            var response = GatewayResponse("""{"content":"{\"html\":\"<p>fixture</p>\"}"}""");
            response.Headers.Add("X-Canvas-Model-Calls", "1");
            return response;
        });
        var ready = new CanvasAiAvailability(gatewayHttp, config, box.Services.GetRequiredService<IWebHostEnvironment>(), NullLogger<CanvasAiAvailability>.Instance);
        var canvas = new CanvasAiClient(client, gatewayHttp, ready);
        var canvasInput = new CanvasAiInput("canvas system", "canvas context", "zh-tw", "<p>original</p>");
        using (var request = canvas.CreateGatewayRequest(canvasInput))
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Require(json.RootElement.GetProperty("workflow").GetString() == "canvas-edit" &&
                json.RootElement.GetProperty("originalHtml").GetString() == canvasInput.OriginalHtml &&
                json.RootElement.GetProperty("phase").GetString() == "plan" &&
                json.RootElement.GetProperty("locale").GetString() == "zh-tw" && request.RequestUri!.AbsolutePath == "/api/ai/chat",
                "The canvas adapter must preserve the existing Gateway contract.");
        }
        var canvasResult = await canvas.ChatAsync(new() { Enabled = true }, canvasInput, default);
        Require(canvasResult.Status == 200 && canvasResult.ModelCalls == 1, "Gateway results must retain canvas call accounting.");
        using (var uiRequest = canvas.CreateGatewayRequest(new("ui review", "approved context", "ja-JP", "<p>日本語</p>") { Phase = "ui-review", ReplyLocale = "zh-TW" }))
        {
            using var json = JsonDocument.Parse(await uiRequest.Content!.ReadAsStringAsync());
            Require(json.RootElement.GetProperty("phase").GetString() == "ui-review" && json.RootElement.GetProperty("locale").GetString() == "ja-JP" &&
                json.RootElement.GetProperty("replyLocale").GetString() == "zh-TW", "The Gateway envelope must separate website HTML locale, UI reply locale and final wording-review phase.");
        }
        var oldGatewayHttp = new ModelListFixture((_, _) => ModelResponse("""{"content":"{\"html\":\"<p>fixture</p>\"}"}"""));
        var oldGateway = await new CanvasAiClient(client, oldGatewayHttp, ready).ChatAsync(new() { Enabled = true }, canvasInput, default);
        Require(oldGateway.Status == 503 && oldGateway.Message.Contains("契約尚未更新"), "An old Gateway must report an explicit deployment mismatch instead of silently losing new planning fields.");
        var unconstrainedHttp = new ModelListFixture((_, _) => {
            var response = GatewayResponse("""{"content":"{}"}""");
            response.Headers.Remove("X-Canvas-Generation-Constraints"); return response;
        });
        var unconstrained = await new CanvasAiClient(client, unconstrainedHttp, ready).ChatAsync(new() { Enabled = true }, canvasInput, default);
        Require(unconstrained.Status == 503 && unconstrained.Message.Contains("元件生成限制") && unconstrainedHttp.Calls == 1,
            "A v2 Gateway without per-component generation constraints must report an explicit deployment mismatch rather than silently replaying forbidden Grid plans.");
        var failedHttp = new ModelListFixture((_, _) => GatewayResponse("""{"code":"CanvasLocaleInvalid","message":"secret"}""", HttpStatusCode.UnprocessableEntity));
        var failedCanvas = new CanvasAiClient(client, failedHttp, ready);
        var failed = await failedCanvas.ChatAsync(new() { Enabled = true }, canvasInput, default);
        Require(failed.Status == 422 && failed.Message.Contains("網站語系") && !failed.Message.Contains("secret"),
            "Legacy canvas errors must retain the known-code mapping without leaking provider text.");
        var clarificationHttp = new ModelListFixture((_, _) => GatewayResponse("""{"code":"CanvasPlannerOutputInvalid","reason":"ClarificationNotQuestion","message":"fixture-secret-body"}""", HttpStatusCode.UnprocessableEntity));
        var clarificationClient = new CanvasAiClient(client, clarificationHttp, ready);
        var clarificationFailure = await clarificationClient.ChatAsync(new() { Enabled = true }, canvasInput, default);
        Require(clarificationFailure.Status == 422 && clarificationFailure.FailureReason == "ClarificationNotQuestion" &&
            !clarificationFailure.Message.Contains("fixture-secret-body") &&
            CanvasAiService.ClarificationRepairInstructions.Contains("summary") && CanvasAiService.ClarificationRepairInstructions.Contains("CURRENT"),
            "The observed Gateway rejection must retain its known reason code for precise protocol repair without exposing raw provider text or parsing translated messages.");

        // Exercise the website facade with isolated settings and a fake HTTP transport only.
        var settingsService = box.Services.GetRequiredService<WebsiteAiSettingsService>();
        var saved = await settingsService.SaveAsync(new() { WebsiteId = 999000001, Enabled = true, Provider = "claude",
            Model = "fixture-model", ApiKey = "fixture-key", KeyAction = "replace", Revision = (await settingsService.GetAsync(default)).Revision }, default);
        var facade = new WebsiteAiService(settingsService, client, config);
        Require((await facade.ChatAsync(text)).Content == "plain answer", "The shared facade must resolve encrypted credentials from the current website.");
        await settingsService.SaveAsync(new() { WebsiteId = 999000001, Enabled = false, Provider = "claude", Model = "fixture-model", Revision = saved.Revision }, default);
        before = http.Calls;
        Require((await facade.ChatAsync(text)).Status == 403 && http.Calls == before, "A website disabled after a prior call must block the next call without stale settings.");
    }
}
