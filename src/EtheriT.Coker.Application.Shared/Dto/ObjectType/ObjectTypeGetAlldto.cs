using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.ObjectType
{
    public class ObjectTypeGetAlldto: ResponseMessageDto
    {
        public List<ObjectTypeItemDto> List { get; set; }
    }
}
