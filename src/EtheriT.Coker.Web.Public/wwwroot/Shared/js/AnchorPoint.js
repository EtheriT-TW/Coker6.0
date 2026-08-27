function AnchorPointInit() {
    // 配置觀察選項（需要觀察哪些變動）
    const config = { childList: true, subtree: true };
    const $wrapper = $(`[data-gjs-type="wrapper"]`);
    if ($wrapper.length > 0 && typeof ($wrapper.data("observer")) == "undefined") {
        // 當觀察到變動時執行的回呼函數
        const callback = function (mutationsList, observer) {
            observer.disconnect();
            for (const mutation of mutationsList) {
                if (mutation.type === 'childList') {
                    //子節點被添加或移除了
                    $(".anchor_directory").removeClass("selectList");
                    AnchorPointInit();
                }
            }
            observer.observe($wrapper[0], config);
        };
        // 創建一個觀察器實例並將其與回呼函數關聯
        const observer = new MutationObserver(callback);

        // 開始觀察目標節點的指定變動
        observer.observe($wrapper[0], config);
        $wrapper.data("observer", observer);
    }
    $(".anchor_directory:not(.selectList)").each(function () {
        var $directory = $(this);
        $directory.children("ul").empty();
        $(".anchor_title").each(function () {
            var $self = $(this);
            var text = $self.text().indexOf('\n') > -1 ? $self.text().replace(/\n/g,"") : $self.text();
            $directory.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="#${$self.attr("id")}"><div class="p-2 px-4">${text}</div></a></li>`);
        });
        $directory.find('a[href^="#"]').on("click", function (event) {
            var id = $(this).attr("href");
            var target = $(id).offset().top - 50;
            if ($('nav').hasClass('position-fixed')) {
                target -= $('nav').outerHeight()-10;
            }
            $('html, body').animate({ scrollTop: target }, 0);
            event.preventDefault();
        });
        $directory.addClass("selectList");
    });
    $(window).off("resize.selectList").on("resize.selectList", (event) => {
        $(".selectList").each((i, e) => {
            const $self = $(e);
            if ($self.find("select").length == 0 || $self.find("select").children().length == 1) {
                const $list = $self.find("a");
                const $select = $("<select id='jampSelect'>");
                const $label = $("<label for='jampSelect' class='d-none'>").text("前往：");
                $self.find("select").remove();
                if ($self.find('a[href^="#"]').length > 0) {
                    $select.append($("<option>").val("").text("請選擇將前往的項目"))
                } else {
                    const text = $self.data("option-text") || "請選擇前往的頁面";
                    $select.append($("<option>").val("").text(text));
                }
                $list.each((j, a) => {
                    $select.append(
                        $("<option>").data("trigger", a).val(a.href).text($(a).text())
                    );
                });
                $select.on("change", function () {
                    const $o = $select.children(":selected");
                    if (/^#/.test($o.val())) $o.data("trigger").trigger("click");
                    else location.href = $o.val();

                    const anchor = $o.val().split('#')[1];
                    const $targetElement = $('#' + anchor);
                    if ($('nav').hasClass('position-fixed')) {
                        $('html, body').animate({
                            scrollTop: $targetElement.offset().top - $('nav').outerHeight() - 10
                        });
                    }
                });
                $self.append($label).append($select);
            }
            if ($(window).width() > 576) {
                $self.children().first().removeClass("d-none");
                $self.children("select").addClass("d-none");
            } else {
                $self.children().first().addClass("d-none");
                $self.children("select").removeClass("d-none");
            }
        });
    });
    $(window).trigger("resize.selectList");
}

/* ========================================================================
頁內錨點導覽 - 附加功能（aside 包裹 + 點擊/捲動高亮）
======================================================================== */
var ANCHOR_ASIDE_CLASS = "anchor_aside";
var ANCHOR_ACTIVE_CLASS = "anchor_active";
var anchorSpyObserver = null;
var anchorSpyVisible = [];

function AnchorPointAsideInit($root) {
    var $scope = ($root && $root.jquery && $root.length > 0) ? $root : $(document);

    // 1) 把 .anchor_directory 包進 <aside>（已包過就跳過），並改寫手機下拉行為
    $scope.find(".anchor_directory").addBack(".anchor_directory").each(function () {
        var $directory = $(this);
        if ($directory.parent("." + ANCHOR_ASIDE_CLASS).length === 0) {
            $directory.wrap(
                $("<aside>").addClass(ANCHOR_ASIDE_CLASS).attr("aria-label", "頁內導覽")
            );
        }
        AnchorPointSelectRebind($directory);
        AnchorPointIconInit($directory);
    });
    // 2) 點擊高亮：用事件委派，選單被重建後仍然有效
    $(document)
        .off("click.anchorActive")
        .on("click.anchorActive", '.anchor_directory a[href^="#"]', function () {
            AnchorPointSetActive($(this).attr("href"));
        });

    // 3) 捲動時同步高亮
    AnchorPointSpyInit();

    // 4) 進站網址若帶 hash，先標記
    if (location.hash) AnchorPointSetActive(location.hash);
}

function AnchorPointSetActive(hash) {
    if (!hash || hash === "#") return;
    $(".anchor_directory").each(function () {
        var $directory = $(this);
        var $links = $directory.find('a[href^="#"]');
        $links.closest("li").removeClass(ANCHOR_ACTIVE_CLASS);
        $links.filter(function () {
            return $(this).attr("href") === hash;
        }).closest("li").addClass(ANCHOR_ACTIVE_CLASS);

        // 手機版 select 同步選取
        var $option = $directory.children("select").children("option").filter(function () {
            return (this.value || "").split("#")[1] === hash.slice(1);
        });
        if ($option.length > 0) $directory.children("select").val($option.val());
    });
}

/* 手機版下拉：同頁錨點不寫入網址列，直接捲動 */
function AnchorPointSelectRebind($directory) {
    var $select = $directory.children("select");
    if ($select.length === 0) return;

    $select.off("change").on("change", function () {
        var value = $select.children(":selected").val() || "";
        if (value === "") return;

        var hashIndex = value.indexOf("#");
        if (hashIndex === -1) {
            location.href = value;          // 跨頁連結維持原本行為
            return;
        }

        // 同頁錨點：抓當下 DOM 裡對應的 <a>，沿用它已 preventDefault 的 click
        var hash = value.slice(hashIndex);
        $directory.find('a[href^="#"]').filter(function () {
            return $(this).attr("href") === hash;
        }).trigger("click");
    });
}

function AnchorPointSpyInit() {
    if (typeof IntersectionObserver === "undefined") return;
    if (anchorSpyObserver) anchorSpyObserver.disconnect();
    anchorSpyVisible = [];

    var offset = 60;
    if ($("nav").hasClass("position-fixed")) offset += $("nav").outerHeight();

    anchorSpyObserver = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            var i = anchorSpyVisible.indexOf(entry.target);
            if (entry.isIntersecting && i === -1) anchorSpyVisible.push(entry.target);
            if (!entry.isIntersecting && i > -1) anchorSpyVisible.splice(i, 1);
        });
        if (anchorSpyVisible.length === 0) return;

        var top = anchorSpyVisible.slice().sort(function (a, b) {
            return a.getBoundingClientRect().top - b.getBoundingClientRect().top;
        })[0];
        if (top && top.id) AnchorPointSetActive("#" + top.id);
    }, { rootMargin: "-" + offset + "px 0px -70% 0px", threshold: 0 });

    $(".anchor_title").each(function () {
        if (this.id) anchorSpyObserver.observe(this);
    });
}

// 掛在原本的 AnchorPointInit 之後執行，維持既有呼叫端不變
(function () {
    if (window.AnchorPointAsideHooked) return;
    window.AnchorPointAsideHooked = true;
    var originalInit = window.AnchorPointInit;
    if (typeof originalInit !== "function") return;
    window.AnchorPointInit = function ($root) {
        var result = originalInit.apply(this, arguments);
        AnchorPointAsideInit($root);
        return result;
    };
})();

var ANCHOR_ICON_CLASS = "anchor_icon";

/* 依 .anchor_title 的 data-anchor-icon 在目錄項目前插入圖示 */
function AnchorPointIconInit($directory) {
    $directory.find('a[href^="#"]').each(function () {
        var $link = $(this);
        var $box = $link.children("div").first();
        if ($box.length === 0 || $box.children("." + ANCHOR_ICON_CLASS).length > 0) return;

        var target = document.getElementById($link.attr("href").slice(1));
        var icon = target ? target.getAttribute("data-anchor-icon") : "";
        if (!icon) return;

        $box.prepend(
            $("<span>")
                .addClass("material-symbols-outlined " + ANCHOR_ICON_CLASS)
                .attr("aria-hidden", "true")
                .text(icon)
        );
    });
}