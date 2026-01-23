using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
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
        /// <summary>
        /// 增加或減少紅利點數
        /// '+' or '-'
        /// </summary>
        public string? TransactionOperation { get; set; }
        public int TransactionPoint { get; set; }
        public string? TransactionReason { get; set; }

        /// <summary>
        /// 是否寄送紅利異動通知信件(預設啟用)
        /// </summary>
        public bool IsSendMail { get; set; } = true;
        public long? RefKey { get; set; }
        public BonusLogTypeEnum Type { get; set; }
        public bool EnableIdempotencyByRefKey { get; set; } = false;
    }
}
