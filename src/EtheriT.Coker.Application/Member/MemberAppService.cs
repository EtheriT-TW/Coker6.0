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
                await EnsureMissingFrontRoleMappingsAsync(websiteId);

                var baseQuery =
                    from f in db.FrontUsers
                    join s in db.MappingFrontUserAndWebsite on f.Id equals s.FK_UserId
                    where s.FK_WebsiteId == websiteId && !s.IsDeleted && !f.IsDeleted
                    select f;

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
                    RoleId = db.MappingUserAndRoles
                        .Where(m => !m.IsDeleted && m.UUID == f.UUID)
                        .Where(m => f.FK_User.HasValue && m.UserId == f.FK_User.Value)
                        .Where(m => m.Role != null)
                        .Where(m => m.Role!.FK_WebsiteId == websiteId)
                        .Where(m => m.Role!.Type == RoleTypeEnum.前台)
                        .Where(m => !m.Role!.IsDeleted)
                        .Select(m => (long?)m.RoleId)
                        .FirstOrDefault(),
                    CreationTime = f.CreationTime,
                    Total = (
                        from order in db.Order_Headers
                        where order.State == OrderStatusEnum.已完成 &&
                              order.FK_WebsiteId == websiteId &&
                              order.FK_UUID == f.UUID
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
            catch
            {
                // 補綁或清單查詢失敗都是真正錯誤，不可偽裝成空清單。
                throw;
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
                    output.RoleId = await db.MappingUserAndRoles
                        .Where(e => !e.IsDeleted && e.UUID == result.UUID)
                        .Where(e => e.Role != null)
                        .Where(e => e.Role!.FK_WebsiteId == websideId)
                        .Where(e => e.Role!.Type == RoleTypeEnum.前台 && !e.Role!.IsDeleted)
                        .Select(e => e.RoleId)
                        .FirstOrDefaultAsync();
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

                var role = await db.Roles
                    .Where(e => e.FK_WebsiteId == websiteId)
                    .Where(e => e.Type == RoleTypeEnum.前台 && !e.IsDeleted)
                    .Where(e => !dto.RoleId.HasValue || dto.RoleId == 0 || e.Id == dto.RoleId.Value)
                    .OrderBy(e => e.Ser_No)
                    .ThenBy(e => e.Id)
                    .FirstOrDefaultAsync();
                if (role == null) throw new Exception("角色不存在");

                var newUser = mapper.Map<FrontUser>(dto);
                newUser.UUID = Guid.NewGuid();
                newUser.Password = _stringHandler.RandonCode(RandomStringType.數字加英文大小寫及符號, 32);
                newUser.ForgetID = Guid.NewGuid();
                newUser.ForgeIDSendDate = DateTime.Now;
                newUser.Level = role.Id;
                await loginUserData.setOptionParameter(newUser);
                db.FrontUsers.Add(newUser);

                if (bUser == null)
                {
                    bUser = mapper.Map<User>(dto);
                    // Users.UUID 保留舊設計：第一次建立 User 時，跟第一個 FrontUser.UUID 一致
                    bUser.UUID = newUser.UUID;
                    bUser.Password = newUser.Password;
                    await loginUserData.setOptionParameter(bUser);
                    db.Users.Add(bUser);
                }

                // 關聯
                newUser.User = bUser; // 注意：若 bUser 新增，Id 會在 SaveChanges 後才有
                                            // 更好的作法是用 navigation（如果你有）：newUser.User = bUser;

                var roleMaping = new MappingUserAndRole
                {
                    User = bUser,
                    RoleId = role.Id,
                    UUID = newUser.UUID
                };
                db.MappingUserAndRoles.Add(roleMaping);
                await loginUserData.setOptionParameter(roleMaping);

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
            long websiteId = await loginUserData.GetWebsiteId();
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    return await FrontAdd(dto);
                }

                var result = await (
                    from user in db.FrontUsers
                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                    where user.Id == dto.Id
                       && map.FK_WebsiteId == websiteId
                       && !map.IsDeleted
                    select user
                ).FirstOrDefaultAsync();

                if (result == null)
                {
                    throw new Exception("查無會員資料");
                }

                if (dto.Status == null) dto.Status = result.Status;
                if (dto.TelPhone == null) dto.TelPhone = result.TelPhone;

                mapper.Map(dto, result);

                if (result.Status != (int)UserStatusEnum.開通)
                {
                    await InvalidateFrontUserTokensAsync(result.UUID, websiteId);
                }

                if (dto.RoleId != null && dto.RoleId != 0)
                {
                    var targetRole = await db.Roles
                        .Where(e => e.FK_WebsiteId == websiteId)
                        .Where(e => e.Type == RoleTypeEnum.前台)
                        .Where(e => e.Id == dto.RoleId.Value)
                        .Where(e => !e.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (targetRole == null)
                    {
                        throw new Exception("角色不存在");
                    }

                    var systemUser = await GetOrCreateSystemUserForFrontUserAsync(result);

                    var roleMapping = await db.MappingUserAndRoles
                        .Include(e => e.Role)
                        .Where(e => !e.IsDeleted)
                        .Where(e => e.UUID == result.UUID)
                        .Where(e => e.UserId == systemUser.Id)
                        .Where(e => e.Role != null)
                        .Where(e => e.Role.FK_WebsiteId == websiteId)
                        .Where(e => e.Role.Type == RoleTypeEnum.前台)
                        .Where(e => !e.Role.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (roleMapping == null)
                    {
                        roleMapping = new MappingUserAndRole
                        {
                            UserId = systemUser.Id,
                            UUID = result.UUID,
                            RoleId = dto.RoleId.Value
                        };

                        db.MappingUserAndRoles.Add(roleMapping);
                        await loginUserData.setOptionParameter(roleMapping);
                    }
                    else if (roleMapping.RoleId != dto.RoleId.Value)
                    {
                        roleMapping.RoleId = dto.RoleId.Value;
                        await loginUserData.setOptionParameter(roleMapping);
                    }

                    result.Level = dto.RoleId.Value;
                }

                await loginUserData.setOptionParameter(result);
                await db.SaveChangesAsync();

                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.InnerException?.Message ?? e.Message;
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

        private async Task EnsureMissingFrontRoleMappingsAsync(long websiteId)
        {
            var executionStrategy = db.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                var frontRoles = await db.Roles
                .Where(e => e.FK_WebsiteId == websiteId)
                .Where(e => e.Type == RoleTypeEnum.前台 && !e.IsDeleted)
                .Where(e => e.Id > 3)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .ToListAsync();

            if (!frontRoles.Any())
                throw new Exception("尚未設定可用的前台會員角色");

            var defaultRoleId = frontRoles.First().Id;
            var validRoleIds = frontRoles.Select(e => e.Id).ToHashSet();

            var websiteFrontUsers = await (
                from frontUser in db.FrontUsers
                join websiteMap in db.MappingFrontUserAndWebsite
                    on frontUser.Id equals websiteMap.FK_UserId
                where websiteMap.FK_WebsiteId == websiteId
                    && !websiteMap.IsDeleted
                    && !frontUser.IsDeleted
                select frontUser
            ).Distinct().ToListAsync();

            var memberUuids = websiteFrontUsers.Select(e => e.UUID).Distinct().ToList();
            var mappings = await db.MappingUserAndRoles
                .Include(e => e.Role)
                .Where(e => memberUuids.Contains(e.UUID) && !e.IsDeleted)
                .ToListAsync();
            var mappingsByUuid = mappings
                .GroupBy(e => e.UUID)
                .ToDictionary(e => e.Key, e => e.OrderBy(mapping => mapping.Id).ToList());

            foreach (var frontUser in websiteFrontUsers)
            {
                var memberMappings = mappingsByUuid.TryGetValue(frontUser.UUID, out var existingMappings)
                    ? existingMappings
                    : new List<MappingUserAndRole>();

                // RoleId 1~3 屬於早期資料，不視為有效會員角色。
                foreach (var legacyMapping in memberMappings.Where(e => e.RoleId <= 3))
                {
                    legacyMapping.IsDeleted = true;
                    legacyMapping.DeletionTime = DateTime.Now;
                }

                var validMappings = memberMappings
                    .Where(e => e.RoleId > 3)
                    .Where(e => e.Role != null)
                    .Where(e => !e.Role!.IsDeleted)
                    .Where(e => e.Role!.FK_WebsiteId == websiteId)
                    .Where(e => e.Role!.Type == RoleTypeEnum.前台)
                    .OrderBy(e => e.Id)
                    .ToList();

                // FK_User 只能在身分資料相符時沿用；舊資料可能錯指到其他 Users。
                var systemUser = await GetOrCreateSystemUserForFrontUserAsync(frontUser);

                if (validMappings.Any())
                {
                    var activeMapping = validMappings.First();
                    if (activeMapping.UserId != systemUser.Id)
                    {
                        activeMapping.UserId = systemUser.Id;
                        await loginUserData.setOptionParameter(activeMapping);
                    }
                    frontUser.Level = activeMapping.RoleId;

                    // 同一網站只保留一個有效前台角色。
                    foreach (var duplicate in validMappings.Skip(1))
                    {
                        duplicate.IsDeleted = true;
                        duplicate.DeletionTime = DateTime.Now;
                    }

                    continue;
                }

                var targetRoleId = frontUser.Level.HasValue &&
                    frontUser.Level.Value > 3 &&
                    validRoleIds.Contains(frontUser.Level.Value)
                        ? frontUser.Level.Value
                        : defaultRoleId;

                var newMapping = new MappingUserAndRole
                {
                    UserId = systemUser.Id,
                    UUID = frontUser.UUID,
                    RoleId = targetRoleId
                };

                await loginUserData.setOptionParameter(newMapping);
                db.MappingUserAndRoles.Add(newMapping);
                frontUser.Level = targetRoleId;
            }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }

        public async Task<List<MemberRoleSelectDto>> GetAllRole()
        {
            List<MemberRoleSelectDto> output = new List<MemberRoleSelectDto>();
            long websideId = await loginUserData.GetWebsiteId();
            try
            {
                output = await (from role in db.Roles
                                where role.FK_WebsiteId == websideId
                                where role.Type == RoleTypeEnum.前台
                                where !role.IsDeleted
                                orderby role.Ser_No, role.Id
                                select new MemberRoleSelectDto()
                                {
                                    Id = role.Id,
                                    Ser_No = role.Ser_No,
                                    Name = role.Name
                                }).ToListAsync();

                if (output.Count > 0)
                {
                    output[0].IsDefault = true;
                }
            }
            catch (Exception e)
            {

            }
            return output;
        }

        private async Task InvalidateFrontUserTokensAsync(Guid frontUserUuid, long websiteId)
        {
            if (frontUserUuid == Guid.Empty) return;

            var tokenUuids = await db.MappingOldNewUUID
                .Where(e => e.UserUUID == frontUserUuid && !e.IsDeleted)
                .Select(e => e.TempUUID)
                .ToListAsync();
            tokenUuids.Add(frontUserUuid);

            var now = DateTime.Now;
            var activeTokens = await db.Tokens
                .Where(e =>
                    e.websiteId == websiteId &&
                    tokenUuids.Contains(e.UUID) &&
                    e.EndTime != null &&
                    e.EndTime > now)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.EndTime = now;
            }
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

                        // 新增角色一律排在現有角色之後，避免意外取代
                        // 正常註冊與舊會員補綁時所使用的第一個預設角色。
                        var maxSerNo = await db.Roles
                            .Where(e =>
                                e.FK_WebsiteId == websiteId &&
                                e.Type == RoleTypeEnum.前台 &&
                                !e.IsDeleted)
                            .Select(e => (int?)e.Ser_No)
                            .MaxAsync() ?? 0;
                        role.Ser_No = maxSerNo + 1;

                        db.Roles.Add(role);
                    }
                    else
                    {
                        data.Id = id;
                        role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                        if (role != null)
                        {
                            // DevExtreme 更新時只會傳送有變更的欄位，不可將未傳入的
                            // nullable Ser_No 透過 AutoMapper 寫成 0，否則只改名稱也會改變預設角色。
                            if (!string.IsNullOrEmpty(data.Name)) role.Name = data.Name;
                            if (data.Ser_No.HasValue) role.Ser_No = data.Ser_No.Value;
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
        private async Task<User> GetOrCreateSystemUserForFrontUserAsync(FrontUser frontUser)
        {
            User? user = null;

            // 1. 優先使用 FrontUsers.FK_User 對應 Users.Id
            if (frontUser.FK_User != null && frontUser.FK_User > 0)
            {
                user = await db.Users
                    .FirstOrDefaultAsync(e => e.Id == frontUser.FK_User.Value);

                if (user != null && !IsSameFrontUserIdentity(frontUser, user))
                    user = null;
            }

            // 2. 舊資料若沒有 FK_User，先用 Email 找 Users
            if (user == null && !string.IsNullOrWhiteSpace(frontUser.Email))
            {
                user = await db.Users
                    .FirstOrDefaultAsync(e => e.Email == frontUser.Email);
            }

            // 3. 再用 UUID 補救舊資料
            //    因為舊資料 Users.UUID 可能等於第一個 FrontUser.UUID
            if (user == null && frontUser.UUID != Guid.Empty)
            {
                user = await db.Users
                    .FirstOrDefaultAsync(e => e.UUID == frontUser.UUID);
            }

            // 4. 如果真的沒有 Users，補建 Users 總表資料
            if (user == null)
            {
                // 不可使用 AutoMapper，否則會把 FrontUsers.Id 複製到 Users.Id，
                // 導致 SQL Server Identity 欄位寫入失敗。
                user = new User
                {
                    UUID = frontUser.UUID,
                    Account = frontUser.Account,
                    Email = frontUser.Email,
                    Name = frontUser.Name,
                    CellPhone = frontUser.CellPhone,
                    TelPhone = frontUser.TelPhone,
                    Address = frontUser.Address,
                    Password = frontUser.Password,
                    Sex = frontUser.Sex,
                    Status = frontUser.Status,
                    ErrorTimes = frontUser.ErrorTimes,
                    LockTime = frontUser.LockTime
                };

                await loginUserData.setOptionParameter(user);
                db.Users.Add(user);

                // 先存 Users，取得 user.Id，後面 MappingUserAndRoles.UserId 才不會 FK 衝突
                await db.SaveChangesAsync();
            }

            // 5. 回補 FrontUsers.FK_User，避免下次又靠 Email / UUID 找
            if (frontUser.FK_User != user.Id)
            {
                frontUser.FK_User = user.Id;
                await loginUserData.setOptionParameter(frontUser);
            }

            return user;
        }

        private static bool IsSameFrontUserIdentity(FrontUser frontUser, User user)
        {
            if (!string.IsNullOrWhiteSpace(frontUser.Email) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                string.Equals(frontUser.Email.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (frontUser.UUID != Guid.Empty &&
                user.UUID.HasValue &&
                user.UUID.Value == frontUser.UUID)
                return true;

            if (!string.IsNullOrWhiteSpace(frontUser.Account) &&
                !string.IsNullOrWhiteSpace(user.Account) &&
                string.Equals(frontUser.Account.Trim(), user.Account.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            // 新補建的 Users 會沿用相同姓名與密碼，可避免無 Email／帳號資料重複建立。
            return !string.IsNullOrWhiteSpace(frontUser.Name) &&
                string.Equals(frontUser.Name, user.Name, StringComparison.Ordinal) &&
                string.Equals(frontUser.Password, user.Password, StringComparison.Ordinal);
        }
    }
}

