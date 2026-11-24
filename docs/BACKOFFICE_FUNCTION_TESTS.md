## 2026-11-24 實頁重現：連結HTML拒絕與目錄內嵌Swiper

本次內建瀏覽器以實際頁面49／11019重現兩個失敗，未儲存或發布。選取Span送「調整為連結」：開發診斷第一回EmptyProposal，第三回本地HTML安全拒絕，當次尚無具體標籤／屬性原因，不可推定一定是metadata造成。選取目錄送「調整為四欄位」：依序是外框#selected禁止Grid、不存在#catalog、外框#layout-container禁止Grid。DOM實際為catalog_frame > six_swiper > swiper > catalog.swiper-wrapper，卡片由Swiper配置inline寬度；前一輪一般Grid修正未涵蓋此結構。

修正規劃資料辨識一至六欄既有Swiper版型、提供唯一catalog精確別名，樣式契約新增--coker-swiper-columns（僅1至6、既有對應Swiper根元件）。Swiper內Grid計畫回傳實際根ID與可用能力，前端沿既有版型class切換、destroy既有Swiper、清除isInit並重跑原SwiperInit；不重建目錄HTML／資料來源／樣板，重新初始化失敗回滾。此能力是元件契約，不以客戶中文句子或數字正則分派語意。

連結HTML安全驗證的具體原因回饋規劃，不再一律泛稱不安全；允許a上的系統data-link-type=link及經既有HTTPS／站內／錨點規則驗證的data-link-value，空值視為待設定。前端移除空href避免未設定連結回到當頁，仍拒絕javascript、任意data屬性與危險HTML；不放寬全域data-*。

沿既有sandbox/ai-provider-contracts新增目錄外框→catalog內Grid拒絕→Swiper能力修正的真實巢狀fixture，普通圖庫不得用Swiper能力、原生待設定連結metadata可通過、metadata中的javascript拒絕。固定HTTP fixture僅驗證本地資料與契約，尚未執行。人工需重測原兩句、實際4／6欄卡片寬度、手機斷點、目錄換頁／重載、連結modal與還原；尚未編譯或瀏覽器驗證本輪修正，不宣稱已通過。

本輪修改CanvasAiService／CanvasAiPlanning、aiAssistantPlugin及隔離斷言／清單，沒有改生成資產。需使用者手動編譯／重啟MVC（可能還原套件、產生bin／obj）及packages/EtheriT.Coker.GrapesJS的npm run build:mvc（生成dist並複製MVC模組），耗時依環境。本輪沒有再改MVC舊Grapes腳本，不需為本輪額外重跑create-bundles；上輪若尚未產生grapesjs-Coker6.min.js，仍需完成上輪指定流程。

## 2026-11-24 AI 交接至既有連結／目錄設定視窗

前端依已註冊的固定設定命令宣告AvailableSettings（link／directory），與目前HTML、結構及需求送交AI判讀，沒有網址／目錄關鍵字分派。模型可在原有計畫附settingsActions（最多一筆kind＋targetId）；後端只接受已宣告能力且目標為當前片段內唯一相容的a或目錄外框，越界、錯誤類型、任意command／未註冊能力皆拒絕。缺值的必要澄清可直接交接設定視窗；可先完成的連結結構修改則先驗證內容，再交接新的a。missingInput=link-destination／directory-source亦可對相容選取元件轉成設定交接。UI語系校正只能改訊息，不可改已驗證設定目標。

linkComponentPlugin註冊固定命令並沿用openLinkComponentEditor，僅在真正關閉而非檔案庫暫停時通知交接完成；目錄命令沿用原data-dirid trait及PopupDirectory，維持原有站台API、選擇驗證與確認／取消。AI不替使用者填猜測網址或選目錄。前端開啟前再次定位目前元件，保存設定前畫布，暫時隱藏聊天；關閉設定後同頁才返回聊天，若資料改變則保留設定後與設定前兩個版本。不自動送下一次AI請求、不自動保存伺服器。聊天保留設定入口可重新開啟；歷史目標失效不開其他元件；待處理、跨頁、編輯器銷毀不重新開啟舊聊天。

沿既有sandbox/ai-provider-contracts的AiCanvasPlannerChecks新增：Span先轉a再交接連結、既有連結／目錄純設定交接零HTML、最後語系校正無權替換設定目標、未宣告能力／不相容元件／越界ID／任意動作拒絕。新增斷言已綁既有executor，但尚未執行，不宣稱真實模型或瀏覽器通過。人工驗收：兩個設定視窗的開啟／關閉／取消／確認，link檔案庫暫停返回不提早重開聊天，目錄API失敗仍可關閉；設定後接新指令依最新資料、還原設定前／後、舊聊天重開、跨站／跨頁／唯讀與取消生成不開設定。這些人工項目保留review，不列自動通過數。

本輪只做來源修改、JS語法、JSON與diff靜態檢查；未編譯、執行檢測、bundle或寫入資料庫。手動編譯／重啟MVC可能還原相依並寫入bin／obj；packages/EtheriT.Coker.GrapesJS的npm run build:mvc更新dist與MVC模組；src/EtheriT.Coker.Web.MVC的npm run create-bundles更新grapesjs-Coker6.min.js等既有引用資產，其實際流程先檢查DevExtreme授權再yarn及gulp buildDev，可能下載相依、修改lock file、產生多個資產與啟動監看。三者耗時依環境，本次均未執行。

## 2026-11-24 對話語系、目錄動態卡片與修改前還原點（目前有效規格）

本節取代下方「建議尚未實作」與瀏覽器強制回覆語系的舊記錄。模型以結構化replyLocale／replyLanguageBasis判讀：明確指定回覆語言優先，其次充分明確的使用者輸入；引文、HTML待改文案、品牌、網址、數字與短句不作切換依據。continuity由後端維持已驗證ConversationLocale，首次才依瀏覽器預設；中性語言判讀不得抹去同語言的區域偏好。最後UI校正採已選語系，新增HTML仍依授權網站WebsiteLocale。前端只持久化成功或必要澄清的回覆語系。真實模型語言判斷列人工驗收，不把fixture當多語能力通過。

未提供網址時可先將文字改為無href的a，摘要需說明尚未設定並詢問目的網址；後續以對話承接。確實缺少link-destination／new-text／asset-source可用missingInput與唯一SelectedId作澄清證據，不再強迫虛構兩個競爭目標；寬度、間距等仍依當前資料推斷。

目錄結構依Directory renderer實際.catalog_frame／.catalog／.template／.templatecontent區分。外框設欄軌計畫須回AI修正；只允許授權HTML中既有目錄.catalog的精確範圍選擇器「#id > .template」保留至執行時，避免把複製商品卡片規則轉為隱藏樣板ID。Grid補齊直接卡片width:auto／max-width:100%／min-width:0／flex-basis:auto；資料來源、樣板及HTML不重建。前端驗證實際Grid卡片未縮至格寬一半以下，失敗回滾；尚未瀏覽器驗證，不能宣稱已重現或解決所有目錄版型。

AI齒輪設定使用原生dialog／showModal，含標題、關閉、Esc與背景遮罩。版本選取不立即還原，需明確按還原。每次送出前保存實際畫布，與歷史聊天有無版本無關；成功訊息保留修改前／修改後ID，新增頂部「還原上次AI修改前」。還原其他版本前亦保存當前畫布以便返回；重新載入的畫布不視為最後聊天版本。20版本／200對話上限、頁面與站台草稿隔離維持，本機還原不等於伺服器儲存。

沿既有sandbox/ai-provider-contracts擴充AiCanvasPlannerChecks：輸入語言選擇與短句延續傳遞、網站語系不變、目錄外框錯誤修正到catalog及runtime規則、非目錄／越界runtime選擇器拒絕、缺目的網址的單目標澄清。前端人工驗收：齒輪hover與鍵盤開啟／Esc／焦點返回、選版本不立即套用、重新整理且聊天保留後送新指令再還原修改前、手動修改後切版本與返回、20版上限、取消／失敗／跨頁隔離、目錄改4→6欄與換頁／重載卡片及桌機手機。未執行隔離檢測、編譯、bundle或瀏覽器。

使用者需手動編譯／重啟MVC（可能還原套件並寫入bin／obj），並在packages/EtheriT.Coker.GrapesJS執行npm run build:mvc；已確認package.json為vite build後copy-to-mvc，會更新dist與MVC產生資產，耗時依環境；本次未執行、不修改產生資產或資料庫。

## 2026-11-24 連結結構編輯與安全錯誤原因

截圖的上游拒絕尚未取得該次reason，不能認定為缺少設定。靜態修正CanvasAiPlanning：選取文字轉連結須走content並保留完整片段；未提供網址建立無href的可編輯a，不猜網址，摘要說明尚未設定目的網址。上游422僅保留CanvasAiClient已白名單映射的安全訊息，三次失敗後不再用通用錯誤蓋掉。

沿既有sandbox/ai-provider-contracts的AiCanvasPlannerChecks加入Span轉a且保留ID／文字／無href的服務斷言；登錄既有案例，不新增通過數。未執行編譯、隔離檢測、真實Gateway或瀏覽器；需人工驗證原句、提供網址、後續編輯網址、內容保留及實際導航，並重現上游拒絕核對具體安全原因。僅C#變更，需手動編譯／重啟，可能還原套件及寫入bin／obj，耗時依環境；不需bundle。

語系新建議（尚未實作，現行仍依下節）：明確回覆語言要求優先；充分且明確的使用者輸入可切換對話語言；短句、品牌、引用及混合語言不宜觸發切換，資訊不足沿用使用者既有語系或瀏覽器預設。中文區域偏好不能只由字形推斷；HTML文案仍依網站語系。不得把此建議視為已通過的功能。

# 後台功能檢測流程與維護規則

## 瀏覽器回覆語系與網站HTML內容語系分離（目前有效規格）

本節取代先前所有「摘要／澄清依網站語系」規格。前端每次請求送navigator.languages[0]／navigator.language為BrowserLocale；Controller以長度、語系標籤與CultureInfo檢查，建立回覆專用ReplyLocale。未取得有效偏好時依Accept-Language品質權重選有效語系，全部缺少才回退網站語系。WebsiteLocale仍僅由授權站台取得，瀏覽器不能覆寫網站內容語系。兩者明確放入模型上下文、規劃與問題審查。

ReplyLocale用於AI摘要與必要澄清，需區域詞彙、慣用語與句型，不做單純繁簡字形轉換；WebsiteLocale用於新增／改寫HTML標題、本文、caption、alt、title。未要求更動的原文、ID、class、URL不翻譯。規劃Gateway仍用網站語系處理HTML，最後獨立UI訊息校正呼叫改以ReplyLocale指定回覆語系，僅取顯示文字，不採它的HTML或操作。ApprovedPlan及當前需求是操作與數量依據，Source僅草稿，不得把六欄誤述四欄。

AiCanvasPlannerChecks沿原sandbox/ai-provider-contracts補明確瀏覽器偏好、Accept-Language品質排序與q=0排除、無效標籤回退、無偏好回退網站；新增台灣回覆／日文網站的實際服務隔離案例，固定模型日文HTML通過內容審查後UI回繁中，UI校正故意產出中文HTML仍只能改訊息、日文HTML不變。既有日文回覆案例改為明確ReplyLocale=ja-JP，其他案例明確傳入zh-tw回覆語系。使用者瀏覽器與網站語系不同的真實模型能力、區域用語及手機／權限站台隔離列人工驗收，不新增自動通過數。

修改來源為aiAssistantPlugin.js、CanvasAiController、CanvasAiService、CanvasAiPlanning與隔離案例／清單；未執行build、test、bundle或資料寫入。請於根目錄手動dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重啟MVC；於packages/EtheriT.Coker.GrapesJS手動npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。前者可能還原相依並產生bin/obj；後者產生dist及複製MVC資產，可能修改多個產物，耗時依環境。尚未執行隔離案例或實頁驗收，不宣稱跨語系已通過。

