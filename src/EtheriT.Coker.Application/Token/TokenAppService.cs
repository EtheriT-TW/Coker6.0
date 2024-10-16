using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using System.Security.Principal;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using EtheriT.Coker.Web.Core.Models;
using System.Security.Claims;
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
        private readonly IMapper mapper;
        public TokenAppService(
            JwtHelpers jwt,
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            LoginUserData loginUserData,
            IDistributedCache cache,
            IConfiguration configuration,
            IMapper mapper)
        {
            this.jwt = jwt;
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.cache = cache;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public async Task<TokenResponseDto> CreateToken()
        {
            TokenResponseDto output = new TokenResponseDto();
            try
            {
                string? tokenStr = null;
                string? RefreshTokenStr = null;
                DateTime date = DateTime.Now;
                TokenKeyItem tokenItem = new TokenKeyItem();
                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Token", out tokenStr);
                if (!string.IsNullOrEmpty(tokenStr))
                {
                    output = CheckToken();
                    if (output.Success) return output;
                }
                httpContextAccessor.HttpContext?.Response.Cookies.Delete("Token");
                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("RefreshToken", out RefreshTokenStr);
                if (!string.IsNullOrEmpty(RefreshTokenStr) && Guid.TryParse(RefreshTokenStr, out Guid rt))
                {
                    var RefreshTokens = db.Tokens.Where(e => e.id == rt);
                    if (RefreshTokens.Any())
                    {
                        var RefreshToken = await RefreshTokens.Where(e => e.StartTime < date && date < e.EndTime).FirstOrDefaultAsync();
                        var oidRefreshToken = await RefreshTokens.FirstOrDefaultAsync();
                        if (RefreshToken != null)
                        {
                            var mapfrontuserandweb = await db.MappingFrontUserAndWebsite.Where(e => e.UUID == RefreshToken.UUID && !e.IsDeleted).FirstOrDefaultAsync();
                            if (mapfrontuserandweb != null) {
								var user = await db.FrontUsers.Where(e => e.Id == mapfrontuserandweb.FK_UserId && !e.IsDeleted).FirstOrDefaultAsync();
								var useraccount = "";
                                if (user != null)
                                {
                                    useraccount = user.Account == null ? user.Email : user.Account;
                                    tokenItem = await NewToken(useraccount, RefreshToken.UUID, user?.Id);
                                    output.name = user?.Name;
                                }
                                else tokenItem = await NewToken(null, RefreshToken.UUID);
							}else tokenItem = await NewToken(null, RefreshToken?.UUID);
                        }
                        else tokenItem = await NewToken(null, oidRefreshToken?.UUID);
                    }else tokenItem = await NewToken();
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

            return output;
        }
        public async Task<TokenKeyItem> NewToken(string? Accont = null, Guid? UUID = null, long? UserId = null)
        {
            if (string.IsNullOrEmpty(Accont)) Accont = Guid.NewGuid().ToString();
            if (UUID == null) UUID = generateUUID();
            DateTime date = DateTime.Now;
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
        public TokenResponseDto CheckToken()
        {
            TokenResponseDto output = new TokenResponseDto();
            try
            {
                string? token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token)) httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Token", out token);
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
                    if (Guid.TryParse(jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value, out var Sid))
                    {
                        output.RefreshToken = Sid;
                    }
                    if (Guid.TryParse(jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value, out var Sub)) output.IsLogin = false;
                    else output.IsLogin = true;
                    output.name = jwtToken?.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                }
                else throw new Exception();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<Guid> GetUUID()
        {
            Guid tokenId = new Guid();
            Guid UUID = new Guid();
            var token = CheckToken();
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
        public async Task<string> CreateToken(string account, Guid secret, int expireMinutes = 30)
        {
            List<KeyValuePair<string, string>> custClaims = new List<KeyValuePair<string, string>>();
            if (account.IndexOf("@") < 0) {

                var user = db.Users.Where(e => e.Account == account).FirstOrDefault();
                if (user != null)
                {
                    custClaims.Add(new KeyValuePair<string, string>("username", user.Name));
                }
            }
            else
            {
                var user = db.FrontUsers.Where(e => e.Email == account).FirstOrDefault();
                if (user != null)
                {
                    custClaims.Add(new KeyValuePair<string, string>("username", user.Name));
                }
            }

            List<string> roles = new List<string> {
                "Admin",
                "Users"
            };
            string token = await jwt.GenerateToken(account, roles, secret, expireMinutes, custClaims);
            httpContextAccessor.HttpContext?.Response.Cookies.Append("Token", token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(15) // 設定過期時間
            });
            httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", secret.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMonths(3) // 設定過期時間
            });
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
