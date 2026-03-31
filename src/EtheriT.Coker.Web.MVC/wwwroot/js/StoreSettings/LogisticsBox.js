(function () {
    "use strict";

    const FORM_ID = "LogisticsBoxForm";
    let box_list = null;
    let hashPage = null;

    function clearForm() {
        if (window.co && co.Form && typeof co.Form.clear === "function") {
            co.Form.clear(FORM_ID);
        } else {
            const form = document.getElementById(FORM_ID);
            if (form) {
                form.reset();
                $(form).removeClass("was-validated");
            }
        }
    }

    function insertFormData(data) {
        if (!data) return;

        if (window.co && co.Form && typeof co.Form.insertData === "function") {
            co.Form.insertData(data, `#${FORM_ID}`);
        }
    }

    function getFormData() {
        if (window.co && co.Form && typeof co.Form.getJson === "function") {
            return co.Form.getJson(FORM_ID);
        }

        return {};
    }

    async function loadEditData(id) {
        try {
            const res = await co.LogisticsBox.Get(id);

            insertFormData(res.object);
            $(`#${FORM_ID} [name="id"]`).val(id);
        } catch (err) {
            co.sweet.error("讀取失敗", err.result?.error || err.message || "讀取失敗");
        }
    }

    function enterNewMode() {
        clearForm();
        $("#LogisticsBoxContent [data-hash-title]").text("新增箱型");
    }

    async function enterEditMode(id) {
        clearForm();
        $("#LogisticsBoxContent [data-hash-title]").text("編輯箱型");
        $(`#${FORM_ID} [name="id"]`).val(id);
        await loadEditData(id);
    }

    async function submitForm() {
        const dto = getFormData();

        if (window.co && co.LogisticsBox && typeof co.LogisticsBox.AddUp === "function") {
            try {
                const res = await co.LogisticsBox.AddUp(dto);

                Coker.sweet.success(res.message || "儲存成功", null, true);

                if (box_list && typeof box_list.refresh === "function") {
                    box_list.refresh();
                }

                if (hashPage) {
                    hashPage.goList();
                }

                return res;
            } catch (err) {
                co.sweet.error("儲存失敗", err.result?.error || err.message || "儲存失敗");
                throw err;
            }
        }

        console.log("LogisticsBox submit dto:", dto);
        Coker.sweet.warning("尚未串接儲存 API", "找不到 co.LogisticsBox.AddUp");
        return null;
    }

    function bindForm() {
        const form = document.getElementById(FORM_ID);
        if (!form) return;

        if (window.co && co.Form && typeof co.Form.init === "function") {
            co.Form.init(FORM_ID, function () {
                return submitForm();
            });
            return;
        }

        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            await submitForm();
        });
    }

    const edit = function (e) {
        if (!e || !e.row || !hashPage) return;
        hashPage.goId(e.row.key);
    };

    const del = function (e) {
        if (!e || !e.row) return;

        Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", async function () {
            try {
                const res = await co.LogisticsBox.Delete(e.row.key);

                Coker.sweet.success(res.message || "刪除成功", null, true);

                if (e.component && typeof e.component.refresh === "function") {
                    e.component.refresh();
                }

                if (hashPage) {
                    hashPage.goList();
                }
            } catch (err) {
                co.sweet.error("刪除失敗", err.result?.error || err.message || "刪除失敗");
            }
        });
    };

    const gridReady = function (e) {
        box_list = e && e.component ? e.component : e;
    };

    const init = function () {
        bindForm();

        hashPage = Coker.HashPage.create({
            root: "#LogisticsBoxPage",
            defaultHash: "List",
            listHash: "List",
            newHash: "new",
            listPageKey: "List",
            contentPageKey: "Content",
            titleSelector: "[data-hash-title]",
            scrollTarget: '[data-hash-page="Content"]',
            useStack: true,
            onList: function () {
                // 預設不自動 refresh，避免重置使用者狀態
            },
            onNew: function () {
                enterNewMode();
            },
            onEdit: function (state) {
                enterEditMode(state.id);
            }
        });
    };

    window.PageReady = init;
    window.contentReady = gridReady;
    window.editButtonClicked = edit;
    window.deleteButtonClicked = del;
})();