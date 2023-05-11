using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class FileUploadAppService : IFileUploadAppService
    {
        private readonly CokerDbContext db;
        public readonly FileAllow fileAllow;
        private readonly LoginUserData loginUserData;
        private readonly string _folder;
        private readonly string AppName;
        public FileUploadAppService(
            IOptions<VirtualDirectory> fileAllow,
            LoginUserData loginUserData,
            CokerDbContext db
        )
        {
            this.fileAllow = fileAllow.Value.FileAllow;
            this.db = db;
            this.loginUserData = loginUserData;
            _folder = fileAllow.Value.upload;
            AppName = "FileUpload";
        }
        public async Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                foreach (var file in files)
                {
                    FileItemDto item = await SaveFile(file, "htmlConten");
                    response.Files.Add(item);
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorFiles.Add(ex.Message);
            }
            await loginUserData.SetLogs(AppName, "uploadProdtFiles", "FileBinary...", JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<UploadFileOutputDto> uploadProdtFiles(IList<IFormFile> files, long id)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                foreach (var file in files)
                {
                    FileItemDto item = await SaveFile(file, "Prod");
                    response.Files.Add(item);
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorFiles.Add(ex.Message);
            }
            return response;
        }
        public async Task<UploadFileOutputDto> getHtmlContentFiles()
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>()
            };
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                string orgName = await loginUserData.GetWebsiteOrgName();
                var files = db.FileUploads
                            .Where(e => e.FK_WebsiteId == websiteId)
                            .Where(e => !e.IsDeleted)
                            .Where(e => (e.DownloadFileName ?? "").Contains("htmlConten"));
                var result = from file in files
                             select new FileItemDto
                             {
                                 Guid = file.GuidKey,
                                 Name = file.OriginalFileName,
                                 Path = (file.DownloadFileName ?? "").Replace(@"\", "/").Replace("/upload/", $"/upload/{orgName}/"),
                             };
                response.Files = await result.ToListAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> deleteFile(Guid key)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                string orgName = await loginUserData.GetWebsiteOrgName();
                long websiteId = await loginUserData.GetWebsiteId();
                string s = "";
                var rootPath = $"{_folder}/{orgName}";
                var files = await db.FileUploads.Where(e => e.GuidKey == key).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (files != null)
                {
                    s = files.DownloadFileName.Replace(@"\", "/").Replace("/upload", "");
                    File.Delete($"{rootPath}{s}");
                    files.IsDeleted = true;
                    response.Success = true;
                    await loginUserData.SaveChanges(files);
                }
                else throw new Exception("檔案不存在");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(AppName, "deleteFile", key.ToString(), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<string> getImgUrl(long? imgid, long websiteid)
        {
            try
            {
                var files = await (db.FileUploads
                            .Where(e => e.FK_WebsiteId == websiteid)
                            .Where(e => e.Id == imgid)
                            .Where(e => !e.IsDeleted)
                            .Select(e => e.DownloadFileName).FirstOrDefaultAsync());
                return files;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private async Task<FileItemDto> SaveFile(IFormFile file, string directory)
        {
            if (file.Length > 0)
            {
                string orgName = await loginUserData.GetWebsiteOrgName();
                Guid key = Guid.NewGuid();
                string[] sp = file.FileName.Split('.');
                string ext = sp[sp.Length - 1];
                var rootPath = $"{_folder}/{orgName}";
                var directoryPath = $"{rootPath}/{directory}";
                var path = $"/{directory}/{key}.{ext}";
                if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception();
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                using (var stream = new FileStream($"{rootPath}{path}", FileMode.Create))
                {
                    FileUpload fileUpload = new FileUpload
                    {
                        FK_WebsiteId = await loginUserData.GetWebsiteId(),
                        FileGuid = key,
                        GuidKey = Guid.NewGuid(),
                        DownloadFileName = $@"/upload{path}",
                        OriginalFileName = file.FileName,
                        ContentType = file.ContentType,
                        Size = file.Length
                    };
                    await file.CopyToAsync(stream);
                    db.FileUploads.Add(fileUpload);
                    await loginUserData.SaveChanges(fileUpload);
                    return new FileItemDto
                    {
                        Name = fileUpload.OriginalFileName,
                        Path = fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
                        Guid = fileUpload.GuidKey,
                    };
                }
            }
            else throw new Exception("上傳失敗");
        }
    }
}
