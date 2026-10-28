# MVC AI 輔助畫布

## 程式分層

`packages/EtheriT.Coker.GrapesJS/src/plugins/aiAssistant/aiAssistantPlugin.js` 負責對話 UI、畫布套用、瀏覽器 HTML 檢查、取消、草稿與版本還原；樣式位於 package 的 `src/styles/aiAssistant.css`，由主入口輸出。

`createCokerGrapesEditor` 註冊插件，預設不啟用。需同時提供 `aiAssistant.enabled: true`、`aiAssistant.draftKey` 與 `adapter.ai.generate` 才顯示按鈕。`createEditorAdapter` 提供可覆寫的 `ai.generate` 接點，package 主入口也匯出 `aiAssistantPlugin`。

```js
const adapter = createEditorAdapter({
    ai: {
        async generate(request, { signal } = {}) {
            // 由宿主呼叫自己的後端；失敗時 throw Error。
            // request: { scope, prompt, html, css, history, preserveContent, styleContext }
            // signal: AbortSignal；取消時應停止 HTTP 請求。
            // 成功回傳 { html, htmlChanged, styles: [{ selector: '#id', properties: { 'font-size': '28px' } }], message }。
        }
    }
});

createCokerGrapesEditor({
    adapter,
    aiAssistant: {
        enabled: true,
        // 同一 editor 切換內容時，callback 必須回傳新內容的識別。
        draftKey: editor => 'account/site/page/container/content-id'
    }
});
```

MVC 的 `_ScriptComponents.cshtml` 只提供目前權限開關、草稿識別 callback 與 HTTP adapter（API 路徑、防偽權杖、站台／來源頁面資訊）。Package 不依賴 MVC 路由、防偽 Header、jQuery 或 Parallel。MVC 後端繼續負責權限與輸出驗證、Parallel 金鑰與呼叫。

## 設定與權限

本機測試沿用既有 `src/EtheriT.Coker.Web.MVC/appsettings.Local.json`，只加入 ParallelAi 區段，保留原設定與既有 Git skip-worktree 狀態。不新增專用設定檔，也不更動 Git 追蹤設定。選用 `EtheriT.Coker.Web.MVC.Local` 啟動設定（ASPNETCORE_ENVIRONMENT=Local）時，由 ASP.NET Core 自動載入；Development 啟動設定不會自動載入 Local 檔。不要輸出或提交此檔案中的金鑰。本機 BaseUrl 設為 `https://localhost:7162/`，對應 Parallel 的 https 啟動設定。Parallel 啟用 HTTPS 重新導向，使用 http://localhost:5032/ 可能收到 307；MVC 刻意不跟隨重新導向，避免將 Key 轉送，因此必須直接設定最終 HTTPS 位址。HTTP 僅允許 Local／Development 的 loopback 位址，其他情況仍須 HTTPS；不略過 TLS 憑證驗證。

具權限的編輯頁面先由 MVC 後端攜帶 `X-Api-Key` 呼叫 `GET /health/ready`（最多 8 秒，預留 Parallel 最多 5 秒檢查 Ollama 及 HTTP 傳輸時間），僅 HTTP 成功且 JSON status 為 Ready 時顯示 AI。成功快取 30 秒、失敗快取 15 秒；同時進入的其他頁面等待並共用同一檢查結果，不會因檢查進行中直接判定為不可用。沒有設定、服務未啟動、Key 無效、模型未就緒、回傳異常或逾時時均不顯示 AI，不影響一般畫布。不發起問答或模型推論，Key 不會傳至瀏覽器。

Parallel 的匿名 `/health` 保留作為 Gateway 存活檢查，MVC 不用它決定是否顯示 AI。`/health/ready` 使用現有 API Key 驗證，缺少／錯誤 Key 回傳 401；驗證通過後呼叫 Ollama `/api/tags` 確認預設模型存在（省略 tag 視為 :latest）。連線、逾時、格式錯誤或模型不存在回傳 503／NotReady。此檢查不代表後續推論保證成功，實際問答錯誤仍會停用 AI。

使用中若發生服務異常、忙碌、連線或登入／權限失效，插件移除 AI 按鈕並停用對話送出，保留已開啟的本機草稿／版本操作。恢復服務後重新整理頁面重新檢查。指令超長或模型輸出不符規格等內容錯誤不會停用整個功能。此次未啟動服務或實際呼叫 readiness／問答 API。

