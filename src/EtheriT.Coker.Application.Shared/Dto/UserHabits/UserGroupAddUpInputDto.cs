using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.UserHabits
{
    public class UserGroupAddUpInputDto : ResponseObject
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Enable { get; set; }
        public List<TagSelectedDto> Tags { get; set; } = new List<TagSelectedDto>();
    }
}
