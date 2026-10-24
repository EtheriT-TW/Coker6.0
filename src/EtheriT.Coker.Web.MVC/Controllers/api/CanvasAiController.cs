using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Extensions;
using EtheriT.Coker.Web.MVC.Services.Ai;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[ApiController, Authorize, Route("api/CanvasAi")]
public sealed class CanvasAiController(LoginUserData user, NavigationProvider navigation, CanvasAiService ai, CanvasAiAvailability availability) : ControllerBase
{
    [HttpPost("Chat"), ValidateAntiForgeryToken, RequestSizeLimit(262144)]
    public async Task<IActionResult> Chat([FromBody] CanvasAiRequest input, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        if (input.WebsiteId <= 0 || input.WebsiteId != await user.GetWebsiteId())
            return StatusCode(409, new { message = "站台已切換，請重新開啟編輯頁面。" });
        if (input.Controller == null || input.Action == null || input.Html == null || input.Css == null || input.History == null || input.StyleContext == null ||
            input.Controller.Length > 80 || input.Action.Length > 80 ||
            input.Scope is not ("canvas" or "selected" or "new") || string.IsNullOrWhiteSpace(input.Prompt) ||
            input.Prompt.Length > 4000 || input.Html.Length > 64000 || input.Css.Length > 32000 || input.History.Length > 8000 || input.StyleContext.Length > 16000 ||
            (input.Scope == "new" && input.PreserveContent))
            return BadRequest(new { message = "指令或畫布內容不符合限制。" });

        // API requests skip page navigation middleware: rebuild current user's site permissions explicitly.
        await navigation.BuildAuthorizedSiteAsync(controllerName: input.Controller, actionName: input.Action);
        var permission = HttpContext.GetPermission();
        if (!permission.CanVisble || !permission.CanUpdate || !HttpContext.HasPermission(CokerContextKeys.AiAssistant))
            return StatusCode(403, new { message = "需要目前頁面的編輯權限與 AI 輔助權限。" });
        var result = await ai.Generate(input, cancellationToken);
        var serviceUnavailable = result.Status == 429 || result.Status >= 500;
        if (serviceUnavailable) await availability.MarkUnavailableAsync();
        return StatusCode(result.Status, new { html = result.Html, styles = result.Styles, htmlChanged = result.HtmlChanged, message = result.Message, preserveContent = input.PreserveContent, serviceUnavailable });
    }
}
