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
                string? token = null;
                string? RefreshToken = null;
                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Token", out token);
                if (string.IsNullOrEmpty(token))
                {
                    output = await CheckToken();
                    if (output.Success) return output;
                }
                DateTime dateTime = DateTime.Now;
                DateTime EndDateTime = dateTime.AddDays(30);
                Core.Models.Token t = new Core.Models.Token
                {
                    ip = loginUserData.GetClientIP()??"",
                    UserID = null,
                    UUID = generateUUID(),
                    StartTime = dateTime,
                    EndTime = EndDateTime,
                    websiteId = configuration.GetValue<long>("WebConfig:SiteId")
                };

                httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("RefreshToken", out token);
                if (!string.IsNullOrEmpty(RefreshToken)) {
                    if (Guid.TryParse(RefreshToken, out Guid rt)) {
                        var oldTokenn = db.Tokens.Where(e => e.id == rt).FirstOrDefault();
                        if (oldTokenn != null) { 
                            t.UUID = oldTokenn.UUID;
                            t.UserID = oldTokenn.UserID;
                        }
                    }
                }
                db.Tokens.Add(t);
                await db.SaveChangesAsync();
                output.Success = true;
                output.Token = t.id.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<TokenResponseDto> CheckToken()
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
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration.GetValue<string>("JwtSettings:SignKey"))),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                    handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                    var jwtToken = validatedToken as JwtSecurityToken;
                    output.Success = true;
                    output.name = jwtToken?.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                }
                else throw new Exception();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = "Token is invalid";
            }

            return output;
        }
        public async Task<TokenResponseDto> RefreshToken(Guid? id) {
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
                        var user = db.Users.Where(e => e.Status == (int)UserStatus.開通 && !e.IsDeleted && e.Id == tokens.UserID);
                        output.name = db.Users.Where(e => e.Status == (int)UserStatus.開通 && !e.IsDeleted).FirstOrDefault()?.Name;
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
        public async Task<string> CreateToken(string account,Guid secret)
        {
            var user = db.Users.Where(e => e.Account == account).FirstOrDefault();
            if (user == null)
            {
            }
            List<string> roles = new List<string> {
                "Admin",
                "Users"
            };
            return await jwt.GenerateToken(account, roles, secret);
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
        private Guid generateUUID() { 
            Guid id = Guid.NewGuid();
            var c = db.Tokens.Where(e => e.UUID == id);
            if (c.Any()) return generateUUID();
            else {
                var u = db.Users.Where(e => !e.IsDeleted && e.UUID == id);
                if (u.Any()) return generateUUID();
                return id;
            }
        }
    }
}
