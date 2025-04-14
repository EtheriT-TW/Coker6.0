var time = 0;
function HeaderInit() {
    const $contain = $("body #main");
    $("#breadcrumb").prependTo($contain);
    $("body.home #breadcrumb").remove();

    var Mega_Menu = document.getElementById("Offcanvas_Mega_Menu");
    var observer = new MutationObserver(function (mutations) {
        var icon = document.getElementById("menuIcon");
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === 'class') {
                if (Mega_Menu.classList.contains('show')) {
                    icon.classList.remove("fa-bars");
                    icon.classList.add("fa-times-square"); // 切換成叉叉
                } else {
                    icon.classList.remove("fa-times-square");
                    icon.classList.add("fa-bars"); // 切換回漢堡
                }
            }
        });
    });
     observer.observe(Mega_Menu, { attributes: true }); 

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
}