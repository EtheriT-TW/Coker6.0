using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EtheriT.Coker.Application.Member
{
    public class MemberAppService : IMemberAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IMapper mapper;
        private readonly IBonusManagementAppService _bonusManagementAppService;
        private readonly IMailTemplateAppService _mailTemplateAppService;
        private readonly MailAppService _mailAppService;
        private readonly StringHandler _stringHandler;
        private readonly IHtmlProcessor htmlProcessor;

        public MemberAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IMapper mapper,
            IBonusManagementAppService bonusManagementAppService,
            IMailTemplateAppService mailTemplateAppService,
            IHtmlProcessor htmlProcessor,
            MailAppService mailAppService,
            StringHandler stringHandler)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.mapper = mapper;
            this.htmlProcessor = htmlProcessor;
            _stringHandler = stringHandler;
            _bonusManagementAppService = bonusManagementAppService;
            _mailTemplateAppService = mailTemplateAppService;
            _mailAppService = mailAppService;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Users;

                if (result != null)
                {
                    var dataQuery = from e in db.Users
                                    where !e.IsDeleted
                                    select new MemberGetAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        CellPhone = e.CellPhone.Substring(0, 3) + "****" + e.CellPhone.Substring(7),
                                        TelPhone = e.TelPhone == "" ? "" : e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6),
                                        Address = (e.Address == null || e.Address == "") ? "" : e.Address.Substring(0, e.Address.LastIndexOf(" ")).Replace(" ", "") + "***",
                                        Email = e.Email.Substring(0, 2) + "***" + e.Email.Substring(e.Email.IndexOf("@") - 1),
                                        Total = e.Total,
                                        Level = e.Level,
                                        CreationTime = e.CreationTime,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MemberGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAllFrontList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();

                var baseQuery =
                    from f in db.FrontUsers
                    join s in db.MappingFrontUserAndWebsite on f.Id equals s.FK_UserId
                    where s.FK_WebsiteId == websiteId
                    select f;

                var defaultRoleId = await db.Roles
                    .Where(r => r.FK_WebsiteId == websiteId && r.Type == RoleTypeEnum.前台)
                    .Select(r => (long?)r.Id)
                    .FirstOrDefaultAsync();

                // 1) 這裡只放「明文」欄位（不要呼叫任何 MaskXXX）
                var dataQuery = baseQuery.Select(f => new MemberGetAllListDto
                {
                    Id = f.Id,
                    UUID = f.UUID,
                    Name = f.Name,
                    CellPhone = f.CellPhone,
                    TelPhone = f.TelPhone,
                    Address = f.Address,
                    Email = f.Email ?? "",
                    Level = f.Level ?? defaultRoleId,
                    CreationTime = f.CreationTime,
                    Total = (
                        from order in db.Order_Headers
                        where order.State == OrderStatusEnum.已完成 &&
                              (
                                  (from m in db.MappingOldNewUUID
                                   where m.UserUUID == f.UUID
                                   select m.TempUUID).Contains(order.FK_UUID)
                                  || order.FK_UUID == f.UUID
                              )
                        select order
                    ).Sum(x => x.Subtotal - x.Discount + x.Freight),
                    Bonus = 0
                });

                // 2) 讓 DataSourceLoader 用明文做 filter/sort/search/paging
                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);

                // 3) 只對「當頁資料」做遮蔽（DI 在這裡用，安全）
                if (output?.data is IEnumerable<MemberGetAllListDto> list)
                {
                    var page = list.ToList();

                    var uuids = page.Select(x => x.UUID).ToList();
                    var userBonus = await _bonusManagementAppService.GetQueryFrontUsersTotalAvaliableBonus(uuids);

                    foreach (var item in page)
                    {
                        item.Bonus = userBonus.FirstOrDefault(x => x.UserUUID == item.UUID)?.TotalAvaliableBonus ?? 0;

                        item.Name = _stringHandler.MaskName(item.Name);
                        item.CellPhone = _stringHandler.MaskCellPhone(item.CellPhone);
                        item.TelPhone = _stringHandler.MaskTelPhone(item.TelPhone);
                        item.Address = _stringHandler.MaskAddress(item.Address);
                        item.Email = _stringHandler.MaskEmail(item.Email);
                    }

                    output.data = page;
                }

                return new JsonResult(output, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception ex)
            {
                // 不用 StatusCode，回 DevExtreme 可解析格式
                var empty = new { data = new List<MemberGetAllListDto>(), totalCount = 0 };
                return new JsonResult(empty, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
        }
        public async Task<JsonResult> GetAllManagerList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var result = db.Users;

                if (result != null)
                {
                    var dataQuery = from e in db.Users.Include(n => n.Roles)
                                    join r in db.MappingUserAndWebsites on e.Id equals r.UserId
                                    where r.WebsiteId == websiteId
                                    select new ManagerAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        TelPhone = e.TelPhone == "" ? "" : e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6),
                                        Email = e.Email.Substring(0, 2) + "***" + e.Email.Substring(e.Email.IndexOf("@") - 1),
                                        Status = (UserStatusEnum)(e.Status ?? 0),
                                        Roles = String.Join("、", (
                                            from o in db.Roles.Where(e => e.FK_WebsiteId == websiteId)
                                            join ur in e.Roles on o.Id equals ur.RoleId
                                            select o.Name
                                        ).ToList())
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MemberGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<MemberGetAllDataDto> GetAllData(long id)
        {
            try
            {
                bool isSystemUser = await loginUserData.isSystemUser();
                long websideId = await loginUserData.GetWebsiteId();
                var result = await (from user in db.Users.Where(e => e.Id == id)
                                    join map in db.MappingUserAndWebsites on user.Id equals map.UserId
                                    where map.WebsiteId == websideId || isSystemUser
                                    select user).FirstOrDefaultAsync();


                if (result != null)
                {
                    MemberGetAllDataDto output = mapper.Map<MemberGetAllDataDto>(result);
                    output.RoleId = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).Select(e => e.RoleId).FirstOrDefaultAsync();
                    output.Id = ("000000000" + result.Id).Substring(result.Id.ToString().Length);
                    return output;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<MemberGetAllDataDto> GetFrontAllData(long id)
        {
            try
            {
                long websideId = await loginUserData.GetWebsiteId();
                var result = await (from user in db.FrontUsers.Where(e => e.Id == id)
                                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                                    where map.FK_WebsiteId == websideId
                                    select user).FirstOrDefaultAsync();

                if (result != null)
                {
                    MemberGetAllDataDto output = mapper.Map<MemberGetAllDataDto>(result);
                    if (!result.UUID.IsNullOrEmpty())
                        output.bonus = (await _bonusManagementAppService.GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { result.UUID })).FirstOrDefault()?.TotalAvaliableBonus ?? 0;
                    output.Tags = db.UserTagStatistics.Include(e => e.Tag).Where(e => e.UUID == result.UUID).OrderByDescending(e => e.Weight).Take(5).Select(e => e.Tag.Title).ToList();
                    output.RoleId = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).Select(e => e.RoleId).FirstOrDefaultAsync();
                    output.Id = ("000000000" + result.Id).Substring(result.Id.ToString().Length);
                    return output;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<MemberGetAllDataDto> GetSelfData()
        {
            long userId = await loginUserData.GetUserId();
            return await GetAllData(userId);
        }
        public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        {
            long usetId = await loginUserData.GetUserId();
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Users.Where(e => e.Id == dto.Id).FirstOrDefault();

                if (result != null)
                {
                    if (dto.Status == null) dto.Status = result.Status;
                    if (dto.TelPhone == null) dto.TelPhone = result.TelPhone;

                    mapper.Map(dto, result);
                    await loginUserData.SaveChanges(result);

                    var role = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).FirstOrDefaultAsync();
                    if (dto.RoleId != null && role != null && role.RoleId != dto.RoleId)
                    {
                        role.RoleId = (long)dto.RoleId;
                        await loginUserData.SaveChanges(role);
                    }

                    output.Success = true;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        private async Task<ResponseMessageDto> FrontAdd(MemberUpdateDto dto)
        {
            long websiteId = await loginUserData.GetWebsiteId();
            var website = await db.Websites.Where(e => e.Id == websiteId).FirstOrDefaultAsync();
            var websiteUrl = website?.DefaultUrl ?? "";
            var output = new ResponseMessageDto() { Success = false };

            try
            {
                var exists = await db.FrontUsers
                    .AnyAsync(e => e.Email == dto.Email && e.Websites.Any(w => w.FK_WebsiteId == websiteId));
                if (exists) throw new Exception("此電子郵件已被使用");

                var bUser = await db.Users.FirstOrDefaultAsync(e => e.Email == dto.Email);

                var newUser = mapper.Map<FrontUser>(dto);
                newUser.UUID = Guid.NewGuid();
                newUser.Password = _stringHandler.RandonCode(RandomStringType.數字加英文大小寫及符號, 32);
                newUser.ForgetID = Guid.NewGuid();
                newUser.ForgeIDSendDate = DateTime.Now;
                newUser.Level = dto.RoleId;
                await loginUserData.setOptionParameter(newUser);
                db.FrontUsers.Add(newUser);

                if (bUser == null)
                {
                    bUser = mapper.Map<User>(dto);
                    bUser.UUID = newUser.UUID;
                    bUser.Password = newUser.Password;
                    await loginUserData.setOptionParameter(bUser);
                    db.Users.Add(bUser);
                }

                // 關聯
                newUser.User = bUser; // 注意：若 bUser 新增，Id 會在 SaveChanges 後才有
                                            // 更好的作法是用 navigation（如果你有）：newUser.User = bUser;

                if (dto.RoleId != null && dto.RoleId != 0)
                {
                    var role = await db.Roles
                        .Where(e => e.FK_WebsiteId == websiteId && e.Type == RoleTypeEnum.前台 && e.Id == dto.RoleId)
                        .FirstOrDefaultAsync();
                    if (role == null) throw new Exception("角色不存在");

                    var roleMaping = new MappingUserAndRole
                    {
                        User = bUser,
                        RoleId = dto.RoleId.Value,
                        UUID = newUser.UUID
                    };
                    db.MappingUserAndRoles.Add(roleMaping);
                    await loginUserData.setOptionParameter(roleMaping);
                }

                var mapping = new MappingFrontUserAndWebsite
                {
                    User = newUser,
                    FK_WebsiteId = websiteId
                };

                db.MappingFrontUserAndWebsite.Add(mapping);
                await loginUserData.setOptionParameter(mapping);
                await db.SaveChangesAsync();

                await SendFrontUserCreateNoticeMailAsync(newUser,website!);
                output.Success = true;
                return output;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
                return output;
            }
        }

        public async Task<ResponseMessageDto> FrontAddUpdate(MemberUpdateDto dto)
        {
            long usetId = await loginUserData.GetUserId();
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0) return await FrontAdd(dto);
                var result = db.FrontUsers.Where(e => e.Id == dto.Id).FirstOrDefault();

                if (result != null)
                {
                    if (dto.Status == null) dto.Status = result.Status;
                    if (dto.TelPhone == null) dto.TelPhone = result.TelPhone;
                    mapper.Map(dto, result);
                    await loginUserData.SaveChanges(result);

                    var role = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).FirstOrDefaultAsync();
                    if (dto.RoleId != null && role != null && role.RoleId != dto.RoleId)
                    {
                        role.RoleId = (long)dto.RoleId;
                        result.Level = role.RoleId;
                        await loginUserData.SaveChanges(role);
                    }

                    output.Success = true;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> ResendFrontUserCreateNoticeMailAsync(long frontUserId)
        {
            var output = new ResponseMessageDto { Success = false };

            try
            {
                var (frontUser, website) = await GetFrontUserAndWebsiteAsync(frontUserId);

                if (string.IsNullOrWhiteSpace(frontUser.Email))
                    throw new Exception("會員未設定電子郵件，無法寄送通知信");

                // 重置設定密碼連結效期（避免過期）
                frontUser.ForgetID = Guid.NewGuid();
                frontUser.ForgeIDSendDate = DateTime.Now;

                await loginUserData.setOptionParameter(frontUser);
                await db.SaveChangesAsync();

                await SendFrontUserCreateNoticeMailAsync(frontUser, website);

                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                output.Success = false;
                output.Error = ex.Message;
                return output;
            }
        }

        public async Task<List<SelectDto>> GetAllRole()
        {
            List<SelectDto> output = new List<SelectDto>();
            long websideId = await loginUserData.GetWebsiteId();
            try
            {
                output = await (from role in db.Roles
                                where role.FK_WebsiteId == websideId
                                where role.Type == RoleTypeEnum.前台
                                orderby role.Ser_No
                                select new SelectDto()
                                {
                                    Id = role.Id,
                                    Ser_No = role.Ser_No,
                                    Name = role.Name
                                }).ToListAsync();
            }
            catch (Exception e)
            {

            }
            return output;
        }
        public async Task<JsonResult> GetDevAllRole(DataSourceLoadOptions loadOptions)
        {
            var dataQuery = await GetAllRole();
            var output = DataSourceLoader.Load(dataQuery, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> RoleAddUp([FromForm] DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                var data = JsonConvert.DeserializeObject<AddRoleDto>(dto.Values);
                var websiteId = await loginUserData.GetWebsiteId();
                var id = dto.Key;
                if (data != null)
                {
                    Role? role;
                    if (id == null || id == 0)
                    {
                        if (string.IsNullOrEmpty(data?.Name)) throw new Exception("角色名稱不可為空");
                        role = mapper.Map<Role>(data);
                        role.Type = RoleTypeEnum.前台;
                        role.FK_WebsiteId = websiteId;
                        db.Roles.Add(role);
                    }
                    else
                    {
                        data.Id = id;
                        role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                        if (role != null)
                        {
                            if (string.IsNullOrEmpty(data?.Name)) data.Name = role.Name;
                            mapper.Map(data, role);
                        }
                    }
                    if (role == null) throw new Exception("角色不存在");
                    await loginUserData.SaveChanges(role);
                    output.Success = true;
                    await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
                }
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<ResponseMessageDto> RoleDelete(long id)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                if (role != null)
                {
                    role.IsDeleted = true;
                    await loginUserData.SaveChanges(role);
                    output.Success = true;
                    await loginUserData.SetLogs(JsonConvert.SerializeObject(new { id }), JsonConvert.SerializeObject(output));
                }
                else throw new Exception();
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            return output;
        }
        private async Task<(FrontUser frontUser, Website website)> GetFrontUserAndWebsiteAsync(long frontUserId)
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var website = await db.Websites.FirstOrDefaultAsync(e => e.Id == websiteId);
            if (website == null) throw new Exception("網站不存在");

            var frontUser = await db.FrontUsers
                .Include(e => e.Websites)
                .FirstOrDefaultAsync(e =>
                    e.Id == frontUserId &&
                    !e.IsDeleted &&
                    e.Websites.Any(w => w.FK_WebsiteId == websiteId && !w.IsDeleted));

            if (frontUser == null) throw new Exception("會員不存在或不屬於此網站");

            return (frontUser, website);
        }

        private async Task SendFrontUserCreateNoticeMailAsync(FrontUser frontUser,Website website)
        {
            var model = mapper.Map<BackendTemplateResuleDto>(frontUser);

            await SendFrontUserMailAsync(
                frontUser,
                website,
                MailTemplateTypeEnum.後台會員建置,
                subject: $"【{website.Title}】會員帳號建立通知",
                model: model
            );
        }
        private async Task SendFrontUserMailAsync<TModel>(
            FrontUser frontUser,
            Website website,
            MailTemplateTypeEnum templateType,
            string subject,
            TModel model
        ){
            if (frontUser == null) throw new ArgumentNullException(nameof(frontUser));
            if (website == null) throw new ArgumentNullException(nameof(website));
            if (string.IsNullOrWhiteSpace(frontUser.Email)) return;

            var websiteUrl = website.DefaultUrl ?? string.Empty;

            // 統一補上完整網址（只針對慣例欄位）
            if (!string.IsNullOrWhiteSpace(websiteUrl) &&
                model is BackendTemplateResuleDto dto &&
                !string.IsNullOrWhiteSpace(dto.SetPasswordUrl))
            {
                var baseUri = new Uri(
                    websiteUrl.EndsWith("/") ? websiteUrl : websiteUrl + "/");

                dto.SetPasswordUrl =
                    new Uri(baseUri, dto.SetPasswordUrl).ToString();
            }

            var mailData = new List<MailTemplateInputDto>
            {
                new()
                {
                    Key = frontUser.UUID.ToString(),
                    Model = model!
                }
            };

            var mailTemp = await _mailTemplateAppService
                .GetTemplateRenderAsync(templateType, mailData);

            if (mailTemp?.Any() == true)
            {
                var content = mailTemp.First();

                await _mailAppService.sendMail(new SenderDto
                {
                    Recipients = new List<MailUserDataDto>
                    {
                        new MailUserDataDto
                        {
                            Email = frontUser.Email!,
                            Name = frontUser.Name
                        }
                    },
                    Subject = subject,
                    Body = content?.Body ?? string.Empty,
                    Css = content?.Style ?? string.Empty
                });
            }
        }

    }
}

