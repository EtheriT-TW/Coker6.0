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
        createCartPayload, runBuyGuard, submitCart, parseExternalVideo, ProductSelectionEngine
    } = I;

    class ProductMediaViewer {
        constructor(options) {
            this.options = options;
            this.$modal = $(options.selectors.modal);
            this.$image = $('#Pro_Image');
            this.$video = $('#Pro_Video');
            this.$youtube = this.ensureExternalVideoHost();
            this.$view360 = $('#Pro_360View');
            this.$dots = this.$modal.find('.pro-display-dots');
            this.items = [];
            this.renderToken = 0;
            this.swipeStartX = 0;
            this.swipeStartY = 0;
            this.swipeStartTime = 0;
            this.navigationHideTimer = null;
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

        ensureExternalVideoHost() {
            const $current = $('#Pro_Youtube');
            if (!$current.is('iframe')) return $current;

            const $host = $('<div/>', {
                id: 'Pro_Youtube',
                class: `${$current.attr('class') || ''} pro_external_video`.trim(),
                'aria-label': '外嵌影片播放器'
            });
            $current.replaceWith($host);
            return $host;
        }

        refreshMediaElements() {
            this.$image = $('#Pro_Image');
            this.$video = $('#Pro_Video');
            this.$youtube = this.ensureExternalVideoHost();
            this.$view360 = $('#Pro_360View');
            this.$dots = this.$modal.find('.pro-display-dots');
        }

        bindModalEvents() {
            const modalElement = this.$modal.get(0);
            if (!modalElement) return;

            modalElement.addEventListener('hidden.bs.modal', () => {
                if (window.CI360) window.CI360.destroy();

                this.refreshMediaElements();
                window.clearTimeout(this.navigationHideTimer);
                this.navigationHideTimer = null;
                this.$modal.removeClass('is-360-media show-360-navigation');

                this.$video.attr('src', '').addClass('d-none');
                this.$youtube.empty().addClass('d-none');

                this.$image
                    .addClass('d-none')
                    .removeClass('cloudimage-360 initialized is-zoomed')
                    .empty()
                    .removeAttr('data-folder data-filename-x data-amount-x');

                this.$view360
                    .addClass('d-none')
                    .removeClass('cloudimage-360 initialized')
                    .empty()
                    .removeAttr('data-folder data-filename-x data-image-list-x data-amount-x data-active-frame data-frame-count aria-label');
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
                    if ($(e.target).closest('#Pro_360View').length) return;
                    if ($(e.target).closest('#Pro_Image.is-zoomed').length) return;

                    const touch = e.originalEvent.touches?.[0];
                    if (!touch) return;

                    this.swipeStartX = touch.clientX;
                    this.swipeStartY = touch.clientY;
                    this.swipeStartTime = Date.now();
                });

            this.$modal
                .off('touchend.productMediaSwipe', '.modal-body')
                .on('touchend.productMediaSwipe', '.modal-body', (e) => {
                    if ($(e.target).closest('#Pro_360View').length) return;
                    if ($(e.target).closest('#Pro_Image.is-zoomed').length) return;

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

        initializeFrameViewer($container, imageList, alt) {
            const frames = Array.isArray(imageList)
                ? imageList.filter(link => typeof link === 'string' && link.trim() !== '')
                : [];

            if (!$container.length || !frames.length) return;

            const $image = $('<img/>', {
                class: 'pro_360view_frame',
                src: frames[0],
                alt: alt || '360° 商品圖',
                draggable: false
            });
            const $hint = $('<div/>', {
                class: 'pro_360view_hint',
                'aria-hidden': 'true'
            }).append(
                $('<img/>', {
                    src: '/images/product/icon_360.png',
                    alt: ''
                }),
                $('<span/>', { text: '360° 拖曳旋轉' })
            );
            const element = $image.get(0);
            const state = {
                frameIndex: 0,
                pointerId: null,
                lastX: 0,
                remainder: 0
            };
            const pixelsPerFrame = 8;

            const revealNavigation = () => {
                window.clearTimeout(this.navigationHideTimer);
                this.$modal.addClass('show-360-navigation');
                this.navigationHideTimer = window.setTimeout(() => {
                    this.$modal.removeClass('show-360-navigation');
                }, 4000);
            };

            const showFrame = (index) => {
                state.frameIndex = (index + frames.length) % frames.length;
                element.src = frames[state.frameIndex];
                $container.attr('data-active-frame', state.frameIndex + 1);
            };

            frames.slice(1).forEach(src => {
                const frame = new Image();
                frame.src = src;
            });

            $image
                .on('dragstart.productMedia360', e => e.preventDefault())
                .on('pointerdown.productMedia360', (e) => {
                    const event = e.originalEvent;
                    if (!event || (event.button != null && event.button !== 0)) return;

                    revealNavigation();
                    state.pointerId = event.pointerId;
                    state.lastX = event.clientX;
                    state.remainder = 0;
                    element.setPointerCapture?.(event.pointerId);
                    e.preventDefault();
                    e.stopPropagation();
                })
                .on('pointermove.productMedia360', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    const delta = event.clientX - state.lastX;
                    state.lastX = event.clientX;
                    state.remainder += delta;

                    const steps = Math.trunc(state.remainder / pixelsPerFrame);
                    if (steps !== 0) {
                        state.remainder -= steps * pixelsPerFrame;
                        showFrame(state.frameIndex + steps);
                    }

                    e.preventDefault();
                    e.stopPropagation();
                })
                .on('pointerup.productMedia360 pointercancel.productMedia360', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    element.releasePointerCapture?.(event.pointerId);
                    state.pointerId = null;
                    e.stopPropagation();
                });

            $container
                .attr({
                    'data-active-frame': 1,
                    'data-frame-count': frames.length,
                    'aria-label': `360° 商品圖，共 ${frames.length} 張影格；左右拖曳可旋轉`
                })
                .empty()
                .append($image, $hint);
        }

        initializeZoomViewer($container, src, alt) {
            if (!$container.length || !src) return;

            const $image = $('<img/>', {
                class: 'pro_zoom_frame',
                src,
                alt: alt || '商品圖片',
                draggable: false
            });
            const element = $image.get(0);
            const state = {
                scale: 1,
                x: 0,
                y: 0,
                pointerId: null,
                startX: 0,
                startY: 0,
                originX: 0,
                originY: 0,
                moved: false
            };

            const clampPosition = () => {
                const maxX = Math.max(0, $container.innerWidth() * (state.scale - 1) / 2);
                const maxY = Math.max(0, $container.innerHeight() * (state.scale - 1) / 2);
                state.x = Math.max(-maxX, Math.min(maxX, state.x));
                state.y = Math.max(-maxY, Math.min(maxY, state.y));
            };

            const applyTransform = () => {
                clampPosition();
                element.style.transform = `translate3d(${state.x}px, ${state.y}px, 0) scale(${state.scale})`;
                $container.toggleClass('is-zoomed', state.scale > 1);
            };

            const toggleZoom = () => {
                state.scale = state.scale > 1 ? 1 : 2;
                state.x = 0;
                state.y = 0;
                applyTransform();
            };

            $image
                .on('dragstart.productMediaZoom dblclick.productMediaZoom', e => e.preventDefault())
                .on('pointerdown.productMediaZoom', (e) => {
                    const event = e.originalEvent;
                    if (!event || (event.button != null && event.button !== 0)) return;

                    state.pointerId = event.pointerId;
                    state.startX = event.clientX;
                    state.startY = event.clientY;
                    state.originX = state.x;
                    state.originY = state.y;
                    state.moved = false;
                    element.setPointerCapture?.(event.pointerId);

                    if (state.scale > 1) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                })
                .on('pointermove.productMediaZoom', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    const deltaX = event.clientX - state.startX;
                    const deltaY = event.clientY - state.startY;
                    if (Math.abs(deltaX) > 5 || Math.abs(deltaY) > 5) state.moved = true;

                    if (state.scale <= 1) return;

                    state.x = state.originX + deltaX;
                    state.y = state.originY + deltaY;
                    applyTransform();
                    e.preventDefault();
                    e.stopPropagation();
                })
                .on('pointerup.productMediaZoom pointercancel.productMediaZoom', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    element.releasePointerCapture?.(event.pointerId);
                    state.pointerId = null;

                    if (!state.moved && e.type === 'pointerup') {
                        toggleZoom();
                        e.preventDefault();
                        e.stopPropagation();
                    } else if (state.scale > 1) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                })
                .on('wheel.productMediaZoom', (e) => {
                    const event = e.originalEvent;
                    if (!event) return;

                    state.scale = Math.max(1, Math.min(3, state.scale + (event.deltaY < 0 ? .25 : -.25)));
                    if (state.scale === 1) {
                        state.x = 0;
                        state.y = 0;
                    }
                    applyTransform();
                    e.preventDefault();
                    e.stopPropagation();
                });

            $container
                .removeClass('is-zoomed')
                .empty()
                .append($image);
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
                    ? 'external-video'
                    : item.fileType === 2
                        ? '360view'
                        : 'image';

            this.render(item, type);
        }

        render(item, type) {
            const renderToken = ++this.renderToken;

            if (window.CI360) window.CI360.destroy();

            this.refreshMediaElements();
            window.clearTimeout(this.navigationHideTimer);
            this.navigationHideTimer = null;
            this.$modal
                .toggleClass('is-360-media', type === '360view')
                .removeClass('show-360-navigation');

            this.$image
                .addClass('d-none')
                .removeClass('cloudimage-360 initialized is-zoomed')
                .empty()
                .removeAttr('data-folder data-filename-x data-amount-x');

            this.$video.addClass('d-none').attr('src', '');
            this.$youtube.addClass('d-none').empty();

            this.$view360
                .addClass('d-none')
                .removeClass('cloudimage-360 initialized')
                .empty()
                .removeAttr('data-folder data-filename-x data-image-list-x data-amount-x data-active-frame data-frame-count aria-label');

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

            if (type === 'external-video') {
                this.resetModalSize();

                const externalVideo = parseExternalVideo(item.name);
                if (!externalVideo) return;

                const configuredRatio = item.aspectRatio || item.AspectRatio || item.link?.[2] || 'auto';
                const ratioKey = configuredRatio === 'auto'
                    ? (externalVideo.isReel ? '9:16' : '16:9')
                    : configuredRatio;
                const ratioMap = {
                    '16:9': { width: 16, height: 9, maxWidth: 960 },
                    '9:16': { width: 9, height: 16, maxWidth: 480 },
                    '1:1': { width: 1, height: 1, maxWidth: 720 },
                    '4:3': { width: 4, height: 3, maxWidth: 840 }
                };
                const ratio = ratioMap[ratioKey] || ratioMap['16:9'];
                const availableWidth = Math.max(280, window.innerWidth * .92);
                const availableHeight = Math.max(280, window.innerHeight * .82);
                let playerWidth = Math.min(ratio.maxWidth, availableWidth, availableHeight * ratio.width / ratio.height);
                let playerHeight = playerWidth * ratio.height / ratio.width;
                if (playerHeight > availableHeight) {
                    playerHeight = availableHeight;
                    playerWidth = playerHeight * ratio.width / ratio.height;
                }

                this.$modal.addClass('is-external-video');
                this.$modal.find('.modal-dialog').addClass('ytshow').css({
                    width: `${Math.round(playerWidth)}px`,
                    maxWidth: '92vw'
                });
                this.$modal.find('.modal-body').css({
                    height: `${Math.round(playerHeight)}px`,
                    maxHeight: '82vh',
                    overflow: 'hidden'
                });

                this.$youtube.removeClass('d-none').empty().css({ width: '100%', height: '100%' });

                if (externalVideo.provider === 'facebook') {
                    const $embed = $('<div class="fb-video"></div>').attr({
                        'data-href': externalVideo.url,
                        'data-width': String(Math.max(280, Math.round(playerWidth))),
                        'data-show-text': 'false',
                        'data-allowfullscreen': 'true'
                    }).append($('<blockquote class="fb-xfbml-parse-ignore"></blockquote>').attr('cite', externalVideo.url)
                        .append($('<a target="_blank" rel="noopener noreferrer">在 Facebook 查看影片</a>').attr('href', externalVideo.url)));
                    const $facebookHost = $('<div class="external-video-facebook is-loading d-flex justify-content-center w-100"></div>')
                        .append('<div class="external-video-facebook__loading"><i class="fa-brands fa-facebook"></i><span>正在載入 Facebook 影片…</span></div>')
                        .append($embed);
                    this.$youtube.append($facebookHost);

                    let revealTimer = window.setTimeout(() => {
                        $facebookHost.addClass('is-ready').removeClass('is-loading');
                    }, 5000);
                    const revealFacebook = () => {
                        window.clearTimeout(revealTimer);
                        $facebookHost.addClass('is-ready').removeClass('is-loading');
                        observer.disconnect();
                    };
                    const observer = new MutationObserver(() => {
                        const iframe = $facebookHost.find('iframe').get(0);
                        if (iframe && !iframe.dataset.productContentLoadBound) {
                            iframe.dataset.productContentLoadBound = 'true';
                            iframe.addEventListener('load', revealFacebook, { once: true });
                        }
                    });
                    observer.observe($facebookHost.get(0), { childList: true, subtree: true });

                    const renderFacebook = () => {
                        if (window.FB?.XFBML) window.FB.XFBML.parse(this.$youtube.get(0));
                    };
                    if (!document.getElementById('fb-root')) document.body.insertAdjacentHTML('afterbegin', '<div id="fb-root"></div>');
                    let script = document.getElementById('facebook-jssdk');
                    if (window.FB?.XFBML) renderFacebook();
                    else if (!script) {
                        script = document.createElement('script');
                        script.id = 'facebook-jssdk';
                        script.src = 'https://connect.facebook.net/zh_TW/sdk.js#xfbml=1&version=v25.0';
                        script.async = true;
                        script.defer = true;
                        script.crossOrigin = 'anonymous';
                        document.head.appendChild(script);
                        script.addEventListener('load', renderFacebook, { once: true });
                    } else script.addEventListener('load', renderFacebook, { once: true });
                } else if (externalVideo.provider === 'threads') {
                    const safeUrl = externalVideo.url.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                    const source = `<!doctype html><html><head><meta name="viewport" content="width=device-width,initial-scale=1"><style>html,body{box-sizing:border-box;margin:0;min-height:100%;width:100%;overflow-x:hidden}body{display:flex;justify-content:center;padding:8px}body>blockquote,body>iframe{box-sizing:border-box!important;margin:0!important;max-width:none!important;min-width:0!important;width:100%!important}</style></head><body><blockquote class="text-post-media" data-text-post-permalink="${safeUrl}" data-text-post-version="0"><a href="${safeUrl}" target="_blank" rel="noopener noreferrer">在 Threads 查看貼文</a></blockquote><script async src="https://www.threads.com/embed.js"></script><script>new MutationObserver(function(){document.querySelectorAll('body>iframe').forEach(function(frame){frame.style.setProperty('width','100%','important');frame.style.setProperty('max-width','none','important')})}).observe(document.body,{childList:true,subtree:true})</script></body></html>`;
                    this.$youtube.append($('<iframe class="external-video-threads-frame" title="Threads 貼文播放器" frameborder="0"></iframe>')
                        .attr('srcdoc', source));
                } else if (externalVideo.provider === 'x') {
                    const target = this.$youtube.get(0);
                    const renderPost = () => {
                        if (window.twttr?.widgets?.createTweet) {
                            window.twttr.widgets.createTweet(externalVideo.externalId, target, {
                                align: 'center',
                                conversation: 'none'
                            });
                        } else {
                            this.$youtube.append($('<a target="_blank" rel="noopener noreferrer">在 X 查看貼文</a>').attr('href', externalVideo.url));
                        }
                    };

                    if (window.twttr?.widgets) renderPost();
                    else {
                        let script = document.querySelector('script[src="https://platform.twitter.com/widgets.js"]');
                        if (!script) {
                            script = document.createElement('script');
                            script.src = 'https://platform.twitter.com/widgets.js';
                            script.async = true;
                            document.head.appendChild(script);
                        }
                        script.addEventListener('load', renderPost, { once: true });
                    }
                } else {
                    this.$youtube.append($('<iframe class="w-100 h-100" title="外嵌影片播放器" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>')
                        .attr('src', externalVideo.embedUrl));
                }

                return;
            }

            if (type === '360view') {
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

                const imageList = Array.isArray(item.link) ? item.link.filter(Boolean) : [];
                if (!imageList.length) return;

                this.$view360
                    .removeAttr('data-folder data-filename-x data-image-list-x data-amount-x')
                    .attr({
                        'data-image-list-x': JSON.stringify(imageList),
                        'data-amount-x': imageList.length
                    });

                this.initializeFrameViewer(this.$view360, imageList, item.name);

                this.fitModalByImage(src, () => {
                    if (renderToken !== this.renderToken) return;
                    this.refreshMediaElements();
                    this.$view360.removeClass('d-none');
                });

                return;
            }

            const src = item.link?.[0] || '';

            if (!src) return;

            this.refreshMediaElements();

            this.$image
                .removeClass('d-none cloudimage-360 initialized is-zoomed')
                .empty()
                .removeAttr('data-folder data-filename-x data-amount-x')
                .css({
                    position: 'relative',
                    width: '100%',
                    height: '100%'
                });

            this.initializeZoomViewer(this.$image, src, item.name);

            this.fitModalByImage(src, () => {
                if (renderToken !== this.renderToken) return;

                this.refreshMediaElements();
                this.$image.removeClass('d-none');
            });
        }

        resetModalSize() {
            this.$modal.removeClass('is-external-video');
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
