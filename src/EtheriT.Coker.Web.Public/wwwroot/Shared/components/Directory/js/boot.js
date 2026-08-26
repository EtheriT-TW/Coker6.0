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
        if (raw === null || typeof raw === "undefined" || String(raw).trim() === "") {
            return null;
        }

        const take = Number(raw);

        // 舊版未設定最大筆數時會回傳全部符合廣告；0 或無效值也視為未限制。
        if (!Number.isFinite(take) || take < 1) return null;

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
    function normalizePage(page) {
        const value = String(page == null ? "" : page).trim();
        if (!/^\d+$/.test(value)) return "1";

        const number = parseInt(value, 10);
        return number > 0 ? String(number) : "1";
    }

    function getPageUrl(page) {
        const normalizedPage = normalizePage(page);
        const url = new URL(w.location.href);

        // 第一頁使用目錄原始網址，避免 /?Page=1 與原始網址形成重複內容。
        if (normalizedPage === "1") url.searchParams.delete("Page");
        else url.searchParams.set("Page", normalizedPage);

        // 相容舊版 #2 連結，但不要把舊頁碼 hash 帶進新的正式網址。
        if (/^#\d+$/.test(url.hash)) url.hash = "";

        return `${url.pathname}${url.search}${url.hash}`;
    }

    function syncCanonicalPage(page) {
        const canonical = document.querySelector('link[rel="canonical"]');
        if (!canonical || !canonical.href) return;

        const normalizedPage = normalizePage(page);
        const canonicalUrl = new URL(canonical.href);

        if (normalizedPage === "1") canonicalUrl.searchParams.delete("Page");
        else canonicalUrl.searchParams.set("Page", normalizedPage);

        canonical.href = canonicalUrl.href;
    }

    function getLocationPage() {
        const url = new URL(w.location.href);
        const queryPage = url.searchParams.get("Page");

        if (queryPage !== null) return normalizePage(queryPage);

        const legacyHash = (url.hash || "").match(/^#(\d+)$/);
        if (!legacyHash) return "1";

        const page = normalizePage(legacyHash[1]);

        // 舊網址仍可開啟，載入時以 replaceState 無轉址升級成 ?Page=N。
        if (w.history && typeof w.history.replaceState === "function") {
            w.history.replaceState(w.history.state, "", getPageUrl(page));
        }

        return page;
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
        const locationPage = page != null ? page.toString() : getLocationPage();

        if (typeof $self.data("page") !== "undefined" && $self.data("page") === locationPage) {
            return;
        }

        page = normalizePage(locationPage);

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

                    $self.removeData("directoryScrollAfterRender");

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
            const loadingKey = `${directoryKey}|${take === null ? "all" : take}`;

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

        // DTO 目前是整批共用一個 Take；只要任一元件未限制，就讓 API
        // 回傳全部，再由有設定 maxlen 的元件各自裁切。
        const hasUnlimitedItem = pendingItems.some(function (item) {
            return item.take === null;
        });
        const take = hasUnlimitedItem
            ? null
            : pendingItems.reduce(function (max, item) {
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
                const currentTake = getAdvertiseTake(item.$element);
                const currentKey = `${getDirIds(item.$element).join(",")}|${currentTake === null ? "all" : currentTake}`;
                if (currentKey !== item.loadingKey) {
                    if (item.$element.data("advertiseLoadingKey") === item.loadingKey) {
                        hideAdvertiseLoading(item.$element, false);
                    }
                    return;
                }

                if (w.DirectoryBlocks && typeof w.DirectoryBlocks.renderAdvertise === "function") {
                    const advertisements = resultByKey[item.key] || [];
                    w.DirectoryBlocks.renderAdvertise(
                        item.$element,
                        item.take === null ? advertisements : advertisements.slice(0, item.take)
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

    function bindLocationChangeIfNeeded($root) {
        const dirLength = $root.find(".catalog_frame").filter(function () {
            return canAutoLoadCatalog($(this));
        }).length;

        if (dirLength !== 1) return;

        $(w)
            .off("popstate.directoryPager")
            .on("popstate.directoryPager", locationChangeDirectory)
            .off("hashchange.directoryPager")
            .on("hashchange.directoryPager", locationChangeDirectory);
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
        bindLocationChangeIfNeeded($root);
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

    function navigateToPage($item, page) {
        const normalizedPage = normalizePage(page);
        const $catalogs = $(document).find(".catalog_frame").filter(function () {
            return canAutoLoadCatalog($(this));
        });

        if ($catalogs.length === 1 && w.history && typeof w.history.pushState === "function") {
            const nextUrl = getPageUrl(normalizedPage);
            const currentUrl = `${w.location.pathname}${w.location.search}${w.location.hash}`;

            if (nextUrl !== currentUrl) {
                w.history.pushState({ directoryPage: normalizedPage }, "", nextUrl);
            }
        }

        syncCanonicalPage(normalizedPage);
        initElemntAndLoadDir($item, normalizedPage);
    }

    function locationChangeDirectory() {
        const page = getLocationPage();
        syncCanonicalPage(page);
        initElemntAndLoadDir(null, page);
    }

    DirectoryBoot.init = DirectoryGetDataInit;
    DirectoryBoot.initElemntAndLoadDir = initElemntAndLoadDir;
    DirectoryBoot.getPageUrl = getPageUrl;
    DirectoryBoot.navigateToPage = navigateToPage;
    DirectoryBoot.locationChangeDirectory = locationChangeDirectory;
    DirectoryBoot.hashChangeDirectory = locationChangeDirectory;

    // 舊版相容
    w.DirectoryGetDataInit = DirectoryGetDataInit;
    w.initElemntAndLoadDir = initElemntAndLoadDir;
    w.hashChangeDirectory = locationChangeDirectory;

})(window, window.jQuery);
