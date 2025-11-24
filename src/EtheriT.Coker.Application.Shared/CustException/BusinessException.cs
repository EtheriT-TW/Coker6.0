using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.CustException
{
    public class BusinessException: Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
