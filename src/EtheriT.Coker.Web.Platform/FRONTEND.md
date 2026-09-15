# Platform 前端資源流程

## 分層

1. `wwwroot/js/devextreme` 與 `wwwroot/lib` 是 vendor 層。它們提供 Razor Helper 所需的全域 `jQuery` 與 `DevExpress`，不匯入 Platform 的 ES module bundle。
2. `wwwroot/js/modules/platform.js` 是全站 entry point，負責 Layout、語系與全站初始化。
3. `wwwroot/js/modules/pages` 是頁面 entry points。每個頁面只載入自己的 entry，並從 `shared`、`vendor` 或 `shell` 匯入共用功能。
4. `wwwroot/dist` 是正式環境產物，不直接編輯，也不提交 Git。

## 載入方式

- Development：Razor 直接載入 `wwwroot/js/modules`，修改後重新整理即可，不依賴 bundle。
- 非 Development：Razor 載入 `wwwroot/dist/js`。部署前由開發者或 CI 明確產生 bundle。
- vendor scripts 放在 `<head>`，並且必須先於 `RenderBody()` 載入；DevExtreme Razor Helper 可能在 View 內直接輸出依賴全域 `jQuery` 與 `DevExpress` 的初始化程式。
- Platform 與 page modules 放在頁尾。它們不直接匯入 vendor 檔案，而是透過 `modules/vendor/devextreme.js` 取得已存在的全域物件。

## 指令

Platform 對外使用與 MVC 相同的必要指令；兩個專案背後可以採用不同的建置工具。

```text
npm install
npm run create-bundles
npm run build
```

- `create-bundles`：開發用資產，產生 source map、不壓縮。
- `build`：正式資產，壓縮並以 ESM code splitting 產生共用 chunks。

若 Platform 開發時需要持續監看檔案，可另外執行 `npm run watch`；它是開發輔助，不是必要的標準建置指令。

這些指令不掛入 .NET build，避免每次後端建置都重新處理前端資源。

## 新增頁面 entry

1. 在 `wwwroot/js/modules/pages` 建立來源檔。
2. 將它加入 `build/esbuild.mjs` 的 `entryPoints`。
3. 在對應 View 的 `Scripts` section 分別指定 Development 來源與正式環境產物。

不要把 module 檔加入 MVC 舊有的 Gulp concat 流程，也不要在來源檔中匯入 `.min.js`。
