using EtheriT.Coker.Application.Shared.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public sealed class DashboardController : ControllerBase
    {
        private readonly IDashboardAppService dashboardAppService;

        public DashboardController(IDashboardAppService dashboardAppService)
        {
            this.dashboardAppService = dashboardAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemOverview()
        {
            return Ok(await dashboardAppService.GetSystemOverview());
        }

        [HttpGet]
        public async Task<IActionResult> GetTraffic(int days = 7)
        {
            return Ok(await dashboardAppService.GetTraffic(days));
        }

        [HttpGet]
        public async Task<IActionResult> GetPopularPages(int take = 5)
        {
            return Ok(await dashboardAppService.GetPopularPages(take));
        }

        [HttpGet]
        public async Task<IActionResult> GetContacts(int take = 5)
        {
            return Ok(await dashboardAppService.GetContacts(take));
        }
    }
}
