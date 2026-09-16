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

## AccessDenied 注意事項

目前 Platform 的 LoginPath 與 AccessDeniedPath 都是 /Home/AccessDenied。
因此這個畫面可能代表未收到 Cookie、無法解密 Cookie、Session 無效，或真正沒有系統維護權限，不能只依畫面文字判定。
