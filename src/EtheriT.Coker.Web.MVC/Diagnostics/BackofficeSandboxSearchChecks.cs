using EtheriT.Coker.Application.Shared.Search;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Authentication;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static void SearchKeywordEncoding()
    {
        const string keyword = "LL-17 FE+";
        const string encoded = "~b64~TEwtMTcgRkUr";
        Require(SearchKeywordCodec.Encode(keyword) == encoded && SearchKeywordCodec.Decode(encoded) == keyword,
            "Keyword encoding must match the UTF-8 Base64Url browser protocol.");
        foreach (var text in new[] { "中文機油 + / ? # & %2B", "emoji 🚗", "  space  ", "\uFEFF標籤", "~b64~TEwtMTcgRkUr" })
        {
            var segment = SearchKeywordCodec.Encode(text);
            Require(!segment.Contains('+') && !segment.Contains('/') && !segment.Contains('=') && !segment.Contains('%'),
                "Generated route segments must avoid IIS-sensitive and URL delimiter characters.");
            Require(SearchKeywordCodec.Decode(segment) == text, "Keyword round-trip must preserve text without trimming or double decoding.");
        }
        foreach (var legacy in new[] { "LL-17 FE+", "中文", "literal%2B", "TEwtMTcgRkUr", "~b64~", "~b64~bad*", "~b64~YQ==", "~b64~_w", "~b64~YR" })
            Require(SearchKeywordCodec.Decode(legacy) == legacy, "Unmarked or malformed legacy keywords must stay literal.");
        Require(SearchKeywordCodec.Decode(null) == "" && SearchKeywordCodec.Encode("") == "", "Empty searches must keep their existing behavior.");
    }

    private static async Task SearchLogValidation(BackofficeSandbox box)
    {
        const long site = 999000001;
        const string signKey = "diagnostics-only-search-log-signing-key-0123456789";
        var config = box.Services.GetRequiredService<IConfiguration>();
        config["WebConfig:SiteId"] = site.ToString();
        config["JwtSettings:SignKey"] = signKey;
        var http = box.Services.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        var service = box.Services.GetRequiredService<ICustSearchAppService>();
        var visitor = Guid.NewGuid();
        var token = new Token { id = Guid.NewGuid(), UUID = visitor, websiteId = site,
            StartTime = DateTime.Now.AddMinutes(-1), EndTime = DateTime.Now.AddHours(1), ip = "127.0.0.1" };
        box.Db.Tokens.Add(token);
        await box.Db.SaveChangesAsync();

        async Task Save(string? keyword, long target = SearchTargetIds.Product)
        {
            await service.SaveSearchLog(new SaveSearchLogDto { Key = keyword!, FK_WebsiteId = site, FK_CustSearchId = target });
        }
        void UseToken()
        {
            var jwt = new JwtSecurityToken(claims: new[] { new Claim(ClaimTypes.Sid, token.id.ToString()) },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey)), SecurityAlgorithms.HmacSha256));
            http.Items["Token"] = new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        await Save("missing-token");
        http.Items["Token"] = "invalid-token";
        await Save("invalid-token");
        Require(!await box.Db.SearchLogs.AnyAsync(), "Requests without a valid visitor token must not insert empty UUID logs.");

        UseToken();
        foreach (var keyword in new string?[] { null, "", "  ", "undefined", " Undefined ", "NULL", new string('x', 101) })
            await Save(keyword);
        Require(!await box.Db.SearchLogs.AnyAsync(), "Missing, placeholder and overlength keywords must not be logged even with a valid token.");

        await Save("  LL-17 FE+  ");
        await Save("LL-17 FE+");
        await Save("LL-17 FE+", SearchTargetIds.Article);
        await Save("中文機油");
        var logs = await box.Db.SearchLogs.AsNoTracking().ToListAsync();
        Require(logs.Count == 3 && logs.All(x => x.UUID == visitor && x.FK_WebsiteId == site) &&
            logs.Count(x => x.Key == "LL-17 FE+") == 2 && logs.Any(x => x.Key == "中文機油"),
            "Valid anonymous searches must preserve the token UUID, trim keywords, retain target isolation and deduplicate within the day.");

        token.EndTime = DateTime.Now.AddMinutes(-1);
        await box.Db.SaveChangesAsync();
        await Save("expired-token");
        token.EndTime = DateTime.Now.AddHours(1);
        token.websiteId = site + 1;
        await box.Db.SaveChangesAsync();
        await Save("foreign-token");
        token.websiteId = site;
        token.UUID = Guid.Empty;
        await box.Db.SaveChangesAsync();
        await Save("empty-visitor");
        Require(await box.Db.SearchLogs.CountAsync() == 3, "Expired, foreign-site and empty UUID tokens must not add logs.");

        token.UUID = visitor;
        await box.Db.SaveChangesAsync();
        UseToken();
        http.User = new ClaimsPrincipal(new ClaimsIdentity());
        http.Request.Headers["Cookie"] = "Token=" + http.Items["Token"] + "; RefreshToken=" + token.id;
        var tokenService = box.Services.GetRequiredService<ITokenAppService>();
        var reused = await tokenService.CreateToken();
        Require(reused.Success && reused.RefreshToken == token.id && await box.Db.Tokens.CountAsync() == 1,
            "A valid anonymous access token must be reused instead of being treated as a missing member.");

        // Replace only sign-in infrastructure; all token issuance and cookies still use real services.
        http.RequestServices = new SearchTokenFixtureProvider(http.RequestServices);
        var expiredAccess = new JwtSecurityToken(claims: new[] { new Claim(ClaimTypes.Sid, token.id.ToString()) },
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey)), SecurityAlgorithms.HmacSha256));
        http.Request.Headers["Cookie"] = "Token=" + new JwtSecurityTokenHandler().WriteToken(expiredAccess) + "; RefreshToken=" + token.id;
        var renewed = await tokenService.CreateToken();
        Require(renewed.Success && renewed.RefreshToken != token.id && !renewed.IsLogin,
            "Expired access tokens must renew from the anonymous refresh token without granting member login.");
        var renewedToken = await box.Db.Tokens.SingleAsync(x => x.id == renewed.RefreshToken);
        http.Request.Headers["Cookie"] = "Token=" + renewed.Token + "; RefreshToken=" + renewed.RefreshToken;
        Require(renewedToken.UUID == visitor && token.EndTime > DateTime.Now && await tokenService.GetUUID() == visitor,
            "Anonymous renewal must preserve UUID and leave the prior refresh token valid for concurrent tabs.");
        await Save("after-renewal");
        Require(await box.Db.SearchLogs.AnyAsync(x => x.Key == "after-renewal" && x.UUID == visitor),
            "Searches after renewal must use the same nonempty visitor identity.");

    }

    private sealed class SearchTokenFixtureProvider(IServiceProvider inner) : IServiceProvider
    {
        private readonly IAuthenticationService authentication = new SearchTokenFixtureAuthentication();
        public object? GetService(Type type) => type == typeof(IAuthenticationService) ? authentication : inner.GetService(type);
    }

    private sealed class SearchTokenFixtureAuthentication : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }
}
