using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.OAuth
{
    public enum CookiePurposeEnum
    {
        Default,
        FrontAuthToken,      // 前台 JWT + Token cookie
        BackstageAuthToken,  // 後台 JWT + BackstageToken cookie
        RefreshToken,        // 共用的 RefreshToken（DB Token.id）
        RefreshIdentifier, 
        XsrfToken,
        Language,
        TempData,
        ShortTerm,
        LongTerm
    }

}