Parallel 接口為 `POST /api/ai/chat`，使用 `X-Api-Key`；MVC 僅傳送 system/user 訊息，使用 Parallel 預設模型。沒有直接相依 Parallel 專案，也沒有新增套件或資料庫欄位。

`src/EtheriT.Coker.Web.MVC/appsettings.json` 的 `ParallelAi` 預設停用，BaseUrl 為預計部署位置。由管理員在伺服器設定實際 Key 並啟用；也可使用設定提供者的 `ParallelAi__ApiKey`、`ParallelAi__Enabled`、`ParallelAi__BaseUrl` 環境變數覆寫。Key 為原始 Key，不能填 Parallel 端儲存的 SHA256 雜湊。不要提交實際 Key。BaseUrl 必須是 HTTPS，禁止跟隨重新導向，避免 Key 被轉送。

新增隱藏的 Execute 權限項目 `AiAssistant`（AI 輔助），預設未啟用。先透過既有站台功能權限設定開啟，再授予角色／使用者 `AiAssistant.Execute`。既有超級管理員仍依現有規則取得已啟用功能。使用者還必須有目前編輯頁面的 View 與 Edit 權限。重新載入頁面後，GrapesJS 工具列會出現 AI 按鈕。

AI 輔助是獨立的根層級權限項目，不依附內容管理；只使用頁面管理的站台也可單獨啟用。保留 AiAssistant／AiAssistant.Execute 名稱，原授權資料不需遷移。若按鈕未出現，在瀏覽器 Console 輸入 `window.CokerCanvasAiStatus`：canEditPage 為頁面編輯權限、canExecuteAi 為 AI 執行權限、serviceReady 為 readiness 結果（未通過權限時為 null，表示未呼叫）、pluginAvailable 為目前載入的 bundle 是否匯出 AI 插件。診斷不包含金鑰或畫布內容。

`serviceReason` 提供安全的失敗分類：DisabledInConfiguration／MissingOrInvalidApiKey／InvalidBaseUrl／HttpNotAllowedInCurrentEnvironment 表示請求發送前的設定檢查未通過；ApiKeyRejected 表示 HTTP 401；ReadinessRedirectNotAllowed 表示回傳重新導向；OllamaOrModelNotReady 表示 HTTP 503；ConnectionOrTlsFailed、ReadinessTimeout、InvalidReadinessResponse 分別為連線、逾時、回應格式問題。`serviceHttpStatus` 是收到的 HTTP 狀態（未收到時為 null），`serviceResultCached` 表示是否共用快取。PreviousChatFailed 表示先前問答失敗後的短期停用。MVC 的 Visual Studio Output／應用程式日誌也會記錄分類及執行環境，不記錄 Key 或上游回應本文。檢查由 MVC 伺服器發送，不會出現在瀏覽器 Network 的 Fetch/XHR。

MVC `POST /api/CanvasAi/Chat` 驗證登入、防偽權杖、目前站台、來源頁面及 AI 權限；不接受瀏覽器指定模型或 system 訊息。API Key 僅在後端加入上游請求。錯誤不回傳上游回應或金鑰。

## 使用方式與範圍

