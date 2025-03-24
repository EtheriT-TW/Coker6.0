using EtheriT.Coker.Application.Reporting;
using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Web.MVC.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            r001ViewModel.ReportObject.RequestParameters = false;
            if (r001ViewModel.ReportModel != null)
            {
                r001ViewModel.ReportObject.Parameters["網站名稱"].Value = r001ViewModel.ReportModel.網站名稱;
                r001ViewModel.ReportObject.Parameters["列印時間"].Value = r001ViewModel.ReportModel.列印時間;
                r001ViewModel.ReportObject.Parameters["訂單編號"].Value = r001ViewModel.ReportModel.訂單編號;
                r001ViewModel.ReportObject.Parameters["收件人"].Value = r001ViewModel.ReportModel.收件人;
                r001ViewModel.ReportObject.Parameters["訂單日期"].Value = r001ViewModel.ReportModel.訂單日期;
                r001ViewModel.ReportObject.Parameters["客戶名稱"].Value = r001ViewModel.ReportModel.客戶名稱;
                r001ViewModel.ReportObject.DataSource = r001ViewModel.ReportModel.訂單明細;
                // 設定匯出的檔名
                r001ViewModel.ReportObject.ExportOptions.PrintPreview.DefaultFileName = "R001檢貨單";
                Console.WriteLine(JsonConvert.SerializeObject(r001ViewModel.ReportObject.DataSource, Formatting.Indented));
            }
            else
            {
                return RedirectToAction("Index", "OrderManagement");
            }
            for (int i=0;i< r001ViewModel.ReportModel.訂單明細.Count; i++) { 
                var detail = r001ViewModel.ReportModel.訂單明細[i];
                Console.WriteLine($"{detail.商品名稱} - {detail.商品單價} - {detail.商品折扣} - ({detail.商品規格}) - {detail.商品數量} - {detail.商品小計}");
            }
            return View(r001ViewModel);
        }
    }
}
