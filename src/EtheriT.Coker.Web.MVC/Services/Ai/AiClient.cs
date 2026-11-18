using System.Net.Http.Headers;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed record AiChatInput(string System, string Context, string Locale, string OriginalHtml);
public sealed record AiChatResult(int Status, string? Content, string Message, int? ModelCalls = null);

public interface IAiClient
{
    Task<AiChatResult> ChatAsync(WebsiteAiSettings settings, AiChatInput input, CancellationToken token);
}

// Fixed destinations only. Neither the browser nor StoreSet can supply a URL or authentication header.
public sealed class AiClient(IHttpClientFactory clients, CanvasAiAvailability selfAvailability) : IAiClient
{
    public async Task<AiChatResult> ChatAsync(WebsiteAiSettings settings, AiChatInput input, CancellationToken token)
    {
        if (!settings.Enabled) return new(403, null, "本站尚未開放 AI 輔助。");
        using var request = CreateRequest(settings, input);
        try
        {
            using var http = clients.CreateClient("WebsiteAi");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            int? calls = settings.EffectiveProvider == "self" ? null : 1;
            if (settings.EffectiveProvider == "self" && response.Headers.TryGetValues("X-Canvas-Model-Calls", out var values) &&
                int.TryParse(values.FirstOrDefault(), out var count) && count is >= 0 and <= 1) calls = count;
            if (!response.IsSuccessStatusCode)
            {
                var code = (int)response.StatusCode;
                if (settings.EffectiveProvider == "self" && code == 422)
                    return new(422, null, await CanvasAiService.ReadCanvasFailure(response, token), calls);
                return new(code == 429 ? 429 : code == 422 ? 422 : 502, null, code switch
                {
                    401 or 403 => "AI 金鑰無效或沒有使用此模型的權限，請管理員檢查設定。",
                    404 => "AI 模型不存在或帳戶無法使用，請管理員檢查模型設定。",
                    429 => "AI 額度不足或請求過於頻繁，請稍後重試或檢查帳戶額度。",
                    422 => "AI 修改提案未通過驗證，未修改畫布。請重新描述需求。",
                    _ => "AI 服務呼叫失敗，請管理員檢查服務與模型設定。"
                }, calls);
            }
            var body = await ReadBoundedAsync(response, token);
            using var document = JsonDocument.Parse(body);
            var content = ExtractContent(settings.EffectiveProvider, document.RootElement);
            return string.IsNullOrWhiteSpace(content)
                ? new(422, null, "AI 沒有回傳完整可用的文字結果。", calls)
                : new(200, content.Trim(), "", calls);
        }
        catch (HttpRequestException) { return new(502, null, "無法連線至 AI 服務。"); }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or IOException or IndexOutOfRangeException)
        { return new(502, null, "AI 回覆格式不正確或超過大小限制。"); }
    }

    internal HttpRequestMessage CreateRequest(WebsiteAiSettings settings, AiChatInput input)
    {
        var provider = settings.EffectiveProvider;
        HttpRequestMessage request;
        object payload;
        switch (provider)
        {
            case "self":
                if (!selfAvailability.TryGetConnection(out var baseUri, out var key))
                    throw new AiSettingsException("系統預設 AI 服務尚未啟用或連線設定未完成。");
                request = new(HttpMethod.Post, new Uri(baseUri, "api/ai/chat"));
                request.Headers.Add("X-Api-Key", key);
                payload = new { workflow = "canvas-edit", locale = input.Locale, originalHtml = input.OriginalHtml,
                    messages = new[] { new { role = "system", content = input.System }, new { role = "user", content = input.Context } } };
                break;
            case "openai":
                request = new(HttpMethod.Post, "https://api.openai.com/v1/responses");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                payload = new { model = settings.Model, instructions = input.System, input = input.Context,
                    store = false, max_output_tokens = 16000, text = new { format = new { type = "json_object" } } };
                break;
            case "gemini":
                request = new(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/" + Uri.EscapeDataString(settings.Model) + ":generateContent");
                request.Headers.Add("x-goog-api-key", settings.ApiKey);
                payload = new { systemInstruction = new { parts = new[] { new { text = input.System } } },
                    contents = new[] { new { role = "user", parts = new[] { new { text = input.Context } } } },
                    generationConfig = new { responseMimeType = "application/json", maxOutputTokens = 16000 } };
                break;
            case "claude":
                request = new(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
                request.Headers.Add("x-api-key", settings.ApiKey);
                request.Headers.Add("anthropic-version", "2026-10-21");
                payload = new { model = settings.Model, max_tokens = 16000, system = input.System,
                    messages = new[] { new { role = "user", content = input.Context } } };
                break;
            default: throw new AiSettingsException("不支援的 AI 供應商。");
        }
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        if (bytes.Length > 250000) { request.Dispose(); throw new AiSettingsException("AI 請求內容超過大小限制。"); }
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new("application/json");
        return request;
    }

    internal static string ExtractContent(string provider, JsonElement root)
    {
        switch (provider)
        {
            case "self": return root.GetProperty("content").GetString() ?? "";
            case "openai":
                if (root.GetProperty("status").GetString() != "completed") return "";
                return string.Concat(root.GetProperty("output").EnumerateArray()
                    .Where(x => x.GetProperty("type").GetString() == "message")
                    .SelectMany(x => x.GetProperty("content").EnumerateArray())
                    .Where(x => x.GetProperty("type").GetString() == "output_text")
                    .Select(x => x.GetProperty("text").GetString()));
            case "gemini":
                var candidate = root.GetProperty("candidates")[0];
                if (candidate.GetProperty("finishReason").GetString() != "STOP") return "";
                return string.Concat(candidate.GetProperty("content").GetProperty("parts").EnumerateArray()
                    .Where(x => !(x.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True) && x.TryGetProperty("text", out _))
                    .Select(x => x.GetProperty("text").GetString()));
            case "claude":
                if (root.GetProperty("stop_reason").GetString() != "end_turn") return "";
                return string.Concat(root.GetProperty("content").EnumerateArray()
                    .Where(x => x.GetProperty("type").GetString() == "text").Select(x => x.GetProperty("text").GetString()));
            default: throw new JsonException();
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(HttpResponseMessage response, CancellationToken token)
    {
        const int limit = 262144;
        if (response.Content.Headers.ContentLength > limit) throw new IOException();
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int length;
        while ((length = await stream.ReadAsync(chunk, token)) > 0)
        {
            if (buffer.Length + length > limit) throw new IOException();
            buffer.Write(chunk, 0, length);
        }
        return buffer.ToArray();
    }
}

public sealed class WebsiteAiAvailability(WebsiteAiSettingsService settings, CanvasAiAvailability selfAvailability)
{
    public async Task<CanvasAiReadiness> CheckAsync(CancellationToken token)
    {
        try
        {
            var current = await settings.GetAsync(token);
            return await CheckAsync(current, token);
        }
        catch (AiSettingsException) { return new(false, "WebsiteSettingsInvalid"); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return new(false, "WebsiteSettingsUnavailable"); }
    }

    public Task<CanvasAiReadiness> CheckAsync(WebsiteAiSettings current, CancellationToken token)
    {
        if (!current.Enabled) return Task.FromResult(new CanvasAiReadiness(false, "DisabledForWebsite"));
        // No paid inference on page load. External credentials are verified by explicit test/use.
        return current.EffectiveProvider == "self" ? selfAvailability.CheckAsync(token)
            : Task.FromResult(new CanvasAiReadiness(true, "ExternalConfiguredNotVerified"));
    }
}
