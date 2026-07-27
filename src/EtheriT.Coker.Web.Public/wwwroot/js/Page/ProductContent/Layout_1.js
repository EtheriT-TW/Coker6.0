(function ($) {
    'use strict';

    const M = window.ProductContentModule;

    if (!M) {
        console.error('Layout_1: ProductContentModule 未載入，請確認 ProductContent.min.js 在本檔之前載入。');
        return;
    }

    const normalizeNullableInt = M.normalizeNullableInt;
    const formatNumber = M.formatNumber;
    const formatPriceText = M.formatPriceText;
    const cloneTemplate = M.cloneTemplate;
    const analyzeSpecStructure = M.analyzeSpecStructure;
    const isLoggedIn = M.isLoggedIn;
    const isStockAvailable = M.isStockAvailable;
    const runBuyGuard = M.runBuyGuard;
    const submitCart = M.submitCart;

    const SELECTORS = {
        options: '.options',
        priceFrame: '.priceframe',
        priceBaseMeta: '.price-base-meta',
        counter: '.counter',
        quantityInput: '.input_pro_quantity',
        quantityWrap: '.counter_input',
        addToCartButton: '.btn_addToCar'
    };

    const TEMPLATES = {
        specRadio: '#Template_Spec_Radio',
        priceItem: '#PriceListTemplate'
    };

    function buildPriceViewModel(priceItem, stock, controller, product) {
        const currentPrice = normalizeNullableInt(priceItem.price);
        const currentBonus = normalizeNullableInt(priceItem.bonus);
        const originalPrice = normalizeNullableInt(priceItem.oriPrice);
        const suggestPrice = normalizeNullableInt(stock.suggestPrice);
        const isTimePrice = !!stock.timePrice;
        const disabled = !!priceItem.disabled;

        const itemRoleName = priceItem.roleName || '';
        const baseRoleName = product.baseRoleName || priceItem.baseRoleName || '非會員';

        const saleText = isTimePrice
            ? controller.t('marketPrice')
            : formatPriceText(currentPrice, currentBonus);

        const showSuggestPrice =
            !isTimePrice &&
            suggestPrice > 0 &&
            suggestPrice !== currentPrice;

        const showOriginalPrice =
            !isTimePrice &&
            originalPrice > 0 &&
            originalPrice !== currentPrice;

        const originalPriceText = showOriginalPrice
            ? `${baseRoleName} $${formatNumber(originalPrice)}`
            : '';

        const showBonusLack =
            disabled &&
            currentBonus > 0;

        // 核心規則：
        // 只有「這筆價格有自己的角色名稱，且它不是基準角色」時才顯示
        const showRoleName =
            !!itemRoleName &&
            itemRoleName !== baseRoleName;

        return {
            saleText,
            roleName: itemRoleName,
            showRoleName,
            showSuggestPrice,
            suggestPriceText: showSuggestPrice
                ? `${controller.t('suggestedPrice')}$${formatNumber(suggestPrice)}`
                : '',
            showOriginalPrice,
            originalPriceText,
            showBonusLack
        };
    }

    function buildPriceBaseViewModel(stock, priceOptions, controller, product) {
        stock = stock || {};
        const safePrices = Array.isArray(priceOptions) ? priceOptions : [];

        const suggestPrice = normalizeNullableInt(stock.suggestPrice);
        const baseRoleName =
            product?.baseRoleName ||
            safePrices.map(x => x.baseRoleName).find(x => !!x) ||
            '非會員';

        const originalPrice = safePrices
            .map(x => normalizeNullableInt(x.oriPrice))
            .find(x => x > 0) || 0;

        const currentPrices = safePrices.map(x => normalizeNullableInt(x.price));
        const currentBonuses = safePrices.map(x => normalizeNullableInt(x.bonus));

        const hasSameSuggestPrice = currentPrices.some(x => x === suggestPrice);
        const hasSameOriginalPrice = currentPrices.some((price, index) =>
            price === originalPrice && currentBonuses[index] === 0
        );

        const showSuggestPrice =
            !stock.timePrice &&
            suggestPrice > 0 &&
            !hasSameSuggestPrice;

        const showOriginalPrice =
            !stock.timePrice &&
            originalPrice > 0 &&
            !hasSameOriginalPrice &&
            originalPrice !== suggestPrice;

        return {
            showSuggestPrice,
            suggestPriceLabel: controller.t('suggestedPrice'),
            suggestPriceValue: showSuggestPrice
                ? `$${formatNumber(suggestPrice)}`
                : '',

            showOriginalPrice,
            originalPriceLabel: `${baseRoleName}價`,
            originalPriceValue: showOriginalPrice
                ? `$${formatNumber(originalPrice)}`
                : ''
        };
    }

    function createLayout1(controller) {
        const $pageRoot = controller.$pageRoot;
        const $root = controller.$root;
        const $quantityInput = $root.find(SELECTORS.quantityInput);
        const $quantityWrap = $root.find(SELECTORS.quantityWrap);
        const $addToCartButton = $pageRoot.find(SELECTORS.addToCartButton);

        function renderSelectionArea() {
            renderSpecs();
            renderPrices();
            renderQuantity();
            syncButtonState();

            if (typeof controller.options.hooks.onSelectionChanged === 'function') {
                controller.options.hooks.onSelectionChanged(controller.state.selection, controller);
            }
        }

        function renderSpecs() {
            const $options = $root.find(SELECTORS.options);
            $options.find('.radio').remove();

            const stocks = controller.state.selection.stocks || [];
            const specInfo = analyzeSpecStructure(stocks);

            if (specInfo.mode === 'none') {
                return;
            }

            const spec1Options = controller.state.selection.getSpec1Options();
            const spec2Options = controller.state.selection.getSpec2Options(controller.state.selection.current.s1);

            if (specInfo.mode === 'double' && spec2Options.length > 0) {
                const $spec2 = cloneTemplate(TEMPLATES.specRadio).attr('data-stype', '2');
                const $control = $spec2.find('.spec_control');

                spec2Options.forEach(item => {
                    const checked = item.id === controller.state.selection.current.s2 ? 'checked' : '';
                    const disabled = item.enabled ? '' : 'disabled="disabled"';
                    $control.append(`
                        <input id="s2_${item.id}" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="${item.id}" ${checked} ${disabled}>
                    `);
                    $control.append(`
                        <label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s2_${item.id}">
                            ${item.title}
                        </label>
                    `);
                });

                $options.prepend($spec2);
            }

            if (specInfo.mode === 'single' || specInfo.mode === 'double') {
                const $spec1 = cloneTemplate(TEMPLATES.specRadio).attr('data-stype', '1');
                const $spec1Control = $spec1.find('.spec_control');

                spec1Options.forEach(item => {
                    const checked = item.id === controller.state.selection.current.s1 ? 'checked' : '';
                    const disabled = item.enabled ? '' : 'disabled="disabled"';
                    $spec1Control.append(`
                        <input id="s1_${item.id}" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="${item.id}" ${checked} ${disabled}>
                    `);
                    $spec1Control.append(`
                        <label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s1_${item.id}">
                            ${item.title}
                        </label>
                    `);
                });

                $options.prepend($spec1);
            }
        }

        function renderPriceBaseMeta(priceOptions) {
            const $baseMeta = $root.find(SELECTORS.priceBaseMeta);
            const $suggestPrice = $baseMeta.find('.suggest-price');
            const $originalPrice = $baseMeta.find('.original-price');

            if (!$baseMeta.length) return;

            const stock = controller.state.selection.getActiveStock();
            const vm = buildPriceBaseViewModel(stock, priceOptions, controller, controller.state.product);

            let hasMeta = false;

            if (vm.showSuggestPrice) {
                $suggestPrice
                    .removeClass('d-none')
                    .empty()
                    .append($('<span/>', {
                        class: 'price-meta-label',
                        text: vm.suggestPriceLabel
                    }))
                    .append($('<span/>', {
                        class: 'price-meta-value',
                        text: vm.suggestPriceValue
                    }));

                hasMeta = true;
            } else {
                $suggestPrice.addClass('d-none').empty();
            }

            if (vm.showOriginalPrice) {
                $originalPrice
                    .removeClass('d-none')
                    .empty()
                    .append($('<span/>', {
                        class: 'price-meta-label',
                        text: vm.originalPriceLabel
                    }))
                    .append($('<span/>', {
                        class: 'price-meta-value',
                        text: vm.originalPriceValue
                    }));

                hasMeta = true;
            } else {
                $originalPrice.addClass('d-none').empty();
            }

            $baseMeta.toggleClass('d-none', !hasMeta);
        }

        function renderPrices() {
            const $priceFrame = $root.find(SELECTORS.priceFrame).empty();
            const priceOptions = controller.state.selection.getPriceOptions();
            const hasMultiplePrice = priceOptions.length > 1;

            renderPriceBaseMeta(priceOptions);

            if (!priceOptions.length) {
                $addToCartButton.addClass('d-none');
                $priceFrame.addClass('d-none');
                $root.find(SELECTORS.options).addClass('d-none');
                $root.find(SELECTORS.priceBaseMeta).addClass('d-none');
                return;
            }

            $priceFrame.removeClass('d-none');
            $root.find(SELECTORS.options).removeClass('d-none');

            priceOptions.forEach((item, index) => {
                const $price = cloneTemplate(TEMPLATES.priceItem);

                const $input = $price.find('.price-option-input');
                const $label = $price.find('.price-option-label');
                const $roleBadge = $price.find('.price-role-badge');
                const $roleName = $price.find('.price-role-name');
                const $saleRoleName = $price.find('.sale-role-name');
                const $salePrice = $price.find('.sale-price');
                const $sub = $price.find('.price-option-sub');
                const $badge = $price.find('.price-badge');
                const $hint = $price.find('.price-hint');

                const id = `price_${item.id || index}`;
                const stock = item.stock;
                const vm = buildPriceViewModel(item, stock, controller, controller.state.product);

                const stockAvailable =
                    stock &&
                    !stock.timePrice &&
                    isStockAvailable(stock, controller.state.selection.noStockManagement);

                const isSelectable =
                    hasMultiplePrice &&
                    stockAvailable &&
                    controller.state.selection.canAddToCart();

                $price.toggleClass('is-multi-price', hasMultiplePrice);
                $price.toggleClass('is-single-price', !hasMultiplePrice);
                $price.toggleClass('is-selectable', isSelectable);

                $input
                    .attr('id', id)
                    .attr('name', 'priceRadio')
                    .data('priceid', item.id)
                    .prop('disabled', !!item.disabled)
                    .prop('checked', !!item.checked)
                    .toggleClass('d-none', !isSelectable);

                $label.attr('for', id);

                // multi: 多價格一律使用 price-role-badge 顯示角色名稱
                if (hasMultiplePrice && vm.showRoleName) {
                    $roleBadge.removeClass('d-none');
                    $roleName.text(vm.roleName);
                    $price.addClass('has-role-badge');
                } else {
                    $roleBadge.addClass('d-none');
                    $roleName.text('');
                    $price.removeClass('has-role-badge');
                }

                // single: 只有單價才把角色名稱放在金額前
                if (!hasMultiplePrice && vm.showRoleName) {
                    $saleRoleName.removeClass('d-none').text(`${vm.roleName} `);
                } else {
                    $saleRoleName.addClass('d-none').text('');
                }

                // 每一筆價格方案只顯示自己的實際售價
                $salePrice
                    .text(vm.saleText)
                    .removeClass('bonus_lack');

                let hasSub = false;

                if (vm.showBonusLack) {
                    $badge.removeClass('d-none').text(controller.t('bonusInsufficient'));
                    $salePrice.addClass('bonus_lack');
                    hasSub = true;
                } else {
                    $badge.addClass('d-none').text('');
                }

                if ($hint.length > 0) {
                    $hint.addClass('d-none').text('');
                }

                if (hasSub) {
                    $sub.removeClass('d-none');
                } else {
                    $sub.addClass('d-none');
                }

                $priceFrame.append($price);
            });
        }

        function renderQuantity() {
            const stock = controller.state.selection.getActiveStock();
            if (!stock) return;

            const noStock = controller.state.selection.noStockManagement;
            const min = stock.minQty;

            $quantityInput.attr({ min, step: stock.minQty }).val(controller.state.selection.current.quantity);

            if (noStock) {
                $quantityInput.removeAttr('max');
            } else {
                $quantityInput.attr('max', stock.stock - (stock.stock % stock.minQty));
            }

            if (!noStock && stock.stock < stock.minQty) {
                $quantityWrap.addClass('isEmpty');
            } else {
                $quantityWrap.removeClass('isEmpty');
            }
        }

        function syncButtonState() {
            const stock = controller.state.selection.getActiveStock();
            const canAdd = controller.state.selection.canAddToCart();
            const priceOptions = controller.state.selection.getPriceOptions();
            const selectedPrice = priceOptions.find(
                x => normalizeNullableInt(x.id) === normalizeNullableInt(controller.state.selection.current.priceId)
            );
            const loggedIn = isLoggedIn();
            const selectedIsBonusLack =
                loggedIn &&
                !!selectedPrice &&
                !!selectedPrice.disabled &&
                normalizeNullableInt(selectedPrice.bonus) > 0;

            $addToCartButton.removeClass('close bonus_lack');

            if (!controller.options.canShop || !stock || stock.timePrice) {
                $addToCartButton.addClass('close');
            } else if (!canAdd) {
                if (selectedIsBonusLack) {
                    $addToCartButton.addClass('bonus_lack');
                } else {
                    $addToCartButton.addClass('close');
                }
            }

            if (!controller.options.canShop || !stock) {
                $root.find(SELECTORS.counter).addClass('d-none');
            } else {
                $root.find(SELECTORS.counter).removeClass('d-none');
            }
        }

        function addToCart() {
            if (typeof controller.options.hooks.beforeAddToCart === 'function') {
                const shouldContinue = controller.options.hooks.beforeAddToCart(controller);
                if (shouldContinue === false) return;
            }

            const t = controller.t.bind(controller);
            const priceOptions = controller.state.selection.getPriceOptions();
            const selectedPrice = priceOptions.find(
                x => normalizeNullableInt(x.id) === normalizeNullableInt(controller.state.selection.current.priceId)
            );

            const passed = runBuyGuard({
                t: t,
                bonus: selectedPrice ? selectedPrice.bonus : 0,
                totalBonus: controller.options.totalBonus
            });

            if (!passed) return;

            if (!controller.state.selection.canAddToCart()) {
                Coker.sweet.warning(
                    t('addCartWarningTitle'),
                    t('addCartNeedSelection')
                );
                return;
            }

            submitCart({
                t: t,
                api: controller.options.api,
                payload: controller.state.selection.buildCartPayload(controller.state.productId),
                onSuccess: function (result) {
                    controller.state.selection.decreaseStockAfterAdd();
                    controller.state.selection.setQuantity(controller.state.selection.getActiveStock()?.minQty || 1);
                    renderSelectionArea();

                    if (typeof controller.options.hooks.afterAddToCart === 'function') {
                        controller.options.hooks.afterAddToCart(result, controller);
                    }
                }
            });
        }

        function bindEvents() {
            $pageRoot.off('click.productContent', '.btn_count_plus').on('click.productContent', '.btn_count_plus', () => {
                const current = normalizeNullableInt($quantityInput.val(), 1);
                const step = normalizeNullableInt($quantityInput.attr('step'), 1);
                controller.state.selection.setQuantity(current + step);
                renderQuantity();
            });

            $pageRoot.off('click.productContent', '.btn_count_minus').on('click.productContent', '.btn_count_minus', () => {
                const current = normalizeNullableInt($quantityInput.val(), 1);
                const step = normalizeNullableInt($quantityInput.attr('step'), 1);
                controller.state.selection.setQuantity(current - step);
                renderQuantity();
            });

            $pageRoot.off('change.productContent', SELECTORS.quantityInput).on('change.productContent', SELECTORS.quantityInput, (e) => {
                controller.state.selection.setQuantity($(e.currentTarget).val());
                renderQuantity();
            });

            $pageRoot.off('click.productContent', SELECTORS.addToCartButton).on('click.productContent', SELECTORS.addToCartButton, () => {
                addToCart();
            });

            $pageRoot.off('change.productContent', 'input[name="S1_Radio"]').on('change.productContent', 'input[name="S1_Radio"]', (e) => {
                controller.state.selection.setSpec(1, $(e.currentTarget).val());
                renderSelectionArea();
            });

            $pageRoot.off('change.productContent', 'input[name="S2_Radio"]').on('change.productContent', 'input[name="S2_Radio"]', (e) => {
                controller.state.selection.setSpec(2, $(e.currentTarget).val());
                renderSelectionArea();
            });

            $pageRoot.off('change.productContent', 'input[name="priceRadio"]').on('change.productContent', 'input[name="priceRadio"]', (e) => {
                controller.state.selection.setPrice($(e.currentTarget).data('priceid'));
                syncButtonState();
            });
        }

        return {
            bindEvents,
            renderSelectionArea
        };
    }

    M.registerLayout(createLayout1);

})(window.jQuery);