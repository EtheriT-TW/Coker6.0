using DevExpress.XtraReports;
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
            var report = r001ViewModel.ReportObject;
            report.RequestParameters = false;
            if (r001ViewModel.ReportModel != null)
            {
                report.Parameters["網站名稱"].Value = r001ViewModel.ReportModel.網站名稱;
                report.Parameters["列印時間"].Value = r001ViewModel.ReportModel.列印時間;
                report.Parameters["訂單編號"].Value = r001ViewModel.ReportModel.訂單編號;
                report.Parameters["收件人"].Value = r001ViewModel.ReportModel.收件人;
                report.Parameters["訂單日期"].Value = r001ViewModel.ReportModel.訂單日期;
                report.Parameters["客戶名稱"].Value = r001ViewModel.ReportModel.客戶名稱;
                report.Parameters["聯絡電話"].Value = r001ViewModel.ReportModel.收件人電話;
                report.Parameters["支付方式"].Value = r001ViewModel.ReportModel.支付方式;
                report.Parameters["收件地址"].Value = r001ViewModel.ReportModel.收件人地址;
                report.Parameters["送貨方式"].Value = r001ViewModel.ReportModel.送貨方式;
                report.Parameters["發票資訊"].Value = r001ViewModel.ReportModel.發票資訊;
                report.Parameters["用戶備註"].Value = r001ViewModel.ReportModel.用戶備註;
                report.Parameters["運費"].Value = r001ViewModel.ReportModel.運費;
                report.Parameters["總計"].Value = r001ViewModel.ReportModel.訂單總金額;
                report.Parameters["合計"].Value = r001ViewModel.ReportModel.合計;
                report.Parameters["訂單紅利折抵"].Value = r001ViewModel.ReportModel.訂單紅利折抵;
                report.Parameters["紅利折抵金額"].Value = r001ViewModel.ReportModel.紅利折抵;
                report.Parameters["訂單折抵"].Value = r001ViewModel.ReportModel.訂單折抵;
                report.Parameters["優惠券折抵金額"].Value = r001ViewModel.ReportModel.優惠券折抵;


                if (string.IsNullOrEmpty(r001ViewModel.ReportModel.發票資訊))
                {
                    var invoceDataRow = report.FindControl("tableRow7", true);
                    invoceDataRow.Visible = false;
                }
                if (r001ViewModel.ReportModel.紅利折抵 <= 0)
                {
                    var sumBonusRow = report.FindControl("tableRow12", true);
                    sumBonusRow.Visible = false;
                }
                if (r001ViewModel.ReportModel.優惠券折抵 <= 0)
                {
                    var sumCuponRow = report.FindControl("tableRow13", true);
                    sumCuponRow.Visible = false;
                }
                if (r001ViewModel.ReportModel.訂單折抵 <= 0)
                {
                    var sumDiscountRow = report.FindControl("tableRow15", true);
                    sumDiscountRow.Visible = false;
                }
                if (r001ViewModel.ReportModel.訂單紅利折抵 <= 0)
                {
                    var sumBonusDiscountRow = report.FindControl("tableRow3", true);
                    sumBonusDiscountRow.Visible = false;
                }
                report.DataSource = r001ViewModel.ReportModel.訂單明細;
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
