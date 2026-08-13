using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Import
{
	public class ImportMassageItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Sheet { get; set; } = string.Empty;
		public List<int> RowNumbers { get; set; } = new();
		public List<ImportMassageComparisonValue> ComparisonValues { get; set; } = new();
		public bool CanIgnore { get; set; }
	}

	public class ImportMassageComparisonValue
	{
		public int RowNumber { get; set; }
		public string Label { get; set; } = string.Empty;
		public string Value { get; set; } = string.Empty;
	}
}