## 連續欄數切換通過但摘要語系失守

2026-11-24內建瀏覽器續測同一品牌列：四欄4／4／4、六欄6／6、改回四欄4／4／4皆實際生效，原12圖片src與12連結href及順序保留。使用者指出回覆簡中，已讀取當次聊天確認前兩則為「将当前布局调整为每行四个项目。」「将每行显示的项目数调整为6个。」，第三則才為繁中；DOM lang及內嵌siteLocale均zh-tw，確定不是網站設定遺失。此輪排版成功不能宣稱整體語系通過。

缺口是樣式計畫驗證成功後直接回傳summary，語系僅受規劃提示約束。新增固定一次UI訊息獨立校正，傳遞WebsiteLocale與來源訊息、保留已驗證ApprovedPlan供既有Gateway協定使用；移除History避免舊簡中對話成為語系基準，只採用校正後summary／clarification，絕不採用其新增或變更的操作。原最多3次規劃＋1次訊息校正，合計最多4次HTTP模型呼叫，沿原整體逾時及取消。校正服務／格式失敗不回可套用計畫，不顯示未校正摘要。

AiCanvasPlannerChecks補簡中來源→固定繁中校正、zh-tw傳遞與History清空、校正回覆故意含#outside操作仍僅採原操作、校正無效JSON不套用；既有成功呼叫數同步包含校正，原id／executor不變。人工清單補每次四→六→四的文字及必要問題驗收。新增校正未編譯、未執行隔離案例、未實際模型驗收；不能保證只靠新提示就已修好實際語系。

本次只改後端／檢測規格，需手動於根目錄dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重新執行MVC，可能還原相依並寫bin/obj，耗時依環境。本次不執行、不需bundle；現有未儲存四欄畫面保留。

## 四欄原句成功、六欄後續被 Gateway 澄清欄位驗證拒絕

本次使用者編譯後，內建瀏覽器「換成四個一排」實際成功：Grid四軌239px、12子項4／4／4且寬度均239px，12圖片src與12連結href及順序與修改前完全相同。續送「6個一排」失敗，開發診斷實際reason為「模型以解說代替必要的澄清問題」（Gateway ClarificationNotQuestion）；原四欄仍完整。已取得與使用者截圖同類provider-validation，不再將單次四欄成功當穩定完成。

CanvasAiClient改用畫布專用回覆型別保留已知FailureReason；ReadCanvasFailureDetails沿既有4096-byte上限與固定錯誤白名單取reason，不暴露供應商message／本文，原ReadCanvasFailure字串入口保留相容。規劃流程以結構化ClarificationNotQuestion分類，回饋明確欄位修正：clarification留空、說明放summary、回傳當前需求與當前畫布的操作，不要求客戶重述；三次預算、權限及不可破壞原圖防護維持。不解析客戶語言或錯誤翻譯文字來分派。

BackofficeSandboxAiGenericChecks沿原sandbox/ai-provider-contracts新增已觀察Gateway錯誤reason傳遞、原始供應商假秘密不洩漏、修正指令包含summary與CURRENT；人工清單補四→六→四連續實頁驗證。新增修正尚未編譯／執行隔離案例，六欄仍未驗收成功；目前內建頁面保留未儲存的四欄，未儲存／發布。

本次只改後端／檢測規格，需於根目錄手動dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重新執行MVC；可能還原相依並寫bin/obj，耗時依環境。本次不執行、不需因本次修正重建前端。

## 不同說法重現未生效的 Grid 計畫

使用者回報「換成四個一排」遭上游拒絕，本次內建瀏覽器用同句重現卻得到成功訊息；DOM量測顯示display:flex、grid-template-columns雖為repeat(4,...)但12項仍6／6且各159px。已確認另一個真實不穩定情境：軌道設定缺少Grid display相依，不可將前次單一成功當整體穩定。已重載還原未儲存結果，未儲存／發布。使用者截圖的上游拒絕本次未重現，仍不可宣稱已解決。

CSS適配依結構化操作補齊缺少的display:grid，再沿既有Bootstrap子項寬度釋放；若計畫明確指定display:flex等非Grid模式卻同時設定Grid軌道，回交AI修正，不覆寫其明確選擇。none／initial／unset／inherit／revert不強制啟用Grid。前端套用後對目前修改子樹的Grid目標檢查實際computed display，未生效則沿快照回滾，不能只憑CSS內容不同顯示成功。

開發診斷另加Reason，取已安全分類的驗證／上游錯誤翻譯（最多1000字），不放供應商原文、HTML或需求，讓provider-validation不再只有泛分類。AiCanvasPlannerChecks沿原executor增加實際缺display計畫一次成功、13條規則與原HTML保留，以及明確矛盾三次拒絕且Reason完整。人工清單補computed結果與使用者上游失敗原因。新增案例未執行、修正未編譯或實頁驗收；既有桌面單次成功紀錄仍只代表該次觀察。

需手動於根目錄dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重啟；在packages/EtheriT.Coker.GrapesJS執行npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原並寫bin/obj；前端產生dist並複製MVC資產，可能修改多個產物，耗時依環境。本次不執行、不改生成檔。

## 四欄桌面實頁驗證結果

本次使用者編譯後，在已登入內建瀏覽器重新載入站台49／canvas11019，以實際品牌列送出「調整為四個一排」。本次模型回覆「將容器調整為四個一排」，實際DOM驗證display:grid，四軌各239px；12個直接子項top座標分組為4／4／4，各項寬度均239px，視覺確認品牌圖正常，不再縮成40px細條。逐一比較前後12張圖片src及12個連結href，內容與順序完全一致。

這次桌面瀏覽器情境已實際驗證目標別名與Bootstrap→Grid欄寬適配；不能擴張宣稱所有AI檢測通過。未驗證手機、其他語言、四→五→六連續切換、保存重開、SQL寫入或自動隔離案例。未儲存／發布頁面，保留目前未儲存的四欄畫面供使用者檢查；AI面板已關閉以顯示完整品牌列。驗證截圖保存於本次工作階段visualizations目錄，不新增自動通過數，不改既有案例ID或executor。本次僅更新驗證記錄，沒有新增程式碼或重新建置。

## 實頁定位成功後的 Bootstrap 子項二次縮放

本次內建瀏覽器確認目標別名修正後「調整為四個一排」成功，容器實際Grid四軌各239px，12項分三排，原12張圖片與12個連結順序及網址完全保留。然而子項仍col-lg-2，寬度成每個Grid格的1/6約40px，畫面變細條，不能視為完成。已重新載入原頁還原未儲存結果，確認恢復Flex與每項159px；未儲存或發布。

新增框架套用適配AdaptBootstrapGridItems：當已驗證計畫明確把容器改為grid／inline-grid且指定grid-template-columns時，對其已有ID的直接Bootstrap col子項補width:auto、max-width:100%、min-width:0，釋放Flex欄寬在Grid的殘留效果。只補模型未明確指定的屬性，不改class、gutter、HTML、圖片或連結，不根據使用者句子判斷。新增規則仍沿100項／CSS安全校驗。

AiCanvasPlannerChecks沿既有sandbox/ai-provider-contracts補實際根容器別名Grid計畫、12個col-lg-2子項釋放繼承寬度、13條規則、一次呼叫與HTML不變斷言；人工清單補實際圖片寬度，不能只看欄數或成功摘要。新增適配尚未編譯、執行隔離案例或瀏覽器驗收；本次只確認定位修正已生效及視覺衝突可重現。

本次只改後端與檢測規格，手動於根目錄dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重新執行MVC；可能還原相依並寫bin/obj，耗時依環境。本次不執行、不需bundle，登入頁面保留供後續確認完整四欄視覺。

## 已取得模型實際未命中名稱與明確目標別名

本次建置後內建瀏覽器實際重現，Development／Local診斷已可讀：當前選取ihpg4且availableIds有完整49個品牌元件，第一個計畫卻用#container、第三個用#layout-container，分類均targets-unmatched。已確認這次不是XPath計算，而是模型使用未宣告名稱；未儲存頁面。

新增TargetAliases作為API明確契約：只有唯一且具唯一ID的fragment根才宣告#container／#layout-container，#selected對應實際點選元件。若片段內已有相同真實ID，不宣告該別名；多根不猜根。計畫驗證前只將已宣告的完整ID token轉回真實ID，可搭配後代／子代選擇器，再走原normalizer、範圍與CSS驗證；其他未知ID仍拒絕。不以需求關鍵字分派、不任意將所有錯誤目標改成根、不改HTML／圖片／metadata。

AiCanvasPlannerChecks沿sandbox/ai-provider-contracts新增實際觀察到的兩種名稱，斷言TargetAliases宣告、根與兩子項精確映射、一次成功且HTML完全相同；補同名真實ID與多根不宣告根別名。尚未編譯或執行新增案例；瀏覽器只確認原始失敗計畫，修正尚待使用者編譯重啟後驗收。

本次只改後端／檢測規格，手動於根目錄dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"並重新執行MVC；可能還原相依並寫bin/obj，耗時依環境。本次不執行、不需bundle。內建登入頁面保留供後續確認實際四欄結果。

## Local 啟動環境的診斷可見性

重啟後內建瀏覽器重現同一未命中，且唯讀確認Razor已載入console診斷程式，但沒有後端診斷輸出。檢查launchSettings確認本專案另有ASPNETCORE_ENVIRONMENT=Local；先前僅允許Development，漏掉此啟動profile。補Local且RemoteIpAddress為IPv4／IPv6 loopback時允許，Local遠端／缺IP與Production（含本機）仍不允許。失敗回應附diagnosticsEnabled布林，Razor在沒有計畫資料時也輸出availability，避免再次無法區分環境關閉或零筆資料。此環境缺口不代表已確認模型selector未命中的根因。

AiCanvasPlannerChecks沿既有executor補Development、Local兩種loopback、Local遠端／null、Production loopback六項閘門斷言；人工驗收確認兩種啟動profile可見性與正式環境無資料。尚未編譯或執行上述案例。本次瀏覽器只重現失敗，未儲存頁面。

需手動重新編譯並重新執行MVC，於根目錄dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"。可能還原相依並寫bin/obj，耗時依環境。本次不執行、不需bundle，內建登入頁面已保留供後續讀取實際selector。

## 實頁重現與開發用 selector 比對

本次已在內建瀏覽器登入後，以實際站台49／canvas11019選取品牌列重現「調整為四個一排」未命中錯誤，未儲存頁面。唯讀確認root為iqprv、row g-3 text-center brand-img，12個直接div子項皆col-4 col-md-3 col-lg-2，含既有連結metadata與圖片。這不是少資料或單純選錯層；但既有日誌無實際selector且本機沒有持久化日誌，尚未取得模型計畫，未定位根因。

新增最多三筆記憶體PlanDiagnostics，僅含attempt/category/SelectedId/最多100個當前ID及最多100個經字元／長度限制的selector；不含HTML、需求、供應商本文或金鑰，JsonIgnore禁止作為客戶端請求欄位輸入。Controller只有Development且失敗才回傳，其他情況null；MVC adapter只寫開發console、不進使用者聊天畫面。登入、站台與編輯權限檢查不變，不新增公開診斷端點或持久化檔案。

既有AiCanvasPlannerChecks補三次失敗保存實際安全selector／當前ID、request序列化排除的斷言。人工驗收需開發環境失敗console可讀、正式環境與成功回應不含資料，以及原畫布／圖片不變。案例未執行；瀏覽器本次僅重現失敗，不能算修復驗收通過。

