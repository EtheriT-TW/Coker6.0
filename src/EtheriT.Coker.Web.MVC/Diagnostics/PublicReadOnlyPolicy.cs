using System.Text.Json.Nodes;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// Reviewed public read endpoints only. Never infer read-only behavior from POST/GET
// or an action name. No customer cookies or authentication are forwarded.
public sealed class PublicReadOnlyPolicy(Uri origin, long websiteId, IReadOnlySet<string> features)
{
    internal byte[] PrepareBody(byte[] body)
    {
        var input = JsonNode.Parse(body)!.AsObject();
        foreach (var key in input.Select(pair => pair.Key).Where(key => key.Equals("SiteId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("WebsiteId", StringComparison.OrdinalIgnoreCase)).ToList()) input.Remove(key);
        input["SiteId"] = websiteId;
        input["WebsiteId"] = websiteId;
        foreach (var name in new[] { "ShowNum", "MaxLen" })
        {
            var key = input.Select(pair => pair.Key).FirstOrDefault(key => key.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (key == null || input[key] == null || input[key]!.GetValue<int>() == 0)
                input[key ?? name] = name == "ShowNum" ? 12 : 100;
        }
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(input);
    }
    private static readonly HashSet<string> DirectoryReads = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/Directory/GetReleInfo", "/api/Directory/GetReleMenu", "/api/Directory/GetReleAd",
        "/api/Directory/GetReleAdBatch", "/api/Directory/SwitchPage", "/api/Directory/GetFacet",
        "/api/Directory/GetSearchKeyList"
    };
    private static readonly HashSet<string> PublicGetReads = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/Marquee/GetAll", "/api/WebMenu/GetMainMenu", "/api/Tags/GetArticleDataAll"
    };

    internal bool BlocksUnreviewedGet(Uri uri, string method) => (method is "GET" or "HEAD")
        && PublicPageChecks.SameOrigin(origin, uri) && uri.AbsolutePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
        && !PublicGetReads.Contains(uri.AbsolutePath.TrimEnd('/'));

    internal bool Allows(Uri uri, string method, byte[]? body)
    {
        if (method != "POST" || websiteId <= 0 || !PublicPageChecks.SameOrigin(origin, uri)
            || uri.Query.Length != 0 || body is not { Length: > 0 and <= 65536 }) return false;
        var path = uri.AbsolutePath.TrimEnd('/');
        if (!DirectoryReads.Contains(path)) return false;
        // These endpoints return directory/search display DTOs and facet definitions;
        // none calls the edit/delete/facet-save actions. Commerce writes remain denied.
        try
        {
            if (JsonNode.Parse(body) is not JsonObject input) return false;
            foreach (var pair in input)
            {
                var key = pair.Key.ToLowerInvariant();
                if (key is "siteid" or "websiteid")
                {
                    var value = pair.Value?.GetValue<long>() ?? 0;
                    if (value != 0 && value != websiteId) return false;
                }
                if (key is "shownum" or "maxlen" or "page" or "totalpage")
                {
                    var value = pair.Value?.GetValue<int>() ?? 0;
                    if (value < 0 || value > 100) return false;
                }
                if ((key is "ids" or "dirids") && pair.Value is JsonArray ids && ids.Count > 100) return false;
                if ((key is "searchtext" or "productnamefilter") && pair.Value?.ToString().Length > 200) return false;
                if (key == "type" && pair.Value?.ToString().Equals("search", StringComparison.OrdinalIgnoreCase) == true
                    && !features.Contains("CustomPage")) return false;
            }
            // Site-bound endpoints accept these fields; the signed-in MVC site cannot
            // be replaced by another tab or by data attributes on the public page.
            return true;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidOperationException or FormatException)
        { return false; }
    }
}
