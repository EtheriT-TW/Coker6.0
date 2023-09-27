var tag_check_list, tag_text, tag_changedBySelectBox, tag_clearSelectionButton;
var $tag;

$.fn.extend({
    TagListModalInit: function () {
        $tag = $(this).find();
    }
});
function TagList_ClearBtnInit(e) {
    tag_clearSelectionButton = e.component;
}

function TagDataClear() {
    tag_list = [];
    tag_check_list = [];
    tag_text = "";
    $tag.val(".InputTag");
    getTagListDataGridInstance().clearSelection();
}

function getTagListDataGridInstance() {
    return $("#TagList").dxDataGrid("instance");
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