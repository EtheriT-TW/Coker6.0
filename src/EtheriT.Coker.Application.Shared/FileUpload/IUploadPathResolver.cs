using System;
using System.Collections.Generic;
using System.Text;

namespace EtheriT.Coker.Application.Shared
{
    public interface IUploadPathResolver
    {
        string GetRootPath(string orgName);
        string GetDirectoryPath(string orgName, string directory);
        string GetPhysicalPath(string orgName, string relativePath);
        string GetPhysicalPathFromDownloadFileName(string orgName, string downloadFileName);
    }
}
