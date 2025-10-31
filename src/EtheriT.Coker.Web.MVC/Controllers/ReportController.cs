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
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            R001ViewModel r001ViewModel = new R001ViewModel();
            r001ViewModel.ReportModel = await _reportingAppService.GetR001ModelAsync(id);
            r001ViewModel.ReportObject = new R001撿貨單();
            r001ViewModel.ReportObject.RequestParameters = false;
            if (r001ViewModel.ReportModel != null)
            {
                r001ViewModel.ReportObject.Parameters["網站名稱"].Value = r001ViewModel.ReportModel.網站名稱;
                r001ViewModel.ReportObject.Parameters["列印時間"].Value = r001ViewModel.ReportModel.列印時間;
                r001ViewModel.ReportObject.Parameters["訂單編號"].Value = r001ViewModel.ReportModel.訂單編號;
                r001ViewModel.ReportObject.Parameters["收件人"].Value = r001ViewModel.ReportModel.收件人;
                r001ViewModel.ReportObject.Parameters["訂單日期"].Value = r001ViewModel.ReportModel.訂單日期;
                r001ViewModel.ReportObject.Parameters["客戶名稱"].Value = r001ViewModel.ReportModel.客戶名稱;
                r001ViewModel.ReportObject.Parameters["聯絡電話"].Value = r001ViewModel.ReportModel.收件人電話;
                r001ViewModel.ReportObject.Parameters["支付方式"].Value = r001ViewModel.ReportModel.支付方式;
                r001ViewModel.ReportObject.Parameters["收件地址"].Value = r001ViewModel.ReportModel.收件人地址;
                r001ViewModel.ReportObject.Parameters["送貨方式"].Value = r001ViewModel.ReportModel.送貨方式;
                r001ViewModel.ReportObject.Parameters["發票載具"].Value = r001ViewModel.ReportModel.發票載具;
                r001ViewModel.ReportObject.Parameters["用戶備註"].Value = r001ViewModel.ReportModel.用戶備註;
                r001ViewModel.ReportObject.Parameters["運費"].Value = r001ViewModel.ReportModel.運費;

                r001ViewModel.ReportObject.DataSource = r001ViewModel.ReportModel.訂單明細;
                // 設定匯出的檔名
                r001ViewModel.ReportObject.ExportOptions.PrintPreview.DefaultFileName = "R001檢貨單";
            }
            else
            {
                return RedirectToAction("Index", "OrderManagement");
            }
            return View(r001ViewModel);
        }
    }
}
