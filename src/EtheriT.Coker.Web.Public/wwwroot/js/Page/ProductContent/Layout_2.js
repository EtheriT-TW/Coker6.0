(function ($) {
    'use strict';

    const M = window.ProductContentModule;

    if (!M) {
        console.error('Layout_2: ProductContentModule 未載入，請確認 ProductContent.min.js 在本檔之前載入。');
        return;
    }

    const normalizeNullableInt = M.normalizeNullableInt;
    const clampQuantity = M.clampQuantity;
    const isStockAvailable = M.isStockAvailable;
    const readMinQty = M.readMinQty;
    const isLoggedIn = M.isLoggedIn;
    const runBuyGuard = M.runBuyGuard;
    const submitCart = M.submitCart;
    const formatNumber = M.formatNumber;

    // 沿用版型一的語系設定；額外保留舊 key（MarketPrice / ProdEmpty）的查詢，
    // 維持版型二原本「新 key 查不到就退回舊 key」的行為。
    const TEXT_OPTIONS = {
        texts: M.DEFAULT_TEXTS,
        i18n: function (key, fallback) {
            const legacyKeys = { marketPrice: 'MarketPrice', prodEmpty: 'ProdEmpty' };
            const legacyKey = legacyKeys[key];

            if (legacyKey && window.L && typeof window.L.get === 'function') {
                const value = window.L.get(key) || window.L.get(legacyKey);
                if (value) return value;
            }

            return M.defaultI18n(key, fallback);
        }
    };

    const COPY_FEEDBACK_MS = 1200;

    // 保存最近一次 render 的情境，供加購成功後重繪單列使用
    let renderContext = {
        vis: { showPrice: false, showBuy: false },
        noStockManagement: false,
        state: null,
        galleryItems: [],
        galleryStartIndex: {}
    };

    function t(key, fallback) {
        return M.resolveText(TEXT_OPTIONS, key, fallback);
    }

    function readState() {
        const s = window.ProductBuyState || {};
        return {
            storeBuyState: s.storeBuyState || '',
            isLogin: s.isLogin === true,
            loginEnable: s.loginEnable === true,
            shoppingEnable: s.shoppingEnable === true,
            hasShoppingCar: s.hasShoppingCar === true
        };
    }

    function resolveVisibility(state, product) {
        let showPrice = false;
        let showBuy = false;

        switch (state.storeBuyState) {
            case 'Pay':
                showPrice = true; showBuy = true; break;
            case 'menberPay':
                showPrice = state.isLogin; showBuy = state.isLogin; break;
            case 'noPay':
                showPrice = true; showBuy = false; break;
            case 'noPayNoShow':
                showPrice = false; showBuy = false; break;
            default:
                showPrice = false; showBuy = false; break;
        }

        showBuy = showBuy && state.shoppingEnable && state.hasShoppingCar;

        // 與版型一一致：狀態 2（停售）不開放購買
        if (product && normalizeNullableInt(product.status) === 2) {
            showBuy = false;
        }

        return { showPrice, showBuy };
    }

    // 顯示順序：非會員價（原價）在上，會員價在下。
    // 後端回傳是「目前角色的方案在前」，與畫面要的順序相反，這裡重排。
    // 同群組內維持後端原順序（現金價先於紅利價），用穩定排序不打亂。
    function sortPlans(prices) {
        const isGuestPrice = price => normalizeNullableInt(price.fK_RId ?? price.roleId) <= 1;

        return prices.slice().sort((a, b) => (isGuestPrice(a) ? 0 : 1) - (isGuestPrice(b) ? 0 : 1));
    }

    // 一個規格可能有多筆價格方案（非會員價 / 會員價 / 紅利價…），全部都要顯示。
    // 沒有 prices 時（例如時價規格）退回一筆假方案，讓版面仍有一列可畫。
    function getPlans(stock) {
        const prices = Array.isArray(stock.prices) ? stock.prices : [];
        if (prices.length > 0) return sortPlans(prices);

        return [{ id: 0, price: stock.price, bonus: 0 }];
    }

    // 已登入且紅利不足 → 該方案不可買（其他方案不受影響）。
    // 未登入的紅利方案不在這裡擋，交給 runBuyGuard 提示登入。
    function isBonusLack(price, state) {
        const bonus = normalizeNullableInt(price && price.bonus);
        if (bonus <= 0) return false;
        if (!isLoggedIn(state && state.isLogin)) return false;

        return normalizeNullableInt(window.totalBonus) < bonus;
    }

    // 建議售價是規格層級的值，整個規格只印一次。
    // 任何一列售價等於它就不印，避免「建議售價 $980 ／ 售價 $980」。
    function buildSuggestText(stock, plans) {
        if (stock.timePrice) return '';

        const suggest = normalizeNullableInt(stock.suggestPrice);
        if (suggest <= 0) return '';
        if (plans.some(p => normalizeNullableInt(p.price) === suggest)) return '';

        return '$' + formatNumber(suggest);
    }

    // 版型二的紅利文字不帶冒號。
    // 共用的 M.formatPriceText 版型一仍在用，不動它，這裡自己格式化。
    function formatPlanPriceText(price, bonus) {
        const money = normalizeNullableInt(price);
        const bonusValue = normalizeNullableInt(bonus);

        if (bonusValue <= 0) return '$' + formatNumber(money);
        if (money === 0) return local.Bonus + ' ' + formatNumber(bonusValue);

        return '$' + formatNumber(money) + ' + ' + local.Bonus + ' ' + formatNumber(bonusValue);
    }

    function specName(stock) {
        return [stock.s1_Title, stock.s2_Title].filter(Boolean).join(' / ');
    }

    function specImageItems(stock) {
        const mm = Array.isArray(stock.multimedia) ? stock.multimedia : [];
        return mm.filter(m => (m.fileType === 1 || m.fileType === 2) && Array.isArray(m.link) && m.link[0]);
    }

    function buyGuardPassed(price, state) {
        return runBuyGuard({
            t: t,
            bonus: price ? price.bonus : 0,
            totalBonus: window.totalBonus,
            isLoginFallback: state.isLogin
        });
    }

    function buildCartPayload(stock, price, qty) {
        return M.createCartPayload(window.PageId, {
            priceId: price ? price.id : 0,
            s1id: stock.fK_S1id ?? stock.s1id,
            s2id: stock.fK_S2id ?? stock.s2id,
            quantity: qty
        });
    }

    function submitAddToCart($btn, stock, price, qty) {
        const $card = $btn.closest('.spec-card');
        let payload = buildCartPayload(stock, price, qty);
        if (window.ProductAddOnPurchase && typeof window.ProductAddOnPurchase.applyToPayload === 'function') {
            payload = window.ProductAddOnPurchase.applyToPayload(payload, qty);
        }

        $btn.prop('disabled', true);

        submitCart({
            t: t,
            payload: payload,
            onSuccess: function () {
                if (window.productContentPage && typeof window.productContentPage.load === 'function') {
                    window.productContentPage.load();
                }
                if (window.ProductAddOnPurchase) window.ProductAddOnPurchase.reset();
            },
            onAlways: function () { $btn.prop('disabled', false); }
        });
    }

    function bindCopyName($container) {
        $container.on('click', '.copy_btn', function () {
            const $btn = $(this);
            const text = $btn.attr('data-copy');
            if (!text)
                return;

            if (!navigator.clipboard || !window.isSecureContext) {
                Coker.sweet.error(local.Error, local.ErrorCopyNotSupported, null, true);
                return;
            }

            navigator.clipboard.writeText(text)
                .then(function () {
                    const $icon = $btn.find('.copy-icon');
                    $icon.text('check');
                    setTimeout(function () { $icon.text('content_copy'); }, COPY_FEEDBACK_MS);
                    Coker.sweet.success(local.CopySuccess, null, false);
                })
                .catch(function () {
                    Coker.sweet.error(local.Error, local.CopyFailed, null, true);
                });
        });
    }

    function bindAddToCart($container, state) {
        $container.on('click', '.btn_spec_add', function () {
            const $btn = $(this);
            const $plan = $btn.closest('.spec-plan');
            const $card = $btn.closest('.spec-card');
            const stock = $card.data('stock');
            const price = $plan.data('price');
            if (!stock) return;
            if (!buyGuardPassed(price, state)) return;

            const $qty = $plan.find('.spec-qty-input');
            const qty = clampQuantity(stock, $qty.val(), $card.data('noStockManagement') === true);
            $qty.val(qty);

            submitAddToCart($btn, stock, price, qty);
        });
    }

    function buildPlanRow(stock, price, options) {
        const $plan = $('<div class="spec-plan"></div>').data('price', price);
        const bonusLack = isBonusLack(price, options.state);

        const $priceCell = $('<div class="spec-plan-price"></div>')
            .append($('<span class="spec-price"></span>').text(
                stock.timePrice ? local.MarketPrice : formatPlanPriceText(price.price, price.bonus)
            ));

        if (bonusLack) {
            $priceCell.append($('<span class="spec-plan-badge"></span>').text(local.BonusInsufficient));
        }

        $plan.append($priceCell);

        if (!options.showBuy) return $plan;

        // 售完 / 時價是整個規格的狀態，只在第一列標一次，其餘列只留價格
        if (!options.stockBuyable) {
            if (!options.isFirst) return $plan;

            $plan.append($('<div class="emptyProd"></div>').text(stock.timePrice ? '-' : local.ProdEmpty));
            $plan.append($('<button type="button" class="btn_spec_add bg-secondary" disabled></button>').text(local.AddtoCart));

            return $plan;
        }

        if (bonusLack) {
            // 佔位，讓按鈕仍落在第三欄，跨列不歪
            $plan.append('<div class="spec-qty-placeholder"></div>');
        } else {
            const minQty = readMinQty(stock);
            const stockCount = normalizeNullableInt(stock.stock);
            const maxAttr = options.noStockManagement
                ? ''
                : ' max="' + (stockCount - (stockCount % minQty)) + '"';

            $plan.append($(
                '<div class="spec-qty">' +
                '<button type="button" class="spec-qty-minus">−</button>' +
                '<input type="number" class="spec-qty-input" value="' + minQty + '" min="' + minQty + '" step="' + minQty + '"' + maxAttr + ' />' +
                '<button type="button" class="spec-qty-plus">+</button>' +
                '</div>'
            ));
        }

        const $btn = $('<button type="button" class="btn_spec_add"></button>').text(local.AddtoCart);
        if (bonusLack) $btn.addClass('bg-secondary').prop('disabled', true);
        $plan.append($btn);

        return $plan;
    }

    function buildActions(stock, vis, noStockManagement, state) {
        const $actions = $('<div class="spec-actions"></div>');
        if (!vis.showPrice) return $actions;   // noPayNoShow：價格與購買都不顯示

        const plans = getPlans(stock);
        const suggestText = buildSuggestText(stock, plans);

        if (suggestText) {
            $actions.append(
                $('<div class="spec-suggest"></div>')
                    .append($('<span class="spec-suggest-value text-decoration-line-through"></span>').text(suggestText))
            );
        }

        const stockBuyable = !stock.timePrice && isStockAvailable(stock, noStockManagement);

        plans.forEach((price, index) => {
            $actions.append(buildPlanRow(stock, price, {
                showBuy: vis.showBuy,
                stockBuyable: stockBuyable,
                noStockManagement: noStockManagement,
                state: state,
                isFirst: index === 0
            }));
        });

        if (!vis.showBuy) $actions.addClass('spec-actions-no-buy');

        return $actions;
    }

    function buildRow(stock, vis, noStockManagement, state) {
        const $card = $('<div class="spec-card"></div>')
            .attr('data-stock-id', stock.id)
            .data('stock', stock)
            .data('noStockManagement', !!noStockManagement);

        const imgItems = specImageItems(stock);
        if (imgItems.length) {
            $card.append(
                $('<img class="spec-thumb" alt="" />')
                    .attr('src', imgItems[0].link[0])
                    // 圖片清單改由 renderContext 統一持有，這裡只記「從第幾張開始看」
                    .data('galleryIndex', renderContext.galleryStartIndex[stock.id] ?? 0)
            );
        } else {
            $card.append('<div class="spec-thumb spec-thumb-empty"></div>');
        }

        const $body = $('<div class="spec-body"></div>');
        const name = specName(stock);

        // 不用 Bootstrap 的 .text-primary：它帶 !important，而頁面編輯器存的自訂 CSS
        // （_Layout 最後注入的 #frameCss）會重新定義 primary 色系，讓這裡跟著變色。
        // 顏色改由 Layout_2.css 的 .spec-name 指定。
        $body.append($('<button type="button" class="spec-name copy_btn">')
            .attr('data-copy', name)
            .text(name)
            .append('<span class="material-symbols-outlined copy-icon">content_copy</span>'));

        // 描述固定佔一格（即使空的），否則沒描述的規格會整列往左位移
        $body.append($('<div class="spec-desc"></div>').html(stock.specDescription || ''));
        $card.append($body);

        const $actions = buildActions(stock, vis, noStockManagement, state);
        if ($actions.children().length) $card.append($actions);

        return $card;
    }

    function requestedStockId() {
        const value = new URLSearchParams(window.location.search).get('psid');
        const stockId = normalizeNullableInt(value, 0);
        return stockId > 0 ? stockId : null;
    }

    function activateStockCard($container, $card, updateUrl) {
        if (!$card || !$card.length) return;

        $container.find('.spec-card')
            .removeClass('is-selected')
            .removeAttr('aria-current');
        $card.addClass('is-selected').attr('aria-current', 'true');

        if (updateUrl && window.productContentPage && typeof window.productContentPage.setVariantUrl === 'function') {
            window.productContentPage.setVariantUrl($card.attr('data-stock-id'));
        }
    }

    function applyRequestedStock($container) {
        const stockId = requestedStockId();
        if (!stockId) return;

        const $card = $container.find(`.spec-card[data-stock-id="${stockId}"]`).first();
        if (!$card.length) return;

        activateStockCard($container, $card, false);
        window.requestAnimationFrame(() => {
            $card.get(0)?.scrollIntoView({ block: 'center', behavior: 'auto' });
        });
    }

    function bindVariantSelection($container) {
        $container.on('click focusin', '.spec-card', function () {
            activateStockCard($container, $(this), true);
        });
    }

    function stepQuantity($btn, direction) {
        const $input = $btn.siblings('.spec-qty-input');
        const $card = $btn.closest('.spec-card');
        const stock = $card.data('stock');
        const step = readMinQty(stock);
        const current = normalizeNullableInt($input.val(), step);

        const quantity = clampQuantity(stock, current + direction * step, $card.data('noStockManagement') === true);
        $input.val(quantity);
        if (window.ProductAddOnPurchase) window.ProductAddOnPurchase.updateQuantity(quantity);
    }

    function bindQtyStepper($container) {
        $container.on('click', '.spec-qty-minus', function () {
            stepQuantity($(this), -1);
        });

        $container.on('click', '.spec-qty-plus', function () {
            stepQuantity($(this), 1);
        });

        $container.on('change', '.spec-qty-input', function () {
            const $input = $(this);
            const $card = $input.closest('.spec-card');

            const quantity = clampQuantity($card.data('stock'), $input.val(), $card.data('noStockManagement') === true);
            $input.val(quantity);
            if (window.ProductAddOnPurchase) window.ProductAddOnPurchase.updateQuantity(quantity);
        });
    }

    function openSpecGallery(startIndex) {
        const page = window.productContentPage;
        const viewer = page && page.mediaViewer;
        if (!viewer || typeof bootstrap === 'undefined') return;

        const items = renderContext.galleryItems;
        if (!items.length) return;

        const original = viewer.items;          // 先快照商品圖
        viewer.setItems(items);                  // 換成「全部規格」的共用清單
        viewer.showByIndex(startIndex);           // 從被點的規格那張開始（含 dots）

        const $modal = $('#ProDisplayModal');
        $modal.one('hidden.bs.modal', function () {
            viewer.setItems(original);           // 關閉後還原商品圖
        });

        bootstrap.Modal.getOrCreateInstance($modal.get(0)).show();
    }

    function bindSpecGallery($container) {
        $container.on('click', '.spec-thumb', function () {
            const index = normalizeNullableInt($(this).data('galleryIndex'), -1);
            if (index >= 0) openSpecGallery(index);
        });
    }

    function hasRealProductImage(result) {
        const imgs = result && Array.isArray(result.img_Medium) ? result.img_Medium : [];
        return imgs.some(img =>
            Array.isArray(img.link) && img.link[0] && img.link[0] !== '/images/noImg.jpg'
        );
    }

    function applyImageLayout(result) {
        if (hasRealProductImage(result))
            return;          // 有真圖 → 維持原樣
        $('#Product > .image').addClass('d-none');           // 沒圖 → 收掉 .image
        $('#Product > .content').removeClass('col-md-6 col-sm-12').addClass('col-12'); // .content 補滿
    }

    function buildSharedGallery(stocks) {
        const items = [];
        const startIndex = {};
        const seen = new Map();

        stocks.forEach(stock => {
            let first = -1;

            specImageItems(stock).forEach(item => {
                const key = normalizeNullableInt(item.id, -1);
                let index;

                if (key >= 0 && seen.has(key)) {
                    index = seen.get(key);          // 重複的圖沿用既有位置
                } else {
                    index = items.length;
                    items.push(item);
                    if (key >= 0) seen.set(key, index);
                }

                if (first < 0) first = index;
            });
            if (first >= 0) startIndex[stock.id] = first;
        });
        return { items: items, startIndex: startIndex };
    }

    function render($container, result, state) {
        const stocks = result && Array.isArray(result.stocks) ? result.stocks : [];
        const gallery = buildSharedGallery(stocks);

        renderContext = {
            vis: resolveVisibility(state, result),
            noStockManagement: !!(result && result.noStockManagement),
            state: state,
            galleryItems: gallery.items,
            galleryStartIndex: gallery.startIndex
        };

        $container.empty();
        stocks.forEach(stock => $container.append(
            buildRow(stock, renderContext.vis, renderContext.noStockManagement, renderContext.state)
        ));
    }

    function refreshRow($card) {
        const stock = $card.data('stock');
        if (!stock) return;

        $card.replaceWith(buildRow(stock, renderContext.vis, renderContext.noStockManagement, renderContext.state));
    }

    function init() {
        const pid = window.PageId;
        const $container = $('.spec-list');
        if (!pid || $container.length === 0 || typeof Product === 'undefined' || !Product.GetOne) return;

        const state = readState();
        bindQtyStepper($container);
        bindSpecGallery($container);
        bindAddToCart($container, state);
        bindCopyName($container);
        bindVariantSelection($container);

        Product.GetOne.ProdMainDisplay(pid)
            .done(result => {
                render($container, result, state);
                applyRequestedStock($container);
                applyImageLayout(result);
            })
            .fail(() => {
                console.error('Layout_2: 讀取規格資料失敗');
            });
    }

    $(init);

})(window.jQuery);
