import { ArticleManager } from "./ArticleManager.js";
import { ArticleImport } from "./ArticleImport.js";
import "./DirectoryFacet.js";

var $btn_display, $bind_type, $title, $title_text, $description, $description_text;
var keyId = 0;
var disp_opt = true;
var DirectoryId = 0;
var DirectoryType = "n";
var directory_list;
var DirectorytForms;
var $DirectorytTags;
var $DirectoryReverseTags;
var articleOnly = false;
var hashPage;
var $templateBlock, $templateKey, $btnReapplyTemplate, $btnArticleImport;
// 目錄載入時已儲存的樣板，存檔時用來判斷有沒有換樣板。
var loadedTemplateKey = "";
const responsiveLists = new WeakSet();
const articleDateFields = new Set(["NodeDate", "EventStartDate", "EventEndDate", "LastModificationTime"]);

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
        $DirectoryReverseTags = DirectorytForms.find(".InputReverseTag").TagListModalInit();
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

    if (!articleOnly) {
        ArticleImport.init({
            navigate: navigate,
            backToList: BackToList,
            refreshDirectories: function () {
                if (directory_list) directory_list.component.refresh();
            }
        });
    }

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

    var importRoute = articleOnly ? null : ArticleImport.parseRoute(hash);
    if (importRoute) return importRoute;

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
    // 離開匯入頁時停止輪詢進度。
    if (route.scope === "import") {
        ArticleImport.enterRoute(route);
        return;
    }
    ArticleImport.leave();

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
    DirectorytForms.find(".reverse-tag > input").attr("disabled", "disabled");
    DirectorytForms.find(".webmenu > input").attr("disabled", "disabled");

    $templateBlock = DirectorytForms.find(".article-template");
    $templateKey = $templateBlock.find("#ArticleTemplateKey");
    $btnReapplyTemplate = $templateBlock.find(".btn-reapply-template");
    $btnArticleImport = $("#DirectoryContent .btn-article-import");
    LoadArticleTemplates();
}

function LoadArticleTemplates() {
    co.Directory.GetArticleTemplates().done(function (list) {
        (list || []).forEach(function (item) {
            $("<option>").val(item.key).text(item.name).attr("title", item.description).appendTo($templateKey);
        });
        // 清單比目錄資料晚回來時，補選目錄已儲存的樣板。
        if (loadedTemplateKey) $templateKey.val(loadedTemplateKey);
    });
}

// 只有文章目錄能選樣板；「重新套用」只在已存檔且有樣板時出現。
function ToggleArticleTemplate() {
    var isArticle = parseInt($bind_type.val()) === 2;
    $templateBlock.toggleClass("d-none", !isArticle);
    if (!isArticle) $templateKey.val("");
    $btnReapplyTemplate.toggleClass("d-none", !(isArticle && keyId > 0 && loadedTemplateKey));
    // 匯入資料只對已存檔的文章目錄開放。
    $btnArticleImport.toggleClass("d-none", !(isArticle && keyId > 0));
}

function EscapeHtml(text) {
    return $("<div>").text(text == null ? "" : String(text)).html();
}

function ErrorMessage(xhr) {
    return (xhr && xhr.responseJSON && xhr.responseJSON.message) || "發生未知錯誤，請稍後再試。";
}

function BuildApplyMessage(info) {
    var lines = [
        "是否將此目錄底下 <b>" + info.applicableCount + "</b> 篇文章都改為使用「" + EscapeHtml(info.templateName) + "」樣板？",
        "文章的草稿與發布內容都會被覆蓋，可在各文章的還原歷程找回。"
    ];
    if (info.noDataCount > 0) lines.push("另有 " + info.noDataCount + " 篇不是由資料匯入建立，不會套用。");
    if (info.noPermissionCount > 0) lines.push("另有 " + info.noPermissionCount + " 篇您沒有編輯權限，不會套用。");
    if (info.missingFieldLabels && info.missingFieldLabels.length > 0) {
        lines.push("新樣板沒有以下欄位，這些資料套用後不會顯示：" + info.missingFieldLabels.map(EscapeHtml).join("、"));
    }
    return lines.join("<br>");
}

