function PageReady() {
    var product_swiper = new Swiper(".GuessYouLikeSwiper", {
        slidesPerView: jQuery(window).width() > 768 ? 4 : jQuery(window).width() > 576 ? 3 : jQuery(window).width() >= 375 ? 2 : 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_guessyoulike",
            prevEl: ".btn_swiper_prev_guessyoulike",
        },
    });

    var product_swiper = new Swiper(".HotProductsSwiper", {
        slidesPerView: jQuery(window).width() > 768 ? 4 : jQuery(window).width() > 576 ? 3 : jQuery(window).width() >= 375 ? 2 : 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_hotproducts",
            prevEl: ".btn_swiper_prev_hotproducts",
        },
    });

    var product_swiper = new Swiper(".RelatedProductsSwiper", {
        slidesPerView: jQuery(window).width() > 768 ? 4 : jQuery(window).width() > 576 ? 3 : jQuery(window).width() >= 375 ? 2 : 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_relatedproducts",
            prevEl: ".btn_swiper_prev_relatedproducts",
        },
    });

    var ads_swiper = new Swiper(".AdsSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_ads",
            prevEl: ".btn_swiper_prev_ads",
        },
    });
}