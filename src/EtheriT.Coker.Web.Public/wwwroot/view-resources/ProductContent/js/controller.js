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
        createCartPayload, runBuyGuard, submitCart, parseExternalVideo, ProductSelectionEngine, ProductMediaViewer
    } = I;

    function normalizePublicMediaPath(value, orgName) {
        const path = String(value || '').trim().replace(/\\/g, '/');
        if (!path || /^(?:https?:|data:|blob:)/i.test(path)) return path;

        const normalizedOrgName = String(orgName || '').trim().replace(/^\/+|\/+$/g, '');
        if (!normalizedOrgName) return path;

        const orgPrefix = `/upload/${normalizedOrgName}/`;
        if (path.toLowerCase().startsWith(orgPrefix.toLowerCase())) {
            return `/upload/${path.substring(orgPrefix.length)}`;
        }

        return path;
    }

    class ProductContentController {
        constructor(options) {
            this.options = $.extend(true, {}, DEFAULTS, options || {});
            this.$pageRoot = $(this.options.pageRoot);
            this.$root = this.$pageRoot.find(this.options.selectors.product);
            this.$contentRoot = this.$root.find(this.options.selectors.content);
            this.options.totalBonus = normalizeNullableInt(this.options.totalBonus, 0);
            this.options.orderPrice = !!this.options.orderPrice;
            this.state = {
                productId: this.resolveProductId(),
                product: null,
                selection: null,
                previewSwiper: null,
                productSwiper: null,
                navigationHideTimer: null,
                requestId: 0
            };
            this.mediaViewer = new ProductMediaViewer(this.options);
            const layoutFactory = getLayoutFactory();
            this.layout = layoutFactory ? layoutFactory(this) : null;
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

        getRequestedStockId() {
            const value = new URLSearchParams(window.location.search).get('psid');
            const stockId = normalizeNullableInt(value, 0);
            return stockId > 0 ? stockId : null;
        }

        setVariantUrl(stockId) {
            const normalizedStockId = normalizeNullableInt(stockId, 0);
            const url = new URL(window.location.href);

            if (normalizedStockId > 0) {
                url.searchParams.set('psid', String(normalizedStockId));
            } else {
                url.searchParams.delete('psid');
            }

            if (url.href === window.location.href) return;

            const historyState = $.extend({}, window.history.state || {}, {
                productId: Number(this.state.productId)
            });
            window.history.replaceState(historyState, '', url.href);
            this.refreshShareUrl();
        }

        syncVariantUrlFromSelection() {
            const activeStock = this.state.selection?.getActiveStock?.();
            if (activeStock?.id > 0) {
                this.setVariantUrl(activeStock.id);
            }
        }

        refreshShareUrl() {
            const shareHref = window.location.pathname + window.location.search;
            this.$pageRoot.find('.shareBlock').each(function () {
                const $share = $(this);
                $share.find('a[data-icon]').off().remove();
                if (typeof window.ProShare === 'function') {
                    $share.off('mouseenter', window.ProShare).off('mouseleave', window.ProShare);
                }
                $share.removeData('init')
                    .data('href', shareHref)
                    .attr('data-href', shareHref);
            });

            if (typeof window.ShareBlockInit === 'function') {
                window.ShareBlockInit(this.$pageRoot);
            }
        }

        init() {
            this.bindStaticEvents();
            if (typeof this.bindNavigation === 'function') this.bindNavigation();
            this.logClick();
            return this.load();
        }

        bindStaticEvents() {
            this.$pageRoot.off('click.productContent', '.pro_display').on('click.productContent', '.pro_display', (e) => {
                const index = normalizeNullableInt($(e.currentTarget).data('index'), -1);

                if (index >= 0) {
                    this.mediaViewer.showByIndex(index);
                    return;
                }

                const id = $(e.currentTarget).data('id');
                this.mediaViewer.showById(id);
            });

            this.$pageRoot.off('click.productContent', '.PreviewSwiper img').on('click.productContent', '.PreviewSwiper img', (e) => {
                const index = normalizeNullableInt($(e.currentTarget).data('index'), -1);

                if (index < 0 || !this.state.productSwiper) return;

                if (typeof this.state.productSwiper.slideToLoop === 'function') {
                    this.state.productSwiper.slideToLoop(index);
                } else {
                    this.state.productSwiper.slideTo(index);
                }
            });

            this.$pageRoot.off('click.productContent', '.btn_tc').on('click.productContent', '.btn_tc', (e) => {
                $('#TabContent > .tab-pane').removeClass('active show');
                $('#TechnicalDocuments').addClass('active show');
                $('#btn_tab .nav-link').removeClass('active');
                $('#pills-documents-tab').addClass('active');

                const tcid = $(e.currentTarget).data('tcid');
                const $target = $(`.badge_${tcid}`);
                if ($target.length > 0) {
                    $('html, body').animate({ scrollTop: $target.offset().top - ($('header > nav').height() || $('header').height() || 0) * 2 }, 0);
                }
            });

            if (this.layout && typeof this.layout.bindEvents === 'function') {
                this.layout.bindEvents();
            }
        }

        logClick() {
            try {
                this.options.api.clickLog(this.state.productId);
            } catch (error) {
                console.warn('Product click log failed.', error);
            }
        }

        load() {
            const productId = this.state.productId;
            const requestId = ++this.state.requestId;
            const request = typeof this.getProductDataRequest === 'function'
                ? this.getProductDataRequest(productId)
                : this.options.api.getMainDisplay(productId);
            this.pendingRequest = request;

            return request.done((result) => {
                if (requestId !== this.state.requestId || String(productId) !== String(this.state.productId)) return;

                if (!result) {
                    window.location.href = this.pendingNavigation?.url
                        || window.location.pathname.substring(0, window.location.pathname.lastIndexOf('/'));
                    return;
                }

                this.state.product = result;
                const requestedStockId = this.getRequestedStockId();
                this.state.selection = new ProductSelectionEngine(result, {
                    ...this.options,
                    initialStockId: requestedStockId
                });

                if (requestedStockId && !this.state.selection.initialStockMatched) {
                    this.setVariantUrl(null);
                }

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

            if (this.layout && typeof this.layout.renderSelectionArea === 'function') {
                this.layout.renderSelectionArea();
            }

            this.initShare();
            this.initFavorite();
            this.initSwitchPage();

            const $main = this.$pageRoot.find(selectors.mainContent);
            $main.removeClass('d-none');

            const $firstTab = this.$pageRoot.find(selectors.tabButtons).filter(':visible').first();
            if ($firstTab.length > 0) {
                $firstTab.trigger('click');
            }

            if (typeof this.options.hooks.afterRender === 'function') {
                this.options.hooks.afterRender(result, this);
            }

            if (typeof this.afterProductNavigation === 'function') {
                this.afterProductNavigation();
            }

            // AJAX 切換商品後同步檢查伺服器輸出的 JSON-LD 是否仍對應目前商品。
            this.renderStructuredData();
        }

        renderStructuredData() {
            const result = this.state.product;
            const scriptId = 'product-structured-data';
            const existingScript = document.getElementById(scriptId);
            const renderedProductId = existingScript?.dataset?.productId;
            const breadcrumbScript = document.getElementById('breadcrumb-structured-data');
            const breadcrumbPageType = breadcrumbScript?.dataset?.pageType;
            const breadcrumbPageId = breadcrumbScript?.dataset?.pageId;

            // JSON-LD 僅採用伺服器端產生的非會員公開價格。
            // AJAX 切換商品時沒有對應的伺服器 SEO 資料，移除舊商品資料以免內容不一致。
            if (!result || !existingScript || String(result.id) !== String(renderedProductId)) {
                existingScript?.remove();
            }
            if (!result ||
                !breadcrumbScript ||
                breadcrumbPageType !== 'Product' ||
                String(result.id) !== String(breadcrumbPageId)) {
                breadcrumbScript?.remove();
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
            const css = this.state.product.css || '';
            const selectors = this.options.selectors;
            const $panel = this.$pageRoot.find(selectors.htmlPanel);
            const $section = this.$pageRoot.find('#ProductDescription');
            const $tab = this.$pageRoot.find('#btn_tab .description');
            let styleElement = document.getElementById('productDescriptionCss');

            if (!styleElement) {
                styleElement = document.createElement('style');
                styleElement.id = 'productDescriptionCss';
                const nonce = document.querySelector('meta[name="csp-nonce"]')?.getAttribute('content');
                if (nonce) styleElement.setAttribute('nonce', nonce);
                document.head.appendChild(styleElement);
            }
            // 首次整頁載入沿用後端已處理 upload 路徑的 CSS；AJAX 換商品時才完整替換。
            if (this.pendingNavigation || !styleElement.textContent.trim()) {
                styleElement.textContent = css;
            }

            if (html && html.trim() !== '') {
                // API 僅回傳後端已清洗、已 Decode 的發布 HTML。
                $section.add($tab).removeClass('d-none');
                $panel.removeClass('d-none').html(html);
            } else {
                $panel.empty();
                $section.add($tab).addClass('d-none');
            }
        }

        renderTechCerts() {
            const result = this.state.product;
            const selectors = this.options.selectors;
            const $root = this.$pageRoot;
            const $list = $root.find(selectors.techCertList).empty();
            const $content = $root.find(selectors.techCertContent).empty();
            const $sections = $root.find('#btn_tab > .technical,.pro_tc,#TechnicalDocuments');

            if (!Array.isArray(result.techCertDatas) || result.techCertDatas.length === 0) {
                $sections.addClass('d-none');
                return;
            }

            $sections.removeClass('d-none');

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
            const $list = this.$pageRoot.find(selectors.fileList).empty();
            const $sections = this.$pageRoot.find('#btn_tab > .files,#FileDownload');

            if (!Array.isArray(result.files) || result.files.length === 0) {
                $sections.addClass('d-none');
                return;
            }

            $sections.removeClass('d-none');

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

            $tagList.removeClass('d-none');

            result.tagDatas.forEach(item => {
                $tagList.prepend(`<li><a class="round_tag rounded-pill me-1 px-3 py-1" href="/${this.options.orgName}/Search/Get/-2/${item.tag_Name}">${item.tag_Name}</a></li>`);
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

        initializeInline360($display, links) {
            const frames = Array.isArray(links)
                ? links.filter(link => typeof link === 'string' && link.trim() !== '')
                : [];

            if (!$display.length || frames.length < 2) return;

            const element = $display.get(0);
            const state = {
                frameIndex: 0,
                pointerId: null,
                lastX: 0,
                remainder: 0,
                moved: false,
                suppressClickUntil: 0
            };
            const pixelsPerFrame = 8;

            const revealNavigation = () => {
                window.clearTimeout(this.state.navigationHideTimer);
                this.$root.addClass('show-360-navigation');
                this.state.navigationHideTimer = window.setTimeout(() => {
                    this.$root.removeClass('show-360-navigation');
                }, 4000);
            };

            const showFrame = (index) => {
                state.frameIndex = (index + frames.length) % frames.length;
                element.src = frames[state.frameIndex];
            };

            frames.slice(1).forEach(src => {
                const frame = new Image();
                frame.src = src;
            });

            $display
                .addClass('inline_360view')
                .attr({
                    draggable: 'false',
                    role: 'button',
                    tabindex: '0',
                    'aria-label': `360° 商品圖，共 ${frames.length} 張影格；左右拖曳可旋轉，點擊可放大`
                })
                .off('.inline360')
                .on('dragstart.inline360', e => e.preventDefault())
                .on('pointerdown.inline360', (e) => {
                    const event = e.originalEvent;
                    if (!event || (event.button != null && event.button !== 0)) return;

                    revealNavigation();
                    state.pointerId = event.pointerId;
                    state.lastX = event.clientX;
                    state.remainder = 0;
                    state.moved = false;
                    element.setPointerCapture?.(event.pointerId);
                    e.stopPropagation();
                })
                .on('pointermove.inline360', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    const delta = event.clientX - state.lastX;
                    state.lastX = event.clientX;
                    state.remainder += delta;

                    const steps = Math.trunc(state.remainder / pixelsPerFrame);
                    if (steps !== 0) {
                        state.moved = true;
                        state.remainder -= steps * pixelsPerFrame;
                        showFrame(state.frameIndex + steps);
                    }

                    e.preventDefault();
                    e.stopPropagation();
                })
                .on('pointerup.inline360 pointercancel.inline360', (e) => {
                    const event = e.originalEvent;
                    if (!event || state.pointerId !== event.pointerId) return;

                    if (state.moved) state.suppressClickUntil = Date.now() + 300;
                    element.releasePointerCapture?.(event.pointerId);
                    state.pointerId = null;
                    e.stopPropagation();
                })
                .on('click.inline360', (e) => {
                    if (Date.now() <= state.suppressClickUntil) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                    }
                })
                .on('keydown.inline360', (e) => {
                    if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;

                    showFrame(state.frameIndex + (e.key === 'ArrowRight' ? 1 : -1));
                    e.preventDefault();
                    e.stopPropagation();
                });
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

            medium.forEach((img, index) => {
                let $slide;

                if (img.fileType === 3) {
                    $slide = cloneTemplate(templates.videoSlide);
                    $slide.find('.pro_display').attr({
                        src: img.link[0],
                        alt: img.name,
                        'data-id': img.id,
                        'data-index': index,
                        'data-display-protype': 'video'
                    });
                } else if (img.fileType === 4) {
                    $slide = cloneTemplate(templates.ytVideoSlide);
                    const externalVideo = parseExternalVideo(img.name);
                    const thumbnail = img.thumbnail || img.link?.[1] || externalVideo?.thumbnail || '/images/defaultImage/video.jpg';
                    $slide.find('.pro_display').attr({
                        src: normalizePublicMediaPath(thumbnail, this.options.orgName),
                        alt: img.name,
                        'data-id': img.id,
                        'data-index': index,
                        'data-external-video': img.name,
                        'data-display-protype': 'external-video'
                    });
                    $slide.find('.schematic_youtube')
                        .addClass(`provider-${externalVideo?.provider || 'external'}`)
                        .find('i').attr('class', externalVideo?.iconClass || 'fa-solid fa-link');
                } else if (img.fileType === 2) {
                    $slide = cloneTemplate(templates.slide3d);
                    const links = Array.isArray(img.link) ? img.link : [];
                    const $display = $slide.find('.pro_display');
                    $display.attr({
                        src: links[0] || '',
                        alt: img.name,
                        'data-id': img.id,
                        'data-index': index,
                        'data-display-protype': '360view',
                        'data-image-list-x': JSON.stringify(links),
                        'data-amount-x': img.amountX || links.length
                    });
                    this.initializeInline360($display, links);
                } else {
                    $slide = cloneTemplate(templates.imageSlide);
                    $slide.find('.pro_display').attr({
                        src: img.link[0],
                        alt: img.name,
                        'data-id': img.id,
                        'data-index': index,
                        'data-display-protype': 'image'
                    });
                }

                $slide.find('.pro_display').imgCheck?.();
                $productWrapper.append($slide);
            });

            small.forEach((img, index) => {
                const $slide = cloneTemplate(templates.previewSlide);
                const $img = $slide.find('img');
                let src = img.link?.[0] || '';

                if (img.fileType === 3) {
                    src = '/images/videopreview.jpg';
                    $slide.addClass('video-preview');
                }
                if (img.fileType === 4) {
                    const externalVideo = parseExternalVideo(img.name);
                    const thumbnail = img.thumbnail || img.link?.[1] || externalVideo?.thumbnail || '/images/defaultImage/video.jpg';
                    src = normalizePublicMediaPath(thumbnail, this.options.orgName);
                    $slide.addClass('video-preview external-video-preview');
                    $slide.append($('<span class="external-video-preview__provider"></span>')
                        .addClass(`provider-${externalVideo?.provider || 'external'}`)
                        .attr('title', externalVideo?.provider || '外嵌影片')
                        .append($('<i></i>').attr('class', externalVideo?.iconClass || 'fa-solid fa-link')));
                }

                if (img.fileType === 3 || img.fileType === 4) {
                    $slide.append($('<span class="video-preview__play" aria-hidden="true"></span>'));
                }

                $img.attr({
                    src,
                    alt: img.name,
                    'data-id': img.id,
                    'data-index': index
                });
                $img.imgCheck?.();
                $previewWrapper.append($slide);
            });

            this.mediaViewer.setItems(original);
            this.initSwipers(small.length);
        }

        initSwipers(smallCount) {
            if (this.state.previewSwiper && this.state.previewSwiper.destroy) {
                this.state.previewSwiper.destroy(true, true);
                this.state.previewSwiper = null;
            }

            if (this.state.productSwiper && this.state.productSwiper.destroy) {
                this.state.productSwiper.destroy(true, true);
                this.state.productSwiper = null;
            }

            const previewEl = this.$root.find('.PreviewSwiper').get(0);
            const productEl = this.$root.find('.ProductSwiper').get(0);
            const prevBtnEl = this.$root.find('.btn_swiper_prev_product').get(0);
            const nextBtnEl = this.$root.find('.btn_swiper_next_product').get(0);
            const scrollbarEl = this.$root.find('.swiper-scrollbar').get(0);
            const $pagination = this.$root.find('.ProductSwiperPagination');

            if (!previewEl || !productEl) return;

            const shouldHidePreview = smallCount <= 1;

            this.$root
                .find('.PreviewSwiper,.btn_swiper_prev_product,.btn_swiper_next_product')
                .toggleClass('d-none', shouldHidePreview);

            // 只有一張圖片時，不顯示圖片張數
            if (smallCount <= 1) {
                $pagination
                    .empty()
                    .removeClass(
                        'swiper-pagination-fraction ' +
                        'swiper-pagination-horizontal ' +
                        'swiper-pagination-lock'
                    )
                    .addClass('d-none');

                return;
            }

            // 多張圖片時重新顯示
            $pagination
                .removeClass('d-none')
                .empty();

            this.state.previewSwiper = new Swiper(previewEl, {
                a11y: true,
                slidesPerView: 4,
                loop: false,
                spaceBetween: 10,
                freeMode: true,
                watchSlidesProgress: true,
                slideToClickedSlide: true,
                scrollbar: scrollbarEl ? { el: scrollbarEl } : undefined,
                breakpoints: {
                    576: { slidesPerView: 4 },
                    768: { slidesPerView: 6 },
                    992: { slidesPerView: 8 }
                }
            });

            this.state.productSwiper = new Swiper(productEl, {
                a11y: true,
                spaceBetween: 15,
                loop: true,
                navigation: {
                    nextEl: nextBtnEl,
                    prevEl: prevBtnEl
                },
                pagination: {
                    el: $pagination.get(0),
                    type: 'fraction'
                },
                breakpoints: {
                    768: { allowTouchMove: true },
                    992: { allowTouchMove: false }
                },
                thumbs: {
                    swiper: this.state.previewSwiper
                },
                on: {
                    init: (swiper) => this.syncInline360Navigation(swiper),
                    slideChange: (swiper) => this.syncInline360Navigation(swiper)
                }
            });
        };

        syncInline360Navigation(swiper) {
            const activeSlide = swiper?.slides?.[swiper.activeIndex];
            const hasActive360 = !!activeSlide?.querySelector('.inline_360view');

            this.$root.toggleClass('is-360-slide-active', hasActive360);

            if (!hasActive360) {
                window.clearTimeout(this.state.navigationHideTimer);
                this.state.navigationHideTimer = null;
                this.$root.removeClass('show-360-navigation');
            }
        }

        initShare() {
            this.refreshShareUrl();
        }

        initFavorite() {
            const $btn = this.$pageRoot.find(this.options.selectors.favoritesButton);
            if ($btn.length === 0) return;
            if (!this.options.api.checkFavorite || !this.options.api.addFavorite || !this.options.api.deleteFavorite) return;

            const productId = this.state.productId;
            $btn.removeClass('turn').removeData('Fid').attr('title', local.AddFavorite);

            this.options.api.checkFavorite(productId)?.done((check) => {
                if (String(productId) !== String(this.state.productId)) return;

                if (check && check.success) {
                    $btn.data('Fid', check.message);
                    $btn.addClass('turn');
                    $btn.attr('title', local.RemoveFavorite);
                } else {
                    $btn.attr('title', local.AddFavorite);
                }
            });

            $btn.off('click.productFavorite').on('click.productFavorite', () => {
                if ($btn.hasClass('turn')) {
                    this.options.api.deleteFavorite($btn.data('Fid'))?.done((result) => {
                        if (result.success) {
                            $btn.removeClass('turn');
                            $btn.attr('title', local.AddFavorite);
                            Coker.sweet.success(local.RemoveFavoriteSuccess, null, true);
                        }
                    });
                } else {
                    this.options.api.addFavorite(this.state.productId)?.done((favorites) => {
                        if (favorites.success) {
                            $btn.addClass('turn');
                            $btn.data('Fid', favorites.message);
                            $btn.attr('title', local.RemoveFavorite);
                            Coker.sweet.success(local.AddFavoriteSuccess, null, true);
                        }
                    });
                }
            });
        }

        initSwitchPage() {
            const $switch = $(this.options.selectors.switchPage);
            if ($switch.length === 0 || !this.options.api.switchPage) return;

            $switch.removeClass('d-none');
            $switch.find('.btn_prev,.btn_next')
                .addClass('disabled')
                .attr({ href: '', title: '' });

            const currentUrl = window.location.pathname + window.location.search;
            const productMarker = '/product/';
            const productIndex = currentUrl.indexOf(productMarker);

            if (productIndex < 0) {
                $switch.addClass('d-none');
                return;
            }

            const catalog = currentUrl.substring(0, productIndex);
            const pathAfterProduct = currentUrl.split('/product/')[1] || '';
            const pathPart = pathAfterProduct.split('?')[0];
            const pathParts = pathPart.split('/').filter(Boolean);
            const productid = pathParts[0];
            let searchtext = pathParts[1] || '';
            try {
                searchtext = decodeURIComponent(searchtext);
            } catch (error) {
                console.warn('ProductContent: invalid search path encoding.', error);
            }
            const urlParams = new URLSearchParams(window.location.search);
            const dirid = urlParams.get('dirid');
            const diridList = dirid == null ? null : dirid.split(',').map(Number);
            const filter = urlParams.get('filter');
            const routername = catalog.substring(catalog.lastIndexOf('/') + 1).toLowerCase();
            const encodedSearchText = encodeURIComponent(searchtext);
            const filterSuffix = filter ? `?filter=${encodeURIComponent(filter)}` : '';
            const searchCacheKey = `product-${searchtext}-${filter || ''}`;

            $switch.find('.btn_list').attr('href', catalog);

            const bindSearchMode = (list) => {
                if (!Array.isArray(list) || list.length === 0) {
                    $switch.addClass('d-none');
                    return;
                }

                const index = list.findIndex(p => String(p.key) === String(productid));
                $switch.find('.btn_list').attr('href', `${catalog}/Get/3/${encodedSearchText}${filterSuffix}`);

                if (index > 0) {
                    const prev = list[index - 1];
                    const link = `${catalog}/product/${prev.key}/${encodedSearchText}${filterSuffix}`;
                    $switch.find('.btn_prev').attr({ href: link, title: prev.value }).removeClass('disabled');
                }

                if (index < list.length - 1 && index >= 0) {
                    const next = list[index + 1];
                    const link = `${catalog}/product/${next.key}/${encodedSearchText}${filterSuffix}`;
                    $switch.find('.btn_next').attr({ href: link, title: next.value }).removeClass('disabled');
                }
            };

            if (routername === 'search' && searchtext) {
                try {
                    const cachedList = sessionStorage.getItem(searchCacheKey);
                    if (cachedList) {
                        const parsedList = JSON.parse(cachedList);
                        if (Array.isArray(parsedList)) {
                            bindSearchMode(parsedList);
                            return;
                        }
                    }
                } catch (error) {
                    console.warn('ProductContent: search navigation cache is unavailable.', error);
                }
            }

            this.options.api.switchPage({
                id: productid,
                dirids: diridList,
                routername: routername,
                searchtext: searchtext,
                filters: filter,
                type: 1
            })?.done((result) => {
                if (String(productid) !== String(this.state.productId)) return;

                if (!Array.isArray(result) || result.length === 0) {
                    $switch.addClass('d-none');
                    return;
                }

                if (routername === 'search') {
                    try {
                        sessionStorage.setItem(searchCacheKey, JSON.stringify(result));
                    } catch (error) {
                        console.warn('ProductContent: unable to cache search navigation.', error);
                    }
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

    I.ProductContentController = ProductContentController;
})(window, window.jQuery);
