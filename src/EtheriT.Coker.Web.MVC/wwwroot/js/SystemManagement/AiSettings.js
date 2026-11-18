(function () {
    "use strict";
    function init() {
        var root = document.getElementById("AiSettingsMain");
        if (!root) return;
        var canEdit = root.dataset.canEdit === "true";
        var site = Number(root.dataset.websiteId);
        var base = root.dataset.apiBase;
        var enabled = root.querySelector("#AiEnabled");
        var provider = root.querySelector("#AiProvider");
        var apiKey = root.querySelector("#AiApiKey");
        var action = root.querySelector("#AiKeyAction");
        var model = root.querySelector("#AiModel");
        var modelSelect = root.querySelector("#AiModelSelect");
        var loadModels = root.querySelector("#AiLoadModels");
        var modelHelp = root.querySelector("#AiModelHelp");
        var workspace = root.querySelector("#AiClaudeWorkspaceId");
        var save = root.querySelector("#AiSave");
        var test = root.querySelector("#AiTest");
        var reset = root.querySelector("#AiReset");
        var saved = null;
        var busy = false;

        var co = Coker;
        function popupText(text) {
            var element = document.createElement("span");
            element.textContent = text;
            return element.innerHTML;
        }

        function message(text, error) {
            var target = root.querySelector("#AiMessage");
            target.textContent = text;
            target.className = "alert " + (error ? "alert-danger" : "alert-info");
        }
        function hasKey() {
            return provider.value !== "self" && (action.value === "replace"
                ? apiKey.value.trim().length > 0
                : action.value === "keep" && saved && saved.provider === provider.value && saved.hasApiKey);
        }
        function modelChoices(items) {
            modelSelect.replaceChildren();
            function option(value, text) {
                var item = document.createElement("option");
                item.value = value;
                item.textContent = text;
                modelSelect.appendChild(item);
            }
            option("", "請載入並選擇模型");
            items.forEach(function (item) { option(item.id, item.name === item.id ? item.id : item.name + "（" + item.id + "）"); });
            var current = model.value.trim();
            if (current && !items.some(function (item) { return item.id === current; })) option(current, "目前模型：" + current);
            option("__custom__", "自訂模型 ID…");
            modelSelect.value = current;
            root.querySelector("#AiCustomModelFields").hidden = true;
        }
        function invalidateModels() {
            modelChoices([]);
            modelHelp.textContent = "填寫金鑰後按「載入模型」再選擇，也可自訂模型 ID。查詢不會儲存或執行模型生成。";
        }
        function hasChanges() {
            return saved && (enabled.checked !== saved.enabled || provider.value !== saved.provider
                || model.value.trim() !== (saved.model || "") || action.value !== "keep"
                || (provider.value === "claude" ? workspace.value.trim() : "") !== (saved.claudeWorkspaceId || ""));
        }
        function render() {
            var editable = canEdit && saved && !busy;
            enabled.disabled = !editable;
            root.querySelector("#AiServiceFields").disabled = !editable;
            root.querySelector("#AiExternalFields").hidden = provider.value === "self";
            root.querySelector("#AiClaudeWorkspaceFields").hidden = provider.value !== "claude";
            workspace.disabled = !editable || provider.value !== "claude";
            apiKey.disabled = action.value !== "replace" || provider.value === "self";
            save.disabled = !editable;
            test.disabled = !editable || hasChanges();
            loadModels.disabled = !editable || !hasKey();
            reset.disabled = busy;
            root.querySelector("#AiKeyStatus").textContent = saved && saved.hasApiKey ? "已保存金鑰（不顯示原文）" : "尚未保存外部金鑰";
            root.querySelector("#AiProviderHelp").textContent = provider.value === "self"
                ? "由系統提供服務，不需要另外填寫 API Key。" : "未設定外部金鑰時使用系統預設 AI。";
            var status = root.querySelector("#AiStatus");
            status.textContent = enabled.checked ? "開放（儲存後生效）" : "關閉（儲存後生效）";
            status.className = "badge mb-3 " + (enabled.checked ? "text-bg-primary" : "text-bg-secondary");
            root.querySelector("#AiEffectiveService").textContent = !enabled.checked ? "AI 輔助關閉"
                : hasKey() && provider.selectedIndex >= 0 ? provider.options[provider.selectedIndex].text : "系統預設 AI";
            root.querySelector("#AiSummaryModel").textContent = !enabled.checked ? "—"
                : hasKey() ? model.value.trim() || "尚未填寫" : "系統預設";
        }
        async function read(response) {
            var data;
            try { data = await response.json(); } catch (_) { throw new Error("服務回應異常，請重新載入後再試。"); }
            if (!response.ok) throw new Error(data.message || "操作失敗，請確認權限並重新載入後再試。");
            return data;
        }
        function apply(data) {
            if (data.websiteId !== site) throw new Error("站台已切換，請重新開啟 AI 設定頁面。");
            if (!Array.isArray(data.providers) || !data.providers.some(function (item) { return item.key === data.provider; }))
                throw new Error("AI 供應商選項尚未完整載入，請重新整理並確認設定資料已部署。");
            saved = data;
            provider.replaceChildren();
            data.providers.forEach(function (item) {
                var option = document.createElement("option");
                option.value = item.key;
                option.textContent = item.name;
                provider.appendChild(option);
            });
            enabled.checked = data.enabled;
            provider.value = data.provider;
            model.value = data.model;
            workspace.value = data.claudeWorkspaceId || "";
            action.value = "keep";
            apiKey.value = "";
            invalidateModels();
            root.querySelector("#AiTestHelp").textContent = "尚未檢查";
        }
        async function load() {
            busy = true;
            saved = null;
            apiKey.value = "";
            enabled.checked = false;
            provider.value = "self";
            model.value = "";
            workspace.value = "";
            action.value = "keep";
            invalidateModels();
            render();
            try {
                apply(await read(await fetch(base + "/Settings", { credentials: "same-origin", cache: "no-store" })));
                message("已載入本站設定。變更後請儲存，測試連線不會儲存或開放功能。", false);
            } catch (error) { message(error.message, true); }
            finally { busy = false; render(); }
        }
        function input(forModels) {
            if (action.value === "replace" && !apiKey.value.trim()) throw new Error("請輸入新的 API Key。");
            if (!forModels && hasKey() && !model.value.trim()) throw new Error("請選擇模型或填寫自訂模型 ID。");
            return { websiteId: site, enabled: enabled.checked, provider: provider.value, model: forModels ? "" : model.value.trim(),
                keyAction: action.value, apiKey: action.value === "replace" ? apiKey.value.trim() : "", revision: saved.revision,
                claudeWorkspaceId: provider.value === "claude" ? workspace.value.trim() : "" };
        }
        async function fetchModels() {
            if (busy || !canEdit || !saved || !hasKey()) return;
            try {
                var payload = JSON.stringify(input(true));
                busy = true;
                loadModels.textContent = "載入中…";
                modelHelp.textContent = "正在查詢帳戶可用模型…";
                render();
                var csrf = await read(await fetch(base + "/Token", { credentials: "same-origin", cache: "no-store" }));
                if (csrf.websiteId !== site) throw new Error("站台已切換，請重新開啟 AI 設定頁面。");
                var headers = { "Content-Type": "application/json" };
                headers[csrf.header || "RequestVerificationToken"] = csrf.token;
                var data = await read(await fetch(base + "/Models", {
                    method: "POST", credentials: "same-origin", cache: "no-store", headers: headers, body: payload
                }));
                if (data.websiteId !== site || data.provider !== provider.value || !Array.isArray(data.models))
                    throw new Error("模型清單與目前設定不符，請重新載入設定。");
                modelChoices(data.models);
                modelHelp.textContent = data.models.length
                    ? "已載入 " + data.models.length + " 個模型。選擇後請儲存；清單不代表此模型已通過連線或畫布功能驗證。"
                    : "帳戶沒有回傳可選模型，可使用自訂模型 ID 或檢查帳戶權限。";
            } catch (error) {
                invalidateModels();
                modelHelp.textContent = "模型載入失敗；目前模型已保留，可重試或使用自訂模型 ID。";
                co.sweet.error("模型載入失敗", popupText(error.message));
            } finally { busy = false; loadModels.textContent = "載入模型"; render(); }
        }
        async function post(path) {
            if (busy || !canEdit || !saved) return;
            try {
                var testing = path === "/Test";
                if (testing && hasChanges()) throw new Error("請先儲存設定，再測試連線。");
                var payload = testing ? undefined : JSON.stringify(input());
                busy = true;
                render();
                if (testing && saved.provider !== "self" && saved.hasApiKey
                    && !await co.sweet.confirmAsync("測試連線", "測試會使用本站已儲存的 AI 設定，可能產生少量 API 費用。是否繼續？", "開始測試", "取消")) return;
                if (!testing) {
                    save.textContent = "儲存中…";
                }
                render();
                // Refresh cookie/token before each POST; never retry a business POST.
                var csrf = await read(await fetch(base + "/Token", { credentials: "same-origin", cache: "no-store" }));
                if (csrf.websiteId !== site) throw new Error("站台已切換，請重新開啟 AI 設定頁面。");
                var headers = {};
                if (!testing) headers["Content-Type"] = "application/json";
                headers[csrf.header || "RequestVerificationToken"] = csrf.token;
                var data = await read(await fetch(base + path, {
                    method: "POST", credentials: "same-origin", cache: "no-store", headers: headers, body: payload
                }));
                if (path === "/Settings") {
                    apply(data);
                    co.sweet.success("AI 設定已儲存。");
                } else {
                    root.querySelector("#AiTestHelp").textContent = data.message;
                    co.sweet.success(popupText(data.message));
                }
            } catch (error) {
                co.sweet.error(path === "/Settings" ? "儲存失敗" : "測試未成功", popupText(error.message));
                if (path === "/Test") root.querySelector("#AiTestHelp").textContent = "測試未成功；" + error.message;
            } finally { busy = false; save.textContent = "儲存設定"; render(); }
        }
        function changed() {
            root.querySelector("#AiTestHelp").textContent = hasChanges() ? "請先儲存設定，再測試連線。" : "尚未檢查";
            render();
        }
        provider.addEventListener("change", function () {
            apiKey.value = "";
            model.value = saved && saved.provider === provider.value ? saved.model : "";
            action.value = saved && saved.hasApiKey && saved.provider !== provider.value
                ? provider.value === "self" ? "clear" : "replace" : "keep";
            invalidateModels();
            changed();
        });
        action.addEventListener("change", function () { apiKey.value = ""; invalidateModels(); changed(); });
        enabled.addEventListener("change", changed);
        apiKey.addEventListener("input", function () { invalidateModels(); changed(); });
        model.addEventListener("input", changed);
        workspace.addEventListener("input", function () { invalidateModels(); changed(); });
        modelSelect.addEventListener("change", function () {
            var custom = modelSelect.value === "__custom__";
            root.querySelector("#AiCustomModelFields").hidden = !custom;
            model.value = custom ? "" : modelSelect.value;
            if (custom) model.focus();
            changed();
        });
        loadModels.addEventListener("click", fetchModels);
        save.addEventListener("click", function () { post("/Settings"); });
        test.addEventListener("click", function () { post("/Test"); });
        reset.addEventListener("click", async function () {
            if (busy) return;
            busy = true;
            render();
            var confirmed;
            try {
                confirmed = await co.sweet.confirmAsync("重新載入設定", "重新載入會放棄尚未儲存的變更，是否繼續？", "重新載入", "取消");
            } finally { busy = false; render(); }
            if (confirmed) load();
        });
        window.addEventListener("pagehide", function () { apiKey.value = ""; });
        window.addEventListener("pageshow", function (event) { if (event.persisted) load(); });
        load();
    }
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
