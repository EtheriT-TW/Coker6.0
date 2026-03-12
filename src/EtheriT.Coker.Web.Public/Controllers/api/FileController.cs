using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

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
            var result = await fileUploadAppService.DecryptFile(fid);

            if (!result.Success) return BadRequest(result.ErrorMessage);

            var disposition = CanInline(result.ContentType) ? "inline" : "attachment";

            var ext = Path.GetExtension(result.FileName);
            var asciiFileName = $"download{ext}";

            var contentDisposition = new ContentDispositionHeaderValue(disposition)
            {
                FileName = asciiFileName,
                FileNameStar = result.FileName
            };

            Response.Headers["Content-Disposition"] = contentDisposition.ToString();

            if (!result.IsEncryptedFile)
            {
                var stream = new FileStream(result.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, result.ContentType);
            }
            else return File(result.Bytes, result.ContentType);
        }
        private bool CanInline(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return false;
            return contentType.StartsWith("image/") || contentType == "application/pdf" || contentType.StartsWith("text/") || contentType.StartsWith("video/");
        }
    }
}
