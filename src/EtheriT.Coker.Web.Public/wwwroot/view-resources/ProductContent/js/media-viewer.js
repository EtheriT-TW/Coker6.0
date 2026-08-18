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
        createCartPayload, runBuyGuard, submitCart, ProductSelectionEngine
    } = I;

    class ProductMediaViewer {
        constructor(options) {
            this.options = options;
            this.$modal = $(options.selectors.modal);
            this.$image = $('#Pro_Image');
            this.$video = $('#Pro_Video');
            this.$youtube = $('#Pro_Youtube');
            this.$view360 = $('#Pro_360View');
            this.$dots = this.$modal.find('.pro-display-dots');
            this.items = [];
            this.renderToken = 0;
            this.swipeStartX = 0;
            this.swipeStartY = 0;
            this.swipeStartTime = 0;
            this.hasSeenSwipeHint = sessionStorage.getItem('ProductMediaSwipeHintSeen') === '1';
            this.bindModalEvents();
        }

        setItems(items) {
            this.items = Array.isArray(items) ? items : [];
            this.renderDots();
        }

        renderDots() {
            this.refreshMediaElements();

            const hasMultipleMedia = this.items.length > 1;

            this.$modal.toggleClass('has-multiple-media', hasMultipleMedia);
            this.$modal.toggleClass('swipe-hint-seen', this.hasSeenSwipeHint);

            if (!this.$dots.length) return;

            this.$dots.empty();
            this.$dots.toggleClass('d-none', !hasMultipleMedia);

            if (!hasMultipleMedia) return;

            this.items.forEach((item, index) => {
                this.$dots.append(
                    $('<button/>', {
                        type: 'button',
                        class: 'pro-display-dot',
                        'aria-label': local.GoToSlide.format(index + 1),
                        'data-index': index
                    })
                );
            });
        }

        syncDots(index) {
            this.refreshMediaElements();

            if (!this.$dots.length) return;

            this.$dots
                .find('.pro-display-dot')
                .removeClass('active')
                .attr('aria-current', 'false');

            this.$dots
                .find(`.pro-display-dot[data-index="${index}"]`)
                .addClass('active')
                .attr('aria-current', 'true');
        }

        markSwipeHintSeen() {
            this.hasSeenSwipeHint = true;
            sessionStorage.setItem('ProductMediaSwipeHintSeen', '1');
            this.$modal.addClass('swipe-hint-seen');
        }

        refreshMediaElements() {
            this.$image = $('#Pro_Image');
            this.$video = $('#Pro_Video');
            this.$youtube = $('#Pro_Youtube');
            this.$view360 = $('#Pro_360View');
            this.$dots = this.$modal.find('.pro-display-dots');
        }

        bindModalEvents() {
            const modalElement = this.$modal.get(0);
            if (!modalElement) return;

            modalElement.addEventListener('hidden.bs.modal', () => {
                if (window.CI360) window.CI360.destroy();

                this.refreshMediaElements();

                this.$video.attr('src', '').addClass('d-none');
                this.$youtube.attr('src', '').addClass('d-none');

                this.$image
                    .addClass('d-none')
                    .removeClass('cloudimage-360 initialized')
                    .empty()
                    .removeAttr('data-folder data-filename-x data-amount-x');

                this.$view360
                    .addClass('d-none')
                    .removeClass('cloudimage-360 initialized')
                    .empty()
                    .removeAttr('data-folder data-filename-x data-amount-x');
            });

            this.$modal.find('.btn-tool.prev-btn').off('click').on('click', (e) => {
                e.preventDefault();
                this.move(-1);
            });

            this.$modal.find('.btn-tool.next-btn').off('click').on('click', (e) => {
                e.preventDefault();
                this.move(1);
            });

            this.$modal
                .off('click.productMediaDots', '.pro-display-dot')
                .on('click.productMediaDots', '.pro-display-dot', (e) => {
                    e.preventDefault();

                    const index = normalizeNullableInt($(e.currentTarget).attr('data-index'), -1);
                    if (index < 0) return;

                    this.showByIndex(index);
                });

            this.$modal
                .off('touchstart.productMediaSwipe', '.modal-body')
                .on('touchstart.productMediaSwipe', '.modal-body', (e) => {
                    const touch = e.originalEvent.touches?.[0];
                    if (!touch) return;

                    this.swipeStartX = touch.clientX;
                    this.swipeStartY = touch.clientY;
                    this.swipeStartTime = Date.now();
                });

            this.$modal
                .off('touchend.productMediaSwipe', '.modal-body')
                .on('touchend.productMediaSwipe', '.modal-body', (e) => {
                    const touch = e.originalEvent.changedTouches?.[0];
                    if (!touch) return;

                    const diffX = touch.clientX - this.swipeStartX;
                    const diffY = touch.clientY - this.swipeStartY;
                    const elapsed = Date.now() - this.swipeStartTime;

                    const isHorizontalSwipe =
                        Math.abs(diffX) >= 60 &&
                        Math.abs(diffX) > Math.abs(diffY) * 1.5 &&
                        elapsed <= 800;

                    if (!isHorizontalSwipe) return;

                    e.preventDefault();

                    this.markSwipeHintSeen();
                    if (diffX < 0) {
                        this.move(1);
                    } else {
                        this.move(-1);
                    }
                });
        }

        move(step) {
            if (!this.items.length) return;

            let index = normalizeNullableInt(this.$modal.data('index'), 0);

            if (index < 0 || index >= this.items.length) {
                index = 0;
            }

            index = (index + step + this.items.length) % this.items.length;

            this.showByIndex(index);
        }

        showById(id) {
            const index = this.items.findIndex(x =>
                normalizeNullableInt(x.id) === normalizeNullableInt(id)
            );

            if (index < 0) return;

            this.showByIndex(index);
        }

        showByIndex(index) {
            index = normalizeNullableInt(index, -1);

            if (index < 0 || index >= this.items.length) return;

            const item = this.items[index];
            if (!item) return;

            this.$modal.data('id', item.id);
            this.$modal.data('index', index);
            this.syncDots(index);

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
            const renderToken = ++this.renderToken;

            if (window.CI360) window.CI360.destroy();

            this.refreshMediaElements();

            this.$image
                .addClass('d-none')
                .removeClass('cloudimage-360 initialized')
                .empty()
                .removeAttr('data-folder data-filename-x data-amount-x');

            this.$video.addClass('d-none').attr('src', '');
            this.$youtube.addClass('d-none').attr('src', '');

            this.$view360
                .addClass('d-none')
                .removeClass('cloudimage-360 initialized')
                .empty()
                .removeAttr('data-folder data-filename-x data-amount-x');

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

            const src = item.link?.[0] || '';

            if (!src) return;

            const folder = src.substring(0, src.lastIndexOf('/') + 1);
            const filename = src.substring(src.lastIndexOf('/') + 1);

            this.refreshMediaElements();

            this.$image
                .removeClass('d-none cloudimage-360 initialized')
                .empty()
                .removeAttr('data-folder data-filename-x data-amount-x')
                .attr({
                    'data-folder': folder,
                    'data-filename-x': filename,
                    'data-amount-x': 1
                })
                .css({
                    position: 'relative',
                    width: '100%',
                    height: '100%'
                });

            this.fitModalByImage(src, () => {
                if (renderToken !== this.renderToken) return;

                this.refreshMediaElements();

                this.$image
                    .removeClass('initialized')
                    .addClass('cloudimage-360');

                if (window.CI360) {
                    window.CI360._viewers = [];
                    window.CI360.add('Pro_Image');
                }

                setTimeout(() => {
                    if (renderToken !== this.renderToken) return;

                    this.refreshMediaElements();

                    const canvasHeight = this.$image.find('canvas').outerHeight();

                    if (canvasHeight > 0) {
                        this.$modal.find('.modal-body').css({
                            height: canvasHeight + 'px'
                        });
                    }
                }, 180);
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

                let maxWidthRatio = 1;
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

    I.ProductMediaViewer = ProductMediaViewer;
})(window, window.jQuery);
