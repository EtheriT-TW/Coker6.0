using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
	public class DirectoryArrangeImportDto
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public List<DirectoryArrangeImportDto> Child { get; set; } = new List<DirectoryArrangeImportDto>();
		public List<TagGetSelectedDto>? Tags { get; set; }
	}
}
