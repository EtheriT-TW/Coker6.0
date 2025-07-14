using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class CreateUserTransactionDto
    {
        public List<Guid> MemberUUID { get; set; } = new List<Guid>();
        public string? TransactionOperation { get; set; }
        public int TransactionPoint { get; set; }
        public string? TransactionReason { get; set; }

        /// <summary>
        /// 是否寄送紅利異動通知信件(預設啟用)
        /// </summary>
        public bool IsSendMail { get; set; } = true;
    }
}
