using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EtheriT.Coker.Application.FileManagement
{
    public class CustomFileSystemProvider : PhysicalFileSystemProvider
    {
        private readonly CokerDbContext _dbContext;
        private readonly long _userId;
        private readonly string _downloadFilePath;
        private readonly IConfiguration _configuration;

        private readonly int _maxFileSizeMB = 0;

        public CustomFileSystemProvider(string rootDirectoryPath,
                                        CokerDbContext dbContext,
                                        string orgName,
                                        long userId,
                                        IConfiguration configuration)
            : base(rootDirectoryPath)
        {
            _dbContext = dbContext;
            _userId = userId;
            // _downloadFilePath 不要包含 orgName，需求確認於2025/5/26 by Charles LINE
            _downloadFilePath = $"/upload";
            _configuration = configuration;

            // 讀取最大檔案大小的設定，預設為 0，表示不限制
            int.TryParse(_configuration.GetValue<string>("VirtualDirectory:FileAllow:MaxSize"), out _maxFileSizeMB);
        }

        public CustomFileSystemProvider(Action<FileSystemInfo, FileSystemItem> prepareFileSystemItemCallback,
                                        string rootDirectoryPath,
                                        CokerDbContext dbContext,
                                        string orgName,
                                        long userId,
                                        IConfiguration configuration)
            : base(rootDirectoryPath, prepareFileSystemItemCallback)
        {
            _dbContext = dbContext;
            _userId = userId;
            // _downloadFilePath 不要包含 orgName，需求確認於2025/5/26 by Charles LINE
            _downloadFilePath = $"/upload";
            _configuration = configuration;
        }

        /// <summary>
        /// 覆寫取得檔案系統項目的方法，將資料庫中的檔案資訊加入到檔案系統項目中。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override IEnumerable<FileSystemItem> GetItems(FileSystemLoadItemOptions options)
        {
            var items = base.GetItems(options);

            var dbFileUploads = _dbContext.FileUploads
                .AsNoTracking()
                .Where(f => f.FK_WebsiteId == GetWebsiteId())
                .ToList();

            foreach (var item in items)
            {
                if (!item.IsDirectory)
                {
                    // 構建完整的相對路徑來進行精確匹配
                    string currentDirectory = options.Directory != null ? options.Directory.Path : string.Empty;
                    string itemPath = Path.Combine(currentDirectory, item.Name).Replace("\\", "/");

                    // 確保路徑格式一致以便比較
                    string downloadPathPrefix = _downloadFilePath.TrimEnd('/');

                    var matchingFileUpload = dbFileUploads.FirstOrDefault(x =>
                        x.DownloadFileName != null &&
                        !x.IsDeleted &&
                        (
                            // 精確比對完整路徑
                            x.DownloadFileName.EndsWith("/" + itemPath) ||
                            // 如果 DownloadFileName 包含下載路徑前綴和完整路徑
                            (x.DownloadFileName.StartsWith(downloadPathPrefix) &&
                             x.DownloadFileName.EndsWith(itemPath))
                        ));
                    if (matchingFileUpload != null)
                    {
                        item.CustomFields[nameof(matchingFileUpload.OriginalFileName)] = matchingFileUpload.OriginalFileName;
                        item.CustomFields[nameof(matchingFileUpload.DownloadFileName)] = matchingFileUpload.DownloadFileName;
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 覆寫刪除檔案的方法，將檔案從檔案系統中刪除，並在資料庫中標記為已刪除。
        /// </summary>
        /// <param name="options"></param>
        public override void DeleteItem(FileSystemDeleteItemOptions options)
        {
            //刪除的時候直接針對 FileUploads 做刪除即可
            //update FileUploads set IsDeleted = 1, DeletionTime = GETDATE(), DeleterUserId =? where GuidKey =?
            //需求確認於2025/5/16 by Charles LINE

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
                    var fileUpload = _dbContext.FileUploads.Include(x => x.fileBinds)
                                                           .FirstOrDefault(f => f.FileGuid == guidKey);
                    if (fileUpload != null)
                    {
                        fileUpload.fileBinds?.ForEach(fb =>
                        {
                            fb.IsDeleted = true;
                            fb.DeleterUserId = _userId;
                            fb.DeletionTime = DateTime.Now;
                        });
                        fileUpload.IsDeleted = true;
                        fileUpload.DeletionTime = DateTime.Now;
                        fileUpload.DeleterUserId = _userId;

                        var fileBindMores = _dbContext.FileBindMores
                       .Where(f => f.FK_FileUploadId == fileUpload.Id && !f.IsDeleted);
                        foreach (var fileBindMore in fileBindMores)
                        {
                            fileBindMore.IsDeleted = true;
                            fileBindMore.DeleterUserId = _userId;
                            fileBindMore.DeletionTime = DateTime.Now;
                        }

                        _dbContext.SaveChanges();
                    }
                }
            }

            // 如果找不到對應的資料庫記錄，則執行原始的刪除方法
            base.DeleteItem(options);
        }

        /// <summary>
        /// 覆寫移動檔案的方法，將檔案從來源目錄移動到目標目錄，並更新資料庫中的 FileUpload 記錄。
        /// </summary>
        /// <param name="options"></param>
        public override void MoveItem(FileSystemMoveItemOptions options)
        {            string sourcePath = options.Item.Path;
            string destDirectoryPath = options.DestinationDirectory.Path;

            string fullSourcePath = Path.Combine(RootDirectoryPath, PreparePath(sourcePath));
            string sourceFileName = Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(RootDirectoryPath, PreparePath(destDirectoryPath), sourceFileName);

            // 檢查目標檔案是否已存在
            if (File.Exists(destFullPath))
            {
                throw new FileSystemException(FileSystemErrorCode.FileExists, $"目標位置的檔案 {sourceFileName} 已存在");
            }

            // 先執行實際的檔案搬移
            base.MoveItem(options);

            // 現在處理資料庫更新
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            if (Guid.TryParse(fileName, out Guid guidKey))
            {                // 取得檔案新的資訊
                FileInfo fileInfo = new FileInfo(destFullPath);
                long fileSize = fileInfo.Length;
                string relativeDestPath = Path.GetRelativePath(RootDirectoryPath, destFullPath).Replace("\\", "/");
                string downloadFileName = Path.Combine(_downloadFilePath, relativeDestPath).Replace("\\", "/");

                // 更新 FileUploads 表
                var fileUpload = _dbContext.FileUploads.FirstOrDefault(f => f.FileGuid == guidKey);
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

        /// <summary>
        /// 覆寫上傳檔案的方法，將檔案上傳到指定的目錄並在資料庫中建立 FileUpload 記錄。
        /// 如果上傳的檔名已存在會蓋掉舊檔，並更新資料庫欄位。
        /// </summary>
        /// <param name="options"></param>
        public override void UploadFile(FileSystemUploadFileOptions options)
        {
            if (_maxFileSizeMB > 0 &&
                options.TempFile.Length > _maxFileSizeMB * 1024 * 1024)
            {
                throw new FileSystemException(FileSystemErrorCode.MaxFileSizeExceeded, $"檔案過大，不可超過{_maxFileSizeMB}MB");
            }

            try
            {
                // 獲取目標路徑和檔案名稱
                string targetDirectory = PreparePath(options.DestinationDirectory.Path);
                string targetFileName = Path.GetFileName(options.FileName);
                string targetFilePath = Path.Combine(RootDirectoryPath, targetDirectory, targetFileName);
                string relativePath = Path.Combine(targetDirectory, targetFileName).Replace("\\", "/");
                string downloadFilePath = Path.Combine(_downloadFilePath, relativePath).Replace("\\", "/");



                // 生成新的檔案 GUID
                Guid fileGuid = Guid.NewGuid();
                string extension = Path.GetExtension(targetFileName);
                string newFileName = fileGuid.ToString() + extension;
                string newFilePath = Path.Combine(RootDirectoryPath, targetDirectory, newFileName);

                if (File.Exists(targetFilePath))
                {
                    // 檔案已存在的情況，進行更新
                    // 1. 備份原始檔案
                    string backupFilePath = targetFilePath + ".bak";
                    if (File.Exists(backupFilePath))
                    {
                        File.Delete(backupFilePath);
                    }
                    File.Move(targetFilePath, backupFilePath);

                    try
                    {
                        // 2. 上傳新檔案
                        base.UploadFile(options);

                        // 3. 檢查是否存在相同下載路徑的檔案記錄
                        var existingFileUpload = _dbContext.FileUploads
                            .FirstOrDefault(f => f.DownloadFileName == downloadFilePath && !f.IsDeleted);
                        if (existingFileUpload != null)
                        {
                            // 4. 更新資料庫記錄
                            existingFileUpload.Size = new FileInfo(targetFilePath).Length;
                            existingFileUpload.LastModificationTime = DateTime.Now;
                            existingFileUpload.LastModifierUserId = _userId;
                        }

                        // 5. 刪除備份檔案
                        if (File.Exists(backupFilePath))
                        {
                            File.Delete(backupFilePath);
                        }

                        _dbContext.SaveChanges();
                    }
                    catch (IOException ex)
                    {
                        // 發生 IO 異常時還原備份
                        RestoreBackupFile(backupFilePath, targetFilePath);
                        throw new FileSystemException(FileSystemErrorCode.NoAccess, $"檔案 IO 錯誤：{ex.Message}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // 發生權限異常時還原備份
                        RestoreBackupFile(backupFilePath, targetFilePath);
                        throw new FileSystemException(FileSystemErrorCode.NoAccess, $"沒有足夠權限存取檔案：{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // 發生其他異常時還原備份
                        RestoreBackupFile(backupFilePath, targetFilePath);
                        throw new FileSystemException(FileSystemErrorCode.Unspecified, $"上傳檔案時發生錯誤：{ex.Message}");
                    }
                }
                else
                {
                    // 檔案不存在的情況，建立新記錄
                    // 1. 上傳檔案
                    base.UploadFile(options);

                    // 2. 重新命名為 GUID.副檔名 格式
                    if (File.Exists(targetFilePath))
                    {
                        if (File.Exists(newFilePath))
                        {
                            File.Delete(newFilePath);
                        }
                        File.Move(targetFilePath, newFilePath);
                    }

                    // 3. 建立資料庫記錄
                    // 建立新的下載路徑，使用 fileGuid + 副檔名
                    string guidDownloadFilePath = Path.Combine(_downloadFilePath, targetDirectory, newFileName).Replace("\\", "/");

                    var fileUpload = new FileUpload
                    {
                        FK_WebsiteId = GetWebsiteId(),
                        FileGuid = fileGuid,
                        GuidKey = Guid.NewGuid(),
                        DownloadFileName = guidDownloadFilePath,
                        OriginalFileName = options.FileName,
                        ContentType = GetContentType(extension),
                        Size = new FileInfo(newFilePath).Length,
                        CreatorUserId = _userId,
                        CreationTime = DateTime.Now
                    };

                    _dbContext.FileUploads.Add(fileUpload);
                    _dbContext.SaveChanges();
                }
            }
            catch (IOException ex)
            {
                // 處理 IO 相關的異常
                System.Diagnostics.Debug.WriteLine($"IO error uploading file: {ex.Message}");
                throw new FileSystemException(FileSystemErrorCode.NoAccess, $"檔案存取錯誤：{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                // 處理權限相關的異常
                System.Diagnostics.Debug.WriteLine($"Access error uploading file: {ex.Message}");
                throw new FileSystemException(FileSystemErrorCode.NoAccess, $"沒有足夠權限存取檔案：{ex.Message}");
            }
            catch (Exception ex)
            {
                // 處理其他上傳過程中的異常
                System.Diagnostics.Debug.WriteLine($"Error uploading file: {ex.Message}");
                throw new FileSystemException(FileSystemErrorCode.Unspecified, $"上傳檔案時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 重新命名檔案，將檔案從舊名稱更改為新名稱。會檢查副檔名是否為允許的類型。
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="FileSystemException"></exception>
        public override void RenameItem(FileSystemRenameItemOptions options)
        {
            string oldPath = options.Item.Path;
            string newName = options.ItemNewName;
            string directoryPath = Path.GetDirectoryName(oldPath) ?? string.Empty;
            string newPath = Path.Combine(directoryPath, newName);

            // 檢查副檔名是否為允許更改檔名的類型
            string extension = Path.GetExtension(newName).ToLower();
            List<string> allowedExtensions = new List<string>
            {
                ".pdf",
                ".dwg"
            };
            if (!allowedExtensions.Contains(extension.ToLower()))
            {
                throw new FileSystemException(FileSystemErrorCode.WrongFileExtension, $"僅支援更名的副檔名為 {string.Join(", ", allowedExtensions)} 檔案");
            }

            // 準備檔案路徑
            string fullOldPath = Path.Combine(RootDirectoryPath, PreparePath(oldPath));
            string fullNewPath = Path.Combine(RootDirectoryPath, PreparePath(newPath));

            // 檢查新檔案名稱是否已存在
            if (File.Exists(fullNewPath))
            {
                throw new FileSystemException(FileSystemErrorCode.FileExists, $"{newName}已存在");
            }

            // 執行實際的檔案重新命名
            try
            {
                // 取得來源檔案名稱（不含路徑）
                string sourceFileName = Path.GetFileName(oldPath);

                // 實際重新命名檔案
                File.Move(fullOldPath, fullNewPath);

                // 更新資料庫記錄
                // 嘗試從檔名解析 Guid
                string fileName = Path.GetFileNameWithoutExtension(oldPath);
                bool isGuidFile = Guid.TryParse(fileName, out Guid guidKey);

                // 取得網站 ID
                long websiteId = GetWebsiteId();
                if (websiteId <= 0)
                {
                    return; // 如果無法獲取網站 ID，則僅重新命名檔案，不更新資料庫
                }                // 查找相關的 FileUpload 記錄
                FileUpload? fileUpload = null;

                // 如果檔名是 Guid，則用 FileGuid 查找
                if (isGuidFile)
                {
                    fileUpload = _dbContext.FileUploads.FirstOrDefault(f => f.FileGuid == guidKey);
                }

                // 如果未找到，則嘗試通過 DownloadFileName 查找
                if (fileUpload == null)
                {
                    string relativeOldPath = oldPath.Replace("\\", "/");
                    string downloadPath = Path.Combine(_downloadFilePath, relativeOldPath).Replace("\\", "/");

                    fileUpload = _dbContext.FileUploads
                        .FirstOrDefault(f => f.FK_WebsiteId == websiteId &&
                                          f.DownloadFileName != null &&
                                          f.DownloadFileName.EndsWith(sourceFileName));
                }

                // 如果找到資料庫記錄，更新它
                if (fileUpload != null)
                {
                    // 取得新的相對路徑和下載路徑
                    string relativeNewPath = newPath.Replace("\\", "/");
                    string newDownloadPath = Path.Combine(_downloadFilePath, relativeNewPath).Replace("\\", "/");

                    // 更新記錄
                    fileUpload.DownloadFileName = newDownloadPath;
                    fileUpload.LastModifierUserId = _userId;
                    fileUpload.LastModificationTime = DateTime.Now;

                    // 儲存變更
                    _dbContext.SaveChanges();
                }
            }
            catch (IOException ex)
            {
                // 處理 IO 異常
                throw new FileSystemException(FileSystemErrorCode.Unspecified, ex.Message);
            }
            catch (Exception ex)
            {
                // 處理其他異常
                System.Diagnostics.Debug.WriteLine($"Error renaming file: {ex.Message}");
                throw new FileSystemException(FileSystemErrorCode.Unspecified, $"重新命名檔案時發生錯誤{ex.Message}");
            }
        }

        /// <summary>
        /// 還原備份檔案，將備份檔案還原到原始路徑。上傳發生錯誤時會使用此方法。
        /// </summary>
        /// <param name="backupFilePath"></param>
        /// <param name="targetFilePath"></param>
        private void RestoreBackupFile(string backupFilePath, string targetFilePath)
        {
            try
            {
                // 發生異常時還原備份
                if (File.Exists(backupFilePath))
                {
                    if (File.Exists(targetFilePath))
                    {
                        File.Delete(targetFilePath);
                    }
                    File.Move(backupFilePath, targetFilePath);
                }
            }
            catch (Exception ex)
            {
                // 記錄備份還原失敗，但不拋出新的異常
                System.Diagnostics.Debug.WriteLine($"Failed to restore backup file: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得檔案的 MIME 類型，根據副檔名來判斷。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private string GetContentType(string extension)
        {
            // 使用 FileExtensionContentTypeProvider 來取得對應的 MIME 類型
            var provider = new FileExtensionContentTypeProvider();
            string? contentType;
            if (provider.TryGetContentType("file" + extension, out contentType))
            {
                return contentType;
            }
            return "application/octet-stream";
        }

        /// <summary>
        /// 取得當前網站的 ID
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 處理路徑，將斜線轉換為反斜線並去除多餘的反斜線。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string PreparePath(string path)
        {
            return path?.Replace('/', '\\').Trim('\\') ?? string.Empty;
        }

    }
}