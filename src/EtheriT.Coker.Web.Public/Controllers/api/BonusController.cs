using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BonusController : Controller
    {
        private readonly IBonusManagementAppService bonusManagementAppService;
        public BonusController(IBonusManagementAppService bonusManagementAppService)
        {
            this.bonusManagementAppService = bonusManagementAppService;
        }
        [HttpGet]
        public Task<GetFrontUserBonusHistoryOutput> GetFrontUserBonusHistory(int page, int pageSize = 15)
        {
            return bonusManagementAppService.GetFrontUserBonusHistory(page, pageSize);
        }
    }
}
