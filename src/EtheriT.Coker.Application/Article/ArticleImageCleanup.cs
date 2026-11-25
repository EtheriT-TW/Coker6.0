using System.Security.Cryptography;
using System.Text;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.FileManagement;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.Article;

public partial class ArticleAppService
{
    private sealed record ImageCleanupTicket(long WebsiteId, long UserId, long ArticleId,
        DateTime ExpiresUtc, string Version, long[] FileIds);

    private static string?[] ArticleImageFields(Core.Models.Article article) => new[]
    {
        article.SaveHtml, article.SaveCss, article.Html, article.Css,
        article.NewsletterHtml, article.NewsletterCss, article.DataJson, article.Description, article.Subtitle
    };

    private static string ImageCleanupVersion(Core.Models.Article article) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ArticleImageFields(article)))));

    private async Task<HashSet<long>> CaptureArticleImageIdsAsync(Core.Models.Article article)
    {
        var orgName = await loginUserData.GetWebsiteOrgName();
        var paths = ArticleImageFields(article)
            .SelectMany(value => FileReferenceIndexingService.ExtractPaths(value, orgName))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (paths.Length == 0) return new();
        var storedPaths = paths.Concat(paths.Select(path => path.Replace("/upload/", $"/upload/{orgName}/"))).Distinct().ToArray();
        return (await db.FileUploads.AsNoTracking()
            .Where(file => file.FK_WebsiteId == article.FK_WebsiteId && !file.IsDeleted
                && file.ContentType.StartsWith("image/") && storedPaths.Contains(file.DownloadFileName!))
            .Select(file => file.Id).ToListAsync()).ToHashSet();
    }

    private async Task<long[]> EligibleRemovedImagesAsync(long websiteId, IEnumerable<long> ids)
    {
        if (imageCleanupScanner == null || imageCleanupPaths == null)
            throw new InvalidOperationException("目前無法確認圖片引用，已保留圖片。");
        var candidates = ids.Distinct().Take(200).ToArray();
        if (candidates.Length == 0) return Array.Empty<long>();
        // The live scanner covers drafts, published content, other pages, bindings,
        // duplicate paths and related image variants, even if their index is stale.
        var referenced = await imageCleanupScanner.GetReferencedFileIdsAsync(websiteId);
        var files = await db.FileUploads.AsNoTracking()
            .Where(file => file.FK_WebsiteId == websiteId && !file.IsDeleted
                && candidates.Contains(file.Id) && file.ContentType.StartsWith("image/"))
            .Select(file => new { file.Id, file.DownloadFileName }).ToListAsync();
        var orgName = await loginUserData.GetWebsiteOrgName();
        return files.Where(file => !referenced.Contains(file.Id) && !string.IsNullOrWhiteSpace(file.DownloadFileName)
            && File.Exists(imageCleanupPaths.GetPhysicalPathFromDownloadFileName(orgName, file.DownloadFileName!)))
            .Select(file => file.Id).ToArray();
    }

    private async Task<ArticleImageCleanupOfferDto?> CreateArticleImageCleanupOfferAsync(
        Core.Models.Article article, HashSet<long> previousImages)
    {
        if (previousImages.Count == 0) return null;
        if (imageCleanupProtector == null || imageCleanupFiles == null) return new() { CheckFailed = true };
        try
        {
            previousImages.ExceptWith(await CaptureArticleImageIdsAsync(article));
            if (previousImages.Count == 0) return null;
            var ids = await EligibleRemovedImagesAsync(article.FK_WebsiteId, previousImages);
            if (ids.Length == 0) return null;
            var ticket = new ImageCleanupTicket(article.FK_WebsiteId, await loginUserData.GetUserId(),
                article.Id, DateTime.UtcNow.AddMinutes(15), ImageCleanupVersion(article), ids);
            return new() { Count = ids.Length, Token = imageCleanupProtector.Protect(JsonConvert.SerializeObject(ticket)) };
        }
        catch
        {
            // Saving has succeeded; a cleanup lookup failure must never undo that result.
            return new() { CheckFailed = true };
        }
    }

    public async Task<ResponseMessageDto> RecycleUnusedImages(ArticleImageCleanupInputDto dto)
    {
        var response = new ResponseMessageDto();
        try
        {
            if (imageCleanupProtector == null || imageCleanupFiles == null)
                throw new InvalidOperationException("目前無法清理圖片，請稍後再試。");
            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            if (string.IsNullOrWhiteSpace(dto.Token) || dto.Token.Length > 20000)
                throw new InvalidOperationException("清理確認已失效，請重新儲存文章後再試。");
            ImageCleanupTicket? ticket;
            try { ticket = JsonConvert.DeserializeObject<ImageCleanupTicket>(imageCleanupProtector.Unprotect(dto.Token)); }
            catch { throw new InvalidOperationException("清理確認已失效，請重新儲存文章後再試。"); }
            if (ticket == null || ticket.WebsiteId != websiteId || ticket.UserId != userId
                || ticket.ArticleId != dto.Id || ticket.ExpiresUtc <= DateTime.UtcNow
                || ticket.FileIds.Length is < 1 or > 200)
                throw new InvalidOperationException("清理確認已失效，請重新儲存文章後再試。");
            var article = await db.Article.AsNoTracking().FirstOrDefaultAsync(row => row.Id == dto.Id
                && row.FK_WebsiteId == websiteId && !row.IsDeleted);
            if (article == null || !await CanSaveArticleAsync(dto.Id))
                throw new InvalidOperationException("目前無法清理這篇文章的圖片。");
            if (ImageCleanupVersion(article) != ticket.Version)
                throw new InvalidOperationException("文章已再次更新，請重新儲存後再確認清理圖片。");

            var eligible = await EligibleRemovedImagesAsync(websiteId, ticket.FileIds);
            var moved = 0;
            var failed = false;
            foreach (var id in eligible)
            {
                try
                {
                    if (!await db.FileUploads.AsNoTracking().AnyAsync(file => file.Id == id
                        && file.FK_WebsiteId == websiteId && !file.IsDeleted)) continue;
                    if (!await db.FileCleanupCandidates.AnyAsync(row => row.FK_WebsiteId == websiteId
                        && row.FK_FileUploadId == id && !row.IsDeleted))
                    {
                        db.FileCleanupCandidates.Add(new FileCleanupCandidate
                        {
                            FK_WebsiteId = websiteId, FK_FileUploadId = id,
                            FirstDetectedTime = DateTime.Now, LastConfirmedTime = DateTime.Now,
                            Reason = "文章更新後不再引用的圖片", CreatorUserId = userId
                        });
                        await db.SaveChangesAsync();
                    }
                    // Existing recycle logic checks LIVE references again immediately
                    // before moving this image and its related variants.
                    await imageCleanupFiles.MoveToRecycleBinAsync(id);
                    moved++;
                }
                catch
                {
                    db.ChangeTracker.Clear();
                    failed = true;
                    break;
                }
            }
            response.Object = new FileCleanupBatchResultDto { MovedCount = moved, SkippedCount = ticket.FileIds.Length - moved };
            response.Success = !failed;
            if (failed) response.Error = "部分圖片未能移到回收桶，文章已儲存，請稍後至檔案管理確認。";
            else response.Message = $"已將 {moved} 張不再使用的圖片移到回收桶；仍被使用或已處理的圖片會保留。";
        }
        catch (InvalidOperationException ex) { response.Error = ex.Message; }
        catch { response.Error = "圖片清理失敗，文章已儲存，請稍後再試。"; }
        return response;
    }
}
