var time = 0;
function HeaderInit() {
    syncResponsiveHeaderMenu();
    $(window).off("resize.layout7Menu").on("resize.layout7Menu", syncResponsiveHeaderMenu);

    var Mega_Menu = document.getElementById("Offcanvas_Mega_Menu");
    var observer = new MutationObserver(function (mutations) {
        var icon = document.getElementById("menuIcon");
        var narTopBar = document.getElementById("narTopBar");
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === 'class') {
                if (Mega_Menu.classList.contains('show')) {
                    icon.classList.remove("fa-bars");
                    icon.classList.add("fa-times-square"); // 切換成叉叉
                    if (narTopBar) narTopBar.classList.add("menu-open");
                } else {
                    icon.classList.remove("fa-times-square");
                    icon.classList.add("fa-bars"); // 切換回漢堡
                    if (narTopBar) narTopBar.classList.remove("menu-open");
                }
            }
        });
    });
    observer.observe(Mega_Menu, { attributes: true });

    
    // 加入購物車成功後自動打開購物車抽屜
    $(document)
        .off("productQuickCart:added.cartDrawer")
        .on("productQuickCart:added.cartDrawer", function () {
            var modalEl = document.getElementById("ShoppingCarModal");

            // 規格 modal 沒開著就直接開抽屜
            if (!modalEl || !modalEl.classList.contains("show")) {
                OpenCartDrawer();
                return;
            }

            // 等 modal 關完再開，避免它收尾時把 offcanvas 需要的捲動鎖一起清掉。
            // 但 hidden.bs.modal 不保證一定送得出來（例如 modal 實例狀態對不上），
            // 所以加一道逾時保險，兩邊只會執行一次。
            var done = false;
            var run = function () {
                if (done) return;
                done = true;
                OpenCartDrawer();
            };

            $(modalEl).one("hidden.bs.modal", run);
            setTimeout(run, 600);
        });

    /* ThreeSwiper */
    var threeSwiper = new Swiper(".threeSwiper", {
        a11y: true,
        slidesPerView: 1,
        loop: true,
        allowTouchMove: true,
        navigation: {
            nextEl: ".threeSwiper>.swiper-button-next",
            prevEl: ".threeSwiper>.swiper-button-prev",
        },
        keyboard: {
            enabled: true,
        },
        breakpoints: {
            768: {
                slidesPerView: 3,
                spaceBetween: 50,
                allowTouchMove: false,
            },
        },
    });

    /*Planning Swiper */
    var planningSwiper = new Swiper(".planningSwiper", {
        a11y: true,
        loop: true,
        slidesPerView: 1,
        breakpoints: {
            768: {
                direction: "vertical",
                watchSlidesProgress: true,
                allowTouchMove: false,
            },
        },
        keyboard: {
            enabled: true,
        },
        navigation: {
            nextEl: ".planningSwiper>.swiper-button-next",
            prevEl: ".planningSwiper>.swiper-button-prev",
        },
    });

    var planningThumbsSwiper = new Swiper(".planningThumbsSwiper", {
        a11y: true,
        allowTouchMove: true,
        direction: "vertical",
        effect: "coverflow",
        loop: true,
        slidesPerView: 2,
        centeredSlides: true,
        coverflowEffect: {
            rotate: 0,
            stretch: 0,
            depth: 150,
            scale: 0.9,
            modifier: 1,
            slideShadows: false,
        },
        keyboard: {
            enabled: true,
        },
        navigation: {
            nextEl: ".planning>.swiper-button-next",
            prevEl: ".planning>.swiper-button-prev",
        },
        thumbs: {
            swiper: planningSwiper,
        },
    });

    /*Outcome Swiper */
    var outcomeswiper = new Swiper(".outcomeSwiper", {
        a11y: true,
        slidesPerView: 2,
        grid: {
            rows: 2,
        },
        breakpoints: {
            768: {
                slidesPerView: 4,
            },
        },
        keyboard: {
            enabled: true,
        },
        pagination: {
            el: ".outcomeSwiper .swiper-pagination",
        },
        navigation: {
            nextEl: ".outcomeSwiper>.swiper-button-next",
            prevEl: ".outcomeSwiper>.swiper-button-prev",
        },
    });

    $(window).scroll(function () {
        $('.hideme:not(.show)').each(function (i) {
            var $self = $(this);
            var bottom_of_object = $self.offset().top + $self.outerHeight();
            var bottom_of_window = $(window).scrollTop() + $(window).height();
            if (bottom_of_window > bottom_of_object) {
                $self.addClass("show");
                time = $self.data("swiper-slide-index") * 500;
                $self.delay(time).animate({ 'opacity': '1' }, 500);
            }
        });
    });

    /* TopBar 捲動狀態:header 橫幅滑出畫面後在 body 加上 scrolled,供各站後台自訂 CSS 取用 */
    var SCROLL_THRESHOLD = 40;
    var scrollTarget = document.querySelector(".image-wh, .one_swiper");

    if (scrollTarget && "IntersectionObserver" in window) {
        new IntersectionObserver(function (entries) {
            document.body.classList.toggle("scrolled", !entries[0].isIntersecting);
        }, { rootMargin: "-168px 0px 0px 0px" }).observe(scrollTarget);
    } else {
        var isScrolled = false;
        var updateScrolledState = function () {
            var shouldScroll = window.scrollY > SCROLL_THRESHOLD;
            if (shouldScroll !== isScrolled) {
                isScrolled = shouldScroll;
                document.body.classList.toggle("scrolled", isScrolled);
            }
        };
        window.addEventListener("scroll", updateScrolledState, { passive: true });
        updateScrolledState();
    }
}

function OpenCartDrawer() {
    var panel = document.getElementById("Car_Dropdown");
    if (!panel || !panel.classList.contains("offcanvas")) return;
    bootstrap.Offcanvas.getOrCreateInstance(panel).show();
}