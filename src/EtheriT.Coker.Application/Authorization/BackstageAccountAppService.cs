using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.User;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EtheriT.Coker.Application.Authorization
{
    public sealed class BackstageAccountAppService : IBackstageAccountAppService
    {
        private readonly AccountAppService core;
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IConfiguration configuration;

        public BackstageAccountAppService(
            AccountAppService core,
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor,
            ICookieManagerAppService cookieManager,
            IConfiguration configuration)
        {
            this.core = core;
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.httpContextAccessor = httpContextAccessor;
            this.cookieManager = cookieManager;
            this.configuration = configuration;
        }

        public async Task<LoginOutputDto> Login(LoginInputDto dto)
        {
            var output = new LoginOutputDto { Success = false };
            long? userId = null;
            long? websiteId = null;
            try
            {
                if (string.IsNullOrEmpty(dto.UserName))
                    throw new Exception("使用者名稱不可為空");
                if (string.IsNullOrEmpty(dto.Password))
                    throw new Exception("密碼不可為空");

                var user = await db.Users.FirstOrDefaultAsync(e =>
                    e.Account == dto.UserName ||
                    e.CellPhone == dto.UserName ||
                    e.Email == dto.UserName);
                if (user == null || !passwordHasher.VerifyHashedPassword(user.Password, dto.Password))
                    throw new Exception("帳號或密碼錯誤");

                userId = user.Id;
                long bindId = 0;
                if (httpContextAccessor.HttpContext != null)
                    long.TryParse(httpContextAccessor.HttpContext.Request.Cookies["lastWebSite"], out bindId);

                if (!await loginUserData.CheckedWebSiteId(user.Id, bindId))
                {
                    var defaultWeb = await db.MappingUserAndWebsites
                        .Where(e => !e.IsDeleted && e.UserId == user.Id)
                        .OrderByDescending(e => e.WebsiteId)
                        .FirstOrDefaultAsync();
                    if (defaultWeb == null)
                        throw new Exception("無可管理的網站");

                    bindId = defaultWeb.WebsiteId;
                }
                websiteId = bindId;

                var endDateTime = DateTime.Now.AddMinutes(30);
                var token = new Core.Models.Token
                {
                    ip = loginUserData.GetClientIP() ?? "",
                    UserID = user.Id,
                    StartTime = DateTime.Now,
                    EndTime = endDateTime,
                    websiteId = websiteId.Value
                };
                db.Tokens.Add(token);
                await db.SaveChangesAsync();

                output.Token = await tokenAppService.CreateToken(
                    user.Account,
                    token.id,
                    CookiePurposeEnum.BackstageAuthToken,
                    "Backstage");
                output.Secret = token.id;
                output.EndDateTime = endDateTime;
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }

            dto.Password = "******";
            await loginUserData.SetLogs(
                userId,
                websiteId,
                JsonConvert.SerializeObject(dto),
                JsonConvert.SerializeObject(output));
            return output;
        }

        public Task<UserDto> GetCurrentUser() => core.GetCurrentUser();

        public async Task<LoginOutputDto> Chech()
        {
            var response = new LoginOutputDto { Success = false };
            try
            {
                ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;
                var name = principal?.Identity?.Name;
                var secret = cookieManager.Get("BackstageRefreshToken");

                if (string.IsNullOrWhiteSpace(name) ||
                    !Guid.TryParse(secret, out var refreshTokenId))
                    throw new Exception("登入已過期");

                var token = await db.Tokens.FirstOrDefaultAsync(e => e.id == refreshTokenId);
                if (token?.EndTime == null || token.EndTime < DateTime.Now)
                    throw new Exception("登入已過期");

                var user = await db.Users.FirstOrDefaultAsync(e =>
                    e.Id == token.UserID && !e.IsDeleted);
                if (user == null)
                    throw new Exception("登入已過期");
                if (!string.Equals(user.Account, name, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("登入狀態異常");

                if (token.EndTime < DateTime.Now.AddMinutes(15))
                {
                    token.EndTime = DateTime.Now.AddMinutes(30);
                    await db.SaveChangesAsync();
                }

                response.Token = await tokenAppService.CreateToken(
                    user.Account,
                    token.id,
                    CookiePurposeEnum.BackstageAuthToken,
                    "Backstage");
                response.Secret = token.id;
                response.EndDateTime = token.EndTime.Value;
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Error = e.Message;
                try { await tokenAppService.DelToken(); } catch { }
                ClearBackstageCookies();
            }

            try
            {
                var expiredTokens = db.Tokens.Where(e => e.EndTime < DateTime.Now);
                db.Tokens.RemoveRange(expiredTokens);
                await db.SaveChangesAsync();
            }
            catch { }

            return response;
        }

        public async Task<ResponseMessageDto> Logout()
        {
            var secret = cookieManager.Get("BackstageRefreshToken");
            try
            {
                if (Guid.TryParse(secret, out var refreshTokenId))
                {
                    var token = await db.Tokens.FirstOrDefaultAsync(e => e.id == refreshTokenId);
                    if (token != null)
                    {
                        if (token.UserID != null)
                        {
                            db.Account_Logs.Add(new Account_Log
                            {
                                UUID = token.UUID,
                                WebsiteId = configuration.GetValue<long>("WebConfig:SiteId"),
                                Status = (int)AccountStatusEnum.登出,
                                CreatorUserId = token.UserID.Value,
                                LastLoginTime = DateTime.Now,
                                CreationTime = DateTime.Now,
                            });
                        }
                        db.Tokens.Remove(token);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch { }

            try { await tokenAppService.DelToken(); } catch { }
            ClearBackstageCookies();
            return new ResponseMessageDto { Success = true };
        }

        public Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto) => core.UpdatePassword(dto);
        public Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto) => core.GetEditUser(dto);
        public Task<ResponseMessageDto> AddUser(AddUser dto) => core.AddUser(dto);
        public Task<ResponseMessageDto> SendForget(long userId) => core.SendForget(userId);

        private void ClearBackstageCookies()
        {
            cookieManager.Delete("BackstageToken");
            cookieManager.Delete("BackstageRefreshToken");
            cookieManager.Delete(".Coker6.Back.Auth");
        }
    }
}
