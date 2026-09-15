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

- `src/services/http-client.ts` 是底層 Cookie、Anti-forgery 與 `401/403` 控制；功能頁統一由 `@/core/coker` 的 `api` 呼叫，不直接使用原生 `fetch`。
- `session-lifecycle.ts` 只在偵測到點擊、鍵盤或輸入等操作後回報活動，不會讓閒置頁面無限續期。
- 預設每 5 分鐘最多回報一次；資料庫 Session 剩餘 15 分鐘時，伺服器延長為 30 分鐘。
- 登入意外失效時保留目前 Vue 畫面，以目前帳號輸入密碼後接續原本操作。

## Coker 前端核心

功能頁由 `@/core/coker` 使用統一入口，不要直接散落原生 `fetch`：

```ts
import { api, rules, useManagedForm } from "@/core/coker";

const form = useManagedForm({
  id: "company-editor",
  initialValue: { Name: "", Email: "" },
  validation: {
    Name: [rules.required("請輸入公司名稱。")],
    Email: [rules.email()]
  },
  save: values => api.post("/api/companies", values),
  afterSave: () => {
    // 顯示成功訊息或重新載入清單
  }
});
```

`useManagedForm` 統一提供：

- `model`、`errors`、`isDirty`、`isSaving` 與 `lastSavedAt`。
- 儲存前欄位驗證及登入狀態驗證。
- 登入過期時等待密碼 Modal 驗證，成功後接續原本儲存。
- API 回傳 ASP.NET ModelState errors 時同步到欄位錯誤。
- `Ctrl+S`／`Cmd+S` 觸發目前頁面最後註冊的表單。
- `registerSavePipelineHooks` 註冊全站 before／after／error 流程。

`api` 提供 `get`、`post`、`put`、`patch`、`delete`，並統一加入 Cookie、Anti-forgery Token、Request ID、逾時與錯誤解析。API 路徑限定為同來源 `/api/*`。

後端所有 Controller 預設套用 Platform 權限及 `AutoValidateAntiforgeryToken`；Data Annotation／ModelState 的欄位錯誤會由 `api` 轉成 `ApiError.fieldErrors`。重新登入端點另外使用限時加密票證固定原帳號，並按來源 IP 限制為每分鐘五次。

## 新增功能頁

1. 在 `ClientApp/src/views` 建立 `.vue` 檔。
2. 在 `ClientApp/src/router/index.ts` 加入 route。
3. 在 `PlatformLayout.vue` 加入選單入口。
4. 若需要資料，在 `Controllers/Api` 建立受後端授權保護的 API。
