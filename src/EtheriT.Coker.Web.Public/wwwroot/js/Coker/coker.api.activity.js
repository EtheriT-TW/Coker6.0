(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Activity: {
            Click: function (FK_Aid) {
                return Coker.api.get("/api/Advertise/ActivityClick/", { FK_Aid: FK_Aid }, { auth: false });
            },
            Exposure: function (FK_Aid) {
                return Coker.api.get("/api/Advertise/ActivityExposure/", { FK_Aid: FK_Aid }, { auth: false });
            }
        }
    });

})(window);