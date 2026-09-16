(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-products", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});
        var firstPageResults = {};

        function hasProductData(result, needSuccess) {
            var datas = result && Array.isArray(result.data) ? result.data : [];
            return needSuccess ? !!(result && result.success && datas.length > 0) : datas.length > 0;
        }

        function checkTabAvailability(request, tabSelector, cacheKey, needSuccess, completed) {
            request
                .done(function (result) {
                    firstPageResults[cacheKey] = result;
                    $(tabSelector).closest(".nav-item").toggleClass("d-none", !hasProductData(result, needSuccess));
                })
                .fail(function () {
                    // 查詢失敗不等同於沒有資料，保留入口讓使用者仍可重試。
                    $(tabSelector).closest(".nav-item").removeClass("d-none");
                })
                .always(completed);
        }

        function takeFirstPageResult(cacheKey, page) {
            if (page !== 1 || !Object.prototype.hasOwnProperty.call(firstPageResults, cacheKey)) return null;

            var result = firstPageResults[cacheKey];
            delete firstPageResults[cacheKey];
            return result;
        }

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

            var hasData = hasProductData(result, needSuccess);

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
            initTabVisibility: function () {
                var deferred = $.Deferred();
                var pending = 2;

                function completed() {
                    pending -= 1;
                    if (pending === 0) deferred.resolve();
                }

                checkTabAvailability(
                    C.Favorites.GetDisplay(1),
                    MemberPage.Selectors.favoriteTab,
                    "favorites",
                    false,
                    completed
                );
                checkTabAvailability(
                    C.Product.GetHistoryDisplay(1),
                    MemberPage.Selectors.historyTab,
                    "browsing",
                    true,
                    completed
                );

                return deferred.promise();
            },

            loadFavoritesPage: function (number) {
                var $pane = $(MemberPage.Selectors.favoritePane);
                var $content = $pane.find(".content");
                var $pageBtn = $pane.find(".page_btn");
                var $noData = $pane.find(".nodata");
                var $switch = $pane.find(".switch_control");
                var cachedResult = takeFirstPageResult("favorites", number);

                if (cachedResult) {
                    handleProductResult({
                        result: cachedResult,
                        $pane: $pane,
                        $content: $content,
                        $pageBtn: $pageBtn,
                        $noData: $noData,
                        $switch: $switch,
                        page: number,
                        hashName: "favorites",
                        needSuccess: false
                    });
                    return;
                }

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
                var cachedResult = takeFirstPageResult("browsing", number);

                if (cachedResult) {
                    handleProductResult({
                        result: cachedResult,
                        $pane: $pane,
                        $content: $content,
                        $pageBtn: $pageBtn,
                        $noData: $noData,
                        $switch: $switch,
                        page: number,
                        hashName: "browsing",
                        needSuccess: true
                    });
                    return;
                }

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
