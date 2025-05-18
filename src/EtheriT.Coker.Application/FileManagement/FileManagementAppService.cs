using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application.Shared.FileManagement;
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

        public FileManagementAppService(IConfiguration configuration, LoginUserData loginUserData)
        {
            _configuration = configuration;
            _loginUserData = loginUserData;
        }

        public object FileSystem(FileSystemCommand command, string arguments, HttpRequest request)
        {
            string orgName =  _loginUserData.GetWebsiteOrgName().Result;
            var filePath = $"{_configuration.GetValue<string>("VirtualDirectory:upload")}\\" + orgName + "";

            var config = new FileSystemConfiguration
            {
                Request = request,
                FileSystemProvider = new PhysicalFileSystemProvider(filePath),
                //uncomment the code below to enable file/folder management
                //AllowCopy = true,
                //AllowCreate = true,
                //AllowMove = true,
                //AllowDelete = true,
                //AllowRename = true,
                //AllowUpload = true,
                AllowDownload = true,
                //AllowedFileExtensions = new[] { ".js", ".json", ".css" }
            };

            switch (command)
            {
                case FileSystemCommand.GetDirContents:
                    break;
                case FileSystemCommand.CreateDir:
                    break;
                case FileSystemCommand.Rename:
                    break;
                case FileSystemCommand.Move:
                    break;
                case FileSystemCommand.Copy:
                    break;
                case FileSystemCommand.Remove:
                    break;
                case FileSystemCommand.UploadChunk:
                    break;
                case FileSystemCommand.AbortUpload:
                    break;
                case FileSystemCommand.Download:
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
