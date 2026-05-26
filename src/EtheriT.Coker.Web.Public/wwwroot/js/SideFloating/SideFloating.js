(function (window, $, co) {
    "use strict";

    const MODULE_NAME = "SideFloating";

    const SELECTORS = {
        root: "#Floating_Center",
        item: ".side-floating-item",
        expandTrigger: ".side-floating-expand-trigger",
        panel: "[data-side-floating-panel]",
        close: ".side-floating-close"
    };

    function init(root) {
        const $scope = root ? $(root) : $(document);

        let $root = $scope.is(SELECTORS.root)
            ? $scope
            : $scope.find(SELECTORS.root).first();

        if (!$root.length && !root) {
            $root = $(SELECTORS.root);
        }

        if (!$root.length) {
            return;
        }

        if ($root.data("side-floating-init") === true) {
            return;
        }

        $root.data("side-floating-init", true);

        bindEvents($root);
    }

    function bindEvents($root) {
        $root
            .off("click.sideFloatingExpand", SELECTORS.expandTrigger)
            .on("click.sideFloatingExpand", SELECTORS.expandTrigger, function (event) {
                event.preventDefault();
                const $trigger = $(this);
                const $item = $trigger.closest(SELECTORS.item);
                const $panel = $item.find(SELECTORS.panel).first();

                if (!$panel.length) {
                    return;
                }

                togglePanel($root, $trigger, $panel);
            });

        $root
            .off("click.sideFloatingClose", SELECTORS.close)
            .on("click.sideFloatingClose", SELECTORS.close, function (event) {
                event.preventDefault();

                const $panel = $(this).closest(SELECTORS.panel);
                closePanel($panel);
            });
    }

    function togglePanel($root, $trigger, $panel) {
        const isOpen = !$panel.hasClass("d-none");

        closeAllPanels($root);

        if (isOpen) {
            return;
        }

        openPanel($trigger, $panel);
    }

    function openPanel($trigger, $panel) {
        const $item = $trigger.closest(SELECTORS.item);

        updatePanelMaxHeight($trigger, $panel);

        $panel.removeClass("d-none");
        $item.addClass("is-open");
        $trigger.attr("aria-expanded", "true");

        initPanelContent($panel);
    }

    function closePanel($panel) {
        if (!$panel || !$panel.length) {
            return;
        }

        const id = $panel.attr("id");
        const $item = $panel.closest(SELECTORS.item);

        $panel.addClass("d-none");
        $item.removeClass("is-open");
        $panel.css("--side-floating-panel-max-height", "");

        if (id) {
            $(`[aria-controls="${id}"]`).attr("aria-expanded", "false");
        }
    }

    function closeAllPanels($root) {
        $root.find(SELECTORS.panel).each(function () {
            closePanel($(this));
        });
    }

    function initPanelContent($panel) {
        if ($panel.data("side-floating-inited") === true) {
            return;
        }

        if (co && co.page && typeof co.page.init === "function") {
            co.page.init($panel, {
                force: true
            });
        }

        $panel.data("side-floating-inited", true);
    }

    function updatePanelMaxHeight($trigger, $panel) {
        if (!$trigger || !$trigger.length || !$panel || !$panel.length) {
            return;
        }

        const trigger = $trigger.get(0);
        const rect = trigger.getBoundingClientRect();

        const viewportHeight = getViewportHeight();
        const topSafeGap = 12;

        const marginBottom = parseFloat($panel.css("margin-bottom")) || 0;

        // panel 是 bottom: 100%，所以 panel 的底部約等於 trigger 的 top - marginBottom。
        // 因此可用高度 = trigger.top - 上方安全距離 - panel margin-bottom。
        const availableHeight = rect.top - topSafeGap - marginBottom;

        const maxHeight = Math.max(160, Math.min(720, availableHeight));

        $panel.css("--side-floating-panel-max-height", maxHeight + "px");
    }

    function getViewportHeight() {
        if (window.visualViewport && window.visualViewport.height) {
            return window.visualViewport.height;
        }

        return window.innerHeight || document.documentElement.clientHeight || 0;
    }

    window.SideFloating = {
        init: init
    };

})(window, jQuery, window.co || window.Coker);