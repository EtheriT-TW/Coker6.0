# 單站 TLS 安裝與續期

TLS 憑證監控與伺服器監控頁共用 `services/site-tls.ts`，送出含 SiteName 與 HostNames 的 InstallSsl 任務。測試頁不再提供 SSL 安裝，API 拒絕沒有 SiteName 的 SSL 任務。

Worker 重新讀取 IIS，確認網域屬於指定網站，再唯讀盤點 renewal：

- 一筆既有設定：指定 renewal ID 強制續期，沿用原儲存與安裝設定。
- 多站共用一筆：沿用整筆續期；頁面建議逐站拆分。不自動改寫或取消共用設定。
- 多筆重疊：拒絕操作並要求整理，避免一次重複安裝。
- 沒有設定：建立固定 ID `coker-site-{siteId}`，僅包含該站來源，使用 CentralSsl。
- 既有設定限定部分網域：沿用它續期，提示人工編輯來源加入缺少網域。不另建重複設定。

目前只辨識包含 SourcePluginOptions.SiteIds（可為 null）與 IncludeHosts 的 IIS 來源格式。進階篩選只提供人工確認建議；若來源格式不能辨識或讀取失敗，阻止安裝以避免重複建立。這不是完整的網域覆蓋證明。

## 主機設定

`ProvisioningWorker:WacsRenewalDirectory` 必須指向 WacsPath 那套 win-acme 實際使用的 ConfigPath／ACME endpoint 目錄。預設為 `C:\ProgramData\win-acme\acme-v02.api.letsencrypt.org`。不要指向 Log、Certificates 或其他 win-acme instance 的目錄。

新建設定需啟用 IIS 中央憑證存放區，並提供 AcmeEmail 及與中央存放區一致的 TlsPfxPassword。密碼由主機安全設定提供，勿提交至 Repository。沿用舊設定時不更換舊設定的儲存方式。

盤點建議隨 TLS 每日採樣回報，啟動 Worker 時亦採樣一次。頁面重新整理不會即時採樣；安裝結果先看任務紀錄。DryRun 不會執行安裝或續期。

## 手動驗證

1. 無伺服器控制權限者看不到 TLS 安裝操作。
2. 兩個監控頁選同一網站，任務都含相同 SiteName，且只送該站 HostNames；離線時不能送出。
3. 測試頁不再出現 SSL 表單，沒有 SiteName 的舊 API 請求被拒絕。
4. 用測試主機的實際 renewal JSON 確認盤點格式；多站設定顯示拆分建議。
5. 已有單筆設定時指定原 ID 續期，renewal 數量不增加；多站共用設定的來源不被縮成單站。
6. 重複、設定檔不能辨識、網域不屬於網站等情況阻止建立。
   在測試主機取消某站唯一的 renewal 後重新採樣，確認「未發現 renewal」卡片計入該站、可篩選且列入需處理網站；目前憑證仍有效也應提醒。讀取失敗或進階篩選不能判定時，不應誤列為缺少 renewal。
7. 無設定時只建立該站固定 ID；再次執行沿用 ID，PFX 可由中央存放區讀取。
8. HTTP-only 網站有 HostNames 時可從 TLS 頁安裝，新增 binding 後沿用沒有網域限制的 IIS 來源。

下列命令由使用者決定執行，尚未執行：

```powershell
dotnet build src/EtheriT.Coker.Provisioning.Worker/EtheriT.Coker.Provisioning.Worker.csproj
dotnet build src/EtheriT.Coker.Web.Platform/EtheriT.Coker.Web.Platform.csproj
```

驗證 C# 編譯，可能還原套件並產生 bin/obj；耗時依套件與環境而定。

在 `src/EtheriT.Coker.Web.Platform` 目錄執行：

```powershell
npm run build
```

此流程先執行依賴與授權檢查，再檢查 TypeScript 並產生 Vite 資產；可能下載相依套件、產生授權檔及修改前端產物，可能超過 30 秒。未執行 bundle、套件還原或部署。
