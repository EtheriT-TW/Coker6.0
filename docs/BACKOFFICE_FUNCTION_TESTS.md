# 後台功能檢測流程與維護規則

## 執行範圍

MVC「系統檢測 → 後台功能測試」的開始按鈕會真正呼叫已登錄的執行器。
它不執行 dotnet test、build、restore、bundle、Migration，也不透過 HTTP 遍歷所有 GET Action。
這些開發／產物操作仍依根目錄 AGENTS.md 由使用者控制。

目前登錄 220 個自動檢測案例：160 個列表查詢／條件／排序／分頁檢查、16 個設定或統計檢查、
44 個隔離操作案例。實際適用數量依目前站台版本與啟用功能篩選，不固定為 220。
原本的 160 個較廣泛人工驗收規格保留在同一份內部清單；尚未綁定 executor 的規格不列入自動檢測畫面及通過數。
這不是宣稱原本的完整瀏覽器與外部服務驗收已全部自動化。

## 三種實際執行方式

1. 頁面基本檢查：確認 MVC 入口已註冊及 Razor View 可解析。不渲染或執行業務 Action。
2. 唯讀整合檢查：呼叫白名單中已追蹤的實際 Application Service，使用目前站台資料。
   列表限制最多兩筆，驗證 DTO 必要欄位及序列化、互斥條件的空列表、識別欄位篩選、遞增／遞減排序及 Skip / Take 分頁。
   沒有樣本或不足兩筆時，相依檢查標為未執行，不能當作通過。
   設定、元件、網站地圖、流量及營收使用各自的回應與資料條件檢查。
3. 隔離業務檢查：每個案例建立獨立 DI 容器、EF Core InMemory 資料庫、測試網站與帳號，以及獨立暫存目錄。
   呼叫實際業務服務新增、修改、刪除，再清除追蹤並重新查詢資料核對結果。
   已涵蓋商品、庫存、會員價格、批次庫存、跨站保護、庫存警示、規格、證照、文章、目錄、廣告、標籤、
   分群、會員角色、箱型、跑馬燈、版頭／頁尾／全站設定、表單批次處理／回覆、信件建構、檔案回收／還原／刪除與路徑界線。

InMemory 只用來驗證隔離資料中的業務邏輯。它不等同 SQL Server，不驗證真實資料庫的外鍵／唯一限制、交易、
定序、SQL 翻譯與所有關聯式行為；真實 SQL 查詢由唯讀整合檢查另外覆蓋。
目前也未提供完整瀏覽器點擊、所有匯入匯出、登入工作階段及金流／物流外部契約測試。
不得將頁面基本檢查或記憶體資料檢查描述成整個功能或整站驗收通過。

## 資料安全

- 唯讀查詢期間以 AsyncLocal 範圍啟用 EF SaveChanges 與 SQL Command 攔截器，阻擋資料寫入。
  允許分頁 FETCH NEXT，但仍拒絕會推進序號的 NEXT VALUE FOR。
  SQL 僅允許受檢查的 SELECT，拒絕非查詢命令與修改資料／結構的關鍵字；服務吞掉資料庫例外時仍判為失敗。
  不得因 HTTP GET、方法名稱含 Get 或交易 rollback 就認定安全。
- 訂單正常列表原有逾期訂單維護保持預設啟用；診斷專用 GetAllListForDiagnostics 省略逾期訂單維護，避免查詢順便改單。
- 隔離容器的 CokerDbContext 與型別化 DbContextOptions 都明確改用新的 InMemoryDatabaseRoot；不得使用客戶連線字串。
  共用資料模型僅掃描 FullAuditedEntity 所在領域組件，不遍歷已載入的第三方組件，以免可選套件載入失敗阻斷新模型建立。
  組態不帶入客戶設定，快取與 HttpContextAccessor 也分開；不得使用真實資料當可修改的 fixture。
- 網路 HttpClient 與背景工作在隔離容器中阻擋。已登錄的隔離案例不使用靜態 Hangfire API 或未審查的直接網路呼叫。
- 郵件經實際信封、收件人及本文建構後，在伺服器內部 DiagnosticsSandboxScope 中攔截並存於記憶體；
  這個分支在建立 SMTP client 前返回，不連線、不寄出。外部請求的 header、cookie 或參數不能開啟此範圍。
  實際郵件服務是否可投遞不包含在此項通過結果。
