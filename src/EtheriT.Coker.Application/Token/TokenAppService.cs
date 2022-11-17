using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Token
{
    public class TokenAppService: ITokenAppService
    {
        private readonly JwtHelpers jwt;
        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDistributedCache cache;
        public TokenAppService(
            JwtHelpers jwt, 
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache)
        {
            this.jwt = jwt;
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
        }
        public async Task<string> CreateToken(string account) {
            var user = db.Users.Where(e => e.Account == account).First();
            if (user == null) {
            }
            List<string> roles = new List<string> { 
                "Admin",
                "Users"
            };
            return await jwt.GenerateToken(account, roles);
        }
        public async Task<bool> DelToken() {
            string token = GetCurrentAsync();
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
        private string GetCurrentAsync()
        {
            var authorizationHeader = httpContextAccessor
                .HttpContext.Request.Headers["Authorization"];

            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last();
        }
        private static string GetKey(string token)
        {
            return $"tokens:{token}:deactivated";
        }
    }
}
