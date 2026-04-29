function PageReady() {
    const formId = "StoreSet";
    co.Form.init(formId, () => {
        var datas = co.Object.objectToArray(co.Form.getJson(formId, true));
        const payload = datas.map(x => ({
            key: x.key,
            value: Array.isArray(x.value)
                ? x.value
                : (x.value == null || x.value === "" ? [] : [x.value])
        }));

        co.StoreSet.SaveValues(payload).done(function (result) {
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