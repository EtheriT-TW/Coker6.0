using EtheriT.Coker.Web.MVC.Models.Report;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult R001(long id)
        {
            R001ViewModel r001ViewModel = new R001ViewModel();
            return View(r001ViewModel);
        }
    }
}