- 測試檔案僅在系統暫存目錄 CokerDiagnostics / 隨機 GUID 下產生，執行後清理。
  清理前比對完整目錄邊界，不能刪除其他路徑。路徑保護檢查僅解析危險路徑，不寫出界檔案。

## 按鈕、進度與結果

已移除重複的「檢測紀錄」選單、History Action／View 與前台頁連結；後台、前台圖片連結、前台功能各自在測試頁查詢背景進度與最後結果。
畫面分別顯示前後台結果。前台已改用 Playwright 的獨立瀏覽器工作階段，驗證圖片實際載入／解碼、第三方框的內容與錯誤及公開連結目的地；不再一律要求人工確認。
前台圖片連結結果獨立存於 App_Data/PublicDiagnostics，功能結果另存於 App_Data/PublicFunctionDiagnostics，不合併到後台案例數；遵守網站／帳號、功能範圍隔離與內部例外不外洩規則。
執行範圍、網路限制與第三方判斷詳見 docs/PUBLIC_FUNCTION_TESTS.md；不可把隔離 handler 測試當成實際前台測試通過。
只提供目前網站／帳號的最後儲存結果，不是每次執行的歷史列表；逐頁複測可能混合不同時間結果。
各自測試頁預設列出未通過項目，可切換全部目前適用項目並複測；全通過、未執行與無紀錄均須明確區分。
範圍或規格改變時可讀取最後更新時間，但不展示舊範圍的通過數或明細，提示重新檢測。
前後台測試頁保留平台管理者驗證、固定網站／帳號、功能範圍篩選與錯誤處理；不將 Errors 字典傳入 View。
隨頁面移除刪除 SystemDiagnostics.History 與其人工案例；最新結果讀取及網站／帳號隔離仍由 sandbox/diagnostics-history 驗證，保留既有案例 ID 及執行器。
移除入口的人工回歸併入 result-controls：檢查選單、前台頁連結與舊 History 網址不再提供紀錄頁；此回歸尚未執行。

開始按鈕以具防偽驗證的 POST 提交整份前台或後台檢測，回傳 202，不在 HTTP 請求內執行案例。
目標頁面／網址由後端依目前站台、帳號與功能範圍產生；前端只傳站台、範圍指紋與是否複測，不接受任意 URL、JavaScript 或瀏覽器操作。
工作使用現有 Hangfire 儲存與 diagnostics 專用佇列，另設一個執行緒，不占用一般業務佇列；以分散式鎖限制此檢測方法同時執行一份。
同一網站／帳號同時僅接受一份前台或後台工作；工作狀態、進度及停止要求存於 Hangfire 的專屬 hash，不新增資料表或 Migration。
每頁建立新的 DI scope，以 BackgroundOperationContext 固定網站及帳號，重新檢查平台管理者與功能範圍；權限撤銷或範圍變更中止。
頁面每五秒查詢狀態，進度變更時重新載入目前適用的結果；離開、關閉或重新整理頁面不取消工作，返回可恢復顯示。
停止透過具防偽驗證的 POST 提出要求，執行器每兩秒檢查；排隊工作可直接取消。執行中的停止須等待目前不接受 CancellationToken 的舊服務結束，已完成結果保留。
每頁取消期限三分鐘，整份工作取消期限四十五分鐘；不接受取消訊號的舊服務仍須等待其返回。前台既有瀏覽器、網路與數量上限仍適用。逾時、中斷與查詢失敗不得顯示為完成或通過。
Hangfire 自動重試設為零；網站關閉造成的 Hangfire 重新排隊可從已保存頁數繼續，執行時仍重驗範圍。失敗、刪除或遺失的工作標為中斷，不會永遠鎖住開始按鈕。
Hangfire 在 MVC 程序內執行，網站停止或 IIS 回收期間不會持續執行；仍需部署環境確保程序可持續運作。
重啟後恢復需要保留 Hangfire 儲存及結果 App_Data；兩者都不是每次執行歷史清單。同機 Hangfire 不提供與 MVC 的權限隔離，Playwright 仍在 MVC 發布內容。

