using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net;

namespace EtheriT.Coker.Application.Authorization
{
    public sealed class FrontAccountAppService : IFrontAccountAppService
    {
        private readonly AccountAppService core;
        private readonly CokerDbContext db;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IConfiguration configuration;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly IHostEnvironment env;

        public FrontAccountAppService(
            AccountAppService core,
            CokerDbContext db,
            ICookieManagerAppService cookieManager,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IShoppingCartAppService shoppingCartAppService,
            IHostEnvironment env)
        {
            this.core = core;
            this.db = db;
            this.cookieManager = cookieManager;
            this.configuration = configuration;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.stringHandler = stringHandler;
            this.shoppingCartAppService = shoppingCartAppService;
            this.env = env;
        }

        public async Task<LoginOutputDto> FrontLogin(FrontLoginInputDto dto)
        {
            var output = new LoginOutputDto { Success = false };
            try
            {
                if (string.IsNullOrEmpty(dto.Email)) throw new Exception(L.get("EmailRequired"));
                if (string.IsNullOrEmpty(dto.Password)) throw new Exception(L.get("PasswordRequired"));

                await tokenAppService.CreateToken();
                var tempUuid = await tokenAppService.GetUUID();
                var oldToken = await db.Tokens.FirstOrDefaultAsync(e => e.UUID == tempUuid);
                var frontUser = await (
                    from user in db.FrontUsers
                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                    where user.Email == dto.Email && map.FK_WebsiteId == dto.WebsiteId
                    select user).FirstOrDefaultAsync();

                if (frontUser == null)
                    throw new Exception(L.get("AccountOrPasswordIncorrect"));

                if (frontUser.Status == (int)UserStatusEnum.鎖定 &&
                    frontUser.LockTime?.AddMinutes(15) > DateTime.Now)
                {
                    throw new Exception(L.get("AccountLocked", frontUser.LockTime.Value.AddMinutes(15)));
                }

                if (!passwordHasher.VerifyHashedPassword(frontUser.Password, dto.Password))
                {
                    frontUser.ErrorTimes += 1;
                    var accountLog = new Account_Log
                    {
                        UUID = frontUser.UUID,
                        WebsiteId = dto.WebsiteId,
                        ErrorTimes = frontUser.ErrorTimes,
                        CreatorUserId = frontUser.Id,
                        CreationTime = DateTime.Now
                    };

                    if (frontUser.ErrorTimes >= 3)
                    {
                        frontUser.LockTime = DateTime.Now;
                        frontUser.Status = (int)UserStatusEnum.鎖定;
                        accountLog.LockTime = frontUser.LockTime;
                        accountLog.Status = (int)AccountStatusEnum.鎖定;
                        await loginUserData.SaveChanges(frontUser);
                        db.Account_Logs.Add(accountLog);
                        await db.SaveChangesAsync();
                        throw new Exception(L.get("PasswordErrorTooMany"));
                    }

                    accountLog.Status = (int)AccountStatusEnum.登入失敗;
                    db.Account_Logs.Add(accountLog);
                    await db.SaveChangesAsync();
                    throw new Exception(L.get("AccountOrPasswordIncorrect"));
                }

                if (frontUser.Status == (int)UserStatusEnum.未開通)
                {
                    output.Message = L.get("ResendActivationMail");
                    throw new Exception(L.get("MemberNotActivated"));
                }

                output = await NoPasswordLogin(frontUser, dto.WebsiteId, dto);
                if (output.Success)
                {
                    if (oldToken != null && frontUser.PrivacyAgreeTime == null)
                        frontUser.PrivacyAgreeTime = oldToken.PrivacyAgreeTime;
                    frontUser.ErrorTimes = 0;
                    frontUser.LockTime = null;
                    if (frontUser.Status == (int)UserStatusEnum.鎖定)
                        frontUser.Status = (int)UserStatusEnum.開通;
                    await loginUserData.SaveChanges(frontUser);
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }

        public async Task<LoginOutputDto> FrontLoginByToken(Guid token)
        {
            var output = new LoginOutputDto();
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            try
            {
                var tokenInfo = await db.Tokens
                    .Where(t => t.id == token && t.websiteId == websiteId)
                    .Select(t => new
                    {
                        t.UUID,
                        Map = db.MappingOldNewUUID.FirstOrDefault(m => m.TempUUID == t.UUID)
                    })
                    .FirstOrDefaultAsync();
                if (tokenInfo?.Map == null)
                    throw new Exception(((int)OAuthErrorTypeEnum.無效的登入方式).ToString());

                var userInfo = await db.FrontUsers
                    .Where(u => u.UUID == tokenInfo.Map.UserUUID)
                    .Select(u => new
                    {
                        User = u,
                        WebsiteMap = db.MappingFrontUserAndWebsite.FirstOrDefault(m =>
                            m.FK_UserId == u.Id && m.FK_WebsiteId == websiteId)
                    })
                    .FirstOrDefaultAsync();
                if (userInfo?.User == null)
                    throw new Exception(((int)OAuthErrorTypeEnum.使用者不存在).ToString());
                if (userInfo.WebsiteMap == null)
                    throw new Exception(((int)OAuthErrorTypeEnum.無效的登入方式).ToString());

                var tempUuid = await tokenAppService.GetUUID();
                var oldToken = await db.Tokens.FirstOrDefaultAsync(e => e.UUID == tempUuid);
                output = await NoPasswordLogin(userInfo.User, websiteId, new FrontLoginInputDto
                {
                    WebsiteId = websiteId,
                    Email = userInfo.User.Email,
                    Remember = true
                });

                if (oldToken != null && userInfo.User.PrivacyAgreeTime == null)
                    userInfo.User.PrivacyAgreeTime = oldToken.PrivacyAgreeTime;
                await loginUserData.SaveChanges(userInfo.User);
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            return output;
        }

        public async Task<LoginOutputDto> FrontThirdLogin(FrontThirdLoginInputDto dto)
        {
            var output = new LoginOutputDto { Success = false };
            var user = await db.FrontUsers
                .Join(db.MappingFrontUserAndWebsite, u => u.Id, m => m.FK_UserId, (u, m) => new { u, m })
                .FirstOrDefaultAsync(g => g.u.Email == dto.Email && g.m.FK_WebsiteId == dto.FK_WebsiteId);

            if (user == null)
            {
                var password = stringHandler.RandonCode(RandomStringType.數字加英文大小寫及符號, 16);
                await core.AddFrontUser(new FrontAddUserDto
                {
                    Email = dto.Email,
                    Name = dto.Name,
                    WebsiteId = dto.FK_WebsiteId,
                    Password = password,
                    PasswordConfirm = password,
                    SendWelcomeMail = dto.SendWelcomeMail,
                    SendActivationMail = dto.SendActivationMail
                });
                user = await db.FrontUsers
                    .Join(db.MappingFrontUserAndWebsite, u => u.Id, m => m.FK_UserId, (u, m) => new { u, m })
                    .FirstOrDefaultAsync(g => g.u.Email == dto.Email && g.m.FK_WebsiteId == dto.FK_WebsiteId);
            }

            if (user == null)
            {
                output.Error = ((int)OAuthErrorTypeEnum.使用者建立失敗).ToString();
                return output;
            }

            var id = Guid.NewGuid();
            user.u.Status = (int)UserStatusEnum.開通;
            var loginToken = new Core.Models.Token
            {
                id = id,
                UserID = user.u.Id,
                websiteId = dto.FK_WebsiteId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(30),
                ip = loginUserData.GetClientIP() ?? "",
                UUID = user.u.UUID
            };
            db.Tokens.Add(loginToken);

            var mappings = db.MappingOldNewUUID
                .Where(e => e.UserUUID == user.u.UUID && e.TempUUID == loginToken.UUID);
            if (await mappings.AnyAsync())
            {
                var mappingList = await mappings.OrderByDescending(e => e.CreationTime).ToListAsync();
                if (mappingList.Count > 1)
                    db.MappingOldNewUUID.RemoveRange(mappingList.Skip(1));
            }
            else
            {
                db.MappingOldNewUUID.Add(new MappingOldNewUUID
                {
                    TempUUID = loginToken.UUID,
                    UserUUID = user.u.UUID
                });
            }
            await db.SaveChangesAsync();

            output.Secret = id;
            output.Success = true;
            return output;
        }
        public async Task<LoginOutputDto> FrontLogout()
        {
            var output = new LoginOutputDto();
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var refreshToken = cookieManager.Get("RefreshToken");
            try
            {
                if (Guid.TryParse(refreshToken, out var refreshTokenId))
                {
                    var token = await db.Tokens.FirstOrDefaultAsync(e =>
                        e.id == refreshTokenId &&
                        e.websiteId == websiteId);
                    if (token?.UserID != null)
                    {
                        db.Account_Logs.Add(new Account_Log
                        {
                            UUID = token.UUID,
                            WebsiteId = websiteId,
                            Status = (int)AccountStatusEnum.登出,
                            CreatorUserId = token.UserID.Value,
                            CreationTime = DateTime.Now,
                        });
                        token.UserID = null;
                        await db.SaveChangesAsync();
                    }
                }
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            finally
            {
                ClearFrontCookies(websiteId);
            }
            return output;
        }
        public Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto) => core.AddFrontUser(dto);
        public Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto) => core.FrontUserEdit(dto);
        public Task<ResponseUserEditDto> GetFrontUserData() => core.GetFrontUserData();
        public Task<string> GetFrontUserLevelName() => core.GetFrontUserLevelName();
        public Task<ResponseMessageDto> AccountOpening(Guid openId) => core.AccountOpening(openId);
        public Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto) => core.ReSendOpening(dto);
        public Task<ResponseMessageDto> SendForget(SendForgetDto dto) => core.SendForget(dto);
        public Task<ResponseMessageDto> ForgetIdCheck(Guid forgetId) => core.ForgetIdCheck(forgetId);
        public Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto) => core.PasswordChage(dto);
        public Task<ResponseMessageDto> EmailChage(EmailChangeDto dto) => core.EmailChage(dto);
        public CheckRedirectUrlOutputDto checkRedirectUrl(string? redirectUrl)
        {
            var output = new CheckRedirectUrlOutputDto();
            if (string.IsNullOrEmpty(redirectUrl)) return output;

            redirectUrl = WebUtility.UrlDecode(redirectUrl);
            if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri))
                return output;

