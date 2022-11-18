
function PageReady() {
    co.Marquees = {
        Delete: function (id) {
            return $.ajax({
                url: "/api/Marquee/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        }
    };
}

function addButtonClicked(e) {
    window.location.href = "/ContentManagement/Marquee?id=" + e.row.key;
}

function editButtonClicked(e) {
    window.location.href = "/ContentManagement/Marquee?id=" + e.row.key;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Marquees.Delete(e.row.key);
    });
}