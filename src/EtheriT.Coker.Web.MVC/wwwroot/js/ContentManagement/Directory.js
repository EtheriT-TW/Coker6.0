var $btn_display, $bind_type, title, $title_text, $description, $description_text
var keyId, disp_opt = true, DirectoryId = 0, DirectoryType = "n";
let directory_list, editor, permissionDetailsModal;
let DirectorytForms, $DirectorytTags;
let ArticletForms, $ArticletTags, ArticletId;
var total_files = [];

function PageReady() {
    DirectorytForms = $('#DirectorytForm');
    ArticletForms = $('#ArticletForm');
    permissionDetailsModal = new bootstrap.Modal(document.getElementById("PermissionDetailsModal"));
    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.CanCreate) $(".btn_add").remove();
    });

    /* File Upload */
    co.File.ListFileInit();

    ElementInit();
    WebmenuListModalInit();
    $DirectorytTags = $(DirectorytForms).find(".InputTag").TagListModalInit();
    $ArticletTags = $(ArticletForms).find(".InputTag").TagListModalInit();

    editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.Articles.SaveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Articles.ImportConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });
    $bind_type.on("change", function () {
        switch (parseInt($bind_type.val())) {
            case 1:
            case 2:
                $(DirectorytForms).find(".webmenu > input").attr("disabled", "disabled")
                $(DirectorytForms).find(".tag > input").removeAttr("disabled");
                WebmenuDataClear();
                break;
            case 3:
                $(DirectorytForms).find(".tag > input").attr("disabled", "disabled");
                $(DirectorytForms).find(".webmenu > input").removeAttr("disabled", "disabled")
                $DirectorytTags.TagDataClear();
                break;
        }
    });

    (() => {
        Array.from(ArticletForms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於文章列表", "儲存", "取消", function () {
                        AddUpArticlet("已成功儲存", "儲存發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })();

    (() => {
        Array.from(DirectorytForms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於安排的位置", "儲存", "取消", function () {
                        AddUp("已成功儲存", "儲存發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })();

    $("#DirectoryContent .btn_back").on("click", function () {
        Coker.sweet.confirm("返回目錄列表", "資料將不被保存", "確定", "取消", function () {
            directory_list.component.refresh();
            BackToList();
        });
    });
    $("#ArticleContent .btn_back").on("click", function () {
        const dir = $("#DirectoryItemps").data("dir");
        Coker.sweet.confirm(`返回${dir.title}文章列表`, "資料將不被保存", "確定", "取消", function () {
            directoryDatailList.component.refresh();
            location.hash = `Articles_${dir.id}`;
        });
    })

    $("#DirectoryList .btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $("#DirectoryItemps .btn_add").on("click", function () {
        window.location.hash = `ArticlesEditor_${DirectoryId}_0`;
    });
    $("#DirectoryItemps .btn_back").off("click").on("click", function () {
        BackToList();
    });

    $(document).on('click', '.btn-open-facet', function (e) {
        e.preventDefault();
        co.DirectoryFacetModal.open(DirectoryId);
    });

    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })
    $(".btn_permission_details").on("click", function (e) {
        e.preventDefault();
        var title = $("#ArticletForm [name='title']").val();
        $("#RolesDetailsModal").setRolesData({ pageId: ArticletId, title: title, type: 5 }).modal("show");
    });

    $title_text.on('keyup', function () {
        var $self = $(this);
        $title.children("div").children(".count").text($self.val().length)
    });

    $description_text.on('keyup', function () {
        var $self = $(this);
        $description.children("div").children(".count").text($self.val().length)
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $(".btn_display");
    $bind_type = $("#BindType");
    $title = $(".title");
    $title_text = $title.children("textarea");
    $description = $(".description");
    $description_text = $description.children("textarea");

    $(DirectorytForms).find(".tag > input").attr("disabled", "disabled")
    $(DirectorytForms).find(".webmenu > input").attr("disabled", "disabled")
}

function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}

function HashDataEdit() {
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (!!hash && isNaN(hash)) {
                if (hash.indexOf("Editor") > -1) MoveToItemArticle();
                else {
                    let t = null;
                    const f = function () {
                        clearTimeout(t);
                        if (directoryDatailList != null) {
                            MoveToItemList();
                        } else t = setTimeout(f, 100);
                    }
                    f();
                }
            } else if (parseInt(hash) == 0) {
                window.location.hash = 0;
                keyId = 0;
                DirectoryId = 0;
                FormDataClear();
                MoveToContent();
            } else {
                MoveToContent();
                co.Directory.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        DirectoryId = result.id;
                        MoveToContent();
                        FormDataSet(result);
                    } else {
                        window.location.hash = ""
                        keyId = "";
                    }
                })
            }
        }
    } else {
        BackToList();
    }
}

