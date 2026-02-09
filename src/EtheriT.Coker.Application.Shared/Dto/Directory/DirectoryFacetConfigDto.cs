using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryFacetConfigDto
    {
        public long DirectoryId { get; set; }
        public DirectoryFacetTypeEnum FacetType { get; set; } = DirectoryFacetTypeEnum.None;
        public DirectoryCalendarTypeEnum CalendarType { get; set; } = DirectoryCalendarTypeEnum.西元年;
        public bool AutoBucketUncovered { get; set; } = true;
        public List<DirectoryFacetRangeDto> Ranges { get; set; } = new();
    }

}