預設只列未通過項目，#failed / #all 與 history.replaceState 切換不重載、不重跑檢測。
尚無完成或還原結果時只顯示「開始檢測」，隱藏複測及兩個結果篩選按鈕；即使網址帶 #all 也不顯示未執行清單。
前後台在狀態尚未確認、提交中、排隊中、執行中與等待停止時，隱藏開始／重新檢測與複測按鈕；有目前工作時顯示停止按鈕。
第一頁完成後可切換結果篩選；確認工作結束後恢復開始／重新檢測，有結果才顯示複測，沒有失敗時複測停用。
result-controls 已補上述按鈕顯示及離開返回的人工回歸情境，尚未執行瀏覽器驗收。
`SystemDiagnostics.Backoffice.result-controls` 為內部人工瀏覽器回歸規格，涵蓋無紀錄、停止、部分結果、紀錄還原及全部通過，不計入自動檢測數。
重跑前清除本次目標頁面的舊狀態，避免把前次通過誤當成本次結果。
每頁完成後將最後結果以原子檔案取代方式保存在 ContentRoot / App_Data / BackofficeDiagnostics，按網站、使用者隔離；不位於 wwwroot。
每組網站／使用者只保留一份最新紀錄，不因記憶體快取清除、24 小時經過或服務重啟而失效。
功能範圍指紋包含頁面、來源及案例執行器、步驟、預期結果與功能依賴；規格或範圍不符時不載入，下一次完成檢測會取代舊範圍紀錄。
重新編譯不再以組件識別碼直接使紀錄失效。歷史結果僅供複測參考；業務原始碼修改後仍需手動重新檢測。
重新整理會載入目前仍適用的歷史結果並標示最後更新時間；它不是本次重新執行的結果。
不寫入客戶資料庫。部署須保留 App_Data 並允許應用程式身分讀寫；多台主機需使用同一持久化儲存，否則各自保存結果。
讀寫失敗會記錄內部日誌並提示使用者，不將未存檔結果宣稱為已儲存。舊版只存在記憶體或瀏覽器頁面的結果無法追溯轉存。
`SystemDiagnostics.Backoffice.history-storage` 綁定 `sandbox/diagnostics-history`，在獨立暫存目錄驗證新實例還原、網站／帳號／範圍隔離及複測更新保留其他頁；不遞迴執行檢測 API。
`SystemDiagnostics.Backoffice.model-and-query-guards` 綁定 `sandbox/diagnostics-guards`，檢查記憶體模型、軟刪除與實際命令攔截器的分頁／寫入判斷；命令不連線也不送至資料庫。新增斷言未執行前不得宣稱通過。
「複測未通過項目」僅重跑有失敗的頁面，包含該頁所有案例，維持相依查詢與 fixture 的完整性；其他頁保留原結果。
提交前即清除本次目標頁的持久化舊結果；停止或失敗時，未完成頁保持尚未檢測，其他非重測頁保留原結果。

內部人工回歸規格（未綁定執行器，不計入自動檢測通過數）：
- `diagnostics-history-refresh`：完成檢測後重新整理，同網站同帳號應顯示原結果與時間；切換帳號、站台或不相同功能範圍不得載入該份紀錄。
- `diagnostics-history-retry`：有失敗結果時點選複測，僅重跑含失敗的功能頁，重新計算結果，其他頁結果保留；沒有失敗時按鈕停用。
- `diagnostics-history-expiry`：服務重啟後原紀錄仍可載入；範圍／規格不同時顯示尚未檢測；部署目錄不可寫時須提示未儲存，不得把空紀錄當成通過。

