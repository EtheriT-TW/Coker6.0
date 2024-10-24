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
using Microsoft.AspNetCore.Mvc;

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

                var frontuser = await (from user in db.FrontUsers
                                       join MapFrontUserAndWeb in db.MappingFrontUserAndWebsite on user.Id equals MapFrontUserAndWeb.FK_UserId
                                       where user.Email == dto.Email
                                       where MapFrontUserAndWeb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();

                Account_Log account_Log = new Account_Log();

                if (frontuser != null)
                {
                    if (frontuser.Status == (int)UserStatusEnum.停權 && frontuser.LockTime != null && ((DateTime)frontuser.LockTime).AddMinutes(15).CompareTo(DateTime.Now) > 0)
                    {
                        throw new Exception($"帳號鎖定中，請於{((DateTime)frontuser.LockTime).AddMinutes(15)}後再次嘗試。");
                    }
                    else
                    {
                        string password = frontuser.Password;
                        if (passwordHasher.VerifyHashedPassword(password, dto.Password))
                        {
                            if (frontuser.Status == (int)UserStatusEnum.未開通)
                            {
                                output.Success = false;
                                output.Message = "重新寄送通知信";
                                throw new Exception("尚未開通會員，請至郵箱確認或重新寄送通知信。");
                            }
                            else
                            {
                                output = await NoPasswordLogin(frontuser, dto.WebsiteId, dto);

                                frontuser.ErrorTimes = 0;
                                frontuser.LockTime = null;
                                if (frontuser.Status == (int)UserStatusEnum.停權) frontuser.Status = (int)UserStatusEnum.開通;

                                await loginUserData.SaveChanges(frontuser);
                            }
                        }
                        else
                        {
                            frontuser.ErrorTimes += 1;
                            account_Log = new Account_Log()
                            {
                                UUID = frontuser.UUID,
                                WebsiteId = dto.WebsiteId,
                                ErrorTimes = frontuser.ErrorTimes
                            };
                            if (frontuser.ErrorTimes >= 3)
                            {
                                frontuser.LockTime = DateTime.Now;
                                account_Log.LockTime = frontuser.LockTime;

                                frontuser.Status = (int)UserStatusEnum.停權;
                                account_Log.Status = (int)AccountStatusEnum.停權;

                                await loginUserData.SaveChanges(frontuser);

                                account_Log.CreatorUserId = frontuser.Id;
                                account_Log.CreationTime = DateTime.Now;

                                db.Account_Logs.Add(account_Log);
                                db.SaveChanges();

                                output.Success = false;
                                throw new Exception("密碼錯誤次數三次以上，請於15分鐘後再次嘗試。");
                            }
                            else
                            {
                                account_Log.Status = (int)AccountStatusEnum.登入失敗;

                                account_Log.CreatorUserId = frontuser.Id;
                                account_Log.CreationTime = DateTime.Now;
                                db.Account_Logs.Add(account_Log);
                                db.SaveChanges();

                                output.Success = false;
                                throw new Exception("帳號或密碼有誤，請重新輸入");
                            }
                        }
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
                    Account_Log account_Log = new Account_Log()
                    {
                        UUID = t.UUID,
                        WebsiteId = configuration.GetValue<long>("WebConfig:SiteId"),
                        Status = (int)AccountStatusEnum.登出,
                        CreatorUserId = (long)t.UserID,
                        LastLoginTime = DateTime.Now,
                        CreationTime = DateTime.Now,
                    };
                    db.Account_Logs.Add(account_Log);
                    db.Tokens.Remove(t);
                    db.SaveChanges();
                }
            }
            return new ResponseMessageDto
            {
                Success = await tokenAppService.DelToken()
            };
        }
        public async Task<LoginOutputDto> FrontLogout()
        {
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
            catch (Exception e)
            {
                tokenItem.Error = e.Message;
            }
            return output;
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
                long userid = 0;

                var frontuser = await (from user in db.FrontUsers
                                       join MapFrontUserAndWeb in db.MappingFrontUserAndWebsite on user.Id equals MapFrontUserAndWeb.FK_UserId
                                       where user.Email == dto.Email
                                       where MapFrontUserAndWeb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();

                string passwordError = checkPassword(dto.Password);

                if (dto.Password != dto.PasswordConfirm) throw new Exception("輸入的密碼不相符");
                if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);

                if (frontuser == null)
                {
                    frontuser = mapper.Map<FrontUser>(dto);
                    frontuser.Password = passwordHasher.HashPassword(dto.Password);
                    frontuser.UUID = UUID;
                    frontuser.Status = (int)UserStatusEnum.未開通;
                    frontuser.OpenID = Guid.NewGuid();
                    frontuser.OpenIDSendDate = DateTime.Now;

                    var user = await db.Users.Where(e => e.Email == frontuser.Email).FirstOrDefaultAsync();
                    var newuser = new User();
                    if (user == null)
                    {
                        user = mapper.Map<User>(dto);
                        user.Password = frontuser.Password;
                        db.Users.Add(user);
                        await loginUserData.SaveChanges(user);
                        newuser = user;
                    }
                    frontuser.FK_User = user.Id;
                    db.FrontUsers.Add(frontuser);
                    await loginUserData.SaveChanges(frontuser);
                    userid = frontuser.Id;

                    MappingUserAndRole mapuserrole = new MappingUserAndRole()
                    {
                        UserId = user.Id,
                        UUID = frontuser.UUID,
                        RoleId = dto.RoleId,
                    };
                    db.MappingUserAndRoles.Add(mapuserrole);
                    await loginUserData.SaveChanges(mapuserrole);

                    MappingFrontUserAndWebsite mapuserandweb = new MappingFrontUserAndWebsite()
                    {
                        FK_UserId = frontuser.Id,
                        FK_WebsiteId = dto.WebsiteId,
                    };
                    db.MappingFrontUserAndWebsite.Add(mapuserandweb);
                    await loginUserData.SaveChanges(mapuserandweb);

                    Account_Log account_Log = new Account_Log()
                    {
                        UUID = UUID,
                        WebsiteId = dto.WebsiteId,
                        Status = (int)AccountStatusEnum.註冊,
                        CreatorUserId = frontuser.Id,
                        CreationTime = DateTime.Now,
                    };
                    db.Account_Logs.Add(account_Log);
                    db.SaveChanges();

                    var senddto = mapper.Map<SendOpeningDto>(dto);
                    senddto.OpenId = frontuser.OpenID;
                    senddto.OpenIdSendDate = frontuser.OpenIDSendDate;

                    var sendsuccess = await SendOpening(senddto);

                    response = sendsuccess;

                    if (!sendsuccess.Success)
                    {
                        mapuserandweb.IsDeleted = true;
                        mapuserandweb.DeletionTime = DateTime.Now;

                        mapuserrole.IsDeleted = true;
                        mapuserrole.DeletionTime = DateTime.Now;

                        newuser.IsDeleted = true;
                        newuser.DeletionTime = DateTime.Now;

                        frontuser.IsDeleted = true;
                        frontuser.DeletionTime = DateTime.Now;

                        db.SaveChanges();
                    }

                }
                else
                {
                    if (frontuser.Status == (int)UserStatusEnum.未開通)
                    {
                        if (frontuser.OpenIDSendDate.AddDays(1) < DateTime.Now)
                        {
                            response.Message = "重新寄送通知信";
                            throw new Exception("郵箱已存在且已過開通期限，是否重新寄送通知信？");
                        }
                        else
                        {
                            response.Message = "重新寄送通知信";
                            throw new Exception("郵箱已存在但尚未開通，請至郵箱確認或重新寄送通知信。");
                        }
                    }
                    else if (frontuser.Status == (int)UserStatusEnum.開通)
                    {
                        response.Message = "郵箱已存在";
                        throw new Exception("郵箱已存在，請更換一個郵箱或直接登入。");
                    }
                }

                dto.Password = "*********";
                dto.PasswordConfirm = "*********";
                await loginUserData.SetLogs(controllerName, "saveEditUser", userid, dto.WebsiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                long WebsiteID = dto.WebsiteId == 0 ? await loginUserData.GetWebsiteId() : dto.WebsiteId;

                var frontUser = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                       where user.UUID == UUID && mapuserweb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();
                if (frontUser != null)
                {
                    if (dto.Email != null) { }
                    else dto.Email = frontUser.Email;

                    mapper.Map(dto, frontUser);
                    await loginUserData.SaveChanges(frontUser);

                    response.Success = true;
                }
                else throw new Exception("用戶不存在。");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseUserEditDto> GetFrontUserData(Guid refreshToken)
        {
            ResponseUserEditDto UserData = new ResponseUserEditDto();
            try
            {
                var websiteid = configuration.GetValue<long>("WebConfig:SiteId");
                var token = await db.Tokens.Where(e => e.id == refreshToken).FirstOrDefaultAsync();
                if (token != null)
                {
                    var userdata = await (from user in db.FrontUsers
                                          join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                          where user.UUID == token.UUID && mapuserweb.FK_WebsiteId == websiteid
                                          select user).FirstOrDefaultAsync();
                    if (userdata != null)
                    {
                        EditUserDto data = mapper.Map<EditUserDto>(userdata);
                        data.Birthday = userdata.Birthday == null ? "" : ((DateTime)userdata.Birthday).ToString("yyyy-MM-dd");
                        UserData.data = data;
                        UserData.Success = true;
                    }
                    else throw new Exception("會員不存在");
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                UserData.Error = ex.Message;
            }
            return UserData;
        }
        public async Task<ResponseMessageDto> AccountOpening(Guid OpenId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var frontUser = await db.FrontUsers.Where(e => e.OpenID == OpenId).FirstOrDefaultAsync();
                if (frontUser != null)
                {
                    if (frontUser.Status == (int)UserStatusEnum.未開通)
                    {
                        if (frontUser.OpenIDSendDate.AddDays(1).CompareTo(DateTime.Now) < 0)
                        {
                            response.Message = "ReSendOrNot";
                            throw new Exception("開通連結已失效，是否重新寄送？");
                        }
                        else
                        {
                            frontUser.Status = 1;
                            frontUser.OpenDate = DateTime.Now;

                            await loginUserData.SaveChanges(frontUser);

                            var FK_WebsiteId = await db.MappingFrontUserAndWebsite.Where(e => e.FK_UserId == frontUser.Id).Select(e => e.FK_WebsiteId).FirstOrDefaultAsync();

                            await NoPasswordLogin(frontUser, FK_WebsiteId, null);

                            response.Success = true;
                        }
                    }
                    else if (frontUser.Status == (int)UserStatusEnum.開通) throw new Exception("帳號已開通。");
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
                var frontUser = await (from user in db.FrontUsers
                                       join MapFrontUserAndWeb in db.MappingFrontUserAndWebsite on user.Id equals MapFrontUserAndWeb.FK_UserId
                                       where dto.OpenId == null ? user.Email == dto.Email : user.OpenID == dto.OpenId
                                       where MapFrontUserAndWeb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();

                if (frontUser != null)
                {
                    frontUser.OpenID = Guid.NewGuid();
                    frontUser.OpenIDSendDate = DateTime.Now;
                    await loginUserData.SaveChanges(frontUser);

                    dto.Email = frontUser.Email;
                    dto.Name = frontUser.Name;
                    dto.OpenId = frontUser.OpenID;
                    dto.OpenIdSendDate = frontUser.OpenIDSendDate;

                    response = await SendOpening(dto);
                    return response;
                }
                else throw new Exception("發生未知錯誤");
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
                                                        $"<div class='text-bold text-red'>提醒您：此封『會員通知』為系統發出，請勿直接回覆。</div>" +
                                                        $"<hr/>" +
                                                        $"<hr/>" +
                                                        $"<div class='text-bold text-red'>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款</div>" +
                                                        $"<div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢</div>" +
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
        public async Task<ResponseMessageDto> SendForget(SendForgetDto dto)
        {

            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var frontUser = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                       where user.Email == dto.Email && mapuserweb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();

                if (frontUser != null)
                {
                    frontUser.ForgetID = Guid.NewGuid();
                    frontUser.ForgeIDSendDate = DateTime.Now;
                    frontUser.LastModificationTime = DateTime.Now;
                    await loginUserData.SaveChanges(frontUser);

                    var mailhtml = $"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>" +
                                                    $"<hr/>" +
                                                    $"<div>請熟記以下重要訊息</div>" +
                                                    $"<br/>" +
                                                    $"<div class='d-flex text-bold'><div>您的帳號：</div><u>{dto.Email}</u></div>" +
                                                    $"<br/>" +
                                                    $"<div text-bold>密碼重設網址</div>" +
                                                    $"<a href='{dto.WebsiteLink}/?useraction=passwordforget&forgetid={frontUser.ForgetID}' title='前往開通帳號'>{dto.WebsiteLink}/?useraction=passwordforget&forgetid={frontUser.ForgetID}</a>" +
                                                    $"<div class='text-gray'>請由該網址進入重新設定您的密碼</div>" +
                                                    $"<div class='text-gray'>這個連結僅能使用一次，並於 {((DateTime)frontUser.ForgeIDSendDate).AddDays(1)} 到期，請在期限內重設，謝謝。</div>" +
                                                    $"<br/>" +
                                                    $"<div class='text-bold text-red'>提醒您：此封『密碼重設通知』為系統發出，請勿直接回覆。</div>" +
                                                    $"<hr/>" +
                                                    $"<hr/>" +
                                                    $"<div class='text-bold text-red'>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款</div>" +
                                                    $"<div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢</div>" +
                                                    $"<hr/>" +
                                                    $"<hr/>" +
                                                    $"<br/></div>";
                    var mailcss = ".text-size1{ font-size: 1rem; } .d-flex{ display: flex; } .text-bold { font-weight: bold; } .text-red { color: red;} .text-gray{ color: gray ; }";

                    await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>(){
                                    new MailUserDataDto()
                                    {
                                        Name = frontUser.Name,
                                        Email = frontUser.Email,
                                    }
                                },
                        Subject = $"【{dto.WebsiteName}】 密碼重設通知",
                        Body = mailhtml,
                        Css = mailcss,
                    }, dto.WebsiteId);
                    response.Success = true;
                }
                else throw new Exception("會員不存在");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ForgetIdCheck(Guid ForgetId)
        {
            ResponseMessageDto response = new ResponseMessageDto();

            try
            {
                var frontUser = await db.FrontUsers.Where(e => e.ForgetID == ForgetId).FirstOrDefaultAsync();

                if (frontUser != null)
                {
                    if (((DateTime)frontUser.ForgeIDSendDate).AddDays(1).CompareTo(DateTime.Now) > 0)
                    {
                        response.Success = true;
                    }
                    else throw new Exception("連結失效");
                }
                else throw new Exception("連結不存在");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();

            try
            {
                Guid UUID = await tokenAppService.GetUUID();

                FrontUser? frontUser = new FrontUser();

                if (UUID != null)
                {
                    if (dto.ForgetID != null)
                    {
                        frontUser = await (from user in db.FrontUsers
                                           join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                           where user.ForgetID == dto.ForgetID && mapuserweb.FK_WebsiteId == dto.WebsiteId
                                           select user).FirstOrDefaultAsync();
                    }
                    else if (dto.OldPassword != null)
                    {
                        frontUser = await (from user in db.FrontUsers
                                           join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                           where user.UUID == UUID && mapuserweb.FK_WebsiteId == dto.WebsiteId
                                           select user).FirstOrDefaultAsync();
                        if (frontUser != null)
                        {
                            if (frontUser.Status == (int)UserStatusEnum.停權 && frontUser.LockTime != null && ((DateTime)frontUser.LockTime).AddMinutes(15).CompareTo(DateTime.Now) > 0)
                            {
                                throw new Exception($"帳號鎖定中，請於{((DateTime)frontUser.LockTime).AddMinutes(15)}後再次嘗試。");
                            }
                            if (!passwordHasher.VerifyHashedPassword(frontUser.Password, dto.OldPassword))
                            {
                                frontUser.ErrorTimes += 1;
                                Account_Log account_Log = new Account_Log()
                                {
                                    UUID = frontUser.UUID,
                                    WebsiteId = dto.WebsiteId,
                                    ErrorTimes = frontUser.ErrorTimes
                                };
                                if (frontUser.ErrorTimes >= 3)
                                {
                                    frontUser.LockTime = DateTime.Now;
                                    account_Log.LockTime = frontUser.LockTime;

                                    frontUser.Status = (int)UserStatusEnum.停權;
                                    account_Log.Status = (int)AccountStatusEnum.停權;

                                    await loginUserData.SaveChanges(frontUser);

                                    account_Log.CreatorUserId = frontUser.Id;
                                    account_Log.CreationTime = DateTime.Now;

                                    db.Account_Logs.Add(account_Log);
                                    db.SaveChanges();

                                    response.Message = "密碼錯誤";
                                    throw new Exception("密碼錯誤次數三次以上，請於15分鐘後再次嘗試。");
                                }
                                await loginUserData.SaveChanges(frontUser);
                                response.Message = "密碼錯誤";
                                throw new Exception("舊有密碼輸入錯誤，請重新輸入。");
                            }
                        }
                    }

                    if (frontUser != null)
                    {
                        frontUser.Password = passwordHasher.HashPassword(dto.Password);
                        frontUser.LastModifierUserId = frontUser.Id;
                        frontUser.LastModificationTime = DateTime.Now;
                        frontUser.ForgetID = null;
                        frontUser.ForgeIDSendDate = null;
                        frontUser.ErrorTimes = 0;
                        frontUser.LockTime = null;
                        frontUser.Status = (int)UserStatusEnum.開通;
                        await loginUserData.SaveChanges(frontUser);

                        Account_Log account_Log = new Account_Log()
                        {
                            UUID = UUID,
                            WebsiteId = dto.WebsiteId,
                            Status = (int)AccountStatusEnum.密碼重置,
                            CreatorUserId = frontUser.Id,
                            CreationTime = DateTime.Now,
                        };
                        db.Account_Logs.Add(account_Log);
                        db.SaveChanges();

                        response.Success = true;
                    }
                    else throw new Exception("會員不存在");
                }
                else throw new Exception("Token錯誤");
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
        private async Task<LoginOutputDto> NoPasswordLogin(FrontUser frontuser, long WebsiteId, FrontLoginInputDto? dto)
        {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            try
            {
                var tokenItem = await tokenAppService.CreateToken();
                Guid Temp_UUID = await tokenAppService.GetUUID();
                Account_Log account_Log = new Account_Log();

                DateTime dateTime = DateTime.Now;
                DateTime EndDateTime = dateTime.AddMinutes(30);

                var token = await db.Tokens.Where(e => e.UUID == Temp_UUID && e.id == tokenItem.RefreshToken && e.websiteId == WebsiteId).FirstOrDefaultAsync();
                if (token != null)
                {
                    token.UUID = frontuser.UUID;
                    token.UserID = frontuser.Id;
                    if (frontuser != null && !string.IsNullOrEmpty(frontuser.Email))
                    {
                        output.Token = await tokenAppService.CreateToken(frontuser.Email, token.id, 15);
                    }
                }
                db.SaveChanges();
                output.Secret = token.id;
                output.EndDateTime = EndDateTime;

                if (dto != null) dto.Password = "******";
                await loginUserData.SetLogs("Account", "Login", frontuser.Id, WebsiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));

                account_Log = new Account_Log()
                {
                    UUID = frontuser.UUID,
                    WebsiteId = WebsiteId,
                    Status = (int)AccountStatusEnum.登入,
                    LastLoginTime = DateTime.Now,
                    CreatorUserId = frontuser.Id,
                    CreationTime = DateTime.Now,
                };
                db.Account_Logs.Add(account_Log);
                db.SaveChanges();

                if (frontuser.UUID != Temp_UUID)
                {
                    MappingOldNewUUID mapoldnew = new MappingOldNewUUID
                    {
                        TempUUID = Temp_UUID,
                        UserUUID = frontuser.UUID
                    };
                    db.MappingOldNewUUID.Add(mapoldnew);
                    await loginUserData.SaveChanges(mapoldnew);
                }

                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }

            return output;
        }
    }
}