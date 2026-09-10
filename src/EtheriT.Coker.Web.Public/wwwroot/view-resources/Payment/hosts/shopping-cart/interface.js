// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.redirect.js
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
        var providerCode = Coker.Payment.Core.has(parsed.paymentType)
            ? parsed.paymentType
            : "Default";
        var pageProvider = cart.Payment.Core.getProvider(providerCode);
        var provider = pageProvider && pageProvider.paymentProvider
            ? pageProvider.paymentProvider
            : Coker.Payment.Core.create(providerCode, {
                rootSelector: "#EmbeddedPayment"
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

    // 掛載所有已註冊且支援 ShoppingCart Host 的嵌入式付款 Adapter。
    Coker.Payment.Embedded.attachAll("ShoppingCart", {
        cart: cart
    });
})(window.ShoppingCart, window.jQuery);
