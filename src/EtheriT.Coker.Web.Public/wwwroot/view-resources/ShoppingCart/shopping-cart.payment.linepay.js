// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.linepay.js
(function (cart) {
    "use strict";

    cart.Payment.Core.register({
        code: "LinePay",
        type: "redirect",
        afterOrderCreated: function (orderResult, context) {
            return cart.Payment.Redirect.requestPaymentUrl(context.orderId, "LinePay");
        }
    });
})(window.ShoppingCart);
