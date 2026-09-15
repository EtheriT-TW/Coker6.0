# Platform 前端架構

## 權責

- ASP.NET Core：登入驗證、授權、API、SPA Host、AccessDenied 與 Error。
- Vue 3：Platform Layout、功能畫面與瀏覽器端狀態。
- Vue Router：`/companies`、`/websites` 等畫面路由。
- DevExtreme Vue：DataGrid 與後續 Platform UI 元件。
- Vite：Vue、TypeScript、CSS、code splitting、minify 與檔名 hash。

正常業務畫面放在 `ClientApp/src/views`，不要再建立同名 Razor View。Razor 只保留 SPA Host 與不依賴 JavaScript 的錯誤頁面。

## 安裝套件

Vite 8 需要 Node.js `20.19+` 或 `22.12+`；團隊應再統一固定一個符合條件的 LTS 版本。

套件已列入 `package.json`。開發者可以直接執行：

```text
npm run create-bundles
```

指令執行前會自動檢查 `package-lock.json`、`node_modules` 與直接相依套件版本。首次執行、套件清單改變或本機套件不完整時，才會自動執行 `npm install`；平常不會重複安裝。

相依套件完成後，流程也會檢查模組化的 `ClientApp/src/devextreme-license.ts`。若不存在，會以目前開發者電腦已註冊的 DevExpress 授權自動產生。該檔案已排除於 Git，不得提交或交由不同開發者共用。

若只想手動下載、不建立資產，仍可單獨執行 `npm install`。

若要清楚看到套件分類，等價的明確安裝指令為：

```text
npm install --save-exact vue@3.5.42 vue-router@5.3.1 devextreme@25.2.8 devextreme-vue@25.2.8
npm install --save-dev --save-exact vite@8 @vitejs/plugin-vue@6.0.8 typescript@5.9 vue-tsc@3.3.11
```

## 統一指令

Platform 對外使用與 MVC 相同的必要指令，背後由 Vite 執行：

```text
npm run create-bundles
npm run build
```

- `create-bundles`：開發監看。先產生不壓縮且包含 source map 的資產，之後持續監看檔案並自動重建；按 `Ctrl+C` 才會結束。
- `build`：先執行 Vue/TypeScript 型別檢查，再產生壓縮後的正式資產。

上述兩個指令都會先執行同一個 `scripts/ensure-dependencies.mjs`，新夥伴不需要另外記住首次安裝步驟。

`create-bundles` 與 `build` 也會在 Vite啟動前執行 `scripts/ensure-devextreme-license.mjs`。MVC 使用 non-modular license；Vue Platform 使用 modular TypeScript license，兩者不可直接交換載入方式。

輸出位於 `wwwroot/dist`，`.NET Build` 不會自動執行前端建置。未產生前端資源時，SPA Host 會顯示操作提示。

## 路由

- `/api/**`：ASP.NET Core API，不得由 Vue Router 使用。
- `/Home/AccessDenied`、`/Home/Error`：ASP.NET Core Razor fallback。
- 其他 Platform 畫面路徑：由 ASP.NET Core 回傳 SPA Host，再交由 Vue Router 解讀。

Vue Router 的畫面權限只控制使用者體驗；所有資料 API 仍必須由 ASP.NET Core 授權。

## 後台 Session

- 所有 Vue API 呼叫統一使用 `src/services/http-client.ts` 的 `platformFetch`，以便一致處理 `401` 與 `403`。
- `session-lifecycle.ts` 只在偵測到點擊、鍵盤或輸入等操作後回報活動，不會讓閒置頁面無限續期。
- 預設每 5 分鐘最多回報一次；資料庫 Session 剩餘 15 分鐘時，伺服器延長為 30 分鐘。
- 登入意外失效時保留目前 Vue 畫面，使用者可另開 MVC 登入後回來繼續。

## 新增功能頁

1. 在 `ClientApp/src/views` 建立 `.vue` 檔。
2. 在 `ClientApp/src/router/index.ts` 加入 route。
3. 在 `PlatformLayout.vue` 加入選單入口。
4. 若需要資料，在 `Controllers/Api` 建立受後端授權保護的 API。
