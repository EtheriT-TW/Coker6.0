using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Web.Platform.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformReauthenticationService(
    CokerDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    PlatformReauthenticationTicketService ticketService,
    IOptions<EtheriT.Coker.Authentication.Backoffice.BackofficeSessionOptions> sessionOptions)
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    public async Task<ReauthenticateResponse> ReauthenticateAsync(
        ReauthenticateRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
        {
            return new ReauthenticateResponse(false, "無法取得目前的連線狀態。");
        }

        if (!ticketService.TryRead(request.Ticket, out var account))
        {
            return new ReauthenticateResponse(false, "重新驗證資訊已失效，請返回 MVC 重新登入。");
        }

        var user = await db.Users.FirstOrDefaultAsync(candidate =>
            !candidate.IsDeleted &&
            candidate.Account == account,
            cancellationToken);

        if (user == null)
        {
            return new ReauthenticateResponse(false, "目前帳號已不存在或已刪除，請返回 MVC 重新登入。");
        }

        // Match MVC login and Platform access/session validation: backoffice
        // access is determined by the non-deleted user and assigned roles.
        if (!VerifyPassword(user, request.Password))
        {
            return new ReauthenticateResponse(false, "密碼不正確，請重新輸入。");
        }

        var canAccessPlatform = await db.MappingUserAndRoles.AnyAsync(mapping =>
            !mapping.IsDeleted &&
            mapping.UserId == user.Id &&
            mapping.Role != null &&
            !mapping.Role.IsDeleted &&
            mapping.Role.Type == RoleTypeEnum.系統維護,
            cancellationToken);

        if (!canAccessPlatform)
        {
            return new ReauthenticateResponse(false, "目前帳號沒有客戶管理平台權限。");
        }

        var websiteId = await ResolveWebsiteIdAsync(context, cancellationToken);
        if (websiteId <= 0)
        {
            return new ReauthenticateResponse(false, "目前沒有可用的管理站台。");
        }

        var now = DateTime.Now;
        var session = new EtheriT.Coker.Core.Models.Token
        {
            id = Guid.NewGuid(),
            ip = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserID = user.Id,
            StartTime = now,
            EndTime = now.Add(
                sessionOptions.Value.IdleTimeout > TimeSpan.Zero
                    ? sessionOptions.Value.IdleTimeout
                    : TimeSpan.FromMinutes(30)),
            websiteId = websiteId
        };

        db.Tokens.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        var identity = new ClaimsIdentity(
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.Name, user.Account!));
        identity.AddClaim(new Claim(ClaimTypes.Sid, session.id.ToString()));
        identity.AddClaim(new Claim("websiteId", websiteId.ToString()));

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            });

        var cookieDomain = configuration["BackofficeAuthentication:CookieDomain"];
        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.Now.AddDays(30),
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
        };
        context.Response.Cookies.Append(
            "BackstageRefreshToken",
            session.id.ToString(),
            refreshCookieOptions);
        context.Response.Cookies.Append(
            "LastWebSite",
            websiteId.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(180)
            });

        return new ReauthenticateResponse(true);
    }

    private static bool VerifyPassword(User user, string password)
    {
        try
        {
            return PasswordHasher.VerifyHashedPassword(user, user.Password, password) !=
                PasswordVerificationResult.Failed;
        }
        catch
        {
            return false;
        }
    }

    private async Task<long> ResolveWebsiteIdAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (long.TryParse(context.Request.Cookies["LastWebSite"], out var lastWebsiteId) &&
            await db.Websites.AnyAsync(
                website => website.Id == lastWebsiteId && !website.IsDeleted,
                cancellationToken))
        {
            return lastWebsiteId;
        }

        return await db.Websites
            .Where(website => !website.IsDeleted)
            .OrderBy(website => website.Id)
            .Select(website => website.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
