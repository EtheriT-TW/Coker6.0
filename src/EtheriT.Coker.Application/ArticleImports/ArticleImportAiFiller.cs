using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Shared.Ai;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.ArticleImports;

// 送給 AI 的一列：Data 是勾選欄位的原始值（Drive 連結已換成 image:N 代號），Images 是下載成功的圖片。
public sealed record ArticleImportAiImage(string Token, string Column, string FileName, string Path);

public sealed record ArticleImportAiRow(
    int RowNumber,
    IReadOnlyDictionary<string, string> Data,
    IReadOnlyList<ArticleImportAiImage> Images);

// Fields 有值＝成功；Error 有值＝這列失敗。
public sealed record ArticleImportAiResult(int RowNumber, Dictionary<string, List<string>>? Fields, string? Error);

// AI 設定不能用（沒設定、關閉、系統預設不支援），整個匯入直接失敗，不要把每一列都標成失敗。
public sealed class ArticleImportAiUnavailableException(string message) : Exception(message);

// 每次送 5 列給 AI；整批失敗（逾時、格式錯、漏列）就把沒拿到結果的列逐筆重送，仍失敗才標成失敗。
public sealed class ArticleImportAiFiller(IWebsiteAiService ai)
{
    public const int ChunkSize = 5;
    public const int MaxDescriptionLength = 300;
    private const int MaxOutputTokens = 12000;
    private static readonly Regex ImageToken = new(@"image:(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IReadOnlyList<ArticleImportAiResult>> FillAsync(
        ArticleTemplate template,
        IReadOnlyList<ArticleImportAiRow> rows,
        CancellationToken token)
    {
        var results = new Dictionary<int, ArticleImportAiResult>();
        var (parsed, batchError) = await RequestAsync(template, rows, token);
        foreach (var item in parsed) results[item.RowNumber] = item;

        // 整批失敗或漏掉的列逐筆重試；只有一列時不用重試同一個請求。
        foreach (var row in rows.Where(r => !results.ContainsKey(r.RowNumber)))
        {
            if (rows.Count == 1)
            {
                results[row.RowNumber] = new ArticleImportAiResult(row.RowNumber, null, batchError ?? "AI 沒有回傳這一列。");
                continue;
            }
            var (single, error) = await RequestAsync(template, [row], token);
            results[row.RowNumber] = single.FirstOrDefault()
                ?? new ArticleImportAiResult(row.RowNumber, null, error ?? "AI 沒有回傳這一列。");
        }

        return rows.Select(r => results[r.RowNumber]).ToList();
    }

    internal async Task<string> SummarizeIntroductionAsync(string introduction)
    {
        var input = new AiChatInput(
            $"依提供的公司簡介原文撰寫一段簡單、完整的文章描述，最多 {MaxDescriptionLength} 字元（含標點與空白）。只使用原文資訊，維持原文語言，不新增事實。只回傳 JSON 物件，description 為描述字串。",
            [new AiMessage("user", JsonConvert.SerializeObject(new { introduction }))])
        {
            ResponseFormat = AiResponseFormat.JsonObject,
            MaxOutputTokens = 1000
        };
        var response = await ai.ChatAsync(input);
        if (response.Status != 200 || string.IsNullOrWhiteSpace(response.Content))
            throw new InvalidOperationException("AI 無法產生文章描述，請確認 AI 設定後重新匯入。");

        try
        {
            var description = JObject.Parse(StripCodeFence(response.Content))["description"];
            if (description?.Type == JTokenType.String)
            {
                var text = description.ToString().ReplaceLineEndings("\n").Trim();
                if (text.Length is > 0 and <= MaxDescriptionLength) return text;
            }
        }
        catch (JsonException)
        {
            // 無效的回覆和不符合長度的描述都交由匯入流程顯示失敗原因。
        }

        throw new InvalidOperationException($"AI 回傳的文章描述無效，請重新匯入。描述須為 1 至 {MaxDescriptionLength} 字元。");
    }

    private async Task<(IReadOnlyList<ArticleImportAiResult> Results, string? Error)> RequestAsync(
        ArticleTemplate template,
        IReadOnlyList<ArticleImportAiRow> rows,
        CancellationToken token)
    {
        var input = new AiChatInput(BuildSystemPrompt(), [new AiMessage("user", BuildUserMessage(template, rows))])
        {
            ResponseFormat = AiResponseFormat.JsonObject,
            MaxOutputTokens = MaxOutputTokens
        };
        var response = await ai.ChatAsync(input, token);
        if (response.Status is 401 or 403 or 409 or 501)
            throw new ArticleImportAiUnavailableException(response.Message);
        if (response.Status != 200 || string.IsNullOrWhiteSpace(response.Content))
            return ([], string.IsNullOrWhiteSpace(response.Message) ? "AI 沒有回覆。" : response.Message);

        try
        {
            return (ParseResponse(response.Content, template, rows), null);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return ([], "AI 回覆的格式不正確。");
        }
    }

