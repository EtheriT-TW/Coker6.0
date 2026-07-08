using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Common;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ContentManagementController : Controller
    {
        private readonly NavigationProvider navigation;
        public ContentManagementController(NavigationProvider navigation) {
            this.navigation = navigation;
        }
        public IActionResult Tag()
        {
            return View("Tag");
        }
        public async Task<IActionResult> Directory()
        {
            return View("Directory");
        }
        public IActionResult Article()
        {
            return View("Article");
        }
        public IActionResult Marquee()
        {
            return View("Marquee");
        }
        public IActionResult ContactUs()
        {
            return View("ContactUs", EnumHelper.EnumToKeyValueList<ContactStatusEnum>());
        }

        public IActionResult SettingCSS()
        {
            return View("SettingCSS");
        }
    }
}
