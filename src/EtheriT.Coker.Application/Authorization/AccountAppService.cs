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
