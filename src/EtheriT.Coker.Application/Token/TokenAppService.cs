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
                DateTime dateTime = DateTime.Now;
                DateTime EndDateTime = dateTime.AddDays(30);
                Core.Models.Token t = new Core.Models.Token
                {
                    ip = loginUserData.GetClientIP()??"",
                    UserID = null,
                    StartTime = dateTime,
                    EndTime = EndDateTime,
                    websiteId = configuration.GetValue<long>("WebConfig:SiteId")
                };
                db.Tokens.Add(t);
                db.SaveChanges();
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
        public async Task<TokenResponseDto> CheckToken(Guid? id)
        {
            TokenResponseDto output = new TokenResponseDto();
            try
            {
                if (id == null) throw new Exception("Token type error");
                db.Tokens.RemoveRange(db.Tokens.Where(e => e.EndTime < DateTime.Now || e.websiteId == 0));
                db.SaveChanges();
                var tokens = db.Tokens.Where(e => e.id == id).Where(e => e.EndTime > DateTime.Now).First();
                if (tokens == null)
                {
                    output.Success = false;
                    output.Error = "Token doesn't exist";
                }
                else
                {
                    var token = await CreateToken();
                    var newToken = db.Tokens.Where(e => e.id == Guid.Parse(token.Token??"")).First();
                    newToken.UserID = tokens.UserID;
                    newToken.UUID = tokens.UUID;
                    mapper.Map(output, token);
                    output.IsLogin = tokens.UserID != null;
                    output.Token = token.Token;
                    output.Success = true;
                    if (output.IsLogin)
                        output.name = db.Users.Where(e => e.Status == (int)UserStatus.開通 && !e.IsDeleted).FirstOrDefault()?.Name;
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
            var user = db.Users.Where(e => e.Account == account).First();
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
    }
}
