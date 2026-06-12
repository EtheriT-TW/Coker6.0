// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.pchomepay.js
(function (cart) {
    "use strict";

    cart.Payment.Core.register({
        code: "PCHomePay",
        type: "redirect",
        afterOrderCreated: function (orderResult, context) {
            return cart.Payment.Redirect.requestPaymentUrl(context.orderId, "PCHomePay");
        }
    });
})(window.ShoppingCart);
