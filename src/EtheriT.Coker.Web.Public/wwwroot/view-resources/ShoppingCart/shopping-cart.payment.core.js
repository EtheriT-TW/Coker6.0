// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.core.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Payment = cart.Payment || {};
    cart.Payment.Core = cart.Payment.Core || {};

function GetCheckedPaymentRadio() {
    return $('#RadioPayment input[name="RadioPayment"]:checked');
}
function GetCheckedPaymentValue() {
    return cart.Payment.Core.GetCheckedPaymentRadio().val();
}
function IsPaymentRadioECPay($radio) {
    return Number($radio.attr("data-third-party-id") || 0) === S.ECPAY_THIRD_PARTY_ID;
}
function IsECPaySelected() {
    var $checked = cart.Payment.Core.GetCheckedPaymentRadio();

    return S.HasECPay && (($checked.length > 0 && cart.Payment.Core.IsPaymentRadioECPay($checked)) || $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li.ecpay-pl-act").length > 0);
}
function GetECPayEntryRadio() {
    return $('#RadioPayment input[name="RadioPayment"][data-third-party-id="' + S.ECPAY_THIRD_PARTY_ID + '"]').first();
}
function GetECPayEntryValue() {
    var $radio = cart.Payment.Core.GetECPayEntryRadio();
    return $radio.length ? $radio.val() : null;
}
function updatePaymentRadioUI($target) {
    $('#RadioPayment .payment_display').removeClass("checked first last");

    $target.find("input").prop("checked", true);
    $target.find(".payment_display").addClass("checked");

    var $visibleList = $("#RadioPayment > .form-check:not(.d-none)");

    $visibleList.first().find(".payment_display").addClass("first");
    if (!S.ECPayReady) $visibleList.last().find(".payment_display").addClass("last");
    $target.prevAll(".form-check:not(.d-none)").first().find(".payment_display").addClass("last");
    $target.nextAll(".form-check:not(.d-none)").first().find(".payment_display").addClass("first");
}
function RadioPayment() {
    var $pay_text = $(".payment_method");
    $pay_text.addClass("fs-2 fw-bold px-3");
    var $checked = S.$pay_method.filter(':checked');
    if ($checked.length) {
        var val = $checked.val();
        if (val == 1) {
            $('.pay_info').removeClass('d-none');
        } else {
            $('.pay_info').addClass('d-none');
        }
        $pay_text.text($checked.data('title'));
    }
    S.buy_step_swiper.update();
}
function Step3Monitor() {
    S.OrdererFilled = cart.Forms.FormCheck(S.OrdererForms)
    if (S.RecipientOpen) {
        S.RecipientFilled = cart.Forms.FormCheck(S.RecipientForms);
    } else {
        switch ($(`[name="RecipientRadio"]:checked`).val()) {
            case "order":
                cart.Forms.RecipientSameOrderer();
                S.RecipientFilled = true;
                break;
        }
    }
    if (S.InvoiceOpen) {
        S.InvoiceFilled = cart.Forms.FormCheck(S.InvoiceForms)
    } else if ($(`[name="InvoiceRadio"]`).length == 0) {
        S.InvoiceFilled = true;
    } else {
        switch ($(`[name="InvoiceRadio"]:checked`).val()) {
            case "order":
            case "recipient":
                S.InvoiceFilled = true;
                break;
        }
    }

    S.shipMethodsChosen = cart.Forms.FormCheck(S.ShippingForms);
    S.payMethodsChosen = cart.Forms.FormCheck(S.PaymentForms);
}

var providers = {};

function register(provider) {
    if (!provider || !provider.code) throw new Error("Payment provider code is required.");
    providers[provider.code] = provider;
}
function getProvider(code) {
    return providers[code] || null;
}
function parseOrderResult(orderResult) {
    var message = String(orderResult && orderResult.message || "");
    var parts = message.split(",");
    return {
        paymentType: parts[0] || "Default",
        orderId: parts[1] || "",
        creationTime: parts[2] || ""
    };
}
function initAll() {
    Object.keys(providers).forEach(function (key) {
        var provider = providers[key];
        if (provider && typeof provider.init === "function") provider.init();
    });
}
function afterOrderCreated(orderResult, context) {
    var parsed = parseOrderResult(orderResult);
    var provider = getProvider(parsed.paymentType) || getProvider("Default");
    var nextContext = $.extend({}, context || {}, parsed);

    if (provider && typeof provider.afterOrderCreated === "function") {
        return provider.afterOrderCreated(orderResult, nextContext);
    }

    return $.Deferred().resolve({ success: true }).promise();
}
function onAmountChanged() {
    if (cart.Payment.ECPay && typeof cart.Payment.ECPay.MarkECPayDirty === "function") {
        cart.Payment.ECPay.MarkECPayDirty();
    }
}

register({
    code: "Default",
    type: "internal",
    afterOrderCreated: function () {
        setTimeout(function () {
            cart.CheckoutResult.goToResultPage();
        }, 300);
    }
});


    Object.assign(cart.Payment.Core, {
        GetCheckedPaymentRadio: GetCheckedPaymentRadio,
        GetCheckedPaymentValue: GetCheckedPaymentValue,
        IsPaymentRadioECPay: IsPaymentRadioECPay,
        IsECPaySelected: IsECPaySelected,
        GetECPayEntryRadio: GetECPayEntryRadio,
        GetECPayEntryValue: GetECPayEntryValue,
        updatePaymentRadioUI: updatePaymentRadioUI,
        RadioPayment: RadioPayment,
        Step3Monitor: Step3Monitor,
        register: register,
        getProvider: getProvider,
        parseOrderResult: parseOrderResult,
        initAll: initAll,
        afterOrderCreated: afterOrderCreated,
        onAmountChanged: onAmountChanged
    });
})(window.ShoppingCart, window.jQuery);
