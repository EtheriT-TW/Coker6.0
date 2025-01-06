using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
    public static class VarExtensions
    {
        public static bool IsNullOrEmpty(this Guid id)
        {
            return Guid.Empty == id;
        }
        public static bool IsNullOrEmpty(this Guid? id)
        {
            return id == null || IsNullOrEmpty(id.Value);
        }
        public static bool IsNullOrEmpty(this long id)
        {
            return 0 == id;
        }
        public static bool IsNullOrEmpty(this long? id)
        {
            return id == null || IsNullOrEmpty(id.Value);
        }
        public static bool IsNullOrEmpty(this int id)
        {
            return 0 == id;
        }
        public static bool IsNullOrEmpty(this int? id)
        {
            return id == null || IsNullOrEmpty(id.Value);
        }
        public static bool IsNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
    }
}
