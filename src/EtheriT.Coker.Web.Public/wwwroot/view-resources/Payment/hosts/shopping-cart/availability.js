// Shopping-cart host adapter for payment availability.
(function (cart, $) {
    "use strict";

    var S = cart.State;

    cart.Payment = cart.Payment || {};
    cart.Payment.Availability = cart.Payment.Availability || {};

    function getSelectedPaymentValue() {
        return String(cart.Payment.Core.GetCheckedPaymentValue() || "");
    }

    function getEmbeddedProviders() {
        return cart.Payment.Core.getProviders().filter(function (provider) {
            return provider && provider.type === "embedded";
        });
    }

    function applyEmbeddedAvailability(items) {
        return getEmbeddedProviders().map(function (provider) {
            if (typeof provider.applyAvailability === "function") {
                var state = provider.applyAvailability(items) || {};
                state.provider = provider;
                state.items = state.items || [];
                state.availableItems = state.availableItems || [];
                state.available = state.available === true;
                state.entry = state.entry || $();
                return state;
            }

            var providerItems = typeof provider.getAvailabilityItems === "function"
                ? provider.getAvailabilityItems(items)
                : [];
            var availableItems = providerItems.filter(function (item) {
                return item.isAvailable === true;
            });

            return {
                provider: provider,
                items: providerItems,
                availableItems: availableItems,
                available: availableItems.length > 0,
                entry: typeof provider.getEntryRadio === "function"
                    ? provider.getEntryRadio()
                    : $()
            };
        });
    }

    function getEmbeddedStateByProvider(states, provider) {
        return (states || []).find(function (state) {
            return state.provider === provider;
        }) || null;
    }

    function getEmbeddedStateByPaymentValue(states, value) {
        var target = String(value || "");

        return (states || []).find(function (state) {
            return (state.items || []).some(function (item) {
                return String(item.id) === target;
            });
        }) || null;
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
                .attr("data-provider-code", payment.providerCode || "")
                .attr("data-render-mode", payment.renderMode || "")
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
            $fallback.insertBefore("#EmbeddedPayment");
        });
    }

    function reloadAvailableEmbeddedProviders() {
        var requests = [];

        getEmbeddedProviders().forEach(function (provider) {
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

    function apply(preferredPaymentValue) {
        var payments = S.AvailablePayments || [];
        var available = payments.filter(function (item) {
            return item.isAvailable === true;
        });
        var allowedIds = new Set(available.map(function (item) {
            return String(item.id);
        }));
        var selectedValue = preferredPaymentValue != null && preferredPaymentValue !== ""
            ? String(preferredPaymentValue)
            : getSelectedPaymentValue();

        $("#RadioPayment input:radio").prop("checked", false);
        $("#RadioPayment > .form-check > .payment_display").removeClass("checked first last");

        $("#RadioPayment > .form-check").each(function () {
            var $formCheck = $(this);
            var $input = $formCheck.find('input[name="RadioPayment"]').first();
            var isEmbedded =
                String($input.attr("data-render-mode") || "").toLowerCase() === "embedded" ||
                cart.Payment.Core.isEmbeddedPaymentRadio($input);
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

        var embeddedStates = applyEmbeddedAvailability(payments);
        var selectedEmbeddedState = getEmbeddedStateByPaymentValue(
            embeddedStates,
            selectedValue
        );

        if (selectedEmbeddedState && selectedEmbeddedState.available) {
            selectedValue = String(selectedEmbeddedState.entry.val() || "");
        }

        var $target = selectedValue
            ? $('#RadioPayment > .form-check:not(.payment-embedded-unavailable) input[name="RadioPayment"][value="' + selectedValue + '"]')
            : $();
        var $targetForm = $target.closest(".form-check");

        var targetIsEmbedded = $target.length &&
            cart.Payment.Core.isEmbeddedPaymentRadio($target);
        var targetProvider = targetIsEmbedded
            ? cart.Payment.Core.getProviderByRadio($target)
            : null;
        var targetEmbeddedState = getEmbeddedStateByProvider(
            embeddedStates,
            targetProvider
        );
        var targetIsAvailable = $target.length && (
            (targetIsEmbedded && targetEmbeddedState && targetEmbeddedState.available &&
                targetEmbeddedState.availableItems.some(function (item) {
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

        if (!$target.length) {
            var firstAvailableEmbedded = embeddedStates.find(function (state) {
                return state.available && state.entry && state.entry.length;
            });

            if (firstAvailableEmbedded) {
                $target = firstAvailableEmbedded.entry;
            }
        }

        if ($target.length) {
            cart.Payment.Core.updatePaymentRadioUI($target.closest(".form-check"));
        }

        var hasAvailablePayment =
            $('#RadioPayment > .form-check:not(.d-none) input[name="RadioPayment"]:not(:disabled)').length > 0 ||
            embeddedStates.some(function (state) {
                return state.available === true;
            });

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

            if (!S.isRestoringLogisticsSelection) {
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
        getEmbeddedPayments: function () {
            return (S.AvailablePayments || []).filter(function (item) {
                return String(item.renderMode || "").toLowerCase() === "embedded";
            });
        }
    });
})(window.ShoppingCart, window.jQuery);
