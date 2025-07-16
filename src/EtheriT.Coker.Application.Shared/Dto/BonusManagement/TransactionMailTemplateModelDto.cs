using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class TransactionMailTemplateModelDto
    {
        public string? MemberName { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public long MemberId { get; set; }
        public string? TransactionOperation { get; set; }
        public string? TransactionOperationName { get; set; }
        public int TransactionPoint { get; set; }
        public string? TransactionDescription { get; set; }
        public int TransactionBalance { get; set; }
        public DateTime RewardPointsExpireDateTime { get; set; }
        public string? WebSiteName { get; set; }
        public string? WebSiteUrl { get; set; }
    }
}
