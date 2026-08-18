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
        createCartPayload, runBuyGuard, submitCart, ProductSelectionEngine, ProductMediaViewer,
        ProductContentController
    } = I;

    function createProductContent(options) {
        const controller = new ProductContentController(options);
        controller.init();
        return controller;
    }

    window.ProductContentModule = {
        create: createProductContent,
        registerLayout,
        ProductContentController,
        ProductSelectionEngine,
        ProductMediaViewer,
        DEFAULT_TEXTS,
        toInt,
        normalizeNullableInt,
        formatNumber,
        readMinQty,
        defaultI18n,
        cloneTemplate,
        formatPriceText,
        buildPriceSummary,
        resolveText,
        isStockAvailable,
        clampQuantity,
        isLoggedIn,
        createCartPayload,
        runBuyGuard,
        submitCart,
        analyzeSpecStructure
    };

    window.PageReady = function () {
        window.productContentPage = createProductContent({
            productId: window.PageId,
            canShop: $('.btn_addToCar').length > 0,
            totalBonus: typeof totalBonus !== 'undefined' ? totalBonus : 0,
            orderPrice: typeof orderPrice !== 'undefined' ? orderPrice : false,
            i18n: defaultI18n
        });
    };
})(window, window.jQuery);
