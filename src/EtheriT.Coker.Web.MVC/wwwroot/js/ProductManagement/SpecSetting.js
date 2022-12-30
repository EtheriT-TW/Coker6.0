
function PageReady() {
    co.Spec = {
        Delete: function (id) {
            return $.ajax({
                url: "/api/Specification/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        }
    }
}

function editButtonClicked(e) {
    var dataGrid = e.component;
    dataGrid.option('editing.editRowKey', e.row.key);
}

function editButtonVisible(e) {
    return !e.row.isEditing;
}

function deleteButtonClicked(e) {
    co.sweet.confirm("刪除規格", "確定刪除？規格刪除後不可復原", "確　定", "取　消", function () {
        co.Spec.Delete(e.row.key).done(function () {
            e.component.refresh();
        })
    })
}