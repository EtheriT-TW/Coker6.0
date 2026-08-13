(function (window, $) {
    "use strict";

    const RULE = {
        amountDiscount: 1,
        percentDiscount: 2,
        addOnPurchase: 30
    };

    const CONDITION = {
        orderAmount: 1,
        scopeAmount: 2,
        scopeQuantity: 4,
        buySpecificProduct: 5
    };

    const SCOPE = {
        allOrder: 1,
        specificProducts: 2
    };

    const TARGET = {
        product: 1
    };

    function valueOf(source, camelName, pascalName, fallback) {
        if (!source) return fallback;
        const value = source[camelName] ?? source[pascalName];
        return value === undefined || value === null ? fallback : value;
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function formatMoney(value) {
        return parseNumber(value).toLocaleString("zh-TW", {
            maximumFractionDigits: 2
        });
    }

    function parseNumber(value) {
        const normalized = String(value ?? "").replace(/,/g, "").trim();
        if (normalized === "") return 0;
        const number = Number(normalized);
        return Number.isFinite(number) ? number : 0;
    }

    function summarizeProductNames(items, propertyName, emptyText) {
        if (!items.length) return emptyText;
        const names = items.slice(0, 2).map(x => `「${x[propertyName]}」`);
        return items.length > 2
            ? `${names.join("、")}等 ${items.length} 項商品`
            : names.join("、");
    }

    const MarketingPage = {
        formId: "MarketingForm",
        pageRootSelector: "#MarketingPageRoot",
        hashPage: null,
        marketingListGridEvent: null,
        isGridLookupApplied: false,
        keyId: 0,
        isInitialized: false,
        options: null,
        scopeItems: [],
        rewardItems: [],
        rewardItemKeySeed: 0,
        canUseProdAddition: false,
        productModalReady: false,
        offerPreviewSwiper: null,
        isRuleModeLocked: false,
        repeatableWasCustomized: false,
        canStackWasCustomized: false,

        init: function () {
            if (this.isInitialized) return;
            this.isInitialized = true;

            this.cacheElements();
            this.initCommonForm();
            this.initHashPage();
            this.initStaticEvents();
            this.loadOptions();
            this.waitForProductModal();
        },

        cacheElements: function () {
            this.canUseProdAddition = $("#MarketingCanUseProdAddition").val() === "true";
            this.$status = $("#SelectStatus");
            this.$ruleMode = $("input[name='RuleMode']");
            this.$neverEnd = $("#CheckNeverEnd");
            this.$endTime = $("#InputEndTime");
            this.$endTimeSection = $(".endTimeSection");
            this.$discountSettings = $("#DiscountRuleSettings");
            this.$addOnSettings = $("#AddOnRuleSettings");
            this.$rewardAmountSection = $(".rewardAmountSection");
            this.$rewardPercentSection = $(".rewardPercentSection");
            this.$discountAmount = $("#InputDiscountAmount");
            this.$discountPercent = $("#InputDiscountPercent");
            this.$maxDiscountAmount = $("#InputMaxDiscountAmount");
            this.$minAmount = $("#InputMinAmount");
            this.$addOnMinAmount = $("#InputAddOnMinAmount");
            this.$addOnMinQuantity = $("#InputAddOnMinQuantity");
            this.$scopeProductSection = $(".scopeProductSection");
            this.$addOnAmountSection = $("#AddOnAmountSection");
            this.$addOnQuantitySection = $("#AddOnQuantitySection");
            this.$canStack = $("#CheckCanStack");
            this.$repeatable = $("#CheckRepeatable");
            this.$repeatableSection = $(".repeatableSection");
        },

        initCommonForm: function () {
            const self = this;
            _c.Form.init(this.formId, function () {
                return self.submitForm();
            });
        },

        initHashPage: function () {
            const self = this;
            this.hashPage = Coker.HashPage.create({
                root: this.pageRootSelector,
                defaultHash: "List",
                listHash: "List",
                newHash: "new",
                listPageKey: "List",
                contentPageKey: "Content",
                titleSelector: "[data-hash-title]",
                scrollTarget: "[data-hash-page='Content']",
                useStack: true,
                onList: function () { self.onEnterList(); },
                onNew: function () { self.onEnterNew(); },
                onEdit: function (state) { self.onEnterEdit(state.id); }
            });
        },

        initStaticEvents: function () {
            const self = this;

            $(".btn_back").off("click.marketing").on("click.marketing", function (e) {
                e.preventDefault();
                Coker.sweet.confirm("返回行銷活動列表", "資料將不被保存", "確定", "取消", function () {
                    if (self.hashPage) self.hashPage.goList();
                    else window.location.hash = "List";
                });
            });

            $(".btn_add").off("click.marketing").on("click.marketing", function (e) {
                e.preventDefault();
                if (self.hashPage) self.hashPage.goNew();
            });

            this.$ruleMode.off("change.marketing").on("change.marketing", function () {
                if (self.keyId === 0 && !self.repeatableWasCustomized) {
                    self.$repeatable.prop("checked", self.isAddOnMode());
                }
                if (self.keyId === 0 && !self.canStackWasCustomized) {
                    self.$canStack.prop("checked", self.isAddOnMode());
                }
                self.applyRuleModeUI();
            });

            this.$canStack.off("change.marketing").on("change.marketing", function () {
                if (self.keyId === 0) self.canStackWasCustomized = true;
            });

            this.$repeatable.off("change.marketingPreview").on("change.marketingPreview", function () {
                if (self.keyId === 0) self.repeatableWasCustomized = true;
                self.renderOfferPreview();
            });

            $(".marketing-rule-card").off("click.marketingRuleCard").on("click.marketingRuleCard", function (e) {
                if ($(e.target).is("input")) return;
                e.preventDefault();
                const $input = $(this).find("input[type='radio']");
                if (!$input.prop("checked")) $input.prop("checked", true).trigger("change");
            });

            this.$neverEnd.off("change.marketing").on("change.marketing", function () {
                self.applyNeverEndUI();
            });

            this.$discountPercent.off("input.marketing").on("input.marketing", function () {
                let value = String($(this).val() || "").replace(/[^\d]/g, "");
                if (value.length > 2) value = value.substring(0, 2);
                $(this).val(value);
            }).on("blur.marketing", function () {
                const normalizedValue = self.normalizeDiscountRateForView($(this).val());
                if (normalizedValue !== "") $(this).val(normalizedValue);
            });

            $(document)
                .off("click.marketingPicker", "[data-open-product-picker]")
                .on("click.marketingPicker", "[data-open-product-picker]", function () {
                    const selector = $(this).attr("data-open-product-picker");
                    if (window.ProdListModalApi) window.ProdListModalApi.open(selector);
                })
                .off("click.marketingScopeRemove", "[data-remove-scope]")
                .on("click.marketingScopeRemove", "[data-remove-scope]", function () {
                    self.removeScopeProduct(Number($(this).attr("data-remove-scope")));
                })
                .off("change.marketingRewardStock", "[data-reward-stock]")
                .on("change.marketingRewardStock", "[data-reward-stock]", function () {
                    self.changeRewardStock($(this).attr("data-reward-stock"), Number($(this).val()));
                })
                .off("input.marketingReward", "[data-reward-field]")
                .on("input.marketingReward", "[data-reward-field]", function () {
                    const field = $(this).attr("data-reward-field");
                    self.updateRewardField(
                        $(this).attr("data-reward-key"),
                        field,
                        $(this).val()
                    );
                    if (field === "maxQuantityPerOrder") {
                        self.validateRewardItemLimitInput(this, true);
                    }
                })
                .off("click.marketingRewardDuplicate", "[data-duplicate-reward]")
                .on("click.marketingRewardDuplicate", "[data-duplicate-reward]", function () {
                    self.duplicateRewardProduct($(this).attr("data-duplicate-reward"));
                })
                .off("click.marketingRewardRemove", "[data-remove-reward]")
                .on("click.marketingRewardRemove", "[data-remove-reward]", function () {
                    self.removeRewardProduct($(this).attr("data-remove-reward"));
                });

            $("#" + this.formId)
                .off("keydown.marketingEnter")
                .on("keydown.marketingEnter", function (event) {
                    if (event.key !== "Enter" || $(event.target).is("textarea")) return;

                    // Enter in the batch-price field applies the price to reward rows only;
                    // it must not submit and save the entire marketing campaign.
                    event.preventDefault();
                    if (event.target.id === "RewardBatchPriceValue") {
                        self.applyBatchOfferPrices();
                    }
                });

            $("#RewardBatchPriceMode")
                .off("change.marketingBatchPrice")
                .on("change.marketingBatchPrice", function () { self.updateBatchPriceUI(); });

            $("#ApplyRewardBatchPrice")
                .off("click.marketingBatchPrice")
                .on("click.marketingBatchPrice", function () { self.applyBatchOfferPrices(); });

            $("#InputSelectionQuantity, #InputAddOnMinAmount, #InputAddOnMinQuantity")
                .off("input.marketingPreview")
                .on("input.marketingPreview", function () {
                    if (this.id === "InputSelectionQuantity") self.syncRewardItemLimitInputs();
                    self.renderOfferPreview();
                });
        },

        waitForProductModal: function (attempt) {
            const self = this;
            const count = attempt || 0;

            if (window.ProdListModalApi) {
                window.ProdListModalApi.bind("#ScopeProductPicker", { setAsDefault: true });
                window.ProdListModalApi.bind("#RewardProductPicker");
                window.ProdListModalApi.setAfterSaveCallback(function (target, state) {
                    self.onProductModalSaved(target, state);
                    window.ProdListModalApi.refresh();
                });
                this.productModalReady = true;
                this.syncModalStates();
                return;
            }

            if (count < 100) {
                window.setTimeout(function () { self.waitForProductModal(count + 1); }, 100);
            }
        },

        onProductModalSaved: function (target, state) {
            const picker = target?.dataset?.marketingPicker;
            const selected = (state?.items || []).filter(x => !x.IsDeleted);

            if (picker === "scope") {
                this.applySelectedScopeProducts(selected);
                return;
            }

            if (picker === "reward") {
                this.applySelectedRewardProducts(selected);
            }
        },

        applySelectedScopeProducts: function (selected) {
            const oldMap = new Map(this.scopeItems.map(x => [x.targetId, x]));
            this.scopeItems = selected.map(function (item) {
                const productId = Number(item.FK_ProdId ?? item.fK_ProdId ?? item.Id ?? item.id);
                const existing = oldMap.get(productId);
                return Object.assign(existing || {
                    id: 0,
                    targetType: TARGET.product,
                    targetId: productId,
                    requiredQuantityPerQualification: 1
                }, {
                    targetName: item.Prod_Name ?? item.prod_Name ?? item.Title ?? item.title ?? "",
                    imageUrl: item.MinsizeImage ?? item.minsizeImage ?? "/images/noImg.jpg",
                    productStatus: Number(item.ProductStatus ?? item.productStatus ?? 0),
                    productStatusName: item.ProductStatusName ?? item.productStatusName ?? "一般",
                    visible: item.Visible ?? item.visible ?? true,
                    available: item.Available ?? item.available ?? true,
                    noStockManagement: item.NoStockManagement ?? item.noStockManagement ?? false,
                    stockQuantity: item.StockQuantity ?? item.stockQuantity ?? null,
                    alertQuantity: item.AlertQuantity ?? item.alertQuantity ?? null
                });
            });
            this.renderScopeRows();
        },

        applySelectedRewardProducts: function (selected) {
            const self = this;
            const selectedIds = selected.map(x => Number(x.FK_ProdId ?? x.fK_ProdId ?? x.Id ?? x.id));
            this.rewardItems = this.rewardItems.filter(x => selectedIds.includes(x.productId));

            const selectedMap = new Map(selected.map(function (item) {
                const id = Number(item.FK_ProdId ?? item.fK_ProdId ?? item.Id ?? item.id);
                return [id, item];
            }));
            this.rewardItems.forEach(function (item) {
                const selectedProduct = selectedMap.get(item.productId);
                if (!selectedProduct) return;
                item.productStatus = Number(selectedProduct.ProductStatus ?? selectedProduct.productStatus ?? item.productStatus ?? 0);
                item.productStatusName = selectedProduct.ProductStatusName ?? selectedProduct.productStatusName ?? item.productStatusName ?? "一般";
                item.visible = selectedProduct.Visible ?? selectedProduct.visible ?? item.visible ?? true;
                item.available = selectedProduct.Available ?? selectedProduct.available ?? item.available ?? true;
                item.noStockManagement = selectedProduct.NoStockManagement ?? selectedProduct.noStockManagement ?? item.noStockManagement ?? false;
            });

            const existingIds = new Set(this.rewardItems.map(x => x.productId));
            const newProducts = selected.filter(function (item) {
                const id = Number(item.FK_ProdId ?? item.fK_ProdId ?? item.Id ?? item.id);
                return !existingIds.has(id);
            });

            Promise.all(newProducts.map(function (item) {
                return self.createRewardItemFromProduct(item);
            })).then(function (items) {
                items.filter(Boolean).forEach(x => self.rewardItems.push(x));
                self.renderRewardRows();
            }).catch(function (err) {
                self.handleRequestError(err, "讀取商品規格失敗");
            });

            this.renderRewardRows();
        },

        createRewardItemFromProduct: function (product) {
            const productId = Number(product.FK_ProdId ?? product.fK_ProdId ?? product.Id ?? product.id);
            const productName = product.Prod_Name ?? product.prod_Name ?? product.Title ?? product.title ?? "";
            const imageUrl = product.MinsizeImage ?? product.minsizeImage ?? "/images/noImg.jpg";

            return co.Product.Get.ProdStock(productId).then(function (stocks) {
                const normalizedStocks = (stocks || []).map(MarketingPage.normalizeStock);
                if (!normalizedStocks.length) {
                    Coker.sweet.error("無法加入商品", `${productName} 沒有可使用的商品規格。`, null, true);
                    return null;
                }

                const stock = normalizedStocks[0];
                return {
                    clientKey: MarketingPage.nextRewardItemKey(),
                    id: 0,
                    productId: productId,
                    productStockId: stock.id,
                    productName: productName,
                    stockName: stock.name,
                    sku: stock.sku,
                    originalPrice: stock.originalPrice,
                    suggestPrice: stock.suggestPrice,
                    prices: stock.prices,
                    productStatus: Number(product.ProductStatus ?? product.productStatus ?? 0),
                    productStatusName: product.ProductStatusName ?? product.productStatusName ?? "一般",
                    visible: product.Visible ?? product.visible ?? true,
                    available: product.Available ?? product.available ?? true,
                    noStockManagement: product.NoStockManagement ?? product.noStockManagement ?? false,
                    stockQuantity: stock.stock,
                    alertQuantity: stock.alertQuantity,
                    offerPrice: stock.originalPrice,
                    offerPriceCustomized: false,
                    maxQuantityPerOrder: 1,
                    enabled: true,
                    sortOrder: MarketingPage.rewardItems.length,
                    imageUrl: imageUrl,
                    stocks: normalizedStocks
                };
            });
        },

        nextRewardItemKey: function () {
            this.rewardItemKeySeed += 1;
            return `reward-${this.rewardItemKeySeed}`;
        },

        normalizeStock: function (stock) {
            const s1 = valueOf(stock, "s1_Title", "S1_Title", "");
            const s2 = valueOf(stock, "s2_Title", "S2_Title", "");
            const description = valueOf(stock, "specDescription", "SpecDescription", "");
            const sku = valueOf(stock, "subItemNo", "SubItemNo", "");
            const name = [s1, s2].filter(Boolean).join(" / ") || description || sku || "預設規格";
            const prices = (valueOf(stock, "prices", "Prices", []) || []).map(function (price) {
                return {
                    roleId: Number(valueOf(price, "fK_RId", "FK_RId", 0)),
                    roleName: valueOf(price, "roleName", "RoleName", ""),
                    price: Number(valueOf(price, "price", "Price", 0)),
                    bonus: Number(valueOf(price, "bonus", "Bonus", 0))
                };
            });
            const cashPrices = prices.filter(x => x.bonus === 0 && x.price >= 0);
            const basePrice = cashPrices.find(x => x.roleId === 0 || x.roleId === 1) || cashPrices[0];
            const suggestPrice = Number(valueOf(stock, "suggestPrice", "SuggestPrice",
                valueOf(stock, "price", "Price", 0)));
            return {
                id: Number(valueOf(stock, "id", "Id", 0)),
                name: name,
                sku: sku,
                originalPrice: basePrice ? basePrice.price : suggestPrice,
                suggestPrice: suggestPrice,
                prices: prices,
                stock: valueOf(stock, "stock", "Stock", null),
                alertQuantity: valueOf(stock, "alert_Qty", "Alert_Qty", null)
            };
        },

        hydrateRewardStocks: function () {
            const self = this;
            return Promise.all(this.rewardItems.map(function (item) {
                return co.Product.Get.ProdStock(item.productId).then(function (stocks) {
                    item.stocks = (stocks || []).map(self.normalizeStock);
                    const selectedStock = item.stocks.find(x => x.id === item.productStockId);
                    if (selectedStock) {
                        item.stockName = selectedStock.name;
                        item.sku = selectedStock.sku;
                        item.originalPrice = selectedStock.originalPrice;
                        item.suggestPrice = selectedStock.suggestPrice;
                        item.prices = selectedStock.prices;
                        item.stockQuantity = selectedStock.stock;
                        item.alertQuantity = selectedStock.alertQuantity;
                    }
                });
            }));
        },

        removeScopeProduct: function (productId) {
            this.scopeItems = this.scopeItems.filter(x => x.targetId !== productId);
            this.renderScopeRows();
            this.syncModalStates();
        },

        removeRewardProduct: function (clientKey) {
            this.rewardItems = this.rewardItems.filter(x => x.clientKey !== clientKey);
            this.renderRewardRows();
            this.syncModalStates();
        },

        duplicateRewardProduct: function (clientKey) {
            const sourceIndex = this.rewardItems.findIndex(x => x.clientKey === clientKey);
            if (sourceIndex < 0) return;

            const source = this.rewardItems[sourceIndex];
            const usedStockIds = new Set(this.rewardItems
                .filter(x => x.productId === source.productId)
                .map(x => x.productStockId));
            const stock = (source.stocks || []).find(x => !usedStockIds.has(x.id));
            if (!stock) {
                Coker.sweet.error("無其他規格", "此商品的所有規格都已加入活動。", null, true);
                return;
            }

            const duplicate = Object.assign({}, source, {
                clientKey: this.nextRewardItemKey(),
                id: 0,
                productStockId: stock.id,
                stockName: stock.name,
                sku: stock.sku,
                originalPrice: stock.originalPrice,
                suggestPrice: stock.suggestPrice,
                prices: stock.prices,
                stockQuantity: stock.stock,
                alertQuantity: stock.alertQuantity,
                offerPrice: stock.originalPrice,
                offerPriceCustomized: false,
                sortOrder: sourceIndex + 1
            });
            this.rewardItems.splice(sourceIndex + 1, 0, duplicate);
            this.renderRewardRows();
        },

        changeRewardStock: function (clientKey, stockId) {
            const item = this.rewardItems.find(x => x.clientKey === clientKey);
            if (!item) return;
            const stock = (item.stocks || []).find(x => x.id === stockId);
            if (!stock) return;
            if (this.rewardItems.some(x => x.clientKey !== clientKey && x.productStockId === stockId)) {
                Coker.sweet.error("規格重複", "同一商品規格不可重複加入活動。", null, true);
                this.renderRewardRows();
                return;
            }

            const shouldFollowOriginalPrice = item.offerPriceCustomized !== true;
            item.productStockId = stock.id;
            item.stockName = stock.name;
            item.sku = stock.sku;
            item.originalPrice = stock.originalPrice;
            item.suggestPrice = stock.suggestPrice;
            item.prices = stock.prices;
            item.stockQuantity = stock.stock;
            item.alertQuantity = stock.alertQuantity;
            if (shouldFollowOriginalPrice) item.offerPrice = stock.originalPrice;
            this.renderRewardRows();
        },

        updateRewardField: function (clientKey, field, rawValue) {
            const item = this.rewardItems.find(x => x.clientKey === clientKey);
            if (!item) return;
            item[field] = parseNumber(rawValue);
            if (field === "offerPrice") {
                item.offerPriceCustomized = true;
                this.updateBenefitBadge(item);
            }
            this.renderOfferPreview();
        },

        updateBenefitBadge: function (item) {
            const isGift = Number(item.offerPrice) === 0;
            const $badge = $(`[data-reward-benefit="${item.clientKey}"]`);
            $badge
                .toggleClass("is-gift", isGift)
                .toggleClass("is-addon", !isGift)
                .text(isGift ? "贈品" : "加價購");
        },

        refreshNumberFormats: function (scope) {
            const $scope = scope ? $(scope) : $("#" + this.formId);
            if (!$scope.length || !_c.Form) return;

            _c.Form.initNumberFormatter($scope);
            $scope.find('input[data-origin-type="number"], input[data-form-type="number"], input[data-form-type="number-format"]').each(function () {
                if (document.activeElement === this || $(this).val() === "") return;
                $(this).val(_c.Form.formatElementValue($(this), $(this).val()));
            });
        },

        getProductAdminUrl: function (productId) {
            return `/ProductManagement/ProductList#${encodeURIComponent(productId)}`;
        },

        getProductFrontUrl: function (productId) {
            const rootUrl = typeof defaultUrl === "string" ? defaultUrl.replace(/\/+$/, "") : "";
            const websiteOrgName = typeof OrgName === "string" ? OrgName.replace(/^\/+|\/+$/g, "") : "";
            return `${rootUrl}/${encodeURIComponent(websiteOrgName)}/search/product/${encodeURIComponent(productId)}`;
        },

        renderScopeRows: function () {
            const self = this;
            const html = this.scopeItems.map(function (item) {
                return `<div class="marketing-product-row">
                    <div class="marketing-product-row-header">
                        <div class="marketing-product-main">
                            <img src="${escapeHtml(item.imageUrl || "/images/noImg.jpg")}" alt="">
                            <div class="marketing-product-heading">
                                <div class="fw-bold">${escapeHtml(item.targetName)}</div>
                                <div class="text-muted small">商品 ID：${item.targetId}</div>
                                ${self.renderProductMeta(item)}
                            </div>
                        </div>
                        <div class="marketing-product-actions">
                            <a class="marketing-product-link is-admin" href="${escapeHtml(self.getProductAdminUrl(item.targetId))}"
                                target="_blank" rel="noopener" title="前往後台商品管理" aria-label="前往後台管理 ${escapeHtml(item.targetName)}">
                                <span class="material-symbols-outlined" aria-hidden="true">edit_square</span>
                            </a>
                            <a class="marketing-product-link is-front" href="${escapeHtml(self.getProductFrontUrl(item.targetId))}"
                                target="_blank" rel="noopener" title="查看前台商品" aria-label="查看前台商品 ${escapeHtml(item.targetName)}">
                                <span class="material-symbols-outlined" aria-hidden="true">storefront</span>
                            </a>
                            <button type="button" class="marketing-remove-button" data-remove-scope="${item.targetId}" title="移除商品" aria-label="移除 ${escapeHtml(item.targetName)}">
                                <span class="material-symbols-outlined" aria-hidden="true">delete</span>
                            </button>
                        </div>
                    </div>
                </div>`;
            }).join("");

            $("#ScopeProductRows").html(html || '<div class="marketing-empty-state">尚未選擇指定商品</div>');
            this.refreshNumberFormats("#ScopeProductRows");
        },

        renderRewardRows: function () {
            const self = this;
            const selectionLimit = Math.max(parseNumber($("#InputSelectionQuantity").val()), 1);
            const html = this.rewardItems.map(function (item) {
                const usedStockIds = new Set(self.rewardItems
                    .filter(x => x.productId === item.productId)
                    .map(x => x.productStockId));
                const canDuplicate = (item.stocks || []).some(x => !usedStockIds.has(x.id));
                const stockOptions = (item.stocks || []).map(function (stock) {
                    const selected = stock.id === item.productStockId ? " selected" : "";
                    const stockText = stock.stock == null ? "不限庫存" : `庫存 ${Number(stock.stock).toLocaleString()}`;
                    return `<option value="${stock.id}"${selected}>${escapeHtml(stock.name)}${stock.sku ? `｜${escapeHtml(stock.sku)}` : ""}｜${stockText}</option>`;
                }).join("");
                const typeLabel = Number(item.offerPrice) === 0 ? "贈品" : "加價購";
                const typeClass = Number(item.offerPrice) === 0 ? "is-gift" : "is-addon";

                return `<div class="marketing-reward-row">
                    <div class="marketing-product-row-header">
                        <div class="marketing-product-main">
                            <img src="${escapeHtml(item.imageUrl || "/images/noImg.jpg")}" alt="">
                            <div class="marketing-product-heading">
                                <div class="fw-bold">${escapeHtml(item.productName)}</div>
                                <span class="marketing-benefit-badge ${typeClass}" data-reward-benefit="${item.clientKey}">${typeLabel}</span>
                                ${self.renderProductMeta(item)}
                            </div>
                        </div>
                        <div class="marketing-product-actions">
                            <a class="marketing-product-link is-admin" href="${escapeHtml(self.getProductAdminUrl(item.productId))}"
                                target="_blank" rel="noopener" title="前往後台商品管理" aria-label="前往後台管理 ${escapeHtml(item.productName)}">
                                <span class="material-symbols-outlined" aria-hidden="true">edit_square</span>
                            </a>
                            <button type="button" class="marketing-copy-button" data-duplicate-reward="${item.clientKey}"
                                title="${canDuplicate ? "複製並選擇其他規格" : "所有規格都已加入活動"}"
                                aria-label="複製 ${escapeHtml(item.productName)} 的其他規格"${canDuplicate ? "" : " disabled"}>
                                <span class="material-symbols-outlined" aria-hidden="true">content_copy</span>
                            </button>
                            <button type="button" class="marketing-remove-button" data-remove-reward="${item.clientKey}" title="移除商品規格" aria-label="移除 ${escapeHtml(item.productName)}">
                                <span class="material-symbols-outlined" aria-hidden="true">delete</span>
                            </button>
                        </div>
                    </div>
                    <div class="marketing-reward-fields">
                        <label>商品規格
                            <select class="form-select form-select-sm" data-reward-stock="${item.clientKey}">${stockOptions}</select>
                        </label>
                        ${self.renderPriceReferences(item)}
                        <label>活動價
                            <div class="input-group input-group-sm">
                                <span class="input-group-text">NT$</span>
                                <input class="form-control" type="number" min="0" value="${item.offerPrice}"
                                    data-reward-key="${item.clientKey}" data-reward-field="offerPrice">
                            </div>
                        </label>
                        <label>每次資格可買
                            <div class="input-group input-group-sm">
                                <input class="form-control" type="number" min="1" max="${selectionLimit}" value="${item.maxQuantityPerOrder}"
                                    data-reward-key="${item.clientKey}" data-reward-field="maxQuantityPerOrder">
                                <span class="input-group-text">件</span>
                            </div>
                        </label>
                    </div>
                </div>`;
            }).join("");

            $("#RewardProductRows").html(html);
            $("#RewardProductEmpty").toggleClass("d-none", this.rewardItems.length > 0);
            this.refreshBatchPriceBaseOptions();
            this.refreshNumberFormats("#RewardProductRows");
            this.renderOfferPreview();
        },

        syncRewardItemLimitInputs: function () {
            const selectionLimit = Math.max(parseNumber($("#InputSelectionQuantity").val()), 1);
            const self = this;
            let hasReported = false;
            $("#RewardProductRows [data-reward-field='maxQuantityPerOrder']")
                .attr("max", selectionLimit)
                .each(function () {
                    const isValid = self.validateRewardItemLimitInput(this, !hasReported);
                    if (!isValid) hasReported = true;
                });
        },

        validateRewardItemLimitInput: function (input, shouldReport) {
            const selectionLimit = Math.max(parseNumber($("#InputSelectionQuantity").val()), 1);
            const value = parseNumber($(input).val());
            const isInvalid = value > selectionLimit;
            const message = isInvalid
                ? `每次資格可買不可超過每次資格可選件數（目前為 ${selectionLimit} 件）。`
                : "";

            input.setCustomValidity(message);
            $(input).toggleClass("is-invalid", isInvalid);
            if (isInvalid && shouldReport && typeof input.reportValidity === "function") {
                input.reportValidity();
            }
            return !isInvalid;
        },

        refreshBatchPriceBaseOptions: function () {
            const $select = $("#RewardBatchPriceMode");
            if (!$select.length) return;

            const current = String($select.val() || "fixed");
            const roleNames = new Map();
            let originalRoleName = "";
            this.rewardItems.forEach(function (item) {
                (item.prices || []).forEach(function (price) {
                    const roleId = Number(price.roleId || 0);
                    if (Number(price.bonus || 0) !== 0) return;
                    if (roleId <= 1) {
                        if (!originalRoleName) originalRoleName = price.roleName || "非會員";
                        return;
                    }
                    roleNames.set(roleId, price.roleName || `角色 ${roleId} 價`);
                });
            });

            const options = [
                '<option value="fixed">全部固定金額</option>',
                `<option value="original">${escapeHtml(originalRoleName || "原價")}（原價）打折</option>`
            ];
            Array.from(roleNames.entries())
                .sort(function (a, b) { return String(a[1]).localeCompare(String(b[1]), "zh-Hant"); })
                .forEach(function (entry) {
                    options.push(`<option value="role:${entry[0]}">${escapeHtml(entry[1])}打折</option>`);
                });
            options.push('<option value="suggested">建議售價打折</option>');

            $select.html(options.join(""));
            $select.val($select.find(`option[value="${current}"]`).length ? current : "fixed");
            $("#RewardBatchPricePanel").toggleClass("d-none", this.rewardItems.length === 0);
            this.updateBatchPriceUI();
        },

        updateBatchPriceUI: function () {
            const isFixed = $("#RewardBatchPriceMode").val() === "fixed";
            const $value = $("#RewardBatchPriceValue");
            $("#RewardBatchPriceValueLabel").text(isFixed ? "活動價" : "折扣折數");
            $("#RewardBatchPriceUnit").text(isFixed ? "NT$" : "折");
            $value.attr({
                min: isFixed ? "0" : "1",
                step: isFixed ? "1" : "1",
                placeholder: isFixed ? "請輸入金額" : "例如 9 或 90（九折）"
            });
            if (isFixed) $value.removeAttr("max");
            else $value.attr("max", "99");
            $("#RewardBatchPriceFeedback").text("");
        },

        getBatchPriceBase: function (item, mode) {
            if (mode === "original") {
                const originalPrice = (item.prices || []).find(function (candidate) {
                    const roleId = Number(candidate.roleId || 0);
                    return roleId <= 1 && Number(candidate.bonus || 0) === 0;
                });
                return originalPrice ? Number(originalPrice.price || 0) : Number(item.originalPrice || 0);
            }
            if (mode === "suggested") return Number(item.suggestPrice || 0);
            if (mode.indexOf("role:") === 0) {
                const roleId = Number(mode.split(":")[1] || 0);
                const price = (item.prices || []).find(function (candidate) {
                    return Number(candidate.roleId || 0) === roleId && Number(candidate.bonus || 0) === 0;
                });
                return price ? Number(price.price || 0) : 0;
            }
            return 0;
        },

        applyBatchOfferPrices: function () {
            if (!this.rewardItems.length) {
                Coker.sweet.error("提醒", "請先加入要設定的優惠商品。", null, true);
                return;
            }

            const mode = String($("#RewardBatchPriceMode").val() || "fixed");
            const rawValue = String($("#RewardBatchPriceValue").val() || "").trim();
            const value = Number(rawValue);
            const discountPercent = mode === "fixed" ? null : this.normalizeDiscountPercentForSave(value);
            if (!rawValue || !Number.isFinite(value) || value < 0 ||
                (mode !== "fixed" && (!Number.isInteger(value) || discountPercent === null))) {
                Coker.sweet.error("批次價格格式不正確", mode === "fixed"
                    ? "請輸入大於或等於 0 的金額。"
                    : "請輸入 1 到 99 的整數，例如 9 或 90 表示九折，85 表示八五折。", null, true);
                return;
            }

            let applied = 0;
            let skipped = 0;
            this.rewardItems.forEach(item => {
                let offerPrice = value;
                if (mode !== "fixed") {
                    const basePrice = this.getBatchPriceBase(item, mode);
                    if (!(basePrice > 0)) {
                        skipped += 1;
                        return;
                    }
                    offerPrice = Math.round(basePrice * discountPercent / 100);
                }
                item.offerPrice = Math.max(0, offerPrice);
                item.offerPriceCustomized = true;
                applied += 1;
            });

            if (!applied) {
                Coker.sweet.error("無法套用批次價格", "所有商品都沒有選定的價格基準。", null, true);
                return;
            }

            this.renderRewardRows();
            $("#RewardBatchPriceFeedback").text(`已套用 ${applied} 項${skipped ? `，略過 ${skipped} 項無此價格基準的商品` : ""}。`);
        },

        renderPriceReferences: function (item) {
            const prices = (item.prices || []).slice();
            let originalIndex = prices.findIndex(x => x.bonus === 0 && (x.roleId === 0 || x.roleId === 1));
            if (originalIndex < 0) originalIndex = prices.findIndex(x => x.bonus === 0);

            const orderedPrices = originalIndex > 0
                ? [prices[originalIndex]].concat(prices.filter((_, index) => index !== originalIndex))
                : prices;

            const priceRows = orderedPrices.map(function (price, index) {
                const roleName = price.roleName || (price.roleId === 0 || price.roleId === 1 ? "非會員" : `角色 ${price.roleId}`);
                const isOriginal = originalIndex >= 0 && index === 0;
                let valueText = "";
                if (price.price > 0) valueText += `NT$ ${formatMoney(price.price)}`;
                if (price.bonus > 0) valueText += `${valueText ? " ＋ " : ""}紅利 ${formatMoney(price.bonus)}`;
                if (!valueText) valueText = "NT$ 0";

                return `<div class="marketing-price-reference-row${isOriginal ? " is-original" : ""}">
                    <span>${escapeHtml(roleName)}${isOriginal ? "（原價）" : ""}</span>
                    <strong>${escapeHtml(valueText)}</strong>
                </div>`;
            });

            if (!priceRows.length) {
                priceRows.push(`<div class="marketing-price-reference-row is-original">
                    <span>原價</span><strong>NT$ ${formatMoney(item.originalPrice)}</strong>
                </div>`);
            }

            if (Number(item.suggestPrice) > 0) {
                priceRows.push(`<div class="marketing-price-reference-row is-suggested">
                    <span>建議售價</span><strong>NT$ ${formatMoney(item.suggestPrice)}</strong>
                </div>`);
            }

            return `<div class="marketing-price-reference">
                <div class="marketing-price-reference-title">價格參考</div>
                ${priceRows.join("")}
            </div>`;
        },

        renderProductMeta: function (item) {
            const stateText = item.available === false
                ? "下架"
                : item.visible === false
                    ? "隱藏"
                    : (item.productStatusName || "一般");
            const stockState = this.getStockState(item);
            const unavailable = item.available === false || item.visible === false ||
                Number(item.productStatus) === 2 || (!item.noStockManagement && Number(item.stockQuantity || 0) <= 0);

            return `<div class="marketing-product-meta">
                <span class="marketing-status-badge${unavailable ? " is-unavailable" : ""}">${escapeHtml(stateText)}</span>
                <span class="marketing-stock-badge ${stockState.className}" title="${escapeHtml(stockState.title)}">${escapeHtml(stockState.text)}</span>
            </div>`;
        },

        getStockState: function (item) {
            if (item.noStockManagement) {
                return { text: "不限庫存", className: "is-unlimited", title: "此商品不進行庫存控管" };
            }

            const stock = Number(item.stockQuantity || 0);
            const alert = Number(item.alertQuantity || 0);
            const countText = stock.toLocaleString("zh-TW");
            const alertText = alert > 0 ? `警示量 ${alert.toLocaleString("zh-TW")}` : "未設定警示量";

            if (Number(item.productStatus) === 2 || stock <= 0) {
                return { text: `售完 ${countText}`, className: "is-empty", title: alertText };
            }
            if (alert <= 0) {
                return { text: `庫存 ${countText}`, className: "is-normal", title: alertText };
            }

            const ratio = stock / alert;
            if (stock <= alert) {
                return { text: `庫存告急 ${countText}`, className: "is-critical", title: `${alertText}；已達警示範圍` };
            }
            if (ratio < 3) {
                return { text: `低庫存 ${countText}`, className: "is-low", title: `${alertText}；目前為警示量 ${ratio.toFixed(1)} 倍` };
            }
            if (ratio < 10) {
                return { text: `庫存正常 ${countText}`, className: "is-normal", title: `${alertText}；目前為警示量 ${ratio.toFixed(1)} 倍` };
            }
            return { text: `庫存充足 ${countText}`, className: "is-plenty", title: `${alertText}；目前為警示量 ${ratio.toFixed(1)} 倍` };
        },

        renderOfferPreview: function () {
            const $preview = $("#MarketingOfferPreview");
            const $summary = $("#MarketingActivitySummary");

            if (this.offerPreviewSwiper) {
                this.offerPreviewSwiper.destroy(true, true);
                this.offerPreviewSwiper = null;
            }

            if (!this.isAddOnMode()) {
                $preview.empty().addClass("d-none");
                $summary.addClass("d-none");
                return;
            }

            const selection = parseNumber($("#InputSelectionQuantity").val()) || 1;
            const mode = this.getRuleMode();
            const conditionQuantity = parseNumber(this.$addOnMinQuantity.val());
            const conditionAmount = parseNumber(this.$addOnMinAmount.val());
            const scopeSummary = summarizeProductNames(this.scopeItems, "targetName", "尚未選擇指定商品");
            const rewardSummary = summarizeProductNames(this.rewardItems, "productName", "右側優惠商品");
            let conditionText = "";

            $("#SelectionQuantityHelp").text("每取得一次資格，可選擇的優惠商品總件數。");

            if (mode === "product-addon") {
                const quantity = conditionQuantity || 1;
                conditionText = `${scopeSummary}任選合計 ${quantity} 件，即取得一次資格；每次可從 ${rewardSummary} 中選擇 ${selection} 件。`;
            }
            if (mode === "order-amount-addon") conditionText = `整筆訂單滿 NT$ ${formatMoney(conditionAmount)}，即取得一次資格；每次可從 ${rewardSummary} 中選擇 ${selection} 件。`;
            if (mode === "scope-amount-addon") conditionText = `${scopeSummary}的有效小計滿 NT$ ${formatMoney(conditionAmount)}，即取得一次資格；每次可從 ${rewardSummary} 中選擇 ${selection} 件。`;

            const repeatText = this.$repeatable.prop("checked") ? "購買達倍數時可重複取得資格" : "每筆訂單僅取得一次資格";
            const hasCompleteCondition = mode === "product-addon"
                ? this.scopeItems.length > 0 && conditionQuantity > 0
                : mode === "scope-amount-addon"
                    ? this.scopeItems.length > 0 && conditionAmount > 0
                    : conditionAmount > 0;
            const canShowSummary = hasCompleteCondition && this.rewardItems.length > 0;
            $("#MarketingActivitySummaryText").text(canShowSummary ? `${conditionText} ${repeatText}。` : "");
            $summary.toggleClass("d-none", !canShowSummary);

            const previewItems = this.rewardItems.map(function (item) {
                const isGift = Number(item.offerPrice) === 0;
                const priceHtml = isGift
                    ? '<span class="marketing-storefront-price is-gift">免費贈送</span>'
                    : `<span class="marketing-storefront-price">NT$ ${formatMoney(item.offerPrice)}</span>`;
                const originalPriceHtml = !isGift && Number(item.originalPrice) > Number(item.offerPrice)
                    ? `<del>原價 NT$ ${formatMoney(item.originalPrice)}</del>` : "";

                return `<div class="swiper-slide">
                  <div class="marketing-storefront-item">
                    <div class="marketing-storefront-image">
                        <img src="${escapeHtml(item.imageUrl || "/images/noImg.jpg")}" alt="">
                        <span>${isGift ? "贈品" : "加價購"}</span>
                    </div>
                    <div class="marketing-storefront-item-copy">
                        <strong>${escapeHtml(item.productName)}</strong>
                        ${item.stockName ? `<small>${escapeHtml(item.stockName)}</small>` : ""}
                        <div>${priceHtml}${originalPriceHtml}</div>
                    </div>
                    <div class="marketing-storefront-select"><span class="material-symbols-outlined" aria-hidden="true">radio_button_unchecked</span>選擇</div>
                  </div>
                </div>`;
            }).join("");

            const rewardContent = previewItems
                ? `<div class="swiper marketing-storefront-swiper">
                    <div class="swiper-wrapper">${previewItems}</div>
                    <div class="swiper-button-prev"></div>
                    <div class="swiper-button-next"></div>
                   </div>`
                : '<div class="marketing-storefront-empty">新增優惠商品後，這裡會模擬顧客可選擇的商品畫面。</div>';

            $preview.removeClass("d-none").html(`<div class="marketing-preview-label">前台效果預覽（示意）</div>
                <div class="marketing-storefront-preview">
                    <div class="marketing-storefront-heading">
                        <span class="marketing-preview-tag">優惠選購</span>
                        <div>
                            <strong>您已符合優惠資格</strong>
                            <small>每取得一次資格，可選 ${selection} 件優惠商品</small>
                        </div>
                    </div>
                    ${rewardContent}
                </div>`);

            const swiperElement = $preview.find(".marketing-storefront-swiper")[0];
            if (swiperElement && window.Swiper) {
                this.offerPreviewSwiper = new window.Swiper(swiperElement, {
                    slidesPerView: 1.2,
                    spaceBetween: 12,
                    watchOverflow: true,
                    navigation: {
                        prevEl: swiperElement.querySelector(".swiper-button-prev"),
                        nextEl: swiperElement.querySelector(".swiper-button-next")
                    },
                    breakpoints: {
                        576: { slidesPerView: 2.2 },
                        992: { slidesPerView: 3.2 },
                        1400: { slidesPerView: 4.2 }
                    }
                });
            }
        },

        syncModalStates: function () {
            if (!this.productModalReady || !window.ProdListModalApi) return;

            window.ProdListModalApi.setActiveTarget("#ScopeProductPicker");
            window.ProdListModalApi.setData(this.scopeItems.map(x => ({
                FK_ProdId: x.targetId,
                Prod_Name: x.targetName,
                MinsizeImage: x.imageUrl,
                ProductStatus: x.productStatus,
                ProductStatusName: x.productStatusName,
                Visible: x.visible,
                Available: x.available,
                NoStockManagement: x.noStockManagement,
                StockQuantity: x.stockQuantity
            })));

            window.ProdListModalApi.setActiveTarget("#RewardProductPicker");
            const rewardProducts = Array.from(new Map(this.rewardItems.map(x => [x.productId, x])).values());
            window.ProdListModalApi.setData(rewardProducts.map(x => ({
                FK_ProdId: x.productId,
                Prod_Name: x.productName,
                MinsizeImage: x.imageUrl,
                ProductStatus: x.productStatus,
                ProductStatusName: x.productStatusName,
                Visible: x.visible,
                Available: x.available,
                NoStockManagement: x.noStockManagement,
                StockQuantity: x.stockQuantity
            })));
        },

        loadOptions: function () {
            const self = this;
            return co.Marketing.GetOptions().then(function (res) {
                const data = res.object || res.Object || res;
                self.options = self.normalizeOptions(data || {});
                self.renderStatusOptions();
                self.applyGridLookups();
            }).catch(function (err) {
                self.handleRequestError(err, "讀取行銷活動選項失敗");
            });
        },

        normalizeOptions: function (options) {
            return {
                campaignTypes: options.campaignTypes || options.CampaignTypes || [],
                ruleTypes: options.ruleTypes || options.RuleTypes || [],
                displayStatuses: options.displayStatuses || options.DisplayStatuses || [],
                editableStatuses: options.editableStatuses || options.EditableStatuses || []
            };
        },

        normalizeLookupItems: function (items) {
            return (items || []).map(function (item) {
                return { value: item.value ?? item.Value, text: item.text ?? item.Text };
            });
        },

        renderStatusOptions: function () {
            const selectedValue = this.$status.val() ?? "0";
            this.$status.empty();
            const self = this;
            (this.options?.editableStatuses || []).forEach(function (item) {
                $("<option>").attr("value", item.value ?? item.Value).text(item.text ?? item.Text).appendTo(self.$status);
            });
            this.$status.val(selectedValue);
            if (this.$status.val() == null) this.$status.val("0");
        },

        applyGridLookups: function () {
            if (this.isGridLookupApplied || !this.marketingListGridEvent?.component || !this.options) return;
            const grid = this.marketingListGridEvent.component;
            grid.beginUpdate();
            try {
                grid.columnOption("CampaignType", "lookup", {
                    dataSource: this.normalizeLookupItems(this.options.campaignTypes), valueExpr: "value", displayExpr: "text"
                });
                grid.columnOption("Status", "lookup", {
                    dataSource: this.normalizeLookupItems(this.options.displayStatuses), valueExpr: "value", displayExpr: "text"
                });
                this.isGridLookupApplied = true;
            } finally {
                grid.endUpdate();
            }
        },

        onEnterList: function () {
            this.clearFormState();
        },

        onEnterNew: function () {
            this.keyId = 0;
            this.clearFormState();
            this.setRuleModeLocked(false);
            this.applyNewFormDefaults();
            this.applyNeverEndUI();
            this.applyRuleModeUI();
        },

        applyNewFormDefaults: function () {
            this.$status.val("0");
            $("#InputPriority").val("0");
            $("#RuleAmountDiscount").prop("checked", true);
            $("#InputStartTime").val(this.toDateTimeLocal(new Date()));
            this.$neverEnd.prop("checked", true);
            this.$endTime.val("");
            $("#CheckCanStack").prop("checked", false);
            $("#CheckRepeatable").prop("checked", false);

            this.$minAmount.val("1000");
            this.$discountAmount.val("100");
            this.$discountPercent.val("9");
            this.$maxDiscountAmount.val("");
            this.$addOnMinAmount.val("1000");
            this.$addOnMinQuantity.val("1");
            $("#InputSelectionQuantity").val("1");
            this.refreshNumberFormats();
        },

        onEnterEdit: function (id) {
            const self = this;
            this.setRuleModeLocked(false);
            if (!id) {
                if (this.hashPage) this.hashPage.goList();
                return;
            }

            co.Marketing.Get(id).then(function (res) {
                const data = res.object || res.Object || res;
                if (!data) throw new Error("讀取行銷活動資料失敗");
                if (!self.canUseProdAddition && Number(data.ruleType ?? data.RuleType) === RULE.addOnPurchase) {
                    Coker.sweet.error("沒有使用權限", "此帳號未開通商品加價購／滿額贈服務。", null, true);
                    if (self.hashPage) self.hashPage.goList();
                    return null;
                }
                self.keyId = data.id || data.Id || id;
                return self.fillForm(data);
            }).catch(function (err) {
                self.handleRequestError(err, "讀取行銷活動資料失敗");
                if (self.hashPage) self.hashPage.goList();
            });
        },

        clearFormState: function () {
            this.keyId = 0;
            this.repeatableWasCustomized = false;
            this.canStackWasCustomized = false;
            this.setRuleModeLocked(false);
            this.scopeItems = [];
            this.rewardItems = [];
            this.rewardItemKeySeed = 0;
            _c.Form.clear(this.formId);
            $("#" + this.formId).removeClass("was-validated");
            $("input[name='Id']").val(0);
            this.$discountAmount.val("").removeAttr("required");
            this.$discountPercent.val("").removeAttr("required");
            this.$maxDiscountAmount.val("");
            this.$addOnMinAmount.val("");
            this.$addOnMinQuantity.val("1");
            $("#InputSelectionQuantity").val("1");
            this.renderScopeRows();
            this.renderRewardRows();
            this.applyNeverEndUI();
            this.syncModalStates();
        },

        fillForm: function (result) {
            const self = this;
            const flat = {
                Id: valueOf(result, "id", "Id", 0),
                Name: valueOf(result, "name", "Name", ""),
                Description: valueOf(result, "description", "Description", ""),
                CampaignType: valueOf(result, "campaignType", "CampaignType", 0),
                Status: valueOf(result, "status", "Status", 0),
                StartTime: this.toDateTimeLocal(valueOf(result, "startTime", "StartTime", null)),
                EndTime: this.toDateTimeLocal(valueOf(result, "endTime", "EndTime", null)),
                NeverEnd: valueOf(result, "neverEnd", "NeverEnd", false),
                Priority: valueOf(result, "priority", "Priority", 0),
                CanStack: valueOf(result, "canStack", "CanStack", false),
                Repeatable: valueOf(result, "repeatable", "Repeatable", false),
                MinAmount: valueOf(result, "minAmount", "MinAmount", ""),
                DiscountAmount: valueOf(result, "discountAmount", "DiscountAmount", ""),
                DiscountPercent: this.normalizeDiscountRateForView(valueOf(result, "discountPercent", "DiscountPercent", "")),
                MaxDiscountAmount: valueOf(result, "maxDiscountAmount", "MaxDiscountAmount", "")
            };

            _c.Form.insertData(flat, "#" + this.formId);
            $("input[name='Id']").val(this.keyId);

            const ruleType = Number(valueOf(result, "ruleType", "RuleType", RULE.amountDiscount));
            const conditionType = Number(valueOf(result, "conditionType", "ConditionType", CONDITION.orderAmount));
            $("input[name='RuleMode'][value='" + this.modeFromRule(ruleType, conditionType) + "']").prop("checked", true);
            const campaignStatus = Number(valueOf(result, "status", "Status", 0));
            this.setRuleModeLocked(campaignStatus !== 0);

            this.$addOnMinAmount.val(valueOf(result, "minAmount", "MinAmount", ""));
            this.$addOnMinQuantity.val(valueOf(result, "minQuantity", "MinQuantity", 1));
            $("#InputSelectionQuantity").val(valueOf(result, "selectionQuantityPerQualification", "SelectionQuantityPerQualification", 1));
            this.refreshNumberFormats();

            const scopeRows = valueOf(result, "scopeItems", "ScopeItems", []);
            this.scopeItems = scopeRows.map(function (item) {
                return {
                    id: Number(valueOf(item, "id", "Id", 0)),
                    targetType: Number(valueOf(item, "targetType", "TargetType", TARGET.product)),
                    targetId: Number(valueOf(item, "targetId", "TargetId", 0)),
                    targetName: valueOf(item, "targetName", "TargetName", ""),
                    requiredQuantityPerQualification: Number(valueOf(item, "requiredQuantityPerQualification", "RequiredQuantityPerQualification", 1)),
                    productStatus: Number(valueOf(item, "productStatus", "ProductStatus", 0)),
                    productStatusName: valueOf(item, "productStatusName", "ProductStatusName", "一般"),
                    visible: valueOf(item, "visible", "Visible", true),
                    available: valueOf(item, "available", "Available", true),
                    noStockManagement: valueOf(item, "noStockManagement", "NoStockManagement", false),
                    stockQuantity: valueOf(item, "stockQuantity", "StockQuantity", null),
                    alertQuantity: valueOf(item, "alertQuantity", "AlertQuantity", null),
                    imageUrl: valueOf(item, "imageUrl", "ImageUrl", "/images/noImg.jpg")
                };
            });

            const rewardRows = valueOf(result, "rewardItems", "RewardItems", []);
            this.rewardItems = rewardRows.map(function (item) {
                return {
                    clientKey: self.nextRewardItemKey(),
                    id: Number(valueOf(item, "id", "Id", 0)),
                    productId: Number(valueOf(item, "productId", "ProductId", 0)),
                    productStockId: Number(valueOf(item, "productStockId", "ProductStockId", 0)),
                    productName: valueOf(item, "productName", "ProductName", ""),
                    stockName: valueOf(item, "stockName", "StockName", ""),
                    sku: valueOf(item, "sku", "Sku", ""),
                    originalPrice: Number(valueOf(item, "originalPrice", "OriginalPrice", 0)),
                    suggestPrice: null,
                    prices: [],
                    productStatus: Number(valueOf(item, "productStatus", "ProductStatus", 0)),
                    productStatusName: valueOf(item, "productStatusName", "ProductStatusName", "一般"),
                    visible: valueOf(item, "visible", "Visible", true),
                    available: valueOf(item, "available", "Available", true),
                    noStockManagement: valueOf(item, "noStockManagement", "NoStockManagement", false),
                    stockQuantity: valueOf(item, "stockQuantity", "StockQuantity", null),
                    alertQuantity: valueOf(item, "alertQuantity", "AlertQuantity", null),
                    offerPrice: Number(valueOf(item, "offerPrice", "OfferPrice", 0)),
                    offerPriceCustomized: true,
                    maxQuantityPerOrder: Number(valueOf(item, "maxQuantityPerOrder", "MaxQuantityPerOrder", 1)),
                    enabled: valueOf(item, "enabled", "Enabled", true),
                    sortOrder: Number(valueOf(item, "sortOrder", "SortOrder", 0)),
                    imageUrl: valueOf(item, "imageUrl", "ImageUrl", "/images/noImg.jpg"),
                    stocks: []
                };
            });

            this.applyNeverEndUI();
            this.applyRuleModeUI();
            this.renderScopeRows();
            this.renderRewardRows();
            this.syncModalStates();

            return this.hydrateRewardStocks().then(function () {
                self.renderRewardRows();
            });
        },

        getRuleMode: function () {
            return this.$ruleMode.filter(":checked").val() || "amount-discount";
        },

        isAddOnMode: function () {
            return ["product-addon", "order-amount-addon", "scope-amount-addon"].includes(this.getRuleMode());
        },

        modeFromRule: function (ruleType, conditionType) {
            if (ruleType === RULE.percentDiscount) return "percent-discount";
            if (ruleType !== RULE.addOnPurchase) return "amount-discount";
            if (conditionType === CONDITION.scopeQuantity || conditionType === CONDITION.buySpecificProduct) return "product-addon";
            if (conditionType === CONDITION.scopeAmount) return "scope-amount-addon";
            return "order-amount-addon";
        },

        setRuleModeLocked: function (locked) {
            this.isRuleModeLocked = locked === true;
            this.$ruleMode.prop("disabled", this.isRuleModeLocked);
            $("#RuleTypeLockHint").toggleClass("d-none", !this.isRuleModeLocked);
            this.applyRuleTypeVisualState();
        },

        applyRuleTypeVisualState: function () {
            const selectedMode = this.getRuleMode();
            const hasSelection = this.$ruleMode.filter(":checked").length > 0;

            $(".marketing-rule-card").each((_, element) => {
                const $card = $(element);
                const isSelected = $card.find("input[name='RuleMode']").val() === selectedMode;
                $card
                    .toggleClass("is-rule-selected", hasSelection && isSelected)
                    .toggleClass("is-rule-muted", hasSelection && !isSelected && !this.isRuleModeLocked)
                    .toggleClass("is-rule-locked", this.isRuleModeLocked && isSelected)
                    .toggleClass("d-none", this.isRuleModeLocked && !isSelected);
            });
        },

        applyRuleModeUI: function () {
            const mode = this.getRuleMode();
            const isAddOn = this.isAddOnMode();
            const needsScope = mode === "product-addon" || mode === "scope-amount-addon";
            const needsAmount = mode === "order-amount-addon" || mode === "scope-amount-addon";
            const needsQuantity = mode === "product-addon";

            this.$discountSettings.toggleClass("d-none", isAddOn);
            this.$addOnSettings.toggleClass("d-none", !isAddOn);
            this.$scopeProductSection.toggleClass("d-none", !needsScope);
            this.$addOnAmountSection.toggleClass("d-none", !needsAmount);
            this.$addOnQuantitySection.toggleClass("d-none", !needsQuantity);
            $(".marketing-option-grid").toggleClass("is-addon", isAddOn);
            $("#CanStackSection").toggleClass("d-none", isAddOn);
            if (isAddOn) $("#CheckCanStack").prop("checked", true);
            $("#ScopeAmountExplanation").toggleClass("d-none", mode !== "scope-amount-addon");
            this.$rewardAmountSection.toggleClass("d-none", mode !== "amount-discount");
            this.$rewardPercentSection.toggleClass("d-none", mode !== "percent-discount");

            this.$minAmount.prop("required", !isAddOn);
            this.$discountAmount.prop("required", mode === "amount-discount");
            this.$discountPercent.prop("required", mode === "percent-discount");
            this.$addOnMinAmount.prop("required", needsAmount);
            this.$addOnMinQuantity.prop("required", needsQuantity);

            this.$repeatableSection.toggleClass("d-none", mode === "percent-discount");
            if (mode === "percent-discount") this.$repeatable.prop("checked", false);

            if (mode === "product-addon") {
                $("#RepeatableLabel").text("依購買數量重複取得資格");
                $("#RepeatableHelp").text("例如每買 2 件取得一次資格，購買 4 件是否取得兩次資格。");
            } else if (isAddOn) {
                $("#RepeatableLabel").text("依滿額倍數重複取得資格");
                $("#RepeatableHelp").text("例如滿 3,000 取得一次資格，消費 6,000 是否取得兩次資格。");
            } else {
                $("#RepeatableLabel").text("允許滿額折抵可累計");
                $("#RepeatableHelp").text("例如滿 1000 折 100，消費 2000 是否折 200。");
            }

            if (mode === "product-addon") {
                $("#AddOnConditionTitle").text("指定商品任選滿件條件");
            } else if (mode === "scope-amount-addon") {
                $("#AddOnConditionTitle").text("指定商品金額條件");
            } else if (mode === "order-amount-addon") {
                $("#AddOnConditionTitle").text("訂單金額條件");
            }

            $("#AddOnAmountLabel").text(mode === "scope-amount-addon"
                ? "指定商品合計門檻"
                : "訂單滿額門檻");

            $("#AddOnAmountHelp").text(mode === "scope-amount-addon"
                ? "達到此金額後，客戶即可選擇右側優惠商品。"
                : "以整筆訂單有效商品金額判斷，不含運費、贈品及加價購商品。");

            this.renderScopeRows();
            this.renderOfferPreview();
            this.applyRuleTypeVisualState();
        },

        applyNeverEndUI: function () {
            if (this.$neverEnd.prop("checked")) {
                this.$endTime.val("").attr("disabled", "disabled").removeAttr("required");
                this.$endTimeSection.addClass("is-disabled");
                return;
            }
            this.$endTime.removeAttr("disabled");
            this.$endTimeSection.removeClass("is-disabled");
        },

        buildPayload: function () {
            const form = _c.Form.getJson(this.formId);
            const mode = this.getRuleMode();
            const isAddOn = this.isAddOnMode();
            const neverEnd = form.NeverEnd === true;
            let ruleType = RULE.amountDiscount;
            let conditionType = CONDITION.orderAmount;

            if (mode === "percent-discount") ruleType = RULE.percentDiscount;
            if (isAddOn) ruleType = RULE.addOnPurchase;
            if (mode === "product-addon") conditionType = CONDITION.scopeQuantity;
            if (mode === "scope-amount-addon") conditionType = CONDITION.scopeAmount;

            return {
                Id: this.keyId,
                Name: form.Name,
                Description: form.Description || null,
                CampaignType: isAddOn ? 30 : 0,
                Status: Number(form.Status || 0),
                StartTime: form.StartTime || null,
                EndTime: neverEnd ? null : (form.EndTime || null),
                NeverEnd: neverEnd,
                Priority: Number(form.Priority || 0),
                CanStack: isAddOn || form.CanStack === true,
                Repeatable: mode !== "percent-discount" && form.Repeatable === true,
                StackingConflictConfirmed: false,
                RuleType: ruleType,
                ConditionType: conditionType,
                ScopeType: conditionType === CONDITION.orderAmount ? SCOPE.allOrder : SCOPE.specificProducts,
                MinAmount: isAddOn
                    ? (conditionType === CONDITION.orderAmount || conditionType === CONDITION.scopeAmount
                        ? parseNumber(this.$addOnMinAmount.val()) : null)
                    : parseNumber(form.MinAmount),
                MinQuantity: conditionType === CONDITION.scopeQuantity
                    ? parseNumber(this.$addOnMinQuantity.val()) : null,
                DiscountAmount: ruleType === RULE.amountDiscount ? Number(form.DiscountAmount || 0) : null,
                DiscountPercent: ruleType === RULE.percentDiscount ? this.normalizeDiscountPercentForSave(form.DiscountPercent) : null,
                MaxDiscountAmount: ruleType === RULE.percentDiscount && form.MaxDiscountAmount !== "" && form.MaxDiscountAmount != null
                    ? Number(form.MaxDiscountAmount || 0) : null,
                SelectionQuantityPerQualification: isAddOn ? parseNumber($("#InputSelectionQuantity").val()) : 1,
                MaxSelectionQuantityPerOrder: null,
                ScopeItems: isAddOn && conditionType !== CONDITION.orderAmount
                    ? this.scopeItems.map(x => ({
                        Id: x.id || 0,
                        TargetType: TARGET.product,
                        TargetId: x.targetId,
                        TargetName: x.targetName,
                        RequiredQuantityPerQualification: 1
                    })) : [],
                RewardItems: isAddOn ? this.rewardItems.map(function (x, index) {
                    return {
                        Id: x.id || 0,
                        ProductId: x.productId,
                        ProductStockId: x.productStockId,
                        ProductName: x.productName,
                        StockName: x.stockName,
                        Sku: x.sku,
                        OriginalPrice: x.originalPrice,
                        OfferPrice: Number(x.offerPrice),
                        MaxQuantityPerOrder: Number(x.maxQuantityPerOrder),
                        Enabled: true,
                        SortOrder: index
                    };
                }) : []
            };
        },

        validatePayload: function (payload) {
            if (!payload.NeverEnd && payload.EndTime && payload.StartTime &&
                new Date(payload.EndTime) <= new Date(payload.StartTime)) {
                return this.showValidationError("結束時間必須晚於開始時間。");
            }

            if (payload.RuleType === RULE.amountDiscount || payload.RuleType === RULE.percentDiscount) {
                if (!payload.MinAmount || payload.MinAmount <= 0) return this.showValidationError("滿額門檻必須大於 0。");
            }

            if (payload.RuleType === RULE.amountDiscount) {
                if (!payload.DiscountAmount || payload.DiscountAmount <= 0) return this.showValidationError("折抵金額必須大於 0。");
                if (payload.DiscountAmount > payload.MinAmount) return this.showValidationError("折抵金額不可大於滿額門檻。");
            }

            if (payload.RuleType === RULE.percentDiscount) {
                const inputRate = Number(this.$discountPercent.val());
                if (!Number.isInteger(inputRate) || inputRate < 1 || inputRate > 99) {
                    return this.showValidationError("折扣折數請輸入 1 到 99 的整數，例如 9 或 90 表示九折，85 表示八五折。");
                }
            }

            if (payload.RuleType === RULE.addOnPurchase) {
                if ((payload.ConditionType === CONDITION.orderAmount || payload.ConditionType === CONDITION.scopeAmount) &&
                    (!payload.MinAmount || payload.MinAmount <= 0)) {
                    return this.showValidationError("滿額門檻必須大於 0。");
                }
                if (payload.ConditionType === CONDITION.scopeQuantity &&
                    (!payload.MinQuantity || payload.MinQuantity <= 0)) {
                    return this.showValidationError("指定商品合計購買件數必須大於 0。");
                }
                if (payload.ConditionType !== CONDITION.orderAmount && !payload.ScopeItems.length) {
                    return this.showValidationError("請至少選擇一項指定商品。");
                }
                if (!payload.RewardItems.length) return this.showValidationError("請至少設定一項加價購或贈品商品。");
                if (payload.SelectionQuantityPerQualification <= 0) return this.showValidationError("每次資格可選件數必須大於 0。");
                if (payload.RewardItems.some(x => x.ProductStockId <= 0 || x.OfferPrice < 0 || x.MaxQuantityPerOrder <= 0)) {
                    return this.showValidationError("請確認每項優惠商品的規格、活動價及每次資格可買件數。");
                }
                if (payload.RewardItems.some(x => x.MaxQuantityPerOrder > payload.SelectionQuantityPerQualification)) {
                    return this.showValidationError("優惠商品的「每次資格可買」不可大於活動的「每次資格可選件數」。");
                }
                if (new Set(payload.RewardItems.map(x => x.ProductStockId)).size !== payload.RewardItems.length) {
                    return this.showValidationError("相同商品規格不可重複設定。");
                }
            }

            return true;
        },

        showValidationError: function (message) {
            Coker.sweet.error("錯誤", message, null, true);
            return false;
        },

        submitForm: function () {
            const payload = this.buildPayload();
            if (!this.validatePayload(payload)) return Promise.reject();
            const self = this;

            return this.savePayload(payload).catch(function (err) {
                self.handleRequestError(err, "儲存行銷活動發生錯誤");
            });
        },

        savePayload: function (payload) {
            const self = this;
            return co.Marketing.AddUp(payload).then(function (res) {
                if (res && (res.success === false || res.Success === false)) {
                    const errorCode = res.error || res.Error;
                    if (errorCode === "MarketingStackingConfirmationRequired") {
                        return self.confirmStackingConflict(payload, res);
                    }
                    throw new Error(errorCode || res.message || res.Message || "儲存失敗");
                }
                Coker.sweet.success((res && (res.message || res.Message)) || "行銷活動儲存成功", null, true);
                setTimeout(function () {
                    if (self.hashPage) self.hashPage.goList();
                    self.reloadGrid();
                }, 300);
            }).catch(function (err) {
                const result = err?.result || err?.responseJSON || {};
                const errorCode = result.error || result.Error;
                if (errorCode === "MarketingStackingConfirmationRequired") {
                    return self.confirmStackingConflict(payload, result);
                }
                throw err;
            });
        },

        confirmStackingConflict: function (payload, result) {
            const self = this;
            const conflicts = valueOf(result, "object", "Object", []);
            const names = conflicts.map(function (item) {
                return `<li>${escapeHtml(valueOf(item, "name", "Name", "未命名活動"))}</li>`;
            }).join("");
            const message =
                "目前有下列期間重疊的滿額優惠也允許併用：" +
                `<ul class="text-start mt-3 mb-3">${names}</ul>` +
                "這些活動啟用後，同一張訂單可能同時套用多組折扣。請確認這是預期的設定。";

            return new Promise(function (resolve, reject) {
                Coker.sweet.confirm(
                    "確認重複抵扣設定",
                    message,
                    "確認並儲存",
                    "返回調整",
                    function () {
                        payload.StackingConflictConfirmed = true;
                        self.savePayload(payload).then(resolve).catch(reject);
                    },
                    function () { resolve(false); }
                );
            });
        },

        toDateTimeLocal: function (value) {
            if (!value) return "";
            const date = new Date(value);
            if (isNaN(date.getTime())) return "";
            const pad = n => String(n).padStart(2, "0");
            return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
        },

        normalizeDiscountRateForView: function (value) {
            if (value === null || value === undefined || value === "") return "";
            const percent = Math.floor(Number(value));
            if (!Number.isFinite(percent) || percent <= 0) return "";
            return percent >= 10 && percent <= 90 && percent % 10 === 0 ? percent / 10 : percent;
        },

        normalizeDiscountPercentForSave: function (value) {
            const rate = Math.floor(Number(value));
            if (!Number.isFinite(rate)) return null;
            if (rate >= 1 && rate <= 9) return rate * 10;
            if (rate >= 10 && rate <= 99) return rate;
            return null;
        },

        handleRequestError: function (err, defaultMessage) {
            const result = err?.result || {};
            const message = result.message || result.Message || err?.message || defaultMessage || "操作失敗";
            Coker.sweet.error("錯誤", message, null, true);
        },

        reloadGrid: function () {
            const grid = this.marketingListGridEvent?.component;
            if (!grid) return;
            const dataSource = grid.getDataSource();
            if (dataSource) dataSource.reload();
            else grid.refresh();
        },

        onGridContentReady: function (e) {
            this.marketingListGridEvent = e;
            this.applyGridLookups();
        },

        onEditClick: function (e) {
            if (!this.canUseProdAddition && Number(e.row?.data?.ruleType ?? e.row?.data?.RuleType) === RULE.addOnPurchase) {
                Coker.sweet.error("沒有使用權限", "此帳號未開通商品加價購／滿額贈服務。", null, true);
                return;
            }
            if (this.hashPage) this.hashPage.goId(e.row.key);
        },

        onDeleteClick: function (e) {
            if (!this.canUseProdAddition && Number(e.row?.data?.ruleType ?? e.row?.data?.RuleType) === RULE.addOnPurchase) {
                Coker.sweet.error("沒有使用權限", "此帳號未開通商品加價購／滿額贈服務。", null, true);
                return;
            }
            const self = this;
            Coker.sweet.confirm("刪除行銷活動", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Marketing.Delete(e.row.key).then(function (res) {
                    Coker.sweet.success((res && (res.message || res.Message)) || "刪除成功", null, true);
                    self.reloadGrid();
                }).catch(function (err) {
                    self.handleRequestError(err, "刪除行銷活動失敗");
                });
            });
        }
    };

    function formatMarketingDiscountPercent(value) {
        const percent = Number(value || 0);
        if (!percent) return "";
        return percent % 10 === 0 ? (percent / 10).toString() : percent.toString();
    }

    function formatMarketingDateTime(value) {
        if (!value) return "";
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";
        const pad = number => String(number).padStart(2, "0");
        return `${date.getFullYear()}/${pad(date.getMonth() + 1)}/${pad(date.getDate())} ` +
            `${pad(date.getHours())}:${pad(date.getMinutes())}`;
    }

    window.MarketingSettingsPageReady = function () { MarketingPage.init(); };
    window.PageReady = window.MarketingSettingsPageReady;
    window.marketingContentReady = function (e) { MarketingPage.onGridContentReady(e); };
    window.marketingEditButtonClicked = function (e) { MarketingPage.onEditClick(e); };
    window.marketingDeleteButtonClicked = function (e) { MarketingPage.onDeleteClick(e); };

    window.marketingActivityTimeCellValue = function (rowData) {
        if (!rowData) return "";
        const start = formatMarketingDateTime(rowData.startTime ?? rowData.StartTime);
        const end = formatMarketingDateTime(rowData.endTime ?? rowData.EndTime);
        const neverEnd = (rowData.neverEnd ?? rowData.NeverEnd) === true;
        if (!start) return "未設定活動時間";
        return `${start} ～ ${neverEnd ? "永久" : (end || "未設定結束時間")}`;
    };

    window.marketingRepeatableCellValue = function (rowData) {
        if (!rowData) return "";
        return (rowData.repeatable ?? rowData.Repeatable) === true ? "是" : "否";
    };

    window.marketingCanStackCellValue = function (rowData) {
        if (!rowData) return "";
        const ruleType = Number(rowData.ruleType ?? rowData.RuleType);
        if (ruleType === RULE.addOnPurchase) return "—";
        return (rowData.canStack ?? rowData.CanStack) === true ? "是" : "否";
    };

    window.marketingRuleSummaryCellValue = function (rowData) {
        if (!rowData) return "";
        const ruleType = Number(rowData.ruleType ?? rowData.RuleType);
        const conditionType = Number(rowData.conditionType ?? rowData.ConditionType);
        const minAmount = rowData.minAmount ?? rowData.MinAmount;
        const minQuantity = Number(rowData.minQuantity ?? rowData.MinQuantity ?? 0);
        const discountAmount = rowData.discountAmount ?? rowData.DiscountAmount;
        const discountPercent = rowData.discountPercent ?? rowData.DiscountPercent;
        const rewardItemCount = Number(rowData.rewardItemCount ?? rowData.RewardItemCount ?? 0);
        const minOfferPrice = Number(rowData.minOfferPrice ?? rowData.MinOfferPrice ?? 0);
        const maxOfferPrice = Number(rowData.maxOfferPrice ?? rowData.MaxOfferPrice ?? 0);
        const amountText = Number(minAmount || 0).toLocaleString();

        if (ruleType === RULE.amountDiscount) return `滿 ${amountText} 折 ${Number(discountAmount || 0).toLocaleString()}`;
        if (ruleType === RULE.percentDiscount) return `滿 ${amountText} 打 ${formatMarketingDiscountPercent(discountPercent)} 折`;
        if (ruleType === RULE.addOnPurchase) {
            let condition = "訂單滿額加價購／贈品";
            if (conditionType === CONDITION.scopeQuantity) condition = `指定商品任選滿 ${minQuantity} 件加價購／贈品`;
            if (conditionType === CONDITION.buySpecificProduct) condition = "指定商品加價購／贈品";
            if (conditionType === CONDITION.scopeAmount) condition = "指定商品滿額加價購／贈品";
            const price = minOfferPrice === 0 && maxOfferPrice === 0
                ? "贈品"
                : minOfferPrice === maxOfferPrice
                    ? `活動價 $${formatMoney(minOfferPrice)}`
                    : `$${formatMoney(minOfferPrice)}～$${formatMoney(maxOfferPrice)}`;
            return `${condition}｜${rewardItemCount} 項優惠商品｜${price}`;
        }
        return "尚未設定";
    };

})(window, window.jQuery);
