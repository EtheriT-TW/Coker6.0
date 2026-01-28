(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    // New: Coker.config.timeout
    Coker.extend({
        config: {
            timeout: {
                time: 1500
            }
        }
    }, { overwrite: false });

    // Legacy: Coker.timeout
    Coker.timeout = Coker.timeout || Coker.config.timeout;

})(window);
