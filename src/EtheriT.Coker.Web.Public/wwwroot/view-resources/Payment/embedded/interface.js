(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-embedded-interface", function (C) {
        var adapters = {};

        function normalize(value) {
            return String(value || "").trim();
        }

        function registerAdapter(providerCode, hostCode, factory) {
            var provider = C.Payment.Core.get(providerCode);
            var normalizedHostCode = normalize(hostCode);

            if (!provider) throw new Error("Embedded payment provider is not registered: " + providerCode);
            if (!normalizedHostCode) throw new Error("Embedded payment host code is required.");
            if (typeof factory !== "function") throw new Error("Embedded payment adapter factory is required.");

            adapters[provider.code] = adapters[provider.code] || {};
            adapters[provider.code][normalizedHostCode] = factory;
        }

        function createAdapter(providerCode, hostCode, context) {
            var provider = C.Payment.Core.get(providerCode);
            var normalizedHostCode = normalize(hostCode);
            var factory = provider && adapters[provider.code] && adapters[provider.code][normalizedHostCode];

            if (typeof factory !== "function") return null;
            return factory(context || {});
        }

        function attachAll(hostCode, context) {
            return C.Payment.Core.list().map(function (providerCode) {
                return createAdapter(providerCode, hostCode, context);
            }).filter(function (adapter) {
                return adapter != null;
            });
        }

        C.Payment.Embedded = {
            registerAdapter: registerAdapter,
            createAdapter: createAdapter,
            attachAll: attachAll
        };
    });
})(window);
