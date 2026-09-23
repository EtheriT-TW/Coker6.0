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
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;

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
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly BackgroundTaskService _backgroundTaskService;
        private readonly CokerDbContext _dbContext;

        public FileManagementController(
            IFileManagementAppService fileManagementAppService,
            IThumbnailGeneratorService thumbnailGeneratorService,
            LoginUserData loginUserData,
            IUploadPathResolver uploadPathResolver,
            IBackgroundJobClient backgroundJobs,
            BackgroundTaskService backgroundTaskService,
            CokerDbContext dbContext,
            ILogger<FileManagementController> logger)
        {
            _fileManagementAppService = fileManagementAppService;
            _thumbnailGeneratorService = thumbnailGeneratorService;
            _loginUserData = loginUserData;
            _uploadPathResolver = uploadPathResolver;
            _backgroundJobs = backgroundJobs;
            _backgroundTaskService = backgroundTaskService;
            _dbContext = dbContext;
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

                return await CreateThumbnailResultAsync(physicalPath, cancellationToken);
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

        [HttpGet]
        public async Task<IActionResult> RecycleThumbnail(
            [FromQuery] long fileUploadId,
            CancellationToken cancellationToken)
        {
            try
            {
                var websiteId = await _loginUserData.GetWebsiteId();
                var orgName = await _loginUserData.GetWebsiteOrgName();
                var file = await (
                    from recycle in _dbContext.FileRecycleBinItems.AsNoTracking()
                    join upload in _dbContext.FileUploads.IgnoreQueryFilters().AsNoTracking()
                        on recycle.FK_FileUploadId equals upload.Id
                    where recycle.FK_WebsiteId == websiteId
                        && recycle.FK_FileUploadId == fileUploadId
                        && !recycle.IsDeleted
                        && upload.FK_WebsiteId == websiteId
                    select new { upload.DownloadFileName }
                ).FirstOrDefaultAsync(cancellationToken);
                if (file == null || string.IsNullOrWhiteSpace(file.DownloadFileName))
                    return NotFound();

                var physicalPath = FileRecycleStorage.GetStoredRecyclePath(
                    _uploadPathResolver,
                    orgName,
                    file.DownloadFileName);
                return await CreateThumbnailResultAsync(physicalPath, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "產生資源回收桶縮圖失敗：{FileUploadId}", fileUploadId);
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> RecyclePreview(
            [FromQuery] long fileUploadId,
            CancellationToken cancellationToken)
        {
            try
            {
                var websiteId = await _loginUserData.GetWebsiteId();
                var orgName = await _loginUserData.GetWebsiteOrgName();
                var file = await (
                    from recycle in _dbContext.FileRecycleBinItems.AsNoTracking()
                    join upload in _dbContext.FileUploads.IgnoreQueryFilters().AsNoTracking()
                        on recycle.FK_FileUploadId equals upload.Id
                    where recycle.FK_WebsiteId == websiteId
                        && recycle.FK_FileUploadId == fileUploadId
                        && !recycle.IsDeleted
                        && upload.FK_WebsiteId == websiteId
                    select new
                    {
                        upload.DownloadFileName,
                        upload.ContentType
                    }
                ).FirstOrDefaultAsync(cancellationToken);
                if (file == null || string.IsNullOrWhiteSpace(file.DownloadFileName))
                    return NotFound();

                var physicalPath = FileRecycleStorage.GetStoredRecyclePath(
                    _uploadPathResolver,
                    orgName,
                    file.DownloadFileName);
                if (!System.IO.File.Exists(physicalPath))
                    return NotFound();

                var contentType = file.ContentType;
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    var contentTypeProvider = new FileExtensionContentTypeProvider();
                    if (!contentTypeProvider.TryGetContentType(physicalPath, out contentType))
                        contentType = "application/octet-stream";
                }

                return PhysicalFile(
                    physicalPath,
                    contentType,
                    enableRangeProcessing: true);
            }
            catch (OperationCanceledException)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "讀取資源回收桶預覽失敗：{FileUploadId}", fileUploadId);
                return NotFound();
            }
        }

        private async Task<IActionResult> CreateThumbnailResultAsync(
            string physicalPath,
            CancellationToken cancellationToken)
        {
            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            // ICO 常包含多組尺寸／多個 frame，ImageMagick 單圖解碼可能失敗。
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

        [HttpGet]
        public async Task<IActionResult> UnreferencedFiles()
        {
            return Ok(await _fileManagementAppService.GetUnreferencedFilesAsync());
        }

        [HttpGet]
        public async Task<IActionResult> RecycleBinFiles()
        {
            return Ok(await _fileManagementAppService.GetRecycleBinFilesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> StartReferenceScan()
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var jobId = _backgroundJobs.Enqueue<FileCleanupWorking>(
                job => job.ScanWebsiteAsync(websiteId));
            return Ok(new { jobId });
        }

        [HttpPost]
        public async Task<IActionResult> StartMoveSelectedToRecycleBin(
            [FromBody] long[] fileUploadIds)
        {
            var ids = (fileUploadIds ?? Array.Empty<long>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();
            if (ids.Length == 0)
                return BadRequest(new { message = "請先選擇要移至資源回收桶的檔案。" });
            if (ids.Length > 10_000)
                return BadRequest(new { message = "單次最多處理 10,000 筆檔案。" });

            var websiteId = await _loginUserData.GetWebsiteId();
            var userId = await _loginUserData.GetUserId();
            BackgroundTaskRecord? task = null;
            try
            {
                task = await _backgroundTaskService.CreateFileCleanupTaskAsync(
                    websiteId,
                    userId,
                    ids.Length);
                var jobId = _backgroundJobs.Enqueue<FileCleanupWorking>(job =>
                    job.MoveSelectedToRecycleBinAsync(task.Id, websiteId, userId, ids));
                await _backgroundTaskService.SetHangfireJobIdAsync(task.Id, jobId);
                return Accepted(new { taskId = task.Id });
            }
            catch (BackgroundTaskConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                if (task != null)
                    await _backgroundTaskService.FailAsync(task.Id, ex.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartPermanentlyDeleteSelected(
            [FromBody] long[] fileUploadIds)
        {
            var ids = (fileUploadIds ?? Array.Empty<long>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();
            if (ids.Length == 0)
                return BadRequest(new { message = "請先選擇要永久刪除的回收桶項目。" });
            if (ids.Length > 10_000)
                return BadRequest(new { message = "單次最多處理 10,000 筆檔案。" });

            var websiteId = await _loginUserData.GetWebsiteId();
            var userId = await _loginUserData.GetUserId();
            BackgroundTaskRecord? task = null;
            try
            {
                task = await _backgroundTaskService.CreateFileCleanupTaskAsync(
                    websiteId,
                    userId,
                    ids.Length);
                var jobId = _backgroundJobs.Enqueue<FileCleanupWorking>(job =>
                    job.PermanentlyDeleteSelectedAsync(task.Id, websiteId, userId, ids));
                await _backgroundTaskService.SetHangfireJobIdAsync(task.Id, jobId);
                return Accepted(new { taskId = task.Id });
            }
            catch (BackgroundTaskConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                if (task != null)
                    await _backgroundTaskService.FailAsync(task.Id, ex.Message);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> FileCleanupTask([FromQuery] long? taskId = null)
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var userId = await _loginUserData.GetUserId();
            var task = taskId.HasValue
                ? await _backgroundTaskService.GetForUserAsync(taskId.Value, websiteId, userId)
                : await _backgroundTaskService.GetActiveFileCleanupTaskForUserAsync(websiteId, userId);
            if (task == null
                || task.Type != BackgroundTaskTypeEnum.FileCleanup)
            {
                return NotFound();
            }

            return Ok(new
            {
                taskId = task.Id,
                status = task.Status.ToString(),
                task.Progress,
                task.Message,
                task.Error,
                task.ResultJson,
                task.CompletionTime
            });
        }

        [HttpPost]
        public async Task<IActionResult> MoveToRecycleBin([FromBody] long fileUploadId)
        {
            return await ExecuteFileActionAsync(
                () => _fileManagementAppService.MoveToRecycleBinAsync(fileUploadId));
        }

        [HttpPost]
        public async Task<IActionResult> MoveAllUnreferencedToRecycleBin()
        {
            try
            {
                return Ok(await _fileManagementAppService
                    .MoveAllUnreferencedToRecycleBinAsync());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (IOException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return BadRequest(new { message = "伺服器沒有權限搬移檔案，請檢查 Upload 目錄權限。" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestoreFromRecycleBin([FromBody] long fileUploadId)
        {
            return await ExecuteFileActionAsync(
                () => _fileManagementAppService.RestoreFromRecycleBinAsync(fileUploadId));
        }

        [HttpPost]
        public async Task<IActionResult> PermanentlyDelete([FromBody] long fileUploadId)
        {
            return await ExecuteFileActionAsync(
                () => _fileManagementAppService.PermanentlyDeleteAsync(fileUploadId));
        }

        private static async Task<IActionResult> ExecuteFileActionAsync(Func<Task> action)
        {
            try
            {
                await action();
                return new OkResult();
            }
            catch (InvalidOperationException ex)
            {
                return new BadRequestObjectResult(new { message = ex.Message });
            }
            catch (IOException ex)
            {
                return new BadRequestObjectResult(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return new BadRequestObjectResult(new
                {
                    message = "伺服器沒有權限搬移檔案，請檢查 Upload 目錄權限。"
                });
            }
        }
    }
}
