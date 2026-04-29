using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
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
                    !e.IsDeleted)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .Select(e => new FrontRoleLevelDto
                {
                    Id = e.Id,
                    Name = e.Name ?? string.Empty
                })
                .ToListAsync();

            roleLevels.AddRange(frontRoles);

            var currentRoleId = await EnsureFrontUserRoleAsync(uuid, websiteId, guestRoleId);

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

        private async Task<long> EnsureFrontUserRoleAsync(Guid uuid, long websiteId, long guestRoleId)
        {
            var userId = await db.Tokens
                .Where(e => e.UUID == uuid && e.UserID != null && e.UserID > 0)
                .OrderByDescending(e => e.StartTime)
                .Select(e => e.UserID)
                .FirstOrDefaultAsync();

            if (userId == null || userId == 0)
                return guestRoleId;

            var validFrontRoleIds = await db.Roles
                .Where(e =>
                    e.Type == RoleTypeEnum.前台 &&
                    e.FK_WebsiteId == websiteId &&
                    !e.IsDeleted)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .Select(e => e.Id)
                .ToListAsync();

            if (!validFrontRoleIds.Any())
                return guestRoleId;

            var fallbackRoleId = validFrontRoleIds.First();

            var mapping = await db.MappingUserAndRoles
                .Where(e => e.UUID == uuid)
                .FirstOrDefaultAsync();

            if (mapping != null && validFrontRoleIds.Contains(mapping.RoleId))
                return mapping.RoleId;

            if (mapping == null)
            {
                db.MappingUserAndRoles.Add(new MappingUserAndRole
                {
                    UUID = uuid,
                    RoleId = fallbackRoleId
                });
            }
            else
            {
                mapping.RoleId = fallbackRoleId;
            }

            await db.SaveChangesAsync();

            return fallbackRoleId;
        }
    }
}