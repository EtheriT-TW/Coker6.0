// Declarative payment-provider catalog. Keep behavior in provider modules.
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-provider-catalog", function (C) {
        var definitions = [
            {
                code: "Default",
                mode: "internal",
                aliases: [0, 1, "Internal"]
            },
            {
                code: "PCHomePay",
                mode: "redirect",
                aliases: [2]
            },
            {
                code: "LinePay",
                mode: "redirect",
                aliases: [3]
            },
            {
                code: "ECPay",
                mode: "embedded",
                aliases: [4],
                moduleUrl: "/js/Payment/providers/ECPay.min.js",
                hosts: ["Member", "ShoppingCart"]
            }
        ];
        var aliases = {};

        function normalize(value) {
            return String(value == null ? "" : value).trim();
        }

        definitions.forEach(function (definition) {
            aliases[normalize(definition.code).toLowerCase()] = definition.code;

            (definition.aliases || []).forEach(function (alias) {
                aliases[normalize(alias).toLowerCase()] = definition.code;
            });

            Object.freeze(definition.aliases || []);
            Object.freeze(definition.hosts || []);
            Object.freeze(definition);
        });

        function resolveCode(identifier) {
            var normalized = normalize(identifier);
            if (!normalized) return "Default";
            return aliases[normalized.toLowerCase()] || normalized;
        }

        function get(identifier) {
            var code = resolveCode(identifier);

            return definitions.find(function (definition) {
                return definition.code === code;
            }) || null;
        }

        function list(filter) {
            var result = definitions.slice();

            if (filter && filter.mode) {
                result = result.filter(function (definition) {
                    return definition.mode === filter.mode;
                });
            }

            if (filter && filter.host) {
                result = result.filter(function (definition) {
                    return (definition.hosts || []).indexOf(filter.host) >= 0;
                });
            }

            return result;
        }

        C.Payment.Catalog = {
            resolveCode: resolveCode,
            get: get,
            list: list
        };
    });
})(window);
