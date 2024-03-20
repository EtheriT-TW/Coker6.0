var $btn_display, $bind_type, title, $title_text, $description, $description_text
var keyId, disp_opt = true, DirectoryId = 0, DirectoryType = "n";
let directory_list, editor, permissionDetailsModal;
let DirectorytForms, $DirectorytTags;
let ArticletForms, $ArticletTags;

function PageReady() {
    DirectorytForms = $('#DirectorytForm');
    ArticletForms = $('#ArticletForm');
    permissionDetailsModal = new bootstrap.Modal(document.getElementById("PermissionDetailsModal"));
    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.CanCreate) $(".btn_add").remove();
    });

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

    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })

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
                else MoveToItemList();
            } else if (parseInt(hash) == 0) {
                window.location.hash = 0;
                keyId = 0;
                FormDataClear();
                MoveToContent();
            } else {
                MoveToContent();
                co.Directory.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
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
    } else co.sweet.warn("尚未開放","目前僅文章可編輯查看");
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
        TagSelected: tag_list,
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
    });
}

function MoveToContent() {
    $(DirectorytForms).removeClass("was-validated");
    if (!!keyId && isNaN(keyId)) {

    } if (keyId == 0) {
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
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryList").removeClass("d-none");
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
                if (id > 0) {
                    co.Articles.GetDataOne(id).done(function (result) {
                        if (result != null) {
                            result.startEndDate = 0;
                            result.sortCheckbox = 1;
                            result.ImageUpload = 1;
                            co.Form.insertData(result, "#ArticletForm");
                            $ArticletTags.TagDataSet(result.tagDatas);
                        } else BackToList();
                    })
                } else {
                    co.Form.clear("ArticletForm");
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
    $("#PermissionDetailsModal").setData({ id: e.row.key, type: 3 }).modal("show");
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