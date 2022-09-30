function PageReady() {
    var guess_you_like_swiper = new Swiper(".GuessYouLikeSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_guessyoulike",
            prevEl: ".btn_swiper_prev_guessyoulike",
        },
        breakpoints: {
            375: {
                slidesPerView: 2,
            },
            576: {
                slidesPerView: 3,
            },
            769: {
                slidesPerView: 4,
            }
        }
    });

    var hot_products_swiper = new Swiper(".HotProductsSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_hotproducts",
            prevEl: ".btn_swiper_prev_hotproducts",
        },
        breakpoints: {
            375: {
                slidesPerView: 2,
            },
            576: {
                slidesPerView: 3,
            },
            769: {
                slidesPerView: 4,
            }
        }
    });

    var related_products_swiper = new Swiper(".RelatedProductsSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_relatedproducts",
            prevEl: ".btn_swiper_prev_relatedproducts",
        },
        breakpoints: {
            375: {
                slidesPerView: 2,
            },
            576: {
                slidesPerView: 3,
            },
            769: {
                slidesPerView: 4,
            }
        }
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