function PageReady() {
    if (typeof (ShoppingCarModalInit) != "undefined") ShoppingCarModalInit();

    $(".btn_gonews").on("click", function () {
        $('html, body').animate({ scrollTop: $("#NewsSwiper").offset().top - ($("header").height() * 2) }, 0);
    });

    $(".btn_buy").on("click", function () {
        $frame = $(this).parents(".frame").first();
        $("#ShoppingCarModal > .Modal").data("pid", $frame.data("pid"));
        ModalDefaultSet();
    });

    $(".btn_addcart").on("click", function () {
        $frame = $(this).parents("li").first();
        $("#ShoppingCarModal > .Modal").data("pid", $frame.data("pid"));
        ModalDefaultSet();
    });

    var banner_swiper = new Swiper("#BannerSwiper > .swiper", {
        a11y: true,
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        },
        pagination: {
            el: "#BannerSwiper > .swiper_pagination_banner",
            clickable: true,
        },
        navigation: {
            nextEl: "#BannerSwiper > .swiper_button >.btn_swiper_next_banner",
            prevEl: "#BannerSwiper > .swiper_button >.btn_swiper_prev_banner",
        }
    });

    var new_swiper = new Swiper("#NewsSwiper > .swiper", {
        a11y: true,
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        },
        pagination: {
            el: "#NewsSwiper > .swiper_pagination_news",
            clickable: true,
        },
        breakpoints: {
            769: {
                slidesPerView: 2,
            }
        }
    });
}