(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryBoot = (w.DirectoryBoot = w.DirectoryBoot || {});

    function to$root(root) {
        if (!root) return $(document);
        if (root.jquery) return root;
        return $(root);
    }

    function getDirIds($el) {
        const raw = $el.attr("data-dirid");

        if (typeof raw === "undefined") return [];

        const value = String(raw).trim();

        if (value === "") return [];

        return value
            .split(",")
            .map(function (x) {
                return x.trim();
            })
            .filter(function (x) {
                return x !== "" && x !== "0";
            });
    }

    function getAdvertiseTake($el) {
        const raw = $el.data("maxlen");
        const take = Number(raw);

        if (!Number.isFinite(take) || take < 1) return 1;

        return Math.min(Math.floor(take), 20);
    }

    function showAdvertiseLoading($el) {
        if (!$el || !$el.length || $el.children(".coker-ad-loading").length) return;

        const lang = String(document.documentElement.lang || "").toLowerCase();
        const loadingText = lang.indexOf("zh") === 0 ? "廣告載入中…" : "Loading advertisement…";

        $el.addClass("coker-ad-is-loading").append(
            `<div class="coker-ad-loading" role="status" aria-live="polite">
                <span class="coker-ad-loading-spinner" aria-hidden="true"></span>
                <span class="coker-ad-loading-text">${loadingText}</span>
            </div>`
        );
    }

    function hideAdvertiseLoading($el, revealContent) {
        if (!$el || !$el.length) return;

        $el.removeClass("coker-ad-is-loading");

        const $loading = $el.children(".coker-ad-loading");

        if (revealContent) {
            $el.addClass("coker-ad-is-revealing");

            const clearRevealState = function () {
                $el.removeClass("coker-ad-is-revealing");
            };

            $el.children().not(".coker-ad-loading")
                .one("animationend.cokerAdvertiseReveal", clearRevealState);
            window.setTimeout(clearRevealState, 500);
        }

        if (!$loading.length) return;

        $loading.addClass("coker-ad-loading-leave")
            .one("animationend.cokerAdvertiseLoading", function () {
                $(this).remove();
            });
        window.setTimeout(function () {
            $loading.remove();
        }, 350);
    }

    function canAutoLoadCatalog($el) {
        if (!$el || !$el.length) return false;

        const dirIds = getDirIds($el);

        if (dirIds.length > 0) return true;

        const type = ($el.attr("data-type") || "").toString().trim().toLowerCase();
        const searchText = ($el.attr("data-search-text") || "").toString().trim();

        // 若你的搜尋元件允許沒有 dirid，但需要 searchText，才開這段
        if (type === "search" && searchText !== "") return true;

        return false;
    }

    function hasNearestCoordinates($el) {
        if (!$el.hasClass("getlatlng")) return true;

        const longitude = Number($el.data("longitude"));
        const latitude = Number($el.data("latitude"));

        return Number.isFinite(longitude) && Number.isFinite(latitude);
    }
    function getHashPage() {
        return location.hash.replace("#", "");
    }

    function normalizePage(page) {
        if (isNaN(page) || page === "") return "1";
        return String(page);
    }

    function buildCatalogOption($self, page) {
        const shownum = typeof $self.data("shownum") !== "undefined" ? $self.data("shownum") : 12;
        const maxlen = typeof $self.data("maxlen") !== "undefined" && $self.data("maxlen") !== "" ? $self.data("maxlen") : 0;
        const dirid = getDirIds($self);

        return {
            Ids: dirid,
            SiteId: typeof w.SiteId === "undefined" ? 0 : w.SiteId,
            Page: page,
            ShowNum: shownum,
            MaxLen: maxlen,
            Type: typeof $self.data("type") === "undefined" ? null : $self.data("type"),
            SearchText: typeof $self.data("searchText") === "undefined" ? null : ($self.data("searchText") || "").toString().trim(),
            Filters: $self.data("filtered"),
            DirectoryType: $self.data("directoryTypeChecked") || 0,
            target: typeof $self.data("target") === "undefined" ? null : $self.data("target"),
            FindNearest: $self.hasClass("getlatlng") && hasNearestCoordinates($self),
            Longitude: typeof $self.data("longitude") !== "undefined" ? $self.data("longitude") : null,
            Latitude: typeof $self.data("latitude") !== "undefined" ? $self.data("latitude") : null,
            Facet: typeof $self.attr("data-facet") === "undefined" ? null : ($self.attr("data-facet") || null)
        };
    }

    function bindCatalogFilterReload($self, page) {
        $self.off("filter.directory").on("filter.directory", function () {
            $self.removeData("page");
            initElemntAndLoadDir($self, page);
        });
    }

    function initSingleCatalog($self, page) {
        if (!canAutoLoadCatalog($self)) return;
        // 鄰近據點必須等瀏覽器定位完成，避免送出缺少座標的查詢。
        if (!hasNearestCoordinates($self)) return;

        const dirid = getDirIds($self);
        const hashPage = !!page ? page.toString() : getHashPage();

        if (typeof $self.data("page") !== "undefined" && $self.data("page") === hashPage) {
            return;
        }

        page = normalizePage(hashPage);

        const option = buildCatalogOption($self, page);

        $self.data("prevdirid", dirid.join(","));
        $self.find(".catalog>.template").remove();

        if (w.DirectoryService && typeof w.DirectoryService.getCatalogData === "function") {
            const requestId = Number($self.data("directoryRequestId") || 0) + 1;
            $self.data("directoryRequestId", requestId);

            if (w.DirectoryRenderer && typeof w.DirectoryRenderer.showLoading === "function") {
                w.DirectoryRenderer.showLoading($self);
            }

            w.DirectoryService.getCatalogData($self, option)
                .done(function (result) {
                    if ($self.data("directoryRequestId") !== requestId) return;

                    if (w.DirectoryRenderer && typeof w.DirectoryRenderer.renderCatalogResult === "function") {
                        w.DirectoryRenderer.renderCatalogResult($self, option, result, requestId);
                    }
                })
                .fail(function (error) {
                    if ($self.data("directoryRequestId") !== requestId) return;

                    if (w.DirectoryRenderer && typeof w.DirectoryRenderer.hideLoading === "function") {
                        w.DirectoryRenderer.hideLoading($self);
                    }
                    if (w.DirectoryService && typeof w.DirectoryService.handleError === "function") {
                        w.DirectoryService.handleError(error);
                    }
                });
        } else if (w.DirectoryRenderer && typeof w.DirectoryRenderer.hideLoading === "function") {
            w.DirectoryRenderer.hideLoading($self);
        }

        bindCatalogFilterReload($self, page);
        $self.data("page", page);
    }

    function initMenuDirectories($root) {
        $root.find(".menu_directory").each(function () {
            const $self = $(this);
            const dirid = getDirIds($self);
            if (!dirid.length) return;

            const showUnvisible = typeof $self.attr("data-show-unvisible") !== "undefined"
                ? $self.attr("data-show-unvisible").toLowerCase() === "true"
                : false;

            if (typeof $self.data("prevdirid") !== "undefined" && dirid == $self.data("prevdirid")) {
                return;
            }

            $self.data("prevdirid", dirid);
            $self.find(".title").text("");
            $self.find(".accordion").empty();

            if (w.DirectoryService && typeof w.DirectoryService.getMenuData === "function") {
                w.DirectoryService.getMenuData({
                    Ids: dirid,
                    WebsiteId: typeof w.SiteId !== "undefined" ? w.SiteId : 0,
                    showUnvisible: showUnvisible
                }).done(function (result) {
                    if (w.DirectoryBlocks && typeof w.DirectoryBlocks.renderMenu === "function") {
                        w.DirectoryBlocks.renderMenu($self, result);
                    }
                });
            }
        });
    }

    function initAdvertiseDirectories($root) {
        const pendingItems = [];
        const groups = [];
        const seenGroups = {};
        const $advertiseDirectories = $root
            .filter(".advertise_directory")
            .add($root.find(".advertise_directory"));

        $advertiseDirectories.each(function () {
            const $self = $(this);
            const dirid = getDirIds($self);
            if (!dirid.length) return;

            const directoryKey = dirid.join(",");
            const take = getAdvertiseTake($self);
            const loadingKey = `${directoryKey}|${take}`;

            if (
                $self.data("advertiseLoadingKey") === loadingKey ||
                $self.data("advertiseLoadedKey") === loadingKey
            ) {
                return;
            }

            $self.data("advertiseLoadingKey", loadingKey);
            showAdvertiseLoading($self);
            pendingItems.push({
                $element: $self,
                directoryIds: dirid,
                key: directoryKey,
                loadingKey: loadingKey,
                take: take
            });

            if (!seenGroups[directoryKey]) {
                seenGroups[directoryKey] = true;
                groups.push(dirid);
            }
        });

        if (!pendingItems.length) return;
        if (!w.DirectoryService || typeof w.DirectoryService.getAdvertiseBatchData !== "function") {
            pendingItems.forEach(function (item) {
                item.$element.removeData("advertiseLoadingKey");
                hideAdvertiseLoading(item.$element);
            });
            return;
        }

        const take = pendingItems.reduce(function (max, item) {
            return Math.max(max, item.take);
        }, 1);
        const request = w.DirectoryService.getAdvertiseBatchData(groups, take);

        request.done(function (result) {
            const resultByKey = {};

            (result || []).forEach(function (groupResult) {
                const key = groupResult.key || groupResult.Key || "";
                resultByKey[key] = groupResult.advertisements || groupResult.Advertisements || [];
            });

            pendingItems.forEach(function (item) {
                // Ignore a stale response when the editor changed the directory
                // association or max length while this batch request was in flight.
                const currentKey = `${getDirIds(item.$element).join(",")}|${getAdvertiseTake(item.$element)}`;
                if (currentKey !== item.loadingKey) {
                    if (item.$element.data("advertiseLoadingKey") === item.loadingKey) {
                        hideAdvertiseLoading(item.$element, false);
                    }
                    return;
                }

                if (w.DirectoryBlocks && typeof w.DirectoryBlocks.renderAdvertise === "function") {
                    w.DirectoryBlocks.renderAdvertise(
                        item.$element,
                        (resultByKey[item.key] || []).slice(0, item.take)
                    );
                }

                item.$element.data("advertiseLoadedKey", item.loadingKey);
                hideAdvertiseLoading(item.$element, true);
            });
        }).fail(function () {
            pendingItems.forEach(function (item) {
                if (item.$element.data("advertiseLoadingKey") === item.loadingKey) {
                    item.$element.removeData("advertiseLoadedKey");
                    hideAdvertiseLoading(item.$element, false);
                }
            });
        }).always(function () {
            pendingItems.forEach(function (item) {
                if (item.$element.data("advertiseLoadingKey") === item.loadingKey) {
                    item.$element.removeData("advertiseLoadingKey");
                }
            });
        });
    }

    function bindHashChangeIfNeeded($root) {
        const dirLength = $root.find(".catalog_frame").filter(function () {
            return canAutoLoadCatalog($(this));
        }).length;

        if (dirLength !== 1) return;

        if ("onhashchange" in window) {
            window.onhashchange = hashChangeDirectory;
        } else {
            setInterval(hashChangeDirectory, 1000);
        }
    }

    function DirectoryGetDataInit(root) {
        const $root = to$root(root);
        const $catalogs = $root.find(".catalog_frame").filter(function () {
            return canAutoLoadCatalog($(this));
        });

        $catalogs.each(function () {
            const $self = $(this);
            const dirid = getDirIds($self).join(",");

            if (typeof $self.data("prevdirid") === "undefined" || dirid !== $self.data("prevdirid")) {
                initElemntAndLoadDir($self);
            }
        });

        initMenuDirectories($root);
        initAdvertiseDirectories($root);
        bindHashChangeIfNeeded($root);
    }

    function initElemntAndLoadDir($dir, page) {
        const $self = $dir && $dir.length
            ? $dir
            : $(".catalog_frame").filter(function () {
                return canAutoLoadCatalog($(this));
            }).first();

        if (!$self || !$self.length) return;
        if (!canAutoLoadCatalog($self)) return;

        const tempSiblings = $self.find(".templatecontent").siblings();
        if (tempSiblings.length > 0) {
            for (let i = 0; i < tempSiblings.length; i++) {
                if (!$(tempSiblings[i]).hasClass("templatecontent-tag")) {
                    tempSiblings[i].remove();
                }
            }
        }

        initSingleCatalog($self, page);
    }

    function hashChangeDirectory(e) {
        if (!!e) {
            initElemntAndLoadDir();
            e.preventDefault();
        } else {
            console.log("HashChange錯誤");
        }
    }

    DirectoryBoot.init = DirectoryGetDataInit;
    DirectoryBoot.initElemntAndLoadDir = initElemntAndLoadDir;
    DirectoryBoot.hashChangeDirectory = hashChangeDirectory;

    // 舊版相容
    w.DirectoryGetDataInit = DirectoryGetDataInit;
    w.initElemntAndLoadDir = initElemntAndLoadDir;
    w.hashChangeDirectory = hashChangeDirectory;

})(window, window.jQuery);
