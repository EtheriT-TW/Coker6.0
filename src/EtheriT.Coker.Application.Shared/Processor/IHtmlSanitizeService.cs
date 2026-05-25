using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Processor
{
    public interface IHtmlSanitizeService
    {
        public Task<HtmlSanitizeResult> EnsurePublicContentAsync(HtmlSanitizeInput input);

        public Task<bool> IsCurrentAsync(
            long websiteId,
            HtmlSanitizeSourceType sourceType,
            long sourceId,
            string contentKey = "Default",
            string sanitizePolicy = "PublicHtml"
        );
    }
}
