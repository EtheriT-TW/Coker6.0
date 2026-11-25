namespace EtheriT.Coker.Web.MVC.Services.Ai;

// Request-scoped observer only. No shared job store, provider headers or credentials.
public sealed record CanvasAiTraceEvent(string Stage, string Message, DateTimeOffset Time, long ElapsedMs, string? Detail);

internal static class CanvasAiTrace
{
    // Display projection only; the original model request is never shortened here.
    internal static string DescribeRequest(string phase, string system, string context, string? previousSystem = null, string? previousContext = null)
    {
        var text = new System.Text.StringBuilder($"階段：{phase}\n此為請求內容摘要；實際送出內容不受影響。\n");
        text.AppendLine(system == previousSystem ? "系統規則：沿用前一輪，未重複顯示。" : "系統規則節錄：\n" + Clip(system, 3000));
        using var current = System.Text.Json.JsonDocument.Parse(context);
        using var previous = System.Text.Json.JsonDocument.Parse(previousContext ?? "{}");
        var fields = current.RootElement.EnumerateObject().ToArray();
        var unchanged = new List<string>();
        var priority = new[] { "Prompt", "SelectedId", "ValidationFeedback", "ValidationFailures", "RejectedProposal", "ReviewProposal", "ResourceKnowledge" };
        foreach (var field in fields.OrderBy(field => { var index = Array.IndexOf(priority, field.Name); return index < 0 ? priority.Length : index; }))
        {
            if (previous.RootElement.TryGetProperty(field.Name, out var old) && old.GetRawText() == field.Value.GetRawText())
            { unchanged.Add(field.Name); continue; }
            if (text.Length > 16000) { text.AppendLine("其餘欄位超過摘要額度，未列出。"); break; }
            var value = field.Value.ValueKind == System.Text.Json.JsonValueKind.String ? field.Value.GetString() ?? "" :
                System.Text.Json.JsonSerializer.Serialize(field.Value, DisplayOptions);
            text.AppendLine($"\n{field.Name}：\n{Clip(value, field.Name == "ResourceKnowledge" ? 3000 : 1600)}");
        }
        if (unchanged.Count > 0) text.AppendLine("\n沿用前一輪且未重複顯示的欄位：" + string.Join(", ", unchanged));
        return text.ToString();
    }

    private static string Clip(string value, int limit) => value.Length <= limit ? value : value[..limit] + $"\n（節錄；原長度 {value.Length} 字元）";
    private static readonly System.Text.Json.JsonSerializerOptions DisplayOptions = new() {
        WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}

