using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductImportDto: ProductImportUpateDto
	{
		public long Id { get; set; } = 0;
		public decimal Price { get; set; } = 0;
		public HashSet<string> ImportedColumns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
		public int SourceRowNumber { get; set; }
	}
}
