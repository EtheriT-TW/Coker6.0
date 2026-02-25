(function (w) {
    "use strict";
    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Token: {
            GetToken: function () {
                return Coker.api.post("/api/Token/CreateToken", null, { auth: false });
            },
            CheckToken: function () {
                return Coker.api.get("/api/Token/CheckToken/");
            },
            AgreePrivacy: function () {
                return Coker.api.get("/api/Token/AgreePrivacy/");
            }
        }
    });

})(window);