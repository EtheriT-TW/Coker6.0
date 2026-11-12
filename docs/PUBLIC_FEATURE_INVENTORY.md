# 前台功能盤點與逐頁檢測規格

本文件是原始碼盤點與內部驗收規格，不是已執行結果。
目前已拆分圖片連結與功能入口，並提供 29 類觸發式執行器，另保留嵌入檢查。
下表是進階驗收範圍；各類已實作的斷言見 PUBLIC_FUNCTION_TESTS.md，不能把有限執行器等同下表所有變體、真實交易與外部服務均已通過。
本次未啟動網站、瀏覽器或測試；不能據此判定站台 49 或任何特定頁面實際使用哪些功能。

## 盤點來源與適用性

以 src/EtheriT.Coker.Web.Public/wwwroot 為以下腳本路徑的根目錄。
從 js/Coker/coker.page.init.js 的二十一筆 registry 出發，追蹤直接相關模組、Layout、搜尋與購物流程。
這是已確認範圍，不代表全站所有功能的完整清冊；後續仍需依實際載入的頁面腳本及動態元件補登。

適用性必須同時確認目前站台版本、功能設定、父層開關、頁面／元件設定與實際功能入口。
載入共用 bundle 或提供全域函式，不等於目前頁面使用該功能。
隱藏樣板不列實際實例；但由可見按鈕開啟的隱藏電子書／購物車 modal，仍要依觸發器列入。
不同目錄、輪播、表單、電子書需以元件實例區分，不能第一個成功就代表整頁全部成功。

## page.init 已登錄的二十一種功能

以下為各功能的內部驗收範圍；均已綁定操作執行器，但不代表所有裝置與資料變體皆已驗證，不得以初始化旗標代替行為斷言。

| 內部識別 | 使用條件（registry） | 待等待／驗證的功能行為 |
| --- | --- | --- |
| directoryGetData | .catalog_frame、.menu_directory、.advertise_directory | 每個目錄資料請求完成、資料與設定相符、正確空結果、排序／過濾／分頁、明細連結；選單及廣告目錄另分實例 |
| swiper | .swiper_components、.one_swiper、.one_swiper_thumbs、.two_swiper～.six_swiper、.picture-category、.three_two_grid_swiper、.vertical_swiper_thumbs | 實例建立、圖片載入、前後切換、縮圖同步、不同視窗寬度；內容不足時不能假測多頁切換 |
| marqueeSwiper | .marqueeSwiper | 站台跑馬燈資料完成、無資料處理、輪播與連結 |
| frameInit | .masonry、.YTmodal_frame、.imageList.layout-masonry | 圖片就緒後排版、影片觸發／關閉、正確來源及錯誤狀態 |
| viewTypeChange | .type_change_frame | 切換列表／網格後內容及數量維持、狀態與版型同步 |
| categoryFilter | .filter-btn | 分類切換、對應內容、無結果與重設；不可只驗證按鈕 class |
| hoverMask | .hover_mask | 滑入／離開及鍵盤焦點呈現；不遮擋原連結 |
| sitemap | .sitemap_hierarchical_frame | 階層資料完成、父子關係、展開收合與正確路由 |
| linkWithIcon | .link_with_icon | 依設定呈現圖示、文字、連結與開啟方式 |
| anchorPoint | .anchor_directory、.anchor_title | 目錄與錨點對應、捲動定位與固定頁首偏移 |
| shareBlock | .shareBlock | 分享選單產生、當頁／指定網址及編碼正確；不實際發文或寄信 |
| flipTimer | .flipdown | 合法時間、倒數更新、到期呈現與錯誤輸入 |
| articleTags | root.closest('.article') 有結果且含 .article-tags | 標籤資料、文字與連結；需另確認 document 根節點初始化及動態根節點的差異 |
| contactForm | .ContactForm、.DynamicForm | 表單設定、欄位與驗證碼載入、必填／格式／條件欄位；不提交真實表單或寄信 |
| remoteTracking | root 含 body、存在 RemoteTrackingToken 及 RemoteTracking.init | 設定與初始化錯誤觀測；實際追蹤可能產生紀錄，不能自動重放追蹤呼叫 |
| bgCanvas | .BGCanvas | 畫布初始化、視窗變更與呈現；不要等待持續動畫完全停止 |
| flipBook | .FlipBookItem | 依可見入口確認 PDF 來源、下載／解析／首屏完成、頁數、翻頁／縮放／關閉；Layout 共用 modal 本身不算使用 |
| mapMessage | .MapMessage | 圖區與訊息目標存在、滑入圖區後切換對應訊息；此功能不是第三方地圖 |
| geolocation | .getlatlng | 權限允許／拒絕／不可用的顯示處理；不要求測試主機真實位置 |
| mobileNavbarMove | root 含 body、body 寬小於 992、有 #lanBar 及 #layout4 #NavbarContent | 行動尺寸移動位置、展開／收合、桌面與行動版切換；只跑目前 1365px 視窗不會涵蓋 |
| sideFloating | #Floating_Center、.side-floating-item | 依設定顯示、展開／收合、正確動作及連結；避免觸發電話或外部通訊 |

## registry 以外已確認的流程

