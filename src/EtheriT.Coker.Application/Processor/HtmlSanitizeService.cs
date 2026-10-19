using EtheriT.Coker.Application.Processor.Option;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace EtheriT.Coker.Application.Processor
{
    public class HtmlSanitizeService : IHtmlSanitizeService
    {
        private readonly CokerDbContext db;
        private readonly IHtmlSanitizer htmlSanitizer;
        private readonly IUploadPathResolver uploadPaths;
        private readonly ILogger<HtmlSanitizeService> logger;
        private readonly IHttpContextAccessor contextAccessor;
        private static readonly object RequestMemoKey = new();
        private sealed record Memo(string Html, string Css, HtmlSanitizeResult Result);

        public HtmlSanitizeService(
            CokerDbContext db,
            IHtmlSanitizer htmlSanitizer,
            IUploadPathResolver uploadPaths,
            ILogger<HtmlSanitizeService> logger,
            IHttpContextAccessor contextAccessor)
        {
            this.db = db;
            this.htmlSanitizer = htmlSanitizer;
            this.uploadPaths = uploadPaths;
            this.logger = logger;
            this.contextAccessor = contextAccessor;
        }

        public async Task<HtmlSanitizeResult> EnsurePublicContentAsync(HtmlSanitizeInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            NormalizeInput(input);

            var memo = GetRequestMemo();
            var memoKey = (input.WebsiteId, (int)input.SourceType, input.SourceId, input.ContentKey, input.SanitizePolicy);
            if (!input.Force && memo != null && memo.TryGetValue(memoKey, out var cached)
                && ((cached.Html == input.Html && cached.Css == input.Css)
                    || (cached.Result.Html == input.Html && cached.Result.Css == input.Css)))
                return CopyForRead(cached.Result);

            HtmlSanitizeResult Remember(HtmlSanitizeResult result)
            {
                if (memo != null && input.SourceId > 0)
                    memo[memoKey] = new Memo(input.Html, input.Css, CopyForRead(result));
                return result;
            }

            var version = HtmlSanitizerVersions.PublicHtml;

            var currentInputHash = ComputeContentHash(
                input.Html,
                input.Css,
                input.SanitizePolicy,
                version
            );

            var state = await FindStateAsync(
                input.WebsiteId,
                input.SourceType,
                input.SourceId,
                input.ContentKey,
                input.SanitizePolicy
            );

            var isCurrent =
                !input.Force &&
                state != null &&
                state.SanitizeVersion == version &&
                state.ContentHash == currentInputHash;

            if (isCurrent)
            {
                var published = await GetPublishedHtmlAsync(input, input.Html);
                return Remember(new HtmlSanitizeResult
                {
                    Html = input.Html,
                    PublishedHtml = published.Html,
                    FirstImageUrl = published.FirstImageUrl,
                    HasImageMetadata = published.HasImageMetadata,
                    Css = input.Css,
                    ContentHash = currentInputHash,
                    SanitizeVersion = version,
                    SanitizePolicy = input.SanitizePolicy,
                    WasSanitized = false
                });
            }

            var cleanHtml = htmlSanitizer.SanitizePublicHtml(input.Html ?? "");
            var cleanCss = htmlSanitizer.SanitizePublicCss(input.Css ?? "");

            var cleanHash = ComputeContentHash(
                cleanHtml,
                cleanCss,
                input.SanitizePolicy,
                version
            );

            await UpsertStateAsync(input, version, cleanHash, state);

            var cleanPublished = await GetPublishedHtmlAsync(input, cleanHtml);
            return Remember(new HtmlSanitizeResult
            {
                Html = cleanHtml,
                PublishedHtml = cleanPublished.Html,
                FirstImageUrl = cleanPublished.FirstImageUrl,
                HasImageMetadata = cleanPublished.HasImageMetadata,
                Css = cleanCss,
                ContentHash = cleanHash,
                SanitizeVersion = version,
                SanitizePolicy = input.SanitizePolicy,
                WasSanitized = true
            });
        }

        public string RepairLegacyPublishedHtml(string publishedHtml, string editorHtml)
        {
            return htmlSanitizer.RepairLegacyPublishedHtml(publishedHtml, editorHtml);
        }

        private async Task<PublishedHtmlFiles.Result> GetPublishedHtmlAsync(HtmlSanitizeInput input, string html)
        {
            if (input.WebsiteId <= 0 || input.SourceId <= 0 || string.IsNullOrWhiteSpace(html))
                return new(html, null, false);

            var orgName = await db.Websites.AsNoTracking()
                .Where(site => site.Id == input.WebsiteId)
                .Select(site => site.OrgName).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(orgName)) return new(html, null, false);
            try
            {
                return await PublishedHtmlFiles.GetAsync(uploadPaths, orgName, input.WebsiteId,
                    (int)input.SourceType, input.SourceId, input.ContentKey, input.SanitizePolicy, html,
                    ex => logger.LogWarning(ex,
                        "已發布 HTML 儲存／資源讀取失敗，使用本次產生結果：Website={WebsiteId}, Source={SourceType}/{SourceId}",
                        input.WebsiteId, input.SourceType, input.SourceId));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
                or System.Security.SecurityException or NotSupportedException
                or System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                // 產生檔是衍生資料；權限／磁碟異常不能阻斷發布或輸出未清洗內容。
                logger.LogWarning(ex, "無法建立已發布 HTML 產物：Website={WebsiteId}, Source={SourceType}/{SourceId}",
                    input.WebsiteId, input.SourceType, input.SourceId);
                return new(html, null, false);
            }
        }

        private Dictionary<(long, int, long, string, string), Memo>? GetRequestMemo()
        {
            var context = contextAccessor.HttpContext;
            if (context == null || !(HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)))
                return null;
            if (!context.Items.TryGetValue(RequestMemoKey, out var value))
                context.Items[RequestMemoKey] = value = new Dictionary<(long, int, long, string, string), Memo>();
            return (Dictionary<(long, int, long, string, string), Memo>)value!;
        }

        private static HtmlSanitizeResult CopyForRead(HtmlSanitizeResult result) => new()
        {
            Html = result.Html, Css = result.Css, PublishedHtml = result.PublishedHtml,
            ContentHash = result.ContentHash, SanitizeVersion = result.SanitizeVersion,
            SanitizePolicy = result.SanitizePolicy, FirstImageUrl = result.FirstImageUrl,
            HasImageMetadata = result.HasImageMetadata, WasSanitized = false
        };

        public async Task<bool> IsCurrentAsync(
            long websiteId,
            HtmlSanitizeSourceType sourceType,
            long sourceId,
            string html,
            string css,
            string contentKey = "Default",
            string sanitizePolicy = "PublicHtml")
        {
            contentKey = NormalizeKey(contentKey, "Default");
            sanitizePolicy = NormalizeKey(sanitizePolicy, "PublicHtml");

            var state = await FindStateAsync(
                websiteId,
                sourceType,
                sourceId,
                contentKey,
                sanitizePolicy
            );

            if (state == null ||
                state.SanitizeVersion != HtmlSanitizerVersions.PublicHtml ||
                string.IsNullOrWhiteSpace(state.ContentHash))
            {
                return false;
            }

            var currentContentHash = ComputeContentHash(
                html ?? "",
                css ?? "",
                sanitizePolicy,
                HtmlSanitizerVersions.PublicHtml
            );

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(state.ContentHash),
                Encoding.UTF8.GetBytes(currentContentHash)
            );
        }

        private async Task<HtmlSanitizeState?> FindStateAsync(
            long websiteId,
            HtmlSanitizeSourceType sourceType,
            long sourceId,
            string contentKey,
            string sanitizePolicy)
        {
            return await db.HtmlSanitizeStates.FirstOrDefaultAsync(e =>
                e.FK_WebsiteId == websiteId &&
                e.SourceType == sourceType &&
                e.FK_Bid == sourceId &&
                e.ContentKey == contentKey &&
                e.SanitizePolicy == sanitizePolicy
            );
        }

        private async Task UpsertStateAsync(
            HtmlSanitizeInput input,
            string version,
            string contentHash,
            HtmlSanitizeState? state)
        {
            var isNew = state == null;
            if (state == null)
            {
                state = new HtmlSanitizeState
                {
                    FK_WebsiteId = input.WebsiteId,
                    SourceType = input.SourceType,
                    FK_Bid = input.SourceId,
                    ContentKey = input.ContentKey,
                    SanitizePolicy = input.SanitizePolicy,
                    SanitizeVersion = version,
                    ContentHash = contentHash
                };

                db.HtmlSanitizeStates.Add(state);
            }
            else
            {
                state.SanitizeVersion = version;
                state.ContentHash = contentHash;
                state.LastModificationTime = DateTime.Now;
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException) when (isNew)
            {
                // 舊資料第一次被多個前台 request 同時讀取時，unique index 只允許一筆 state。
                // 失敗的 request 改讀取先完成的 state，再將它更新成這次清洗結果。
                db.Entry(state).State = EntityState.Detached;

                var concurrentState = await FindStateAsync(
                    input.WebsiteId,
                    input.SourceType,
                    input.SourceId,
                    input.ContentKey,
                    input.SanitizePolicy
                );

                if (concurrentState == null)
                    throw;

                concurrentState.SanitizeVersion = version;
                concurrentState.ContentHash = contentHash;
                concurrentState.LastModificationTime = DateTime.Now;
                await db.SaveChangesAsync();
            }
        }

        private static void NormalizeInput(HtmlSanitizeInput input)
        {
            input.ContentKey = NormalizeKey(input.ContentKey, "Default");
            input.SanitizePolicy = NormalizeKey(input.SanitizePolicy, "PublicHtml");
            input.Html ??= "";
            input.Css ??= "";
        }

        private static string NormalizeKey(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }

        private static string ComputeContentHash(
            string html,
            string css,
            string sanitizePolicy,
            string sanitizeVersion)
        {
            var source = string.Join("\n", new[]
            {
                "POLICY:",
                sanitizePolicy ?? "",
                "VERSION:",
                sanitizeVersion ?? "",
                "HTML:",
                html ?? "",
                "CSS:",
                css ?? ""
            });

            using var sha = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(source);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
