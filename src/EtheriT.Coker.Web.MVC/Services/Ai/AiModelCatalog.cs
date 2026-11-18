using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed record AiModelOption(string Id, string Name);
public sealed record AiModelListResult(int Status, IReadOnlyList<AiModelOption> Models, string Message);

// Only metadata GETs to fixed provider endpoints. No inference, persistence or shared credential cache.
public sealed class AiModelCatalog(IHttpClientFactory clients)
{
    public async Task<AiModelListResult> ListAsync(WebsiteAiSettings settings, CancellationToken token)
    {
        if (settings.EffectiveProvider == "self")
            throw new AiSettingsException("請先選擇外部 AI 供應商並填寫或保留 API Key。");
        WebsiteAiSettingsService.ValidateApiKey(settings.ApiKey);
        if (settings.Provider == "claude") WebsiteAiSettingsService.ValidateClaudeWorkspaceId(settings.ClaudeWorkspaceId);
        var models = new Dictionary<string, AiModelOption>(StringComparer.Ordinal);
        var cursors = new HashSet<string>(StringComparer.Ordinal);
        string? cursor = null;
        using var http = clients.CreateClient("WebsiteAi");
        try
        {
            for (var page = 0; page < 20; page++)
            {
                using var request = CreateRequest(settings, cursor);
                using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                if (!response.IsSuccessStatusCode)
                {
                    var status = (int)response.StatusCode;
                    return new(status == 429 ? 429 : 502, [], status switch
                    {
                        401 or 403 => "無法取得模型，請檢查 API Key 及模型清單存取權限。",
                        400 when settings.Provider == "claude" => "Claude 拒絕模型查詢，請檢查金鑰與工作區 ID；未綁定工作區的金鑰必須填寫工作區 ID。",
                        429 => "模型查詢過於頻繁或帳戶受限，請稍後再試。",
                        _ => "無法取得供應商模型清單，請檢查設定或稍後再試。"
                    });
                }
                using var document = JsonDocument.Parse(await AiClient.ReadBoundedAsync(response, token));
                cursor = ReadPage(settings.Provider, document.RootElement, models);
                if (models.Count > 2000) throw new JsonException();
                if (string.IsNullOrEmpty(cursor))
                    return new(200, models.Values.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray(), "");
                if (cursor.Length > 2048 || !cursors.Add(cursor)) throw new JsonException();
            }
            return new(502, [], "模型清單超過查詢上限，請使用自訂模型 ID。");
        }
        catch (HttpRequestException) { return new(502, [], "無法連線至模型清單服務，請檢查伺服器網路與 Proxy 設定。"); }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException or KeyNotFoundException)
        { return new(502, [], "供應商模型清單格式不正確或超過大小限制，請稍後再試。"); }
    }

    internal static HttpRequestMessage CreateRequest(WebsiteAiSettings settings, string? cursor)
    {
        var url = settings.Provider switch
        {
            "claude" => "https://api.anthropic.com/v1/models?limit=100" +
                (cursor == null ? "" : "&after_id=" + Uri.EscapeDataString(cursor)),
            "openai" => "https://api.openai.com/v1/models",
            "gemini" => "https://generativelanguage.googleapis.com/v1beta/models?pageSize=100" +
                (cursor == null ? "" : "&pageToken=" + Uri.EscapeDataString(cursor)),
            _ => throw new AiSettingsException("此供應商不提供外部模型清單。")
        };
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        switch (settings.Provider)
        {
            case "claude":
                request.Headers.Add("x-api-key", settings.ApiKey);
                request.Headers.Add("anthropic-version", "2026-10-21");
                if (settings.ClaudeWorkspaceId.Length > 0) request.Headers.Add("anthropic-workspace-id", settings.ClaudeWorkspaceId);
                break;
            case "openai": request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey); break;
            case "gemini": request.Headers.Add("x-goog-api-key", settings.ApiKey); break;
        }
        return request;
    }

    private static string? ReadPage(string provider, JsonElement root, Dictionary<string, AiModelOption> result)
    {
        foreach (var item in root.GetProperty(provider == "gemini" ? "models" : "data").EnumerateArray())
        {
            if (provider == "gemini" && (!item.TryGetProperty("supportedGenerationMethods", out var methods) ||
                !methods.EnumerateArray().Any(x => x.GetString() == "generateContent"))) continue;
            var id = item.GetProperty(provider == "gemini" ? "name" : "id").GetString() ?? "";
            if (provider == "gemini" && id.StartsWith("models/", StringComparison.Ordinal)) id = id[7..];
            // Only expose IDs that the existing generation/save contract can accept.
            if (id.Length is 0 or > 150 || !Regex.IsMatch(id, "\\A[a-zA-Z0-9][a-zA-Z0-9._:-]*\\z")) continue;
            var nameProperty = provider == "claude" ? "display_name" : "displayName";
            var name = item.TryGetProperty(nameProperty, out var label) ? label.GetString() : id;
            if (string.IsNullOrWhiteSpace(name)) name = id;
            result.TryAdd(id, new(id, name.Length > 200 ? name[..200] : name));
        }
        if (provider == "claude" && root.TryGetProperty("has_more", out var more) && more.GetBoolean())
        {
            var cursor = root.GetProperty("last_id").GetString();
            if (string.IsNullOrEmpty(cursor)) throw new JsonException();
            return cursor;
        }
        return provider == "gemini" && root.TryGetProperty("nextPageToken", out var next) ? next.GetString() : null;
    }
}
