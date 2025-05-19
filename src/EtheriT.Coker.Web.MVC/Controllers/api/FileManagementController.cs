using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Hosting;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.FileManagement;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class FileManagementController : Controller
    {
        private readonly IFileManagementAppService _fileManagementAppService;

        public FileManagementController(IFileManagementAppService fileManagementAppService)
        {
            _fileManagementAppService = fileManagementAppService;
        }

        public object FileSystem(FileSystemCommand command, string arguments)
        {
            return _fileManagementAppService.FileSystem(command, arguments, this.Request);
        }
    }
}