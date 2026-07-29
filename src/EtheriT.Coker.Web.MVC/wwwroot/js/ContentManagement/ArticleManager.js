var DirectoryId = 0, DirectoryType = "n";
let editor, directoryDatailList, ArticletForms, $ArticletTags, ArticletId, setPage;
var total_files = [];
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

    function BackToList() {
        if (articleOnly) return DirectoryId = 0, DirectoryType = "Articles", void ("#Articles_0" !== window.location.hash ? window.location.hash = "Articles_0" : MoveToItemList());
        resetWorkspaceBackButton();
        $("#pages>.card").addClass("d-none"), $("#DirectoryList,#TopLine").removeClass("d-none"), 
        DirectoryId = 0, DirectoryType = "n", window.location.hash = "";
    }
    function MoveToItemList() {
        const para = window.location.hash.replace("#", "").split("_");
        plan = "", $("#pages>.card,#TopLine").addClass("d-none"), $("#DirectoryItemps").removeClass("d-none"), 
        articleOnly && ($("#TopLine").removeClass("d-none"), $("#TopLine .title").text("文章管理")), 
        resetWorkspaceBackButton(),
        0 == $(`#DirectoryItemps>.${para[0].toLowerCase()}`).removeClass("d-none").length ? BackToList() : para.length > 1 && !isNaN(para[1]) && (DirectoryId = parseInt(para[1]), 
        DirectoryType = para[0], "function" == typeof articleManagerOptions.setDirectoryContext && articleManagerOptions.setDirectoryContext(DirectoryId, DirectoryType), 
        $("body").removeClass("grapesEdit"), $(".linkToFront").addClass("d-none"), "Articles" === DirectoryType ? (directoryDatailList.component.refresh(), 
        $(".data_upload").each((function() {
            UploadPreviewFrameClear($(this));
        })), $(".data_upload > ul > .upload_list").remove(), total_files = [], $(".data_upload").remove()) : BackToList());
    }
    function MoveToItemArticle() {
        const para = window.location.hash.replace("#", "").split("_");
        if ($("#pages>.card,#TopLine").addClass("d-none"), $ArticletTags && "function" == typeof $ArticletTags.TagDataClear && $ArticletTags.TagDataClear(), 
        para.length > 2 && !isNaN(para[1]) && !isNaN(para[2])) {
            const id = parseInt(para[2]);
            switch (DirectoryId = parseInt(para[1]), "function" == typeof articleManagerOptions.setDirectoryContext && articleManagerOptions.setDirectoryContext(DirectoryId, "Articles"), 
            para[0]) {
              case "ArticlesEditor":
                const _dfr = $.Deferred();
                articleOnly ? ($("#DirectoryItemps").data("dir", {
                    id: 0,
                    title: "全部",
                    tagDatas: []
                }), _dfr.resolve()) : co.Directory.Get(DirectoryId).done((result => {
                    $("#DirectoryItemps").data("dir", result), _dfr.resolve();
                })), co.Form.clear("ArticletForm"), setArticlePopularValue(0), syncAdvancedSettingsVisibility(null), id > 0 ? co.Articles.GetDataOne(id).done((function(result) {
                    null != result ? (ArticletId = result.id, setArticleSaveMode(!0 === result.canSave || !0 === result.CanSave), 
                    result.startEndDate = 0, result.sortCheckbox = 1, result.ImageUpload = 1, $(".linkToFront").removeClass("d-none").attr("href", `${defaultUrl}/${OrgName}/search/article/${result.id}`), 
                    result.fileAreas.length > 0 && result.fileAreas.forEach((function(area) {
                        var item = $($("#TemplateArticleFile").html()).clone(), item_title = item.find(".upload_title"), item_upload_frame = item.find(".upload_frame");
                        "File" == area.type ? item_title.text(`${area.label} (單一檔案區塊)`) : item_title.text(`${area.label} (多檔案區塊)`), 
                        item.attr({
                            "data-edit-type": area.type,
                            "data-key": area.key.toLowerCase(),
                            "data-label": area.label
                        }), item_upload_frame.attr("data-upload-id", `${area.key.toLowerCase()}file`), $("#ArticleFileAreas").append(item);
                    })), co.File.ListFileInit(), co.Form.insertData(result, "#ArticletForm"), setArticlePopularValue(result.popular ?? result.Popular), 
                    syncAdvancedSettingsVisibility(result), $ArticletTags.TagDataSet(result.tagDatas),
                    result.files.forEach((file => {
                        UploadListAdd(file, $(`.data_upload[data-key="${file.areakey.toLowerCase()}"]`));
                    }))) : BackToList();
                })) : (setArticleSaveMode(!0), _dfr.promise().done((function() {
                    const directory = $("#DirectoryItemps").data("dir");
                    directory && Array.isArray(directory.tagDatas) && $ArticletTags.TagDataSet(directory.tagDatas);
                }))), $("#ArticleContent").removeClass("d-none");
                break;

              case "ArticlesEditorView":
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
    function UploadListAdd(result, $target) {
        var isUseLessFile = !1;
        0 == $target.length && (0 == $("#UselessFileFrame").length && ((item = $($("#TemplateArticleFile").html()).clone()).find(".upload_title").text("無對應區塊檔案 "), 
        item.attr({
            Id: "UselessFileFrame",
            "data-edit-type": "Files"
        }), $("#ArticleFileAreas").append(item), co.File.ListFileInit()), $target = $("#UselessFileFrame"), 
        isUseLessFile = !0);
        var item, item_name = (item = $($("#TemplateUploadList").html()).clone()).find("input[name='name']"), item_serno = item.find(".ser_no"), item_size = item.find("span.size"), item_btn_preview = item.find(".btn_preview"), item_btn_remove = item.find(".btn_remove"), item_btn_lock = item.find(".btn_lock"), item_visible = item.find("label.visible");
        item_visible.find("input").prop("checked", !0), isUseLessFile && (item.prepend('<select class="form-select form-select-sm area_select" aria-label="AreaKey Select" name="editkey"><option selected disabled value="">請選擇對應區塊</option></select>'), 
        $(".data_upload").not("#UselessFileFrame").each((function() {
            var $this = $(this), option = `<option value="${$this.data("label")}">${$this.data("label")}</option>`;
            item.find("select.area_select").append(option);
        })), item.find("select.area_select").on("change", (function() {
            var $this = $(this), $parent = $this.parents("li.upload_list"), $RelatedFrame = $(`.data_upload[data-label="${$this.val()}"]`);
            UploadListAdd(result, $RelatedFrame), 0 == $parent.siblings("li.upload_list ").length ? $parent.parents(".data_upload").remove() : $parent.remove();
        })));
        var tempId = total_files.length;
        void 0 === file_num && (file_num = 0);
        var file_num = $target.find("ul > li.upload_list").length;
        if ($target.find("ul > li").each((function() {
            var $self = $(this);
            $self.hasClass("upload_list") && "" == $self.find("input[name='name']").val() && $self.remove();
        })), null == result) file_num += 1, item.data("tempid", tempId), item.data("serno", file_num), 
        item_serno.val(file_num), 0 == $target.find(".select_frame").length && void 0 !== $target.data("uploadtype") ? item.data("uploadtype", $target.data("uploadtype")) : item.data("uploadtype", 0), 
        item.data("edit", !1), item.on("click", (function() {
            co.File.ListFile($(this));
        })); else if (void 0 === result.id) item.attr({
            "data-tempid": result.TempId,
            "data-serno": file_num,
            "data-uploadtype": result.Type,
            "data-oldname": result.Name,
            "data-edit": !1
        }), item_name.val(result.Name), item_name.attr("placeholder", result.Name), item_serno.val(file_num), 
        item_btn_preview.data("priviewUrl", URL.createObjectURL(result.File)), result.File.size < 1024 ? item_size.text(result.File.size + " B") : result.File.size < 1048576 ? item_size.text((result.File.size / 1024).toFixed(1) + " KB") : result.File.size < 1073741824 ? item_size.text((result.File.size / 1048576).toFixed(1) + " MB") : item_size.text((result.File.size / 1073741824).toFixed(1) + " GB"); else {
            file_num += 1, item.attr({
                "data-id": result.id,
                "data-serno": file_num,
                "data-oldserno": file_num,
                "data-oldname": result.name,
                "data-uploadtype": result.fileType,
                "data-edit": !1,
                "data-old-isvisible": result.isVisible,
                "data-old-editkey": result.areakey
            }), item_serno.val(file_num), item_name.val(result.name), item_name.attr("placeholder", result.name), 
            item_size.text(result.size), item_btn_preview.data("priviewUrl", result.link[0]), 
            item_visible.find("input").prop("checked", result.isVisible), result.isEncryption && (item_btn_lock.addClass("lock"), 
            item_btn_lock.attr({
                title: "已上鎖檔案不可解鎖",
                "data-status": "locked"
            }));
            var obj = {};
            obj.Id = result.id, obj.Name = result.name, obj.SerNo = file_num;
            var link = result.link[0];
            4 == result.fileType ? obj.File = result.name : obj.File = link, obj.Type = result.fileType, 
            obj.IsDelete = !1, obj.IsEncryption = result.isEncryption, total_files.push(obj);
        }
        item_serno.on("blur", (function() {
            var $self = $(this), $uploadList = $target.find(".upload_list");
            $self.val() < 1 ? $self.val(1) : $self.val() > $uploadList.length && $self.val($uploadList.length), 
            $self.val() != item.data("serno") && ($self.val() > item.data("serno") ? (SortChange($uploadList, "bigger", item.data("serno"), $self.val()), 
            $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).after(item)) : $self.val() < item.data("serno") && (SortChange($uploadList, "smaller", $self.val(), item.data("serno")), 
            $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).before(item))), 
            item.data("serno", $self.val());
        })), item_name.on("blur", (function() {
            var $self = $(this);
            "" == $self.val() && $self.val(item.data("oldname"));
        })), item_btn_preview.on("click", (function(e) {
            e.preventDefault(), window.open(item_btn_preview.data("priviewUrl"), "_blank");
        })), item_btn_lock.on("click", (function(e) {
            e.preventDefault();
            var $self = $(this);
            "locked" == $self.data("status") ? co.sweet.warn("操作無效", "已上鎖檔案不可解鎖。") : $self.toggleClass("lock");
        })), item_btn_remove.on("click", (function(e) {
            e.preventDefault();
            var $self = $(this).parents("li").first(), $uploadList = $target.find(".upload_list"), file_num = $target.find("ul > li.upload_list").length;
            if (item.data("serno") < file_num && SortChange($uploadList, "bigger", item.data("serno"), file_num), 
            void 0 !== $self.data("id")) total_files.find((item => item.Id == $self.data("id"))).IsDelete = !0; else if (void 0 !== $self.data("tempid")) {
                var tempid = $self.data("tempid"), index = total_files.findIndex((item => item.TempId == tempid));
                index >= 0 && (total_files.splice(index, 1), total_files.forEach((file => {
                    file.TempId = file.TempId > tempid ? file.TempId - 1 : file.TempId;
                })));
            }
            UploadPreviewFrameClear($target), $self.remove();
        })), $target.find("ul > .btn_upload_add").before(item), co.File.ListFile(item);
    }
    function SortChange($self, change, minindex, maxindex) {
        $self.each((function() {
            var $li_self = $(this);
            "bigger" == change ? $li_self.data("serno") > minindex && $li_self.data("serno") <= maxindex && ($li_self.find(".ser_no").val(parseInt($li_self.data("serno")) - 1), 
            $li_self.data("serno", $li_self.find(".ser_no").val())) : "smaller" == change && $li_self.data("serno") >= minindex && $li_self.data("serno") < maxindex && ($li_self.find(".ser_no").val(parseInt($li_self.data("serno")) + 1), 
            $li_self.data("serno", $li_self.find(".ser_no").val()));
        }));
    }
    function UploadPreviewFrameClear($target) {
        var $self = $target.find(".preview_frame");
        $self.find(".default_frame").addClass("d-flex"), $self.find(".upload_frame").addClass("d-none"), 
        $self.find(".media_frame").removeClass("d-flex"), $self.find(".youtube_frame").removeClass("d-flex"), 
        $self.find(".select_frame").removeClass("d-flex"), $self.find(".youtube_preview").empty(), 
        $self.find(".media_preview > div").empty();
    }
    function canHandleHash(hash) {
        return /^Articles(?:Editor|EditorView)?_\d+(?:_\d+)?$/.test(hash);
    }
    setPage = function(id) {
        $("body").addClass("grapesEdit"), showCanvasBackButton(), editor || (editor = GrapesEditorManager.create("article", {
            container: "#gjs",
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
        window.location.hash = `ArticlesEditor_${DirectoryId}_${e.row.key}`;
    }, window.paletteArticlesButtonClicked = function(e) {
        window.location.hash = `ArticlesEditorView_${DirectoryId}_${e.row.key}`;
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
                Array.from(ArticletForms).forEach((function(form) {
                    "true" !== form.dataset.advancedValidationBound && (form.addEventListener("invalid", (function(event) {
                        event.target.closest("#ArticleAdvancedSettings") && (document.getElementById("ArticleAdvancedSettings").open = !0);
                    }), !0), form.dataset.advancedValidationBound = "true");
                    form.addEventListener("submit", (function(event) {
                        if (event.preventDefault(), !articleCanSave) return event.stopPropagation(), void showArticleReadonlyMessage();
                        plan = event.submitter && event.submitter.classList.contains("btn_to_canvas") ? "canvas" : "",
                        form.checkValidity() ? Coker.sweet.confirm("即將儲存", "儲存後將顯示於文章列表", "儲存", "取消", (function() {
                            !function(success_text) {
                                const data = co.Form.getJson($(ArticletForms).attr("id"));
                                null != $("#ImageUpload .img_input_frame").data("delectList") && co.File.DeleteFileById({
                                    Sid: data.id,
                                    Type: 6,
                                    Fid: $("#ImageUpload .img_input_frame").data("delectList")
                                }), co.Articles.AddUp(data).done((result => {
                                    co.sweet.loading();
                                    var requests = [];
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
                                    total_files.length > 0 && ($(".data_upload > ul > li.upload_list").each((function() {
                                        var $self = $(this), $parentarea = $self.parents(".data_upload"), data = [];
                                        if (void 0 !== $self.data("id") ? data = total_files.find((item => $self.data("id") == item.Id)) : void 0 !== $self.data("tempid") && (data = total_files.find((item => $self.data("tempid") == item.TempId))), 
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
                                            var SerNoChange = data.SerNo != Number($self.find(".ser_no").val()), FileNameChange = $self.data("oldname") != $self.find("input[name='name']").val(), IsVisibleChange = $self.data("old-isvisible") != $self.find("label.visible input").prop("checked"), AreaKeyChange = normalizeFileAreaKey($self.data("old-editkey")) !== normalizeFileAreaKey($parentarea.data("key"));
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
                                    })), total_files.forEach((file => {
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
                                            directoryDatailList.component.refresh(), location.hash = "canvas" == plan ? `ArticlesEditorView_${DirectoryId}_${result.message}` : `Articles_${DirectoryId}`;
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
                                    co.sweet.error("文章儲存發生錯誤");
                                }));
                            }("已成功儲存");
                        })) : event.stopPropagation(), form.classList.add("was-validated");
                    }), !1);
                })), $("#ArticleContent .btn_back").off("click.articleManager").on("click.articleManager", (function() {
                    const dir = $("#DirectoryItemps").data("dir") || {
                        id: 0,
                        title: "全部"
                    }, listTitle = articleOnly ? "全部文章列表" : `${dir.title}文章列表`;
                    Coker.sweet.confirm(`返回${listTitle}`, "資料將不被保存", "確定", "取消", (function() {
                        directoryDatailList && directoryDatailList.component.refresh(), window.location.hash = `Articles_${articleOnly ? 0 : dir.id}`;
                    }));
                })), $("#DirectoryItemps .btn_add").off("click.articleManager").on("click.articleManager", (function() {
                    window.location.hash = `ArticlesEditor_${DirectoryId}_0`;
                })), $("#DirectoryItemps .btn_back").off("click.articleManager").on("click.articleManager", (function() {
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
            normalizeLegacyHash: function(hash) {
                return !(!articleOnly || ("" === hash ? (window.location.hash = "Articles_0", 0) : /^\d+-1$/.test(hash) ? (window.location.hash = `ArticlesEditorView_0_${parseInt(hash)}`, 
                0) : !/^\d+$/.test(hash) || (window.location.hash = `ArticlesEditor_0_${hash}`, 
                0)));
            },
            canHandleHash: canHandleHash,
            handleHash: function(hash) {
                if (!canHandleHash(hash)) return !1;
                if (hash.indexOf("Editor") >= 0) return MoveToItemArticle(), !0;
                const openList = function() {
                    directoryDatailList ? MoveToItemList() : setTimeout(openList, 50);
                };
                return openList(), !0;
            }
};

export { ArticleManager };
