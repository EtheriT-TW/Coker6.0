// wwwroot/view-resources/ShoppingCart/shopping-cart.marketing.js
(function (cart, $) {
    "use strict";

    var S = cart.State;

    cart.Marketing = cart.Marketing || {};

    function getValue(data, key) {
        if (!data || !key) return undefined;

        if (Object.prototype.hasOwnProperty.call(data, key)) {
            return data[key];
        }

        var lowerKey = key.charAt(0).toLowerCase() + key.slice(1);
        if (Object.prototype.hasOwnProperty.call(data, lowerKey)) {
            return data[lowerKey];
        }

        var upperKey = key.charAt(0).toUpperCase() + key.slice(1);
        if (Object.prototype.hasOwnProperty.call(data, upperKey)) {
            return data[upperKey];
        }

        return undefined;
    }

    function normalizeCampaignResult(result) {
        if (!result || !result.success || !result.object) {
            return {
                orderDiscounts: []
            };
        }

        return {
            orderDiscounts: getValue(result.object, "orderDiscounts") || []
        };
    }

    function loadCartMarketingCampaigns() {
        if (!window.Coker ||
            !Coker.Marketing ||
            typeof Coker.Marketing.GetCartMarketingCampaigns !== "function") {

            S.marketingCampaigns = {
                orderDiscounts: []
            };

            return $.Deferred().resolve(S.marketingCampaigns).promise();
        }

        return Coker.Marketing.GetCartMarketingCampaigns()
            .done(function (result) {
                S.marketingCampaigns = normalizeCampaignResult(result);
            })
            .fail(function () {
                S.marketingCampaigns = {
                    orderDiscounts: []
                };
            });
    }

    function calculateOrderDiscount(productSubtotal) {
        var subtotal = Number(productSubtotal || 0);

        var emptyResult = {
            applied: false,
            campaignId: null,
            ruleId: null,
            campaignName: "",
            discountAmount: 0,
            memo: ""
        };

        if (subtotal <= 0) {
            return emptyResult;
        }

        var campaigns = S.marketingCampaigns && S.marketingCampaigns.orderDiscounts
            ? S.marketingCampaigns.orderDiscounts
            : [];

        if (!campaigns.length) {
            return emptyResult;
        }

        for (var i = 0; i < campaigns.length; i++) {
            var campaign = campaigns[i];
            var rules = getValue(campaign, "rules") || [];

            for (var j = 0; j < rules.length; j++) {
                var rule = rules[j];
                var discount = calculateRuleDiscount(campaign, rule, subtotal);

                if (discount && discount.discountAmount > 0) {
                    return discount;
                }
            }
        }

        return emptyResult;
    }

    function calculateRuleDiscount(campaign, rule, subtotal) {
        var ruleType = Number(getValue(rule, "ruleType") || 0);
        var minAmount = Number(getValue(rule, "minAmount") || 0);

        if (minAmount <= 0 || subtotal < minAmount) {
            return null;
        }

        var campaignName = getValue(campaign, "name") || "";
        var repeatable = !!getValue(campaign, "repeatable");

        var appliedTimes = 1;
        var discount = 0;

        switch (ruleType) {
            // AmountDiscount = 1
            case 1: {
                var discountAmount = Number(getValue(rule, "discountAmount") || 0);

                appliedTimes = repeatable
                    ? Math.floor(subtotal / minAmount)
                    : 1;

                appliedTimes = Math.max(1, appliedTimes);

                discount = repeatable
                    ? discountAmount * appliedTimes
                    : discountAmount;

                break;
            }

            // PercentDiscount = 2
            case 2: {
                var percent = Number(getValue(rule, "discountPercent") || 0);

                // 後台填 90 表示 9 折，所以折扣金額是 10%
                discount = Math.floor(subtotal * (100 - percent) / 100);

                var maxDiscountAmount = Number(getValue(rule, "maxDiscountAmount") || 0);
                if (maxDiscountAmount > 0) {
                    discount = Math.min(discount, maxDiscountAmount);
                }

                appliedTimes = 1;
                break;
            }

            default:
                return null;
        }

        discount = Math.max(0, discount);
        discount = Math.min(discount, subtotal);

        if (discount <= 0) {
            return null;
        }

        var memo = appliedTimes > 1
            ? "行銷活動：" + campaignName + "，折抵 " + discount.toLocaleString() + " 元，套用 " + appliedTimes + " 次"
            : "行銷活動：" + campaignName + "，折抵 " + discount.toLocaleString() + " 元";

        return {
            applied: true,
            campaignId: getValue(campaign, "id") || null,
            ruleId: getValue(rule, "id") || null,
            campaignName: campaignName,
            discountAmount: discount,
            memo: memo
        };
    }

    function renderOrderDiscount(discountResult) {
        discountResult = discountResult || {};

        var discount = Number(discountResult.discountAmount || 0);
        var campaignName = discountResult.campaignName || "";

        S.marketingDiscount = discount;
        S.marketingDiscountMemo = discountResult.memo || "";

        var $line = $(".marketingDiscountLine");
        var $value = $(".marketingDiscount");
        var $name = $(".marketingDiscountName");

        if (!$line.length) {
            return;
        }

        if (discount > 0) {
            $line.removeClass("d-none");
            $value.text(discount.toLocaleString());

            if ($name.length) {
                $name.text(campaignName ? "（" + campaignName + "）" : "");
            }
        } else {
            $line.addClass("d-none");
            $value.text("");

            if ($name.length) {
                $name.text("");
            }
        }
    }

    Object.assign(cart.Marketing, {
        loadCartMarketingCampaigns: loadCartMarketingCampaigns,
        calculateOrderDiscount: calculateOrderDiscount,
        renderOrderDiscount: renderOrderDiscount
    });

})(window.ShoppingCart, window.jQuery);