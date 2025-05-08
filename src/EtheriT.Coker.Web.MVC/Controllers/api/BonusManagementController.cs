using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BonusManagementController : Controller, IBonusManagementAppService
    {
        private readonly IBonusManagementAppService _bonusManagementAppService;
        public BonusManagementController(IBonusManagementAppService bonusManagementAppService)
        {
            _bonusManagementAppService = bonusManagementAppService;
        }

        public async Task<GetBonusSettingForEditOutput> GetBonusSettingForEdit()
        {
            return await _bonusManagementAppService.GetBonusSettingForEdit();
        }
    }
}