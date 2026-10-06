// 與 Frame.css 的 0.5rem 欄距一致
const ALBUM_MASONRY_GUTTER = 8;
const ALBUM_MASONRY_SELECTOR = ".imageList.layout-masonry";
const ALBUM_MASONRY_SIZER_CLASS = "masonry-sizer";
// 2 倍圖若會在旁邊墊出超過「一欄寬 × 這個比例」的空白，就先讓後面的圖補位
const ALBUM_GAP_TOLERANCE = 0.1;
// 2 倍圖最多往後延幾個位置，避免和後台排的順序差太多
const ALBUM_MAX_DEFER = 3;

// masonry 以隱形的 .masonry-sizer 當作一欄寬；沒有它時會拿第一張圖的寬度，第一張是 2 倍就會算錯
function ensureAlbumSizer($grid) {
    if ($grid.children("." + ALBUM_MASONRY_SIZER_CLASS).length) return;
    $grid.prepend('<div class="' + ALBUM_MASONRY_SIZER_CLASS + '" aria-hidden="true"></div>');
}

// 與格列共用同一張佔位圖；LazyImage 未載入時退回站內預設圖
function showAlbumPlaceholder(image) {
    var placeholder = (window.Coker && window.Coker.LazyImage && window.Coker.LazyImage.placeholder)
        || "/images/noImg.jpg";
    // 佔位圖本身也失敗時不再重設，避免無限循環
    if (image.getAttribute("src") === placeholder) return;
    image.setAttribute("src", placeholder);
}

// 照 masonry 的規則算出格子會放在哪：1 欄放最矮的欄；跨欄找「範圍內最高那欄」最低的位置（同高取最左）。
// gap 是放下去後，範圍內較矮的欄被墊出來的空白高度
function measureAlbumPlacement(msnry, colYs, item) {
    item.getSize();
    var remainder = item.size.outerWidth % msnry.columnWidth;
    var mathMethod = remainder && remainder < 1 ? "round" : "ceil";
    var colSpan = Math.min(Math[mathMethod](item.size.outerWidth / msnry.columnWidth), msnry.cols);
    var best = null;
    for (var start = 0; start + colSpan <= colYs.length; start++) {
        var top = Math.max.apply(null, colYs.slice(start, start + colSpan));
        if (!best || top < best.top) best = { start: start, top: top };
    }
    var gap = 0;
    for (var i = 0; i < colSpan; i++) gap += best.top - colYs[best.start + i];
    return { start: best.start, top: best.top, colSpan: colSpan, gap: gap, height: item.size.outerHeight };
}

// 只調整 masonry 的擺放順序、不動 DOM：2 倍圖放下去會留空白時先往後延，
// 讓後面不會留空白的圖先補進空位；每次排版都依當下欄數重算，所以 4／3／2 欄各自成立
function orderAlbumItemsForGaps(msnry, items) {
    if (!msnry.cols || msnry.cols < 2) return items;
    var colYs = msnry.colYs.slice();
    var tolerance = msnry.columnWidth * ALBUM_GAP_TOLERANCE;
    var pending = items.map(function (item) { return { item: item, deferred: 0 }; });
    var ordered = [];
    while (pending.length) {
        var first = pending[0];
        var pick = 0;
        var placement = measureAlbumPlacement(msnry, colYs, first.item);
        if (placement.gap > tolerance && first.deferred < ALBUM_MAX_DEFER) {
            for (var i = 1; i < pending.length; i++) {
                var candidate = measureAlbumPlacement(msnry, colYs, pending[i].item);
                if (candidate.gap <= tolerance) {
                    pick = i;
                    placement = candidate;
                    break;
                }
            }
        }
        if (pick > 0) first.deferred += 1;
        for (var c = 0; c < placement.colSpan; c++) {
            colYs[placement.start + c] = placement.top + placement.height;
        }
        ordered.push(pending.splice(pick, 1)[0].item);
    }
    return ordered;
}

// 同一畫格內多次觸發只重排一次
function queueAlbumLayout($grid) {
    if ($grid.data("isLayoutQueued")) return;
    $grid.data("isLayoutQueued", true);
    window.requestAnimationFrame(function () {
        $grid.data("isLayoutQueued", false);
        if ($grid.data("masonry")) $grid.masonry("layout");
    });
}

