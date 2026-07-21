using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Hosting;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Application.FileManagement;
using Microsoft.AspNetCore.StaticFiles;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class FileManagementController : Controller
    {
        private readonly IFileManagementAppService _fileManagementAppService;
        private readonly IThumbnailGeneratorService _thumbnailGeneratorService;
        private readonly LoginUserData _loginUserData;
        private readonly IUploadPathResolver _uploadPathResolver;
        private readonly ILogger<FileManagementController> _logger;

        public FileManagementController(
            IFileManagementAppService fileManagementAppService,
            IThumbnailGeneratorService thumbnailGeneratorService,
            LoginUserData loginUserData,
            IUploadPathResolver uploadPathResolver,
            ILogger<FileManagementController> logger)
        {
            _fileManagementAppService = fileManagementAppService;
            _thumbnailGeneratorService = thumbnailGeneratorService;
            _loginUserData = loginUserData;
            _uploadPathResolver = uploadPathResolver;
            _logger = logger;
        }

        public IActionResult FileSystem(FileSystemCommand command, string arguments)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _fileManagementAppService.FileSystem(command, arguments, this.Request);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(
            [FromQuery] string path,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            try
            {
                var orgName = await _loginUserData.GetWebsiteOrgName();
                var physicalPath = _uploadPathResolver.GetPhysicalPath(orgName, path);

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound();

                // ICO 常包含多組尺寸／多個 frame，ImageMagick 單圖解碼可能失敗。
                // 原始 ICO 通常很小，直接交給瀏覽器縮放比再次轉碼更穩定。
                if (string.Equals(
                    Path.GetExtension(physicalPath),
                    ".ico",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return PhysicalFile(physicalPath, "image/x-icon");
                }

                var thumbnail = await _thumbnailGeneratorService.GetOrCreateThumbnailAsync(
                    physicalPath,
                    cancellationToken);
                if (thumbnail == null || !thumbnail.Exists)
                    return NotFound();

                var contentTypeProvider = new FileExtensionContentTypeProvider();
                if (!contentTypeProvider.TryGetContentType(thumbnail.Name, out var contentType))
                    contentType = "application/octet-stream";

                return PhysicalFile(thumbnail.FullName, contentType);
            }
            catch (OperationCanceledException)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "產生檔案縮圖失敗：{Path}", path);
                return NotFound();
            }
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
