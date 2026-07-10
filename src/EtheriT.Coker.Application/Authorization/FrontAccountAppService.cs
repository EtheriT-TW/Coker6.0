using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
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
        private readonly FrontRegistrationService registrationService;
        private readonly CokerDbContext db;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IConfiguration configuration;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly IHostEnvironment env;
        private readonly IMapper mapper;
        private readonly MailAppService mailAppService;
        private readonly IMailTemplateAppService mailTemplateAppService;

        public FrontAccountAppService(
            AccountAppService core,
            FrontRegistrationService registrationService,
            CokerDbContext db,
            ICookieManagerAppService cookieManager,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IShoppingCartAppService shoppingCartAppService,
            IHostEnvironment env,
            IMapper mapper,
            MailAppService mailAppService,
            IMailTemplateAppService mailTemplateAppService)
        {
            this.core = core;
            this.registrationService = registrationService;
            this.db = db;
            this.cookieManager = cookieManager;
            this.configuration = configuration;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.stringHandler = stringHandler;
            this.shoppingCartAppService = shoppingCartAppService;
            this.env = env;
            this.mapper = mapper;
            this.mailAppService = mailAppService;
            this.mailTemplateAppService = mailTemplateAppService;
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
                await registrationService.AddFrontUser(new FrontAddUserDto
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
        public Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto) =>
            registrationService.AddFrontUser(dto);
        public async Task<ResponseMessageDto> AccountOpening(Guid openId)
        {
            var response = new ResponseMessageDto();
            try
            {
                var frontUser = await db.FrontUsers.FirstOrDefaultAsync(e => e.OpenID == openId);
                if (frontUser == null)
                    throw new Exception(L.get("LinkExpired"));

                if (frontUser.Status == (int)UserStatusEnum.開通)
                    throw new Exception(L.get("AccountActivated"));

                if (frontUser.Status != (int)UserStatusEnum.未開通)
                    throw new Exception(L.get("LinkExpired"));

                if (frontUser.OpenIDSendDate.AddDays(1) < DateTime.Now)
                {
                    response.Message = "ReSendOrNot";
                    throw new Exception(L.get("ActivationLinkExpiredResend"));
                }

                var websiteId = await db.MappingFrontUserAndWebsite
                    .Where(e => e.FK_UserId == frontUser.Id)
                    .Select(e => e.FK_WebsiteId)
                    .FirstOrDefaultAsync();
                if (websiteId == 0)
                    throw new Exception(L.get("LinkExpired"));

                frontUser.Status = (int)UserStatusEnum.開通;
                frontUser.OpenDate = DateTime.Now;
                await loginUserData.SaveChanges(frontUser);

                var loginResult = await NoPasswordLogin(frontUser, websiteId, null);
                if (!loginResult.Success)
                    throw new Exception(loginResult.Error);

                response.Success = true;
            }
            catch (Exception e)
            {
                response.Error = e.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var frontUser = await (
                    from user in db.FrontUsers
                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                    where (dto.OpenId == null ? user.Email == dto.Email : user.OpenID == dto.OpenId)
                       && map.FK_WebsiteId == dto.WebsiteId
                    select user).FirstOrDefaultAsync();
                if (frontUser == null)
                    throw new Exception("發生未知錯誤");

                frontUser.OpenID = Guid.NewGuid();
                frontUser.OpenIDSendDate = DateTime.Now;
                await loginUserData.SaveChanges(frontUser);

                dto.Email = frontUser.Email;
                dto.Name = frontUser.Name;
                dto.OpenId = frontUser.OpenID;
                dto.OpenIdSendDate = frontUser.OpenIDSendDate;
                return await registrationService.SendOpening(dto);
            }
            catch (Exception e)
            {
                response.Error = e.Message;
                return response;
            }
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


        public Task<ResponseMessageDto> SendForget(SendForgetDto dto) => core.SendForget(dto);
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
                var tokenCheck = await tokenAppService.CheckToken(null);
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
                        await ClearFrontLoginState(tokenCheck.RefreshToken, dto.WebsiteId);
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
                var tokenCheck = await tokenAppService.CheckToken(null);
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

                            var mailTemp = await mailTemplateAppService.GetTemplateRenderAsync(MailTemplateTypeEnum.變更電子信箱, new List<MailTemplateInputDto> { new MailTemplateInputDto {
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

                                await ClearFrontLoginState(tokenCheck.RefreshToken, WebsiteID);
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

        private async Task ClearFrontLoginState(Guid? refreshTokenId, long websiteId)
        {
            if (refreshTokenId.HasValue)
            {
                var token = await db.Tokens.FirstOrDefaultAsync(e =>
                    e.id == refreshTokenId.Value &&
                    e.websiteId == websiteId);
                if (token != null)
                {
                    token.UserID = null;
                    await db.SaveChangesAsync();
                }
            }

            ClearFrontCookies(websiteId);
        }
    }
}
