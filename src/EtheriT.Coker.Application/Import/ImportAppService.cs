using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
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
		public async Task<List<ProductImportDto>> ProdReplace(IList<IFormFile> files)
		{
			UploadFileOutputDto upload = await fileUploadAppService.uploadTempFiles(files);
			List<ProductImportDto> products = new List<ProductImportDto>();
			if (upload.Files != null)
			{
				for (int i = 0; i < upload.Files.Count; i++)
				{
					var file = upload.Files[i];
					string path = $"{_folder.Replace("\\", "/")}{(file.Path??"").Replace("/upload", "")}";
					products.AddRange(readExcel(path));
					await fileUploadAppService.deleteFile(path);
				}
			}
			return products;
		}
		private List<ProductImportDto> readExcel(string path)
		{
			List<ProductImportDto> data = new List<ProductImportDto>();
			var rows = MiniExcel.Query<ProductImportDto>(path, sheetName: "商品",startCell: "A2").ToList();
			try
			{
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i] != null)
					{
						data.Add(rows[i]);
					}
				}
			}
			catch (Exception ex) { }

			return data;
		}
	}
}