需使用者手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj" 並更新執行中的後端，再以已登入內建頁面繼續重現讀取console診斷。編譯可能還原相依並產生bin/obj，耗時依環境。本次不執行build、restore、bundle、不儲存頁面；Razor adapter變更不需因本次修改重建GrapesJS資產。

## 整頁祖先路徑與選取片段定位

定位器原先直接將整條 selector 套到選取片段，body／外層container等祖先不在fragment中時，即使尾端 #gallery／.imageList 與子項真實存在仍回傳未命中。新增受限重定位：僅當完整 selector 未命中，且可移除完全不在片段中的前綴，剩餘路徑以現存唯一 ID 或唯一fragment根為錨點時才解析；不搜尋整頁、不猜近似ID、不跳過已命中局部節點後的錯誤路徑。原 specificity 保留，最終仍轉單一既有ID並沿既有CSS白名單驗證。

AiCanvasPlannerChecks 沿 sandbox/ai-provider-contracts 增加 body #gallery > a、.page-wrapper .imageList > a、#page-outside #gallery > a 三種片段外祖先路徑，一次成功、僅兩子項且原HTML／metadata不變。這是定位器具體缺口，仍無本次實際模型 selector，不能確認截圖就是此原因。尚未執行編譯、案例、真實模型或瀏覽器。

本次後端與檢測規格變更，手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"；可能還原套件並寫bin/obj，耗時依環境。本次不執行、不需因這次修改重建前端。

## 元件定位失敗細分與片段 scope

CanvasAiStyleNormalizer 新增診斷 overload，保留舊呼叫相容性。未命中、選擇器語法、重複ID、規則格式、規則數及命中數上限各自回傳固定分類；ValidatePlan 將具體修正方向交給AI，最後畫面顯示易懂原因，不暴露原始selector、ID或路徑。伺服器既有安全日誌亦可區分，而非全部歸類targets。這僅修正可觀察性，沒有本次實際模型回覆／日誌，不能斷定截圖是哪一類。

確認原轉換器不支援合法 :scope，補上片段根容器的 :scope > a 等定位，只在目前fragment內解析並轉成既有ID規則，不擴大到整頁。其他動態偽類仍拒絕。

既有 AiCanvasPlannerChecks／sandbox/ai-provider-contracts 新增未命中與動態偽類各三次拒絕後不同訊息，以及 :scope > a 一次成功、只命中兩個既有子項、HTML不重建的斷言。人工清單補五類錯誤核對。保留ID／executor，未執行隔離案例、編譯、瀏覽器或真實模型。

本次只改後端與檢測規格，請手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"；可能還原相依並寫入bin/obj，耗時依環境。本次不執行、不需因這次修改重建前端。

## 當前快照優先與有依據的澄清

移除「審查過一次後所有澄清皆直接回傳」的缺口。當前需求／SelectedId／HTML／EditorStructure／CSS／computed style 優先，History 只補充需求意圖；舊助手提出的圖片／文字、置中或寬度問題不是事實，不可重新開啟已被目前資料解決的問題。

客戶可見的目標澄清需附 clarificationTargets（2～8個既有唯一 ID），由程式驗證彼此獨立、不是祖先／子孫、不是已由 SelectedId 指向其中一組的選擇。未提供依據或已被目前選取解決則內部繼續規劃，三次耗盡不顯示無依據問題、不套用內容。維持既有 Gateway 欄位；clarificationTargets 為可選澄清證據，若上游未保留該欄位則不會對客戶顯示未驗證問題。此版本可顯示的澄清限有結構證據的目標選擇，其他設計細節由 AI 沿用目前設定，不宣稱能判定所有語意歧義。

AiCanvasPlannerChecks 沿原 sandbox/ai-provider-contracts 增加舊助手問題＋現有圖片快照、連問兩次後改操作、連問三次不顯示、兩個獨立既有目標允許澄清、已點選其中一組圖片後不再詢問；人工清單增加改選／還原後以最新資料判斷。固定回覆僅驗證流程，未執行案例、編譯或真實模型／瀏覽器，不宣稱實際模型已成功套用。

本次只改後端與檢測規格，請手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，可能還原相依並寫入bin/obj，耗時依環境；本次不執行，也不需因這次修正再產生前端資產。

## 框架與實際排列結構進入規劃上下文

確認先前 selected 通用提示未攜帶非 selected 提示中的 Bootstrap 說明。新增 EditorStructure，明列 GrapesJS、專案 package.json 確認的 Bootstrap 5.3、Coker imageList／imageItem、Bootstrap 斷點，並依 HTML 結構盤點 row／相簿／重複圖片容器、父層、直接子項 ID／數量／class／圖片數／隱藏狀態；最多8容器、每組48項並標記截斷。此盤點不讀需求關鍵字，也不直接決定客戶操作。前端樣式參考另附實際 componentType／parentSelector，仍受原預算限制。

AI 規劃與問題審查必須以結構為基準，Bootstrap row／col／row-cols 及 gutters 是既有框架設定，目前像素欄寬是可修改的基準，不可誤認容器放不下。指定四欄已授權調整子項寬度，保留容器並換行；不能再問保留現有寬度或自動調整。既有契約以 scoped CSS 表達相容欄寬，避免為換 class 重建自訂元件；未新增不相容 Gateway 欄位。

既有 AiCanvasPlannerChecks／sandbox/ai-provider-contracts 補上12品牌 Bootstrap row／row-cols-lg-6／col-lg-2／g-3，第一回覆誤問容器寬度不足、第二回覆固定子項25%計畫，斷言框架／數量／原class均送達、12項規則、原HTML完整且兩次呼叫。人工清單補桌面三排四個、原容器／間距／手機斷點保留及相簿。固定回覆僅驗證契約，不代表模型結構理解、視覺或實際站台已通過；未執行編譯、隔離案例、瀏覽器或真實模型。

請手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原相依並寫bin/obj；前端產生dist及複製MVC資產，可能修改多個產物，耗時依環境。本次不執行、不改生成檔。

## 無動作的驗證診斷與合法 CSS 運算

靜態追查確認前後端 CSS 值白名單拒絕乘號，會擋下依既有間距計算等寬的合法 calc((100% - 3 * 16px) / 4)。兩端同步允許乘法字元，原分號／URL／expression／javascript／import 防護不變。不以此宣稱已確認截圖的實際模型失敗原因。

規劃提示由空陣列模板改成含可執行樣式項目的格式範例，仍由 AI 選取真實目標和值，不從需求正則分派。修正請求累積 ValidationFailures，避免只帶最後一種錯誤造成來回失敗。服務新增可選 ILogger，拒絕時記錄 attempt、provider、category 與已校驗的固定原因，不記錄 HTML、需求、RejectedProposal、金鑰或原始供應商本文。用戶錯誤區分空計畫、目標、CSS、格式與上游拒絕，不再把執行失敗推給客戶補充需求；開發分類只留日誌。

既有 AiCanvasPlannerChecks／sandbox/ai-provider-contracts 新增合法乘法計畫一次成功、三次空計畫拒絕且不要求補充，以及第三次修正包含前兩次失敗的斷言。人工清單增加四欄原句與伺服器拒絕分類核對。未執行隔離案例、編譯、真實模型或瀏覽器；缺少實際請求與日誌，不能宣稱截圖問題已重現或修復驗收通過。

請手動於根目錄 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原套件並寫 bin/obj；前端產生 dist 並複製 MVC 資產，可能修改多個產物，耗時依環境。本次不執行、不改生成檔。

## 網站語系與可推斷細節不詢問客戶（目前澄清流程）

本節更新下節「澄清直接回使用者」行為。SelectedInstructions 明定所有 summary／clarification 使用授權網站 WebsiteLocale，含區域及繁簡變體；每次模型呼叫再附權威語系，不跟隨需求語言或歷史簡中回覆。未指定寬度、間距、排列方向、圖片比例與斷點時，沿用現有 CSS、computed style、元件結構及視覺慣例；必要時選保守相容預設。每排數量已明確就足以規劃，不要求客戶提供像素寬度、間距或 CSS 選擇。

首次澄清不直接顯示，將完整 ReviewProposal、原上下文、網站語系與「先推斷既有設定」的具體回饋交回 AI 審查；能完成就回可執行計畫，仍有真實且無法推斷的客戶可見選擇才顯示簡單問題。仍沿三次總呼叫預算；審查不是判斷中文關鍵字，也不新增語言白名單。前端補收 row/column gap、margin、padding、box-sizing、object-fit 與 aspect-ratio，保留 40 元件／16000 字元 JSON 預算。

既有 AiCanvasPlannerChecks／sandbox/ai-provider-contracts 新增截圖原句＋簡中技術問題草稿，斷言第二次送回語系、完整問題、實際間距後取得固定四欄計畫、不顯示草稿問題；真實兩組圖片選擇經第二次審查仍可回問；日文網站中文需求仍帶 ja-JP 並傳回固定日文摘要。沿案例 ID／executor，不新增自動通過數。固定模型回覆僅驗證流程與契約，不代表真實模型語系／推斷能力通過；尚未執行隔離案例、編譯或瀏覽器。

請手動於根目錄執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原套件並寫入 bin/obj；前端產生 dist 並複製 MVC 資產，可能改動多個產物，耗時依環境。本次不執行、不修改生成檔。

## AI 規劃與驗證取代語言關鍵字分派（目前有效流程）

本節取代下方歷次記錄的中文欄數／放大／連結捷徑與零模型預期。CanvasAiService 移除所有以需求文字正則分派的局部處理；保留既有 Gateway editMode／styles／textStyles／html／clarification 契約作為可驗證編輯計畫。所有需求原文與近期對話交 AI 理解，站台語系控制新增文案，不限制輸入語言。新增 CanvasAiPlanning partial 統一規劃、驗證與最多三次總模型呼叫（內容修改另需一次 AI 核對，也計入三次），每次重設 PreserveContent 並以原 HTML 為基準。校驗失敗回傳具體 ValidationFeedback 與可容納的失敗計畫；澄清直接回使用者，429／權限／網路／服務失敗不自動重送。模型呼叫數累計，未知次數不猜測。

樣式操作不重建 HTML；無效 selector／CSS、空計畫、與已知 computed style 完全相同的計畫與遺失圖片的內容回覆皆不套用。內容替換目前要求原圖片（含重複圖片／source）保留；移除或換掉原圖片的能力仍受保護限制，不能宣稱已支援所有編輯動作。內容審查的 ReviewProposal 完整保留於預算內，無法容納時拒絕，不假稱已審查。

前端不再從需求句子判斷欄數／字級或是否擴大範圍；以渲染結構提供最多四層內最近的排列容器作編輯邊界，SelectedId 保留實際點選子元件，交 AI 決定最小目標。未選取／wrapper 不送出；原 ID 範圍、大小限制、取消、等待變更、權限與站台隔離維持。class-only 回覆原地更新，不以語言判定；替換前後及實際套用後檢查圖片完整，失敗沿快照回滾。不宣稱只檢查圖片數即可驗證排版效果。

既有 AiCanvasEdits 保留輸入邊界、JSON 總預算與 selector 正規化，改呼叫 BackofficeSandboxAiPlannerChecks：六種輸入原文與 SelectedId／locale 傳遞、固定樣式計畫保留自訂清單、區塊外修正、JSON／CSS 三次內修正、刪圖三次拒絕、澄清一次、429 一次與內容額外審查。沿 sandbox/ai-provider-contracts 原 ID／executor，不新增通過數。這是純記憶體 HTTP 回覆與獨立 InMemory fixture，不連真實模型／客戶資料庫；不能據此宣稱日文／越文模型理解、SQL 或瀏覽器通過。shared-ai-regression 與片語人工規格已重寫，不再要求中文捷徑零模型。舊記錄保留作變更歷史。

