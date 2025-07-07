using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.FileManagement
{
    public interface IFileManagementAppService
    {
        object FileSystem(FileSystemCommand command, string arguments, HttpRequest request);
        Task<bool> CheckFileHasBindingsAsync(string filePath);
        Task<bool> CheckFileExistsAsync(string directoryPath, string fileName);
    }
}