使用者只看易懂的項目與結果。原始碼路徑、案例 ID、Controller / Action、入口盤點、排除原因及規格保留內部。
例外僅記錄伺服器日誌，不將原始錯誤、資料內容或憑證回傳給使用者。
為避免執行環境未提供日誌輸出，檢測結果檔另外以 Errors 字典保存案例例外（含內層例外與堆疊）或失敗斷言。
此為 App_Data 中的內部維護資料，不加入 ViewModel、使用者畫面或下載 API；維護時只取需要的案例，分享前移除機敏資料。
提交複測時清除目標頁舊證據與結果，完成後以本次結果取代；其他頁證據保留。取消前未完成的頁面保持尚未檢測。
已在 history-storage 執行器加入錯誤還原、通過後清除舊錯誤及保留其他頁錯誤的斷言；登錄不表示已執行通過。

## 前台拆分回歸

新增 SystemDiagnostics.Functions，綁定 sandbox/public-functions，驗證操作證據、兩種類結果隔離與工作種類還原；既有 public-diagnostics 增加 HEAD／GET 回退。
新增功能頁人工驗收：實際輪播／版型／分類操作、嵌入、無觸發、未就緒、超限、離開返回與兩種類結果／停止隔離。均尚未執行，不宣稱通過。

## 背景工作回歸

新增 sandbox/diagnostics-background，使用記憶體 IStorageConnection 替身驗證實際工作狀態儲存器：
網站／帳號隔離、舊工作不可修改新工作、進度跨實例還原、停止要求保存、終態不可重啟、Hangfire 失敗／遺失狀態與重測範圍。
另使用獨立暫存目錄驗證前台重測只移除目標頁。未連接 SQL、啟動 Hangfire Server 或瀏覽器，不代表背景端到端驗證通過。
background-lifecycle 保留人工驗收：離開返回、跨分頁重複提交、停止、權限撤銷、功能變更及網站重啟。
新增與修改的案例均尚未執行。

## 新增、修改功能時必做

本次回歸細項：未綁標籤目錄刪除的成功回應、先建立頁尾再以實際 ID 保存 HTML/CSS、
上傳路徑的正反斜線父層跳脫與相鄰前綴目錄、訪客熱圖 1–7 星期編碼及 168 個唯一時段。
分別由既有 directory-crud、template-footer、file-path、traffic-heatmap 執行器驗證；未執行不得視為通過。

1. 閱讀 AGENTS.md，從本次變更的 View、Partial、JavaScript 追蹤直接相關 API、DTO 與 Service。
2. 盤點真實操作及成功／無效輸入／邊界／權限／跨站隔離／失敗分支。不可只新增一個選單或 HTTP 200 測試。
3. 在 Diagnostics/backoffice-cases.json 新增或更新 page / case，使用穩定唯一 ID，保留來源、步驟與預期結果。
4. 若可自動執行，必須實作白名單執行器並綁定 executor；僅填入文字步驟不算已實作。
   - 列表流程：BackofficeReadOnlyChecks，同時指定頁面、識別欄位、必要欄位及明確 Service 呼叫。
   - 設定／統計：BackofficeObjectChecks，編寫實際回應及資料條件。
   - 寫入／檔案：BackofficeSandboxChecks 與其 partial，編寫獨立 fixture、實際操作及重新查詢斷言。
   - 只許登錄固定程式操作，不能讓清單指定任意 URL、SQL、反射方法或外部命令。
5. 更新 Supports 白名單。載入清單時會拒絕未知或頁面不匹配的 executor，不能默默略過錯字。
6. 不可為了消除待建置項目，就把人工規格綁到只檢查入口的執行器；不涵蓋的範圍需保留於內部規格並明確說明。
7. Bug 修正需補能重現問題的斷言；保留未變更 ID，不因排序或標題變動重新編號。
8. 靜態核對 JSON、唯一 ID、來源路徑、executor 對應、介面簽章與 Git diff，說明未執行的驗證。

每個 page 要有 controller、action、title、module、sources、cases。每個 case 要有 id、title、steps、expected、mode。
mode 使用 review / isolated / external；executor 是已實作程式的識別，不是可以執行任意程式的腳本。
沒有執行器的較廣泛人工規格可以保留，但不出現在自動檢測清單，不計入通過。

## 站台適用範圍