尚未執行編譯、隔離案例、瀏覽器或真實模型。使用者需於根目錄手動 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，並於 packages/EtheriT.Coker.GrapesJS 手動 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原相依並寫入 bin/obj；前端產生 dist 並複製 MVC 模組資產，可能修改多個產物，耗時依環境。本次不執行、不修改生成檔。

## 五個一排與一般欄數

先前欄數解析僅允許二、三、四、六，因此截圖的 5個一排／修改為5個一排落入模型後被內容保護拒絕。改為辨識 1～12 的數字及中文，超出範圍或數字溢位明確拒絕且不呼叫模型。Bootstrap 無法平均分配十二格的欄數（如五、七）改走 scoped Grid 純樣式；已有 Grid 時後續六／四欄也更新 Grid，避免只改 Bootstrap class 卻殘留舊 Grid。圖片清單沿原純 CSS 路徑保留 HTML 與元件。

既有 AiCanvasEdits／sandbox/ai-provider-contracts 補上五欄原句、中文五與其他 1～12 欄數、0／13／超大數字拒絕，以及 Bootstrap Grid 五→六→四的規則斷言。shared-ai-regression 補 12 圖五欄五、五、二的視覺驗收與再切換六／四，沿用案例 ID 與 executor。未執行隔離案例、編譯或瀏覽器，不能視為畫布已通過。

本次不改前端，請於根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"；可能還原相依並產生 bin/obj，耗時依環境。本次不執行 build、test 或 bundle。

## 四欄後以短句改回六欄

截圖的「6個一排」未包含原辨識要求的修改動詞，落入模型 HTML 流程後被內容保護拒絕。明確欄數解析現接受省略動詞的數字／中文短句，例如 6個一排、六個一排、4個一排、四個一排，並支援請幫我改成等前綴；仍以整句匹配，複合需求不會只執行部分指令。沿既有純 CSS 圖片清單流程覆寫同一 #id 的 grid-template-columns，原圖、連結與 HTML 不變。

既有 AiCanvasEdits／sandbox/ai-provider-contracts 增加四欄 CSS 與成功對話存在時，五種短句／前綴各自回傳指定四或六欄、零模型、46 條 scoped 規則與原 HTML 不變的斷言。shared-ai-regression 補 12 品牌四→六→四及還原後再次修改的人工驗收。沿用 ID／executor，未執行隔離案例或瀏覽器，不宣稱畫面已通過。

本次只有後端與檢測規格變更，不需因這次修正再產生前端資產。請於根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，可能還原相依並產生 bin/obj，耗時依環境。本次未執行編譯、測試、bundle。

## 圖片清單直接修改每排數量

精確欄數指令除 Bootstrap 外，新增圖片清單純 CSS 路徑：在選取片段定位最內層、直接子項皆含圖片的唯一重複項目容器，改 grid 欄數與項目寬度，HTML 完全不變。自訂 data-*、原圖、連結、子元件及標題均保留，不送模型重寫 HTML，也不因既有 data-* 被安全 HTML 重建流程拒絕。多組候選仍澄清；缺少可用 ID 明確拒絕，不回退重建。hidden／d-none／backstageType 樣板不納入項目。Bootstrap 遇既有自訂屬性亦可改走此路徑。

前端 ID 準備涵蓋整個選取片段，與最多 40 個 computed style 預算分離，避免圖片項目超過 40 時後段不能定位。純 CSS 路徑沿既有 scoped ID 與最多 100 條安全規則檢查；目前圖片清單指定欄數會作用於所有螢幕寬度，不宣稱保留原手機欄數。

AiCanvasEdits／既有 sandbox/ai-provider-contracts 新增 45 圖、data-*、外層標題、零模型、原 HTML 完整相同、46 規則與末項定位、缺 ID 拒絕斷言；原破壞性模型回覆案例改用仍走模型的欄數說法，繼續檢查刪圖防護。shared-ai-regression 補上相簿、一般卡片、超過 40 項、手機、還原與保存重開。案例 ID／executor 不變，尚未執行隔離斷言或瀏覽器。

請手動編譯 MVC，並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原相依、寫入 bin/obj；前端產生 dist 並複製 MVC 資產，可能修改多個產物，耗時依環境。本次不執行編譯、測試、bundle，不修改生成檔。

## 排版成功卻清空純圖片品牌列

截圖顯示成功後品牌列空白。靜態追查確認原 content 回覆可整段替換，文字檢查不能保護沒有文字的圖片；尚無實際請求／回覆可判定是模型刪圖或 GrapesJS 重建自訂元件造成。排版需求現由後端比對完整節點結構、深度、非 class 屬性、圖片／連結網址、ID、重複圖片與文字，僅允許 class／CSS 調整。前端套用前再比對 DOM，將 class 更新至既有元件，不刪除重建元件樹，保留自訂類型與子層；定位失敗不套用。取消與原有回滾流程維持。

AiCanvasEdits 沿既有 sandbox/ai-provider-contracts 增加純圖片空容器、刪一張相同網址圖片、改圖址、改連結四種實際服務拒絕斷言，以及完整樹只改 class 可通過。shared-ai-regression 人工驗收補元件實例、子層及自訂資料保留、錯誤沒有成功版本、設定選修改前版本還原。沿用案例 ID；尚未執行隔離斷言、編譯或瀏覽器，不宣稱已重現實際清空根因或通過視覺驗收。

手動重新編譯 MVC（dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"）並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原套件並寫入 bin/obj；前端產生 dist 並複製 MVC 模組資產，可能修改多個產物，耗時依環境。本次不執行、不修改生成檔；先由設定還原到修改前版本再驗收，不儲存空白結果。

## 畫布排版參考資料合併超限

最新截圖的「畫布或對話太長」來自 CanvasAiService 合併模型 context 後的 32000 字元檢查；新增 CSS／排版參考與重複 SelectedTarget 文字後，個別合法欄位仍可能合計超限。模型訊息內層 JSON 改用保留中文的編碼，避免中文及 HTML 字元跳脫造成膨脹；此字串只作模型輸入，不插入網頁，外層 HTTP JSON 仍沿原序列化流程。

實際序列化後超限時，先省略可選 CSS 並標記截斷，再省略 HTML 中已有的 SelectedTarget 重複文字，最後由尾端移除完整樣式參考項目，至少保留第一筆。完整 HTML、目前指令、AllowedSelectors、屬性限制與近期對話都不截斷；必要資料仍超限時回傳明確的小區塊提示，不宣稱清除對話即可解決。不提高 Gateway 限制、不自動重送、不改權限或站台範圍。

既有 sandbox/ai-provider-contracts 的 AiCanvasEdits 增加合併超限精簡、中文不膨脹、原始 HTML／指令／selector／近期澄清對話完整、樣式陣列有效且保留第一筆，以及必要內容超限拒絕的實際 helper 斷言。shared-ai-regression 增加截圖原句與原有對話的人工回歸。維持案例 ID／executor；未執行隔離斷言、編譯、瀏覽器或真實模型，不能宣稱畫布已驗證通過。

本次只改後端與檢測規格，不需因這次修改重建前端。請於根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj" 確認編譯；可能還原相依並產生 bin/obj，耗時依環境且可能較長。本次未執行。

## 畫布選取層級與排版子層

選取片段是可編輯邊界，不再暗示只能調整根節點。明確欄數指令可定位外層容器內唯一 Bootstrap row，保留其他標題與說明；多組 row 回傳澄清且不套用。前端僅對每排／欄數意圖，在所選片段沒有 row／grid／橫向 flex 排列時，最多向上四層尋找最近排列容器，不能選到整頁 wrapper；定位後在畫布顯示實際目標，一般文字或圖片需求不擴大。原取消、站台／資源與等待期間變更檢查繼續綁定這個實際目標。

computed style 補上寬度、grid、flex、間距與對齊，先收集直接子層再收集深層，保留最多 40 元件及 16000 字元有效 JSON 預算。selected 請求提供最多 8000 字元既有 CSS 作為參考並標記截斷；仍不得把 class 規則套到區塊外，整體請求大小限制維持。

既有 AiCanvasEdits／sandbox/ai-provider-contracts 補上外層包住唯一品牌列時調整子層且保留標題／說明、多組排列澄清且零修改的執行斷言。shared-ai-regression 人工規格補上選取外層、row、內層圖片／連結、四層與 wrapper 邊界、文字改色不擴大、排版參考大小、取消／還原及權限站台隔離。保留案例 ID；前端人工規格不列自動檢測數。上述隔離斷言、瀏覽器與模型驗收均未執行。

使用者需手動編譯 MVC，並於 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc（已確認為 vite build && node scripts/copy-to-mvc.mjs）。編譯可能還原相依並產生 bin/obj；前端流程產生 dist 並複製 MVC min.js／CSS 等資產，可能修改多個產物且耗時依環境。本次不執行 build、test、bundle，也不修改生成檔。

## 品牌畫布四個一排未提出修改

截圖的 EmptyProposal 是服務未提出修改，與輸入大小驗證不同。selected 提示補上明確欄數是可執行排版需求、完整 HTML 與圖片／連結保留規則。對精確的「修改為四個一排」「幫我改為四個一排」等欄數指令，已知 Bootstrap row 的直接 col 子項由本機調整 md 與更大斷點欄寬；保留手機欄數、ID、其他 class、圖片網址、連結與順序，不呼叫模型。未知容器、複合需求及不安全 HTML 不走局部捷徑，維持模型與原驗證流程。

既有 sandbox/ai-provider-contracts 的 AiCanvasEdits 增加兩個原句、12 品牌完整保留、桌面四欄與手機兩欄、零模型呼叫，以及複合需求／未知容器交模型的隔離斷言。沿用案例 ID 與 executor；shared-ai-regression 補上桌面三排四個、手機、連結、還原、唯讀／跨站與等待期間修改的人工驗收。未執行隔離案例、編譯、瀏覽器或真實模型，不代表四欄視覺結果已通過。

本次只變更後端來源與檢測規格，無新增前端資產變更。請於根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"，確認 C# 編譯；可能還原相依並寫入 bin/obj，耗時依環境且可能較長。本次不執行。

## 選取容器排版請求的輸入大小

「指令或畫布內容不符合限制」是 Chat 在模型呼叫前的欄位驗證，不代表模型拒絕四欄指令。插件收集最多 40 個元件的 computed style 時，新增序列化總長 16000 字元預算，省略空白及超長單一值，參考資料達預算就停止收集，不截斷 JSON，也不截斷選取 HTML。HTML／CSS 超限在送出前明確提示；Chat 保留既有大小與權限限制，拆分為內容、樣式、對話、樣式參考及無效欄位提示。

既有 sandbox/ai-provider-contracts 的 AiCanvasEdits 新增四個一排合法請求、樣式參考邊界及超限、HTML／CSS／對話超限分類斷言，不連模型或客戶資料庫。shared-ai-regression 人工規格補上含 40 個以上子元件的品牌容器排四欄、檢查請求樣式參考為有效且不超限 JSON、只影響所選容器；未執行隔離檢測、編譯或瀏覽器驗收，不宣稱四欄結果已通過。

需手動重新編譯 MVC 及在 packages/EtheriT.Coker.GrapesJS 執行 npm run build:mvc；編譯可能還原相依並產生 bin/obj，前端流程產生 dist 及複製 MVC 資產，可能修改多個產物，耗時依環境。本次不執行上述流程。

## AI 設定工具列顯示回歸

AI 面板標頭改用專屬 div 工具列，避免宿主針對頁面 header 的樣式或顯示控制影響設定／關閉按鈕；以面板直屬 CSS 明確設定正常定位與不壓縮高度。更新 shared-ai-regression 人工規格，核對編輯器一般／全螢幕及窄螢幕下標題、設定與關閉均可見；設定可展開版本與清除入口，長對話捲動不帶走工具列。未執行瀏覽器驗收或 bundle，不能視為通過；需依下方既有 build:mvc 流程手動更新資產。

