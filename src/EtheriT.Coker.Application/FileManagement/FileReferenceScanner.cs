using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.FileManagement
{
    public interface IFileReferenceScanner
    {
        Task<HashSet<long>> GetReferencedFileIdsAsync(
            long websiteId,
            CancellationToken cancellationToken = default);
    }

    public sealed class FileReferenceScanner : IFileReferenceScanner
    {
        private static readonly Regex UploadPathRegex = new(
            "/upload/[^\\s\\\"'<>\\)\\(]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly CokerDbContext db;

        public FileReferenceScanner(CokerDbContext db)
        {
            this.db = db;
        }

        public async Task<HashSet<long>> GetReferencedFileIdsAsync(
            long websiteId,
            CancellationToken cancellationToken = default)
        {
            var website = await db.Websites
                .AsNoTracking()
                .Where(item => item.Id == websiteId)
                .Select(item => new
                {
                    item.OrgName,
                    item.DefaultUrl,
                    item.Icon,
                    item.Logo,
                    item.Css,
                    item.Description,
                    item.Statement
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (website == null)
                return new HashSet<long>();

            var uploads = await db.FileUploads
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId)
                .Select(item => new
                {
                    item.Id,
                    item.GuidKey,
                    item.DownloadFileName
                })
                .ToListAsync(cancellationToken);
            var uploadsByPath = uploads
                .Where(item => !string.IsNullOrWhiteSpace(item.DownloadFileName))
                .GroupBy(item => NormalizeUploadPath(item.DownloadFileName!, website.OrgName))
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.Id).ToArray(),
                    StringComparer.OrdinalIgnoreCase);
            var uploadIdByGuid = uploads.ToDictionary(item => item.GuidKey, item => item.Id);
            var uploadIds = uploads.Select(item => item.Id).ToHashSet();

            var referenced = (await (
                from binding in db.FileBinds.AsNoTracking()
                join upload in db.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on binding.FK_FileUploadId equals (long?)upload.Id
                where !binding.IsDeleted && upload.FK_WebsiteId == websiteId
                select upload.Id
            ).Distinct().ToListAsync(cancellationToken)).ToHashSet();

            var texts = new List<string?>
            {
                website.DefaultUrl,
                website.Icon,
                website.Logo,
                website.Css,
                website.Description,
                website.Statement
            };
            await AppendContentTextsAsync(texts, websiteId, cancellationToken);

            foreach (var text in texts.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                foreach (Match match in UploadPathRegex.Matches(
                    WebUtility.HtmlDecode(text!).Replace('\\', '/')))
                {
                    var path = NormalizeUploadPath(match.Value, website.OrgName);
                    if (!uploadsByPath.TryGetValue(path, out var ids))
                        continue;

                    foreach (var id in ids)
                        referenced.Add(id);
                }
            }

            var relations = await (
                from relation in db.FileBindMores.AsNoTracking()
                join upload in db.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on relation.FK_FileUploadId equals (long?)upload.Id
                where !relation.IsDeleted && upload.FK_WebsiteId == websiteId
                select new { relation.FK_FileBindGuid, FileId = upload.Id }
            ).ToListAsync(cancellationToken);
            var familyEdges = new Dictionary<long, HashSet<long>>();
            foreach (var relation in relations)
            {
                if (!uploadIdByGuid.TryGetValue(relation.FK_FileBindGuid, out var parentId)
                    || !uploadIds.Contains(relation.FileId))
                    continue;

                AddFamilyEdge(familyEdges, parentId, relation.FileId);
                AddFamilyEdge(familyEdges, relation.FileId, parentId);
            }

            var queue = new Queue<long>(referenced);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!familyEdges.TryGetValue(current, out var family))
                    continue;

                foreach (var familyId in family)
                {
                    if (referenced.Add(familyId))
                        queue.Enqueue(familyId);
                }
            }

            return referenced;
        }

        private async Task AppendContentTextsAsync(
            List<string?> texts,
            long websiteId,
            CancellationToken cancellationToken)
        {
            var articles = await db.Article.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new
                {
                    item.SaveHtml, item.SaveCss, item.Html, item.Css,
                    item.NewsletterHtml, item.NewsletterCss, item.DataJson,
                    item.Description, item.Subtitle
                }).ToListAsync(cancellationToken);
            articles.ForEach(item => texts.AddRange(new[] {
                item.SaveHtml, item.SaveCss, item.Html, item.Css,
                item.NewsletterHtml, item.NewsletterCss, item.DataJson,
                item.Description, item.Subtitle
            }));

            var menus = await db.WebMenus.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new {
                    item.SaveHtml, item.SaveCss, item.Html, item.Css,
                    item.icon, item.LinkUrl, item.Description
                }).ToListAsync(cancellationToken);
            menus.ForEach(item => texts.AddRange(new[] {
                item.SaveHtml, item.SaveCss, item.Html, item.Css,
                item.icon, item.LinkUrl, item.Description
            }));

            var products = await db.Prods.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new {
                    item.SaveHtml, item.SaveCss, item.Html, item.Css,
                    item.Introduction, item.Description
                })
                .ToListAsync(cancellationToken);
            products.ForEach(item => texts.AddRange(new[] {
                item.SaveHtml, item.SaveCss, item.Html, item.Css,
                item.Introduction, item.Description
            }));

            var advertisements = await db.Advertise.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new {
                    item.SaveHtml, item.SaveCss, item.Html, item.Css,
                    item.Img, item.Link, item.Describe
                }).ToListAsync(cancellationToken);
            advertisements.ForEach(item => texts.AddRange(new[] {
                item.SaveHtml, item.SaveCss, item.Html, item.Css,
                item.Img, item.Link, item.Describe
            }));

            var htmlContents = await db.Html_Contents.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new { item.Html, item.Css, item.Img, item.Icon, item.Link })
                .ToListAsync(cancellationToken);
            htmlContents.ForEach(item => texts.AddRange(new[] {
                item.Html, item.Css, item.Img, item.Icon, item.Link
            }));

            var certificates = await db.TechnicalCertificates.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => new {
                    item.SaveHtml, item.SaveCss, item.Html, item.Css,
                    item.Img, item.Description
                }).ToListAsync(cancellationToken);
            certificates.ForEach(item => texts.AddRange(new[] {
                item.SaveHtml, item.SaveCss, item.Html, item.Css,
                item.Img, item.Description
            }));

            texts.AddRange(await (
                from contact in db.Contacts.AsNoTracking()
                join menu in db.WebMenus.AsNoTracking()
                    on contact.FK_WebMenuId equals menu.Id
                where !contact.IsDeleted
                    && !menu.IsDeleted
                    && menu.FK_WebsiteId == websiteId
                select contact.Html
            ).ToListAsync(cancellationToken));

            texts.AddRange(await db.JsonObjects.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId && !item.IsDeleted)
                .Select(item => item.Json)
                .ToListAsync(cancellationToken));

            var templates = await db.Templates.AsNoTracking()
                .Where(item => item.FK_WebsiteID == websiteId && !item.IsDeleted)
                .Select(item => new { item.LayoutConfig, item.Css })
                .ToListAsync(cancellationToken);
            templates.ForEach(item => texts.AddRange(new[] { item.LayoutConfig, item.Css }));

            var sections = await (
                from section in db.TemplateSections.AsNoTracking()
                join template in db.Templates.AsNoTracking()
                    on section.FK_TemplateID equals template.Id
                where template.FK_WebsiteID == websiteId
                    && !section.IsDeleted
                    && !template.IsDeleted
                select section.ContentConfig
            ).ToListAsync(cancellationToken);
            texts.AddRange(sections);

            var footers = await (
                from footer in db.FooterTemplates.AsNoTracking()
                join section in db.TemplateSections.AsNoTracking()
                    on footer.FK_TemplateSectionsId equals section.Id
                join template in db.Templates.AsNoTracking()
                    on section.FK_TemplateID equals template.Id
                where template.FK_WebsiteID == websiteId
                    && !footer.IsDeleted
                    && !section.IsDeleted
                    && !template.IsDeleted
                select new { footer.html, footer.css, footer.saveHtml, footer.saveCss }
            ).ToListAsync(cancellationToken);
            footers.ForEach(item => texts.AddRange(new[] {
                item.html, item.css, item.saveHtml, item.saveCss
            }));
        }

        private static void AddFamilyEdge(
            Dictionary<long, HashSet<long>> edges,
            long source,
            long target)
        {
            if (!edges.TryGetValue(source, out var targets))
            {
                targets = new HashSet<long>();
                edges[source] = targets;
            }
            targets.Add(target);
        }

        private static string NormalizeUploadPath(string value, string orgName)
        {
            var path = Uri.UnescapeDataString(value)
                .Replace('\\', '/')
                .Split('?', '#')[0]
                .TrimEnd('/', ',', ';');
            var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex >= 0)
                path = path[uploadIndex..];
            var orgPrefix = $"/upload/{orgName}/";
            if (path.StartsWith(orgPrefix, StringComparison.OrdinalIgnoreCase))
                path = "/upload/" + path[orgPrefix.Length..];
            return path;
        }
    }
}
