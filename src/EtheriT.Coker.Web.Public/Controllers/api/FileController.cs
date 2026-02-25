using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly IFileUploadAppService fileUploadAppService;
        public FileController(IFileUploadAppService fileUploadAppService)
        {
            this.fileUploadAppService = fileUploadAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> insertNotFondFile(InsertNotFoundFileDto dto)
        {
            return await fileUploadAppService.insertNotFondFile(dto);
        }
        [HttpGet]
        public async Task<IActionResult> DecryptFile(long fid)
        {
            var decryptfile_response = await fileUploadAppService.DecryptFile(fid);

            if (!decryptfile_response.Success) return StatusCode(403, decryptfile_response.ErrorMessage);

            var contentType = decryptfile_response.ContentType ?? "application/octet-stream";

            if (decryptfile_response.IsEncryptedFile)
            {
                return File(decryptfile_response.Bytes, contentType, decryptfile_response.FileName);
            }
            return PhysicalFile(decryptfile_response.PhysicalPath, contentType, decryptfile_response.FileName);
        }
    }
}
