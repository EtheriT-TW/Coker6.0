(function (window, $, co) {
    "use strict";

    const MODULE_NAME = "RightSideAdPage";

    const DEFAULT_OPTIONS = {
        rootSelector: "#RightSideAdPage",
        formSelector: "#AdForm",
        listSelector: "#RightSideAdList",
        contentSelector: "#RightSideAdContent",
        imageUploadSelector: "#ImageUpload",

        visibleSelector: "#AdFormVisible",
        sortCheckSelector: "#AdvertiseSortCheck",
        sortInputSelector: "#AdvertiseSerNO",
        targetSelector: "#TargetCheck",
        dateSelector: "#InputDate",
        permanentSelector: "#PermanentCheck",
        actionTypeSelector: "#AdvertiseActionType",
        linkGroupSelector: ".js-action-link",
        linkSelector: "#AdvertiseLink",
        htmlEditButtonSelector: ".js-edit-html-content",
        canvasSelector: "#RightSideAdHtmlContent",
        editorSelector: "#gjs",
        canvasHashSuffix: "-1",

        adType: 2,
        fileType: 5,
        fileSize: 1,
        fileSerNo: 500,

        defaultHash: "List",
        listHash: "List",
        newHash: "new",
        listPageKey: "List",
        contentPageKey: "Content",

        actionTypes: {
            link: 1,
            expandHtml: 2,
            none: 3
        }
    };

    function createRightSideAdPage(options) {
        const opt = $.extend(true, {}, DEFAULT_OPTIONS, options || {});

        const state = {
            initialized: false,
            hashPage: null,
            gridEvent: null,
            keyId: 0,
            editor: null
        };

        const dom = {};

        function init() {
            if (state.initialized) {
                refreshHash();
                return;
            }

            cacheDom();

            if (!dom.$root.length) {
                console.warn(`${MODULE_NAME}: root not found`, opt.rootSelector);
                return;
            }

            if (!dom.$form.length) {
                console.warn(`${MODULE_NAME}: form not found`, opt.formSelector);
                return;
            }

            ImageUploadModalInit(dom.$imageUpload);

            initPicker();
            bindEvents();
            initHashPage();
            initForm();
            initGrapesEditor();

            window.addEventListener("hashchange", handleCanvasHashChange);

            state.initialized = true;

            handleCanvasHashChange();
        }

        function cacheDom() {
            dom.$root = $(opt.rootSelector);

            dom.$form = findInRoot(opt.formSelector);
            dom.$imageUpload = findInRoot(opt.imageUploadSelector);

            dom.$visible = findInRoot(opt.visibleSelector);
            dom.$sortCheck = findInRoot(opt.sortCheckSelector);
            dom.$sortInput = findInRoot(opt.sortInputSelector);
            dom.$target = findInRoot(opt.targetSelector);
            dom.$date = findInRoot(opt.dateSelector);
            dom.$permanent = findInRoot(opt.permanentSelector);
            dom.$actionType = findInRoot(opt.actionTypeSelector);
            dom.$linkGroup = findInRoot(opt.linkGroupSelector);
            dom.$link = findInRoot(opt.linkSelector);
            dom.$htmlEditButton = findInRoot(opt.htmlEditButtonSelector);
            dom.$canvas = findInRoot(opt.canvasSelector);
            dom.$editor = findInRoot(opt.editorSelector);
        }

        function findInRoot(selector) {
            const $el = dom.$root.find(selector);

            if ($el.length) {
                return $el;
            }

            return $(selector);
        }

        function initPicker() {
            if (dom.$date.length) {
                co.Picker.Init(dom.$date);
            }
        }

        function bindEvents() {
            dom.$visible
                .off("change.rightSideAd")
                .on("change.rightSideAd", function () {
                    syncCheckboxValue($(this));
                });

            dom.$permanent
                .off("change.rightSideAd")
                .on("change.rightSideAd", function () {
                    applyPermanentState($(this).is(":checked"));
                });

            dom.$actionType
                .off("change.rightSideAd")
                .on("change.rightSideAd", function () {
                    applyActionTypeState(getActionType());
                });

            dom.$htmlEditButton
                .off("click.rightSideAd")
                .on("click.rightSideAd", function (e) {
                    e.preventDefault();
                    goCanvasFromButton();
                });
        }

        function initHashPage() {
            state.hashPage = co.HashPage.create({
                root: opt.rootSelector,
                defaultHash: opt.defaultHash,
                listHash: opt.listHash,
                newHash: opt.newHash,
                listPageKey: opt.listPageKey,
                contentPageKey: opt.contentPageKey,
                titleSelector: "[data-hash-title]",
                scrollTarget: opt.contentSelector,
                onList: handleList,
                onNew: handleNew,
                onEdit: handleEdit
            });
        }

        function initForm() {
            co.Form.init(dom.$form.attr("id"), function () {
                if (!validateBeforeSave()) {
                    return resolvedDeferred();
                }

                return co.Form.confirmSubmit({
                    title: "即將發布",
                    text: "發布後將直接顯示於安排的位置",
                    confirmButtonText: "發布",
                    cancelButtonText: "取消",
                    onConfirm: function () {
                        return save("已成功發布", "發布發生未知錯誤");
                    }
                });
            });
        }
        function initGrapesEditor() {
            if (typeof grapesInit !== "function") {
                console.warn(`${MODULE_NAME}: grapesInit not found`);
                return;
            }

            if (state.editor) {
                return;
            }

            state.editor = grapesInit({
                save: function (html, css) {
                    const defer = $.Deferred();
                    const id = getCanvasId();

                    if (!id) {
                        co.sweet.error("錯誤", "找不到要儲存的廣告資料", null, true);
                        defer.reject();
                        return defer.promise();
                    }

                    co.Advertise.SaveConten({
                        Id: id,
                        SaveHtml: html,
                        SaveCss: css
                    }).done(function (result) {
                        if (result.success) {
                            defer.resolve();
                        } else {
                            co.sweet.error("錯誤", result.error || result.message || "儲存內容失敗", null, true);
                            defer.reject(result);
                        }
                    }).fail(function (xhr) {
                        co.sweet.error("錯誤", "儲存內容失敗", null, true);
                        defer.reject(xhr);
                    });

                    return defer.promise();
                },

                import: function (html, css) {
                    const defer = $.Deferred();
                    const id = getCanvasId();

                    if (!id) {
                        co.sweet.error("錯誤", "找不到要匯入的廣告資料", null, true);
                        defer.reject();
                        return defer.promise();
                    }

                    co.Advertise.ImportConten({
                        Id: id,
                        SaveHtml: html,
                        SaveCss: css
                    }).done(function (result) {
                        if (result.success) {
                            defer.resolve();
                        } else {
                            co.sweet.error("錯誤", result.error || result.message || "匯入內容失敗", null, true);
                            defer.reject(result);
                        }
                    }).fail(function (xhr) {
                        co.sweet.error("錯誤", "匯入內容失敗", null, true);
                        defer.reject(xhr);
                    });

                    return defer.promise();
                },

                getComponer: function () {
                    const defer = $.Deferred();

                    co.HtmlContent.GetAllComponent().done(function (result) {
                        if (result.success) {
                            defer.resolve(result.list);
                        } else {
                            co.sweet.error("錯誤", result.error || result.message || "取得元件失敗", null, true);
                            defer.reject(result);
                        }
                    }).fail(function (xhr) {
                        co.sweet.error("錯誤", "取得元件失敗", null, true);
                        defer.reject(xhr);
                    });

                    return defer.promise();
                }
            });
        }

        function goCanvasFromButton() {
            co.sweet.confirmSave(
                "前往頁右廣告編輯頁",
                "是否保存資料?",
                function () {
                    saveAndGoCanvas();
                },
                function () {
                    goCanvasWithoutSave();
                }
            );
        }
        function goCanvasWithoutSave() {
            const id = state.keyId || parseInt(dom.$form.find("input[name='id']").val(), 10);

            if (!id || id <= 0) {
                co.sweet.error("提醒", "請先發布基本資料後，再進行進階編輯。", null, false);
                return;
            }

            window.location.hash = `${id}${opt.canvasHashSuffix}`;
        }
        function saveAndGoCanvas() {
            if (!validateBeforeSave()) {
                return resolvedDeferred();
            }

            return save("資料已儲存", "儲存發生未知錯誤", {
                afterSuccess: function (context) {
                    co.sweet.success(context.successText, null, true);

                    setTimeout(function () {
                        window.location.hash = `${context.sid}${opt.canvasHashSuffix}`;

                        if (state.gridEvent && state.gridEvent.component) {
                            state.gridEvent.component.refresh();
                        }
                    }, 1000);
                }
            });
        }
        function handleCanvasHashChange() {
            const id = getCanvasIdFromHash(window.location.hash);

            if (!id) {
                return false;
            }

            state.keyId = id;
            moveToCanvas(id);

            return true;
        }

        function getCanvasIdFromHash(hash) {
            const raw = String(hash || "").replace(/^#/, "");
            const suffix = opt.canvasHashSuffix;

            if (!raw.endsWith(suffix)) {
                return 0;
            }

            const idText = raw.substring(0, raw.length - suffix.length);

            if (!/^\d+$/.test(idText)) {
                return 0;
            }

            return parseInt(idText, 10) || 0;
        }

        function getCanvasId() {
            const editorId = parseInt(dom.$editor.data("id"), 10);

            if (editorId && editorId > 0) {
                return editorId;
            }

            return state.keyId;
        }

        function moveToCanvas(id) {
            if (!state.editor) {
                initGrapesEditor();
            }

            if (!state.editor) {
                co.sweet.error("錯誤", "GrapesJS 尚未正確初始化", null, true);
                return;
            }

            unmarkValidated();

            dom.$root.find("[data-hash-page]").addClass("d-none");

            if (dom.$canvas.length) {
                dom.$canvas.removeClass("d-none");
            }

            $("body").addClass("grapesEdit");

            dom.$editor.data("id", id);

            setCanvasPage(id);
        }

        function exitCanvasMode() {
            $("body").removeClass("grapesEdit");
            $("#TopLine .title").text("右側浮動廣告");
            $("#TopLine > div > a").addClass("d-none");
        }

        function setCanvasPage(id) {
            co.Advertise.GetConten({ Id: id }).done(function (result) {
                if (!result.success) {
                    co.sweet.error("錯誤", result.error || result.message || "取得內容失敗", null, true);
                    return;
                }

                const content = result.conten || result.content || result.object || {};
                const saveHtml = content.saveHtml || content.SaveHtml || "";
                const saveCss = content.saveCss || content.SaveCss || "";
                const html = co.Data.HtmlDecode(saveHtml);

                co.Grapes.setEditor(state.editor, html, saveCss);
                co.Grapes.setFile(state.editor, id, opt.fileType);

                if (content.title) {
                    $("#TopLine .title").text(content.title);
                    $("#TopLine > div > a").removeClass("d-none");
                }
            }).fail(function () {
                co.sweet.error("錯誤", "取得內容失敗", null, true);
            });
        }

        function handleList() {
            exitCanvasMode();

            state.keyId = 0;
            unmarkValidated();
        }

        function handleNew() {
            exitCanvasMode();

            state.keyId = 0;
            clearForm();
            unmarkValidated();
        }

        function handleEdit(hashState) {
            exitCanvasMode();

            const id = parseInt(hashState.id, 10);

            if (!id || id <= 0) {
                goList();
                return;
            }

            state.keyId = id;
            loadData(id);
        }

        function setGridEvent(e) {
            state.gridEvent = e;
            refreshHash();
        }

        function refreshHash() {
            if (state.hashPage) {
                state.hashPage.refresh();
            }

            handleCanvasHashChange();
        }

        function edit(e) {
            const id = e && e.row ? e.row.key : null;

            if (!id) {
                return;
            }

            if (state.hashPage) {
                state.hashPage.goId(id);
            } else {
                window.location.hash = String(id);
            }
        }

        function remove(e) {
            const id = e && e.row ? e.row.key : null;

            if (!id) {
                co.sweet.error("錯誤", "找不到要刪除的資料", null, true);
                return;
            }

            co.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Advertise.Delete(id).done(function (result) {
                    if (result.success) {
                        if (e.component) {
                            e.component.refresh();
                        }
                    } else {
                        co.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
                    }
                }).fail(function () {
                    co.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
                });
            });
        }

        function showPaletteButton(e) {
            const data = e && e.row ? e.row.data : null;

            if (!data) {
                return false;
            }

            const actionType = parseInt(
                data.actionType ?? data.ActionType,
                10
            );

            return actionType === opt.actionTypes.expandHtml;
        }

        function palette(e) {
            const id = e && e.row ? e.row.key : null;

            if (!id) {
                co.sweet.error("錯誤", "找不到要進階編輯的資料", null, true);
                return;
            }

            window.location.hash = `${id}-1`;
        }
        function loadData(id) {
            clearForm();

            co.Advertise.GetDataOne(id).done(function (result) {
                if (result != null) {
                    setFormData(result);
                } else {
                    goList();
                }
            }).fail(function () {
                co.sweet.error("錯誤", "取得資料發生錯誤", null, true);
                goList();
            });
        }

        function setFormData(result) {
            clearForm();

            state.keyId = result.id;

            result.startEndDate = buildDateRangeText(result);
            result.sortCheckbox = true;

            co.Form.insertData(result, opt.formSelector);

            dom.$visible.val(result.visible);
            dom.$visible.prop("checked", result.visible);

            syncCurrentLinkFieldsToBackup();

            applyPermanentState(result.permanent);
            applyActionTypeState(getActionType());

            loadImage(result.id);
        }

        function buildDateRangeText(result) {
            if (result.permanent) {
                return "";
            }

            const start = result.startTime || result.startDate;
            const end = result.endTime || result.endDate;

            if (!start || !end) {
                return "";
            }

            return `${formatDateTime(start)} ~ ${formatDateTime(end)}`;
        }

        function formatDateTime(value) {
            if (!value) {
                return "";
            }

            return moment(value).format("YYYY/MM/DD HH:mm");
        }

        function loadImage(id) {
            co.File.getImgFile({
                Sid: id,
                Type: opt.fileType,
                Size: opt.fileSize
            }).done(function (file) {
                if (file.length > 0) {
                    ImageUploadModalDataInsert(
                        dom.$imageUpload,
                        file[0].id,
                        file[0].link,
                        file[0].name
                    );
                }
            });
        }

        function clearForm() {
            ImageUploadModalClear(dom.$imageUpload);
            co.Form.clear(dom.$form.attr("id"));

            dom.$form.find("input[name='id']").val("");

            state.keyId = 0;

            clearLinkBackup();

            dom.$visible.prop("checked", true);
            dom.$visible.val(true);

            dom.$sortCheck.prop("checked", false);
            dom.$sortInput.prop("disabled", true);
            dom.$sortInput.val("");

            dom.$permanent.prop("checked", true);
            dom.$permanent.val(true);

            applyPermanentState(true);
            resetActionTypeState();
        }

        function getActionType() {
            const value = parseInt(dom.$actionType.val(), 10);

            if (!value || isNaN(value)) {
                return opt.actionTypes.link;
            }

            return value;
        }

        function resetActionTypeState() {
            dom.$actionType.val(opt.actionTypes.link);

            clearLinkBackup();

            toggleFieldGroup(dom.$linkGroup, true);
            setRequired(dom.$link, true);

            dom.$htmlEditButton.addClass("d-none");
        }

        function applyActionTypeState(actionType) {
            const isLink = actionType === opt.actionTypes.link;
            const isExpandHtml = actionType === opt.actionTypes.expandHtml;

            if (!isLink) {
                backupLinkFields();
            }

            toggleFieldGroup(dom.$linkGroup, isLink);
            setRequired(dom.$link, isLink);

            if (isLink) {
                restoreLinkFields();
            } else {
                clearLinkFields();
            }

            dom.$htmlEditButton.toggleClass("d-none", !isExpandHtml);
        }

        function backupLinkFields() {
            if (dom.$link.length && !dom.$link.prop("disabled")) {
                dom.$link.data("backup-value", dom.$link.val() || "");
            }

            if (dom.$target.length && !dom.$target.prop("disabled")) {
                dom.$target.data("backup-checked", dom.$target.prop("checked") === true);
            }
        }

        function syncCurrentLinkFieldsToBackup() {
            if (dom.$link.length) {
                dom.$link.data("backup-value", dom.$link.val() || "");
            }

            if (dom.$target.length) {
                dom.$target.data("backup-checked", dom.$target.prop("checked") === true);
            }
        }

        function restoreLinkFields() {
            if (dom.$link.length) {
                const backupValue = dom.$link.data("backup-value");

                if (typeof backupValue !== "undefined") {
                    dom.$link.val(backupValue);
                }
            }

            if (dom.$target.length) {
                const backupChecked = dom.$target.data("backup-checked");

                if (typeof backupChecked !== "undefined") {
                    dom.$target.prop("checked", backupChecked === true);
                    dom.$target.val(backupChecked === true);
                }
            }
        }

        function clearLinkFields() {
            if (dom.$link.length) {
                dom.$link.val("");
            }

            if (dom.$target.length) {
                dom.$target.prop("checked", false);
                dom.$target.val(false);
            }
        }

        function clearLinkBackup() {
            if (dom.$link.length) {
                dom.$link.removeData("backup-value");
            }

            if (dom.$target.length) {
                dom.$target.removeData("backup-checked");
            }
        }

        function applyPermanentState(isPermanent) {
            dom.$permanent.val(isPermanent);

            if (isPermanent) {
                dom.$date.val("");
                dom.$date.attr("disabled", "disabled");
            } else {
                dom.$date.removeAttr("disabled");
            }
        }

        function toggleFieldGroup($group, visible) {
            if (!$group || !$group.length) {
                return;
            }

            $group.toggleClass("d-none", !visible);

            $group.find("input, select, textarea").each(function () {
                const $field = $(this);

                if (visible) {
                    $field.removeAttr("disabled");
                } else {
                    $field.attr("disabled", "disabled");
                }
            });
        }

        function setRequired($field, required) {
            if (!$field || !$field.length) {
                return;
            }

            if (required) {
                $field.attr("required", "required");
            } else {
                $field.removeAttr("required");
            }
        }

        function syncCheckboxValue($checkbox) {
            $checkbox.val($checkbox.prop("checked") === true);
        }

        function validateBeforeSave() {
            markValidated();

            const form = dom.$form.get(0);

            if (form && !form.checkValidity()) {
                dom.$form.addClass("was-validated");
                return false;
            }

            if (!hasUploadImage()) {
                co.sweet.error("資料有誤", "圖示不可為空", null, false);
                return false;
            }

            return true;
        }
        function save(successText, errorText, options) {
            const saveOptions = $.extend({
                afterSuccess: function (context) {
                    saveSuccess(context.successText);
                }
            }, options || {});

            const data = co.Form.getJson(dom.$form.attr("id"));

            data.type = opt.adType;

            deleteMarkedImageIfNeeded();

            return co.Advertise.AddUp(data).then(function (result) {
                if (!result.success) {
                    co.sweet.error("錯誤", errorText, null, true);
                    return $.Deferred().reject({ handled: true, result: result }).promise();
                }

                const sid = result.message;
                const id = parseInt(sid, 10);

                if (id && id > 0) {
                    state.keyId = id;
                    dom.$form.find("input[name='id']").val(id);
                }

                return uploadImageIfNeeded(sid)
                    .done(function (uploadResult) {
                        if (typeof saveOptions.afterSuccess === "function") {
                            saveOptions.afterSuccess({
                                result: result,
                                uploadResult: uploadResult,
                                sid: sid,
                                id: id,
                                successText: successText,
                                errorText: errorText
                            });
                        }
                    });
            }).fail(function (error) {
                if (error && error.handled) {
                    return;
                }

                co.sweet.error("錯誤", errorText, null, true);
            });
        }

        function deleteMarkedImageIfNeeded() {
            const delectList = dom.$imageUpload.find(".img_input_frame").data("delectList");

            if (typeof delectList !== "undefined" && delectList != null) {
                co.File.DeleteFileById({
                    sid: state.keyId,
                    type: opt.fileType,
                    fid: delectList
                });
            }
        }

        function uploadImageIfNeeded(sid) {
            const defer = $.Deferred();
            const fileData = dom.$imageUpload.find(".img_input_frame > .img_input").data("file");
            const file = fileData ? fileData.File : null;

            if (file == null) {
                defer.resolve();
                return defer.promise();
            }

            const formData = new FormData();

            formData.append("files", file);
            formData.append("type", opt.fileType);
            formData.append("sid", sid);
            formData.append("serno", opt.fileSerNo);

            co.File.Upload(formData).done(function (result) {
                if (result.success) {
                    defer.resolve(result);
                } else {
                    co.sweet.error("錯誤", "圖片上傳發生錯誤", null, true);
                    defer.reject(result);
                }
            }).fail(function (xhr) {
                co.sweet.error("錯誤", "圖片上傳發生錯誤", null, true);
                defer.reject(xhr);
            });

            return defer.promise();
        }

        function saveSuccess(successText) {
            co.sweet.success(successText, null, true);

            setTimeout(function () {
                clearForm();
                goList();
                refreshGrid();
            }, 1000);
        }

        function goList() {
            if (state.hashPage) {
                state.hashPage.goList();
            } else {
                window.location.hash = opt.listHash;
            }
        }

        function refreshGrid() {
            if (state.gridEvent && state.gridEvent.component) {
                state.gridEvent.component.refresh();
            }
        }

        function hasUploadImage() {
            const $imgInput = dom.$imageUpload.find(".img_input_frame > .img_input");

            if (!$imgInput.length) {
                return false;
            }

            const fileData = $imgInput.data("file");

            if (fileData != null) {
                return true;
            }

            const src = $imgInput.attr("src");
            const value = $imgInput.val();

            return !!src || !!value;
        }

        function markValidated() {
            dom.$root.find(".icon_hint").addClass("pe-4");
            dom.$sortCheck.parents(".checkbox").first().addClass("pe-4");
            dom.$target.parents(".checkbox").first().addClass("pe-4");
            dom.$permanent.parents(".checkbox").first().addClass("pe-4");
        }

        function unmarkValidated() {
            dom.$form.removeClass("was-validated");
            dom.$root.find(".icon_hint").removeClass("pe-4");
            dom.$sortCheck.parents(".checkbox").first().removeClass("pe-4");
            dom.$target.parents(".checkbox").first().removeClass("pe-4");
            dom.$permanent.parents(".checkbox").first().removeClass("pe-4");
        }

        function resolvedDeferred() {
            return $.Deferred().resolve().promise();
        }

        return {
            init: init,
            setGridEvent: setGridEvent,
            edit: edit,
            delete: remove,
            palette: palette,
            showPaletteButton: showPaletteButton,
            clear: clearForm,
            refresh: refreshHash
        };
    }

    window[MODULE_NAME] = createRightSideAdPage();

    window.PageReady = function () {
        window[MODULE_NAME].init();
    };

    window.contentReady = function (e) {
        window[MODULE_NAME].setGridEvent(e);
    };

    window.editButtonClicked = function (e) {
        window[MODULE_NAME].edit(e);
    };

    window.deleteButtonClicked = function (e) {
        window[MODULE_NAME].delete(e);
    };

    window.paletteButtonClicked = function (e) {
        window[MODULE_NAME].palette(e);
    };

    window.showPaletteButton = function (e) {
        return window[MODULE_NAME].showPaletteButton(e);
    };

})(window, jQuery, window.co);