NavigationProvider.BuildDiagnosticsScopeAsync 先套網站功能設定，再套版本上限，遞迴保留有效父層下的啟用功能。
平台管理者不因此取得站台方案沒有的功能；形象站不納入訂單、營收或檢貨單。
跨功能 case 的 requiredFeatures 必須全部啟用，例如 ContactUsExported、DirectoryFacet 或 OrderManagement。
登入／忘記密碼／歡迎頁是明確登錄的共用入口；其他無功能映射入口預設不納入。

入口描述資料仍與全部內部規格比對 MissingPages / StalePages，排除需記錄 reason；結果只在伺服器模型，供維護者偵錯查看。
此比對不能自動辨識既有頁面新增按鈕或規則，所以開發時仍必須維護案例與執行器。

## 資產、套件與驗證

清單以 EmbeddedResource 置入 MVC 組件，更新後需重新編譯啟動。
MVC View 引用 Backoffice.min.js，原始碼為 wwwroot/js/SystemDiagnostics/Backoffice.js；由使用者手動執行 npm run create-bundles。
該 script 先檢查授權資產，再執行 yarn 與 gulp buildDev，會產生前端資產及啟動監看，可能下載套件及更新 lock file。
不由 Codex 執行。

隔離容器使用 Microsoft.EntityFrameworkCore.InMemory 10.0.10，與本專案 EF Core 版本一致，沒有升級既有套件。
新增 PackageReference 不表示已還原或已編譯成功；還原、編譯及實際執行由使用者依 AGENTS.md 決定。

建議手動檢查：形象／購物站範圍、未授權帳號、逐頁進度、停止後狀態、重跑清除舊結果、hash 切換、
空資料／不足資料的未執行狀態、隔離 CRUD 與郵件擷取、暫存目錄清理，以及一般寄信／訂單列表既有行為。

## 前台功能擴充回歸

新增五個綁定執行器的隔離案例：購物車新增／數量／刪除與所有權、加價購資格與優惠價、結帳優惠門檻、紅利有效期限與折抵限制、付款金額／站台／物流限制。
public-functions 同步核對讀取 API 白名單、拒絕寫入型 GET、站台與數量限制及部分進度。以上案例本次未執行；不代表 SQL 或瀏覽器交易驗證通過。

本次新增 sandbox/directory-all-content（Page.Index，依賴 ProductList 與 Article）：以獨立記憶體站台驗證全商品／文章的公開條件、會員瀏覽限制、站台範圍、分頁、最大筆數、不寫搜尋紀錄與未知來源拒絕。另補前台連結與畫布來源切換人工規格。均尚未執行；不代表 SQL 翻譯或瀏覽器通過。

新增 sandbox/menu-content-cache：先建立空頁的暖選單 JSON 快取，驗證發布新增／清空內容後 menu 版本與 hasContan 自動更新，只存草稿及其他站台版本不變；使用獨立記憶體資料庫，未執行，不代表 SQL 或瀏覽器驗證通過。人工複驗需確認前台重新載入後主選單可點擊狀態更新，不需再編輯選單。

menu-content-cache 補充：版本只於有／無內容切換時更新；有轉有、空轉空與草稿重用版本。空白 body／段落／nbsp 與樣式文字不算內容，發布後清空正式及編輯 HTML/CSS；讀取不復原空殼，純圖片保留。上述新增斷言未執行。

新增 sandbox/search-keyword-codec（Page.Index）：固定 UTF-8 Base64Url 協定向量、中文／emoji／特殊字元往返、無前綴舊關鍵字原值、非法格式回退。只驗證共用 C# 編碼類別，不表示 JavaScript、IIS 或瀏覽器已驗證；尚未執行。

## 自訂搜尋後台管理

新增 sandbox/cust-search-settings，沿用 SearchManagement.CustSearch.edit ID 綁定實際隔離執行器。涵蓋商品／文章固定項目、列表預設切換、三類混合範圍、全部旗標、預設切換、軟刪除、失效／跨站參照拒絕及不修改 Directory／搜尋紀錄。測試使用獨立 InMemory；尚未執行，不表示 SQL filtered unique index、Serializable 交易、API 授權或瀏覽器已通過。
editor 與 delete 人工規格補 API 權限、防偽、快速切換、錯誤保留表單、失效選項及取消操作；不計入自動通過數。前台 GetSearchList／SearchReleInfo 尚未接此階段設定，不宣稱後台預設已於前台生效。schema 與手動更新流程見下節。

