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
        const dirid = $el.data("dirid");
        if (typeof dirid === "undefined") return 0;
        if (typeof dirid === "string") return dirid.split(",");
        return [dirid];
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
            SearchText: typeof $self.data("searchText") === "undefined" ? null : $self.data("searchText"),
            Filters: $self.data("filtered"),
            directoryType: $self.data("directoryTypeChecked"),
            target: typeof $self.data("target") === "undefined" ? null : $self.data("target"),
            FindNearest: $self.hasClass("getlatlng"),
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
        const dirid = getDirIds($self);
        const hashPage = !!page ? page.toString() : getHashPage();

        if (typeof $self.data("page") !== "undefined" && $self.data("page") === hashPage) {
            return;
        }

        page = normalizePage(hashPage);

        if (dirid.length === 1 && dirid[0] === "") return;

        const option = buildCatalogOption($self, page);

        $self.data("prevdirid", dirid);
        $self.find(".catalog>.template").remove();

        if (w.DirectoryService && typeof w.DirectoryService.getCatalogData === "function") {
            w.DirectoryService.getCatalogData($self, option)
                .done(function (result) {
                    if (w.DirectoryRenderer && typeof w.DirectoryRenderer.renderCatalogResult === "function") {
                        w.DirectoryRenderer.renderCatalogResult($self, option, result);
                    }
                });
        }

        bindCatalogFilterReload($self, page);
        $self.data("page", page);
    }

    function initMenuDirectories($root) {
        $root.find(".menu_directory").each(function () {
            const $self = $(this);
            const dirid = getDirIds($self);
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
        $root.find(".advertise_directory").each(function () {
            const $self = $(this);
            const dirid = getDirIds($self);

            if (w.DirectoryService && typeof w.DirectoryService.getAdvertiseData === "function") {
                w.DirectoryService.getAdvertiseData({
                    Ids: dirid,
                    WebsiteId: typeof w.SiteId !== "undefined" ? w.SiteId : 0,
                    showUnvisible: true
                }).done(function (result) {
                    if (w.DirectoryBlocks && typeof w.DirectoryBlocks.renderAdvertise === "function") {
                        w.DirectoryBlocks.renderAdvertise($self, result);
                    }
                });
            }
        });
    }

    function bindHashChangeIfNeeded($root) {
        const dirLength = $root.find(".catalog_frame").length;
        if (dirLength !== 1) return;

        if ("onhashchange" in window) {
            window.onhashchange = hashChangeDirectory;
        } else {
            setInterval(hashChangeDirectory, 1000);
        }
    }

    function DirectoryGetDataInit(root) {
        const $root = to$root(root);
        const $catalogs = $root.find(".catalog_frame");

        $catalogs.each(function () {
            const $self = $(this);
            const dirid = getDirIds($self);

            if (typeof $self.data("prevdirid") === "undefined" || dirid != $self.data("prevdirid")) {
                initElemntAndLoadDir($self);
            }
        });

        initMenuDirectories($root);
        initAdvertiseDirectories($root);
        bindHashChangeIfNeeded($root);
    }

    function initElemntAndLoadDir($dir, page) {
        const $self = $dir || $(".catalog_frame").first();

        if (!$self || !$self.length) return;

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