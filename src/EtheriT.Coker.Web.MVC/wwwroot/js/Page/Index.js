function PageReady() {
    initializeSiteCrawl();
    var menuEditor;
    var initialStateRestored = false;
    const myOffcanvas = new bootstrap.Offcanvas('#offcanvasSite');
    const menuForm = new CokerMenuEditorForm({
        getPageTypes: function () {
            return co.WebMesnus.GetPageTypeList();
        }
    });

    function initializeSiteCrawl() {
        const panel = document.getElementById("SiteCrawlOffcanvas");
        if (!panel) return;

        const field = name => document.getElementById("siteCrawl" + name);
        const websiteId = panel.dataset.websiteId;
        const activeStatuses = ["Queued", "Running", "CancelRequested"];
        const routerPattern = /^[A-Za-z0-9][A-Za-z0-9_-]{0,49}$/;
        const keySeparator = "\u001f";
        const hints = {
            1: "輸入舊站首頁網址後按「偵測舊站」。偵測只讀取舊站，不會寫入新站。",
            2: "正在讀取舊站的主選單與列表，完成後會列出結構讓你勾選。",
            3: "勾選要匯入的頁面。路徑名稱已預先填好匯入後會用的名稱，需要時再改，然後按「下一步」。",
            4: "確認每個列表要匯入成文章還是商品，再按「開始匯入」。匯入會建立主導覽選單、把輪播附加到預設版頭、以舊站頁尾更新預設頁尾，儲存後立即影響前台。",
            5: "匯入進行中可以關閉面板，稍後再打開查看進度。",
            done: "匯入完成，可以打開「網站地圖」檢查選單。要再匯入請按「重新偵測」。",
            detectFailed: "偵測失敗，請確認網址後重新偵測。",
            importFailed: "匯入沒有完成，請看上方錯誤，重新偵測後再匯入。"
        };
        const logoStatusText = {
            imported: " 網站Logo已匯入。",
            unchanged: " 網站Logo與舊站相同。",
            kept: " 網站Logo已有設定，未覆蓋。"
        };
        let timer;
        let busy = false;
        let current;
        let currentMode;
        let reloadedTaskId;
        let reloadingTaskId;
        let nodeSeq = 0;
        let conflictTaskId;
        let conflictBusy = false;
        let removalBusy = false;
        // 偵測結果：{ taskId, sourceUrl, menus, lists }；confirmPage：確認畫面目前在第 3 步（頁面與路徑名稱）或第 4 步（列表類型）。
        let detected;
        let confirmPage = 3;
        // choices：列表 key → 是否當商品；menuBoxes：選單 key → 勾選框；listMenus：列表 key → 所屬主選單 key。
        const choices = new Map();
        const menuBoxes = new Map();
        const listMenus = new Map();

        function normalizeUrl(value) {
            try {
                const url = new URL(String(value ?? "").trim());
                url.hash = "";
                return url.href;
            } catch {
                return "";
            }
        }

        function detectionMatchesInput() {
            return !!detected && normalizeUrl(field("Url").value) === normalizeUrl(detected.sourceUrl);
        }

        function isRunning() {
            return activeStatuses.includes(current?.status);
        }

        // 匯入任務一律是第 5 步；偵測中第 2 步；有對得上網址的偵測結果是第 3 或 4 步；其餘第 1 步。
        function stepState() {
            if (current && currentMode === "import")
                return { step: 5, state: isRunning() ? "active" : current.status === "Succeeded" ? "done" : "error" };
            if (isRunning()) return { step: 2, state: "active" };
            if (detectionMatchesInput()) return { step: confirmPage, state: "active" };
            if (current?.status === "Failed") return { step: 2, state: "error" };
            return { step: 1, state: "active" };
        }

        function hintFor({ step, state }) {
            if (state === "done") return hints.done;
            if (state === "error") return step === 2 ? hints.detectFailed : hints.importFailed;
            return hints[step];
        }

        function renderSteps(steps) {
            field("Steps").querySelectorAll("li[data-step]").forEach(item => {
                const number = Number(item.dataset.step);
                const isCurrent = number === steps.step;
                item.classList.toggle("is-done", number < steps.step || (isCurrent && steps.state === "done"));
                item.classList.toggle("is-active", isCurrent && steps.state === "active");
                item.classList.toggle("is-error", isCurrent && steps.state === "error");
                if (isCurrent) item.setAttribute("aria-current", "step");
                else item.removeAttribute("aria-current");
            });
            field("Hint").textContent = hintFor(steps);
        }

        function includedCount() {
            return field("Menus").querySelectorAll("[data-role=include]:checked").length;
        }

        function updateButtons() {
            const running = isRunning();
            const matches = detectionMatchesInput();
            const steps = stepState();
            field("Url").readOnly = running;
            field("Detect").disabled = busy || running;
            field("Detect").classList.toggle("d-none", running);
            field("Detect").textContent = detected || steps.step === 5 ? "重新偵測" : "偵測舊站";
            field("Detect").classList.toggle("btn-primary", steps.step < 3);
            field("Detect").classList.toggle("btn-outline-primary", steps.step >= 3);
            field("Back").classList.toggle("d-none", running || !matches || confirmPage !== 4);
            field("Back").disabled = busy;
            field("Next").classList.toggle("d-none", running || !matches || confirmPage !== 3);
            field("Next").disabled = busy || includedCount() === 0;
            field("Start").classList.toggle("d-none", running || !matches || confirmPage !== 4);
            field("Start").disabled = busy;
            field("Stale").classList.toggle("d-none", !detected || matches);
            renderSteps(steps);
        }

        function showConfirmPage(page) {
            confirmPage = page;
            field("MenusPanel").classList.toggle("d-none", page !== 3);
            field("ListsPanel").classList.toggle("d-none", page !== 4);
            if (page === 4) refreshListVisibility();
            updateButtons();
        }

        function updateMenusCount() {
            const total = field("Menus").querySelectorAll("[data-role=include]").length;
            field("MenusCount").textContent = detected ? `選單 ${total} 項、匯入 ${includedCount()} 項` : "";
        }

        function isListIncluded(key) {
            const menuKey = listMenus.get(key);
            return !menuKey || menuBoxes.get(menuKey)?.checked !== false;
        }

        function updateListsCount() {
            const lists = detected?.lists.filter(item => isListIncluded(item.key)) ?? [];
            const products = lists.filter(item => choices.get(item.key)).length;
            field("ListsCount").textContent = detected ? `列表 ${lists.length} 個、商品 ${products} 個` : "";
        }

        // 上一步取消勾選的頁面上的列表不會匯入，進到列表類型時不列出。
        function refreshListVisibility() {
            let visible = 0;
            field("Lists").querySelectorAll("li[data-list-key]").forEach(row => {
                const included = isListIncluded(row.dataset.listKey);
                row.classList.toggle("d-none", !included);
                if (included) visible++;
            });
            field("ListsEmpty").classList.toggle("d-none", visible > 0);
            updateListsCount();
        }

        function createSourceLink(url) {
            const link = document.createElement("a");
            link.href = url;
            link.target = "_blank";
            link.rel = "noopener noreferrer";
            link.textContent = "舊站頁面";
            return link;
        }

        function createNote(text) {
            const note = document.createElement("span");
            note.className = "site-crawl-node__note";
            note.textContent = text;
            return note;
        }

        function createListSelect(item) {
            const select = document.createElement("select");
            select.className = "form-select form-select-sm";
            select.setAttribute("aria-label", `「${item.title}」匯入成`);
            select.dataset.listKey = item.key;
            select.dataset.locked = String(item.locked);
            for (const [value, text] of [["article", "文章"], ["product", "商品"]]) {
                const option = document.createElement("option");
                option.value = value;
                option.textContent = text;
                select.append(option);
            }
            select.value = choices.get(item.key) ? "product" : "article";
            select.disabled = item.locked;
            select.addEventListener("change", () => {
                choices.set(item.key, select.value === "product");
                updateListsCount();
            });
            return select;
        }

        // 輸入框預先填好「不改的話匯入後的路徑名稱」：既有選單是原本的，新選單是偵測時推算的 import-…；
        // 清空時退回這個預設值（placeholder 也顯示它）。
        function createRouterInput(menu) {
            const input = document.createElement("input");
            input.type = "text";
            input.className = "form-control form-control-sm";
            input.maxLength = 50;
            input.value = menu.routerName ?? "";
            input.placeholder = menu.routerName ?? "";
            input.autocomplete = "off";
            input.spellcheck = false;
            input.dataset.role = "router";
            input.dataset.suggested = menu.routerName ?? "";
            input.dataset.existing = String(menu.existingRoute);
            input.setAttribute("aria-label", `「${menu.title}」的路徑名稱`);
            input.addEventListener("input", () => {
                validateRouters();
                updateButtons();
            });
            return input;
        }

        function createRouterCell(menu) {
            const cell = document.createElement("div");
            cell.className = "site-crawl-node__router";
            if (menu.isHome) cell.append(createNote("Home（首頁固定）"));
            else if (menu.external) cell.append(createNote("外站連結，不需要路徑名稱"));
            else if (menu.duplicate) cell.append(createNote("和上方選單同一頁，不另建選單"));
            else if (menu.linkToParent) cell.append(createNote("就是上層那一頁，連到上層選單"));
            else cell.append(createRouterInput(menu));
            return cell;
        }

        function describeMenu(menu) {
            const notes = [];
            if (menu.isHome) notes.push("首頁，一定匯入");
            if (!menu.url) notes.push("下拉選單上層（沒有頁面）");
            if (menu.section) notes.push("左欄子頁");
            if (menu.category) notes.push("分類頁的子分類（不顯示在主選單，選單目錄會列出）");
            if (menu.existingRoute) notes.push("網站已有同名選單，會更新它");
            return notes.join("・");
        }

        function createMenuNode(menu) {
            const item = document.createElement("li");
            item.dataset.menuKey = menu.key;
            const row = document.createElement("div");
            row.className = "site-crawl-node";

            const main = document.createElement("div");
            main.className = "site-crawl-node__main form-check mb-0";
            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.className = "form-check-input";
            checkbox.id = "siteCrawlMenu" + (++nodeSeq);
            checkbox.checked = true;
            checkbox.disabled = menu.isHome;
            checkbox.dataset.role = "include";
            checkbox.dataset.fixed = String(menu.isHome);
            menuBoxes.set(menu.key, checkbox);
            const label = document.createElement("label");
            label.className = "form-check-label site-crawl-node__title";
            label.htmlFor = checkbox.id;
            label.textContent = menu.title;
            const meta = document.createElement("div");
            meta.className = "site-crawl-node__meta";
            const description = describeMenu(menu);
            if (description) meta.append(description + "　");
            if (menu.url) meta.append(createSourceLink(menu.url));
            main.append(checkbox, label, meta);

            row.append(main, createRouterCell(menu));
            item.append(row);
            checkbox.addEventListener("change", () => {
                setIncluded(item, checkbox.checked);
                validateRouters();
                updateMenusCount();
                updateButtons();
            });
            if (menu.children?.length) {
                const children = document.createElement("ul");
                children.append(...menu.children.map(child => createMenuNode(child)));
                item.append(children);
            }
            return item;
        }

        function createListRow(item) {
            const entry = document.createElement("li");
            entry.dataset.listKey = item.key;
            const row = document.createElement("div");
            row.className = "site-crawl-node site-crawl-node--list";
            const main = document.createElement("div");
            main.className = "site-crawl-node__main";
            const title = document.createElement("div");
            title.className = "site-crawl-node__title fw-semibold";
            title.textContent = item.title;
            const meta = document.createElement("div");
            meta.className = "site-crawl-node__meta";
            const menuKey = listMenus.get(item.key);
            const path = menuKey ? "主選單：" + menuKey.split(keySeparator).join(" > ") : item.path;
            if (path) meta.append(path + "　");
            meta.append(createSourceLink(item.url));
            const reason = document.createElement("div");
            reason.className = "site-crawl-node__meta";
            reason.textContent = `${item.kindName}列表・${item.reason}`;
            main.append(title, meta, reason);
            const type = document.createElement("div");
            type.className = "site-crawl-node__type";
            type.append(createListSelect(item));
            row.append(main, type);
            entry.append(row);
            return entry;
        }

        // 取消勾選＝整支不匯入：底下子選單一起取消並鎖住；重新勾選時整支恢復。
        function setIncluded(item, included) {
            [item, ...item.querySelectorAll("li[data-menu-key]")].forEach((node, index) => {
                const row = node.querySelector(":scope > .site-crawl-node");
                const checkbox = row.querySelector("[data-role=include]");
                checkbox.checked = included;
                checkbox.disabled = checkbox.dataset.fixed === "true" || (index > 0 && !included);
                row.classList.toggle("is-excluded", !included);
                row.querySelectorAll("[data-role=router]").forEach(control => {
                    control.disabled = !included;
                });
            });
        }

        // 全部選取／全部取消：逐一套用第一層（整支跟著變）；首頁一定匯入，只改它底下的子選單。
        function setAllMenus(included) {
            field("Menus").querySelectorAll(":scope > li[data-menu-key]").forEach(item => {
                const checkbox = item.querySelector(":scope > .site-crawl-node [data-role=include]");
                if (checkbox.dataset.fixed === "true")
                    item.querySelectorAll(":scope > ul > li[data-menu-key]").forEach(child => setIncluded(child, included));
                else
                    setIncluded(item, included);
            });
            validateRouters();
            updateMenusCount();
            updateButtons();
        }

        // 實際會用的路徑名稱：輸入框的值，清空時用預設值（既有選單的原名或推算的 import-…）。
        function effectiveRouter(input) {
            return input.value.trim() || input.dataset.suggested;
        }

        // 改過的路徑名稱：英數與 - _、最多 50 字、不能叫 Home；沒改的預設值不檢查格式（既有選單的原名可能是手動取的）。
        // 勾選的項目之間不能重複。
        function validateRouters() {
            const inputs = [...field("Menus").querySelectorAll("input[data-role=router]")];
            const counts = new Map();
            for (const input of inputs) {
                const value = effectiveRouter(input).toLowerCase();
                if (!input.disabled && value) counts.set(value, (counts.get(value) ?? 0) + 1);
            }
            let valid = true;
            for (const input of inputs) {
                const value = effectiveRouter(input);
                const changed = value !== input.dataset.suggested;
                const message = input.disabled ? ""
                    : changed && !routerPattern.test(value) ? "只能用英文、數字、「-」「_」，最多 50 字，開頭不能是符號。"
                    : changed && value.toLowerCase() === "home" ? "Home 是首頁專用的路徑名稱。"
                    : counts.get(effectiveRouter(input).toLowerCase()) > 1 ? "和其他選單的路徑名稱重複。" : "";
                input.setCustomValidity(message);
                input.classList.toggle("is-invalid", !!message);
                input.title = message;
                if (message) valid = false;
            }
            return valid;
        }

        function collectListMenus(menus) {
            for (const menu of menus) {
                if (menu.listKey && !listMenus.has(menu.listKey)) listMenus.set(menu.listKey, menu.key);
                collectListMenus(menu.children ?? []);
            }
        }

        // 只有新的偵測結果才重建畫面；輪詢同一個任務時保留使用者改過的勾選、路徑名稱與列表類型。
        function showDetection(status, result) {
            const isNew = detected?.taskId !== status.taskId;
            detected = {
                taskId: status.taskId, sourceUrl: result.sourceUrl,
                menus: result.menus ?? [], lists: result.lists ?? []
            };
            if (!isNew) return;
            choices.clear();
            menuBoxes.clear();
            listMenus.clear();
            for (const item of detected.lists) choices.set(item.key, item.product);
            if (!field("Url").value.trim()) field("Url").value = result.sourceUrl;
            if (detected.menus.length) {
                field("Menus").replaceChildren(...detected.menus.map(menu => createMenuNode(menu)));
            } else {
                const empty = document.createElement("li");
                empty.className = "small text-body-secondary";
                empty.textContent = "入口頁沒有辨識到主選單，無法匯入，請看上方錯誤。";
                field("Menus").replaceChildren(empty);
            }
            collectListMenus(detected.menus);
            field("Lists").replaceChildren(...detected.lists.map(createListRow));
            updateMenusCount();
            showConfirmPage(3);
        }

        function setAllChoices(product) {
            if (!detected) return;
            for (const item of detected.lists) {
                if (!item.locked) choices.set(item.key, product);
            }
            field("Lists").querySelectorAll("select[data-list-key]").forEach(select => {
                if (select.dataset.locked !== "true") select.value = product ? "product" : "article";
            });
            updateListsCount();
        }

        function countMenus(menus) {
            return (menus ?? []).reduce((total, menu) => total + 1 + countMenus(menu.children), 0);
        }

        function summarize(result, isDetection, isSucceeded) {
            if (result?.pagesScanned == null) return "";
            if (isDetection)
                return `偵測讀取 ${result.pagesScanned} 頁，選單共 ${countMenus(result.menus)} 項（含子選單）、列表 ${result.lists?.length ?? 0} 個。`;
            return `掃描 ${result.pagesScanned} 個內容頁，新增選單 ${result.menusCreated} 個、更新選單 ${result.menusUpdated ?? 0} 個。` +
                (result.unchanged != null ? ` 未變更，已略過 ${result.unchanged} 項。` : "") +
                (result.conflicts != null ? ` 衝突 ${result.conflicts} 項。` : "") +
                (isSucceeded && result.removed != null ? ` 舊站已刪除 ${result.removed} 項。` : "") +
                (isSucceeded && result.notScanned != null ? ` 這次沒掃到 ${result.notScanned} 項。` : "") +
                (result.menusExcluded > 0 ? ` 依勾選略過選單 ${result.menusExcluded} 項。` : "") +
                (result.pagesWithBanner == null ? "" :
                    ` 辨識到版頭 ${result.pagesWithBanner} 頁、頁尾 ${result.pagesWithFooter} 頁。`) +
                (result.bannerVotes > 0 || result.footerVotes > 0 ?
                    ` 新增輪播 ${result.slidesAdded} 張，頁尾${result.footerUpdated ? "已更新" : "未更新"}。` : "") +
                (logoStatusText[result.logoStatus] ?? "") +
                (result.articlesCreated > 0 || result.articlesUpdated > 0 ?
                    ` 文章新增 ${result.articlesCreated} 篇、更新 ${result.articlesUpdated} 篇。` : "") +
                (result.productsCreated > 0 || result.productsUpdated > 0 ?
                    ` 商品新增 ${result.productsCreated} 個、更新 ${result.productsUpdated} 個。` : "");
        }

        function renderImportDetails(panel, list, titles) {
            const items = titles ?? [];
            field(panel)?.classList.toggle("d-none", items.length === 0);
            field(list)?.replaceChildren(...items.map(title => {
                const item = document.createElement("li");
                item.textContent = title;
                return item;
            }));
        }

        const conflictKinds = { Menu: "選單", Article: "文章", Product: "商品", Footer: "頁尾", Marquee: "跑馬燈", Slider: "輪播" };
        const conflictSelects = () => [...field("ConflictRows").querySelectorAll("select")];

        function conflictLink(label, address) {
            if (!address || address.startsWith("//")) return null;
            try {
                const url = new URL(address, window.location.origin);
                if (!["http:", "https:"].includes(url.protocol)) return null;
                const link = document.createElement("a");
                link.textContent = label;
                link.href = url.href;
                link.target = "_blank";
                link.rel = "noopener noreferrer";
                return link;
            } catch {
                return null;
            }
        }

        function conflictRow(item) {
            const row = document.createElement("li");
            row.className = "list-group-item";
            const grid = document.createElement("div");
            grid.className = "row g-2 align-items-center";
            const heading = document.createElement("div");
            heading.className = "col-12 col-sm-6 fw-semibold text-break";
            heading.textContent = `${conflictKinds[item.kind]}｜${item.title}`;
            const links = document.createElement("div");
            links.className = "col-12 col-sm-4 d-flex flex-wrap gap-2 site-crawl-conflict-links";
            links.append(...[conflictLink("舊站", item.sourceUrl), conflictLink("新站", item.newUrl)].filter(Boolean));
            const choices = document.createElement("div");
            choices.className = "col-12 col-sm-2";
            const select = document.createElement("select");
            select.className = "form-select form-select-sm";
            select.dataset.conflictId = item.id;
            select.setAttribute("aria-label", `處理${heading.textContent}`);
            select.append(new Option("保留", "keep"), new Option("覆蓋", "overwrite"));
            choices.append(select);
            grid.append(heading, links, choices);
            row.append(grid);
            return row;
        }

        function updateConflictButtons() {
            const selects = conflictSelects();
            field("ConflictControls").classList.toggle("d-none", !selects.length);
            field("ApplyConflicts").classList.toggle("d-none", !selects.length);
            for (const name of ["OverwriteAll", "KeepAll", "ApplyConflicts"]) field(name).disabled = conflictBusy;
            for (const select of selects) select.disabled = conflictBusy;
        }

        const removalChecks = () => [...field("RemovalRows").querySelectorAll("input[type=checkbox]")];

        function updateRemovalControls() {
            const checks = removalChecks();
            const selected = checks.filter(x => x.checked).length;
            const selectAll = field("SelectAllRemoved");
            selectAll.checked = checks.length > 0 && selected === checks.length;
            selectAll.indeterminate = selected > 0 && selected < checks.length;
            selectAll.disabled = removalBusy;
            field("Unpublish").disabled = removalBusy || selected === 0;
            field("RemovalControls").classList.toggle("d-none", checks.length === 0);
            for (const check of checks) check.disabled = removalBusy;
        }

        function removalRow(item, canUnpublish) {
            const row = document.createElement("li");
            row.className = "list-group-item d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-2";
            const label = document.createElement(canUnpublish ? "label" : "span");
            label.className = canUnpublish ? "d-flex align-items-start gap-2 text-break" : "text-break";
            if (canUnpublish) {
                const check = document.createElement("input");
                check.type = "checkbox";
                check.value = item.id;
                check.dataset.kind = item.kind;
                check.className = "form-check-input flex-shrink-0";
                label.append(check);
            }
            label.append(`${conflictKinds[item.kind]}｜${item.title}`);
            row.append(label);
            const link = conflictLink("舊站", item.sourceUrl);
            if (link) {
                link.className = "flex-shrink-0";
                row.append(link);
            }
            return row;
        }

        function showRemovals(removed, notScanned) {
            field("RemovalReview").classList.toggle("d-none", !removed.length);
            field("RemovalFeedback").textContent = "";
            field("RemovalRows").replaceChildren(...removed.map(x => removalRow(x, true)));
            field("NotScanned").classList.toggle("d-none", !notScanned.length);
            field("NotScannedRows").replaceChildren(...notScanned.map(x => removalRow(x, false)));
            updateRemovalControls();
        }

        async function unpublishSelected() {
            if (removalBusy || !conflictTaskId) return;
            const taskId = conflictTaskId;
            const checks = removalChecks().filter(x => x.checked);
            if (!checks.length) return;
            removalBusy = true;
            updateRemovalControls();
            field("RemovalFeedback").textContent = "正在下架選取的項目。";
            try {
                const result = await co.SiteCrawl.unpublish(taskId, websiteId, checks.map(x => x.value));
                if (conflictTaskId !== taskId) return;
                const outcomes = result.outcomes ?? [];
                const done = new Set(outcomes.filter(x => x.status === "unpublished").map(x => x.id));
                for (const check of checks) {
                    if (done.has(check.value)) check.closest("li").remove();
                }
                const remaining = outcomes.filter(x => x.status !== "unpublished").map(x => x.title);
                field("RemovalFeedback").textContent = `已下架 ${done.size} 項。`
                    + (remaining.length ? ` 未能下架：${remaining.join("、")}。` : "");
                if (menuEditor && checks.some(x => x.dataset.kind === "Menu" && done.has(x.value)))
                    menuReload(menuEditor, myOffcanvas, () => {}, false);
            } catch (error) {
                if (conflictTaskId === taskId) field("RemovalFeedback").textContent = error.message;
            } finally {
                if (conflictTaskId === taskId) {
                    removalBusy = false;
                    updateRemovalControls();
                }
            }
        }

        field("RemovalRows")?.addEventListener("change", updateRemovalControls);
        field("SelectAllRemoved")?.addEventListener("change", event => {
            for (const check of removalChecks()) check.checked = event.target.checked;
            updateRemovalControls();
        });
        field("Unpublish")?.addEventListener("click", unpublishSelected);

        async function showConflicts(taskId, hasConflicts) {
            if (!field("ConflictReview") || taskId === conflictTaskId) return;
            conflictTaskId = taskId;
            conflictBusy = false;
            removalBusy = false;
            showRemovals([], []);
            field("ConflictRows").replaceChildren();
            field("ConflictReview").classList.toggle("d-none", !taskId || !hasConflicts);
            updateConflictButtons();
            if (!taskId) return;
            field("ConflictFeedback").textContent = "正在讀取待處理的衝突。";
            try {
                const result = await co.SiteCrawl.pending(taskId, websiteId);
                if (conflictTaskId !== taskId) return;
                const conflicts = result.conflicts ?? [];
                showRemovals(result.removed ?? [], result.notScanned ?? []);
                field("ConflictRows").replaceChildren(...conflicts.map(conflictRow));
                field("ConflictFeedback").textContent = conflicts.length ? "" : "目前沒有待處理的衝突。";
                if (conflicts.length) field("Conflicts")?.classList.add("d-none");
                updateConflictButtons();
            } catch (error) {
                if (conflictTaskId !== taskId) return;
                conflictTaskId = undefined;
                field(hasConflicts ? "ConflictFeedback" : "Error").textContent = error.message;
            }
        }

        async function applyConflicts() {
            if (conflictBusy || !conflictTaskId) return;
            const taskId = conflictTaskId;
            const selects = conflictSelects();
            const overwrite = selects.filter(x => x.value === "overwrite").map(x => x.dataset.conflictId);
            conflictBusy = true;
            updateConflictButtons();
            field("ConflictFeedback").textContent = "正在套用選擇。";
            try {
                const result = await co.SiteCrawl.applyConflicts(taskId, websiteId, overwrite);
                if (conflictTaskId !== taskId) return;
                const outcomes = result.outcomes ?? [];
                const handled = new Set(outcomes.filter(x => x.status === "applied" || x.status === "kept").map(x => x.id));
                for (const select of selects) {
                    if (handled.has(select.dataset.conflictId)) select.closest("li").remove();
                    else select.value = "keep";
                }
                const applied = outcomes.filter(x => x.status === "applied").length;
                const kept = outcomes.filter(x => x.status === "kept").length;
                const changed = outcomes.filter(x => x.status === "changed").map(x => x.title);
                const failed = outcomes.filter(x => x.status === "failed").map(x => x.title);
                field("ConflictFeedback").textContent = `已覆蓋 ${applied} 項，已保留 ${kept} 項。`
                    + (changed.length ? ` 新站內容已有變動，未覆蓋：${changed.join("、")}。` : "")
                    + (failed.length ? ` 套用失敗：${failed.join("、")}。` : "");
                if (menuEditor && outcomes.some(x => x.status === "applied" && x.id.startsWith("Menu-")))
                    menuReload(menuEditor, myOffcanvas, () => {}, false);
            } catch (error) {
                if (conflictTaskId === taskId) field("ConflictFeedback").textContent = error.message;
            } finally {
                if (conflictTaskId === taskId) {
                    conflictBusy = false;
                    updateConflictButtons();
                }
            }
        }

        field("OverwriteAll")?.addEventListener("click", () => conflictSelects().forEach(select => { select.value = "overwrite"; }));
        field("KeepAll")?.addEventListener("click", () => conflictSelects().forEach(select => { select.value = "keep"; }));
        field("ApplyConflicts")?.addEventListener("click", applyConflicts);

        function renderErrors(result) {
            const errors = (result?.errors ?? []).map(message => {
                const item = document.createElement("li");
                item.textContent = message;
                return item;
            });
            field("Errors").replaceChildren(...errors);
            for (const path of result?.queryLimitPaths ?? []) {
                const item = document.createElement("li");
                item.textContent = "已停止展開查詢參數：" + path;
                field("Errors").append(item);
            }
            if (result?.pageLimitReached || result?.errorCount > errors.length) {
                const item = document.createElement("li");
                item.textContent = result.pageLimitReached ? "已達掃描頁數上限。" : "其餘錯誤未列出。";
                field("Errors").append(item);
            }
        }

        function render(status) {
            const result = status?.resultJson ? JSON.parse(status.resultJson) : null;
            const isDetection = result?.mode === "detect";
            current = status;
            currentMode = isDetection ? "detect" : "import";
            field("ProgressPanel").classList.toggle("d-none", !status);
            field("Progress").value = status?.progress ?? 0;
            field("Message").textContent = status?.message ?? "";
            field("Error").textContent = status?.error ?? "";
            const canCancel = status?.status === "Queued" || status?.status === "Running";
            field("Cancel").classList.toggle("d-none", !canCancel);
            if (isDetection && status.status === "Succeeded") showDetection(status, result);
            else if (status && !isDetection) detected = undefined;
            field("ConfirmPanel").classList.toggle("d-none", !detected || isRunning());
            field("Summary").textContent = summarize(result, isDetection, status?.status === "Succeeded");
            renderImportDetails("Conflicts", "ConflictTitles", result?.conflictTitles);
            renderImportDetails("Unchanged", "UnchangedMenuTitles", result?.unchangedMenuTitles);
            renderErrors(result);
            const hasReview = result?.conflicts > 0 || result?.removed > 0 || result?.notScanned > 0;
            void showConflicts(!isDetection && status?.status === "Succeeded" && hasReview ? status.taskId : undefined, result?.conflicts > 0);

            if (menuEditor && status?.status === "Succeeded" &&
                (result?.menusCreated > 0 || result?.menusUpdated > 0) &&
                status.taskId !== reloadedTaskId && status.taskId !== reloadingTaskId) {
                const taskId = status.taskId;
                reloadingTaskId = taskId;
                menuReload(menuEditor, myOffcanvas, () => {
                    reloadedTaskId = taskId;
                }, false).always(() => {
                    if (reloadingTaskId === taskId) reloadingTaskId = undefined;
                });
            }
        }

        async function refresh(action) {
            if (busy) return;
            clearTimeout(timer);
            busy = true;
            updateButtons();
            field("Cancel").disabled = true;
            try {
                render(await action());
            } catch (error) {
                field("Error").textContent = error.message;
            } finally {
                busy = false;
                updateButtons();
                field("Cancel").disabled = false;
                if (isRunning()) {
                    timer = setTimeout(() => refresh(() => co.SiteCrawl.getStatus(current.taskId, websiteId)), 1000);
                }
            }
        }

        // mode：detect／import，排入後到伺服器回應前就先切到對的步驟。
        function queue(create, mode) {
            refresh(async () => {
                const task = await create();
                render({
                    taskId: task.taskId, status: "Queued", message: "等待伺服器開始處理",
                    resultJson: JSON.stringify({ mode })
                });
                return co.SiteCrawl.getStatus(task.taskId, websiteId);
            });
        }

        function goToLists() {
            if (busy || isRunning() || !detectionMatchesInput() || includedCount() === 0) return;
            if (!validateRouters()) {
                field("Menus").querySelector("input[data-role=router]:invalid")?.reportValidity();
                return;
            }
            showConfirmPage(4);
        }

        // 沒改的新選單也送出預設的推算名稱，匯入結果就和畫面一致（排除項目不會讓後面的流水號跟著變）；
        // 沒改的既有選單送 null，沿用原本的路徑名稱。
        function startImport() {
            if (busy || isRunning() || !detectionMatchesInput()) return;
            if (!validateRouters()) {
                showConfirmPage(3);
                field("Menus").querySelector("input[data-role=router]:invalid")?.reportValidity();
                return;
            }
            const menus = [...field("Menus").querySelectorAll("li[data-menu-key]")].map(item => {
                const row = item.querySelector(":scope > .site-crawl-node");
                const input = row.querySelector("[data-role=router]");
                const value = input ? effectiveRouter(input) : "";
                const keepExisting = input?.dataset.existing === "true" && value === input.dataset.suggested;
                return {
                    key: item.dataset.menuKey,
                    include: row.querySelector("[data-role=include]").checked,
                    routerName: keepExisting ? null : value || null
                };
            });
            const productLists = detected.lists
                .filter(item => choices.get(item.key) && isListIncluded(item.key)).map(item => item.key);
            queue(() => co.SiteCrawl.start(detected.sourceUrl, websiteId, productLists, menus), "import");
        }

        panel.addEventListener("shown.bs.offcanvas", () => {
            refresh(() => co.SiteCrawl.latest(websiteId));
        });
        // 表單送出（Enter 或「偵測舊站」）一律是偵測；匯入要走完確認步驟按「開始匯入」，避免沒確認就匯入。
        field("Form").addEventListener("submit", event => {
            event.preventDefault();
            if (busy || isRunning()) return;
            queue(() => co.SiteCrawl.detect(field("Url").value, websiteId), "detect");
        });
        field("MenusAll").addEventListener("click", () => setAllMenus(true));
        field("MenusNone").addEventListener("click", () => setAllMenus(false));
        field("Next").addEventListener("click", goToLists);
        field("Back").addEventListener("click", () => showConfirmPage(3));
        field("Start").addEventListener("click", startImport);
        field("Url").addEventListener("input", updateButtons);
        field("AllArticles").addEventListener("click", () => setAllChoices(false));
        field("AllProducts").addEventListener("click", () => setAllChoices(true));
        field("Cancel").addEventListener("click", () => {
            refresh(() => co.SiteCrawl.cancel(current.taskId, websiteId));
        });
    }

    function reopenMenuItem(id) {
        const targetId = String(id);
        const $target = $("#myEditor").find("li").filter(function () {
            const itemId = $(this).data("id") ?? $(this).data("Id");
            return String(itemId) === targetId;
        }).first();

        if (!$target.length) return;

        $target.parents("li").each(function () {
            const $parent = $(this);
            const $opener = $parent.find(".sortableListsOpener").first();
            if ($parent.hasClass("sortableListsClosed") && $opener.length) {
                $opener.trigger("mousedown");
            }
        });

        $("#myEditor").find("li.selectItem").removeClass("selectItem");
        $target.addClass("selectItem");
        $target.find(".btnEdit").first().trigger("click");
    }

    function findMenuItem(id) {
        const targetId = String(id);
        return $("#myEditor").find("li").filter(function () {
            const itemId = $(this).data("id") ?? $(this).data("Id");
            return String(itemId) === targetId;
        }).first();
    }

    function openMenuItemState(state) {
        const $target = findMenuItem(state.id);
        const buttonSelector = state.mode === "canvas" ? ".btnPage" : ".btnEdit";
        const $button = $target.find(buttonSelector).first();
        if (!$target.length || !$button.length) {
            pageState.clear();
            myOffcanvas.show();
            return;
        }

        $target.parents("li").each(function () {
            const $parent = $(this);
            const $opener = $parent.find(".sortableListsOpener").first();
            if ($parent.hasClass("sortableListsClosed") && $opener.length) {
                $opener.trigger("mousedown");
            }
        });

        $("#myEditor").find("li.selectItem").removeClass("selectItem");
        $target.addClass("selectItem");
        $button.trigger("click");
    }

    const pageState = Coker.HashPage.createEditorState({
        onRestore: openMenuItemState
    });

    function restoreInitialState() {
        if (initialStateRestored) return;
        initialStateRestored = true;
        pageState.restore();
    }

    var editor = grapesInit({
        restoreHistory: { source: "WebMenu" },
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.WebMesnus.saveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.WebMesnus.importConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(result.error);
            });
            return _dfr.promise();
        }
    });
    let editorStting = {
        textConfirmDelete: "是否確認將<span class='ConfirmKeyWord'>{0}</span>選單刪除?",
        listOptions: {
            placeholderCss: { 'background-color': "#cccccc" }
        },
        iconPicker: {
            searchText: "Buscar...", labelHeader: "{0}/{1}"
        },
        maxLevel: -1, // (Optional) Default is -1 (no level limit)
        element: {
            Form: "#frmEdit",
            Update: "#btnUpdate",
            Add: '#btnAdd',
            Refresh: '#btnRefresh',
            moveEnable: "#moveEnable"
        },
        on: {
            ready: function () {
                menuForm.initialize().fail(function (error) {
                    co.sweet.error(
                        "載入失敗",
                        typeof error === "string" ? error : "無法取得頁面類型"
                    );
                });
            },
            edit: function (data) {
                menuForm.prepareEdit();
                pageState.replace("edit", data.id);
            },
            loadEditData: function (summary) {
                return co.WebMesnus.getEditorDetail(summary.id).then(function (result) {
                    if (!result.success || !result.item) {
                        return $.Deferred()
                            .reject(result.error || "無法取得選單資料")
                            .promise();
                    }

                    return result.item;
                });
            },
            editLoadError: function (error) {
                const message = typeof error === "string"
                    ? error
                    : error?.responseJSON?.error || error?.statusText || "無法取得選單資料";
                co.sweet.error("載入失敗", message);
            },
            del: function (data) {
                if ($("#myEditor>li").length == 0) {
                    $("#myEditor").addClass("d-none");
                    $("#myEditor + .emptyList").removeClass("d-none");
                }
                co.WebMesnus.delete(data.id).done(function (result) {
                    if (result.success) co.sweet.success("已成功刪除");
                    else co.sweet.error(result.error);
                });
            },
            validate: async function (data) {
                return await menuForm.validate(data);
            },
            add: async function (cEl) {
                var data = cEl.data();
                const check = await this.validate(data);
                if (!check) return;

                $("#myEditor").removeClass("d-none");
                $("#myEditor + .emptyList").addClass("d-none");
                const $selected = $("#myEditor").find("li.selectItem").first();
                if ($selected.length > 0) {
                    const f_data = $selected.data() || {};
                    data.fK_TopNodeId = f_data.id;
                    data.level = (f_data.level || 0) + 1;
                    data.fK_RootNodeId = f_data.fK_RootNodeId || f_data.id;
                }
                co.WebMesnus.createOrEdit(data).done(function (result) {
                    if (!result.success) co.sweet.error(result.error);
                    else {
                        data.id = parseInt(result.message);
                        var ico_success = 0, img_success = 0, overimg_success = 0;

                        var $ico_file = menuForm.getUploadInput("icon");
                        if (typeof ($ico_file.data("file")) != "undefined" && $ico_file.data("file") != null) {
                            var formData = new FormData();
                            formData.append("files", $ico_file.data("file").File);
                            formData.append("type", 9);
                            formData.append("sid", data.id);
                            formData.append("serno", 500);
                            co.File.Upload(formData).done(function (result) {
                                if (result.success) ico_success = 1;
                                else ico_success = -1;
                            });
                        } else ico_success = 1;

                        var $file = menuForm.getUploadInput("image");
                        if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null) {
                            var formData = new FormData();
                            formData.append("files", $file.data("file").File);
                            formData.append("type", 2);
                            formData.append("sid", data.id);
                            formData.append("serno", 500);
                            co.File.Upload(formData).done(function (result) {
                                if (result.success) img_success = 1;
                                else img_success = -1;
                            });
                        } else img_success = 1;

                        var $over_file = menuForm.getUploadInput("overImage");
                        if (typeof ($over_file.data("file")) != "undefined" && $over_file.data("file") != null) {
                            var formData = new FormData();
                            formData.append("files", $over_file.data("file").File);
                            formData.append("type", 3);
                            formData.append("sid", data.id);
                            formData.append("serno", 500);
                            co.File.Upload(formData).done(function (result) {
                                if (result.success) overimg_success = 1;
                                else overimg_success = -1;
                            });
                        } else overimg_success = 1;

                        const timmer = function () {
                            if (ico_success != 0 && img_success != 0 && overimg_success != 0) {
                                menuForm.clearUploads();
                                menuReload(menuEditor, myOffcanvas, function () {
                                    reopenMenuItem(data.id);
                                });
                                if (!result.success) co.sweet.error(result.error);
                                else {
                                    if (ico_success == -1 || img_success == -1 || overimg_success == -1) co.sweet.erro("圖片上傳失敗");
                                    else co.sweet.success("新增成功");
                                }
                            } else setTimeout(timmer, 100);
                        };
                        setTimeout(timmer, 100);
                    }
                });
            },
            update: async function (data) {
                const check = await this.validate(data);
                if (!check) return;

                co.WebMesnus.createOrEdit(data).done(function (result) {
                    if (!result.success) co.sweet.error(result.error);
                    else {
                        var iconimg_success = 0, img_success = 0, overimg_success = 0, deliconimg_success = 0, delimg_success = 0, deloverimg_success = 0;

                        var $icon_del_list = menuForm.getDeleteList("icon");
                        if ($icon_del_list != null) {
                            co.File.DeleteFileById({
                                sid: data.id,
                                type: 9,
                                fid: $icon_del_list,
                            }).done(function (result) {
                                if (result.success) deliconimg_success = 1
                                else deliconimg_success = -1
                                data.IconUrl = "";
                                data.IconId = "";
                            });
                        } else deliconimg_success = 1

                        var $del_list = menuForm.getDeleteList("image");
                        if ($del_list != null) {
                            co.File.DeleteFileById({
                                sid: data.id,
                                type: 2,
                                fid: $del_list,
                            }).done(function (result) {
                                if (result.success) delimg_success = 1
                                else delimg_success = -1
                            });
                        } else delimg_success = 1

                        var $over_del_list = menuForm.getDeleteList("overImage");
                        if ($over_del_list != null) {
                            co.File.DeleteFileById({
                                sid: data.id,
                                type: 3,
                                fid: $over_del_list,
                            }).done(function (result) {
                                if (result.success) deloverimg_success = 1
                                else deloverimg_success = -1
                            });
                        } else deloverimg_success = 1
                        const del_timmer = function () {
                            if (deliconimg_success != 0 && delimg_success != 0 && deloverimg_success != 0) {
                                if (deliconimg_success == 1) {
                                    var $file = menuForm.getUploadInput("icon");
                                    if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null && $file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $file.data("file").File);
                                        formData.append("type", 9);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) iconimg_success = 1;
                                            else iconimg_success = -1;
                                        });
                                    } else iconimg_success = 1;
                                } else iconimg_success = -1;

                                if (delimg_success == 1) {
                                    var $file = menuForm.getUploadInput("image");
                                    if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null && $file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $file.data("file").File);
                                        formData.append("type", 2);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) img_success = 1;
                                            else img_success = -1;
                                        });
                                    } else img_success = 1;
                                } else img_success = -1;

                                if (deloverimg_success == 1) {
                                    var $over_file = menuForm.getUploadInput("overImage");
                                    if (typeof ($over_file.data("file")) != "undefined" && $over_file.data("file") != null && $over_file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $over_file.data("file").File);
                                        formData.append("type", 3);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) overimg_success = 1;
                                            else overimg_success = -1;
                                        });
                                    } else overimg_success = 1;
                                } else overimg_success = -1;

                                const timmer = function () {
                                    if (iconimg_success != 0 && img_success != 0 && overimg_success != 0) {
                                        menuForm.clearUploads();
                                        menuReload(menuEditor, myOffcanvas, function () {
                                            reopenMenuItem(data.id);
                                        });
                                        if (iconimg_success == -1 || img_success == -1 || overimg_success == -1) co.sweet.error("圖片上傳失敗");
                                        else co.sweet.success("儲存成功");
                                    } else setTimeout(timmer, 100);
                                }
                                setTimeout(timmer, 100);
                            } else setTimeout(del_timmer, 100);
                        }
                        setTimeout(del_timmer, 100);
                    }
                });
                
            },
            drop: function (cEl) {
                let saveList = [];
                let ps = cEl.parents('li');
                let root = ps.last();
                let fa = ps.first();
                let ul = cEl.parents('ul');
                let fK_TopNodeId, fK_RootNodeId;
                let isAdd = false;
                if (fa.length == 0) {
                    fK_TopNodeId = null;
                } else {
                    fK_TopNodeId = fa.data("id");
                }
                if (root.length == 0) {
                    fK_RootNodeId = null;
                } else {
                    fK_RootNodeId = root.data("id");
                }
                if (cEl.data("fK_TopNodeId") != fK_TopNodeId || cEl.data("fK_RootNodeId") != fK_RootNodeId) {
                    cEl.data({
                        "fK_TopNodeId": fK_TopNodeId,
                        "fK_RootNodeId": fK_RootNodeId
                    });
                    isAdd = true;
                    saveList.push($(cEl).data());
                }

                ul.children("li").each(function (index, element) {
                    var s = $(element).data("serNO");
                    if (s != (index + 1)) {
                        s = index + 1;
                        $(element).data("serNO", s);
                        if ($(element).data("id") != cEl.data("id")) saveList.push($(element).data());
                        else if (!isAdd) saveList.push($(element).data());
                    }
                });
                co.WebMesnus.updateLevelAndSerNo(saveList).done(function (result) {
                    if (!result.success) co.sweet.error(result.error);
                });
            },
            page: function (data) {
                $("#gjs").data("id", data.id);
                $("#gjs").removeClass("d-none");
                $("#gjs + .emptyList").addClass("d-none");
                co.WebMesnus.getConten(data.id).done(function (result) {
                    if (result.success) {
                        var html = co.Data.HtmlDecode(result.conten.saveHtml);
                        co.Grapes.setEditor(editor, html, result.conten.saveCss);
                        co.Grapes.setFile(editor, data.id, 1);
                        $("body").addClass("grapesEdit");
                        $("#TopLine .title").text(data.text);
                        pageState.replace("canvas", data.id);
                        myOffcanvas.hide();
                    } else {
                        pageState.clear();
                        myOffcanvas.show();
                        co.sweet.error(result.error);
                    }
                });
            },
            updateMenuEditorAddTitle: updateMenuEditorAddTitle
        }, btn: [
            // 1) visible：標題後面的 radio+label
            {
                key: 'visible',
                position: 'title',
                render: function (ctx) {
                    var id = ctx.data.id || ('new_' + ctx.$li.index());
                    var name = 'visible_' + id;
                    var $wrap = $('<span class="menu-visible-toggle ms-2">');
                    var $radio = $('<input type="checkbox">').addClass("selectedItem")
                        .attr({ 'id': name }, 'name', name);
                    var $label = $('<label>')
                        .attr({
                            'for': name,
                            'title': '切換前台顯示'
                        }).append(`<span class="material-symbols-outlined">visibility</span><span class="material-symbols-outlined">visibility_off</span>`);

                    $wrap.append($radio).append($label);
                    return $wrap;
                },
                init: function (ctx) {
                    var visible = ctx.data.visible === true
                        || ctx.data.visible === 1
                        || ctx.data.visible === 'true';

                    var $checkbox = ctx.$button.find('input.selectedItem');
                    $checkbox.prop('checked', visible);
                },
                click: function (ctx) {
                    var $checkbox = ctx.$button.find('input.selectedItem');
                    var newChecked = !$checkbox.prop('checked');
                    $checkbox.prop('checked', newChecked);

                    var newVisible = newChecked;           // 勾選 = 顯示
                    ctx.data.visible = newVisible;
                    ctx.$li.data(ctx.data);
                    co.WebMesnus.SetVisible(ctx.data.id, newVisible).done(function (result) {
                        if (!result.success) co.sweet.error("儲存失敗",result.error);
                    });
                    // 需要的話這裡打 API
                }
            },

            // 2) 後台權限設定：排序群後面、編輯前面
            {
                key: 'setPower',
                position: 'action',
                render: function (ctx) {
                    const hasPermission = ctx.data.hasBackstagePermission === true
                        || ctx.data.hasBackstagePermission === 1
                        || ctx.data.hasBackstagePermission === "true";

                    return $('<a class="btn btn-warning btn-sm permission-state-btn backstage-permission-btn">')
                        .toggleClass("has-permission", hasPermission)
                        .attr({
                            title: hasPermission ? "後台權限設定：已設定" : "後台權限設定：未設定",
                            "aria-label": hasPermission ? "後台權限設定，已設定" : "後台權限設定，未設定"
                        })
                        .append(`
                            <span class="permission-icon-wrap" aria-hidden="true">
                                <i class="fa-solid fa-user-shield permission-main-icon"></i>
                                <i class="fa-solid fa-circle-check permission-check-icon"></i>
                            </span>
                        `);
                },
                click: function (ctx) {
                    const data = ctx.data;
                    $("#PermissionDetailsModal").setData({ pageId: data.id, title: data.text, type: 0 }).modal("show");
                }
            },

            // 3) 前台瀏覽權限設定：跟 setPower 並排
            {
                key: 'setFrontPower',
                position: 'action',
                permission: hasRole,
                render: function (ctx) {
                    const hasPermission = ctx.data.hasFrontPermission === true
                        || ctx.data.hasFrontPermission === 1
                        || ctx.data.hasFrontPermission === "true";

                    return $('<a class="btn btn-info btn-sm permission-state-btn front-permission-btn">')
                        .toggleClass("has-permission", hasPermission)
                        .attr({
                            title: hasPermission ? "前台瀏覽權限：已設定" : "前台瀏覽權限：未設定",
                            "aria-label": hasPermission ? "前台瀏覽權限，已設定" : "前台瀏覽權限，未設定"
                        })
                        .append(`
                            <span class="permission-icon-wrap" aria-hidden="true">
                                <i class="fa-solid fa-user-group permission-main-icon"></i>
                                <i class="fa-solid fa-circle-check permission-check-icon"></i>
                            </span>
                        `);
                },
                click: function (ctx) {
                    const data = ctx.data;
                    $("#RolesDetailsModal").setRolesData({ pageId: data.id, title: data.text, type: 4 }).modal("show");
                }
            }
        ]
    };
    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.superManager) delete editorStting.on.setPower;
        menuEditor = new MenuEditor('myEditor', editorStting);
        $("#PermissionDetailsModal, #RolesDetailsModal")
            .off("hidden.bs.modal.permissionStateReload")
            .on("hidden.bs.modal.permissionStateReload", function () {
                if (menuEditor) {
                    menuReload(menuEditor, myOffcanvas);
                }
        });
        $('#offcanvasSite').on('show.bs.offcanvas', function () {
            menuForm.close();
        });
        $('#offcanvasSite').on("click", ".btn-close", function (e) {
            e.preventDefault();
            if ($("#offcanvasSite.offcanvas-lg").length > 0) {
                menuForm.close();
                pageState.clear();
            }
            else myOffcanvas.hide();
        });
        $("#btnExtend").on("click", function () {
            menuForm.prepareAdd();
            $("#myEditor .editItem").removeClass("editItem");
            updateMenuEditorAddTitle();
        });

        const initialState = pageState.getState();
        menuReload(
            menuEditor,
            myOffcanvas,
            restoreInitialState,
            !initialState || initialState.mode !== "canvas"
        );
    });
    /*$(".material-symbols-outlined").each(function () {
        console.log(`"${$(this).text().trim()}"`);
    });*/
    /*$($.iconset_fontawesome_6.icons).each(function () {
        console.log(`"${this.replace(/[-]{3}[\w]{2,4}$/g,"")}"`);
    });*/
}

