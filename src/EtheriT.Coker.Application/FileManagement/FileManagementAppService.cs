using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.FileManagement
{
    public class FileManagementAppService : IFileManagementAppService
    {
        private readonly IConfiguration _configuration;
        private readonly LoginUserData _loginUserData;
        private readonly CokerDbContext _dbContext;

        public FileManagementAppService(IConfiguration configuration, LoginUserData loginUserData, CokerDbContext dbContext)
        {
            _configuration = configuration;
            _loginUserData = loginUserData;
            _dbContext = dbContext;
        }

        public object FileSystem(FileSystemCommand command, string arguments, HttpRequest request)
        {
            string orgName = _loginUserData.GetWebsiteOrgName().Result;
            var filePath = $"{_configuration.GetValue<string>("VirtualDirectory:upload")}\\" + orgName + "";            // 取得目前登入使用者的 ID
            long userId = _loginUserData.GetUserId().Result;

            var config = new FileSystemConfiguration
            {
                Request = request,
                FileSystemProvider = new CustomFileSystemProvider(filePath, _dbContext, orgName, userId),
                //FileSystemProvider = new PhysicalFileSystemProvider(filePath),
                //uncomment the code below to enable file/folder management
                //AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowUpload = true,
                AllowDownload = true,
                AllowedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".txt" }
            };

            // 根據不同的命令做相應處理
            switch (command)
            {
                case FileSystemCommand.GetDirContents:
                    // 列出目錄內容
                    break;
                case FileSystemCommand.CreateDir:
                    // 創建目錄
                    break;
                case FileSystemCommand.Rename:
                    // 重命名文件或目錄，由 CustomFileSystemProvider 處理更新資料庫
                    System.Diagnostics.Debug.WriteLine($"處理檔案重命名操作: {arguments}");
                    break;
                case FileSystemCommand.Move:
                    // 移動文件或目錄，由 CustomFileSystemProvider 處理更新資料庫
                    System.Diagnostics.Debug.WriteLine($"處理檔案移動操作: {arguments}");
                    break;
                case FileSystemCommand.Copy:
                    // 複製文件或目錄
                    break;
                case FileSystemCommand.Remove:
                    // 刪除文件或目錄，由 CustomFileSystemProvider 處理更新資料庫
                    System.Diagnostics.Debug.WriteLine($"處理檔案刪除操作: {arguments}");
                    break;
                case FileSystemCommand.UploadChunk:
                    // 上傳文件塊，上傳完成後由 CustomFileSystemProvider 處理資料插入
                    break;
                case FileSystemCommand.AbortUpload:
                    // 中止上傳
                    break;
                case FileSystemCommand.Download:
                    // 下載文件
                    break;
                default:
                    throw new NotSupportedException($"The command '{command}' is not supported.");
            }

            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return result.GetClientCommandResult();
        }
    }
}
