using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Core.Models;
using HtmlAgilityPack;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task ArticleImageCleanup(BackofficeSandbox box)
    {
        box.Services.GetRequiredService<IOptions<VirtualDirectory>>().Value.FileAllow.Ext = new() { "image/png" };
        var service = box.Services.GetRequiredService<IArticleAppService>();
        var codec = box.Services.GetRequiredService<StringHandler>();
        var paths = box.Services.GetRequiredService<IUploadPathResolver>();
        using var red = new MagickImage(MagickColors.Red, 2, 2);
        using var blue = new MagickImage(MagickColors.Blue, 2, 2);
        var redHtml = "<img src='data:image/png;base64," + Convert.ToBase64String(red.ToByteArray(MagickFormat.Png)) + "'>";
        var blueHtml = "<img src='data:image/png;base64," + Convert.ToBase64String(blue.ToByteArray(MagickFormat.Png)) + "'>";
        var article = new Article { FK_WebsiteId = 999000001, Title = "文章圖片清理" };
        box.Db.Article.Add(article); await box.Db.SaveChangesAsync();
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = redHtml }));
        var stored = await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == article.Id);
        var oldDoc = new HtmlDocument(); oldDoc.LoadHtml(codec.HtmlDecode(stored.SaveHtml));
        var oldPath = oldDoc.DocumentNode.SelectSingleNode("//img").GetAttributeValue("src", "");
        var oldFile = await box.Db.FileUploads.AsNoTracking().SingleAsync();
        var physicalPath = paths.GetPhysicalPathFromDownloadFileName("diagnostics", oldFile.DownloadFileName!);
        Success(await service.ImportConten(new() { Id = checked((int)article.Id), SaveHtml = redHtml }));
        var draft = await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = blueHtml });
        Success(draft);
        Require(draft.Object == null, "An image still used by published content must not be offered for cleanup.");
        var publish = await service.ImportConten(new() { Id = checked((int)article.Id), SaveHtml = blueHtml });
        Success(publish);
        var offer = publish.Object as ArticleImageCleanupOfferDto;
        Require(offer is { Count: 1, CheckFailed: false } && !string.IsNullOrEmpty(offer.Token), "Removed image did not produce a scoped cleanup offer.");
        Require(File.Exists(physicalPath) && !await box.Db.FileRecycleBinItems.AnyAsync()
            && !await box.Db.FileCleanupCandidates.AnyAsync(), "Offering or cancelling cleanup must not move files or stage candidates.");
        Require(!(await service.RecycleUnusedImages(new() { Id = article.Id + 1, Token = offer!.Token! })).Success,
            "A confirmation for another article was accepted.");
        Require(!(await service.RecycleUnusedImages(new() { Id = article.Id, Token = "invalid-ticket" })).Success,
            "A forged confirmation was accepted.");
        var protector = box.Services.GetRequiredService<IDataProtectionProvider>().CreateProtector("Coker.Article.ImageCleanup.v1");
        foreach (var field in new[] { "WebsiteId", "UserId", "ExpiresUtc" })
        {
            var payload = JObject.Parse(protector.Unprotect(offer!.Token!));
            payload[field] = field == "ExpiresUtc" ? JToken.FromObject(DateTime.UtcNow.AddMinutes(-1)) : JToken.FromObject(999000002L);
            var invalidContext = protector.Protect(payload.ToString());
            Require(!(await service.RecycleUnusedImages(new() { Id = article.Id, Token = invalidContext })).Success,
                "An expired or wrong site/user confirmation was accepted.");
        }
        Require(File.Exists(physicalPath), "Rejected confirmations changed a file.");

        // Add a reference AFTER the offer without updating the reference index.
        var shared = new Article { FK_WebsiteId = 999000001, Title = "共用圖片", SaveHtml = "<img src='" + oldPath + "'>" };
        box.Db.Article.Add(shared); await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        var protectedResult = await service.RecycleUnusedImages(new() { Id = article.Id, Token = offer!.Token! });
        Success(protectedResult);
        Require(protectedResult.Object is FileCleanupBatchResultDto { MovedCount: 0, SkippedCount: 1 }
            && File.Exists(physicalPath), "A new live reference was not protected at confirmation time.");
        (await box.Db.Article.SingleAsync(row => row.Id == shared.Id)).SaveHtml = "";
        await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = blueHtml + "<p>後續修改</p>" }));
        Require(!(await service.RecycleUnusedImages(new() { Id = article.Id, Token = offer.Token! })).Success
            && File.Exists(physicalPath), "An outdated content confirmation was accepted.");

        // Reintroduce then remove the image to obtain a fresh offer for this save.
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = "<img src='" + oldPath + "'>" }));
        var removed = await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = blueHtml });
        Success(removed);
        var fresh = removed.Object as ArticleImageCleanupOfferDto;
        Require(fresh is { Count: 1 } && !string.IsNullOrEmpty(fresh.Token), "The new removal did not produce a fresh offer.");
        var recycle = await service.RecycleUnusedImages(new() { Id = article.Id, Token = fresh!.Token! });
        Success(recycle); box.Db.ChangeTracker.Clear();
        Require(recycle.Object is FileCleanupBatchResultDto { MovedCount: 1 }
            && !File.Exists(physicalPath) && await box.Db.FileRecycleBinItems.AnyAsync(row => row.FK_FileUploadId == oldFile.Id),
            "Confirmed cleanup did not move only the removed image into the recycle bin.");
        var replay = await service.RecycleUnusedImages(new() { Id = article.Id, Token = fresh.Token! });
        Success(replay);
        Require(replay.Object is FileCleanupBatchResultDto { MovedCount: 0 }, "Replayed cleanup moved an unrelated file.");
        var current = await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == article.Id);
        Require(codec.HtmlDecode(current.SaveHtml).Contains("/upload/")
            && !codec.HtmlDecode(current.SaveHtml).Contains(oldPath), "Cleanup changed the saved article or retained its removed image.");
        // Reinsert the same bytes after recycling: it must get a new upload identity,
        // so the reference scanner and recycle family lookup do not see duplicate keys.
        var blueFile = await box.Db.FileUploads.AsNoTracking().SingleAsync(row => row.Id != oldFile.Id);
        var spoof = new FileUpload { FK_WebsiteId = 999000001, GuidKey = Guid.NewGuid(),
            OriginalFileName = oldFile.OriginalFileName, DownloadFileName = blueFile.DownloadFileName, ContentType = "image/png" };
        box.Db.FileUploads.Add(spoof); await box.Db.SaveChangesAsync();
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = redHtml }));
        var reinserted = await box.Db.FileUploads.AsNoTracking().Where(row => row.Id != oldFile.Id && row.Id != spoof.Id
            && row.OriginalFileName == oldFile.OriginalFileName).SingleAsync();
        Require(reinserted.GuidKey != oldFile.GuidKey && reinserted.DownloadFileName != blueFile.DownloadFileName, "Reinserted image reused the recycled upload identity.");
        var removeAgain = await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = blueHtml });
        Success(removeAgain);
        Require(removeAgain.Object is ArticleImageCleanupOfferDto { Count: 1, CheckFailed: false },
            "Reinserting a recycled image broke cleanup/reference scanning.");
        await box.Services.GetRequiredService<IFileManagementAppService>().RestoreFromRecycleBinAsync(oldFile.Id);
        Require(File.Exists(physicalPath), "The removed image could not be restored from the recycle bin.");
    }
}
