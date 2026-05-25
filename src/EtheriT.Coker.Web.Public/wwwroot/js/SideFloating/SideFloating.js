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
        $panel.removeClass("d-none");
        $trigger.attr("aria-expanded", "true");

        initPanelContent($panel);
    }

    function closePanel($panel) {
        if (!$panel || !$panel.length) {
            return;
        }

        const id = $panel.attr("id");

        $panel.addClass("d-none");

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

    window.SideFloating = {
        init: init
    };

})(window, jQuery, window.co || window.Coker);