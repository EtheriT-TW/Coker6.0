using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Extensions;
using EtheriT.Coker.Web.MVC.Services.Ai;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[ApiController, Authorize, Route("api/CanvasAi")]
public sealed class CanvasAiController(LoginUserData user, NavigationProvider navigation, CanvasAiService ai) : ControllerBase
{
    [HttpPost("Chat"), ValidateAntiForgeryToken, RequestSizeLimit(262144)]
    public async Task<IActionResult> Chat([FromBody] CanvasAiRequest input, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        if (input.WebsiteId <= 0 || input.WebsiteId != await user.GetWebsiteId())
            return StatusCode(409, new { message = "站台已切換，請重新開啟編輯頁面。" });
        var validationError = ValidatePayload(input);
        if (validationError != null) return BadRequest(new { message = validationError });

        // API requests skip page navigation middleware: rebuild current user's site permissions explicitly.
        await navigation.BuildAuthorizedSiteAsync(controllerName: input.Controller, actionName: input.Action);
        var permission = HttpContext.GetPermission();
        if (!permission.CanVisble || !permission.CanUpdate || !HttpContext.HasPermission(CokerContextKeys.AiAssistant))
            return StatusCode(403, new { message = "需要目前頁面的編輯權限與 AI 輔助權限。" });
        // Always resolve locale from the authorized website, never from browser input.
        input.WebsiteLocale = (await user.GetWebsiteLocal())?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(input.WebsiteLocale))
            return UnprocessableEntity(new { message = "目前網站尚未設定語系，請先設定網站語系。" });
        input.ReplyLocale = ResolveReplyLocale(input.BrowserLocale, Request.Headers.AcceptLanguage.ToString(), input.WebsiteLocale);
        input.ConversationLocale = string.IsNullOrEmpty(input.ConversationLocale) ? "" : ResolveReplyLocale(input.ConversationLocale, "", input.ReplyLocale);
        if (input.ConversationLocale.Length > 0) input.ReplyLocale = input.ConversationLocale;
        input.MemoryUserId = (await user.GetUserId()).ToString(System.Globalization.CultureInfo.InvariantCulture);
        input.PermissionScope = input.Controller + "/" + input.Action;
        // Streaming is opt-in and starts only after the same site, edit, AI and
        // antiforgery checks as ordinary Chat. No second endpoint or shared trace IDs.
        var streaming = Request.Headers.Accept.ToString().Split(',')
            .Any(value => value.Trim().Equals("application/x-ndjson", StringComparison.OrdinalIgnoreCase));
        if (streaming)
        {
            Response.ContentType = "application/x-ndjson; charset=utf-8";
            Response.Headers["X-Accel-Buffering"] = "no";
            HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();
            input.TraceObserver = (entry, token) => WriteStreamEvent(new { type = "progress", data = entry }, token);
        }
        CanvasAiResult result;
        try { result = await ai.Generate(input, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        finally { input.TraceObserver = null; }
        if (!Response.HasStarted && input.ModelCalls.HasValue)
            Response.Headers["X-Canvas-Model-Calls"] = input.ModelCalls.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var serviceUnavailable = result.Status == 429 || result.Status >= 500;
        var environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var diagnosticsEnabled = AllowsPlanDiagnostics(environment.EnvironmentName, HttpContext.Connection.RemoteIpAddress);
        var diagnostics = diagnosticsEnabled && result.Status != 200
            ? input.PlanDiagnostics : null;
        var body = new { html = result.Html, styles = result.Styles, htmlChanged = result.HtmlChanged, message = result.Message, replyLocale = input.ReplyLocale, settingsActions = result.SettingsActions, preserveContent = input.PreserveContent, needsClarification = result.NeedsClarification, serviceUnavailable, diagnostics, diagnosticsEnabled };
        if (!streaming) return StatusCode(result.Status, body);
        await WriteStreamEvent(new { type = "result", status = result.Status, modelCalls = input.ModelCalls, data = body }, cancellationToken);
        return new EmptyResult();
    }

    private async Task WriteStreamEvent(object entry, CancellationToken token)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(entry, StreamJsonOptions);
        await Response.WriteAsync(json + "\n", token);
        await Response.Body.FlushAsync(token);
    }

