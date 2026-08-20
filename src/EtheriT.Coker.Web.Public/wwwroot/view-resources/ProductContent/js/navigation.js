(function (window, $) {
    'use strict';

    if (!$) return;

    const I = window.ProductContentInternals;
    if (!I || !I.ProductContentController) return;

    const Controller = I.ProductContentController;

    function getProductIdFromUrl(url) {
        const match = url.pathname.match(/\/product\/(\d+)/i);
        return match ? Number(match[1]) : null;
    }

    function setMetaContent(selector, value) {
        const element = document.querySelector(selector);
        if (element) element.setAttribute('content', value || '');
    }

    function getDescription(product) {
        const text = product.description || product.introduction || product.html || '';
        return $('<div>').html(text).text().replace(/\s+/g, ' ').trim().slice(0, 200);
    }

    function getImageUrl(product) {
        const groups = [product.img_Original, product.img_Medium, product.img_Small];
        for (const group of groups) {
            if (!Array.isArray(group) || group.length === 0) continue;
            const link = Array.isArray(group[0].link) ? group[0].link[0] : group[0].link;
            if (link) return new URL(link, window.location.origin).href;
        }
        return '';
    }

    function scrollToImmediately(top) {
        const elements = [document.documentElement, document.body];
        const previousStyles = elements.map((element) => ({
            value: element.style.getPropertyValue('scroll-behavior'),
            priority: element.style.getPropertyPriority('scroll-behavior')
        }));

        elements.forEach((element) => element.style.setProperty('scroll-behavior', 'auto', 'important'));
        window.scrollTo(0, top);
        elements.forEach((element, index) => {
            const previous = previousStyles[index];
            if (previous.value) {
                element.style.setProperty('scroll-behavior', previous.value, previous.priority);
            } else {
                element.style.removeProperty('scroll-behavior');
            }
        });
    }

    function getTopOverlayBottom() {
        let bottom = 0;

        $('header:visible, header > nav:visible').each(function () {
            const rootRect = this.getBoundingClientRect();
            const rootStyle = window.getComputedStyle(this);
            const isTopOverlay = rootStyle.position === 'fixed'
                || rootStyle.position === 'sticky'
                || (rootRect.top <= 1 && rootRect.bottom > 0);

            if (!isTopOverlay) return;

            $(this).find('*').addBack().each(function () {
                const rect = this.getBoundingClientRect();
                const style = window.getComputedStyle(this);
                const minimumLayerWidth = Math.max(120, window.innerWidth * 0.45);

                if (style.display === 'none'
                    || style.visibility === 'hidden'
                    || Number(style.opacity) === 0
                    || rect.height <= 0
                    || rect.width < minimumLayerWidth
                    || rect.bottom <= 0
                    || rect.top >= window.innerHeight
                    || rect.right <= 0
                    || rect.left >= window.innerWidth) {
                    return;
                }

                bottom = Math.max(bottom, rect.bottom);
            });
        });

        return Math.max(0, Math.min(bottom, window.innerHeight));
    }

    function keepElementBelowHeader($element, padding) {
        if (!$element.length) return;

        const minimumTop = getTopOverlayBottom() + padding;
        const actualTop = $element[0].getBoundingClientRect().top;
        if (actualTop < minimumTop) {
            scrollToImmediately(Math.max(0, window.scrollY - (minimumTop - actualTop)));
        }
    }

    Controller.prototype.getProductDataRequest = function (productId) {
        this.productDataRequests = this.productDataRequests || {};
        const cacheKey = String(productId);
        if (this.productDataRequests[cacheKey]) return this.productDataRequests[cacheKey];

        const request = this.options.api.getMainDisplay(productId);
        this.productDataRequests[cacheKey] = request;
        request.fail(() => {
            if (this.productDataRequests[cacheKey] === request) {
                delete this.productDataRequests[cacheKey];
            }
        });
        return request;
    };

    Controller.prototype.showProductNavigationPreview = function ($link) {
        if ($link.hasClass('disabled')) return;

        const targetUrl = new URL($link.attr('href'), window.location.href);
        const productId = getProductIdFromUrl(targetUrl);
        if (!productId) return;

        const linkTitle = $link.attr('title') || $link.data('previewTitle') || '';
        const directionText = $link.children('.translate').first().text().trim();
        if (linkTitle) {
            $link.data('previewTitle', linkTitle)
                .attr('aria-label', directionText ? `${directionText}：${linkTitle}` : linkTitle)
                .removeAttr('title');
        }

        let $preview = $link.children('.product-switch-preview');
        if (!$preview.length) {
            $preview = $('<span>', {
                class: 'product-switch-preview',
                'aria-hidden': 'true'
            }).append(
                $('<span>', { class: 'product-switch-preview__image is-loading' }),
                $('<span>', { class: 'product-switch-preview__title' }).text(linkTitle)
            );
            $link.append($preview);
        }

        if (String($preview.data('productId')) === String(productId)) return;
        $preview.data('productId', productId);
        $preview.find('.product-switch-preview__title').text(linkTitle);

        const request = this.getProductDataRequest(productId);
        request.done((product) => {
            if (String($preview.data('productId')) !== String(productId) || !product) return;

            const title = product.title || linkTitle;
            const imageUrl = getImageUrl(product);
            const $image = $preview.find('.product-switch-preview__image')
                .removeClass('is-loading is-empty')
                .empty();

            $preview.find('.product-switch-preview__title').text(title);
            $link.data('previewTitle', title)
                .attr('aria-label', directionText ? `${directionText}：${title}` : title);
            if (imageUrl) {
                $preview.removeClass('is-text-only');
                $image.append($('<img>', { src: imageUrl, alt: '', loading: 'lazy' }).on('error', () => {
                    $preview.addClass('is-text-only');
                    $image.addClass('is-empty').empty();
                }));
            } else {
                $preview.addClass('is-text-only');
                $image.addClass('is-empty');
            }
        });
        request.fail(() => {
            if (String($preview.data('productId')) !== String(productId)) return;
            $preview.addClass('is-text-only');
            $preview.find('.product-switch-preview__image')
                .removeClass('is-loading')
                .addClass('is-empty');
        });
    };

    Controller.prototype.clearKeyboardNavigation = function (options) {
        const settings = $.extend({
            preserveLastPress: true
        }, options || {});

        if (this.keyboardNavigationTimer) {
            window.clearInterval(this.keyboardNavigationTimer);
            this.keyboardNavigationTimer = null;
        }

        if (this.keyboardNavigationTimeout) {
            window.clearTimeout(this.keyboardNavigationTimeout);
            this.keyboardNavigationTimeout = null;
        }

        this.keyboardNavigationKey = null;
        this.keyboardNavigationStartedAt = 0;
        this.keyboardNavigationIsDown = false;
        this.keyboardNavigationBoundaryIsDown = false;
        this.keyboardNavigationBoundaryKey = null;

        if (!settings.preserveLastPress) {
            this.keyboardNavigationLastPress = 0;
            this.keyboardNavigationLastKey = null;
        }

        $('.product-keyboard-navigation')
            .removeClass('show')
            .remove();
    };

    Controller.prototype.getKeyboardNavigationLink = function (key) {
        if (key === 'ArrowLeft') {
            return this.$pageRoot.find('#SwitchPage .btn_prev').first();
        }

        if (key === 'ArrowRight') {
            return this.$pageRoot.find('#SwitchPage .btn_next').first();
        }

        return $();
    };

    Controller.prototype.showKeyboardNavigationBoundary = function (key) {
        $('.product-keyboard-navigation').remove();

        const isPrevious = key === 'ArrowLeft';

        const $notice = $('<div>', {
            class: 'product-keyboard-navigation ' + (isPrevious ? 'is-prev' : 'is-next'),
            'aria-hidden': 'true'
        }).append(
            $('<div>', {
                class: 'product-keyboard-navigation__content'
            }).append(
                $('<div>', {
                    class: 'product-keyboard-navigation__direction'
                }).text(isPrevious ? '上一則' : '下一則'),

                $('<div>', {
                    class: 'product-keyboard-navigation__title'
                }).text(isPrevious ? '已經是第一則' : '已經是最後一則')
            )
        );

        $('body').append($notice);

        window.requestAnimationFrame(() => {
            $notice.addClass('show');
        });

        window.setTimeout(() => {
            $notice.removeClass('show');

            window.setTimeout(() => {
                $notice.remove();
            }, 200);
        }, 1200);
    };

    Controller.prototype.showKeyboardNavigation = function ($link, key, remainingSeconds) {
        let $notice = $('.product-keyboard-navigation');

        if (!$notice.length) {
            $notice = $('<div>', {
                class: 'product-keyboard-navigation',
                'aria-hidden': 'true'
            }).append(
                $('<div>', {
                    class: 'product-keyboard-navigation__image'
                }),
                $('<div>', {
                    class: 'product-keyboard-navigation__content'
                }).append(
                    $('<div>', {
                        class: 'product-keyboard-navigation__direction'
                    }),
                    $('<div>', {
                        class: 'product-keyboard-navigation__countdown'
                    }),
                    $('<div>', {
                        class: 'product-keyboard-navigation__label',
                        text: '即將前往'
                    }),
                    $('<div>', {
                        class: 'product-keyboard-navigation__title'
                    })
                )
            );

            $('body').append($notice);
        }

        const isPrevious = key === 'ArrowLeft';

        const title =
            $link.data('previewTitle') ||
            $link.attr('title') ||
            '';

        $notice
            .removeClass('is-prev is-next')
            .addClass(isPrevious ? 'is-prev' : 'is-next');

        $notice
            .find('.product-keyboard-navigation__direction')
            .text(isPrevious ? '上一則' : '下一則');

        $notice
            .find('.product-keyboard-navigation__countdown')
            .text(remainingSeconds);

        $notice
            .find('.product-keyboard-navigation__title')
            .text(title);

        window.requestAnimationFrame(() => {
            $notice.addClass('show');
        });
    };

    Controller.prototype.loadKeyboardNavigationPreview = function ($link, key) {
        if (!$link.length || $link.hasClass('disabled')) return;

        const href = $link.attr('href');
        if (!href) return;

        const targetUrl = new URL(href, window.location.href);
        const productId = getProductIdFromUrl(targetUrl);

        if (!productId) return;

        const request = this.getProductDataRequest(productId);

        request.done((product) => {
            if (!product) return;

            // 已經換方向或結束長按，就不要更新舊提示
            if (this.keyboardNavigationKey !== key) return;

            const title =
                product.title ||
                $link.data('previewTitle') ||
                $link.attr('title') ||
                '';

            const imageUrl = getImageUrl(product);

            $link.data('previewTitle', title);

            const $notice = $('.product-keyboard-navigation');

            if (!$notice.length) return;

            $notice
                .find('.product-keyboard-navigation__title')
                .text(title);

            const $image = $notice
                .find('.product-keyboard-navigation__image')
                .empty()
                .removeClass('is-empty');

            if (imageUrl) {
                $('<img>', {
                    src: imageUrl,
                    alt: '',
                    loading: 'eager'
                })
                    .on('error', function () {
                        $image.empty().addClass('is-empty');
                    })
                    .appendTo($image);
            } else {
                $image.addClass('is-empty');
            }
        });
    };

    Controller.prototype.navigateByKeyboard = function ($link) {
        if (!$link.length || $link.hasClass('disabled')) return;

        const href = $link.attr('href');
        if (!href) return;

        const targetUrl = new URL(href, window.location.href);
        const productId = getProductIdFromUrl(targetUrl);

        if (!productId || targetUrl.origin !== window.location.origin) return;

        this.clearKeyboardNavigation({
            preserveLastPress: false
        });

        this.navigateToProduct(productId, targetUrl.href, {
            pushState: true,
            scroll: true
        });
    };

    Controller.prototype.bindNavigation = function () {
        const initialId = Number(this.state.productId);
        const currentState = $.extend({}, window.history.state || {}, { productId: initialId });
        window.history.replaceState(currentState, '', window.location.href);

        const titleMatch = document.title.match(/\s+-\s+【.*】$/);
        this.productTitleSuffix = titleMatch ? titleMatch[0] : '';

        this.$pageRoot
            .off('click.productNavigation', '#SwitchPage .btn_prev, #SwitchPage .btn_next')
            .on('click.productNavigation', '#SwitchPage .btn_prev, #SwitchPage .btn_next', (event) => {
                const $link = $(event.currentTarget);
                if ($link.hasClass('disabled')) return;
                if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

                const targetUrl = new URL($link.attr('href'), window.location.href);
                const productId = getProductIdFromUrl(targetUrl);
                if (!productId || targetUrl.origin !== window.location.origin) return;

                event.preventDefault();
                this.navigateToProduct(productId, targetUrl.href, { pushState: true, scroll: true });
            })
            .off('mouseenter.productNavigation focusin.productNavigation', '#SwitchPage .btn_prev, #SwitchPage .btn_next')
            .on('mouseenter.productNavigation focusin.productNavigation', '#SwitchPage .btn_prev, #SwitchPage .btn_next', (event) => {
                this.showProductNavigationPreview($(event.currentTarget));
            });

        $(window).off('popstate.productNavigation').on('popstate.productNavigation', () => {
            const targetUrl = new URL(window.location.href);
            const productId = getProductIdFromUrl(targetUrl);
            if (!productId || String(productId) === String(this.state.productId)) return;

            this.navigateToProduct(productId, targetUrl.href, { pushState: false, scroll: false });
        });

        const keyboardHoldDuration = 5000;
        const keyboardDoublePressDuration = 400;

        $(document)
            .off('keydown.productKeyboardNavigation')
            .on('keydown.productKeyboardNavigation', (event) => {
                if (
                    event.key !== 'ArrowLeft' &&
                    event.key !== 'ArrowRight'
                ) {
                    return;
                }

                const target = event.target;

                if (
                    target &&
                    (
                        target.tagName === 'INPUT' ||
                        target.tagName === 'TEXTAREA' ||
                        target.tagName === 'SELECT' ||
                        target.isContentEditable
                    )
                ) {
                    return;
                }

                const $link = this.getKeyboardNavigationLink(event.key);

                if (!$link.length) {
                    return;
                }

                event.preventDefault();

                if ($link.hasClass('disabled') || !$link.attr('href')) {
                    // 同一顆方向鍵仍持續按住時，不要重複顯示 boundary
                    if (
                        this.keyboardNavigationBoundaryIsDown &&
                        this.keyboardNavigationBoundaryKey === event.key
                    ) {
                        return;
                    }

                    this.keyboardNavigationBoundaryIsDown = true;
                    this.keyboardNavigationBoundaryKey = event.key;

                    this.showKeyboardNavigationBoundary(event.key);

                    return;
                }

                

                /*
                 * 重點：
                 * 不依賴 event.repeat。
                 *
                 * 一旦某個方向鍵已經處於按住狀態，
                 * 後續所有重複 keydown 全部忽略。
                 */
                if (
                    this.keyboardNavigationIsDown &&
                    this.keyboardNavigationKey === event.key
                ) {
                    return;
                }

                const now = Date.now();

                /*
                 * 快速按兩次：
                 *
                 * 第一次：
                 * keydown -> keyup
                 *
                 * 第二次：
                 * 只看 lastKey / lastPress，
                 * 不看目前正在按住的 keyboardNavigationKey。
                 */
                if (
                    this.keyboardNavigationLastKey === event.key &&
                    this.keyboardNavigationLastPress &&
                    now - this.keyboardNavigationLastPress <= keyboardDoublePressDuration
                ) {
                    this.keyboardNavigationLastKey = null;
                    this.keyboardNavigationLastPress = 0;

                    this.navigateByKeyboard($link);
                    return;
                }

                /*
                 * 如果原本正在按另一個方向，
                 * 先清掉舊的長按狀態。
                 */
                this.clearKeyboardNavigation({
                    preserveLastPress: true
                });

                this.keyboardNavigationLastKey = event.key;
                this.keyboardNavigationLastPress = now;

                this.keyboardNavigationKey = event.key;
                this.keyboardNavigationIsDown = true;
                this.keyboardNavigationStartedAt = now;

                // 先顯示 5 秒
                this.showKeyboardNavigation(
                    $link,
                    event.key,
                    5
                );

                // 非同步抓完整商品標題 + 圖片
                this.loadKeyboardNavigationPreview(
                    $link,
                    event.key
                );

                /*
                 * 每 100ms 更新一次，
                 * 但畫面顯示整數秒：
                 *
                 * 5 -> 4 -> 3 -> 2 -> 1 -> 切換
                 */
                this.keyboardNavigationTimer = window.setInterval(() => {
                    if (
                        !this.keyboardNavigationIsDown ||
                        this.keyboardNavigationKey !== event.key
                    ) {
                        return;
                    }

                    const elapsed =
                        Date.now() -
                        this.keyboardNavigationStartedAt;

                    const remaining = Math.max(
                        1,
                        Math.ceil(
                            (keyboardHoldDuration - elapsed) / 1000
                        )
                    );

                    this.showKeyboardNavigation(
                        $link,
                        event.key,
                        remaining
                    );
                }, 100);

                /*
                 * 按滿 5 秒才真正切換。
                 */
                this.keyboardNavigationTimeout = window.setTimeout(() => {
                    if (
                        !this.keyboardNavigationIsDown ||
                        this.keyboardNavigationKey !== event.key
                    ) {
                        return;
                    }

                    this.navigateByKeyboard($link);

                }, keyboardHoldDuration);
            });

        $(document)
            .off('keyup.productKeyboardNavigation')
            .on('keyup.productKeyboardNavigation', (event) => {
                if (
                    event.key !== 'ArrowLeft' &&
                    event.key !== 'ArrowRight'
                ) {
                    return;
                }

                // 先清掉 boundary 按住狀態
                if (
                    this.keyboardNavigationBoundaryIsDown &&
                    this.keyboardNavigationBoundaryKey === event.key
                ) {
                    this.keyboardNavigationBoundaryIsDown = false;
                    this.keyboardNavigationBoundaryKey = null;

                    return;
                }

                // 正常商品切換的長按狀態
                if (
                    !this.keyboardNavigationIsDown ||
                    this.keyboardNavigationKey !== event.key
                ) {
                    return;
                }

                this.clearKeyboardNavigation({
                    preserveLastPress: true
                });
            });

        $(window)
            .off('blur.productKeyboardNavigation')
            .on('blur.productKeyboardNavigation', () => {
                this.clearKeyboardNavigation({
                    preserveLastPress: false
                });
            });
    };

    Controller.prototype.navigateToProduct = function (productId, url, options) {
        const settings = $.extend({ pushState: true, scroll: true }, options || {});
        if (!productId || String(productId) === String(this.state.productId)) return null;

        if (this.pendingRequest && typeof this.pendingRequest.abort === 'function') {
            this.pendingRequest.abort();
        }

        if (settings.pushState) {
            window.history.pushState({ productId: productId }, '', url);
        }

        this.pendingNavigation = { productId: productId, url: url, scroll: settings.scroll };
        this.state.productId = productId;
        this.state.product = null;
        this.state.selection = null;
        window.PageId = productId;

        this.resetProductNavigationState();
        this.logClick();

        const request = this.load();
        if (request && typeof request.fail === 'function') {
            request.fail((xhr, status) => {
                if (status === 'abort') return;
                if (this.pendingNavigation && String(this.pendingNavigation.productId) === String(productId)) {
                    window.location.href = url;
                }
            });
        }
        return request;
    };

    Controller.prototype.resetProductNavigationState = function () {
        this.clearKeyboardNavigation({
            preserveLastPress: false
        });
        this.$root.attr('aria-busy', 'true');
        this.$pageRoot.find(this.options.selectors.favoritesButton)
            .removeClass('turn')
            .removeData('Fid');
        this.$pageRoot.find('#SwitchPage .btn_prev,#SwitchPage .btn_next')
            .addClass('disabled')
            .attr({ href: '', title: '' })
            .removeAttr('aria-label')
            .removeData('previewTitle')
            .children('.product-switch-preview').remove();
        const shareHref = window.location.pathname;
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
        if (window.ProductAddOnPurchase && typeof window.ProductAddOnPurchase.load === 'function') {
            window.ProductAddOnPurchase.load(this.state.productId);
        }
    };

    Controller.prototype.afterProductNavigation = function () {
        const navigation = this.pendingNavigation;
        if (!navigation || String(navigation.productId) !== String(this.state.productId)) return;

        const product = this.state.product || {};
        const title = product.title || '';
        const description = getDescription(product);
        const imageUrl = getImageUrl(product);
        const canonicalUrl = `${window.location.origin}/${this.options.orgName}/search/product/${this.state.productId}`;

        document.title = `${title}${this.productTitleSuffix || ''}`;
        setMetaContent('meta[name="description"]', description);
        setMetaContent('meta[itemprop="name"]', document.title);
        setMetaContent('meta[itemprop="description"]', description);
        setMetaContent('meta[name="twitter:title"]', document.title);
        setMetaContent('meta[name="twitter:description"]', description);
        setMetaContent('meta[property="og:title"]', document.title);
        setMetaContent('meta[property="og:url"]', window.location.href);
        setMetaContent('meta[property="og:description"]', description);
        setMetaContent('meta[name="twitter:image"]', imageUrl);
        setMetaContent('meta[property="og:image"]', imageUrl);

        const canonical = document.querySelector('link[rel="canonical"]');
        if (canonical) canonical.setAttribute('href', canonicalUrl);

        const $breadcrumbCurrent = this.$pageRoot.find('.top_title_1 .bread li').last();
        if ($breadcrumbCurrent.length) {
            $breadcrumbCurrent.text(title).attr('title', title);
        }

        this.$root.removeAttr('aria-busy');
        this.pendingNavigation = null;

        if (navigation.scroll) {
            const $main = this.$pageRoot.find(this.options.selectors.mainContent).first();
            const $title = this.$root.find(this.options.selectors.title).first();
            const topPadding = 24;
            if ($main.length) {
                $('html, body').stop(true);
                scrollToImmediately(Math.max(0, $main.offset().top - getTopOverlayBottom() - topPadding));

                // 等瀏覽器完成本次商品 DOM 排版後再校正一次，確保標題沒有被 sticky/fixed Header 蓋住。
                window.requestAnimationFrame(() => {
                    window.requestAnimationFrame(() => keepElementBelowHeader($title, topPadding));
                });
            }
        }
    };

})(window, window.jQuery);
