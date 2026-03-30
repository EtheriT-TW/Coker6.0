(function () {
    "use strict";

    let keyId = null;
    let box_list = null;
    let lastHash = "";

    const HASH_LIST = "List";
    const HASH_NEW = "new";

    function getHash() {
        return (window.location.hash || "").replace(/^#/, "").trim();
    }

    function setHash(value) {
        const nextHash = String(value || "").trim();

        if (!nextHash) {
            window.location.hash = HASH_LIST;
            return;
        }

        if (getHash() === nextHash) {
            HashDataEdit();
            return;
        }

        window.location.hash = nextHash;
    }

    function isNumericHash(hash) {
        return /^\d+$/.test(hash);
    }

    function showList() {
        $("#LogisticsBoxList").removeClass("d-none");
        $("#LogisticsBoxContent").addClass("d-none");
    }

    function showContent() {
        $("#LogisticsBoxList").addClass("d-none");
        $("#LogisticsBoxContent").removeClass("d-none");
        moveToContent();
    }

    function moveToContent() {
        const $target = $("#LogisticsBoxContent");
        if (!$target.length) return;

        $("html, body").stop().animate({
            scrollTop: Math.max($target.offset().top - 20, 0)
        }, 200);
    }

    function setContentTitle(text) {
        $("#LogisticsBoxContent .title").text(text || "");
    }

    function resetForm() {
        const $form = $("#LogisticsBoxForm");

        if ($form.length && $form[0]) {
            $form[0].reset();
        }

        $form.removeClass("was-validated");

        $('input[name="Id"]').val("");
        $("#InputName").val("");
        $("#InputCapacityPoint").val("");
        $("#InputSort").val(0);
        $("#InputIsActive").prop("checked", true);
    }

    function fillForm(data) {
        if (!data) return;

        $('input[name="Id"]').val(data.Id ?? data.id ?? "");
        $("#InputName").val(data.Name ?? data.name ?? "");
        $("#InputCapacityPoint").val(data.CapacityPoint ?? data.capacityPoint ?? "");
        $("#InputSort").val(data.Sort ?? data.sort ?? 0);

        const isActive = data.IsActive ?? data.isActive;
        $("#InputIsActive").prop("checked", isActive === undefined ? true : !!isActive);
    }

    function enterNewMode() {
        keyId = null;
        resetForm();
        setContentTitle("新增箱型");
        showContent();
    }

    function enterEditMode(id) {
        keyId = parseInt(id, 10);
        resetForm();
        $('input[name="Id"]').val(keyId);
        setContentTitle("編輯箱型");
        showContent();
        loadEditData(keyId);
    }

    function loadEditData(id) {
        if (!id) return;

        // 若你之後有單筆查詢 API，可直接接這裡。
        // 目前先做「不報錯、可正常切頁」的安全寫法。

        if (
            window.co &&
            co.LogisticsBox &&
            typeof co.LogisticsBox.Get === "function"
        ) {
            co.LogisticsBox.Get(id)
                .done(function (res) {
                    fillForm(res);
                });

            return;
        }
    }

    function HashDataEdit() {
        const hash = getHash();

        if (!hash || hash.toLowerCase() === HASH_LIST.toLowerCase()) {
            keyId = null;
            showList();
            return;
        }

        if (hash.toLowerCase() === HASH_NEW.toLowerCase()) {
            enterNewMode();
            return;
        }

        if (isNumericHash(hash)) {
            enterEditMode(hash);
            return;
        }

        // 不認得的 hash，一律回列表
        setHash(HASH_LIST);
    }

    function hashChange(e) {
        HashDataEdit();

        if (e && typeof e.preventDefault === "function") {
            e.preventDefault();
        }
    }

    function bindHashChange() {
        if ("onhashchange" in window) {
            window.addEventListener("hashchange", hashChange);
        } else {
            setInterval(function () {
                const currentHash = window.location.hash;
                if (currentHash !== lastHash) {
                    lastHash = currentHash;
                    HashDataEdit();
                }
            }, 300);
        }

        lastHash = window.location.hash;
    }

    function bindButtons() {
        $(".btn_add")
            .off("click.logisticsbox")
            .on("click.logisticsbox", function (e) {
                e.preventDefault();
                setHash(HASH_NEW);
            });

        $(".btn_back")
            .off("click.logisticsbox")
            .on("click.logisticsbox", function (e) {
                e.preventDefault();
                setHash(HASH_LIST);
            });

        $("#LogisticsBoxForm")
            .off("submit.logisticsbox")
            .on("submit.logisticsbox", function (e) {
                e.preventDefault();

                const form = this;
                if (form && typeof form.checkValidity === "function" && !form.checkValidity()) {
                    $(form).addClass("was-validated");
                    return;
                }

                const dto = {
                    Id: $('input[name="Id"]').val() ? parseInt($('input[name="Id"]').val(), 10) : null,
                    Name: $("#InputName").val()?.trim() || "",
                    CapacityPoint: parseInt($("#InputCapacityPoint").val(), 10) || 0,
                    Sort: parseInt($("#InputSort").val(), 10) || 0,
                    IsActive: $("#InputIsActive").is(":checked")
                };

                console.log("LogisticsBox submit dto:", dto);

                // 尚未串接儲存 API 時，至少不讓表單整頁送出
                Coker?.sweet?.warning?.("尚未串接儲存 API", "目前僅完成前端互動");
            });
    }

    const edit = function (e) {
        if (!e || !e.row) return;
        setHash(e.row.key);
    };

    const del = function (e) {
        if (!e || !e.row) return;

        Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
            co.LogisticsBox.Delete(e.row.key).done(function () {
                Coker.sweet.success("刪除成功", null, true);

                if (e.component && typeof e.component.refresh === "function") {
                    e.component.refresh();
                }

                if (String(getHash()) === String(e.row.key)) {
                    setHash(HASH_LIST);
                }
            });
        });
    };

    const gridReady = function (e) {
        box_list = e && e.component ? e.component : e;
        bindButtons();
        HashDataEdit();
    };

    const init = function () {
        bindHashChange();
        bindButtons();

        if (!getHash()) {
            setHash(HASH_LIST);
            return;
        }

        HashDataEdit();
    };

    window.PageReady = init;
    window.contentReady = gridReady;
    window.editButtonClicked = edit;
    window.deleteButtonClicked = del;
})();