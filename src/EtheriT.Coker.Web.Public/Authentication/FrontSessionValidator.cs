using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EtheriT.Coker.Web.Public.Authentication
{
    public sealed class FrontSessionValidator : IFrontSessionValidator
    {
        private readonly CokerDbContext db;

        public FrontSessionValidator(CokerDbContext db)
        {
            this.db = db;
        }

        public async Task<FrontSessionValidationResult> ValidateAsync(
            ClaimsPrincipal? principal,
            long websiteId,
            CancellationToken cancellationToken = default)
        {
            var websiteClaim = principal?.FindFirst("websiteId")?.Value;
            var sidClaim = principal?.FindFirst(ClaimTypes.Sid)?.Value;

            if (!long.TryParse(websiteClaim, out var tokenWebsiteId) ||
                tokenWebsiteId != websiteId ||
                !Guid.TryParse(sidClaim, out var sid))
            {
                return FrontSessionValidationResult.Fail("Token 不屬於目前網站");
            }

            var now = DateTime.Now;
            var dbToken = await db.Tokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.id == sid &&
                    t.websiteId == websiteId &&
                    t.StartTime <= now &&
                    t.EndTime != null &&
                    t.EndTime > now,
                    cancellationToken);

            if (dbToken == null)
                return FrontSessionValidationResult.Fail("Token 不存在或已失效");

            // 訪客 Token 沒有綁定會員，不需要檢查會員狀態。
            if (dbToken.UserID == null)
                return FrontSessionValidationResult.Success;

            var memberUuid = await db.MappingOldNewUUID
                .AsNoTracking()
                .Where(m => m.TempUUID == dbToken.UUID && !m.IsDeleted)
                .Select(m => m.UserUUID)
                .FirstOrDefaultAsync(cancellationToken);
            if (memberUuid == Guid.Empty) memberUuid = dbToken.UUID;

            var memberIsActive = await db.FrontUsers
                .AsNoTracking()
                .AnyAsync(u =>
                    u.UUID == memberUuid &&
                    u.Status == (int)UserStatusEnum.開通 &&
                    !u.IsDeleted &&
                    u.Websites.Any(w =>
                        w.FK_WebsiteId == websiteId &&
                        !w.IsDeleted),
                    cancellationToken);

            return memberIsActive
                ? FrontSessionValidationResult.Success
                : FrontSessionValidationResult.Fail("會員已停權或登入狀態已失效");
        }
    }
}
