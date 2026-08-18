(function (window, $) {
    'use strict';

    if (!$) return;

    const state = {
        campaigns: [],
        selected: new Map(),
        cartItems: [],
        purchaseQuantity: 1,
        swipers: []
    };

    const $root = () => $('[data-product-addon]').first();
    const number = value => Number.parseInt(value, 10) || 0;
    const money = value => number(value).toLocaleString('zh-TW');
    const read = (object, camel, pascal) => object?.[camel] ?? object?.[pascal];

    function qualificationCount(campaign, purchaseQuantity) {
        const required = Math.max(number(read(campaign, 'requiredQuantity', 'RequiredQuantity')), 1);
        const repeatable = read(campaign, 'repeatable', 'Repeatable') === true;
        return repeatable
            ? Math.floor(Math.max(purchaseQuantity, 0) / required)
            : purchaseQuantity >= required ? 1 : 0;
    }

    function qualifyingQuantity(campaign, purchaseQuantity) {
        const scopeIds = (read(campaign, 'scopeProductIds', 'ScopeProductIds') || []).map(number);
        const quantityInCart = state.cartItems.reduce((sum, item) => {
            const productId = number(read(item, 'pId', 'PId'));
            const isAdditional = read(item, 'isAdditional', 'IsAdditional') === true;
            return !isAdditional && scopeIds.includes(productId)
                ? sum + number(read(item, 'quantity', 'Quantity'))
                : sum;
        }, 0);
        return quantityInCart + Math.max(number(purchaseQuantity), 0);
    }

    function campaignLimit(campaign, purchaseQuantity) {
        const perQualification = Math.max(number(read(campaign, 'selectionQuantityPerQualification', 'SelectionQuantityPerQualification')), 1);
        return qualificationCount(campaign, qualifyingQuantity(campaign, purchaseQuantity)) * perQualification;
    }

    function itemLimit(campaign, $card, purchaseQuantity) {
        const configuredLimit = Math.max(number($card.data('maxQuantity')), 1);
        const repeatable = read(campaign, 'repeatable', 'Repeatable') === true;
        return repeatable
            ? configuredLimit * qualificationCount(campaign, qualifyingQuantity(campaign, purchaseQuantity))
            : configuredLimit;
    }

    function sameRule(selection, campaignId, ruleId) {
        return selection.campaignId === campaignId && selection.ruleId === ruleId;
    }

    function existingForRule(campaign) {
        const rewardItems = read(campaign, 'rewardItems', 'RewardItems') || [];
        const rewardItemIds = rewardItems.map(item => number(read(item, 'rewardItemId', 'RewardItemId')));
        const legacyRewardKeys = rewardItems.map(item => ({
            productStockId: number(read(item, 'productStockId', 'ProductStockId')),
            offerPrice: Number(read(item, 'offerPrice', 'OfferPrice')) || 0
        }));

        return state.cartItems.reduce((sum, item) => {
            if (read(item, 'isAdditional', 'IsAdditional') !== true) return sum;

            const rewardItemId = number(
                item?.fK_MarketingRewardItemId ??
                item?.fkMarketingRewardItemId ??
                item?.FK_MarketingRewardItemId);
            const productStockId = number(read(item, 'psId', 'PSId'));
            const price = Number(read(item, 'price', 'Price')) || 0;
            const belongsToRule = rewardItemId > 0
                ? rewardItemIds.includes(rewardItemId)
                : legacyRewardKeys.some(key => key.productStockId === productStockId && key.offerPrice === price);
            return belongsToRule ? sum + number(read(item, 'quantity', 'Quantity')) : sum;
        }, 0);
    }

    function existingForItem(rewardItemId, productStockId, offerPrice) {
        return state.cartItems.reduce((sum, item) => {
            if (read(item, 'isAdditional', 'IsAdditional') !== true) return sum;
            const cartRewardItemId = number(
                item?.fK_MarketingRewardItemId ??
                item?.fkMarketingRewardItemId ??
                item?.FK_MarketingRewardItemId);
            const matches = cartRewardItemId > 0
                ? cartRewardItemId === rewardItemId
                : number(read(item, 'psId', 'PSId')) === productStockId &&
                  (Number(read(item, 'price', 'Price')) || 0) === offerPrice;
            return matches ? sum + number(read(item, 'quantity', 'Quantity')) : sum;
        }, 0);
    }

    function selectedForRule(campaign) {
        const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
        const ruleId = number(read(campaign, 'ruleId', 'RuleId'));
        return Array.from(state.selected.values())
            .filter(x => sameRule(x, campaignId, ruleId))
            .reduce((sum, x) => sum + x.quantity, existingForRule(campaign));
    }

    function trimSelection(campaign, limit) {
        const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
        const ruleId = number(read(campaign, 'ruleId', 'RuleId'));
        let remaining = Math.max(limit - existingForRule(campaign), 0);
        Array.from(state.selected.entries())
            .filter(([, value]) => sameRule(value, campaignId, ruleId))
            .forEach(([key, value]) => {
                if (remaining <= 0) {
                    state.selected.delete(key);
                    return;
                }
                value.quantity = Math.min(value.quantity, remaining);
                remaining -= value.quantity;
            });
    }

    function updateState(purchaseQuantity) {
        state.purchaseQuantity = Math.max(number(purchaseQuantity), 1);
        state.campaigns.forEach(campaign => trimSelection(campaign, campaignLimit(campaign, state.purchaseQuantity)));

        state.campaigns.forEach(campaign => {
            const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
            const ruleId = number(read(campaign, 'ruleId', 'RuleId'));
            const required = Math.max(number(read(campaign, 'requiredQuantity', 'RequiredQuantity')), 1);
            const totalQualifyingQuantity = qualifyingQuantity(campaign, state.purchaseQuantity);
            const limit = campaignLimit(campaign, state.purchaseQuantity);
            const selected = selectedForRule(campaign);
            const selectionFull = limit > 0 && selected >= limit;
            const $campaign = $root().find(`[data-campaign-id="${campaignId}"][data-rule-id="${ruleId}"]`);
            const $rule = $campaign.find('.product-addon__rule');
            $campaign.toggleClass('is-single-choice', limit === 1);
            if (limit > 0) {
                $rule.text(`購物車與本次商品合計 ${totalQualifyingQuantity} 件，可選 ${limit} 件，目前已選 ${selected} 件`)
                    .removeClass('is-waiting').addClass('is-qualified');
            } else {
                $rule.text(`購買數量達 ${required} 件後，即可選擇優惠商品`)
                    .removeClass('is-qualified').addClass('is-waiting');
            }

            $campaign.find('[data-reward-item-id]').each(function () {
                const $card = $(this);
                const rewardItemId = number($card.data('rewardItemId'));
                const productStockId = number($card.data('productStockId'));
                const offerPrice = Number($card.data('offerPrice')) || 0;
                const item = state.selected.get(rewardItemId);
                const existingQuantity = existingForItem(rewardItemId, productStockId, offerPrice);
                const itemSelected = !!item || existingQuantity > 0;
                const itemFull = existingQuantity + (item?.quantity || 0) >= itemLimit(campaign, $card, state.purchaseQuantity);
                const disabled = limit <= 0 || (selectionFull && !item) || (itemFull && !item);
                $card.toggleClass('is-disabled', disabled).toggleClass('is-selected', itemSelected);
                $card.find('.product-addon__toggle')
                    .prop('disabled', disabled)
                    .text(item ? '已選取' : existingQuantity > 0 ? `購物車已有 ${existingQuantity} 件` : '選擇');
                const canAdjustQuantity = !!item && limit > 1 && itemLimit(campaign, $card, state.purchaseQuantity) > 1;
                $card.find('.product-addon__quantity').toggleClass('d-none', !canAdjustQuantity);
                $card.find('.product-addon__qty-value').text(item?.quantity || 0);
                if (item) {
                    const maxQuantity = Math.min(
                        itemLimit(campaign, $card, state.purchaseQuantity),
                        limit - selected + item.quantity);
                    $card.find('.product-addon__qty-button[data-delta="1"]')
                        .prop('disabled', item.quantity >= maxQuantity);
                }
            });
        });

        const total = Array.from(state.selected.values()).reduce((sum, x) => sum + x.quantity, 0);
        $root().find('.product-addon__selection-summary').text(total > 0 ? `已選 ${total} 件` : '');
    }

    function itemCard(campaign, item) {
        const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
        const ruleId = number(read(campaign, 'ruleId', 'RuleId'));
        const rewardItemId = number(read(item, 'rewardItemId', 'RewardItemId'));
        const productStockId = number(read(item, 'productStockId', 'ProductStockId'));
        const offerPrice = number(read(item, 'offerPrice', 'OfferPrice'));
        const originalPrice = number(read(item, 'originalPrice', 'OriginalPrice'));
        const $card = $('<article class="product-addon__card"></article>')
            .attr('data-reward-item-id', rewardItemId)
            .data({
                campaignId,
                ruleId,
                rewardItemId,
                productStockId,
                offerPrice,
                maxQuantity: Math.max(number(read(item, 'maxQuantityPerOrder', 'MaxQuantityPerOrder')), 1)
            });

        const $image = $('<img class="product-addon__image" alt="" loading="lazy" />')
            .attr('src', read(item, 'imageUrl', 'ImageUrl') || '/images/noImg.jpg')
            .on('error', function () {
                if (!String(this.src).endsWith('/images/noImg.jpg')) this.src = '/images/noImg.jpg';
            });
        $card.append($image);
        $card.append($('<span class="product-addon__benefit-tag"></span>').text(offerPrice === 0 ? '贈品' : '加價購'));
        $card.append($('<div class="product-addon__product-name"></div>').text(read(item, 'productName', 'ProductName') || '優惠商品'));
        $card.append($('<div class="product-addon__stock-name"></div>').text(read(item, 'stockName', 'StockName') || ''));

        const $price = $('<div class="product-addon__price-row"></div>');
        $price.append($('<span class="product-addon__offer"></span>').text(offerPrice === 0 ? '免費贈送' : `NT$ ${money(offerPrice)}`));
        if (originalPrice > 0 && originalPrice !== offerPrice) {
            $price.append($('<span class="product-addon__original"></span>').text(`原價 NT$ ${money(originalPrice)}`));
        }
        $card.append($price);
        $card.append('<button type="button" class="product-addon__toggle">選擇</button>');
        $card.append('<div class="product-addon__quantity d-none"><button type="button" class="product-addon__qty-button" data-delta="-1" aria-label="減少">−</button><span class="product-addon__qty-value">1</span><button type="button" class="product-addon__qty-button" data-delta="1" aria-label="增加">＋</button></div>');

        return $('<div class="swiper-slide"></div>').append($card);
    }

    function render(campaigns) {
        const $container = $root().find('.product-addon__campaigns').empty();
        state.swipers.forEach(swiper => swiper?.destroy?.(true, true));
        state.swipers = [];

        campaigns.forEach(campaign => {
            const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
            const ruleId = number(read(campaign, 'ruleId', 'RuleId'));
            const required = Math.max(number(read(campaign, 'requiredQuantity', 'RequiredQuantity')), 1);
            const selectable = Math.max(number(read(campaign, 'selectionQuantityPerQualification', 'SelectionQuantityPerQualification')), 1);
            const $campaign = $('<div class="product-addon__campaign"></div>')
                .attr({ 'data-campaign-id': campaignId, 'data-rule-id': ruleId });
            const $header = $('<div class="product-addon__campaign-header"></div>');
            $header.append($('<h4 class="product-addon__campaign-name"></h4>').text(read(campaign, 'name', 'Name') || '限定優惠'));
            $header.append($('<span class="product-addon__hint"></span>').text(`任選滿 ${required} 件，可選 ${selectable} 件`));
            $campaign.append($header, '<div class="product-addon__rule"></div>');

            const $swiper = $(
                '<div class="product-addon__swiper swiper">' +
                    '<div class="swiper-wrapper"></div>' +
                    '<button type="button" class="product-addon__nav product-addon__nav--prev" aria-label="上一組優惠商品">' +
                        '<i class="fa-solid fa-chevron-left" aria-hidden="true"></i>' +
                    '</button>' +
                    '<button type="button" class="product-addon__nav product-addon__nav--next" aria-label="下一組優惠商品">' +
                        '<i class="fa-solid fa-chevron-right" aria-hidden="true"></i>' +
                    '</button>' +
                    '<div class="product-addon__pagination swiper-pagination" aria-label="優惠商品分頁"></div>' +
                '</div>');
            const $wrapper = $swiper.find('.swiper-wrapper');
            (read(campaign, 'rewardItems', 'RewardItems') || []).forEach(item => $wrapper.append(itemCard(campaign, item)));
            $campaign.append($swiper);
            $container.append($campaign);

            if (typeof window.Swiper === 'function') {
                const swiper = new window.Swiper($swiper.get(0), {
                    slidesPerView: 'auto',
                    spaceBetween: 12,
                    watchOverflow: true,
                    a11y: true,
                    navigation: {
                        prevEl: $swiper.find('.product-addon__nav--prev').get(0),
                        nextEl: $swiper.find('.product-addon__nav--next').get(0)
                    },
                    pagination: {
                        el: $swiper.find('.product-addon__pagination').get(0),
                        clickable: true
                    },
                    on: {
                        lock: function () { $swiper.addClass('is-locked'); },
                        unlock: function () { $swiper.removeClass('is-locked'); }
                    }
                });
                $swiper.toggleClass('is-locked', swiper.isLocked === true);
                state.swipers.push(swiper);
            }
        });

        $root().removeClass('d-none');
        updateState(1);
    }

    function toggleItem($card) {
        if ($card.hasClass('is-disabled')) return;
        const rewardItemId = number($card.data('rewardItemId'));
        const campaignId = number($card.data('campaignId'));
        const ruleId = number($card.data('ruleId'));
        const campaign = state.campaigns.find(x =>
            number(read(x, 'campaignId', 'CampaignId')) === campaignId &&
            number(read(x, 'ruleId', 'RuleId')) === ruleId);
        if (!campaign) return;

        if (state.selected.has(rewardItemId)) {
            state.selected.delete(rewardItemId);
        } else if (campaignLimit(campaign, state.purchaseQuantity) === 1) {
            Array.from(state.selected.entries())
                .filter(([, value]) => sameRule(value, campaignId, ruleId))
                .forEach(([key]) => state.selected.delete(key));
            state.selected.set(rewardItemId, { campaignId, ruleId, rewardItemId, quantity: 1 });
        } else if (selectedForRule(campaign) < campaignLimit(campaign, state.purchaseQuantity)) {
            state.selected.set(rewardItemId, { campaignId, ruleId, rewardItemId, quantity: 1 });
        } else {
            Coker.sweet.warning('請注意', '已達本次可選的優惠商品件數。');
        }
        updateState(state.purchaseQuantity);
    }

    function changeQuantity($card, delta) {
        const rewardItemId = number($card.data('rewardItemId'));
        const selected = state.selected.get(rewardItemId);
        if (!selected) return;
        const campaign = state.campaigns.find(x =>
            number(read(x, 'campaignId', 'CampaignId')) === selected.campaignId &&
            number(read(x, 'ruleId', 'RuleId')) === selected.ruleId);
        const maxItem = itemLimit(campaign, $card, state.purchaseQuantity);
        const maxCampaign = campaignLimit(campaign, state.purchaseQuantity);
        const otherSelected = selectedForRule(campaign) - selected.quantity;
        selected.quantity = Math.max(0, Math.min(selected.quantity + delta, maxItem, maxCampaign - otherSelected));
        if (selected.quantity === 0) state.selected.delete(rewardItemId);
        updateState(state.purchaseQuantity);
    }

    function applyToPayload(payload, purchaseQuantity) {
        updateState(purchaseQuantity);
        payload.RewardSelections = Array.from(state.selected.values()).map(x => ({
            CampaignId: x.campaignId,
            RuleId: x.ruleId,
            RewardItemId: x.rewardItemId,
            Quantity: x.quantity
        }));
        return payload;
    }

    function reset() {
        state.selected.clear();
        updateState(state.purchaseQuantity);
    }

    function init() {
        if (!$root().length || !window.PageId) return;
        $root().on('click', '.product-addon__toggle', function () { toggleItem($(this).closest('.product-addon__card')); });
        $root().on('click', '.product-addon__card', function (event) {
            if ($(event.target).closest('.product-addon__toggle, .product-addon__quantity').length) return;
            toggleItem($(this));
        });
        $root().on('click', '.product-addon__qty-button', function () { changeQuantity($(this).closest('.product-addon__card'), number($(this).data('delta'))); });
        $(document).on('change.productAddon input.productAddon', '.input_pro_quantity, .spec-qty-input', function () { updateState($(this).val()); });

        $.get('/api/Marketing/GetProductAddOnCampaigns', { productId: window.PageId })
            .done(result => {
                const campaigns = read(result, 'object', 'Object') || [];
                if (read(result, 'success', 'Success') !== false && campaigns.length) {
                    state.campaigns = campaigns;
                    const cartRequest = window.Product?.GetAll?.Cart
                        ? window.Product.GetAll.Cart()
                        : $.Deferred().resolve([]).promise();
                    cartRequest
                        .done(items => { state.cartItems = Array.isArray(items) ? items : []; })
                        .always(() => render(campaigns));
                }
            });
    }

    window.ProductAddOnPurchase = { applyToPayload, updateQuantity: updateState, reset };
    $(init);
})(window, window.jQuery);
