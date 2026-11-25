# AI 共用呼叫與功能入口

功能 API 各自處理登入、功能權限、防偽、站台／資料隔離、費用或副作用確認；功能服務取得授權資料並建立提示詞，最後呼叫 `IWebsiteAiService`。不要將商品、客服或購物診斷需求送到 `/api/CanvasAi/Chat`。

```text
功能 API → 功能服務 → IWebsiteAiService → IAiClient → Claude／OpenAI／Gemini
畫布 API → CanvasAiService → CanvasAiClient → IAiClient（外部供應商）
                                         → 畫布 Gateway 契約 v2（系統預設）
```

## 後端呼叫

畫布 Gateway 原始碼在 `D:/Ai整合/Parallel`，須與 MVC 同步編譯及重啟。v2 在原 `canvas-edit` workflow 增加 `phase=plan/ui-review`、獨立 `replyLocale` 及有型別限制的來源分析／class 變更／設定交接欄位；`locale` 保持網站 HTML 語系。ui-review 只校正訊息並移除操作，不執行原修改。MVC 以 `X-Canvas-Contract-Version: 2` 拒絕舊服務的成功／422回應，避免不相容計畫被誤當一般失敗。此更新沒有開放通用 self workflow、任意命令或放寬 MVC 的授權及內容安全驗證。Parallel 的契約文件與獨立 fake-provider 執行器分別為 `CANVAS_CONTRACT.md`、`scripts/CanvasContractChecks`；尚未執行，不代表實頁通過。

在已授權的功能服務注入 `IWebsiteAiService`（namespace `EtheriT.Coker.Application.Shared.Ai`）。介面與訊息型別位於 Application.Shared，Application Service 不必引用 MVC；MVC 啟動時註冊供應商與設定實作，其他 host 若要使用亦需註冊實作：

```csharp
var result = await ai.ChatAsync(new AiChatInput(
    "你是商品資料整理助手。只根據提供的資料回答，不推測售價與庫存。",
    [new AiMessage("user", "請整理以下商品描述：...")]), cancellationToken);
if (result.Status != 200)
{
    // 交由功能 API 回傳適合該功能的錯誤，不直接保存不完整結果。
}
var text = result.Content;
```

服務依目前登入／背景作業站台讀取已保存設定與解密金鑰；呼叫端不傳站台 ID、API Key、供應商網址、HTML 或畫布範圍。背景作業必須先以既有 BackgroundOperationContext 設定經授權的站台與使用者，不能讓 AI 自行選站台。這是程式內部服務，不是公開的通用提示詞 API，也不替功能 API 授予商品、訂單或客戶資料權限。

`AiChatInput.Messages` 可包含 user／assistant 的文字歷史，最後一則須為 user。預設 `ResponseFormat.Text`，不套用 JSON、畫布 schema、語系、HTML/CSS 規則；提示詞與網站語系由功能服務自行決定。需要 JSON 時明確指定：

```csharp
var request = new AiChatInput("依提供內容產生商品草稿，不新增未知資訊。",
    [new AiMessage("user", "請從照片辨識品名、特徵，無法確認的欄位回傳 null。",
        [new AiImage("image/jpeg", validatedImageBase64)])])
{
    ResponseFormat = AiResponseFormat.JsonObject,
    MaxOutputTokens = 2000
};
var result = await ai.ChatAsync(request, cancellationToken);
```

功能服務要自行解析、驗證 JSON 與業務欄位，格式提示不是 schema／商品規則驗證。AI 回覆只作為草稿；不得以模型輸出直接執行 SQL、修改訂單、金流或其他未授權副作用。

## 輸入與能力

- 圖片只接受 inline Base64 的 PNG／JPEG／WebP；檢查 MIME、檔頭與大小，不抓取任意 URL、不接受 SVG 或檔案路徑。功能入口仍需進行上傳與圖片完整性驗證，不可把檔頭檢查當成完整解碼驗證。
- 最多 32 則訊息、系統提示 32000 字元、每則 64000 字元、總文字 128000 字元；最多 4 張圖片、每張 4 MiB、總計 8 MiB；輸出上限 1～16000 tokens。純文字序列化請求仍受 250000 bytes 限制，含圖片為 12 MiB；回應上限 256 KiB。超限明確拒絕，不截斷對話或丟棄圖片。
- 輸入格式支援不代表所選模型具備視覺或 JSON 能力，需選用相容模型並完成實際驗收。回覆只接受完整文字結果；工具呼叫、串流、檢索／知識庫、音訊與影片尚未實作。
- 通用文字／圖片目前支援三家外部供應商。系統預設 Gateway 的 Repository 外服務只有已知 `canvas-edit` 契約，透過 CanvasAiClient 保留；其他通用任務使用系統預設回傳 501、零模型呼叫，不自動套畫布提示詞或偷偷改用其他供應商。待 Gateway 提供並驗證通用契約後再接入。
- `IAiClient` 是較低層的供應商轉換介面，需明確傳入後端設定；一般功能優先使用 `IWebsiteAiService` 以避免遺漏站台最新設定、開關與逾時處理。不記錄提示詞、圖片、金鑰或原始供應商錯誤本文。

## 既有畫布相容與驗證

`/api/CanvasAi/Chat`、前端請求、CanvasAiService 的提示詞、語系、選取範圍、HTML/CSS 安全與結果驗證維持；外部供應商由 CanvasAiClient 指定 JSON，系統預設保留 workflow／locale／originalHtml、已知錯誤映射及 X-Canvas-Model-Calls。設定頁 Test 使用共用 JSON 請求但仍是固定測試，不依賴任何畫布輸入。

此次不新增資料庫或前端資產變更，不需 Migration 或 bundle。尚未執行編譯、隔離案例或真實供應商驗證。建議使用者手動：

```powershell
dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
```

編譯可能還原套件並更新 bin/obj，時間依環境。接著在測試環境驗收原畫布三種範圍、連線測試、各供應商純文字／圖片、JSON、關閉 AI、權限與跨站拒絕。圖片產品草稿、購物診斷與客服目前只有可共用的底層能力，尚未建立其功能 API 或 UI。
## 畫布本機模型上下文容量（2026-11-24）

Parallel的Ollama設定新增`CanvasContextLength`（32768）及`CanvasMaxOutputTokens`（8192）。畫布plan／ui-review由伺服器設定明確傳入`options.num_ctx`／`num_predict`，一般聊天保留既有預設；外部HTTP不能指定這兩個內部額度。上下文允許8192–131072，輸出至少1024且不超過上下文一半，仍須符合實際模型支援範圍及硬體能力。增加容量可能提高記憶體需求與推論時間。

本輪實際`GET /api/ps`顯示Qwen上下文僅4096，原程式未送上下文選項；這是觀察到的配置缺口。新紀錄只含設定容量、輸入／輸出token數與是否碰輸出上限，不能將修改程式碼視為實際模型或瀏覽器驗證通過。重新編譯／重啟Parallel後，再送畫布指令並以`api/ps`及伺服器metadata確認實際容量。隔離斷言位於Parallel的`scripts/CanvasContractChecks`，尚未執行。沒有資料庫或前端資產變更。
