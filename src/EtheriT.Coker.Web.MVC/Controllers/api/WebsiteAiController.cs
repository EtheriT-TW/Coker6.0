using System.Text.Json;
using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Extensions;
using EtheriT.Coker.Web.MVC.Services.Ai;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[ApiController, Authorize, Route("api/WebsiteAi"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class WebsiteAiController(LoginUserData user, NavigationProvider navigation,
    WebsiteAiSettingsService settings, IAiClient client, IAntiforgery antiforgery, CanvasAiAvailability selfAvailability,
    AiModelCatalog modelCatalog) : ControllerBase
{
    private async Task<bool> Allowed(bool edit)
    {
        await navigation.BuildAuthorizedSiteAsync(controllerName: "SystemManagement", actionName: "AiSettings");
        var permission = HttpContext.GetPermission();
        return permission.CanVisble && (!edit || permission.CanUpdate);
    }

    [HttpGet("Settings")]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        if (!await Allowed(false)) return Forbid();
        try { return Ok((await settings.GetAsync(token)).Public(await user.GetWebsiteId())); }
        catch (AiSettingsException ex) { return UnprocessableEntity(new { message = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return StatusCode(503, new { message = "無法讀取 AI 設定，請稍後再試。" }); }
    }

    [HttpGet("Token")]
    public async Task<IActionResult> Token()
    {
        if (!await Allowed(true)) return Forbid();
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { websiteId = await user.GetWebsiteId(), token = tokens.RequestToken, header = tokens.HeaderName });
    }

    [HttpPost("Settings"), ValidateAntiForgeryToken, RequestSizeLimit(16384)]
    public async Task<IActionResult> Save([FromBody] AiSettingsInput input, CancellationToken token)
    {
        if (!await Allowed(true)) return Forbid();
        try { return Ok(await settings.SaveAsync(input, token)); }
        catch (AiSettingsException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return StatusCode(503, new { message = "AI 設定儲存失敗，請重新載入確認後再試。" }); }
    }

    [HttpPost("Models"), ValidateAntiForgeryToken, RequestSizeLimit(16384)]
    public async Task<IActionResult> Models([FromBody] AiSettingsInput input, CancellationToken token)
    {
        if (!await Allowed(true)) return Forbid();
        var site = await user.GetWebsiteId();
        if (site <= 0 || site != input.WebsiteId) return Conflict(new { message = "站台已切換，請重新載入設定。" });
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            var draft = WebsiteAiSettingsService.Prepare(await settings.GetAsync(timeout.Token), input, requireModel: false);
            var result = await modelCatalog.ListAsync(draft, timeout.Token);
            if (result.Status != 200) return StatusCode(result.Status, new { message = result.Message });
            return Ok(new { websiteId = site, provider = draft.Provider, models = result.Models });
        }
        catch (AiSettingsException ex) { return Conflict(new { message = ex.Message }); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { return StatusCode(504, new { message = "模型清單查詢逾時，請稍後再試。" }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return StatusCode(503, new { message = "模型清單查詢失敗，請稍後再試。" }); }
    }

    [HttpPost("Test"), ValidateAntiForgeryToken, RequestSizeLimit(16384)]
    public async Task<IActionResult> Test(CancellationToken token)
    {
        if (!await Allowed(true)) return Forbid();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            var draft = await settings.GetAsync(timeout.Token);
            if (draft.EffectiveProvider == "self")
            {
                var readiness = await selfAvailability.CheckAsync(timeout.Token);
                return StatusCode(readiness.Available ? 200 : 503, new { message = readiness.Available
                    ? "系統預設 AI 服務就緒；未執行模型生成，設定尚未儲存。"
                    : "系統預設 AI 服務尚未就緒，請管理員檢查服務設定。" });
            }
            draft.Enabled = true; // Test only: never persist or enable the website.
            var result = await client.ChatAsync(draft, new("Return only JSON: {\"ok\":true}.", "Connection test. Return the requested JSON.", "zh-tw", ""), timeout.Token);
            if (result.Status != 200) return StatusCode(result.Status, new { message = result.Message });
            var content = (result.Content ?? "").Trim();
            if (content.StartsWith("```"))
            {
                var start = content.IndexOf('\n');
                var end = content.LastIndexOf("```", StringComparison.Ordinal);
                if (start >= 0 && end > start) content = content[(start + 1)..end].Trim();
            }
            using var json = JsonDocument.Parse(content);
            if (!json.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
                return UnprocessableEntity(new { message = "AI 可連線，但測試回覆未符合要求。" });
            return Ok(new { message = "連線與 JSON 回覆正常；設定尚未儲存，此檢查不代表畫布編輯已驗證。" });
        }
        catch (AiSettingsException ex) { return Conflict(new { message = ex.Message }); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { return StatusCode(504, new { message = "AI 連線測試逾時。" }); }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        { return UnprocessableEntity(new { message = "AI 測試回覆不是預期的 JSON 格式。" }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return StatusCode(503, new { message = "AI 連線測試失敗，請檢查設定。" }); }
    }
}
