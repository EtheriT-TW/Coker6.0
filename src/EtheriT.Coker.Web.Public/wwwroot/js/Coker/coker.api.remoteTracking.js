(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        RemoteTracking: {
            Collect: function (data, useBeacon) {
                var url = "/api/UserStatistic/Collect";
                var json = JSON.stringify(data);

                if (useBeacon && navigator.sendBeacon) {
                    return navigator.sendBeacon(
                        url,
                        new Blob([json], { type: "application/json" })
                    );
                }

                return $.ajax({
                    url: url,
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    data: json
                });
            }
        }
    });
})(window);
