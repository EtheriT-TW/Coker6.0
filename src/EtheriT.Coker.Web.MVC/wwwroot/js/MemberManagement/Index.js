function PageReady() {
    const formId = "MemberManagement";
    const $form = $(`#${formId}`);
    const groupId = $form.data("groupid");
    co.StoreSet.CreateFrom(formId).done(function () {
        co.Form.init(formId, () => {
            co.StoreSet.SaveValues(co.Object.objectToArray(co.Form.getJson(formId, true))).done(function (result) {
                if (result.success) co.sweet.success("儲存成功");
                else co.sweet.error("儲存失敗", result.message);
            });
            return false;
        });
    });
}