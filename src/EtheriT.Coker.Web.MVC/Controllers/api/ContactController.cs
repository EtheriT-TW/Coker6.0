using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactAppService contactAppService;
        private readonly NavigationProvider navigation;
        private readonly LoginUserData loginUserData;
        private readonly IAntiforgery antiforgery;
        public ContactController(IContactAppService contactAppService, NavigationProvider navigation, LoginUserData loginUserData, IAntiforgery antiforgery)
        {
            this.contactAppService = contactAppService;
            this.navigation = navigation;
            this.loginUserData = loginUserData;
            this.antiforgery = antiforgery;
        }
        [HttpGet]
        public async Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions)
        {
            return await contactAppService.GetContactListAll(loadOptions);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> GetDataOne(long id)
        {
            return await contactAppService.GetDataOne(id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ReplyContact(ContactReplyDto dto)
        {
            return await contactAppService.ReplyContact(dto);
        }

        /// <summary>
        /// 取得可匯出的表單類別；需先確認使用者可進入聯絡我們管理頁。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContactExportFormTypes()
        {
            if (!await CanAccessContactUsAsync())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ResponseMessageDto
                {
                    Success = false,
                    Error = "權限不足，無法執行匯出。",
                    Message = "權限不足，無法執行匯出。",
                    ErrorCode = ErrorCodeEnum.Forbidden
                });
            }

            return Ok(await contactAppService.GetContactExportFormTypesAsync());
        }

        /// <summary>
        /// 取得匯出 POST 使用的最新防偽權杖；避免頁面停留或其他 AJAX 更新 cookie 後使用到舊 token。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContactExportAntiforgeryToken()
        {
            if (!await CanAccessContactUsAsync())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    error = "權限不足，無法執行匯出。",
                    message = "權限不足，無法執行匯出。",
                    errorCode = "E005"
                });
            }

            // GetAndStoreTokens 會同步寫入防偽 cookie，回傳的 RequestToken 需立即放進下一次匯出 POST header。
            var tokenSet = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new
            {
                success = true,
                token = tokenSet.RequestToken,
                headerName = tokenSet.HeaderName
            });
        }

        /// <summary>
        /// 匯出聯絡表單 Excel；使用防偽權杖並在無權限時留下拒絕紀錄。
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportContacts([FromBody] ContactExportRequestDto dto)
        {
            if (!await CanAccessContactUsAsync())
            {
                await WriteExportDeniedLogAsync(dto);
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    error = "權限不足，無法執行匯出。",
                    message = "權限不足，無法執行匯出。",
                    errorCode = "E005"
                });
            }

            var result = await contactAppService.ExportContactsAsync(dto);
            if (result.Success && result.FileContents != null)
            {
                // 成功時直接回傳檔案串流，前端用 Content-Disposition 取得下載檔名。
                return File(result.FileContents, result.ContentType, result.FileName);
            }

            // 失敗時回傳 JSON，讓前端可以顯示規格定義的錯誤訊息與代碼。
            return StatusCode(result.HttpStatusCode, new
            {
                success = false,
                error = result.Error,
                message = result.Message ?? result.Error,
                errorCode = result.ErrorCodeKey,
                // 回傳本次後端採用的匯出上限，讓前端錯誤後仍能同步最新提示。
                maxRows = result.MaxRows
            });
        }

        /// <summary>
        /// 沿用後台選單權限判斷，確保匯出權限與聯絡我們頁面可視權限一致。
        /// </summary>
        private async Task<bool> CanAccessContactUsAsync()
        {
            var site = await navigation.getMenus();
            await navigation.SetPower(site);
            await navigation.SetWebsite(site);
            await navigation.setUserJob(site);
            var menu = navigation.FindJob(site.Jobs, "ContentManagement", "ContactUs");
            return menu?.CanVisble == true;
        }

        /// <summary>
        /// 使用者無權限匯出時仍寫入稽核紀錄，避免敏感資料存取嘗試沒有軌跡。
        /// </summary>
        private async Task WriteExportDeniedLogAsync(ContactExportRequestDto dto)
        {
            try
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(new
                {
                    HttpStatusCode = StatusCodes.Status403Forbidden,
                    ErrorCode = "E005",
                    Error = "權限不足，無法執行匯出。"
                }));
            }
            catch
            {
            }
        }
    }
}
