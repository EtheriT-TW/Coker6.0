using AutoMapper;
using DevExpress.CodeParser;
using DevExpress.Internal;
using DevExpress.XtraRichEdit.Import.Html;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Application.Shared.Dto.User;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Data;
using System.IO.Pipelines;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EtheriT.Coker.Application.Authorization
{
    public class AccountAppService
    {
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly string controllerName;
        private readonly MailAppService mailAppService;
        private readonly IMailTemplateAppService _mailTemplateAppService;
        private readonly INewsletterAppService newsletterAppService;
        private readonly IBonusManagementAppService bonusManagementAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IConfiguration configuration;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IHostEnvironment _env;
        public AccountAppService(
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            MailAppService mailAppService,
            StringHandler stringHandler,
            INewsletterAppService newsletterAppService,
            IBonusManagementAppService bonusManagementAppService,
            IFileUploadAppService fileUploadAppService,
            IConfiguration configuration,
            IShoppingCartAppService shoppingCartAppService,
            ICookieManagerAppService cookieManager,
            IMailTemplateAppService mailTemplateAppService,
            IHostEnvironment env
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
            this.fileUploadAppService = fileUploadAppService;
            this.configuration = configuration;
            this.shoppingCartAppService = shoppingCartAppService;
            this.stringHandler = stringHandler;
            this._env = env;
            this.cookieManager = cookieManager;
            this.bonusManagementAppService = bonusManagementAppService;
            _mailTemplateAppService = mailTemplateAppService;
            controllerName = "Account";
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
                                Name = w.Title,
                                DefaultUrl = w.DefaultUrl ?? string.Empty,
                            };

                    output.Account = theUser.Account;
                    output.UserName = theUser.Name;
                    var profileImg = await fileUploadAppService.getImgFiles(new Shared.Dto.Files.FileGetImgInputDto
                    {
                        Sid = theUser.Id,
                        Size = 3,
                        Type = (int)FileBindTypeEnum.大頭貼
                    });
                    if (profileImg.Any())
                    {
                        output.ProfileImage = profileImg.FirstOrDefault()?.Link ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(output.ProfileImage)) output.ProfileImage = "/images/user.png";
                    output.Webs = await o.ToListAsync();
                }
            }
            catch
            {
                output.Account = "";
            }
            return output;
        }

        private void ClearBackstageCookies()
        {
            cookieManager.Delete("BackstageToken");
            cookieManager.Delete("BackstageRefreshToken");
            cookieManager.Delete(".Coker6.Back.Auth");
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
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
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
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
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
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                long WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");

                var frontUser = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                       where user.UUID == UUID && mapuserweb.FK_WebsiteId == WebsiteID
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
        public async Task<ResponseUserEditDto> GetFrontUserData()
        {
            ResponseUserEditDto UserData = new ResponseUserEditDto();
            try
            {
                var websiteid = configuration.GetValue<long>("WebConfig:SiteId");
                Guid UUID = await tokenAppService.GetUUID();
                var token = await tokenAppService.CheckToken(null);

                if (token != null && token.IsLogin)
                {
                    var userdata = await (from user in db.FrontUsers
                                          join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                          where user.UUID == UUID && mapuserweb.FK_WebsiteId == websiteid
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

        public async Task<string> GetFrontUserLevelName()
        {
            try
            {
                var websiteid = configuration.GetValue<long>("WebConfig:SiteId");
                Guid UUID = await tokenAppService.GetUUID();
                var token = await tokenAppService.CheckToken(null);

                if (token == null || !token.IsLogin) return "";

                var levelName = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite
                                           on user.Id equals mapuserweb.FK_UserId
                                       join role in db.Roles
                                           on user.Level equals role.Id
                                       where user.UUID == UUID
                                          && mapuserweb.FK_WebsiteId == websiteid
                                       select role.Name).FirstOrDefaultAsync();

                return levelName ?? "";
            }
            catch
            {
                return "";
            }
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
                            throw new Exception(L.get("ActivationLinkExpiredResend"));
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
                    else if (frontUser.Status == (int)UserStatusEnum.開通) throw new Exception(L.get("AccountActivated"));
                }
                else throw new Exception(L.get("LinkExpired"));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }
        public async Task<ResponseMessageDto> SendForget(long userId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();

                var frontUser = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                       where user.Id == userId && mapuserweb.FK_WebsiteId == websiteId
                                       select user).FirstOrDefaultAsync();
                if (frontUser != null)
                {

                    response = await SendForget(new SendForgetDto
                    {
                        WebsiteId = websiteId,
                        WebsiteLink = await loginUserData.GetWebsiteUrl(),
                        WebsiteName = await loginUserData.GetWebsiteName(),
                        Email = frontUser.Email
                    });
                }
                else throw new Exception();
            }
            catch
            {
                response.Error = "會員資料錯誤";

            }
            return response;

        }
        public async Task<ResponseMessageDto> SendForget(SendForgetDto dto)
        {

            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (string.IsNullOrEmpty(dto.Email)) throw new Exception("請輸入會員信箱");
                var website = await db.Websites.Where(e => e.Id == dto.WebsiteId).FirstOrDefaultAsync();
                if (website == null) throw new Exception("網站資料錯誤");
                var frontUser = await (from user in db.FrontUsers
                                       join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                       where user.Email == dto.Email && mapuserweb.FK_WebsiteId == dto.WebsiteId
                                       select user).FirstOrDefaultAsync();

                if (frontUser != null && !string.IsNullOrEmpty(frontUser.Email))
                {
                    frontUser.ForgetID = Guid.NewGuid();
                    frontUser.ForgeIDSendDate = DateTime.Now;
                    frontUser.LastModificationTime = DateTime.Now;
                    await loginUserData.SaveChanges(frontUser);

                    ForgetTemplateResultDto resultDto = new ForgetTemplateResultDto
                    {
                        Email = dto.Email,
                        WebsiteLink = dto.WebsiteLink,
                        ForgetID = frontUser.ForgetID.Value,
                        ForgeIDSendDate = frontUser.ForgeIDSendDate.Value.AddDays(1)
                    };

                    var mailTemp = await _mailTemplateAppService.GetTemplateRenderAsync(MailTemplateTypeEnum.密碼重設通知, new List<MailTemplateInputDto> { new MailTemplateInputDto {
                        Key = frontUser.ForgetID.ToString(),
                        Model = resultDto
                    } });

                    if (mailTemp?.Any() == true)
                    {
                        var content = mailTemp.First();

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
                            Body = content?.Body ?? string.Empty,
                            Css = content?.Style ?? string.Empty,
                        }, website.Contact);
                    }

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

                if (frontUser != null && frontUser.ForgeIDSendDate != null && frontUser.ForgeIDSendDate.Value.Date.AddDays(1).CompareTo(DateTime.Now) > 0)
                {
                    response.Success = true;
                }
                else throw new Exception(L.get("LinkExpired"));
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
                            if (frontUser.Status == (int)UserStatusEnum.鎖定 && frontUser.LockTime != null && ((DateTime)frontUser.LockTime).AddMinutes(15).CompareTo(DateTime.Now) > 0)
                            {
                                throw new Exception(L.get("AccountLocked", ((DateTime)frontUser.LockTime).AddMinutes(15)));
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

                                    frontUser.Status = (int)UserStatusEnum.鎖定;
                                    account_Log.Status = (int)AccountStatusEnum.鎖定;

                                    await loginUserData.SaveChanges(frontUser);

                                    account_Log.CreatorUserId = frontUser.Id;
                                    account_Log.CreationTime = DateTime.Now;

                                    db.Account_Logs.Add(account_Log);
                                    db.SaveChanges();

                                    response.Message = L.get("PasswordIncorrect");
                                    throw new Exception(L.get("PasswordErrorTooMany"));
                                }
                                await loginUserData.SaveChanges(frontUser);
                                response.Message = L.get("PasswordIncorrect");
                                throw new Exception(L.get("OldPasswordIncorrect"));
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
        public async Task<ResponseMessageDto> EmailChage(EmailChangeDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();

            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                long WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");

                var frontuser = await db.FrontUsers.Where(e => e.UUID == UUID).FirstOrDefaultAsync();
                var Website = await db.Websites.Where(e => e.Id == WebsiteID).FirstOrDefaultAsync();
                if (frontuser != null && Website != null)
                {
                    if (passwordHasher.VerifyHashedPassword(frontuser.Password, dto.Password))
                    {
                        var other_frontuser = await (from user in db.FrontUsers
                                                     join mapuserweb in db.MappingFrontUserAndWebsite on user.Id equals mapuserweb.FK_UserId
                                                     where user.Email == dto.Email
                                                     where mapuserweb.FK_WebsiteId == WebsiteID
                                                     select user.Id).ToListAsync();

                        if (other_frontuser != null && other_frontuser.Count > 0)
                        {
                            response.Error = L.get("EmailAlreadyExistsTitle");
                            response.Message = L.get("EmailAlreadyUsed");
                        }
                        else
                        {
                            var account_Log = new Account_Log()
                            {
                                UUID = frontuser.UUID,
                                WebsiteId = WebsiteID,
                                CreatorUserId = frontuser.Id,
                                CreationTime = DateTime.Now,
                                Status = (int)AccountStatusEnum.Email重置,
                            };

                            var hidden_mail = dto.Email.Substring(0, 1) + "******" + dto.Email.Substring(dto.Email.IndexOf('@') - 1, 1) + dto.Email.Substring(dto.Email.IndexOf('@'));

                            ChangeEmailMailTemplateDto resultDto = new ChangeEmailMailTemplateDto
                            {
                                Email = hidden_mail,
                                CreationTime = account_Log.CreationTime,
                                Name = frontuser.Name,
                                Title = Website.Title,
                                Url = $"{Website.DefaultUrl}/{Website.OrgName}/Member"
                            };

                            var mailTemp = await _mailTemplateAppService.GetTemplateRenderAsync(MailTemplateTypeEnum.變更電子信箱, new List<MailTemplateInputDto> { new MailTemplateInputDto {
                                Key = frontuser.UUID.ToString(),
                                Model = resultDto
                            } });


                            if (mailTemp?.Any() == true)
                            {
                                var content = mailTemp.First();

                                var sedResult = await mailAppService.sendMail(new SenderDto
                                {
                                    Recipients = new List<MailUserDataDto>(){
                                    new MailUserDataDto()
                                    {
                                        Name = frontuser.Name,
                                        Email = frontuser.Email,
                                    }
                                },
                                    Subject = $"【{Website.Title}】{L.get("MailNotifyEmailChanged")}",
                                    Body = content.Body ?? string.Empty,
                                    Css = content.Style ?? string.Empty,
                                }, Website.Contact);

                                response.Success = sedResult.Success;
                                response.Message = sedResult.Message;
                                response.Error = sedResult.Error;
                            }

                            if (response.Success)
                            {
                                frontuser.Email = dto.Email;
                                var user = await db.Users.Where(e => e.Email == frontuser.Email).FirstOrDefaultAsync();
                                if (user == null)
                                {
                                    user = mapper.Map<User>(frontuser);
                                    user.Id = 0;
                                    user.Password = frontuser.Password;
                                    db.Users.Add(user);
                                    await loginUserData.SaveChanges(user);
                                }
                                frontuser.FK_User = user.Id;
                                db.Account_Logs.Add(account_Log);
                                await loginUserData.SaveChanges(frontuser);
                            }
                        }
                    }
                    else
                    {
                        frontuser.ErrorTimes += 1;
                        var account_Log = new Account_Log()
                        {
                            UUID = frontuser.UUID,
                            WebsiteId = WebsiteID,
                            ErrorTimes = frontuser.ErrorTimes
                        };
                        if (frontuser.ErrorTimes >= 3)
                        {
                            frontuser.LockTime = DateTime.Now;
                            account_Log.LockTime = frontuser.LockTime;

                            frontuser.Status = (int)UserStatusEnum.鎖定;
                            account_Log.Status = (int)AccountStatusEnum.鎖定;

                            await loginUserData.SaveChanges(frontuser);

                            account_Log.CreatorUserId = frontuser.Id;
                            account_Log.CreationTime = DateTime.Now;

                            db.Account_Logs.Add(account_Log);
                            db.SaveChanges();

                            response.Error = L.get("PasswordErrorThreeTimesTitle");
                            response.Message = L.get("PasswordErrorTooMany");
                        }
                        else
                        {
                            account_Log.Status = (int)AccountStatusEnum.登入失敗;

                            account_Log.CreatorUserId = frontuser.Id;
                            account_Log.CreationTime = DateTime.Now;
                            db.Account_Logs.Add(account_Log);
                            db.SaveChanges();

                            response.Error = L.get("PasswordIncorrect");
                            response.Message = L.get("PasswordIncorrectRetry");
                        }
                    }
                }
                else if (frontuser == null) throw new Exception(L.get("MemberNotFound"));
                else if (Website == null) throw new Exception(L.get("WebsiteDataError"));
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
                字串長度在 8 ~ 32 個字母之間
                Regex regex = new Regex(@"^(?=.*\d)(?=.*[a-zA-Z])(?=.*\W).{8,32}$");
             */
            try
            {
                //密碼長度須為8-32之間
                if (password.Length < 8 || password.Length > 32) throw new Exception(L.get("PasswordRuleLength"));
                //密碼有數字
                Regex regex1 = new Regex(@"^(?=.*\d).{8,32}$");
                if (regex1.IsMatch(password)) matchCount++;
                //密碼有英文小寫
                Regex regex2 = new Regex(@"^(?=.*[a-z]).{8,32}$");
                if (regex2.IsMatch(password)) matchCount++;
                //密碼有英文大寫
                Regex regex3 = new Regex(@"^(?=.*[A-Z]).{8,32}$");
                if (regex3.IsMatch(password)) matchCount++;
                //密碼有符號
                Regex regex4 = new Regex(@"^(?=.*\W).{8,32}$");
                if (regex4.IsMatch(password)) matchCount++;
                if (matchCount < 3) throw new Exception(L.get("PasswordRuleComposition"));
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
                DateTime TokenEndDateTime = dateTime.AddMinutes(15);
                DateTime EndDateTime = dateTime.AddMinutes(30);

                var token = await db.Tokens
                    .Where(e => e.id == tokenItem.RefreshToken && e.websiteId == WebsiteId)
                    .FirstOrDefaultAsync();

                if (token == null)
                {
                    throw new InvalidOperationException("登入狀態與目前網站不符，請重新登入");
                }

                if (frontuser.UUID == Guid.Empty)
                {
                    var other_user = await db.FrontUsers.Where(e => e.UUID == token.UUID).FirstOrDefaultAsync();
                    if (other_user != null)
                    {
                        frontuser.UUID = Guid.NewGuid();
                    }
                    else frontuser.UUID = token.UUID;
                }

                token.UUID = frontuser.UUID;
                token.UserID = frontuser.FK_User;
                if (!string.IsNullOrEmpty(frontuser.Email))
                {
                    output.Token = await tokenAppService.CreateToken(frontuser.Email, token.id, CookiePurposeEnum.FrontAuthToken);
                }
                db.SaveChanges();
                output.Secret = token.id;
                output.EndDateTime = EndDateTime;

                if (dto != null) dto.Password = "******";
                await loginUserData.SetLogs(frontuser.Id, WebsiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));

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

                if (frontuser.UUID != Temp_UUID && Temp_UUID != Guid.Empty && frontuser.UUID != Guid.Empty)
                {
                    var other_mapping = await db.MappingOldNewUUID.Where(e => e.UserUUID == Temp_UUID || e.TempUUID == Temp_UUID).FirstOrDefaultAsync();
                    if (other_mapping == null)
                    {
                        MappingOldNewUUID mapoldnew = new MappingOldNewUUID
                        {
                            TempUUID = Temp_UUID,
                            UserUUID = frontuser.UUID
                        };
                        db.MappingOldNewUUID.Add(mapoldnew);
                        await loginUserData.SaveChanges(mapoldnew);
                        await shoppingCartAppService.UpdateUUID(frontuser.UUID, Temp_UUID);
                    }
                }

                output.Success = true;
                if (dto != null)
                {
                    cookieManager.Set("RememberMe", dto.Remember ? "1" : "0", CookiePurposeEnum.RefreshIdentifier);
                    if (!dto.Remember)
                    {
                        if (output.Token != null)
                        {
                            cookieManager.Set(
                                "Token",
                                output.Token,
                                dto.Remember ? CookiePurposeEnum.FrontAuthToken : CookiePurposeEnum.none
                            );
                        }
                        if (output.Secret != null)
                        {
                            cookieManager.Set(
                                "RefreshToken",
                                output.Secret.Value.ToString(),
                                dto.Remember ? CookiePurposeEnum.RefreshToken : CookiePurposeEnum.none
                            );
                        }
                        cookieManager.Delete("RememberMe");
                    }
                }
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }

            return output;
        }
    }
}
