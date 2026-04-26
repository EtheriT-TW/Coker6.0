using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace EtheriT.Coker.Application.Token
{
    public class TokenAppService : ITokenAppService
    {
        private readonly JwtHelpers jwt;
        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LoginUserData loginUserData;
        private readonly IDistributedCache cache;
        private readonly IConfiguration configuration;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IMapper mapper;
        public TokenAppService(
            JwtHelpers jwt,
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            LoginUserData loginUserData,
            IDistributedCache cache,
            IConfiguration configuration,
            ICookieManagerAppService cookieManager,
            IMapper mapper)
        {
            this.jwt = jwt;
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.cache = cache;
            this.configuration = configuration;
            this.cookieManager = cookieManager;
            this.mapper = mapper;
        }
        public async Task<TokenResponseDto> CreateToken()
        {
            TokenResponseDto output = new TokenResponseDto();
            var websiteId = loginUserData.GetFrontWebsiteId();
            try
            {
                string? tokenStr = null;
                string? RefreshTokenStr = null;
                DateTime date = DateTime.Now;
                TokenKeyItem tokenItem = new TokenKeyItem();
                bool userStatus = loginUserData.IsLoggedIn();
                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Token", out tokenStr);

                if (!string.IsNullOrEmpty(tokenStr) && userStatus)
                {
                    output = await CheckToken(null);
                    if (output.Success) return output;
                }
                httpContextAccessor.HttpContext?.Response.Cookies.Delete("Token");
                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("RefreshToken", out RefreshTokenStr);
                if (!string.IsNullOrEmpty(RefreshTokenStr) && Guid.TryParse(RefreshTokenStr, out Guid rt))
                {
                    var RefreshTokens = db.Tokens.Where(e => e.id == rt && e.websiteId == websiteId);
                    
                    if (RefreshTokens.Any())
                    {
                        var RefreshToken = await RefreshTokens.Where(e => e.StartTime < date && date < e.EndTime).FirstOrDefaultAsync();
                        var oidRefreshToken = await RefreshTokens.FirstOrDefaultAsync();
                        if (RefreshToken != null)
                        {
                            var uuid = db.MappingOldNewUUID.Where(e => e.TempUUID == RefreshToken.UUID).Select(e => e.UserUUID).FirstOrDefault();
                            var frontUser = await db.FrontUsers.Include(e => e.Websites).Where(e => e.UUID == uuid && e.Websites.Any(s => s.FK_WebsiteId == websiteId)).FirstOrDefaultAsync();
                            if (frontUser != null)
                            {
                                var useraccount = frontUser.Account == null ? frontUser.Email : frontUser.Account;
                                tokenItem = await NewToken(useraccount, RefreshToken.UUID, frontUser?.Id);
                                output.name = frontUser?.Name;
                            }
                            else tokenItem = await NewToken(null, RefreshToken?.UUID);
                        }
                        else tokenItem = await NewToken(null, oidRefreshToken?.UUID);
                    }
                    else tokenItem = await NewToken();
                }
                else tokenItem = await NewToken();
                output.IsLogin = tokenItem.IsLogin;
                output.Token = tokenItem.AccessToken;
                output.RefreshToken = tokenItem.RefreshToken;
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally {
                try
                {
                    await CleanupExpiredTokensAsync(websiteId);
                }
                catch
                {
                    // 這邊一定要吃掉，不可以讓清理錯誤影響到主流程
                    // 有 Log 系統的話建議寫 Log
                }

            }

            return output;
        }
        /// <summary>
        /// 清理過期或失聯的 Token / ShoppingCart：
        /// 1. 清孤兒購物車：沒有對應 Token 的、且尚未成立訂單
        /// 2. 清過期 Token
        /// 3. 要刪的購物車：這些 Token 底下、尚未成立訂單的
        /// </summary>
        private async Task CleanupExpiredTokensAsync(long websiteId)
        {
            var now = DateTime.Now;

            // === 1) 清孤兒購物車：沒有對應 Token 的、且尚未成立訂單 ===
            var orphanCarts = await (
                from c in db.ShoppingCarts   // 依你的 DbSet 名稱調整
                where !c.IsOrder
                      && !db.Tokens.Any(t => t.id == c.FK_Tid)
                select c
            )
            .Take(100)
            .ToListAsync();

            if (orphanCarts.Count > 0)
            {
                db.ShoppingCarts.RemoveRange(orphanCarts);
                await db.SaveChangesAsync();
            }

            // === 2) 清過期 Token ===

            // 找候選 Token：該網站且已過期
            var expiredTokens = await (
                from t in db.Tokens
                where t.websiteId == websiteId
                      && t.EndTime != null
                      && t.EndTime < now
                orderby t.EndTime  // 先刪最舊的
                select t
            )
            .Include(t => t.ShoppingCarts)
            .Take(100)
            .ToListAsync();

            if (expiredTokens.Count == 0)
                return;

            // 要刪的購物車：這些 Token 底下、尚未成立訂單的
            var cartsToDelete = expiredTokens
                .SelectMany(t => t.ShoppingCarts.Where(c => !c.IsOrder))
                .ToList();

            if (cartsToDelete.Count > 0)
            {
                db.ShoppingCarts.RemoveRange(cartsToDelete);
            }

            // 不管是否有訂單，只要 Token 過期就刪
            db.Tokens.RemoveRange(expiredTokens);

            await db.SaveChangesAsync();
        }

        public async Task<TokenKeyItem> NewToken(string? Accont = null, Guid? UUID = null, long? UserId = null)
        {
            if (string.IsNullOrEmpty(Accont)) Accont = Guid.NewGuid().ToString();
            if (UUID.IsNullOrEmpty()) UUID = generateUUID();
            DateTime date = DateTime.Now;
            DateTime TokenEndTime = date.AddMinutes(15);
            DateTime EndDateTime = date.AddDays(30);
            var item = new TokenKeyItem { UUID = UUID.Value };
            Core.Models.Token? Token = new Core.Models.Token
            {
                ip = loginUserData.GetClientIP() ?? "",
                UserID = UserId,
                UUID = UUID.Value,
                StartTime = date,
                EndTime = EndDateTime,
                websiteId = configuration.GetValue<long>("WebConfig:SiteId")
            };
            db.Tokens.Add(Token);
            await db.SaveChangesAsync();
            item.RefreshToken = Token.id;
            item.IsLogin = UserId != null;
            item.AccessToken = await CreateToken(Accont.ToString(), Token.id);
            return item;
        }
        public async Task<TokenResponseDto> CheckToken(string? token)
        {
            TokenResponseDto output = new TokenResponseDto();
            try
            {
                if (string.IsNullOrEmpty(token)) token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token)) httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Token", out token);
                if (string.IsNullOrEmpty(token)) token = httpContextAccessor.HttpContext?.Items["Token"]?.ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSettings:SignKey"))),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                    handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                    var jwtToken = validatedToken as JwtSecurityToken;
                    output.Success = true;
                    output.Token = token;

                    if (Guid.TryParse(jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value, out var Sid)) output.RefreshToken = Sid;

                    if (Guid.TryParse(jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value, out var Sub)) output.IsLogin = false;
                    else
                    {
                        output.IsLogin = true;
                        output.name = jwtToken?.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                    }

                    var db_token = await db.Tokens.Where(e => e.id == Sid).FirstOrDefaultAsync();
                    if (db_token != null)
                    {
                        if (output.IsLogin)
                        {
                            var frontUser = await db.FrontUsers.Where(e => e.UUID == db_token.UUID).FirstOrDefaultAsync();
                            if (frontUser != null && frontUser.Status == (int)UserStatusEnum.開通)
                            {
                                if (frontUser.PrivacyAgreeTime != null)
                                {
                                    var agreetime = frontUser.PrivacyAgreeTime.Value;
                                    if (agreetime.AddYears(1) > DateTime.Now) output.AgreePrivacy = true;
                                }
                            }
                            else output.IsLogin = false;
                        }
                        else
                        {
                            if (db_token.PrivacyAgreeTime != null)
                            {
                                var agreetime = db_token.PrivacyAgreeTime.Value;
                                if (agreetime.AddYears(1) > DateTime.Now) output.AgreePrivacy = true;
                            }
                        }
                    }
                }
                else throw new Exception("Token取得錯誤");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally { 
                
            }
            return output;
        }
        public async Task<ResponseMessageDto> AgreePrivacy()
        {
            ResponseMessageDto response = new ResponseMessageDto();

            try
            {
                var tokencheck = await CheckToken(null);
                if (tokencheck != null)
                {
                    var token = await db.Tokens.Where(e => e.id == tokencheck.RefreshToken).FirstOrDefaultAsync();
                    if (token != null)
                    {
                        token.PrivacyAgreeTime = DateTime.Now;
                        if (tokencheck.IsLogin)
                        {
                            var frontuser = await db.FrontUsers.Where(e => e.UUID == token.UUID).FirstOrDefaultAsync();
                            if (frontuser != null)
                            {
                                frontuser.PrivacyAgreeTime = token.PrivacyAgreeTime;
                                await loginUserData.SaveChanges(frontuser);
                            }
                        }
                        db.SaveChanges();
                        response.Success = true;
                        response.Message = token.PrivacyAgreeTime?.ToString("yyyy-MM-ddTHH:mm:ss") ?? null;
                    }
                }
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }
            return response;
        }
        public async Task<Guid> GetUUID()
        {
            Guid tokenId = new Guid();
            Guid UUID = new Guid();
            var token = await CheckToken(null);
            if (token != null && token.Success)
            {
                if (token.RefreshToken != null)
                {
                    var t = await db.Tokens.Where(e => e.id == token.RefreshToken).FirstOrDefaultAsync();
                    if (t != null) UUID = t.UUID;
                }
            }
            return UUID;
        }
        public Guid GetUUID(Guid oldUUID)
        {
            var data = db.MappingOldNewUUID.Where(e => e.UserUUID == oldUUID || e.TempUUID == oldUUID).Select(e => e.UserUUID).FirstOrDefault();
            return data == Guid.Empty ? oldUUID : data;
        }
        public async Task<List<Guid>> GetAllUUID(Guid UUID)
        {
            var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID && e.TempUUID != Guid.Empty).Select(e => e.TempUUID).ToListAsync();
            uuids.Add(UUID);
            return uuids;
        }
        public async Task<TokenResponseDto> RefreshToken(Guid? id)
        {
            TokenResponseDto output = new TokenResponseDto();
            try
            {
                if (id == null) throw new Exception("Token type error");
                Core.Models.Token tokens = db.Tokens.Where(e => e.id == id).Where(e => e.EndTime > DateTime.Now).First();
                if (tokens == null)
                {
                    output.Success = false;
                    output.Error = "Token doesn't exist";
                }
                else if (tokens.EndTime < DateTime.Now.AddMonths(1))
                {
                    var token = await CreateToken();
                    var newToken = db.Tokens.Where(e => e.id == Guid.Parse(token.Token ?? "")).Where(e => e.EndTime > DateTime.Now).First();
                    newToken.UUID = tokens.UUID;
                    newToken.UserID = tokens.UserID;
                    output.Success = true;
                    db.SaveChanges();
                    tokens = newToken;
                }
                else output.Success = true;
                if (output.Success)
                {
                    string token;
                    if (tokens.UserID == null) token = await CreateToken(Guid.NewGuid().ToString(), tokens.id);
                    else
                    {
                        var user = await db.Users.Where(e => e.IsDeleted && e.UUID == tokens.UUID).FirstOrDefaultAsync();
                        if (user != null)
                        {
                            token = await CreateToken(user.Account ?? user.Email ?? user.UUID.ToString() ?? Guid.NewGuid().ToString(), tokens.id);
                        }
                        else token = await CreateToken(Guid.NewGuid().ToString(), tokens.id);
                    }

                    output.IsLogin = tokens.UserID != null;
                    output.Token = token;
                    output.Success = true;
                    if (output.IsLogin)
                    {
                        var user = db.Users.Where(e => e.Status == (int)UserStatusEnum.開通 && !e.IsDeleted && e.Id == tokens.UserID);
                        output.name = db.Users.Where(e => e.Status == (int)UserStatusEnum.開通 && !e.IsDeleted).FirstOrDefault()?.Name;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<string> CreateToken(string account, Guid secret, CookiePurposeEnum tokenPurpose = CookiePurposeEnum.BackstageAuthToken, string position = "")
        {
            List<KeyValuePair<string, string>> custClaims = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(position))
            {

                var user = db.Users.Where(e => e.Account == account).FirstOrDefault();
                if (user != null)
                {
                    custClaims.Add(new KeyValuePair<string, string>("username", user.Name));
                }
            }
            else
            {
                var websiteid = loginUserData.GetFrontWebsiteId();
                var user = await (from fu in db.FrontUsers
                                  join mapuserweb in db.MappingFrontUserAndWebsite on fu.Id equals mapuserweb.FK_UserId
                                  where mapuserweb.FK_WebsiteId == websiteid && fu.Email == account
                                  select fu).FirstOrDefaultAsync();
                if (user != null)
                {
                    custClaims.Add(new KeyValuePair<string, string>("username", user.Name));
                }
            }

            List<string> roles = new List<string> {
                "Admin",
                "Users"
            };
            string token = await jwt.GenerateToken(account, roles, secret, tokenPurpose, custClaims);
            //cookieManager.Set($"{position}Token", token, CookiePurposeEnum.XsrfToken);
            cookieManager.Set($"{position}Token", token, tokenPurpose);
            cookieManager.Set($"{position}RefreshToken", secret.ToString(), CookiePurposeEnum.RefreshToken);

            var key = $"{position}Token";
            var items = httpContextAccessor.HttpContext!.Items;
            if (!items.ContainsKey(key))
            {
                items.Add(key, token);
            }
            else
            {
                items[key] = token;
            }
            return token;
        }
        public async Task<bool> DelToken()
        {
            string token = loginUserData.GetAuthorization();
            await cache.SetStringAsync(
                GetKey(token),
                " ",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(0.001)
                }
            );
            return true;
        }
        public async Task<bool> IsTokenRevoked(string token)
        {
            var revoked = await cache.GetStringAsync(GetKey(token));
            return !string.IsNullOrEmpty(revoked);
        }
        private static string GetKey(string token)
        {
            return $"tokens:{token}:deactivated";
        }
        private Guid generateUUID()
        {
            Guid id = Guid.NewGuid();
            var c = db.Tokens.Where(e => e.UUID == id);
            if (c.Any()) return generateUUID();
            else
            {
                var u = db.Users.Where(e => !e.IsDeleted && e.UUID == id);
                if (u.Any()) return generateUUID();
                return id;
            }
        }
    }
}
