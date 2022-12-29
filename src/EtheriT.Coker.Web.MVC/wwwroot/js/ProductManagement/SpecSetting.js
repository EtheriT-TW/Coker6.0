var $btn_display, $title, $illustrate, $illustrate_count, $input_sort, $check_sort, $date, $permanent
var startDate, endDate, keyId, disp_opt = true
var SpecSetting_list

function PageReady() {

    ElementInit();
}

function ElementInit() {

}

function addButtonClicked(e) {
    var dataGrid = e.component;
    var key = new DevExpress.data.Guid().toString();
    dataGrid.option('editing.changes', [{
        key,
        type: 'insert',
        insertAfterKey: e.row.key,
    }]);
    dataGrid.option('editing.editRowKey', key);
}

function addButtonVisible(e) {
    return !e.row.isEditing;
}

function newRowPositionChanged(e) {
    console.log("newRowPositionChanged");
    var dataGrid = $("#gridContainer").dxDataGrid("instance");
    dataGrid.option('editing.newRowPosition', e.value);
}

function scrollingModeChanged(e) {
    console.log("scrollingModeChanged");
    var dataGrid = $("#gridContainer").dxDataGrid("instance");
    dataGrid.option('scrolling.mode', e.value);
}

function onRowInserted(e) {
    console.log("onRowInserted");
    e.component.navigateToRow(e.key);
}