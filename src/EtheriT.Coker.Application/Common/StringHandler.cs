using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtheriT.Coker.Application.Common
{
    public class StringHandler
    {
        public string HtmlDecode(string? html) {
            string output;
            if (html == null) output = "";
            else
            {
                output = HttpUtility.HtmlDecode(html);
                if (output.IndexOf("&amp;") >= 0)
                {
                    output = HtmlDecode(output);
                }
            }
            return output;
        }
        public string HtmlEncode(string? html)
        {
            string output = HtmlDecode(html);
            return HttpUtility.HtmlEncode(output);
        }
    }
}
