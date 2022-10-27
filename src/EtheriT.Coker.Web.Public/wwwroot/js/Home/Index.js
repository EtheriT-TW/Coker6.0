function PageReady() {
    ShoppingCarModalInit();

    $(".btn_gonews").on("click", function () {
        $('html, body').animate({ scrollTop: $("#NewsSwiper").offset().top - ($("header").height() * 2) }, 0);
    });

    var banner_swiper = new Swiper("#BannerSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        },
        pagination: {
            el: ".swiper_pagination_banner",
            clickable: true,
        },
        navigation: {
            nextEl: ".btn_swiper_next_banner",
            prevEl: ".btn_swiper_prev_banner",
        }
    });

    var new_swiper = new Swiper("#NewsSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        },
        pagination: {
            el: ".swiper_pagination_news",
            clickable: true,
        },
        breakpoints: {
            769: {
                slidesPerView: 2,
            }
        }
    });
}