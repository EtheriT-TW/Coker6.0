var cssCompleter = {
    getCompletions: function (editor, session, position, prefix, callback) {
        var completions = [
            { value: "color", score: 1000, meta: "CSS" },
            { value: "background-color", score: 1000, meta: "CSS" },
            { value: "font-size", score: 1000, meta: "CSS" },
            // 更多標準的 CSS 屬性
            { value: "margin", score: 1000, meta: "CSS" },
            { value: "padding", score: 1000, meta: "CSS" },
            { value: "width", score: 1000, meta: "CSS" },
            // 可以加入更多 CSS 關鍵字
        ];
        callback(null, completions);
    }
};
window.PageReady = function () {
    const formId = "SettingCss";
    const editor = ace.edit("InputWebsiteCss");
    editor.setTheme("ace/theme/monokai");
    editor.session.setMode("ace/mode/css");
    editor.setOptions({
        enableBasicAutocompletion: true,   // 啟用基本的字詞自動補全
        enableLiveAutocompletion: true     // 啟用即時自動補全
    });
    var autocomplete = ace.require("ace/autocomplete").FilteredList;
    editor.completers = [cssCompleter]; 
    co.Form.set(formId, function () {
        co.sweet.confirm("是否確認儲存?", "即將將您設定的css發佈在前台所有網站上!", "確認", "取消", function () {
            var Errors = editor.getSession().getAnnotations();
            if (Errors.length > 0) co.sweet.error("失敗", co.sweet.TitleHilight(`字串包含不支援或無效的規則 {0}`, Errors[0].text));
            else {
                co.WebSite.SettingCss(editor.getValue()).done(function (result) {
                    if (result.success) co.sweet.success("儲存成功!");
                    else co.sweet.error("失敗", result.error)
                });
            }
        });
    });
    co.WebSite.LoadFrameCss().done(function (result) {
        editor.setValue(result.message, 1);
    });
}