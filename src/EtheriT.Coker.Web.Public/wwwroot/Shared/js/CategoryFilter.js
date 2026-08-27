/* CategoryFilter.js
 * 編輯器內容用的分類篩選（後台編輯器會移除 <script>，故行為集中於共用 JS）
 *
 * HTML 約定：
 *   篩選連結：<a href="#Tech" class="filter-btn">科技</a>
 *   卡片：    <div class="company-card cat-tech">
 *
 *   - 分類代碼取自連結 href 的 hash（或 data-filter 覆寫），與顯示文字無關，
 *     因此多語系換字不影響功能；hash 大小寫不拘，卡片 class 一律小寫。
 *   - 卡片以 cat-<代碼> class 標記，可同時掛多個代表跨分類。
 *   - 只承認連結宣告過的代碼，其餘 cat- 開頭的 class 一律忽略。
 *   - 保留字 all：href="#All" 代表顯示全部。
 *   - 網址 hash 若不屬於這組連結（或沒有 hash），該組退回顯示全部。
 */
(function (w) {
    "use strict";

    var ALL = "all";
    var CATEGORY_PREFIX = "cat-";

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

    // 這組連結宣告的所有分類代碼（不含保留字 all）
    function getCategoryKeys($links) {
        var keys = [];

        $links.each(function () {
            var key = getLinkKey($(this));
            if (key && key !== ALL && keys.indexOf(key) < 0) keys.push(key);
        });

        return keys;
    }

    // hash 不是這組連結持有的代碼時（含沒有 hash），視為顯示全部
    function resolveKey($links, hashKey) {
        var keys = $links.map(function () {
            return getLinkKey($(this));
        }).get();

        return keys.indexOf(hashKey) >= 0 ? hashKey : ALL;
    }

    function toCardSelector(keys) {
        return keys.map(function (key) {
            return "." + CATEGORY_PREFIX + key;
        }).join(",");
    }

    // 找出這組連結要控制的卡片：
    // 1. 連結容器有 data-filter-target 就用該選擇器
    // 2. 否則往上找到第一個含有分類 class 的祖先，取其底下全部卡片
    function findCards($nav, keys) {
        if (keys.length === 0) return $();

        var selector = toCardSelector(keys);
        var target = $nav.attr("data-filter-target");
        if (target) return $(target).find(selector);

        var $scope = $nav.parent();
        while ($scope.length > 0 && $scope[0] !== document) {
            var $found = $scope.find(selector);
            if ($found.length > 0) return $found;
            $scope = $scope.parent();
        }

        return $();
    }

    // 讀出這張卡片實際持有的分類（可同時屬於多個）
    function getCardKeys($card, keys) {
        return keys.filter(function (key) {
            return $card.hasClass(CATEGORY_PREFIX + key);
        });
    }

    function buildItems($cards, keys) {
        var items = [];

        $cards.each(function () {
            var $card = $(this);
            items.push({
                categories: getCardKeys($card, keys),
                $target: $card
            });
        });

        return items;
    }

    function applyFilter(items, key) {
        items.forEach(function (item) {
            var isMatch = (key === ALL) || (item.categories.indexOf(key) >= 0);
            item.$target.toggleClass("show", isMatch).toggleClass("hide", !isMatch);
        });
    }

    function collectNavs($links) {
        var navs = [];

        $links.each(function () {
            var nav = $(this).closest(".nav-container")[0] || this.parentNode;
            if (nav && navs.indexOf(nav) < 0) navs.push(nav);
        });

        return navs;
    }

    function syncGroup($links, items) {
        var key = resolveKey($links, getHashKey());

        $links.each(function () {
            var $link = $(this);
            $link.toggleClass("active", getLinkKey($link) === key);
        });

        applyFilter(items, key);
    }

    function CategoryFilterInit(root) {
        var $root = root ? (root.jquery ? root : $(root)) : $(document);
        var $all = $root.is(".filter-btn") ? $root : $root.find(".filter-btn");

        if ($all.length === 0) return;

        collectNavs($all).forEach(function (nav) {
            var $nav = $(nav);
            if ($nav.data("categoryFilterInited")) return;

            var $links = $nav.find(".filter-btn");
            var keys = getCategoryKeys($links);
            var $cards = findCards($nav, keys);
            if ($cards.length === 0) return;

            var items = buildItems($cards, keys);

            // 點擊只負責改 hash，實際切換交給 hashchange，
            // 讓上一頁/下一頁與直接輸入網址走同一條路徑
            $(w).on("hashchange", function () {
                syncGroup($links, items);
            });

            syncGroup($links, items);
            $nav.data("categoryFilterInited", true);
        });
    }

    w.CategoryFilterInit = CategoryFilterInit;
})(window);