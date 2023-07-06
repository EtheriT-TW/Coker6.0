using AutoMapper;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
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
		private readonly IMapper mapper;
		public ImportAppService(
			IFileUploadAppService fileUploadAppService, 
			IOptions<VirtualDirectory> VirtualDirectory,
			IMapper mapper
		){
			this.fileUploadAppService = fileUploadAppService;
			_folder = VirtualDirectory.Value.upload;
			this.mapper = mapper;
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
					products.AddRange(readProdExcel(path));
					await fileUploadAppService.deleteFile(path);
				}
			}
			return products;
		}
		private List<ProductImportDto> readProdExcel(string path)
		{
			List<ProductImportDto> data = new List<ProductImportDto>();
			var rows = MiniExcel.Query<ProductImportDto>(path, sheetName: "商品",startCell: "A2").ToList();
			var Techs = MiniExcel.Query<TechCertImportDto>(path, sheetName: "技術證照", startCell: "A2").ToList(); 
			try
			{
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i] != null)
					{
						var t = Techs.FindAll(e => e.ProdName == rows[i].Title);
						rows[i].Techs = mapper.Map<List<TechCertDto>>(t);
						data.Add(rows[i]);
					}
				}
			}
			catch (Exception ex) { }

			return data;
		}
	}
}
