(function (window, $) {
    'use strict';

    if (!$) return;

    const state = {
        campaigns: [],
        selected: new Map(),
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

    function campaignLimit(campaign, purchaseQuantity) {
        const perQualification = Math.max(number(read(campaign, 'selectionQuantityPerQualification', 'SelectionQuantityPerQualification')), 1);
        return qualificationCount(campaign, purchaseQuantity) * perQualification;
    }

    function itemLimit(campaign, $card, purchaseQuantity) {
        const configuredLimit = Math.max(number($card.data('maxQuantity')), 1);
        const repeatable = read(campaign, 'repeatable', 'Repeatable') === true;
        return repeatable
            ? configuredLimit * qualificationCount(campaign, purchaseQuantity)
            : configuredLimit;
    }

    function selectedForCampaign(campaignId) {
        return Array.from(state.selected.values())
            .filter(x => x.campaignId === campaignId)
            .reduce((sum, x) => sum + x.quantity, 0);
    }

    function trimSelection(campaign, limit) {
        const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
        let remaining = limit;
        Array.from(state.selected.entries())
            .filter(([, value]) => value.campaignId === campaignId)
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
            const required = Math.max(number(read(campaign, 'requiredQuantity', 'RequiredQuantity')), 1);
            const limit = campaignLimit(campaign, state.purchaseQuantity);
            const selected = selectedForCampaign(campaignId);
            const selectionFull = limit > 0 && selected >= limit;
            const $campaign = $root().find(`[data-campaign-id="${campaignId}"]`);
            const $rule = $campaign.find('.product-addon__rule');
            $campaign.toggleClass('is-single-choice', limit === 1);
            if (limit > 0) {
                $rule.text(`已符合資格，可選 ${limit} 件，目前已選 ${selected} 件`)
                    .removeClass('is-waiting').addClass('is-qualified');
            } else {
                $rule.text(`購買數量達 ${required} 件後，即可選擇優惠商品`)
                    .removeClass('is-qualified').addClass('is-waiting');
            }

            $campaign.find('[data-reward-item-id]').each(function () {
                const $card = $(this);
                const rewardItemId = number($card.data('rewardItemId'));
                const item = state.selected.get(rewardItemId);
                const disabled = limit <= 0 || (selectionFull && !item);
                $card.toggleClass('is-disabled', disabled).toggleClass('is-selected', !!item);
                $card.find('.product-addon__toggle')
                    .prop('disabled', disabled)
                    .text(item ? '已選取' : '選擇');
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
        $root().find('.product-addon__selection-summary').text(total > 0 ? `已選 ${total} 件` : '可不選');
    }

    function itemCard(campaign, item) {
        const campaignId = number(read(campaign, 'campaignId', 'CampaignId'));
        const rewardItemId = number(read(item, 'rewardItemId', 'RewardItemId'));
        const offerPrice = number(read(item, 'offerPrice', 'OfferPrice'));
        const originalPrice = number(read(item, 'originalPrice', 'OriginalPrice'));
        const $card = $('<article class="product-addon__card"></article>')
            .attr('data-reward-item-id', rewardItemId)
            .data({ campaignId, rewardItemId, maxQuantity: Math.max(number(read(item, 'maxQuantityPerOrder', 'MaxQuantityPerOrder')), 1) });

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
            const required = Math.max(number(read(campaign, 'requiredQuantity', 'RequiredQuantity')), 1);
            const selectable = Math.max(number(read(campaign, 'selectionQuantityPerQualification', 'SelectionQuantityPerQualification')), 1);
            const $campaign = $('<div class="product-addon__campaign"></div>').attr('data-campaign-id', campaignId);
            const $header = $('<div class="product-addon__campaign-header"></div>');
            $header.append($('<h4 class="product-addon__campaign-name"></h4>').text(read(campaign, 'name', 'Name') || '限定優惠'));
            $header.append($('<span class="product-addon__hint"></span>').text(`任選滿 ${required} 件，可選 ${selectable} 件`));
            $campaign.append($header, '<div class="product-addon__rule"></div>');

            const $swiper = $('<div class="product-addon__swiper swiper"><div class="swiper-wrapper"></div></div>');
            const $wrapper = $swiper.find('.swiper-wrapper');
            (read(campaign, 'rewardItems', 'RewardItems') || []).forEach(item => $wrapper.append(itemCard(campaign, item)));
            $campaign.append($swiper);
            $container.append($campaign);

            if (typeof window.Swiper === 'function') {
                state.swipers.push(new window.Swiper($swiper.get(0), {
                    slidesPerView: 'auto',
                    spaceBetween: 12,
                    watchOverflow: true
                }));
            }
        });

        $root().removeClass('d-none');
        updateState(1);
    }

    function toggleItem($card) {
        if ($card.hasClass('is-disabled')) return;
        const rewardItemId = number($card.data('rewardItemId'));
        const campaignId = number($card.data('campaignId'));
        const campaign = state.campaigns.find(x => number(read(x, 'campaignId', 'CampaignId')) === campaignId);
        if (!campaign) return;

        if (state.selected.has(rewardItemId)) {
            state.selected.delete(rewardItemId);
        } else if (campaignLimit(campaign, state.purchaseQuantity) === 1) {
            Array.from(state.selected.entries())
                .filter(([, value]) => value.campaignId === campaignId)
                .forEach(([key]) => state.selected.delete(key));
            state.selected.set(rewardItemId, { campaignId, rewardItemId, quantity: 1 });
        } else if (selectedForCampaign(campaignId) < campaignLimit(campaign, state.purchaseQuantity)) {
            state.selected.set(rewardItemId, { campaignId, rewardItemId, quantity: 1 });
        } else {
            Coker.sweet.warning('請注意', '已達本次可選的優惠商品件數。');
        }
        updateState(state.purchaseQuantity);
    }

    function changeQuantity($card, delta) {
        const rewardItemId = number($card.data('rewardItemId'));
        const selected = state.selected.get(rewardItemId);
        if (!selected) return;
        const campaign = state.campaigns.find(x => number(read(x, 'campaignId', 'CampaignId')) === selected.campaignId);
        const maxItem = itemLimit(campaign, $card, state.purchaseQuantity);
        const maxCampaign = campaignLimit(campaign, state.purchaseQuantity);
        const otherSelected = selectedForCampaign(selected.campaignId) - selected.quantity;
        selected.quantity = Math.max(0, Math.min(selected.quantity + delta, maxItem, maxCampaign - otherSelected));
        if (selected.quantity === 0) state.selected.delete(rewardItemId);
        updateState(state.purchaseQuantity);
    }

    function applyToPayload(payload, purchaseQuantity) {
        updateState(purchaseQuantity);
        payload.RewardSelections = Array.from(state.selected.values()).map(x => ({
            CampaignId: x.campaignId,
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
                    render(campaigns);
                }
            });
    }

    window.ProductAddOnPurchase = { applyToPayload, updateQuantity: updateState, reset };
    $(init);
})(window, window.jQuery);
