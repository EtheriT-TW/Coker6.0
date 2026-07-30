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
var hashPage;

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
        backToDirectoryList: BackToList,
        navigate: navigate
    });

    hashPage = Coker.HashPage.create({
        root: "#ContentManagementWorkspace",
        defaultHash: articleOnly ? "Articles_0" : "List",
        listHash: "List",
        newHash: "0",
        listPageKey: "DirectoryList",
        contentPageKey: "DirectoryContent",
        useStack: true,
        parseState: parseRoute,
        onChange: enterRoute
    });
}

function navigate(hash) {
    if (hashPage) {
        hashPage.setHash(hash);
        return;
    }

    window.location.hash = hash;
}

function parseRoute(hash) {
    var articleRoute = ArticleManager.parseRoute(hash);
    if (articleRoute) return articleRoute;

    var value = String(hash || "").trim();
    if (!value || value.toLowerCase() === "list") {
        return {
            raw: "List",
            mode: "directory-list",
            pageKey: "DirectoryList",
            title: "目錄管理"
        };
    }

    if (value === "0" || value.toLowerCase() === "new") {
        return {
            raw: value,
            mode: "directory-new",
            pageKey: "DirectoryContent",
            title: "新增目錄",
            directoryId: 0
        };
    }

    if (/^\d+$/.test(value)) {
        return {
            raw: value,
            mode: "directory-edit",
            pageKey: "DirectoryContent",
            title: "編輯目錄",
            directoryId: parseInt(value, 10)
        };
    }

    return {
        raw: articleOnly ? "Articles_0" : "List",
        mode: articleOnly ? "article-list" : "directory-list",
        pageKey: articleOnly ? "ArticleList" : "DirectoryList",
        title: articleOnly ? "文章管理" : "目錄管理",
        scope: articleOnly ? "article" : "directory",
        directoryId: 0,
        articleId: 0
    };
}

function enterRoute(route) {
    if (route.scope === "article") {
        ArticleManager.enterRoute(route);
        return;
    }

    switch (route.mode) {
        case "directory-new":
            keyId = 0;
            DirectoryId = 0;
            FormDataClear();
            MoveToContent();
            break;
        case "directory-edit":
            MoveToContent();
            co.Directory.Get(route.directoryId).done(function (result) {
                if (result != null) {
                    DirectoryId = result.id;
                    FormDataSet(result);
                } else {
                    BackToList();
                }
            });
            break;
        case "directory-list":
        default:
            ShowDirectoryList();
            break;
    }
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

    co.Form.init("DirectorytForm", function () {
        return co.Form.confirmSubmit({
            title: "即將儲存",
            text: "儲存後將顯示於安排的位置",
            confirmButtonText: "儲存",
            cancelButtonText: "取消",
            onConfirm: function () {
                return AddUp("已成功儲存", "儲存發生未知錯誤");
            }
        });
    });

    $("#DirectoryContent .btn_back").on("click", function () {
        Coker.sweet.confirm("返回目錄列表", "資料將不被保存", "確定", "取消", function () {
            if (directory_list) directory_list.component.refresh();
            BackToList();
        });
    });

    $("#DirectoryList .btn_add").on("click", function () {
        FormDataClear();
        navigate("0");
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

function contentReady(e) {
    directory_list = e;
    if (hashPage) hashPage.refresh();
}

function editButtonClicked(e) {
    keyId = e.row.key;
    navigate(String(keyId));
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
        navigate(type + "_" + e.row.key);
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
    co.Form.clear("DirectorytForm");
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

    return co.Directory.AddUp({
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
        navigate("Articles_0");
        return;
    }

    if (hashPage && hashPage.getHash().toLowerCase() !== "list") {
        hashPage.goList();
        return;
    }

    ShowDirectoryList();
}

function ShowDirectoryList() {
    $("#pages>.card").addClass("d-none");
    $("#DirectoryList,#TopLine").removeClass("d-none");
    DirectoryId = 0;
    DirectoryType = "n";
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