# 自訂搜尋後台設定與手動更新

## 本階段範圍

後台列表／單筆／範圍選項／新增修改／刪除改用 Search API，不再呼叫 Directory API。商品 -2、文章 -1 以唯讀虛擬項目提供，名稱、範圍、顯示及順序固定，不提供編輯或刪除；只能由列表 SetDefault 選為本站預設，首次選取時保存該站設定。舊版已保存的系統名稱等覆寫不再套用，只保留預設用途。自訂項目可混合全部／指定標籤商品、全部／指定標籤文章、全部／指定選單。

同類標籤／選單預定採 OR、多類結果合併去重；本階段只保存範圍，前台消費尚待下一階段，不宣稱已生效。預設必須顯示；清除／刪除預設後，後台讀取回退至既有站台自動選擇。有商品時優先商品，否則文章；不在讀取時寫入資料。

API 每次依目前授權站台及 CustSearch 功能權限檢查讀取／新增／修改／刪除。設定預設透過獨立 SetDefault 操作並需修改權限；AddUp 忽略傳入預設值，修改自訂項時保留原預設。POST 需防偽 Token。所有範圍 ID 必須屬本站且有效。讀取／儲存錯誤不可變成空範圍的成功；原文顯示使用文字輸出。

## 必要 schema 變更（未建立或執行 Migration）

CustSearch 新增：Description（nullable nvarchar(150)）、IsDefault（bool 預設 false）、SystemTargetId（nullable bigint）、ScopeJson（nullable nvarchar(max)）。既有三個 SearchAll 旗標保持。ScopeJson 保存 ProductTagIds、ArticleTagIds、MenuIds，不使用可清除的 JsonObjects 快取存放業務設定。

新增兩個只套有效資料的 unique filtered index：站台＋SystemTargetId（非 null），站台＋IsDefault（true）。切換預設於關聯式資料庫使用 Serializable 交易，先清除舊預設再保存新預設；InMemory 不模擬此交易，不能當成 SQL 驗證。

建議手動建立 Migration：

~~~powershell
dotnet ef migrations add CompleteCustSearchSettings --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此命令可能編譯與還原套件，會新增 Migration／更新 snapshot，耗時依環境而定。先檢視產物，確認僅包含預期的欄位及索引；本次未產生任何 Migration。

完成審查及備份後，由使用者自行決定是否執行：

~~~powershell
dotnet ef database update --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此命令會改變指定環境的資料庫 schema，可能需要鎖定與較長時間；不得在未確認連線環境前執行。原始碼不可先於 schema 單獨啟用，否則讀取新增欄位會失敗。

既有 CustSearch 資料保留，新欄位為空值／false；不刪改舊標籤關聯，也不推測無法區分商品／文章的舊標籤所屬範圍。舊資料若沒有完整範圍，需管理者於表單重新確認後保存；空範圍不允許保存。

## 前端資產與手動驗收

MVC 頁面仍引用 CustSearch.min.js。由使用者在 src/EtheriT.Coker.Web.MVC 執行 npm run create-bundles。實際 script 先檢查 DevExtreme 授權，再執行 yarn && gulp buildDev；可能下載相依、修改 lock file、產生大量資產並啟動監看，耗時依環境而定。未由 Codex 執行。

手動核對：兩個系統項目、混合範圍保存重開、顯示／排序、切換預設、全部旗標、失效範圍、取消／軟刪除、跨站隔離、無權限及防偽失敗。不得只以 HTTP 200 作為儲存成功。

## 下一階段前台接入

GetSearchList 需載入顯示項目、站台名稱／排序覆寫與預設。SearchReleInfo 需依自訂範圍查詢並套用公開／期間／站台／會員權限；同一解析器亦需供上下則使用。
既有正整數 1／2 有站台歷史行為、3 有舊商品搜尋相容語意；串接正數 CustSearch ID 時必須另外辨識新自訂來源，不可直接覆蓋既有路由。後台此階段不產生新的前台自訂搜尋連結，也不改動既有前台判斷。

