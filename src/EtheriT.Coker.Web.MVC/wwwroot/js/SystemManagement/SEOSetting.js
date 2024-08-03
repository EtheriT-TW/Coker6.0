function PageReady() {
    const formId = "StoreSet";
    co.Form.set(formId, () => {
        co.StoreSet.SaveValues(co.Object.objectToArray(co.Form.getJson(formId, true))).done(function (result) {
            if (result.success) co.sweet.success("儲存成功");
            else co.sweet.error("儲存失敗", result.message);
        });
        return false;
    });
    co.StoreSet.GetValues({ StoreSetGroupId: $(`#${formId}`).data("groupid") }).done(function (result) {
        if (result.success)
            co.Form.insertData(co.Object.arrayToObject(result.storeSetDetails));
    });
}