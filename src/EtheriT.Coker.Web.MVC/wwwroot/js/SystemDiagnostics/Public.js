(() => {
    "use strict";
    const root = document.getElementById("public-diagnostics");
    if (!root) return;
    const form = root.querySelector("#public-diagnostics-form"), run = root.querySelector("#public-run");
    const retry = root.querySelector("#public-retry"), stop = root.querySelector("#public-stop");
    const results = root.querySelector("#public-results"), message = root.querySelector("#public-message");
    const filters = [...root.querySelectorAll("[data-public-filter]")];
    let running = true, all = location.hash === "#all";
    const groups = () => [...results.querySelectorAll("[data-public-group]")];
    function hide(element, hidden) { element.hidden = hidden; element.classList.toggle("d-none", hidden); }
    function update() {
        const hasResults = groups().some(group => group.dataset.hasResults === "true" || group.dataset.completed === "true");
        const rows = [...results.querySelectorAll("[data-public-row]")];
        const count = status => rows.filter(row => row.dataset.status === status).length;
        root.querySelector("#public-counts").textContent = "已完成 " + groups().filter(group => group.dataset.completed === "true").length + " / " + groups().length +
            " 個抽樣頁面；通過 " + count("Passed") + " 項；異常 " + count("Failed") + " 項；無法確認 " + count("NeedsReview") + " 項；未檢測 " + count("NotRun") + " 項。";
        hide(run, running);
        run.disabled = running || groups().length === 0;
        run.textContent = running ? "檢測中…" : hasResults ? "重新檢測前台" : "開始前台檢測";
        retry.disabled = running || !results.querySelector(functionScan ? '[data-status="Failed"], [data-status="NeedsReview"], [data-status="NotRun"], [data-completed="false"]' : '[data-status="Failed"], [data-status="NeedsReview"]');
        hide(retry, running || !hasResults);
        for (const link of filters) hide(link, !hasResults);
        for (const group of groups()) {
            let visible = !hasResults || (functionScan && group.dataset.completed !== "true");
            for (const row of group.querySelectorAll("[data-public-row]")) {
                hide(row, !all && row.dataset.status !== "Failed" && row.dataset.status !== "NeedsReview" && !(functionScan && row.dataset.status === "NotRun"));
                if (!row.hidden) visible = true;
            }
            hide(group, !visible && !all);
        }
    }
    for (const link of filters) link.addEventListener("click", event => {
        event.preventDefault(); all = link.dataset.publicFilter === "all";
        const url = new URL(location.href); url.hash = all ? "all" : "issues";
        history.replaceState(history.state, "", url); update();
    });
    window.addEventListener("hashchange", () => { all = location.hash === "#all"; update(); });
    let pending = false, polling = false, revision = null, timer = null, disposed = false;
    const publicScan = true;
    const functionScan = root.dataset.functionScan === "true";
    const identity = () => {
        const body = new FormData(form);
        body.set("publicScan", String(publicScan));
        body.set("functionScan", String(functionScan));
        return body;
    };
    function endpoint(url) {
        const target = new URL(url, location.href);
        for (const key of ["websiteId", "scopeStamp", "publicScan", "functionScan"]) target.searchParams.set(key, identity().get(key));
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
                if (!template.content.querySelector("[data-public-summary]")) throw new Error("未取得檢測結果，請重新整理確認。");
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
