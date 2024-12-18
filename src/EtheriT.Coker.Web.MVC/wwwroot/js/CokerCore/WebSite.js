Coker.extend({
    WebSite: {
        exchange: function (id) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/Website/Exchange",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    co.Cookie.EffectiveTime = co.Data.Time.DataRetentionLongTime;
                    co.Cookie.Add("LastWebSite", result.message);
                    co.Cookie.EffectiveTime = co.Data.Time.DataRetentionTime;
                }
                _dfr.resolve(result);
            });
            return _dfr.promise();
        },
        getPrivacyAndTerms: function () {
            return $.ajax({
                url: "/api/Website/GetPrivacyAndTerms/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        Save: function (data) {
            return $.ajax({
                url: "/api/Website/Save",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        LoadFrameCss: function () {
            return $.ajax({
                url: "/api/Website/LoadFrameCss",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        SettingCss: function (cssString) {
            let error = "";
            const _dfr = $.Deferred();
            function isValidCss() {
                try {
                    if (cssString == "") return true;
                    // 使用 CSSStyleSheet 嘗試完整解析 CSS 字串
                    const sheet = new CSSStyleSheet();
                    sheet.replaceSync(cssString);
                    if (sheet.cssRules.length === 0) {
                        throw new Error("格式不正確或包含空白內容");
                    }
                    // 解析成功則進行進一步逐條檢查
                    for (let rule of sheet.cssRules) {
                        // 若該條目無效則認為整體 CSS 無效
                        if (!(rule instanceof CSSStyleRule)) {
                            throw new Error(co.sweet.TitleHilight(`字串包含不支援或無效的規則 {0}`,rule.cssText));
                            return false;
                        }
                        if (!rule.selectorText || !rule.style.cssText) {
                            throw new Error(co.sweet.TitleHilight(`格式不正確，缺少選擇器或屬性 {0}`,rule.cssText));
                            return false;
                        }
                    }
                    return true;
                } catch (e) {
                    error = `CSS 格式錯誤:${e.message}`;
                    return false;
                }
            }
            //if (isValidCss()) {
                $.ajax({
                    url: "/api/Website/SettingCss",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify({ css: cssString }),
                    dataType: "json"
                }).done(function (result) {
                    _dfr.resolve(result);
                });
            //} else _dfr.resolve({ success: false, error: error });
            return _dfr.promise();
        }
    }
});