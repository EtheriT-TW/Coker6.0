(() => {
    "use strict";
    const root = document.getElementById("backoffice-diagnostics");
    if (!root) return;
    const results = root.querySelector("#diagnostics-results");
    const form = root.querySelector("#diagnostics-run-form");
    const run = root.querySelector("#diagnostics-run");
    const retry = root.querySelector("#diagnostics-retry");
    const stop = root.querySelector("#diagnostics-stop");
    const message = root.querySelector("#diagnostics-message");
    const filters = [...root.querySelectorAll("[data-diagnostics-filter]")];
    let running = true;
    let filter = window.location.hash === "#all" || (!window.location.hash &&
        new URL(window.location.href).searchParams.get("showAll") === "true") ? "all" : "failed";
    const groups = () => [...results.querySelectorAll("[data-diagnostics-group]")];
    function setHidden(element, hidden) { element.hidden = hidden; element.classList.toggle("d-none", hidden); }
    function update() {
        const hasResults = groups().some(group => group.dataset.hasRun === "true");
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
                setHidden(row, !hasResults || (filter !== "all" && status !== "Failed"));
                if (!row.hidden) count++;
            }
            setHidden(group, count === 0);
            if (count) visible++;
        }
        for (const link of filters) {
            setHidden(link, !hasResults);
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
            hasResults ? "已執行的項目未發現異常；其餘未執行項目不代表通過，請查看全部檢測項目。" :
            "尚未執行檢測，請點選「開始檢測」。";
        setHidden(run, running);
        run.disabled = running || summary.dataset.pagesCount === "0";
        if (!running) run.textContent = hasResults ? "重新檢測" : "開始檢測";
        setHidden(retry, running || !hasResults);
        retry.disabled = running || failed === 0;
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
    let pending = false, polling = false, revision = null, timer = null, disposed = false;
    const publicScan = false;
    const identity = () => {
        const body = new FormData(form);
        body.set("publicScan", String(publicScan));
        return body;
    };
    function endpoint(url) {
        const target = new URL(url, location.href);
        for (const key of ["websiteId", "scopeStamp", "publicScan"]) target.searchParams.set(key, identity().get(key));
        return target;
    }
    async function request(url, options) {
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 20000);
        try {
            const response = await fetch(url, { credentials: "same-origin", cache: "no-store", ...options, signal: controller.signal });
            if (response.redirected || !response.ok) throw new Error(response.status === 409
                ? "站台或檢測範圍已變更，請重新整理。" : response.status === 429
                ? "已有背景檢測執行中，請稍後查看狀態。" : "無法取得檢測狀態，請確認登入與伺服器狀態。");
            return response;
        } finally { clearTimeout(timeout); }
    }
    function visibility(element, hidden) { element.hidden = hidden; element.classList.toggle("d-none", hidden); }
    async function refresh() {
        if (polling || pending || disposed) return;
        polling = true;
        try {
            const response = await request(endpoint(root.dataset.statusUrl));
            const state = await response.json();
            if (state.revision !== revision) {
                const html = await (await request(endpoint(root.dataset.resultsUrl))).text();
                const template = document.createElement("template");
                template.innerHTML = html;
                if (!template.content.querySelector("[data-diagnostics-summary]")) throw new Error("未取得檢測結果，請重新整理確認。");
                results.replaceChildren(template.content);
                revision = state.revision;
            }
            running = state.active;
            update();
            visibility(stop, !state.active || state.revision === "other");
            stop.disabled = !!state.stopping;
            message.textContent = state.message + (state.total ? " 已完成 " + state.completed + " / " + state.total + " 頁。" : "") +
                (state.active ? "離開頁面不會停止工作。" : "");
        } catch (error) {
            running = true; // Unknown state must not enable duplicate submissions.
            update();
            visibility(stop, true);
            message.textContent = error.name === "AbortError" ? "狀態查詢逾時，背景工作不受影響，稍後自動重試。" : error.message;
        } finally {
            polling = false;
            clearTimeout(timer);
            if (!disposed) timer = setTimeout(refresh, 5000);
        }
    }
    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (running || pending || polling || run.disabled) return;
        pending = true; running = true; update();
        const body = identity();
        body.set("retryOnly", String(event.submitter === retry));
        try {
            await request(root.dataset.runUrl, { method: "POST", body });
            revision = null;
            message.textContent = "已提交背景檢測，可離開頁面，稍後返回查看。";
        } catch (error) { message.textContent = error.name === "AbortError" ? "提交逾時，正在確認背景工作狀態，請勿重複提交。" : error.message; }
        finally { pending = false; await refresh(); }
    });
    stop.addEventListener("click", async () => {
        if (pending || stop.disabled) return;
        pending = true; stop.disabled = true;
        try { await request(root.dataset.stopUrl, { method: "POST", body: identity() }); }
        catch (error) { message.textContent = error.name === "AbortError" ? "停止要求逾時，正在確認狀態。" : error.message; }
        finally { pending = false; await refresh(); }
    });
    window.addEventListener("pagehide", () => { disposed = true; clearTimeout(timer); });
    window.addEventListener("pageshow", event => { if (event.persisted) { disposed = false; refresh(); } });
    update();
    refresh();
})();
