// Central payment availability for standard and embedded payment providers.
(function (cart, $) {
    "use strict";

    var S = cart.State;

    cart.Payment = cart.Payment || {};
    cart.Payment.Availability = cart.Payment.Availability || {};

    function getSelectedPaymentValue() {
        return String(cart.Payment.Core.GetCheckedPaymentValue() || "");
    }

    function getEcpayPayments(items) {
        return (items || []).filter(function (item) {
            return item.providerCode === "ECPay" && item.renderMode === "Embedded";
        });
    }

    function reloadAvailableEmbeddedProviders() {
        var requests = [];

        cart.Payment.Core.getProvidersByType("embedded").forEach(function (provider) {
            if (typeof provider.reload !== "function") return;

            var request = provider.reload();
            if (request && typeof request.always === "function") {
                requests.push(request);
            }
        });

        return requests;
    }

    function ensureNoPaymentWarning() {
        var $warning = $("#RadioPayment .noPaymentWarning");

        if (!$warning.length) {
            $warning = $("<div>", {
                class: "noPaymentWarning d-none",
                text: "目前選擇的物流方式或訂單金額沒有可用的付款方式"
            }).appendTo("#RadioPayment");
        }

        return $warning;
    }

    function updateEcpayEntry(ecpayPayments) {
        var provider = cart.Payment.Core.getProvider("ECPay");
        var $entry = provider && typeof provider.getEntryRadio === "function"
            ? provider.getEntryRadio()
            : $();

        S.ECPayAvailable = ecpayPayments.length > 0;
        S.HasECPay = S.ECPayAvailable && $entry.length > 0;

        if (!$entry.length) return;

        if (S.ECPayAvailable) {
            var first = ecpayPayments[0];
            $entry
                .val(first.id)
                .attr("data-code", first.code)
                .attr("data-minamount", first.minAmount)
                .attr("data-maxamount", first.maxAmount == null ? "" : first.maxAmount);
        } else {
            $entry.prop("checked", false);
            if (provider && typeof provider.clear === "function") provider.clear();
            $(".ecpay_loading").addClass("d-none");
        }
    }

    function apply(preferredPaymentValue) {
        var available = S.AvailablePayments || [];
        var allowedIds = new Set(available.map(function (item) {
            return String(item.id);
        }));
        var ecpayPayments = getEcpayPayments(available);
        var selectedValue = preferredPaymentValue != null && preferredPaymentValue !== ""
            ? String(preferredPaymentValue)
            : getSelectedPaymentValue();

        $("#RadioPayment input:radio").prop("checked", false);
        $("#RadioPayment > .form-check > .payment_display").removeClass("checked first last");

        $("#RadioPayment > .form-check").each(function () {
            var $formCheck = $(this);
            var $input = $formCheck.find('input[name="RadioPayment"]').first();
            var isEmbedded = cart.Payment.Core.isEmbeddedPaymentRadio($input);
            var isAvailable = allowedIds.has(String($input.val()));

            $formCheck.toggleClass("d-none", isEmbedded || !isAvailable);
        });

        updateEcpayEntry(ecpayPayments);

        var provider = cart.Payment.Core.getProvider("ECPay");
        var $ecpayEntry = provider && typeof provider.getEntryRadio === "function"
            ? provider.getEntryRadio()
            : $();

        if (S.ECPayAvailable && ecpayPayments.some(function (item) {
            return String(item.id) === selectedValue;
        })) {
            selectedValue = String($ecpayEntry.val() || "");
        }

        var $target = selectedValue
            ? $('#RadioPayment input[name="RadioPayment"][value="' + selectedValue + '"]')
            : $();
        var $targetForm = $target.closest(".form-check");

        var targetIsEmbedded = $target.length &&
            cart.Payment.Core.isEmbeddedPaymentRadio($target);
        var targetIsAvailable = $target.length && (
            (targetIsEmbedded && S.ECPayAvailable) ||
            (!targetIsEmbedded &&
                !$targetForm.hasClass("d-none") &&
                allowedIds.has(String($target.val())))
        );

        if (!targetIsAvailable) {
            $target = $("#RadioPayment > .form-check:not(.d-none)")
                .first()
                .find('input[name="RadioPayment"]');
        }

        if (!$target.length && S.ECPayAvailable) {
            $target = $ecpayEntry;
        }

        if ($target.length) {
            cart.Payment.Core.updatePaymentRadioUI($target.closest(".form-check"));
        }

        var hasAvailablePayment =
            $("#RadioPayment > .form-check:not(.d-none)").length > 0 ||
            S.ECPayAvailable;

        ensureNoPaymentWarning().toggleClass("d-none", hasAvailablePayment);
        $(".ecpayWarning").toggleClass("d-none", hasAvailablePayment);

        cart.Payment.Core.RadioPayment();

        if (S.buy_step_swiper) S.buy_step_swiper.update();
    }

    function refresh(preferredPaymentValue) {
        var $shipping = $('input[name="RadioShipping"]:checked');

        if (!$shipping.length || !Coker.Payment ||
            typeof Coker.Payment.GetAvailablePayments !== "function") {
            return $.Deferred().resolve([]).promise();
        }

        cart.Pricing.TotalCount();

        var requestId = ++S.PaymentAvailabilityRequestId;
        var request = Coker.Payment.GetAvailablePayments({
            LogisticsSettingId: Number($shipping.val() || 0),
            Amount: Number(S.total || 0)
        });

        request.done(function (items) {
            if (requestId !== S.PaymentAvailabilityRequestId) return;

            S.AvailablePayments = Array.isArray(items) ? items : [];
            S.PaymentAvailabilityLoaded = true;
            apply(preferredPaymentValue);

            if (!S.isRestoringECPayLogistics) {
                reloadAvailableEmbeddedProviders();
            }
        });

        request.fail(function () {
            if (requestId !== S.PaymentAvailabilityRequestId) return;

            S.AvailablePayments = [];
            S.PaymentAvailabilityLoaded = true;
            apply(null);
        });

        return request;
    }

    function scheduleRefresh(preferredPaymentValue) {
        if (S.PaymentAvailabilityTimer != null) {
            clearTimeout(S.PaymentAvailabilityTimer);
        }

        S.PaymentAvailabilityTimer = setTimeout(function () {
            S.PaymentAvailabilityTimer = null;
            refresh(preferredPaymentValue);
        }, 100);
    }

    function isAvailable(paymentTypeId) {
        return (S.AvailablePayments || []).some(function (item) {
            return Number(item.id) === Number(paymentTypeId);
        });
    }

    Object.assign(cart.Payment.Availability, {
        apply: apply,
        refresh: refresh,
        scheduleRefresh: scheduleRefresh,
        isAvailable: isAvailable,
        getEcpayPayments: function () {
            return getEcpayPayments(S.AvailablePayments || []);
        }
    });
})(window.ShoppingCart, window.jQuery);
