(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        WebMenu: {
            getMainMenu: function () {
                return Coker.api.get("/api/WebMenu/GetMainMenu/", null, { auth: false });
            }
        }
    });

})(window);
