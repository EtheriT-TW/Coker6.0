using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Shared.Dto.Recipients
{
    public class RecipientsDto
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Cellphone { get; set; }
        public string Telephone { get; set; }
        public SexEnum Sex { get; set; }
        public long FK_WebsiteId { get; set; }
    }
}
