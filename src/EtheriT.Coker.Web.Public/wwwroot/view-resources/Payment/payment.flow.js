// Shared checkout and repayment lifecycle.
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-flow", function (C) {
        function resolveProvider(providerOrCode, options) {
            if (providerOrCode && typeof providerOrCode === "object") return providerOrCode;
            return C.Payment.Core.create(providerOrCode, options);
        }

        function prepareCheckout(providerOrCode, context, callback) {
            var provider = resolveProvider(providerOrCode, context && context.providerOptions);

            if (typeof provider.prepareCheckout !== "function") {
                callback(true, null);
                return;
            }

            return provider.prepareCheckout(context || {}, callback);
        }

        function afterOrderCreated(providerOrCode, context) {
            var provider = resolveProvider(providerOrCode, context && context.providerOptions);

            if (typeof provider.afterOrderCreated === "function") {
                return provider.afterOrderCreated(context || {});
            }

            if (context && typeof context.onComplete === "function") context.onComplete();
        }

        function repay(providerOrCode, context) {
            var paymentContext = context || {};
            return C.Payment.Loader.ensure(providerOrCode).then(function () {
                return repayWithRegisteredProvider(providerOrCode, paymentContext);
            }, function (ex) {
                if (typeof paymentContext.onError === "function") {
                    paymentContext.onError("此付款方式尚未註冊，無法重新付款。", ex, null);
                }
                return null;
            });
        }

        function repayWithRegisteredProvider(providerOrCode, paymentContext) {
            var provider = resolveProvider(
                providerOrCode,
                paymentContext.providerOptions
            );

            return C.Payment.Repay({ ohid: paymentContext.orderId }).done(function (checkResult) {
                if (!checkResult || !checkResult.success) {
                    if (typeof paymentContext.onError === "function") {
                        paymentContext.onError(
                            checkResult && checkResult.message || "重新付款發生錯誤。",
                            checkResult,
                            provider
                        );
                    }
                    return;
                }

                if (typeof provider.repay !== "function") {
                    if (typeof paymentContext.onError === "function") {
                        paymentContext.onError("此付款方式不支援重新付款。", null, provider);
                    }
                    return;
                }

                provider.repay(paymentContext);
            }).fail(function (result) {
                if (typeof paymentContext.onError === "function") {
                    paymentContext.onError("重新付款服務連線失敗，請稍後再試。", result, provider);
                }
            });
        }

        C.Payment.Flow = {
            resolveProvider: resolveProvider,
            prepareCheckout: prepareCheckout,
            afterOrderCreated: afterOrderCreated,
            repay: repay
        };
    });
})(window);
