using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class SearchController(ICustSearchAppService service, NavigationProvider navigation, IAntiforgery antiforgery) : Controller
{
    private async Task<bool> Allowed(string operation)
    {
        var site = await navigation.BuildAuthorizedSiteAsync(false, "SearchManagement", "CustSearch");
        var job = navigation.FindJob(site.Jobs, "SearchManagement", "CustSearch");
        if (job == null || !job.Enable || !job.CanVisble) return false;
        return operation switch
        {
            "create" => job.CanCreate,
            "update" => job.CanUpdate,
            "delete" => job.CanRemove,
            _ => true
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(DataSourceLoadOptions loadOptions) =>
        await Allowed("read") ? await service.GetAll(loadOptions) : Forbid();

    [HttpGet]
    public async Task<IActionResult> GetDataOne(long id)
    {
        if (!await Allowed("read")) return Forbid();
        var result = await service.GetDataOne(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetOptions() =>
        await Allowed("read") ? Ok(await service.GetOptions()) : Forbid();

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetAntiforgeryToken()
    {
        if (!await Allowed("read")) return Forbid();
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUp(CustSearchEditDto dto)
    {
        if (!await Allowed(dto.Id == 0 ? "create" : "update")) return Forbid();
        return Ok(await service.AddUp(dto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault([FromBody] CustSearchDeleteDto dto) =>
        await Allowed("update") ? Ok(await service.SetDefault(dto.Id)) : Forbid();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] CustSearchDeleteDto dto) =>
        await Allowed("delete") ? Ok(await service.Delete(dto.Id)) : Forbid();
}
