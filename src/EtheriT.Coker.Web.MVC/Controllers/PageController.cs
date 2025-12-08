using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Member;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Shared.Member;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class PageController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly WebsiteFrameDto WebsiteFrame;
        private readonly IMemberAppService memberAppService;
        public PageController(LoginUserData loginUserData, IMemberAppService memberAppService)
        {
            this.loginUserData = loginUserData;
            this.memberAppService = memberAppService;
        }
        public async Task<IActionResult> Index()
        {
            bool hasRole = (await memberAppService.GetAllRole()).Any();
            WebsiteFrameDto dto = new WebsiteFrameDto { Level = await loginUserData.GetWebsiteUseFrameLevel(), hasRole = hasRole };
            return View(dto);
        }
        public async Task<IActionResult> ComponerManager()
        {
            return View("ComponerManager");
        }
    }
}
