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
    
    function updatePaymentRadioUI($target) {
        $('#RadioPayment .payment_display').removeClass("checked first last");

        $target.find("input").prop("checked", true);
        $target.find(".payment_display").addClass("checked");

        var $visibleList = $("#RadioPayment > .form-check:not(.d-none)");

        $visibleList.first().find(".payment_display").addClass("first");
        if (!areProvidersByTypeReady("embedded")) {
            $visibleList.last().find(".payment_display").addClass("last");
        }
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
    function getProviders() {
        return Object.keys(providers).map(function (key) {
            return providers[key];
        });
    }

    function getProvidersByType(type) {
        return getProviders().filter(function (provider) {
            return provider && provider.type === type;
        });
    }

    function hasProvidersByType(type) {
        return getProvidersByType(type).length > 0;
    }

    function areProvidersByTypeReady(type) {
        var list = getProvidersByType(type);

        if (list.length === 0) {
            return true;
        }

        return list.every(function (provider) {
            if (typeof provider.isReady === "function") {
                return provider.isReady();
            }

            return true;
        });
    }

    function clearProvidersByType(type) {
        getProvidersByType(type).forEach(function (provider) {
            if (typeof provider.clear === "function") {
                provider.clear();
            }
        });
    }

    function setProvidersMonitorByType(type, enabled) {
        getProvidersByType(type).forEach(function (provider) {
            if (typeof provider.setMonitor === "function") {
                provider.setMonitor(enabled);
            }
        });
    }

    function getProviderByRadio($radio) {
        if (!$radio || !$radio.length) return null;

        var thirdPartyId = Number($radio.attr("data-third-party-id") || 0);
        var value = String($radio.val() || "");

        return getProviders().find(function (provider) {
            if (typeof provider.isMatchRadio === "function") {
                return provider.isMatchRadio($radio);
            }

            if (provider.thirdPartyId != null) {
                return Number(provider.thirdPartyId) === thirdPartyId;
            }

            if (provider.entryValue != null) {
                return String(provider.entryValue) === value;
            }

            return false;
        }) || null;
    }
    function isEmbeddedPaymentRadio($radio) {
        var provider = getProviderByRadio($radio);
        return provider != null && provider.type === "embedded";
    }
    function getActiveProvider() {
        var $checked = GetCheckedPaymentRadio();
        var provider = getProviderByRadio($checked);

        if (provider) return provider;

        // embedded provider 可能 radio 被暫時清掉，但模組內已有選取狀態
        return getProvidersByType("embedded").find(function (provider) {
            return typeof provider.isSelected === "function" && provider.isSelected();
        }) || null;
    }

    function getActiveEmbeddedProvider() {
        var provider = getActiveProvider();

        if (!provider || provider.type !== "embedded") {
            return null;
        }

        return provider;
    }

    function isActiveEmbeddedPayment() {
        return getActiveEmbeddedProvider() != null;
    }

    function isActiveEmbeddedReady() {
        var provider = getActiveEmbeddedProvider();

        if (!provider) {
            return true;
        }

        if (typeof provider.isReady === "function") {
            return provider.isReady();
        }

        return true;
    }

    function isActiveEmbeddedLoaded() {
        var provider = getActiveEmbeddedProvider();

        if (!provider) {
            return true;
        }

        if (typeof provider.isLoaded === "function") {
            return provider.isLoaded();
        }

        return true;
    }

    function getActivePaymentValue() {
        var provider = getActiveProvider();

        if (provider && typeof provider.getPaymentValue === "function") {
            var providerValue = provider.getPaymentValue();
            if (providerValue != null && providerValue !== "") {
                return providerValue;
            }
        }

        var checkedValue = GetCheckedPaymentValue();
        return checkedValue != null && checkedValue !== "" ? checkedValue : null;
    }

    function reloadActiveEmbeddedProvider() {
        var provider = getActiveProvider();

        if (!provider || provider.type !== "embedded") {
            return $.Deferred().resolve({ success: true }).promise();
        }

        if (typeof provider.reload !== "function") {
            return $.Deferred().resolve({ success: true }).promise();
        }

        return provider.reload();
    }

    function validateActiveEmbeddedPayment(callback) {
        var provider = getActiveProvider();

        if (!provider || provider.type !== "embedded") {
            callback(true, null);
            return;
        }

        if (typeof provider.validatePayment !== "function") {
            callback(true, null);
            return;
        }

        provider.validatePayment(callback);
    }

    function submitActiveEmbeddedPayment(callback) {
        var provider = getActiveProvider();

        if (!provider || provider.type !== "embedded") {
            callback(true, null);
            return;
        }

        if (typeof provider.submitPayment === "function") {
            provider.submitPayment(callback);
            return;
        }

        if (typeof provider.validatePayment === "function") {
            provider.validatePayment(callback);
            return;
        }

        callback(true, null);
    }

    function onAmountChanged() {
        getProvidersByType("embedded").forEach(function (provider) {
            if (typeof provider.markDirty === "function") {
                provider.markDirty();
            }
        });
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

        updatePaymentRadioUI: updatePaymentRadioUI,
        RadioPayment: RadioPayment,
        Step3Monitor: Step3Monitor,

        register: register,
        getProvider: getProvider,
        getProviders: getProviders,
        getProvidersByType: getProvidersByType,
        hasProvidersByType: hasProvidersByType,
        areProvidersByTypeReady: areProvidersByTypeReady,
        clearProvidersByType: clearProvidersByType,
        setProvidersMonitorByType: setProvidersMonitorByType,
        getProviderByRadio: getProviderByRadio,
        isEmbeddedPaymentRadio: isEmbeddedPaymentRadio,
        getActiveProvider: getActiveProvider,
        getActiveEmbeddedProvider: getActiveEmbeddedProvider,
        isActiveEmbeddedPayment: isActiveEmbeddedPayment,
        isActiveEmbeddedReady: isActiveEmbeddedReady,
        isActiveEmbeddedLoaded: isActiveEmbeddedLoaded,
        getActivePaymentValue: getActivePaymentValue,
        reloadActiveEmbeddedProvider: reloadActiveEmbeddedProvider,
        validateActiveEmbeddedPayment: validateActiveEmbeddedPayment,
        submitActiveEmbeddedPayment: submitActiveEmbeddedPayment,

        parseOrderResult: parseOrderResult,
        initAll: initAll,
        afterOrderCreated: afterOrderCreated,
        onAmountChanged: onAmountChanged
    });
})(window.ShoppingCart, window.jQuery);
