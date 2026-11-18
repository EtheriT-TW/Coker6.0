using System.Net;
using System.Text;
using System.Text.Json;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.MVC.Services.Ai;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiSettingsIsolation(BackofficeSandbox box)
    {
        const long site = 999000001;
        const string fakeKey = "diagnostics-fake-not-a-real-key";
        var service = box.Services.GetRequiredService<WebsiteAiSettingsService>();
        var empty = await service.GetAsync(default);
        Require(!empty.Enabled && empty.EffectiveProvider == "self" && empty.ApiKey == "", "AI must default to disabled without writes.");
        Require(!await box.Db.StoreSetDetail.AnyAsync(d => AiStoreSettingNames.All.Contains(d.StoreSet.key)), "Reading AI defaults must not save settings.");
        var definitions = await box.Db.StoreSet.AsNoTracking().Include(s => s.storeSetItems).Where(s => AiStoreSettingNames.All.Contains(s.key)).ToListAsync();
        Require(definitions.Count == 4 && definitions.Select(s => s.FK_StoreSetGroupId).Distinct().Count() == 1,
            "AI must use four seeded parameters under one StoreSetGroup.");
        Require(empty.Providers.Select(o => o.Key).SequenceEqual(new[] { "self", "openai", "gemini", "claude" }), "Provider choices must come from seeded StoreSetItems.");
        Require(empty.Providers.Single(o => o.Key == "self").Name == "系統預設 AI", "The default provider label must not expose hosting details.");
        var enabledDefinition = definitions.Single(s => s.key == AiStoreSettingNames.Enabled);
        Require(enabledDefinition.type == SeoSetDataTypeEnum.checkBox && enabledDefinition.DefaultValue == "false" &&
            enabledDefinition.storeSetItems.Count == 0, "AI enabled must be one boolean checkbox with no option rows and default false.");
        var groupCount = await box.Db.StoreSetGroup.CountAsync();
        var definitionCount = await box.Db.StoreSet.CountAsync();
        var first = await service.SaveAsync(new() { WebsiteId = site, Enabled = true, Provider = "openai", Model = "fixture-model", KeyAction = "replace", ApiKey = fakeKey }, default);
        box.Db.ChangeTracker.Clear();
        var loaded = await service.GetAsync(default);
        Require(loaded.Enabled && loaded.ApiKey == fakeKey && loaded.Provider == "openai" && first.HasApiKey, "Encrypted AI settings must reload.");
        var rows = await box.Db.StoreSetDetail.AsNoTracking().Where(d => d.FK_WebsiteId == site && AiStoreSettingNames.All.Contains(d.StoreSet.key))
            .Select(d => new { d.StoreSet.key, d.value }).ToListAsync();
        Require(rows.Count == 4 && rows.Single(r => r.key == AiStoreSettingNames.Enabled).value == "true" &&
            rows.Single(r => r.key == AiStoreSettingNames.Provider).value == "openai" && rows.Single(r => r.key == AiStoreSettingNames.Model).value == "fixture-model",
            "Each AI parameter must have its own StoreSetDetail value, not a JSON blob.");
        Require(await box.Db.StoreSetGroup.CountAsync() == groupCount && await box.Db.StoreSet.CountAsync() == definitionCount,
            "Saving website values must never create groups or parameter definitions.");
        var stored = await box.Db.StoreSetDetail.AsNoTracking().SingleAsync(d => d.FK_WebsiteId == site && d.StoreSet.key == AiStoreSettingNames.ApiKey);
        Require(stored.value != null && !stored.value.Contains(fakeKey), "StoreSet must not store plaintext credentials.");
        var visible = JsonSerializer.Serialize(first);
        Require(!visible.Contains(fakeKey) && !visible.Contains(stored.value!), "Public AI settings must exclude plaintext and ciphertext.");
        var generic = box.Services.GetRequiredService<IStoreSetAppService>();
        var genericRead = await generic.getValues(new StoreSetGetValueInput { StoreSetGroupId = definitions[0].FK_StoreSetGroupId });
        var genericJson = JsonSerializer.Serialize(genericRead);
        Require(!genericJson.Contains(fakeKey) && !genericJson.Contains(stored.value!), "Generic settings reads must not expose AI secrets.");
        foreach (var key in AiStoreSettingNames.All)
        {
            var bypass = await generic.CreateOrUpdate(new() { new() { key = key.ToLowerInvariant() + " ", value = new() { "tamper" } } });
            Require(!bypass.Success, "Generic settings writes must reject AI parameters, including case and trailing spaces.");
        }

        var kept = await service.SaveAsync(new() { WebsiteId = site, Provider = "openai", Model = "fixture-model", Revision = first.Revision }, default);
        Require((await service.GetAsync(default)).ApiKey == fakeKey && !kept.Enabled, "Disabling AI must preserve its credential.");
        Require((await box.Db.StoreSetDetail.AsNoTracking().SingleAsync(d => d.FK_WebsiteId == site && d.FK_StoreSetId == enabledDefinition.Id)).value == "false",
            "Unchecking AI enabled must persist false, not fall back to a checked default.");
        Require(!(await box.Services.GetRequiredService<WebsiteAiAvailability>().CheckAsync(default)).Available, "Disabled website readiness must be false.");
        Require((await box.Services.GetRequiredService<WebsiteAiAvailability>().CheckAsync(new WebsiteAiSettings
            { Enabled = true, Provider = "openai", Model = "fixture-model", ApiKey = fakeKey }, default)).Available,
            "External configuration must not depend on the disabled self-hosted gateway.");
        var rejected = false;
        try { await service.SaveAsync(new() { WebsiteId = site, Provider = "claude", Model = "fixture-model", Revision = kept.Revision }, default); }
        catch (AiSettingsException) { rejected = true; }
        Require(rejected, "A provider change must not reuse the previous provider key.");
        rejected = false;
        try { await service.SaveAsync(new() { WebsiteId = site, Provider = "openai", Model = "fixture-model", Revision = first.Revision }, default); }
        catch (AiSettingsException) { rejected = true; }
        Require(rejected, "A stale revision must not overwrite newer settings.");
        rejected = false;
        try { await service.SaveAsync(new() { WebsiteId = site + 1, Revision = kept.Revision }, default); }
        catch (AiSettingsException) { rejected = true; }
        Require(rejected, "A cross-site save must be rejected.");

        // Copying ciphertext across sites must fail authentication, not silently enable AI.
        box.Db.Websites.Add(new Website { Id = site + 1, Title = "AI隔離", OrgName = "diagnostics-ai", Locale = "zh-tw", Type = "web", DefaultUrl = "https://ai.invalid" });
        foreach (var row in await box.Db.StoreSetDetail.AsNoTracking().Where(d => d.FK_WebsiteId == site && AiStoreSettingNames.All.Contains(d.StoreSet.key)).ToListAsync())
            box.Db.StoreSetDetail.Add(new StoreSetDetail { FK_WebsiteId = site + 1, FK_StoreSetId = row.FK_StoreSetId, value = row.value });
        await box.Db.SaveChangesAsync();
        var context = box.Services.GetRequiredService<BackgroundOperationContext>();
        context.Set(site + 1, site);
        rejected = false;
        try { await service.GetAsync(default); } catch (AiSettingsException) { rejected = true; }
        finally { context.Set(site, site); }
        Require(rejected, "Copied site ciphertext must not decrypt in a different website.");
        var cleared = await service.SaveAsync(new() { WebsiteId = site, Provider = "self", KeyAction = "clear", Revision = kept.Revision }, default);
        var final = await service.GetAsync(default);
        Require(!cleared.HasApiKey && final.ApiKey == "" && final.EffectiveProvider == "self", "Explicit clear must remove the key and select self-hosting.");
        var missing = await box.Db.StoreSet.SingleAsync(s => s.key == AiStoreSettingNames.Model);
        missing.IsDeleted = true;
        await box.Db.SaveChangesAsync();
        rejected = false;
        try { await service.GetAsync(default); } catch (AiSettingsException) { rejected = true; }
        Require(rejected, "Missing seeded parameters must block AI rather than create definitions at runtime.");
    }

    private static async Task AiProviderContracts(BackofficeSandbox box)
    {
        var clients = new AiFixtureClients();
        var client = new AiClient(clients, box.Services.GetRequiredService<CanvasAiAvailability>());
        var input = new AiChatInput("Return JSON", "fixture input", "zh-tw", "<p>fixture</p>");
        var disabled = await client.ChatAsync(new(), input, default);
        Require(disabled.Status == 403 && clients.Calls == 0, "Disabled AI must never call HTTP.");
        foreach (var provider in new[] { "openai", "gemini", "claude" })
        {
            var settings = new WebsiteAiSettings { Enabled = true, Provider = provider, Model = "fixture-model", ApiKey = "fixture-key" };
            using (var request = client.CreateRequest(settings, input))
            {
                var expectedHost = provider switch { "openai" => "api.openai.com", "gemini" => "generativelanguage.googleapis.com", _ => "api.anthropic.com" };
                Require(request.RequestUri!.Host == expectedHost && request.RequestUri.Scheme == "https", "Provider destinations must be fixed HTTPS URLs.");
                Require(!request.RequestUri.Query.Contains("fixture-key"), "Credentials must never appear in a URL.");
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                if (provider == "openai") Require(request.Headers.Authorization?.Parameter == "fixture-key" && !body.RootElement.GetProperty("store").GetBoolean(), "OpenAI authentication/storage mapping incorrect.");
                if (provider == "gemini") Require(request.Headers.Contains("x-goog-api-key") && body.RootElement.GetProperty("generationConfig").GetProperty("responseMimeType").GetString() == "application/json", "Gemini request mapping incorrect.");
                if (provider == "claude") Require(request.Headers.Contains("x-api-key") && request.Headers.Contains("anthropic-version"), "Claude authentication/version mapping incorrect.");
            }
            clients.Body = provider switch
            {
                "openai" => "{\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"{\\\"ok\\\":true}\"}]}]}",
                "gemini" => "{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"thought\":true,\"text\":\"hidden\"},{\"text\":\"{\\\"ok\\\":true}\"}]}}]}",
                _ => "{\"stop_reason\":\"end_turn\",\"content\":[{\"type\":\"text\",\"text\":\"{\\\"ok\\\":true}\"}]}"
            };
            var result = await client.ChatAsync(settings, input, default);
            Require(result.Status == 200 && result.Content == "{\"ok\":true}" && result.ModelCalls == 1, "Provider response normalization failed.");
            clients.Body = provider switch
            {
                "openai" => "{\"status\":\"incomplete\"}",
                "gemini" => "{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\"}]}",
                _ => "{\"stop_reason\":\"max_tokens\"}"
            };
            Require((await client.ChatAsync(settings, input, default)).Status == 422, "Truncated generations must not be applied.");
            clients.Status = HttpStatusCode.Unauthorized;
            clients.Body = "fixture-key must not be returned";
            var before = clients.Calls;
            result = await client.ChatAsync(settings, input, default);
            Require(result.Status == 502 && !result.Message.Contains("fixture-key") && clients.Calls == before + 1, "Authentication failure must not leak secrets, retry or fall back.");
            clients.Status = HttpStatusCode.OK;
            clients.Body = "not JSON";
            Require((await client.ChatAsync(settings, input, default)).Status == 502, "Malformed provider responses must be rejected.");
            clients.Body = new string('x', 262145);
            Require((await client.ChatAsync(settings, input, default)).Status == 502, "Oversized responses must be rejected.");
        }
        Require(WebsiteAiSettingsService.Prepare(new(), new() { Provider = "gemini" }).EffectiveProvider == "self", "Missing external keys must select self-hosting.");
    }

    // No sockets: every HTTP response is constructed inside this isolated diagnostic.
    private sealed class AiFixtureClients : IHttpClientFactory
    {
        public string Body { get; set; } = "{}";
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public int Calls { get; private set; }
        public HttpClient CreateClient(string name) => new(new AiFixtureHandler(this));
        private sealed class AiFixtureHandler(AiFixtureClients owner) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                owner.Calls++;
                return Task.FromResult(new HttpResponseMessage(owner.Status) { Content = new StringContent(owner.Body, Encoding.UTF8, "application/json") });
            }
        }
    }
}
