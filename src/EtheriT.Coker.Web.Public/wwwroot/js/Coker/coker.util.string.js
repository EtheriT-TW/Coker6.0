(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        util: {
            string: {
                generateRandomString: function (num) {
                    var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    var result = "";
                    var len = characters.length;

                    for (var i = 0; i < num; i++) {
                        result += characters.charAt(Math.floor(Math.random() * len));
                    }
                    return result;
                },

                isNullOrEmpty: function (str) {
                    if (typeof str === "undefined" || str === null) return true;
                    var s = String(str);
                    return s.trim() === "";
                },

                getWeekNumber: function (i) {
                    var characters = "一二三四五六日";
                    return characters.charAt(i - 1);
                },

                thousandSign: function (input) {
                    var num = parseFloat(String(input).replace(/,/g, ""));
                    return isNaN(num) ? "0" : num.toLocaleString();
                },
                replace: function (str, ...args) {
                    return str.replace(/\{(\d+)\}/g, (match, index) => {
                        return typeof args[index] !== "undefined" ? args[index] : match;
                    });
                }
            }
        }
    });

    // Legacy: Coker.String.*
    Coker.String = Coker.String || {};
    Coker.String.generateRandomString = Coker.String.generateRandomString || Coker.util.string.generateRandomString;
    Coker.String.isNullOrEmpty = Coker.String.isNullOrEmpty || Coker.util.string.isNullOrEmpty;
    Coker.String.getWeekNumber = Coker.String.getWeekNumber || Coker.util.string.getWeekNumber;
    Coker.String.thousandSign = Coker.String.thousandSign || Coker.util.string.thousandSign;

})(window);