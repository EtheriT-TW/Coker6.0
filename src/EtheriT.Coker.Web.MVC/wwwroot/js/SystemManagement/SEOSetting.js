function PageReady() {
    co.StoreSet.GetValues({ StoreSetGroupId: 1 }).done(function (result) {
        if (result.success)
            co.Form.insertData(co.Object.arrayToObject(result.storeSetDetails));
    });
    $(".submit").on("click", function () {
        co.StoreSet.SaveValues(co.Object.objectToArray(co.Form.getJson("SEOSet"))).done(function (result) {
            if (result.success) co.sweet.success("儲存成功");
            else co.sweet.error("儲存失敗", result.message);
            console.log(result);
        });
    });
}