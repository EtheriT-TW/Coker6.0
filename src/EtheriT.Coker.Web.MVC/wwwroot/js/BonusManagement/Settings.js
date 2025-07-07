$(function () {
    $("#submitButton").on("click", function () {
        var form = $("#bonusSettingsForm").dxForm("instance");
        var formData = form.option("formData");

        $.ajax({
            url: "/api/BonusManagement/SaveSetting",
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

$(document).ready(function () {
    setTimeout(function () {
        var suspendValueChanged;

        // 為所有 NumberBox 增加事件處理
        // ref:https://supportcenter.devexpress.com/ticket/details/T741892/numberbox-how-to-prevent-only-the-mouse-wheel-event-and-keep-arrows-active
        $(".dx-numberbox").each(function () {
            var instance = $(this).dxNumberBox("instance");
            if (instance) {
                instance.option("onValueChanged", function (e) {
                    if (e.event && e.event.type === "dxmousewheel") {
                        if (suspendValueChanged) {
                            suspendValueChanged = false;
                            return;
                        }
                        suspendValueChanged = true;
                        e.component.option('value', e.previousValue);
                    }
                });
            }
        });
    }, 300); // 給予適當的延遲以確保控件已經初始化
});