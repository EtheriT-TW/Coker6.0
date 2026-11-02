(function (w, $) {
    "use strict";

    if (!$) return;

    var ROOT_SELECTOR = "#ProductRecommendations";
    var RECOMMEND_COUNT = 10;
    var currentProductId = null;
    var currentRequest = null;
    // 滑鼠移動超過這個距離才算拖曳，避免單純點擊被誤判
    var DRAG_THRESHOLD = 5;

    function buildCard(item) {
        var product = item.product || {};
        var title = product.title || item.title || "";

        var $card = $("<a>", {
            "class": "product-recommend-card text-decoration-none",
            href: item.link,
            title: title
        });

        var $image = $("<div>", { "class": "product-recommend-image" }).appendTo($card);
        $("<img>", {
            src: product.mainImage || "/images/noImg.jpg",
            alt: title,
            loading: "lazy"
        }).appendTo($image);

        $("<div>", { "class": "product-recommend-title" }).text(title).appendTo($card);

        var priceHtml = w.DirectoryPrice && typeof w.DirectoryPrice.buildHtml === "function"
            ? w.DirectoryPrice.buildHtml(product)
            : "";
        if (priceHtml) {
            $("<div>", { "class": "product-recommend-price" }).html(priceHtml).appendTo($card);
        }

        return $("<li>", { "class": "product-recommend-item" }).append($card);
    }

    function bindMouseDrag($list) {
        if ($list.data("dragBound")) return;
        $list.data("dragBound", true);

        var list = $list[0];
        var startX = 0;
        var startScrollLeft = 0;
        var isPointerDown = false;
        var hasDragged = false;

        // 關掉瀏覽器原生的圖片／連結拖曳，才不會拖出半透明圖片
        $list.on("dragstart.recommendDrag", "a, img", function (e) {
            e.preventDefault();
        });

        // 只處理滑鼠左鍵；手機觸控維持瀏覽器原生捲動
        $list.on("pointerdown.recommendDrag", function (e) {
            var pointer = e.originalEvent;
            if (pointer.pointerType !== "mouse" || pointer.button !== 0) return;
            isPointerDown = true;
            hasDragged = false;
            startX = pointer.clientX;
            startScrollLeft = list.scrollLeft;
        });

        $(document).on("pointermove.recommendDrag", function (e) {
            if (!isPointerDown) return;
            var deltaX = e.originalEvent.clientX - startX;
            if (!hasDragged && Math.abs(deltaX) < DRAG_THRESHOLD) return;
            if (!hasDragged) {
                hasDragged = true;
                $list.addClass("is-dragging");
            }
            list.scrollLeft = startScrollLeft - deltaX;
        });

        $(document).on("pointerup.recommendDrag pointercancel.recommendDrag", function () {
            if (!isPointerDown) return;
            isPointerDown = false;
            $list.removeClass("is-dragging");
        });

        // 拖曳放開時瀏覽器仍會觸發 click，攔下來避免誤開商品頁
        $list.on("click.recommendDrag", "a", function (e) {
            if (!hasDragged) return;
            e.preventDefault();
            hasDragged = false;
        });
    }

    function render($root, items) {
        var $list = $root.find(".product-recommend-list").empty();
        items.forEach(function (item) {
            $list.append(buildCard(item));
        });
        bindMouseDrag($list);
        // 先顯示再歸零；隱藏中（d-none）設定捲動位置會被瀏覽器忽略
        $root.toggleClass("d-none", items.length === 0);
        $list.scrollLeft(0);
    }

    function load(productId) {
        var $root = $(ROOT_SELECTOR);
        var id = Number(productId);

        // 後台關閉模組時頁面沒有容器；同一個商品重複 render 時不重抓
        if (!$root.length || !id || id === currentProductId) return;
        if (!w.Coker || !w.Coker.Product || typeof w.Coker.Product.GetRecommendations !== "function") return;

        currentProductId = id;
        if (currentRequest && typeof currentRequest.abort === "function") currentRequest.abort();
        $root.addClass("d-none");

        currentRequest = w.Coker.Product.GetRecommendations(id, RECOMMEND_COUNT)
            .done(function (result) {
                if (id !== currentProductId) return;
                render($root, result && Array.isArray(result.items) ? result.items : []);
            })
            .fail(function () {
                if (id !== currentProductId) return;
                render($root, []);
            });
    }

    w.ProductRecommendations = { load: load };
})(window, window.jQuery);