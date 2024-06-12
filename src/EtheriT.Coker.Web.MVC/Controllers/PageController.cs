using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.Webs;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class PageController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly WebsiteFrameDto WebsiteFrame;
        public PageController(LoginUserData loginUserData)
        {
            this.loginUserData = loginUserData;
        }
        public async Task<IActionResult> Index()
        {
            WebsiteFrameDto dto = new WebsiteFrameDto { Level = await loginUserData.GetWebsiteUseFrameLevel() };
            return View(dto);
        }
        public async Task<IActionResult> ComponerManager()
        {
            return View("ComponerManager");
        }
    }
}
