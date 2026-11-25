using System.Net.Http.Headers;
using System.Text.Json;
using EtheriT.Coker.Application.Shared.Ai;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// The existing Gateway protocol is canvas-specific; never send other features through it.
public sealed record CanvasAiInput(string System, string Context, string Locale, string OriginalHtml)
{
    public string Phase { get; init; } = "plan";
    public string? ReplyLocale { get; init; }
}
public sealed record CanvasAiChatResult(int Status, string? Content, string Message, int? ModelCalls = null)
{
    public string? FailureReason { get; init; }
}

public sealed class CanvasAiClient(IAiClient client, IHttpClientFactory clients, CanvasAiAvailability availability)
{
    public async Task<CanvasAiChatResult> ChatAsync(WebsiteAiSettings settings, CanvasAiInput input, CancellationToken token)
    {
        if (!settings.Enabled) return new(403, null, "本站尚未開放 AI 輔助。");
        if (settings.EffectiveProvider != "self")
        {
            var result = await client.ChatAsync(settings, new(input.System, [new("user", input.Context)])
                { ResponseFormat = AiResponseFormat.JsonObject }, token);
            return new(result.Status, result.Content, result.Status == 422
                ? "AI 修改提案未通過驗證，未修改畫布。請重新描述需求。" : result.Message, result.ModelCalls);
        }
        using var request = CreateGatewayRequest(input);
        try
        {
            using var http = clients.CreateClient("WebsiteAi");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            int? calls = null;
            if (response.Headers.TryGetValues("X-Canvas-Model-Calls", out var values) &&
                int.TryParse(values.FirstOrDefault(), out var count) && count is >= 0 and <= 1) calls = count;
            if ((response.IsSuccessStatusCode || (int)response.StatusCode == 422) &&
                (!response.Headers.TryGetValues("X-Canvas-Contract-Version", out var versions) || !versions.Contains("2")))
                return new(503, null, "AI 畫布服務的契約尚未更新，請管理員同步編譯並重啟 Parallel／Gateway；目前未修改畫布。", calls);
            if ((response.IsSuccessStatusCode || (int)response.StatusCode == 422) &&
                (!response.Headers.TryGetValues("X-Canvas-Generation-Constraints", out var constraints) || !constraints.Contains("2")))
                return new(503, null, "AI 畫布服務尚未支援元件生成限制，請同步編譯並重啟 Parallel／Gateway；目前未修改畫布。", calls);
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                var failure = status == 422 ? await CanvasAiService.ReadCanvasFailureDetails(response, token) : null;
                return new(status is 429 or 422 ? status : 502, null, status switch
                {
                    422 => failure!.Message,
                    401 or 403 => "AI 金鑰無效或沒有使用此模型的權限，請管理員檢查設定。",
                    429 => "AI 額度不足或請求過於頻繁，請稍後重試或檢查帳戶額度。",
                    _ => "AI 服務呼叫失敗，請管理員檢查服務與模型設定。"
                }, calls) { FailureReason = failure?.Reason };
            }
            using var json = JsonDocument.Parse(await AiClient.ReadBoundedAsync(response, token));
            var content = json.RootElement.GetProperty("content").GetString();
            return string.IsNullOrWhiteSpace(content) ? new(422, null, "AI 沒有回傳完整可用的文字結果。", calls)
                : new(200, content.Trim(), "", calls);
        }
        catch (HttpRequestException) { return new(502, null, "無法連線至 AI 服務。"); }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException or KeyNotFoundException)
        { return new(502, null, "AI 回覆格式不正確或超過大小限制。"); }
    }

    internal HttpRequestMessage CreateGatewayRequest(CanvasAiInput input)
    {
        if (!availability.TryGetConnection(out var baseUri, out var key))
            throw new AiSettingsException("系統預設 AI 服務尚未啟用或連線設定未完成。");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new { workflow = "canvas-edit", phase = input.Phase, locale = input.Locale, replyLocale = input.ReplyLocale, originalHtml = input.OriginalHtml,
            messages = new[] { new { role = "system", content = input.System }, new { role = "user", content = input.Context } } });
        if (bytes.Length > 250000) throw new AiInputException("AI 請求內容超過大小限制。");
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "api/ai/chat"));
        request.Headers.Add("X-Api-Key", key);
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return request;
    }
}
