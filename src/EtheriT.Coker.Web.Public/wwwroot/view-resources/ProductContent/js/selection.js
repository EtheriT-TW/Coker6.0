(function (window, $) {
    'use strict';

    if (!$) {
        throw new Error('ProductContent requires jQuery.');
    }

    const I = (window.ProductContentInternals = window.ProductContentInternals || {});
    const {
        DEFAULT_TEXTS, DEFAULTS, registerLayout, getLayoutFactory, toInt, normalizeNullableInt,
        readMinQty, cloneTemplate, formatNumber, formatText, resolveText, defaultI18n,
        formatPriceText, analyzeSpecStructure, buildPriceSummary, buildPriceViewModel,
        buildPriceBaseViewModel, isStockAvailable, clampQuantity, isLoggedIn,
        createCartPayload, runBuyGuard, submitCart, ProductMediaViewer
    } = I;

    class ProductSelectionEngine {
        constructor(product, options) {
            this.product = product || { stocks: [] };
            this.options = options;
            this.canShop = !!options.canShop;
            this.noStockManagement = !!(this.product && this.product.noStockManagement);
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
                minQty: readMinQty(stock),
                canPurchase: typeof stock.canPurchase === 'boolean' ? stock.canPurchase : null,
                maxPurchaseQuantity: stock.maxPurchaseQuantity == null
                    ? null
                    : normalizeNullableInt(stock.maxPurchaseQuantity),
                purchaseUnavailableReason: stock.purchaseUnavailableReason || '',
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

            const firstAvailable = this.stocks.find(x => !this.canShop || x.canPurchase === true) || this.stocks[0];

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
                this.current.quantity = isStockAvailable(activeStock, this.noStockManagement)
                    ? activeStock.minQty
                    : 0;
            }
        }

        getSpec1Options() {
            return Array.from(this.specMap.spec1.entries()).map(([id, title]) => ({
                id,
                title,
                enabled: this.stocks.some(x => x.s1id === id && (!this.canShop || x.canPurchase === true))
            }));
        }

        getSpec2Options(spec1Id) {
            return this.stocks
                .filter(x => x.s1id === normalizeNullableInt(spec1Id))
                .map(x => ({
                    id: x.s2id,
                    title: x.s2Title,
                    enabled: !this.canShop || x.canPurchase === true
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
                this.current.quantity = isStockAvailable(activeStock, this.noStockManagement)
                    ? activeStock.minQty
                    : 0;
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

            if (!isLoggedIn()) return false;

            return normalizeNullableInt(this.options.totalBonus) < bonus;
        }

        setPrice(priceId) {
            this.current.priceId = normalizeNullableInt(priceId, null);
        }

        setQuantity(quantity) {
            const stock = this.getActiveStock();
            if (!stock) return;

            this.current.quantity = clampQuantity(stock, quantity, this.noStockManagement);
        }

        canAddToCart() {
            const stock = this.getActiveStock();
            if (!this.canShop) return false;
            if (this.product.canPurchase === false) return false;
            if (!stock) return false;
            if (stock.canPurchase !== true) return false;
            if (!this.current.priceId) return false;
            return true;
        }

        buildCartPayload(productId) {
            return createCartPayload(productId, {
                priceId: this.current.priceId,
                s1id: this.current.s1,
                s2id: this.current.s2,
                quantity: this.current.quantity
            });
        }
    }

    I.ProductSelectionEngine = ProductSelectionEngine;
})(window, window.jQuery);