            var redirectBaseUrl = $"{redirectUri.Scheme}://{redirectUri.Host}";
            if (!redirectUri.IsDefaultPort) redirectBaseUrl += $":{redirectUri.Port}";
            redirectBaseUrl = redirectBaseUrl.TrimEnd('/');

            var matchedSite = db.Websites
                .Where(w => w.DefaultUrl != null)
                .AsEnumerable()
                .FirstOrDefault(w => redirectBaseUrl.StartsWith(
                    w.DefaultUrl!.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase));

            if (env.IsProduction() && matchedSite != null)
            {
                output.RedirectUrl = matchedSite.DefaultUrl ?? string.Empty;
                output.FK_WebsiteId = matchedSite.Id;
                return output;
            }

            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(redirectUri.Query);
            if (queryParams.TryGetValue("siteId", out var siteIdValue) &&
                long.TryParse(siteIdValue, out var siteId) &&
                db.Websites.Any(w => w.Id == siteId))
            {
                output.RedirectUrl = redirectBaseUrl;
                output.FK_WebsiteId = siteId;
            }
            return output;
        }

        private async Task<LoginOutputDto> NoPasswordLogin(
            FrontUser frontUser,
            long websiteId,
            FrontLoginInputDto? dto)
        {
            var output = new LoginOutputDto { Success = false };
            try
            {
                var tokenItem = await tokenAppService.CreateToken();
                var tempUuid = await tokenAppService.GetUUID();
                var token = await db.Tokens.FirstOrDefaultAsync(e =>
                    e.id == tokenItem.RefreshToken &&
                    e.websiteId == websiteId);
                if (token == null)
                    throw new InvalidOperationException("登入狀態與目前網站不符，請重新登入");

                if (frontUser.UUID == Guid.Empty)
                {
                    var uuidAlreadyUsed = await db.FrontUsers.AnyAsync(e => e.UUID == token.UUID);
                    frontUser.UUID = uuidAlreadyUsed ? Guid.NewGuid() : token.UUID;
                }

                token.UUID = frontUser.UUID;
                token.UserID = frontUser.FK_User;
                if (!string.IsNullOrEmpty(frontUser.Email))
                {
                    output.Token = await tokenAppService.CreateToken(
                        frontUser.Email,
                        token.id,
                        CookiePurposeEnum.FrontAuthToken);
                }
                await db.SaveChangesAsync();

                output.Secret = token.id;
                output.EndDateTime = DateTime.Now.AddMinutes(30);
                if (dto != null) dto.Password = "******";
                await loginUserData.SetLogs(
                    frontUser.Id,
                    websiteId,
                    JsonConvert.SerializeObject(dto),
                    JsonConvert.SerializeObject(output));

                db.Account_Logs.Add(new Account_Log
                {
                    UUID = frontUser.UUID,
                    WebsiteId = websiteId,
                    Status = (int)AccountStatusEnum.登入,
                    LastLoginTime = DateTime.Now,
                    CreatorUserId = frontUser.Id,
                    CreationTime = DateTime.Now,
                });
                await db.SaveChangesAsync();

                if (frontUser.UUID != tempUuid &&
                    tempUuid != Guid.Empty &&
                    frontUser.UUID != Guid.Empty)
                {
                    var mappingExists = await db.MappingOldNewUUID.AnyAsync(e =>
                        e.UserUUID == tempUuid || e.TempUUID == tempUuid);
                    if (!mappingExists)
                    {
                        var mapping = new MappingOldNewUUID
                        {
                            TempUUID = tempUuid,
                            UserUUID = frontUser.UUID
                        };
                        db.MappingOldNewUUID.Add(mapping);
                        await loginUserData.SaveChanges(mapping);
                        await shoppingCartAppService.UpdateUUID(frontUser.UUID, tempUuid);
                    }
                }

                output.Success = true;
                if (dto != null)
                {
                    cookieManager.Set(
                        "RememberMe",
                        dto.Remember ? "1" : "0",
                        CookiePurposeEnum.RefreshIdentifier);
                    if (!dto.Remember)
                    {
                        if (output.Token != null)
                            cookieManager.Set("Token", output.Token, CookiePurposeEnum.none);
                        if (output.Secret != null)
                            cookieManager.Set("RefreshToken", output.Secret.Value.ToString(), CookiePurposeEnum.none);
                        cookieManager.Delete("RememberMe");
                    }
                }
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            return output;
        }

        private void ClearFrontCookies(long websiteId)
        {
            cookieManager.Delete("Token");
            cookieManager.Delete("RefreshToken");
            cookieManager.Delete("RememberMe");
            cookieManager.Delete("sessionId");
            cookieManager.Delete("sessionRemember");
            cookieManager.Delete($".Coker6.Front.Auth.{websiteId}");
        }
    }
}
