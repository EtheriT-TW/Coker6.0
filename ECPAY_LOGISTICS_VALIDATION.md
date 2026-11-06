# 綠界物流優先修正與手動驗收

本次修改只包含原始碼與本文，未執行編譯、測試、套件還原、bundle、資料庫更新或綠界實際交易。

## 修改檔案

| 檔案（相對 Repository） | 修改目的 |
| --- | --- |
| src/EtheriT.Coker.Application/ThirdParty/ThirdPartyAppService.cs | 驗證後台 JWT 簽章、issuer、audience、有效期、撤銷狀態、登入紀錄與網站角色；驗證失敗一律失敗。前台 API 網址改為可設定。此共用驗證也用於金流操作，需回歸退款。 |
| src/EtheriT.Coker.Application/ThirdParty/ECPayLogisticsAppService.cs | 修正萊爾富退貨、收件人手機、超商建單與查詢回應驗證、錯誤成功旗標、查詢回補、網站與物流類型檢查、重複退貨與通知時間順序；保留出貨及退貨資料的區隔。 |
| src/EtheriT.Coker.Application/ThirdParty/ECPayLogisticsState.cs | 共用物流狀態文字、物流編號及訂單出貨同步規則；讀取已驗證的退貨通知紀錄。 |
| src/EtheriT.Coker.Application/Order/OrderAppService.cs | 前後台輸出物流進度、退貨進度、退貨託運單號、查詢能力與物流編號備援。保留人工填寫的物流編號。 |
| src/EtheriT.Coker.Application.Shared/Dto/Order/OrderHeaderDisplayDto.cs | 增加後台退貨與補查資訊。 |
| src/EtheriT.Coker.Application.Shared/Dto/Order/OrderHeaderGetOneDto.cs | 增加前台物流與退貨資訊。 |
| src/EtheriT.Coker.Application.Shared/Dto/ThirdParty/ECPayLogistics/ECPayLogisticsTradeInfoResponseDto.cs | 重量改為 decimal?，允許綠界回傳小數公斤。 |
| src/EtheriT.Coker.Web.Public/Controllers/api/ThirdPartyController.cs | 驗證失敗回傳可讀的失敗訊息。 |
| src/EtheriT.Coker.Web.MVC/Views/OrderManagement/OrderList.cshtml | 增加獨立退貨進度顯示。 |
| src/EtheriT.Coker.Web.MVC/wwwroot/js/OrderManagement/OrderList.js | 未出貨亦能補查；補查成功更新列印入口及單號，保留未儲存內容；申請退貨成功隱藏按鈕，獨立顯示進度。 |
| src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Member/js/Member.Orders.js | 會員訂單顯示物流進度、退貨進度與退貨託運單號。 |

## 環境設定

- MVC 呼叫 Public 的預設位置為目前網站的 DefaultUrl。
- 本機或測試環境可在 MVC 的本機設定、User Secrets 或環境變數設定 ThirdParty:FrontApiBaseUrl，例如 https://localhost:7193。環境變數名稱為 ThirdParty__FrontApiBaseUrl。此值必須對應同一個網站的 Public，勿指向其他商家。
- 不設定時不再固定呼叫 lcb 測試站。金流代理也使用同一個網址，需一併確認。
- Public 用來驗證後台 JWT 的 JwtSettings:SignKey、Issuer、Audience，需與 MVC 發行後台憑證的設定相容；Audience 未設定時採 Issuer。
- Public 的 WebConfig:SiteId 必須是目前網站。登入紀錄也必須屬於此網站，並有有效後台角色。
- 綠界回呼需能從外部存取 Public 網址；本機 localhost 本身不能供綠界伺服器回呼。

## 資料與狀態規則

- 未新增 Entity 欄位，不需要本次 Migration。未執行資料庫操作。
- 退貨通知的代碼、訊息、時間、交易編號及託運單號，儲存於既有 AuditLogs 的獨立 ECPayLogisticsReturnState 紀錄，按網站、訂單及原物流記錄關聯；不覆蓋原出貨狀態及託運單號。
- 此次修正後的新合法通知才會產生這種紀錄；既有歷史通知未自動回填。若日後清除 AuditLogs，必須保留 ECPayLogisticsReturnState，否則歷史退貨進度將無法顯示。長期若要將退貨資料改存獨立欄位，需另外規劃 Migration。
- 配送、配達或取件可將待確認／已付款／待付款訂單推進為已出貨。已取消、付款失敗與已完成不會被改回已出貨。
- 配達與取件不會自動改成已完成，不自動退款，也不自動發送郵件，以保留既有人工完成、通知及退款期限流程。
- 正向通知使用既有 UpdateStatusDate 保存綠界貨態時間。過舊通知不覆蓋較新通知；補查回應不覆蓋等待期間收到的新通知狀態。
- 查詢回補不覆蓋原本人工輸入的 TrackingNumber。綠界 B2C 的 ShipmentNo 由查詢補回，宅配使用 BookingNote，C2C 7-ELEVEN 使用寄貨編號與驗證碼組合。