function contentReady(e) {
    directory_list = e;
    HashDataEdit();
}
function DirectoryDatailListReady(e) {
    directoryDatailList = e;
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function reladataButtonClicked(e) {
    let type;
    switch (e.row.data.Type) {
        case "文章":
            type = "Articles";
            break;
        case "商品":
            type = "Products";
            break;
        case "選單":
            type = "Menus";
            break;
        default:
            BackToList();
            break;
    }
    if (type == "Articles") {
        keyId = `${type}_${e.row.key}`;
        window.location.hash = keyId;
    } else co.sweet.warn("尚未開放", "目前僅文章可編輯查看");
}
function GetDirectoryId() {
    return DirectoryId;
}
function GetDirectoryType() {
    return DirectoryType;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Directory.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
        e.component.refresh();
    });
}

function FormDataClear() {
    $DirectorytTags.TagDataClear();
    WebmenuDataClear();
    keyId = 0;
    $btn_display.children("span").text("visibility");
    $bind_type.val(null);
    $title_text.val("");
    $description_text.val("");
}
function ArticleDataClear() {
    $(".data_upload").each(function () {
        UploadPreviewFrameClear($(this));
    });
    $(".data_upload > ul > .upload_list").remove();
    total_files = [];
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    disp_opt = result.visible;
    if (disp_opt) {
        $btn_display.children("span").text("visibility");
    } else {
        $btn_display.children("span").text("visibility_off");
    }
    $bind_type.val(result.type);

    switch (parseInt($bind_type.val())) {
        case 1:
        case 2:
            $(DirectorytForms).find(".webmenu > input").attr("disabled", "disabled")
            $(DirectorytForms).find(".tag > input").removeAttr("disabled")
            $DirectorytTags.TagDataSet(result.tagDatas);
            WebmenuDataClear();
            break;
        case 3:
            $(DirectorytForms).find(".tag > input").attr("disabled", "disabled")
            $(DirectorytForms).find(".webmenu > input").removeAttr("disabled", "disabled")
            WebmenuDataSet(result.fK_MId);
            $DirectorytTags.TagDataClear();
            break;
    }
    $title_text.val(result.title);
    $description_text.val(result.description);
}

function AddUp(success_text, error_text) {
    var Fk_Mid = null;
    if (webmenu_list.length > 0 && !webmenu_list[webmenu_list.length - 1].IsDeleted) Fk_Mid = webmenu_list[webmenu_list.length - 1].FK_MId;

    co.Directory.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $description_text.val(),
        Type: parseInt($bind_type.val()),
        Visible: disp_opt,
        TagSelected: $DirectorytTags.data("tagList"),
        Fk_Mid: Fk_Mid
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        directory_list.component.refresh();
        BackToList();
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}
function AddUpArticlet(success_text, error_text) {
    const data = co.Form.getJson($(ArticletForms).attr("id"));
    if ($("#ImageUpload .img_input_frame").data("delectList") != null) {
        co.File.DeleteFileById({
            Sid: data.id,
            Type: 6,
            Fid: $("#ImageUpload .img_input_frame").data("delectList")
        });
    }
    co.Articles.AddUp(data).done((result) => {
        const success = function () {
            Coker.sweet.success(success_text, null, true);
            directoryDatailList.component.refresh();
            location.hash = `Articles_${DirectoryId}`;
        }

        if ($("#ImageUpload .img_input").data("file") != null && $("#ImageUpload .img_input").data("file").File != null && $("#ImageUpload .img_input").data("file").id == 0) {
            var formData = new FormData();
            formData.append("files", $("#ImageUpload .img_input").data("file").File);
            formData.append("type", 6);
            formData.append("sid", result.message);
            formData.append("serno", 500);
            co.File.Upload(formData).done(function () {
                success();
            });
        } else success();

        if (total_files.length > 0) {
            //console.log(total_files)
            $("#ArticleFiles > ul > li.upload_list").each(function () {
                var $self = $(this);
                var data = [];
                if (typeof ($self.data("id")) != "undefined") data = total_files.find(item => $self.data("id") == item.Id);
                else if (typeof ($self.data("tempid")) != "undefined") data = total_files.find(item => $self.data("tempid") == item.TempId);

                if (typeof (data["Id"]) == "undefined" || (!data["IsEncryption"] && $self.find(".btn_lock").hasClass("lock"))) {
                    var formData = new FormData();
                    formData.append("files", data["File"]);
                    formData.append("type", 15);
                    if (typeof (data["Id"]) != "undefined") formData.append("id", data["Id"]);
                    formData.append("sid", result.message);
                    formData.append("serno", $self.find(".ser_no").val());
                    formData.append("isEncryption", $self.find(".btn_lock").hasClass("lock"));
                    co.File.Upload(formData).done(function (result) {
                        console.log(result)
                    })
                } else if (data['SerNo'] != $self.find(".ser_no").val()) {
                    co.File.fileSortChange({
                        Id: data["Id"],
                        sid: result.message,
                        SerNo: $self.find(".ser_no").val(),
                    });
                }
            });

            //移除要移除的檔案
            total_files.forEach(file => {
                if ((typeof (file["IsDelete"]) != "undefined" && file["IsDelete"] == true && typeof (file["Id"]) != "undefined")) {
                    var deleteid_list = [];
                    deleteid_list.push(file["Id"]);
                    co.File.DeleteFileById({
                        Sid: parseInt(result.message),
                        Type: 15,
                        Fid: deleteid_list,
                    })
                }
            });
        }
    });
}

