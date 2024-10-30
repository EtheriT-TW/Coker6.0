function PageReady() {
    const formId = "StoreSet";
    co.Form.set(formId, () => {
        const array = co.Object.objectToArray(co.Form.getJson(formId, true));
        const PaymentType = array[co.Array.Search(array, { key: "paymentType" })];
        const savaData = {
            PaymentType: PaymentType != null ? PaymentType.value : null,
            ThirdParties: []
        };
        $("#ThirdParty>.accordion-item").each(function () {
            const $e = $(this);
            const Id = $e.data("groupid");
            savaData.ThirdParties.push({
                id: Id,
                value: co.Object.objectToArray(co.Form.getJsonByFieldset(`thirdPartyForm_${Id}`, true))
            });
        });
        co.Product.ThirdParty.save(savaData).done(function (result) {
            if (result.success) co.sweet.success("儲存成功");
            else co.sweet.error("儲存失敗", result.message);
        });
        return false;
    });
}