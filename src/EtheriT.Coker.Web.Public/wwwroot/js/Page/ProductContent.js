(function (window, $) {
    'use strict';

    if (!$) {
        throw new Error('ProductContent requires jQuery.');
    }

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
            priceFrame: '.priceframe',
            options: '.options',
            quantityInput: '.input_pro_quantity',
            quantityWrap: '.counter_input',
            addToCartButton: '.btn_addToCar',
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
            specRadio: '#Template_Spec_Radio',
            priceItem: '#PriceListTemplate'
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

    function toInt(value, fallback = 0) {
        const num = parseInt(value, 10);
        return Number.isNaN(num) ? fallback : num;
    }

    function normalizeNullableInt(value, fallback = 0) {
        if (value === null || typeof value === 'undefined' || value === '') return fallback;
        return toInt(value, fallback);
    }

    function cloneTemplate(selector) {
        return $($(selector).html()).clone();
    }

    function htmlDecode(value) {
        if (!value) return '';
        if ($.htmlDecode) return $.htmlDecode(value);
        const textarea = document.createElement('textarea');
        textarea.innerHTML = value;
        return textarea.value;
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

    function formatPriceText(price, bonus, withDollar = true) {
        price = normalizeNullableInt(price);
        bonus = normalizeNullableInt(bonus);

        const money = withDollar ? `$${formatNumber(price)}` : formatNumber(price);

        if (bonus > 0) {
            if (price === 0) return `紅利:${formatNumber(bonus)}`;
            return `${money} + 紅利:${formatNumber(bonus)}`;
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
            return resolveText(options, 'marketPrice');
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
                ? `$${formatNumber(min)} ~ ${resolveText(options, 'marketPrice')}`
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

    class ProductSelectionEngine {
        constructor(product, options) {
            this.product = product || { stocks: [] };
            this.options = options;
            this.canShop = !!options.canShop;
            this.stocks = Array.isArray(this.product.stocks) ? this.product.stocks.map(this.normalizeStock.bind(this)) : [];
            this.specMap = this.buildSpecMap();
            this.specMode = analyzeSpecStructure(this.stocks).mode;
            this.current = {
                s1: null,
                s2: null,
                priceId: null,
                quantity: 1
            };
            this.bootstrap();
        }

        normalizeStock(stock) {
            const prices = Array.isArray(stock.prices) ? stock.prices : [];
            const normalized = {
                id: normalizeNullableInt(stock.id),
                s1id: normalizeNullableInt(stock.fK_S1id ?? stock.s1id),
                s2id: normalizeNullableInt(stock.fK_S2id ?? stock.s2id),
                s1Title: stock.s1_Title || stock.s1Title || '',
                s2Title: stock.s2_Title || stock.s2Title || '',
                stock: normalizeNullableInt(stock.stock),
                minQty: Math.max(normalizeNullableInt(stock.min_Qty ?? stock.minQty, 1), 1),
                timePrice: !!stock.timePrice,
                suggestPrice: normalizeNullableInt(stock.suggestPrice ?? stock.price),
                prices: prices.map(p => ({
                    id: normalizeNullableInt(p.id),
                    roleId: normalizeNullableInt(p.fK_RId ?? p.roleId),
                    roleName: p.roleName || p.baseRoleName || '',
                    price: normalizeNullableInt(p.price),
                    bonus: normalizeNullableInt(p.bonus),
                    oriPrice: normalizeNullableInt(p.oriPrice)
                }))
            };

            if (normalized.timePrice && normalized.prices.length === 0) {
                normalized.prices = [{ id: 0, roleId: 0, roleName: '', price: 0, bonus: 0, oriPrice: 0 }];
            }

            return normalized;
        }

        buildSpecMap() {
            const spec1 = new Map();
            const spec2 = new Map();

            this.stocks.forEach(stock => {
                if (stock.s1id > 0 && !spec1.has(stock.s1id)) {
                    spec1.set(stock.s1id, stock.s1Title);
                }
                if (stock.s2id > 0 && !spec2.has(stock.s2id)) {
                    spec2.set(stock.s2id, stock.s2Title);
                }
            });

            return { spec1, spec2 };
        }

        bootstrap() {
            if (this.stocks.length === 0) return;

            const firstAvailable = this.stocks.find(x => !this.canShop || x.stock >= x.minQty) || this.stocks[0];

            if (this.specMode === 'none') {
                this.current.s1 = 0;
                this.current.s2 = 0;
            } else if (this.specMode === 'single') {
                this.current.s1 = firstAvailable.s1id;
                this.current.s2 = 0;
            } else {
                this.current.s1 = firstAvailable.s1id;
                this.current.s2 = firstAvailable.s2id;
            }

            const activeStock = this.getActiveStock();
            if (activeStock && activeStock.prices.length > 0) {
                const firstEnabledPrice = activeStock.prices.find(p => !this.isBonusLack(p)) || activeStock.prices[0];
                this.current.priceId = firstEnabledPrice.id;
                this.current.quantity = activeStock.minQty;
            }
        }

        getSpec1Options() {
            return Array.from(this.specMap.spec1.entries()).map(([id, title]) => ({
                id,
                title,
                enabled: this.stocks.some(x => x.s1id === id && (!this.canShop || x.stock >= x.minQty))
            }));
        }

        getSpec2Options(spec1Id) {
            return this.stocks
                .filter(x => x.s1id === normalizeNullableInt(spec1Id))
                .map(x => ({
                    id: x.s2id,
                    title: x.s2Title,
                    enabled: !this.canShop || x.stock >= x.minQty
                }))
                .filter(x => x.id > 0)
                .filter((item, index, array) => array.findIndex(x => x.id === item.id) === index);
        }

        setSpec(type, id) {
            const value = normalizeNullableInt(id);

            if (type === 1) {
                this.current.s1 = value;

                if (this.specMode === 'double') {
                    const validS2 = this.getSpec2Options(value).filter(x => x.enabled);
                    if (validS2.length === 0) {
                        this.current.s2 = null;
                    } else if (!validS2.some(x => x.id === this.current.s2)) {
                        this.current.s2 = validS2[0].id;
                    }
                }
            }

            if (type === 2) {
                this.current.s2 = value;
            }

            const activeStock = this.getActiveStock();
            if (activeStock) {
                const enabledPrice = activeStock.prices.find(p => !this.isBonusLack(p)) || activeStock.prices[0] || null;
                this.current.priceId = enabledPrice ? enabledPrice.id : null;
                this.current.quantity = activeStock.minQty;
            }
        }

        getActiveStock() {
            if (this.stocks.length === 0) return null;
            if (this.stocks.length === 1) return this.stocks[0];

            if (this.specMode === 'none') {
                return this.stocks[0] || null;
            }

            if (this.specMode === 'single') {
                return this.stocks.find(stock =>
                    normalizeNullableInt(stock.s1id) === normalizeNullableInt(this.current.s1)
                ) || null;
            }

            return this.stocks.find(stock => {
                const s1Matched = normalizeNullableInt(stock.s1id) === normalizeNullableInt(this.current.s1);
                const s2Matched = normalizeNullableInt(stock.s2id) === normalizeNullableInt(this.current.s2 || 0);
                return s1Matched && s2Matched;
            }) || null;
        }

        getPriceOptions() {
            const stock = this.getActiveStock();
            if (!stock) return [];

            return stock.prices.map(price => {
                const disabled = this.isBonusLack(price);

                return {
                    ...price,
                    disabled,
                    checked: normalizeNullableInt(price.id) === normalizeNullableInt(this.current.priceId),
                    stock
                };
            });
        }

        isBonusLack(price) {
            if (!this.canShop) return false;

            const bonus = normalizeNullableInt(price.bonus);
            if (bonus <= 0) return false;

            const isLoggedIn = co.auth.isLoggedIn();
            if (!isLoggedIn) return false;

            return normalizeNullableInt(this.options.totalBonus) < bonus;
        }

        setPrice(priceId) {
            this.current.priceId = normalizeNullableInt(priceId, null);
        }

        setQuantity(quantity) {
            const stock = this.getActiveStock();
            if (!stock) return;

            const step = stock.minQty;
            const min = step;
            const max = stock.stock - (stock.stock % step);
            let value = normalizeNullableInt(quantity, min);

            value -= value % step;
            if (value < min) value = min;
            if (max > 0 && value > max) value = max;
            if (value === 0) value = min;

            this.current.quantity = value;
        }

        decreaseStockAfterAdd() {
            const stock = this.getActiveStock();
            if (!stock) return;
            stock.stock = Math.max(stock.stock - normalizeNullableInt(this.current.quantity), 0);
        }

        canAddToCart() {
            const stock = this.getActiveStock();
            if (!this.canShop) return false;
            if (!stock) return false;
            if (stock.timePrice) return false;
            if (stock.stock < stock.minQty) return false;
            if (!this.current.priceId) return false;
            return true;
        }

        buildCartPayload(productId) {
            return {
                FK_Pid: normalizeNullableInt(productId),
                FK_PriceId: normalizeNullableInt(this.current.priceId),
                FK_S1id: normalizeNullableInt(this.current.s1),
                FK_S2id: normalizeNullableInt(this.current.s2),
                Quantity: normalizeNullableInt(this.current.quantity)
            };
        }
    }

    class ProductMediaViewer {
        constructor(options) {
            this.options = options;
            this.$modal = $(options.selectors.modal);
            this.$image = $('#Pro_Image');
            this.$video = $('#Pro_Video');
            this.$youtube = $('#Pro_Youtube');
            this.$view360 = $('#Pro_360View');
            this.items = [];
            this.bindModalEvents();
        }

        setItems(items) {
            this.items = Array.isArray(items) ? items : [];
        }

        bindModalEvents() {
            const modalElement = this.$modal.get(0);
            if (!modalElement) return;

            modalElement.addEventListener('hidden.bs.modal', () => {
                if (window.CI360) window.CI360.destroy();
                this.$video.attr('src', '').addClass('d-none');
                this.$youtube.attr('src', '').addClass('d-none');
                this.$image.addClass('d-none').empty();
                this.$view360.addClass('d-none').empty();
            });

            this.$modal.find('.btn-tool.prev-btn').off('click').on('click', (e) => {
                e.preventDefault();
                this.move(-1);
            });

            this.$modal.find('.btn-tool.next-btn').off('click').on('click', (e) => {
                e.preventDefault();
                this.move(1);
            });
        }

        move(step) {
            if (!this.items.length) return;
            const currentId = normalizeNullableInt(this.$modal.data('id'));
            let index = this.items.findIndex(x => normalizeNullableInt(x.id) === currentId);
            if (index < 0) index = 0;
            index = (index + step + this.items.length) % this.items.length;
            this.showById(this.items[index].id);
        }

        showById(id) {
            const item = this.items.find(x => normalizeNullableInt(x.id) === normalizeNullableInt(id));
            if (!item) return;

            this.$modal.data('id', item.id);

            const type = item.fileType === 3
                ? 'video'
                : item.fileType === 4
                    ? 'youtube'
                    : item.fileType === 5
                        ? '360view'
                        : 'image';

            this.render(item, type);
        }

        render(item, type) {
            if (window.CI360) window.CI360.destroy();
            this.$image.addClass('d-none').empty();
            this.$video.addClass('d-none').attr('src', '');
            this.$youtube.addClass('d-none').attr('src', '');
            this.$view360.addClass('d-none').empty();

            if (type === 'video') {
                this.resetModalSize();

                this.$modal.find('.modal-dialog').css({
                    width: 'min(90vw, 960px)',
                    maxWidth: '100%'
                });

                this.$modal.find('.modal-body').css({
                    height: 'min(70vh, 540px)'
                });

                this.$video
                    .removeClass('d-none')
                    .attr('src', item.link[0])
                    .css({
                        width: '100%',
                        height: '100%',
                        objectFit: 'contain'
                    });

                return;
            }

            if (type === 'youtube') {
                this.resetModalSize();

                this.$modal.find('.modal-dialog').addClass('ytshow').css({
                    width: 'min(90vw, 960px)',
                    maxWidth: '100%'
                });

                this.$modal.find('.modal-body').css({
                    height: 'min(70vh, 540px)'
                });

                const youtubeParts = (item.name || '').split('&t=');
                let url = `https://www.youtube-nocookie.com/embed/${youtubeParts[0]}`;
                if (youtubeParts[1]) url += `?start=${youtubeParts[1]}`;

                this.$youtube
                    .removeClass('d-none')
                    .attr('src', url)
                    .css({
                        width: '100%',
                        height: '100%'
                    });

                return;
            }

            if (type === '360view') {

                // 重新抓 DOM（避免 CI360.destroy() 造成舊 reference 失效）
                this.$view360 = $('#Pro_360View');

                this.$view360
                    .removeClass('d-none cloudimage-360 initialized')
                    .empty()
                    .css({
                        position: 'relative',
                        width: '100%',
                        height: '100%'
                    });

                const src = item.link?.[0] || '';

                if (!src) return;

                const folder = src.substring(0, src.lastIndexOf('/') + 1);
                const filename = src.substring(src.lastIndexOf('/') + 1);

                const amountX = item.amountX || this.$view360.data('amount-x') || 15;

                // ⚠️ 只補必要欄位，不覆蓋原本 HTML 設定
                this.$view360.attr({
                    'data-folder': folder,
                    'data-filename-x': filename,
                    'data-amount-x': amountX
                });

                // ⚠️ 先算 modal 尺寸（關鍵，不然 canvas = 0）
                this.fitModalByImage(src, () => {

                    // ⚠️ 再初始化 360
                    this.$view360.addClass('cloudimage-360');

                    setTimeout(() => {
                        if (!window.CI360) return;

                        window.CI360._viewers = window.CI360._viewers || [];
                        window.CI360.add('Pro_360View');

                        setTimeout(() => {
                            const canvas = this.$view360.find('canvas');
                            const h = canvas.outerHeight();

                            if (h > 0) {
                                this.$modal.find('.modal-body').css({
                                    height: h + 'px'
                                });
                            }
                        }, 120);

                    }, 120);

                });

                return;
            }

            const src = item.link[0];
            const folder = src.substring(0, src.lastIndexOf('/') + 1);
            const filename = src.substring(src.lastIndexOf('/') + 1);

            this.$image
                .removeClass('d-none')
                .empty()
                .removeClass('cloudimage-360')
                .attr({
                    'data-folder': folder,
                    'data-filename-x': filename,
                    'data-amount-x': 1
                })
                .css('position', 'relative');

            this.fitModalByImage(src, () => {
                this.$image.addClass('cloudimage-360');

                setTimeout(() => {
                    window.CI360._viewers = window.CI360._viewers || [];
                    window.CI360.add('Pro_Image');

                    setTimeout(() => {
                        const canvasHeight = this.$image.find('canvas').outerHeight();
                        if (canvasHeight > 0) {
                            this.$modal.find('.modal-body').css({
                                height: canvasHeight + 'px'
                            });
                        }
                    }, 80);
                }, 300);
            });
        }

        resetModalSize() {
            this.$modal.find('.modal-dialog').removeAttr('style').removeClass('ytshow');
            this.$modal.find('.modal-body').removeAttr('style').css('height', 'auto');
        }

        fitModalByImage(src, callback) {
            const dialog = this.$modal.find('.modal-dialog');
            const body = this.$modal.find('.modal-body');

            if (!src || dialog.length === 0) {
                if (typeof callback === 'function') callback();
                return;
            }

            const preloadImg = new Image();
            preloadImg.src = src;

            preloadImg.onload = () => {
                const imgWidth = preloadImg.naturalWidth || 1;
                const imgHeight = preloadImg.naturalHeight || 1;
                const imgRatio = imgWidth / imgHeight;

                const rectBefore = dialog[0].getBoundingClientRect();
                const beforeW = Math.round(rectBefore.width);
                const beforeH = Math.round(rectBefore.height);

                const currentWidth = dialog.outerWidth();
                const currentHeight = dialog.outerHeight();

                dialog.css({
                    width: currentWidth,
                    height: currentHeight
                });

                void dialog[0].offsetWidth;

                const winWidth = window.innerWidth;
                const winHeight = window.innerHeight;

                let maxWidthRatio = 0.8;
                if (imgRatio > 1.2) maxWidthRatio = 0.9;

                const maxWidth = winWidth * maxWidthRatio;
                const maxHeight = winHeight * 0.9;

                const heightByMaxWidth = maxWidth / imgRatio;
                let targetWidth;
                let targetHeight;

                if (heightByMaxWidth <= maxHeight) {
                    targetWidth = maxWidth;
                    targetHeight = heightByMaxWidth;
                } else {
                    targetHeight = maxHeight;
                    targetWidth = maxHeight * imgRatio;
                }

                dialog.css({
                    minWidth: '',
                    minHeight: '',
                    width: targetWidth + 'px',
                    height: targetHeight + 'px',
                    maxWidth: '100%',
                    maxHeight: ''
                });

                body.css({
                    height: targetHeight + 'px'
                });

                const sizeChanged =
                    beforeW !== Math.round(targetWidth) ||
                    beforeH !== Math.round(targetHeight);

                const proceed = () => {
                    if (typeof callback === 'function') callback(targetWidth, targetHeight);
                };

                if (sizeChanged) {
                    let finished = false;

                    const finish = () => {
                        if (finished) return;
                        finished = true;
                        dialog.off('transitionend.productMediaResize');
                        proceed();
                    };

                    dialog
                        .off('transitionend.productMediaResize')
                        .on('transitionend.productMediaResize', function (e) {
                            if (e.target === dialog[0]) finish();
                        });

                    setTimeout(finish, 350);
                } else {
                    requestAnimationFrame(proceed);
                };
            };

            preloadImg.onerror = () => {
                this.resetModalSize();
                if (typeof callback === 'function') callback();
            };
        }
    }

    class ProductContentController {
        constructor(options) {
            this.options = $.extend(true, {}, DEFAULTS, options || {});
            this.$pageRoot = $(this.options.pageRoot);
            this.$root = this.$pageRoot.find(this.options.selectors.product);
            this.$contentRoot = this.$root.find(this.options.selectors.content);
            this.$quantityInput = this.$root.find(this.options.selectors.quantityInput);
            this.$quantityWrap = this.$root.find(this.options.selectors.quantityWrap);
            this.$addToCartButton = this.$pageRoot.find(this.options.selectors.addToCartButton);
            this.options.totalBonus = normalizeNullableInt(this.options.totalBonus, 0);
            this.options.orderPrice = !!this.options.orderPrice;
            this.state = {
                productId: this.resolveProductId(),
                product: null,
                selection: null,
                previewSwiper: null,
                productSwiper: null
            };
            this.mediaViewer = new ProductMediaViewer(this.options);
        }

        t(key, fallback, params) {
            return resolveText(this.options, key, fallback, params);
        }

        resolveProductId() {
            if (this.options.productId != null && this.options.productId !== '') {
                return this.options.productId;
            }

            console.warn('ProductContent: productId is missing.');
            return null;
        }

        init() {
            this.bindStaticEvents();
            this.logClick();
            return this.load();
        }

        bindStaticEvents() {
            const selectors = this.options.selectors;

            this.$pageRoot.off('click.productContent', '.btn_count_plus').on('click.productContent', '.btn_count_plus', () => {
                const current = normalizeNullableInt(this.$quantityInput.val(), 1);
                const step = normalizeNullableInt(this.$quantityInput.attr('step'), 1);
                this.state.selection.setQuantity(current + step);
                this.renderQuantity();
            });

            this.$pageRoot.off('click.productContent', '.btn_count_minus').on('click.productContent', '.btn_count_minus', () => {
                const current = normalizeNullableInt(this.$quantityInput.val(), 1);
                const step = normalizeNullableInt(this.$quantityInput.attr('step'), 1);
                this.state.selection.setQuantity(current - step);
                this.renderQuantity();
            });

            this.$pageRoot.off('change.productContent', selectors.quantityInput).on('change.productContent', selectors.quantityInput, (e) => {
                this.state.selection.setQuantity($(e.currentTarget).val());
                this.renderQuantity();
            });

            this.$pageRoot.off('click.productContent', selectors.addToCartButton).on('click.productContent', selectors.addToCartButton, () => {
                this.addToCart();
            });

            this.$pageRoot.off('change.productContent', 'input[name="S1_Radio"]').on('change.productContent', 'input[name="S1_Radio"]', (e) => {
                this.state.selection.setSpec(1, $(e.currentTarget).val());
                this.renderSelectionArea();
            });

            this.$pageRoot.off('change.productContent', 'input[name="S2_Radio"]').on('change.productContent', 'input[name="S2_Radio"]', (e) => {
                this.state.selection.setSpec(2, $(e.currentTarget).val());
                this.renderSelectionArea();
            });

            this.$pageRoot.off('change.productContent', 'input[name="priceRadio"]').on('change.productContent', 'input[name="priceRadio"]', (e) => {
                this.state.selection.setPrice($(e.currentTarget).data('priceid'));
                this.syncButtonState();
            });

            this.$pageRoot.off('click.productContent', '.pro_display').on('click.productContent', '.pro_display', (e) => {
                const id = $(e.currentTarget).data('id');
                this.mediaViewer.showById(id);
            });

            this.$pageRoot.off('click.productContent', '.btn_tc').on('click.productContent', '.btn_tc', (e) => {
                $('#ProductDescription').removeClass('active show');
                $('#TechnicalDocuments').addClass('active show');
                $('#btn_tab .nav-link').removeClass('active');
                $('#pills-documents-tab').addClass('active');

                const tcid = $(e.currentTarget).data('tcid');
                const $target = $(`.badge_${tcid}`);
                if ($target.length > 0) {
                    $('html, body').animate({ scrollTop: $target.offset().top - ($('header > nav').height() || $('header').height() || 0) * 2 }, 0);
                }
            });
        }

        logClick() {
            try {
                this.options.api.clickLog(this.state.productId);
            } catch (error) {
                console.warn('Product click log failed.', error);
            }
        }

        load() {
            return this.options.api.getMainDisplay(this.state.productId).done((result) => {
                if (!result) {
                    window.location.href = window.location.pathname.substring(0, window.location.pathname.lastIndexOf('/'));
                    return;
                }

                this.state.product = result;
                this.state.selection = new ProductSelectionEngine(result, this.options);

                if (typeof this.options.hooks.afterLoad === 'function') {
                    this.options.hooks.afterLoad(result, this);
                }

                this.render();
            });
        }

        render() {
            const result = this.state.product;
            const selectors = this.options.selectors;
            const $root = this.$root;
            const $content = this.$contentRoot;

            if (result.status == 2) {
                this.options.canShop = false;
                this.state.selection.canShop = false;
                result.stocks?.forEach(stock => { stock.stock = 0; });
            }

            $content.find(selectors.title).text(result.title || '');
            $content.find(selectors.itemNo).text(result.itemNo || '');

            const introHtml = (result.introduction || '')
                .split('\n')
                .filter(Boolean)
                .map(x => `<li>${x}</li>`)
                .join('');
            $content.find(selectors.introduce).html(introHtml);

            const descHtml = (result.description || '')
                .split('\n')
                .filter(Boolean)
                .map(x => `<li>${x}</li>`)
                .join('');
            $root.find(selectors.specList).html(descHtml);

            this.toggleSpecDetailButton();
            this.renderHtmlContent();
            this.renderTechCerts();
            this.renderFiles();
            this.renderTags();
            this.renderStatus();
            this.renderMedia();
            this.renderSelectionArea();
            this.initShare();
            this.initFavorite();
            this.initSwitchPage();

            const $main = this.$pageRoot.find(selectors.mainContent);
            $main.removeClass('d-none');

            const $firstTab = this.$pageRoot.find(selectors.tabButtons).first();
            if ($firstTab.length > 0) {
                $firstTab.trigger('click');
            }

            if (typeof this.options.hooks.afterRender === 'function') {
                this.options.hooks.afterRender(result, this);
            }
        }

        toggleSpecDetailButton() {
            const $btn = this.$root.find(this.options.selectors.detailedButton);
            const $list = this.$root.find(this.options.selectors.specList);
            let specHeight = 0;

            $list.children('li').each(function () {
                specHeight += $(this).outerHeight(true) || 0;
            });

            if (specHeight > 96) {
                $btn.removeClass('d-none');
            } else {
                $btn.addClass('d-none');
            }
        }

        renderHtmlContent() {
            const html = this.state.product.html;
            const selectors = this.options.selectors;
            if (html && html.trim() !== '') {
                $(selectors.htmlPanel).removeClass('d-none').html(htmlDecode(html));
            } else {
                $('#ProductDescription,#btn_tab .description').remove();
            }
        }

        renderTechCerts() {
            const result = this.state.product;
            const selectors = this.options.selectors;
            const $root = this.$pageRoot;
            const $list = $root.find(selectors.techCertList).empty();
            const $content = $root.find(selectors.techCertContent).empty();

            if (!Array.isArray(result.techCertDatas) || result.techCertDatas.length === 0) {
                $('#btn_tab > .technical,.pro_tc').remove();
                return;
            }

            let hasAnyImage = false;

            result.techCertDatas.forEach(item => {
                (item.img_small || []).forEach(img => {
                    hasAnyImage = true;
                    $list.append(`<li class="me-1"><button class="btn_tc bg-transparent border-0" data-tcid="${item.id}"><img src="${img.link}" alt="${img.name}" /></button></li>`);
                });

                (item.img_orig || []).forEach(img => {
                    $content.append(`
                        <div class="badge_${item.id} row pb-3">
                            <div class="col-12 col-lg-2 col-md-5 text-center verticalAlign">
                                <img src="${img.link}" alt="${img.name}" />
                            </div>
                            <div class="description align-self-center col">${item.description || ''}</div>
                        </div>
                        <hr class="m-1" />
                    `);
                });
            });

            if (!hasAnyImage) {
                $('.pro_tc').addClass('d-none');
            }

            $root.find('.pro_tc img, .pro_tc_content img').imgCheck?.();
        }

        renderFiles() {
            const result = this.state.product;
            const selectors = this.options.selectors;
            const $list = $(selectors.fileList).empty();

            if (!Array.isArray(result.files) || result.files.length === 0) {
                $('#btn_tab > .files,#FileDownload').remove();
                return;
            }

            result.files.forEach(file => {
                const link = window.IsFaPage === true ? file.link : file.link.replace('upload', `upload/${this.options.orgName}`);
                $list.append(`
                    <div class="file border-bottom">
                        <a href="${link}" download="${file.name}" title="${file.name}" class="link_with_icon d-flex text-decoration-none edit_lock">
                            <div draggable="true" class="icon pe-2"></div>
                            <div draggable="true" class="name">${file.name}</div>
                        </a>
                    </div>
                `);
            });

            if (window.LinkWithIconInit) {
                window.LinkWithIconInit();
            }
        }

        renderTags() {
            const result = this.state.product;
            const $tagList = $(this.options.selectors.tagList).empty();

            if (!Array.isArray(result.tagDatas) || result.tagDatas.length === 0) {
                $tagList.addClass('d-none');
                return;
            }

            result.tagDatas.forEach(item => {
                $tagList.prepend(`<li><a class="round_tag rounded-pill me-1 px-3 py-1" href="/${this.options.orgName}/Search/Get/3/${item.tag_Name}">${item.tag_Name}</a></li>`);
            });
        }

        renderStatus() {
            const result = this.state.product;

            const $status = this.$root
                .find(this.options.selectors.imageRoot)
                .first()
                .find('.status');

            if (normalizeNullableInt(result.status) !== 0) {
                $status
                    .removeClass('d-none')
                    .attr('class', `status status${result.status}`)
                    .text(result.statusName);
            } else {
                $status.addClass('d-none').text('');
            }
        }

        renderMedia() {
            const result = this.state.product;
            const selectors = this.options.selectors;
            const templates = this.options.templates;
            const $productWrapper = this.$root.find(selectors.productSwiperWrapper).empty();
            const $previewWrapper = this.$root.find(selectors.previewSwiperWrapper).empty();
            const medium = Array.isArray(result.img_Medium) ? result.img_Medium : [];
            const small = Array.isArray(result.img_Small) ? result.img_Small : [];
            const original = Array.isArray(result.img_Original) ? result.img_Original : [];

            medium.forEach(img => {
                let $slide;

                if (img.fileType === 3) {
                    $slide = cloneTemplate(templates.videoSlide);
                    $slide.find('.pro_display').attr({
                        src: img.link[0],
                        alt: img.name,
                        'data-id': img.id,
                        'data-display-protype': 'video'
                    });
                } else if (img.fileType === 4) {
                    $slide = cloneTemplate(templates.ytVideoSlide);
                    const videoid = (img.name || '').split('&t=')[0];
                    $slide.find('.pro_display').attr({
                        src: `https://img.youtube.com/vi/${videoid}/hqdefault.jpg`,
                        alt: img.name,
                        'data-id': img.id,
                        'data-youtube-link': img.name,
                        'data-display-protype': 'youtube'
                    });
                } else if (img.fileType === 5) {
                    $slide = cloneTemplate(templates.slide3d);
                    $slide.find('.pro_display').attr({
                        src: img.link?.[0] || '',
                        alt: img.name,
                        'data-id': img.id,
                        'data-display-protype': '360view',
                        'data-filename-x': img.filenameX || '{index}.jpg',
                        'data-amount-x': img.amountX || 15
                    });
                } else {
                    $slide = cloneTemplate(templates.imageSlide);
                    $slide.find('.pro_display').attr({
                        src: img.link[0],
                        alt: img.name,
                        'data-id': img.id,
                        'data-display-protype': 'image'
                    });
                }

                $slide.find('.pro_display').imgCheck?.();
                $productWrapper.append($slide);
            });

            small.forEach(img => {
                const $slide = cloneTemplate(templates.previewSlide);
                const $img = $slide.find('img');
                let src = img.link?.[0] || '';

                if (img.fileType === 3) src = '/images/videopreview.jpg';
                if (img.fileType === 4) {
                    const videoid = (img.name || '').split('&t=')[0];
                    src = `https://img.youtube.com/vi/${videoid}/hqdefault.jpg`;
                }

                $img.attr({ src, alt: img.name }).data('id', img.id);
                $img.imgCheck?.();
                $previewWrapper.append($slide);
            });

            this.mediaViewer.setItems(original);
            this.initSwipers(small.length);
        }

        initSwipers(smallCount) {
            if (this.state.previewSwiper && this.state.previewSwiper.destroy) this.state.previewSwiper.destroy(true, true);
            if (this.state.productSwiper && this.state.productSwiper.destroy) this.state.productSwiper.destroy(true, true);

            if (smallCount <= 1) {
                $('.PreviewSwiper,.btn_swiper_prev_product,.btn_swiper_next_product').toggleClass('d-none', smallCount <= 1);
                return;
            }

            this.state.previewSwiper = new Swiper('.PreviewSwiper', {
                a11y: true,
                slidesPerView: 4,
                loop: false,
                spaceBetween: 10,
                freeMode: true,
                watchSlidesProgress: true,
                scrollbar: { el: '.swiper-scrollbar' },
                breakpoints: {
                    576: { slidesPerView: 4 },
                    768: { slidesPerView: 6 },
                    992: { slidesPerView: 8 }
                }
            });

            this.state.productSwiper = new Swiper('.ProductSwiper', {
                a11y: true,
                spaceBetween: 15,
                loop: true,
                navigation: {
                    nextEl: '.btn_swiper_next_product',
                    prevEl: '.btn_swiper_prev_product'
                },
                breakpoints: {
                    768: { allowTouchMove: true },
                    992: { allowTouchMove: false }
                },
                thumbs: { swiper: this.state.previewSwiper }
            });
        }

        renderSelectionArea() {
            this.renderSpecs();
            this.renderPrices();
            this.renderQuantity();
            this.syncButtonState();

            if (typeof this.options.hooks.onSelectionChanged === 'function') {
                this.options.hooks.onSelectionChanged(this.state.selection, this);
            }
        }

        renderSpecs() {
            const selectors = this.options.selectors;
            const templates = this.options.templates;
            const $options = this.$root.find(selectors.options);
            $options.find('.radio').remove();

            const stocks = this.state.selection.stocks || [];
            const specInfo = analyzeSpecStructure(stocks);

            if (specInfo.mode === 'none') {
                return;
            }

            const spec1Options = this.state.selection.getSpec1Options();
            const spec2Options = this.state.selection.getSpec2Options(this.state.selection.current.s1);

            if (specInfo.mode === 'double' && spec2Options.length > 0) {
                const $spec2 = cloneTemplate(templates.specRadio).attr('data-stype', '2');
                const $control = $spec2.find('.spec_control');

                spec2Options.forEach(item => {
                    const checked = item.id === this.state.selection.current.s2 ? 'checked' : '';
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
                const $spec1 = cloneTemplate(templates.specRadio).attr('data-stype', '1');
                const $spec1Control = $spec1.find('.spec_control');

                spec1Options.forEach(item => {
                    const checked = item.id === this.state.selection.current.s1 ? 'checked' : '';
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

        renderPriceBaseMeta(priceOptions) {
            const $baseMeta = this.$root.find('.price-base-meta');
            const $suggestPrice = $baseMeta.find('.suggest-price');
            const $originalPrice = $baseMeta.find('.original-price');

            if (!$baseMeta.length) return;

            const stock = this.state.selection.getActiveStock();
            const vm = buildPriceBaseViewModel(stock, priceOptions, this, this.state.product);

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

        renderPrices() {
            const $priceFrame = this.$root.find(this.options.selectors.priceFrame).empty();
            const priceOptions = this.state.selection.getPriceOptions();
            const hasMultiplePrice = priceOptions.length > 1;

            this.renderPriceBaseMeta(priceOptions);

            if (!priceOptions.length) {
                this.$addToCartButton.addClass('d-none');
                $priceFrame.addClass('d-none');
                this.$root.find('.options').addClass('d-none');
                this.$root.find('.price-base-meta').addClass('d-none');
                return;
            }

            $priceFrame.removeClass('d-none');
            this.$root.find('.options').removeClass('d-none');

            priceOptions.forEach((item, index) => {
                const $price = cloneTemplate(this.options.templates.priceItem);

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
                const vm = buildPriceViewModel(item, stock, this, this.state.product);

                const stockAvailable =
                    stock &&
                    !stock.timePrice &&
                    normalizeNullableInt(stock.stock) >= normalizeNullableInt(stock.minQty, 1);

                const isSelectable =
                    hasMultiplePrice &&
                    stockAvailable &&
                    this.state.selection.canAddToCart();

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
                    $badge.removeClass('d-none').text(this.t('bonusInsufficient'));
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

        renderQuantity() {
            const stock = this.state.selection.getActiveStock();
            if (!stock) return;

            const min = stock.minQty;
            const max = stock.stock - (stock.stock % stock.minQty);
            this.$quantityInput.attr({ min, max, step: stock.minQty }).val(this.state.selection.current.quantity);

            if (stock.stock < stock.minQty) {
                this.$quantityWrap.addClass('isEmpty');
            } else {
                this.$quantityWrap.removeClass('isEmpty');
            }
        }

        syncButtonState() {
            const stock = this.state.selection.getActiveStock();
            const canAdd = this.state.selection.canAddToCart();
            const priceOptions = this.state.selection.getPriceOptions();
            const selectedPrice = priceOptions.find(
                x => normalizeNullableInt(x.id) === normalizeNullableInt(this.state.selection.current.priceId)
            );
            const isLoggedIn = co.auth.isLoggedIn();
            const selectedIsBonusLack =
                isLoggedIn &&
                !!selectedPrice &&
                !!selectedPrice.disabled &&
                normalizeNullableInt(selectedPrice.bonus) > 0;

            this.$addToCartButton.removeClass('close bonus_lack');

            if (!this.options.canShop || !stock || stock.timePrice) {
                this.$addToCartButton.addClass('close');
            } else if (!canAdd) {
                if (selectedIsBonusLack) {
                    this.$addToCartButton.addClass('bonus_lack');
                } else {
                    this.$addToCartButton.addClass('close');
                }
            }

            if (!this.options.canShop || !stock) {
                this.$root.find('.counter').addClass('d-none');
            } else {
                this.$root.find('.counter').removeClass('d-none');
            }
        }

        addToCart() {
            if (typeof this.options.hooks.beforeAddToCart === 'function') {
                const shouldContinue = this.options.hooks.beforeAddToCart(this);
                if (shouldContinue === false) return;
            }

            if (localStorage.getItem('AgreePrivacy') == null) {
                Coker.sweet.warning(
                    this.t('addCartWarningTitle'),
                    this.t('addCartNeedPrivacy')
                );
                return;
            }

            const priceOptions = this.state.selection.getPriceOptions();
            const selectedPrice = priceOptions.find(
                x => normalizeNullableInt(x.id) === normalizeNullableInt(this.state.selection.current.priceId)
            );
            const isLoggedIn = co.auth.isLoggedIn();
            const selectedBonus = normalizeNullableInt(selectedPrice?.bonus);

            // 未登入且目前選的是紅利價：先要求登入
            if (!isLoggedIn && selectedBonus > 0) {
                Coker.sweet.warning(
                    this.t('addCartWarningTitle'),
                    '請登入會員',
                    () => {
                        if (typeof loginModal !== 'undefined' && loginModal && typeof loginModal.show === 'function') {
                            loginModal.show();
                        }
                    }
                );
                return;
            }

            // 已登入但紅利不足（前端先做 UX 提示；後端仍需再驗證）
            if (
                isLoggedIn &&
                selectedPrice &&
                normalizeNullableInt(selectedPrice.bonus) > 0 &&
                normalizeNullableInt(this.options.totalBonus) < normalizeNullableInt(selectedPrice.bonus)
            ) {
                Coker.sweet.warning(
                    this.t('addCartWarningTitle'),
                    this.t('bonusInsufficient')
                );
                return;
            }

            if (!this.state.selection.canAddToCart()) {
                Coker.sweet.warning(
                    this.t('addCartWarningTitle'),
                    this.t('addCartNeedSelection')
                );
                return;
            }

            const payload = this.state.selection.buildCartPayload(this.state.productId);

            this.options.api.addToCart(payload).done((result) => {
                if (!result.success) {
                    if (result.error === '商品庫存不足') {
                        Coker.sweet.warning(result.error, result.message, function () {
                            location.reload(true);
                        });
                    } else {
                        Coker.sweet.error(
                            this.t('commonErrorTitle'),
                            result.message || this.t('addCartError'),
                            null
                        );
                    }
                    return;
                }

                Coker.sweet.success(this.t('addCartSuccess'), null, true);

                const type = (result.message || '').substr(0, 1);
                const id = (result.message || '').substr(1);

                this.options.api.getCartDropOne(id).done((drop) => {
                    if (type === 'N') {
                        if (typeof window.CartDropAdd === 'function') window.CartDropAdd(drop);
                    } else {
                        if (typeof window.CartDropUpdate === 'function') window.CartDropUpdate(drop);
                    }
                });

                this.state.selection.decreaseStockAfterAdd();
                this.state.selection.setQuantity(this.state.selection.getActiveStock()?.minQty || 1);
                this.renderSelectionArea();

                if (typeof this.options.hooks.afterAddToCart === 'function') {
                    this.options.hooks.afterAddToCart(result, this);
                }
            }).fail(() => {
                Coker.sweet.error(
                    this.t('commonErrorTitle'),
                    this.t('addCartError'),
                    null,
                    true
                );
            });
        }

        initShare() {
            if (typeof window.ShareBlockInit === 'function') {
                window.ShareBlockInit();
            }
        }

        initFavorite() {
            const $btn = this.$pageRoot.find(this.options.selectors.favoritesButton);
            if ($btn.length === 0) return;
            if (!this.options.api.checkFavorite || !this.options.api.addFavorite || !this.options.api.deleteFavorite) return;

            this.options.api.checkFavorite(this.state.productId)?.done((check) => {
                if (check && check.success) {
                    $btn.data('Fid', check.message);
                    $btn.addClass('turn');
                    $btn.attr('title', this.t('removeFavorite'));
                } else {
                    $btn.attr('title', this.t('addFavorite'));
                }
            });

            $btn.off('click.productFavorite').on('click.productFavorite', () => {
                if ($btn.hasClass('turn')) {
                    this.options.api.deleteFavorite($btn.data('Fid'))?.done((result) => {
                        if (result.success) {
                            $btn.removeClass('turn');
                            $btn.attr('title', this.t('addFavorite'));
                            Coker.sweet.success(this.t('removeFavoriteSuccess'), null, true);
                        }
                    });
                } else {
                    this.options.api.addFavorite(this.state.productId)?.done((favorites) => {
                        if (favorites.success) {
                            $btn.addClass('turn');
                            $btn.data('Fid', favorites.message);
                            $btn.attr('title', this.t('removeFavorite'));
                            Coker.sweet.success(this.t('addFavoriteSuccess'), null, true);
                        }
                    });
                }
            });
        }

        initSwitchPage() {
            const $switch = $(this.options.selectors.switchPage);
            if ($switch.length === 0 || !this.options.api.switchPage) return;

            const currentUrl = window.location.pathname + window.location.search;
            const productMarker = '/product/';
            const productIndex = currentUrl.indexOf(productMarker);

            if (productIndex < 0) {
                $switch.remove();
                return;
            }

            const catalog = currentUrl.substring(0, productIndex);
            const pathAfterProduct = currentUrl.split('/product/')[1] || '';
            const pathPart = pathAfterProduct.split('?')[0];
            const pathParts = pathPart.split('/').filter(Boolean);
            const productid = pathParts[0];
            const searchtext = decodeURIComponent(pathParts[1] || '');
            const urlParams = new URLSearchParams(window.location.search);
            const dirid = urlParams.get('dirid');
            const diridList = dirid == null ? null : dirid.split(',').map(Number);
            const filter = urlParams.get('filter');
            const routername = catalog.substring(catalog.lastIndexOf('/') + 1);

            $switch.find('.btn_list').attr('href', catalog);

            const bindSearchMode = (list) => {
                if (!Array.isArray(list) || list.length === 0) {
                    $switch.remove();
                    return;
                }

                const index = list.findIndex(p => String(p.key) === String(productid));
                $switch.find('.btn_list').attr('href', `${catalog}/Get/3/${searchtext}`);

                if (index > 0) {
                    const prev = list[index - 1];
                    const link = `${catalog}/product/${prev.key}/${searchtext}${filter ? `?filter=${filter}` : ''}`;
                    $switch.find('.btn_prev').attr({ href: link, title: prev.value }).removeClass('disabled');
                }

                if (index < list.length - 1 && index >= 0) {
                    const next = list[index + 1];
                    const link = `${catalog}/product/${next.key}/${searchtext}${filter ? `?filter=${filter}` : ''}`;
                    $switch.find('.btn_next').attr({ href: link, title: next.value }).removeClass('disabled');
                }
            };

            if (routername === 'search' && searchtext && sessionStorage.getItem(`product-${searchtext}`)) {
                bindSearchMode(JSON.parse(sessionStorage.getItem(`product-${searchtext}`)));
                return;
            }

            this.options.api.switchPage({
                id: productid,
                dirids: diridList,
                routername: routername,
                searchtext: searchtext,
                filters: filter,
                type: 1
            })?.done((result) => {
                if (!Array.isArray(result) || result.length === 0) {
                    $switch.remove();
                    return;
                }

                if (routername === 'search') {
                    sessionStorage.setItem(`product-${searchtext}`, JSON.stringify(result));
                    bindSearchMode(result);
                    return;
                }

                if (result[0]?.key != null) {
                    $switch.find('.btn_prev').attr({
                        href: `${catalog}/product/${result[0].key}`,
                        title: result[0].value
                    }).removeClass('disabled');
                }

                if (result[1]?.key != null) {
                    $switch.find('.btn_next').attr({
                        href: `${catalog}/product/${result[1].key}`,
                        title: result[1].value
                    }).removeClass('disabled');
                }
            });
        }
    }

    function createProductContent(options) {
        const controller = new ProductContentController(options);
        controller.init();
        return controller;
    }

    window.ProductContentModule = {
        create: createProductContent,
        ProductContentController,
        ProductSelectionEngine,
        ProductMediaViewer,
        formatPriceText,
        buildPriceSummary,
        resolveText,
        analyzeSpecStructure,
        buildPriceViewModel
    };

    window.PageReady = function () {
        window.productContentPage = createProductContent({
            productId: window.PageId,
            canShop: $('.btn_addToCar').length > 0,
            totalBonus: typeof totalBonus !== 'undefined' ? totalBonus : 0,
            orderPrice: typeof orderPrice !== 'undefined' ? orderPrice : false,
            i18n: function (key, fallback) {
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
        });
    };

})(window, window.jQuery);