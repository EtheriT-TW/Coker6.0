using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.BackgroundJob
{
    /// <summary>
    /// Gradually persists legacy YTmodal_frame attributes as data-* attributes.
    /// The job is idempotent and uses the original column values as an optimistic
    /// concurrency check, so an editor save is never overwritten by a stale batch.
    /// </summary>
    public sealed class HtmlLegacyAttributeNormalizationJob
    {
        private const int BatchSize = 100;

        private readonly CokerDbContext db;
        private readonly StringHandler stringHandler;
        private readonly IHtmlSanitizer htmlSanitizer;
        private readonly IHtmlSanitizeService htmlSanitizeService;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly ILogger<HtmlLegacyAttributeNormalizationJob> logger;

        public HtmlLegacyAttributeNormalizationJob(
            CokerDbContext db,
            StringHandler stringHandler,
            IHtmlSanitizer htmlSanitizer,
            IHtmlSanitizeService htmlSanitizeService,
            IHtmlProcessor htmlProcessor,
            ILogger<HtmlLegacyAttributeNormalizationJob> logger)
        {
            this.db = db;
            this.stringHandler = stringHandler;
            this.htmlSanitizer = htmlSanitizer;
            this.htmlSanitizeService = htmlSanitizeService;
            this.htmlProcessor = htmlProcessor;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(1800)]
        public async Task Run()
        {
            var updated = 0;
            updated += await ProcessArticlesAsync();
            updated += await ProcessProductsAsync();
            updated += await ProcessMenusAsync();
            updated += await ProcessAdvertisesAsync();
            updated += await ProcessFootersAsync();
            updated += await ProcessHtmlContentsAsync();

            if (updated > 0)
                logger.LogInformation("Normalized legacy HTML attributes for {Count} records.", updated);
        }

        private async Task<int> ProcessArticlesAsync()
        {
            var rows = await db.Article.AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    ((x.Html != null && (x.Html.Contains(" link=") || x.Html.Contains(" yttitle="))) ||
                     (x.SaveHtml != null && (x.SaveHtml.Contains(" link=") || x.SaveHtml.Contains(" yttitle=")))))
                .OrderBy(x => x.Id)
                .Select(x => new ContentRow(x.Id, x.FK_WebsiteId, x.Html, x.Css, x.SaveHtml, x.PageText))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var content = await NormalizeRowAsync(row, HtmlSanitizeSourceType.文章, true);
                if (!content.HasChanges) continue;

                updated += await db.Article
                    .Where(x => x.Id == row.Id && x.Html == row.Html && x.SaveHtml == row.SaveHtml)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Html, content.Html)
                        .SetProperty(x => x.Css, content.Css)
                        .SetProperty(x => x.SaveHtml, content.SaveHtml)
                        .SetProperty(x => x.PageText, content.PageText));
            }
            return updated;
        }

        private async Task<int> ProcessProductsAsync()
        {
            var rows = await db.Prods.AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    ((x.Html != null && (x.Html.Contains(" link=") || x.Html.Contains(" yttitle="))) ||
                     (x.SaveHtml != null && (x.SaveHtml.Contains(" link=") || x.SaveHtml.Contains(" yttitle=")))))
                .OrderBy(x => x.Id)
                .Select(x => new ContentRow(x.Id, x.FK_WebsiteId, x.Html, x.Css, x.SaveHtml, x.PageText))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var content = await NormalizeRowAsync(row, HtmlSanitizeSourceType.商品, true);
                if (!content.HasChanges) continue;

                updated += await db.Prods
                    .Where(x => x.Id == row.Id && x.Html == row.Html && x.SaveHtml == row.SaveHtml)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Html, content.Html)
                        .SetProperty(x => x.Css, content.Css)
                        .SetProperty(x => x.SaveHtml, content.SaveHtml)
                        .SetProperty(x => x.PageText, content.PageText));
            }
            return updated;
        }

        private async Task<int> ProcessMenusAsync()
        {
            var rows = await db.WebMenus.AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    ((x.Html != null && (x.Html.Contains(" link=") || x.Html.Contains(" yttitle="))) ||
                     (x.SaveHtml != null && (x.SaveHtml.Contains(" link=") || x.SaveHtml.Contains(" yttitle=")))))
                .OrderBy(x => x.Id)
                .Select(x => new ContentRow(x.Id, x.FK_WebsiteId, x.Html, x.Css, x.SaveHtml, x.PageText))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var content = await NormalizeRowAsync(row, HtmlSanitizeSourceType.選單, true);
                if (!content.HasChanges) continue;

                updated += await db.WebMenus
                    .Where(x => x.Id == row.Id && x.Html == row.Html && x.SaveHtml == row.SaveHtml)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Html, content.Html)
                        .SetProperty(x => x.Css, content.Css)
                        .SetProperty(x => x.SaveHtml, content.SaveHtml)
                        .SetProperty(x => x.PageText, content.PageText));
            }
            return updated;
        }

        private async Task<int> ProcessAdvertisesAsync()
        {
            var rows = await db.Advertise.AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    ((x.Html != null && (x.Html.Contains(" link=") || x.Html.Contains(" yttitle="))) ||
                     (x.SaveHtml != null && (x.SaveHtml.Contains(" link=") || x.SaveHtml.Contains(" yttitle=")))))
                .OrderBy(x => x.Id)
                .Select(x => new ContentRow(x.Id, x.FK_WebsiteId, x.Html, x.Css, x.SaveHtml, null))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var content = await NormalizeRowAsync(row, HtmlSanitizeSourceType.廣告, false);
                if (!content.HasChanges) continue;

                updated += await db.Advertise
                    .Where(x => x.Id == row.Id && x.Html == row.Html && x.SaveHtml == row.SaveHtml)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Html, content.Html)
                        .SetProperty(x => x.Css, content.Css)
                        .SetProperty(x => x.SaveHtml, content.SaveHtml));
            }
            return updated;
        }

        private async Task<int> ProcessFootersAsync()
        {
            var rows = await db.FooterTemplates.AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    ((x.html != null && (x.html.Contains(" link=") || x.html.Contains(" yttitle="))) ||
                     (x.saveHtml != null && (x.saveHtml.Contains(" link=") || x.saveHtml.Contains(" yttitle=")))))
                .OrderBy(x => x.Id)
                .Select(x => new ContentRow(
                    x.Id,
                    x.templateSections.template.FK_WebsiteID,
                    x.html,
                    x.css,
                    x.saveHtml,
                    null))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var content = await NormalizeRowAsync(row, HtmlSanitizeSourceType.頁尾, false);
                if (!content.HasChanges) continue;

                updated += await db.FooterTemplates
                    .Where(x => x.Id == row.Id && x.html == row.Html && x.saveHtml == row.SaveHtml)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.html, content.Html)
                        .SetProperty(x => x.css, content.Css)
                        .SetProperty(x => x.saveHtml, content.SaveHtml));
            }
            return updated;
        }

        private async Task<int> ProcessHtmlContentsAsync()
        {
            var rows = await db.Html_Contents.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Html != null &&
                    (x.Html.Contains(" link=") || x.Html.Contains(" yttitle=")))
                .OrderBy(x => x.Id)
                .Select(x => new HtmlContentRow(x.Id, x.Html))
                .Take(BatchSize)
                .ToListAsync();

            var updated = 0;
            foreach (var row in rows)
            {
                var decodedHtml = DecodeStoredComponentHtml(row.Html ?? string.Empty);
                var normalizedHtml = htmlSanitizer.NormalizeLegacyAttributes(decodedHtml);
                if (string.Equals(decodedHtml, normalizedHtml, StringComparison.Ordinal))
                    continue;

                var encodedHtml = stringHandler.HtmlEncode(normalizedHtml);
                updated += await db.Html_Contents
                    .Where(x => x.Id == row.Id && x.Html == row.Html)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Html, encodedHtml));
            }

            return updated;
        }

        private static string DecodeStoredComponentHtml(string html)
        {
            // Html_Contents may contain more than one legacy HtmlEncode layer.
            // Component rows represent markup, so decode until the value is
            // stable before parsing and persist it with one canonical encoding.
            var decoded = html;
            for (var index = 0; index < 5; index++)
            {
                var next = System.Net.WebUtility.HtmlDecode(decoded);
                if (string.Equals(next, decoded, StringComparison.Ordinal))
                    break;

                decoded = next;
            }

            return decoded;
        }

        private async Task<NormalizedContent> NormalizeRowAsync(
            ContentRow row,
            HtmlSanitizeSourceType sourceType,
            bool updatePageText)
        {
            var decodedHtml = stringHandler.HtmlDecode(row.Html ?? string.Empty);
            var normalizedHtml = htmlSanitizer.NormalizeLegacyAttributes(decodedHtml);
            var publishedChanged = !string.Equals(decodedHtml, normalizedHtml, StringComparison.Ordinal);

            var decodedSaveHtml = stringHandler.HtmlDecode(row.SaveHtml ?? string.Empty);
            var normalizedSaveHtml = htmlSanitizer.NormalizeLegacyAttributes(decodedSaveHtml);
            var draftChanged = !string.Equals(decodedSaveHtml, normalizedSaveHtml, StringComparison.Ordinal);

            if (!publishedChanged && !draftChanged)
                return NormalizedContent.Unchanged(row);

            var outputHtml = row.Html;
            var outputCss = row.Css;
            var pageText = row.PageText;

            if (publishedChanged)
            {
                var sanitized = await htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
                {
                    WebsiteId = row.WebsiteId,
                    SourceType = sourceType,
                    SourceId = row.Id,
                    ContentKey = "Published",
                    SanitizePolicy = "PublicHtml",
                    Html = normalizedHtml,
                    Css = row.Css ?? string.Empty,
                    Force = true
                });

                outputHtml = stringHandler.HtmlEncode(sanitized.Html);
                outputCss = sanitized.Css;
                if (updatePageText)
                    pageText = htmlProcessor.text(sanitized.Html);
            }

            return new NormalizedContent(
                outputHtml,
                outputCss,
                draftChanged ? stringHandler.HtmlEncode(normalizedSaveHtml) : row.SaveHtml,
                pageText,
                true);
        }

        private sealed record ContentRow(
            long Id,
            long WebsiteId,
            string? Html,
            string? Css,
            string? SaveHtml,
            string? PageText);

        private sealed record HtmlContentRow(long Id, string? Html);

        private sealed record NormalizedContent(
            string? Html,
            string? Css,
            string? SaveHtml,
            string? PageText,
            bool HasChanges)
        {
            public static NormalizedContent Unchanged(ContentRow row) =>
                new(row.Html, row.Css, row.SaveHtml, row.PageText, false);
        }
    }
}