| 功能 | 已追蹤來源 | 使用與完成判斷／待驗證 |
| --- | --- | --- |
| 搜尋入口 | js/Coker/coker.feature.search.js | 實際呼叫 Search.Init 的入口與 .input_sear、.btn_sear、#SearchInput；空白提示、分類、Enter／按鈕、網址編碼與搜尋結果 |
| 搜尋結果與建議 | js/Search/Search.js（實際載入頁仍需核對） | .search-category、.search-input、.search-suggestions 及目錄；關鍵字資料完成、建議、分類切換、無結果、分頁及瀏覽器本地紀錄；來源存在不等於該頁有載入 |
| 商品與快速購物車 | js/Modal/ShoppingCarModal.js、js/ProductAction.js、Views/Shared/_Layout.cshtml | 可見購買觸發器，而非共用 #ShoppingCarModal；規格、售價、庫存、數量上限及錯誤狀態；加入／修改購物車需隔離環境 |
| 商品加價購 | js/Page/ProductContent/ProductAddOnPurchase.js、Views/Shared/_ProductAddOnPurchase.cshtml | [data-product-addon]、活動 API GetProductAddOnCampaigns；活動請求完成、資格、數量、價錢、與主商品選擇同步；不可讀取尚未完成的空陣列便判定無活動 |
| 購物車／結帳 | view-resources/ShoppingCart/shopping-cart.init.js 與同目錄模組 | 實際購物車頁的初始化入口、商品／規格／群組、收件人、物流、步驟與總額；實際新增刪除、訂單及物流操作不能在客戶資料執行 |
| 行銷優惠 | js/Coker/coker.api.marketing.js、view-resources/ShoppingCart/shopping-cart.marketing.js | GetCartMarketingCampaigns 完成後比對滿額／折扣／加價購的適用及金額；該讀取介面使用 POST，不能只按 HTTP 方法判定有寫入或可安全放行 |
| 付款方式取得 | js/Coker/coker.api.payment.js、view-resources/Payment/hosts/shopping-cart/availability.js | GetAvailablePayments 回應與最後一次物流／金額條件一致、種類／可用狀態及提示；非僅 HTTP 200。重新付款、CheckOrder 與付款種類取得需分開審查 |
| 紅利試算 | view-resources/ShoppingCart/shopping-cart.pricing.js | 實際紅利區與會員／站台設定；折抵上限、餘額不足、商品選擇變動與總額一致，使用隔離會員資料 |
| 紅利紀錄 | js/Coker/coker.api.bonus.js、view-resources/Member/js/Member.Bonus.js | 會員頁實際載入與 GetFrontUserBonusHistory；登入、分頁、無資料、網站／會員隔離；不能用管理員 Cookie 當前台會員 |
| 全域連結事件 | js/Coker/coker.global.events.js | a.is-no-link 的委派事件；不跳頁但保留事件冒泡，例如行動導覽收合 |

js/Page/Search.js、js/Page/Product.js 亦定義 PageReady、排序／版型／輪播／分享／購物車等入口。
Views/Page/Search.cshtml 已確認直接引用 js/Page/Search.js；不能將其誤認成 js/Search/Search.js 的新式搜尋流程。
其他同名函式或舊腳本是否有效，必須比對該頁實際引用及呼叫鏈，不能因檔案存在就擴大檢測清單。

## 逐頁清單與等待模型

每頁需先輸出內部功能實例清單，再交由固定白名單執行器處理：

1. 依站台功能與頁面設定，建立預期存在的功能；即使載入失敗、DOM 沒產生，也不能漏掉該功能。
2. 瀏覽器在頁面程式開始前觀測模組錯誤、相關請求、CSP 阻擋、動態 DOM；不從不可信頁面取得可執行測試指令。
3. 以實際 DOM／可見入口確認實例，排除樣板；追蹤目錄載入後新增的輪播、分享等相依元件。
4. 每個實例有獨立完成條件與期限，例如最後一份目錄請求完成、電子書首屏完成，而不是固定 sleep 或全站 networkidle。
5. 資料回應後核對實際 DOM／結果，再執行已審查的操作斷言。HTTP 200、函式存在與初始化旗標都不能代替功能通過。
6. 依序區分待處理、載入中、可驗證、執行中、通過、失敗、受限制／逾時／未實作；不把未測試或不適用算成通過。
7. 完整清單在背景逐步保存；離開頁面不取消，返回可查看目前適用且已有實際執行器的檢測狀態。

使用者畫面僅顯示易懂功能名稱、檢測項目與狀態；內部識別、selector、來源、API 與開發規格不輸出。
尚未綁定實際執行器的人工規格保留本文件，不加入使用者自動清單或通過數。

## 目前檢測器的限制

- 圖片連結流程仍採有上限的載入等待；功能流程已對輪播／版型做有限就緒等待與操作斷言，各類操作改用限定時間等待資料／元件證據。
- registry.safeRun 只包同步呼叫，並未 await 非同步完成；部分 run 在函式不存在時直接返回，仍會 markInited。
  因此 coker_inited_* 只能當觀測資訊，不能當成功證據。
- 目錄讀取 POST 已逐一追蹤並登錄固定白名單；其他業務 POST 與未審查 API GET 阻擋。產品顯示與驗證碼改供應隔離回應。
- 不可為了功能測試全面開放 POST、Cookie、內網或移除 CSP。CSP 阻擋不得用另外直接請求成功覆蓋。
- 目前抽樣最多二十頁，不能聲稱盤點了全站每個頁面的功能；頁面覆蓋與功能行為覆蓋應分別記錄。

## 後續驗收

確認同站不同頁的功能清單不同、關閉功能不列入、共用 modal／bundle 不造成誤判、動態元件可補登，
並分別驗證正常、空資料、慢回應、請求失敗、CSP 阻擋、函式缺失及初始化失敗。
每個可自動化項目需實作執行器與斷言後才登錄功能檢測清單；交易及會員情境另用隔離資料。

## 本次執行器補齊

21 個註冊功能及搜尋、快速購物車畫面、購物車／加價購／結帳優惠／紅利／付款隔離規則、無連結選單，共 29 類均有操作與斷言。完整邊界見 PUBLIC_FUNCTION_TESTS.md。
這是執行器涵蓋範圍，不是實際檢測通過宣告；未觸發、元件未就緒、樣本不足與受限制保留未完成。
