using EtheriT.Coker.Application.Processor.Option;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EtheriT.Coker.Application.Processor
{
    public class HtmlSanitizeService : IHtmlSanitizeService
    {
        private readonly CokerDbContext db;
        private readonly IHtmlSanitizer htmlSanitizer;

        public HtmlSanitizeService(
            CokerDbContext db,
            IHtmlSanitizer htmlSanitizer)
        {
            this.db = db;
            this.htmlSanitizer = htmlSanitizer;
        }

        public async Task<HtmlSanitizeResult> EnsurePublicContentAsync(HtmlSanitizeInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            NormalizeInput(input);

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
                return new HtmlSanitizeResult
                {
                    Html = input.Html,
                    Css = input.Css,
                    ContentHash = currentInputHash,
                    SanitizeVersion = version,
                    SanitizePolicy = input.SanitizePolicy,
                    WasSanitized = false
                };
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

            return new HtmlSanitizeResult
            {
                Html = cleanHtml,
                Css = cleanCss,
                ContentHash = cleanHash,
                SanitizeVersion = version,
                SanitizePolicy = input.SanitizePolicy,
                WasSanitized = true
            };
        }

        public async Task<bool> IsCurrentAsync(
            long websiteId,
            HtmlSanitizeSourceType sourceType,
            long sourceId,
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

            return state != null &&
                   state.SanitizeVersion == HtmlSanitizerVersions.PublicHtml &&
                   !string.IsNullOrWhiteSpace(state.ContentHash);
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

            await db.SaveChangesAsync();
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