    private static readonly System.Text.Json.JsonSerializerOptions StreamJsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web);

    internal static bool AllowsPlanDiagnostics(string environment, System.Net.IPAddress? remoteAddress) =>
        string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase) ||
        (string.Equals(environment, "Local", StringComparison.OrdinalIgnoreCase) && remoteAddress != null && System.Net.IPAddress.IsLoopback(remoteAddress));

    internal static string ResolveReplyLocale(string? browserLocale, string? acceptLanguage, string websiteLocale)
    {
        static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 80 ||
                !System.Text.RegularExpressions.Regex.IsMatch(value, @"\A[a-zA-Z]{2,8}(?:-[a-zA-Z0-9]{1,8})*\z")) return null;
            try { return System.Globalization.CultureInfo.GetCultureInfo(value).Name; }
            catch (System.Globalization.CultureNotFoundException) { return null; }
        }
        var explicitLocale = Normalize(browserLocale);
        if (explicitLocale != null) return explicitLocale;
        var preferences = new List<(string Locale, decimal Quality)>();
        foreach (var entry in (acceptLanguage ?? "").Split(',').Take(20))
        {
            var parts = entry.Trim().Split(';');
            var locale = Normalize(parts[0].Trim());
            if (locale == null) continue;
            var quality = 1m;
            if (parts.Length > 1 && (!parts[1].Trim().StartsWith("q=", StringComparison.OrdinalIgnoreCase) ||
                !decimal.TryParse(parts[1].Trim()[2..], System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture, out quality))) continue;
            if (quality > 0 && quality <= 1) preferences.Add((locale, quality));
        }
        return preferences.OrderByDescending(p => p.Quality).Select(p => p.Locale).FirstOrDefault() ?? Normalize(websiteLocale) ?? "en-US";
    }

    internal static string? ValidatePayload(CanvasAiRequest input)
    {
        if (input.Controller == null || input.Action == null || input.Html == null || input.Css == null || input.History == null || input.StyleContext == null ||
            input.Controller.Length > 80 || input.Action.Length > 80)
            return "編輯頁面資訊不完整，請重新開啟編輯頁面。";
        if (input.Scope is not ("canvas" or "selected" or "new") || (input.Scope == "new" && input.PreserveContent))
            return "修改範圍無效，請重新選取區塊後重試。";
        if (string.IsNullOrWhiteSpace(input.Prompt)) return "請輸入需求。";
        if (input.Prompt.Length > 4000) return "需求太長，請縮短後重試。";
        if (input.SelectedId == null || input.SelectedId.Length > 200) return "選取目標資訊不正確，請重新選取區塊。";
        if (input.BrowserLocale == null || input.BrowserLocale.Length > 80) return "瀏覽器語系資訊不正確。";
        if (input.ConversationLocale == null || input.ConversationLocale.Length > 80) return "對話語系資訊不正確。";
        if (input.AvailableSettings == null || input.AvailableSettings.Length > 2 ||
            input.AvailableSettings.Distinct().Count() != input.AvailableSettings.Length ||
            input.AvailableSettings.Any(kind => kind is not ("link" or "directory"))) return "設定功能資訊不正確。";
        if (input.Resources == null || input.Resources.Length > 32 || input.Resources.Any(path => path == null || path.Length > 500))
            return "畫布資源資訊不正確。";
        if (input.Html.Length > 64000) return "選取區塊的內容太長，請選取較小的區塊後重試。";
        if (input.Css.Length > 32000) return "選取區塊的樣式太多，請選取較小的區塊後重試。";
        if (input.History.Length > 8000) return "對話內容太長，請清除本機對話後重試。";
        if (input.StyleContext.Length > 16000) return "區塊樣式參考資料太多，請更新編輯器後重試。";
        return null;
    }
}
