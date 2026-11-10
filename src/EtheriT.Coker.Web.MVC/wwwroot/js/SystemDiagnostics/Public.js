(() => {
    "use strict";
    const root = document.getElementById("public-diagnostics");
    if (!root) return;
    const form = root.querySelector("#public-diagnostics-form"), run = root.querySelector("#public-run");
    const retry = root.querySelector("#public-retry"), stop = root.querySelector("#public-stop");
    const results = root.querySelector("#public-results"), message = root.querySelector("#public-message");
    const filters = [...root.querySelectorAll("[data-public-filter]")];
    let running = false, controller = null, all = location.hash === "#all";
    const groups = () => [...results.querySelectorAll("[data-public-group]")];
    function hide(element, hidden) { element.hidden = hidden; element.classList.toggle("d-none", hidden); }
    function update() {
        const hasResults = groups().some(group => group.dataset.completed === "true");
        const rows = [...results.querySelectorAll("[data-public-row]")];
        const count = status => rows.filter(row => row.dataset.status === status).length;
        root.querySelector("#public-counts").textContent = "已完成 " + groups().filter(group => group.dataset.completed === "true").length + " / " + groups().length +
            " 個抽樣頁面；通過 " + count("Passed") + " 項；異常 " + count("Failed") + " 項；無法確認 " + count("NeedsReview") + " 項；未檢測 " + count("NotRun") + " 項。";
        run.disabled = running || groups().length === 0;
        run.textContent = running ? "檢測中…" : hasResults ? "重新檢測前台" : "開始前台檢測";
        retry.disabled = running || !results.querySelector('[data-status="Failed"], [data-status="NeedsReview"]');
        hide(retry, !hasResults);
        for (const link of filters) hide(link, !hasResults);
        for (const group of groups()) {
            let visible = !hasResults;
            for (const row of group.querySelectorAll("[data-public-row]")) {
                hide(row, !all && row.dataset.status !== "Failed" && row.dataset.status !== "NeedsReview");
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
    stop.addEventListener("click", () => controller?.abort());
    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (running || run.disabled) return;
        const targets = groups().filter(group => event.submitter !== retry || group.querySelector('[data-status="Failed"], [data-status="NeedsReview"]'));
        running = true; controller = new AbortController(); hide(stop, false);
        for (const group of targets) {
            group.dataset.completed = "false";
            group.querySelectorAll("[data-public-row]").forEach(row => { row.dataset.status = "NotRun"; row.lastElementChild.textContent = "等待本次檢測"; });
        }
        update();
        let done = 0, warning = "";
        try {
            for (const group of targets) {
                message.textContent = "檢測 " + (done + 1) + " / " + targets.length + "：" + group.querySelector("summary").textContent;
                const body = new FormData(form); body.set("pageIndex", group.dataset.index);
                const timeout = setTimeout(() => controller.abort(), 180000);
                let response, html;
                try {
                    response = await fetch(root.dataset.runUrl, { method: "POST", credentials: "same-origin", body, signal: controller.signal });
                    html = await response.text();
                } finally { clearTimeout(timeout); }
                if (response.redirected || !response.ok) throw new Error(response.status === 409 ? "站台或檢測範圍已變更，請重新整理。" :
                    response.status === 429 ? "此帳號正在檢測，請稍後重試。" : "請求未完成（" + response.status + "），請確認登入與伺服器狀態。");
                const template = document.createElement("template"); template.innerHTML = html;
                const replacement = [...template.content.querySelectorAll("[data-public-group]")].find(item => item.dataset.index === group.dataset.index);
                if (!replacement || replacement.dataset.completed !== "true") throw new Error("未取得此頁的檢測結果。");
                warning = template.content.querySelector("[data-public-warning]")?.textContent || warning;
                group.replaceWith(replacement); done++; update();
                if (replacement.dataset.stopRun === "true") throw new Error("瀏覽器檢測尚未就緒或執行中止，已停止後續頁面。請確認檢測主機的瀏覽器、套件與執行權限，再重新檢測。");
            }
            message.textContent = "抽樣檢測完成（" + done + " / " + targets.length + "）。" + (warning || "最後結果已儲存。") + "預設僅顯示異常與無法確認項目；其餘可查看全部項目。";
        } catch (error) {
            message.textContent = error.name === "AbortError" ? "檢測已停止或逾時，已完成的頁面仍保留。" : error.message;
        } finally { running = false; controller = null; hide(stop, true); update(); }
    });
    update();
    message.textContent = groups().some(group => group.dataset.completed === "true") ? "已載入最後保存的前台結果，可複測需處理的頁面。" : "請開始前台檢測；本次僅檢查列出的公開頁面與抽樣資源。";
})();