// isManual：按「重新套用」進來的（沒有可套用文章時要提示）；存檔後詢問的則改顯示存檔成功。
function AskApplyArticleTemplate(directoryId, templateKey, fromTemplateKey, isManual) {
    var done = $.Deferred();
    co.Directory.GetArticleTemplateChangeInfo(directoryId, templateKey, fromTemplateKey).done(function (info) {
        if (!info || info.applicableCount === 0) {
            if (isManual) co.sweet.warn("沒有可套用的文章", "這個目錄沒有由資料匯入建立的文章。");
            else co.sweet.success("已成功儲存", null, true);
            done.resolve();
            return;
        }
        co.sweet.confirm("套用文章樣板", BuildApplyMessage(info), "全部套用", "先不要", function () {
            co.Directory.ApplyArticleTemplate(directoryId).done(function () {
                co.sweet.success("已開始在背景套用樣板，通常幾分鐘內完成。稍後可到文章的還原歷程確認內容是否已更新。");
            }).fail(function (xhr) {
                co.sweet.error("無法套用樣板", EscapeHtml(ErrorMessage(xhr)));
            }).always(function () {
                done.resolve();
            });
        }, function () {
            if (!isManual) co.sweet.success("已成功儲存（未套用樣板）", null, true);
            done.resolve();
        });
    }).fail(function (xhr) {
        co.sweet.error("無法取得樣板資訊", EscapeHtml(ErrorMessage(xhr)));
        done.resolve();
    });
    return done.promise();
}

