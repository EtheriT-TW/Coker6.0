using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Import
{
	public class ProdImportAllDto
	{
		public List<ProductImportDto> Products { get; set; } = new List<ProductImportDto>();
		public List<DirectoryImportDto> Directories { get; set; } = new List<DirectoryImportDto>();
	}
}
