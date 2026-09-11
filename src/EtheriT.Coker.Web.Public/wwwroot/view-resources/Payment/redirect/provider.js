// Shared redirect-provider implementations.
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-redirect", function (C) {
        function createRedirectProvider(code) {
            return function () {
                var provider = {
                    code: code,
                    mode: "redirect",

                    prepareCheckout: function (context, callback) {
                        callback(true, null);
                    },

                    requestPayment: function (context) {
                        if (typeof context.onStart === "function") context.onStart(provider);

                        return C.ThirdParty.Request(context.orderId, provider.code, null).done(function (result) {
                            if (!result || !result.success) {
                                if (typeof context.onError === "function") {
                                    context.onError(result && result.message, result, provider);
                                }
                                return;
                            }

                            if (typeof context.onRedirect === "function") {
                                context.onRedirect(result.message, result, provider);
                            }
                        }).fail(function (result) {
                            if (typeof context.onError === "function") {
                                context.onError("付款服務連線失敗，請稍後重新嘗試。", result, provider);
                            }
                        });
                    },

                    afterOrderCreated: function (context) {
                        return provider.requestPayment(context);
                    },

                    repay: function (context) {
                        return provider.requestPayment(context);
                    }
                };

                return provider;
            };
        }

        C.Payment.Catalog.list().forEach(function (definition) {
            if (definition.mode === "redirect") {
                // 有獨立模組代表流程不符合共用 PayRequest 契約，交由 Loader 載入。
                if (definition.moduleUrl) return;

                C.Payment.Core.register({
                    code: definition.code,
                    mode: definition.mode,
                    aliases: definition.aliases,
                    create: createRedirectProvider(definition.code)
                });
                return;
            }

            if (definition.mode !== "internal") return;

            C.Payment.Core.register({
                code: definition.code,
                mode: definition.mode,
                aliases: definition.aliases,
                create: function () {
                    return {
                        code: definition.code,
                        mode: definition.mode,
                        prepareCheckout: function (context, callback) {
                            callback(true, null);
                        },
                        afterOrderCreated: function (context) {
                            if (typeof context.onComplete === "function") context.onComplete();
                        }
                    };
                }
            });
        });
    });
})(window);