// masonry 內建過濾用 instanceof HTMLElement 判斷，但後台 GrapesJS 畫布的元素由外層頁面建立，
// 屬於外層視窗的 HTMLElement，會被全部濾掉（抓到 0 格）；改用 matches 自行過濾
function filterAlbumItems(elems) {
    return Array.prototype.filter.call(elems, function (elem) {
        return elem.nodeType === 1 && elem.matches(".imageItem");
    });
}

function startAlbumMasonry($grid) {
    // 先加標記再初始化：CSS 需要這個 class 才會覆蓋 Bootstrap 的 position-relative
    $grid.addClass("is-masonry-ready");
    ensureAlbumSizer($grid);
    $grid.masonry({
        itemSelector: ".imageItem",
        columnWidth: "." + ALBUM_MASONRY_SIZER_CLASS,
        percentPosition: true,
        gutter: ALBUM_MASONRY_GUTTER,
        // 圖片陸續載入會多次重排，關閉滑動動畫避免畫面飄移
        transitionDuration: 0,
        // 先不排版，換掉過濾函式後再重新抓格子
        initLayout: false
    });
    var msnry = $grid.data("masonry");
    msnry._filterFindItemElements = filterAlbumItems;
    // 每次排版（含視窗縮放、圖片載入後重排）前先調整擺放順序，避免 2 倍圖旁邊留空白
    var originalLayoutItems = msnry.layoutItems;
    msnry.layoutItems = function (items, isInstant) {
        return originalLayoutItems.call(this, orderAlbumItemsForGaps(this, this._getItemsForLayout(items)), isInstant);
    };
    msnry.reloadItems();
    msnry.layout();
    // 綁定前就已經失敗的圖（complete 但沒有尺寸）也要補換
    $grid.find("img").each(function () {
        if (this.complete && this.naturalWidth === 0) showAlbumPlaceholder(this);
    });
}

function stopAlbumMasonry($grid) {
    // destroy 會清掉 masonry 寫在元素上的定位
    if ($grid.data("masonry")) $grid.masonry("destroy");
    $grid.children("." + ALBUM_MASONRY_SIZER_CLASS).remove();
    $grid.removeClass("is-masonry-ready");
}

// 依目前 class 決定啟動、關閉或重排；後台切換排版、相簿編輯增刪圖片、
// GrapesJS 重寫 class（洗掉 is-masonry-ready）都會走到這裡
function syncAlbumMasonry(grid, hasChildChange) {
    var $grid = $(grid);
    var isRunning = !!$grid.data("masonry");

    if (!$grid.hasClass("layout-masonry")) {
        if (isRunning || $grid.hasClass("is-masonry-ready")) stopAlbumMasonry($grid);
        return;
    }
    if (!isRunning) {
        startAlbumMasonry($grid);
        return;
    }
    if (!$grid.hasClass("is-masonry-ready")) $grid.addClass("is-masonry-ready");
    if (hasChildChange) {
        // GrapesJS 重畫子元素時會把 sizer 一起清掉，要補回來，否則 masonry 抓不到欄寬
        ensureAlbumSizer($grid);
        $grid.masonry("reloadItems");
    }
    queueAlbumLayout($grid);
}

function bindAlbumMasonry(grid) {
    var $grid = $(grid);
    if (!$grid.data("isMasonryBound")) {
        $grid.data("isMasonryBound", true);
        // load／error 不會冒泡，改用捕獲階段才收得到每張圖的事件
        grid.addEventListener("load", function (event) {
            if (event.target.tagName === "IMG" && $grid.data("masonry")) queueAlbumLayout($grid);
        }, true);
        // 找不到圖時比照格列（LazyImage）換成佔位圖；佔位圖載入後會觸發上方的 load 重排
        grid.addEventListener("error", function (event) {
            if (event.target.tagName === "IMG" && $grid.hasClass("layout-masonry")) {
                showAlbumPlaceholder(event.target);
            }
        }, true);
        new MutationObserver(function (mutations) {
            var hasChildChange = mutations.some(function (mutation) {
                return mutation.type === "childList";
            });
            syncAlbumMasonry(grid, hasChildChange);
        }).observe(grid, { attributes: true, attributeFilter: ["class"], childList: true });
    }
    syncAlbumMasonry(grid, false);
}

