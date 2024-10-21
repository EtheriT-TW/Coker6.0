using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Net;
using EtheriT.Coker.Application.Dto;
using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using EtheriT.Coker.Core.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using EtheriT.Coker.Application.Shared.Dto.User;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto;
using System.Xml.Linq;
using AutoMapper;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.Data;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Token;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.Authorization
{
    public class AccountAppService : IAccountAppService
    {
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly string controllerName;
        private readonly MailAppService mailAppService;
        private readonly INewsletterAppService newsletterAppService;
        private readonly IConfiguration configuration;
        public AccountAppService(
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            MailAppService mailAppService,
            INewsletterAppService newsletterAppService,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.mailAppService = mailAppService;
            this.newsletterAppService = newsletterAppService;
            this.configuration = configuration;
            controllerName = "Account";
        }
        public async Task<LoginOutputDto> Login(LoginInputDto dto)
        {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            long? userId = null, websiteId = null;
            try
            {
                if (string.IsNullOrEmpty(dto.UserName)) throw new Exception("使用者名稱不可為空");
                else if (string.IsNullOrEmpty(dto.Password)) throw new Exception("密碼不可為空");
                var users = db.Users.Where(e => e.Account == dto.UserName || e.CellPhone == dto.UserName || e.Email == dto.UserName);
                if (users.Any())
                {
                    var user = await users.FirstOrDefaultAsync();
                    string password = user.Password;
                    userId = user.Id;
                    if (passwordHasher.VerifyHashedPassword(password, dto.Password))
                    {
                        DateTime dateTime = DateTime.Now;
                        DateTime EndDateTime = dateTime.AddMinutes(30);
                        long bindID = 0;
                        if (httpContextAccessor.HttpContext != null)
                        {
                            long.TryParse(httpContextAccessor.HttpContext.Request.Cookies["lastWebSite"], out bindID);
                        }
                        if (!await loginUserData.CheckedWebSiteId(user.Id, bindID))
                        {
                            var defaultWeb = await db.MappingUserAndWebsites.Where(e => !e.IsDeleted).Where(m => m.UserId == user.Id).FirstOrDefaultAsync();
                            if (defaultWeb != null)
                            {
                                bindID = defaultWeb.UserId;
                                websiteId = defaultWeb.WebsiteId;
                            }
                            else throw new Exception("無可管理的網站");
                        }
                        Core.Models.Token t = new Core.Models.Token
                        {
                            ip = loginUserData.GetClientIP() ?? "",
                            UserID = user.Id,
                            StartTime = dateTime,
                            EndTime = EndDateTime,
                            websiteId = websiteId ?? 0
                        };
                        db.Tokens.Add(t);
                        db.SaveChanges();
                        output.Success = true;
                        output.Token = await tokenAppService.CreateToken(user.Account, t.id);
                        output.Secret = t.id;
                        output.EndDateTime = EndDateTime;
                    }
                    else throw new Exception("帳號或密碼錯誤");
                }
                else throw new Exception("帳號或密碼錯誤");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            dto.Password = "******";
            await loginUserData.SetLogs("Account", "Login", userId, websiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<LoginOutputDto> FrontLogin(FrontLoginInputDto dto)
        {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            try
            {
                if (string.IsNullOrEmpty(dto.Email)) throw new Exception("電子信箱不可為空");
                else if (string.IsNullOrEmpty(dto.Password)) throw new Exception("密碼不可為空");
                var tokenItem = await tokenAppService.CreateToken();
                Guid Temp_UUID = await tokenAppService.GetUUID();
                var user = await db.FrontUsers.Where(e => e.Email == dto.Email && !e.IsDeleted).FirstOrDefaultAsync();
                if (user != null)
                {
                    var mapuserandweb = await db.MappingFrontUserAndWebsite.Where(e => e.FK_UserId == user.Id && e.FK_WebsiteId == dto.WebsiteId && !e.IsDeleted).FirstOrDefaultAsync();
                    Account_Log account_Log = new Account_Log();

                    if (mapuserandweb != null)
                    {
                        if (mapuserandweb.Status == (int)UserStatusEnum.停權 && user.LockTime!=null && ((DateTime)user.LockTime).AddMinutes(15).CompareTo(DateTime.Now) > 0)
                        {
                            throw new Exception($"帳號鎖定中，請於{((DateTime)user.LockTime).AddMinutes(15)}後再次嘗試。");
                        }
                        else
                        {
                            string password = user.Password;
                            if (passwordHasher.VerifyHashedPassword(password, dto.Password))
                            {
                                if(mapuserandweb.Status == (int)UserStatusEnum.未開通)
                                {
                                    output.Success = false;
                                    output.Message = "未開通";
                                    throw new Exception("尚未開通會員，請至郵箱確認或重新寄送通知信。");
                                }
                                else
                                {
                                    DateTime dateTime = DateTime.Now;
                                    DateTime EndDateTime = dateTime.AddMinutes(30);

                                    var token = await db.Tokens.Where(e => e.UUID == Temp_UUID && e.id == tokenItem.RefreshToken && e.websiteId == dto.WebsiteId).FirstOrDefaultAsync();
                                    if (token != null)
                                    {
                                        token.UUID = mapuserandweb.UUID;
                                        token.UserID = user.Id;
                                        if (user != null && !string.IsNullOrEmpty(user.Email))
                                        {
                                            output.Token = await tokenAppService.CreateToken(user.Email, token.id, 15);
                                        }
                                    }
                                    db.SaveChanges();
                                    output.Secret = token.id;
                                    output.EndDateTime = EndDateTime;

                                    dto.Password = "******";
                                    await loginUserData.SetLogs("Account", "Login", user.Id, dto.WebsiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));

                                    user.ErrorTimes = 0;
                                    await loginUserData.SaveChanges(user);
                                    if (mapuserandweb.Status == (int)UserStatusEnum.停權)
                                    {
                                        mapuserandweb.Status = (int)UserStatusEnum.開通;
                                        await loginUserData.SaveChanges(mapuserandweb);
                                    }

                                    account_Log = new Account_Log()
                                    {
                                        UUID = mapuserandweb.UUID,
                                        WebsiteId = dto.WebsiteId,
                                        Status = (int)AccountStatusEnum.登入,
                                        LastLoginTime = DateTime.Now,
                                        CreatorUserId = user.Id,
                                        CreationTime = DateTime.Now,
                                    };
                                    db.Account_Logs.Add(account_Log);
                                    db.SaveChanges();

                                    if (mapuserandweb.UUID != Temp_UUID)
                                    {
                                        MappingOldNewUUID mapoldnew = new MappingOldNewUUID
                                        {
                                            TempUUID = Temp_UUID,
                                            UserUUID = mapuserandweb.UUID
                                        };
                                        db.MappingOldNewUUID.Add(mapoldnew);
                                        await loginUserData.SaveChanges(mapoldnew);
                                    }

                                    output.Success = true;
                                }
                            }
                            else
                            {
                                user.ErrorTimes += 1;
                                account_Log = new Account_Log()
                                {
                                    UUID = mapuserandweb.UUID,
                                    WebsiteId = dto.WebsiteId,
                                    ErrorTimes = user.ErrorTimes
                                };
                                if (user.ErrorTimes >= 3)
                                {
                                    user.LockTime = DateTime.Now;
                                    account_Log.LockTime = user.LockTime;

                                    mapuserandweb.Status = (int)UserStatusEnum.停權;
                                    account_Log.Status = (int)AccountStatusEnum.停權;

                                    await loginUserData.SaveChanges(user);
                                    await loginUserData.SaveChanges(mapuserandweb);

                                    account_Log.CreatorUserId = user.Id;
                                    account_Log.CreationTime = DateTime.Now;

                                    db.Account_Logs.Add(account_Log);
                                    db.SaveChanges();

                                    output.Success = false;
                                    throw new Exception("密碼錯誤次數三次以上，請於15分鐘後再次嘗試。");
                                }
                                else
                                {
                                    account_Log.Status = (int)AccountStatusEnum.登入;

                                    account_Log.CreatorUserId = user.Id;
                                    account_Log.CreationTime = DateTime.Now;
                                    db.Account_Logs.Add(account_Log);
                                    db.SaveChanges();
                                    
                                    output.Success = false;
                                    throw new Exception("帳號或密碼有誤，請重新輸入");
                                }
                            }
                        }
                    }
                    else
                    {
                        output.Message = "已存在其他站";
                        throw new Exception("帳戶尚未於此站開通，是否帶入相關資料(姓名、密碼等)於此站開通?");
                    }
                }
                else throw new Exception("帳號或密碼有誤，請重新輸入");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<LoginOutputDto> FrontLogout() {
            LoginOutputDto output = new LoginOutputDto();
            var tokenItem = await tokenAppService.CreateToken();
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                httpContextAccessor.HttpContext?.Response.Cookies.Delete("Token");
                httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                var token = await db.Tokens.Where(e => e.id == tokenItem.RefreshToken).FirstOrDefaultAsync();
                if (token != null && token.UserID != null)
                {
                    Account_Log account_Log = new Account_Log()
                    {
                        UUID = token.UUID,
                        WebsiteId = websiteId,
                        Status = (int)AccountStatusEnum.登出,
                        CreatorUserId = token.UserID.Value,
                        CreationTime = DateTime.Now,
                    };
                    db.Account_Logs.Add(account_Log);
                    db.SaveChanges();
                    token.UserID = null;
                    db.SaveChanges();
                    output.Success = true;
                }
            }
            catch(Exception e) {
                tokenItem.Error = e.Message;
            }
            return output;
        }
        [Authorize]
        public async Task<UserDto> GetCurrentUser()
        {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            UserDto output = new UserDto();
            try
            {
                var theUser = await db.Users
                    .Where(e => e.Account == name).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (theUser != null)
                {
                    var o = from w in db.Websites
                            join m in db.MappingUserAndWebsites on w.Id equals m.WebsiteId
                            where m.UserId == theUser.Id
                            select new Webs.Dto.WebsDto
                            {
                                Id = w.Id,
                                Name = w.Title
                            };
                    output.Account = theUser.Account;
                    output.UserName = theUser.Name;
                    output.Webs = await o.ToListAsync();
                }
            }
            catch
            {
                output.Account = "";
            }
            return output;
        }
        public async Task<LoginOutputDto> Chech()
        {
            LoginOutputDto response = new LoginOutputDto
            {
                Success = false
            };
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            string? secret = httpContextAccessor.HttpContext?.Request.Headers["secret"].ToString();
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (string.IsNullOrEmpty(secret))
                    {
                        response.Success = true;
                    }
                    else
                    {
                        var t = await db.Tokens.Where(e => e.id == Guid.Parse(secret ?? "")).FirstOrDefaultAsync();
                        if (t != null)
                        {
                            var users = await db.Users.Where(e => e.Id == t.UserID).FirstOrDefaultAsync();
                            if (t.EndTime < DateTime.Now.AddMinutes(10))
                            {
                                t.EndTime = DateTime.Now.AddMinutes(30);
                                db.SaveChanges();
                            }
                            response.Success = true;
                            response.Token = await tokenAppService.CreateToken(users.Account, t.id);
                            response.Secret = t.id;
                            response.EndDateTime = t.EndTime.Value;
                        }
                        else throw new Exception("登入已過期");
                    }
                }
                else throw new Exception("登入已過期");
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }
            var removeToken = db.Tokens.Where(e => e.EndTime < DateTime.Now);
            db.Tokens.RemoveRange(removeToken);
            db.SaveChanges();
            return response;
        }
        public async Task<ResponseMessageDto> Logout()
        {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            string? secret = httpContextAccessor.HttpContext?.Request.Headers["secret"].ToString();
            if (!string.IsNullOrEmpty(secret))
            {
                var t = await db.Tokens.Where(e => e.id == Guid.Parse(secret ?? "")).FirstOrDefaultAsync();
                if (t != null)
                {
                    db.Tokens.Remove(t);
                    db.SaveChanges();
                }
            }
            return new ResponseMessageDto
            {
                Success = await tokenAppService.DelToken()
            };
        }
        public async Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto)
        {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            long userId = await loginUserData.GetUserId();
            var users = await db.Users
                .Where(e => e.Id == userId)
                .Where(e => !e.IsDeleted)
                .Where(e => e.Status != 0)
                .FirstOrDefaultAsync();
            if (users == null) output.Message = "使用者已被登出";
            else if (!passwordHasher.VerifyHashedPassword(users.Password, dto.Password)) output.Message = "原始密碼錯誤";
            else
            {
                try
                {
                    string passwordError = checkPassword(dto.NewPassword);
                    if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);

                    string HashedPassword = passwordHasher.HashPassword(dto.NewPassword);
                    users.Password = HashedPassword;
                    await loginUserData.SaveChanges(users);
                    output.Success = true;
                }
                catch (Exception ex)
                {
                    output.Message = ex.Message;
                }
            }
            await loginUserData.SetLogs("Account", "UpdatePassword", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto)
        {
            ResponseUserEditDto output = new ResponseUserEditDto();
            try
            {
                var siteId = await loginUserData.GetWebsiteId();
                var theUser = await db.Users.Include(e => e.Webs)
                    .Where(e => e.Id == dto.Id)
                    .Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (theUser != null)
                {
                    var webMap = theUser.Webs.Where(e => e.WebsiteId == siteId);
                    if (webMap.Any())
                    {
                        mapper.Map(theUser, output.data);
                    }
                    else throw new Exception("該使用者並未授權管理該網站");
                }
                else throw new Exception("使用者不存在");
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "GetEditUser", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<ResponseMessageDto> AddUser(AddUser dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var theUser = await db.Users
                    .Where(e => e.Account == dto.Account || (!string.IsNullOrEmpty(e.Email) && e.Email == dto.Email))
                    .Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                string passwordError = checkPassword(dto.Password);
                if (theUser != null) throw new Exception("該使用者的帳號或信箱已存在");
                else if (dto.Password != dto.PasswordConfirm) throw new Exception("該使用者的帳號或信箱已存在");
                else if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);
                else
                {
                    User user = mapper.Map<User>(dto);
                    user.Password = passwordHasher.HashPassword(dto.Password);
                    db.Users.Add(user);
                    await loginUserData.SaveChanges(user);
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            dto.Password = "*********";
            dto.PasswordConfirm = "*********";
            await loginUserData.SetLogs(controllerName, "saveEditUser", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                long WebsiteID = dto.WebsiteId == 0 ? await loginUserData.GetWebsiteId() : dto.WebsiteId;

                var theUser = await db.FrontUsers
                    .Where(e => (!string.IsNullOrEmpty(e.Email) && e.Email == dto.Email))
                    .Where(e => !e.IsDeleted).FirstOrDefaultAsync();

                string passwordError = checkPassword(dto.Password);

                if (theUser != null)
                {
                    var UserMapWeb = await db.MappingFrontUserAndWebsite
                        .Where(e => e.FK_UserId == theUser.Id)
                        .Where(e => e.FK_WebsiteId == WebsiteID)
                        .Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                    if (UserMapWeb != null)
                    {
                        switch (UserMapWeb.Status)
                        {
                            case (int)UserStatusEnum.未開通:
                                if (UserMapWeb.OpenIDSendDate.AddDays(1) < DateTime.Now)
                                {
                                    response.Message = "重新寄送通知信";
                                    throw new Exception("郵箱已存在且已過開通期限，是否重新寄送通知信？");
                                }
                                else
                                {
                                    response.Message = "重新寄送通知信";
                                    throw new Exception("郵箱已存在但尚未開通，請至郵箱確認或重新寄送通知信。");
                                }
                            default:
                                response.Message = "郵箱已存在";
                                throw new Exception("郵箱已存在，請更換一個郵箱或直接登入。");
                        }
                    }
                    else
                    {
                        response.Message = "已存在其他站";
                        throw new Exception("郵箱已存在於其他網站，是否帶入相關資料(姓名、密碼等)於此站開通?");
                    }
                }
                else if (dto.Password != dto.PasswordConfirm) throw new Exception("輸入的密碼不相符");
                else if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);
                else
                {
                    FrontUser frontUser = mapper.Map<FrontUser>(dto);
                    frontUser.Password = passwordHasher.HashPassword(dto.Password);
                    var user = await db.Users.Where(e => e.Email == frontUser.Email).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        user = mapper.Map<User>(dto);
                        user.Password = frontUser.Password;
                        db.Users.Add(user);
                        await loginUserData.SaveChanges(user);
                    }
                    frontUser.FK_User = user.Id;
                    db.FrontUsers.Add(frontUser);
                    await loginUserData.SaveChanges(frontUser);

                    MappingFrontUserAndWebsite mapuserandweb = new MappingFrontUserAndWebsite()
                    {
                        FK_WebsiteId = dto.WebsiteId,
                        UUID = UUID,
                        FK_UserId = frontUser.Id,
                        Status = (int)UserStatusEnum.未開通,
                        OpenID = Guid.NewGuid(),
                        OpenIDSendDate = DateTime.Now
                    };
                    db.MappingFrontUserAndWebsite.Add(mapuserandweb);
                    await loginUserData.SaveChanges(mapuserandweb);

                    MappingFrontUserAndRole mapuserrole = new MappingFrontUserAndRole()
                    {
                        UUID = UUID,
                        UserId = mapuserandweb.Id,
                        RoleId = dto.RoleId,
                    };
                    db.MappingFrontUserAndRoles.Add(mapuserrole);
                    await loginUserData.SaveChanges(mapuserrole);

                    Account_Log account_Log = new Account_Log()
                    {
                        UUID = UUID,
                        WebsiteId = dto.WebsiteId,
                        Status = (int)AccountStatusEnum.註冊,
                        CreatorUserId = frontUser.Id,
                        CreationTime = DateTime.Now,
                };
                    db.Account_Logs.Add(account_Log);
                    db.SaveChanges();

                    var senddto = mapper.Map<SendOpeningDto>(dto);
                    senddto.OpenId = mapuserandweb.OpenID;
                    senddto.OpenIdSendDate = mapuserandweb.OpenIDSendDate;

                    await SendOpening(senddto);

                    response.Success = true;
                }
                dto.Password = "*********";
                dto.PasswordConfirm = "*********";
                await loginUserData.SetLogs(controllerName, "saveEditUser", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> AccountOpening(Guid OpenId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var UserData = await db.MappingFrontUserAndWebsite.Where(e => e.OpenID == OpenId).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (UserData != null)
                {
                    if (UserData.Status == 0)
                    {
                        if (UserData.OpenIDSendDate.AddDays(1).CompareTo(DateTime.Now) < 0)
                        {
                            response.Message = "ReSendOrNot";
                            throw new Exception("開通連結已失效，是否重新寄送？");
                        }
                        else
                        {
                            UserData.Status = 1;
                            UserData.OpenDate = DateTime.Now;

                            await loginUserData.SaveChanges(UserData);

                            response.Success = true;
                        }
                    }
                    else throw new Exception("帳號已開通。");
                }
                else throw new Exception("連結不存在。");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }
        public async Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var UserData = new Core.Models.MappingFrontUserAndWebsite();

                if (dto.OpenId == null)
                {

                    Guid UUID = await tokenAppService.GetUUID();
                    UserData = await db.MappingFrontUserAndWebsite.Where(e => e.UUID == UUID).Where(e => e.FK_WebsiteId == dto.WebsiteId).Where(e => !e.IsDeleted).FirstOrDefaultAsync();

                    if (UserData == null)
                    {
                        var theUser = await db.FrontUsers
                            .Where(e => (!string.IsNullOrEmpty(e.Email) && e.Email == dto.Email))
                            .Where(e => !e.IsDeleted).FirstOrDefaultAsync();

                        MappingFrontUserAndWebsite mapuserandweb = new MappingFrontUserAndWebsite()
                        {
                            FK_WebsiteId = dto.WebsiteId,
                            UUID = UUID,
                            FK_UserId = theUser.Id,
                            Status = (int)UserStatusEnum.未開通,
                            OpenID = Guid.NewGuid(),
                            OpenIDSendDate = DateTime.Now
                        };
                        db.MappingFrontUserAndWebsite.Add(mapuserandweb);
                        await loginUserData.SaveChanges(mapuserandweb);

                        Account_Log account_Log = new Account_Log()
                        {
                            UUID = UUID,
                            WebsiteId = dto.WebsiteId,
                            Status = (int)AccountStatusEnum.註冊,
                            CreatorUserId = theUser.Id,
                            CreationTime = DateTime.Now,
                        };
                        db.Account_Logs.Add(account_Log);
                        db.SaveChanges();

                        dto.OpenId = mapuserandweb.OpenID;
                        dto.OpenIdSendDate = mapuserandweb.OpenIDSendDate;

                        response = await SendOpening(dto);
                        return response;
                    }
                }
                else
                {
                    UserData = await db.MappingFrontUserAndWebsite.Where(e => e.OpenID == dto.OpenId).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                }

                if (UserData != null)
                {
                    var FrontUser = await db.Users.Where(e => e.Id == UserData.FK_UserId).FirstOrDefaultAsync();
                    if (FrontUser != null)
                    {
                        UserData.OpenID = Guid.NewGuid();
                        UserData.OpenIDSendDate = DateTime.Now;
                        await loginUserData.SaveChanges(UserData);

                        dto.Email = FrontUser.Email;
                        dto.Name = FrontUser.Name;
                        dto.OpenId = UserData.OpenID;
                        dto.OpenIdSendDate = UserData.OpenIDSendDate;

                        await SendOpening(dto);
                        response = await SendOpening(dto);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SendOpening(SendOpeningDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var mailhtml = $"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！歡迎加入{dto.WebsiteName}會員</h2>" +
                                                        $"<hr/>" +
                                                        $"<div>以下是您的帳號資料，請熟記以下重要訊息</div>" +
                                                        $"<br/>" +
                                                        $"<div class='d-flex text-bold'><div>您的帳號：</div><u>{dto.Email}</u></div>" +
                                                        $"<br/>" +
                                                        $"<div text-bold>開通帳號網址</div>" +
                                                        $"<a href='{dto.WebsiteLink}/?useraction=accountoping&openid={dto.OpenId}' title='前往開通帳號'>{dto.WebsiteLink}/?useraction=accountoping&openid={dto.OpenId}</a>" +
                                                        $"<div class='text-gray'>為了啟動您的帳號，請點選連結或是複製連結在瀏覽器貼上</div>" +
                                                        $"<div class='text-gray'>這個連結僅能使用一次，並於 {((DateTime)dto.OpenIdSendDate).AddDays(1)} 到期，請在期限內開通。</div>" +
                                                        $"<div class='text-gray'>感謝您的加入！~</div>" +
                                                        $"<br/>" +
                                                        $"<div class='text-bold text-red'>提醒您：此封『會員通知』微系統發出，請勿直接回覆。</div>" +
                                                        $"<hr/>" +
                                                        $"<hr/>" +
                                                        $"<div>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款</div>" +
                                                        $"<div>若有上述情形，請立即撥打165防詐騙專線查詢</div>" +
                                                        $"<hr/>" +
                                                        $"<hr/>" +
                                                        $"<br/></div>";
                var mailcss = ".text-size1{ font-size: 1rem; } .d-flex{ display: flex; } .text-bold { font-weight: bold; } .text-red { color: red;} .text-gray{ color: gray ; }";

                await mailAppService.sendMail(new SenderDto
                {
                    Recipients = new List<MailUserDataDto>(){
                                    new MailUserDataDto()
                                    {
                                        Name = dto.Name,
                                        Email = dto.Email,
                                    }
                                },
                    Subject = $"加入會員通知【{dto.WebsiteName}】",
                    Body = mailhtml,
                    Css = mailcss,
                }, dto.WebsiteId);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        private string checkPassword(string password)
        {
            string error = string.Empty;
            int matchCount = 0;
            /*
                至少有一個數字
                至少有一個大寫或小寫英文字母
                至少有一個特殊符號
                字串長度在 8 ~ 30 個字母之間
                Regex regex = new Regex(@"^(?=.*\d)(?=.*[a-zA-Z])(?=.*\W).{8,30}$");
             */
            try
            {
                //密碼長度須為8-30之間
                if (password.Length < 8 || password.Length > 30) throw new Exception("密碼長度須為8-30之間");
                //密碼有數字
                Regex regex1 = new Regex(@"^(?=.*\d).{8,30}$");
                if (regex1.IsMatch(password)) matchCount++;
                //密碼有英文小寫
                Regex regex2 = new Regex(@"^(?=.*[a-z]).{8,30}$");
                if (regex2.IsMatch(password)) matchCount++;
                //密碼有英文大寫
                Regex regex3 = new Regex(@"^(?=.*[A-Z]).{8,30}$");
                if (regex3.IsMatch(password)) matchCount++;
                //密碼有符號
                Regex regex4 = new Regex(@"^(?=.*\W).{8,30}$");
                if (regex4.IsMatch(password)) matchCount++;
                if (matchCount < 3) throw new Exception("密碼須滿足有英文大寫、小寫、符號、數字中的三個");
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return error;
        }
    }
}