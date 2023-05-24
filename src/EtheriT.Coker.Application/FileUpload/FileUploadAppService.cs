using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        public async Task<UploadFileOutputDto> uploadImageFiles(IList<IFormFile> files, int type, long sid, string page)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                List<FileItemDto> items = await SaveImage(files, type, page, sid);
                foreach (var item in items)
                {
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
        public async Task<ResponseMessageDto> deleteImgBySId(FileGetImgInputDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            response.Success = true;

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();

                var imgdatas = await db.FileBinds.Where(e => e.Sid == dto.Sid && e.type == dto.Type && !e.IsDeleted).ToListAsync();

                foreach (var imgdata in imgdatas)
                {
                    var delete_response = await deleteImgByImgId(imgdata.FK_FileUploadId);
                    if (!delete_response.Success)
                    {
                        response.Success = false;
                    }
                }

            }
            catch (Exception e)
            {
                response.Error = e.Message;
                response.Success = false;
            }

            return response;
        }
        public async Task<ResponseMessageDto> deleteImgByImgId(long? imgid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            response.Success = true;

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();

                var faimg = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == imgid).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                if (faimg != null)
                {
                    var chimg_binds = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == faimg.GuidKey).Where(e => !e.IsDeleted).ToListAsync());
                    if (chimg_binds != null)
                    {
                        foreach (var chimg_bind in chimg_binds)
                        {
                            var chimg = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == chimg_bind.FK_FileUploadId).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                            if (chimg != null)
                            {
                                chimg.IsDeleted = true;
                                chimg.DeletionTime = DateTime.Now;
                                chimg.DeleterUserId = usetId;
                                db.SaveChanges();
                            }
                            chimg_bind.IsDeleted = true;
                            chimg_bind.DeletionTime = DateTime.Now;
                            chimg_bind.DeleterUserId = usetId;
                        }
                        db.SaveChanges();
                    }

                    var fbs = await (db.FileBinds.Where(e => e.FK_FileUploadId == faimg.Id).ToListAsync());
                    foreach (var fb in fbs)
                    {
                        fb.IsDeleted = true;
                        fb.DeletionTime = DateTime.Now;
                        fb.DeleterUserId = usetId;
                        db.SaveChanges();
                    }

                    faimg.IsDeleted = true;
                    faimg.DeletionTime = DateTime.Now;
                    faimg.DeleterUserId = usetId;
                }

                db.SaveChanges();

            }
            catch (Exception e)
            {
                response.Error = e.Message;
                response.Success = false;
            }

            await loginUserData.SetLogs(AppName, "deleteImgFile", imgid.ToString(), JsonConvert.SerializeObject(response));
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
        // size = 1 原圖 2中縮圖 3小縮圖
        public async Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto)
        {
            var result = new List<FileGetImgDto>();
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                string orgName = await loginUserData.GetWebsiteOrgName();

                var faids = await (db.FileBinds.Where(e => e.Sid == dto.Sid && e.type == dto.Type).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId)).ToListAsync();

                if (faids != null)
                {
                    if (dto.Size == 1)
                    {
                        foreach (var faid in faids)
                        {
                            var fadata = await (db.FileUploads.Where(e => e.Id == faid)).FirstOrDefaultAsync();

                            if (fadata.GuidKey != Guid.Empty)
                            {
                                result.Add(new FileGetImgDto
                                {
                                    Id = fadata.Id,
                                    Name = fadata.OriginalFileName,
                                    Link = fadata.DownloadFileName.Replace("upload", $"upload/{orgName}")
                                });
                            }
                        }
                    }
                    else
                    {
                        foreach (var faid in faids)
                        {
                            var fadata = await (db.FileUploads.Where(e => e.Id == faid)).FirstOrDefaultAsync();

                            if (fadata.GuidKey != Guid.Empty)
                            {
                                var chimg_ids = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == fadata.GuidKey).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId).ToListAsync());
                                if (chimg_ids.Count > 0)
                                {
                                    var chimg = new List<FileUpload>();
                                    foreach (var chimg_id in chimg_ids)
                                    {
                                        chimg.Add(await (db.FileUploads.Where(e => e.Id == chimg_id).Where(e => !e.IsDeleted).FirstOrDefaultAsync()));

                                    }
                                    chimg = chimg.OrderByDescending(e => e.Size).ToList();
                                    result.Add(new FileGetImgDto
                                    {
                                        Id = faid.Value,
                                        Name = chimg[dto.Size - 2].OriginalFileName,
                                        Link = chimg[dto.Size - 2].DownloadFileName.Replace("upload", $"upload/{orgName}")
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
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
                if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
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
        private async Task<List<FileItemDto>> SaveImage(IList<IFormFile> files, int type, string directory, long sid)
        {
            if (files.Count() > 0)
            {
                Guid faimg_id = new Guid();
                string orgName = await loginUserData.GetWebsiteOrgName();
                var return_item = new List<FileItemDto>();
                try
                {
                    foreach (var file in files)
                    {
                        Guid key = Guid.NewGuid();
                        string[] sp = file.FileName.Split('.');
                        string ext = sp[sp.Length - 1];
                        var rootPath = $"{_folder}/{orgName}";
                        var directoryPath = $"{rootPath}/{directory}";
                        var path = $"/{directory}/{key}.{ext}";
                        if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception();
                        if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
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
                            if (faimg_id != Guid.Empty)
                            {
                                FileBindMore fileBindMore = new FileBindMore
                                {
                                    type = type,
                                    FK_FileBindGuid = faimg_id,
                                    FK_FileUploadId = fileUpload.Id
                                };
                                db.FileBindMores.Add(fileBindMore);
                                await loginUserData.SaveChanges(fileBindMore);
                            }
                            else
                            {
                                faimg_id = fileUpload.GuidKey;
                                long userid = await loginUserData.GetUserId();
                                Core.Models.FileBind fb = new Core.Models.FileBind
                                {
                                    Guid = Guid.NewGuid(),
                                    Name = fileUpload.OriginalFileName,
                                    type = type,
                                    Sid = sid,
                                    num = 1,
                                    SerNo = 1,
                                    MediaLink = "",
                                    FK_FileUploadId = fileUpload.Id,
                                    CreatorUserId = userid,
                                };
                                db.FileBinds.Add(fb);
                                db.SaveChanges();

                            }

                            return_item.Add(new FileItemDto
                            {
                                Id = fileUpload.Id,
                                Name = fileUpload.OriginalFileName,
                                Path = fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
                                Guid = fileUpload.GuidKey,
                            });
                        }
                    }
                    return return_item;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            else throw new Exception("上傳失敗");
        }
    }
}
