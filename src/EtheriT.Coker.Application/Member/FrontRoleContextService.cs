using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Member
{
    public class FrontRoleContextService : IFrontRoleContextService
    {
        private readonly CokerDbContext db;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;

        public FrontRoleContextService(
            CokerDbContext db,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
        }

        public async Task<FrontRoleContextDto> GetCurrentContextAsync(string orgName = "")
        {
            var uuid = await tokenAppService.GetUUID();
            return await GetContextByUuidAsync(uuid, orgName);
        }

        public async Task<FrontRoleContextDto> GetContextByUuidAsync(Guid uuid, string orgName = "")
        {
            const long guestRoleId = 1;
            const string guestRoleName = "非會員";

            var websiteId = await loginUserData.GetCommonWebsiteId(orgName);

            var roleLevels = new List<FrontRoleLevelDto>
            {
                new FrontRoleLevelDto
                {
                    Id = guestRoleId,
                    Name = guestRoleName
                }
            };

            var frontRoles = await db.Roles
                .Where(e =>
                    e.Type == RoleTypeEnum.前台 &&
                    e.FK_WebsiteId == websiteId &&
                    !e.IsDeleted &&
                    e.Id > 3)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .Select(e => new FrontRoleLevelDto
                {
                    Id = e.Id,
                    Name = e.Name ?? string.Empty
                })
                .ToListAsync();

            roleLevels.AddRange(frontRoles);

            var currentRoleId = await GetRoleIdAsync(uuid, websiteId);

            var roleIndex = roleLevels.FindIndex(e => e.Id == currentRoleId);
            if (roleIndex < 0)
            {
                currentRoleId = guestRoleId;
                roleIndex = 0;
            }

            var currentRole = roleLevels[roleIndex];

            return new FrontRoleContextDto
            {
                WebsiteId = websiteId,
                CurrentRoleId = currentRoleId,
                CurrentRoleName = currentRole.Name,
                IsGuest = currentRoleId == guestRoleId,
                RoleIndex = roleIndex,
                RoleLevels = roleLevels,
                VisibleRoleIds = roleLevels
                    .Take(roleIndex + 1)
                    .Select(e => e.Id)
                    .ToList()
            };
        }

        public async Task<long> GetRoleIdAsync(Guid uuid, long websiteId)
        {
            const long guestRoleId = 1;

            if (uuid == Guid.Empty)
                return guestRoleId;

            var validFrontRoleIds = await db.Roles
                .Where(e =>
                    e.Type == RoleTypeEnum.前台 &&
                    e.FK_WebsiteId == websiteId &&
                    !e.IsDeleted &&
                    e.Id > 3)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .Select(e => e.Id)
                .ToListAsync();

            if (!validFrontRoleIds.Any())
                return guestRoleId;

            var fallbackRoleId = validFrontRoleIds.First();

            var frontUser = await (
                from user in db.FrontUsers
                join websiteMap in db.MappingFrontUserAndWebsite
                    on user.Id equals websiteMap.FK_UserId
                where user.UUID == uuid
                    && websiteMap.FK_WebsiteId == websiteId
                    && !websiteMap.IsDeleted
                select user
            ).FirstOrDefaultAsync();

            if (frontUser == null)
                return guestRoleId;

            // 舊版會員可能沒有建立前台角色對應。
            // 只查找目前網站的前台角色，避免誤用或改到其他網站的角色對應。
            var mapping = await db.MappingUserAndRoles
                .Include(e => e.Role)
                .Include(e => e.User)
                .Where(e => !e.IsDeleted)
                .Where(e => e.UUID == uuid)
                .Where(e => frontUser.FK_User.HasValue && e.UserId == frontUser.FK_User.Value)
                .Where(e => e.User != null)
                .Where(e =>
                    (!string.IsNullOrEmpty(frontUser.Email) && e.User!.Email == frontUser.Email) ||
                    (frontUser.UUID != Guid.Empty && e.User!.UUID == frontUser.UUID) ||
                    (!string.IsNullOrEmpty(frontUser.Account) && e.User!.Account == frontUser.Account) ||
                    (e.User!.Name == frontUser.Name && e.User.Password == frontUser.Password))
                .Where(e => e.Role != null)
                .Where(e => e.Role!.Type == RoleTypeEnum.前台)
                .Where(e => e.Role!.FK_WebsiteId == websiteId)
                .Where(e => !e.Role!.IsDeleted)
                .FirstOrDefaultAsync();

            if (mapping != null && validFrontRoleIds.Contains(mapping.RoleId))
            {
                // Level 僅保留舊版相容性；MappingUserAndRoles 才是角色主來源。
                if (frontUser.Level != mapping.RoleId)
                {
                    frontUser.Level = mapping.RoleId;
                    await db.SaveChangesAsync();
                }

                return mapping.RoleId;
            }

            var systemUser = frontUser.FK_User.HasValue
                ? await db.Users.FirstOrDefaultAsync(e =>
                    e.Id == frontUser.FK_User.Value &&
                    !e.IsDeleted)
                : null;

            if (systemUser != null && !IsSameFrontUserIdentity(frontUser, systemUser))
                systemUser = null;

            if (systemUser == null && !string.IsNullOrWhiteSpace(frontUser.Email))
            {
                systemUser = await db.Users
                    .Where(e => e.Email == frontUser.Email && !e.IsDeleted)
                    .FirstOrDefaultAsync();
            }

            if (systemUser == null && frontUser.UUID != Guid.Empty)
            {
                systemUser = await db.Users
                    .Where(e => e.UUID == frontUser.UUID && !e.IsDeleted)
                    .FirstOrDefaultAsync();
            }

            if (systemUser == null)
                return guestRoleId;

            var legacyMappings = await db.MappingUserAndRoles
                .Where(e => !e.IsDeleted)
                .Where(e => e.UUID == uuid && e.RoleId <= 3)
                .ToListAsync();
            foreach (var legacyMapping in legacyMappings)
            {
                legacyMapping.IsDeleted = true;
                legacyMapping.DeletionTime = DateTime.Now;
            }

            var targetRoleId = frontUser.Level.HasValue &&
                frontUser.Level.Value > 3 &&
                validFrontRoleIds.Contains(frontUser.Level.Value)
                    ? frontUser.Level.Value
                    : fallbackRoleId;

            db.MappingUserAndRoles.Add(new MappingUserAndRole
            {
                UserId = systemUser.Id,
                UUID = uuid,
                RoleId = targetRoleId
            });

            frontUser.FK_User = systemUser.Id;
            frontUser.Level = targetRoleId;

            await db.SaveChangesAsync();

            return targetRoleId;
        }

        private static bool IsSameFrontUserIdentity(FrontUser frontUser, User user)
        {
            if (!string.IsNullOrWhiteSpace(frontUser.Email) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                string.Equals(frontUser.Email.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (frontUser.UUID != Guid.Empty && user.UUID == frontUser.UUID)
                return true;

            if (!string.IsNullOrWhiteSpace(frontUser.Account) &&
                !string.IsNullOrWhiteSpace(user.Account) &&
                string.Equals(frontUser.Account.Trim(), user.Account.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            return !string.IsNullOrWhiteSpace(frontUser.Name) &&
                string.Equals(frontUser.Name, user.Name, StringComparison.Ordinal) &&
                string.Equals(frontUser.Password, user.Password, StringComparison.Ordinal);
        }
    }
}