function DirectoryEventsInit() {
    let saveForImport = false;

    $bind_type.on("change", function () {
        switch (parseInt($bind_type.val())) {
            case 1:
            case 2:
                DirectorytForms.find(".webmenu > input").attr("disabled", "disabled");
                DirectorytForms.find(".tag > input").removeAttr("disabled");
                DirectorytForms.find(".reverse-tag > input").removeAttr("disabled");
                WebmenuDataClear();
                break;
            case 3:
                DirectorytForms.find(".tag > input").attr("disabled", "disabled");
                DirectorytForms.find(".reverse-tag > input").attr("disabled", "disabled");
                DirectorytForms.find(".webmenu > input").removeAttr("disabled");
                $DirectorytTags.TagDataClear();
                $DirectoryReverseTags.TagDataClear();
                break;
        }
        ToggleArticleTemplate();
    });

    $btnArticleImport.on("click", function () {
        const form = DirectorytForms[0];
        if (form.dataset.submitting === "true") return;

        const selectedTemplateKey = $templateKey.val();
        if (!selectedTemplateKey) {
            co.sweet.warn("這個目錄還不能匯入", "請先選擇文章樣板，再點「匯入資料」。");
            return;
        }

        if (selectedTemplateKey !== loadedTemplateKey) {
            saveForImport = true;
            try {
                form.requestSubmit();
            } finally {
                saveForImport = false;
            }
            return;
        }

        var directoryId = keyId;
        Coker.sweet.confirm("前往匯入資料", "目錄尚未儲存的變更不會保存。", "前往匯入", "取消", function () {
            ArticleImport.open(directoryId);
        });
    });

    $btnReapplyTemplate.on("click", function () {
        if (($templateKey.val() || "") !== loadedTemplateKey) {
            co.sweet.warn("請先儲存目錄", "文章樣板已變更，儲存目錄時會詢問是否套用。");
            return;
        }
        AskApplyArticleTemplate(keyId, loadedTemplateKey, null, true);
    });

    co.Form.init("DirectorytForm", function () {
        const importAfterSave = saveForImport;
        const directoryId = keyId;
        return co.Form.confirmSubmit({
            title: importAfterSave ? "儲存並前往匯入" : "即將儲存",
            text: importAfterSave ? "將儲存目前的目錄設定（包含文章樣板），再前往匯入資料。" : "儲存後將顯示於安排的位置",
            confirmButtonText: importAfterSave ? "儲存並前往匯入" : "儲存",
            cancelButtonText: "取消",
            onConfirm: function () {
                const onSaved = importAfterSave ? function () { ArticleImport.open(directoryId); } : BackToList;
                return AddUp("已成功儲存", "儲存發生未知錯誤", onSaved);
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
    configureResponsiveContentList(e);
    if (hashPage) hashPage.refresh();
}

function articleListReady(e) {
    window.DirectoryDatailListReady(e);
    configureResponsiveContentList(e, true);
}

function configureResponsiveContentList(e, isArticleList = false) {
    const grid = e.component;
    if (responsiveLists.has(grid)) return;
    responsiveLists.add(grid);

    const element = e.element[0] || e.element;
    const body = element.closest(".content-list-body");
    const columns = grid.option("columns").map((column, index) => ({
        index,
        dataField: column.dataField,
        caption: column.caption,
        visible: column.visible !== false,
        width: column.width,
        minWidth: column.minWidth,
        isAction: column.type === "buttons"
    }));
    const details = columns.filter(column =>
        column.visible && column.dataField && !["Title", "Visible"].includes(column.dataField));
    const desktopVisibilityIndex = isArticleList ? grid.columnOption("Visible", "visibleIndex") : null;
    const compactVisibilityIndex = isArticleList ? grid.columnOption("Title", "visibleIndex") + 1 : null;
    let compact = false;

    const observer = new ResizeObserver(() => {
        const width = body.getBoundingClientRect().width;
        const isCompact = width <= 768;
        // 隱藏的列表會在重新顯示時收到新的寬度。
        if (!width || compact === isCompact) return;
        compact = isCompact;
        body.classList.toggle("content-list-compact", compact);

        grid.beginUpdate();
        try {
            grid.option({
                columnHidingEnabled: !compact,
                columnAutoWidth: !compact,
                "filterRow.visible": !compact,
                "headerFilter.visible": !compact,
                masterDetail: {
                    enabled: compact,
                    template: (container, options) => renderContentListDetails(container, options.data, details)
                }
            });

            if (isArticleList) {
                grid.columnOption("Visible", "visibleIndex", compact ? compactVisibilityIndex : desktopVisibilityIndex);
            }

            columns.forEach(column => {
                if (column.isAction) {
                    grid.columnOption(column.index, { width: compact ? 76 : column.width });
                } else if (column.dataField === "Visible") {
                    grid.columnOption(column.index, {
                        width: compact ? 56 : column.width,
                        caption: compact ? "顯示" : column.caption
                    });
                } else if (column.dataField === "Title") {
                    grid.columnOption(column.index, { minWidth: compact ? 0 : column.minWidth });
                } else if (column.visible) {
                    grid.columnOption(column.index, "visible", !compact);
                }
            });
        } finally {
            grid.endUpdate();
        }
    });
    observer.observe(body, { box: "border-box" });
    grid.on("disposing", function () {
        observer.disconnect();
        body.classList.remove("content-list-compact");
    });
}

function renderContentListDetails(container, data, columns) {
    const list = $("<dl class='content-list-details'></dl>");
    columns.forEach(column => {
        let value = data[column.dataField];
        if (typeof value === "boolean") {
            value = value ? "是" : "否";
        } else if (value && articleDateFields.has(column.dataField)) {
            const date = new Date(value);
            value = column.dataField === "LastModificationTime"
                ? date.toLocaleString("zh-TW")
                : date.toLocaleDateString("zh-TW");
        }
        list.append(
            $("<dt></dt>").text(column.caption),
            $("<dd></dd>").text(value === null || value === undefined || value === "" ? "－" : value)
        );
    });
    $(container).append(list);
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
    $DirectoryReverseTags.TagDataClear();

    WebmenuDataClear();

    keyId = 0;
    disp_opt = true;

    $btn_display.children("span").text("visibility");
    $bind_type.val(null);
    $title_text.val("");
    $description_text.val("");

    loadedTemplateKey = "";
    $templateKey.val("");
    ToggleArticleTemplate();
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
            DirectorytForms.find(".reverse-tag > input").removeAttr("disabled");

            $DirectorytTags.TagDataSet(result.tagDatas);
            $DirectoryReverseTags.TagDataSet(result.reverseTagDatas);

            WebmenuDataClear();
            break;
        case 3:
            DirectorytForms.find(".tag > input").attr("disabled", "disabled");
            DirectorytForms.find(".reverse-tag > input").attr("disabled", "disabled");

            DirectorytForms.find(".webmenu > input").removeAttr("disabled");

            WebmenuDataSet(result.fK_MId);

            $DirectorytTags.TagDataClear();
            $DirectoryReverseTags.TagDataClear();
            break;
    }

    $title_text.val(result.title);
    $description_text.val(result.description);

    loadedTemplateKey = result.articleTemplateKey || "";
    $templateKey.val(loadedTemplateKey);
    ToggleArticleTemplate();
}

function AddUp(successText, errorText, onSaved) {
    var menuId = null;
    if (webmenu_list.length > 0 && !webmenu_list[webmenu_list.length - 1].IsDeleted) {
        menuId = webmenu_list[webmenu_list.length - 1].FK_MId;
    }

    var type = parseInt($bind_type.val());
    var directoryId = keyId;
    var templateKey = type === 2 ? ($templateKey.val() || null) : null;
    var previousTemplateKey = loadedTemplateKey;

    return co.Directory.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $description_text.val(),
        Type: type,
        Visible: disp_opt,
        ArticleTemplateKey: templateKey,
        TagSelected: $DirectorytTags.data("tagList"),
        ReverseTagSelected: $DirectoryReverseTags.data("tagList"),
        Fk_Mid: menuId
    }).then(function (result) {
        if (result && result.success === false) {
            Coker.sweet.error("錯誤", EscapeHtml(result.error || errorText));
            return;
        }
        if (directory_list) directory_list.component.refresh();
        // 既有目錄換了樣板：詢問是否把目錄底下的文章都改用新樣板。
        if (directoryId > 0 && templateKey && templateKey !== previousTemplateKey) {
            return AskApplyArticleTemplate(directoryId, templateKey, previousTemplateKey, false).then(onSaved);
        }
        Coker.sweet.success(successText, null, true);
        onSaved();
    }, function () {
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
    articleListReady,
    editButtonClicked,
    reladataButtonClicked,
    GetDirectoryId,
    GetDirectoryType,
    groupArticlesButtonClicked,
    deleteButtonClicked
});

export { PageReady };
