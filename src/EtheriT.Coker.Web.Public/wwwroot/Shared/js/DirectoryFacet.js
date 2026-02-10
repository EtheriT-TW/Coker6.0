//# sourceURL=DirectoryFacet.js
(function (w, $) {
    "use strict";

    // --- Guard ---
    if (!$) return;

    var DirectoryFacet = (w.DirectoryFacet = w.DirectoryFacet || {});

    // ===== Settings (可由 DirectoryFacet.configure 覆寫) =====
    var settings = {
        frameSelector: ".catalog_frame",

        // Facet UI 插入位置（frame 內）
        facetContainerSelector: "[data-dir-facet-container], .dir_facet_container",

        // Menu 切換用（可自行呼叫 bindMenuList）
        menuItemSelector: "[data-dirid]",

        // 預設是否 DOMReady 自動 init
        autoInit: true,

        // 是否快取 facet options（以 dirId 為 key）
        enableCache: true,

        // Facet 儲存到 frame 的 data key
        facetDataKey: "facet",

        // Facet 清單載入中 flag
        facetLoadingKey: "facetLoading",

        // Facet 的「全部」按鈕
        showAllItem: true,
        allText: "全部",

        // 觸發目錄內容重載方式：
        // 1) 優先呼叫 initElemntAndLoadDir($frame, "1")
        // 2) 若不存在，fallback 用 trigger("filter")
        reloadContent: function ($frame) {
            if (typeof w.initElemntAndLoadDir === "function") {
                $frame.removeData("page"); // 重置分頁
                w.initElemntAndLoadDir($frame, "1");
                return;
            }
            $frame.trigger("filter");
        },

        // 你後端 facet API 的 request payload 組法（走 Coker.Directory.getFacet）
        buildFacetRequest: function ($frame) {
            // dirid 可能是 "1,2,3" 或 "xxx-6u3" 這種字串
            var dirRaw = getDirId($frame);
            var dirIds = normalizeDirId(dirRaw);

            return {
                DirectoryId: dirIds && dirIds.length === 1 ? dirIds[0] : (dirIds || dirRaw),
                SiteId: getSiteId(),
                CalendarType: $frame.data("calendarType") // 可無
            };
        },

        // Facet options response 解析（容錯 Success/success + Items/items）
        normalizeFacetResponse: function (resp) {
            var ok = resp && (resp.success === true || resp.Success === true);
            var items = (resp && (resp.items || resp.Items)) || [];
            if (!ok) items = [];

            // items 期望格式：
            // { key, text, type, count, payload }
            // 若你後端回來字段不同，可以在這裡做映射
            return items;
        },

        // Facet item UI 產生（可自行改成 template / badge 等）
        renderItem: function (item, isActive) {
            var key = item && item.key != null ? String(item.key) : "";
            var text = item && (item.text != null ? String(item.text) : key);
            var count = (item && typeof item.count === "number") ? item.count : null;

            var cls = "btn btn-sm me-2 mb-2 dir-facet-item " + (isActive ? "btn-primary active" : "btn-outline-secondary");
            var countHtml = (count != null) ? '<span class="ms-2 text-black-50">(' + count + ')</span>' : "";

            return (
                '<button type="button"' +
                ' class="' + cls + '"' +
                ' data-facet-key="' + escapeAttr(key) + '"' +
                ' data-facet-type="' + escapeAttr(item.type || "") + '"' +
                '>' +
                escapeHtml(text) +
                countHtml +
                '</button>'
            );
        },

        // 「全部」按鈕 UI
        renderAllItem: function (type, isActive) {
            var cls = "btn btn-sm me-2 mb-2 dir-facet-item dir-facet-all " + (isActive ? "btn-primary active" : "btn-outline-secondary");
            return (
                '<button type="button"' +
                ' class="' + cls + '"' +
                ' data-facet-all="1"' +
                ' data-facet-type="' + escapeAttr(type || "") + '"' +
                '>' +
                escapeHtml(settings.allText) +
                '</button>'
            );
        }
    };

    // ===== Cache =====
    var cache = {
        optionsByDirId: {} // dirId(string) => items[]
    };

    // ===== Public API =====

    /**
     * 覆寫設定
     */
    DirectoryFacet.configure = function (opt) {
        if (!opt || typeof opt !== "object") return;
        Object.keys(opt).forEach(function (k) {
            settings[k] = opt[k];
        });
    };

    /**
     * 初始化：對 scope 內所有 frame 掛 facet reload、並首次載入 facet
     */
    DirectoryFacet.init = function (scope) {
        var $scope = scope ? $(scope) : $(document);
        var $frames = $scope.find(settings.frameSelector);

        $frames.each(function () {
            var $frame = $(this);
            if ($frame.data("facetInit")) return;
            $frame.data("facetInit", true);

            // facet:reload 事件（menu 切換 dirid 或外部要求重撈）
            $frame.off("facet:reload.dirFacet").on("facet:reload.dirFacet", function (e, force) {
                DirectoryFacet.load($frame, !!force);
            });

            // facet item click（委派）
            $frame.off("click.dirFacet").on("click.dirFacet", ".dir-facet-item", function (e) {
                e.preventDefault();

                var $btn = $(this);
                // 全部
                if ($btn.attr("data-facet-all") === "1" || $btn.data("facet-all") === 1) {
                    DirectoryFacet.clear($frame, {
                        type: String($btn.data("facet-type") || "")
                    });
                    return;
                }

                // 套用某個 facet
                var key = String($btn.data("facet-key") || "");
                var items = $frame.data("facetOptions") || [];
                var facet = findFacetByKey(items, key);
                if (!facet) return;

                DirectoryFacet.apply($frame, facet);
            });

            // 首次載入 facet（不自動載內容，避免干擾你原本初始化節奏）
            DirectoryFacet.load($frame, false);
        });
    };

    /**
     * 載入 facet options 並 render（只處理分類 UI）
     * @param {jQuery} $frame
     * @param {boolean} forceReload
     */
    DirectoryFacet.load = function ($frame, forceReload) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return $.Deferred().reject("no frame").promise();

        var dirId = String(getDirId($frame) || "");
        if (!dirId) {
            renderFacet($frame, []);
            return $.Deferred().resolve({ success: true, items: [] }).promise();
        }

        // container required
        var $ct = resolveFacetContainer($frame);
        if (!$ct.length) {
            // 不自動建立，避免插錯位置；你要就自己在版型放容器
            return $.Deferred().resolve({ success: true, items: [] }).promise();
        }

        // cache
        if (settings.enableCache && !forceReload && cache.optionsByDirId[dirId]) {
            var cached = cache.optionsByDirId[dirId];
            $frame.data("facetOptions", cached);
            renderFacet($frame, cached);
            return $.Deferred().resolve({ success: true, items: cached }).promise();
        }

        // loading guard
        if ($frame.data(settings.facetLoadingKey)) return $.Deferred().reject("loading").promise();
        $frame.data(settings.facetLoadingKey, true);

        // build request
        var payload = settings.buildFacetRequest($frame);

        // call Coker API (no URL here)
        if (!w.Coker || !Coker.Directory || typeof Coker.Directory.getFacet !== "function") {
            $frame.data(settings.facetLoadingKey, false);
            return $.Deferred().reject("Coker.Directory.getFacet not found").promise();
        }

        return Coker.Directory.getFacet(payload)
            .done(function (resp) {
                var items = settings.normalizeFacetResponse(resp) || [];
                $frame.data("facetOptions", items);

                if (settings.enableCache) {
                    cache.optionsByDirId[dirId] = items;
                }

                renderFacet($frame, items);
            })
            .fail(function () {
                renderFacet($frame, []);
            })
            .always(function () {
                $frame.data(settings.facetLoadingKey, false);
            });
    };

    /**
     * 套用 facet：存 data + 更新 active + 觸發內容重載
     * @param {jQuery} $frame
     * @param {object} facet
     */
    DirectoryFacet.apply = function ($frame, facet) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return;

        $frame.data(settings.facetDataKey, facet);

        setActiveByKey($frame, facet && facet.key != null ? String(facet.key) : "");

        // 提供外部 hook（例如你下一步要把 facet 合併進 filtered 再送出）
        $frame.trigger("facet:change", [facet]);

        // 內容重載（走你既有流程）
        settings.reloadContent($frame);
    };

    /**
     * 清除 facet（可指定只清某 type；目前先做全清/同 type 清）
     */
    DirectoryFacet.clear = function ($frame, opt) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return;

        opt = opt || {};
        var cur = $frame.data(settings.facetDataKey);

        // 若指定 type，且目前 type 不一致，則不動作
        if (opt.type && cur && String(cur.type || "") !== String(opt.type)) return;

        $frame.removeData(settings.facetDataKey);
        setActiveByKey($frame, ""); // active 回到 all

        $frame.trigger("facet:change", [null]);
        settings.reloadContent($frame);
    };

    /**
     * menuList 綁定：點 menu item (data-dirid) => 切換共用 frame dirid => reload facet + reload content
     * @param {string} menuSelector
     * @param {string} frameSelector
     */
    DirectoryFacet.bindMenuList = function (menuSelector, frameSelector) {
        var $menu = $(menuSelector || ".menuList");
        var $frame = frameSelector ? $(frameSelector).first() : $(settings.frameSelector).first();
        if (!$menu.length || !$frame.length) return;

        $menu.off("click.dirFacetMenu").on("click.dirFacetMenu", settings.menuItemSelector, function (e) {
            e.preventDefault();

            var $it = $(this);
            var dirid = $it.data("dirid");
            if (dirid == null || dirid === "") dirid = $it.attr("data-dirid");
            if (dirid == null || dirid === "") return;

            DirectoryFacet.switchDirectory($frame, String(dirid));
        });
    };

    /**
     * 切換共用 frame 的 dirid（你說的情境：同一 frame，只改 dirid）
     * - 清除 page & facet
     * - facet:reload (force)
     * - reload content
     */
    DirectoryFacet.switchDirectory = function ($frame, newDirId) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return;

        newDirId = String(newDirId || "");
        if (!newDirId) return;

        // 更新 dirid（字串安全）
        $frame.attr("data-dirid", newDirId);
        $frame.data("dirid", newDirId);

        // 清 page + facet（避免上個目錄殘留）
        $frame.removeData("page");
        $frame.removeData(settings.facetDataKey);

        // 也清掉 filtered 內 facet（如果你未來把 facet 寫入 filtered）
        // 這段不會破壞你原本過濾器，只清 filtered.facet
        var filtered = $frame.data("filtered");
        filtered = normalizeFiltered(filtered);
        if (filtered && typeof filtered === "object") {
            delete filtered.facet;
            $frame.data("filtered", filtered);
        }

        // 重撈 facet options（force reload，避免 cache 誤用）
        $frame.trigger("facet:reload", [true]);

        // 重載內容（先不等 facet，讓內容先出；若你要等 facet 完再 reload，可改成 load().always 後呼叫）
        settings.reloadContent($frame);
    };

    // ===== Internals =====

    function getSiteId() {
        return (typeof w.SiteId === "undefined") ? 0 : w.SiteId;
    }

    function getDirId($frame) {
        var v = $frame.data("dirid");
        if (v == null || v === "") v = $frame.attr("data-dirid");
        return v != null ? String(v) : "";
    }

    function normalizeDirId(raw) {
        if (raw == null) return null;
        if (Array.isArray(raw)) return raw.map(function (x) { return String(x); });

        var s = String(raw);
        // 若是 "1,2,3"
        if (s.indexOf(",") >= 0) {
            var arr = s.split(",").map(function (x) { return x.trim(); }).filter(Boolean);
            return arr.length ? arr : null;
        }
        return [s];
    }

    function normalizeFiltered(v) {
        if (!v) return {};
        if (typeof v === "object") return v;
        if (typeof v === "string") {
            try {
                var o = JSON.parse(v);
                if (o && typeof o === "object") return o;
            } catch (_) { }
        }
        return {};
    }

    function resolveFacetContainer($frame) {
        return $frame.find(settings.facetContainerSelector).first();
    }

    function escapeHtml(str) {
        str = (str == null) ? "" : String(str);
        return str
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function escapeAttr(str) {
        return escapeHtml(str).replaceAll("`", "&#96;");
    }

    function findFacetByKey(items, key) {
        if (!items || !items.length) return null;
        for (var i = 0; i < items.length; i++) {
            if (String(items[i].key) === String(key)) return items[i];
        }
        return null;
    }

    function renderFacet($frame, items) {
        var $ct = resolveFacetContainer($frame);
        if (!$ct.length) return;

        items = items || [];

        if (!items.length) {
            $ct.empty().addClass("d-none");
            return;
        }

        $ct.removeClass("d-none").empty();

        var cur = $frame.data(settings.facetDataKey);
        var curKey = (cur && cur.key != null) ? String(cur.key) : "";

        var html = "";

        // 依 type 分組（如果你的 facet response 有 type；沒有就當成單組）
        var groups = groupByType(items);

        Object.keys(groups).forEach(function (type) {
            // group block
            html += '<div class="dir-facet-group mb-2" data-facet-group="' + escapeAttr(type) + '">';

            // all item
            if (settings.showAllItem) {
                var allActive = !(cur && String(cur.type || "") === String(type));
                html += settings.renderAllItem(type, allActive);
            }

            // items
            groups[type].forEach(function (it) {
                var active = (cur && String(cur.type || "") === String(type) && String(it.key) === curKey);
                html += settings.renderItem(it, active);
            });

            html += "</div>";
        });

        $ct.append(html);
    }

    function groupByType(items) {
        var map = {};
        for (var i = 0; i < items.length; i++) {
            var t = items[i].type != null ? String(items[i].type) : "";
            if (!map[t]) map[t] = [];
            map[t].push(items[i]);
        }
        return map;
    }

    function setActiveByKey($frame, key) {
        var $ct = resolveFacetContainer($frame);
        if (!$ct.length) return;

        $ct.find(".dir-facet-item").removeClass("active btn-primary").addClass("btn-outline-secondary");
        if (!key) {
            // active all (each group)
            $ct.find(".dir-facet-all").each(function () {
                var $b = $(this);
                $b.addClass("active btn-primary").removeClass("btn-outline-secondary");
            });
            return;
        }

        $ct.find(".dir-facet-item").each(function () {
            var $b = $(this);
            if (String($b.data("facet-key") || "") === String(key)) {
                $b.addClass("active btn-primary").removeClass("btn-outline-secondary");
            }
        });
    }

    // ===== Auto init =====
    $(function () {
        if (!settings.autoInit) return;
        DirectoryFacet.init(document);
    });

})(window, window.jQuery);