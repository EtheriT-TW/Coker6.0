// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.redirect.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Payment = cart.Payment || {};
    cart.Payment.Redirect = {
        requestPaymentUrl: requestPaymentUrl
    };

    function requestPaymentUrl(orderId, paymentType) {
        Coker.sweet.loading();

        return Coker.ThirdParty.Request(orderId, paymentType, null).done(function (result) {
            Swal.close();

            if (result.success) {
                localStorage.setItem("lastSaveTime", new Date().toISOString());
                localStorage.setItem("lastSaveToken", localStorage.getItem("token"));

                cart.CheckoutResult.setStatus("訂單已成立，即將進入付款流程。");
                cart.CheckoutResult.showThirdPayLink(result.message);
                cart.CheckoutResult.goToResultPage();

                window.open(result.message, "_blank");
            } else {
                cart.CheckoutResult.setStatus("付款流程發生未知錯誤，請稍後重新嘗試，或直接聯繫客服人員。");
                cart.CheckoutResult.goToResultPage();
            }
        });
    }
})(window.ShoppingCart, window.jQuery);
