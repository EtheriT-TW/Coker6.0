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
            if (r001ViewModel.ReportModel != null) {
                // 設定匯出的檔名
                r001ViewModel.ReportObject.ExportOptions.PrintPreview.DefaultFileName = "R001檢貨單";
                r001ViewModel.ReportObject.DataSource = r001ViewModel.ReportModel.訂單明細;
                r001ViewModel.ReportObject.Parameters["網站名稱"].Value = r001ViewModel.ReportModel.網站名稱;
                r001ViewModel.ReportObject.Parameters["列印時間"].Value = r001ViewModel.ReportModel.列印時間;
                r001ViewModel.ReportObject.Parameters["訂單編號"].Value = r001ViewModel.ReportModel.訂單編號;
                r001ViewModel.ReportObject.Parameters["收件人"].Value = r001ViewModel.ReportModel.收件人;
                r001ViewModel.ReportObject.Parameters["訂單日期"].Value = r001ViewModel.ReportModel.訂單日期;
                r001ViewModel.ReportObject.Parameters["客戶名稱"].Value = r001ViewModel.ReportModel.客戶名稱;

            }
            foreach (var param in r001ViewModel.ReportObject.Parameters)
            {
                Console.WriteLine($"{param.Name} - {param.Value} (可寫入: {param.MultiValue})");
            }
            return View(r001ViewModel);
        }
    }
}
