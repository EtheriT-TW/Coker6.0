(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        util: {
            html: {
                replaceAndSinge: function (str) {
                    if (!str) return "";
                    var s = String(str).replace(/&amp;/g, "&");
                    // 若仍含 &amp;，遞迴處理
                    return (s.indexOf("&amp;") > -1) ? Coker.util.html.replaceAndSinge(s) : s;
                },

                htmlEncode: function (text) {
                    var div = document.createElement("div");
                    div.appendChild(document.createTextNode(text));
                    return div.innerHTML;
                }
            }
        }
    });

    // Legacy: Coker.stringManager.*
    Coker.stringManager = Coker.stringManager || {};
    Coker.stringManager.ReplaceAndSinge = Coker.stringManager.ReplaceAndSinge || Coker.util.html.replaceAndSinge;
    Coker.stringManager.htmlEncode = Coker.stringManager.htmlEncode || Coker.util.html.htmlEncode;

})(window);
