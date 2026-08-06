using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Common;
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
using System.Text.RegularExpressions;

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
        private readonly IMapper mapper;
        private readonly IFileUploadAppService fileUploadAppService;

        public BackstageAccountAppService(
            AccountAppService core,
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor,
            ICookieManagerAppService cookieManager,
            IConfiguration configuration,
            IMapper mapper,
            IFileUploadAppService fileUploadAppService)
        {
            this.core = core;
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.httpContextAccessor = httpContextAccessor;
            this.cookieManager = cookieManager;
            this.configuration = configuration;
            this.mapper = mapper;
            this.fileUploadAppService = fileUploadAppService;
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
                {
                    var lastWebsiteCookie =
                        httpContextAccessor.HttpContext.Request.Cookies["LastWebSite"] ??
                        httpContextAccessor.HttpContext.Request.Cookies["lastWebSite"];
                    long.TryParse(lastWebsiteCookie, out bindId);
                }

                if (!await loginUserData.CheckedWebSiteId(user.Id, bindId))
                {
                    var isSystemUser = await db.MappingUserAndRoles
                        .Include(m => m.Role)
                        .AnyAsync(m =>
                            m.UserId == user.Id &&
                            m.Role != null &&
                            m.Role.Type == RoleTypeEnum.系統維護);

                    if (isSystemUser)
                    {
                        bindId = await db.Websites
                            .Where(w => !w.IsDeleted)
                            .OrderBy(w => w.Id)
                            .Select(w => w.Id)
                            .FirstOrDefaultAsync();
                        if (bindId <= 0)
                            throw new Exception("此帳號沒有可用的管理站台");
                    }
                    else
                    {
                        var defaultWeb = await db.MappingUserAndWebsites
                            .Where(e => !e.IsDeleted && e.UserId == user.Id)
                            .OrderByDescending(e => e.WebsiteId)
                            .FirstOrDefaultAsync();
                        if (defaultWeb == null)
                            throw new Exception("無可管理的網站");

                        bindId = defaultWeb.WebsiteId;
                    }
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
                cookieManager.Set(
                    "LastWebSite",
                    websiteId.Value.ToString(),
                    CookiePurposeEnum.LastWebsite);
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

        public async Task<UserDto> GetCurrentUser()
        {
            var output = new UserDto();
            try
            {
                var name = httpContextAccessor.HttpContext?.User.Identity?.Name;
                var user = await db.Users.FirstOrDefaultAsync(e => e.Account == name && !e.IsDeleted);
                if (user == null) return output;

                output.Account = user.Account;
                output.UserName = user.Name;
                var profileImages = await fileUploadAppService.getImgFiles(new Shared.Dto.Files.FileGetImgInputDto
                {
                    Sid = user.Id,
                    Size = 3,
                    Type = (int)FileBindTypeEnum.大頭貼
                });
                output.ProfileImage = profileImages.FirstOrDefault()?.Link ?? "/images/user.png";
                output.Webs = await (
                    from website in db.Websites
                    join mapping in db.MappingUserAndWebsites on website.Id equals mapping.WebsiteId
                    where mapping.UserId == user.Id
                    select new Webs.Dto.WebsDto
                    {
                        Id = website.Id,
                        Name = website.Title,
                        DefaultUrl = website.DefaultUrl ?? string.Empty
                    }).ToListAsync();
            }
            catch
            {
                output.Account = string.Empty;
            }
            return output;
        }

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

        public async Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto)
        {
            var output = new ResponseMessageDto();
            var userId = await loginUserData.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == userId && !e.IsDeleted && e.Status != 0);
            if (user == null) output.Message = "使用者已被登出";
            else if (!passwordHasher.VerifyHashedPassword(user.Password, dto.Password)) output.Message = "原始密碼錯誤";
            else
            {
                var passwordError = CheckPassword(dto.NewPassword);
                if (!string.IsNullOrEmpty(passwordError)) output.Message = passwordError;
                else
                {
                    user.Password = passwordHasher.HashPassword(dto.NewPassword);
                    await loginUserData.SaveChanges(user);
                    output.Success = true;
                }
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }

        public async Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto)
        {
            var output = new ResponseUserEditDto();
            try
            {
                var siteId = await loginUserData.GetWebsiteId();
                var user = await db.Users.Include(e => e.Webs)
                    .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);
                if (user == null) throw new Exception("使用者不存在");
                if (!user.Webs.Any(e => e.WebsiteId == siteId))
                    throw new Exception("該使用者並未授權管理該網站");
                mapper.Map(user, output.data);
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }

        public async Task<ResponseMessageDto> AddUser(AddUser dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var existingUser = await db.Users.FirstOrDefaultAsync(e =>
                    !e.IsDeleted &&
                    (e.Account == dto.Account || (!string.IsNullOrEmpty(e.Email) && e.Email == dto.Email)));
                var passwordError = CheckPassword(dto.Password);
                if (existingUser != null) throw new Exception("該使用者的帳號或信箱已存在");
                if (dto.Password != dto.PasswordConfirm) throw new Exception("輸入的密碼不相符");
                if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);

                var user = mapper.Map<User>(dto);
                user.Password = passwordHasher.HashPassword(dto.Password);
                db.Users.Add(user);
                await loginUserData.SaveChanges(user);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            dto.Password = "*********";
            dto.PasswordConfirm = "*********";
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<ResponseMessageDto> SendForget(long userId)
        {
            return await core.SendForget(userId);
        }

        private void ClearBackstageCookies()
        {
            cookieManager.Delete("BackstageToken");
            cookieManager.Delete("BackstageRefreshToken");
            cookieManager.Delete(".Coker6.Back.Auth");
        }

        private static string CheckPassword(string password)
        {
            if (password.Length < 8 || password.Length > 32)
                return Shared.i18n.L.get("PasswordRuleLength");
            var matchCount = 0;
            if (Regex.IsMatch(password, @"^(?=.*\d).{8,32}$")) matchCount++;
            if (Regex.IsMatch(password, @"^(?=.*[a-z]).{8,32}$")) matchCount++;
            if (Regex.IsMatch(password, @"^(?=.*[A-Z]).{8,32}$")) matchCount++;
            if (Regex.IsMatch(password, @"^(?=.*\W).{8,32}$")) matchCount++;
            return matchCount < 3 ? Shared.i18n.L.get("PasswordRuleComposition") : string.Empty;
        }
    }
}
