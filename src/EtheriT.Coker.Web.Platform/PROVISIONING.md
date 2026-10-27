# Provisioning 設定

主機只維護在 `Provisioning:Servers`；網站主機選單、版本預選、監控與 Worker 驗證皆讀取這份清單。`WebsiteHosting` 和 `Provisioning:AgentApiKeys` 已停用，遺留設定會在啟動時報錯。

## 環境設定

- 共用 `appsettings.json` 只放監控共用參數，Servers 為空、DefaultUrlSuffix 為空。
- `appsettings.Development.json` 提供 LOCAL-DRYRUN（127.0.0.1）與 NAS封存，網址尾碼為 `.localhost`。
- 正式／其他環境必須自行提供完整的 Servers 與 DefaultUrlSuffix，否則啟動驗證失敗。本次未新增正式主機或正式金鑰。
- 各環境應使用各自的資料庫、Server ID、金鑰與 Worker PlatformBaseUrl。此設定整合不會自動隔離資料庫或已部署的 Worker。
- ASP.NET Core 會依索引合併陣列，環境變數覆寫 Servers 時請依實際順序完整提供每筆欄位，避免殘留舊索引或 SupportedLevels。不要同時載入其他環境的主機設定。

## 每筆 Servers 欄位

| 欄位 | 用途 |
|---|---|
| Id | 唯一識別碼，有 Worker 時必須與其 ServerId 相同。 |
| DisplayName | 畫面顯示文字；未填時網站選單顯示 HostLocation。 |
| HostLocation | 寫入 Website.HostLocation 的值；保留既有資料使用的字串。只做 DNS 的伺服器可留 null。網址同步目前仍要求此值為 Worker 回報的 IP。 |
| SupportedLevels | 簡約／形象／會員／購物的陣列，取代 DefaultHostByLevel 設定。用於新增預選，不限制使用者手動選擇。 |
| ShowInMonitoring | true：列入監控、接受 Worker 驗證及操作。false：僅提供網站主機位置選項，不接受 Worker，適用 NAS封存。此欄位並非單純隱藏畫面。 |
| ApiKey | 該 Worker 的金鑰，不回傳前端。空值會拒絕驗證；建議由外部機密設定注入。 |
| IsDnsServer | 平台是否允許將 DNS 任務交給此 Worker。 |
| AllowedDnsZones | 允許操作的既有 DNS Zone，空陣列不允許任何 Zone。 |

只有一台主機宣告支援某版本時，新網站自動預選它；零台或多台時留空讓使用者選擇。新增期間換版本會重新預選／清空，編輯既有網站不自動改主機。封存位置的 SupportedLevels 請留空。

API 回應的 DefaultHostByLevel 是由 Servers 即時計算的表單資料，並非另一份設定清單。

## 舊設定搬移

1. 有 Worker 的主機保留原本 Id、HostLocation；把舊 DefaultHostByLevel 的對應版本放進該主機 SupportedLevels。
2. ArchiveLocations 每個值改為一筆 Servers，ShowInMonitoring=false、SupportedLevels=[]，HostLocation 保留原值。
3. AgentApiKeys 的值移到相同 Id 那筆 Servers.ApiKey。環境變數範例：`Provisioning__Servers__0__ApiKey`；需確認索引 0 是預期伺服器。
4. DefaultUrlSuffix 移至 Provisioning.DefaultUrlSuffix，刪除整個 WebsiteHosting 與舊 AgentApiKeys。
5. Worker 自己的 ProvisioningWorker 設定格式不變；DryRun 仍由 Worker 決定，平台顯示名稱不會開啟 Dry Run。

## 手動驗證

- Development 啟動後，網站主機選單包含本機與 NAS封存，監控只列出本機。
- 新增網站四個版本目前都預選本機；暫設兩台支援同版本，該版本應不預選。
- 編輯既有網站不因換版本而改主機；既有清單外位置仍可保留。
- Worker 與 Servers.ApiKey 配對後應可回報，錯誤／空金鑰或 ShowInMonitoring=false 應被拒絕。
- 舊設定、重複 Id／HostLocation、錯誤版本名稱及未設定網址尾碼應於啟動時報錯。
- 使用正式環境設定時，確認只有預期的正式主機與網址尾碼，沒有 LOCAL-DRYRUN。

前端原始碼有修改，需要自行於本專案目錄執行 `npm run build` 才會更新前端資產。此命令先檢查相依套件，必要時會自動執行 npm install（可能下載套件、修改 lock file），授權檔不存在時也會產生授權原始檔，之後進行型別檢查並產生 bundle；可能耗時較久。本次未執行。