    public static string BuildSystemPrompt() => """
        你是資料整理助手，負責把會員資料表（Excel）每一列的內容，整理成網站文章樣板的欄位。
        規則：
        1. 只能使用該列提供的資料，不可推測、補充或編造；找不到對應資料的欄位就不要輸出。
        2. 欄位值一律是字串陣列。pagetitle、string、textarea、link、image 只放一個字串；
           list 每個項目一個字串（把頓號、逗號、斜線、換行分隔的項目拆開，去掉編號與多餘符號）；gallery 可放多個圖片代號，不超過 maxCount。
        3. image、gallery 只能填該列 images 清單裡的代號（例如 "image:2"），依欄名與檔名判斷圖片用途（例如官網橫幅、Logo），不可填網址或其他文字。
           樣板的 image 欄位只要有圖片就一定要選一張；只有一張圖片時放在樣板的 image，gallery 留空；樣板的 image 用過的圖片不可再放進 gallery。
           article.cover（文章封面）獨立選圖。有封面或主圖就優先選，沒有時選一張能代表該列文章的圖片；可與橫幅或 Logo 共用同一張。沒有圖片時留空。
        4. required 為 true 的欄位必填；pagetitle 通常是公司或單位的中文名稱。
        5. 文字照原文，不翻譯、不改寫，只去除多餘空白；textarea 保留原本的換行。英文欄位只在資料有英文時才填。
        6. link 欄位填原文的網址、電子郵件或電話。同一格同時有網址與電子郵件時（例如「www.abc.com / abc@mail.com」），
           自行判斷哪個是網址、哪個是電子郵件，分別填入對應的欄位（依欄位 label 判斷），不要整串放在同一個欄位。
        7. 同一段介紹文字要分給「短介紹」與「完整簡介」兩類欄位時（例如主視覺短介紹與公司簡介）：
           若原文用「公司簡介」和「經營理念」小標題分段，主視覺短介紹完整照抄經營理念前的公司簡介內文，包含所有句子和段落；經營理念小標題及內文放進完整簡介。
           原文沒有以上小標題時，一律先填短介紹：只要有介紹文字，短介紹就一定要有值；文字只有一到兩句時只填短介紹，完整簡介留空；
           文字較長時再依下列方式拆分。短介紹只放最能說明公司主要業務的一到兩句（原文照抄，約 100 字內，通常是開頭說明「主要生產／提供什麼」的句子）；
           其餘內容（含經營理念等段落）放進完整簡介，保留原本的段落與換行，不要重複短介紹已放的句子；
           「公司簡介」「經營理念」這類小標題：與欄位名稱相同的開頭標題去掉，其他小標題保留成獨立一行。
        8. 只輸出 JSON：{"rows":[{"row":列號,"fields":{"欄位key":["值"]}}]}。輸入的每一列都要輸出一筆，row 照輸入的列號。
        """;

    public static string BuildUserMessage(ArticleTemplate template, IReadOnlyList<ArticleImportAiRow> rows)
        => JsonConvert.SerializeObject(new
        {
            fields = ArticleImportFieldRules.GetFields(template).Select(f => new
            {
                key = f.Key,
                label = f.Label,
                type = f.Type,
                required = f.Required || f.Key == template.PageTitleKey,
                maxCount = f.MaxCount,
                section = f.Section
            }),
            rows = rows.Select(r => new
            {
                row = r.RowNumber,
                data = r.Data,
                images = r.Images.Select(i => new { id = i.Token, column = i.Column, fileName = i.FileName })
            })
        });

    // 解析 AI 回覆並驗證：只收輸入的列號、樣板有的欄位；圖片代號換成這列下載檔案的路徑。
    public static IReadOnlyList<ArticleImportAiResult> ParseResponse(
        string content,
        ArticleTemplate template,
        IReadOnlyList<ArticleImportAiRow> rows)
    {
        var root = JObject.Parse(StripCodeFence(content));
        if (root["rows"] is not JArray items)
            throw new InvalidOperationException("AI 回覆缺少 rows。");

        var byNumber = rows.ToDictionary(r => r.RowNumber);
        var results = new Dictionary<int, ArticleImportAiResult>();
        foreach (var item in items.OfType<JObject>())
        {
            if (!int.TryParse(item["row"]?.ToString(), out var number)
                || !byNumber.TryGetValue(number, out var row)
                || results.ContainsKey(number))
                continue;

            var raw = ReadFields(item["fields"] as JObject);
            ArticleImportFieldRules.ApplyIntroductionSections(template, raw, row.Data);
            var images = row.Images.ToDictionary(i => i.Token, i => i.Path, StringComparer.OrdinalIgnoreCase);
            var (fields, error) = ArticleImportFieldRules.Normalize(template, raw, value =>
            {
                var match = ImageToken.Match(value);
                return match.Success && images.TryGetValue($"image:{match.Groups[1].Value}", out var path) ? path : null;
            }, row.Images.Select(i => new ArticleImportImageChoice(i.Path, i.Column, i.FileName)).ToList());
            results[number] = error == null
                ? new ArticleImportAiResult(number, fields, null)
                : new ArticleImportAiResult(number, null, $"AI {error}");
        }
        return results.Values.ToList();
    }

    private static Dictionary<string, List<string>> ReadFields(JObject? fields)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        if (fields == null) return result;
        foreach (var property in fields.Properties())
        {
            result[property.Name] = property.Value switch
            {
                JArray array => array.Where(v => v.Type is not (JTokenType.Object or JTokenType.Array or JTokenType.Null))
                    .Select(v => v.ToString()).ToList(),
                JValue value when value.Type != JTokenType.Null => [value.ToString()],
                _ => []
            };
        }
        return result;
    }

    // 有些模型即使指定 JSON 仍會包一層 ```json。
    private static string StripCodeFence(string content)
    {
        var text = content.Trim();
        if (!text.StartsWith("```", StringComparison.Ordinal)) return text;
        var start = text.IndexOf('\n');
        var end = text.LastIndexOf("```", StringComparison.Ordinal);
        return start > 0 && end > start ? text[(start + 1)..end].Trim() : text;
    }
}
