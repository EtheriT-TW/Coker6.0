(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectorySortControl = (w.DirectorySortControl = w.DirectorySortControl || {});
    const contentTypes = {
        product: "product",
        article: "article"
    };
    const sortOptions = {
        product: [
            { value: "default", labelKey: "SortDefault", fallback: "推薦" },
            { value: "price", labelKey: "SortPrice", fallback: "價格" },
            { value: "name", labelKey: "SortName", fallback: "名稱" },
            { value: "model", labelKey: "SortModel", fallback: "型號" }
        ],
        article: [
            { value: "default", labelKey: "SortDefault", fallback: "推薦" },
            { value: "title", labelKey: "SortTitle", fallback: "標題" },
            { value: "publishdate", labelKey: "SortPublishDate", fallback: "發布日期" },
            { value: "lastmodified", labelKey: "SortLastModified", fallback: "最後編輯時間" }
        ]
    };

    function text(key, fallback) {
        return w.local && w.local[key] ? w.local[key] : fallback;
    }

    function normalizeContentType(value) {
        const normalized = String(value == null ? "" : value).trim().toLowerCase();

        if (normalized === "1" || normalized === "product" || normalized === "商品") {
            return contentTypes.product;
        }
        if (normalized === "2" || normalized === "article" || normalized === "文章") {
            return contentTypes.article;
        }
        return null;
    }

    function resolveContentType($frame, result) {
        const explicitType = normalizeContentType(
            result?.contentType
            ?? result?.ContentType
            ?? $frame.attr("data-sort-content-type")
            ?? $frame.attr("data-type")
        );
        if (explicitType) return explicitType;

        if (String($frame.attr("data-type") || "").toLowerCase() === "search") {
            const searchTarget = String($frame.attr("data-dirid") || "").split(",")[0].trim();
            if (searchTarget === "-2" || searchTarget === "3") return contentTypes.product;
            if (searchTarget === "-1") return contentTypes.article;
        }

        return null;
    }

    function getStorageKey($frame, contentType) {
        let scope = String(w.location.pathname || "").toLowerCase();
        const directoryIds = String($frame.attr("data-dirid") || "").trim();
        const searchIndex = scope.indexOf("/search");
        if (searchIndex >= 0) {
            scope = scope.substring(0, searchIndex + "/search".length);
            // 沿用既有搜尋頁儲存鍵，升級共用元件後不會遺失使用者偏好。
            return `searchSort-${scope}-${directoryIds}`;
        }
        return `directorySort-${scope}-${directoryIds}-${contentType}`;
    }

    function readPreference($frame, contentType) {
        const validOptions = sortOptions[contentType].map(function (option) { return option.value; });

        try {
            const saved = JSON.parse(w.localStorage.getItem(getStorageKey($frame, contentType)) || "null");
            const sortBy = String(saved?.sortBy || "default").toLowerCase();
            const validSortBy = validOptions.includes(sortBy) ? sortBy : "default";
            return {
                sortBy: validSortBy,
                direction: validSortBy !== "default" && saved?.direction === "desc" ? "desc" : "asc"
            };
        } catch {
            return { sortBy: "default", direction: "asc" };
        }
    }

    function writePreference($frame, contentType, sortBy, direction) {
        try {
            w.localStorage.setItem(getStorageKey($frame, contentType), JSON.stringify({
                sortBy: sortBy,
                direction: direction
            }));
        } catch {
            // localStorage 無法使用時，排序仍保留至本頁離開為止。
        }
    }

    function buildControl(contentType) {
        const $control = $("<div>", {
            class: "directory-sort-control d-flex align-items-center me-auto text-black",
            "data-directory-sort-type": contentType
        });
        const $icon = $("<span>", {
            class: "sort-control-icon material-symbols-outlined me-2",
            "aria-hidden": "true",
            title: text("SortBy", "排序"),
            text: "sort"
        });
        const $hiddenLabel = $("<span>", {
            class: "visually-hidden",
            text: text("SortBy", "排序")
        });
        const $group = $("<div>", {
            class: "btn-group btn-group-sm",
            role: "group",
            "aria-label": text("SortBy", "排序")
        });
        const $direction = $("<button>", {
            type: "button",
            class: "directory-sort-direction btn btn-outline-secondary",
            "aria-disabled": "true",
            "data-ascending-label": text("SortAscending", "升序"),
            "data-descending-label": text("SortDescending", "降序"),
            "data-unavailable-label": text("SortDirectionUnavailable", "請先選擇排序欄位")
        }).append($("<span>", { class: "directory-sort-label" }));
        const $toggle = $("<button>", {
            type: "button",
            class: "btn btn-outline-secondary dropdown-toggle dropdown-toggle-split",
            "data-bs-toggle": "dropdown",
            "data-bs-auto-close": "true",
            "aria-expanded": "false",
            "aria-label": text("SortOptions", "選擇排序方式"),
            title: text("SortOptions", "選擇排序方式")
        }).append($("<span>", {
            class: "visually-hidden",
            text: text("SortOptions", "選擇排序方式")
        }));
        const $menu = $("<ul>", { class: "directory-sort-menu dropdown-menu dropdown-menu-end" });

        sortOptions[contentType].forEach(function (option) {
            $menu.append(
                $("<li>").append($("<button>", {
                    type: "button",
                    class: "dropdown-item",
                    "data-sort": option.value,
                    text: text(option.labelKey, option.fallback)
                }))
            );
        });

        $group.append($direction, $toggle, $menu);
        return $control.append($icon, $hiddenLabel, $group);
    }

    function closeMenu($control) {
        const toggle = $control.find("[data-bs-toggle='dropdown']")[0];
        if (toggle && w.bootstrap?.Dropdown) {
            w.bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
        } else {
            $control.find(".directory-sort-menu").removeClass("show");
            $(toggle).removeClass("show").attr("aria-expanded", "false");
        }
    }

    function updateControl($frame, $control) {
        const sortBy = String($frame.data("searchSortBy") || "default").toLowerCase();
        const direction = $frame.data("searchSortDirection") === "desc" ? "desc" : "asc";
        const isDefault = sortBy === "default";
        const $direction = $control.find(".directory-sort-direction");
        const $selected = $control.find(`[data-sort="${sortBy}"]`).first();
        const label = isDefault
            ? $direction.data("unavailableLabel")
            : $direction.data(direction === "desc" ? "descendingLabel" : "ascendingLabel");

        $direction
            .toggleClass("is-default", isDefault)
            .attr("aria-disabled", isDefault ? "true" : "false")
            .attr("aria-label", label)
            .attr("title", isDefault ? "" : label)
            .find(".directory-sort-label")
            .text($selected.text().trim());
        $control.find(".btn-group")
            .removeClass("sort-default sort-asc sort-desc")
            .addClass(isDefault ? "sort-default" : `sort-${direction}`);
        $control.find(".sort-control-icon")
            .toggleClass("is-ascending", !isDefault && direction === "asc");
        $control.find(".dropdown-item").removeClass("active").removeAttr("aria-current");
        $selected.addClass("active").attr("aria-current", "true");
    }

    function reload($frame) {
        $frame.removeData("page");
        if (w.DirectoryBoot && typeof w.DirectoryBoot.navigateToPage === "function") {
            w.DirectoryBoot.navigateToPage($frame, 1);
        } else {
            $frame.trigger("filter");
        }
    }

    function bind($frame, $control, contentType) {
        $control.off("click.directorySort");
        $control.on("click.directorySort", ".dropdown-item", function () {
            closeMenu($control);
            const sortBy = String($(this).data("sort") || "default").toLowerCase();
            $frame.data("searchSortBy", sortBy);
            $frame.data("searchSortDirection", "asc");
            writePreference($frame, contentType, sortBy, "asc");
            updateControl($frame, $control);
            reload($frame);
        });
        $control.on("click.directorySort", ".directory-sort-direction", function () {
            closeMenu($control);
            if (String($frame.data("searchSortBy") || "default").toLowerCase() === "default") return;

            const direction = $frame.data("searchSortDirection") === "desc" ? "asc" : "desc";
            $frame.data("searchSortDirection", direction);
            writePreference($frame, contentType, $frame.data("searchSortBy"), direction);
            updateControl($frame, $control);
            reload($frame);
        });

        const toggle = $control.find("[data-bs-toggle='dropdown']")[0];
        if (toggle && w.bootstrap?.Dropdown) {
            w.bootstrap.Dropdown.getOrCreateInstance(toggle, { autoClose: true });
        }
    }

    DirectorySortControl.init = function ($frame, result) {
        if (!$frame || !$frame.length) return null;

        const requestType = String($frame.attr("data-type") || "").trim().toLowerCase();
        if (requestType === "techcert") return null;

        const $switchControl = $frame.find(".switch_control").first();
        if (!$switchControl.length) return null;

        const contentType = resolveContentType($frame, result);
        if (!contentType) return null;

        // ViewTypeChange 會在只有一種檢視模式時隱藏 switch_control；
        // 排序器存在後它仍有操作用途，必須恢復顯示。
        $switchControl.removeClass("d-none");

        let $control = $switchControl.children(".directory-sort-control").first();
        let created = false;
        if ($control.length && $control.attr("data-directory-sort-type") !== contentType) {
            $control.remove();
            $frame.removeData("searchSortBy searchSortDirection");
            $control = $();
        }
        if (!$control.length) {
            $control = buildControl(contentType);
            $switchControl.prepend($control);
            created = true;
        }

        if (typeof $frame.data("searchSortBy") === "undefined") {
            const saved = readPreference($frame, contentType);
            $frame.data("searchSortBy", saved.sortBy);
            $frame.data("searchSortDirection", saved.direction);
        }

        bind($frame, $control, contentType);
        updateControl($frame, $control);
        return {
            contentType: contentType,
            // 一般目錄要等首次回應才能得知類型。若已有非預設偏好，重新請求一次，
            // 避免控制器顯示已排序但畫面仍是伺服器預設順序。
            requiresReload: Boolean(result && created && $frame.data("searchSortBy") !== "default")
        };
    };

    $(document)
        .off("pointerdown.directorySortMenu")
        .on("pointerdown.directorySortMenu", function (event) {
            if ($(event.target).closest(".directory-sort-control").length) return;
            $(".directory-sort-control").each(function () { closeMenu($(this)); });
        });
    $(w)
        .off("blur.directorySortMenu")
        .on("blur.directorySortMenu", function () {
            $(".directory-sort-control").each(function () { closeMenu($(this)); });
        });

})(window, window.jQuery);