建議手動編譯指定 MVC 專案（尚未執行）：

~~~powershell
dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此命令可能自動還原套件並寫入 bin／obj，耗時依環境而定。完成 schema 及資產更新後，再於測試環境啟動並依功能清單驗收。

## 搜尋設定版面與共用標籤選擇回歸

列表使用單選控制直接保存預設；兩個固定項沒有編輯／刪除入口且 AddUp 後端拒絕固定 ID。表單不再包含預設欄位，縮短基本資料與三類範圍排列。商品／文章標籤共用既有 TagSelectModal 與 TagListModalInit，各欄位各自保存 tagList，讀取明細只載入已選標籤名稱，不預載全部標籤；搜尋、群組及分頁沿用共用模組。選單下拉限制高度及寬度。
原 sandbox/cust-search-settings 補 SetDefault、固定項拒絕、一般保存不可修改預設、選取名稱與不預載全部標籤斷言。editor 人工規格補單選預設失敗還原、共用標籤兩欄切換、清除／取消／確認及緊湊／窄螢幕版面；尚未執行。此次沒有新增 schema 變更，未執行 bundle／build／Migration。

精簡操作補充：儲存按鈕直接送出一次，不再顯示第二次確認；返回僅在有未保存變更時詢問。預設切換只改目標旗標，不從表單重寫名稱或範圍，另補隔離斷言。

## 搜尋預設設定 HTTP 400 回歸

SetDefault 回傳無欄位 errors 的 HTTP 400，符合防偽驗證拒絕的回應；尚未取得伺服器驗證日誌，因此不宣稱已確認唯一原因。比照 Contact 匯出流程，Search API 新增經讀取權限檢查、no-store 的 GetAntiforgeryToken；前端在新增／修改／預設／刪除 POST 前取得最新 Cookie／RequestToken 配對，更新 hidden 與 header，取得失敗就不寫入，不自動重送業務 POST。原 ValidateAntiForgeryToken 不移除。
editor 人工規格補舊頁 Token／Cookie 更新、先取 Token 再 POST、Token 失敗零寫入、直接缺 Token POST 拒絕及預設失敗還原。未執行 API、瀏覽器或資料庫寫入測試；只核對原始碼、JS 語法、JSON 與 diff。MVC 需重新編譯及手動 npm run create-bundles；資產影響與命令見前述章節。

## 商品搜尋選項依可見資料判斷

HasSearchableProductsAsync 為後台與現有前台 GetSearchList 共用條件：站台等級可用商品、本站、未刪除、Visible、未下架且永久或位於有效上架期間。沒有符合商品時不回傳固定商品項，Options.HasProducts=false，編輯區不提供新增商品範圍，後端亦拒絕設定商品預設或新增／變更商品範圍。
既有商品範圍保留，可保存未變更的範圍或明確移除，不因商品暫時下架而自動刪除。純商品自訂項在無商品時 IsAvailable=false，不可設為有效預設；混合項仍可使用文章／選單部分。有效預設回退文章，但不於讀取時重寫已存偏好；商品恢復後可重新提供。點已勾選的自動回退項同樣送出 SetDefault，使管理者能明確保存目前選擇。
原 sandbox/cust-search-settings 補無商品、不可見／下架／過期／未開始／刪除／跨站商品、有效商品恢復、最後可見商品消失、範圍保留與移除及預設回退斷言。使用隔離 fixture，尚未執行，不表示 SQL 或瀏覽器通過；editor 人工規格同步更新。未新增資料庫欄位或 Migration，未執行編譯、bundle 或資料庫操作。

搜尋設定列表顯示順序回歸：移除欄位 SortOrder／SortIndex，預設排序改由 GetAll 的載入選項提供，避免顯示多欄排序箭頭及順位 2；保留外部明確排序供原列表唯讀檢查使用。系統列的順序數字留白，自訂列顯示其 SerNO；欄名維持「排序」並加寬至 110px；數字僅作排列先後，不是前台顯示內容。editor 人工規格補橫向標題、無標記及系統列留白／自訂列順序與分頁；尚未執行瀏覽器驗證。

