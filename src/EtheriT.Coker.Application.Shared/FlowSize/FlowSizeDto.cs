using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.FlowSize
{
	public class FlowSizeDto
	{
		public long WebsiteId { get; set; }        // 站台 ID
		public string WebsiteName { get; set; }    // 站台名稱
		public long RequestSize { get; set; }       // 請求大小
		public long ResponseSize { get; set; }      // 回應大小
		public long Total { get; set; }             // 總流量
		public DateTime ActionTime { get; set; }   // 最後統計時間
	}
}