- 選取區塊：先選 GrapesJS 元件，要求放大文字、縮小文字、三欄改四欄等修改。
- 修改方式預設為「只調整樣式」：保持原文，整個元件修改只合併 CSS 屬性；局部文字格式需要增加 span，但不改寫文字。內容／結構變更請選「調整內容與排版」。全新 layout 自動使用內容模式。傳給 AI 的規則要求繁體中文（zh-TW），不可擅自轉換原文字體或改寫文案。純樣式模式由程式保留原文，不依賴模型重寫 HTML。
- 局部文字樣式：AI 使用 `textStyles: [{ text: '德瑞克', properties: { color: '#e53935', 'font-style': 'italic', 'font-size': 'calc(1em + 2px)' } }]`，而不是對整個容器改樣式。MVC 在目前選取 HTML 內精確匹配原文，可跨相鄰文字節點；為匹配文字加上唯一 ID 的 span，回傳 htmlChanged=true 與 CSS 規則。半級比周圍文字大 2px。前後端均核對原文字內容不變，仍禁止 inline-style／script；找不到原文、輸出超限或不符合安全限制則不套用。相同字串多次出現時會全部套用。套用後若 HTML／CSS 都沒有差異，提示未產生變更，不顯示成功或新增紀錄。
- 明確的基本局部格式指令，例如「德瑞克三個字體大一級，斜體，改為紅色」或「德瑞克三個字改為紅字，斜體，跟其他文字大半級」，由 MVC 解析目標原文與基本格式組合：字數須符合、原文須存在選取區塊，支援紅／藍／黑色、粗／斜體、大／小一級或半級。整條指令必須全部符合才使用此路徑；其他語意仍交由 AI。兩者共用局部 span 與 CSS 安全檢查，沒有放寬 script／inline-style 限制。定位失敗、屬性不支援、HTML／CSS 驗證失敗會分別提示，不再使用同一則泛用錯誤。
- 請求包含目標實際 computed font-size／font-weight／font-style／font-family 等參考值。純樣式模式明確要求「文字大一級／小一級」時，依原字級加減 4px，僅套用 font-size，保留粗體、斜體與其他屬性。
- 「目前選取區塊＋只調整樣式」的單一基本指令，例如「文字放大一級」「文字縮小一級」「字級大一級」，由 MVC 直接根據 StyleContext 與原 HTML 的實際文字節點產生 CSS 差異，不要求模型重新生成 CSS；仍需通過登入、站台、AI 權限及 readiness。只改 font-size、不改 HTML、不產生 inline-style，並沿用相同 CSS 範圍與白名單驗證。其他複合指令仍走 AI。
- 整個畫布：以目前完整 HTML 與 CSS 作為參考，回傳完整 HTML 片段。
- 全新 layout：確認後取代畫布內容，Public 的外層頁首／頁尾仍由原系統管理。
- 圖片可沿用原本資產管理器，或在對話中提供既有 HTTPS／站內根目錄圖片 URL；此版本不提供圖片生成或 AI 上傳。
- 輸出要求 Bootstrap 5 響應式結構、語意化標籤、標題階層與圖片 alt，字型沿用 Public 的 Google fonts。這是生成約束，SEO 品質與實際版面仍需人工檢查。
- AI 回覆分開傳回 HTML 與結構化 CSS 差異（styles），每條規則僅能指定目標 HTML 內單一 #id 與白名單屬性。使用 GrapesJS Css.setRule 的 addStyles 合併，不覆寫整份 CSS。新的 inline-style、script、style/link、事件屬性、URL／expression 等 CSS 注入均會被拒絕。Public 仍由原儲存流程取得 HTML 與獨立 CSS。表單、iframe、SVG、外部依賴、data-* 與動態 Coker 元件不開放由 AI 產生，因此含這些元件的大範圍 HTML 改寫可能被拒絕，請使用純樣式模式或選取單純內容區塊。
- 不替換既有全域 CSS。等待回覆期間可繼續編輯，但內容改動後 AI 回覆會被拒絕套用。取消及失敗均不套用回覆。
- Parallel 每則訊息上限 32000 字元；MVC 將指令、HTML、CSS、對話合成一則訊息，超限時提示縮小範圍。服務忙碌時不自動重送。

## 本機草稿與版本

本機版本將畫布專案資料（包含 HTML 元件及 CSS）存入 localStorage，保留最近 20 筆：首次初始版本、每次成功且有差異的 AI 修改，以及明確點擊保存草稿。開啟／關閉、離開頁面、送出請求前與失敗不再自動建立版本。未保存草稿的手動編輯不會另建紀錄；需要保留時請先點保存草稿。舊版已存在的紀錄會保留，不擅自清除。

版本下拉選單直接切換，另提供上一版／下一版快速比較。AI 完成、保存草稿或切換既有版本後，若 HTML／CSS 沒有其他變更，可以直接來回切換，不需確認；若有手動修改而尚未保存為版本，才提示切換將取代修改。取消則保留畫布與原版本選擇。切換不新增紀錄，重複內容也不重複建立紀錄。版本依帳號、站台、頁面、畫布容器與內容 ID 區隔；沒有內容 ID 時以初始 HTML 識別。

