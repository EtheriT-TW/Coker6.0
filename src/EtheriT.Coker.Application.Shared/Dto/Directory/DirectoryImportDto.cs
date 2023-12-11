using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
	public class DirectoryImportDto
	{
		public string Level1 { get; set; }
		public string? Level2 { get; set; }
		public string? Level3 { get; set; }
		public string Tag1 { get; set; }
		public string? Tag2 { get; set; }
		public string? Tag3 { get; set; }
	}
}
