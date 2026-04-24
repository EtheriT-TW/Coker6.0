(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    function normalizeLoginValue(value) {
        if (value === true) return true;
        if (value === false) return false;
        if (value === 1 || value === "1") return true;
        if (value === 0 || value === "0") return false;

        if (typeof value === "string") {
            var text = value.trim().toLowerCase();
            if (text === "true" || text === "y" || text === "yes") return true;
            if (text === "false" || text === "n" || text === "no" || text === "") return false;
        }

        return false;
    }

    Coker.extend({
        auth: {
            normalizeLoginValue: normalizeLoginValue,
            isLoggedIn: function () {
                return normalizeLoginValue(w.__LOGIN_STATE__);
            }
        }
    }, { overwrite: false });

    // Legacy / shortcut
    Coker.Auth = Coker.Auth || {};
    Coker.Auth.isLoggedIn = Coker.Auth.isLoggedIn || Coker.auth.isLoggedIn;
    Coker.Auth.normalizeLoginValue = Coker.Auth.normalizeLoginValue || Coker.auth.normalizeLoginValue;

})(window);