function updateMenuEditorAddTitle() {
    const $editItem = $("#myEditor").find("li.editItem").first();
    const $selected = $("#myEditor").find("li.selectItem").first();
    let titleText = "";
    if ($editItem.length !== 0) return;
    if ($selected.length === 0) {
        // 沒有選任何選單 → 新增主選單
        titleText = "新增主選單";
    } else {
        const data = $selected.data() || {};
        // 先用 data.text，沒有再退回去抓畫面文字
        const menuTitle =
            (data.text && data.text.toString().trim()) ||
            $selected.find(".txt").first().text().trim() ||
            "選單";

        titleText = `新增「${menuTitle}」的子選單`;
    }

    $("#MenuEditorForm>.card-header>.title").text(titleText);
}

function menuReload(menuEditor, myOffcanvas, afterReload, showOffcanvas) {
    return co.WebMesnus.getAll().done(function (result) {
        if (result.success) {
            //console.log(result.maps)
            (menuEditor.setDataPreserve || menuEditor.setData).call(menuEditor, result.maps);
            //menuEditor.setData(result.maps);
            $("#myEditor").removeClass("d-none");
            if (result.maps.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
            if (showOffcanvas !== false) myOffcanvas.show();
            typeof afterReload === "function" && afterReload();
        } else {
            menuEditor.setData([]);
        }
    });
}
