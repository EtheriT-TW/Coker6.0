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
    //[Authorize]
    public class BonusManagementController : Controller
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

        public async Task<GetBonusSettingHelpTextForEditOutput> GetBonusSettingHelpTextForEdit()
        {
            return await _bonusManagementAppService.GetBonusSettingHelpTextForEdit();
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateOrUpdateSettingsDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 呼叫 Service 層方法來更新設定
                await _bonusManagementAppService.Save(model);
                return Ok(new { success = true, message = "設定已成功更新" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}