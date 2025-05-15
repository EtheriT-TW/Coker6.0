$(function () {
    $("#submitButton").on("click", function () {
        var form = $("#bonusSettingsForm").dxForm("instance");
        var formData = form.option("formData");

        $.ajax({
            url: "/api/BonusManagement/Save",
            type: "POST",
            data: JSON.stringify(formData),
            contentType: "application/json",
            headers: co.Data.Header,
            success: function (response) {
                if (response.success) {
                    Coker.sweet.success(response.message, null, true);
                } else {
                    Coker.sweet.error("錯誤", response.message, null, true);
                }
            },
            error: function (req, status, error) {
                Coker.sweet.error("錯誤", "儲存失敗" + req.responseJSON.message, null, true);
            }
        });
    });
});
