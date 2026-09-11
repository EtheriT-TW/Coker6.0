// Loads provider-specific bundles once, only when a page or order needs them.
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-provider-loader", function (C) {
        var loading = {};

        function ensure(identifier) {
            if (identifier && typeof identifier === "object" && identifier.code) {
                if (C.Payment.Core.has(identifier.code)) {
                    return Promise.resolve(identifier.code);
                }
                identifier = identifier.code;
            }

            var definition = C.Payment.Catalog.get(identifier);

            if (!definition) {
                return Promise.reject(new Error(
                    "Payment provider is not configured: " + String(identifier || "")
                ));
            }

            if (C.Payment.Core.has(definition.code)) {
                return Promise.resolve(definition.code);
            }

            if (!definition.moduleUrl) {
                return Promise.reject(new Error(
                    "Payment provider module URL is missing: " + definition.code
                ));
            }

            if (loading[definition.code]) return loading[definition.code];

            loading[definition.code] = new Promise(function (resolve, reject) {
                var script = w.document.createElement("script");
                script.src = definition.moduleUrl;
                script.async = true;
                script.setAttribute("data-payment-provider-module", definition.code);
                script.onload = function () {
                    if (!C.Payment.Core.has(definition.code)) {
                        delete loading[definition.code];
                        reject(new Error(
                            "Payment provider did not register itself: " + definition.code
                        ));
                        return;
                    }

                    resolve(definition.code);
                };
                script.onerror = function () {
                    delete loading[definition.code];
                    reject(new Error(
                        "Payment provider module failed to load: " + definition.code
                    ));
                };
                w.document.head.appendChild(script);
            });

            return loading[definition.code];
        }

        function ensureAll(identifiers) {
            var codes = [];

            (identifiers || []).forEach(function (identifier) {
                var code = C.Payment.Catalog.resolveCode(identifier);
                if (code && codes.indexOf(code) < 0) {
                    codes.push(code);
                }
            });

            return Promise.all(codes.map(ensure));
        }

        C.Payment.Loader = {
            ensure: ensure,
            ensureAll: ensureAll
        };
    });
})(window);
