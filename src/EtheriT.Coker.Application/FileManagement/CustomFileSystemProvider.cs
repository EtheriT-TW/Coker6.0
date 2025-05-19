using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.FileManagement
{
    public class CustomFileSystemProvider : PhysicalFileSystemProvider
    {
        private readonly CokerDbContext _dbContext;
        private readonly long _userId;
        private readonly string _downloadFilePath;

        public CustomFileSystemProvider(string rootDirectoryPath,
                                        CokerDbContext dbContext,
                                        string orgName,
                                        long userId)
            : base(rootDirectoryPath)
        {
            _dbContext = dbContext;
            _userId = userId;
            _downloadFilePath = $"/upload/{orgName}";
        }

        public CustomFileSystemProvider(string rootDirectoryPath,
                                        Action<FileSystemInfo, FileSystemItem> prepareFileSystemItemCallback,
                                        CokerDbContext dbContext,
                                        string orgName,
                                        long userId)
            : base(rootDirectoryPath, prepareFileSystemItemCallback)
        {
            _dbContext = dbContext;
            _userId = userId;
            _downloadFilePath = $"/upload/{orgName}";
        }

        public override void DeleteItem(FileSystemDeleteItemOptions options)
        {
            string path = options.Item.Path;
            string fullPath = Path.Combine(RootDirectoryPath, PreparePath(path));

            // 擷取檔名，GuidKey 通常是檔名的一部分
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(fileName))
            {
                // 嘗試解析檔名中的 Guid
                if (Guid.TryParse(fileName, out Guid guidKey))
                {
                    // 更新資料庫中的紀錄
                    var fileUpload = _dbContext.FileUploads.FirstOrDefault(f => f.GuidKey == guidKey);
                    if (fileUpload != null)
                    {
                        fileUpload.IsDeleted = true;
                        fileUpload.DeletionTime = DateTime.Now;
                        fileUpload.DeleterUserId = _userId;
                        _dbContext.SaveChanges();

                        // 不實際刪除檔案，而是只在資料庫中標記為刪除
                        return;
                    }
                }
            }

            // 如果找不到對應的資料庫記錄，則執行原始的刪除方法
            base.DeleteItem(options);
        }

        public override void MoveItem(FileSystemMoveItemOptions options)
        {
            string sourcePath = options.Item.Path;
            string destDirectoryPath = options.DestinationDirectory.Path;

            string fullSourcePath = Path.Combine(RootDirectoryPath, PreparePath(sourcePath));
            string sourceFileName = Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(RootDirectoryPath, PreparePath(destDirectoryPath), sourceFileName);

            // 先執行實際的檔案搬移
            base.MoveItem(options);

            // 現在處理資料庫更新
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            if (Guid.TryParse(fileName, out Guid guidKey))
            {
                // 取得檔案新的資訊
                FileInfo fileInfo = new FileInfo(destFullPath);
                long fileSize = fileInfo.Length;
                string downloadFileName = Path.GetFileName(destFullPath);

                // 更新 FileUploads 表
                var fileUpload = _dbContext.FileUploads.FirstOrDefault(f => f.GuidKey == guidKey);
                if (fileUpload != null)
                {
                    fileUpload.DownloadFileName = downloadFileName;
                    fileUpload.Size = fileSize;
                    fileUpload.LastModifierUserId = _userId;
                    fileUpload.LastModificationTime = DateTime.Now;

                    // 更新 FileBinds 表中的 MediaLink
                    var fileBinds = _dbContext.FileBinds.Where(f => f.FK_FileUploadId == fileUpload.Id).ToList();
                    foreach (var fileBind in fileBinds)
                    {
                        // 假設 MediaLink 包含檔案路徑，需要更新
                        string mediaLink = fileBind.MediaLink;
                        // 取代舊路徑為新路徑
                        if (!string.IsNullOrEmpty(mediaLink))
                        {
                            string oldFileName = Path.GetFileName(sourcePath);
                            string newFileName = Path.GetFileName(destFullPath);
                            string newMediaLink = mediaLink.Replace(oldFileName, newFileName);
                            fileBind.MediaLink = newMediaLink;
                        }
                    }

                    _dbContext.SaveChanges();
                }
            }
        }
        // 提供一個公共方法來訪問 PreparePath，因為它在基類中是 private 的
        public string PreparePath(string path)
        {
            return path?.Replace('/', '\\').Trim('\\') ?? string.Empty;
        }

        public override void UploadFile(FileSystemUploadFileOptions options)
        {
            // 先執行原始的上傳檔案方法
            base.UploadFile(options);

            try
            {
                // 取得網站 ID
                long websiteId = GetWebsiteId();
                if (websiteId <= 0)
                {
                    return;
                }

                // 取得檔案資訊
                string destPath = Path.Combine(options.DestinationDirectory.Path, options.FileName);
                string fullPath = Path.Combine(RootDirectoryPath, PreparePath(destPath));
                FileInfo fileInfo = new FileInfo(fullPath);

                // 檢查檔案是否存在
                if (!fileInfo.Exists)
                {
                    return;
                }

                // 產生唯一識別碼，這會被用作 GuidKey
                Guid guidKey = Guid.NewGuid();

                // 建立 FileUpload 記錄
                FileUpload fileUpload = new FileUpload
                {
                    FK_WebsiteId = websiteId,
                    GuidKey = guidKey,
                    ContentType = GetContentType(fileInfo.Extension),
                    OriginalFileName = options.FileName,
                    DownloadFileName = Path.Combine(_downloadFilePath, options.FileName).Replace("\\", "/"),
                    Size = fileInfo.Length,
                    FileGuid = guidKey,
                    CreatorUserId = _userId,
                    IsDeleted = false,
                    CreationTime = DateTime.Now
                };

                // 將記錄添加到資料庫
                _dbContext.FileUploads.Add(fileUpload);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // 記錄錯誤，但不影響上傳功能
                System.Diagnostics.Debug.WriteLine($"Error inserting FileUpload record: {ex.Message}");
            }
        }

        // 根據檔案副檔名取得 ContentType
        private string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".zip":
                    return "application/zip";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }

        // 取得網站 ID
        private long GetWebsiteId()
        {
            try
            {
                // 從資料庫或其他方式獲取當前網站 ID
                // 這裡簡化處理，從目錄結構獲取網站 ID
                string orgName = Path.GetFileName(RootDirectoryPath);
                var website = _dbContext.Websites.FirstOrDefault(w => w.OrgName == orgName);
                return website?.Id ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public override void RenameItem(FileSystemRenameItemOptions options)
        {
            string path = options.Item.Path;
            bool isDirectory = options.Item.IsDirectory;
            string newName = options.ItemNewName;

            // 如果是目錄，使用基類方法處理
            if (isDirectory)
            {
                base.RenameItem(options);
                return;
            }

            // 取得原始檔案的完整路徑
            string fullPath = Path.Combine(RootDirectoryPath, PreparePath(path));

            // 取得檔名並嘗試解析為 Guid
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(fileName) && Guid.TryParse(fileName, out Guid guidKey))
            {
                // 先執行基本的重命名操作
                base.RenameItem(options);

                // 取得新檔案的資訊
                string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
                string newFullPath = Path.Combine(directory, newName);
                FileInfo fileInfo = new FileInfo(newFullPath);

                // 如果新檔案存在，更新資料庫記錄
                if (fileInfo.Exists)
                {
                    // 更新 FileUploads 表
                    var fileUpload = _dbContext.FileUploads.FirstOrDefault(f => f.GuidKey == guidKey);
                    if (fileUpload != null)
                    {
                        fileUpload.DownloadFileName = newName;
                        fileUpload.LastModifierUserId = _userId;
                        fileUpload.LastModificationTime = DateTime.Now;

                        // 更新 FileBinds 表中的 MediaLink
                        var fileBinds = _dbContext.FileBinds.Where(f => f.FK_FileUploadId == fileUpload.Id).ToList();
                        foreach (var fileBind in fileBinds)
                        {
                            // 假設 MediaLink 包含檔案路徑，需要更新
                            string mediaLink = fileBind.MediaLink;
                            // 取代舊檔名為新檔名
                            if (!string.IsNullOrEmpty(mediaLink))
                            {
                                string oldFileName = Path.GetFileName(path);
                                string newMediaLink = mediaLink.Replace(oldFileName, newName);
                                fileBind.MediaLink = newMediaLink;
                            }
                        }

                        _dbContext.SaveChanges();
                    }
                }
            }
            else
            {
                // 若無法識別為 Guid，則使用基類方法處理
                base.RenameItem(options);
            }
        }
    }
}
