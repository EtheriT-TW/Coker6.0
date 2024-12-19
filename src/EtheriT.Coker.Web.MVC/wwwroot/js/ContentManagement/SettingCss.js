var cssCompleter = {
    getCompletions: function (editor, session, position, prefix, callback) {
        // 獲取當前行的內容
        var cursor = editor.getCursorPosition();
        var line = session.getLine(cursor.row);

        // 預設：CSS Property 建議
        var propertySuggestions = [
            { value: "color", score: 1000, meta: "CSS Property" },
            { value: "background", score: 990, meta: "CSS Property" },
            { value: "background-color", score: 980, meta: "CSS Property" },
            { value: "border", score: 970, meta: "CSS Property" },
            { value: "border-width", score: 960, meta: "CSS Property" },
            { value: "width", score: 950, meta: "CSS Property" },
            { value: "height", score: 940, meta: "CSS Property" },
            { value: "margin", score: 910, meta: "CSS Property" },
            { value: "padding", score: 900, meta: "CSS Property" },
            { value: "display", score: 890, meta: "CSS Property" },
            { value: "position", score: 880, meta: "CSS Property" },
            { value: "justify-content", score: 920, meta: "CSS Property" },
            { value: "align-items", score: 910, meta: "CSS Property" },
            { value: "align-self", score: 900, meta: "CSS Property" },
            { value: "flex-direction", score: 890, meta: "CSS Property" },
            { value: "flex-wrap", score: 880, meta: "CSS Property" },
            // 文字相關
            { value: "font-size", score: 930, meta: "CSS Property" },
            { value: "font-weight", score: 920, meta: "CSS Property" },
            { value: "text-align", score: 950, meta: "CSS Property" },
            { value: "line-height", score: 940, meta: "CSS Property" },

            // 鼠標顯示
            { value: "cursor", score: 930, meta: "CSS Property" },

            // 列表樣式
            { value: "list-style", score: 920, meta: "CSS Property" },

            // 事件反應
            { value: "pointer-events", score: 910, meta: "CSS Property" },

            // 物件樣式
            { value: "object-fit", score: 900, meta: "CSS Property" },
            { value: "object-position", score: 890, meta: "CSS Property" }
        ];

        // 預設：全域 CSS Value 建議
        var globalValueSuggestions = [
            { value: "inherit", score: 1000, meta: "CSS Value" },
            { value: "initial", score: 900, meta: "CSS Value" },
            { value: "unset", score: 800, meta: "CSS Value" }
        ];

        // 嘗試檢測光標前是否已輸入 CSS Property
        var match = line.substring(0, cursor.column).match(/([\w-]+)\s*:\s*/);
        var property = match ? match[1] : null;

        // 如果尚未輸入屬性名稱，回傳屬性建議
        if (!property) {
            callback(null, propertySuggestions);
            return;
        }

        // 根據 Property 提供特定 Value 建議
        var valueSuggestions = [];
        if (property === "color" || property === "background-color") {
            valueSuggestions = [
                { value: "red", score: 1000, meta: "CSS Value" },
                { value: "blue", score: 900, meta: "CSS Value" },
                { value: "green", score: 800, meta: "CSS Value" },
                { value: "black", score: 700, meta: "CSS Value" },
                { value: "white", score: 600, meta: "CSS Value" }
            ];
        } else if (property === "font-size") {
            valueSuggestions = [
                { value: "1rem", score: 1000, meta: "CSS Value" },
                { value: "1.1rem", score: 900, meta: "CSS Value" },
                { value: "1.2rem", score: 800, meta: "CSS Value" },
                { value: "1.6rem", score: 700, meta: "CSS Value" },
                { value: "2rem", score: 600, meta: "CSS Value" },
                { value: "2.5rem", score: 500, meta: "CSS Value" },
                { value: "3rem", score: 400, meta: "CSS Value" }
            ];
        } else if (property === "font-weight") {
            valueSuggestions = [
                { value: "normal", score: 1000, meta: "CSS Value" },
                { value: "bold", score: 900, meta: "CSS Value" },
                { value: "bolder", score: 800, meta: "CSS Value" },
                { value: "lighter", score: 700, meta: "CSS Value" },
                { value: "100", score: 600, meta: "CSS Value" },
                { value: "200", score: 500, meta: "CSS Value" },
                { value: "300", score: 400, meta: "CSS Value" },
                { value: "400", score: 300, meta: "CSS Value" },
                { value: "500", score: 200, meta: "CSS Value" },
                { value: "600", score: 100, meta: "CSS Value" }
            ];
        } else if (property === "position") {
            valueSuggestions = [
                { value: "static", score: 1000, meta: "CSS Value" },
                { value: "relative", score: 900, meta: "CSS Value" },
                { value: "absolute", score: 800, meta: "CSS Value" },
                { value: "fixed", score: 700, meta: "CSS Value" },
                { value: "sticky", score: 600, meta: "CSS Value" }
            ];
        } else if (property === "background") {
            valueSuggestions = [
                { value: "url('image.png')", score: 1000, meta: "CSS Value" },
                { value: "linear-gradient(red, blue)", score: 900, meta: "CSS Value" },
                { value: "none", score: 800, meta: "CSS Value" }
            ];
        } else if (property === "border") {
            valueSuggestions = [
                { value: "1px solid black", score: 1000, meta: "CSS Value" },
                { value: "2px dashed red", score: 900, meta: "CSS Value" },
                { value: "3px dotted blue", score: 800, meta: "CSS Value" }
            ];
        } else if (property === "display") {
            valueSuggestions = [
                { value: "block", score: 1000, meta: "CSS Value" },
                { value: "inline", score: 900, meta: "CSS Value" },
                { value: "flex", score: 800, meta: "CSS Value" }
            ];
        } else if (property === "justify-content") {
            valueSuggestions = [
                { value: "flex-start", score: 1000, meta: "CSS Value" },
                { value: "flex-end", score: 900, meta: "CSS Value" },
                { value: "center", score: 800, meta: "CSS Value" },
                { value: "space-between", score: 700, meta: "CSS Value" },
                { value: "space-around", score: 600, meta: "CSS Value" },
                { value: "space-evenly", score: 500, meta: "CSS Value" }
            ];
        } else if (property === "align-items") {
            valueSuggestions = [
                { value: "flex-start", score: 1000, meta: "CSS Value" },
                { value: "flex-end", score: 900, meta: "CSS Value" },
                { value: "center", score: 800, meta: "CSS Value" },
                { value: "baseline", score: 700, meta: "CSS Value" },
                { value: "stretch", score: 600, meta: "CSS Value" }
            ];
        } else if (property === "align-self") {
            valueSuggestions = [
                { value: "auto", score: 1000, meta: "CSS Value" },
                { value: "flex-start", score: 900, meta: "CSS Value" },
                { value: "flex-end", score: 800, meta: "CSS Value" },
                { value: "center", score: 700, meta: "CSS Value" },
                { value: "baseline", score: 600, meta: "CSS Value" },
                { value: "stretch", score: 500, meta: "CSS Value" }
            ];
        } else if (property === "flex-direction") {
            valueSuggestions = [
                { value: "row", score: 1000, meta: "CSS Value" },
                { value: "row-reverse", score: 900, meta: "CSS Value" },
                { value: "column", score: 800, meta: "CSS Value" },
                { value: "column-reverse", score: 700, meta: "CSS Value" }
            ];
        } else if (property === "flex-wrap") {
            valueSuggestions = [
                { value: "nowrap", score: 1000, meta: "CSS Value" },
                { value: "wrap", score: 900, meta: "CSS Value" },
                { value: "wrap-reverse", score: 800, meta: "CSS Value" }
            ];
        } else if (property === "text-align") {
            valueSuggestions = [
                { value: "left", score: 1000, meta: "CSS Value" },
                { value: "right", score: 990, meta: "CSS Value" },
                { value: "center", score: 980, meta: "CSS Value" },
                { value: "justify", score: 970, meta: "CSS Value" },
                { value: "start", score: 960, meta: "CSS Value" },
                { value: "end", score: 950, meta: "CSS Value" }
            ];
        } else if (property === "line-height") {
            valueSuggestions = [
                { value: "normal", score: 1000, meta: "CSS Value" },
                { value: "1", score: 990, meta: "CSS Value" },
                { value: "1.5", score: 980, meta: "CSS Value" },
                { value: "2", score: 970, meta: "CSS Value" },
                { value: "2.5", score: 960, meta: "CSS Value" },
                { value: "3", score: 950, meta: "CSS Value" }
            ];
        } else if (property === "cursor") {
            valueSuggestions = [
                { value: "default", score: 1000, meta: "CSS Value" },
                { value: "pointer", score: 990, meta: "CSS Value" },
                { value: "move", score: 980, meta: "CSS Value" },
                { value: "text", score: 970, meta: "CSS Value" },
                { value: "wait", score: 960, meta: "CSS Value" },
                { value: "help", score: 950, meta: "CSS Value" },
                { value: "not-allowed", score: 940, meta: "CSS Value" }
            ];
        } else if (property === "list-style") {
            valueSuggestions = [
                { value: "disc", score: 1000, meta: "CSS Value" },
                { value: "circle", score: 990, meta: "CSS Value" },
                { value: "square", score: 980, meta: "CSS Value" },
                { value: "decimal", score: 970, meta: "CSS Value" },
                { value: "none", score: 960, meta: "CSS Value" }
            ];
        } else if (property === "pointer-events") {
            valueSuggestions = [
                { value: "auto", score: 1000, meta: "CSS Value" },
                { value: "none", score: 990, meta: "CSS Value" },
                { value: "visible", score: 980, meta: "CSS Value" },
                { value: "visibleFill", score: 970, meta: "CSS Value" },
                { value: "visibleStroke", score: 960, meta: "CSS Value" }
            ];
        } else if (property === "object-fit") {
            valueSuggestions = [
                { value: "fill", score: 1000, meta: "CSS Value" },
                { value: "contain", score: 990, meta: "CSS Value" },
                { value: "cover", score: 980, meta: "CSS Value" },
                { value: "none", score: 970, meta: "CSS Value" },
                { value: "scale-down", score: 960, meta: "CSS Value" }
            ];
        } else if (property === "object-position") {
            valueSuggestions = [
                { value: "top", score: 1000, meta: "CSS Value" },
                { value: "center", score: 990, meta: "CSS Value" },
                { value: "bottom", score: 980, meta: "CSS Value" },
                { value: "left", score: 970, meta: "CSS Value" },
                { value: "right", score: 960, meta: "CSS Value" }
            ];
        }

        // 結合全域值建議並回傳
        callback(null, valueSuggestions.concat(globalValueSuggestions));
    }
};
window.PageReady = function () {
    const formId = "SettingCss";
    const editor = ace.edit("InputWebsiteCss");
    editor.setTheme("ace/theme/monokai");
    editor.session.setMode("ace/mode/css");
    editor.setOptions({
        autoIndent:true,
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