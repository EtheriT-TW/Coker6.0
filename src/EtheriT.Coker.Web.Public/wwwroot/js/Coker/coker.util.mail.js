(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    // 小工具：安全取 local key（避免某頁沒注入 local 就炸）
    function t(local, key, fallback) {
        if (local && Object.prototype.hasOwnProperty.call(local, key) && local[key]) return local[key];
        return fallback || key;
    }

    // 小工具：組 mailto
    function buildMailto(to, subject, body) {
        var mailto = "mailto:" + (to ? encodeURIComponent(to) : "");
        var qs = [];
        if (subject) qs.push("subject=" + encodeURIComponent(subject));
        if (body) qs.push("body=" + encodeURIComponent(body));
        if (qs.length) mailto += "?" + qs.join("&");
        return mailto;
    }

    Coker.extend({
        util: {
            mail: {
                /**
                 * 開啟 mailto（依裝置顯示提示）
                 * @param {Object} opt
                 * @param {string} [opt.to] 收件者，可空（例如分享用 mailto:?subject=...）
                 * @param {string} [opt.subject]
                 * @param {string} [opt.body]
                 * @param {Object} [opt.local] 多語系字典（const local）
                 * @param {boolean} [opt.desktopConfirm=true] 桌機是否先 confirm 再觸發
                 * @param {boolean} [opt.mobileHint=true] 行動裝置是否顯示提醒（不阻擋）
                 * @param {string} [opt.confirmTitleKey]
                 * @param {string} [opt.confirmBodyKey]
                 * @param {string} [opt.confirmOkKey]
                 * @param {string} [opt.confirmCancelKey]
                 * @param {string} [opt.mobileTitleKey]
                 * @param {string} [opt.mobileBodyKey]
                 */
                open: function (opt) {
                    opt = opt || {};

                    var isMobile = !!(Coker.util && Coker.util.device && Coker.util.device.isMobileDevice)
                        ? Coker.util.device.isMobileDevice()
                        : /(mobile|android|pad)/i.test(navigator.userAgent);

                    var mailto = buildMailto(opt.to, opt.subject, opt.body);

                    // keys (允許外部覆蓋)
                    var local = opt.local || w.local; // 你頁面上是 const local = ...
                    var confirmTitleKey = opt.confirmTitleKey || "MailtoHintTitle";
                    var confirmBodyKey = opt.confirmBodyKey || "MailtoHintBody";
                    var okKey = opt.confirmOkKey || "MailtoHintOk";
                    var cancelKey = opt.confirmCancelKey || "MailtoHintCancel";
                    var mobileTitleKey = opt.mobileTitleKey || "MailtoHintMobileTitle";
                    var mobileBodyKey = opt.mobileBodyKey || "MailtoHintMobileBody";

                    // 📱 Mobile：提示但不中斷（避免多一步確認）
                    if (isMobile) {
                        if (opt.mobileHint !== false && Coker.ui && Coker.ui.sweet && typeof Coker.ui.sweet.info === "function") {
                            Coker.ui.sweet.info(
                                t(local, mobileTitleKey, "Email"),
                                t(local, mobileBodyKey, "")
                            );
                        }
                        w.location.href = mailto;
                        return;
                    }

                    // 🖥 Desktop：先 confirm 再開（確保「提醒有看到」）
                    var needConfirm = (opt.desktopConfirm !== false);

                    if (needConfirm && Coker.sweet && typeof Coker.sweet.confirm === "function") {
                        Coker.sweet.confirm(
                            t(local, confirmTitleKey, "Email"),
                            t(local, confirmBodyKey, ""),
                            t(local, okKey, "OK"),
                            t(local, cancelKey, "Cancel"),
                            function () {
                                w.location.href = mailto;
                            }
                        );
                        return;
                    }

                    // fallback：直接開
                    w.location.href = mailto;
                }
            }
        }
    });

})(window);