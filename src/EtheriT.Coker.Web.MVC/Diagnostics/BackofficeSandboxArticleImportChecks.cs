using EtheriT.Coker.Application;
using EtheriT.Coker.Application.ArticleImports;
using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task ArticleImportCover(BackofficeSandbox box)
    {
        const long websiteId = 999000001;
        const string coverKey = ArticleImportFieldRules.CoverKey;
        const string bannerKey = "hero.banner";
        var template = ArticleTemplateCatalog.Get("vendor-profile");
        var directory = new global::EtheriT.Coker.Core.Models.Directory
        {
            FK_WebsiteId = websiteId, Title = "匯入封面", Type = (int)DirectoryTypeEnum.文章, ArticleTemplateKey = template.Key
        };
        var tag = new Tag { FK_WebsiteId = websiteId, Title = "匯入封面" };
        var scan = new BackgroundTaskRecord
        {
            FK_WebsiteId = websiteId, FK_UserId = websiteId, Status = BackgroundTaskStatusEnum.AwaitingConfirmation
        };
        var banner = NewFile("banner.png");
        var cover = NewFile("cover.png");
        box.Db.AddRange(directory, tag, scan, banner, cover);
        await box.Db.SaveChangesAsync();
        var batch = new ArticleImportBatch
        {
            FK_WebsiteId = websiteId, FK_DirectoryId = directory.Id, TemplateKey = template.Key, FK_BackgroundTaskId = scan.Id
        };
        box.Db.AddRange(batch, new Tag_Associate { FK_AId = directory.Id, FK_TId = tag.Id, Type = TagAssociateTypeEnum.目錄 });
        await box.Db.SaveChangesAsync();

        var bannerImage = NewImage("image:1", "官網橫幅", banner);
        var coverImage = NewImage("image:2", "文章封面", cover);
        ArticleImportImageRecord[] records = [bannerImage, coverImage];
        var aiImages = records.Select(i => new ArticleImportAiImage(i.Token, i.Column, i.FileName, i.Path!)).ToList();
        Dictionary<string, List<string>> ReadAiFields(IReadOnlyList<ArticleImportAiImage> images, string? coverToken, string? bannerToken = null)
        {
            var aiRow = new ArticleImportAiRow(2, new Dictionary<string, string>(), images);
            var fields = new Dictionary<string, List<string>> { [template.PageTitleKey] = ["匯入文章"] };
            if (coverToken != null) fields[coverKey] = [coverToken];
            if (bannerToken != null) fields[bannerKey] = [bannerToken];
            var response = JsonConvert.SerializeObject(new { rows = new[] { new { row = 2, fields } } });
            return ArticleImportAiFiller.ParseResponse(response, template, [aiRow]).Single().Fields
                ?? throw new InvalidOperationException("Import fixture could not parse the AI result.");
        }

        var selected = ReadAiFields([aiImages[0]], "image:1", "image:1");
        var explicitChoice = ReadAiFields(aiImages, "image:1", "image:1");
        var fallback = ReadAiFields(aiImages, "image:99");
        Require(selected[coverKey][0] == bannerImage.Path && selected[bannerKey][0] == bannerImage.Path
            && explicitChoice[coverKey][0] == bannerImage.Path && fallback[coverKey][0] == coverImage.Path,
            "The cover must honor the AI choice, accept the same image as the banner, and fall back to a row image.");
        Require(!ReadAiFields([], null).ContainsKey(coverKey), "An image-free row must have no cover.");

        var first = NewRow(2, selected, records);
        fallback.Remove(coverKey);
        fallback[template.PageTitleKey] = ["第二篇文章"];
        var second = NewRow(3, fallback, records);
        box.Db.AddRange(first, second);
        await box.Db.SaveChangesAsync();

        var app = box.Services.GetRequiredService<ArticleImportAppService>();
        var preview = await app.GetBatchAsync(batch.Id);
        Require(preview.Fields.Where(f => f.Type == ArticleFieldTypes.Image).Take(2).Select(f => f.Key)
            .SequenceEqual([coverKey, bannerKey]) && preview.Rows[1].Fields[coverKey][0] == coverImage.Path,
            "The preview must place the cover before the banner and provide a default for older pending rows.");
        selected[coverKey] = [coverImage.Path!];
        var edited = await app.UpdateRowAsync(new() { RowId = first.Id, Fields = selected });
        Require(edited.Row.Fields[coverKey][0] == coverImage.Path && edited.Row.Fields[bannerKey][0] == bannerImage.Path
            && !await box.Db.FileBinds.AnyAsync(), "Changing the preview cover must preserve the banner and defer article writes.");

        var writer = box.Services.GetRequiredService<ArticleImportWriter>();
        var result = await writer.WriteAsync(batch.Id, (_, _) => Task.CompletedTask);
        Require(result.CreatedCount == 2 && result.Failures.Count == 0,
            $"Cover import failed: {string.Join(", ", result.Failures.Select(f => f.Error))}");
        var articles = await box.Db.Article.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var files = box.Services.GetRequiredService<IFileUploadAppService>();
        var ids = articles.Select(a => a.Id).ToList();
        var images = await files.getImgsFiles(new() { Sid = ids, Type = (int)FileBindTypeEnum.文章管理, Size = 3 });
        Require(images.Select(i => i.Sid).SequenceEqual(ids) && images.All(i => i.Id == cover.Id),
            "Each article must resolve its own cover even when both refer to the same file.");
        Require(articles.All(a => !ArticleFieldValues.Parse(a.FieldDataJson).ContainsKey(coverKey)),
            "The cover must be saved as an article image separately from the template fields.");

        var update = NewRow(4, ReadAiFields(aiImages, "image:1"), records);
        var withoutImage = ReadAiFields([], null);
        withoutImage[template.PageTitleKey] = ["第二篇文章"];
        box.Db.AddRange(update, NewRow(5, withoutImage, []));
        await box.Db.SaveChangesAsync();
        var updated = await writer.WriteAsync(batch.Id, (_, _) => Task.CompletedTask);
        images = await files.getImgsFiles(new() { Sid = ids, Type = (int)FileBindTypeEnum.文章管理, Size = 3 });
        Require(updated.UpdatedCount == 2 && images.Select(i => i.Id).SequenceEqual([banner.Id, cover.Id])
            && !await box.Db.FileUploads.AnyAsync(f => f.IsDeleted),
            "Replacing a cover must preserve the shared file; an image-free update must preserve the article's cover.");

        FileUpload NewFile(string name) => new()
        {
            FK_WebsiteId = websiteId, GuidKey = Guid.NewGuid(), ContentType = "image/png",
            OriginalFileName = name, DownloadFileName = "/upload/htmlConten/" + name
        };
        static ArticleImportImageRecord NewImage(string token, string column, FileUpload file)
            => new(token, column, file.OriginalFileName, file.Id, "/upload/diagnostics/htmlConten/" + file.OriginalFileName, false, null);
        ArticleImportRow NewRow(int number, Dictionary<string, List<string>> fields, IReadOnlyList<ArticleImportImageRecord> images)
            => new()
            {
                FK_ArticleImportBatchId = batch.Id, RowNumber = number, Status = ArticleImportRowStatusEnum.Ready,
                FieldDataJson = ArticleFieldValues.Serialize(fields), DownloadedFilesJson = ArticleImportRowImages.Serialize(images)
            };
    }
}
