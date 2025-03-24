using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.DevExpress
{
    [Authorize]
    [Route("DXXRDV")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomWebDocumentViewerController: WebDocumentViewerController
    {
        public CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService) : base(controllerService)
        {
        }
    }

    [Authorize]
    [Route("DXXQB")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomQueryBuilderController : QueryBuilderController
    {
        public CustomQueryBuilderController(IQueryBuilderMvcControllerService controllerService) : base(controllerService)
        {
        }

        //[HttpGet]
        //public override Task<IActionResult> Invoke()
        //{
        //    return base.Invoke();
        //}

        //[HttpGet]
        //public override ActionResult GetLocalization()
        //{
        //    return base.GetLocalization();
        //}
    }

    [Authorize]
    [Route("DXXRD")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomReportDesignerController : ReportDesignerController
    {
        public CustomReportDesignerController(IReportDesignerMvcControllerService controllerService) : base(controllerService)
        {
        }
    }
}
