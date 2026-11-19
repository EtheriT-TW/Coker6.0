using EtheriT.Coker.Application.Shared.Ai;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// Internal entry point for authorized feature services; credentials and site are resolved on the server.
// This is not an HTTP endpoint and does not grant product/order/customer permissions to its callers.
public sealed class WebsiteAiService(WebsiteAiSettingsService settings, IAiClient client, IConfiguration configuration) : IWebsiteAiService
{
    public async Task<AiChatResult> ChatAsync(AiChatInput input, CancellationToken token = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue<int?>("ParallelAi:TimeoutSeconds") ?? 600, 10, 600)));
        try
        {
            var current = await settings.GetAsync(timeout.Token);
            return await client.ChatAsync(current, input, timeout.Token);
        }
        catch (AiInputException ex) { return new(400, null, ex.Message, 0); }
        catch (AiSettingsException ex) { return new(409, null, ex.Message, 0); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { return new(504, null, "AI 回覆逾時，請稍後重試。"); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return new(503, null, "AI 設定或服務暫時無法使用，請稍後再試。"); }
    }
}
