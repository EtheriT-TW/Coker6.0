var DirectoryId = 0, DirectoryType = "n";
let editor, directoryDatailList, ArticletForms, $ArticletTags, ArticletId, setPage;
var articleFileManagers = [];
let plan = "", articleCanSave = !0, articleOnly = !1, articleManagerOptions = {};

    function getWorkspaceBackButton() {
        return $('#TopLine [data-role="workspace-back"]');
    }

    function showCanvasBackButton() {
        getWorkspaceBackButton()
            .removeClass("d-none")
            .attr("href", `#Articles_${DirectoryId}`)
            .text("回到文章列表");
    }

    function resetWorkspaceBackButton() {
        const $button = getWorkspaceBackButton();
        $button
            .attr("href", "#")
            .text($button.data("default-text") || "返回選單")
            .toggleClass("d-none", articleOnly);
    }

    function setArticlePopularValue(value) {
        const popular = Number(value);
        $("#ArticlePopularValue").text(
            Number.isFinite(popular) ? popular.toLocaleString("zh-TW") : "0"
        );
    }

    function syncAdvancedSettingsVisibility(data) {
        const details = document.getElementById("ArticleAdvancedSettings");
        if (!details) return;

        if (!data) {
            details.open = false;
            return;
        }

        const longitude = data.longitude ?? data.Longitude;
        const latitude = data.latitude ?? data.Latitude;
        const permanent = data.permanent ?? data.Permanent;
        const serNo = Number(data.serNO ?? data.SerNO ?? 500);
        const hasLocation =
            longitude !== null && longitude !== undefined && longitude !== "" ||
            latitude !== null && latitude !== undefined && latitude !== "";

        details.open = permanent === false || hasLocation || serNo !== 500;
    }

    function setArticleSaveMode(canSave) {
        articleCanSave = !0 === canSave;
        const $articleContent = $("#ArticleContent");
        $articleContent.attr("data-can-save", articleCanSave ? "true" : "false"), $articleContent.find(".btn_done").toggleClass("d-none", !articleCanSave), 
        $articleContent.find(".btn_to_canvas").toggleClass("d-none", !articleCanSave), $articleContent.find(".btn_permission_details").toggleClass("d-none", !articleCanSave), 
        $articleContent.find(".readonly-hint").remove(), articleCanSave || $articleContent.find(".card-body").first().prepend('\n            <div class="readonly-hint alert alert-warning py-2 mb-3">\n                此目錄已設定權限，您目前僅能檢視，不能儲存修改。\n            </div>\n        '), 
        editor && "function" == typeof editor.setSavePanelVisible && editor.setSavePanelVisible(articleCanSave);
    }
    function showArticleReadonlyMessage() {
        co.sweet.warn("無法儲存", "此目錄已設定權限，您目前僅能檢視，不能儲存修改。");
    }
    function wrapRequest(req, meta) {
        var dfd = $.Deferred();
        return req && "function" == typeof req.done && "function" == typeof req.fail ? (req.done((function(data, textStatus, jqXHR) {
            const payload = {
                success: !0,
                meta: meta,
                data: data,
                textStatus: textStatus,
                httpStatus: jqXHR ? jqXHR.status : req.status,
                responseText: jqXHR ? jqXHR.responseText || "" : req.responseText || ""
            };
            dfd.resolve(payload);
        })), req.fail((function(jqXHR, textStatus, errorThrown) {
            const payload = {
                success: !1,
                meta: meta,
                textStatus: textStatus,
                error: errorThrown,
                httpStatus: jqXHR ? jqXHR.status : req.status,
                responseText: jqXHR ? jqXHR.responseText || "" : req.responseText || ""
            };
            dfd.resolve(payload);
        })), dfd.promise()) : (dfd.resolve({
            success: !1,
            meta: meta,
            httpStatus: void 0,
            responseText: "",
            reason: "invalid-request-object"
        }), dfd.promise());
    }

    function normalizeFileAreaKey(value) {
        return String(value ?? "").trim().toLowerCase();
    }

    function getArticleFiles() {
        return articleFileManagers.flatMap(manager => manager.getFiles());
    }

    function destroyArticleFileAreas() {
        articleFileManagers.forEach(manager => manager.destroy());
        articleFileManagers = [];
        $("#ArticleFileAreas").empty();
    }

    function getArticleFileManager($root) {
        return $root && $root.data("file-list-manager");
    }

    function renderArticleFileRow($row, file) {
        if (file.Id !== undefined && file.OriginalSerNo === undefined) {
            file.OriginalSerNo = file.SerNo;
            file.OriginalName = file.Name;
            file.OriginalIsVisible = file.IsVisible !== false;
            file.OriginalAreaKey = file.AreaKey;
        }
        $row.data({
            oldname: file.OriginalName ?? file.Name,
            oldserno: file.OriginalSerNo ?? file.SerNo,
            "old-isvisible": file.OriginalIsVisible ?? (file.IsVisible !== false),
            "old-editkey": file.OriginalAreaKey ?? file.AreaKey
        });

        if (this.$root.attr("id") !== "UselessFileFrame") return;
        const $select = $('<select class="form-select form-select-sm area_select" aria-label="AreaKey Select" name="editkey"><option selected disabled value="">請選擇對應區塊</option></select>');
        articleFileManagers.forEach(manager => {
            if (manager === this || manager.$root.attr("id") === "UselessFileFrame") return;
            const label = manager.$root.data("label");
            $select.append($("<option>", { value: label, text: label }));
        });
        $row.prepend($select);
    }

    function createArticleFileArea($root) {
        const manager = new Coker.FileListManager($root, {
            type: Coker.FileListManager.Types.File,
            template: "#TemplateUploadList",
            renderItem: renderArticleFileRow
        });
        $root.off("blur.articleFileArea").on("blur.articleFileArea", 'input[name="name"]', function () {
            const $input = $(this);
            if (!$input.val()) $input.val($input.closest(".upload_list").data("oldname") || "");
        });
        articleFileManagers.push(manager);
        return manager;
    }

    function createUselessFileArea() {
        let $root = $("#UselessFileFrame");
        if ($root.length) return getArticleFileManager($root);
        $root = $($("#TemplateArticleFile").html()).clone();
        $root.attr({ id: "UselessFileFrame", "data-edit-type": "Files" });
        $root.find(".upload_title").text("無對應區塊檔案");
        $root.find(".upload_frame").attr("data-upload-id", "uselessarticlefile");
        $("#ArticleFileAreas").append($root);
        return createArticleFileArea($root);
    }

    function addArticleFile(file, $target) {
        const manager = $target && $target.length ? getArticleFileManager($target) : createUselessFileArea();
        return manager.add(file);
    }

    function transferArticleFile($select) {
        const $row = $select.closest(".upload_list");
        const $sourceRoot = $row.closest(".data_upload");
        const $targetRoot = $("#ArticleFileAreas .data_upload").filter(function () {
            return $(this).data("label") === $select.val();
        }).first();
        const source = getArticleFileManager($sourceRoot);
        const target = getArticleFileManager($targetRoot);
        if (!source || !target) return;
        source.transferTo($row, target);
        if ($sourceRoot.attr("id") === "UselessFileFrame" && !$sourceRoot.find(".upload_list").length) {
            source.destroy();
            articleFileManagers = articleFileManagers.filter(manager => manager !== source);
            $sourceRoot.remove();
        }
    }

    function navigate(hash) {
        if ("function" == typeof articleManagerOptions.navigate) {
            articleManagerOptions.navigate(hash);
            return;
        }

        window.location.hash = hash;
    }

    function BackToList() {
        if (articleOnly) return DirectoryId = 0, DirectoryType = "Articles", void navigate("Articles_0");
        resetWorkspaceBackButton();
        $("#pages>.card").addClass("d-none"), $("#DirectoryList,#TopLine").removeClass("d-none"), 
        DirectoryId = 0, DirectoryType = "n", "function" == typeof articleManagerOptions.backToDirectoryList && articleManagerOptions.backToDirectoryList();
    }
    function MoveToItemList(route) {
        plan = "", $("#pages>.card,#TopLine").addClass("d-none"), $("#ArticleWorkspace").removeClass("d-none"),
        articleOnly && ($("#TopLine").removeClass("d-none"), $("#TopLine .title").text("文章管理")), 
        resetWorkspaceBackButton(),
        0 == $("#ArticleList").removeClass("d-none").length ? BackToList() : (DirectoryId = Number(route.directoryId || 0),
        DirectoryType = "Articles", "function" == typeof articleManagerOptions.setDirectoryContext && articleManagerOptions.setDirectoryContext(DirectoryId, DirectoryType),
        $("body").removeClass("grapesEdit"), $(".linkToFront").addClass("d-none"), "Articles" === DirectoryType ? (directoryDatailList.component.refresh(), 
        destroyArticleFileAreas()) : BackToList());
    }
    function MoveToItemArticle(route) {
        if ($("#pages>.card,#TopLine").addClass("d-none"), $ArticletTags && "function" == typeof $ArticletTags.TagDataClear && $ArticletTags.TagDataClear(), 
        route && Number.isInteger(route.directoryId) && Number.isInteger(route.articleId)) {
            const id = route.articleId;
            switch (DirectoryId = route.directoryId, "function" == typeof articleManagerOptions.setDirectoryContext && articleManagerOptions.setDirectoryContext(DirectoryId, "Articles"),
            route.mode) {
              case "article-editor":
                const _dfr = $.Deferred();
                articleOnly ? ($("#ArticleWorkspace").data("dir", {
                    id: 0,
                    title: "全部",
                    tagDatas: []
                }), _dfr.resolve()) : co.Directory.Get(DirectoryId).done((result => {
                    $("#ArticleWorkspace").data("dir", result), _dfr.resolve();
                })), destroyArticleFileAreas(), co.Form.clear("ArticletForm"), setArticlePopularValue(0), syncAdvancedSettingsVisibility(null), id > 0 ? co.Articles.GetDataOne(id).done((function(result) {
                    null != result ? (ArticletId = result.id, setArticleSaveMode(!0 === result.canSave || !0 === result.CanSave), 
                    result.startEndDate = 0, result.sortCheckbox = 1, result.ImageUpload = 1, $(".linkToFront").removeClass("d-none").attr("href", `${defaultUrl}/${OrgName}/search/article/${result.id}`), 
                    result.fileAreas.length > 0 && result.fileAreas.forEach((function(area) {
                        var item = $($("#TemplateArticleFile").html()).clone(), item_title = item.find(".upload_title"), item_upload_frame = item.find(".upload_frame");
                        "File" == area.type ? item_title.text(`${area.label} (單一檔案區塊)`) : item_title.text(`${area.label} (多檔案區塊)`), 
                        item.attr({
                            "data-edit-type": area.type,
                            "data-key": area.key.toLowerCase(),
                            "data-label": area.label
                        }), item_upload_frame.attr("data-upload-id", `${area.key.toLowerCase()}file`), $("#ArticleFileAreas").append(item), createArticleFileArea(item);
                    })), co.Form.insertData(result, "#ArticletForm"), setArticlePopularValue(result.popular ?? result.Popular),
                    syncAdvancedSettingsVisibility(result), $ArticletTags.TagDataSet(result.tagDatas),
                    result.files.forEach((file => {
                        addArticleFile(file, $(`#ArticleFileAreas .data_upload[data-key="${file.areakey.toLowerCase()}"]`));
                    }))) : BackToList();
                })) : (setArticleSaveMode(!0), _dfr.promise().done((function() {
                    const directory = $("#ArticleWorkspace").data("dir");
                    directory && Array.isArray(directory.tagDatas) && $ArticletTags.TagDataSet(directory.tagDatas);
                }))), $("#ArticleContent").removeClass("d-none");
                break;

              case "article-canvas":
                $("#HtmlCanvas,#TopLine").removeClass("d-none"), $("#gjs").data("id", id), 
                setPage(id);
                break;

              default:
                BackToList();
            }
        }
    }
    function waitGrapesEditorReady(callback, retryCount) {
        retryCount = retryCount || 0, editor && editor.DomComponents && "undefined" != typeof co && co.Grapes && "function" == typeof co.Grapes.setEditor ? callback(editor) : retryCount >= 30 ? co.sweet.error("錯誤", "編輯器尚未初始化完成，請重新進入編輯畫面。") : setTimeout((function() {
            waitGrapesEditorReady(callback, retryCount + 1);
        }), 100);
    }
    function parseRoute(hash) {
        const value = String(hash || "").trim();
        let match = /^Articles_(\d+)$/i.exec(value);

        if (match) {
            return {
                raw: value,
                scope: "article",
                mode: "article-list",
                pageKey: "ArticleList",
                title: "文章列表",
                directoryId: parseInt(match[1], 10),
                articleId: 0
            };
        }

        match = /^ArticlesEditor_(\d+)_(\d+)$/i.exec(value);
        if (match) {
            return {
                raw: value,
                scope: "article",
                mode: "article-editor",
                pageKey: "ArticleEditor",
                title: parseInt(match[2], 10) > 0 ? "編輯文章" : "新增文章",
                directoryId: parseInt(match[1], 10),
                articleId: parseInt(match[2], 10)
            };
        }

        match = /^ArticlesEditorView_(\d+)_(\d+)$/i.exec(value);
        if (match) {
            return {
                raw: value,
                scope: "article",
                mode: "article-canvas",
                pageKey: "ArticleCanvas",
                title: "文章內容編輯",
                directoryId: parseInt(match[1], 10),
                articleId: parseInt(match[2], 10)
            };
        }

        if (!articleOnly) return null;

        if (!value || value.toLowerCase() === "list") {
            return {
                raw: "Articles_0",
                scope: "article",
                mode: "article-list",
                pageKey: "ArticleList",
                title: "文章管理",
                directoryId: 0,
                articleId: 0
            };
        }

        match = /^(\d+)-1$/.exec(value);
        if (match) {
            return {
                raw: value,
                scope: "article",
                mode: "article-canvas",
                pageKey: "ArticleCanvas",
                title: "文章內容編輯",
                directoryId: 0,
                articleId: parseInt(match[1], 10)
            };
        }

        if (/^\d+$/.test(value)) {
            return {
                raw: value,
                scope: "article",
                mode: "article-editor",
                pageKey: "ArticleEditor",
                title: "編輯文章",
                directoryId: 0,
                articleId: parseInt(value, 10)
            };
        }

        return null;
    }
    setPage = function(id) {
        $("body").addClass("grapesEdit"), showCanvasBackButton(), editor || (editor = GrapesEditorManager.create("article", {
            container: "#gjs",
            restoreHistory: { source: "Article" },
            getPageId: function() {
                return Number($("#gjs").data("id") || 0);
            },
            canSave: function() {
                return !0 === articleCanSave;
            },
            readonlyMessage: showArticleReadonlyMessage,
            save: function(html, css) {
                var deferred = $.Deferred();
                return co.Articles.SaveConten({
                    Id: $("#gjs").data("id"),
                    SaveHtml: html,
                    SaveCss: css
                }).done((function(result) {
                    result.success ? deferred.resolve() : co.sweet.error(result.error);
                })), deferred.promise();
            },
            import: function(html, css) {
                var deferred = $.Deferred();
                return co.Articles.ImportConten({
                    Id: $("#gjs").data("id"),
                    SaveHtml: html,
                    SaveCss: css
                }).done((function(result) {
                    result.success ? deferred.resolve() : co.sweet.error(result.error);
                })), deferred.promise();
            },
            getComponer: function() {
                var deferred = $.Deferred();
                return co.HtmlContent.GetAllComponent().done((function(result) {
                    result.success ? deferred.resolve(result.list) : co.sweet.error(result.error);
                })), deferred.promise();
            }
        })), $("#gjs").data("id", id), co.Articles.GetConten({
            Id: id
        }).done((function(result) {
            if (result.success) {
                setArticleSaveMode(!0 === result.canSave || !0 === result.CanSave), editor && "function" == typeof editor.setSavePanelVisible && editor.setSavePanelVisible(articleCanSave);
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                waitGrapesEditorReady((function(readyEditor) {
                    co.Grapes.setEditor(readyEditor, html, result.conten.saveCss), co.Grapes.setFile(readyEditor, id, 2),
                    showCanvasBackButton(), result.title && $("#TopLine .title").text(result.title);
                }));
            } else co.sweet.error(result.error);
        }));
    }, window.DirectoryDatailListReady = function(e) {
        directoryDatailList = e;
    }, window.articleRowPrepared = function(e) {
        "data" === e.rowType && e.data && (articleOnly || !0 === e.data.CanEdit || !0 === e.data.CanSave || $(e.rowElement).addClass("article-readonly-row").attr("title", "此文章目前僅能檢視，不能儲存或刪除"));
    }, window.editArticlesButtonClicked = function(e) {
        navigate(`ArticlesEditor_${DirectoryId}_${e.row.key}`);
    }, window.paletteArticlesButtonClicked = function(e) {
        navigate(`ArticlesEditorView_${DirectoryId}_${e.row.key}`);
    }, window.deleteArticlesButtonClicked = function(e) {
        articleOnly || e && e.row && e.row.data && (!0 === e.row.data.CanEdit || !0 === e.row.data.CanSave) ? Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", (function() {
            co.Articles.Delete(e.row.key).done((function(result) {
                result.success ? e.component.refresh() : co.sweet.error(result.error || "文章刪除失敗");
            }));
        })) : co.sweet.warn("無法刪除", "此目錄已設定權限，您目前僅能檢視，不能刪除文章。");
    };

