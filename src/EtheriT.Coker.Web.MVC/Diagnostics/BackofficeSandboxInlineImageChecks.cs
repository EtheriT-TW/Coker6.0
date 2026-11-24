using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Core.Models;
using HtmlAgilityPack;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task ArticleInlineImages(BackofficeSandbox box)
    {
        // Only this isolated fixture enables raster uploads, under its temporary root.
        box.Services.GetRequiredService<IOptions<VirtualDirectory>>().Value.FileAllow.Ext =
            new() { "image/png", "image/jpeg", "image/gif", "image/webp", "image/bmp" };
        var service = box.Services.GetRequiredService<IArticleAppService>();
        var files = box.Services.GetRequiredService<IFileUploadAppService>();
        var codec = box.Services.GetRequiredService<StringHandler>();
        var paths = box.Services.GetRequiredService<IUploadPathResolver>();
        using var image = new MagickImage(MagickColors.Red, 2, 2);
        var bytes = image.ToByteArray(MagickFormat.Png);
        var data = "data:image/png;base64," + Convert.ToBase64String(bytes);
        var percent = "data:image/png," + string.Concat(bytes.Select(value => "%" + value.ToString("X2")));
        var article = new Article { FK_WebsiteId = 999000001, Title = "內嵌圖片轉存", Html = "原正式內容", SaveHtml = "原草稿" };
        var foreign = new Article { FK_WebsiteId = 999000002, Title = "其他站台", SaveHtml = "不可變更" };
        box.Db.Article.AddRange(article, foreign);
        await box.Db.SaveChangesAsync();
        var html = $"<p>保留文字</p><img id='one' src='{data}'><img id='two' src='{percent}'><img src='https://example.invalid/logo.png'>";
        var beforeFiles = await box.Db.FileUploads.CountAsync();
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = html, SaveCss = "#one{width:20px}" }));
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == article.Id);
        var decoded = codec.HtmlDecode(saved.SaveHtml);
        var doc = new HtmlDocument(); doc.LoadHtml(decoded);
        var first = doc.GetElementbyId("one").GetAttributeValue("src", "");
        Require(first.StartsWith("/upload/diagnostics/htmlConten/", StringComparison.Ordinal), "Draft image is not owned by the fixture site.");
        Require(doc.GetElementbyId("two").GetAttributeValue("src", "") == first, "Duplicate Base64 and percent-encoded images were not deduplicated.");
        Require(decoded.Contains("保留文字") && decoded.Contains("https://example.invalid/logo.png")
            && !decoded.Contains("data:image", StringComparison.OrdinalIgnoreCase), "Text, remote URLs or inline image replacement changed unexpectedly.");
        Require(saved.Html == "原正式內容" && saved.SaveCss == "#one{width:20px}", "Saving a draft changed published content or lost CSS.");
        Require(await box.Db.FileUploads.CountAsync() == beforeFiles + 1, "Duplicate images created multiple uploads.");
        var uploaded = await box.Db.FileUploads.AsNoTracking().SingleAsync(row => row.FK_WebsiteId == 999000001);
        Require(File.Exists(paths.GetPhysicalPathFromDownloadFileName("diagnostics", uploaded.DownloadFileName!)), "Normalized image has no physical file.");
        Require(await box.Db.FileReferences.AnyAsync(row => row.FK_WebsiteId == 999000001 && row.SourceType == "Article"
            && row.SourceId == article.Id && row.SourceState == "Draft" && row.FK_FileUploadId == uploaded.Id), "Converted draft image is missing its file reference.");
        var read = await service.GetConten(new() { Id = checked((int)article.Id) });
        Require(read.Success && !codec.HtmlDecode(read.Conten.SaveHtml).Contains("data:image", StringComparison.OrdinalIgnoreCase), "Editor reload returned inline image data.");

        // Saving the original input again and publishing must reuse the same file.
        Success(await service.SaveConten(new() { Id = checked((int)article.Id), SaveHtml = html, SaveCss = "#one{width:20px}" }));
        Success(await service.ImportConten(new() { Id = checked((int)article.Id), SaveHtml = html, SaveCss = "#one{width:20px}" }));
        box.Db.ChangeTracker.Clear();
        saved = await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == article.Id);
        Require(!codec.HtmlDecode(saved.Html).Contains("data:image", StringComparison.OrdinalIgnoreCase)
            && codec.HtmlDecode(saved.Html).Contains("/upload/htmlConten/"), "Publishing used the original inline data or retained the editor-only site path.");
        Require(await box.Db.FileUploads.CountAsync() == beforeFiles + 1, "Repeated save or publish duplicated files.");
        using var otherImage = new MagickImage(MagickColors.Blue, 2, 2);
        var freshData = "data:image/png;base64," + Convert.ToBase64String(otherImage.ToByteArray(MagickFormat.Png));
        var baselineDraft = saved.SaveHtml;
        var baselinePublished = saved.Html;
        Require(await box.Db.FileReferences.AnyAsync(row => row.FK_WebsiteId == 999000001 && row.SourceType == "Article"
            && row.SourceId == article.Id && row.SourceState == "Published" && row.FK_FileUploadId == uploaded.Id), "Published image is missing its file reference.");
        foreach (var invalid in new[]
        {
            "<img src='data:image/png;base64,%%%'>",
            "<img src='data:image/png;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not a PNG")) + "'>",
            "<img src='data:image/svg+xml,%3Csvg%3E%3C/svg%3E'>",
            "<img src='blob:https://example.invalid/image'>",
            "<source srcset='" + data + " 1x'>",
            string.Concat(Enumerable.Repeat("<img src='" + data + "'>", 51)),
            "<img src='data:image/png;base64," + new string('A', 14 * 1024 * 1024) + "'>"
        })
        {
            // Put a valid image first: later invalid inputs must be validated before any file creation.
            var result = await service.ImportConten(new() { Id = checked((int)article.Id), SaveHtml = "<img src='" + freshData + "'>" + invalid });
            Require(!result.Success && !string.IsNullOrWhiteSpace(result.Error), "Invalid image input was accepted or had no actionable error.");
            box.Db.ChangeTracker.Clear();
            saved = await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == article.Id);
            Require(saved.SaveHtml == baselineDraft && saved.Html == baselinePublished, "Rejected import changed draft or published content.");
            Require(await box.Db.FileUploads.CountAsync() == beforeFiles + 1, "Rejected input created an upload.");
        }
        Require(!(await service.SaveConten(new() { Id = checked((int)foreign.Id), SaveHtml = html })).Success, "A cross-site save was accepted.");
        Require((await box.Db.Article.AsNoTracking().SingleAsync(row => row.Id == foreign.Id)).SaveHtml == "不可變更", "Other site content was modified.");
        var plain = "<p>不需轉存</p><img src='/upload/diagnostics/logo.png'>";
        Require(await files.NormalizeInlineImageSourcesAsync(plain) == plain, "HTML without inline images must remain unchanged.");
    }
}
