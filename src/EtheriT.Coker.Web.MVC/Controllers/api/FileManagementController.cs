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

        public IActionResult FileSystem(FileSystemCommand command, string arguments)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _fileManagementAppService.FileSystem(command, arguments, this.Request);

            return Ok(result);
        }

        [HttpPost]
        public async Task<bool> CheckFileHasBindings([FromBody] string filePath)
        {
            return await _fileManagementAppService.CheckFileHasBindingsAsync(filePath);
        }

        [HttpPost]
        public async Task<ActionResult<bool>> CheckFileExists([FromBody] FileExistCheckDto fileCheckDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var exists = await _fileManagementAppService.CheckFileExistsAsync(fileCheckDto.DirectoryPath, fileCheckDto.FileName);
            return Ok(exists);
        }
    }
}