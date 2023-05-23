using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Import
{
	public class ImportAppService
	{
		private readonly IFileUploadAppService fileUploadAppService;
		private readonly string _folder;
		public ImportAppService(IFileUploadAppService fileUploadAppService, IOptions<VirtualDirectory> VirtualDirectory)
		{
			this.fileUploadAppService = fileUploadAppService;
			_folder = VirtualDirectory.Value.upload;
		}
		public async Task<ImportOutputDto> ProdReplace(IList<IFormFile> files)
		{
			ImportOutputDto response=new ImportOutputDto { ErrorList=new List<ImportMassageItem>()};
			UploadFileOutputDto upload = await fileUploadAppService.uploadTempFiles(files);
			if (upload.Files != null) {
				for (int i = 0; i < upload.Files.Count; i++)
				{
					var file = upload.Files[i];
					string path = $"{_folder.Replace("\\upload", "").Replace("\\","/")}{file.Path}";
					response.ErrorList.AddRange(readExcel(path));
				}
			}
			
			return response;
		}
		private List<ImportMassageItem> readExcel(string path) {
			List<ImportMassageItem> errorList= new List<ImportMassageItem>();
			var rows = MiniExcel.Query(path,sheetName: "商品").ToList();
			var columns = MiniExcel.GetColumns(path); // e.g result : ["A","B"...]
			var cnt = columns.Count;  // get column count
			try
			{
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i] != null)
					{
						errorList.Add(new ImportMassageItem { Name = rows[i].A });
					}
				}
			}
			catch (Exception ex) { }
			
			return errorList;
		}
	}
}
