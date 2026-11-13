# 前台檢測

## 兩個獨立入口

- SystemDiagnostics/Public：前台圖片、連結檢測。圖片實際載入與解碼；一般連結只檢查 HTTP 可用性，不代表目的地功能正常。
- SystemDiagnostics/Functions：前台功能檢測。逐頁觀察實際元件，再執行已支援的操作與斷言，並保留原有 iframe／影片嵌入檢查。

兩者均提交 Hangfire diagnostics 專用佇列，單次一份工作；離開頁面不取消，可返回查看進度、停止及複測。
每份背景工作共用一個瀏覽器，每頁建立新的非持久工作階段；不共用 Cookie、本地儲存或使用者瀏覽器設定檔。
工作旗標 FunctionScan 持久化於 Hangfire，恢復後不可切換種類。範圍指紋包含種類及新規格版本，舊結果不得冒充新規格通過。
結果分別保存於 App_Data/PublicDiagnostics 與 App_Data/PublicFunctionDiagnostics，網站及帳號各自隔離，複測只重設自己的結果。
兩種入口仍各自最多抽樣二十頁，文章、商品與證照各三筆；這不是全站所有頁面覆蓋。

## 圖片、連結流程

- 最多一百張可見圖片，排除隱藏範本；設為 eager，解碼並行等待，單批最多三秒。仍未就緒不列通過。
- 不再額外建立影片 iframe，也不讀取 iframe 子框 DOM；阻擋子框導航及 media 資源，避免第三方嵌入深入等待。
- 最多一百個可見且不重複的一般連結，最多四個並行。優先 HEAD，只有 405／501 回退 GET，兩者只讀回應標頭。
- 每跳轉址檢查 HTTP／HTTPS、連接埠與公開 IP；最多三次轉址。2xx 只代表 HTTP 可用，401／403／429 為無法確認。
- 電話／郵件格式錯誤、危險操作與不適用本站功能路由保留原有保護。
- 頁面等待縮短；單頁取消期限四十五秒，不宣稱已有實測速度數據。

## 功能流程與實作範圍

依 coker.page.init.js 的 21 個註冊功能，另納入搜尋、快速購物車畫面、五種隔離購物規則及無連結選單，共 29 類執行器。依站台功能及可見元件觸發，不因共用腳本存在就列為使用。

- 資料：目錄回應與畫面順序／標題／連結比對、排序、篩選與分頁；網站地圖、跑馬燈、文章標籤及實際搜尋導頁／無結果顯示。
- 操作：輪播、版型、分類、滑入效果、檔案圖示、頁內定位、分享目的地、倒數、表單必填／格式／條件欄位、背景畫布、電子書翻頁與縮放、互動圖區、模擬定位、行動導覽、側邊工具、圖片排版與影片開關、無連結選單。
- 快速購物車：操作實際開啟入口、規格與數量控制；商品資料由實際服務在隔離容器產生，不查詢客戶商品的會寫入型顯示服務。
- 隔離購物：實際服務的新增／修改／刪除、超庫存、跨會員、滿額優惠、加價購資格與優惠價、有效紅利與超額折抵、付款種類金額界線／跨站／物流限制。結果明示隔離資料，不表示真實瀏覽器交易已驗證。
- 嵌入：保留可見 iframe／影片的內容、錯誤與媒體資料判斷，CSP 阻擋不由另發請求的成功取代。

功能頁預設包含未完成項目，複測納入未完成或尚無結果的抽樣頁。
功能清單在各階段保存，未完成頁面不算完成；動態內容最多重新觀察三輪。每類最多十個元件，電子書最多三個；超限、未就緒、樣本不足或限制均不得當作完整通過。
無觸發代表本次觀察未涵蓋，不代表沒有功能或整頁功能通過。兩種檢測的通過數不可相加當成完整覆蓋。
內部盤點詳見 [PUBLIC_FEATURE_INVENTORY.md](PUBLIC_FEATURE_INVENTORY.md)。

## 安全與生命週期

沿用原有公開網路、DNS-pinned HttpClient、每跳私網／本機阻擋、TLS、CSP 與瀏覽器 sandbox 保護。
前端只能提交目前站台、範圍與複測選項，不能指定 URL、JavaScript 或任意瀏覽器操作。
每頁執行前重新檢查平台管理者與功能範圍。查詢、停止及結果讀取比對種類、站台與帳號。
只允許 GET／HEAD；功能模式唯一 POST 例外仍為原有固定 YouTube 播放資料端點，不開放一般本站或第三方 POST。
已確認目錄資料透過 POST 取得，現有策略會阻擋；未審查 Controller／Service 副作用前不自動放行，不假稱目錄功能通過。
不登入、不提交業務表單、不加入購物車、不建立訂單、不寄信或付款；HTTP／網頁初始化仍可能產生快取或存取紀錄。
每頁三百個瀏覽器請求、單資源八 MB、瀏覽器總資料六十四 MB，功能單頁取消期限一百八十秒。
工作取消期限四十五分鐘；不接受取消的舊服務仍須等待返回。網站停機期間不執行，恢復需保留 Hangfire 儲存與 App_Data。

