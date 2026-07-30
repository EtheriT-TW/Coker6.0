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
        public async Task<IActionResult> GetTraffic(
            int days = 7,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string granularity = "day")
        {
            if (!TryValidateCustomRange(startDate, endDate, out var error))
                return BadRequest(error);

            if (!string.Equals(granularity, "day", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(granularity, "hour", StringComparison.OrdinalIgnoreCase))
                return BadRequest("不支援的流量統計粒度。");

            return Ok(await dashboardAppService.GetTraffic(days, startDate, endDate, granularity));
        }

        [HttpGet]
        public async Task<IActionResult> GetPopularPages(string period = "today", int take = 5)
        {
            var today = DateTime.Today;
            var startDate = period.ToLowerInvariant() switch
            {
                "today" => today,
                "week" => today.AddDays(-6),
                "month" => new DateTime(today.Year, today.Month, 1),
                "year" => new DateTime(today.Year, 1, 1),
                _ => (DateTime?)null
            };

            if (!startDate.HasValue)
                return BadRequest("不支援的熱門頁面統計區間。");

            return Ok(await dashboardAppService.GetPopularPages(take, startDate, today));
        }

        [HttpGet]
        public async Task<IActionResult> GetContacts(int take = 5)
        {
            return Ok(await dashboardAppService.GetContacts(take));
        }

        private static bool TryValidateCustomRange(
            DateTime? startDate,
            DateTime? endDate,
            out string error)
        {
            error = string.Empty;
            if (!startDate.HasValue && !endDate.HasValue)
                return true;

            if (!startDate.HasValue || !endDate.HasValue)
            {
                error = "自訂區間必須同時提供開始與結束日期。";
                return false;
            }

            var start = startDate.Value.Date;
            var end = endDate.Value.Date;
            if (start > end)
            {
                error = "開始日期不可晚於結束日期。";
                return false;
            }

            if ((end - start).TotalDays >= 366)
            {
                error = "自訂區間最多可查詢 366 天。";
                return false;
            }

            return true;
        }
    }
}
