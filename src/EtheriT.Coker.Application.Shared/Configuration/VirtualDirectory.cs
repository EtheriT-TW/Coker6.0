using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Configuration
{
    public class VirtualDirectory
    {
        public Dictionary<string, string> UploadRoots { get; set; } = new();

        public FileAllow FileAllow { get; set; } = new();
    }
}
