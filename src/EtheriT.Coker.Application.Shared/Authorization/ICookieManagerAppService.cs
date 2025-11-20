using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;

namespace EtheriT.Coker.Application.Shared.Authorization
{
    public interface ICookieManagerAppService
    {
        void Set(string key, string value, CookiePurposeEnum purpose = CookiePurposeEnum.Default);
        string Get(string key);
        void Delete(string key);
        void Clear(params string[] exceptKeys);
        TimeSpan GetLifetime(CookiePurposeEnum purpose);
    }
}
