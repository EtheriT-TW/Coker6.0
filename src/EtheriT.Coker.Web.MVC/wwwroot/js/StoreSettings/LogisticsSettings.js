function PageReady() {
    const formId = "StoreSet";
    co.Form.init(formId, () => {
        const array = co.Object.objectToArray(co.Form.getJson(formId, true));
        const savaData = {
            PaymentType: null,
            ThirdParties: [],
            ServiceType: 2,
        };
        $("#ThirdParty>.accordion-item").each(function () {
            const $e = $(this);
            const Id = $e.data("groupid");
            var value = co.Object.objectToArray(co.Form.getJson(`thirdPartyForm_${Id}`, true));
            value = ValueCheck(value);
            savaData.ThirdParties.push({
                id: Id,
                value: value
            });
        });
        co.Product.ThirdParty.save(savaData).done(function (result) {
            if (result.success) co.sweet.success("儲存成功");
            else co.sweet.error("儲存失敗", result.message);
        });
        return false;
    });
}

function ValueCheck(values) {
    values.forEach((value) => {
        if (Array.isArray(value.value)) {
            if (value.value.length > 0) {
                value.value = String(value.value[0]);
            } else {
                value.value = "";
            }
        }
    });
    return values;
}