// wwwroot/view-resources/ShoppingCart/shopping-cart.shipping.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Shipping = cart.Shipping || {};

    function Step2Monitor() {
        S.shipMethodsChosen = cart.Forms.FormCheck(S.ShippingForms);
        S.payMethodsChosen = cart.Forms.FormCheck(S.PaymentForms);

        if (!(S.shipMethodsChosen && S.payMethodsChosen)) {
            Coker.sweet.warning("請注意", "請確實選擇運送及付款方式！", null);
        } else {
            S.buy_step_swiper.slideNext();
        }

        S.buy_step_swiper.update();
    }
    function RadioShipping() {
        var $this = $("[name='RadioShipping']:checked");

        S.ori_freight = Number($this.data("freight") || 0);
        S.low_con = Number($this.data("lowcon") || 0);
        S.disfreight = Number($this.data("disfreight") || 0);
        S.freightType = Number($this.data("freight-type") || 0);

        var rawDiscountFreightType = $this.attr("data-discount-freight-type");
        S.discountFreightType = rawDiscountFreightType === "" || rawDiscountFreightType == null
            ? null
            : Number(rawDiscountFreightType);

        var rawBoxFees = $this.attr("data-boxfees");
        try {
            S.boxFees = rawBoxFees ? JSON.parse(rawBoxFees) : [];
        } catch (err) {
            console.error("RadioShipping data-boxfees parse failed", err, rawBoxFees);
            S.boxFees = [];
        }

        S.freight = S.ori_freight;

        var oldamount = $(".summary-amount.total_amount").first().text();
        cart.Pricing.TotalCount();
        var newamount = $(".summary-amount.total_amount").first().text();
        var isAmountChanged = oldamount != newamount;

        var this_SupportCashOnDelivery = $this.attr("data-support-cash-on-delivery").toLowerCase() == "true";
        var isSupportCashOnDeliveryChanged = S.SupportCashOnDelivery != this_SupportCashOnDelivery;
        S.SupportCashOnDelivery = this_SupportCashOnDelivery;

        if (isAmountChanged || isSupportCashOnDeliveryChanged) {
            cart.Payment.Core.onAmountChanged();
        }

        if (!this_SupportCashOnDelivery && (isAmountChanged || isSupportCashOnDeliveryChanged)) {
            cart.Payment.Core.reloadActiveEmbeddedProvider();
        } else if (isSupportCashOnDeliveryChanged) {
            cart.Payment.Core.clearProvidersByType("embedded");
            cart.Shipping.ConfigurePaymentOptions(null);
        }
    }
    function ConfigurePaymentOptions(val) {
        var $CheckedShipping = $('input[name="RadioShipping"]:checked');
        if ($CheckedShipping.length == 0) return;

        var canCashOnDelivery =
            $CheckedShipping.attr("data-support-cash-on-delivery").toLowerCase() === "true";

        // 必須在取消 radio 前保留原本選取值。
        // ECPay 尚未 ready 時，reloadActiveEmbeddedProvider()
        // 仍需要靠這個 radio 找到 active provider。
        var selectedPaymentValue =
            val != null && val !== ""
                ? String(val)
                : String(cart.Payment.Core.GetCheckedPaymentValue() || "");

        $("#RadioPayment > .form-check").addClass("d-none");
        $(".noPaymentWarning").addClass("d-none");
        $(".ecpayWarning").removeClass("d-none");
        $("#RadioPayment input:radio").prop("checked", false);
        $("#RadioPayment > .form-check > .payment_display").removeClass("checked first last");

        if (canCashOnDelivery) {
            $(".ecpayWarning").addClass("d-none");

            var $codPayment = $("#RadioPayment input[value='28']");

            if ($codPayment.length) {
                $codPayment.prop("checked", true);

                var $formCheck = $codPayment.closest(".form-check");
                $formCheck.removeClass("d-none");
                $formCheck.find(".payment_display").addClass("checked first last");
            } else {
                var $warning = $(".noPaymentWarning");

                if (!$warning.length) {
                    $warning = $("<div>", {
                        class: "noPaymentWarning",
                        text: "店家尚未設定對應的付款方式"
                    }).appendTo("#RadioPayment");
                }

                $warning.removeClass("d-none");
            }

            $(".ecpay_loading").addClass("d-none");
            return;
        }

        var $list = $("#RadioPayment > .form-check");

        // 先顯示所有付款方式
        $list.removeClass("d-none");

        // 非貨到付款情境，不顯示貨到付款
        $list.has("input[value='28']").addClass("d-none");

        $list.each(function () {
            var $formCheck = $(this);
            var $input = $formCheck.find('input[name="RadioPayment"]').first();

            if (cart.Payment.Core.isEmbeddedPaymentRadio($input)) {
                $formCheck.addClass("d-none");
            }
        });

        $(".ecpayWarning").addClass("d-none");

        var $targetInput = $();

        if (selectedPaymentValue !== "") {
            $targetInput = $(`#RadioPayment input[name="RadioPayment"][value="${selectedPaymentValue}"]`);
        }

        var $targetFormCheck = $targetInput.closest(".form-check");

        // 如果 val 是 embedded 付款，允許它維持 checked，但 radio 本身仍然隱藏。
        // 真正顯示的是 provider 自己的付款 UI。
        if ($targetInput.length && cart.Payment.Core.isEmbeddedPaymentRadio($targetInput)) {
            cart.Payment.Core.updatePaymentRadioUI($targetFormCheck);
        } else {
            // 一般付款方式：如果 val 不存在，或 val 對應到隱藏項，就改選第一個可見付款方式
            if (!$targetFormCheck.length || $targetFormCheck.hasClass("d-none")) {
                $targetFormCheck = $("#RadioPayment > .form-check:not(.d-none)").first();
            }

            if ($targetFormCheck.length) {
                cart.Payment.Core.updatePaymentRadioUI($targetFormCheck);
            }
        }

        $(".ecpay_loading").addClass("d-none");
    }
    function getSelectedShippingMeta() {
        var $selected = $("[name='RadioShipping']:checked");
        if (!$selected.length) {
            return {
                id: 0,
                freightType: 0,
                discountFreightType: null,
                freight: 0,
                lowCon: 0,
                disFreight: 0,
                boxFees: []
            };
        }

        var rawBoxFees = $selected.attr("data-boxfees");
        var parsedBoxFees = [];

        if (rawBoxFees) {
            try {
                parsedBoxFees = JSON.parse(rawBoxFees);
            } catch (err) {
                console.error("data-boxfees parse failed", err, rawBoxFees);
                parsedBoxFees = [];
            }
        }

        var rawDiscountFreightType = $selected.attr("data-discount-freight-type");

        return {
            id: Number($selected.val() || 0),
            freightType: Number($selected.data("freight-type") || 0),
            discountFreightType: rawDiscountFreightType === "" || rawDiscountFreightType == null
                ? null
                : Number(rawDiscountFreightType),
            freight: Number($selected.data("freight") || 0),
            lowCon: Number($selected.data("lowcon") || 0),
            disFreight: Number($selected.data("disfreight") || 0),
            boxFees: Array.isArray(parsedBoxFees) ? parsedBoxFees : []
        };
    }
    function calculateDiscountTargetFreight(baseFreight, shippingMeta) {
        var originFreight = Number(baseFreight || 0);
        var disFreight = Number(shippingMeta.disFreight || 0);
        var discountType = shippingMeta.discountFreightType == null
            ? null
            : Number(shippingMeta.discountFreightType);

        if (discountType == null) {
            return originFreight;
        }

        // 1 = 指定折抵後運費：Dis_Freight 本身就是最終運費
        if (discountType === 1) {
            return Math.max(0, disFreight);
        }

        // 2 = 折抵固定運費金額：Dis_Freight 是折抵金額，所以要從原運費扣掉
        if (discountType === 2) {
            return Math.max(0, originFreight - Math.max(0, disFreight));
        }

        return originFreight;
    }
    function shouldShowShippingShortage(freightResult) {
        var shippingMeta = cart.Shipping.getSelectedShippingMeta();

        if (shippingMeta.discountFreightType == null) {
            return false;
        }

        return Number(freightResult.shortage || 0) > 0;
    }
    function applyDiscountFreight(baseFreight, productSubtotal, shippingMeta) {
        var originFreight = Number(baseFreight || 0);
        var freeThreshold = Number(shippingMeta.lowCon || 0);
        var disFreight = Number(shippingMeta.disFreight || 0);
        var discountType = shippingMeta.discountFreightType == null
            ? null
            : Number(shippingMeta.discountFreightType);

        // 沒有門檻 / 未達門檻 / 未設定折抵方式 => 原運費
        if (freeThreshold <= 0 || productSubtotal < freeThreshold || discountType == null) {
            return originFreight;
        }

        // 1 = 指定折抵後運費
        if (discountType === 1) {
            return Math.max(0, disFreight);
        }

        // 2 = 折抵固定運費金額
        if (discountType === 2) {
            return Math.max(0, originFreight - Math.max(0, disFreight));
        }

        return originFreight;
    }
    function getSelectedPackingPoint() {
        var items = cart.Items.getSelectedCartItems();
        return items.reduce(function (sum, item) {
            var point = Number(item.PackingPoint || 0);
            var qty = Number(item.Quantity || 0);
            return sum + (point * qty);
        }, 0);
    }
    function calculateNormalFreight(productSubtotal, shippingMeta) {
        var originFreight = Number(shippingMeta.freight || 0);
        var freeThreshold = Number(shippingMeta.lowCon || 0);

        if (freeThreshold > 0 && productSubtotal >= freeThreshold) {
            return cart.Shipping.calculateDiscountTargetFreight(originFreight, shippingMeta);
        }

        return originFreight;
    }
    function calculateBoxFreightByTotalCapacity(totalPackingPoint, fees) {
        var list = (fees || [])
            .map(function (x) {
                return {
                    logisticsBoxId: Number(x.LogisticsBoxId || x.logisticsBoxId || 0),
                    name: x.Name || x.name || "",
                    capacityPoint: Number(
                        x.CapacityPoint ??
                        x.capacityPoint ??
                        0
                    ),
                    fee: Number(
                        x.Fee ??
                        x.fee ??
                        0
                    )
                };
            })
            .filter(function (x) {
                return x.capacityPoint > 0 && x.fee >= 0;
            })
            .sort(function (a, b) {
                if (a.capacityPoint !== b.capacityPoint) return a.capacityPoint - b.capacityPoint;
                return a.fee - b.fee;
            });

        if (!list.length || totalPackingPoint <= 0) {
            return {
                freight: 0,
                boxes: [],
                totalPackingPoint: totalPackingPoint || 0
            };
        }

        // 單位容量成本最低的箱型優先
        var bestUnitBox = list[0];
        var bestUnitCost = bestUnitBox.fee / bestUnitBox.capacityPoint;

        for (var i = 1; i < list.length; i++) {
            var unitCost = list[i].fee / list[i].capacityPoint;
            if (unitCost < bestUnitCost) {
                bestUnitCost = unitCost;
                bestUnitBox = list[i];
            }
        }

        var remaining = totalPackingPoint;
        var usedBoxes = [];
        var totalFreight = 0;

        if (remaining > bestUnitBox.capacityPoint) {
            var count = Math.floor(remaining / bestUnitBox.capacityPoint);
            if (count > 0) {
                usedBoxes.push({
                    logisticsBoxId: bestUnitBox.logisticsBoxId,
                    name: bestUnitBox.name,
                    capacityPoint: bestUnitBox.capacityPoint,
                    fee: bestUnitBox.fee,
                    count: count
                });
                totalFreight += count * bestUnitBox.fee;
                remaining -= count * bestUnitBox.capacityPoint;
            }
        }

        if (remaining > 0) {
            var fitBox = list.find(function (x) {
                return x.capacityPoint >= remaining;
            });

            if (!fitBox) {
                fitBox = list[list.length - 1];
                var extraCount = Math.ceil(remaining / fitBox.capacityPoint);
                totalFreight += extraCount * fitBox.fee;
                usedBoxes.push({
                    logisticsBoxId: fitBox.logisticsBoxId,
                    name: fitBox.name,
                    capacityPoint: fitBox.capacityPoint,
                    fee: fitBox.fee,
                    count: extraCount
                });
            } else {
                totalFreight += fitBox.fee;
                usedBoxes.push({
                    logisticsBoxId: fitBox.logisticsBoxId,
                    name: fitBox.name,
                    capacityPoint: fitBox.capacityPoint,
                    fee: fitBox.fee,
                    count: 1
                });
            }
        }

        var merged = [];
        usedBoxes.forEach(function (box) {
            var exists = merged.find(function (x) {
                return x.logisticsBoxId === box.logisticsBoxId;
            });
            if (exists) {
                exists.count += box.count;
            } else {
                merged.push({
                    logisticsBoxId: box.logisticsBoxId,
                    name: box.name,
                    capacityPoint: box.capacityPoint,
                    fee: box.fee,
                    count: box.count
                });
            }
        });

        return {
            freight: totalFreight,
            boxes: merged,
            totalPackingPoint: totalPackingPoint
        };
    }
    function calculateFreight(productSubtotal) {
        var shippingMeta = cart.Shipping.getSelectedShippingMeta();

        var lowConValue = Number(shippingMeta.lowCon || 0);
        var shortage = 0;

        if (lowConValue > 0 && productSubtotal < lowConValue) {
            shortage = lowConValue - productSubtotal;
        }

        var canApplyDiscount =
            lowConValue > 0 &&
            productSubtotal >= lowConValue &&
            shippingMeta.discountFreightType != null;

        switch (shippingMeta.freightType) {
            case 1: // 免運費
                return {
                    freight: 0,
                    mode: 1,
                    shortage: 0,
                    boxResult: null,
                    originFreight: 0,
                    targetFreight: 0
                };

            case 3: { // 依箱計費
                var totalPackingPoint = cart.Shipping.getSelectedPackingPoint();
                var boxResult = cart.Shipping.calculateBoxFreightByTotalCapacity(totalPackingPoint, shippingMeta.boxFees);

                var boxOriginFreight = Number(boxResult.freight || 0);

                var boxTargetFreight = canApplyDiscount
                    ? cart.Shipping.calculateDiscountTargetFreight(boxOriginFreight, shippingMeta)
                    : boxOriginFreight;

                return {
                    freight: Number(boxTargetFreight || 0),
                    mode: 3,
                    shortage: shortage,
                    boxResult: boxResult,
                    originFreight: boxOriginFreight,
                    targetFreight: boxTargetFreight
                };
            }

            case 2: // 單筆計算
            default: {
                var originFreight = Number(shippingMeta.freight || 0);

                var targetFreight = canApplyDiscount
                    ? cart.Shipping.calculateDiscountTargetFreight(originFreight, shippingMeta)
                    : originFreight;

                return {
                    freight: Number(targetFreight || 0),
                    mode: 2,
                    shortage: shortage,
                    boxResult: null,
                    originFreight: originFreight,
                    targetFreight: targetFreight
                };
            }
        }
    }

    Object.assign(cart.Shipping, {
        Step2Monitor: Step2Monitor,
        RadioShipping: RadioShipping,
        ConfigurePaymentOptions: ConfigurePaymentOptions,
        getSelectedShippingMeta: getSelectedShippingMeta,
        calculateDiscountTargetFreight: calculateDiscountTargetFreight,
        shouldShowShippingShortage: shouldShowShippingShortage,
        applyDiscountFreight: applyDiscountFreight,
        getSelectedPackingPoint: getSelectedPackingPoint,
        calculateNormalFreight: calculateNormalFreight,
        calculateBoxFreightByTotalCapacity: calculateBoxFreightByTotalCapacity,
        calculateFreight: calculateFreight
    });
})(window.ShoppingCart, window.jQuery);
