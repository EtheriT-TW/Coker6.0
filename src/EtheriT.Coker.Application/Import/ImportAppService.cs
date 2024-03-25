using AutoMapper;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
		public async Task<ProdImportAllDto> ProdReplace(IList<IFormFile> files)
		{
			ProdImportAllDto output = new ProdImportAllDto ();
			UploadFileOutputDto upload = await fileUploadAppService.uploadTempFiles(files);
			if (upload.Files != null)
			{
				for (int i = 0; i < upload.Files.Count; i++)
				{
					var file = upload.Files[i];
					string path = $"{_folder.Replace("\\", "/")}{(file.Path??"").Replace("/upload", "")}";
					output.Products.AddRange(readProdExcel(path));
					output.Directories.AddRange(readDirectoryExcel(path));
					await fileUploadAppService.deleteFile(path);
				}
			}
			return output;
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
						var t = Techs.FindAll(e => e.ProdName == rows[i].ProdName);
						rows[i].Techs = mapper.Map<List<TechCertDto>>(t);
                        if(!string.IsNullOrEmpty(rows[i].Image1)) rows[i].Image1 = $"/upload/Product/{rows[i].Image1}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image2)) rows[i].Image2 = $"/upload/Product/{rows[i].Image2}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image3)) rows[i].Image3 = $"/upload/Product/{rows[i].Image3}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image4)) rows[i].Image4 = $"/upload/Product/{rows[i].Image4}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image5)) rows[i].Image5 = $"/upload/Product/{rows[i].Image5}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image6)) rows[i].Image6 = $"/upload/Product/{rows[i].Image6}".Replace("//", "/");
                        if (!string.IsNullOrEmpty(rows[i].Image7)) rows[i].Image7 = $"/upload/Product/{rows[i].Image7}".Replace("//", "/");

                        if (rows[i].Techs != null)
						{
                            rows[i].Techs.ForEach(e => {
                                if(!string.IsNullOrEmpty(e.Img)) e.Img = $"/upload/TechnicalCertificate/{e.Img}".Replace("//", "/");
                            });
                        }
                        data.Add(rows[i]);
					}
				}
			}
			catch (Exception ex) { }

			return data;
		}
		public List<DirectoryImportDto> readDirectoryExcel(string path) {
			var rows = MiniExcel.Query<DirectoryImportDto>(path, sheetName: "目錄分類", startCell: "A3").ToList();
			return rows;
		}
	}
}
