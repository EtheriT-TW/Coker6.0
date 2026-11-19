using System.Net.Http.Headers;
using System.Text.Json;
using EtheriT.Coker.Application.Shared.Ai;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public interface IAiClient
{
    Task<AiChatResult> ChatAsync(WebsiteAiSettings settings, AiChatInput input, CancellationToken token);
}

// Fixed destinations only. Neither the browser nor StoreSet can supply a URL or authentication header.
public sealed class AiClient(IHttpClientFactory clients, ILogger<AiClient> logger) : IAiClient
{
    public async Task<AiChatResult> ChatAsync(WebsiteAiSettings settings, AiChatInput input, CancellationToken token)
    {
        if (!settings.Enabled) return new(403, null, "本站尚未開放 AI 輔助。");
        if (settings.EffectiveProvider == "self")
        {
            AiInputValidation.Validate(input);
            return new(501, null, "系統預設 AI 尚未提供此類型的共用呼叫，請設定外部 AI 服務或由管理員擴充服務能力。", 0);
        }
        WebsiteAiSettingsService.ValidateApiKey(settings.ApiKey);
        using var request = CreateRequest(settings, input);
        try
        {
            using var http = clients.CreateClient("WebsiteAi");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            int? calls = 1;
            if (!response.IsSuccessStatusCode)
            {
                var code = (int)response.StatusCode;
                return new(code == 429 ? 429 : code == 422 ? 422 : 502, null, code switch
                {
                    401 or 403 => "AI 金鑰無效或沒有使用此模型的權限，請管理員檢查設定。",
                    404 => "AI 模型不存在或帳戶無法使用，請管理員檢查模型設定。",
                    429 => "AI 額度不足或請求過於頻繁，請稍後重試或檢查帳戶額度。",
                    422 => "AI 服務無法處理此請求，請檢查輸入內容與模型能力。",
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
        catch (HttpRequestException ex)
        {
            // Log classifications only: exception messages may contain sensitive provider data.
            logger.LogWarning("AI connection failed. Provider={Provider}, Error={Error}, InnerType={InnerType}",
                settings.EffectiveProvider, ex.HttpRequestError, ex.InnerException?.GetType().Name);
            return new(502, null, ex.HttpRequestError switch
            {
                HttpRequestError.NameResolutionError => "無法解析 AI 服務的網域名稱，請檢查伺服器的 DNS 與網路設定。",
                HttpRequestError.SecureConnectionError => "無法建立與 AI 服務的安全連線，請檢查伺服器時間、憑證信任與 Proxy 設定。",
                HttpRequestError.ConnectionError => "無法建立與 AI 服務的網路連線，請檢查伺服器對外連線、防火牆與 Proxy 設定。",
                HttpRequestError.ProxyTunnelError => "無法透過 Proxy 連線至 AI 服務，請檢查伺服器的 Proxy 設定。",
                HttpRequestError.HttpProtocolError or HttpRequestError.InvalidResponse or HttpRequestError.ResponseEnded
                    => "AI 服務連線中斷或回傳無效的 HTTP 回應，請檢查伺服器網路與 Proxy 設定。",
                _ => "無法連線至 AI 服務，請管理員查看伺服器的 AI 連線錯誤分類。"
            });
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or IOException or IndexOutOfRangeException)
        { return new(502, null, "AI 回覆格式不正確或超過大小限制。"); }
    }

    internal HttpRequestMessage CreateRequest(WebsiteAiSettings settings, AiChatInput input)
    {
        AiInputValidation.Validate(input);
        var system = input.ResponseFormat == AiResponseFormat.JsonObject ? input.System + "\nReturn only a JSON object." : input.System;
        var provider = settings.EffectiveProvider;
        HttpRequestMessage request;
        object payload;
        switch (provider)
        {
            case "openai":
                request = new(HttpMethod.Post, "https://api.openai.com/v1/responses");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                var openAiPayload = new Dictionary<string, object>
                {
                    ["model"] = settings.Model, ["instructions"] = system,
                    ["input"] = input.Messages.Select(m => new { role = m.Role, content = OpenAiParts(m) }).ToArray(),
                    ["store"] = false, ["max_output_tokens"] = input.MaxOutputTokens
                };
                if (input.ResponseFormat == AiResponseFormat.JsonObject)
                    openAiPayload["text"] = new { format = new { type = "json_object" } };
                payload = openAiPayload;
                break;
            case "gemini":
                request = new(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/" + Uri.EscapeDataString(settings.Model) + ":generateContent");
                request.Headers.Add("x-goog-api-key", settings.ApiKey);
                var generationConfig = new Dictionary<string, object> { ["maxOutputTokens"] = input.MaxOutputTokens };
                if (input.ResponseFormat == AiResponseFormat.JsonObject) generationConfig["responseMimeType"] = "application/json";
                payload = new { systemInstruction = new { parts = new[] { new { text = system } } },
                    contents = input.Messages.Select(m => new { role = m.Role == "assistant" ? "model" : "user", parts = GeminiParts(m) }).ToArray(),
                    generationConfig };
                break;
            case "claude":
                WebsiteAiSettingsService.ValidateClaudeWorkspaceId(settings.ClaudeWorkspaceId);
                request = new(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
                if (settings.ClaudeWorkspaceId.Length > 0)
                    request.Headers.Add("anthropic-workspace-id", settings.ClaudeWorkspaceId);
                request.Headers.Add("x-api-key", settings.ApiKey);
                request.Headers.Add("anthropic-version", "2026-10-21");
                payload = new { model = settings.Model, max_tokens = input.MaxOutputTokens,
                    system,
                    messages = input.Messages.Select(m => new { role = m.Role, content = ClaudeParts(m) }).ToArray() };
                break;
            default: throw new AiSettingsException("不支援的 AI 供應商。");
        }
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var limit = input.Messages.Any(m => (m.Images?.Count ?? 0) > 0) ? 12 * 1024 * 1024 : 250000;
        if (bytes.Length > limit) { request.Dispose(); throw new AiInputException("AI 請求內容超過大小限制。"); }
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new("application/json");
        return request;
    }

    private static object[] OpenAiParts(AiMessage message)
    {
        var parts = new List<object>();
        if (!string.IsNullOrWhiteSpace(message.Text)) parts.Add(new { type = "input_text", text = message.Text });
        foreach (var image in message.Images ?? [])
            parts.Add(new { type = "input_image", image_url = $"data:{image.MediaType};base64,{image.Base64Data}" });
        return parts.ToArray();
    }

    private static object[] GeminiParts(AiMessage message)
    {
        var parts = new List<object>();
        if (!string.IsNullOrWhiteSpace(message.Text)) parts.Add(new { text = message.Text });
        foreach (var image in message.Images ?? [])
            parts.Add(new { inlineData = new { mimeType = image.MediaType, data = image.Base64Data } });
        return parts.ToArray();
    }

    private static object[] ClaudeParts(AiMessage message)
    {
        var parts = new List<object>();
        foreach (var image in message.Images ?? [])
            parts.Add(new { type = "image", source = new { type = "base64", media_type = image.MediaType, data = image.Base64Data } });
        if (!string.IsNullOrWhiteSpace(message.Text)) parts.Add(new { type = "text", text = message.Text });
        return parts.ToArray();
    }

    internal static string ExtractContent(string provider, JsonElement root)
    {
        switch (provider)
        {
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

    internal static async Task<byte[]> ReadBoundedAsync(HttpResponseMessage response, CancellationToken token)
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
