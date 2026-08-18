(function (window, $) {
    'use strict';

    if (!$) {
        throw new Error('ProductContent requires jQuery.');
    }

    const I = (window.ProductContentInternals = window.ProductContentInternals || {});
    const DEFAULT_TEXTS = {
        marketPrice: '時價',
        prodEmpty: '缺貨',
        addCartNeedPrivacy: '若要進行商品選購，請先同意隱私權政策',
        addCartNeedSelection: '請確實選擇規格及購買數量',
        addCartSuccess: '商品已成功加入購物車',
        addCartError: '商品加入購物車發生錯誤',
        addCartWarningTitle: '請注意',
        commonErrorTitle: '錯誤',
        suggestedPrice: '建議售價',
        removeFavorite: '移除收藏',
        addFavorite: '加入收藏',
        removeFavoriteSuccess: '已將商品從收藏中移除',
        addFavoriteSuccess: '成功將商品加入收藏',
        bonusInsufficient: '紅利不足',
        bonusApplied: '含紅利折抵'
    };

    const DEFAULTS = {
        root: '#Product',
        pageRoot: document,
        productId: null,
        canShop: true,
        showRange: false,
        orderPrice: false,
        totalBonus: 0,
        orgName: typeof window.OrgName !== 'undefined' ? window.OrgName : '',
        texts: Object.assign({}, DEFAULT_TEXTS),
        i18n: null,
        selectors: {
            modal: '#ProDisplayModal',
            mainContent: 'Content#main',
            product: '#Product',
            imageRoot: '.image',
            content: '.content',
            productSwiper: '.ProductSwiper',
            productSwiperWrapper: '.ProductSwiper > .swiper-wrapper',
            previewSwiper: '.PreviewSwiper',
            previewSwiperWrapper: '.PreviewSwiper > .swiper-wrapper',
            title: '.pro_title',
            itemNo: '.pro_itemNo',
            introduce: '.introduce',
            specList: '#SpecCollapse > ul',
            detailedButton: '.btn_detailed',
            htmlPanel: '#ProductDescription > Content',
            tagList: '.pro_tag',
            techCertRoot: '.pro_tc',
            techCertList: '.pro_tc > ul',
            techCertContent: '.pro_tc_content > .techcert_list',
            filesTab: '#btn_tab > .files',
            fileDownload: '#FileDownload',
            fileList: '#FileDownload > .File_list',
            tabButtons: '#btn_tab>li>button',
            switchPage: '#SwitchPage',
            shareBlock: '.shareBlock',
            favoritesButton: '.btn_favorites'
        },
        templates: {
            imageSlide: '#TemplateImageSlide',
            videoSlide: '#TemplateVideoSlide',
            ytVideoSlide: '#TemplateYTVideoSlide',
            previewSlide: '#TemplatePreviewSlide',
            slide3d: '#Template3DSlide',
        },
        api: {
            clickLog: (pid) => Product.Log.Click(pid),
            getMainDisplay: (pid) => Product.GetOne.ProdMainDisplay(pid),
            addToCart: (payload) => Product.AddUp.Cart(payload),
            getCartDropOne: (id) => Product.GetOne.Cart(id),
            checkFavorite: (pid) => window.Coker?.Favorites?.Check ? Coker.Favorites.Check(pid) : null,
            addFavorite: (pid) => window.Coker?.Favorites?.Add ? Coker.Favorites.Add(pid) : null,
            deleteFavorite: (fid) => window.Coker?.Favorites?.Delete ? Coker.Favorites.Delete(fid) : null,
            switchPage: (args) => window.co?.Directory?.SwitchPage ? co.Directory.SwitchPage(args) : null
        },
        hooks: {
            afterLoad: null,
            afterRender: null,
            beforeAddToCart: null,
            afterAddToCart: null,
            onSelectionChanged: null
        }
    };

    let layoutFactory = null;

    function registerLayout(factory) {
        layoutFactory = typeof factory === 'function' ? factory : null;
    }

    function toInt(value, fallback = 0) {
        const num = parseInt(value, 10);
        return Number.isNaN(num) ? fallback : num;
    }

    function normalizeNullableInt(value, fallback = 0) {
        if (value === null || typeof value === 'undefined' || value === '') return fallback;
        return toInt(value, fallback);
    }

    function readMinQty(stock) {
        if (!stock) return 1;
        return Math.max(normalizeNullableInt(stock.min_Qty ?? stock.minQty, 1), 1);
    }

    function cloneTemplate(selector) {
        return $($(selector).html()).clone();
    }

    function formatNumber(value) {
        return normalizeNullableInt(value).toLocaleString('en-US');
    }

    function formatText(template, params) {
        if (!params) return template;
        return String(template).replace(/\{(\w+)\}/g, function (_, key) {
            return params[key] != null ? params[key] : '';
        });
    }

    function resolveText(options, key, fallback, params) {
        const defaultText =
            fallback ??
            options?.texts?.[key] ??
            DEFAULT_TEXTS[key] ??
            key;

        if (typeof options?.i18n === 'function') {
            const translated = options.i18n(key, defaultText, params);
            if (translated != null && translated !== '') {
                return formatText(translated, params);
            }
        }

        return formatText(defaultText, params);
    }

    function defaultI18n(key, fallback) {
        if (window.L && typeof window.L.get === 'function') {
            const value = window.L.get(key);
            return value || fallback;
        }

        if (window.local && typeof window.local === 'object') {
            const legacyMap = {
                marketPrice: 'MarketPrice',
                prodEmpty: 'ProdEmpty'
            };
            const legacyKey = legacyMap[key];
            if (legacyKey && window.local[legacyKey]) {
                return window.local[legacyKey];
            }
        }

        return fallback;
    }

    function formatPriceText(price, bonus, withDollar = true) {
        price = normalizeNullableInt(price);
        bonus = normalizeNullableInt(bonus);

        const money = withDollar ? `$${formatNumber(price)}` : formatNumber(price);

        if (bonus > 0) {
            if (price === 0) return `${local.Bonus}:${formatNumber(bonus)}`;
            return `${money} + ${local.Bonus}:${formatNumber(bonus)}`;
        }

        return money;
    }

    function analyzeSpecStructure(stocks) {
        const safeStocks = Array.isArray(stocks) ? stocks : [];

        const s1Ids = [...new Set(
            safeStocks
                .map(x => normalizeNullableInt(x.fK_S1id ?? x.s1id))
                .filter(x => x > 0)
        )];

        const s2Ids = [...new Set(
            safeStocks
                .map(x => normalizeNullableInt(x.fK_S2id ?? x.s2id))
                .filter(x => x > 0)
        )];

        return {
            hasS1: s1Ids.length > 0,
            hasS2: s2Ids.length > 0,
            s1Count: s1Ids.length,
            s2Count: s2Ids.length,
            mode:
                s1Ids.length === 0 && s2Ids.length === 0
                    ? 'none'
                    : s1Ids.length > 0 && s2Ids.length === 0
                        ? 'single'
                        : 'double'
        };
    }

    function buildPriceSummary(stocks, options) {
        const safeStocks = Array.isArray(stocks) ? stocks : [];
        const hasTimePrice = safeStocks.some(x => !!x.timePrice);
        const priceCandidates = safeStocks
            .filter(x => !x.timePrice)
            .flatMap(x => (x.prices || []).map(p => ({
                total: normalizeNullableInt(p.price) + normalizeNullableInt(p.bonus),
                price: normalizeNullableInt(p.price),
                bonus: normalizeNullableInt(p.bonus)
            })));

        if (hasTimePrice && priceCandidates.length === 0) {
            return local.MarketPrice;
        }

        if (priceCandidates.length === 0) {
            return '';
        }

        const totals = priceCandidates.map(x => x.total);
        const min = Math.min(...totals);
        const max = Math.max(...totals);
        const target = options.orderPrice ? min : max;

        if (hasTimePrice && priceCandidates.length > 0) {
            return options.showRange
                ? `$${formatNumber(min)} ~ ${local.MarketPrice}`
                : `$${formatNumber(target)}`;
        }

        if (options.showRange) {
            if (min === max) {
                const single = priceCandidates.find(x => x.total === min);
                return formatPriceText(single.price, single.bonus);
            }
            return `$${formatNumber(min)} ~ $${formatNumber(max)}`;
        }

        const selected = priceCandidates.find(x => x.total === target);
        return formatPriceText(selected.price, selected.bonus);
    }

    function buildPriceViewModel(priceItem, stock, controller, product) {
        const currentPrice = normalizeNullableInt(priceItem.price);
        const currentBonus = normalizeNullableInt(priceItem.bonus);
        const originalPrice = normalizeNullableInt(priceItem.oriPrice);
        const suggestPrice = normalizeNullableInt(stock.suggestPrice);
        const isTimePrice = !!stock.timePrice;
        const disabled = !!priceItem.disabled;

        const itemRoleName = priceItem.roleName || '';
        const baseRoleName = product.baseRoleName || priceItem.baseRoleName || local.NonMember;

        const saleText = isTimePrice
            ? local.MarketPrice
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
                ? `${local.SuggestedPrice}$${formatNumber(suggestPrice)}`
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
            local.NonMember;

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
            suggestPriceLabel: local.SuggestedPrice,
            suggestPriceValue: showSuggestPrice
                ? `$${formatNumber(suggestPrice)}`
                : '',

            showOriginalPrice,
            originalPriceLabel: local.RolePriceLabel.format(baseRoleName),
            originalPriceValue: showOriginalPrice
                ? `$${formatNumber(originalPrice)}`
                : ''
        };
    }

    function isStockAvailable(stock, noStockManagement) {
        return !!stock && stock.canPurchase === true;
    }

    // 將數量夾到 [最小購買量, 庫存] 區間，並對齊最小購買量的倍數。
    // noStockManagement 為真時不受庫存上限限制。
    function clampQuantity(stock, quantity, noStockManagement) {
        const step = readMinQty(stock);
        const min = step;
        let value = normalizeNullableInt(quantity, min);

        value -= value % step;
        if (value < min) value = min;

        if (!noStockManagement) {
            const max = normalizeNullableInt(stock && stock.maxPurchaseQuantity);
            // max === 0 代表目前沒有任何可銷售數量，不可把它當成「無上限」。
            if (max <= 0) return 0;
            if (value > max) value = max;
        }

        if (value === 0) value = min;

        return value;
    }

    function isLoggedIn(fallback) {
        if (window.co && co.auth && typeof co.auth.isLoggedIn === 'function') {
            return co.auth.isLoggedIn();
        }

        return !!fallback;
    }

    function createCartPayload(productId, selection) {
        const safe = selection || {};

        return {
            FK_Pid: normalizeNullableInt(productId),
            FK_PriceId: normalizeNullableInt(safe.priceId),
            FK_S1id: normalizeNullableInt(safe.s1id),
            FK_S2id: normalizeNullableInt(safe.s2id),
            Quantity: normalizeNullableInt(safe.quantity)
        };
    }

    // 加入購物車前的共用檢查：隱私權 → 未登入紅利價 → 紅利不足
    // 回傳 false 代表已跳出提示、呼叫端應中止。
    function runBuyGuard(options) {
        const t = options.t;
        const bonus = normalizeNullableInt(options.bonus);
        const totalBonus = normalizeNullableInt(options.totalBonus);
        const loggedIn = isLoggedIn(options.isLoginFallback);

        if (localStorage.getItem('AgreePrivacy') == null) {
            Coker.sweet.warning(
                local.AlertTitle,
                local.AddCartNeedPrivacy
            );
            return false;
        }

        if (!loggedIn && bonus > 0) {
            Coker.sweet.warning(
                local.AlertTitle,
                local.PleaseSignIn,
                function () {
                    if (typeof loginModal !== 'undefined' && loginModal && typeof loginModal.show === 'function') {
                        loginModal.show();
                    }
                }
            );
            return false;
        }

        // 已登入但紅利不足（前端先做 UX 提示；後端仍需再驗證）
        if (loggedIn && bonus > 0 && totalBonus < bonus) {
            Coker.sweet.warning(
                local.AlertTitle,
                local.BonusInsufficient,
            );
            return false;
        }

        return true;
    }

    // 送出加入購物車並處理結果；成功後的狀態更新由 onSuccess 交給呼叫端。
    function submitCart(options) {
        const t = options.t;
        const api = options.api || {};
        const addToCart = typeof api.addToCart === 'function'
            ? api.addToCart
            : (payload) => Product.AddUp.Cart(payload);
        const getCartDropOne = typeof api.getCartDropOne === 'function'
            ? api.getCartDropOne
            : (id) => Product.GetOne.Cart(id);

        const request = addToCart(options.payload)
            .done(function (result) {
                if (!result || !result.success) {
                    const error = result && result.error;

                    if (error === '商品庫存不足') {
                        Coker.sweet.warning(local.StockNotEnough, result.message, function () {
                            location.reload(true);
                        });
                    } else {
                        Coker.sweet.error(
                            local.Error,
                            (result && result.message) || local.AddCartError,
                            null
                        );
                    }
                    return;
                }

                Coker.sweet.success(local.AddCartSuccess, null, true);

                const type = (result.message || '').substr(0, 1);
                const id = (result.message || '').substr(1);

                // 迷你購物車以 append 更新項目，因此必須先完成主商品，再依序處理
                // 加價購商品；若同時送出請求，回應較快的加價購會被排到主商品前面。
                let cartDropRequest = getCartDropOne(id).done(function (drop) {
                    if (type === 'N') {
                        if (typeof window.CartDropAdd === 'function') window.CartDropAdd(drop);
                    } else {
                        if (typeof window.CartDropUpdate === 'function') window.CartDropUpdate(drop);
                    }
                });

                const rewardCartIds = result && (result.object || result.Object);
                if (Array.isArray(rewardCartIds)) {
                    rewardCartIds.forEach(function (rewardCartId) {
                        cartDropRequest = cartDropRequest.then(function () {
                            return getCartDropOne(rewardCartId).done(function (drop) {
                                const exists = $('#Car_Dropdown > ul > li').filter(function () {
                                    return normalizeNullableInt($(this).data('scid')) === normalizeNullableInt(rewardCartId);
                                }).length > 0;
                                if (exists) {
                                    if (typeof window.CartDropUpdate === 'function') window.CartDropUpdate(drop);
                                } else if (typeof window.CartDropAdd === 'function') {
                                    window.CartDropAdd(drop);
                                }
                            });
                        });
                    });
                }

                if (typeof options.onSuccess === 'function') {
                    options.onSuccess(result);
                }
            })
            .fail(function () {
                Coker.sweet.error(
                    local.Error,
                    local.AddCartError,
                    null,
                    true
                );
            });

        if (typeof options.onAlways === 'function') {
            request.always(options.onAlways);
        }

        return request;
    }

    Object.assign(I, {
        DEFAULT_TEXTS, DEFAULTS, registerLayout,
        getLayoutFactory: () => layoutFactory,
        toInt, normalizeNullableInt, readMinQty, cloneTemplate, formatNumber, formatText,
        resolveText, defaultI18n, formatPriceText, analyzeSpecStructure, buildPriceSummary,
        buildPriceViewModel, buildPriceBaseViewModel, isStockAvailable, clampQuantity,
        isLoggedIn, createCartPayload, runBuyGuard, submitCart
    });
})(window, window.jQuery);
