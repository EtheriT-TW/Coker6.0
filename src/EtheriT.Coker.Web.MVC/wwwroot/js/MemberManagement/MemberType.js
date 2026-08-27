let dxData;
function PageReady() {
    
}

function contentReady(e) {
    dxData = e.component;
}

function memberRoleNameCellTemplate(container, options) {
    const name = options.data.Name || "";
    container.append($("<span>").text(name));

    if (options.data.IsDefault) {
        container.append(
            $("<span>")
                .addClass("badge rounded-pill bg-primary ms-2")
                .text("預設")
        );
    }
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
