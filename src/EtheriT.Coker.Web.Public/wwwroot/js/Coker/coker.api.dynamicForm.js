(function (window) {
    "use strict";

    const Coker = window.Coker = window.Coker || {};

    Coker.defineModule("api-dynamic-form", function (C) {
        C.extend({
            DynamicFormApi: {
                Submit: function (data) {
                    return C.api.post("/api/Contact/submit", data, {auth: false});
                }
            }
        }, { overwrite: false });
    });

})(window);