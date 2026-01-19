using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class SendOpeningDto
    {
        public Guid? OpenId { get; set; }
        public DateTime? OpenIdSendDate { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public long WebsiteId { get; set; }
        public string WebsiteLink { get; set; }
        public string WebsiteName { get; set; }
        public string BonusText { get; set; }
    }
}