## 畫布 AI 聊天與本機版本

右側改為聊天、底部輸入與目前選取提示，移除修改範圍及獨立版本工具列；送出固定 selected、autoDetect=true，只處理送出時選取元件，未選取或選到 wrapper 不送出。保留 HTML/CSS 校驗、取消、內容變更與跨資源防護。成功套用的回覆以穩定版本 ID 綁定「還原此版」；設定內保留版本時間／需求摘要選擇器及「清除本機內容」。還原會新增對話紀錄，不自動發布或保存頁面。

既有資源識別鍵保存最多 20 份版本與 200 則對話，舊版僅有版本的資料仍可從設定還原；送往服務只包含最近最多八則 user／assistant 及有效的 8000 字元內 JSON，不送版本資料。清除先確認，只移除目前資源的本機版本及對話，保持畫布與伺服器內容；取消不清除。版本淘汰後舊對話不提供失效還原按鈕。

更新 shared-ai-regression 人工驗收規格：選取／未選取、送出即顯示、成功版本連結、錯誤與取消無成功版本、設定展開、舊紀錄相容、重開與跨頁隔離、兩種還原入口、未存修改確認、清除確認及窄螢幕捲動。保留既有案例 ID，不新增自動通過數；未執行瀏覽器、build、test 或 bundle。修改只涉及插件 JS/CSS 及檢測文件，不新增後端或資料庫操作。

需使用者於 packages/EtheriT.Coker.GrapesJS 手動執行 npm run build:mvc（vite build && node scripts/copy-to-mvc.mjs），產生 dist 並複製 MVC 模組資產，可能修改多個產物且耗時依環境而定；本次不執行，也不修改既有生成檔。

## AI 設定選單預設不開放

NavigationProvider 的 AiSettings 選單預設 Enable=false；沿用既有網站功能權限，只有本站明確授予 AiSettings 才能開放，再依使用者角色判斷檢視與編輯。單獨授予 AiSettings.View 或系統維護身分不能開啟停用的網站功能；既有明確開放的網站維持可用。不變更頁內 AiEnabled 開關、金鑰或 JavaScript，不新增資料庫變更。

既有 sandbox/ai-settings 執行器新增隔離斷言：無網站功能授權時不可見及不可編輯、使用者 View 授權不能開放、本站明確開放後可用、撤銷後再次不可用。權限資料只寫入獨立 InMemory fixture。人工驗收另確認選單、直接頁面及相關 API 均遵守網站功能與角色權限，其他站台不受影響。未執行編譯、檢測或瀏覽器驗收，不代表已通過。這次選單修正不需重建前端資產。

## 畫布修改方式統一自動判斷

移除 aiAssistantPlugin 的「修改方式」選單，使用者只選修改範圍並描述需求。selected／canvas 請求固定 autoDetect=true，初始 preserveContent=true；既有模型結果與後端局部處理決定實際修改方式，前端仍檢查後端回傳 preserveContent。new 範圍仍為 autoDetect=false／preserveContent=false，用於新建內容。保留後端欄位以相容既有客戶端，不新增權限或改變站台隔離，原文保留與 HTML/CSS 套用檢查維持。

更新 shared-ai-regression 人工規格：畫面沒有修改方式選項，selected／canvas／new 各範圍的參數正確；局部放大＋連結、純樣式、明確改寫及新建內容均能沿既有流程處理。取消、等待期間變更、權限／跨站拒絕與本機版本還原不變。這是待執行的瀏覽器驗收，不新增自動案例或通過數；本次未執行編譯、測試或 bundle。

需使用者於 packages/EtheriT.Coker.GrapesJS 手動執行 npm run build:mvc，實際為 vite build && node scripts/copy-to-mvc.mjs，會產生 dist 並 minify／複製 MVC 的模組 JS、CSS 等資產，可能改動多個產物，耗時依環境而定。本次不自行執行，不以 MVC 的 create-bundles 替代此套件流程。

## 局部放大與連結不得刪除未指定文字

修正使用者原句「協助我把入口兩個字變大\n修改為連結」未被局部處理接住，模型自行選 content 後整段替換的問題。新增 TryPresentSelectedPhrase，完整識別已支援的放大／縮小、色碼與待設定網址連結組合，在原 HTML 上處理；不認識的其他操作仍交給模型，不忽略未理解的子指令。兩個字放大以繼承字級加 4px，其他文字、字型與內容保留。放大單一片語使用既有文字 span，連結組合只包覆指定原文，href 留待編輯，零模型呼叫。

另在 selected 的外觀／連結請求加入完整原文比對，模型即使回傳 editMode=content，也不能用只有指定兩字的 HTML 覆蓋整個標題；文字不同即不套用。明確文案改寫／刪除等操作不由這條外觀保護規則攔截，既有文字替換流程維持。

擴充既有 BackofficeSandboxAiCanvasChecks／sandbox/ai-provider-contracts：原句保留「品牌入口」、僅入口成連結且放大、只放大不刪品牌、外觀請求中模型回傳只有入口時拒絕、保留完整文字的模型連結內容仍可套用。使用獨立記憶體 fixture、不連外，尚未執行。人工重現需先回到未被截斷的本機版本，再送原句確認四字保留、只入口放大並可設定網址；此修正不會推測或重建先前已刪掉的內容。未執行編譯、檢測或瀏覽器驗收；只修改後端，不需 bundle／Migration。

## 選取區塊 CSS 與待設定網址的局部連結

AI 回覆的靜態 ID／class／標籤、後代／直接子代與逗號群組選擇器，先在目前修改 HTML fragment 中定位，再由 CanvasAiStyleNormalizer 轉為前端既有可套用的 #id 規則；缺 ID 時僅補 ID，保留原文與 class，處理相同屬性的 specificity／先後順序。不把 class 規則發布到全站，未命中修改範圍或不支援的動態 selector 不套用，仍保留 CSS 屬性／值與 HTML 檢查。前端協定不變，不需 bundle。

「把入口兩個字換成連結\n顏色變成 #336589」這類指定原文＋連結＋明確色碼，由既有局部連結流程直接處理，不呼叫模型；未提供網址時建立可後續編輯的 a 元件且不填 href、不猜網址。保留其他文字與字級，色碼只作用於該連結；原有完整 HTTPS 網址流程保留。重複目標或不在選取區塊時仍不任意擴大範圍。

新增 BackofficeSandboxAiCanvasChecks，綁至既有 sandbox/ai-provider-contracts：class／標籤／直接子代／群組定位、同目標屬性優先順序、補 ID 不改原文與 class、區塊外零套用、實際 CanvasAiService 的 class 回覆成功、危險 CSS 值仍拒絕，以及使用者原句的零模型呼叫／只連結入口／保留品牌／href 留空／指定色碼回歸。所有寫入僅在現有獨立 InMemory fixture 與暫存金鑰，不寫客戶資料庫、不連外；未執行，不宣稱瀏覽器或 SQL 通過。

人工驗收：指定「品牌入口」區塊，使用原句後確認入口成為可編輯連結、顏色為 #336589、品牌與字級不變，之後從連結編輯功能填入網址；另測跨子節點、重複文字、提供完整網址、class／標籤樣式回覆只影響選取區塊，以及不安全 CSS 仍不套用。尚未執行編譯、隔離檢測或瀏覽器。需使用者手動編譯 MVC，可能還原套件並更新 bin/obj，耗時依環境；無 Migration 或資產變更。

## 測試連線成功文案

系統預設與外部供應商測試成功的使用者訊息統一為「連線成功」，不加入生成範圍、JSON 或儲存狀態等技術說明。實際檢查流程與失敗結果不變。更新既有 preview-permissions-and-no-write 人工規格，核對成功彈窗及狀態文字均為「連線成功」，失敗不可顯示成功；尚未執行編譯或實際驗收。本次只修改 C# 與檢測文件，不需 bundle。

## 通用 AI 與畫布適配器分層

新增 Application.Shared/Ai/AiContracts（含 IWebsiteAiService）、MVC 的 WebsiteAiService、AiInputValidation 與 CanvasAiClient；Application Service 可引用共用契約而不反向依賴 MVC。AiClient 移除 canvas-edit、HTML、語系、畫布錯誤解析及固定 JSON 假設，改處理多輪 user／assistant 訊息、inline 圖片、文字／JSON 格式與輸出上限。CanvasAiService 改依賴 CanvasAiClient，保留原畫布校驗；設定頁固定 JSON Test 更新至通用契約。畫布入口 readiness 移至 CanvasAiAvailability 檔，未改其權限／可用性語意。通用服務每次讀取本站設定、尊重開關與取消／逾時；不新增無業務權限邊界的公開 Chat API。

系統預設只有已知畫布 Gateway，保留原協定；通用任務明確回傳 501 且不送 HTTP、不默默回退。外部三家支援文字與 PNG／JPEG／WebP Base64 圖片轉換；各功能仍負責上傳完整性、資料存取、提示詞與業務結果驗證。圖片張數／MIME／檔頭／Base64／大小、訊息角色／數量／長度、輸出上限不合法時零模型呼叫。沒有串流、工具／SQL 執行或客服、商品新增功能。

新增 BackofficeSandboxAiGenericChecks，由既有 sandbox/ai-provider-contracts 呼叫（保留案例 ID／數量），使用隔離記憶體設定與純記憶體 HTTP handler：三家純文字不強制 JSON、多輪 assistant 映射、inline 圖片欄位、非法角色／圖片／張數及 token 限制零 HTTP、通用 self 不支援、舊 Gateway workflow／HTML／語系與呼叫數、已知 422 錯誤不洩漏，以及共用服務讀取加密本站設定／停用後即時拒絕。設定寫入只在獨立 InMemory 與臨時金鑰 fixture，不碰客戶資料庫。原 provider-contracts 的 JSON／截斷／錯誤與模型清單斷言仍保留。

尚未執行上述案例、編譯、瀏覽器或真實供應商，不代表 SQL／圖片分析／畫布已通過。新增內部人工規格確認畫布權限與前端契約不變；沒有新增自動通過數。此修改沒有前端資產或 Migration。整合範例、限制與建議手動編譯命令見 docs/AI_INTEGRATION.md；命令可能還原套件並寫入 bin/obj，耗時依環境而定，未由 Codex 執行。

## 帳戶模型清單與下拉選擇

AI 設定新增「載入模型」及名稱＋ID 下拉，保留自訂模型 ID。使用者明確點擊才查詢，不在開頁、切換或保存時連外。查詢可以使用未保存的新金鑰及 Claude 工作區，未填模型仍可查詢；只有這條模型查詢路徑呼叫 Prepare(requireModel:false)，保存與生成原模型必填規則維持。Models POST 沿用編輯權限、防偽、本站與版本驗證；保留已存金鑰時前端不回填金鑰，後端解密讀取，替換時僅經同源 HTTPS 傳入新金鑰。查詢不保存、開放功能或生成，不使用客戶資料庫做測試寫入，不快取不同站台的憑證或模型清單。

新增 AiModelCatalog，以既有禁止轉址 WebsiteAi HTTP client 對三家固定 HTTPS 目的地執行 metadata GET；Claude 帶工作區與版本，Claude／Gemini 支援分頁，Gemini 只取 generateContent，模型 ID 去重並符合保存格式。限制 30 秒、最多 20 頁、每頁 256 KiB／最多 2000 選項，重複 cursor、超限、非 JSON、HTTP 或網路錯誤都拒絕且不回傳部分清單／供應商原文、不重試／回退。OpenAI 清單未提供 Responses 相容性能力，列出 API 回傳且符合 ID 格式的模型；清單不是測試通過，選擇後仍需保存並測試。新模型取不到或不在清單時可以自訂。切換供應商、金鑰操作、新金鑰或工作區即清除舊選項；查詢失敗保留目前模型，用 co.sweet.error 回饋，模型名稱用 textContent 輸出。

