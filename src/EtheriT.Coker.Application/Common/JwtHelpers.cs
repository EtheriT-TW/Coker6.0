using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Web.MVC.Resources
{
    public class JwtHelpers
    {
        //private readonly CokerDbContext db;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public JwtHelpers(
            //CokerDbContext db,
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor)
        {
            //this.db = db;
            this.Configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GenerateToken(string Account, List<string> roles, Guid secret, int expireMinutes = 30,List<KeyValuePair<string, string>>? custClaims = null)
        {
            //var user = await db.Users.Where(e => e.Account == Account).FirstOrDefaultAsync();
            var issuer = Configuration.GetValue<string>("JwtSettings:Issuer");
            var audience = Configuration.GetValue<string>("JwtSettings:Audience") ?? issuer;
            var signKey = Configuration.GetValue<string>("JwtSettings:SignKey");

            // Configuring "Claims" to your JWT Token
            var claims = new List<Claim>();

            // In RFC 7519 (Section#4), there are defined 7 built-in Claims, but we mostly use 2 of them.
            //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, Account)); // User.Identity.Name
                                                                          //claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "The Audience"));
                                                                          //claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()));
                                                                          //claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
                                                                          //claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID

            // The "NameId" claim is usually unnecessary.
            //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userName));

            // This Claim can be replaced by JwtRegisteredClaimNames.Sub, so it's redundant.
            claims.Add(new Claim(ClaimTypes.Name, Account));

            claims.Add(new Claim(ClaimTypes.Sid, secret.ToString()));
            claims.Add(new Claim("secret", secret.ToString()));
            //claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Account));
            if (custClaims != null) {
                custClaims.ForEach(e => {
					claims.Add(new Claim(e.Key, e.Value));
				});

			}
            // TODO: You can define your "roles" to your Claims.
            roles.ForEach(e => {
                claims.Add(new Claim("roles", e));
            });

            var userClaimsIdentity = new ClaimsIdentity(claims);

            // Create a SymmetricSecurityKey for JWT Token signatures
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

            // HmacSha256 MUST be larger than 128 bits, so the key can't be too short. At least 16 and more characters.
            // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Create SecurityTokenDescriptor
            var now = DateTime.Now;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience, // Sometimes you don't have to define Audience.
                NotBefore = now, // 何時開始生效
                IssuedAt = now, // Default is DateTime.Now
                Subject = userClaimsIdentity,
                Expires = now.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials
            };

            // Generate a JWT securityToken, than get the serialized Token result (string)
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            // also add cookie auth for Swagger Access
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Account));
            identity.AddClaim(new Claim(ClaimTypes.Name, Account));
            var principal = new ClaimsPrincipal(identity);
            await httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1)
                });

            return serializeToken;
        }
    }
}
