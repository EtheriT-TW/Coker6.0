var $btn_tag_save, $tag
var tag_list = []
var $price_modal, priceModal, $techcert_body, techcertModal;
var tag_changedBySelectBox, tag_clearSelectionButton;
var tag_check_list = [], tag_text

function TagListModalInit() {
    TagListModalElementInit();

    tagModal = new bootstrap.Modal(document.getElementById('TagModal'))
    $("#TagModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        tag_list.forEach(function (item) {
            if (!item.IsDeleted) {
                temp_list.push(item.FK_TId);
            }
        })
        getTagListDataGridInstance().selectRows(temp_list);
    })

    $btn_tag_save.on("click", function () {
        if (tag_check_list.length > 0) {
            tag_list.forEach(function (item) {
                var index = tag_check_list.indexOf(item.FK_TId)
                if (index > -1) {
                    item.IsDeleted = false;
                    tag_check_list.splice(index, 1)
                } else {
                    item.IsDeleted = true;
                }
            })
            if (tag_check_list.length > 0) {
                tag_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_TId"] = item;
                    obj["IsDeleted"] = false;
                    tag_list.push(obj);
                })
            }
        } else {
            tag_list.forEach(function (item) {
                item.IsDeleted = true;
            })
        }
        $tag.val(tag_text);
        tagModal.hide();
    })
}

function TagListModalElementInit() {
    $tag = $("#InputTag");
    $btn_tag_save = $(".btn_tag_save");
}

function getTagListDataGridInstance() {
    return $("#TagList").dxDataGrid("instance");
}

function TagList_ClearBtnInit(e) {
    tag_clearSelectionButton = e.component;
}

function TagList_ClearBtnClick() {
    if (tag_list.length > 0) {
        tag_list.forEach(function (item) {
            item.IsDeleted = true;
        })
    }
    getTagListDataGridInstance().clearSelection();
}

function TagList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    tag_check_list = [];
    if (data.length > 0) {
        tag_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            tag_check_list.push(item.Id)
        })
    } else {
        tag_text = "無";
    }

    tag_changedBySelectBox = false;
    tag_clearSelectionButton.option('disabled', !data.length);
}

function TagDataClear() {
    tag_list = [];
    tag_check_list = [];
    tag_text = "";
    $tag.val("")
    getTagListDataGridInstance().clearSelection();
}

function TagDataSet(datas) {
    var text = ""
    if (datas != null && datas.length > 0) {
        var temp_list = [];
        datas.forEach(function (data) {
            var obj = {};
            obj["Id"] = data.id;
            obj["FK_TId"] = data.fK_TId;
            obj["IsDeleted"] = false;
            temp_list.push(data.fK_TId);
            text = text == "" ? data.tag_Name : text + "、" + data.tag_Name;
            tag_list.push(obj)
        })
        getTagListDataGridInstance().selectRows(temp_list);
    }
    $tag.val(text == "" ? "無" : text);
}
function TagInitSet(datas) {
    tag_check_list = [];
    if (datas.length > 0) {
        tag_text = datas.map((value) => `${value.title}`).join("、");
        datas.forEach(function (item) {
            tag_check_list.push(item.id)
        })
    } else {
        tag_text = "無";
    }

    tag_changedBySelectBox = false;
    tag_clearSelectionButton.option('disabled', !datas.length);
    $btn_tag_save.trigger("click");
}