新增 BackofficeSandboxAiModelChecks，從既有 sandbox/ai-provider-contracts 執行器實際呼叫：首次無模型 Prepare 成功但保存 Prepare 拒絕、固定目的地／GET／驗證標頭／無金鑰 URL、Claude workspace／分頁／去重、Gemini 去除 models/、生成方法篩選與 pageToken 編碼、OpenAI ID、空清單、400／401／403／429／500 不洩漏、不重試、非 JSON／超大／重複 cursor 拒絕、網路錯誤及 self 零連外。沿用原自動案例 ID／數量，未執行。

新增模型選擇人工規格（不綁 executor，不列使用者自動通過數）：金鑰尚未保存就載入再選擇保存；舊模型不在清單時保留；自訂模型可改；切換三家、金鑰與工作區後不可選舊清單；唯讀不可查詢；缺防偽、跨站、舊版本及撤權拒絕；慢網路與失敗不清除使用者設定；查詢前後設定及版本不變，Test 仍無 Body。沒有新增 Seed、schema 或 Migration。

本次僅靜態檢查，未執行 build、檢測、瀏覽器、真實供應商呼叫或 bundle。建議使用者編譯 MVC（可能還原套件並寫入 bin/obj，時間依環境），並在 src/EtheriT.Coker.Web.MVC 手動 npm run create-bundles；此流程先檢查授權，再 yarn && gulp buildDev，可能下載相依、修改 lock file、產生大量資產與啟動監看，可能耗時較長。

## 儲存成功不要求重新開啟編輯頁

保存成功彈窗只顯示「AI 設定已儲存」。前端既有 apply(data) 即時套用保存回應與版本，Test 與 CanvasAiService.GenerateCore 均在呼叫時讀取本站最新設定；更換供應商、模型、金鑰與 Claude 工作區 ID 不需額外重開或自動重載編輯器。人工回歸需保存後直接測試最新設定，再於已啟用 AI 的編輯器送出新請求核對最新供應商／工作區；不自動發出付費測試或生成。已開啟編輯器的 AI 入口啟用顯示仍由初始化判定，這次不宣稱已實作跨頁入口同步。尚未執行瀏覽器或外部驗收，未執行 bundle。

## AI 設定共用彈窗回饋

保存與測試結果統一使用 co.sweet.success／error，收到有效保存回應且 apply 成功後才顯示成功；不再以頁首或按鈕旁文字作為操作結果回饋。測試費用及重新載入確認改用 co.sweet.confirmAsync，確認期間鎖定操作，取消不送 API；確認後仍保留原防偽與站台比對。後端文字先 HTML 編碼再交給共用彈窗的 html 參數，避免當作標記解析。

更新既有 preview-controls 人工回歸：捲至底部，成功、前端驗證失敗、防偽取得失敗與保存 API 失敗皆顯示共用彈窗；處理中停用、結束恢復。外部測試與重新載入顯示系統確認窗，取消及連點無 API，確認一次只執行一次，測試結果成功／失敗亦用共用彈窗，錯誤中 HTML 字元顯示原文；窄螢幕可閱讀。尚未執行瀏覽器驗收，不新增自動通過數。只修改 Razor 與 JS 原始碼，需手動執行 MVC 的 npm run create-bundles；可能下載相依、修改 lock file、產生大量資產及啟動監看，耗時依環境而定。本次未執行。

## Claude 工作區 ID

新增選填 AiClaudeWorkspaceId（StoreSet ID 40、群組 9、文字、預設空白），AI 管理參數由四個改為五個；沿用本站明細、通用寫入保護與版本指紋。只有 Claude 顯示欄位並送出 anthropic-workspace-id；空白不送，支援已綁定工作區金鑰。切換至其他供應商保存時清空工作區 ID。限制 wrkspc_ 後接 ASCII 英數、總長不超過 100，拒絕標頭注入。工作區 ID 非密鑰，允許回填；金鑰維持加密及遮罩。Test 仍無 Body，只用已保存設定。

擴充既有 sandbox/ai-settings 與 sandbox/ai-provider-contracts：五筆設定、工作區保存重讀、單獨變更更新版本且保留金鑰、換供應商清空、Claude 正確標頭、空白省略、其他供應商不帶入，以及無效／超長／換行 ID 拒絕。保留穩定案例 ID 與數量；未執行，不能宣稱真實 Claude 或 SQL 驗證通過。

人工驗收：完成 Seed 部署、編譯及資產更新後，保留原金鑰，填入具有存取權的工作區 ID 並保存，重開比對；未保存時不能測試。分別測試已綁定金鑰＋空 ID、未綁定金鑰＋有效 ID、錯誤／無權限工作區、跨站保存拒絕、唯讀停用及切換供應商。此項設定缺 Seed 時沿用既有拒絕讀寫行為，不在執行時補建；部署前需先套用 Seed。

未建立 Migration 或修改既有 Migration／snapshot。建議由使用者於 Repository 根目錄執行並審查：

```powershell
dotnet ef migrations add AddClaudeWorkspaceIdSetting --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
```

命令可能編譯及還原套件，會建立 Migration 與更新 snapshot，耗時依環境而定。預期只新增 StoreSet ID 40；必須確認 ID 沒有衝突，且沒有夾帶其他尚未套用的 Seed／schema 差異。既有站台新明細未存在時使用空白，不變更原金鑰。審查、備份與確認目標環境後，再自行執行：

```powershell
dotnet ef database update --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
```

此命令會套用所有待處理 Migration 並寫入資料庫，可能鎖定資料、編譯、還原且耗時較長。本次未執行。MVC 需手動 npm run create-bundles（先檢查授權，再 yarn && gulp buildDev），可能下載相依、改 lock file、產生大量資產與啟動監看；未由 Codex 執行。

## API Key 非 ASCII 字元回歸

WebsiteAiSettingsService.Prepare 在儲存前拒絕非可見 ASCII 金鑰，AiClient 在建立外部請求前使用同一驗證，避免 HttpClient 寫入標頭才拋出 Unknown 連線例外。既有設定讀取不新增此限制，讓管理者仍能進入頁面更換錯誤金鑰；測試回傳安全設定錯誤，不回傳原金鑰，不修改或猜測字元。

擴充既有 sandbox/ai-provider-contracts 執行器：中文、全形、遮罩及零寬字元不可保存，已存錯誤金鑰不可呼叫 HTTP，錯誤不洩漏假金鑰；案例 ID 與自動案例數不變。尚未執行編譯、隔離檢測、瀏覽器或外部 API。人工驗收需先以完整有效金鑰更換並儲存，再點測試；不可用 Console 列表遮罩內容代替原始金鑰。

## AI 連線失敗分類

AiClient 依 HttpRequestException.HttpRequestError 區分 DNS、安全連線、網路、Proxy 與無效／中斷 HTTP 回應，對使用者回傳固定可理解提示；伺服器 warning 日誌僅記錄供應商鍵值、錯誤分類及內層例外型別，不記錄金鑰、請求內容或原始例外訊息。未知錯誤維持明確失敗，不重試、不回退、不略過 TLS 驗證。

擴充既有 sandbox/ai-provider-contracts 執行器與穩定案例 ID：純記憶體 handler 模擬六種連線錯誤，斷言 502、分類提示、不洩漏例外中的假金鑰及僅呼叫一次。尚未執行，不代表實際 DNS、憑證或 Claude 串接通過。由使用者重新編譯啟動 MVC 後，再點測試取得分類；本次僅修改 C# 與內部檢測資料，不需重產 bundle。

## AI 已儲存設定連線測試

WebsiteAi/Test 改為無 Request Body 的 POST，由後端依登入者目前授權站台讀取已儲存的供應商、模型及解密金鑰，固定測試訊息仍由後端建立；不接受前端草稿覆寫。保留編輯權限、防偽、前端 Token 站台比對、30 秒逾時、費用確認及不保存／不開放功能的行為。未儲存變更時停用測試並提示先儲存。API Key 欄位仍供更換並儲存金鑰使用，保留金鑰時儲存送出空字串，測試則不送出任何設定欄位。

更新既有 preview-permissions-and-no-write 人工規格，涵蓋空 Body、後端使用已存設定、草稿不可測試及舊客戶端傳入設定不可覆寫等回歸。未新增自動通過數；未執行外部呼叫、瀏覽器、build 或 bundle。此修改不代表原連線 502 已解決。前端維持引用 AiSettings.min.js，需手動重新產生資產。

## AI 設定保存與供應商共用呼叫

AiEnabled 改為單一布林 checkBox，DefaultValue=false，不建立「關閉／開放」StoreSetItems。AI 專用頁原本即使用單一開關，因此不修改共用多選 checkbox 元件，也不需因本次調整重產前端資產。StoreSetDetail 仍依站台保存 true／false，沒有本站明細為關閉；只保留四個供應商選項，ID 33～36 不重新編號。已同步既有 AddWebsiteAiStoreSettings 的 Up／Down、Designer 與 snapshot，移除選項 ID 31、32 並將參數 ID 36 的型別改為 4。sandbox/ai-settings 的斷言改為 checkBox＋無選項＋預設 false，並檢查取消勾選確實保存 false；尚未執行。

若 AddWebsiteAiStoreSettings 已套用，修改原檔不會改變已存在的資料庫。需另行審查修正 Migration，將 StoreSet ID 36 的 type 更新為 4，並移除屬於該參數的選項 ID 31、32，保留 StoreSetDetail 既有 true／false 與供應商選項。因 snapshot 已同步目標狀態，後續產生的 Migration 可能為空，需人工補齊並審查 Up／Down，不可直接宣稱資料庫已修正。若需要建立後續修正 Migration，可由使用者自行執行下列指令；會新增 Migration／修改 snapshot，可能編譯及還原套件，耗時依環境而定：

~~~powershell
dotnet ef migrations add UseCheckboxForWebsiteAiEnabled --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

套用修正仍使用下方 database update 指令，由使用者確認環境、備份及內容後決定；會寫入資料庫，可能鎖定資料並耗時較長。本次未建立／執行上述命令，也未查詢或更新資料庫。

AI 設定的客戶顯示名稱統一為「系統預設 AI」，不揭露部署方式；內部 self 鍵值與路由維持。同步修改 Seed、使用者指定的新 Migration 插入文字及其 Designer／snapshot 對應標籤，未重新產生或套用 Migration。若該 Migration 已套用，修改既有檔案不會更新資料庫，需另外審查 Seed 資料更新。sandbox/ai-settings 增加預設選項顯示名稱斷言，preview-controls 人工規格要求供應商、摘要、金鑰提示、測試結果與錯誤訊息皆不顯示部署方式；本次未執行檢測或瀏覽器驗收。

SystemManagement.AiSettings 已接上本站設定的讀取、保存與測試。保留 preview-controls、preview-permissions-and-no-write 穩定 ID，內容更新為設定保存、畫布開關、權限、防偽、跨站、兩分頁版本衝突、連線測試不保存及錯誤不回退的人工驗收；未綁定 executor，不列自動通過數，尚未執行。

新增 settings-isolation → sandbox/ai-settings、provider-contracts → sandbox/ai-provider-contracts，分別驗證加密保存、遮罩、通用設定拒絕保留鍵值、金鑰保留／清除、換供應商拒用舊金鑰、預設關閉、舊版及跨站拒絕，以及三家固定目的地與格式轉換、錯誤不洩漏／不重試／不回退、過大及非 JSON 回覆。使用既有獨立 InMemory 容器、臨時 Data Protection provider 與假 HTTP handler，不連外，不寫客戶資料庫。已實作執行器與斷言但本次未執行；不能宣稱 SQL、瀏覽器、真實供應商或加密金鑰持久化驗證通過。

