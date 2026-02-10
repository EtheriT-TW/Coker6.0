(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Directory: {
            getDirectoryData: function (data) {
                return Coker.api.post("/api/Directory/GetReleInfo", data);
            },
            getDirectoryMenuData: function (data) {
                return Coker.api.post("/api/Directory/GetReleMenu", data);
            },
            getDirectoryAdvertiseData: function (data) {
                return Coker.api.post("/api/Directory/GetReleAd", data);
            },
            SwitchPage: function (data) {
                return Coker.api.post("/api/Directory/SwitchPage", data);
            },
            getFacet: function (data) {
                return Coker.api.post("/api/Directory/GetFacet", data);
            }
        }
    });

})(window);