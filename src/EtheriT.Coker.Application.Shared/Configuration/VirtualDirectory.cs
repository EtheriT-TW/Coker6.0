using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Configuration
{
    public class VirtualDirectory
    {
        // 前台單站模式：此路徑已直接指向該網站的 Upload 目錄。
        public string? Upload { get; set; }

        // 後台多站模式：每個根目錄底下再依 OrgName 區分網站。
        public Dictionary<string, string> UploadRoots { get; set; } = new();

        public FileAllow FileAllow { get; set; } = new();
    }
}
