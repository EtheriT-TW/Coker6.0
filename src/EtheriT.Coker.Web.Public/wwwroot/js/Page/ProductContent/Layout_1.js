(function ($) {
    'use strict';

    const M = window.ProductContentModule;

    if (!M) {
        console.error('Layout_1: ProductContentModule 未載入，請確認 ProductContent.min.js 在本檔之前載入。');
        return;
    }

    const normalizeNullableInt = M.normalizeNullableInt;
    const cloneTemplate = M.cloneTemplate;
    const analyzeSpecStructure = M.analyzeSpecStructure;
    const isLoggedIn = M.isLoggedIn;
    const isStockAvailable = M.isStockAvailable;
    const runBuyGuard = M.runBuyGuard;
    const submitCart = M.submitCart;
    const buildPriceViewModel = M.buildPriceViewModel;
    const buildPriceBaseViewModel = M.buildPriceBaseViewModel;
    const specName = M.specName;
    const specImageItems = M.specImageItems;

    const SELECTORS = {
        options: '.options',
        priceFrame: '.priceframe',
        priceBaseMeta: '.price-base-meta',
        counter: '.counter',
        quantityInput: '.input_pro_quantity',
        quantityWrap: '.counter_input',
        emptyProduct: '.emptyProd',
        addToCartButton: '.btn_addToCar'
    };

    const TEMPLATES = {
        specRadio: '#Template_Spec_Radio',
        priceItem: '#PriceListTemplate'
    };

    const SPEC_SLIDE_SPEED_MS = 0;

    function createLayout1(controller) {
        const $pageRoot = controller.$pageRoot;
        const $root = controller.$root;
        const $quantityInput = $root.find(SELECTORS.quantityInput);
        const $quantityWrap = $root.find(SELECTORS.quantityWrap);
        const $emptyProduct = $root.find(SELECTORS.emptyProduct);
        const $addToCartButton = $pageRoot.find(SELECTORS.addToCartButton);

        // 規格 id → 該規格第一張圖在 ProductSwiper 的 slide 索引
        let specSlideIndexById = {};

        // 開頁網址帶的 psid 只能在這裡讀一次：使用者一旦切過規格，
        // syncVariantUrlFromSelection 就會持續改寫網址，之後再讀就不是「深連結」了。
        const deepLinkStockId = controller.getRequestedStockId();
        let deepLinkJumpDone = false;

        // 版型一：規格圖統一併進 ProductSwiper，接在商品主圖之後。
        // 規格圖只有原圖一種尺寸，medium / small / original 三個清單塞同一筆，
        // 維持 renderMedia 依賴的「三陣列等長同序」前提（data-index ↔ 燈箱索引）。
        function buildMediaLists(result) {
            const medium = Array.isArray(result.img_Medium) ? result.img_Medium.slice() : [];
            const small = Array.isArray(result.img_Small) ? result.img_Small.slice() : [];
            const original = Array.isArray(result.img_Original) ? result.img_Original.slice() : [];
            const stocks = Array.isArray(result.stocks) ? result.stocks : [];
            const indexById = {};

            stocks.forEach(stock => {
                const items = specImageItems(stock);
                if (!items.length) return;   // 沒有規格圖就略過，不補佔位圖

                indexById[stock.id] = medium.length;

                const altParts = [result.title, specName(stock)].filter(Boolean);
                items.forEach((item, index) => {
                    const entry = {
                        ...item,
                        alt: `${altParts.join(' - ')}${index > 0 ? ` - ${index + 1}` : ''}`
                    };
                    medium.push(entry);
                    small.push(entry);
                    original.push(entry);
                });
            });

            specSlideIndexById = indexById;

            return { medium, small, original };
        }

        function slideToActiveSpec() {
            const stock = controller.state.selection.getActiveStock();
            const swiper = controller.state.productSwiper;
            const index = stock ? specSlideIndexById[stock.id] : null;

            if (index == null || !swiper) return;

            if (typeof swiper.slideToLoop === 'function') {
                swiper.slideToLoop(index, SPEC_SLIDE_SPEED_MS);
            } else {
                swiper.slideTo(index, SPEC_SLIDE_SPEED_MS);
            }
        }

        // 帶 psid 進來時，首次渲染就把主圖停在該規格的圖上。
        // 只做一次：之後的重繪（加入購物車、切換商品）不該把使用者拉回去。
        function applyDeepLinkJump() {
            if (deepLinkJumpDone || !deepLinkStockId) return;

            deepLinkJumpDone = true;

            if (!controller.state.selection.initialStockMatched) return;

            slideToActiveSpec();
        }

        function renderSelectionArea() {
            renderSpecs();
            renderPrices();
            renderQuantity();
            syncButtonState();
            applyDeepLinkJump();

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
                    // 不能買（時價／庫存不足）只做視覺提示，不鎖住 input，否則點不動也就看不到該規格的圖
                    const unavailable = item.enabled ? '' : ' is-unavailable';
                    $control.append(`
                        <input id="s2_${item.id}" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="${item.id}" ${checked}>
                    `);
                    $control.append(`
                        <label class="btn_radio me-2 my-1 px-3 py-1 align-self-center${unavailable}" for="s2_${item.id}">
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
                    // 不能買（時價／庫存不足）只做視覺提示，不鎖住 input，否則點不動也就看不到該規格的圖
                    const unavailable = item.enabled ? '' : ' is-unavailable';
                    $spec1Control.append(`
                        <input id="s1_${item.id}" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="${item.id}" ${checked}>
                    `);
                    $spec1Control.append(`
                        <label class="btn_radio me-2 my-1 px-3 py-1 align-self-center${unavailable}" for="s1_${item.id}">
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
                        html: vm.suggestPriceValue
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
                        html: vm.originalPriceValue
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
                    .html(vm.saleText)
                    .removeClass('bonus_lack');

                let hasSub = false;

                if (vm.showBonusLack) {
                    $badge.removeClass('d-none').text(local.BonusInsufficient);
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
                $quantityWrap.removeClass('isLoading').addClass('isEmpty');
                $emptyProduct.text(controller.t('prodEmpty', '商品已售完'));
            } else {
                $quantityWrap.removeClass('isLoading isEmpty');
                $emptyProduct.text('');
            }

            if (window.ProductAddOnPurchase && typeof window.ProductAddOnPurchase.updateQuantity === 'function') {
                window.ProductAddOnPurchase.updateQuantity(controller.state.selection.current.quantity);
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

            // 不能買的規格（時價／庫存不足）仍保留數量列：售完由 renderQuantity 換成售完提示，
            // 能不能結帳一律由 canAddToCart() 與加入購物車按鈕把關。
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
                    local.AlertTitle,
                    local.AddCartNeedSelection
                );
                return;
            }

            let payload = controller.state.selection.buildCartPayload(controller.state.productId);
            if (window.ProductAddOnPurchase && typeof window.ProductAddOnPurchase.applyToPayload === 'function') {
                payload = window.ProductAddOnPurchase.applyToPayload(
                    payload,
                    controller.state.selection.current.quantity
                );
            }

            $addToCartButton.prop('disabled', true);
            submitCart({
                t: t,
                api: controller.options.api,
                payload: payload,
                onSuccess: function (result) {
                    controller.load();
                    if (window.ProductAddOnPurchase?.refreshCart) window.ProductAddOnPurchase.refreshCart();

                    if (typeof controller.options.hooks.afterAddToCart === 'function') {
                        controller.options.hooks.afterAddToCart(result, controller);
                    }
                },
                onAlways: function () { $addToCartButton.prop('disabled', false); }
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
                slideToActiveSpec();
                controller.syncVariantUrlFromSelection();
            });

            $pageRoot.off('change.productContent', 'input[name="S2_Radio"]').on('change.productContent', 'input[name="S2_Radio"]', (e) => {
                controller.state.selection.setSpec(2, $(e.currentTarget).val());
                renderSelectionArea();
                slideToActiveSpec();
                controller.syncVariantUrlFromSelection();
            });

            // 點已選取的規格不會觸發 change，這裡補上：讓使用者滑走圖片後還能點回來
            $pageRoot.off('click.productContent', 'input[name="S1_Radio"]').on('click.productContent', 'input[name="S1_Radio"]', (e) => {
                const value = normalizeNullableInt($(e.currentTarget).val());
                if (value !== normalizeNullableInt(controller.state.selection.current.s1)) return; // 換規格交給 change 處理
                slideToActiveSpec();
            });

            $pageRoot.off('click.productContent', 'input[name="S2_Radio"]').on('click.productContent', 'input[name="S2_Radio"]', (e) => {
                const value = normalizeNullableInt($(e.currentTarget).val());
                if (value !== normalizeNullableInt(controller.state.selection.current.s2)) return;
                slideToActiveSpec();
            });

            $pageRoot.off('change.productContent', 'input[name="priceRadio"]').on('change.productContent', 'input[name="priceRadio"]', (e) => {
                controller.state.selection.setPrice($(e.currentTarget).data('priceid'));
                syncButtonState();
            });
        }

        return {
            bindEvents,
            renderSelectionArea,
            buildMediaLists
        };
    }

    M.registerLayout(createLayout1);

})(window.jQuery);