AI 設定依既有結構由 StoreSetSeed 定義：StoreSetGroup ID 9「AI 設定」；StoreSet ID 36～39 為 AiEnabled、AiProvider、AiModel、AiApiKey；AiEnabled 為單一 checkBox、DefaultValue=false，不建立 StoreSetItems；僅 AiProvider 使用 StoreSetItems ID 33～36，分別為系統預設 AI／OpenAI／Gemini／Claude。站台值各自寫入四筆 StoreSetDetail，只有 AiApiKey.value 加密，purpose 綁定站台及供應商。已移除 Entity 的 AiSettingsKey 常數與整包 JSON 保存，儲存不再新增群組、參數或選項；Seed 未部署完整時拒絕保存及使用，不在執行時補建。介面供應商選項改讀 StoreSetItems。版本由四筆明細的指紋計算，不新增業務參數或資料欄位。

關聯式保存仍使用執行策略內的 Serializable 交易，InMemory 不驗證 SQL 鎖定及 rollback。無本站明細預設關閉；外部金鑰不存在時走原自架 Gateway。共用 AI 呼叫流程不變。通用設定查詢可看到正常參數定義與非敏感值，但金鑰必須遮罩；通用寫入拒絕四個 AI 參數，必須經 AI 設定頁的權限、版本與金鑰處理。先前版本若已保存 __AiSettingsV1，保留原資料且不自動轉移／刪除，通用讀取仍遮罩；新功能不採用舊整包資料。

部署前需確認 ASP.NET Core Data Protection key ring 可持久化、備份及在多台 MVC 主機間共用，並限制存取。不可刪除 key ring 或任意變更應用程式隔離設定，否則既有設定可能無法解密；此時明確拒絕使用，不自動回退或清空。此階段使用框架既有 key ring 設定，未替使用者操作部署或寫入金鑰檔。前端與 API 不回傳密文或原文。外部測試是固定 JSON 回覆檢查，需使用者點擊並同意可能費用；自架測試僅健康檢查，不代表模型生成或畫布流程通過。頁面載入不做付費推論，外部設定的可呼叫狀態不是測試通過。

MVC 頁面引用 AiSettings.min.js，維護 AiSettings.js 原始碼。使用者需於 src/EtheriT.Coker.Web.MVC 手動執行 npm run create-bundles；實際流程先檢查 DevExtreme 授權，再執行 yarn && gulp buildDev，可能下載相依、修改 lock file、產生大量資產及啟動監看，耗時依環境而定。本次未執行 bundle、build、test 或資料庫操作。

本次原始碼調整清單（路徑以 Repository 根目錄為準）：

| 檔案 | 調整與目的 |
| --- | --- |
| src/EtheriT.Coker.Core/Models/StoreSet.cs | 移除先前加入的 AiSettingsKey，Entity 回復原結構。 |
| src/EtheriT.Coker.EntityFrameworkCore/Migrations/Seed/StoreSetSeed.cs | 新增 AI 群組、四個參數與四個供應商選項；不執行資料更新。 |
| src/EtheriT.Coker.Application.Shared/Common/AiStoreSettingNames.cs | 集中參數名稱與金鑰遮罩判斷；不是 Entity 欄位。 |
| src/EtheriT.Coker.Application/StoreSet/StoreSetAppService.cs | 保留正常參數定義及非敏感值，遮罩 API Key；通用寫入拒絕四個 AI 參數，避免繞過專用權限及加密處理。 |
| src/EtheriT.Coker.Web.MVC/Services/Ai/WebsiteAiSettingsService.cs | 讀取 Seed 定義與選項，分筆保存本站值、單獨加密金鑰、版本與金鑰操作驗證。 |
| src/EtheriT.Coker.Web.MVC/Services/Ai/AiClient.cs | IAiClient 共用介面、固定供應商轉換、錯誤映射、大小限制及依本站設定判斷可用性。 |
| src/EtheriT.Coker.Web.MVC/Services/Ai/CanvasAiService.cs | 改用本站設定與共用 AI 呼叫，保留原畫布解析及安全驗證。 |
| src/EtheriT.Coker.Web.MVC/Controllers/api/WebsiteAiController.cs | 安全設定讀取、專用防偽配對、儲存及不保存的草稿測試。 |
| src/EtheriT.Coker.Web.MVC/Controllers/api/CanvasAiController.cs | 移除模型呼叫失敗時污染全域自架可用性狀態的操作。 |
| src/EtheriT.Coker.Web.MVC/Program.cs | 註冊設定、共用呼叫、本站可用性、Data Protection 及禁止轉址的 HTTP client。 |
| src/EtheriT.Coker.Web.MVC/Views/SystemManagement/AiSettings.cshtml | 預覽頁改為實際設定介面，新增保留／更換／清除操作，支援儲存及測試狀態。 |
| src/EtheriT.Coker.Web.MVC/wwwroot/js/SystemManagement/AiSettings.js | 讀取／保存／測試、防偽更新、版本與站台比對、忙碌及讀取失敗控制、金鑰清除與費用提示。 |
| src/EtheriT.Coker.Web.MVC/Views/Shared/GrapesComponents/_ScriptComponents.cshtml | 畫布入口依本站開關與所選服務判斷，不再一律檢查自架服務。 |
| src/EtheriT.Coker.Web.MVC/Diagnostics/BackofficeSandboxAiChecks.cs | 新增實際隔離保存、保護與供應商轉換斷言；未執行。 |
| src/EtheriT.Coker.Web.MVC/Diagnostics/BackofficeSandboxChecks.cs | 登錄兩個新的隔離執行器與頁面綁定。 |
| src/EtheriT.Coker.Web.MVC/Diagnostics/backoffice-cases.json | 新增兩個自動案例並更新兩個既有人工規格，保留原 ID 及其他既有修改。 |
| docs/BACKOFFICE_FUNCTION_TESTS.md | 更新執行器、驗收範圍、部署與手動準備說明。 |

建議手動執行（尚未執行）：

~~~powershell
dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此修改涉及 C# 型別、DI、EF 查詢與 Razor，需要確認編譯結果。命令可能自動還原套件並寫入 bin／obj 等產物，實際耗時依環境而定。完成資產及編譯後，再於測試環境手動驗收前述操作；首次儲存會新增本站設定資料。未執行上述命令或案例，不表示驗證已通過。

### Seed 資料更新（尚未執行）

這次沒有新增資料庫欄位，但 HasData 的新增定義仍需經專案 Migration／部署流程寫入既有資料庫；只改 Seed 原始碼或重新編譯不會插入資料。本次未建立或執行 Migration、未更新資料庫；依使用者要求，已手動同步現有 AddWebsiteAiStoreSettings、其 Designer 與 snapshot 中的 AI 定義。

建議 Migration 名稱 AddWebsiteAiStoreSettings，由使用者自行決定執行：

~~~powershell
dotnet ef migrations add AddWebsiteAiStoreSettings --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此命令可能編譯及還原套件，會建立 Migration 並更新模型 snapshot，耗時依環境而定。請審查應僅新增 AI 群組、四個參數及四個供應商選項；其他尚未登錄的模型差異不可直接套用。

完成審查、備份及確認連線環境後，才由使用者決定：

~~~powershell
dotnet ef database update --project "src/EtheriT.Coker.EntityFrameworkCore/EtheriT.Coker.EntityFrameworkCore.csproj" --startup-project "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj"
~~~

此命令會寫入指定資料庫並套用所有待處理 Migration，可能鎖定資料與耗時較長。先確認沒有同 ID 的客製資料：群組 9、參數 36～39、選項 33～36；尤其先前整包版本若曾在執行時新增資料，可能占用這些 ID。不可盲目覆蓋或刪除；需另行盤點並確認資料轉移方案。舊 __AiSettingsV1 不會被本次程式自動搬移、採用或刪除；新站台設定仍預設關閉，需管理者重新確認及儲存。缺 Seed 時介面提示未部署並停止保存，不產生替代資料。

本次同步擴充 sandbox/ai-settings 的群組／選項來源、四筆獨立明細、保存不新增定義及缺 Seed 拒絕斷言，維持原案例 ID 與數量；尚未執行。瀏覽器需人工比對供應商選項、保存重開、空金鑰、密文遮罩及跨站。AiSettings.js 已修改，仍需使用者在 MVC 目錄手動 npm run create-bundles；作用、產物與耗時如前述。

## 執行範圍

MVC「系統檢測 → 後台功能測試」的開始按鈕會真正呼叫已登錄的執行器。
它不執行 dotnet test、build、restore、bundle、Migration，也不透過 HTTP 遍歷所有 GET Action。
這些開發／產物操作仍依根目錄 AGENTS.md 由使用者控制。

目前登錄 223 個自動檢測案例：160 個列表查詢／條件／排序／分頁檢查、16 個設定或統計檢查、
47 個隔離操作案例。實際適用數量依目前站台版本與啟用功能篩選，不固定為 223。
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

## 搜尋日誌輸入與訪客識別回歸

新增 `Page.Index.search-log-validation`，綁定 `sandbox/search-log-validation`。使用獨立 InMemory 資料庫、測試簽章及實際 Token／搜尋日誌服務，驗證缺少、損壞、過期、跨站與全零 UUID Token 不落日誌；有效匿名 Token 搭配空白、undefined／null 及超長關鍵字不落日誌。有效中文、加號關鍵字保留原 Token UUID，去除前後空白，同日同目標去重，不同目標分開記錄。

人工回歸：以新訪客直接開啟搜尋網址，確認 Token 建立後有效搜尋寫入非零 UUID；模擬 Token 建立失敗或刪除 Token 後直接呼叫搜尋 API，內容仍可查詢而不新增無效日誌。既有異常資料不自動修改。上述案例及瀏覽器驗收尚未執行；隔離案例不代表 SQL 或正式環境驗證通過。

## 搜尋初始化與 Token 更新流程回歸

擴充既有 `sandbox/search-log-validation`：以實際 Token 服務驗證匿名有效 Token 重用、過期 Access Token 透過 RefreshToken 更新、匿名 UUID 保留、舊 RefreshToken 不被誤作廢及更新後日誌 UUID 一致；簽入基礎設施以隔離替身處理，不驗證真實 Cookie 驗證管線。未增加自動案例數。

擴充 `Page.Index.search-keyword-links` 人工瀏覽器驗收：慢網路首次進站多目錄共用初始化；搜尋 API／上下則等待 Token；到期前更新；Token 失敗不送搜尋且可重試；缺少輸入、空白、undefined／null 在前台阻擋；動態關鍵字屬性及 jQuery 快取同步。對應來源為前台 site.js、coker.api.token.js、coker.core.js、coker.api.directory.js、coker.feature.search.js、Search.js 及 Directory boot.js。瀏覽器規格仍未綁定自動執行器，不計自動通過數。上述執行器與人工情境均未執行。


## 文章列表大型內容回歸

ArticleAppService.GetAllList 不再投影正式／草稿 Html、SaveHtml、Css、SaveCss；列表只載入既有中繼欄位，內文仍由編輯／畫布 API 取得。保留既有標籤搜尋與記憶體分頁邏輯，因此尚未解決大量文章中繼資料全量載入的擴充性問題；不能據此宣稱正式站空白畫面唯一原因或 SQL 耗時已驗證。

沿用 ContentManagement.Article.sandbox-article-crud 與 sandbox/article-crud，新增獨立 InMemory 大型內容斷言：兩篇文章每個內文／樣式欄位各 256 KiB，另加其他站台文章；分兩頁查詢確認總筆數、順序、站台與軟刪除隔離，回應內文／樣式為空且單筆序列化小於 8 KiB，原始內容保持完整。不增加自動通過數。

