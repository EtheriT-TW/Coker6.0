using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType
{
    public enum ErrorCodeEnum
    {
        None = 0,

        // 400
        ValidationError = 1001,
        BadRequest = 1002,

        // 401 / 403
        Unauthorized = 2001,
        Forbidden = 2002,

        // 404
        NotFound = 3001,

        // 409
        Conflict = 4001,

        // 500
        ServerError = 5000
    }
}
