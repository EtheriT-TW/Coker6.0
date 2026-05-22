(function (window, $) {
    // 原頁面已使用 PageReady 初始化列表與回覆功能，這裡先保留再追加匯出初始化。
    const previousPageReady = window.PageReady;

    // 聯絡表單匯出視窗的前端狀態與操作集中在同一物件，避免污染全域命名空間。
    const ContactExport = {
        // Bootstrap modal 實例，open 時才顯示。
        modal: null,
        // 表單類別只載入一次，避免每次開窗都打 API。
        formTypesLoaded: false,
        // 由後端設定回傳的匯出筆數上限，用來更新 modal 提示與錯誤同步。
        maxRows: null,
        // 目前選取的快捷時間區間，用來切換按鈕 active 樣式。
        activeRange: "today",
        // 送出中鎖定按鈕，避免重複下載請求。
        isSubmitting: false,

        // 綁定匯出按鈕、欄位驗證、快捷區間與 hash 切頁事件。
        init: function () {
            const modalElement = document.getElementById("ContactExportModal");
            if (!modalElement) return;

            this.modal = new bootstrap.Modal(modalElement, {
                backdrop: "static",
                keyboard: true
            });

            $("#ContactExportOpen").on("click", () => this.open());
            $("#ContactExportFormType").on("change", () => this.validate());
            $("#ContactExportStartTime,#ContactExportEndTime").on("input", () => {
                this.setActiveRange("");
                this.updateRangeHint();
                this.validate();
            });
            $("#ContactExportSubmit").on("click", () => this.submit());
            $("#ContactExportModal [data-export-range]").on("click", event => {
                const range = $(event.currentTarget).data("export-range");
                this.applyRange(range);
            });

            window.addEventListener("hashchange", () => this.toggleOpenButton());
            this.toggleOpenButton();
        },

        // 開啟匯出視窗時重設欄位，載入表單類別並預設為今天。
        open: async function () {
            this.reset();
            await this.loadFormTypes();
            this.updateLimitText();
            this.applyRange("today");
            this.modal.show();
        },

        // 清空前一次匯出條件與錯誤訊息，避免使用者看到舊狀態。
        reset: function () {
            this.isSubmitting = false;
            $("#ContactExportFormType").val("");
            $("#ContactExportModal input[type='checkbox']").prop("checked", false);
            this.setErrors({});
            $("#ContactExportSubmit").prop("disabled", true).text("確認匯出");
        },

        // 從後端取得目前站台可匯出的表單類別，並支援大小寫不同的 JSON 屬性。
        loadFormTypes: async function () {
            if (this.formTypesLoaded) return;

            try {
                const result = await $.ajax({
                    url: "/api/Contact/GetContactExportFormTypes",
                    type: "GET",
                    headers: this.getHeaders(),
                    dataType: "json"
                });

                if (!result || !result.success) {
                    throw new Error(result?.error || result?.message || "表單類別載入失敗");
                }

                const payload = result.object ?? result.Object ?? [];
                const options = Array.isArray(payload)
                    ? payload
                    : (payload.formTypes ?? payload.FormTypes ?? []);
                this.setMaxRows(Array.isArray(payload) ? (result.maxRows ?? result.MaxRows) : (payload.maxRows ?? payload.MaxRows));

                const $select = $("#ContactExportFormType");
                $select.find("option:not(:first)").remove();
                options.forEach(item => {
                    const id = item.id ?? item.Id;
                    const name = item.name ?? item.Name;
                    $select.append($("<option />").val(id).text(name));
                });
                this.formTypesLoaded = true;
            } catch (error) {
                Coker.sweet.error("匯出失敗", error.message || "表單類別載入失敗，請稍後再試。");
            }
        },

        // 套用快捷時間區間；custom 只切換狀態，不覆蓋使用者自行輸入的時間。
        applyRange: function (range) {
            this.setActiveRange(range);

            if (range === "custom") {
                this.updateRangeHint();
                this.validate();
                return;
            }

            const now = new Date();
            const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);
            const endOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59);
            let start = startOfToday;
            let end = endOfToday;

            if (range === "yesterday") {
                start = this.addDays(startOfToday, -1);
                end = new Date(start.getFullYear(), start.getMonth(), start.getDate(), 23, 59, 59);
            } else if (range === "last7") {
                start = this.addDays(startOfToday, -6);
            } else if (range === "thisMonth") {
                start = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0);
            } else if (range === "lastMonth") {
                start = new Date(now.getFullYear(), now.getMonth() - 1, 1, 0, 0, 0);
                end = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59);
            }

            $("#ContactExportStartTime").val(this.toInputValue(start));
            $("#ContactExportEndTime").val(this.toInputValue(end));
            this.updateRangeHint();
            this.validate();
        },

        // 更新快捷區間按鈕樣式，空字串代表使用者手動調整時間。
        setActiveRange: function (range) {
            this.activeRange = range;
            $("#ContactExportModal [data-export-range]").each(function () {
                const isActive = $(this).data("export-range") === range;
                $(this).toggleClass("active", isActive);
            });
        },

        // 將目前輸入的時間轉成易讀文字，讓使用者送出前再確認區間。
        updateRangeHint: function () {
            const start = $("#ContactExportStartTime").val();
            const end = $("#ContactExportEndTime").val();
            const hint = start && end
                ? `ⓘ ${this.toDisplayValue(new Date(start))} ～ ${this.toDisplayValue(new Date(end))}`
                : "";
            $("#ContactExportRangeHint").text(hint);
        },

        // 前端只做必要驗證；筆數上限與權限仍由後端強制檢查。
        validate: function () {
            const errors = {};
            const formTypeId = $("#ContactExportFormType").val();
            const startValue = $("#ContactExportStartTime").val();
            const endValue = $("#ContactExportEndTime").val();
            const start = startValue ? new Date(startValue) : null;
            const end = endValue ? new Date(endValue) : null;

            if (!formTypeId) {
                errors.formType = "請選擇表單類別";
            }

            if (!startValue || !endValue) {
                errors.time = "請設定時間區間";
            } else if (end < start) {
                errors.time = "結束時間不可早於開始時間";
            }

            if (start && end && end >= start) {
                const oneYearMs = 366 * 24 * 60 * 60 * 1000;
                // 大於一年僅提示，不阻擋；真正限制由後端設定的筆數上限處理。
                $("#ContactExportSpanHint").text(end - start > oneYearMs ? "建議匯出區間不超過一年" : "");
            } else {
                $("#ContactExportSpanHint").text("");
            }

            this.setErrors(errors);
            $("#ContactExportSubmit").prop("disabled", this.isSubmitting || Object.keys(errors).length > 0);
            return Object.keys(errors).length === 0;
        },

        // 將驗證錯誤寫回 modal 中對應的錯誤訊息區塊。
        setErrors: function (errors) {
            $("[data-export-error='formType']").text(errors.formType || "");
            $("[data-export-error='time']").text(errors.time || "");
        },

        // 將後端回傳的上限寫入狀態；無效值忽略，避免把提示改成錯誤數字。
        setMaxRows: function (maxRows) {
            const parsed = Number(maxRows);
            if (!Number.isFinite(parsed) || parsed <= 0) return;

            this.maxRows = parsed;
            this.updateLimitText();
        },

        // 使用後端設定值更新提示文字，讓使用者看到真正會被套用的匯出限制。
        updateLimitText: function () {
            const $limit = $("#ContactExportLimit");
            if (!$limit.length) return;

            const loadingText = "⚠️ 正在載入單次匯出筆數上限…";
            if (!this.maxRows) {
                $limit.text(loadingText);
                return;
            }

            const template = $limit.data("export-limit-template") || "⚠️ 單次匯出最多 {0} 筆資料，請縮小時間區間";
            $limit.text(template.replace("{0}", this.maxRows));
        },

        // 送出匯出條件並下載後端回傳的 Excel blob。
        submit: async function () {
            if (!this.validate() || this.isSubmitting) return;

            this.setLoading(true);
            const abortController = new AbortController();

            // 未勾選狀態代表不套用狀態條件；後端會再次驗證合法狀態值。
            const payload = {
                formTypeId: Number($("#ContactExportFormType").val()),
                startTime: $("#ContactExportStartTime").val(),
                endTime: $("#ContactExportEndTime").val(),
                statuses: $("#ContactExportModal input[type='checkbox']:checked").map(function () {
                    return Number(this.value);
                }).get()
            };

            try {
                // 按下確認匯出後立即顯示處理中 popup，讓使用者知道後端正在產生檔案。
                this.showProcessingPopup(abortController);
                const response = await fetch("/api/Contact/ExportContacts", {
                    method: "POST",
                    headers: {
                        ...this.getHeaders(),
                        "Content-Type": "application/json; charset=utf-8"
                    },
                    body: JSON.stringify(payload),
                    signal: abortController.signal
                });

                // 登入逾期時導回登入頁，不嘗試解析檔案回應。
                if (response.status === 401) {
                    window.location.href = "/Account/Index";
                    return;
                }

                if (!response.ok) {
                    throw new Error(await this.getErrorMessage(response));
                }

                const blob = await response.blob();
                const fileName = this.getFileName(response);

                // 檔名由後端 Content-Disposition 決定，成功 popup 與下載檔名都使用同一個值。
                this.showSuccessPopup(fileName);
                this.downloadBlob(blob, fileName);
                this.modal.hide();
            } catch (error) {
                if (error.name === "AbortError") {
                    this.closeExportPopup();
                    return;
                }

                Coker.sweet.error("匯出失敗", error.message || "匯出失敗：系統發生意外錯誤，請稍後再試或聯繫系統管理員。");
            } finally {
                this.setLoading(false);
            }
        },

        // 顯示匯出處理中的 popup；若使用者按取消，會中止前端下載請求。
        showProcessingPopup: function (abortController) {
            Swal.fire({
                html: [
                    "<div class='contact-export-popup-content'>",
                    "  <div class='contact-export-popup-spinner' aria-hidden='true'></div>",
                    "  <div class='contact-export-popup-title'>匯出處理中，請稍候...</div>",
                    "  <div class='contact-export-popup-note'>資料整理中，請勿關閉視窗或離開頁面</div>",
                    "</div>"
                ].join(""),
                showConfirmButton: false,
                showCancelButton: true,
                cancelButtonText: "取消",
                allowOutsideClick: false,
                allowEscapeKey: false,
                buttonsStyling: false,
                customClass: {
                    popup: "contact-export-status-popup",
                    htmlContainer: "contact-export-popup-html",
                    cancelButton: "btn btn-outline-secondary contact-export-popup-cancel"
                }
            }).then(result => {
                // 只有使用者主動按取消才中止請求；成功或錯誤狀態切換 popup 時不影響流程。
                if (result.dismiss === Swal.DismissReason.cancel) {
                    abortController.abort();
                }
            });
        },

        // 匯出成功後在同一個 popup 顯示檔名，並由 submit 流程立即觸發下載。
        showSuccessPopup: function (fileName) {
            Swal.fire({
                html: [
                    "<div class='contact-export-popup-content'>",
                    "  <div class='contact-export-popup-success-icon' aria-hidden='true'>",
                    "    <span class='material-symbols-outlined'>check</span>",
                    "  </div>",
                    "  <div class='contact-export-popup-title'>匯出成功</div>",
                    "  <div class='contact-export-popup-note'>檔案已開始下載</div>",
                    `  <div class='contact-export-popup-file'>${this.escapeHtml(fileName)}</div>`,
                    "</div>"
                ].join(""),
                showConfirmButton: true,
                confirmButtonText: "關閉",
                showCancelButton: false,
                buttonsStyling: false,
                customClass: {
                    popup: "contact-export-status-popup",
                    htmlContainer: "contact-export-popup-html",
                    confirmButton: "btn btn-success contact-export-popup-confirm"
                }
            });
        },

        // 關閉匯出專用 popup；取消下載時避免留下處理中畫面。
        closeExportPopup: function () {
            if (typeof Swal !== "undefined" && Swal.isVisible()) {
                Swal.close();
            }
        },

        // 切換送出中狀態，並在完成後重新跑驗證恢復按鈕狀態。
        setLoading: function (loading) {
            this.isSubmitting = loading;
            const $button = $("#ContactExportSubmit");
            $button.prop("disabled", loading);
            $button.text(loading ? "匯出處理中，請稍候…" : "確認匯出");
            if (!loading) this.validate();
        },

        // 組出 API header；防偽權杖可能在 _c 初始化後才出現，所以每次送出都重新讀 DOM。
        getHeaders: function () {
            const token = $("input[name='AntiforgeryField']").val();
            const headers = {};
            if (typeof _c !== "undefined" && _c.Data && _c.Data.Header) {
                Object.keys(_c.Data.Header).forEach(key => {
                    if (_c.Data.Header[key]) headers[key] = _c.Data.Header[key];
                });
            }
            if (token) headers["x-xsrf-token"] = token;
            return headers;
        },

        // 後端錯誤可能是 JSON 或純文字，這裡統一轉成顯示用訊息。
        getErrorMessage: async function (response) {
            const contentType = response.headers.get("content-type") || "";
            if (contentType.indexOf("application/json") >= 0) {
                const data = await response.json();
                this.setMaxRows(data.maxRows ?? data.MaxRows);
                return data.error || data.message || "匯出失敗";
            }

            return await response.text() || "匯出失敗";
        },

        // 從 Content-Disposition 解析檔名，支援 UTF-8 filename* 格式。
        getFileName: function (response) {
            const disposition = response.headers.get("content-disposition") || "";
            const utf8Match = disposition.match(/filename\*=UTF-8''([^;]+)/i);
            if (utf8Match) return decodeURIComponent(utf8Match[1]);

            const match = disposition.match(/filename="?([^"]+)"?/i);
            return match ? match[1] : "contacts_export.xlsx";
        },

        // 建立暫時下載連結觸發瀏覽器下載，完成後釋放 blob URL。
        downloadBlob: function (blob, fileName) {
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(url);
        },

        // 後端檔名會放進 popup HTML，先做跳脫以避免特殊字元破壞畫面。
        escapeHtml: function (value) {
            return String(value).replace(/[&<>"']/g, char => ({
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                "\"": "&quot;",
                "'": "&#39;"
            }[char]));
        },

        // 聯絡我們明細頁以 hash 切換，匯出按鈕只應出現在列表頁。
        toggleOpenButton: function () {
            $("#ContactExportOpen").toggle(window.location.hash === "");
        },

        // 以本地時間加減天數，避免直接毫秒相加遇到日光節約時間邊界。
        addDays: function (date, days) {
            return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days, date.getHours(), date.getMinutes(), date.getSeconds());
        },

        // datetime-local 需要 yyyy-MM-ddTHH:mm:ss 格式。
        toInputValue: function (date) {
            return [
                date.getFullYear(),
                this.pad(date.getMonth() + 1),
                this.pad(date.getDate())
            ].join("-") + "T" + [
                this.pad(date.getHours()),
                this.pad(date.getMinutes()),
                this.pad(date.getSeconds())
            ].join(":");
        },

        // modal 內提示文字使用 yyyy/MM/dd HH:mm 格式，降低閱讀負擔。
        toDisplayValue: function (date) {
            return [
                date.getFullYear(),
                this.pad(date.getMonth() + 1),
                this.pad(date.getDate())
            ].join("/") + " " + [
                this.pad(date.getHours()),
                this.pad(date.getMinutes())
            ].join(":");
        },

        // 日期與時間欄位固定補成兩位數。
        pad: function (value) {
            return String(value).padStart(2, "0");
        }
    };

    // 串接既有 PageReady，確保原頁面功能與匯出功能都會初始化。
    window.PageReady = function () {
        if (typeof previousPageReady === "function") {
            previousPageReady();
        }

        ContactExport.init();
    };
})(window, jQuery);
