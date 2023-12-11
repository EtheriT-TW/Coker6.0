var RecipientList_dxData;
function PageReady() {}

function contentReady(e) {
    RecipientList_dxData = $("#RecipientList").dxDataGrid("instance");
}

function editButtonClicked(e) {
    var daRecipientrid = e.component;
    daRecipientrid.option('editing.editRowKey', e.row.key);
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
            RecipientList_dxData.refresh();
        } else {
            e.cancel = true;
            co.sweet.error("資料錯誤", "標籤名稱不可以符號開頭", null, null);
        }
    } else {
        RecipientList_dxData.refresh();
    }
}

function RecipientDeleteButtonClicked(e) {
    co.sweet.confirm("刪除規格", "確定刪除？標籤刪除後不可復原", "確　定", "取　消", function () {
        co.Recipient.DeleteRecipients(e.row.key).done(function () {
            RecipientList_dxData.refresh();
        })
    })
}