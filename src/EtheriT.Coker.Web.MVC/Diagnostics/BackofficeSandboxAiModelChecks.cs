using System.Net;
using System.Text;
using EtheriT.Coker.Web.MVC.Services.Ai;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiModelCatalogContracts()
    {
        const string key = "fixture-model-list-key";
        var input = new AiSettingsInput { Provider = "claude", ApiKey = key, KeyAction = "replace", ClaudeWorkspaceId = "wrkspc_fixture" };
        var draft = WebsiteAiSettingsService.Prepare(new(), input, requireModel: false);
        Require(draft.Model == "" && !draft.Enabled, "Model discovery must work before selecting a model without enabling AI.");
        var rejected = false;
        try { WebsiteAiSettingsService.Prepare(new(), input); } catch (AiSettingsException) { rejected = true; }
        Require(rejected, "Model discovery must not relax model validation for saves.");
        var factory = new ModelListFixture((request, count) =>
        {
            Require(request.Method == HttpMethod.Get && request.Content == null && request.RequestUri!.Host == "api.anthropic.com",
                "Model discovery must only use metadata GET requests to fixed hosts.");
            Require(request.Headers.GetValues("x-api-key").Single() == key &&
                request.Headers.GetValues("anthropic-workspace-id").Single() == "wrkspc_fixture",
                "Claude model listing must use the selected credential and workspace.");
            Require(!request.RequestUri!.AbsoluteUri.Contains(key), "Model credentials must not be in URLs.");
            if (count == 2) Require(request.RequestUri.Query.Contains("after_id=claude-a"), "Claude pagination must use the last ID.");
            return ModelResponse(count == 1
                ? """{"data":[{"id":"claude-a","display_name":"Claude A"}],"has_more":true,"last_id":"claude-a"}"""
                : """{"data":[{"id":"claude-a","display_name":"Duplicate"},{"id":"claude-b","display_name":"Claude B"}],"has_more":false}""");
        });
        var catalog = new AiModelCatalog(factory);
        var result = await catalog.ListAsync(draft, default);
        Require(result.Status == 200 && result.Models.Count == 2 && result.Models[0].Name == "Claude A" && factory.Calls == 2,
            "Model pages must be combined and deduplicated without extra calls.");
        Require(!System.Text.Json.JsonSerializer.Serialize(result).Contains(key), "Model responses must not expose credentials.");

        foreach (var provider in new[] { "openai", "gemini" })
        {
            factory = new ModelListFixture((request, _) =>
            {
                var host = provider == "openai" ? "api.openai.com" : "generativelanguage.googleapis.com";
                Require(request.Method == HttpMethod.Get && request.RequestUri!.Host == host && !request.Headers.Contains("anthropic-workspace-id"),
                    "Provider model listing must not reuse Claude headers or destinations.");
                Require(provider == "openai" ? request.Headers.Authorization?.Parameter == key : request.Headers.GetValues("x-goog-api-key").Single() == key,
                    "Model listing must authenticate with the selected provider.");
                return ModelResponse(provider == "openai"
                    ? """{"data":[{"id":"fixture-text"},{"id":"invalid/id"}]}"""
                    : """{"models":[{"name":"models/fixture-text","displayName":"Fixture Text","supportedGenerationMethods":["generateContent"]},{"name":"models/embedding","supportedGenerationMethods":["embedContent"]}]}""");
            });
            result = await new AiModelCatalog(factory).ListAsync(new() { Provider = provider, ApiKey = key }, default);
            Require(result.Status == 200 && result.Models.Count == 1 && result.Models[0].Id == "fixture-text",
                "Model IDs must match the save contract and Gemini must only list generateContent models.");
        }
        factory = new ModelListFixture((request, count) =>
        {
            if (count == 2) Require(request.RequestUri!.Query.Contains("pageToken=page%2Btwo"), "Gemini page tokens must be escaped.");
            return ModelResponse(count == 1 ? """{"models":[],"nextPageToken":"page+two"}""" : """{"models":[]}""");
        });
        result = await new AiModelCatalog(factory).ListAsync(new() { Provider = "gemini", ApiKey = key }, default);
        Require(result.Status == 200 && result.Models.Count == 0 && factory.Calls == 2, "Empty Gemini pages must still follow pagination.");

        foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError })
        {
            factory = new ModelListFixture((_, _) => ModelResponse(key, status));
            result = await new AiModelCatalog(factory).ListAsync(draft, default);
            Require(result.Status != 200 && result.Models.Count == 0 && !result.Message.Contains(key) && factory.Calls == 1,
                "Model lookup failures must not leak bodies, retry or fall back.");
        }
        foreach (var body in new[] { "not JSON", new string('x', 262145), """{"data":[],"has_more":true,"last_id":"same"}""" })
        {
            factory = new ModelListFixture((_, _) => ModelResponse(body));
            result = await new AiModelCatalog(factory).ListAsync(draft, default);
            Require(result.Status == 502 && result.Models.Count == 0 && factory.Calls <= 2,
                "Malformed, oversized and repeated-cursor model responses must fail with no partial list.");
        }
        factory = new ModelListFixture((_, _) => throw new HttpRequestException("secret " + key));
        result = await new AiModelCatalog(factory).ListAsync(draft, default);
        Require(result.Status == 502 && !result.Message.Contains(key) && factory.Calls == 1, "Model network errors must be safe and not retried.");
        var before = factory.Calls;
        rejected = false;
        try { await new AiModelCatalog(factory).ListAsync(new(), default); } catch (AiSettingsException) { rejected = true; }
        Require(rejected && factory.Calls == before, "System default AI must never call external model endpoints.");

        factory = new ModelListFixture((_, count) => ModelResponse(System.Text.Json.JsonSerializer.Serialize(
            new { data = Array.Empty<object>(), has_more = true, last_id = "cursor-" + count })));
        result = await new AiModelCatalog(factory).ListAsync(draft, default);
        Require(result.Status == 502 && result.Models.Count == 0 && factory.Calls == 20,
            "Model pagination must stop at the bounded page limit without returning a partial list.");
    }

    private static HttpResponseMessage ModelResponse(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed class ModelListFixture(Func<HttpRequestMessage, int, HttpResponseMessage> respond) : IHttpClientFactory
    {
        public int Calls { get; private set; }
        public HttpClient CreateClient(string name) => new(new Handler(this));
        private sealed class Handler(ModelListFixture owner) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
                Task.FromResult(owner.Respond(request));
        }
        private HttpResponseMessage Respond(HttpRequestMessage request) => respond(request, ++Calls);
    }
}
