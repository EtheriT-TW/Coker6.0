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
        var save = root.querySelector("#AiSave");
        var test = root.querySelector("#AiTest");
        var reset = root.querySelector("#AiReset");
        var saved = null;
        var busy = false;

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
        function render() {
            var editable = canEdit && saved && !busy;
            enabled.disabled = !editable;
            root.querySelector("#AiServiceFields").disabled = !editable;
            root.querySelector("#AiExternalFields").hidden = provider.value === "self";
            apiKey.disabled = action.value !== "replace" || provider.value === "self";
            save.disabled = !editable;
            test.disabled = !editable;
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
            action.value = "keep";
            apiKey.value = "";
            root.querySelector("#AiTestHelp").textContent = "尚未檢查";
        }
        async function load() {
            busy = true;
            saved = null;
            apiKey.value = "";
            enabled.checked = false;
            provider.value = "self";
            model.value = "";
            action.value = "keep";
            render();
            try {
                apply(await read(await fetch(base + "/Settings", { credentials: "same-origin", cache: "no-store" })));
                message("已載入本站設定。變更後請儲存，測試連線不會儲存或開放功能。", false);
            } catch (error) { message(error.message, true); }
            finally { busy = false; render(); }
        }
        function input() {
            if (action.value === "replace" && !apiKey.value.trim()) throw new Error("請輸入新的 API Key。");
            if (hasKey() && !model.value.trim()) throw new Error("使用外部 AI 時請填寫模型 ID。");
            return { websiteId: site, enabled: enabled.checked, provider: provider.value, model: model.value.trim(),
                keyAction: action.value, apiKey: action.value === "replace" ? apiKey.value.trim() : "", revision: saved.revision };
        }
        async function post(path) {
            if (busy || !canEdit || !saved) return;
            try {
                input();
                if (path === "/Test" && hasKey() && !window.confirm("測試會呼叫所選 AI，可能產生少量 API 費用。是否繼續？")) return;
                busy = true;
                render();
                // Refresh cookie/token before each POST; never retry a business POST.
                var csrf = await read(await fetch(base + "/Token", { credentials: "same-origin", cache: "no-store" }));
                if (csrf.websiteId !== site) throw new Error("站台已切換，請重新開啟 AI 設定頁面。");
                var headers = { "Content-Type": "application/json" };
                headers[csrf.header || "RequestVerificationToken"] = csrf.token;
                var data = await read(await fetch(base + path, {
                    method: "POST", credentials: "same-origin", cache: "no-store", headers: headers, body: JSON.stringify(input())
                }));
                if (path === "/Settings") {
                    apply(data);
                    message("AI 設定已儲存。請重新開啟編輯頁面以套用設定。", false);
                } else {
                    root.querySelector("#AiTestHelp").textContent = data.message;
                    message(data.message, false);
                }
            } catch (error) {
                message(error.message, true);
                if (path === "/Test") root.querySelector("#AiTestHelp").textContent = "測試未成功；" + error.message;
            } finally { busy = false; render(); }
        }
        function changed() {
            root.querySelector("#AiTestHelp").textContent = "設定已變更，尚未檢查";
            render();
        }
        provider.addEventListener("change", function () {
            apiKey.value = "";
            model.value = saved && saved.provider === provider.value ? saved.model : "";
            action.value = saved && saved.hasApiKey && saved.provider !== provider.value
                ? provider.value === "self" ? "clear" : "replace" : "keep";
            changed();
        });
        action.addEventListener("change", function () { apiKey.value = ""; changed(); });
        enabled.addEventListener("change", changed);
        apiKey.addEventListener("input", changed);
        model.addEventListener("input", changed);
        save.addEventListener("click", function () { post("/Settings"); });
        test.addEventListener("click", function () { post("/Test"); });
        reset.addEventListener("click", function () { if (window.confirm("重新載入會放棄尚未儲存的變更，是否繼續？")) load(); });
        window.addEventListener("pagehide", function () { apiKey.value = ""; });
        window.addEventListener("pageshow", function (event) { if (event.persisted) load(); });
        load();
    }
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
