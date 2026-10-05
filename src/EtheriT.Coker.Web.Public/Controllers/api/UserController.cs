using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IFrontAccountAppService accountAppService;
        private readonly IConfiguration configuration;
        private readonly CokerDbContext db;
        private readonly FrontAccountEventRecorder accountEvents;
        private static readonly SemaphoreSlim RegistrationLock = new(1, 1);
        public UserController(
            IFrontAccountAppService accountAppService, IConfiguration configuration,
            CokerDbContext db, FrontAccountEventRecorder accountEvents)
        {
            this.accountAppService = accountAppService;
            this.configuration = configuration;
            this.db = db;
            this.accountEvents = accountEvents;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> AddUser(FrontAddUserDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "register", 10))
                return await Reject("RegistrationRejected", "RateLimited", "操作過於頻繁，請稍後再試", dto.Email);
            var browser = AccountRequestGuard.Browser(HttpContext);
            if (!AccountRequestGuard.Consume($"verified:{browser}"))
                return await Reject("RegistrationRejected", "CaptchaMissingOrRateLimited", "請重新完成驗證碼驗證", dto.Email);
            var website = await CurrentWebsite();
            if (website == null) return await Reject("RegistrationRejected", "WebsiteUnavailable", "網站資料錯誤", dto.Email);
            dto.WebsiteId = website.Id;
            dto.WebsiteName = website.Title ?? string.Empty;
            dto.WebsiteLink = website.DefaultUrl ?? string.Empty;
            dto.SendWelcomeMail = true;
            dto.SendActivationMail = false;
            if (!await RegistrationLock.WaitAsync(0))
                return await Reject("RegistrationRejected", "RegistrationBusy", "目前註冊忙碌，請重新驗證後再試", dto.Email);
            try
            {
                var result = await accountAppService.AddFrontUser(dto);
                if (result.Message == "重新寄送通知信")
                    AccountRequestGuard.Grant($"resend:{browser}:{dto.Email?.Trim().ToUpperInvariant()}", TimeSpan.FromMinutes(2));
                return result;
            }
            finally { RegistrationLock.Release(); }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "resend", 10) ||
                !AccountRequestGuard.Consume($"resend:{AccountRequestGuard.Browser(HttpContext)}:{dto.Email?.Trim().ToUpperInvariant()}"))
                return await Reject("ActivationResendRejected", "CaptchaMissingOrRateLimited", "請重新完成註冊驗證，或稍後再試", dto.Email);
            var website = await CurrentWebsite();
            if (website == null) return await Reject("ActivationResendRejected", "WebsiteUnavailable", "網站資料錯誤", dto.Email);
            dto.WebsiteId = website.Id;
            dto.WebsiteName = website.Title ?? string.Empty;
            dto.WebsiteLink = website.DefaultUrl ?? string.Empty;
            dto.OpenId = null;
            var result = await accountAppService.ReSendOpening(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<LoginOutputDto> Login(FrontLoginInputDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "login", 30))
                return new LoginOutputDto { Success = false, Error = "登入操作過於頻繁，請稍後再試" };
            dto.WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var result = await accountAppService.FrontLogin(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto)
        {
            var result = await accountAppService.FrontUserEdit(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> PasswordForget(SendForgetDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "forget", 10) ||
                !AccountRequestGuard.Consume($"verified:{AccountRequestGuard.Browser(HttpContext)}"))
                return await Reject("PasswordResetRequestRejected", "CaptchaMissingOrRateLimited", "請重新完成驗證碼驗證，或稍後再試", dto.Email);
            var website = await CurrentWebsite();
            if (website == null) return await Reject("PasswordResetRequestRejected", "WebsiteUnavailable", "網站資料錯誤", dto.Email);
            dto.WebsiteId = website.Id;
            dto.WebsiteName = website.Title ?? string.Empty;
            dto.WebsiteLink = website.DefaultUrl ?? string.Empty;
            var result = await accountAppService.SendForget(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "password-change", 10) ||
                !AccountRequestGuard.Consume($"verified:{AccountRequestGuard.Browser(HttpContext)}"))
                return await Reject("PasswordChangeRejected", "CaptchaMissingOrRateLimited", "請重新完成驗證碼驗證，或稍後再試");
            var result = await accountAppService.PasswordChage(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> EmailChage(EmailChangeDto dto)
        {
            if (!AccountRequestGuard.Allow(HttpContext, "email-change", 10) ||
                !AccountRequestGuard.Consume($"verified:{AccountRequestGuard.Browser(HttpContext)}"))
                return await Reject("EmailChangeRejected", "CaptchaMissingOrRateLimited", "請重新完成驗證碼驗證，或稍後再試", dto.Email);
            var result = await accountAppService.EmailChage(dto);
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> GetUserData()
        {
            var result = await accountAppService.GetFrontUserData();
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> ForgetIdCheck(Guid ForgetId)
        {
            var result = await accountAppService.ForgetIdCheck(ForgetId);
            return result;
        }
        [HttpGet]
        public async Task<LoginOutputDto> Logout()
        {
            var result = await accountAppService.FrontLogout();
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> AccountOpening(Guid OpenId)
        {
            var result = await accountAppService.AccountOpening(OpenId);
            return result;
        }

        private Task<Website?> CurrentWebsite() => db.Websites.FirstOrDefaultAsync(e =>
            e.Id == configuration.GetValue<long>("WebConfig:SiteId"));

        private async Task<ResponseMessageDto> Reject(string operation, string reason, string message, string? email = null)
        {
            await accountEvents.RecordAsync(new Account_Log
            {
                WebsiteId = configuration.GetValue<long>("WebConfig:SiteId"), EventName = operation,
                Status = (int)EtheriT.Coker.Application.Shared.Dto.enumType.AccountStatusEnum.請求拒絕,
                Success = false, FailureReason = reason, NewEmail = email,
                VerificationMethod = "ApiGuard"
            });
            return new ResponseMessageDto { Error = message };
        }
    }
}
