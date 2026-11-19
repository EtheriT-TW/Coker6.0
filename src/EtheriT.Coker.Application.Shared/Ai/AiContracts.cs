namespace EtheriT.Coker.Application.Shared.Ai;

public enum AiResponseFormat { Text, JsonObject }
public sealed record AiImage(string MediaType, string Base64Data);
public sealed record AiMessage(string Role, string Text, IReadOnlyList<AiImage>? Images = null);
public sealed record AiChatInput(string System, IReadOnlyList<AiMessage> Messages)
{
    public AiResponseFormat ResponseFormat { get; init; } = AiResponseFormat.Text;
    public int MaxOutputTokens { get; init; } = 16000;
}
public sealed record AiChatResult(int Status, string? Content, string Message, int? ModelCalls = null);
public sealed class AiInputException(string message) : Exception(message) { }

// Called by authorized feature services. Credentials and website context are resolved by the implementation.
public interface IWebsiteAiService
{
    Task<AiChatResult> ChatAsync(AiChatInput input, CancellationToken token = default);
}
