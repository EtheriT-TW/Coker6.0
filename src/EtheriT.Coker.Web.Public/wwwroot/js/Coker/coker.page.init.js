/* coker.page.init.js
 * Page/Block feature dispatcher (root-scoped, re-entrant)
 * - for GrapesJS / dynamic HTML insert scenarios
 * - call: Coker.page.init(root)  // root can be DOM/jQuery/selector
 *
 * Dependencies: jQuery, and your existing feature init functions (DirectoryGetDataInit, SwiperInit, ...)
 */
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    // If you already use module-guard, keep consistent with your core. :contentReference[oaicite:0]{index=0}
    if (typeof Coker.defineModule === "function") {
        Coker.defineModule("page-init", factory);
    } else {
        factory(Coker);
    }

    function factory(C) {
        var $ = w.jQuery;
        if (!$) {
            // Hard fail is usually better here: this file is meaningless without jQuery.
            throw new Error("[coker.page.init] jQuery is required.");
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        function to$root(root) {
            if (!root) return $(document);
            if (root.jquery) return root;
            return $(root);
        }

        function isFn(fn) {
            return typeof fn === "function";
        }

        // Per-feature per-root guard (use data-flag on root element)
        function alreadyInited($root, key) {
            // For document, pick documentElement as the anchor for data-flag
            var $anchor = ($root[0] === document) ? $(document.documentElement) : $root;
            return !!$anchor.data(key);
        }
        function markInited($root, key) {
            var $anchor = ($root[0] === document) ? $(document.documentElement) : $root;
            $anchor.data(key, true);
        }

        // Root-scoped query: match root itself OR its descendants
        function has($root, selector) {
            try {
                return $root.is(selector) || $root.find(selector).length > 0;
            } catch (_) {
                // selector invalid (should not happen, but keep dispatcher resilient)
                return false;
            }
        }

        // Safe invoke: do not break whole boot if one feature dies
        function safeRun(name, fn) {
            try {
                fn();
            } catch (err) {
                // keep your own logger if you have one
                // eslint-disable-next-line no-console
                console.error("[coker.page.init] feature failed:", name, err);
            }
        }

        // -----------------------------
        // Feature registry
        // -----------------------------
        // NOTE:
        // - test($root) MUST be root-scoped (use has())
        // - run($root) SHOULD also be root-scoped (best), but we keep compatibility with your existing global inits.
        var features = [
            {
                name: "directoryGetData",
                key: "coker_inited_directoryGetData",
                test: function ($root) {
                    return has($root, ".catalog_frame, .menu_directory, .advertise_directory");
                },
                run: function ($root) {
                    if (isFn(w.DirectoryGetDataInit)) w.DirectoryGetDataInit($root);
                }
            },
            {
                name: "swiper",
                key: "coker_inited_swiper",
                test: function ($root) {
                    return has(
                        $root,
                        ".one_swiper,.one_swiper_thumbs,.two_swiper,.three_swiper,.four_swiper,.five_swiper,.six_swiper,.picture-category,.three_two_grid_swiper,.vertical_swiper_thumbs"
                    );
                },
                run: function ($root) {
                    if (isFn(w.SwiperInit)) w.SwiperInit($root);
                }
            },
            {
                name: "marqueeSwiper",
                key: "coker_inited_marqueeSwiper",
                test: function ($root) {
                    return has($root, ".marqueeSwiper");
                },
                run: function () {
                    // Your existing code looks like MarqueeSwiper(SiteId)
                    if (isFn(w.MarqueeSwiper)) w.MarqueeSwiper(w.SiteId);
                }
            },
            {
                name: "frameInit",
                key: "coker_inited_frameInit",
                test: function ($root) {
                    return has($root, ".masonry, .YTmodal_frame");
                },
                run: function ($root) {
                    if (isFn(w.FrameInit)) w.FrameInit($root);
                }
            },
            {
                name: "viewTypeChange",
                key: "coker_inited_viewTypeChange",
                test: function ($root) {
                    return has($root, ".type_change_frame");
                },
                run: function ($root) {
                    if (isFn(w.ViewTypeChangeInit)) w.ViewTypeChangeInit($root);
                }
            },
            {
                name: "hoverMask",
                key: "coker_inited_hoverMask",
                test: function ($root) {
                    return has($root, ".hover_mask");
                },
                run: function ($root) {
                    if (isFn(w.HoverEffectInit)) w.HoverEffectInit($root);
                }
            },
            {
                name: "sitemap",
                key: "coker_inited_sitemap",
                test: function ($root) {
                    return has($root, ".sitemap_hierarchical_frame");
                },
                run: function ($root) {
                    if (isFn(w.SiteMapInit)) w.SiteMapInit($root);
                }
            },
            {
                name: "linkWithIcon",
                key: "coker_inited_linkWithIcon",
                test: function ($root) {
                    return has($root, ".link_with_icon");
                },
                run: function ($root) {
                    if (isFn(w.LinkWithIconInit)) w.LinkWithIconInit($root);
                }
            },
            {
                name: "anchorPoint",
                key: "coker_inited_anchorPoint",
                test: function ($root) {
                    return has($root, ".anchor_directory, .anchor_title");
                },
                run: function ($root) {
                    if (isFn(w.AnchorPointInit)) w.AnchorPointInit($root);
                }
            },
            {
                name: "shareBlock",
                key: "coker_inited_shareBlock",
                test: function ($root) {
                    return has($root, ".shareBlock");
                },
                run: function ($root) {
                    if (isFn(w.ShareBlockInit)) w.ShareBlockInit($root);
                }
            },
            {
                name: "flipTimer",
                key: "coker_inited_flipTimer",
                test: function ($root) {
                    return has($root, ".flipdown");
                },
                run: function ($root) {
                    if (isFn(w.FlipTimer)) w.FlipTimer($root);
                }
            },
            {
                name: "articleTags",
                key: "coker_inited_articleTags",
                test: function ($root) {
                    // keep your original logic concept: #conten.article + .article-tags
                    // root-scoped:
                    var $conten = $root.find("#conten");
                    if ($root.is("#conten")) $conten = $root;
                    return $conten.length > 0 && $conten.hasClass("article") && has($root, ".article-tags");
                },
                run: function ($root) {
                    if (isFn(w.ArticleTagsInit)) w.ArticleTagsInit($root);
                }
            },
            {
                name: "contactForm",
                key: "coker_inited_contactForm",
                test: function ($root) {
                    return has($root, ".ContactForm");
                },
                run: function ($root) {
                    // your code looked like setContact(); // From 表單驗證碼
                    if (isFn(w.setContact)) w.setContact($root);
                }
            },
            {
                name: "bgCanvas",
                key: "coker_inited_bgCanvas",
                test: function ($root) {
                    return has($root, ".BGCanvas");
                },
                run: function ($root) {
                    if (isFn(w.setBGCanvas)) w.setBGCanvas($root);
                }
            },
            {
                name: "flipBook",
                key: "coker_inited_flipBook",
                test: function ($root) {
                    return has($root, ".FlipBookItem");
                },
                run: function ($root) {
                    if (isFn(w.FlipBookInit)) w.FlipBookInit($root);
                }
            },
            {
                name: "mapMessage",
                key: "coker_inited_mapMessage",
                test: function ($root) {
                    return has($root, ".MapMessage");
                },
                run: function ($root) {
                    if (isFn(w.MapMessage)) w.MapMessage($root);
                }
            },
            {
                name: "i18nGetLang",
                key: "coker_inited_i18nGetLang",
                test: function ($root) {
                    return has($root, ".getlatlng");
                },
                run: function ($root) {
                    if (isFn(w.GetLang)) w.GetLatLng($root);
                }
            },
            {
                name: "mobileNavbarMove",
                key: "coker_inited_mobileNavbarMove",
                test: function ($root) {
                    // This is inherently "page-level" behavior; only run when root includes body.
                    if (!has($root, "body")) return false;
                    return $("body").width() < 992 && $("#lanBar").length && $("#layout4 #NavbarContent").length;
                },
                run: function () {
                    // Keep original behavior: $("#lanBar").before($("#layout4 #NavbarContent"))
                    $("#lanBar").before($("#layout4 #NavbarContent"));
                }
            }
        ];

        // -----------------------------
        // Public API
        // -----------------------------
        function init(root, options) {
            options = options || {};
            var $root = to$root(root);

            // allow: init({ root, force:true }) style if someone passes object by mistake
            if ($root && $root.root && !$root.jquery) $root = to$root($root.root);

            features.forEach(function (f) {
                if (!f || !f.key || !f.test || !f.run) return;

                // Optional filtering
                if (options.only && Array.isArray(options.only) && options.only.length) {
                    if (options.only.indexOf(f.name) < 0) return;
                }
                if (options.skip && Array.isArray(options.skip) && options.skip.length) {
                    if (options.skip.indexOf(f.name) >= 0) return;
                }

                // Per-root guard unless force
                if (!options.force && alreadyInited($root, f.key)) return;

                // Root-scoped test
                if (!f.test($root)) return;

                safeRun(f.name, function () {
                    f.run($root);
                    markInited($root, f.key);
                });
            });

            return $root;
        }

        // Optional: observe dynamic insertions under a container and auto-init new nodes.
        // You can choose to use it later; default OFF.
        function observe(container, options) {
            options = options || {};
            var $container = to$root(container);
            if (!$container.length) return null;

            var target = $container[0];
            var mo = new MutationObserver(function (mutations) {
                mutations.forEach(function (m) {
                    if (!m.addedNodes || !m.addedNodes.length) return;
                    m.addedNodes.forEach(function (node) {
                        if (node.nodeType !== 1) return; // element only
                        init(node, options);
                    });
                });
            });

            mo.observe(target, { childList: true, subtree: true });
            return mo;
        }

        // Attach to Coker namespace (consistent with your extend pattern) :contentReference[oaicite:1]{index=1}
        if (typeof C.extend === "function") {
            C.extend(
                {
                    page: {
                        init: init,
                        observe: observe,
                        _features: features // for debug only
                    }
                },
                { overwrite: false }
            );
        } else {
            C.page = C.page || {};
            C.page.init = C.page.init || init;
            C.page.observe = C.page.observe || observe;
            C.page._features = C.page._features || features;
        }

        // Auto-run for full page load (keeps backward compatibility)
        $(function () {
            init(document);
        });
    }
})(window);