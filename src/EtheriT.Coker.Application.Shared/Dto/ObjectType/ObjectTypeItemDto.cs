using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.ObjectType
{
    public class ObjectTypeItemDto: PowerOptionDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Text { get { return Title; } }
        public string? icon { get; set; }
        public long? FK_TopNodeId { get; set; }
        public List<ObjectTypeItemDto>? Children { get; set; }
    }
}
