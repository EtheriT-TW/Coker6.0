using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Import
{
	public class ImportOutputDto: ResponseMessageDto
	{
		public List<ImportMassageItem> ErrorList { get;set; }
		public ProductImportSummaryDto Summary { get; set; } = new ProductImportSummaryDto();

		public static implicit operator ImportOutputDto(UploadFileOutputDto v)
		{
			throw new NotImplementedException();
		}
	}

	public class ProductImportSummaryDto
	{
		public List<string> DetectedUpdateScopes { get; set; } = new List<string>();
		public int ProductRowCount { get; set; }
		public int ProductCount { get; set; }
		public int ProductAddedCount { get; set; }
		public int ProductUpdatedCount { get; set; }
		public int ProductBeforeCount { get; set; }
		public int ProductAfterCount { get; set; }

		public int DirectoryRowCount { get; set; }
		public int MenuCount { get; set; }
		public int MenuAddedCount { get; set; }
		public int MenuExistingCount { get; set; }
		public int MenuBeforeCount { get; set; }
		public int MenuAfterCount { get; set; }
	}
}
