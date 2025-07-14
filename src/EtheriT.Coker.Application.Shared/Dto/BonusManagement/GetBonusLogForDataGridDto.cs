using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetBonusLogForDataGridDto
    {
        /// <summary>
        /// 異動紀錄 UUID (格式: Guid)
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// 會員-帳號
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 會員-名稱
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 日期 (格式: yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// 異動紅利 (正負數值)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 備註：異動原因
        /// </summary>
        public string? Note { get; set; }
    }
}
