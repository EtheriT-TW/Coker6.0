/* CategoryFilter.js
 * 編輯器內容用的分類篩選（後台編輯器會移除 <script>，故行為集中於共用 JS）
 *
 * HTML 約定：
 *   篩選連結：<a href="#Tech" class="filter-btn">科技</a>
 *   卡片：    <div class="company-card" data-categories="tech">
 *
 *   - 分類代碼一律取自 href 的 hash（或 data-filter 覆寫），與顯示文字無關，
 *     比對時統一轉小寫，因此多語系換字不影響功能。
 *   - 保留字 all：href="#All" 代表顯示全部。
 *   - data-categories 可用空白或逗號分隔多個分類。
 *   - 網址 hash 若不屬於這組連結（或沒有 hash），該組退回顯示全部。
 *
 * 卡片顯示與否透過 show / hide class 切換，樣式由該頁自身 CSS 定義。
 */
(function (w) {
    "use strict";

    var ALL = "all";

    function normalize(value) {
        return $.trim(value || "").toLowerCase();
    }

    // 分類代碼的來源：data-filter 優先，否則取 href 的 hash
    function getLinkKey($link) {
        var explicit = $link.attr("data-filter");
        if (explicit) return normalize(explicit);

        var href = $link.attr("href") || "";
        var hashIndex = href.indexOf("#");

        return hashIndex >= 0 ? normalize(href.substring(hashIndex + 1)) : "";
    }

    function getHashKey() {
        return normalize((w.location.hash || "").replace(/^#/, ""));
    }

    // hash 不是這組連結持有的代碼時（含沒有 hash），視為顯示全部
    function resolveKey($links, hashKey) {
        var keys = $links.map(function () {
            return getLinkKey($(this));
        }).get();

        return keys.indexOf(hashKey) >= 0 ? hashKey : ALL;
    }

    function applyFilter($cards, key) {
        $cards.each(function () {
            var $card = $(this);
            var categories = normalize($card.attr("data-categories")).split(/[\s,]+/);
            var isMatch = (key === ALL) || (categories.indexOf(key) >= 0);

            $card.toggleClass("show", isMatch).toggleClass("hide", !isMatch);
        });
    }

    // 找出這組連結要控制的卡片：
    // 1. 連結容器有 data-filter-target 就用該選擇器
    // 2. 否則往上找到第一個含有 [data-categories] 的祖先，取其底下全部卡片
    function findCards($nav) {
        var target = $nav.attr("data-filter-target");
        if (target) return $(target).find("[data-categories]");

        var $scope = $nav.parent();
        while ($scope.length > 0 && $scope[0] !== document) {
            var $found = $scope.find("[data-categories]");
            if ($found.length > 0) return $found;
            $scope = $scope.parent();
        }

        return $();
    }

    function collectNavs($links) {
        var navs = [];

        $links.each(function () {
            var nav = $(this).closest(".nav-container")[0] || this.parentNode;
            if (nav && navs.indexOf(nav) < 0) navs.push(nav);
        });

        return navs;
    }

    function syncGroup($links, $cards) {
        var key = resolveKey($links, getHashKey());

        $links.each(function () {
            var $link = $(this);
            $link.toggleClass("active", getLinkKey($link) === key);
        });

        applyFilter($cards, key);
    }

    function CategoryFilterInit(root) {
        var $root = root ? (root.jquery ? root : $(root)) : $(document);
        var $all = $root.is(".filter-btn") ? $root : $root.find(".filter-btn");

        if ($all.length === 0) return;

        collectNavs($all).forEach(function (nav) {
            var $nav = $(nav);
            if ($nav.data("categoryFilterInited")) return;

            var $links = $nav.find(".filter-btn");
            var $cards = findCards($nav);
            if ($cards.length === 0) return;

            // 點擊只負責改 hash，實際切換交給 hashchange，
            // 讓上一頁/下一頁與直接輸入網址走同一條路徑
            $(w).on("hashchange", function () {
                syncGroup($links, $cards);
            });

            syncGroup($links, $cards);
            $nav.data("categoryFilterInited", true);
        });
    }

    w.CategoryFilterInit = CategoryFilterInit;
})(window);