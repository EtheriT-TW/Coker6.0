// wwwroot/view-resources/ShoppingCart/shopping-cart.pricing.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Pricing = cart.Pricing || {};

function computeSelectedSubtotal() {
    let sum = 0, bonus = 0, discountEligibleSum = 0;
    $('.purchase_group li.purchase_item input[name="buyItems"]:checked').each(function () {
        const $li = $(this).closest('li.purchase_item');
        const $sub = $li.find('[data-key="subtotal"]');
        sum += Number($sub.data('subtotal') || 0);
        bonus += Number($sub.data('subtotal_bonus') || 0);
        const scId = Number($li.data('scId'));
        const stateItem = (S.shopping_cart_data || []).find(function (item) { return Number(item.Id) === scId; });
        if (!stateItem || stateItem.IsAdditional !== true) {
            discountEligibleSum += Number($sub.data('subtotal') || 0);
        }
    });
    return { sum, bonus, discountEligibleSum };
}
function TotalCount() {
    const { sum, bonus, discountEligibleSum } = cart.Pricing.computeSelectedSubtotal();
    const rewardAmount = cart.Marketing && typeof cart.Marketing.refreshRewardCampaigns === "function"
        ? Number(cart.Marketing.refreshRewardCampaigns() || 0)
        : 0;

    // 商品原始小計
    S.subtotal = Number(sum || 0) + rewardAmount;
    S.order_data.bonus = Number(bonus || 0);

    var showGeneralProductAmount = S.subtotal > Number(discountEligibleSum || 0);
    $(".generalProductAmountLine").toggleClass("d-none", !showGeneralProductAmount);
    $(".generalProductAmount").text(showGeneralProductAmount
        ? Number(discountEligibleSum || 0).toLocaleString()
        : "");

    // 行銷活動折扣：只影響畫面試算，不作為正式訂單依據
    var marketingDiscountResult = cart.Marketing && typeof cart.Marketing.calculateOrderDiscount === "function"
        ? cart.Marketing.calculateOrderDiscount(Number(discountEligibleSum || 0))
        : { discountAmount: 0, memo: "" };

    var marketingDiscount = Number(marketingDiscountResult.discountAmount || 0);
    marketingDiscount = Math.max(0, Math.min(marketingDiscount, S.subtotal));

    // 確保畫面顯示與後續計算都使用 clamp 後金額
    marketingDiscountResult.discountAmount = marketingDiscount;

    if (cart.Marketing && typeof cart.Marketing.renderOrderDiscount === "function") {
        cart.Marketing.renderOrderDiscount(marketingDiscountResult);
    }

    var subtotalAfterMarketing = Math.max(0, S.subtotal - marketingDiscount);

    // 同步目前 shipping meta 到全域
    var shippingMeta = cart.Shipping.getSelectedShippingMeta();
    S.ori_freight = Number(shippingMeta.freight || 0);
    S.low_con = Number(shippingMeta.lowCon || 0);
    S.disfreight = Number(shippingMeta.disFreight || 0);
    S.freightType = Number(shippingMeta.freightType || 0);
    S.discountFreightType = shippingMeta.discountFreightType == null ? null : Number(shippingMeta.discountFreightType);
    S.boxFees = shippingMeta.boxFees || [];

    // 商品金額
    $(".subtotal").text(S.subtotal.toLocaleString());

    // 商品紅利（作為附註，不放進主計算列）
    const $bonusParts = $(".dual-price .bonus-part");
    const $priceBonus = $(".priceline .bonus");

    if ((bonus || 0) > 0) {
        $priceBonus.text(bonus.toLocaleString());
        $bonusParts.removeClass("d-none");
        $(".dual-price .plus-sign").removeClass("d-none");
    } else {
        $priceBonus.text("");
        $bonusParts.addClass("d-none");
        $(".dual-price .plus-sign").addClass("d-none");
    }

    // ===== 紅利折抵規則 / 本次折抵 =====
    const $bonusDisconLine = $(".bonusDiscionLine");
    const $bonusRuleLine = $(".bonusRuleLine");
    const $redeemRuleText = $(".bonusRedeemRuleText");

    function splitParentheticalDetail($elements, mainClass, detailClass) {
        $elements.each(function () {
            const $element = $(this);
            const parts = $element.text().match(/^(.*?)(\uFF08.*\uFF09)$/);
            if (parts) {
                $element.empty()
                    .append($("<span></span>").addClass(mainClass).text(parts[1]))
                    .append($("<span></span>").addClass(detailClass).text(parts[2]));
            }
        });
    }

    const redeemEnabled = (MinOrderForRedemption > 0 && MaxRedemptionPercent > 0);
    const maximumDiscountAmount = Number(MaximumDiscount);
    const hasMaximumDiscount = MaximumDiscount != null && maximumDiscountAmount > 0;
    const allowsFullProductRedemption = Number(MaxRedemptionPercent) >= 100;
    const redemptionLimitText = allowsFullProductRedemption
        ? (hasMaximumDiscount
            ? `單筆折抵上限 ${cart.Utils.formatMoney(maximumDiscountAmount)}`
            : "商品金額可全額折抵")
        : (hasMaximumDiscount
            ? `折抵上限 ${MaxRedemptionPercent}%，單筆上限 ${cart.Utils.formatMoney(maximumDiscountAmount)}`
            : `折抵上限 ${MaxRedemptionPercent}%`);

    let allBonus = Number(bonus || 0);
    let redeemAmount = 0;
    let payableSubtotal = subtotalAfterMarketing;

    // 預設全部清空（避免殘留）
    $bonusDisconLine.addClass("d-none");
    $bonusDisconLine.find(".summary-label").text("紅利折抵");
    $bonusDisconLine.find(".bonusDiscion").text("");

    $bonusRuleLine.addClass("d-none");
    $redeemRuleText.text("");

    // ===== 狀態判斷 =====
    if (redeemEnabled) {

        // 未達門檻
        if (payableSubtotal < MinOrderForRedemption) {

            const diff = MinOrderForRedemption - payableSubtotal;

            $redeemRuleText.text(
                `再消費 ${cart.Utils.formatMoney(diff)} 可使用紅利折抵（${redemptionLimitText}）`
            );

            $bonusRuleLine.removeClass("d-none");

            splitParentheticalDetail($redeemRuleText, "bonus-rule-main", "bonus-rule-detail");
        }

        // 已達門檻
        else {

            const percentageCap = Math.round(payableSubtotal * MaxRedemptionPercent / 100);
            const cap = hasMaximumDiscount
                ? Math.floor(Math.min(percentageCap, maximumDiscountAmount))
                : percentageCap;
            const memberBonusAmount = Math.max(0, (totalBonus || 0) - bonus);

            redeemAmount = Math.min(cap, memberBonusAmount);

            // ===== 有可折抵 =====
            if (redeemAmount > 0) {

                $bonusDisconLine.removeClass("d-none");

                // label 覆蓋
                $bonusDisconLine.find(".summary-label")
                    .text(`本單可使用紅利折抵（${redemptionLimitText}）`);
                splitParentheticalDetail(
                    $bonusDisconLine.find(".summary-label"),
                    "summary-label-main",
                    "summary-label-detail"
                );

                // 金額
                $bonusDisconLine.find(".bonusDiscion")
                    .text(`${redeemAmount.toLocaleString()} 點`);

                allBonus += redeemAmount;
                payableSubtotal = Math.max(0, payableSubtotal - redeemAmount);

                // 👉 若「被紅利上限卡住」才顯示提示
                if (memberBonusAmount < cap) {
                    $redeemRuleText.text(
                        `目前剩餘紅利 ${memberBonusAmount.toLocaleString()} 點，已全數折抵`
                    );
                    $bonusRuleLine.removeClass("d-none");
                }
            }

            // ===== 可折抵但實際=0（紅利不足）=====
            else {

                $redeemRuleText.text(
                    `目前剩餘紅利 ${memberBonusAmount.toLocaleString()} 點`
                );

                $bonusRuleLine.removeClass("d-none");
            }
        }
    }

    // Step1 小計（所有折抵後，不含運費）
    $(".payable_subtotal").text(parseInt(payableSubtotal, 10).toLocaleString());

    // ===== 紅利回饋提示 =====
    // 回饋紅利基準使用所有折抵後的商品小計，需與後端 BuildDetailSectionAsync 一致
    const $rewardRow = $(".bonusRedeemHintLine");
    const $earnText = $(".bonusEarnHintText");

    const fixedPointsReward = Number(RewardCalculationType) === 2 || RewardCalculationType === "FixedPoints";
    const rewardValue = fixedPointsReward
        ? Number(RewardFixedPoints || 0)
        : Number(RewardRatePercent || 0);
    const earnEnabled = MinOrderForEarnPoints > 0 && rewardValue > 0;
    // 後端 DetailBuildResult 會先將折抵後商品小計四捨五入為整數，再計算回饋。
    const rewardEligibleSubtotal = Math.round(payableSubtotal);

    function calculateBonusEarnPoints(eligibleSubtotal) {
        if (!earnEnabled || eligibleSubtotal < MinOrderForEarnPoints) {
            return 0;
        }

        if (!fixedPointsReward) {
            return Math.floor(eligibleSubtotal * RewardRatePercent / 100);
        }

        const fixedPoints = Math.floor(Number(RewardFixedPoints || 0));
        if (fixedPoints <= 0) {
            return 0;
        }

        const multiplier = RewardFixedPointsCumulative
            ? Math.floor(eligibleSubtotal / MinOrderForEarnPoints)
            : 1;

        return multiplier * fixedPoints;
    }

    function buildBonusEarnRuleText() {
        if (fixedPointsReward) {
            if (RewardFixedPointsCumulative) {
                return `每滿 ${cart.Utils.formatMoney(MinOrderForEarnPoints)}，` +
                    `贈送 ${Number(RewardFixedPoints || 0).toLocaleString()} 點紅利，運費不計。`;
            }

            return `商品滿 ${cart.Utils.formatMoney(MinOrderForEarnPoints)}，` +
                `固定贈送 ${Number(RewardFixedPoints || 0).toLocaleString()} 點紅利，運費不計。`;
        }

        return `商品滿 ${cart.Utils.formatMoney(MinOrderForEarnPoints)}，` +
            `依折抵後商品金額 ${Number(RewardRatePercent || 0).toLocaleString()}% 回饋，運費不計。`;
    }

    if (!earnEnabled) {
        $rewardRow.addClass("d-none");
        $earnText.text("");
    } else if (rewardEligibleSubtotal < MinOrderForEarnPoints) {
        const diff = MinOrderForEarnPoints - rewardEligibleSubtotal;

        $rewardRow.removeClass("d-none");
        $earnText.html(
            `<span class="bonus-earn-main">再消費 ${cart.Utils.formatMoney(diff)} 可獲得紅利回饋</span>` +
            `<span class="bonus-earn-rule d-block">${buildBonusEarnRuleText()}</span>`
        );
    } else {
        const earnPoints = calculateBonusEarnPoints(rewardEligibleSubtotal);

        if (earnPoints > 0) {
            $rewardRow.removeClass("d-none");
            $earnText.html(
                `<span class="bonus-earn-main">本單預計獲得 ${earnPoints.toLocaleString()} 點紅利</span>` +
                `<span class="bonus-earn-rule d-block">${buildBonusEarnRuleText()}</span>`
            );
        } else {
            $rewardRow.addClass("d-none");
            $earnText.text("");
        }
    }

    // ===== 運費 =====
    // 運費門檻使用所有折抵完成後的商品小計，需與後端 BuildDetailSectionAsync 一致
    var freightResult = cart.Shipping.calculateFreight(payableSubtotal);
    S.freight = Number(freightResult.freight || 0);

    $(".shipping_fee").text(S.freight.toLocaleString());

    // 運費提醒：單筆 / 箱型都支援，只要有門檻且未達成就顯示
    if (cart.Shipping.shouldShowShippingShortage(freightResult)) {
        $(".shipping_memo .price").text(Number(freightResult.shortage).toLocaleString());
        $(".shipping_memo").removeClass("d-none");
    } else {
        $(".shipping_memo").addClass("d-none");
        $(".shipping_memo .price").text("");
    }

    // ===== 箱型資訊 =====
    const $boxMemo = $(".shipping_box_memo");
    if ($boxMemo.length) {
        if (freightResult.mode === 3 && freightResult.boxResult && freightResult.boxResult.boxes.length > 0) {
            var boxText = freightResult.boxResult.boxes
                .map(function (x) {
                    return `${x.name} × ${x.count}`;
                })
                .join("、");

            $boxMemo.text(`(本次配箱：${boxText})`);
            $boxMemo.removeClass("d-none");
        } else {
            $boxMemo.text("");
            $boxMemo.addClass("d-none");
        }
    }

    // ===== 全單紅利使用總計 =====
    if (allBonus > 0) {
        $(".bonusUseTotalLine").removeClass("d-none");
        $(".bonusUseTotalValue").text(`${allBonus.toLocaleString()} 點`);
    } else {
        $(".bonusUseTotalLine").addClass("d-none");
        $(".bonusUseTotalValue").text("");
    }

    // ===== Step3 小計（所有折抵後 + 運費）=====
    S.total = payableSubtotal + S.freight;
    $(".total_amount").text(parseInt(S.total, 10).toLocaleString());
}
function setCartPriceBlock($target, cashText, bonusValue, mode) {
    const bonus = Number(bonusValue || 0);

    const $main = $target.find(".cart-price-main");
    const $bonus = $target.find(".cart-price-bonus");

    // ⭐ 預設 fallback：Step1（單行）
    if (!mode) mode = "inline";

    if (mode === "block") {
        // ===== Step4：上下分行 =====
        if ($main.length && $bonus.length) {
            $main.text(cashText || "");

            if (bonus > 0) {
                $bonus.text(`紅利：${bonus.toLocaleString()}`);
                $bonus.removeClass("d-none");
            } else {
                $bonus.text("");
                $bonus.addClass("d-none");
            }
        } else {
            // fallback
            if (bonus > 0) {
                $target.html(`${cashText}<br/>紅利：${bonus.toLocaleString()}`);
            } else {
                $target.text(cashText || "");
            }
        }
    } else {
        // ===== Step1：單行 =====
        if (bonus > 0) {
            $target.text(
                cashText
                    ? `${cashText} + 紅利${bonus.toLocaleString()}`
                    : `紅利${bonus.toLocaleString()}`
            );
        } else {
            $target.text(cashText || "");
        }
    }
}
function updateNextStepByBonus() {
    const { bonus } = cart.Pricing.computeSelectedSubtotal();
    const memberBonus = Number(totalBonus || 0);
    const shortage = Math.max(0, Number(bonus || 0) - memberBonus);
    const isEnough = shortage <= 0;
    const $nextBtn = $(".btn_swiper_next_buystep");
    const $hint = $(".js-bonus-nextstep-hint");
    const $hintText = $(".js-bonus-nextstep-text");

    // 按鈕控制
    $nextBtn.prop("disabled", !isEnough);
    $nextBtn.toggleClass("disabled", !isEnough);

    // 提示控制
    if ($hint.length && $hintText.length) {
        if (isEnough) {
            $hint.addClass("d-none");
            $hintText.text("");
        } else {
            $hint.removeClass("d-none");
            $hintText.text(
                `目前已選商品需紅利 ${Number(bonus || 0).toLocaleString()} 點，` +
                `會員可用紅利 ${memberBonus.toLocaleString()} 點，` +
                `尚差 ${shortage.toLocaleString()} 點。`
            );
        }
    }
}

    Object.assign(cart.Pricing, {
        computeSelectedSubtotal: computeSelectedSubtotal,
        TotalCount: TotalCount,
        setCartPriceBlock: setCartPriceBlock,
        updateNextStepByBonus: updateNextStepByBonus
    });
})(window.ShoppingCart, window.jQuery);
