let dxData;
function PageReady() {
    
}

function contentReady(e) {
    dxData = e.component;
}

function editButtonClicked(e) {
    var dataGrid = e.component;
    dataGrid.option('editing.editRowKey', e.row.key);
}

function deleteButtonClicked(e) {
    co.sweet.confirm("刪除角色", "確定刪除？角色刪除後不可復原", "確　定", "取　消", function () {
        console.log(co.Role);
        co.Role.Delete(e.row.key).done(function () {
            dxData.refresh();
        });
    })
}
