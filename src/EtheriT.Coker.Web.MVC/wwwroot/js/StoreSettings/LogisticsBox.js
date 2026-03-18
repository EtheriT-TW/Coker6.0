(function () {
    "use strict";
    let keyId = null;
    const edit = function (e) {
        MoveToContent();
        keyId = e.row.key;
        window.location.hash = keyId
    }
    const del = function (e) {
        Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
            co.LogisticsBox.Delete(e.row.key).done(function () {
                Coker.sweet.success("刪除成功", null, true);
                e.component.refresh();
            });
        });
    }
    const gridReady = function (e) {
        freight_list = e;
        HashDataEdit();
    }
    function hashChange(e) {
        if (!!e) {
            HashDataEdit();
            e.preventDefault();
        } else {
            console.log("HashChange錯誤")
        }
    }
    function HashDataEdit() {

    }
    function bindHashChange() {
        if ("onhashchange" in window) {
            window.onhashchange = hashChange;
        } else {
            setInterval(hashChange, 1000);
        }
    }

    const init = function () {
        bindHashChange();
    }
    window.PageReady = init;
    window.contentReady = gridReady;
    window.editButtonClicked = edit;
    window.deleteButtonClicked = del;
    
})();