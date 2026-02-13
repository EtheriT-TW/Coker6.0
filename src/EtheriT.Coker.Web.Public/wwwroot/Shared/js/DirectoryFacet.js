//# sourceURL=DirectoryFacet.js
(function (w, $) {
    "use strict";
    if (!$) return;

    var DirectoryFacet = (w.DirectoryFacet = w.DirectoryFacet || {});

    // ======================
    // Settings
    // ======================
    var settings = {
        frameSelector: ".catalog_frame",

        facetContainerSelector: '[data-role="facet-items"]',
        facetItemTemplateSelector: '[data-role="facet-item"]',

        facetDataKey: "facet",
        facetOptionsKey: "facetOptions",
        facetLoadingKey: "facetLoading",

        enableCache: true,

        // 讀取中顯示
        loading: {
            enabled: true,
            text: "載入中…",
            // 簡易 spinner（不吃外部 icon/字型）
            // 你可以用 CSS 覆蓋 .facet-loading / .facet-spinner
            html:
                '<div class="facet-loading" role="status" aria-live="polite" aria-busy="true">' +
                '  <span class="facet-spinner" aria-hidden="true"></span>' +
                '  <span class="facet-loading-text"></span>' +
                '</div>'
        },

        // 空資料/錯誤顯示（可自行調整文案）
        emptyText: "",
        errorText: "載入失敗",

        reloadContent: function ($frame) {
            if (typeof w.initElemntAndLoadDir === "function") {
                $frame.removeData("page").removeData("init");
                $frame.find("[style]").removeAttr("style");
                $frame.find(`.catalog`).empty();
                w.initElemntAndLoadDir($frame, "1");
                return;
            }
            $frame.trigger("filter");
        },

        buildFacetRequest: function ($frame) {
            var dirId = getDirId($frame);
            return { Id: toLong(dirId) };
        },

        normalizeFacetResponse: function (resp) {
            if (!resp) return [];

            var ok =
                resp.Success === true ||
                resp.success === true ||
                (resp.Success == null && resp.success == null);

            if (!ok) return [];

            // 支援 Object / object / Items / items
            var root = resp.Object || resp.object || resp;
            var rawItems =
                (root && (root.Items || root.items)) ||
                resp.Items ||
                resp.items ||
                [];

            if (!rawItems || !rawItems.length) return [];

            var out = [];
            for (var i = 0; i < rawItems.length; i++) {
                var it = rawItems[i] || {};
                var s = toInt(it.start != null ? it.start : it.Start);
                var e = toInt(it.end != null ? it.end : it.End);

                var key = String(s) + "-" + String(e);
                var label = String(it.label != null ? it.label : (it.Label != null ? it.Label : ""));

                out.push({
                    key: key,
                    text: label || key,

                    start: s,
                    end: e,
                    label: label,

                    Start: s,
                    End: e,
                    Label: label
                });
            }

            return out;
        }
    };

    var cache = { optionsByDirId: {} };

    // ======================
    // Public
    // ======================
    DirectoryFacet.configure = function (opt) {
        if (!opt || typeof opt !== "object") return;
        Object.keys(opt).forEach(function (k) { settings[k] = opt[k]; });
    };

    DirectoryFacet.init = function (scope) {
        var $scope = scope ? $(scope) : $(document);
        var $frame = $scope.find(settings.frameSelector).first();
        if (!$frame.length) return;

        if ($frame.data("facetInit")) return;
        $frame.data("facetInit", true);

        clearFacetUI();
    };

    DirectoryFacet.onMenuChanged = function (menuKey, ctx) {
        var $frame = $(settings.frameSelector).first();
        if (!$frame.length) return;

        var dirId = String(menuKey || "");
        if (!dirId) return;

        DirectoryFacet.switchDirectory($frame, dirId, null, ctx);
    };

    DirectoryFacet.onFacetChanged = function (facetValue, ctx) {
        var $frame = $(settings.frameSelector).first();
        if (!$frame.length) return;

        facetValue = String(facetValue || "");
        if (!facetValue) return;

        var items = $frame.data(settings.facetOptionsKey) || [];
        var facet = findFacetByKey(items, facetValue);
        if (!facet) return;

        applyFacet($frame, facet);
    };

    DirectoryFacet.onStateChanged = function (state, ctx) {
        state = state || {};
        var menu = state.menu != null ? String(state.menu) : "";
        var facet = state.facet != null ? String(state.facet) : "";

        var $frame = $(settings.frameSelector).first();
        if (!$frame.length) return;

        if (menu) {
            DirectoryFacet.switchDirectory($frame, menu, facet, ctx);
            return;
        }

        if (facet) DirectoryFacet.onFacetChanged(facet, ctx);
    };

    DirectoryFacet.switchDirectory = function ($frame, newDirId, preferFacetValue, ctx) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return $.Deferred().reject("no frame").promise();

        newDirId = String(newDirId || "");
        if (!newDirId) return $.Deferred().reject("no dirId").promise();

        $frame.attr("data-dirid", newDirId);
        $frame.data("dirid", newDirId);

        $frame.removeData("page");
        $frame.removeData(settings.facetDataKey);
        $frame.removeData(settings.facetOptionsKey);

        // Clear facet snapshot when switching directory
        $frame.removeAttr("data-facet");


        clearFacetUI();

        return DirectoryFacet.load($frame, true)
            .done(function (items) {
                if (!items || !items.length) return;

                var chosen = null;
                if (preferFacetValue) chosen = findFacetByKey(items, preferFacetValue);
                if (!chosen) chosen = items[0];

                applyFacet($frame, chosen);
            });
    };

    DirectoryFacet.load = function ($frame, forceReload) {
        $frame = $frame && $frame.length ? $frame : $(settings.frameSelector).first();
        if (!$frame.length) return $.Deferred().reject("no frame").promise();

        var dirId = String(getDirId($frame) || "");
        if (!dirId) {
            clearFacetUI();
            return $.Deferred().resolve([]).promise();
        }

        // loading guard
        if ($frame.data(settings.facetLoadingKey)) {
            return $.Deferred().reject("loading").promise();
        }

        // cache hit
        if (settings.enableCache && !forceReload && cache.optionsByDirId[dirId]) {
            var cached = cache.optionsByDirId[dirId];
            $frame.data(settings.facetOptionsKey, cached);
            renderFacetButtons(cached);
            return $.Deferred().resolve(cached).promise();
        }

        if (!w.Coker || !Coker.Directory || typeof Coker.Directory.getFacet !== "function") {
            showErrorUI("Coker.Directory.getFacet not found");
            return $.Deferred().reject("no api").promise();
        }

        $frame.data(settings.facetLoadingKey, true);

        // ✅ 先顯示載入中
        showLoadingUI();

        var payload = settings.buildFacetRequest($frame);

        return Coker.Directory.getFacet(payload)
            .then(function (resp) {
                var items = settings.normalizeFacetResponse(resp) || [];
                $frame.data(settings.facetOptionsKey, items);

                if (settings.enableCache) cache.optionsByDirId[dirId] = items;

                if (!items.length) {
                    showEmptyUI();
                } else {
                    renderFacetButtons(items);
                }

                return items;
            })
            .fail(function () {
                showErrorUI(settings.errorText);
                return [];
            })
            .always(function () {
                $frame.data(settings.facetLoadingKey, false);
            });
    };

    // ======================
    // UI helpers
    // ======================
    function showLoadingUI() {
        if (!settings.loading || settings.loading.enabled !== true) return;

        var $ct = $(settings.facetContainerSelector).first();
        if (!$ct.length) return;

        $ct.empty();

        var html = String(settings.loading.html || "");
        if (!html) html = '<div class="facet-loading" role="status" aria-live="polite" aria-busy="true">載入中…</div>';

        var $ui = $(html);
        var $t = $ui.find(".facet-loading-text");
        if ($t.length) $t.text(String(settings.loading.text || "載入中…"));
        else $ui.text(String(settings.loading.text || "載入中…"));

        $ct.append($ui);

        ensureSpinnerStyleOnce();
    }

    function showEmptyUI() {
        var txt = String(settings.emptyText || "");
        var $ct = $(settings.facetContainerSelector).first指出;
        if (!$ct.length) return;

        $ct.empty();
        if (!txt) return; // 你若不想顯示空資料訊息，保持空白即可
        $ct.append($('<div class="facet-empty" role="status" aria-live="polite"></div>').text(txt));
    }

    function showErrorUI(msg) {
        var txt = String(msg || settings.errorText || "載入失敗");
        var $ct = $(settings.facetContainerSelector).first();
        if (!$ct.length) return;

        $ct.empty();
        $ct.append($('<div class="facet-error" role="status" aria-live="polite"></div>').text(txt));
    }

    function ensureSpinnerStyleOnce() {
        // 只注入一次最基本 spinner 樣式（不影響你原本 CSS，可自行覆蓋）
        if (w.__facetSpinnerStyleInjected) return;
        w.__facetSpinnerStyleInjected = true;

        var css =
            ".facet-loading{display:inline-flex;align-items:center;gap:.5rem;}" +
            ".facet-spinner{width:14px;height:14px;border:2px solid currentColor;border-right-color:transparent;border-radius:50%;display:inline-block;animation:facetspin .8s linear infinite;opacity:.7;}" +
            "@keyframes facetspin{to{transform:rotate(360deg);}}";

        var el = document.createElement("style");
        el.appendChild(document.createTextNode(css));
        document.head.appendChild(el);
    }

    // ======================
    // Internals
    // ======================
    function getDirId($frame) {
        var v = $frame.data("dirid");
        if (v == null || v === "") v = $frame.attr("data-dirid");
        return v != null ? String(v) : "";
    }

    function toLong(v) {
        var n = Number(String(v || "").trim());
        return Number.isFinite(n) ? n : v;
    }

    function toInt(v) {
        var n = parseInt(v, 10);
        return Number.isFinite(n) ? n : 0;
    }

    function findFacetByKey(items, key) {
        if (!items || !items.length) return null;
        key = String(key || "");
        for (var i = 0; i < items.length; i++) {
            if (String(items[i].key) === key) return items[i];
        }
        return null;
    }

    function clearFacetUI() {
        var $ct = $(settings.facetContainerSelector).first();
        if (!$ct.length) return;
        $ct.empty();
    }

    function renderFacetButtons(items) {
        var $ct = $(settings.facetContainerSelector).first();
        if (!$ct.length) return;

        $ct.empty();

        if (!items || !items.length) return;

        var $tpl = $(settings.facetItemTemplateSelector).first();
        if (!$tpl.length) return;

        for (var i = 0; i < items.length; i++) {
            var it = items[i];
            var $btn = $tpl.clone(false);

            $btn.removeAttr("hidden").removeAttr("aria-hidden");
            $btn.attr("data-facet-value", String(it.key || ""));
            $btn.prop("disabled", false);

            $btn.removeClass("active");
            $btn.text(String(it.text || ""));

            $ct.append($btn);
        }
    }

    function applyFacet($frame, facet) {
        if (!facet) return;

        // Facet item schema differs by FacetType (Year/Month/YearMonth may have start/end; Tag/DocumentType usually only has key/id/label)
        var rawLabel = (facet.label != null ? facet.label : (facet.Label != null ? facet.Label : (facet.text || "")));
        var label = String(rawLabel || "");

        // "key" is the UI selection key (used to highlight the clicked button)
        var key = String(facet.key != null ? facet.key : (facet.Key != null ? facet.Key : ""));

        // Detect start/end existence (don't default to 0 when missing)
        var hasStart = (facet.start != null || facet.Start != null);
        var hasEnd = (facet.end != null || facet.End != null);

        var s = null;
        var e = null;

        if (hasStart) {
            var ns = parseInt((facet.start != null ? facet.start : facet.Start), 10);
            if (Number.isFinite(ns)) s = ns;
        }
        if (hasEnd) {
            var ne = parseInt((facet.end != null ? facet.end : facet.End), 10);
            if (Number.isFinite(ne)) e = ne;
        }

        // Build the value list that will be sent to GetReleInfo (A scheme: comma-separated values, NO range syntax)
        var values = [];
        if (s != null && e != null) {
            var a = Math.min(s, e);
            var b = Math.max(s, e);

            // Expand inclusive range into value list (usually small: years/months)
            for (var i = a; i <= b; i++) values.push(String(i));

            // If key is empty, use a stable key for UI highlight
            if (!key) key = (a === b) ? String(a) : (String(a) + "-" + String(b));
        } else {
            // Prefer id/value/key; fall back to label only if nothing else exists
            var v = (facet.id != null ? facet.id :
                (facet.Id != null ? facet.Id :
                    (facet.value != null ? facet.value :
                        (facet.Value != null ? facet.Value :
                            (key || "")))));
            v = String(v || "").trim();
            if (!v) v = label;
            values = v ? [v] : [];

            if (!key) key = v;
        }

        var facetValueString = values.join(",");

        // Store facet "state snapshot" on the frame for DirectoryGetData/GetReleInfo
        // A scheme: data-facet="<v1,v2,...>"
        $frame.attr("data-facet", facetValueString);

        var model = {
            key: key,
            label: label,
            values: values,
            value: facetValueString
        };

        // Keep legacy fields to avoid breaking existing code that reads facetDataKey
        if (s != null) {
            model.start = s;
            model.Start = s;
        }
        if (e != null) {
            model.end = e;
            model.End = e;
        }
        model.Label = label;

        $frame.data(settings.facetDataKey, model);

        // UI highlight is based on the clicked facet key (not the expanded value list)
        setActiveFacetValue(model.key);

        $frame.trigger("facet:change", [model]);

        settings.reloadContent($frame);
    }

    function setActiveFacetValue(facetValue) {
        facetValue = String(facetValue || "");
        var $ct = $(settings.facetContainerSelector).first();
        if (!$ct.length) return;

        $ct.find(".active").removeClass("active");
        if (!facetValue) return;

        $ct.find("[data-facet-value]").each(function () {
            var $b = $(this);
            if (String($b.attr("data-facet-value") || "") === facetValue) {
                $b.addClass("active");
            }
        });
    }
    function stepFacet(step) {
        var $ct = $(settings.facetContainerSelector).first(); // [data-role="facet-items"]
        if (!$ct.length) return;

        // 只找「可見且非 template」的 facet-item（你 template 仍是 hidden）
        var $items = $ct.find("[data-facet-value]").filter(function () {
            var $b = $(this);
            return !$b.is("[hidden]") && $b.attr("aria-hidden") !== "true";
        });

        if (!$items.length) return;

        // 目前 active
        var $active = $items.filter(".active").first();

        // 沒 active：預設第一個（next）或最後一個（prev）
        var idx = $active.length ? $items.index($active) : (step > 0 ? -1 : $items.length);

        var nextIdx = idx + (step > 0 ? 1 : -1);
        if (nextIdx < 0) nextIdx = 0;
        if (nextIdx >= $items.length) nextIdx = $items.length - 1;

        var $target = $items.eq(nextIdx);
        if (!$target.length) return;

        // 觸發「切換 facet」：等同使用者點選
        $target.trigger("click");
    }


    $(function () {
        DirectoryFacet.init(document);
        $(document)
            .on("click", "[data-role='facet-prev']", function () {
                stepFacet(-1);
            })
            .on("click", "[data-role='facet-next']", function () {
                stepFacet(1);
            });
    });

})(window, window.jQuery);