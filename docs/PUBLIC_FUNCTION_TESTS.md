# 前台功能檢測

## 執行方式

SystemDiagnostics/Public 現在以 Microsoft.Playwright 1.64.1 啟動獨立的 Edge/Chrome 無介面工作階段，
實際載入前台 HTML、JavaScript、圖片及 iframe；不使用使用者的瀏覽器設定檔、登入 Cookie 或 AI。
僅使用 HTTP 200 的舊版資源盤點執行器已移除，不能再用它代替圖片顯示或嵌入驗證。

目標由目前網站、已發布資料及 BuildDiagnosticsScopeAsync 的版本／功能／父層設定產生。
最多抽樣二十個主要頁面，文章／商品／證照各抽樣三筆，選單來源最多二百筆。
主要頁面抽樣數不限制連結目的地：啟用的公開頁面、搜尋／會員入口及文章／商品／證照明細路由獨立建立白名單。
關閉的會員／購物功能不因此加入；超出來源上限或範圍的項目列未檢測，不能當作通過。

## 自動判斷

- 頁面：瀏覽器導航與 DOM 載入成功才記錄文件載入通過，HTTP 錯誤或連線失敗列異常。
- 圖片：捲動並觸發原生 lazy loading，呼叫圖片 decode，核對 complete、naturalWidth、naturalHeight。
  AVIF、SVG/data URL 等由實際瀏覽器解碼，不因格式或跨網域一律列人工確認。
  來源 404/410/5xx 或確定解碼失敗列異常；404 替代圖即使能解碼仍是異常。
  尚未完成載入、無來源或受檢測器限制者列未檢測；401/403/429 列無法確認。
- 連結：以 GET 驗證目的地並追蹤公開轉址；正常跨網域轉址、LINE、地圖短網址與唯讀搜尋參數不再一律人工確認。
  站內電話／郵件被誤寫成 /Org/tel:...、/Org/mailto:... 時直接列格式錯誤，不對該路徑發請求。
  非目前功能的路由、登出、刪除、確認、退訂、交易等操作連結略過；不執行帳號修改。
- 嵌入：捲動至可見 iframe，讀取實際子框 DOM（含跨網域），檢查可用內容與供應商錯誤畫面。
  地圖 key/地圖錯誤 UI、Video unavailable、Facebook 內容不可用、拒絕連線等已知錯誤列異常；
  有內容且沒有偵測到已知錯誤時，通過的範圍是「嵌入文件已載入」，不代表每個操作或整段影片播放皆已驗證。
  最多三個可見影片來源會建立測試 iframe；依實際主機／副檔名區分 YouTube、Facebook、MP4/WebM/OGV 等，不以元件名稱或 iframe title 判定供應商。
  只有真正的 YouTube URL 才驗證影片 ID；其他來源保留原網址。MP4 等待瀏覽器媒體資料與影像尺寸，並檢查 MediaError，不以 YouTube ID 判斷。
  隱藏、aria-hidden 或樣板中的來源不加入。結果保留頁首／主要內容／頁尾位置、來源次序與按鈕文字／縮圖替代文字；Facebook href 僅保留可信 Facebook 原始路徑與數字影片 v 參數，移除其他 query 機密。
  不因此宣稱原按鈕點擊流程或整段影片播放通過。
  登入、驗證碼、Cookie 同意要求或供應商明確封鎖才列無法確認。
- CSS 背景圖、需登入的流程、表單送出、購物車寫入、訂單、金流、寄信與完整影片播放尚不在本執行範圍。

預設只列異常與無法確認；通過及未檢測可切換全部項目。
未檢測與無法確認都不列通過。錯誤訊息只呈現易懂的結果，資源 URL 隱藏 query/fragment。
瀏覽器未安裝、驅動未就緒、忙碌或檢測中止時列未檢測並停止後續頁面，不能當成網站故障或通過。

## 網路、權限與上限

每次 POST 驗證平台管理者、防偽、目前站台及重新計算的範圍指紋；只接受頁面索引，不能指定任意 URL。
前後台共用網站／使用者閘門，瀏覽器另外有全域單一工作限制，避免大量瀏覽器同時啟動。
瀏覽器採新建非持久工作階段、啟用 sandbox、不忽略 TLS/CSP、不連接既有使用者設定檔。

