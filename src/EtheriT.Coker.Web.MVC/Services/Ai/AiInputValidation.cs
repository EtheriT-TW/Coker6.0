using EtheriT.Coker.Application.Shared.Ai;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

internal static class AiInputValidation
{
    internal static void Validate(AiChatInput input)
    {
        if (input == null || input.System == null || input.System.Length > 32000 ||
            input.Messages == null || input.Messages.Count is < 1 or > 32 ||
            input.MaxOutputTokens is < 1 or > 16000 || !Enum.IsDefined(input.ResponseFormat))
            throw new AiInputException("AI 請求內容或輸出限制不正確。");
        var textLength = input.System.Length;
        var imageCount = 0;
        var imageBytes = 0;
        foreach (var message in input.Messages)
        {
            if (message == null || message.Role is not ("user" or "assistant") || message.Text == null ||
                message.Text.Length > 64000 || (string.IsNullOrWhiteSpace(message.Text) && (message.Images?.Count ?? 0) == 0) ||
                (message.Role == "assistant" && (message.Images?.Count ?? 0) > 0))
                throw new AiInputException("AI 訊息角色或內容不正確。");
            textLength += message.Text.Length;
            if (textLength > 128000) throw new AiInputException("AI 對話內容超過大小限制。");
            foreach (var image in message.Images ?? [])
            {
                if (++imageCount > 4 || image == null || image.MediaType is not ("image/png" or "image/jpeg" or "image/webp") ||
                    string.IsNullOrEmpty(image.Base64Data) || image.Base64Data.Length > 5592408)
                    throw new AiInputException("AI 圖片格式或數量不正確，僅支援 PNG、JPEG、WebP，最多四張。");
                byte[] bytes;
                try { bytes = Convert.FromBase64String(image.Base64Data); }
                catch (FormatException) { throw new AiInputException("AI 圖片資料不是有效的 Base64。"); }
                imageBytes += bytes.Length;
                if (bytes.Length > 4 * 1024 * 1024 || imageBytes > 8 * 1024 * 1024 || !MatchesType(image.MediaType, bytes))
                    throw new AiInputException("AI 圖片內容與格式不符或超過大小限制：每張 4 MiB、合計 8 MiB。");
            }
        }
        if (input.Messages[^1].Role != "user") throw new AiInputException("AI 對話最後一則必須是使用者訊息。");
    }

    private static bool MatchesType(string type, ReadOnlySpan<byte> bytes) => type switch
    {
        "image/png" => bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        "image/jpeg" => bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255,
        "image/webp" => bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };
}
