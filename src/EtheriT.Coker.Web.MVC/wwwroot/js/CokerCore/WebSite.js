Coker.extend({
    WebSite: {
        getPageAll: function (page) {
            return $.ajax({
                url: "/api/Website/GetPageAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { page: page },
            });
        },
        exchange: function (id, websiteName) {
            var _dfr = $.Deferred();
            var waitingTitle = websiteName ? `正在切換至${websiteName}網站` : "正在切換網站";
            co.sweet.loading(waitingTitle, "請稍候，切換完成後將自動重新載入頁面。");

            $.ajax({
                url: "/api/Website/Exchange",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
                dataType: "json"
            }).done(function (result) {
                Swal.close();
                if (result.success) {
                    _c.Data.Header["X-Coker-Website-Id"] = result.message;
                    const pageUrl = new URL(window.location.href);
                    pageUrl.searchParams.set("_site", result.message);
                    pageUrl.searchParams.set("siteChanged", "true");
                    pageUrl.hash = "";
                    _dfr.resolve(result);
                    location.replace(pageUrl.pathname + pageUrl.search);
                    return;
                } else {
                    co.sweet.error("網站切換失敗", result.error || result.message || "無法切換網站，請稍後再試。");
                }
                _dfr.resolve(result);
            }).fail(function (xhr, status, error) {
                Swal.close();
                co.sweet.error("網站切換失敗", "伺服器暫時無法回應，請稍後再試。");
                _dfr.reject(xhr, status, error);
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