所有網路路由都由 DNS-pinned HttpClient 取得實際內容，再將狀態、轉址、原始 CSP 與資料交給瀏覽器。
每次 socket 連線與重新導向均拒絕本機、私網、link-local 與危險 transition 位址。
瀏覽器設定無可用的本機 proxy，阻擋未攔截的直接流量；停用 Service Worker、WebSocket、EventSource、WebRTC、WebTransport。
不帶 Cookie、Authorization 或後台憑證；只轉送公開瀏覽器 User-Agent、Origin、Referer、內容協商及必要的影片客戶端標頭。
一般僅允許 GET/HEAD；唯一 POST 例外是固定 YouTube 主機的 /youtubei/v1/player 與 /youtubei/v1/next，
用於公開播放器資料讀取，body 上限六十四 KB。其他 POST 不送出，不開放一般第三方 GraphQL 或本站業務 POST。
HTTP 請求及前台 JS 仍可能產生快取與存取紀錄；不得宣稱資料庫或第三方服務完全沒有讀取副作用。

每頁最多一百張可見圖片、一百個不重複連結、十個 iframe；最多三百個瀏覽器請求、單資源八 MB及總計六十四 MB。
頁面工作上限一百二十秒；瀏覽器啟動二十秒、導航三十五秒、單個網路讀取十五秒，連結請求最多四個並行。
超限資源不列通過。取消會關閉工作階段並中止請求，不儲存未完成工作為本次結果。
公開網路限制保留，localhost／內網前台不支援，不以關閉 TLS 或私網保護解決。

## 環境準備與使用者驗證

MVC 新增 Microsoft.Playwright 1.64.1 PackageReference，Codex 未執行 restore、build、瀏覽器安裝或實際檢測。
預設使用已安裝的 Microsoft Edge（channel msedge）；可由伺服器設定 Diagnostics:BrowserChannel 改為 chrome 或 chromium。
選 msedge/chrome 時使用系統瀏覽器，不自動下載瀏覽器。IIS 應用程式身分須有啟動瀏覽器與使用暫存目錄的權限。
正式部署前必須在同一應用程式身分下驗證；不建議連接管理員日常瀏覽器。

由使用者在 Visual Studio 還原、編譯並啟動 MVC；若需命令列還原，完整指令：
    dotnet restore src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj

此命令會下載新的 Playwright 套件及相依項目，寫入套件快取與 obj 還原產物；檔案量較大，耗時依網路與快取而定。
Codex 不執行，也不因此取得後續 build/test/bundle 的執行授權。

Public.cshtml 仍引用 Public.min.js，維護 wwwroot/js/SystemDiagnostics/Public.js。
使用者在 MVC 專案目錄手動執行 npm run create-bundles；它會做授權檢查、yarn 與 gulp buildDev，
可能下載套件、更新 lock file、產生資產並啟動監看，耗時依環境而定。Codex 不產生 min.js。
新的 browser-v3 範圍指紋不還原舊的影片誤分類結果，需重新檢測。

## 紀錄與回歸

結果獨立保存在 App_Data/PublicDiagnostics/{網站ID}-{帳號ID}.json，只保留最後狀態並逐頁更新，
不混入 BackofficeDiagnostics；網站、帳號、功能／路由／執行器版本不同時不還原。
部署應保留此目錄並允許程式讀寫。

SystemDiagnostics.Public.parser-and-history 保留原案例 ID並綁定 sandbox/public-diagnostics。
使用隔離瀏覽器證據、假 HTTP handler 與獨立暫存目錄測試實際分類函式、轉址、搜尋參數、電話格式、
內網拒絕、YouTube 讀取端點白名單、資料及紀錄隔離；不啟動真實瀏覽器，不連客戶資料庫。
隔離分類通過不能說成實際前台已驗證。實際瀏覽器載入、iframe、lazy loading、權限及部署環境由
SystemDiagnostics.Public.live-sampling 記錄完整人工端對端驗收步驟，未綁定者不列自動通過數。

規則涵蓋已知供應商的成功與錯誤證據，無法保證偵測所有未來供應商 UI 變更；
新增錯誤樣式時同步更新 classifier 與可重現案例，不能只調整顯示標籤或把無證據項目改成通過。
