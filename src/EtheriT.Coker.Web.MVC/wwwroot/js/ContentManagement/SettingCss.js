function PageReady() {
    const formId = "SettingCss";
    co.Form.set(formId, function () {
        const data = co.Form.getJson(formId);
        co.sweet.confirm("是否確認儲存?", "即將將您設定的css發佈在前台所有網站上!", "確認", "取消", function () {
            co.WebSite.SettingCss(data.Css).done(function (result) {
                if (result.success) co.sweet.success("儲存成功!");
                else co.sweet.error("失敗", result.error)
            });
        });
    });
    co.WebSite.LoadFrameCss().done(function (result) {
        co.Form.insertData({ Css: result.message });
    });
}