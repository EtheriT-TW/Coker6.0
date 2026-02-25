/*!
 * jquery.c-share.js v1.2.3 (Final - i18n external, mailto no target, desktop confirm, mobile hint)
 * - i18n accepts translated text OR locale keys (hybrid-safe)
 * - shareToText moved into i18n to keep all text consistent
 * - email subject uses page title (Coker.util.html.getPageTitle > og:title > document.title > hostname)
 * - share URL uses location.origin + settings.href (good for your usage passing pathname)
 */
(function (factory) {
    typeof define === 'function' && define.amd ? define(factory) : factory();
}((function () {
    'use strict';

    if (!$.fn) return;

    $.fn.cShare = function (options) {
        var _this = this;

        var defaults = {
            description: '',
            showButtons: ['line', 'fb', 'twitter'],
            href: '',

            data: {
                fb: {
                    fa: 'fab fa-facebook-f',
                    name: 'Fb',
                    href: function (url) { return "https://www.facebook.com/sharer.php?u=" + url; },
                    show: true
                },
                line: {
                    fa: 'fab fa-line fa-2x',
                    name: 'Line',
                    href: function (url) { return "https://social-plugins.line.me/lineit/share?url=" + url; },
                    show: true,
                    hideWrapper: true
                },
                plurk: {
                    fa: 'fa-plurk',
                    name: 'Plurk',
                    href: function (url, description) { return "http://www.plurk.com/?qualifier=shares&status=" + description + " " + url; },
                    show: false
                },
                weibo: {
                    fa: 'fab fa-weibo',
                    name: '微博',
                    href: function (url, description) { return "http://service.weibo.com/share/share.php?title=" + description + "&url=" + url; },
                    show: false
                },
                twitter: {
                    fa: 'fab fa-twitter',
                    name: 'Twitter',
                    href: function (url, description) {
                        return "https://twitter.com/intent/tweet?original_referer=" + url + "&url=" + url + "&text=" + encodeURIComponent(description || '');
                    },
                    show: false
                },
                tumblr: {
                    fa: 'fab fa-tumblr',
                    name: 'Tumblr',
                    href: function (url, description) { return "http://www.tumblr.com/share/link?name=" + description + " " + url + "&url=" + url; },
                    show: false
                },
                email: {
                    fa: 'fas fa-envelope',
                    name: 'E-mail',
                    // NOTE: real mailto will be generated at click time
                    href: function () { return "mailto:"; },
                    show: false
                }
            },

            spacing: 6,

            // All text should come from i18n (external).
            // Accepts translated strings OR locale keys (hybrid-safe via t()).
            i18n: {
                shareToText: '',

                confirmTitle: '',
                confirmHtml: '',
                okText: '',
                cancelText: '',

                mobileTitle: '',
                mobileText: ''
            },

            email: {
                desktopConfirm: true,
                mobileHint: true,
                // subject override; if empty -> use page title
                subject: '',
                includeDescriptionInBody: true
            }
        };

        // deep merge to keep nested objects
        var settings = $.extend(true, {}, defaults, options || {});

        // -------------------------
        // helpers
        // -------------------------
        function getLocalDict() {
            // user injects: const local = ...
            try { return (typeof window.local !== 'undefined') ? window.local : null; } catch (e) { return null; }
        }

        // Hybrid t(): if input matches a key in window.local -> return translation,
        // otherwise treat input as translated text and return it as-is.
        function t(keyOrText, fallback) {
            if (keyOrText == null || keyOrText === '') return fallback || '';
            var dict = getLocalDict();
            if (dict && Object.prototype.hasOwnProperty.call(dict, keyOrText) && dict[keyOrText]) {
                return dict[keyOrText];
            }
            return String(keyOrText);
        }

        function isMobileDevice() {
            if (window.Coker && Coker.util && Coker.util.device && typeof Coker.util.device.isMobileDevice === 'function') {
                return !!Coker.util.device.isMobileDevice();
            }
            return /(mobile|android|pad)/i.test(navigator.userAgent);
        }

        function getPageTitle() {
            // prefer your shared helper if exists
            if (window.Coker && Coker.util && Coker.util.html && typeof Coker.util.html.getPageTitle === 'function') {
                try {
                    var v = Coker.util.html.getPageTitle();
                    if (v) return String(v).trim();
                } catch (e) { /* ignore */ }
            }

            var og = document.querySelector('meta[property="og:title"]');
            if (og && og.content) return og.content.trim();

            if (document.title) return document.title.trim();

            return location.hostname;
        }

        function normalizePath(href) {
            // settings.href may be "/path" or "path" or full url
            if (!href) return location.pathname;
            var s = String(href).trim();
            if (/^https?:\/\//i.test(s)) return s;
            if (s[0] !== '/') s = '/' + s;
            return location.origin + s;
        }

        function buildShareUrl() {
            // use location.origin + settings.href (fits your usage)
            var full = normalizePath(settings.href || location.pathname);
            if (full.includes("/embed/posts/")) full = full.replace("/embed/posts/", `/${settings.orgName}/search/article/`);
            return encodeURIComponent(full);
        }

        function decodeURIComponentSafe(s) {
            try { return decodeURIComponent(s); } catch (e) { return s; }
        }

        function buildMailto(subject, body) {
            var qs = [];
            if (subject) qs.push('subject=' + encodeURIComponent(subject));
            if (body) qs.push('body=' + encodeURIComponent(body));
            // allow subject-only or body-only; still valid
            return 'mailto:?' + qs.join('&');
        }

        function hasSweetConfirm() {
            return !!(window.Coker && Coker.sweet && typeof Coker.sweet.confirm === 'function');
        }

        function hasSweetInfo() {
            return !!(window.Coker && Coker.ui && Coker.ui.sweet && typeof Coker.ui.sweet.info === 'function');
        }

        function showDesktopEmailConfirm(onOk) {
            var i18n = settings.i18n || {};

            var title = t(i18n.confirmTitle, 'Email');
            var html = t(i18n.confirmHtml, '');
            var okText = t(i18n.okText, 'OK');
            var cancelText = t(i18n.cancelText, 'Cancel');

            if (hasSweetConfirm()) {
                Coker.sweet.confirm(title, html, okText, cancelText, function () {
                    if (typeof onOk === 'function') onOk();
                });
                return;
            }

            // fallback confirm (plain)
            var plain = (title + "\n\n" + String(html).replace(/<br\s*\/?>/ig, '\n').replace(/<[^>]+>/g, ''));
            if (window.confirm(plain)) {
                if (typeof onOk === 'function') onOk();
            }
        }

        function showMobileEmailHint() {
            if (!settings.email || settings.email.mobileHint === false) return;

            var i18n = settings.i18n || {};
            var title = t(i18n.mobileTitle, 'Email');
            var text = t(i18n.mobileText, '');

            if (hasSweetInfo()) {
                Coker.ui.sweet.info(title, text);
                return;
            }

            if (text) window.alert(title + "\n\n" + text);
        }

        // -------------------------
        // build buttons
        // -------------------------
        var shareUrl = buildShareUrl();

        settings.showButtons.forEach(function (shareName) {
            var item = settings.data[shareName];
            if (!item) return;

            var link = item.href.call(null, shareUrl, settings.description);
            var isEmail = (shareName === 'email');

            // mailto must NOT use target blank
            var targetRelAttr = isEmail ? '' : ' target="_blank" rel="noopener noreferrer"';

            var shareToText = t((settings.i18n || {}).shareToText, '');
            var titleAttr = co.util.string.replace(shareToText, item.name);
            _this.append(
                '\n<a href="' + link + '" title="' + titleAttr + '"' +
                targetRelAttr +
                ' data-icon="' + shareName + '">' +
                '<span class="d-none">' + shareName + '</span>' +
                '<span class="fa-stack">' +
                (!item.hideWrapper ? '<i class="fas fa-circle fa-stack-2x"></i>' : '') +
                '<i class="' + item.fa + ' fa-stack-1x"></i>' +
                '</span>' +
                '</a>\n'
            );
        });

        this.find('.fa-plurk').text('P');

        // -------------------------
        // click behavior
        // -------------------------
        var mobile = isMobileDevice();

        this.find('a').on('click', function (e) {
            var $a = $(this);
            var icon = $a.attr('data-icon') || '';
            var href = $a.attr('href') || '';

            // EMAIL
            if (icon === 'email' || href.indexOf('mailto:') === 0) {
                // generate mailto on click to capture latest title
                var pageTitle = (settings.email && settings.email.subject)
                    ? String(settings.email.subject)
                    : getPageTitle();

                var bodyParts = [];
                bodyParts.push(pageTitle);
                bodyParts.push(decodeURIComponentSafe(shareUrl));

                if (settings.email && settings.email.includeDescriptionInBody && settings.description) {
                    bodyParts.push('');
                    bodyParts.push(settings.description);
                }

                var mailto = buildMailto(pageTitle, bodyParts.join('\n'));

                if (mobile) {
                    // mobile: hint but do not block
                    showMobileEmailHint();
                    $a.attr('href', mailto);
                    return; // allow default navigation
                }

                // desktop: confirm first (if enabled)
                if (settings.email && settings.email.desktopConfirm) {
                    e.preventDefault();
                    showDesktopEmailConfirm(function () {
                        window.location.href = mailto;
                    });
                    return;
                }

                // desktop no confirm: open directly (no window.open)
                $a.attr('href', mailto);
                return;
            }

            // NON-EMAIL
            if (!mobile) {
                e.preventDefault();
                window.open(href, '_blank', 'noopener,width=500,height=600');
            }
        });

        // -------------------------
        // styling (keep original vibe)
        // -------------------------
        this.children('a').css({
            display: 'inline-block',
            margin: "auto " + (Number(settings.spacing) / 2) + "px",
            textDecoration: 'none',
            '-webkit-transition': 'all .2s',
            '-moz-transition': 'all .2s',
            transition: 'all .2s'
        });

        if (!mobile) {
            this.children('a').hover(function () {
                $(this).css({ transform: 'translateY(-4px)' });
            }, function () {
                $(this).css({ transform: 'translateY(0px)' });
            });
        }

        // colors (keep original)
        this.find('.fa-stack-1x').css('color', '#ffffff');
        this.find('[data-icon=fb] .fa-stack-2x').css('color', '#3B5998');
        this.find('[data-icon=line] .fa-stack-1x').css('color', '#00c300');

        this.find('[data-icon=plurk] .fa-stack-2x').css('color', '#cf682f');
        this.find('[data-icon=plurk] .fa-plurk').css({
            'font-family': 'arial',
            'font-style': 'normal',
            'font-weight': 'bold'
        });

        this.find('[data-icon=weibo] .fa-stack-2x').css('color', '#F5CA59');
        this.find('[data-icon=twitter] .fa-stack-2x').css('color', '#2ba9e1');
        this.find('[data-icon=tumblr] .fa-stack-2x').css('color', '#35465d');
        this.find('[data-icon=email] .fa-stack-2x').css('color', '#939598');

        return this;
    };

})));