## 測試前的手動驗證命令

建議在 Repository 根目錄手動執行：

    dotnet build src/EtheriT.Coker.Web.MVC/EtheriT.Coker.Web.MVC.csproj
    dotnet build src/EtheriT.Coker.Web.Public/EtheriT.Coker.Web.Public.csproj

用途：檢查 C# 型別、介面、依賴注入與 Razor 編譯。可能還原套件、下載相依項目並寫入 bin/obj；耗時依環境而定，可能超過 30 秒。本次未執行，不能視為通過。

前後台都修改了 bundle 來源，請分別在下列目錄手動執行 npm run create-bundles：

- src/EtheriT.Coker.Web.MVC
- src/EtheriT.Coker.Web.Public

已確認兩個 package.json 都有此 script，會執行 yarn 與 gulp buildDev，可能下載套件、修改 lock file、產生多項前端資產，並啟動持續監看；耗時依環境而定。MVC 另有前置授權檔處理。執行後檢查 generated diff，並確認瀏覽器取得新的 JS。

## 手動驗收清單

1. 正常後台登入：超商／宅配建單、查詢、列印、退貨及原金流退款均可操作。
2. 授權拒絕：過期、修改內容、錯誤簽章、撤銷、無登入紀錄、停權、無本站角色及其他網站憑證，都不得建單、列印、查詢或退貨；驗證失敗應顯示明確訊息。也測試系統維護角色的既有權限。
3. MVC/Public 環境對應：本機設定 FrontApiBaseUrl 後呼叫本機 Public；未設定時呼叫目前網站 DefaultUrl，不能再固定送至 lcb 測試站。
4. 超商建單：B2C 全家、7-ELEVEN、冷凍店取、萊爾富，以及既有 C2C 流程各驗證。錯誤檢查碼、格式不完整、儲存失敗不得顯示成功。
5. 宅配建單：黑貓常溫／冷藏／冷凍與中華郵政，確認單號、實際寄收件資料、公斤小數、列印及原有規格限制。
6. 建單逾時補查：在綠界已有單但本地缺少物流編號的測試資料上，待確認／已付款／待付款仍可查詢；回補 AllPayLogisticsID、CVSPaymentNo、CVSValidationNo、BookingNote，並能列印。測試只能在隔離測試資料處理，不要為測試修改正式資料。
7. 查詢驗證：錯誤檢查碼、不同訂單交易編號、其他商家回應不得改資料。POST 失敗、綠界回傳 0|錯誤時不得開啟列印或顯示成功。
8. 查詢重量：中華郵政回傳如 1.250 公斤、null 重量都能解析。
9. 回呼順序：合法同筆重送、先新後舊、補查期間收到新通知，都不能倒退物流進度；錯誤檢查碼不得變更資料。
10. 訂單同步：配送／配達／取件後更新已出貨；取消、付款失敗、完成訂單不可被回呼改回已出貨；配達不自動完成、不自動退款或寄信。
11. 萊爾富 B2C 退貨：確實送至 ReturnHilifeCVS，使用實際收件人姓名與手機；取得退貨編號，重複申請受阻擋。其他 B2C 超商亦須回歸。
12. 黑貓退貨：申請後按鈕隱藏，合法回呼保留獨立退貨代碼、訊息、時間與託運單號；原出貨單號與進度不能被覆蓋。中華郵政、C2C 超商不提供此逆物流流程。
13. 前台會員訂單：宅配託運單號、C2C 編號、物流進度、退貨進度正確；B2C 在補查後顯示配送編號；不同會員不能看見其他會員資料。
14. 既有功能：非綠界訂單、手動物流編號、更新通知信、出貨通知信、取消與人工退款提醒、退款查詢、完成七天鎖定、一般會員訂單與未儲存編輯內容都需回歸。

## 文件依據

- https://developers.ecpay.com.tw/7418/：物流查詢及回應欄位。
- https://developers.ecpay.com.tw/7422/：逆物流通知及檢查碼。
- https://developers.ecpay.com.tw/7440/：貨態代碼以綠界最新表格為準；本次沿用專案既有對照並集中管理。

本次不包含新增取消物流單、改店、批次列印、測標或前台自助退貨申請。
