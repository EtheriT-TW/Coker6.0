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
    function UpdateRecipientAddressRequirement() {
        var $selected = $("[name='RadioShipping']:checked");
        var isCvs = String($selected.attr("data-is-cvs") || "").toLowerCase() === "true";
        var $addressBlock = $("#Recipient_TWzipcode");
        var $addressFields = $addressBlock.find("select, #RecipientInputAddress");

        $addressBlock.toggleClass("d-none", isCvs);
        $addressFields.prop("required", !isCvs);
        UpdateCvsStoreSelectionDisplay();

        if (S.buy_step_swiper) {
            S.buy_step_swiper.update();
        }
    }
    function IsCvsShippingSelected() {
        var $selected = $("[name='RadioShipping']:checked");
        return $selected.length > 0 &&
            String($selected.attr("data-is-cvs") || "").toLowerCase() === "true";
    }
    function GetSelectedPaymentRadio() {
        var activeValue = cart.Payment.Core.getActivePaymentValue();
        var $radio = activeValue == null || activeValue === ""
            ? $()
            : $('#RadioPayment input[name="RadioPayment"][value="' + activeValue + '"]').first();

        return $radio.length
            ? $radio
            : $('#RadioPayment input[name="RadioPayment"]:checked').first();
    }
    function PaymentGatewaySelectsCvsStore() {
        var $payment = GetSelectedPaymentRadio();
        return $payment.length > 0 &&
            Number($payment.attr("data-cvs-store-selection-mode") || 0) === 1;
    }
    function RequiresMerchantCvsStore() {
        if (!IsCvsShippingSelected()) return false;

        var $payment = GetSelectedPaymentRadio();
        return $payment.length > 0 && !PaymentGatewaySelectsCvsStore();
    }
    function HasSelectedCvsStore() {
        if (!RequiresMerchantCvsStore()) return true;

        var $selected = $("[name='RadioShipping']:checked");
        return $.trim($selected.attr("data-cvsstoreid") || "") !== "" &&
            $.trim($selected.attr("data-cvsstorename") || "") !== "" &&
            $.trim($selected.attr("data-cvsaddress") || "") !== "";
    }
    function UpdateCvsStoreSelectionDisplay() {
        var $section = $("#CvsStoreSelection");
        var $button = $section.find(".btn_getmap").first();
        var $status = $section.find(".cvs-store-status").first();
        var $shipping = $("[name='RadioShipping']:checked");
        var $payment = GetSelectedPaymentRadio();

        $button.addClass("d-none").removeClass("is-missing is-complete").removeAttr("aria-invalid");
        $status.removeClass("d-none is-missing is-complete is-gateway").empty();

        if (!IsCvsShippingSelected()) {
            $section.addClass("d-none");
        } else if (!$payment.length) {
            $section.removeClass("d-none");
            $status.text("請先選擇付款方式，再確認取貨門市的選擇流程。");
        } else if (PaymentGatewaySelectsCvsStore()) {
            var paymentTitle = $.trim($payment.attr("data-title") || "付款平台");
            $section.removeClass("d-none");
            $status.addClass("is-gateway")
                .text("取貨門市將於「" + paymentTitle + "」付款頁面選擇，本站不會重複要求選店。");
        } else {
            var storeName = $.trim($shipping.attr("data-cvsstorename") || "");
            var storeAddress = $.trim($shipping.attr("data-cvsaddress") || "");
            var hasStore = $.trim($shipping.attr("data-cvsstoreid") || "") !== "" &&
                storeName !== "" && storeAddress !== "";

            $section.removeClass("d-none");
            $button.removeClass("d-none");

            if (!hasStore) {
                $button.val("請選擇取貨門市");
                if (S.CvsStoreValidationRequested === true) {
                    $button.addClass("is-missing").attr("aria-invalid", "true");
                    $status.addClass("is-missing").text("尚未選擇門市，完成後才能結帳。");
                } else {
                    $status.text("請選擇本次訂單的取貨門市。");
                }
            } else {
                var storeButtonText = /門市$/.test(storeName) ? storeName : storeName + "門市";
                $button.val(storeButtonText)
                    .attr("title", "變更取貨門市：" + storeName)
                    .addClass("is-complete");
                $status.addClass("d-none is-complete");
            }
        }

        if (cart.Recipients && typeof cart.Recipients.RefreshDisplay === "function") {
            cart.Recipients.RefreshDisplay();
        }
    }
    function GetCvsStoreSelectionTarget() {
        var $button = $("#CvsStoreSelection .btn_getmap").first();
        return $button.length ? $button[0] : $("[name='RadioShipping']:checked")[0];
    }
    function RadioShipping() {
        var $this = $("[name='RadioShipping']:checked");
        S.CvsStoreValidationRequested = false;

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
            S.boxFees = [];
        }

        S.freight = S.ori_freight;

        UpdateRecipientAddressRequirement();
        cart.Pricing.TotalCount();
        cart.Payment.Core.onAmountChanged();
    }
    function ConfigurePaymentOptions(val) {
        var $CheckedShipping = $('input[name="RadioShipping"]:checked');
        if ($CheckedShipping.length == 0) return;

        if (!cart.Payment.Availability) return;

        if (!S.PaymentAvailabilityLoaded) {
            cart.Payment.Availability.refresh(val);
            return;
        }

        cart.Payment.Availability.apply(val);
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
        UpdateRecipientAddressRequirement: UpdateRecipientAddressRequirement,
        UpdateCvsStoreSelectionDisplay: UpdateCvsStoreSelectionDisplay,
        IsCvsShippingSelected: IsCvsShippingSelected,
        PaymentGatewaySelectsCvsStore: PaymentGatewaySelectsCvsStore,
        RequiresMerchantCvsStore: RequiresMerchantCvsStore,
        HasSelectedCvsStore: HasSelectedCvsStore,
        GetCvsStoreSelectionTarget: GetCvsStoreSelectionTarget,
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
