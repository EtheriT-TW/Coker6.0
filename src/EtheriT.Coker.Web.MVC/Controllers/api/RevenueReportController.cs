using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public sealed class RevenueReportController : ControllerBase
    {
        private const int MaxDailyRangeDays = 366;
        private const int MaxMonthlyRangeMonths = 60;

        private readonly IRevenueReportAppService revenueReportAppService;

        public RevenueReportController(IRevenueReportAppService revenueReportAppService)
        {
            this.revenueReportAppService = revenueReportAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenue(
            RevenueReportGranularity granularity,
            DateTime startDate,
            DateTime endDate)
        {
            if (!Enum.IsDefined(granularity))
                return BadRequest("不支援的統計單位。");

            if (startDate.Date > endDate.Date)
                return BadRequest("開始日期不能晚於結束日期。");

            if (granularity == RevenueReportGranularity.Day
                && (endDate.Date - startDate.Date).TotalDays >= MaxDailyRangeDays)
                return BadRequest($"每日報表最多查詢 {MaxDailyRangeDays} 天。");

            var monthSpan = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month + 1;
            if (granularity == RevenueReportGranularity.Month && monthSpan > MaxMonthlyRangeMonths)
                return BadRequest($"每月報表最多查詢 {MaxMonthlyRangeMonths} 個月。");

            return Ok(await revenueReportAppService.GetRevenue(granularity, startDate, endDate));
        }
    }
}