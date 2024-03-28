using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
	public class FileImageImportDto
	{
		public long? Id { get; set; }
		public long SId { get; set; }
		public FileBindTypeEnum Type { get; set; }
		public string mediaLink { get; set; }
        public string? Name { get; set; } = string.Empty;
        public int SerNo { get; set; }
	}
}
