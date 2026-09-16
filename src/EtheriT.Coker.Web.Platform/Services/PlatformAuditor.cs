using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class PlatformAuditor(CokerDbContext db, IHttpContextAccessor accessor) {
    private long? cachedUserId;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        var entries = db.ChangeTracker
            .Entries<FullAuditedEntity>()
            .Where(entry => entry.State is EntityState.Added
                or EntityState.Modified
                or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0) {
            return await db.SaveChangesAsync(cancellationToken);
        }

        var userId = await GetCurrentUserIdAsync(cancellationToken);
        var now = DateTime.Now;

        foreach (var entry in entries) {
            switch (entry.State) {
                case EntityState.Added:
                    entry.Entity.CreatorUserId = userId;
                    entry.Entity.CreationTime = now;
                    break;

                // 呼叫端自己把 IsDeleted 設成 true 的情況。
                case EntityState.Modified when entry.Entity.IsDeleted:
                    entry.Entity.DeleterUserId = userId;
                    entry.Entity.DeletionTime = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifierUserId = userId;
                    entry.Entity.LastModificationTime = now;
                    break;

                // Remove() 進來的硬刪除，改寫成軟刪除。
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeleterUserId = userId;
                    entry.Entity.DeletionTime = now;
                    break;
            }
        }

        return await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<long> GetCurrentUserIdAsync(CancellationToken cancellationToken = default) {
        if (cachedUserId.HasValue)
            return cachedUserId.Value;

        var sessionValue = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Sid);
        if (!Guid.TryParse(sessionValue, out var sessionId)) {
            throw new InvalidOperationException("找不到登入 session，無法識別操作者。");
        }

        var userId = await db.Tokens
            .AsNoTracking()
            .Where(token => token.id == sessionId)
            .Select(token => token.UserID)
            .FirstOrDefaultAsync(cancellationToken);

        if (userId is null or 0) {
            throw new InvalidOperationException("登入 session 未對應到使用者。");
        }

        cachedUserId = userId.Value;
        return userId.Value;
    }
}