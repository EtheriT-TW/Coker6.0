var GroupList_dxData, TagList_dxData;
function PageReady() {
    co.Tag = {
        TagDelete: function (id) {
            return $.ajax({
                url: "/api/Tag/TagDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        TagGroupDelete: function (id) {
            return $.ajax({
                url: "/api/Tag/TagGroupDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        }
    }
}

function contentReady(e) {
    if (e.element[0].id == "GroupList") {
        GroupList_dxData = $("#GroupList").dxDataGrid("instance");
    } else if (e.element[0].id == "TagList") {
        TagList_dxData = $("#TagList").dxDataGrid("instance");
    }
}
function groupSaved(e) {
    TagList_dxData.refresh();
}

function editButtonClicked(e) {
    var dataGrid = e.component;
    dataGrid.option('editing.editRowKey', e.row.key);
}

function editButtonVisible(e) {
    return !e.row.isEditing;
}

function cellTemplate(container, options) {
    var noBreakSpace = "\u00A0",
        text = (options.value || []).map(element => {
            return options.column.lookup.calculateCellValue(element);
        }).join(", ");
    container.text(text || noBreakSpace).attr("title", text);
}

function calculateFilterExpression(filterValue, selectedFilterOperation, target) {
    if (target === "search" && typeof (filterValue) === "string") {
        return [this.dataField, "contains", filterValue]
    }
    return function (data) {
        return (data.AssignedEmployee || []).indexOf(filterValue) !== -1
    }
}

function onSelectionChanged(selectionChangedArgs, component) {
    var setValue = component.option('setValue');
    var selectedRowKey = selectionChangedArgs.selectedRowKeys[0];

    component.option('value', selectedRowKey);
    setValue(selectedRowKey);
    if (selectionChangedArgs.selectedRowKeys.length > 0) {
        component.close();
    }
}

function dataSaving(e) {
    var first_char;
    if ((typeof (e.newData) != "undefined" && typeof (e.newData.Title) != "undefined") || (typeof (e.data) != "undefined" && typeof (e.data.Title) != "undefined")) {
        if (typeof (e.newData) != "undefined") first_char = e.newData.Title.substring(0, 1);
        else if (typeof (e.data) != "undefined") first_char = e.data.Title.substring(0, 1);
        var specialChars = "~·`!！@#$￥%^…&*()（）—-_=+[]{}【】、|\\;:；：'\"“‘,./<>《》?？，。";
        if (specialChars.indexOf(first_char) == -1) {
            e.component.saveEditData();
            GroupList_dxData.refresh();
            TagList_dxData.refresh();
        } else {
            e.cancel = true;
            co.sweet.error("資料錯誤", "標籤名稱不可以符號開頭", null, null);
        }
    } else {
        GroupList_dxData.refresh();
        TagList_dxData.refresh();
    }
}
function groupDeleteButtonClicked(e) {
    co.sweet.confirm("刪除類別", "確定刪除？類別刪除後不可復原", "確　定", "取　消", function () {
        co.Tag.TagGroupDelete(e.row.key).done(function () {
            GroupList_dxData.refresh();
            TagList_dxData.refresh();
        })
    })
}

function tagDeleteButtonClicked(e) {
    co.sweet.confirm("刪除規格", "確定刪除？標籤刪除後不可復原", "確　定", "取　消", function () {
        co.Tag.TagDelete(e.row.key).done(function () {
            GroupList_dxData.refresh();
            TagList_dxData.refresh();
        })
    })
}