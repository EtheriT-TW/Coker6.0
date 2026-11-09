(() => {
    "use strict";
    const root = document.getElementById("backoffice-diagnostics");
    if (!root) return;
    const results = root.querySelector("#diagnostics-results");
    const form = root.querySelector("#diagnostics-run-form");
    const run = root.querySelector("#diagnostics-run");
    const stop = root.querySelector("#diagnostics-stop");
    const message = root.querySelector("#diagnostics-message");
    const filters = [...root.querySelectorAll("[data-diagnostics-filter]")];
    let running = false, controller = null;
    let filter = window.location.hash === "#all" || (!window.location.hash &&
        new URL(window.location.href).searchParams.get("showAll") === "true") ? "all" : "failed";
    const groups = () => [...results.querySelectorAll("[data-diagnostics-group]")];
    function setHidden(element, hidden) { element.hidden = hidden; element.classList.toggle("d-none", hidden); }
    function update() {
        let visible = 0, passed = 0, failed = 0, skipped = 0, pending = 0, basics = 0;
        for (const group of groups()) {
            let count = 0;
            for (const row of group.querySelectorAll("[data-diagnostics-row]")) {
                const status = row.dataset.status;
                if (status === "Failed") failed++;
                if (row.dataset.basic === "true") { if (status !== "NotRun") basics++; }
                else if (status === "Passed") passed++;
                else if (status === "Skipped") skipped++;
                else if (status === "NotRun") pending++;
                setHidden(row, filter !== "all" && status !== "Failed");
                if (!row.hidden) count++;
            }
            setHidden(group, count === 0);
            if (count) visible++;
        }
        for (const link of filters) {
            const active = link.dataset.diagnosticsFilter === filter;
            link.classList.toggle("btn-secondary", active);
            link.classList.toggle("btn-outline-secondary", !active);
            if (active) link.setAttribute("aria-current", "true"); else link.removeAttribute("aria-current");
        }
        const summary = results.querySelector("[data-diagnostics-summary]");
        results.querySelector("[data-diagnostics-counts]").textContent =
            "適用操作項目 " + summary.dataset.total + " 項；通過 " + passed + " 項；未通過（含基本檢查） " + failed +
            " 項；未執行 " + (skipped + pending) + " 項；已完成頁面基本檢查 " + basics + " 項。";
        const empty = results.querySelector("[data-diagnostics-empty]");
        setHidden(empty, visible > 0);
        empty.textContent = summary.dataset.pagesCount === "0" ? "目前網站沒有適用的檢測項目。" : running ? "檢測進行中，暫未發現未通過項目。" :
            groups().some(group => group.dataset.hasRun === "true") ? "已執行的項目未發現異常；其餘未執行項目不代表通過，請查看全部檢測項目。" :
            "尚未執行檢測。可切換全部檢測項目查看範圍。";
        run.disabled = running || summary.dataset.pagesCount === "0";
    }
    function selectFilter(value) {
        filter = value === "all" ? "all" : "failed";
        const url = new URL(window.location.href);
        url.searchParams.delete("showAll"); url.hash = filter;
        history.replaceState(history.state, "", url.pathname + url.search + url.hash);
        update();
    }
    for (const link of filters) link.addEventListener("click", event => { event.preventDefault(); selectFilter(link.dataset.diagnosticsFilter); });
    window.addEventListener("hashchange", () => { filter = window.location.hash === "#all" ? "all" : "failed"; update(); });
    stop.addEventListener("click", () => controller?.abort());
    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (running || run.disabled) return;
        running = true;
        setHidden(stop, false);
        run.textContent = "檢測中…";
        const targets = groups();
        // Clear prior results before a new run so old successes cannot look current.
        for (const group of targets) {
            group.dataset.hasRun = "false";
            for (const row of group.querySelectorAll("[data-diagnostics-row]")) {
                row.dataset.status = "NotRun";
                row.lastElementChild.textContent = "尚未檢測";
            }
        }
        update();
        let completed = 0;
        controller = new AbortController();
        try {
            for (const group of targets) {
                if (controller.signal.aborted) throw new DOMException("Stopped", "AbortError");
                message.textContent = "正在檢測 " + (completed + 1) + " / " + targets.length + "：" + group.querySelector("summary").textContent;
                const body = new FormData(form);
                body.set("pageIndex", group.dataset.pageIndex);
                const timeout = setTimeout(() => controller.abort(), 120000);
                let response, html;
                try {
                    response = await fetch(root.dataset.runUrl, { method: "POST", credentials: "same-origin", body, signal: controller.signal });
                    html = await response.text();
                } finally { clearTimeout(timeout); }
                if (response.redirected) throw new Error("登入狀態已變更，請重新登入。");
                if (!response.ok) throw new Error(response.status === 409 ? "站台或功能權限已變更，請重新整理頁面後再試。" :
                    response.status === 429 ? "此站台已有你的檢測正在執行，請稍後再試。" :
                    response.status === 401 || response.status === 403 ? "登入已失效或沒有執行權限。" : "檢測未完成，請稍後重試或聯絡系統維護人員。");
                const template = document.createElement("template");
                template.innerHTML = html;
                const replacement = template.content.querySelector("[data-diagnostics-group]");
                if (!replacement) throw new Error("未取得檢測結果，請重新登入後再試。");
                replacement.dataset.pageIndex = group.dataset.pageIndex;
                group.replaceWith(replacement);
                completed++;
                update();
            }
            message.textContent = "檢測完成（" + completed + " / " + targets.length + "）。未執行項目不代表通過；結果保留於本頁。";
        } catch (error) {
            message.textContent = error.name === "AbortError" ? "檢測已停止或逾時，已完成的結果仍保留；其餘項目尚未檢測。" : error.message;
        } finally {
            running = false;
            controller = null;
            setHidden(stop, true);
            run.textContent = "重新檢測";
            update();
        }
    });
    selectFilter(filter);
    message.textContent = "結果僅保留於本頁，重新整理後需重新檢測。";
})();
