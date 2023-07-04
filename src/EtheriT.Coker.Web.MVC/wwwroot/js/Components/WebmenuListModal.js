var $btn_webmenu_save, $webmenu
var webmenu_list = []
var webmenu_changedBySelectBox, webmenu_clearSelectionButton;
var webmenu_check_list = [], webmenu_text

function WebmenuListModalInit() {
    WebmenuListModalElementInit();

    webmenuModal = new bootstrap.Modal(document.getElementById('WebmenuModal'))
    $("#WebmenuModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        webmenu_list.forEach(function (item) {
            if (!item.IsDeleted) {
                temp_list.push(item.FK_MId);
            }
        })
        getWebmenuListDataGridInstance().selectRows(temp_list);
    })

    $btn_webmenu_save.on("click", function () {
        if (webmenu_check_list.length > 0) {
            webmenu_list.forEach(function (item) {
                var index = webmenu_check_list.indexOf(item.FK_MId)
                if (index > -1) {
                    item.IsDeleted = false;
                    webmenu_check_list.splice(index, 1)
                } else {
                    item.IsDeleted = true;
                }
            })
            if (webmenu_check_list.length > 0) {
                webmenu_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_MId"] = item;
                    obj["IsDeleted"] = false;
                    webmenu_list.push(obj);
                })
            }
        } else {
            webmenu_list.forEach(function (item) {
                item.IsDeleted = true;
            })
        }
        $webmenu.val(webmenu_text);
        webmenuModal.hide();
    })
}

function WebmenuListModalElementInit() {
    $webmenu = $("#InputWebmenu");
    $btn_webmenu_save = $(".btn_webmenu_save");
}

function getWebmenuListDataGridInstance() {
    return $("#WebmenuList").dxDataGrid("instance");
}

function WebmenuList_ClearBtnInit(e) {
    webmenu_clearSelectionButton = e.component;
}

function WebmenuList_ClearBtnClick() {
    if (webmenu_list.length > 0) {
        webmenu_list.forEach(function (item) {
            item.IsDeleted = true;
        })
    }
    getWebmenuListDataGridInstance().clearSelection();
}

function WebmenuList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    webmenu_check_list = [];
    if (data.length > 0) {
        webmenu_text = data.map((value) => value.Items != null ? `${value.Title}(${value.Items})` : `${value.Title}`).join("、");
        $webmenu.val(webmenu_text);
        data.forEach(function (item) {
            webmenu_check_list.push(item.Id)
        })
    } else {
        webmenu_text = "無";
        $webmenu.val(webmenu_text);
    }

    webmenu_changedBySelectBox = false;
    webmenu_clearSelectionButton.option('disabled', !data.length);
}

function WebmenuDataClear() {
    webmenu_list = [];
    webmenu_check_list = [];
    webmenu_text = "";
    $webmenu.val("")
    getWebmenuListDataGridInstance().clearSelection();
}

function WebmenuDataSet(data) {
    if (data != null) {
        var temp_list = [];
        var obj = {};
        obj["FK_MId"] = data;
        obj["IsDeleted"] = false;
        temp_list.push(data);
        webmenu_list.push(obj)
        getWebmenuListDataGridInstance().selectRows(temp_list);
        if ($webmenu.val() == "") $webmenu.val("載入中，請稍候...");
    }
}