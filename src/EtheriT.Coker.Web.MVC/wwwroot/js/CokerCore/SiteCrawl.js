Coker.extend({
    SiteCrawl: (function () {
        const root = "/api/SiteCrawl/";

        async function request(action, options = {}, websiteId) {
            const headers = new Headers(_c.Data.Header);
            headers.set("Content-Type", "application/json; charset=utf-8");
            if (options.method === "POST") {
                const tokens = await request("RequestToken?websiteId=" + encodeURIComponent(websiteId), { cache: "no-store" });
                if (!tokens?.token || !tokens.headerName) throw new Error("無法取得表單驗證權杖。");
                headers.set(tokens.headerName, tokens.token);
            }

            const response = await fetch(root + action, {
                ...options,
                headers
            });
            if (response.status === 204) return null;
            const data = await response.json().catch(() => null);
            if (!response.ok || !data) {
                const message = response.status === 403
                    ? "沒有網站匯入權限，或目前站台已切換。"
                    : data?.title || "無法取得網站匯入任務資料。";
                const endpoint = action.split("?")[0];
                const validation = data?.errors && Object.values(data.errors).flat().join(" ");
                throw new Error(`${data?.message || validation || message}（${endpoint}，HTTP ${response.status}）`);
            }
            return data;
        }

        function taskQuery(taskId, websiteId) {
            return new URLSearchParams({ taskId, websiteId }).toString();
        }

        return {
            // productLists：確認畫面標成商品的列表 key，這次匯入先略過。
            start: (url, websiteId, productLists) => request("Start?websiteId=" + encodeURIComponent(websiteId), {
                method: "POST", body: JSON.stringify({ url, productLists: productLists ?? [] })
            }, websiteId),
            detect: (url, websiteId) => request("Detect?websiteId=" + encodeURIComponent(websiteId), {
                method: "POST", body: JSON.stringify({ url })
            }, websiteId),
            latest: websiteId => request("Latest?websiteId=" + encodeURIComponent(websiteId)),
            getStatus: (taskId, websiteId) => request("GetStatus?" + taskQuery(taskId, websiteId)),
            cancel: (taskId, websiteId) => request("Cancel?" + taskQuery(taskId, websiteId), { method: "POST" }, websiteId)
        };
    })()
});
