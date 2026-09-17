# Project working rules

本文件適用於本 Repository 內所有 Codex Thread。預設責任為：分析 → 修改必要原始碼 → 檢查 diff → 告訴使用者如何驗證。

除非使用者在當前 Thread 明確要求，否則不得自行執行本文件禁止的命令或流程。授權僅限當前 Thread 明確要求的範圍，不得將修改程式碼的要求視為 restore、build、test、bundle、migration 或 deploy 的授權。

# Default behavior

除非使用者在當前 Thread 明確要求，否則禁止主動執行：

- `dotnet build`
- `dotnet test`
- `dotnet publish`
- `dotnet restore`
- `dotnet ef migrations add`
- `dotnet ef database update`
- 任何 EF Core Migration 建立或套用
- 任何會修改資料庫 schema 或資料內容的 SQL
- `npm install`
- `npm ci`
- `npm run build`
- `npm run bundle`
- `npm run create-bundles`
- `create-bundles`
- `yarn install`
- `yarn build`
- `pnpm install`
- `pnpm build`
- `gulp`
- `webpack`
- `vite build`
- 任何前端 bundle / minify / generated assets 重新產生流程
- 套件升級
- 套件還原
- `git commit`
- `git push`
- `git pull`
- `git merge`
- `git rebase`
- `git reset`
- `git clean`
- `git checkout`
- `git stash`
- 部署相關命令
- IIS 操作
- 正式環境操作

不得透過別名、wrapper、script、IDE、自動化工具或其他間接方式執行上述未獲授權的操作。

# Preferred workflow

一般 Bug 修正、小型功能修改、程式碼調整時，預設流程為：

1. 從使用者指定的檔案或功能開始分析。
2. 只搜尋與問題直接相關的定義、引用與呼叫。
3. 不要無差別掃描整個 Repository，除非問題本身確實需要。
4. 只修改必要的原始碼。
5. 不做無關重構。
6. 不重新格式化大量無關程式碼。
7. 不修改 generated files，除非使用者明確要求。
8. 修改後檢查 Git diff。
9. 確認沒有誤改無關檔案；保留使用者既有修改，不得覆寫或回復。
10. 最後告訴使用者：
    - 修改了哪些檔案。
    - 每個檔案改了什麼。
    - 為什麼這樣修改。
    - 使用者需要手動驗證哪些項目。

# Validation policy

預設只做低成本驗證：

- 靜態閱讀修改內容。
- 搜尋直接引用。
- 檢查明顯語法與邏輯問題。
- 檢查 Git diff。
- 必要時可執行安全、快速、唯讀的搜尋命令，例如 `rg`、`git diff`、`git status`。

不要因為「想確認修改是否正確」就自動進入完整 build、test、bundle 或 migration 流程。

如果判斷 build、bundle、migration、test 或其他命令確實有必要，且使用者尚未在當前 Thread 明確要求，請不要自行執行。改為提供：

- 為什麼需要執行。
- 建議執行的完整指令。
- 該指令會做什麼。
- 是否會修改檔案、資料庫或產生大量產物。
- 是否可能需要較長時間；無法估計時明確說明。

由使用者自行決定是否執行。未執行的驗證不得宣稱已通過。

例如：

```text
建議你手動執行：
dotnet build xxx.csproj

原因：
此修改涉及 C# 型別或介面變更，需要確認編譯結果。

作用與影響：
編譯指定專案，可能自動還原相依套件並寫入 bin / obj 等編譯產物。
可能需要較長時間，實際耗時取決於專案與套件還原狀態。
```

或：

```text
建議你手動執行：
npm run create-bundles

原因：
此修改影響 bundle 來源檔。

作用與影響：
執行專案定義的 create-bundles 流程，重新產生前端資產並可能修改大量產物。
此命令可能需要較長時間，因此不自行執行。
```

建議指令應以已確認的專案設定為依據；上述範例不是自動執行授權，也不代表本專案一定存在該 script。

# Expensive command policy

除非使用者在當前 Thread 明確要求，對任何可能符合以下條件的命令，都不要自行執行：

- 預估可能超過 30 秒。
- 會大量讀寫磁碟。
- 會下載套件。
- 會 restore dependency。
- 會重新產生大量檔案。
- 會修改 lock file。
- 會修改 generated assets。
- 會修改資料庫。
- 會透過 Git 操作修改 Repository 狀態。
- 會影響部署環境。
- 會連正式環境。
- 會影響 Repository 以外的系統。

必要原始碼編輯及使用者要求的文件編輯屬於預設工作範圍；不得將其延伸為 Git 狀態操作或其他有副作用的命令。

這些情況請先依 Validation policy 告訴使用者建議執行的命令、原因、作用、副作用與可能耗時，由使用者自行決定。無法確認命令影響時，先閱讀直接相關的設定或 script，不要以執行方式探查。

# Database safety

未經使用者在當前 Thread 明確要求：

- 不建立 Migration。
- 不執行 Migration。
- 不更新資料庫。
- 不執行 `INSERT` / `UPDATE` / `DELETE` / `MERGE` / `ALTER` / `DROP` / `TRUNCATE`。
- 不執行可能改變資料或 schema 的 script。

如果認為資料庫需要變更，請只提供：

- 建議變更內容。
- 建議 Migration 名稱。
- 建議使用者執行的指令。
- 可能的資料相容性影響。

不要直接執行；並依 Validation policy 說明指令作用、副作用與可能耗時。

# Frontend generated assets

本專案可能有 npm、bundle、create-bundles 或其他前端資產產生流程。

除非使用者在當前 Thread 明確要求：

- 不重新產生 bundle。
- 不 minify。
- 不執行 create-bundles。
- 不重新生成 vendor assets。
- 不修改因 build 產生的檔案。
- 不執行 npm / yarn / pnpm install 或 build。

如果原始碼修改需要重新產生 bundle，請在最後提醒使用者應該手動執行哪條指令，並說明原因、作用、產物影響與可能耗時。

# Git safety

預設只允許安全的 Git 讀取操作，例如：

- `git status`
- `git diff`
- `git log`
- `git show`

除非使用者在當前 Thread 明確要求，不要自行執行任何會修改 Repository 狀態的 Git 指令，包括但不限於 commit、push、pull、merge、rebase、reset、clean、checkout、stash、switch、add、restore 或建立／刪除 branch。

# Scope control

如果使用者明確指定某個檔案，請優先從該檔案開始。

只有在必要時才往外追蹤：

- 直接引用
- Interface
- DTO
- Entity
- Controller
- Service
- View
- JavaScript
- Mapping
- Validation
- Database schema

不要因為可以搜尋整個專案，就自動做大範圍掃描或重構。

# User-controlled validation

本專案的最終執行驗證通常由使用者在 Visual Studio 或實際環境中手動完成。

Codex 的預設責任為：

分析 → 修改必要原始碼 → 檢查 diff → 告訴使用者如何驗證

不要自動延伸成：

修改 → restore → build → test → bundle → migration → deploy

除非使用者在當前 Thread 明確要求。
