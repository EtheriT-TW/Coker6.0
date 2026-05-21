using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Advertise
{
    public enum AdvertiseActionType
    {
        Link = 1,        // 點擊圖片後開啟連結
        ExpandHtml = 2,  // 點擊圖片後展開 Html
        None = 3         // 單純顯示，不做點擊行為
    }
}
