using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorization
{
    public class CookieManagerAppService : ICookieManagerAppService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CookieManagerAppService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public void Set(string key, string value, CookiePurposeEnum purpose = CookiePurposeEnum.Default)
        {
            var options = GetOptions(purpose);
            _contextAccessor.HttpContext?.Response.Cookies.Append(key, value, options);
        }

        public string Get(string key)
        {
            return _contextAccessor.HttpContext?.Request.Cookies[key]??"";
        }

        public void Delete(string key)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(key);
        }
        public void Clear(params string[] exceptKeys)
        {
            var request = _contextAccessor.HttpContext?.Request;
            var response = _contextAccessor.HttpContext?.Response;

            if (request?.Cookies == null || response == null) return;

            foreach (var cookie in request.Cookies.Keys)
            {
                if (exceptKeys.Contains(cookie)) continue;
                response.Cookies.Delete(cookie);
            }
        }
        private CookieOptions GetOptions(CookiePurposeEnum purpose)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };

            options.Expires = purpose switch
            {
                CookiePurposeEnum.AuthToken => DateTimeOffset.UtcNow.AddMinutes(30),
                CookiePurposeEnum.RefreshToken => DateTimeOffset.UtcNow.AddDays(7),
                CookiePurposeEnum.RefreshIdentifier => DateTimeOffset.UtcNow.AddMonths(3),
                CookiePurposeEnum.XsrfToken => DateTimeOffset.UtcNow.AddMinutes(15),
                CookiePurposeEnum.Language => DateTimeOffset.UtcNow.AddYears(1),
                CookiePurposeEnum.ShortTerm => DateTimeOffset.UtcNow.AddMinutes(5),
                CookiePurposeEnum.LongTerm => DateTimeOffset.UtcNow.AddDays(30),
                _ => DateTimeOffset.UtcNow.AddMinutes(10),
            };

            return options;
        }
    }
}
