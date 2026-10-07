# MVC 與 Platform 部署設定

MVC（網站管理後台）與 Platform（客戶管理平台）共用登入 Cookie。
兩個專案的 appsettings.json 均列出 BackofficeAuthentication 欄位；
CookieDomain 與 DataProtectionKeysPath 的空字串是本機開發預設值，不是跨子網域正式部署的完整設定。
正式部署前必須在兩站的 appsettings.Production.json 或環境變數覆寫。
以下是設定片段，請合併到既有 JSON，保留其他設定，不要覆蓋整份檔案。

## MVC 正式環境範例

```json
{
  "SystemLinks": {
    "PlatformUrl": "https://platform.coker.ezsale.tw"
  },
  "BackofficeAuthentication": {
    "CookieDomain": ".coker.ezsale.tw",
    "DataProtectionKeysPath": "D:\\CokerShared\\BackofficeKeys"
  }
}
```

## Platform 正式環境範例

```json
{
  "SystemLinks": {
    "MvcUrl": "https://editor.coker.ezsale.tw"
  },
  "BackofficeAuthentication": {
    "CookieDomain": ".coker.ezsale.tw",
    "DataProtectionKeysPath": "D:\\CokerShared\\BackofficeKeys"
  }
}
```

## 新伺服器檢查清單

- 確認 ASPNETCORE_ENVIRONMENT 為 Production，且正式設定檔已部署；環境變數可能覆寫檔案設定。
- 兩站 ConnectionStrings:Default 指向同一個後台資料庫，且資料庫結構與發布版本相符。
- 將兩站 SystemLinks 改成實際網址；這些欄位只控制導覽連結，不會共享登入。
- 兩站 CookieDomain 使用相同、受信任的共同父網域；localhost 開發保留空字串。
- 兩站 DataProtectionKeysPath 指向同一組金鑰檔案。範例路徑需依伺服器調整；不同主機上相同的路徑名稱不代表共享金鑰。
- 建立金鑰資料夾，授予兩站 IIS App Pool 身分必要的讀寫權限；金鑰不可放在 wwwroot，不可隨發布清除，應限制存取並安全備份。
- 從舊伺服器移轉時保留原金鑰；若更換金鑰或 Cookie 網域，既有登入可能失效，需要重新登入。
- 兩站均套用設定並重啟，不是只發布 Platform；MVC 必須發布支援共用 Cookie 與資料庫 Session 的相容版本。
- 登入帳號需有系統維護角色（Role.Type = 0），且使用者、角色及對應關係均未刪除。
- 重新登入後，確認 platform 請求攜帶 .Coker6.Back.Auth Cookie，並測試登入、跨站切換與登出。不要分享 Cookie 值或連線密碼。

## Platform 網站主機與 Worker 設定

Platform 的 `WebsiteHosting` 決定網站編輯頁可選的主機、各版本預設主機與預設網址尾碼；啟動時會驗證，設定錯誤 Platform 會無法啟動，錯誤訊息會寫在啟動 Log。
`appsettings.json` 的 ServerId 全為 null（只有本機 LOCAL-DRYRUN）；正式環境要在 Platform 的 appsettings.Production.json 補上 Worker 與對應的 ServerId。

```json
{
  "Provisioning": {
    "Servers": [
      { "Id": "HOST-28", "DisplayName": "210.65.132.28", "IsDnsServer": false, "AllowedDnsZones": [] },
      { "Id": "HOST-30", "DisplayName": "210.65.132.30", "IsDnsServer": false, "AllowedDnsZones": [] },
      { "Id": "HOST-31", "DisplayName": "210.65.132.31", "IsDnsServer": false, "AllowedDnsZones": [] }
    ],
    "AgentApiKeys": {
      "HOST-28": "<每台不同的長隨機字串>",
      "HOST-30": "<每台不同的長隨機字串>",
      "HOST-31": "<每台不同的長隨機字串>"
    }
  },
  "WebsiteHosting": {
    "HostLocations": [
      { "Value": "210.65.132.28", "Text": "210.65.132.28", "ServerId": "HOST-28" },
      { "Value": "210.65.132.30", "Text": "210.65.132.30", "ServerId": "HOST-30" },
      { "Value": "210.65.132.31", "Text": "210.65.132.31", "ServerId": "HOST-31" },
      { "Value": "NAS封存", "Text": "NAS封存", "ServerId": null }
    ]
  }
}
```

### 主機 Worker 檢查清單

- Servers 的 Id、IsDnsServer、AllowedDnsZones 依實際主機調整；負責 DNS 的那台才設 IsDnsServer=true 並列出允許的 Zone。
- Provisioning:Servers 是陣列，覆寫時會依索引合併：正式設定的第一筆會蓋掉 appsettings.json 的 LOCAL-DRYRUN；若正式機不需要 LOCAL-DRYRUN，Servers 要從索引 0 開始列完整清單。
- AgentApiKeys 每台使用不同的長隨機字串，不要提交到 Git，可改用環境變數（例如 `Provisioning__AgentApiKeys__HOST-28`）。正式環境要把 LOCAL-DRYRUN 的金鑰保持空字串，它才無法通過驗證。
- 每台主機的 Worker 設定 `ProvisioningWorker:ServerId` 要等於上面的 Id，`ProvisioningWorker:ApiKey` 要等於對應的 AgentApiKeys，`PlatformBaseUrl` 指向正式 Platform 網址，並確認 `DryRun` 已改為 false。
- WebsiteHosting:HostLocations 也是依索引合併，正式設定要四筆寫齊且順序與 appsettings.json 相同；Value 是資料庫 Website.HostLocation 的既有字串，不可修改。
- HostLocations 的 ServerId 必須存在於 Provisioning:Servers，否則 Platform 無法啟動；沒有 Worker 的位置（NAS封存）填 null。
- 要改各版本預設主機或網址尾碼時，在正式設定覆寫 WebsiteHosting:DefaultHostByLevel（key 用簡約／形象／會員／購物）或 DefaultUrlSuffix（必須以 . 開頭）。
- 部署後到 Platform 的主機監控頁確認三台 Worker 都在線，再到網站編輯頁確認主機下拉選單與預設值正確。

## AccessDenied 注意事項

目前 Platform 的 LoginPath 與 AccessDeniedPath 都是 /Home/AccessDenied。
因此這個畫面可能代表未收到 Cookie、無法解密 Cookie、Session 無效，或真正沒有系統維護權限，不能只依畫面文字判定。
