(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        util: {
            device: {
                isMobileDevice: function () {
                    var mobileDevices = ["Android", "webOS", "iPhone", "iPad", "iPod", "BlackBerry", "Windows Phone"];
                    for (var i = 0; i < mobileDevices.length; i++) {
                        if (navigator.userAgent.match(mobileDevices[i])) return true;
                    }
                    return false;
                }
            }
        }
    });

    // Legacy: Coker.isMobileDevice()
    Coker.isMobileDevice = Coker.isMobileDevice || function () {
        return Coker.util.device.isMobileDevice();
    };

})(window);