## 驗證與資產

新增 sandbox/public-functions：以隔離證據檢查結果分類、兩種類結果隔離、複測及背景旗標還原；既有 public-diagnostics 新增 HEAD 不支援的 GET 回退斷言。
新增五個 sandbox/public-commerce 執行器，驗證上述隔離購物流程。
這些案例沒有啟動瀏覽器或連線客戶資料庫，不代表實際瀏覽器／SQL 驗證通過；本次均未執行。
人工驗收涵蓋兩入口、未授權、種類隔離、離開返回、停止、初始化失敗、單張輪播、版型／分類實際操作、CSP 阻擋與超限。

頁面仍引用 Public.min.js；修改原始碼後由使用者在 MVC 專案手動執行 npm run create-bundles。
該流程檢查授權資產後執行 yarn 與 gulp buildDev，可能下載套件、修改 lock file、產生大量資產與啟動監看；耗時依環境而定。
Codex 未執行 build、restore、test、bundle、瀏覽器檢測或任何資料庫操作。

## 全內容目錄回歸

後台元件 data-content-source 支援 all-products／all-articles，前台送 ContentSource 至原目錄 API。
全內容查詢不需關鍵字、不寫搜尋紀錄；全文章僅文章管理內容，商品套公開、上架期間與會員瀏覽權限；保留站台、分頁、排序及最大筆數限制。
未指定 data-dirpath／pageto 時連至 search 下的商品／文章網址，指定路徑優先；圖卡包含真實 a href，含購物按鈕的卡片不建立巢狀互動元素。
新增 sandbox/directory-all-content，使用獨立 InMemory fixture 斷言資料隔離、公開條件、分頁、最大筆數、唯讀與未知來源拒絕。尚未執行，不表示 SQL／瀏覽器通過。
人工驗收另涵蓋全內容列表的公開入口、分頁 a href、排序／過濾、顯式路徑、畫布來源切換與舊目錄上下則；前台自動檢測仍有既有 POST 封鎖限制，不擴大放行。
前台與 MVC 腳本仍使用既有 min.js 產物；由使用者分別在 Web.Public 與 Web.MVC 執行 npm run create-bundles，未由 Codex 執行。

## 無關鍵字正式網址上下則

search/product/ID 與 search/article/ID 未帶關鍵字時，SwitchPage 依同站公開條件及全內容列表預設排序投影 ID／標題，回傳相鄰兩格，首末保留空格。無搜尋日誌、無商品圖片／價格／完整內容批次載入。
商品 controller.js 與文章 wwwroot/js/Page/Index.js 區分有／無關鍵字，無關鍵字不使用搜尋 sessionStorage，不拼接 undefined 或空搜尋字至上下則網址。
沿用 sandbox/directory-all-content 補首末、反向、隱藏內容及文章相鄰斷言，未執行。人工驗收補直接開啟正式網址、商品 AJAX 切換／返回、原目錄與帶關鍵字搜尋回歸；尚未執行瀏覽器驗證。

## 排序與篩選瀏覽參數

共用 coker.api.directory.js 的 getNavigationContext／withNavigationContext：非預設排序帶 sort／direction；名稱、標籤、再分類、目錄與筆數限制以 name／filter／facet／directoryType／dirid／maxLen 傳遞，特殊路徑的全內容元件另帶 source。使用 URLSearchParams 編碼；無條件省略對應參數，canonical 維持原正式網址。
商品 controller 與文章 Index 將條件送 SwitchPage，並套用至上下則及返回列表；關鍵字搜尋 sessionStorage 鍵包含完整條件，避免混用不同排序。列表初始還原 URL 排序及篩選，保留使用者後續操作。
既有 sandbox/directory-all-content 補名稱排序反向、名稱過濾、最大筆數及真實關鍵字搜尋條件斷言；未執行。人工規格補三種列表、中文與特殊字元、另開分頁、清空條件、返回列表及原目錄上下則。

價格降冪人工回歸：從 /C8SHOP/Products?sort=price&direction=desc 依列表開啟商品，前後則須依列表相鄰 ID（跨品牌／目錄仍連續），連續切換及返回列表均保留 sort／direction；另核對未帶參數時使用預設順序。商品全內容分支返回列表不得覆蓋已附加的排序條件。此回歸尚未驗證修正後結果；僅重現原問題。更新 DTO／Application Service 後須重新編譯並重新啟動前台，僅重新產生 JavaScript bundle 不會更新後端排序。
