using EtheriT.Coker.Application.Shared.Dto.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.WebMenu
{
    public class PageTypeOptionDto : EnumDictionaryDto
    {
        /// <summary>
        /// 此頁面類型對應的系統預設路由名稱。
        /// 空值表示沒有系統預設路由。
        /// </summary>
        public string? RouterName { get; set; }

        /// <summary>
        /// 是否顯示路徑名稱欄位。
        /// </summary>
        public bool ShowRouterName { get; set; }

        /// <summary>
        /// 是否顯示外部連結欄位。
        /// </summary>
        public bool ShowLinkUrl { get; set; }
    }
}