const ArticleManager = {
            init: function(options) {
                articleManagerOptions = options || {}, articleOnly = !0 === articleManagerOptions.articleOnly, 
                ArticletForms = $("#ArticletForm"), $ArticletTags = ArticletForms.find(".InputTag").TagListModalInit(), 
                $("#ArticleFileAreas").off("change.articleFileArea").on("change.articleFileArea", ".area_select", function() { transferArticleFile($(this)); }),
                Array.from(ArticletForms).forEach((function(form) {
                    "true" !== form.dataset.advancedValidationBound && (form.addEventListener("invalid", (function(event) {
                        event.target.closest("#ArticleAdvancedSettings") && (document.getElementById("ArticleAdvancedSettings").open = !0);
                    }), !0), form.dataset.advancedValidationBound = "true");
                    co.Form.init(form.getAttribute("id"), function(_formId, context) {
                        if (!articleCanSave) {
                            showArticleReadonlyMessage();
                            return null;
                        }

                        plan = context.submitter && context.submitter.classList.contains("btn_to_canvas") ? "canvas" : "";

                        return co.Form.confirmSubmit({
                            title: "即將儲存",
                            text: "儲存後將顯示於文章列表",
                            confirmButtonText: "儲存",
                            cancelButtonText: "取消",
                            onConfirm: function() {
                                const saveDeferred = $.Deferred();
                            !function(success_text) {
                                const data = co.Form.getJson($(ArticletForms).attr("id"));
                                const pendingRequests = [];
                                null != $("#ImageUpload .img_input_frame").data("delectList") && pendingRequests.push(wrapRequest(co.File.DeleteFileById({
                                    Sid: data.id,
                                    Type: 6,
                                    Fid: $("#ImageUpload .img_input_frame").data("delectList")
                                }), {
                                    action: "圖片刪除",
                                    areaKey: "ImageUpload",
                                    fileName: "文章圖片",
                                    fileId: null,
                                    tempId: null
                                })), co.Articles.AddUp(data).done((result => {
                                    co.sweet.loading();
                                    var requests = pendingRequests;
                                    const imageFileData = $("#ImageUpload .img_input").data("file");
                                    if (null != imageFileData && null != imageFileData.File && 0 == imageFileData.id) {
                                        var formData = new FormData;
                                        formData.append("files", imageFileData.File), formData.append("type", 6), formData.append("sid", result.message), 
                                        formData.append("serno", 500), requests.push(wrapRequest(co.File.Upload(formData), {
                                            action: "圖片上傳",
                                            areaKey: "ImageUpload",
                                            fileName: imageFileData.name || imageFileData.File.name || "未命名圖片",
                                            fileId: null,
                                            tempId: null
                                        }));
                                    }
                                    var isFileUploaded = !1, isFileUpdated = !1, isFileDeleted = !1;
                                    const articleFiles = getArticleFiles();
                                    articleFiles.length > 0 && ($("#ArticleFileAreas .data_upload > ul > li.upload_list").each((function() {
                                        var $self = $(this), $parentarea = $self.parents(".data_upload"), data = [];
                                        if (void 0 !== $self.data("id") ? data = articleFiles.find((item => $self.data("id") == item.Id)) : void 0 !== $self.data("tempid") && (data = articleFiles.find((item => $self.data("tempid") == item.TempId))),
                                        void 0 === data.Id || !data.IsEncryption && $self.find(".btn_lock").hasClass("lock")) {
                                            var formData = new FormData;
                                            formData.append("files", data.File), formData.append("areakey", $parentarea.data("key")), 
                                            formData.append("type", 15), void 0 !== data.Id && formData.append("id", data.Id), 
                                            formData.append("sid", result.message), formData.append("serno", $self.find(".ser_no").val()), 
                                            formData.append("filename", $self.find("input[name='name']").val()), formData.append("isVisible", $self.find("label.visible input").prop("checked")), 
                                            formData.append("isEncryption", $self.find(".btn_lock").hasClass("lock")), void 0 !== data.Id && formData.append("id", data.Id), 
                                            requests.push(wrapRequest(co.File.Upload(formData), {
                                                action: "檔案上傳",
                                                areaKey: $parentarea.data("key"),
                                                fileName: $self.find("input[name='name']").val() || "未命名檔案",
                                                fileId: void 0 !== data.Id ? data.Id : null,
                                                tempId: void 0 !== $self.data("tempid") ? $self.data("tempid") : null
                                            })), isFileUploaded = !0;
                                        } else {
                                            var SerNoChange = $self.data("oldserno") != Number($self.find(".ser_no").val()), FileNameChange = $self.data("oldname") != $self.find("input[name='name']").val(), IsVisibleChange = $self.data("old-isvisible") != $self.find("label.visible input").prop("checked"), AreaKeyChange = normalizeFileAreaKey($self.data("old-editkey")) !== normalizeFileAreaKey($parentarea.data("key"));
                                            (SerNoChange || FileNameChange || IsVisibleChange || AreaKeyChange) && (requests.push(wrapRequest(co.File.fileDataChange({
                                                Id: data.Id,
                                                SId: result.message,
                                                SerNo: SerNoChange ? $self.find(".ser_no").val() : null,
                                                FileName: FileNameChange ? $self.find("input[name='name']").val() : null,
                                                IsVisible: IsVisibleChange ? $self.find("label.visible input").prop("checked") : null,
                                                AreaKey: AreaKeyChange ? $parentarea.data("key") : null
                                            }), {
                                                action: "檔案修改",
                                                areaKey: $parentarea.data("key"),
                                                fileName: $self.find("input[name='name']").val() || "未命名檔案",
                                                fileId: data.Id,
                                                tempId: null
                                            })), isFileUpdated = !0);
                                        }
                                    })), articleFiles.forEach((file => {
                                        if (void 0 !== file.IsDelete && 1 == file.IsDelete && void 0 !== file.Id) {
                                            var deleteid_list = [];
                                            deleteid_list.push(file.Id), requests.push(wrapRequest(co.File.DeleteFileById({
                                                Sid: parseInt(result.message),
                                                Type: 15,
                                                Fid: deleteid_list
                                            }), {
                                                action: "檔案刪除",
                                                areaKey: "DeleteFile",
                                                fileName: file.Name || "未命名檔案",
                                                fileId: file.Id,
                                                tempId: file.TempId
                                            })), isFileDeleted = !0;
                                        }
                                    }))), $.when.apply($, requests).done((function() {
                                        var results = [];
                                        if (1 === requests.length) results = [ arguments[0] ]; else for (let i = 0; i < arguments.length; i++) results.push(arguments[i]);
                                        function finishSave(allResults) {
                                            var errortext = [];
                                            $.each(allResults, (function(index, item) {
                                                if (item && !item.success) {
                                                    var name = item.meta?.fileName || `第 ${index + 1} 筆`, actionfail = item.meta?.action ? item.meta.action + "失敗" : "";
                                                    if (item.errorMessage) errortext.push(`【${name}】${item.errorMessage}`); else switch (item.httpStatus) {
                                                      case 400:
                                                        (item.responseText || "").includes("Request body too large") ? errortext.push(`【${name}】檔案過大 ${actionfail}`) : errortext.push(`【${name}】資料格式錯誤 ${actionfail}`);
                                                        break;

                                                      case 413:
                                                        errortext.push(`【${name}】檔案過大 ${actionfail}`);
                                                        break;

                                                      case 500:
                                                        errortext.push(`【${name}】伺服器錯誤 ${actionfail}`);
                                                        break;

                                                      case 0:
                                                        errortext.push(`【${name}】網路連線失敗 ${actionfail}`);
                                                        break;

                                                      default:
                                                        errortext.push(`【${name}】錯誤 (${item.httpStatus}) ${actionfail}`);
                                                    }
                                                }
                                            })), errortext.length > 0 ? co.sweet.error("錯誤", errortext.join("<br>")) : co.sweet.success(success_text, null, !0), 
                                            directoryDatailList && directoryDatailList.component.refresh(), navigate("canvas" == plan ? `ArticlesEditorView_${DirectoryId}_${result.message}` : `Articles_${DirectoryId}`),
                                            saveDeferred.resolve();
                                        }
                                        isFileUploaded || isFileUpdated || isFileDeleted ? co.Articles.RebuildContentWithFiles(parseInt(result.message)).done((function(rebuildResult) {
                                            rebuildResult && !0 === rebuildResult.success || results.push({
                                                success: !1,
                                                meta: {
                                                    action: "內容重建",
                                                    fileName: "文章檔案"
                                                },
                                                errorMessage: rebuildResult?.error || rebuildResult?.message || "內容重建失敗"
                                            }), finishSave(results);
                                        })).fail((function(xhr) {
                                            results.push({
                                                success: !1,
                                                meta: {
                                                    action: "內容重建",
                                                    fileName: "文章檔案"
                                                },
                                                httpStatus: xhr?.status || 0,
                                                responseText: xhr?.responseText || ""
                                            }), finishSave(results);
                                        })) : finishSave(results);
                                    }));
                                })).fail((function() {
                                    co.sweet.error("文章儲存發生錯誤"), saveDeferred.reject();
                                }));
                            }("已成功儲存");
                                return saveDeferred.promise();
                            }
                        });
                    });
                })), $("#ArticleContent .btn_back").off("click.articleManager").on("click.articleManager", (function() {
                    const dir = $("#ArticleWorkspace").data("dir") || {
                        id: 0,
                        title: "全部"
                    }, listTitle = articleOnly ? "全部文章列表" : `${dir.title}文章列表`;
                    Coker.sweet.confirm(`返回${listTitle}`, "資料將不被保存", "確定", "取消", (function() {
                        directoryDatailList && directoryDatailList.component.refresh(), navigate(`Articles_${articleOnly ? 0 : dir.id}`);
                    }));
                })), $("#ArticleWorkspace .btn_add").off("click.articleManager").on("click.articleManager", (function() {
                    navigate(`ArticlesEditor_${DirectoryId}_0`);
                })), $("#ArticleWorkspace .btn_back").off("click.articleManager").on("click.articleManager", (function() {
                    articleOnly || "function" != typeof articleManagerOptions.backToDirectoryList || articleManagerOptions.backToDirectoryList();
                })), $(".btn_permission_details").off("click.articleManager").on("click.articleManager", (function(event) {
                    event.preventDefault();
                    var articleTitle = ArticletForms.find("[name='title']").val();
                    $("#RolesDetailsModal").setRolesData({
                        pageId: ArticletId,
                        title: articleTitle,
                        type: 5
                    }).modal("show");
                })), ArticletForms.find(".title textarea").on("keyup.articleManager", (function() {
                    ArticletForms.find(".title .count").text($(this).val().length);
                })), ArticletForms.find(".describe textarea").on("keyup.articleManager", (function() {
                    ArticletForms.find(".describe .count").text($(this).val().length);
                }));
            },
            parseRoute: parseRoute,
            enterRoute: function(route) {
                if (!route || "article" !== route.scope) return !1;
                if ("article-list" !== route.mode) return MoveToItemArticle(route), !0;
                const openList = function() {
                    directoryDatailList ? MoveToItemList(route) : setTimeout(openList, 50);
                };
                return openList(), !0;
            }
};

export { ArticleManager };