function MoveToContent() {
    $(DirectorytForms).removeClass("was-validated");
    if (!!keyId && isNaN(keyId)) {

    }

    if (keyId == 0) {
        $(".btn_to_canvas").addClass("text-dark");
        $(".btn_to_canvas").attr('disabled', '');
    } else {
        $(".btn_to_canvas").removeClass("text-dark");
        $(".btn_to_canvas").removeAttr('disabled');
    }
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryContent").removeClass("d-none");
}

function BackToList() {
    $("#pages>.card").addClass("d-none");
    $("#DirectoryList,#TopLine").removeClass("d-none");
    DirectoryId = 0;
    DirectoryType = "n";
    window.location.hash = ""
}

function MoveToItemList() {
    const para = window.location.hash.replace("#", "").split("_");
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryItemps").removeClass("d-none");
    const items = $(`#DirectoryItemps>.${para[0].toLowerCase()}`).removeClass("d-none");
    if (items.length == 0) BackToList();
    else if (para.length > 1 && !isNaN(para[1])) {
        DirectoryId = parseInt(para[1]);
        DirectoryType = para[0];
        switch (DirectoryType) {
            case "Articles":
                directoryDatailList.component.refresh();
                ArticleDataClear();
                break
            default:
                BackToList();
                break
        }

    }
}
function MoveToItemArticle() {
    const para = window.location.hash.replace("#", "").split("_");
    $("#pages>.card,#TopLine").addClass("d-none");
    $ArticletTags.TagDataClear();
    if (para.length > 2 && !isNaN(para[1]) && !isNaN(para[2])) {
        const id = parseInt(para[2]);
        DirectoryId = parseInt(para[1]);
        switch (para[0]) {
            case "ArticlesEditor":
                const _dfr = $.Deferred();
                co.Directory.Get(DirectoryId).done((result) => {
                    $("#DirectoryItemps").data("dir", result);
                    _dfr.resolve();
                });
                co.Form.clear("ArticletForm");
                if (id > 0) {
                    co.Articles.GetDataOne(id).done(function (result) {
                        if (result != null) {
                            ArticletId = result.id;
                            result.startEndDate = 0;
                            result.sortCheckbox = 1;
                            result.ImageUpload = 1;
                            co.Form.insertData(result, "#ArticletForm");
                            $ArticletTags.TagDataSet(result.tagDatas);

                            result.files.forEach(file => {
                                UploadListAdd(file, $("#ArticleFiles"));
                            })
                        } else BackToList();
                    })
                } else {
                    _dfr.promise().done(function () {
                        $ArticletTags.TagDataSet($("#DirectoryItemps").data("dir").tagDatas);
                    });
                }
                $("#ArticleContent").removeClass("d-none");
                break
            case "ArticlesEditorView":
                $("#DirectoryCanvas,#TopLine").removeClass("d-none");
                $("#gjs").data("id", id);
                setPage(id);
                break;
            default:
                BackToList();
                break
        }

    }
}
//設定html資料
setPage = function (id) {
    $("body").addClass("grapesEdit");
    co.Articles.GetConten({ Id: id }).done(function (result) {
        if (result.success) {
            var html = co.Data.HtmlDecode(result.conten.saveHtml);
            co.Grapes.setEditor(editor, html, result.conten.saveCss);
            $("#TopLine a").attr("href", `#Articles_${DirectoryId}`);
            if (!!result.title) $("#TopLine .title").text(result.title);
        } else {
            co.sweet.error(result.error);
        }
    });
}
function editArticlesButtonClicked(e) {
    window.location.hash = `ArticlesEditor_${DirectoryId}_${e.row.key}`;
}
function paletteArticlesButtonClicked(e) {
    window.location.hash = `ArticlesEditorView_${DirectoryId}_${e.row.key}`;
}
function groupArticlesButtonClicked(e) {
    $("#PermissionDetailsModal").setData({ pageId: e.row.key, title: e.row.data.Title, type: 3 }).modal("show");
}
function deleteArticlesButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Articles.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}

