using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class MemberManagementController : Controller
    {
        private readonly IStoreSetAppService storeSetAppService;
        public MemberManagementController(IStoreSetAppService storeSetAppService)
        {
            this.storeSetAppService = storeSetAppService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MemberList()
        {
            var key = "MemberRegister";
            var result = await storeSetAppService.getValues(new StoreSetGetValueInput { 
                key = key
            });
            SavePermissionsItem item = new SavePermissionsItem { 
                Name = key,
                IsGranted = false
            };
            if (result.Success && result.detailItem != null && result.detailItem.value != null) {
                item.IsGranted = result.detailItem.value.Contains("3");
            }
            return View("MemberList", item);
        }

        public IActionResult UserType()
        {
            return View("UserType");
        }
        public IActionResult MemberType()
        {
            return View("MemberType");
        }

        public IActionResult SelfData() {
			return View("SelfData");
		}
        public IActionResult ManagerList(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            return View("ManagerList");
        }
    }
}
