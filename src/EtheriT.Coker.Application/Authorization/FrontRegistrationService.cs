using AutoMapper;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Dto.User;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Authorization
{
    public sealed class FrontRegistrationService
    {
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly MailAppService mailAppService;
        private readonly IMailTemplateAppService mailTemplateAppService;
        private readonly IBonusManagementAppService bonusManagementAppService;

        public FrontRegistrationService(
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IMapper mapper,
            MailAppService mailAppService,
            IMailTemplateAppService mailTemplateAppService,
            IBonusManagementAppService bonusManagementAppService)
        {
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.mailAppService = mailAppService;
            this.mailTemplateAppService = mailTemplateAppService;
            this.bonusManagementAppService = bonusManagementAppService;
        }

        public async Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var uuid = await tokenAppService.GetUUID();
                var websiteId = dto.WebsiteId == 0 ? await loginUserData.GetWebsiteId() : dto.WebsiteId;
                long userId = 0;

                var frontUser = await (
                    from user in db.FrontUsers
                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                    where user.Email == dto.Email && map.FK_WebsiteId == websiteId
                    select user).FirstOrDefaultAsync();
                var role = await db.Roles
                    .Where(e =>
                        e.FK_WebsiteId == websiteId &&
                        e.Type == RoleTypeEnum.前台 &&
                        !e.IsDeleted)
                    .OrderBy(e => e.Ser_No)
                    .ThenBy(e => e.Id)
                    .FirstOrDefaultAsync();
                if (role == null) throw new Exception("尚未設定可用的前台會員角色");

                var passwordError = CheckPassword(dto.Password);
                if (dto.Password != dto.PasswordConfirm) throw new Exception("輸入的密碼不相符");
                if (!string.IsNullOrEmpty(passwordError)) throw new Exception(passwordError);

                if (frontUser == null)
                {
                    frontUser = mapper.Map<FrontUser>(dto);
                    frontUser.Password = passwordHasher.HashPassword(dto.Password);
                    frontUser.UUID = Guid.NewGuid();
                    frontUser.Status = (int)UserStatusEnum.未開通;
                    frontUser.OpenID = Guid.NewGuid();
                    frontUser.OpenIDSendDate = DateTime.Now;

                    var user = await db.Users.FirstOrDefaultAsync(e => e.Email == frontUser.Email);
                    var newUser = new User();
                    if (user == null)
                    {
                        user = mapper.Map<User>(dto);
                        user.Password = frontUser.Password;
                        db.Users.Add(user);
                        await loginUserData.SaveChanges(user);
                        newUser = user;
                    }

                    frontUser.FK_User = user.Id;
                    // Level 僅保留舊版相容；實際角色以 MappingUserAndRoles 為準。
                    frontUser.Level = role.Id;
                    db.FrontUsers.Add(frontUser);
                    await loginUserData.SaveChanges(frontUser);
                    userId = frontUser.Id;

                    var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit(websiteId);
                    var bonusText = string.Empty;
                    if (bonusSetting?.SignupBonusPoints > 0)
                    {
                        await bonusManagementAppService.SaveTransaction(new CreateUserTransactionDto
                        {
                            IsSendMail = false,
                            MemberUUID = new List<Guid> { frontUser.UUID },
                            TransactionPoint = bonusSetting.SignupBonusPoints.Value,
                            TransactionOperation = "+",
                            TransactionReason = "加入會員贈送",
                            Type = BonusLogTypeEnum.Earn
                        });
                        bonusText = $"歡迎加入會員！我們已為您準備加入會員紅利 {bonusSetting.SignupBonusPoints.Value} 點，立即前往會員中心查看。";
                    }

                    var userRole = new MappingUserAndRole
                    {
                        UserId = user.Id,
                        UUID = frontUser.UUID,
                        RoleId = role.Id
                    };
                    db.MappingUserAndRoles.Add(userRole);
                    await loginUserData.SaveChanges(userRole);

                    var userWebsite = new MappingFrontUserAndWebsite
                    {
                        FK_UserId = frontUser.Id,
                        FK_WebsiteId = websiteId
                    };
                    db.MappingFrontUserAndWebsite.Add(userWebsite);
                    await loginUserData.SaveChanges(userWebsite);

                    var accountLog = new Account_Log
                    {
                        UUID = uuid,
                        WebsiteId = websiteId,
                        Status = (int)AccountStatusEnum.註冊,
                        CreatorUserId = frontUser.Id,
                        CreationTime = DateTime.Now
                    };
                    db.Account_Logs.Add(accountLog);
                    await db.SaveChangesAsync();

                    var sendDto = mapper.Map<SendOpeningDto>(dto);
                    sendDto.OpenId = frontUser.OpenID;
                    sendDto.OpenIdSendDate = frontUser.OpenIDSendDate;
                    sendDto.BonusText = bonusText;
                    if (dto.SendWelcomeMail)
                    {
                        response = await SendOpening(sendDto);
                        if (!response.Success)
                        {
                            userWebsite.IsDeleted = true;
                            userWebsite.DeletionTime = DateTime.Now;
                            userRole.IsDeleted = true;
                            userRole.DeletionTime = DateTime.Now;
                            newUser.IsDeleted = true;
                            newUser.DeletionTime = DateTime.Now;
                            frontUser.IsDeleted = true;
                            frontUser.DeletionTime = DateTime.Now;
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        response.Success = true;
                    }

                    if (dto.SendActivationMail)
                        await SendActivationMail(sendDto);
                }
                else if (frontUser.Status == (int)UserStatusEnum.未開通)
                {
                    response.Message = "重新寄送通知信";
                    throw new Exception(frontUser.OpenIDSendDate.AddDays(1) < DateTime.Now
                        ? "郵箱已存在且已過開通期限，是否重新寄送通知信？"
                        : "郵箱已存在但尚未開通，請至郵箱確認或重新寄送通知信。");
                }
                else if (frontUser.Status == (int)UserStatusEnum.開通)
                {
                    response.Message = "郵箱已存在";
                    throw new Exception("郵箱已存在，請更換一個郵箱或直接登入。");
                }

                dto.Password = "*********";
                dto.PasswordConfirm = "*********";
                await loginUserData.SetLogs(userId, websiteId, JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }

        public async Task<ResponseMessageDto> SendOpening(SendOpeningDto dto)
        {
            var website = await db.Websites.FirstOrDefaultAsync(e => e.Id == dto.WebsiteId);
            var response = new ResponseMessageDto();
            if (website == null)
            {
                response.Error = "網站資料錯誤";
                return response;
            }
            if (string.IsNullOrEmpty(dto.Email))
            {
                response.Error = "信箱錯誤";
                return response;
            }

            var websiteName = string.IsNullOrEmpty(dto.WebsiteName) ? website.Title : dto.WebsiteName;
            var websiteLink = string.IsNullOrEmpty(dto.WebsiteLink) ? website.DefaultUrl : dto.WebsiteLink;
            try
            {
                var resultDto = new AccountActivationResultDto
                {
                    WebsiteName = websiteName ?? "網站客服信",
                    Email = dto.Email,
                    Link = $"{websiteLink}/?useraction=accountoping&openid={dto.OpenId}",
                    BonusText = dto.BonusText
                };
                if (dto.OpenIdSendDate != null)
                    resultDto.ExpiresAt = dto.OpenIdSendDate.Value.AddDays(1);

                var templates = await mailTemplateAppService.GetTemplateRenderAsync(
                    MailTemplateTypeEnum.註冊驗證通知,
                    new List<MailTemplateInputDto>
                    {
                        new() { Key = Guid.NewGuid().ToString(), Model = resultDto }
                    });
                if (templates?.Any() == true)
                {
                    var content = templates.First();
                    var sendResult = await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>
                        {
                            new() { Name = dto.Name, Email = dto.Email }
                        },
                        Subject = $"【{websiteName}】註冊會員通知",
                        Body = content.Body ?? string.Empty,
                        Css = content.Style ?? string.Empty
                    }, website.Contact);
                    response.Success = sendResult.Success;
                    response.Message = sendResult.Message;
                    response.Error = sendResult.Error;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }

        private async Task SendActivationMail(SendOpeningDto dto)
        {
            var website = await db.Websites.FirstOrDefaultAsync(e => e.Id == dto.WebsiteId);
            if (website == null) return;

            var frontUser = await (
                from user in db.FrontUsers
                join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                where user.Email == dto.Email && map.FK_WebsiteId == dto.WebsiteId
                select user).FirstOrDefaultAsync();
            if (frontUser?.Email == null) return;

            var resultDto = new AccountCreatedNoticeResultDto
            {
                WebsiteName = string.IsNullOrEmpty(dto.WebsiteName) ? website.Title : dto.WebsiteName,
                CreatedAt = frontUser.CreationTime,
                Email = frontUser.Email,
                PolicyUrl = $"{website.DefaultUrl}#PrivacyStatement",
                BonusText = dto.BonusText
            };
            var templates = await mailTemplateAppService.GetTemplateRenderAsync(
                MailTemplateTypeEnum.註冊完成通知,
                new List<MailTemplateInputDto>
                {
                    new() { Key = Guid.NewGuid().ToString(), Model = resultDto }
                });
            if (templates?.Any() != true) return;

            var content = templates.First();
            await mailAppService.sendMail(new SenderDto
            {
                Recipients = new List<MailUserDataDto>
                {
                    new() { Name = dto.Name, Email = dto.Email }
                },
                Subject = $"【{resultDto.WebsiteName}】註冊會員通知",
                Body = content.Body ?? string.Empty,
                Css = content.Style ?? string.Empty
            }, website.Contact);
        }

        private static string CheckPassword(string password)
        {
            try
            {
                if (password.Length < 8 || password.Length > 32)
                    throw new Exception(L.get("PasswordRuleLength"));

                var matchCount = 0;
                if (Regex.IsMatch(password, @"^(?=.*\d).{8,32}$")) matchCount++;
                if (Regex.IsMatch(password, @"^(?=.*[a-z]).{8,32}$")) matchCount++;
                if (Regex.IsMatch(password, @"^(?=.*[A-Z]).{8,32}$")) matchCount++;
                if (Regex.IsMatch(password, @"^(?=.*\W).{8,32}$")) matchCount++;
                if (matchCount < 3) throw new Exception(L.get("PasswordRuleComposition"));
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
