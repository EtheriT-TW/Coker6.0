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
        AuthToken,
        RefreshToken,
        RefreshIdentifier,
        XsrfToken,
        Language,
        TempData,
        ShortTerm,
        LongTerm
    }
}
