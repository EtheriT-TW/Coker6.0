(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        util: {
            html: {
                replaceAndSinge: function (str) {
                    if (!str) return "";

                    let s = String(str);
                    const re = /&amp;((?:lt|gt|quot|apos);|#\d+;|#x[0-9a-fA-F]+;)/g;

                    for (let i = 0; i < 5; i++) {
                        if (!re.test(s)) break;
                        s = s.replace(re, "&$1");
                    }

                    return s;
                },

                htmlEncode: function (text) {
                    var div = document.createElement("div");
                    div.appendChild(document.createTextNode(text));
                    return div.innerHTML;
                },
                getPageTitle() {
                    var og = document.querySelector('meta[property="og:title"]');
                    if (og && og.content) return og.content.trim();
                    return (document.title || location.hostname).trim();
                }
            }
        }
    });

    // Legacy: Coker.stringManager.*
    Coker.stringManager = Coker.stringManager || {};
    Coker.stringManager.ReplaceAndSinge = Coker.stringManager.ReplaceAndSinge || Coker.util.html.replaceAndSinge;
    Coker.stringManager.htmlEncode = Coker.stringManager.htmlEncode || Coker.util.html.htmlEncode;

})(window);