本機紀錄不跨瀏覽器或電腦同步，不是伺服器持久化的草稿／發布版本；清除瀏覽器資料會遺失。尚未取得 ID 的新內容應避免同一頁同時編輯多筆初始 HTML 相同的草稿。本機儲存失敗會提示，當次編輯器仍保留記憶體版本。對話只保留當次編輯器最近訊息，清除對話不會清除畫布版本。

正式儲存與發布沿用目前畫布的原有操作與權限，不會由 AI 端點直接寫入資料庫。跨裝置草稿與持久化版本可於後續另行規劃資料模型與權限，此次未建立或執行 Migration。

## 手動驗收

1. 選用 Local 啟動設定，驗證停用、未設定／錯誤 Key、readiness 失敗或逾時時不顯示 AI，一般畫布不受影響；Ready 且具權限才顯示。特別驗證 Gateway 正常但 Ollama 停止或指定模型不存在時也不顯示。恢復後待失敗快取到期再重新整理。
2. 啟用站台功能、授權角色：具頁面編輯及 AI 權限者看到 AI；僅檢視或未授權者看不到，直接呼叫 API 亦為 403。未登入、缺防偽權杖、切換站台後舊頁面發送亦須被拒絕。
3. Parallel 部署後驗證基本生成、字級調整、選取區塊三欄改四欄、完整新 layout、提供 URL 換圖，以及桌面／手機預覽。確認瀏覽器 Network 僅呼叫 MVC，沒有 X-Api-Key。
4. 清空測試用本機紀錄後，首次開啟應一筆、成功 AI 修改後兩筆；失敗、切換、關閉重開不增加。用下拉選單及上一版／下一版來回比較，不應提示；手動改文字或樣式後切換，應提示，取消保留原內容、確認才切換。保存草稿後切換不再提示。另驗證不同內容與站台的版本隔離、邊界按鈕停用、請求中不可切換、清除紀錄及 localStorage 不可用的提示。
5. 驗證 AI 忙碌、逾時、錯誤 Key、非 JSON／惡意 HTML／超大回覆、取消，以及等待時修改內容或切換編輯項目。失敗不得覆寫畫布。
6. 使用原有儲存功能後在 Public 確認 HTML、CSS、圖片、字型、SEO 結構及手機版排版。
7. 選取原本繁體、粗體及斜體的文字，使用純樣式模式要求大一級，確認文字逐字不變、字級增加 4px、粗體及斜體保持。再要求小一級確認可還原字級；儲存後 HTML 不含新的 style 屬性或 script，CSS 另行保存。此次修改包含後端 DTO／回覆契約與 package，兩端需一起更新。
8. 選取含「德瑞克」的句子，要求該三字紅色、斜體、比其餘文字大半級；確認只有匹配文字新增 span／獨立 CSS，其他文字及粗體保留，Public 儲存後一致。再測試文字跨粗體／斜體標記、重複出現、找不到原文、沒有實際差異時不誤報成功，以及前後版本切換。

建議在 Visual Studio 編譯，或於 repository 根目錄手動執行：

```powershell
dotnet build src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj
```

原因：驗證新增 C# 及 Razor 整合。會寫入 bin/obj，可能自動還原套件；本專案有 BuildBundlerMinifier，相依 MSBuild 設定也可能產生前端產物。耗時依環境與還原狀態而定，可能超過 30 秒。本次未執行。

前端功能已移入 GrapesJS package，MVC 不再載入獨立的 CanvasAi.js／CanvasAi.css。請在 repository 根目錄手動執行：

```powershell
npm --prefix packages/EtheriT.Coker.GrapesJS run build:mvc
```

原因：需將 package 的新插件與樣式產生到 MVC 使用的模組。已確認此 script 執行 `vite build` 及 `scripts/copy-to-mvc.mjs`，會寫入 package 的 dist，並更新 MVC 的 wwwroot/modules/grapesjs 等複製流程所指定的 generated assets；可能超過 30 秒，實際耗時依環境而定。此命令不包含套件安裝；如相依套件尚未就緒，請先自行處理。此次未執行 build:mvc 或任何 bundle 流程，既有 generated assets 未修改，故重新產生之前瀏覽器載入的舊模組尚不包含 AI 插件。

這次調整後請特別驗證：package 預設未啟用時不出現 AI；MVC 有權限時出現按鈕；自訂 adapter 的 generate 能接收到 AbortSignal；對話、修改、草稿、版本還原與原有儲存仍可使用。
