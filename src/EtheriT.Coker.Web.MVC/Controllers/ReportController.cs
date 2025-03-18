using EtheriT.Coker.Application.Reporting;
using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Web.MVC.Models.Report;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportingAppService _reportingAppService;
        public ReportController(IReportingAppService reportingAppService)
        {
            _reportingAppService = reportingAppService;
        }
        public async Task<IActionResult> R001(long id)
        {
            R001ViewModel r001ViewModel = new R001ViewModel();
            r001ViewModel.ReportModel = await _reportingAppService.GetR001ModelAsync(id);
            r001ViewModel.ReportObject = new R001檢貨單();

            // 設定匯出的檔名
            r001ViewModel.ReportObject.ExportOptions.PrintPreview.DefaultFileName = "R001檢貨單";
            // crossTab的資料來源需另外給
            var crossTab1 = r001ViewModel.ReportObject.FindControl("crossTab1", true) as DevExpress.XtraReports.UI.XRCrossTab;
            var crossTab2 = r001ViewModel.ReportObject.FindControl("crossTab2", true) as DevExpress.XtraReports.UI.XRCrossTab;
            crossTab1.DataSource = r001ViewModel.ReportModel;
            crossTab2.DataSource = r001ViewModel.ReportModel;
            return View(r001ViewModel);
        }
    }
}
