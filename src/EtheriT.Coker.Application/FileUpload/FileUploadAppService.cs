using DevExpress.CodeParser;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application
{
    public class FileUploadAppService : IFileUploadAppService
    {
        private readonly CokerDbContext db;
        public readonly FileAllow fileAllow;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly ITokenAppService tokenAppService;
        private readonly IUploadPathResolver uploadPathResolver;
        public FileUploadAppService(
            IOptions<VirtualDirectory> fileAllow,
            LoginUserData loginUserData,
            CokerDbContext db,
            IConfiguration configuration,
            ITokenAppService tokenAppService,
            IUploadPathResolver uploadPathResolver
        )
        {
            this.fileAllow = fileAllow.Value.FileAllow;
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.tokenAppService = tokenAppService;
            this.uploadPathResolver = uploadPathResolver;
        }
        public async Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files)
        {
            UploadFileOutputDto response = await uploadFiles(files, "temp", isTemp: true);
            return response;
        }
        public async Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files)
        {
            UploadFileOutputDto response = await uploadFiles(files, "htmlConten");
            await loginUserData.SetLogs("FileBinary...", JsonConvert.SerializeObject(response));
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
                    FileItemDto item = await SaveFile(file, "", type, isTemp);
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
        public async Task<UploadFileOutputDto> uploadFiles(IList<IFormFile> files, string filename, string areakey, int type, long id, long sid, int serno, string page, bool isVisible, bool isEncryption)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                List<FileItemDto> filds = await SaveFile(files, filename, areakey, type, serno, page, id, sid, isVisible, isEncryption);
                response.Files = filds.FindAll(e => e.Id != 0 && e.Id != null);
                response.ErrorFiles = filds.FindAll(e => e.Id == 0 || e.Id == null).Select(e => e.Name).ToList();
                if (response.ErrorFiles.Count == 0) response.Success = true;
            }
            catch
            {

            }
            return response;
        }
        public async Task<UploadFileOutputDto> uploadMediaFiles(IList<IFormFile> files, int type, long sid, int serno, string page, bool convert)
        {
            long usetId = await loginUserData.GetUserId();
            ResponseMessageDto DeleResponse;
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>(),
                ErrorFiles = new List<string>()
            };
            try
            {
                if (type == (int)FileBindTypeEnum.自訂廣告)
                {
                    var db_fb = await db.FileBinds.Where(e => e.Sid == sid && e.type == (int)FileBindTypeEnum.自訂廣告 && !e.IsDeleted).FirstOrDefaultAsync();
                    if (db_fb != null)
                    {
                        var db_fu = await db.FileUploads.Where(e => e.Id == db_fb.FK_FileUploadId).FirstOrDefaultAsync();
                        if (db_fu != null)
                        {
                            DeleResponse = await this.deleteFile(db_fu.GuidKey);
                            db_fu.IsDeleted = true;
                            db_fu.DeletionTime = DateTime.Now;
                            db_fu.DeleterUserId = usetId;
                            db.SaveChanges();
                        }
                        db_fb.IsDeleted = true;
                        db_fb.DeletionTime = DateTime.Now;
                        db_fb.DeleterUserId = usetId;
                        db.SaveChanges();
                    }
                }
                List<FileItemDto> items = await SaveImage(files, type, (int)FileBindMoreEnum.壓縮圖片, serno, page, sid, convert);
                foreach (var item in items)
                {
                    response.Files.Add(item);
                }
                response.Success = true;

                var websiteid = await loginUserData.GetWebsiteId();
                switch (type)
                {
                    case (int)FileBindTypeEnum.網站圖示:
                        var icon_db_website = await db.Websites.Where(e => e.Id == sid).FirstOrDefaultAsync();
                        if (icon_db_website != null) icon_db_website.Icon = response.Files[0].Path;
                        break;
                    case (int)FileBindTypeEnum.網站Logo:
                        var logo_db_website = await db.Websites.Where(e => e.Id == sid).FirstOrDefaultAsync();
                        if (logo_db_website != null) logo_db_website.Logo = response.Files[0].Path;
                        break;
                    case (int)FileBindTypeEnum.選單圖:
                        var db_bind = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                        if (db_bind != null) db_bind.ImgId = response.Files[0].Id;
                        break;
                    case (int)FileBindTypeEnum.選單覆蓋:
                        var db_bind_over = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                        if (db_bind_over != null) db_bind_over.OverImgId = response.Files[0].Id;
                        break;
                    case (int)FileBindTypeEnum.選單Icon:
                        var db_bind_icon = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                        if (db_bind_icon != null) db_bind_icon.icon = $"IconId:{response.Files[0].Id}";
                        break;
                }
                db.SaveChanges();
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorFiles.Add(ex.Message);
                return response;
            }
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
        public async Task<ResponseMessageDto> uploadYTLink(FileYTLinkUploadDto dto)
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
                        db_fu.ContentType = "youtube";
                        db_fu.OriginalFileName = dto.File;
                        db_fu.DownloadFileName = $"https://www.youtube.com/watch?v={dto.File}";
                        db_fu.Size = 0;
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
        public async Task<ResponseMessageDto> uploadImageLink(List<FileImageImportDto> dto)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = true };
            if (dto.Count == 0) return response;

            List<FileUpload> files = await AddFileUploads(dto);
            db.FileUploads.AddRange(files);
            await db.SaveChangesAsync();

            List<FileBind> FileBinds = await AddFileBinds(dto);
            db.FileBinds.AddRange(FileBinds);
            await db.SaveChangesAsync();
            return response;
        }
        private async Task<List<FileUpload>> AddFileUploads(List<FileImageImportDto> dto)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            string OrgName = await loginUserData.GetWebsiteOrgName();
            long userId = await loginUserData.GetUserId();
            List<FileUpload> files = new List<FileUpload>();
            List<string> fileNames = dto.Where(e => !string.IsNullOrEmpty(e.mediaLink)).Select(e => e.mediaLink).ToList();
            List<FileUpload> nowFiles = db.FileUploads
                                        .Where(e => !e.IsDeleted)
                                        .Where(e => e.DownloadFileName != null && fileNames.Contains(e.DownloadFileName))
                                        .Where(e => e.FK_WebsiteId == WebsiteID)
                                        .ToList();
            foreach (var file in dto)
            {
                try
                {
                    /*string dir = "";
					switch (file.Type)
					{
						case FileBindTypeEnum.產品:
							dir = "Product";
							break;
						case FileBindTypeEnum.技術證照:
							dir = "TechnicalCertificate";
							break;
						default:
							dir = "Orthers";
							break;
					}
					if (!(new Regex("^http")).IsMatch(file.mediaLink)) file.mediaLink = $"/upload/{dir}/{file.mediaLink}";*/

                    var myFile = nowFiles.Find(e => e.DownloadFileName == file.mediaLink);
                    var hasFile = files.Find(e => e.DownloadFileName == file.mediaLink);
                    if (myFile == null && hasFile == null)
                    {


                        Guid key = Guid.NewGuid();
                        FileUpload fu = new FileUpload
                        {
                            FK_WebsiteId = WebsiteID,
                            GuidKey = Guid.NewGuid(),
                            ContentType = "image/",
                            OriginalFileName = file.mediaLink,
                            DownloadFileName = file.mediaLink,
                            Size = 0,
                            FileGuid = key,
                            CreatorUserId = userId
                        };
                        files.Add(fu);
                    }
                }
                catch (Exception e)
                {

                }
            }
            return files;
        }
        private async Task<List<FileBind>> AddFileBinds(List<FileImageImportDto> dto)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            long userId = await loginUserData.GetUserId();
            var filesName = dto.Select(o => o.mediaLink).ToList();
            var allFile = db.FileUploads
                            .Where(e => e.FK_WebsiteId == WebsiteID)
                            .Where(e => !e.IsDeleted)
                            .Where(f => filesName.Contains(f.DownloadFileName)).ToList();
            List<FileBind> FileBinds = new List<FileBind>();
            foreach (var file in dto)
            {
                long Id = allFile.Find(d => d.DownloadFileName == file.mediaLink).Id;
                var myFileBind = db.FileBinds
                        .Where(e => e.FK_FileUploadId == Id)
                        .Where(e => !e.IsDeleted)
                        .Where(e => e.Sid == file.SId)
                        .Where(e => e.type == (int)file.Type)
                        .FirstOrDefault();
                var hasBind = FileBinds.Find(e => e.FK_FileUploadId == Id && e.Sid == file.SId && e.type == (int)file.Type);
                if (myFileBind == null && hasBind == null)
                {
                    FileBind fb = new FileBind
                    {
                        Guid = Guid.NewGuid(),
                        Name = string.IsNullOrEmpty(file.Name) ? file.mediaLink : file.Name,
                        Sid = file.SId,
                        type = (int)file.Type,
                        num = 1,
                        SerNo = file.SerNo,
                        MediaLink = file.mediaLink,
                        FK_FileUploadId = Id,
                        CreatorUserId = userId,
                        CreationTime = DateTime.Now,
                    };
                    FileBinds.Add(fb);
                }
                else if (myFileBind != null)
                {
                    myFileBind.SerNo = file.SerNo;
                    myFileBind.Name = string.IsNullOrEmpty(file.Name) ? file.mediaLink : file.Name;
                    myFileBind.FK_FileUploadId = Id;
                    myFileBind.MediaLink = file.mediaLink;
                    myFileBind.LastModifierUserId = userId;
                    myFileBind.LastModificationTime = DateTime.Now;
                }
            }
            return FileBinds;
        }
        public async Task<ResponseMessageDto> uploadImageLink(FileImageImportDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = true };

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();
                var myFile = await db.FileUploads.Where(e => e.DownloadFileName == dto.mediaLink).FirstOrDefaultAsync();
                if (myFile != null) dto.Id = myFile.Id;
                else
                {
                    Guid key = Guid.NewGuid();
                    FileUpload fu = new FileUpload
                    {
                        FK_WebsiteId = WebsiteID,
                        GuidKey = Guid.NewGuid(),
                        ContentType = "image/",
                        OriginalFileName = dto.mediaLink,
                        DownloadFileName = dto.mediaLink,
                        Size = 0,
                        FileGuid = key,
                        CreatorUserId = userId
                    };
                    db.FileUploads.Add(fu);
                    db.SaveChanges();
                    dto.Id = fu.Id;
                }
                var myFileBind = db.FileBinds
                        .Where(e => e.FK_FileUploadId == dto.Id)
                        .Where(e => e.Sid == dto.SId)
                        .Where(e => e.type == (int)dto.Type)
                        .FirstOrDefault();
                if (myFileBind == null)
                {
                    FileBind fb = new FileBind
                    {
                        Guid = Guid.NewGuid(),
                        Name = dto.mediaLink,
                        Sid = dto.SId,
                        type = (int)dto.Type,
                        num = 1,
                        SerNo = dto.SerNo,
                        MediaLink = dto.mediaLink,
                        FK_FileUploadId = dto.Id,
                        CreatorUserId = userId,
                        CreationTime = DateTime.Now,
                    };
                    db.FileBinds.Add(fb);
                }
                else
                {
                    myFileBind.SerNo = dto.SerNo;
                    myFileBind.Name = dto.mediaLink;
                    myFileBind.FK_FileUploadId = dto.Id;
                    myFileBind.MediaLink = dto.mediaLink;
                    myFileBind.LastModifierUserId = userId;
                    myFileBind.LastModificationTime = DateTime.Now;
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
        // size = 1 原圖 2中縮圖 3小縮圖
        public async Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto)
        {
            if (dto == null) return new();
            return await getImgsFiles(new FileGetImgsInputDto
            {
                Sid = new List<long> { dto.Sid },
                Type = dto.Type,
                Size = dto.Size
            });
        }
        public async Task<List<FileGetImgDto>> getImgsFiles(FileGetImgsInputDto dto)
        {
            if (dto?.Sid == null || dto.Sid.Count == 0) return new();

            var binds = await db.FileBinds
                .AsNoTracking()
                .Where(b => !b.IsDeleted && dto.Sid.Contains(b.Sid) && b.type == dto.Type)
                .Where(b => b.FK_FileUploadId.HasValue)
                .Select(b => new { b.Sid, UploadId = b.FK_FileUploadId!.Value, b.SerNo, b.Name }) // 取出 FileBinds.Name
                .ToListAsync();

            if (binds.Count == 0) return new();

            var uploadIds = binds.Select(x => x.UploadId).Distinct().ToList();

            // 同一 UploadId 可能對應多筆 bind，沿用 SerNo 最小者
            var uploadIdMeta = binds
                .GroupBy(x => x.UploadId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var first = g.OrderBy(x => x.SerNo).First();
                        return (Sid: first.Sid, BindName: first.Name);
                    });

            var imgDtos = await _getLinksByUploadIdsAsync(uploadIds, dto.Size, uploadIdMeta);

            var serNoMap = binds
                .GroupBy(b => b.UploadId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Min(x => x.SerNo)
                );

            var sidOrder = dto.Sid
                .Select((sid, index) => new { sid, index })
                .ToDictionary(x => x.sid, x => x.index);

            imgDtos = imgDtos
                .OrderBy(d => sidOrder.TryGetValue(d.Sid, out var idx) ? idx : int.MaxValue)
                .ThenBy(d => serNoMap.TryGetValue(d.Id, out var serno) ? serno : int.MaxValue)
                .ThenBy(d => d.Id)
                .ToList();

            return imgDtos;
        }
        public async Task<List<string>> getImgFilesById(List<long> ids, int size)
        {
            var imgDtos = await _getLinksByUploadIdsAsync(ids, size);
            return imgDtos.Select(i => i.Link).ToList();
        }
        private async Task<List<FileGetImgDto>> _getLinksByUploadIdsAsync(
            List<long> uploadIds, int size,
            Dictionary<long, (long Sid, string? BindName)>? uploadIdMeta = null)
        {
            var result = new List<FileGetImgDto>();
            if (uploadIds == null || uploadIds.Count == 0) return result;

            long websiteId = await loginUserData.GetWebsiteId();
            if (websiteId == 0) websiteId = configuration.GetValue<long>("WebConfig:SiteId");

            string orgName = await loginUserData.GetWebsiteOrgName();
            orgName = string.IsNullOrEmpty(orgName) ? "" : $"/{orgName}";

            var uploads = await db.FileUploads
                .AsNoTracking()
                .Where(u => uploadIds.Contains(u.Id) && !u.IsDeleted && u.FK_WebsiteId == websiteId)
                .Select(u => new { u.Id, u.FK_WebsiteId, u.GuidKey, u.DownloadFileName, u.OriginalFileName, u.Size })
                .ToListAsync();

            if (uploads.Count == 0) return result;

            var allSiteIds = new HashSet<long>(uploads.Select(u => (long)u.FK_WebsiteId)) { websiteId };

            Dictionary<Guid, List<long>>? moreIdsByGuid = null;
            Dictionary<long, (long Size, string Download, string Original, long FK_WebsiteId)>? childUploadMap = null;

            if (size != 1)
            {
                var guids = uploads.Where(u => u.GuidKey != Guid.Empty).Select(u => u.GuidKey).Distinct().ToList();
                if (guids.Count > 0)
                {
                    var more = await db.FileBindMores
                        .AsNoTracking()
                        .Where(m => !m.IsDeleted && guids.Contains(m.FK_FileBindGuid) && m.FK_FileUploadId.HasValue)
                        .Select(m => new { m.FK_FileBindGuid, FK_FileUploadId = m.FK_FileUploadId!.Value })
                        .ToListAsync();

                    moreIdsByGuid = more
                        .GroupBy(m => m.FK_FileBindGuid)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.FK_FileUploadId).ToList());

                    var childIds = more.Select(x => x.FK_FileUploadId).Distinct().ToList();
                    if (childIds.Count > 0)
                    {
                        var childs = await db.FileUploads
                            .AsNoTracking()
                            .Where(u => childIds.Contains(u.Id))
                            .Select(u => new { u.Id, u.FK_WebsiteId, u.DownloadFileName, u.OriginalFileName, u.Size })
                            .ToListAsync();

                        childUploadMap = childs.ToDictionary(
                            u => u.Id,
                            u => (u.Size, u.DownloadFileName ?? "", u.OriginalFileName, u.FK_WebsiteId));

                        foreach (var w in childs.Select(c => (long)c.FK_WebsiteId).Distinct())
                            allSiteIds.Add(w);
                    }
                }
            }

            var siteOrgMap = await db.Websites
                .AsNoTracking()
                .Where(w => allSiteIds.Contains(w.Id))
                .Select(w => new { w.Id, w.OrgName })
                .ToDictionaryAsync(w => (long)w.Id, w => w.OrgName);

            string BuildLink(string downloadFileName, long fileWebsiteId)
            {
                var imageOrgName = orgName;
                if (!string.IsNullOrEmpty(imageOrgName) && fileWebsiteId != websiteId)
                {
                    if (siteOrgMap.TryGetValue(fileWebsiteId, out var otherOrg) && !string.IsNullOrEmpty(otherOrg))
                        imageOrgName = $"/{otherOrg}";
                }
                return downloadFileName.Replace("upload", $"upload{imageOrgName}");
            }

            static string FileNameOnly(string? path)
            {
                if (string.IsNullOrEmpty(path)) return "";
                return System.IO.Path.GetFileName(path); // 自動只留檔名（支援 / 與 \）
            }

            foreach (var up in uploads)
            {
                string link = BuildLink(up.DownloadFileName, up.FK_WebsiteId);
                string originalName = up.OriginalFileName; // 可能含路徑，稍後只取檔名

                if (size != 1 && up.GuidKey != Guid.Empty &&
                    moreIdsByGuid != null && childUploadMap != null &&
                    moreIdsByGuid.TryGetValue(up.GuidKey, out var childIds) &&
                    childIds.Count > 0)
                {
                    var candidates = childIds
                        .Where(id => childUploadMap.ContainsKey(id))
                        .Select(id => childUploadMap[id])
                        .OrderByDescending(x => x.Size)
                        .ToList();

                    if (candidates.Count == 2 && (size - 2) >= 0 && (size - 2) < 2)
                    {
                        var pick = candidates[size - 2];
                        link = BuildLink(pick.Download, pick.FK_WebsiteId);
                        originalName = pick.Original;
                    }
                    else if (candidates.Count == 1)
                    {
                        var pick = candidates[0];
                        link = BuildLink(pick.Download, pick.FK_WebsiteId);
                        originalName = pick.Original;
                    }
                }

                // 這裡優先使用 FileBinds.Name
                long sid = 0;
                string name = FileNameOnly(originalName); // 預設退回「只保留檔名」

                if (uploadIdMeta != null && uploadIdMeta.TryGetValue(up.Id, out var meta))
                {
                    sid = meta.Sid;
                    if (!string.IsNullOrWhiteSpace(meta.BindName))
                        name = meta.BindName!;
                }

                result.Add(new FileGetImgDto
                {
                    Id = up.Id,
                    Name = name,     // ← 以 FileBinds.Name 為主；空時退回檔名
                    Link = link,
                    Sid = sid,
                    Size = up.Size
                });
            }

            return result;
        }
        public async Task<List<FileGetProdDisplayDto>> getProdFiles(long Pid)
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

                var fbs = await (db.FileBinds.Where(e => e.Sid == Pid && e.type == (int)FileBindTypeEnum.產品檔案).Where(e => !e.IsDeleted).OrderBy(e => e.SerNo)).ToListAsync();
                if (fbs != null)
                {
                    foreach (var fb in fbs)
                    {
                        var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();
                        if (fu != null)
                        {
                            string MediaLink = fb.MediaLink;
                            if (orgName != "")
                            {
                                MediaLink = MediaLink.Replace("upload", $"upload/{orgName}");
                            }
                            output.Add(new FileGetProdDisplayDto
                            {
                                Id = fu.Id,
                                Name = string.IsNullOrEmpty(fb.Name) ? fu.OriginalFileName : fb.Name,
                                FileType = 5,
                                Link = new List<string> { MediaLink },
                                SerNo = fb.SerNo,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }
        public async Task<List<FileGetArticleDisplayDto>> getArticleFiles(long Aid)
        {
            var output = new List<FileGetArticleDisplayDto>();
            string orgName = await loginUserData.GetWebsiteOrgName();
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                if (websiteId == 0)
                {
                    websiteId = await loginUserData.GetWebsiteId();
                }

                var fbs = await (db.FileBinds.Where(e => e.Sid == Aid && e.type == (int)FileBindTypeEnum.文章檔案).Where(e => !e.IsDeleted).OrderBy(e => e.SerNo)).ToListAsync();
                if (fbs != null)
                {
                    foreach (var fb in fbs)
                    {
                        var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();
                        if (fu != null)
                        {
                            string MediaLink = fb.MediaLink;
                            if (orgName != "")
                            {
                                MediaLink = MediaLink.Replace("upload", $"upload/{orgName}");
                            }
                            if (MediaLink == "") MediaLink = fu.DownloadFileName ?? "";

                            var size = "0";

                            if (fu.Size < 1024) size = $"{fu.Size} B";
                            else if (fu.Size < 1024 * 1024) size = $"{fu.Size / 1024.0:F1} KB";
                            else if (fu.Size < 1024 * 1024 * 1024) size = $"{fu.Size / (1024.0 * 1024):F1} MB";
                            else size = $"{fu.Size / (1024.0 * 1024 * 1024):F1} GB";

                            var link = fu.IsEncryption ? $"/api/FileUpload/DecryptFile?fid={fu.Id}" : MediaLink.Replace("upload", $"upload/{orgName}");

                            output.Add(new FileGetArticleDisplayDto
                            {
                                Id = fu.Id,
                                Name = fb.Name,
                                Extension = GetExtension(fu.ContentType),
                                FileType = 5,
                                Link = new List<string> { link },
                                SerNo = fb.SerNo,
                                isEncryption = fu.IsEncryption,
                                isVisible = fb.IsVisible,
                                areakey = fb.AreaKey ?? "",
                                size = size,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }
        private string GetExtension(string ContentType)
        {
            var mimeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Office
                { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx" },
                { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx" },
                { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx" },
                { "application/msword", "doc" },
                { "application/vnd.ms-excel", "xls" },
                { "application/vnd.ms-powerpoint", "ppt" },

                // PDF
                { "application/pdf", "pdf" },

                // Text
                { "text/plain", "txt" },

                // Images
                { "image/png", "png" },
                { "image/jpeg", "jpg" },
                { "image/jpg", "jpg" },
                { "image/gif", "gif" },
                { "image/webp", "webp" },
                { "image/svg+xml", "svg" },
                { "image/bmp", "bmp" },

                // Archive
                { "application/zip", "zip" },
                { "application/x-rar-compressed", "rar" },
                { "application/x-7z-compressed", "7z" },
            };

            string ext = mimeMap.TryGetValue(ContentType, out var value) ? value : "";

            return ext;
        }
        public async Task<List<FileGetDisplayDto>> getAdvertiseFiles(long Aid, int type)
        {
            var filesByAdvertise = await GetAdvertiseFilesBatchAsync(new[] { Aid }, type);
            return filesByAdvertise.GetValueOrDefault(Aid) ?? new List<FileGetDisplayDto>();
        }

        public async Task<Dictionary<long, List<FileGetDisplayDto>>> GetAdvertiseFilesBatchAsync(
            IReadOnlyCollection<long> advertiseIds,
            int type)
        {
            var ids = advertiseIds?.Where(id => id > 0).Distinct().ToList() ?? new List<long>();
            var output = ids.ToDictionary(id => id, _ => new List<FileGetDisplayDto>());
            if (!ids.Any()) return output;

            var orgName = await loginUserData.GetWebsiteOrgName();
            var fileRows = await (
                from bind in db.FileBinds.AsNoTracking()
                join upload in db.FileUploads.AsNoTracking()
                    on bind.FK_FileUploadId equals (long?)upload.Id
                where ids.Contains(bind.Sid)
                    && bind.type == type
                    && !bind.IsDeleted
                    && !upload.IsDeleted
                orderby bind.Sid, bind.SerNo, bind.Id
                select new
                {
                    AdvertiseId = bind.Sid,
                    FileId = upload.Id,
                    bind.Name,
                    bind.MediaLink,
                    upload.DownloadFileName,
                    upload.ContentType
                }).ToListAsync();

            foreach (var fileRow in fileRows)
            {
                var mediaLink = string.IsNullOrWhiteSpace(fileRow.MediaLink)
                    ? fileRow.DownloadFileName ?? string.Empty
                    : fileRow.MediaLink;
                var contentType = fileRow.ContentType ?? string.Empty;
                var fileType = contentType.Equals("youtube", StringComparison.OrdinalIgnoreCase) ||
                    IsYoutubeMediaLink(mediaLink)
                    ? 3
                    : contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                        ? 1
                        : contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                            ? 2
                            : 0;

                output[fileRow.AdvertiseId].Add(new FileGetDisplayDto
                {
                    Id = fileRow.FileId,
                    Link = ApplyAdvertiseMediaPath(mediaLink, orgName),
                    Name = fileRow.Name,
                    FileType = fileType,
                    Video_Type = fileType == 2 ? contentType : null
                });
            }

            return output;
        }

        private static bool IsYoutubeMediaLink(string mediaLink)
        {
            if (!Uri.TryCreate(mediaLink, UriKind.Absolute, out var uri)) return false;

            var host = uri.Host;
            return host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith(".youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);
        }

        private static string ApplyAdvertiseMediaPath(string mediaLink, string orgName)
        {
            if (string.IsNullOrWhiteSpace(orgName) ||
                !mediaLink.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase) ||
                mediaLink.StartsWith($"/upload/{orgName}/", StringComparison.OrdinalIgnoreCase))
            {
                return mediaLink;
            }

            return $"/upload/{orgName}/{mediaLink.Substring("/upload/".Length)}";
        }

        public Task<List<FileGetProdDisplayDto>> getProdMultimedia(long Pid, int size)
        {
            return getBindMultimedia(Pid, size, FileBindTypeEnum.產品);
        }

        public Task<List<FileGetProdDisplayDto>> getSpecMultimedia(long Pid, int size)
        {
            return getBindMultimedia(Pid, size, FileBindTypeEnum.產品規格圖);
        }

        private async Task<List<FileGetProdDisplayDto>> getBindMultimedia(long Pid, int size, FileBindTypeEnum bindType)
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

                var fbs = await (db.FileBinds.Where(e => e.Sid == Pid && e.type == (int)bindType).Where(e => !e.IsDeleted)).ToListAsync();

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
                                        var file_link = await this.getImgFilesById(ids, size);

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
                        if (orgName != "")
                        {
                            output[i].Link[j] = output[i].Link[j].Replace("upload", $"upload/{orgName}").Replace($"/{orgName}/{orgName}/", $"/{orgName}/");
                        }
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return output;
            }
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
        public async Task<ResponseMessageDto> fileSortChange(FileChangeSortDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var checkStatus = new List<int> { (int)FileBindTypeEnum.產品, (int)FileBindTypeEnum.產品規格圖, (int)FileBindTypeEnum.產品檔案, (int)FileBindTypeEnum.文章檔案 };
                var userid = await loginUserData.GetUserId();
                var db_fb = await db.FileBinds.Where(e => !e.IsDeleted && e.Sid == dto.sid && e.FK_FileUploadId == dto.id)
                    .Where(e => checkStatus.Contains(e.type))
                    .FirstOrDefaultAsync();
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
        public async Task<ResponseMessageDto> fileDataChange(FileDataChangeDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var userid = await loginUserData.GetUserId();
                var db_fb = await db.FileBinds.Where(e => !e.IsDeleted && e.Sid == dto.SId && e.FK_FileUploadId == dto.Id)
                    .Where(e => e.type == (int)FileBindTypeEnum.文章檔案)
                    .FirstOrDefaultAsync();
                if (db_fb != null)
                {
                    if (dto.SerNo.HasValue) db_fb.SerNo = dto.SerNo.Value;
                    if (dto.FileName != null) db_fb.Name = dto.FileName;
                    if (dto.AreaKey != null) db_fb.AreaKey = dto.AreaKey;
                    if (dto.IsVisible.HasValue) db_fb.IsVisible = dto.IsVisible.Value;
                    db_fb.LastModifierUserId = userid;
                    db_fb.LastModificationTime = DateTime.Now;

                    db.SaveChanges();
                    response.Success = true;
                }
                else throw new Exception("查無對應FileBinds");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
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
            await loginUserData.SetLogs(path, JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> deleteFile(Guid key)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                string orgName = await loginUserData.GetWebsiteOrgName();

                var files = await db.FileUploads
                    .Where(e => e.GuidKey == key)
                    .Where(e => !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (files != null)
                {
                    var physicalPath = uploadPathResolver.GetPhysicalPathFromDownloadFileName(
                        orgName,
                        files.DownloadFileName ?? ""
                    );

                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                    }

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
            await loginUserData.SetLogs(key.ToString(), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> deleteFileById(FileDeleteDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            response.Success = true;

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();
                if (dto.Fid != null && dto.Fid.Count > 0)
                {
                    List<FileBind> fafile_other;
                    FileBind? fafile_binds;
                    for (var i = 0; i < dto.Fid.Count; i++)
                    {
                        fafile_other = await (db.FileBinds.Where(e => e.FK_FileUploadId == dto.Fid[i] && e.type == dto.Type && e.Sid != dto.Sid)).ToListAsync();
                        fafile_binds = await db.FileBinds.Where(e => e.FK_FileUploadId == dto.Fid[i] && e.type == dto.Type && e.Sid == dto.Sid).FirstOrDefaultAsync();

                        if (fafile_other.Count > 0 && fafile_binds != null)
                        {
                            fafile_binds.IsDeleted = true;
                            fafile_binds.DeletionTime = DateTime.Now;
                            fafile_binds.DeleterUserId = usetId;
                            db.SaveChanges();
                        }
                        else
                        {
                            var fafile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == dto.Fid[i]).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                            if (fafile != null)
                            {
                                if (fafile_binds != null)
                                {
                                    var chfile_binds = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == fafile.GuidKey && e.type == (int)FileBindMoreEnum.壓縮圖片).Where(e => !e.IsDeleted).ToListAsync());
                                    if (chfile_binds != null)
                                    {
                                        foreach (var chfile_bind in chfile_binds)
                                        {
                                            var chfile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == chfile_bind.FK_FileUploadId).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
                                            if (chfile != null && response.Success)
                                            {
                                                response = await this.deleteFile(chfile.GuidKey);
                                                if (response.Success)
                                                {
                                                    chfile.IsDeleted = true;
                                                    chfile.DeletionTime = DateTime.Now;
                                                    chfile.DeleterUserId = usetId;

                                                    chfile_bind.IsDeleted = true;
                                                    chfile_bind.DeletionTime = DateTime.Now;
                                                    chfile_bind.DeleterUserId = usetId;

                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                    fafile_binds.IsDeleted = true;
                                    fafile_binds.DeletionTime = DateTime.Now;
                                    fafile_binds.DeleterUserId = usetId;
                                    db.SaveChanges();
                                }
                                response = await this.deleteFile(fafile.GuidKey);
                                if (response.Success)
                                {
                                    fafile.IsDeleted = true;
                                    fafile.DeletionTime = DateTime.Now;
                                    fafile.DeleterUserId = usetId;
                                    db.SaveChanges();
                                }
                            }
                            else if (fafile_binds != null)
                            {
                                fafile_binds.IsDeleted = true;
                                fafile_binds.DeletionTime = DateTime.Now;
                                fafile_binds.DeleterUserId = usetId;
                                db.SaveChanges();
                            }
                        }
                        await loginUserData.SetLogs(dto.Fid.ToString(), JsonConvert.SerializeObject(response));

                        var websiteid = await loginUserData.GetWebsiteId();
                        switch (dto.Type)
                        {
                            case (int)FileBindTypeEnum.網站圖示:
                                var website_icon = await db.Websites.Where(e => e.Id == dto.Sid).FirstOrDefaultAsync();
                                if (website_icon != null) website_icon.Icon = null;
                                break;
                            case (int)FileBindTypeEnum.網站Logo:
                                var website_logo = await db.Websites.Where(e => e.Id == dto.Sid).FirstOrDefaultAsync();
                                if (website_logo != null) website_logo.Logo = null;
                                break;
                            case (int)FileBindTypeEnum.選單圖:
                                var db_bind = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                                if (db_bind != null) db_bind.ImgId = null;
                                break;
                            case (int)FileBindTypeEnum.選單覆蓋:
                                var db_bind_over = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                                if (db_bind_over != null) db_bind_over.OverImgId = null;
                                break;
                            case (int)FileBindTypeEnum.右側浮動廣告:
                            case (int)FileBindTypeEnum.進入廣告:
                                var db_html = await db.Html_Contents.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                                if (db_html != null) db_html.Img = null;
                                break;
                            case (int)FileBindTypeEnum.選單Icon:
                                var db_menuicon = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
                                if (db_menuicon != null) db_menuicon.icon = "empty";
                                break;
                        }
                    }
                    db.SaveChanges();
                    return response;
                }
                else
                {
                    var result = new List<long?>();
                    if (dto.Type == (int)FileBindTypeEnum.自訂廣告)
                    {
                        result = await (from fb in db.FileBinds
                                        where fb.Sid == dto.Sid && !fb.IsDeleted
                                        where fb.type == dto.Type
                                        select fb.FK_FileUploadId).ToListAsync();
                    }
                    else
                    {
                        result = await (from fb in db.FileBinds
                                        where fb.Sid == dto.Sid && !fb.IsDeleted
                                        select fb.FK_FileUploadId).ToListAsync();
                    }

                    if (result.Count > 0)
                    {
                        var fid_list = new List<long>();
                        result.ForEach(fid =>
                        {
                            fid_list.Add((long)fid);
                        });

                        response = await this.deleteFileById(new FileDeleteDto
                        {
                            Sid = dto.Sid,
                            Fid = fid_list,
                            Type = dto.Type
                        });
                        return response;
                    }
                    else
                    {
                        response.Error = "Fid為0";
                        response.Success = true;
                        return response;
                    }
                }
            }
            catch (Exception e)
            {
                response.Error = e.Message;
                response.Success = false;
                return response;
            }

        }
        public async Task<ResponseMessageDto> insertNotFondFile(InsertNotFoundFileDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            long websiteId = dto.FK_WebsiteID == 0 ? await loginUserData.GetWebsiteId() : dto.FK_WebsiteID;
            bool haveUri = db.NotFoundImage.Where(e => e.FK_WebsiteId == websiteId && e.Url == dto.Url).Any();
            if (!string.IsNullOrEmpty(dto.Url) && !haveUri)
            {
                db.NotFoundImage.Add(new NotFoundImage
                {
                    FK_WebsiteId = websiteId,
                    Url = dto.Url,
                    From = dto.From
                });
                await db.SaveChangesAsync();
            }
            response.Success = true;
            return response;
        }
        private class ProductListCompressedImageRow
        {
            public Guid GuidKey { get; set; }
            public string? Path { get; set; }
            public long? Size { get; set; }
        }
        public async Task<Dictionary<long, string>> GetMinImageMapAsync(List<long> prodIds)
        {
            if (prodIds == null || prodIds.Count == 0)
                return new Dictionary<long, string>();

            long webid = await loginUserData.GetWebsiteId();
            string orgName = await loginUserData.GetWebsiteOrgName();

            // 把「網站邊界」綁死在查詢裡，避免跨站撈圖
            var siteProdIds =
                from p in db.Prods
                where p.FK_WebsiteId == webid
                   && !p.IsDeleted
                   && prodIds.Contains(p.Id)
                select p.Id;

            // 先抓「商品綁定的原始圖片」(type=產品)
            // 重點：
            // 1. 只取 FileUpload.ContentType 為 image/* 的資料
            // 2. 排除 youtube、video/*、一般檔案
            // 3. 依 FileBinds.SerNo / CreationTime / Id 排序
            var imageBinds = await (
                from fb in db.FileBinds.AsNoTracking()
                join f in db.FileUploads.AsNoTracking() on fb.FK_FileUploadId equals f.Id
                join pid in siteProdIds on fb.Sid equals pid
                where fb.type == (int)FileBindTypeEnum.產品
                   && !fb.IsDeleted
                   && !f.IsDeleted
                   && f.FK_WebsiteId == webid
                   && f.ContentType != null
                   && f.ContentType.StartsWith("image/")
                orderby fb.Sid, fb.SerNo, fb.CreationTime, fb.Id
                select new
                {
                    ProdId = fb.Sid,
                    GuidKey = f.GuidKey,
                    OriginalPath = f.DownloadFileName,
                    SerNo = fb.SerNo,
                    CreationTime = fb.CreationTime,
                    BindId = fb.Id
                }
            ).ToListAsync();

            // 每個商品挑一張「要顯示的圖」：優先商品圖，商品圖沒有時退規格圖
            var firstImageMap = new Dictionary<long, (Guid GuidKey, string? OriginalPath)>();

            foreach (var g in imageBinds.GroupBy(x => x.ProdId))
            {
                var first = g
                    .OrderBy(x => x.SerNo)
                    .ThenBy(x => x.CreationTime)
                    .ThenBy(x => x.BindId)
                    .First();
                firstImageMap[g.Key] = (first.GuidKey, first.OriginalPath);
            }

            // 沒有商品圖的商品，退而使用其規格列(Prod_Stock)上的第一張規格圖(type=產品規格圖)
            var missingProdIds = prodIds
                .Where(id => !firstImageMap.ContainsKey(id))
                .Distinct()
                .ToList();

            if (missingProdIds.Count > 0)
            {
                var specImageBinds = await (
                    from s in db.Prod_Stocks.AsNoTracking()
                    join pid in siteProdIds on s.FK_Pid equals pid
                    join fb in db.FileBinds.AsNoTracking() on s.Id equals fb.Sid
                    join f in db.FileUploads.AsNoTracking() on fb.FK_FileUploadId equals f.Id
                    where !s.IsDeleted
                       && missingProdIds.Contains(s.FK_Pid)
                       && fb.type == (int)FileBindTypeEnum.產品規格圖
                       && !fb.IsDeleted
                       && !f.IsDeleted
                       && f.FK_WebsiteId == webid
                       && f.ContentType != null
                       && f.ContentType.StartsWith("image/")
                    select new
                    {
                        ProdId = s.FK_Pid,
                        GuidKey = f.GuidKey,
                        OriginalPath = f.DownloadFileName,
                        StockSerNo = s.Ser_No,
                        SerNo = fb.SerNo,
                        CreationTime = fb.CreationTime,
                        BindId = fb.Id
                    }
                ).ToListAsync();

                // 先依規格列順序(Ser_No)，再依規格圖 SerNo，取整個商品的第一張規格圖
                foreach (var g in specImageBinds.GroupBy(x => x.ProdId))
                {
                    var first = g
                        .OrderBy(x => x.StockSerNo)
                        .ThenBy(x => x.SerNo)
                        .ThenBy(x => x.CreationTime)
                        .ThenBy(x => x.BindId)
                        .First();
                    firstImageMap[g.Key] = (first.GuidKey, first.OriginalPath);
                }
            }

            if (firstImageMap.Count == 0)
                return new Dictionary<long, string>();

            // 若有壓縮圖，列表優先使用壓縮圖；但選哪一張圖仍以上面挑出的第一張為準
            var guidKeys = firstImageMap.Values
                .Where(x => x.GuidKey != Guid.Empty)
                .Select(x => x.GuidKey)
                .Distinct()
                .ToList();

            var compressedRows = new List<ProductListCompressedImageRow>();

            if (guidKeys.Count > 0)
            {
                compressedRows = await (
                    from m in db.FileBindMores.AsNoTracking()
                    join fc in db.FileUploads.AsNoTracking() on m.FK_FileUploadId equals fc.Id
                    where !m.IsDeleted
                       && !fc.IsDeleted
                       && m.type == (int)FileBindMoreEnum.壓縮圖片
                       && guidKeys.Contains(m.FK_FileBindGuid)
                       && fc.ContentType != null
                       && fc.ContentType.StartsWith("image/")
                    select new ProductListCompressedImageRow
                    {
                        GuidKey = m.FK_FileBindGuid,
                        Path = fc.DownloadFileName,
                        Size = fc.Size
                    }
                ).ToListAsync();
            }

            var compressedMap = compressedRows
                .GroupBy(x => x.GuidKey)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderBy(x => x.Size ?? long.MaxValue)
                        .Select(x => x.Path)
                        .FirstOrDefault()
                );

            return firstImageMap.ToDictionary(
                x => x.Key,
                x =>
                {
                    var (guidKey, originalPath) = x.Value;
                    var path = originalPath;

                    if (guidKey != Guid.Empty &&
                        compressedMap.TryGetValue(guidKey, out var compressedPath) &&
                        !string.IsNullOrWhiteSpace(compressedPath))
                    {
                        path = compressedPath;
                    }

                    return BuildBackendUploadPath(path, orgName, "/images/noImg.jpg");
                }
            );
        }
        private string BuildBackendUploadPath(string? path, string orgName, string fallback = "/images/noImg.jpg")
        {
            if (string.IsNullOrWhiteSpace(path)) return fallback;

            var p = path.Trim().Replace("\\", "/");

            // 絕對 URL 或 data URI 直接通過
            if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return p;

            const string root = "/upload/";
            if (!p.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return p; // 非 /upload/ 開頭就不處理（例如 /images/noImg.jpg）

            // 已經含 orgName 就不重複加
            var after = p.Substring(root.Length);
            if (after.StartsWith(orgName + "/", StringComparison.OrdinalIgnoreCase))
                return p;

            return (root + orgName + "/" + after).Replace("//", "/");
        }
        private async Task<List<FileItemDto>> SaveFile(IList<IFormFile> files, string filename, string areakey, int asotype, int serno, string directory, long id, long sid, bool isVisible, bool isEncryption)
        {
            List<FileItemDto> outputs = new List<FileItemDto>();
            List<FileBind> fileBinds = new List<FileBind>();
            long userId = await loginUserData.GetUserId();
            if (files.Any())
            {
                foreach (var file in files)
                {
                    outputs.Add(await SaveFile(file, areakey, directory, false, true, id, isEncryption));
                }
                ;
            }
            else if (id != 0)
            {
                outputs.Add(await SaveFile(null, "", directory, false, true, id, isEncryption));
            }
            outputs.ForEach(e =>
            {
                if (e.Id != 0)
                {
                    FileBind fb = new FileBind
                    {
                        Guid = Guid.NewGuid(),
                        Name = string.IsNullOrWhiteSpace(filename) ? e.Name : filename,
                        type = asotype,
                        Sid = sid,
                        num = 1,
                        SerNo = serno,
                        MediaLink = "",
                        FK_FileUploadId = e.Id,
                        IsVisible = isVisible,
                        AreaKey = areakey
                    };
                    loginUserData.setOptionParameter(fb, userId);
                    fileBinds.Add(fb);
                }
            });
            db.FileBinds.AddRange(fileBinds);
            db.SaveChanges();
            return outputs;
        }
        private async Task<FileItemDto> SaveFile(IFormFile? file, string areakey, string directory, bool isTemp = false, bool convert = true, long id = 0, bool isEncryption = false)
        {
            if ((file != null && file.Length > 0) || id > 0)
            {
                string orgName = await loginUserData.GetWebsiteOrgName();
                Guid key = Guid.NewGuid();
                var rootPath = uploadPathResolver.GetRootPath(orgName);
                var directoryPath = uploadPathResolver.GetDirectoryPath(orgName, directory);

                if ((file != null && file.Length > 0))
                {
                    string[] sp = file.FileName.Split('.');
                    string ext = sp[sp.Length - 1];
                    var path = $"/{directory}/{key}.{ext}";

                    if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception("Type Error");
                    if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
                    using (var stream = file.OpenReadStream())
                    {
                        string ContentType = file.ContentType;
                        long fileLength = file.Length;
                        if (convert && IsAllowedFileType(file.ContentType))
                        {
                            var fileInfo = await ConvertImageAsync(stream, directoryPath, directory, key.ToString());
                            path = fileInfo.Path;
                            ContentType = fileInfo.ContentType;
                            fileLength = fileInfo.FileLength;
                        }
                        else
                        {
                            var physicalPath = uploadPathResolver.GetPhysicalPath(orgName, path);
                            using (var fileStream = new FileStream(physicalPath, FileMode.Create))
                            {
                                if (isEncryption) await EncryptAndSaveAsync(fileStream, stream);
                                else await file.CopyToAsync(fileStream);
                            }
                        }
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
                            try
                            {
                                FileUpload fileUpload = new FileUpload
                                {
                                    FK_WebsiteId = await loginUserData.GetWebsiteId(),
                                    FileGuid = key,
                                    GuidKey = Guid.NewGuid(),
                                    DownloadFileName = $@"/upload{path}",
                                    OriginalFileName = file.FileName,
                                    ContentType = ContentType,
                                    Size = fileLength,
                                    IsEncryption = isEncryption,
                                };

                                db.FileUploads.Add(fileUpload);
                                await loginUserData.SaveChanges(fileUpload);
                                return new FileItemDto
                                {
                                    Id = fileUpload.Id,
                                    Name = fileUpload.OriginalFileName,
                                    Path = fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
                                    Guid = fileUpload.GuidKey,
                                };
                            }
                            catch (Exception e)
                            {
                                return new FileItemDto
                                {
                                    Id = 0,
                                    Name = file.FileName
                                };
                            }
                        }
                    }
                }
                else if (id != 0 && isEncryption)
                {
                    var siteId = configuration.GetValue<long>("WebConfig:SiteId");
                    var website = await db.Websites.Where(e => e.Id == siteId).FirstOrDefaultAsync();

                    var fileUpload = await db.FileUploads.Where(f => f.Id == id).FirstOrDefaultAsync();
                    if (fileUpload == null) throw new Exception("查無檔案");
                    if (fileUpload.IsEncryption) throw new Exception("原檔案已加密");

                    string[] sp = fileUpload.OriginalFileName.Split('.');
                    string ext = sp[sp.Length - 1];
                    var path = $"/{directory}/{key}.{ext}";

                    string physicalPath = uploadPathResolver.GetPhysicalPathFromDownloadFileName(
                        orgName,
                        fileUpload.DownloadFileName ?? ""
                    );

                    if (!System.IO.File.Exists(physicalPath)) throw new Exception("查無檔案位置");

                    var newPhysicalPath = uploadPathResolver.GetPhysicalPath(orgName, path);

                    long filelength;
                    using (var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                    using (var fileStream_temp = new FileStream(newPhysicalPath, FileMode.Create, FileAccess.Write))
                    {
                        filelength = fileStream.Length;
                        await EncryptAndSaveAsync(fileStream_temp, fileStream);
                    }

                    if (isTemp)
                    {
                        return new FileItemDto
                        {
                            Name = file.FileName,
                            Path = fileUpload.DownloadFileName,
                            Guid = Guid.NewGuid()
                        };
                    }
                    else
                    {
                        try
                        {
                            FileUpload new_fileUpload = new FileUpload
                            {
                                FK_WebsiteId = await loginUserData.GetWebsiteId(),
                                FileGuid = key,
                                GuidKey = Guid.NewGuid(),
                                DownloadFileName = $@"/upload{path}",
                                OriginalFileName = fileUpload.OriginalFileName,
                                ContentType = fileUpload.ContentType,
                                Size = filelength,
                                IsEncryption = isEncryption
                            };

                            db.FileUploads.Add(new_fileUpload);
                            await loginUserData.SaveChanges(new_fileUpload);

                            await deleteFile(fileUpload.GuidKey);

                            return new FileItemDto
                            {
                                Id = new_fileUpload.Id,
                                Name = new_fileUpload.OriginalFileName,
                                Path = new_fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
                                Guid = new_fileUpload.GuidKey,
                            };
                        }
                        catch (Exception e)
                        {
                            return new FileItemDto
                            {
                                Id = 0,
                                Name = file.FileName
                            };
                        }
                    }
                }
                else throw new Exception("上傳失敗");
            }
            else throw new Exception("上傳失敗");
        }
        private async Task<List<FileItemDto>> SaveImage(IList<IFormFile> files, int asotype, int bindtype, int serno, string directory, long sid, bool convert = true)
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
                        var rootPath = uploadPathResolver.GetRootPath(orgName);
                        var directoryPath = uploadPathResolver.GetDirectoryPath(orgName, directory);
                        var path = asotype == (int)FileBindTypeEnum.網站圖示 ? $"/favicon.ico" : $"/{directory}/{key}.{ext}";
                        if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception();
                        if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
                        else if (asotype == (int)FileBindTypeEnum.網站圖示)
                        {
                            string fullPath = uploadPathResolver.GetPhysicalPath(orgName, path);
                            if (File.Exists(fullPath)) File.Delete(fullPath);
                        }
                        using (var stream = file.OpenReadStream())
                        {
                            string ContentType = file.ContentType;
                            long fileLength = file.Length;
                            if (convert && asotype != (int)FileBindTypeEnum.網站圖示 && asotype != (int)FileBindTypeEnum.分享圖示 && IsAllowedFileType(file.ContentType))
                            {
                                var fileInfo = await ConvertImageAsync(stream, directoryPath, directory, key.ToString());
                                path = fileInfo.Path;
                                ContentType = fileInfo.ContentType;
                                fileLength = fileInfo.FileLength;
                            }
                            else
                            {
                                var physicalPath = uploadPathResolver.GetPhysicalPath(orgName, path);
                                using (var fileStream = new FileStream(physicalPath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }
                            }

                            FileUpload fileUpload = new FileUpload
                            {
                                FK_WebsiteId = await loginUserData.GetWebsiteId(),
                                FileGuid = key,
                                GuidKey = Guid.NewGuid(),
                                DownloadFileName = $@"/upload{path}",
                                OriginalFileName = file.FileName,
                                ContentType = ContentType,
                                Size = fileLength
                            };
                            db.FileUploads.Add(fileUpload);
                            await loginUserData.SaveChanges(fileUpload);
                            if (faimg_id != Guid.Empty)
                            {
                                FileBindMore fileBindMore = new FileBindMore
                                {
                                    type = bindtype,
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
                                    type = asotype,
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
                                Path = fileUpload.DownloadFileName,
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
        private bool IsAllowedFileType(string contentType)
        {
            string[] allowedTypes = {
                "image/jpeg",
                "image/jpg",
                "image/pjpeg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/avif",
                "image/bmp",
                "image/heic",
                "image/heif"
            };
            return Array.Exists(allowedTypes, type => type == contentType);
        }
        private async Task<ImageConversionResultDto> ConvertImageAsync(Stream inputStream, string directoryPath, string baseUrlPath, string key)
        {
            // 複製到MemoryStream確保可重複使用
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);
            var originalSize = memoryStream.Length;

            memoryStream.Position = 0;
            using var original = new MagickImage(memoryStream);
            var originalFormat = original.Format;
            var extension = GetExtension(originalFormat);
            var contentType = GetMimeType(originalFormat);
            var fallbackPath = Path.Combine(directoryPath, $"{key}{extension}");

            // 若為 AVIF，直接儲存
            if (originalFormat == MagickFormat.Avif)
            {
                var avifPath = Path.Combine(directoryPath, $"{key}.avif");
                await original.WriteAsync(avifPath);
                return new ImageConversionResultDto
                {
                    Path = $"/{baseUrlPath}/{key}.avif",
                    ContentType = "image/avif",
                    FileLength = new FileInfo(avifPath).Length
                };
            }

            // 若檔案小於40KB，直接存原始
            if (originalSize < 40 * 1024)
            {
                memoryStream.Position = 0;
                using (var fs = File.Create(fallbackPath))
                {
                    await memoryStream.CopyToAsync(fs);
                }

                return new ImageConversionResultDto
                {
                    Path = $"/{baseUrlPath}/{key}{extension}",
                    ContentType = contentType,
                    FileLength = originalSize
                };
            }

            // 嘗試轉 AVIF
            var avifPathTry = Path.Combine(directoryPath, $"{key}.avif");
            original.Format = MagickFormat.Avif;

            if (originalSize < 250 * 1024)
                original.Quality = 88;
            else if (originalSize < 500 * 1024)
                original.Quality = 80;
            else
                original.Quality = 73;

            original.Settings.SetDefine(MagickFormat.Avif, "lossless", "true");
            original.Settings.SetDefine(MagickFormat.Avif, "chroma-subsampling", "4:4:4");

            await original.WriteAsync(avifPathTry);

            var avifSize = new FileInfo(avifPathTry).Length;

            if (avifSize < originalSize)
            {
                return new ImageConversionResultDto
                {
                    Path = $"/{baseUrlPath}/{key}.avif",
                    ContentType = "image/avif",
                    FileLength = avifSize
                };
            }

            // AVIF未壓縮成功，刪除
            File.Delete(avifPathTry);

            // 儲存原始格式
            memoryStream.Position = 0;
            using (var fs = File.Create(fallbackPath))
            {
                await memoryStream.CopyToAsync(fs);
            }

            return new ImageConversionResultDto
            {
                Path = $"/{baseUrlPath}/{key}{extension}",
                ContentType = contentType,
                FileLength = originalSize
            };
        }
        private string GetExtension(MagickFormat format)
        {
            return format switch
            {
                MagickFormat.Jpeg => ".jpg",
                MagickFormat.Png => ".png",
                MagickFormat.WebP => ".webp",
                MagickFormat.Gif => ".gif",
                MagickFormat.Bmp => ".bmp",
                MagickFormat.Avif => ".avif",
                MagickFormat.Heic => ".heic",
                MagickFormat.Heif => ".heif",
                _ => ".img"
            };
        }
        private string GetMimeType(MagickFormat format)
        {
            return format switch
            {
                MagickFormat.Jpeg => "image/jpeg",
                MagickFormat.Png => "image/png",
                MagickFormat.WebP => "image/webp",
                MagickFormat.Gif => "image/gif",
                MagickFormat.Bmp => "image/bmp",
                MagickFormat.Avif => "image/avif",
                MagickFormat.Heic => "image/heic",
                MagickFormat.Heif => "image/heif",
                _ => "application/octet-stream"
            };
        }
        private async Task<ResponseMessageDto> EncryptAndSaveAsync(FileStream fileStream, Stream stream)
        {
            var response = new ResponseMessageDto();

            try
            {
                byte[] fileKey = RandomNumberGenerator.GetBytes(32);
                byte[] nonce = RandomNumberGenerator.GetBytes(12);

                const int TagSize = 16;
                using var aes = new AesGcm(fileKey, TagSize);

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] plaintext = ms.ToArray();
                byte[] ciphertext = new byte[plaintext.Length];

                byte[] tag = new byte[16];
                aes.Encrypt(nonce, plaintext, ciphertext, tag);

                // 加密資料存入Header
                // 1. magic = 識別碼 判斷是否為加密檔(File ENCryption)以及其版本(1)
                // 2. 加密的版本
                // 3. nonce
                // 4. tag
                // 5. fileKey的長度
                // 6. fileKey

                byte[] magic = System.Text.Encoding.ASCII.GetBytes("FENC1");
                await fileStream.WriteAsync(magic);
                await fileStream.WriteAsync(new byte[] { 1 });
                await fileStream.WriteAsync(nonce);
                await fileStream.WriteAsync(tag);
                byte[] encryptedFileKey = ProtectedFileKey(fileKey);
                byte[] keyLengthBytes = BitConverter.GetBytes(encryptedFileKey.Length);
                await fileStream.WriteAsync(keyLengthBytes);
                await fileStream.WriteAsync(encryptedFileKey);

                await fileStream.WriteAsync(ciphertext);

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private byte[] ProtectedFileKey(byte[] fileKey)
        {
            using var aes = Aes.Create();

            var MasterKey = configuration.GetValue<string>("Security:FILE_MASTER_KEY");
            if (string.IsNullOrEmpty(MasterKey)) throw new Exception("FILE_MASTER_KEY 未設定");

            byte[] masterKey = Convert.FromBase64String(MasterKey);
            if (masterKey.Length != 32) throw new Exception("FILE_MASTER_KEY 長度錯誤，必須是 32 bytes");

            aes.Key = masterKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var cipher = encryptor.TransformFinalBlock(fileKey, 0, fileKey.Length);

            return aes.IV.Concat(cipher).ToArray();
        }
        public async Task<DownloadPayload> DecryptFile(long fid)
        {
            var response = new DownloadPayload();
            var siteId = configuration.GetValue<long>("WebConfig:SiteId");

            var isFront = siteId != 0;

            try
            {
                var isLogin = false;
                if (isFront) isLogin = (await tokenAppService.CheckToken(null)).IsLogin;
                else
                {
                    siteId = await loginUserData.GetWebsiteId();
                    isLogin = loginUserData.IsLoggedIn();
                }

                var website = await db.Websites.Where(e => e.Id == siteId).FirstOrDefaultAsync();

                if (!isLogin) throw new Exception("加密檔需登入後才可進行預覽或下載");

                var fileUpload = await db.FileUploads.Where(f => f.Id == fid).FirstOrDefaultAsync();
                if (fileUpload == null) throw new Exception("查無檔案");
                response.ContentType = fileUpload.ContentType;
                response.FileName = fileUpload.OriginalFileName;

                if (website == null || string.IsNullOrWhiteSpace(website.OrgName))
                    throw new Exception("查無網站 OrgName");

                string physicalPath = uploadPathResolver.GetPhysicalPathFromDownloadFileName(
                    website.OrgName,
                    fileUpload.DownloadFileName ?? ""
                );

                if (!System.IO.File.Exists(physicalPath)) throw new Exception("查無檔案位置");

                if (!fileUpload.IsEncryption)
                {
                    response.IsEncryptedFile = false;
                    var fs = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    response.PhysicalPath = physicalPath;
                }
                else
                {
                    response.IsEncryptedFile = true;
                    using var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);

                    byte[] magic = new byte[5];
                    fileStream.Read(magic, 0, 5);
                    string magicString = Encoding.UTF8.GetString(magic);

                    if (magicString != "FENC1") throw new Exception("檔案格式不正確");

                    int version = fileStream.ReadByte();

                    byte[] nonce = new byte[12];
                    fileStream.Read(nonce, 0, 12);

                    byte[] tag = new byte[16];
                    fileStream.Read(tag, 0, 16);

                    byte[] keyLengthBytes = new byte[4];
                    fileStream.Read(keyLengthBytes, 0, 4);

                    int keyLength = BitConverter.ToInt32(keyLengthBytes, 0);

                    byte[] encryptedFileKey = new byte[keyLength];
                    fileStream.Read(encryptedFileKey, 0, keyLength);

                    long cipherLength = fileStream.Length - fileStream.Position;
                    byte[] ciphertext = new byte[cipherLength];
                    fileStream.Read(ciphertext, 0, (int)cipherLength);

                    byte[] fileKey = UnprotectFileKey(encryptedFileKey);
                    byte[] plaintext = new byte[ciphertext.Length];
                    using var aes = new AesGcm(fileKey, tagSizeInBytes: 16);
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);

                    response.Bytes = plaintext;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
        private byte[] UnprotectFileKey(byte[] encrypted)
        {
            using var aes = Aes.Create();

            var MasterKey = configuration.GetValue<string>("Security:FILE_MASTER_KEY");
            if (string.IsNullOrEmpty(MasterKey)) throw new Exception("FILE_MASTER_KEY 未設定");

            byte[] masterKey = Convert.FromBase64String(MasterKey);
            if (masterKey.Length != 32) throw new Exception("FILE_MASTER_KEY 長度錯誤，必須是 32 bytes");

            aes.Key = masterKey;

            byte[] iv = encrypted[..16];
            byte[] cipher = encrypted[16..];
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        }
    }
}
