// Shared payment-provider registry. Loaded only by pages that support payment.
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-core", function (C) {
        var factories = {};
        var aliases = {};

        function normalizeCode(code) {
            return String(code || "").trim();
        }

        function register(providerFactory) {
            var code = normalizeCode(providerFactory && providerFactory.code);

            if (!code) throw new Error("Payment provider code is required.");
            if (typeof providerFactory.create !== "function") {
                throw new Error("Payment provider factory must implement create().");
            }

            providerFactory.code = code;
            factories[code] = providerFactory;
            aliases[code.toLowerCase()] = code;

            (providerFactory.aliases || []).forEach(function (alias) {
                var normalizedAlias = normalizeCode(alias).toLowerCase();
                if (normalizedAlias) aliases[normalizedAlias] = code;
            });

            return providerFactory;
        }

        function resolveCode(identifier) {
            var normalizedIdentifier = normalizeCode(identifier);
            if (!normalizedIdentifier) return "Default";

            return aliases[normalizedIdentifier.toLowerCase()] ||
                (C.Payment.Catalog && C.Payment.Catalog.resolveCode(identifier)) ||
                normalizedIdentifier;
        }

        function get(identifier) {
            return factories[resolveCode(identifier)] || null;
        }

        function create(code, options) {
            var normalizedCode = resolveCode(code);
            var providerFactory = factories[normalizedCode];

            if (!providerFactory) {
                throw new Error("Payment provider is not registered: " + normalizedCode);
            }

            return providerFactory.create(options || {});
        }

        function has(code) {
            return get(code) != null;
        }

        function list() {
            return Object.keys(factories);
        }

        C.extend({
            Payment: {
                Core: {
                    register: register,
                    get: get,
                    resolveCode: resolveCode,
                    create: create,
                    has: has,
                    list: list
                }
            }
        });
    });
})(window);
