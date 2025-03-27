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

function dataSaving(e) {
    var first_char;
    if (typeof (e.newData) != "undefined") {
        if (typeof (e.newData.Title) != "undefined") first_char = e.newData.Title.substring(0, 1);
        else if (typeof (e.newData.Type) != "undefined") first_char = e.newData.Type.substring(0, 1);
    }
    else if (typeof (e.data) != "undefined") {
        if (typeof (e.data.Title) != "undefined") first_char = e.data.Title.substring(0, 1);
        else if (typeof (e.data.Type) != "undefined") first_char = e.data.Type.substring(0, 1);
    }

    var specialChars = "~·`!！@#$￥%^…&*()（）—-_=+[]{}【】、|\\;:；：'\"“‘,./<>《》?？，。";
    if (specialChars.indexOf(first_char) == -1) {
        e.component.saveEditData();
        TypeList_dxData.refresh();
        SpecList_dxData.refresh();
    } else {
        e.cancel = true;
        co.sweet.error("資料錯誤", "標籤名稱不可以符號開頭", null, null);
    }
}

function editButtonClicked(e) {
    var dataGrid = e.component;
    dataGrid.option('editing.editRowKey', e.row.key);
}

function editButtonVisible(e) {
    return !e.row.isEditing;
}

function typeDeleteButtonClicked(e) {
    co.Spec.CheckRelatSpec(e.row.key).done(function (result) {
        if (result.error != null) co.sweet.error("發生錯誤", result.error, null, false);
        else if (result.success) {
            co.sweet.confirm("刪除規格類型", "確定刪除？規格類型刪除後不可復原", "確　定", "取　消", function () {
                co.Spec.SpecDelect(e.row.key).done(function () {
                    TypeList_dxData.refresh();
                    SpecList_dxData.refresh();
                })
            })
        } else {
            var message = result.message.split(",");
            var wraning_text = `已有規格綁定此類型：【${message[0]}】`
            if (message[1] != "") wraning_text += `以及其他共${message[1]}個`
            co.sweet.error("規格類型已綁定", wraning_text, null, false);
        }
    });
}

function specDeleteButtonClicked(e) {
    co.Spec.CheckRelatProd(e.row.key).done(function (result) {
        if (result.error != null) co.sweet.error("發生錯誤", result.error, null, false);
        else if (result.success) {
            co.sweet.confirm("刪除規格", "確定刪除？規格刪除後不可復原", "確　定", "取　消", function () {
                co.Spec.SpecDelect(e.row.key).done(function () {
                    TypeList_dxData.refresh();
                    SpecList_dxData.refresh();
                })
            })
        } else {
            var message = result.message.split(",");
            var wraning_text = `已有商品綁定此規格：【${message[1]}(商品編號${message[0]})】`
            if (message[2] > 0) wraning_text += `以及其他共${message[2]}個`
            co.sweet.error("規格已綁定", wraning_text, null, false);
        }
    });
}

function getType(options) {
    return {
        store: DevExpress.data.AspNet.createStore({
            key: "Id",
            loadUrl: '/api/Specification/GetAllTypeList'
        })
    };
}