搜尋設定編輯版面回歸：移除編輯卡片 col-xl-10／col-xxl-9 的額外限寬，與列表及頁面內容同寬對齊；移除 small、form-control-sm、btn-sm，恢復系統標準文字與控制項尺寸，保留緊湊同列配置。editor 人工規格補寬螢幕對齊、標準字級與窄螢幕換行；未執行瀏覽器或編譯驗證。本次只改 Razor 與檢測說明，不需重產前端 bundle。

搜尋設定操作一致性：編輯標頭沿用商品頁的左側 keyboard_backspace 返回箭頭＋標題、右側儲存；保留有變更才確認返回的既有邏輯。商品／文章／選單範圍改為 col-auto 類別與全部勾選、col 選擇器，不再以 1／2／9 百分比欄寬分配短文字。標準字級與全寬維持。editor 人工規格同步更新，尚未執行瀏覽器驗證；本次只改 Razor，不需重新產生 bundle。

## 搜尋設定 SQL Server 重試策略與交易回歸

MVC Program 已啟用 EnableRetryOnFailure，但 CustSearch SetDefault／AddUp 原本直接開始手動交易，會與 SqlServerRetryingExecutionStrategy 不相容。新增共用 ExecuteAdminTransactionAsync：由 CreateExecutionStrategy.ExecuteAsync 包住完整 Serializable 交易，失敗結果不 commit，DbUpdateException 在策略外處理，使可重試的資料庫錯誤有機會重做。重做前僅清除本站 CustSearch 的已追蹤狀態，重新查詢資料。新增項目保留已配置 ID，以便提交成功但確認遺失時檢查原項目存在，避免再次插入。
原 sandbox/cust-search-settings 補四次預設反覆切換、每次只有一個本站預設斷言；尚未執行。InMemory 不驗證 SQL Server 重試或真正的 rollback。editor 另補啟用 EnableRetryOnFailure 的隔離 SQL Server 人工驗收及暫時連線失敗情境，不得用客戶資料庫寫入來自動檢測，也不把記憶體結果宣稱為 SQL 通過。此次未執行編譯、測試、Migration、bundle 或資料庫操作。

搜尋設定編輯標頭整理：列表專用預設說明於編輯時隱藏；標頭左側維持返回箭頭＋標題，右側為顯示開關與儲存。名稱與搜尋提示排第一列，描述與較窄排序排第二列，不再把顯示開關掛在描述旁。顯示開關移出 fieldset 後，custSearchSetControls 明確維持載入／儲存／無權限時的停用規則，保留原欄位 ID 與保存資料。editor 人工規格同步更新，未執行瀏覽器驗證。此次涉及 CustSearch.js，MVC 需手動 npm run create-bundles；可能下載相依、改 lock file、產生資產並啟動監看，未由 Codex 執行。

## 商品與搜尋編輯頁樣式共用

原商品灰色標頭／表單底色綁定 ProductContent／ProductForm，搜尋頁未套用。將相同規則抽至 wwwroot/css/CokerCore/editor.css，加入既有 CokerCore.min.css 輸入，兩頁以 backoffice-editor 明確套用；移除商品頁重複配色規則，仍使用既有 --color-gray-bg-dark 與全站字級／按鈕色票。搜尋表單白色內容卡沿用商品 card p-2，顯示開關改用既有 selectedItem＋visibility／visibility_off 圖示，保留欄位 ID 及權限／忙碌停用。
ProductManagement.ProductList.edit 與 SearchManagement.CustSearch.editor 人工規格補兩頁配色對照、圖示切換、窄螢幕與商品原操作回歸；共用樣式為選用，不自動改動其他功能。未執行瀏覽器、build 或 bundle，沒有新增 schema 變更。
MVC 應由使用者手動 npm run create-bundles，讓 CokerCore.min.css 與 ProductList.min.css 同步更新；既有流程先檢查授權，再執行 yarn && gulp buildDev，可能下載相依、更新 lock file、產生大量資產及啟動監看，耗時依環境而定。不得直接引用 editor.css 繞過 bundle。
