using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using DevExtreme.AspNet.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> SaveSetting([FromBody] CreateOrUpdateSettingsDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 呼叫 Service 層方法來更新設定
                await _bonusManagementAppService.SaveSetting(model);
                return Ok(new { success = true, message = "設定已成功更新" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTransaction([FromBody] CreateUserTransactionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 呼叫 Service 層方法來更新設定
                await _bonusManagementAppService.SaveTransaction(model);
                return Ok(new { success = true, message = "紅利異動成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        public async Task<JsonResult> GetFrontUsers(DataSourceLoadOptions loadOptions)
        {
            return await _bonusManagementAppService.GetFrontUsers(loadOptions);
        }
    }
}