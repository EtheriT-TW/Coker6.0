
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Shared.Dto.Tag
{
    public class TagAssociateDto
    {
        public long? Id { get; set; }
        public long FK_TId { get; set; }
        public long FK_AId { get; set; }
        public bool IsDeleted { get; set; }
        public TagAssociateTypeEnum Type { get; set; }
    }
}
