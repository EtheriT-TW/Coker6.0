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
                orderDiscounts: [],
                addOnCampaigns: []
            };
        }

        return {
            orderDiscounts: getValue(result.object, "orderDiscounts") || [],
            addOnCampaigns: getValue(result.object, "addOnCampaigns") || []
        };
    }

    function loadCartMarketingCampaigns() {
        if (!window.Coker ||
            !Coker.Marketing ||
            typeof Coker.Marketing.GetCartMarketingCampaigns !== "function") {

            S.marketingCampaigns = {
                orderDiscounts: [],
                addOnCampaigns: []
            };

            return $.Deferred().resolve(S.marketingCampaigns).promise();
        }

        return Coker.Marketing.GetCartMarketingCampaigns()
            .done(function (result) {
                S.marketingCampaigns = normalizeCampaignResult(result);
                refreshRewardCampaigns();
                refreshProductAddOnPrompt();
            })
            .fail(function () {
                S.marketingCampaigns = {
                    orderDiscounts: [],
                    addOnCampaigns: []
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
            memo: "",
            appliedDiscounts: []
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

        var candidates = [];
        for (var i = 0; i < campaigns.length; i++) {
            var campaign = campaigns[i];
            var rules = getValue(campaign, "rules") || [];

            for (var j = 0; j < rules.length; j++) {
                var rule = rules[j];
                var discount = calculateRuleDiscount(campaign, rule, subtotal);

                if (discount && discount.discountAmount > 0) {
                    discount.canStack = getValue(campaign, "canStack") === true;
                    discount.priority = Number(getValue(campaign, "priority") || 0);
                    discount.sequence = candidates.length;
                    candidates.push(discount);
                }
            }
        }

        var selected = selectBestDiscountCombination(candidates, subtotal);
        if (!selected.length) return emptyResult;

        selected.sort(compareDiscountOrder);
        var remaining = subtotal;
        selected.forEach(function (discount) {
            discount.baseAmount = remaining;

            if (discount.ruleType === 2) {
                discount.discountAmount = Math.floor(remaining * (100 - discount.discountPercent) / 100);
                if (discount.maxDiscountAmount > 0) {
                    discount.discountAmount = Math.min(discount.discountAmount, discount.maxDiscountAmount);
                }
            }

            discount.discountAmount = Math.max(0, Math.min(Number(discount.discountAmount || 0), remaining));
            remaining -= discount.discountAmount;
            discount.memo = discount.appliedTimes > 1
                ? "行銷活動：" + discount.campaignName + "，折抵 " + discount.discountAmount.toLocaleString() + " 元，套用 " + discount.appliedTimes + " 次"
                : "行銷活動：" + discount.campaignName + "，折抵 " + discount.discountAmount.toLocaleString() + " 元";
        });
        selected = selected.filter(function (discount) { return discount.discountAmount > 0; });
        if (!selected.length) return emptyResult;

        return {
            applied: true,
            campaignId: selected[0].campaignId,
            ruleId: selected[0].ruleId,
            campaignName: selected.map(function (x) { return x.campaignName; }).join("、"),
            discountAmount: selected.reduce(function (sum, x) { return sum + x.discountAmount; }, 0),
            memo: selected.map(function (x) { return x.memo; }).join("；"),
            appliedDiscounts: selected
        };
    }

    function selectBestDiscountCombination(candidates, subtotal) {
        if (!candidates.length) return [];
        var combinations = [];
        var stackable = candidates.filter(function (x) { return x.canStack; });
        if (stackable.length) combinations.push(stackable);
        candidates.filter(function (x) { return !x.canStack; }).forEach(function (candidate) {
            combinations.push([candidate]);
        });

        combinations.sort(function (left, right) {
            var leftTotal = calculateCombinationDiscount(left, subtotal);
            var rightTotal = calculateCombinationDiscount(right, subtotal);
            if (leftTotal !== rightTotal) return rightTotal - leftTotal;
            var leftPriority = Math.min.apply(null, left.map(function (x) { return x.priority; }));
            var rightPriority = Math.min.apply(null, right.map(function (x) { return x.priority; }));
            if (leftPriority !== rightPriority) return leftPriority - rightPriority;
            var leftSequence = Math.min.apply(null, left.map(function (x) { return x.sequence; }));
            var rightSequence = Math.min.apply(null, right.map(function (x) { return x.sequence; }));
            return leftSequence - rightSequence;
        });
        return combinations[0].slice();
    }

    function compareDiscountOrder(left, right) {
        if (left.minAmount !== right.minAmount) return left.minAmount - right.minAmount;
        if (left.priority !== right.priority) return left.priority - right.priority;
        return left.sequence - right.sequence;
    }

    function calculateCombinationDiscount(discounts, subtotal) {
        var remaining = subtotal;

        discounts.slice().sort(compareDiscountOrder).forEach(function (discount) {
            var amount = Number(discount.discountAmount || 0);

            if (discount.ruleType === 2) {
                amount = Math.floor(remaining * (100 - discount.discountPercent) / 100);
                if (discount.maxDiscountAmount > 0) {
                    amount = Math.min(amount, discount.maxDiscountAmount);
                }
            }

            amount = Math.max(0, Math.min(amount, remaining));
            remaining -= amount;
        });

        return subtotal - remaining;
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
            memo: memo,
            appliedTimes: appliedTimes,
            ruleType: ruleType,
            minAmount: minAmount,
            discountPercent: Number(getValue(rule, "discountPercent") || 0),
            maxDiscountAmount: Number(getValue(rule, "maxDiscountAmount") || 0)
        };
    }

    function renderOrderDiscount(discountResult) {
        discountResult = discountResult || {};

        var discount = Number(discountResult.discountAmount || 0);

        S.marketingDiscount = discount;
        S.marketingDiscountMemo = discountResult.memo || "";

        var $containers = $(".marketingDiscountLines");

        if (!$containers.length) {
            return;
        }

        var appliedDiscounts = Array.isArray(discountResult.appliedDiscounts)
            ? discountResult.appliedDiscounts.filter(function (item) {
                return Number(item && item.discountAmount || 0) > 0;
            })
            : [];

        // 相容尚未提供折扣明細的舊回傳格式。
        if (!appliedDiscounts.length && discount > 0) {
            appliedDiscounts.push({
                campaignName: discountResult.campaignName || "",
                discountAmount: discount
            });
        }

        if (!appliedDiscounts.length) {
            $containers.empty().addClass("d-none");
            return;
        }

        var html = appliedDiscounts.map(function (item) {
            var campaignName = item.campaignName || "";
            var amount = Number(item.discountAmount || 0);
            var label = campaignName ? escapeHtml(campaignName) : "活動折扣";

            return '<div class="summary-main-row marketingDiscountLine">' +
                '<div class="summary-label">' + label + '</div>' +
                '<div class="summary-amount price-negative">$' + amount.toLocaleString() + '</div>' +
                '</div>';
        }).join("");

        $containers.html(html).removeClass("d-none");
    }

    function getSelectedBaseItems() {
        var ids = cart.Items && typeof cart.Items.getSelectedCartIds === "function"
            ? cart.Items.getSelectedCartIds().map(Number)
            : [];

        return (S.shopping_cart_data || []).filter(function (item) {
            return ids.includes(Number(item.Id)) && item.IsAdditional !== true;
        });
    }

    function getCartBaseItems() {
        return (S.shopping_cart_data || []).filter(function (item) {
            return item.IsAdditional !== true && Number(item.Quantity || 0) > 0;
        });
    }

    function getGroupBaseItems(groupId) {
        if (groupId == null || groupId === '') return getCartBaseItems();
        var cartIds = $('.purchase_group[data-group-id="' + groupId + '"] .cart-primary-item').map(function () {
            return Number($(this).data('scId'));
        }).get();
        return getCartBaseItems().filter(function (item) {
            return cartIds.includes(Number(item.Id));
        });
    }

    function getBaseItemAmount(item) {
        var cartId = Number(item.Id || 0);
        var $cartItem = $('.purchase_item').filter(function () {
            return Number($(this).data('scId')) === cartId;
        }).first();
        var $subtotal = $cartItem.find('[data-key="subtotal"]').first();
        if ($subtotal.length) {
            var displayedSubtotal = Number($subtotal.data('subtotal'));
            if (Number.isFinite(displayedSubtotal)) return displayedSubtotal;
        }
        return Number(item.Price || 0) * Number(item.Quantity || 0);
    }

    function getCampaignQualification(campaign, baseItems) {
        var conditionType = Number(getValue(campaign, "conditionType") || 0);
        var scopeIds = (getValue(campaign, "scopeProductIds") || []).map(Number);
        var minAmount = Number(getValue(campaign, "minAmount") || 0);
        var requiredQuantity = Math.max(1, Number(getValue(campaign, "requiredQuantity") || 1));
        var isQuantityCondition = conditionType === 4 || conditionType === 5;
        var quantity = baseItems.reduce(function (sum, item) {
            return scopeIds.includes(Number(item.PId)) ? sum + Number(item.Quantity || 0) : sum;
        }, 0);
        var amount = baseItems.reduce(function (sum, item) {
            if (conditionType === 2 && !scopeIds.includes(Number(item.PId))) return sum;
            return sum + getBaseItemAmount(item);
        }, 0);
        var qualified = isQuantityCondition
            ? quantity >= requiredQuantity
            : minAmount > 0 && amount >= minAmount;
        var basis = isQuantityCondition ? quantity : amount;
        var threshold = isQuantityCondition ? requiredQuantity : minAmount;
        var times = qualified
            ? (getValue(campaign, "repeatable") ? Math.floor(basis / threshold) : 1)
            : 0;
        var perQualification = Math.max(1, Number(getValue(campaign, "selectionQuantityPerQualification") || 1));

        return {
            amount: amount,
            quantity: quantity,
            minAmount: minAmount,
            requiredQuantity: requiredQuantity,
            qualified: qualified,
            times: times,
            allowance: times * perQualification,
            shortage: Math.max(0, minAmount - amount)
        };
    }

    function selectionKey(campaign, item) {
        return [getValue(campaign, "campaignId"), getValue(campaign, "ruleId"), getValue(item, "rewardItemId")].join(":");
    }

    function getCampaignSelectedQuantity(campaign) {
        var prefix = getValue(campaign, "campaignId") + ":" + getValue(campaign, "ruleId") + ":";
        return Object.keys(S.marketingRewardSelections || {}).reduce(function (sum, key) {
            return sum + (key.indexOf(prefix) === 0 ? Number(S.marketingRewardSelections[key] || 0) : 0);
        }, 0);
    }

    function clampSelections(campaign, qualification) {
        var items = getValue(campaign, "rewardItems") || [];
        var remaining = qualification.allowance;

        items.forEach(function (item) {
            var key = selectionKey(campaign, item);
            var current = Math.max(0, Number(S.marketingRewardSelections[key] || 0));
            var itemLimit = Math.max(1, Number(getValue(item, "maxQuantityPerOrder") || 1));
            if (getValue(campaign, "repeatable")) itemLimit *= Math.max(1, qualification.times);
            var stock = getValue(item, "stockQuantity");
            if (!getValue(item, "noStockManagement") && stock != null) itemLimit = Math.min(itemLimit, Number(stock));
            current = qualification.qualified ? Math.min(current, itemLimit, remaining) : 0;
            if (current > 0) {
                S.marketingRewardSelections[key] = current;
                remaining -= current;
            } else {
                delete S.marketingRewardSelections[key];
            }
        });
    }

    function getRewardItemLimit(campaign, item, qualification) {
        var itemLimit = Math.max(1, Number(getValue(item, "maxQuantityPerOrder") || 1));
        if (getValue(campaign, "repeatable")) itemLimit *= Math.max(1, qualification.times);
        var stock = getValue(item, "stockQuantity");
        if (!getValue(item, "noStockManagement") && stock != null) {
            itemLimit = Math.min(itemLimit, Math.max(0, Number(stock) || 0));
        }
        return itemLimit;
    }

    function autoSelectSingleGift(campaign, qualification) {
        if (!qualification.qualified || qualification.allowance <= 0) return;
        var items = getValue(campaign, "rewardItems") || [];
        if (items.length !== 1 || Number(getValue(items[0], "offerPrice") || 0) > 0) return;

        var item = items[0];
        var key = selectionKey(campaign, item);
        if (S.marketingRewardManualSelections && S.marketingRewardManualSelections[key]) return;
        var quantity = Math.min(qualification.allowance, getRewardItemLimit(campaign, item, qualification));
        if (quantity > 0) S.marketingRewardSelections[key] = quantity;
    }

    function escapeHtml(value) {
        return $("<div>").text(value == null ? "" : String(value)).html();
    }

    function renderRewardItem(campaign, item, qualification, campaignSelected) {
        var key = selectionKey(campaign, item);
        var quantity = Number(S.marketingRewardSelections[key] || 0);
        var selected = quantity > 0;
        var offerPrice = Number(getValue(item, "offerPrice") || 0);
        var originalPrice = Number(getValue(item, "originalPrice") || 0);
        var full = campaignSelected >= qualification.allowance;
        var disabled = !qualification.qualified || (!selected && full);
        var itemLimit = Math.max(1, Number(getValue(item, "maxQuantityPerOrder") || 1));
        if (getValue(campaign, "repeatable")) itemLimit *= Math.max(1, qualification.times);
        var canIncrease = selected && quantity < itemLimit && campaignSelected < qualification.allowance;
        var tag = offerPrice <= 0 ? "贈品" : "加價購";
        var priceText = offerPrice <= 0 ? "免費" : "NT$ " + offerPrice.toLocaleString();
        var original = originalPrice > 0
            ? '<span class="cart-reward-original">原價 NT$ ' + originalPrice.toLocaleString() + '</span>'
            : "";

        return '<article class="cart-reward-card' + (selected ? ' is-selected' : '') + (disabled ? ' is-disabled' : '') + '"' +
            ' data-selection-key="' + key + '" role="button" tabindex="' + (disabled ? '-1' : '0') + '">' +
            '<span class="cart-reward-check"><i class="fa-solid fa-check"></i></span>' +
            '<div class="cart-reward-image-wrap"><img src="' + escapeHtml(getValue(item, "imageUrl") || "/images/noImg.jpg") + '" alt="" loading="lazy"></div>' +
            '<div class="cart-reward-body"><span class="cart-reward-tag">' + tag + '</span>' +
            '<div class="cart-reward-name">' + escapeHtml(getValue(item, "productName")) + '</div>' +
            '<div class="cart-reward-spec">' + escapeHtml(getValue(item, "stockName")) + '</div>' +
            '<div class="cart-reward-price"><strong>' + priceText + '</strong>' + original + '</div></div>' +
            (selected ? '<div class="cart-reward-quantity" aria-label="優惠商品數量">' +
                '<button type="button" class="js-reward-minus" aria-label="減少">−</button><span>' + quantity + '</span>' +
                '<button type="button" class="js-reward-plus" aria-label="增加"' + (canIncrease ? '' : ' disabled') + '>＋</button></div>' : '') +
            '</article>';
    }

    function renderScopeProducts(campaign, qualified) {
        var products = getValue(campaign, "scopeProducts") || [];
        if (!products.length) return "";

        var cards = products.map(function (product) {
            var productId = Number(getValue(product, "productId") || 0);
            var available = getValue(product, "available") === true;
            return '<div class="swiper-slide"><article class="cart-scope-product' + (available ? '' : ' is-unavailable') + '"' +
                (available ? ' role="button" tabindex="0"' +
                    ' data-quick-cart-product-id="' + productId + '"' +
                    ' data-quick-cart-product-name="' + escapeHtml(getValue(product, "productName")) + '"' +
                    ' data-quick-cart-image-url="' + escapeHtml(getValue(product, "imageUrl") || "/images/noImg.jpg") + '"' : '') + '>' +
                '<img src="' + escapeHtml(getValue(product, "imageUrl") || "/images/noImg.jpg") + '" alt="" loading="lazy">' +
                '<span><strong>' + escapeHtml(getValue(product, "productName")) + '</strong>' +
                '<small>' + (available ? '可加入購物車累計活動' : '目前無法購買') + '</small></span>' +
                (available ? '<button type="button" class="cart-scope-product-buy">' +
                    '<i class="fa-solid fa-cart-plus" aria-hidden="true"></i><span>快速選購</span></button>' : '') +
                '</article></div>';
        }).join("");

        var collapsed = qualified === true;
        return '<div class="cart-scope-products' + (collapsed ? ' is-collapsed' : ' is-expanded') + '"><div class="cart-scope-products-header">' +
            '<div><strong>選購指定商品</strong><small>' + (collapsed
                ? '已達活動門檻，仍可展開加購或更換商品'
                : '不離開購物車，加入後會立即重新計算活動資格') + '</small></div>' +
            '<button type="button" class="cart-scope-toggle" aria-expanded="' + (!collapsed) + '">' +
            '<span>' + (collapsed ? '展開選購' : '收合') + '</span><i class="fa-solid fa-chevron-down" aria-hidden="true"></i></button></div>' +
            '<div class="cart-scope-products-body"' + (collapsed ? ' hidden' : '') + '>' +
            '<div class="cart-scope-product-nav"><button type="button" class="cart-scope-prev" aria-label="上一組指定商品"><i class="fa-solid fa-chevron-left"></i></button>' +
            '<button type="button" class="cart-scope-next" aria-label="下一組指定商品"><i class="fa-solid fa-chevron-right"></i></button></div>' +
            '<div class="cart-scope-product-swiper swiper"><div class="swiper-wrapper">' + cards + '</div></div></div></div>';
    }

    function initScopeProductSwiper($section) {
        if (typeof window.Swiper !== "function" || $section.data('scopeSwiper') ||
            $section.find('.cart-scope-products-body').prop('hidden')) return;
        var swiper = new window.Swiper($section.find('.cart-scope-product-swiper').get(0), {
            slidesPerView: 'auto',
            spaceBetween: 10,
            watchOverflow: true,
            navigation: {
                prevEl: $section.find('.cart-scope-prev').get(0),
                nextEl: $section.find('.cart-scope-next').get(0)
            }
        });
        $section.data('scopeSwiper', swiper);
        S.scopeProductSwipers.push(swiper);
    }

    function initScopeProductSwipers($root) {
        (S.scopeProductSwipers || []).forEach(function (swiper) { if (swiper && swiper.destroy) swiper.destroy(true, true); });
        S.scopeProductSwipers = [];
        if (typeof window.Swiper !== "function") return;
        $root.find('.cart-scope-products').each(function () {
            initScopeProductSwiper($(this));
        });
    }

    function renderRewardCampaigns(campaignsWithStatus) {
        var $root = $("#CartMarketingRewards");
        if (!$root.length) return;
        if (!campaignsWithStatus.length) {
            $root.addClass("d-none");
            return;
        }

        var totalSelected = 0;
        var html = campaignsWithStatus.map(function (entry) {
            var campaign = entry.campaign;
            var q = entry.qualification;
            var selected = getCampaignSelectedQuantity(campaign);
            totalSelected += selected;
            var conditionName = Number(getValue(campaign, "conditionType")) === 2 ? "指定商品合計" : "本次訂單";
            var hideZeroScopeStatus = !q.qualified && Number(getValue(campaign, "conditionType")) === 2 && q.amount <= 0;
            var status = q.qualified
                ? '已符合資格，可選 <strong>' + q.allowance + '</strong> 件，目前已選 <strong>' + selected + '</strong> 件'
                : hideZeroScopeStatus ? '' : conditionName + '目前 <strong>NT$ ' + q.amount.toLocaleString() + '</strong>，再買 <strong>NT$ ' + q.shortage.toLocaleString() + '</strong> 即可選購';
            var items = (getValue(campaign, "rewardItems") || []).map(function (item) {
                return renderRewardItem(campaign, item, q, selected);
            }).join("");
            var scopeProducts = Number(getValue(campaign, "conditionType")) === 2
                ? renderScopeProducts(campaign, q.qualified) : "";

            return '<section class="cart-marketing-campaign' + (q.qualified ? ' is-qualified' : '') + '">' +
                '<div class="cart-campaign-header"><div><h4>' + escapeHtml(getValue(campaign, "name")) + '</h4>' +
                (status ? '<p>' + status + '</p>' : '') + '</div><span class="cart-campaign-threshold">' + conditionName + '滿 NT$ ' + q.minAmount.toLocaleString() + '</span></div>' +
                scopeProducts + '<div class="cart-reward-track">' + items + '</div></section>';
        }).join("");

        $root.removeClass("d-none");
        $root.find(".cart-marketing-campaign-list").html(html);
        initScopeProductSwipers($root);
        $root.find(".cart-marketing-selected-summary").text(totalSelected > 0 ? "已選 " + totalSelected + " 件" : "優惠品需自行選取");
    }

    function refreshRewardCampaigns() {
        var campaigns = S.marketingCampaigns && S.marketingCampaigns.addOnCampaigns
            ? S.marketingCampaigns.addOnCampaigns : [];
        campaigns = campaigns.filter(function (campaign) {
            var type = Number(getValue(campaign, "conditionType") || 0);
            return type === 1 || type === 2;
        });
        var baseItems = getSelectedBaseItems();
        var validKeys = new Set();
        campaigns.forEach(function (campaign) {
            (getValue(campaign, "rewardItems") || []).forEach(function (item) {
                validKeys.add(selectionKey(campaign, item));
            });
        });
        Object.keys(S.marketingRewardSelections || {}).forEach(function (key) {
            if (!validKeys.has(key)) delete S.marketingRewardSelections[key];
        });
        Object.keys(S.marketingRewardManualSelections || {}).forEach(function (key) {
            if (!validKeys.has(key)) delete S.marketingRewardManualSelections[key];
        });
        var status = campaigns.map(function (campaign) {
            var qualification = getCampaignQualification(campaign, baseItems);
            clampSelections(campaign, qualification);
            autoSelectSingleGift(campaign, qualification);
            return { campaign: campaign, qualification: qualification };
        });

        renderRewardCampaigns(status);
        var amount = 0;
        status.forEach(function (entry) {
            (getValue(entry.campaign, "rewardItems") || []).forEach(function (item) {
                amount += Number(S.marketingRewardSelections[selectionKey(entry.campaign, item)] || 0) *
                    Number(getValue(item, "offerPrice") || 0);
            });
        });
        S.marketingRewardAmount = amount;
        return amount;
    }

    function isProductAddOnCampaign(campaign) {
        var type = Number(getValue(campaign, "conditionType") || 0);
        return type === 4 || type === 5;
    }

    function getProductAddOnCampaigns() {
        return (S.marketingCampaigns && S.marketingCampaigns.addOnCampaigns || [])
            .filter(isProductAddOnCampaign);
    }

    function campaignKey(campaign) {
        return Number(getValue(campaign, 'campaignId')) + ':' + Number(getValue(campaign, 'ruleId'));
    }

    function getModalProductAddOnCampaigns() {
        var keys = $('#CartProductAddOnModal').data('campaignKeys');
        if (!Array.isArray(keys) || keys.length === 0) return getProductAddOnCampaigns();
        return getProductAddOnCampaigns().filter(function (campaign) {
            return keys.includes(campaignKey(campaign));
        });
    }

    function getModalBaseItems() {
        return getGroupBaseItems($('#CartProductAddOnModal').data('groupId'));
    }

    function getPersistedRewardQuantity(item) {
        var stockId = Number(getValue(item, "productStockId") || 0);
        var offerPrice = Number(getValue(item, "offerPrice") || 0);
        return (S.shopping_cart_data || []).reduce(function (sum, cartItem) {
            return cartItem.IsAdditional === true && Number(cartItem.PSId) === stockId && Number(cartItem.Price) === offerPrice
                ? sum + Number(cartItem.Quantity || 0) : sum;
        }, 0);
    }

    function prepareProductAddOnDrafts(force) {
        if (!force && S.productAddOnDrafts) return;
        S.productAddOnDrafts = {};
        getProductAddOnCampaigns().forEach(function (campaign) {
            var items = getValue(campaign, "rewardItems") || [];
            items.forEach(function (item) {
                S.productAddOnDrafts[selectionKey(campaign, item)] = getPersistedRewardQuantity(item);
            });
            var qualification = getCampaignQualification(campaign, getModalBaseItems());
            if (qualification.qualified && items.length === 1 &&
                Number(getValue(items[0], 'offerPrice') || 0) <= 0 &&
                Number(S.productAddOnDrafts[selectionKey(campaign, items[0])] || 0) === 0) {
                S.productAddOnDrafts[selectionKey(campaign, items[0])] = Math.min(
                    qualification.allowance,
                    getRewardItemLimit(campaign, items[0], qualification)
                );
            }
        });
    }

    function getDraftCampaignQuantity(campaign) {
        var prefix = getValue(campaign, "campaignId") + ":" + getValue(campaign, "ruleId") + ":";
        return Object.keys(S.productAddOnDrafts || {}).reduce(function (sum, key) {
            return sum + (key.indexOf(prefix) === 0 ? Number(S.productAddOnDrafts[key] || 0) : 0);
        }, 0);
    }

    function renderProductAddOnCard(campaign, item, qualification) {
        var key = selectionKey(campaign, item);
        var quantity = Number(S.productAddOnDrafts[key] || 0);
        var selectedTotal = getDraftCampaignQuantity(campaign);
        var itemLimit = Math.max(1, Number(getValue(item, "maxQuantityPerOrder") || 1));
        if (getValue(campaign, "repeatable")) itemLimit *= Math.max(1, qualification.times);
        var stock = getValue(item, "stockQuantity");
        if (!getValue(item, "noStockManagement") && stock != null) itemLimit = Math.min(itemLimit, Number(stock));
        var canIncrease = qualification.qualified && quantity < itemLimit && selectedTotal < qualification.allowance;
        var selected = quantity > 0;
        var disabled = !qualification.qualified || (!selected && selectedTotal >= qualification.allowance);
        var offerPrice = Number(getValue(item, "offerPrice") || 0);
        var originalPrice = Number(getValue(item, "originalPrice") || 0);

        return '<article class="cart-reward-card cart-addon-modal-card' + (selected ? ' is-selected' : '') +
            (disabled ? ' is-disabled' : '') + '" data-product-selection-key="' + key + '">' +
            '<span class="cart-reward-check"><i class="fa-solid fa-check"></i></span>' +
            '<div class="cart-reward-image-wrap"><img src="' + escapeHtml(getValue(item, "imageUrl") || "/images/noImg.jpg") + '" alt="" loading="lazy"></div>' +
            '<div class="cart-reward-body"><span class="cart-reward-tag">' + (offerPrice <= 0 ? '贈品' : '加價購') + '</span>' +
            '<div class="cart-reward-name">' + escapeHtml(getValue(item, "productName")) + '</div>' +
            '<div class="cart-reward-spec">' + escapeHtml(getValue(item, "stockName")) + '</div>' +
            '<div class="cart-reward-price"><strong>' + (offerPrice <= 0 ? '免費' : 'NT$ ' + offerPrice.toLocaleString()) + '</strong>' +
            (originalPrice > 0 ? '<span class="cart-reward-original">原價 NT$ ' + originalPrice.toLocaleString() + '</span>' : '') + '</div></div>' +
            '<div class="cart-reward-quantity"><button type="button" class="js-product-addon-minus" aria-label="減少"' + (selected ? '' : ' disabled') + '>−</button>' +
            '<span>' + quantity + '</span><button type="button" class="js-product-addon-plus" aria-label="增加"' + (canIncrease ? '' : ' disabled') + '>＋</button></div></article>';
    }

    function renderProductAddOnModal(message) {
        prepareProductAddOnDrafts(false);
        var baseItems = getModalBaseItems();
        var campaigns = getModalProductAddOnCampaigns();
        var html = campaigns.map(function (campaign) {
            var qualification = getCampaignQualification(campaign, baseItems);
            var selected = getDraftCampaignQuantity(campaign);
            var status = qualification.qualified
                ? '主要商品共 ' + qualification.quantity + ' 件，可選 ' + qualification.allowance + ' 件，目前選了 ' + selected + ' 件'
                : '指定商品需滿 ' + qualification.requiredQuantity + ' 件才可選購';
            var cards = (getValue(campaign, "rewardItems") || []).map(function (item) {
                return renderProductAddOnCard(campaign, item, qualification);
            }).join('');
            return '<section class="cart-addon-modal-campaign"><div class="cart-campaign-header"><div><h4>' +
                escapeHtml(getValue(campaign, "name")) + '</h4><p>' + status + '</p></div>' +
                '<span class="cart-campaign-threshold">每滿 ' + qualification.requiredQuantity + ' 件，可選 ' +
                Number(getValue(campaign, "selectionQuantityPerQualification") || 1) + ' 件</span></div>' +
                '<div class="cart-reward-track">' + cards + '</div></section>';
        }).join('');
        var $campaignRoot = $('.cart-addon-modal-campaigns');
        $campaignRoot.html(html || '<div class="text-muted py-5 text-center">目前沒有可補選的商品優惠</div>');
        $('.cart-addon-modal-summary').text(message || '可在這裡直接補選、減少或更換優惠商品');
    }

    function openProductAddOnModal(message, campaignKeys, groupId) {
        $('#CartProductAddOnModal')
            .data('campaignKeys', Array.isArray(campaignKeys) ? campaignKeys : [])
            .data('groupId', groupId == null ? '' : groupId);
        prepareProductAddOnDrafts(true);
        renderProductAddOnModal(message);
        var element = document.getElementById('CartProductAddOnModal');
        if (element) bootstrap.Modal.getOrCreateInstance(element).show();
    }

    function changeProductAddOnDraft(key, delta) {
        var parts = String(key || '').split(':').map(Number);
        var campaign = getProductAddOnCampaigns().find(function (x) {
            return Number(getValue(x, 'campaignId')) === parts[0] && Number(getValue(x, 'ruleId')) === parts[1];
        });
        if (!campaign) return;
        var item = (getValue(campaign, 'rewardItems') || []).find(function (x) {
            return Number(getValue(x, 'rewardItemId')) === parts[2];
        });
        if (!item) return;
        var qualification = getCampaignQualification(campaign, getModalBaseItems());
        var current = Number(S.productAddOnDrafts[key] || 0);
        var next = Math.max(0, current + delta);
        var itemLimit = Math.max(1, Number(getValue(item, 'maxQuantityPerOrder') || 1)) *
            (getValue(campaign, 'repeatable') ? Math.max(1, qualification.times) : 1);
        if (delta > 0 && (getDraftCampaignQuantity(campaign) >= qualification.allowance || next > itemLimit)) return;
        S.productAddOnDrafts[key] = next;
        renderProductAddOnModal();
    }

    function saveProductAddOnDrafts() {
        var campaigns = getModalProductAddOnCampaigns();
        $('.cart-addon-modal-summary').text('正在儲存優惠商品…');
        var requests = campaigns.map(function (campaign) {
            var selections = (getValue(campaign, 'rewardItems') || []).map(function (item) {
                return {
                    campaignId: Number(getValue(campaign, 'campaignId')),
                    ruleId: Number(getValue(campaign, 'ruleId')),
                    rewardItemId: Number(getValue(item, 'rewardItemId')),
                    quantity: Number(S.productAddOnDrafts[selectionKey(campaign, item)] || 0)
                };
            });
            return Coker.api.post('/api/ShoppingCart/UpdateAddOnSelections', {
                campaignId: Number(getValue(campaign, 'campaignId')),
                ruleId: Number(getValue(campaign, 'ruleId')),
                selections: selections
            });
        });
        $('.js-save-cart-addons').prop('disabled', true);
        $.when.apply($, requests).done(function () {
            var results = requests.length === 1 ? [arguments[0]] : Array.prototype.slice.call(arguments).map(function (x) { return x[0]; });
            var failed = results.find(function (x) { return !x || x.success !== true; });
            if (failed) {
                $('.cart-addon-modal-summary').text(failed.message || '無法更新，請重新整理後再試。');
                Coker.sweet.error('無法更新優惠商品', failed.message || '請重新整理後再試。', null, true);
                $('.js-save-cart-addons').prop('disabled', false);
                return;
            }
            cart.Items.ReloadCartDisplay().done(function () {
                S.productAddOnDrafts = null;
                var modalElement = document.getElementById('CartProductAddOnModal');
                if (modalElement) bootstrap.Modal.getOrCreateInstance(modalElement).hide();
                $('.js-save-cart-addons').prop('disabled', false);
                Coker.sweet.success('優惠商品已更新', null, true);
            }).fail(function () {
                $('.cart-addon-modal-summary').text('優惠商品已儲存，但畫面更新失敗，請重新整理後確認。');
                $('.js-save-cart-addons').prop('disabled', false);
                Coker.sweet.error('畫面更新失敗', '優惠商品已儲存，請重新整理購物車確認。', null, true);
            });
        }).fail(function () {
            $('.cart-addon-modal-summary').text('無法更新，請稍後再試。');
            Coker.sweet.error('無法更新優惠商品', '請稍後再試。', null, true);
            $('.js-save-cart-addons').prop('disabled', false);
        });
    }

    function requireProductAddOnAdjustment() {
        var invalid = getProductAddOnCampaigns().some(function (campaign) {
            var qualification = getCampaignQualification(campaign, getCartBaseItems());
            var persisted = (getValue(campaign, 'rewardItems') || []).reduce(function (sum, item) {
                return sum + getPersistedRewardQuantity(item);
            }, 0);
            return persisted > qualification.allowance;
        });
        if (invalid) openProductAddOnModal('主要商品數量已減少，請調整超出資格的優惠商品後儲存');
        return invalid;
    }

    function hasUnselectedAvailableFreeReward(campaign, qualification) {
        var productAddOn = isProductAddOnCampaign(campaign);
        return (getValue(campaign, 'rewardItems') || []).some(function (item) {
            if (Number(getValue(item, 'offerPrice') || 0) > 0) return false;
            var selected = productAddOn
                ? getPersistedRewardQuantity(item)
                : Number(S.marketingRewardSelections[selectionKey(campaign, item)] || 0);
            return selected < getRewardItemLimit(campaign, item, qualification);
        });
    }

    function getUnclaimedGiftCampaigns() {
        var baseItems = getSelectedBaseItems();
        return (S.marketingCampaigns && S.marketingCampaigns.addOnCampaigns || []).filter(function (campaign) {
            var type = Number(getValue(campaign, 'conditionType') || 0);
            if (![1, 2, 4, 5].includes(type)) return false;
            var qualification = getCampaignQualification(campaign, baseItems);
            if (!qualification.qualified || qualification.allowance <= 0 ||
                !hasUnselectedAvailableFreeReward(campaign, qualification)) return false;

            var selected = isProductAddOnCampaign(campaign)
                ? (getValue(campaign, 'rewardItems') || []).reduce(function (sum, item) {
                    return sum + getPersistedRewardQuantity(item);
                }, 0)
                : getCampaignSelectedQuantity(campaign);
            return selected < qualification.allowance;
        });
    }

    function focusUnclaimedGiftCampaigns(campaigns) {
        var productAddOnCampaign = campaigns.find(isProductAddOnCampaign);
        if (productAddOnCampaign) {
            openProductAddOnModal('還有可領取的贈品尚未選擇', [campaignKey(productAddOnCampaign)]);
            return;
        }
        var root = document.getElementById('CartMarketingRewards');
        if (root) root.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function confirmGiftAbandonment() {
        if (S.skipGiftAbandonWarningOnce) {
            S.skipGiftAbandonWarningOnce = false;
            return true;
        }

        var campaigns = getUnclaimedGiftCampaigns();
        if (!campaigns.length) return true;
        if (S.giftAbandonWarningOpen) return false;

        var continueToNextStep = function () {
            S.skipGiftAbandonWarningOnce = true;
            var nextButton = document.querySelector('.btn_swiper_next_buystep');
            if (nextButton) nextButton.click();
        };
        var returnToGifts = function () { focusUnclaimedGiftCampaigns(campaigns); };
        var message = campaigns.length > 1
            ? '目前有 ' + campaigns.length + ' 組贈品尚未領取，確定要放棄並進入下一步嗎？'
            : '目前還有贈品尚未領取，確定要放棄並進入下一步嗎？';

        if (typeof window.Swal !== 'undefined' && typeof window.Swal.fire === 'function') {
            S.giftAbandonWarningOpen = true;
            window.Swal.fire({
                icon: 'warning',
                title: '還有贈品尚未選取',
                text: message,
                showCancelButton: true,
                confirmButtonText: '放棄贈品並繼續',
                cancelButtonText: '返回選擇贈品',
                reverseButtons: true,
                focusCancel: true,
                returnFocus: false
            }).then(function (result) {
                S.giftAbandonWarningOpen = false;
                if (result.isConfirmed) continueToNextStep();
                else window.requestAnimationFrame(returnToGifts);
            });
            return false;
        }

        if (window.confirm(message)) return true;
        returnToGifts();
        return false;
    }

    function validateProductAddOnBeforeNext() {
        if (requireProductAddOnAdjustment()) return false;
        return confirmGiftAbandonment();
    }

    function refreshProductAddOnPrompt() {
        var campaigns = getProductAddOnCampaigns();
        $('.js-cart-addon-group-action').each(function () {
            var $prompt = $(this);
            var groupId = $prompt.attr('data-group-id');
            var baseItems = getGroupBaseItems(groupId);
            var productIds = baseItems.map(function (item) { return Number(item.PId); });
            var available = 0;
            var selected = 0;
            var relevantCampaigns = campaigns.filter(function (campaign) {
                var scopeIds = (getValue(campaign, 'scopeProductIds') || []).map(Number);
                if (!scopeIds.some(function (id) { return productIds.includes(id); })) return false;
                var qualification = getCampaignQualification(campaign, baseItems);
                var persisted = (getValue(campaign, 'rewardItems') || []).reduce(function (sum, item) {
                    return sum + getPersistedRewardQuantity(item);
                }, 0);
                available += qualification.allowance;
                selected += persisted;
                return qualification.allowance > 0 || persisted > 0;
            });
            var keys = relevantCampaigns.map(campaignKey);
            $prompt.toggleClass('d-none', keys.length === 0);
            $prompt.find('.js-open-cart-addons')
                .attr('data-campaign-keys', keys.join(','))
                .attr('data-group-id', groupId)
                .attr('aria-label', '補選或調整本組優惠商品，目前可選 ' + available + ' 件、已選 ' + selected + ' 件');
        });
    }

    $(document).on('click', '.js-open-cart-addons', function () {
        var keys = String($(this).attr('data-campaign-keys') || '').split(',').filter(Boolean);
        openProductAddOnModal(null, keys, $(this).attr('data-group-id'));
    });
    $(document).on('click', '.cart-addon-modal-card', function (event) {
        if ($(event.target).closest('.cart-reward-quantity').length || $(this).hasClass('is-disabled')) return;
        event.stopImmediatePropagation();
        var key = $(this).attr('data-product-selection-key');
        changeProductAddOnDraft(key, Number(S.productAddOnDrafts[key] || 0) > 0
            ? -Number(S.productAddOnDrafts[key]) : 1);
    });
    $(document).on('click', '.js-product-addon-minus, .js-product-addon-plus', function (event) {
        event.preventDefault();
        event.stopPropagation();
        var key = $(this).closest('.cart-addon-modal-card').attr('data-product-selection-key');
        changeProductAddOnDraft(key, $(this).hasClass('js-product-addon-plus') ? 1 : -1);
    });
    $(document).on('click', '.js-save-cart-addons', function (event) {
        event.preventDefault();
        saveProductAddOnDrafts();
    });
    $(document).on('click', '.cart-scope-toggle', function () {
        var $button = $(this);
        var $section = $button.closest('.cart-scope-products');
        var $body = $section.find('.cart-scope-products-body').first();
        var expanded = $button.attr('aria-expanded') === 'true';
        $button.attr('aria-expanded', String(!expanded));
        $button.find('span').text(expanded ? '展開選購' : '收合');
        $body.prop('hidden', expanded);
        $section.toggleClass('is-expanded', !expanded).toggleClass('is-collapsed', expanded);
        if (!expanded) initScopeProductSwiper($section);
        if (S.buy_step_swiper) {
            S.buy_step_swiper.update();
            S.buy_step_swiper.updateAutoHeight(200);
        }
    });
    function changeRewardQuantity(key, delta) {
        var parts = String(key || "").split(":").map(Number);
        var campaigns = S.marketingCampaigns && S.marketingCampaigns.addOnCampaigns || [];
        var campaign = campaigns.find(function (x) {
            return Number(getValue(x, "campaignId")) === parts[0] && Number(getValue(x, "ruleId")) === parts[1];
        });
        if (!campaign) return;
        var item = (getValue(campaign, "rewardItems") || []).find(function (x) {
            return Number(getValue(x, "rewardItemId")) === parts[2];
        });
        if (!item) return;
        var q = getCampaignQualification(campaign, getSelectedBaseItems());
        if (!q.qualified) return;
        var current = Number(S.marketingRewardSelections[key] || 0);
        var selected = getCampaignSelectedQuantity(campaign);
        var itemLimit = Math.max(1, Number(getValue(item, "maxQuantityPerOrder") || 1)) *
            (getValue(campaign, "repeatable") ? Math.max(1, q.times) : 1);
        var next = Math.max(0, current + delta);
        if (delta > 0 && (selected >= q.allowance || next > itemLimit)) return;
        S.marketingRewardManualSelections = S.marketingRewardManualSelections || {};
        S.marketingRewardManualSelections[key] = true;
        if (next > 0) S.marketingRewardSelections[key] = next;
        else delete S.marketingRewardSelections[key];
        cart.Pricing.TotalCount();
        cart.Payment.Core.onAmountChanged();
    }

    function getRewardSelections() {
        return Object.keys(S.marketingRewardSelections || {}).map(function (key) {
            var parts = key.split(":").map(Number);
            return { campaignId: parts[0], ruleId: parts[1], rewardItemId: parts[2], quantity: Number(S.marketingRewardSelections[key]) };
        }).filter(function (x) { return x.quantity > 0; });
    }

    $(document).on("click", ".cart-reward-card", function (event) {
        var productKey = $(this).attr('data-product-selection-key');
        if (productKey) return;
        if ($(event.target).closest(".cart-reward-quantity").length) return;
        if ($(this).hasClass("is-disabled")) return;
        var key = $(this).attr("data-selection-key");
        changeRewardQuantity(key, Number(S.marketingRewardSelections[key] || 0) > 0 ? -Number(S.marketingRewardSelections[key]) : 1);
    });
    $(document).off("productQuickCart:added.shoppingCart")
        .on("productQuickCart:added.shoppingCart", function () {
            if (!cart.Items || typeof cart.Items.ReloadCartDisplay !== "function") return;
            cart.Items.ReloadCartDisplay().done(function () {
                loadCartMarketingCampaigns();
            });
        });
    $(document).on("keydown", ".cart-reward-card", function (event) {
        if (event.key === "Enter" || event.key === " ") { event.preventDefault(); $(this).trigger("click"); }
    });
    $(document).on("click", ".js-reward-minus", function (event) {
        event.stopPropagation(); changeRewardQuantity($(this).closest(".cart-reward-card").attr("data-selection-key"), -1);
    });
    $(document).on("click", ".js-reward-plus", function (event) {
        event.stopPropagation(); changeRewardQuantity($(this).closest(".cart-reward-card").attr("data-selection-key"), 1);
    });

    Object.assign(cart.Marketing, {
        loadCartMarketingCampaigns: loadCartMarketingCampaigns,
        calculateOrderDiscount: calculateOrderDiscount,
        renderOrderDiscount: renderOrderDiscount,
        refreshRewardCampaigns: refreshRewardCampaigns,
        openProductAddOnModal: openProductAddOnModal,
        requireProductAddOnAdjustment: requireProductAddOnAdjustment,
        validateProductAddOnBeforeNext: validateProductAddOnBeforeNext,
        refreshProductAddOnPrompt: refreshProductAddOnPrompt,
        changeProductAddOnDraft: changeProductAddOnDraft,
        saveProductAddOnDrafts: saveProductAddOnDrafts,
        getRewardSelections: getRewardSelections
    });

})(window.ShoppingCart, window.jQuery);
