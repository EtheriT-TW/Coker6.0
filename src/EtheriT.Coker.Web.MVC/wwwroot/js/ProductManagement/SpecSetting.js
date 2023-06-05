var TypeList_dxData, SpecList_dxData;

function PageReady() {
}

function contentReady(e) {
    if (e.element[0].id == "TypeList") {
        TypeList_dxData = $("#TypeList").dxDataGrid("instance");
    } else if (e.element[0].id == "SpecList") {
        SpecList_dxData = $("#SpecList").dxDataGrid("instance");
    }
}

function typeSaved(e) {
    SpecList_dxData.refresh();
}

function editButtonClicked(e) {
    var dataGrid = e.component;
    dataGrid.option('editing.editRowKey', e.row.key);
}

function editButtonVisible(e) {
    return !e.row.isEditing;
}

function typeDeleteButtonClicked(e) {
    co.sweet.confirm("刪除規格", "規格類型刪除後將一併清除該類型下的所有規格名稱以及相關商品的規格", "確　定", "取　消", function () {
        co.Spec.TypeDelect(e.row.key).done(function () {
            TypeList_dxData.refresh();
            SpecList_dxData.refresh();
        })
    })
}

function specDeleteButtonClicked(e) {
    co.sweet.confirm("刪除規格", "確定刪除？規格刪除後不可復原", "確　定", "取　消", function () {
        co.Spec.SpecDelect(e.row.key).done(function () {
            TypeList_dxData.refresh();
            SpecList_dxData.refresh();
        })
    })
}

function getType(options) {
    return {
        store: DevExpress.data.AspNet.createStore({
            key: "Id",
            loadUrl: '/api/Specification/GetAllTypeList'
        })
    };
}