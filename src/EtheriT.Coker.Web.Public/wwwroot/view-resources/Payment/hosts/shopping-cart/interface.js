// Provider-neutral interface between ShoppingCart and Payment Flow.
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Payment = cart.Payment || {};
    cart.Payment.Redirect = {
        openPaymentUrl: openPaymentUrl,
        showPaymentError: showPaymentError
    };

    function openPaymentUrl(url) {
        Swal.close();
        localStorage.setItem("lastSaveTime", new Date().toISOString());
        localStorage.setItem("lastSaveToken", localStorage.getItem("token"));

        cart.CheckoutResult.setStatus("訂單已成立，即將進入付款流程。");
        cart.CheckoutResult.showThirdPayLink(url);
        cart.CheckoutResult.goToResultPage();

        window.open(url, "_blank");
    }

    function showPaymentError() {
        Swal.close();
        cart.CheckoutResult.setStatus("付款流程發生未知錯誤，請稍後重新嘗試，或直接聯繫客服人員。");
        cart.CheckoutResult.goToResultPage();
    }

    function afterOrderCreated(orderResult, context) {
        var parsed = cart.Payment.Core.parseOrderResult(orderResult);
        return Coker.Payment.Loader.ensure(parsed.paymentType).then(function (providerCode) {
            return runAfterOrderCreated(orderResult, context, parsed, providerCode);
        }, function (error) {
            console.error("[Payment] Provider load failed.", error);
            showPaymentError();
        });
    }

    function runAfterOrderCreated(orderResult, context, parsed, providerCode) {
        var pageProvider = cart.Payment.Core.getProvider(providerCode);
        var provider = pageProvider && pageProvider.paymentProvider
            ? pageProvider.paymentProvider
            : Coker.Payment.Core.create(providerCode, {
                rootSelector: "#ECPayPayment"
            });
        var paymentContext = $.extend({}, context || {}, parsed, {
            onStart: function () {
                Coker.sweet.loading();
            },
            onRedirect: openPaymentUrl,
            onError: showPaymentError,
            onComplete: function () {
                setTimeout(function () {
                    cart.CheckoutResult.goToResultPage();
                }, 300);
            },
            onEmbeddedComplete: function () {
                if (pageProvider && typeof pageProvider.afterOrderCreated === "function") {
                    return pageProvider.afterOrderCreated(orderResult, paymentContext);
                }

                showPaymentError();
            }
        });

        return Coker.Payment.Flow.afterOrderCreated(provider, paymentContext);
    }

    // 購物車既有入口保留，但實際生命週期統一交給全站 Payment Flow。
    cart.Payment.Core.afterOrderCreated = afterOrderCreated;

    var enabledEmbeddedProviderCodes = [];

    $('#RadioPayment input[data-provider-code]').each(function () {
        if (String($(this).attr("data-render-mode") || "").toLowerCase() !== "embedded") {
            return;
        }

        var providerCode = String($(this).attr("data-provider-code") || "");
        if (providerCode && enabledEmbeddedProviderCodes.indexOf(providerCode) < 0) {
            enabledEmbeddedProviderCodes.push(providerCode);
        }
    });

    // 只載入並掛載後台已啟用、且本頁實際需要的嵌入式 Provider。
    cart.Payment.Ready = Coker.Payment.Loader
        .ensureAll(enabledEmbeddedProviderCodes)
        .then(function (providerCodes) {
            return Coker.Payment.Embedded.attach(
                providerCodes,
                "ShoppingCart",
                { cart: cart }
            );
        })
        .catch(function (error) {
            console.error("[Payment] ShoppingCart provider bootstrap failed.", error);
            cart.Payment.LoadError = error;
            return [];
        });
})(window.ShoppingCart, window.jQuery);
