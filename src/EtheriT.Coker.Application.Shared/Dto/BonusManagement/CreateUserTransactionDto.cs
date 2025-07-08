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
        public bool IsSendMail { get; set; } = false;
    }
}
