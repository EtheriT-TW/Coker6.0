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
            return item.providerCode === "ECPay" &&
                item.renderMode === "Embedded" &&
                item.isAvailable === true;
        });
    }

    function getPaymentById(items, paymentTypeId) {
        var targetId = String(paymentTypeId || "");

        return (items || []).find(function (item) {
            return String(item.id) === targetId;
        }) || null;
    }

    function isUnsupportedByLogistics(payment) {
        return payment != null &&
            String(payment.unavailableReasonCode || "") === "UnsupportedByLogistics";
    }

    function updateUnavailableDisplay($formCheck, payment) {
        var isUnavailable = payment != null && payment.isAvailable !== true;
        var reason = isUnavailable
            ? String(payment.unavailableReason || "目前無法使用此付款方式。")
            : "";
        var $input = $formCheck.find('input[name="RadioPayment"]').first();
        var $display = $formCheck.find(".payment_display").first();
        var $reason = $formCheck.find(".payment-unavailable-reason").first();

        $formCheck.toggleClass("payment-unavailable", isUnavailable);
        $input.prop("disabled", isUnavailable);
        $display
            .attr("aria-disabled", isUnavailable ? "true" : "false")
            .attr("title", reason);
        $reason
            .text(reason)
            .toggleClass("d-none", !isUnavailable);
    }

    function renderUnavailableEmbeddedPayments(payments) {
        $("#RadioPayment > .payment-embedded-unavailable").remove();

        (payments || []).filter(function (item) {
            return item.renderMode === "Embedded" &&
                item.isAvailable !== true &&
                !isUnsupportedByLogistics(item);
        }).forEach(function (payment) {
            var $fallback = $("<div>", {
                class: "form-check text-start m-0 p-0 payment-embedded-unavailable"
            });
            var $input = $("<input>", {
                class: "form-check-input d-none",
                type: "radio",
                name: "RadioPayment",
                value: payment.id,
                disabled: true
            })
                .attr("data-availability-id", payment.id)
                .attr("data-third-party-id", payment.thirdPartyId || 0)
                .attr("data-code", payment.code || "")
                .attr("data-cvs-store-selection-mode", payment.cvsStoreSelectionMode || 0)
                .attr("data-title", payment.title || "");
            var $display = $("<div>", {
                class: "payment_display d-flex justify-content-between"
            });
            var $content = $("<div>", {
                class: "d-flex align-items-center"
            });
            var $copy = $("<div>", {
                class: "payment-copy"
            });

            $("<span>", {
                class: "paymentradio d-block"
            }).appendTo($content);
            $("<div>", {
                class: "paymenttitle",
                text: payment.title || "付款方式"
            }).appendTo($copy);
            $("<div>", {
                class: "payment-unavailable-reason small d-none",
                role: "note"
            }).appendTo($copy);

            $copy.appendTo($content);
            $content.appendTo($display);

            if (payment.icon) {
                $("<img>", {
                    class: "paymenticon px-1",
                    src: payment.icon,
                    alt: payment.title || "付款方式"
                }).appendTo($display);
            }

            $fallback.append($input, $display);

            updateUnavailableDisplay($fallback, payment);
            $fallback.insertBefore("#ECPayPayment");
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
                .attr("data-cvs-store-selection-mode", first.cvsStoreSelectionMode || 0)
                .attr("data-minamount", first.minAmount)
                .attr("data-maxamount", first.maxAmount == null ? "" : first.maxAmount);
        } else {
            $entry.prop("checked", false);
            if (provider && typeof provider.clear === "function") provider.clear();
            $(".ecpay_loading").addClass("d-none");
        }
    }

    function apply(preferredPaymentValue) {
        var payments = S.AvailablePayments || [];
        var available = payments.filter(function (item) {
            return item.isAvailable === true;
        });
        var allowedIds = new Set(available.map(function (item) {
            return String(item.id);
        }));
        var ecpayPayments = getEcpayPayments(payments);
        var selectedValue = preferredPaymentValue != null && preferredPaymentValue !== ""
            ? String(preferredPaymentValue)
            : getSelectedPaymentValue();

        $("#RadioPayment input:radio").prop("checked", false);
        $("#RadioPayment > .form-check > .payment_display").removeClass("checked first last");

        $("#RadioPayment > .form-check").each(function () {
            var $formCheck = $(this);
            var $input = $formCheck.find('input[name="RadioPayment"]').first();
            var isEmbedded = cart.Payment.Core.isEmbeddedPaymentRadio($input);
            var availabilityId = $input.attr("data-availability-id") || $input.val();
            var payment = getPaymentById(payments, availabilityId);

            if (isEmbedded) {
                // Keep the provider entry radios untouched: the SDK uses the first one
                // as its selection proxy. Disabled embedded methods are rendered below
                // as display-only fallback rows instead.
                $formCheck.addClass("d-none");
                return;
            }

            updateUnavailableDisplay($formCheck, payment);
            $formCheck.toggleClass(
                "d-none",
                payment == null || isUnsupportedByLogistics(payment)
            );
        });

        renderUnavailableEmbeddedPayments(payments);

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
            ? $('#RadioPayment > .form-check:not(.payment-embedded-unavailable) input[name="RadioPayment"][value="' + selectedValue + '"]')
            : $();
        var $targetForm = $target.closest(".form-check");

        var targetIsEmbedded = $target.length &&
            cart.Payment.Core.isEmbeddedPaymentRadio($target);
        var targetIsAvailable = $target.length && (
            (targetIsEmbedded && S.ECPayAvailable && ecpayPayments.some(function (item) {
                return String(item.id) === String($target.val());
            })) ||
            (!targetIsEmbedded &&
                !$targetForm.hasClass("d-none") &&
                allowedIds.has(String($target.val())))
        );

        if (!targetIsAvailable) {
            $target = $('#RadioPayment > .form-check:not(.d-none) input[name="RadioPayment"]:not(:disabled)')
                .first();
        }

        if (!$target.length && S.ECPayAvailable) {
            $target = $ecpayEntry;
        }

        if ($target.length) {
            cart.Payment.Core.updatePaymentRadioUI($target.closest(".form-check"));
        }

        var hasAvailablePayment =
            $('#RadioPayment > .form-check:not(.d-none) input[name="RadioPayment"]:not(:disabled)').length > 0 ||
            S.ECPayAvailable;

        ensureNoPaymentWarning().toggleClass("d-none", hasAvailablePayment);

        cart.Payment.Core.RadioPayment();
        cart.Shipping.UpdateCvsStoreSelectionDisplay();

        if (cart.CheckoutValidation &&
            typeof cart.CheckoutValidation.RefreshDisplay === "function") {
            cart.CheckoutValidation.RefreshDisplay();
        }

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
            return Number(item.id) === Number(paymentTypeId) &&
                item.isAvailable === true;
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
