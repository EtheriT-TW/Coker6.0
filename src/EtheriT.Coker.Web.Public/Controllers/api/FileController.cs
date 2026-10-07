using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Web.Public.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : Controller
    {
        private static readonly TimeSpan NotFoundImageCacheDuration = TimeSpan.FromHours(1);
        // Keep explicit crawler identities aligned with Coker.util.device.isKnownBot().
        private static readonly Regex KnownBotUserAgent = new Regex(
            "PetalBot|AhrefsBot|AhrefsSiteAudit|Googlebot|GoogleOther|bingbot|BingPreview|Bytespider|Baiduspider|YandexBot|YandexImages|DuckDuckBot|Applebot|SemrushBot|DotBot|MJ12bot|meta-externalagent|meta-webindexer|facebookexternalhit|Facebot|Twitterbot|LinkedInBot|Pinterestbot|Amazonbot|GPTBot|OAI-SearchBot|ClaudeBot|CCBot|UptimeRobot",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(50));

        private readonly IFileUploadAppService fileUploadAppService;
        private readonly NotFoundImageVerifier notFoundImageVerifier;
        private readonly IMemoryCache memoryCache;
        public FileController(IFileUploadAppService fileUploadAppService, NotFoundImageVerifier notFoundImageVerifier, IMemoryCache memoryCache)
        {
            this.fileUploadAppService = fileUploadAppService;
            this.notFoundImageVerifier = notFoundImageVerifier;
            this.memoryCache = memoryCache;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> insertNotFondFile(InsertNotFoundFileDto dto)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            try
            {
                if (KnownBotUserAgent.IsMatch(userAgent))
                {
                    return new ResponseMessageDto { Success = true };
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // Ignore abnormal diagnostic requests without accessing the database.
                return new ResponseMessageDto { Success = true };
            }

            var check = notFoundImageVerifier.Verify(dto, Request.Headers.Referer.ToString());
            if (!check.ShouldRecord)
                return new ResponseMessageDto { Success = true };

            // 跨裝置去重：同一張圖在快取期間內只處理一次，不論是哪台裝置回報
            var cacheKey = $"NotFoundImage|{check.ImagePath}";
            if (memoryCache.TryGetValue(cacheKey, out _))
                return new ResponseMessageDto { Success = true };
            memoryCache.Set(cacheKey, true, NotFoundImageCacheDuration);

            try
            {
                var result = await fileUploadAppService.insertNotFondFile(new InsertNotFoundFileDto
                {
                    Url = check.ImagePath,
                    From = check.PagePath
                });
                if (!result.Success)
                    memoryCache.Remove(cacheKey);
                return result;
            }
            catch
            {
                memoryCache.Remove(cacheKey);
                throw;
            }
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
