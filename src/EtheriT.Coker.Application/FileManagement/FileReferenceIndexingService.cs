using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.FileManagement
{
    public sealed record FileReferenceSource(
        string SourceType,
        long SourceId,
        string SourceState,
        IReadOnlyDictionary<string, string?> Fields);

    public interface IFileReferenceWriter
    {
        Task ReplaceSourceReferencesAsync(
            long websiteId,
            FileReferenceSource source,
            long? userId = null,
            CancellationToken cancellationToken = default);
    }

    public sealed class FileReferenceIndexingService : IFileReferenceWriter
    {
        private static readonly Regex UploadPathRegex = new(
            "/upload/[^\\s\\\"'<>\\)\\(]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly CokerDbContext db;
        private readonly IUploadPathResolver uploadPathResolver;

        public FileReferenceIndexingService(
            CokerDbContext db,
            IUploadPathResolver uploadPathResolver)
        {
            this.db = db;
            this.uploadPathResolver = uploadPathResolver;
        }

        public async Task ReplaceSourceReferencesAsync(
            long websiteId,
            FileReferenceSource source,
            long? userId = null,
            CancellationToken cancellationToken = default)
        {
            await ReplaceSourcesAsync(
                websiteId,
                new[] { source },
                replaceWholeWebsite: false,
                userId,
                cancellationToken);
        }

        public async Task RebuildWebsiteAsync(
            long websiteId,
            long? userId = null,
            CancellationToken cancellationToken = default)
        {
            var sources = new List<FileReferenceSource>();

            var website = await db.Websites.AsNoTracking()
                .Where(x => x.Id == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.DefaultUrl, x.Icon, x.Logo, x.Css, x.Description, x.Statement })
                .FirstOrDefaultAsync(cancellationToken);
            if (website == null)
                return;
            sources.Add(Source("Website", website.Id, "Current", new Dictionary<string, string?>
            {
                ["DefaultUrl"] = website.DefaultUrl, ["Icon"] = website.Icon,
                ["Logo"] = website.Logo, ["Css"] = website.Css,
                ["Description"] = website.Description, ["Statement"] = website.Statement
            }));

            var articles = await db.Article.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.SaveHtml, x.SaveCss, x.Html, x.Css, x.NewsletterHtml, x.NewsletterCss, x.DataJson, x.Description, x.Subtitle })
                .ToListAsync(cancellationToken);
            foreach (var x in articles)
            {
                sources.Add(Source("Article", x.Id, "Draft", Fields(("SaveHtml", x.SaveHtml), ("SaveCss", x.SaveCss), ("DataJson", x.DataJson))));
                sources.Add(Source("Article", x.Id, "Published", Fields(("Html", x.Html), ("Css", x.Css), ("Description", x.Description), ("Subtitle", x.Subtitle))));
                sources.Add(Source("Article", x.Id, "Newsletter", Fields(("NewsletterHtml", x.NewsletterHtml), ("NewsletterCss", x.NewsletterCss))));
            }

            var menus = await db.WebMenus.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.SaveHtml, x.SaveCss, x.Html, x.Css, x.icon, x.LinkUrl, x.Description })
                .ToListAsync(cancellationToken);
            foreach (var x in menus)
            {
                sources.Add(Source("WebMenu", x.Id, "Draft", Fields(("SaveHtml", x.SaveHtml), ("SaveCss", x.SaveCss))));
                sources.Add(Source("WebMenu", x.Id, "Published", Fields(("Html", x.Html), ("Css", x.Css), ("Icon", x.icon), ("LinkUrl", x.LinkUrl), ("Description", x.Description))));
            }

            var products = await db.Prods.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.SaveHtml, x.SaveCss, x.Html, x.Css, x.Introduction, x.Description })
                .ToListAsync(cancellationToken);
            foreach (var x in products)
            {
                sources.Add(Source("Product", x.Id, "Draft", Fields(("SaveHtml", x.SaveHtml), ("SaveCss", x.SaveCss))));
                sources.Add(Source("Product", x.Id, "Published", Fields(("Html", x.Html), ("Css", x.Css), ("Introduction", x.Introduction), ("Description", x.Description))));
            }

            var advertisements = await db.Advertise.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.SaveHtml, x.SaveCss, x.Html, x.Css, x.Img, x.Link, x.Describe })
                .ToListAsync(cancellationToken);
            foreach (var x in advertisements)
            {
                sources.Add(Source("Advertise", x.Id, "Draft", Fields(("SaveHtml", x.SaveHtml), ("SaveCss", x.SaveCss))));
                sources.Add(Source("Advertise", x.Id, "Published", Fields(("Html", x.Html), ("Css", x.Css), ("Img", x.Img), ("Link", x.Link), ("Describe", x.Describe))));
            }

            var contents = await db.Html_Contents.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.Html, x.Css, x.Img, x.Icon, x.Link })
                .ToListAsync(cancellationToken);
            sources.AddRange(contents.Select(x => Source("HtmlContent", x.Id, "Current", Fields(("Html", x.Html), ("Css", x.Css), ("Img", x.Img), ("Icon", x.Icon), ("Link", x.Link)))));

            var certificates = await db.TechnicalCertificates.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.SaveHtml, x.SaveCss, x.Html, x.Css, x.Img, x.Description })
                .ToListAsync(cancellationToken);
            foreach (var x in certificates)
            {
                sources.Add(Source("TechnicalCertificate", x.Id, "Draft", Fields(("SaveHtml", x.SaveHtml), ("SaveCss", x.SaveCss))));
                sources.Add(Source("TechnicalCertificate", x.Id, "Published", Fields(("Html", x.Html), ("Css", x.Css), ("Img", x.Img), ("Description", x.Description))));
            }

            var contacts = await (from contact in db.Contacts.AsNoTracking()
                                  join menu in db.WebMenus.AsNoTracking() on contact.FK_WebMenuId equals menu.Id
                                  where menu.FK_WebsiteId == websiteId && !contact.IsDeleted && !menu.IsDeleted
                                  select new { contact.Id, contact.Html }).ToListAsync(cancellationToken);
            sources.AddRange(contacts.Select(x => Source("Contact", x.Id, "Current", Fields(("Html", x.Html)))));

            var jsonObjects = await db.JsonObjects.AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.Json })
                .ToListAsync(cancellationToken);
            sources.AddRange(jsonObjects.Select(x => Source("JsonObject", x.Id, "Current", Fields(("Json", x.Json)))));

            var templates = await db.Templates.AsNoTracking()
                .Where(x => x.FK_WebsiteID == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.LayoutConfig, x.Css })
                .ToListAsync(cancellationToken);
            sources.AddRange(templates.Select(x => Source("Template", x.Id, "Current", Fields(("LayoutConfig", x.LayoutConfig), ("Css", x.Css)))));

            var sections = await (from section in db.TemplateSections.AsNoTracking()
                                  join template in db.Templates.AsNoTracking() on section.FK_TemplateID equals template.Id
                                  where template.FK_WebsiteID == websiteId && !section.IsDeleted && !template.IsDeleted
                                  select new { section.Id, section.sectionType, section.ContentConfig }).ToListAsync(cancellationToken);
            sources.AddRange(sections.Select(x => Source(
                (int)x.sectionType == 1 ? "Header" : "TemplateSection",
                x.Id,
                "Current",
                Fields(("ContentConfig", x.ContentConfig)))));

            var footers = await (from footer in db.FooterTemplates.AsNoTracking()
                                 join section in db.TemplateSections.AsNoTracking() on footer.FK_TemplateSectionsId equals section.Id
                                 join template in db.Templates.AsNoTracking() on section.FK_TemplateID equals template.Id
                                 where template.FK_WebsiteID == websiteId && !footer.IsDeleted && !section.IsDeleted && !template.IsDeleted
                                 select new { footer.Id, footer.html, footer.css, footer.saveHtml, footer.saveCss }).ToListAsync(cancellationToken);
            foreach (var x in footers)
            {
                sources.Add(Source("Footer", x.Id, "Draft", Fields(("SaveHtml", x.saveHtml), ("SaveCss", x.saveCss))));
                sources.Add(Source("Footer", x.Id, "Published", Fields(("Html", x.html), ("Css", x.css))));
            }

            await ReplaceSourcesAsync(websiteId, sources, true, userId, cancellationToken);
        }

        public async Task<int> RegisterUntrackedPhysicalFilesAsync(
            long websiteId,
            long? userId = null,
            CancellationToken cancellationToken = default)
        {
            var orgName = await GetOrgNameAsync(websiteId, cancellationToken);
            var root = uploadPathResolver.GetRootPath(orgName);
            var existingPaths = (await db.FileUploads.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId && x.DownloadFileName != null)
                .Select(x => x.DownloadFileName!)
                .ToListAsync(cancellationToken))
                .Select(path => NormalizeUploadPath(path, orgName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var added = 0;
            foreach (var physicalPath in System.IO.Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(root, physicalPath).Replace('\\', '/');
                var pathParts = relative.Split('/');
                if (pathParts.Any(part => part.StartsWith("_", StringComparison.Ordinal))
                    || pathParts.Any(part => part.Equals("logs", StringComparison.OrdinalIgnoreCase)
                        || part.Equals("temp", StringComparison.OrdinalIgnoreCase)
                        || part.Equals("BackgroundTasks", StringComparison.OrdinalIgnoreCase)))
                    continue;
                var downloadPath = "/upload/" + relative;
                if (!existingPaths.Add(NormalizeUploadPath(downloadPath, orgName)))
                    continue;
                db.FileUploads.Add(CreateFileUpload(websiteId, physicalPath, downloadPath, userId));
                added++;
            }
            if (added > 0)
                await db.SaveChangesAsync(cancellationToken);
            return added;
        }

        private async Task ReplaceSourcesAsync(
            long websiteId,
            IReadOnlyCollection<FileReferenceSource> sources,
            bool replaceWholeWebsite,
            long? userId,
            CancellationToken cancellationToken)
        {
            var orgName = await GetOrgNameAsync(websiteId, cancellationToken);
            var now = DateTime.Now;
            var parsed = sources.SelectMany(source => source.Fields.SelectMany(field =>
                    ExtractPaths(field.Value, orgName)
                        .GroupBy(path => path, StringComparer.OrdinalIgnoreCase)
                        .Select(group => new { source, Field = field.Key, Path = group.Key, Count = group.Count() })))
                .ToList();
            var uploads = await db.FileUploads.IgnoreQueryFilters()
                .Where(x => x.FK_WebsiteId == websiteId && x.DownloadFileName != null)
                .ToListAsync(cancellationToken);
            var uploadsByPath = uploads
                .GroupBy(x => NormalizeUploadPath(x.DownloadFileName!, orgName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            var addedUploads = false;
            foreach (var path in parsed.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (uploadsByPath.ContainsKey(path))
                    continue;
                var physicalPath = TryGetPhysicalPath(orgName, path);
                if (physicalPath == null || !File.Exists(physicalPath))
                    continue;
                var upload = CreateFileUpload(websiteId, physicalPath, path, userId);
                db.FileUploads.Add(upload);
                uploadsByPath[path] = upload;
                addedUploads = true;
            }
            if (addedUploads)
                await db.SaveChangesAsync(cancellationToken);

            List<FileReference> oldReferences;
            if (replaceWholeWebsite)
            {
                oldReferences = await db.FileReferences
                    .Where(x => x.FK_WebsiteId == websiteId)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                var source = sources.Single();
                oldReferences = await db.FileReferences
                    .Where(x => x.FK_WebsiteId == websiteId
                        && x.SourceType == source.SourceType
                        && x.SourceId == source.SourceId
                        && x.SourceState == source.SourceState)
                    .ToListAsync(cancellationToken);
            }
            foreach (var oldReference in oldReferences)
            {
                oldReference.IsDeleted = true;
                oldReference.DeletionTime = now;
                oldReference.DeleterUserId = userId;
            }

            foreach (var item in parsed)
            {
                uploadsByPath.TryGetValue(item.Path, out var upload);
                var physicalPath = TryGetPhysicalPath(orgName, item.Path);
                db.FileReferences.Add(new FileReference
                {
                    FK_WebsiteId = websiteId,
                    FK_FileUploadId = upload?.Id > 0 ? upload.Id : null,
                    SourceType = item.source.SourceType,
                    SourceId = item.source.SourceId,
                    SourceState = item.source.SourceState,
                    SourceField = item.Field,
                    NormalizedPath = item.Path,
                    OccurrenceCount = item.Count,
                    PhysicalFileExists = physicalPath != null && File.Exists(physicalPath),
                    LastConfirmedTime = now,
                    CreationTime = now,
                    CreatorUserId = userId ?? 0
                });
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        private static FileReferenceSource Source(string type, long id, string state, IReadOnlyDictionary<string, string?> fields)
            => new(type, id, state, fields);

        private static IReadOnlyDictionary<string, string?> Fields(params (string Name, string? Value)[] fields)
            => fields.ToDictionary(x => x.Name, x => x.Value);

        private static IEnumerable<string> ExtractPaths(string? value, string orgName)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;
            var decoded = WebUtility.HtmlDecode(value).Replace('\\', '/');
            foreach (Match match in UploadPathRegex.Matches(decoded))
                yield return NormalizeUploadPath(match.Value, orgName);
        }

        private static string NormalizeUploadPath(string value, string orgName)
        {
            string decoded;
            try
            {
                decoded = Uri.UnescapeDataString(value);
            }
            catch (UriFormatException)
            {
                decoded = value;
            }
            var path = decoded.Replace('\\', '/').Split('?', '#')[0].TrimEnd('/', ',', ';');
            var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex >= 0)
                path = path[uploadIndex..];
            var orgPrefix = $"/upload/{orgName}/";
            if (path.StartsWith(orgPrefix, StringComparison.OrdinalIgnoreCase))
                path = "/upload/" + path[orgPrefix.Length..];
            return path;
        }

        private string? TryGetPhysicalPath(string orgName, string downloadPath)
        {
            try
            {
                return uploadPathResolver.GetPhysicalPathFromDownloadFileName(orgName, downloadPath);
            }
            catch
            {
                return null;
            }
        }

        private static FileUpload CreateFileUpload(long websiteId, string physicalPath, string downloadPath, long? userId)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(physicalPath, out var contentType))
                contentType = "application/octet-stream";
            var info = new FileInfo(physicalPath);
            var stem = Path.GetFileNameWithoutExtension(info.Name);
            return new FileUpload
            {
                FK_WebsiteId = websiteId,
                GuidKey = Guid.NewGuid(),
                FileGuid = Guid.TryParse(stem, out var fileGuid) ? fileGuid : null,
                ContentType = contentType,
                OriginalFileName = info.Name,
                DownloadFileName = downloadPath,
                Size = info.Length,
                CreationTime = info.CreationTime,
                CreatorUserId = userId ?? 0
            };
        }

        private async Task<string> GetOrgNameAsync(long websiteId, CancellationToken cancellationToken)
        {
            return await db.Websites.AsNoTracking()
                .Where(x => x.Id == websiteId && !x.IsDeleted)
                .Select(x => x.OrgName)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("找不到網站資料。");
        }
    }
}
