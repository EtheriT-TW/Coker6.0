using AutoMapper;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Http;
using MiniExcelLibs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Import
{
    public class ImportAppService
    {
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IUploadPathResolver uploadPathResolver;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly ConcurrentDictionary<(Type, string), PropertyInfo[]> _propCache = new();
        public ImportAppService(
            IFileUploadAppService fileUploadAppService,
            IUploadPathResolver uploadPathResolver,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.fileUploadAppService = fileUploadAppService;
            this.uploadPathResolver = uploadPathResolver;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }
        public async Task<ProdImportAllDto> ProdReplace(IList<IFormFile> files)
        {
            ProdImportAllDto output = new ProdImportAllDto();
            UploadFileOutputDto upload = await fileUploadAppService.uploadTempFiles(files);
            if (upload.Files != null)
            {
                for (int i = 0; i < upload.Files.Count; i++)
                {
                    var file = upload.Files[i];
                    var orgName = await loginUserData.GetWebsiteOrgName();
                    string path = uploadPathResolver.GetPhysicalPathFromDownloadFileName(orgName, file.Path ?? "");
                    var fileData = await ProdReplace(path);
                    output.Products.AddRange(fileData.Products);
                    output.Directories.AddRange(fileData.Directories);
                    await fileUploadAppService.deleteFile(path);
                }
            }
            return output;
        }
        public async Task<ProdImportAllDto> ProdReplace(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException("找不到商品匯入檔案。", path);

            var orgName = await loginUserData.GetWebsiteOrgName();
            var output = new ProdImportAllDto();
            output.Products.AddRange(readProdExcel(path, orgName));
            output.Directories.AddRange(readDirectoryExcel(path));
            return output;
        }
        private List<ProductImportDto> readProdExcel(string path, string orgName)
        {
            List<ProductImportDto> data = new List<ProductImportDto>();
            var importedColumns = MiniExcel.Query(
                    path,
                    useHeaderRow: true,
                    sheetName: "商品",
                    startCell: "A2")
                .Cast<IDictionary<string, object>>()
                .FirstOrDefault()?
                .Keys
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var reg = MiniExcel.Query<ProductImportUpateRegDto>(path, sheetName: "商品", startCell: "A2").ToList();
            var rows = mapper.Map<List<ProductImportDto>>(reg);
            var Techs = MiniExcel.Query<TechCertImportDto>(path, sheetName: "技術證照", startCell: "A2").ToList();
            foreach (var row in rows)
            {
                if (row != null)
                {
                    row.ImportedColumns = new HashSet<string>(importedColumns, StringComparer.OrdinalIgnoreCase);
                    NormalizeProductImportRow(row);
                }
            }
            foreach (var tech in Techs)
            {
                if (tech != null) NormalizeTechnicalCertificateRow(tech);
            }
            try
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i] != null)
                    {
                        var t = Techs.FindAll(e => e.ProdName == rows[i].ProdName && e.ItemNo == rows[i].ItemNo);
                        rows[i].Techs = mapper.Map<List<TechCertDto>>(t);
                        pathReplace(rows[i], "Image", "Product", orgName);
                        pathReplace(rows[i], "File", "Product/File", orgName);
                        if (rows[i].Techs != null)
                        {
                            rows[i].Techs.ForEach(e => {
                                pathReplace(e, "Img", "TechnicalCertificate", orgName);
                            });
                        }
                        data.Add(rows[i]);
                    }
                }
            }
            catch (Exception ex) { }

            return data;
        }
        private static void NormalizeProductImportRow(ProductImportDto row)
        {
            row.ProdName = CustomDtoMapper.Normalize(row.ProdName);
            row.Status = CustomDtoMapper.Normalize(row.Status);
            row.ItemNo = CustomDtoMapper.Normalize(row.ItemNo);
            row.SubItemNo = CustomDtoMapper.Normalize(row.SubItemNo);
            row.Visible = CustomDtoMapper.Normalize(row.Visible);
            row.OnShelf = CustomDtoMapper.Normalize(row.OnShelf);
            row.Spec1Name = CustomDtoMapper.Normalize(row.Spec1Name);
            row.Spec1 = CustomDtoMapper.Normalize(row.Spec1);
            row.Spec2Name = CustomDtoMapper.Normalize(row.Spec2Name);
            row.Spec2 = CustomDtoMapper.Normalize(row.Spec2);
            row.RoleName = CustomDtoMapper.Normalize(row.RoleName);
            row.Tag1 = CustomDtoMapper.Normalize(row.Tag1);
            row.Tag2 = CustomDtoMapper.Normalize(row.Tag2);
            row.Tag3 = CustomDtoMapper.Normalize(row.Tag3);
            row.Tag4 = CustomDtoMapper.Normalize(row.Tag4);
            row.Tag5 = CustomDtoMapper.Normalize(row.Tag5);
            row.Tag6 = CustomDtoMapper.Normalize(row.Tag6);
            row.FileName1 = CustomDtoMapper.Normalize(row.FileName1);
            row.FileName2 = CustomDtoMapper.Normalize(row.FileName2);
            row.FileName3 = CustomDtoMapper.Normalize(row.FileName3);
            row.FileName4 = CustomDtoMapper.Normalize(row.FileName4);
            row.FileName5 = CustomDtoMapper.Normalize(row.FileName5);
            row.FileName6 = CustomDtoMapper.Normalize(row.FileName6);
            row.FileName7 = CustomDtoMapper.Normalize(row.FileName7);
        }

        private static void NormalizeTechnicalCertificateRow(TechCertImportDto row)
        {
            row.ProdName = CustomDtoMapper.Normalize(row.ProdName);
            row.ItemNo = CustomDtoMapper.Normalize(row.ItemNo);
            row.Title = CustomDtoMapper.Normalize(row.Title);
        }

        private static void NormalizeDirectoryRow(DirectoryImportDto row)
        {
            row.Level1 = CustomDtoMapper.Normalize(row.Level1);
            row.Level1RouterName = CustomDtoMapper.Normalize(row.Level1RouterName);
            row.Level2 = CustomDtoMapper.Normalize(row.Level2);
            row.Level2RouterName = CustomDtoMapper.Normalize(row.Level2RouterName);
            row.Level3 = CustomDtoMapper.Normalize(row.Level3);
            row.Level3RouterName = CustomDtoMapper.Normalize(row.Level3RouterName);
            row.Tag1 = CustomDtoMapper.Normalize(row.Tag1);
            row.Tag2 = CustomDtoMapper.Normalize(row.Tag2);
            row.Tag3 = CustomDtoMapper.Normalize(row.Tag3);
        }
        private PropertyInfo[] GetProps<T>(string keyPrefix) where T : class
        {
            var k = (typeof(T), keyPrefix.ToLowerInvariant());
            return _propCache.GetOrAdd(k, _ =>
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanRead && p.CanWrite
                                  && p.PropertyType == typeof(string)
                                  && p.Name.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                         .ToArray()
            );
        }
        private void pathReplace<T>(T dto, string key, string dir, string orgName) where T : class
        {
            var props = GetProps<T>(key);
            foreach (var prop in props)
            {
                string? path = prop.GetValue(dto) as string;
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                path = path.Trim();
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    continue;

                var orgPrefix = $"/upload/{orgName}/";
                if (!string.IsNullOrWhiteSpace(orgName) &&
                    path.StartsWith(orgPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = $"/upload/{path.Substring(orgPrefix.Length)}";
                }
                else if (!path.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase))
                {
                    path = $"/upload/{dir}/{path}".Replace("//", "/");
                }

                prop.SetValue(dto, path);
            }
        }
        public List<DirectoryImportDto> readDirectoryExcel(string path)
        {
            var rows = MiniExcel.Query<DirectoryImportDto>(path, sheetName: "目錄分類", startCell: "A3").ToList();
            foreach (var row in rows)
            {
                if (row != null) NormalizeDirectoryRow(row);
            }
            return rows;
        }
    }
}
