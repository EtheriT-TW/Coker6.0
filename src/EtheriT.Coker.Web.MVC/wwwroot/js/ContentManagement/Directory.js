import { ArticleManager } from "./ArticleManager.js";
import "./DirectoryFacet.js";

var $btn_display, $bind_type, $title, $title_text, $description, $description_text;
var keyId = 0;
var disp_opt = true;
var DirectoryId = 0;
var DirectoryType = "n";
var directory_list;
var DirectorytForms;
var $DirectorytTags;
var articleOnly = false;

function PageReady() {
    articleOnly = $("#ContentManagementWorkspace").data("article-only") === true;
    DirectorytForms = $("#DirectorytForm");

    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.CanCreate) $(".btn_add").remove();
    });

    if (!articleOnly) {
        DirectoryElementInit();
        WebmenuListModalInit();
        $DirectorytTags = DirectorytForms.find(".InputTag").TagListModalInit();
        DirectoryEventsInit();
    }

    ArticleManager.init({
        articleOnly: articleOnly,
        getDirectoryId: function () { return DirectoryId; },
        setDirectoryContext: function (id, type) {
            DirectoryId = Number(id || 0);
            DirectoryType = type || "Articles";
        },
        backToDirectoryList: BackToList
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(HashDataEdit, 1000);
    }

    HashDataEdit();
}

function DirectoryElementInit() {
    $btn_display = DirectorytForms.find(".btn_display");
    $bind_type = DirectorytForms.find("#BindType");
    $title = DirectorytForms.find(".title");
    $title_text = $title.children("textarea");
    $description = DirectorytForms.find(".description");
    $description_text = $description.children("textarea");

    DirectorytForms.find(".tag > input").attr("disabled", "disabled");
    DirectorytForms.find(".webmenu > input").attr("disabled", "disabled");
}

function DirectoryEventsInit() {
    $bind_type.on("change", function () {
        switch (parseInt($bind_type.val())) {
            case 1:
            case 2:
                DirectorytForms.find(".webmenu > input").attr("disabled", "disabled");
                DirectorytForms.find(".tag > input").removeAttr("disabled");
                WebmenuDataClear();
                break;
            case 3:
                DirectorytForms.find(".tag > input").attr("disabled", "disabled");
                DirectorytForms.find(".webmenu > input").removeAttr("disabled");
                $DirectorytTags.TagDataClear();
                break;
        }
    });

    Array.from(DirectorytForms).forEach(function (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();
            if (!form.checkValidity()) {
                event.stopPropagation();
            } else {
                Coker.sweet.confirm(
                    "即將儲存",
                    "儲存後將顯示於安排的位置",
                    "儲存",
                    "取消",
                    function () { AddUp("已成功儲存", "儲存發生未知錯誤"); }
                );
            }
            form.classList.add("was-validated");
        }, false);
    });

    $("#DirectoryContent .btn_back").on("click", function () {
        Coker.sweet.confirm("返回目錄列表", "資料將不被保存", "確定", "取消", function () {
            if (directory_list) directory_list.component.refresh();
            BackToList();
        });
    });

    $("#DirectoryList .btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = "0";
    });

    $(document).on("click", ".btn-open-facet", function (event) {
        event.preventDefault();
        co.DirectoryFacetModal.open(DirectoryId);
    });

    $btn_display.on("click", function () {
        disp_opt = !disp_opt;
        $btn_display.children("span").text(disp_opt ? "visibility" : "visibility_off");
    });

    $title_text.on("keyup", function () {
        $title.find(".count").text($(this).val().length);
    });

    $description_text.on("keyup", function () {
        $description.find(".count").text($(this).val().length);
    });
}

function hashChange(event) {
    HashDataEdit();
    if (event) event.preventDefault();
}

