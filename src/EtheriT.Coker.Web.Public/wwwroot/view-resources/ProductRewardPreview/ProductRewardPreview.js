(function (window, $) {
    'use strict';

    if (!$) return;

    var current = null;
    var restoreParent = null;
    var rootSelector = '#ProductRewardPreviewModal';
    var number = function (value) { return Number.parseInt(value, 10) || 0; };
    var money = function (value) { return number(value).toLocaleString('zh-TW'); };

    function $root() { return $(rootSelector); }

    function ensureRootAtBody() {
        var $modal = $root();
        if ($modal.length && !$modal.parent().is('body')) $modal.appendTo(document.body);
        return $modal;
    }

    function productUrl(productId) {
        var orgName = String(window.OrgName || '').replace(/^\/+|\/+$/g, '');
        return (orgName ? '/' + orgName : '') + '/search/product/' + number(productId);
    }

    function render(options) {
        var $modal = $root();
        var offerPrice = Number(options.offerPrice) || 0;
        var originalPrice = Number(options.originalPrice) || 0;
        var productName = options.productName || '優惠商品';
        var selected = options.selected === true;
        var disabled = options.disabled === true && !selected;

        $modal.find('.product-reward-preview__image').attr({
            src: options.imageUrl || '/images/noImg.jpg',
            alt: productName
        });
        $modal.find('.product-reward-preview__benefit').text(options.benefitText || (offerPrice <= 0 ? '贈品' : '加價購'));
        $modal.find('.product-reward-preview__name').text(productName);
        $modal.find('.product-reward-preview__stock').text(options.stockName || '').toggle(!!options.stockName);
        $modal.find('.product-reward-preview__offer').text(offerPrice <= 0 ? '免費贈送' : '優惠價 NT$ ' + money(offerPrice));
        $modal.find('.product-reward-preview__original')
            .text(originalPrice > 0 && originalPrice !== offerPrice ? '原價 NT$ ' + money(originalPrice) : '')
            .toggle(originalPrice > 0 && originalPrice !== offerPrice);
        $modal.find('.product-reward-preview__link')
            .attr('href', options.productUrl || productUrl(options.productId))
            .toggleClass('d-none', number(options.productId) <= 0 && !options.productUrl);
        $modal.find('.product-reward-preview__select')
            .prop('disabled', disabled)
            .toggleClass('is-selected', selected)
            .text(selected
                ? (options.cancelText || '取消選取')
                : disabled
                    ? (options.disabledText || '目前無法選擇')
                    : (options.actionText || (offerPrice <= 0 ? '我要贈品' : '我要加購')));
    }

    function show(options) {
        ensureRootAtBody();
        current = $.extend({}, options || {});
        render(current);
        var modalElement = $root().get(0);
        if (!modalElement || !window.bootstrap || !window.bootstrap.Modal) return;

        var openPreview = function () {
            window.bootstrap.Modal.getOrCreateInstance(modalElement).show();
        };
        var parentElement = current.parentModal ? document.querySelector(current.parentModal) : null;
        var parentInstance = parentElement ? window.bootstrap.Modal.getInstance(parentElement) : null;
        if (parentElement && parentInstance && parentElement.classList.contains('show')) {
            restoreParent = parentElement;
            $(parentElement).one('hidden.bs.modal.productRewardPreview', openPreview);
            parentInstance.hide();
        } else {
            restoreParent = null;
            openPreview();
        }
    }

    function close(shouldRestoreParent) {
        if (shouldRestoreParent === false) restoreParent = null;
        var modalElement = $root().get(0);
        if (modalElement && window.bootstrap && window.bootstrap.Modal) {
            window.bootstrap.Modal.getInstance(modalElement)?.hide();
        }
    }

    $(document)
        .off('click.productRewardPreview', rootSelector + ' .product-reward-preview__select')
        .on('click.productRewardPreview', rootSelector + ' .product-reward-preview__select', function () {
            if (!current || current.disabled === true && current.selected !== true) return;
            if (typeof current.onAction === 'function' && current.onAction(current) === false) return;
            close();
        })
        .off('error.productRewardPreview', rootSelector + ' .product-reward-preview__image')
        .on('error.productRewardPreview', rootSelector + ' .product-reward-preview__image', function () {
            if (!String(this.src).endsWith('/images/noImg.jpg')) this.src = '/images/noImg.jpg';
        })
        .off('hidden.bs.modal.productRewardPreview', rootSelector)
        .on('hidden.bs.modal.productRewardPreview', rootSelector, function () {
            var parentElement = restoreParent;
            restoreParent = null;
            current = null;
            if (parentElement && document.body.contains(parentElement) && window.bootstrap && window.bootstrap.Modal) {
                window.bootstrap.Modal.getOrCreateInstance(parentElement).show();
            }
        });

    window.ProductRewardPreview = { open: show, close: close };
})(window, window.jQuery);
