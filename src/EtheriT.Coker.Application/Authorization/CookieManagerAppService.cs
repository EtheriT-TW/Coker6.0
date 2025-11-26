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
        public TimeSpan GetLifetime(CookiePurposeEnum purpose)
        {
            return purpose switch
            {
                CookiePurposeEnum.FrontAuthToken => TimeSpan.FromMinutes(15),  // 前台登入 15 分鐘
                CookiePurposeEnum.BackstageAuthToken => TimeSpan.FromMinutes(30),  // 後台登入 30 分鐘
                CookiePurposeEnum.RefreshToken => TimeSpan.FromDays(30),     // RefreshToken 30 天
                CookiePurposeEnum.RefreshIdentifier => TimeSpan.FromDays(90),
                CookiePurposeEnum.XsrfToken => TimeSpan.FromMinutes(15),
                CookiePurposeEnum.Language => TimeSpan.FromDays(365),
                CookiePurposeEnum.ShortTerm => TimeSpan.FromMinutes(5),
                CookiePurposeEnum.LongTerm => TimeSpan.FromDays(30),
                CookiePurposeEnum.none => TimeSpan.Zero,
                _ => TimeSpan.FromMinutes(10),
            };
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

            if(purpose != CookiePurposeEnum.none)
                options.Expires = DateTimeOffset.Now.Add(GetLifetime(purpose));
            return options;
        }
    }
}
