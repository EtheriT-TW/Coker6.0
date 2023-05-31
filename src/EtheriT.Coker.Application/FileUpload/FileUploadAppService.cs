using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EtheriT.Coker.Application
{
    public class FileUploadAppService : IFileUploadAppService
    {
        private readonly CokerDbContext db;
        public readonly FileAllow fileAllow;
        private readonly LoginUserData loginUserData;
        private readonly string _folder;
        private readonly string AppName;
        private readonly IConfiguration configuration;
        public FileUploadAppService(
            IOptions<VirtualDirectory> fileAllow,
            LoginUserData loginUserData,
            CokerDbContext db,
            IConfiguration configuration
        )
        {
            this.fileAllow = fileAllow.Value.FileAllow;
            this.db = db;
            this.loginUserData = loginUserData;
            _folder = fileAllow.Value.upload;
            AppName = "FileUpload";
            this.configuration = configuration;
        }
        public async Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files)
        {
            UploadFileOutputDto response = await uploadFiles(files, "temp", isTemp: true);
            return response;
        }
        public async Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files)
        {
            UploadFileOutputDto response = await uploadFiles(files, "htmlConten");
            await loginUserData.SetLogs(AppName, "uploadProdtFiles", "FileBinary...", JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<UploadFileOutputDto> uploadProdtFiles(IList<IFormFile> files, long id)
        {
            UploadFileOutputDto response = await uploadFiles(files, "Prod");
            return response;
        }
        private async Task<UploadFileOutputDto> uploadFiles(IList<IFormFile> files, string type, bool isTemp = false)
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
                    FileItemDto item = await SaveFile(file, type, isTemp);
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
        public async Task<UploadFileOutputDto> uploadMediaFiles(IList<IFormFile> files, int type, long sid, int serno, string page)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                List<FileItemDto> items = await SaveImage(files, type, serno, page, sid);
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
        public async Task<UploadFileOutputDto> upload360Files(IList<IFormFile> files, int type, long? sid, string page)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            return null;
        }
        public async Task<ResponseMessageDto> uploadYTLink(FileYTLinkUploadDto dto, string page)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = true };

            try
            {
                long userId = await loginUserData.GetUserId();

                if (dto.Id == 0)
                {
                    long WebsiteID = await loginUserData.GetWebsiteId();
                    Guid key = Guid.NewGuid();

                    Core.Models.FileUpload fu = new Core.Models.FileUpload
                    {
                        FK_WebsiteId = WebsiteID,
                        GuidKey = Guid.NewGuid(),
                        ContentType = "youtube",
                        OriginalFileName = dto.File,
                        DownloadFileName = $"https://www.youtube.com/watch?v={dto.File}",
                        Size = 0,
                        FileGuid = key,
                        CreatorUserId = userId,
                        CreationTime = DateTime.Now,
                    };
                    db.FileUploads.Add(fu);
                    db.SaveChanges();

                    Core.Models.FileBind fb = new Core.Models.FileBind
                    {
                        Guid = Guid.NewGuid(),
                        Name = fu.OriginalFileName,
                        Sid = dto.SId,
                        type = dto.Type,
                        num = 1,
                        SerNo = dto.SerNo,
                        MediaLink = fu.DownloadFileName,
                        FK_FileUploadId = fu.Id,
                        CreatorUserId = userId,
                        CreationTime = DateTime.Now,
                    };
                    db.FileBinds.Add(fb);
                    db.SaveChanges();
                }
                else
                {
                    var db_fu = db.FileUploads.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_fu != null)
                    {
                        db_fu.OriginalFileName = dto.File;
                        db_fu.DownloadFileName = $"https://www.youtube.com/watch?v={dto.File}";
                        db_fu.LastModificationTime = DateTime.Now;
                        db_fu.LastModifierUserId = userId;

                        var db_fb = db.FileBinds.Where(e => e.FK_FileUploadId == dto.Id).FirstOrDefault();
                        if (db_fb != null)
                        {

                            db_fb.SerNo = dto.SerNo;
                            db_fb.Name = db_fu.OriginalFileName;
                            db_fb.MediaLink = db_fu.DownloadFileName;
                            db_fb.LastModifierUserId = userId;
                            db_fb.LastModificationTime = DateTime.Now;
                        }
                        else
                        {
                            response.Success = false;
                        }
                    }
                    else
                    {
                        response.Success = false;
                    }
                }
                db.SaveChanges();

            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
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
        // size = 1 原圖 2中縮圖 3小縮圖
        public async Task<List<string>> getImgFilesById(List<long> Ids, int size)
        {
            var result = new List<string>();
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                string orgName = await loginUserData.GetWebsiteOrgName();

                if (size == 1)
                {
                    foreach (var id in Ids)
                    {
                        var file = await db.FileUploads.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();
                        if (file != null)
                        {
                            result.Add(file.DownloadFileName == null ? "" : file.DownloadFileName);
                        }
                    }
                }
                else
                {
                    foreach (var id in Ids)
                    {
                        var fa_file = await db.FileUploads.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();

                        if (fa_file != null)
                        {
                            var files = await db.FileBindMores.Where(e => e.FK_FileBindGuid == fa_file.GuidKey && !e.IsDeleted).ToListAsync();

                            if (files != null)
                            {
                                var temp_files = new List<FileGetImgDto>();
                                foreach (var file in files)
                                {
                                    var fu = await db.FileUploads.Where(e => e.Id == file.FK_FileUploadId && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();
                                    if (fu != null)
                                    {
                                        temp_files.Add(new FileGetImgDto()
                                        {
                                            Id = fu.Id,
                                            Name = fu.OriginalFileName,
                                            Link = fu.DownloadFileName,
                                            Size = fu.Size,
                                        });
                                    }
                                }

                                temp_files.Sort((x, y) => x.Size.CompareTo(y.Size));
                                result.Add(temp_files[size - 2].Link);
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
        public async Task<List<FileGetProdDisplayDto>> getProdDisplayFiles(long Pid, int size)
        {
            var output = new List<FileGetProdDisplayDto>();
            string orgName = await loginUserData.GetWebsiteOrgName();

            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                if (websiteId == 0)
                {
                    websiteId = await loginUserData.GetWebsiteId();
                }

                var fbs = await (db.FileBinds.Where(e => e.Sid == Pid && e.type == (int)FileBindTypeEnum.產品).Where(e => !e.IsDeleted)).ToListAsync();

                if (fbs != null)
                {
                    foreach (var fb in fbs)
                    {
                        if (fb.MediaLink != "")
                        {
                            var file_type = 0;
                            var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();

                            if (Regex.IsMatch(fb.MediaLink, @"^https://www.youtube.com/[\w\W]*", RegexOptions.IgnoreCase))
                            {
                                file_type = 4;
                            }
                            else
                            {
                                if (fu != null)
                                {
                                    if (Regex.IsMatch(fu.ContentType, @"^image/[\w\W]*", RegexOptions.IgnoreCase))
                                    {
                                        file_type = fb.num == 1 ? 1 : 2;
                                    }
                                    else if (Regex.IsMatch(fu.ContentType, @"^video/[\w\W]*", RegexOptions.IgnoreCase))
                                    {
                                        file_type = 3;
                                    }
                                }
                            }

                            output.Add(new FileGetProdDisplayDto
                            {
                                Id = fu.Id,
                                Name = fb.Name,
                                FileType = file_type,
                                Link = new List<string> { fb.MediaLink },
                                SerNo = fb.SerNo,
                            });
                        }
                        else
                        {
                            var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();
                            if (fu != null)
                            {
                                if (Regex.IsMatch(fu.ContentType, @"^image/[\w\W]*", RegexOptions.IgnoreCase))
                                {
                                    if (fb.num == 1)
                                    {
                                        var ids = new List<long> { fu.Id };
                                        var file_link = await this.getImgFilesById(ids, 3);

                                        output.Add(new FileGetProdDisplayDto
                                        {
                                            Id = fu.Id,
                                            Name = fb.Name,
                                            FileType = 1,
                                            Link = file_link,
                                            SerNo = fb.SerNo,
                                        });
                                    }
                                    else
                                    {
                                        //360抓圖
                                    }
                                }
                                else if (Regex.IsMatch(fu.ContentType, @"^video/[\w\W]*", RegexOptions.IgnoreCase))
                                {
                                    output.Add(new FileGetProdDisplayDto
                                    {
                                        Id = fu.Id,
                                        Name = fb.Name,
                                        FileType = 3,
                                        Link = new List<string> { fu.DownloadFileName == null ? "" : fu.DownloadFileName },
                                        SerNo = fb.SerNo,
                                    });
                                }
                            }
                        }
                    }
                }

                output.Sort((x, y) => (x.SerNo.CompareTo(y.SerNo) * 2 + x.Id.CompareTo(y.Id)));
                for (var i = 0; i < output.Count; i++)
                {
                    for (var j = 0; j < output[i].Link.Count; j++)
                    {
                        output[i].Link[j] = output[i].Link[j].Replace("upload", $"upload/{orgName}");
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return output;
            }
        }
        public async Task<ResponseMessageDto> fileSortChange(FileChangeSortDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var userid = await loginUserData.GetUserId();
                var db_fb = await db.FileBinds.Where(e => e.FK_FileUploadId == dto.id && !e.IsDeleted && e.type == (int)FileBindTypeEnum.產品).FirstOrDefaultAsync();
                if (db_fb != null)
                {
                    db_fb.SerNo = dto.SerNo;
                    db_fb.LastModifierUserId = userid;
                    db_fb.LastModificationTime = DateTime.Now;
                }
                db.SaveChanges();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;

        }
        public async Task<ResponseMessageDto> deleteFile(string path)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (File.Exists(path))
                {
                    File.Delete($"{path}");
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(AppName, "deleteFile", path, JsonConvert.SerializeObject(response));
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
                    var delete_response = await deleteFileById(imgdata.FK_FileUploadId);
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
        public async Task<ResponseMessageDto> deleteFileById(long? fileid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            response.Success = true;

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();

                var fafile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == fileid).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                if (fafile != null)
                {
                    var chfile_binds = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == fafile.GuidKey).Where(e => !e.IsDeleted).ToListAsync());
                    if (chfile_binds != null)
                    {
                        foreach (var chfile_bind in chfile_binds)
                        {
                            var chfile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == chfile_bind.FK_FileUploadId).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                            if (chfile != null)
                            {
                                chfile.IsDeleted = true;
                                chfile.DeletionTime = DateTime.Now;
                                chfile.DeleterUserId = usetId;
                                db.SaveChanges();
                            }
                            chfile_bind.IsDeleted = true;
                            chfile_bind.DeletionTime = DateTime.Now;
                            chfile_bind.DeleterUserId = usetId;
                        }
                        db.SaveChanges();
                    }

                    var fbs = await (db.FileBinds.Where(e => e.FK_FileUploadId == fafile.Id).ToListAsync());
                    foreach (var fb in fbs)
                    {
                        fb.IsDeleted = true;
                        fb.DeletionTime = DateTime.Now;
                        fb.DeleterUserId = usetId;
                        db.SaveChanges();
                        if (fb.MediaLink != "")
                        {
                            await this.deleteFile(fafile.GuidKey);
                        }
                    }

                    fafile.IsDeleted = true;
                    fafile.DeletionTime = DateTime.Now;
                    fafile.DeleterUserId = usetId;
                }

                db.SaveChanges();

            }
            catch (Exception e)
            {
                response.Error = e.Message;
                response.Success = false;
            }

            await loginUserData.SetLogs(AppName, "deleteImgFile", fileid.ToString(), JsonConvert.SerializeObject(response));
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
        private async Task<FileItemDto> SaveFile(IFormFile file, string directory, bool isTemp = false)
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
                    await file.CopyToAsync(stream);
                    if (isTemp)
                    {
                        return new FileItemDto
                        {
                            Name = file.FileName,
                            Path = $@"/upload{path}".Replace("/upload/", $"/upload/{orgName}/"),
                            Guid = Guid.NewGuid()
                        };
                    }
                    else
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
            }
            else throw new Exception("上傳失敗");
        }
        private async Task<List<FileItemDto>> SaveImage(IList<IFormFile> files, int type, int serno, string directory, long sid)
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
                                    SerNo = serno,
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