function UploadListAdd(result, $target) {
    var item = $($("#TemplateUploadList").html()).clone();
    var item_serno = item.find(".ser_no"),
        item_btn_remove = item.find(".btn_remove"),
        item_btn_lock = item.find(".btn_lock");
    var tempId = total_files.length;
    if (typeof (file_num) == "undefined") file_num = 0;
    var file_num = $target.find("ul > li.upload_list").length;

    if (result == null) {
        // 沒有上傳檔案的話執行此處內容
        // 新增新的欄位
        file_num += 1;

        $target.find("ul > li").each(function () {
            var $self = $(this);
            if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
                $self.remove();
                file_num -= 1;
            }
        })

        item.data("tempid", tempId);
        item.data("serno", file_num);
        item_serno.val(file_num);

        // 如果新增時沒有可選的檔案上傳類型且data裡面存有檔案上傳類型的話
        if ($target.find(".select_frame").length == 0 && typeof ($target.data("uploadtype")) != "undefined")
            // 直接指定該欄位為data裡面存的類型
            item.data("uploadtype", $target.data("uploadtype"));
        else
            // 否則先設置為0
            item.data("uploadtype", 0);

        item.data("edit", false);
        item.on("click", function () {
            co.File.ListFile($(this));
        })
    } else if (typeof (result.id) == "undefined") {
        // 此處為新上傳檔案執行的內容
        item.data("tempid", result.TempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.Type);
        item.data("edit", false);
        item.find(".title").text(result.Name);
    } else {
        // 此處為已有檔案帶入部分
        console.log(result)
        file_num += 1;
        item.data("id", result.id);
        item.data("serno", file_num);
        item.data("oldserno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.fileType);
        item.data("edit", false);
        item.find(".title").text(result.name);
        if (result.isEncryption) {
            item_btn_lock.addClass("lock");
            item_btn_lock.attr({
                title: "已上鎖檔案不可解鎖",
                "data-status": "locked"
            });
        }

        var obj = {};
        obj["Id"] = result.id;
        obj["Name"] = result.name;
        obj["SerNo"] = file_num;
        var link = result.link[0];
        if (result.fileType == 4) {
            obj["File"] = result.name;
        } else {
            obj["File"] = link;
        }
        obj["Type"] = result.fileType;
        obj["IsDelete"] = false;
        obj["IsEncryption"] = result.isEncryption;
        total_files.push(obj);

    }

    // 以下為檔案排序判斷與調整
    item_serno.on("blur", function () {
        var $self = $(this);
        var $uploadList = $target.find(".upload_list");
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $uploadList.length) {
            $self.val($uploadList.length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($uploadList, "bigger", item.data("serno"), $self.val())
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($uploadList, "smaller", $self.val(), item.data("serno"))
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })

    // 檔案是否上鎖的按鈕
    item_btn_lock.on('click', function (e) {
        e.preventDefault();
        var $self = $(this);
        if ($self.data("status") == "locked") co.sweet.warn("操作無效", "已上鎖檔案不可解鎖。");
        else $self.toggleClass('lock');
    });

    // 檔案移除
    item_btn_remove.on("click", function (e) {
        e.preventDefault();
        var $self = $(this).parents("li").first();
        var $uploadList = $target.find(".upload_list");
        var file_num = $target.find("ul > li.upload_list").length;
        // 將所有排序調整
        if (item.data("serno") < file_num) {
            SortChange($uploadList, "bigger", item.data("serno"), file_num);
        }
        // 如果是已存在資料庫的檔案
        if (typeof ($self.data("id")) != "undefined") {
            total_files.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
        } else if (typeof ($self.data("tempid")) != "undefined") {
            // 如果是剛上傳的檔案 要清掉現有暫存檔案裡面的
            var tempid = $self.data("tempid");
            var index = total_files.findIndex(item => item["TempId"] == tempid);
            if (index >= 0) {
                total_files.splice(index, 1);
                total_files.forEach(file => {
                    file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
                })
            }
        }

        UploadPreviewFrameClear($target);
        $self.remove();
    })

    $target.find("ul > .btn_upload_add").before(item);
    co.File.ListFile(item);
}
function SortChange($self, change, minindex, maxindex) {
    $self.each(function () {
        var $li_self = $(this)
        if (change == "bigger") {
            if ($li_self.data("serno") > minindex && $li_self.data("serno") <= maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) - 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        } else if (change == "smaller") {
            if ($li_self.data("serno") >= minindex && $li_self.data("serno") < maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) + 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        }
    })
}
function UploadPreviewFrameClear($target) {
    var $self = $target.find(".preview_frame");
    $self.find(".default_frame").addClass("d-flex");
    $self.find(".upload_frame").addClass("d-none");
    $self.find(".media_frame").removeClass("d-flex");
    $self.find(".youtube_frame").removeClass("d-flex");
    $self.find(".select_frame").removeClass("d-flex");
    $self.find(".youtube_preview").empty();
    $self.find(".media_preview > div").empty();
}