// 供後台 GrapesJS 外掛在切換排版後呼叫（畫布 iframe 內的全域函式）
function refreshAlbumMasonry(grid) {
    if (grid) bindAlbumMasonry(grid);
}

function initAlbumMasonry() {
    $(ALBUM_MASONRY_SELECTOR).each(function () {
        bindAlbumMasonry(this);
    });
}

function FrameInit() {
    initAlbumMasonry();
    $(".masonry").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            const $grid = $(this).find(".grid")
            var grid = $grid.masonry({
                itemSelector: '.grid-item',
                columnWidth: '.grid-item'
            });
            $self.data("isInit", true);
        }
    })
    $(".YTmodal_frame").each(function () {
        var $self = $(this);
        if (typeof ($self.data("isinit")) == "undefined") {
            // data-link / data-yt-title 是新版標準；舊屬性只保留過渡讀取，
            // 並立即正規化目前 DOM，避免其他程式繼續依賴 legacy attribute。
            var legacyLink = $self.attr("link");
            var videoLink = $self.attr("data-link") || legacyLink;
            var legacyTitle = $self.attr("yttitle");
            var videoTitle = $self.attr("data-yt-title") || legacyTitle;

            if (!videoLink) {
                $self.attr("data-isInit", true);
                return;
            }

            if (!$self.attr("data-link")) $self.attr("data-link", videoLink);
            if (!$self.attr("data-yt-title") && videoTitle) $self.attr("data-yt-title", videoTitle);
            $self.removeAttr("link yttitle");

            var vid = "";

            if (videoLink.includes("v=")) {
                var link = videoLink.substring("https://www.youtube.com/watch?".length);
                var vIndex = link.indexOf("v=") + 2;
                var ampersandIndex = link.indexOf("&", vIndex);
                vid = ampersandIndex >= 0 ? link.substring(vIndex, ampersandIndex) : link.substring(vIndex);
            } else {
                var link = videoLink.substring("https://www.youtube.com/embed/".length);
                var tagindex = link.indexOf("?");
                vid = tagindex >= 0 ? link.substring(0, tagindex) : link.substring(vIndex);
            }

            $self.attr("vid", vid);
            if (typeof ($self.attr("vid")) != "undefined") {
                vid = $self.attr("vid");
            } else {
                vid = videoLink.substring(videoLink.indexOf('v=') + 2);
                $self.attr("vid", vid);
            }
            if (videoLink.includes("youtu") && ($self.find("img").attr("src").startsWith("/images/") || $self.find("img").attr("src").startsWith("data:"))) {
                var img_link = `https://img.youtube.com/vi/${vid}/hqdefault.jpg`;
                $self.find("img").attr("src", img_link)
            }
            if (videoTitle) {
                $self.find("img").attr("alt", videoTitle)
            }
            $self.find("button").attr({ "data-bs-toggle": "modal", "data-bs-target": "#YTPreviewModal" })
            if ($("body").find("#YTPreviewModal").length == 0) {
                var html = `<div class="modal fade" id="YTPreviewModal" tabindex="-1" aria-labelledby="YTPreviewModal" aria-hidden="true">
                                                      <div class="modal-dialog modal-xl">
                                                        <div class="modal-content bg-black">
                                                            <div class="modal-header">
                                                            <button type="button" data-bs-dismiss="modal" aria-label="Close" class="bg-light btn-close rounded-circle"</button>
                                                            </div>
                                                            <div class="modal-body"></div>
                                                        </div>
                                                      </div>
                                                    </div>`
                $("body").prepend(html);

                document.getElementById('YTPreviewModal').addEventListener('hidden.bs.modal', function (event) {
                    $("#YTPreviewModal").find(".modal-body").empty();
                })
            }
            $self.find("button").on("click", function () {
                var temp_ytlink = "";
                if (!videoLink.includes("autoplay")) {
                    if (videoLink.includes("v=")) {
                        // 保留原有 embed link 取得方法
                        temp_ytlink = `https://www.youtube-nocookie.com/embed/${$self.attr("vid")}?&autoplay=1`;
                    } else {
                        temp_ytlink = `${videoLink}?&autoplay=1`;
                    }
                } else temp_ytlink = videoLink;
                console.log(temp_ytlink)
                $("#YTPreviewModal").find(".modal-body").append(`<iframe src="${temp_ytlink}" class="w-100 h-100" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>`)
            })
            $self.attr("data-isInit", true);
        }
    })
}
