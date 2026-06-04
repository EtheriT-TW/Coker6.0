(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-products", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        function renderProductPane($pane, $content, datas) {
            var $directory = $pane.find(".type_change_frame.catalog_frame").first();
            var templateHtml = $("#FavoriteTemplate").html();

            if (!MemberPage.Utils.requireRenderer($content)) return;

            w.DirectoryRenderer.renderItemsByExternalTemplate(
                $directory,
                $content,
                templateHtml,
                datas
            );
        }

        function handleProductResult(options) {
            var result = options.result;
            var datas = result && Array.isArray(result.data) ? result.data : [];
            var $pane = options.$pane;
            var $content = options.$content;
            var $pageBtn = options.$pageBtn;
            var $noData = options.$noData;
            var $switch = options.$switch;
            var page = options.page;
            var hashName = options.hashName;
            var needSuccess = !!options.needSuccess;

            var hasData = needSuccess ? (result && result.success && datas.length > 0) : (datas.length > 0);

            if (hasData) {
                $noData.addClass("d-none");

                if (result.page_Total > 1) {
                    $pageBtn.removeClass("d-none");

                    if (!$pageBtn.data("init")) {
                        MemberPage.Pagination.init($pageBtn, result.page_Total, hashName);
                    }

                    MemberPage.Pagination.change($pageBtn, page, result.page_Total);
                } else {
                    $pageBtn.addClass("d-none");
                }

                $switch.removeClass("d-none");
                renderProductPane($pane, $content, datas);
                return;
            }

            if (page != 1) {
                w.location.hash = "#" + hashName + "-1";
                return;
            }

            $pageBtn.addClass("d-none");
            $switch.addClass("d-none");
            $noData.removeClass("d-none");
            $content.empty();
        }

        MemberPage.Products = {
            loadFavoritesPage: function (number) {
                var $pane = $(MemberPage.Selectors.favoritePane);
                var $content = $pane.find(".content");
                var $pageBtn = $pane.find(".page_btn");
                var $noData = $pane.find(".nodata");
                var $switch = $pane.find(".switch_control");

                C.Favorites.GetDisplay(number).done(function (result) {
                    handleProductResult({
                        result: result,
                        $pane: $pane,
                        $content: $content,
                        $pageBtn: $pageBtn,
                        $noData: $noData,
                        $switch: $switch,
                        page: number,
                        hashName: "favorites",
                        needSuccess: false
                    });
                });
            },

            loadBrowsingHistoryPage: function (number) {
                var $pane = $(MemberPage.Selectors.historyPane);
                var $content = $pane.find(".content");
                var $pageBtn = $pane.find(".page_btn");
                var $noData = $pane.find(".nodata");
                var $switch = $pane.find(".switch_control");

                C.Product.GetHistoryDisplay(number).done(function (result) {
                    handleProductResult({
                        result: result,
                        $pane: $pane,
                        $content: $content,
                        $pageBtn: $pageBtn,
                        $noData: $noData,
                        $switch: $switch,
                        page: number,
                        hashName: "browsing",
                        needSuccess: true
                    });
                });
            }
        };
    }
})(window);
