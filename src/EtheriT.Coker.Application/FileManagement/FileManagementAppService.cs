using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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
            var filePath = $"{_configuration.GetValue<string>("VirtualDirectory:upload")}\\" + orgName + "";

            // 取得目前登入使用者的 ID
            long userId = _loginUserData.GetUserId().Result; var allowFileAllowMIME = _configuration.GetSection("VirtualDirectory:FileAllow:Ext").Get<List<string>>() ?? new List<string>();

            // 使用 FileExtensionContentTypeProvider 從 MIME 值反推取得副檔名
            var provider = new FileExtensionContentTypeProvider();

            // 從 MIME 反向查找副檔名並只保留允許的副檔名
            var allowFileExtension = provider.Mappings.Where(x => allowFileAllowMIME.Contains(x.Value))
                                                      .Select(x => x.Key)
                                                      .OrderBy(x => x)
                                                      .Distinct()
                                                      .ToList();

            var customFileSystemProvider = new CustomFileSystemProvider(filePath, _dbContext, orgName, userId, _configuration, request);

            var config = new FileSystemConfiguration
            {
                Request = request,
                FileSystemProvider = customFileSystemProvider,

                //uncomment the code below to enable file/folder management
                //AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowUpload = true,
                AllowDownload = true,
                AllowedFileExtensions = allowFileExtension.ToArray()
            };

            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return result.GetClientCommandResult();
        }

        public async Task<bool> CheckFileHasBindingsAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                string orgName = await _loginUserData.GetWebsiteOrgName();
                long websiteId = await _loginUserData.GetWebsiteId();

                // 正規化檔案路徑，處理不同格式的路徑
                string normalizedPath = filePath.Replace("\\", "/").TrimStart('/');

                // 可能的路徑格式
                var possiblePaths = new List<string>
                {
                    $"/upload/{orgName}/{normalizedPath}",
                    $"/upload/{normalizedPath}",
                    $"\\upload\\{orgName}\\{normalizedPath.Replace("/", "\\")}",
                    $"\\upload\\{normalizedPath.Replace("/", "\\")}"
                };

                // 檢查是否存在於 FileUploads.DownloadFileName
                var fileUpload = await _dbContext.FileUploads
                    .Where(f => f.FK_WebsiteId == websiteId &&
                               !f.IsDeleted &&
                               f.DownloadFileName != null &&
                               possiblePaths.Contains(f.DownloadFileName))
                    .FirstOrDefaultAsync();

                if (fileUpload == null)
                {
                    return false;
                }

                // 檢查是否與 FileBinds 有關連
                var hasBindings = await _dbContext.FileBinds
                    .Where(fb => fb.FK_FileUploadId == fileUpload.Id && !fb.IsDeleted)
                    .AnyAsync();

                return hasBindings;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