人工驗收：以原異常站台資料於測試環境開啟文章管理，核對列表請求狀態、等待時間與回應大小；測試標籤／標題搜尋、排序、分頁、開啟文章與畫布後正式／草稿內容及樣式完整。若仍空白，需保存失敗請求的完整 URL、HTTP 狀態、回應與對應伺服器日誌，再判斷其他初始化請求或例外。DevTools 快取遭逐出訊息本身不等於 API 回應失敗。

本次僅靜態檢查與 Git diff，未執行編譯、隔離檢測、瀏覽器或 SQL 驗證，未連正式站、未修改資料庫。沒有前端資產變更，不需 bundle。使用者可手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj" 確認 C# 編譯；可能還原相依並寫入 bin/obj，耗時依環境且可能較長，本次未執行。


## 文章內嵌圖片在儲存與匯入時強制轉存

文章 SaveContenInternal 在確認目前站台、有效文章及既有編輯權限後，透過 IFileUploadAppService.NormalizeInlineImageSourcesAsync 解析 HTML 的 src。data:image 的 PNG／JPEG／GIF／WebP／BMP 支援 Base64 及百分比原始位元組編碼，確認檔頭、宣告 MIME 及圖片尺寸後，沿用現有 htmlConten 上傳與圖片縮放／轉換流程，替換成本站檔案網址；不下載一般遠端網址。相同圖片位元組以站台及內容摘要去重，同一篇重複引用、重試與再保存共用已存在的本站圖檔，缺實體檔時重建。新方法不新增公開 API。

限制：單張 10 MiB、單次不同圖片合計 25 MiB、最多 50 個 src、單張最多 4000 萬像素；仍受既有 HTTP 請求與上傳格式限制。內嵌 SVG 等其他格式、無效編碼、內容格式不符、blob 及含 data／blob 的 srcset 都明確拒絕，請使用上傳後的圖片網址。先驗證全部輸入，再開始建檔，避免後續無效圖片造成部分新增。實際磁碟或資料庫故障仍沿用既有上傳的錯誤／未引用檔管理，沒有新增跨檔案／SQL 交易保證。此次只處理文章 HTML src，不轉換 CSS 背景、其他功能的內文或瀏覽器暫存圖。

ImportConten 只在草稿保存成功後，從已轉存的 dto 建立正式發布內容，避免發布原始 Base64；前台依舊沿 ResolveFrontUploadPath 移除後台站台路徑。拒絕時不繼續發布。內容儲存／發布查詢明確限制本站及未刪除文章，即使系統管理者也不會先替其他站台建立圖檔。含內嵌資料或超過 1 MiB 的失敗內容不整份寫入操作日誌，只記文章 ID 與長度。

ArticleManager.js 的儲存與發布 callback 在服務拒絕或 HTTP 失敗時 reject deferred，避免一直等待；不重送寫入或自動覆蓋目前畫布。已保存的圖檔網址於重開畫布後載入，當前畫布內原始 src 不會在儲存期間被強制重載；首次貼入大型 HTML 仍可能讓瀏覽器變慢。既有文章不在讀取時自動轉存，只於下一次成功儲存／匯入處理，不執行批次修復、Migration 或資料變更命令。

新增 ContentManagement.Article.inline-images，綁定 sandbox/article-inline-images 與 BackofficeSandboxInlineImageChecks：在獨立 InMemory 及暫存上傳目錄，使用實際服務驗證 Base64／百分比 PNG 轉存、同圖去重與實體檔、草稿與正式檔案引用、一般網址／文字／CSS 保留、草稿不發布、重開畫布內容、重複儲存／匯入不增檔，以及發布路徑。以新的有效圖片放在錯誤輸入前，驗證無效編碼、偽造檔頭、內嵌 SVG、blob、內嵌 srcset、數量與單張大小超限時不建檔、不改草稿／正式內容；另驗證跨站寫入拒絕。測試不連外、不使用客戶資料庫。上述斷言尚未執行，不代表 SQL、圖片原生套件部署或瀏覽器已通過。合計大小、尺寸邊界及多種圖片格式另需手動驗收。

更新既有 Article.canvas 人工規格：使用可編輯／唯讀帳號，貼入大型或重複圖片、儲存並重開、發布後核對前台及樣式、重試檔案數不增加、無效圖片及 HTTP 失敗提示且等待結束、修正後可再次保存。唯讀／跨站不可產生圖片或改文章。這些人工規格不列自動通過數。

本次只做靜態閱讀、JavaScript 語法解析、JSON／案例 ID／executor 綁定與 Git diff 檢查，未執行編譯、測試、bundle 或正式環境操作。
使用者可於根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj" 確認介面及 C# 編譯，可能還原相依、寫入 bin/obj 且耗時較長；本次未執行。
前端仍引用既有 ArticleManager.min.js；請在 src/EtheriT.Coker.Web.MVC 手動 npm run create-bundles，以套用 callback 修正。已確認 package.json 先檢查 DevExtreme 授權，再執行 yarn && gulp buildDev；可能下載相依、更新 lock file、產生大量資產及啟動監看，耗時依環境，本次不自行產生 .min.js。


## 文章更新後僅詢問本次失去引用的圖片

ArticleAppService 在儲存／匯入前，從這篇文章的草稿、正式內容、電子報、CSS 與描述等欄位收集已登錄的本站圖片 ID；成功保存並更新引用後比較新舊集合。沒有移除圖片、圖片仍用於正式內容、同篇其他欄位或其他內容時不提供回收詢問。只在有移除候選時才呼叫既有 FileReferenceScanner，檢查實際內容、綁定、同路徑及相關尺寸圖，不在每次一般保存掃描整站或列舉磁碟。沿用 FileReferenceIndexingService 的路徑解析（改為 internal 共用），不新增另一套引用判斷。

回傳資訊只有數量、確認憑證及檢查失敗旗標，不將內部案例、來源路徑或圖片 ID 顯示在使用者畫面。確認憑證由 DataProtection 保護，綁定目前使用者、站台、文章、保存後內容指紋、最多 200 個本次候選及 15 分鐘期限；日誌不保存憑證。前台共用 ArticleAppService 的新增清理相依採選用，不要求前台註冊後台檔案管理服務，前台讀取流程維持。

ArticleController 僅在目前站台版本及有效父層功能允許、使用者可編輯文章或目錄且有檔案管理刪除權限時提供確認。RecycleUnusedImages POST 需防偽，另有經相同權限檢查且 no-store 的 GetImageCleanupAntiforgeryToken；前端確認後才取最新 Token/Cookie 配對，將快照站台放在 header/query，再送一次清理，不自動重送。清理服務重新核對使用者／站台／文章／期限／內容指紋與編輯資格，重新掃描候選引用，並由既有 MoveToRecycleBinAsync 在實際搬移前再次檢查完整檔案家族引用。只回收本次已移除且仍無引用的本站圖片，不掃除整站其他閒置檔，不永久刪除。

ArticleManager 在保存成功後，有候選才顯示「文章已儲存／已儲存並發布」與圖片數量，提供「移到回收桶／保留圖片」。取消不送清理請求、不新增清理候選或搬檔；畫布內容、目前文章或站台於提示期間改變時也保留。確認期間阻擋同畫布重複儲存／發布。清理失敗明示文章已儲存，部分搬移結果不宣稱全部保留或全部成功。grapesjs-Coker6.js 的共用儲存／發布 callback 支援 handledFeedback，文章清理已提示時不再用一般成功彈窗覆蓋；其他功能仍顯示原成功提示。圖片可由既有回收桶還原；本機歷史版本不是正式引用，還原含已回收舊圖的版本前需先還原圖片。

新增 ContentManagement.Article.unused-image-cleanup，綁定 sandbox/article-image-cleanup，requiredFeatures=FileManagement。獨立 InMemory／暫存圖片與臨時保護金鑰斷言：草稿換圖但正式仍使用舊圖不提示、發布替換後才提供本次移除確認、取消不搬圖及不新增候選、偽造／錯文章／錯站台／錯使用者／逾期確認拒絕、提示後新增未建索引的共用引用仍保留、內容更新後的舊確認拒絕、重新取得確認後回收、重放不搬其他圖、保存內容不變及可還原。沒有連客戶資料庫或執行上述斷言，不代表 SQL、並行交易、HTTP 防偽管線或瀏覽器通過。

新增 unused-image-cleanup-browser 人工規格（無 executor、不列自動通過數），涵蓋沒有候選不詢問、一般成功提示一次、保留／確認、提示期間畫布／文章／站台變更、角色撤權、功能或父層停用、跨站與防偽拒絕、其他內容重新引用、HTTP／搬檔失敗、重複點擊、回收還原及其他功能原儲存提示。不自動清除本機 AI 版本，也不宣稱所有歷史圖檔都已掃描。

本次僅靜態閱讀、修改來源、檢查 JavaScript 語法、JSON／唯一 ID／來源路徑／executor 綁定及 Git diff，未執行編譯、檢測、bundle、Migration、正式環境或實際資料搬移。
建議使用者於 Repository 根目錄手動執行 dotnet build "src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj" 確認 C# 與介面編譯；可能還原相依並產生 bin/obj，耗時依環境且可能較長。
在 src/EtheriT.Coker.Web.MVC 手動執行 npm run create-bundles，以更新 ArticleManager.min.js 與 grapesjs-Coker6.min.js。已確認現有 script 先檢查 DevExtreme 授權，再 yarn && gulp buildDev，可能下載相依、更新 lock file、產生大量資產及啟動監看，耗時依環境；本次不產生 min.js。

回收後重新插入同一張 Base64 圖片的補充：InlineImageSources 以本站與內容摘要命名的 OriginalFileName 尋找有效且有實體檔的圖檔；新建上傳使用獨立 GuidKey，避免已回收紀錄與重新建立的圖共用鍵值，讓既有引用掃描／檔案家族查詢產生重複鍵例外。保留舊版內容鍵查找以相容本階段已產生的有效圖檔。新增隔離斷言確認重新插入後使用不同識別，再移除仍可正常產生清理確認，原回收圖仍可還原；尚未執行。

去重另核對伺服器配置的「站台＋內容摘要＋上傳 ID」GuidKey 標記，不單憑可由使用者指定的 OriginalFileName。新增隔離情境以同名紀錄指向另一張圖片，確認重新插入仍建立正確圖檔，不誤用偽裝檔名；尚未執行。SaveCanvasImageAsync 的新選用參數只由內嵌圖片流程啟用，既有外站圖片匯入維持原鍵值規則。


## 圖片隔離案例編譯型別修正

修正 BackofficeSandboxArticleCleanupChecks 與 BackofficeSandboxInlineImageChecks 的 15 處 long → int 隱含指派。Article Entity.Id 為 long，但既有 ArticleSaveContenDto.Id 與 SearchIDDto.Id 為 int；呼叫 SaveConten／ImportConten／GetConten 時改用 checked((int)entity.Id)，沿用現有目錄隔離案例的做法，避免靜默截斷。不變更正式 DTO、Entity 或資料庫欄位；RecycleUnusedImages 的 long Id 維持原樣。

同步註記既有 inline-images 與 unused-image-cleanup 案例的 fixture 型別要求，保留原 ID、executor 及業務斷言，不新增自動通過數。僅靜態型別／直接引用、JSON 與 diff 檢查，未執行編譯或隔離案例，不能宣稱已編譯通過。本次不涉及前端資產，不需重新 bundle。請使用者在 Visual Studio 重新編譯 MVC；會寫入 bin/obj 並可能還原相依，耗時依環境而定。
