using EtheriT.Coker.Application.Shared.Search;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static void SearchKeywordEncoding()
    {
        const string keyword = "LL-17 FE+";
        const string encoded = "~b64~TEwtMTcgRkUr";
        Require(SearchKeywordCodec.Encode(keyword) == encoded && SearchKeywordCodec.Decode(encoded) == keyword,
            "Keyword encoding must match the UTF-8 Base64Url browser protocol.");
        foreach (var text in new[] { "中文機油 + / ? # & %2B", "emoji 🚗", "  space  ", "\uFEFF標籤", "~b64~TEwtMTcgRkUr" })
        {
            var segment = SearchKeywordCodec.Encode(text);
            Require(!segment.Contains('+') && !segment.Contains('/') && !segment.Contains('=') && !segment.Contains('%'),
                "Generated route segments must avoid IIS-sensitive and URL delimiter characters.");
            Require(SearchKeywordCodec.Decode(segment) == text, "Keyword round-trip must preserve text without trimming or double decoding.");
        }
        foreach (var legacy in new[] { "LL-17 FE+", "中文", "literal%2B", "TEwtMTcgRkUr", "~b64~", "~b64~bad*", "~b64~YQ==", "~b64~_w", "~b64~YR" })
            Require(SearchKeywordCodec.Decode(legacy) == legacy, "Unmarked or malformed legacy keywords must stay literal.");
        Require(SearchKeywordCodec.Decode(null) == "" && SearchKeywordCodec.Encode("") == "", "Empty searches must keep their existing behavior.");
    }
}