function HashDataEdit() {
    var hash = window.location.hash.replace("#", "");

    if (ArticleManager.normalizeLegacyHash(hash)) return;

    if (ArticleManager.canHandleHash(hash)) {
        ArticleManager.handleHash(hash);
        return;
    }

    if (articleOnly) {
        window.location.hash = "Articles_0";
        return;
    }

    if (hash === "") {
        BackToList();
        return;
    }

    if (parseInt(hash) === 0) {
        keyId = 0;
        DirectoryId = 0;
        FormDataClear();
        MoveToContent();
        return;
    }

    if (!isNaN(hash)) {
        MoveToContent();
        co.Directory.Get(parseInt(hash)).done(function (result) {
            if (result != null) {
                DirectoryId = result.id;
                FormDataSet(result);
            } else {
                window.location.hash = "";
            }
        });
    }
}

function contentReady(e) {
    directory_list = e;
    HashDataEdit();
}

function editButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = String(keyId);
}

function reladataButtonClicked(e) {
    var type;
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
            return;
    }

    if (type === "Articles") {
        window.location.hash = type + "_" + e.row.key;
    } else {
        co.sweet.warn("尚未開放", "目前僅文章可編輯查看");
    }
}

function GetDirectoryId() {
    return DirectoryId;
}

function GetDirectoryType() {
    return DirectoryType;
}

function groupArticlesButtonClicked(e) {
    $("#PermissionDetailsModal")
        .setData({ pageId: e.row.key, title: e.row.data.Title, type: 3 })
        .modal("show");
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Directory.Delete(e.row.key).done(function (result) {
            if (result.success) e.component.refresh();
        });
    });
}

function FormDataClear() {
    $DirectorytTags.TagDataClear();
    WebmenuDataClear();
    keyId = 0;
    disp_opt = true;
    $btn_display.children("span").text("visibility");
    $bind_type.val(null);
    $title_text.val("");
    $description_text.val("");
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    disp_opt = result.visible;
    $btn_display.children("span").text(disp_opt ? "visibility" : "visibility_off");
    $bind_type.val(result.type);

    switch (parseInt($bind_type.val())) {
        case 1:
        case 2:
            DirectorytForms.find(".webmenu > input").attr("disabled", "disabled");
            DirectorytForms.find(".tag > input").removeAttr("disabled");
            $DirectorytTags.TagDataSet(result.tagDatas);
            WebmenuDataClear();
            break;
        case 3:
            DirectorytForms.find(".tag > input").attr("disabled", "disabled");
            DirectorytForms.find(".webmenu > input").removeAttr("disabled");
            WebmenuDataSet(result.fK_MId);
            $DirectorytTags.TagDataClear();
            break;
    }

    $title_text.val(result.title);
    $description_text.val(result.description);
}

function AddUp(successText, errorText) {
    var menuId = null;
    if (webmenu_list.length > 0 && !webmenu_list[webmenu_list.length - 1].IsDeleted) {
        menuId = webmenu_list[webmenu_list.length - 1].FK_MId;
    }

    co.Directory.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $description_text.val(),
        Type: parseInt($bind_type.val()),
        Visible: disp_opt,
        TagSelected: $DirectorytTags.data("tagList"),
        Fk_Mid: menuId
    }).done(function () {
        Coker.sweet.success(successText, null, true);
        if (directory_list) directory_list.component.refresh();
        BackToList();
    }).fail(function () {
        Coker.sweet.error("錯誤", errorText, null, true);
    });
}

function MoveToContent() {
    DirectorytForms.removeClass("was-validated");
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryContent").removeClass("d-none");
}

function BackToList() {
    if (articleOnly) {
        window.location.hash = "Articles_0";
        return;
    }

    $("#pages>.card").addClass("d-none");
    $("#DirectoryList,#TopLine").removeClass("d-none");
    DirectoryId = 0;
    DirectoryType = "n";
    if (window.location.hash !== "") window.location.hash = "";
}

window.ContentManagementDirectoryPageReady = PageReady;
window.PageReady = PageReady;

// DevExtreme 的 Razor 設定仍以函式名稱呼叫；僅將必要的介面公開到 window。
Object.assign(window, {
    contentReady,
    editButtonClicked,
    reladataButtonClicked,
    GetDirectoryId,
    GetDirectoryType,
    groupArticlesButtonClicked,
    deleteButtonClicked
});

export { PageReady };
