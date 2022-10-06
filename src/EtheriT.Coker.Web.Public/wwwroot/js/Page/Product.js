function PageReady() {
    $(".btn-sort_price").on("click", SortByPrice);
    $(".btn-typography").on("click", Typography);

    var guess_you_like_swiper = new Swiper(".GuessYouLikeSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn-swiper_next_guessyoulike",
            prevEl: ".btn-swiper_prev_guessyoulike",
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
            nextEl: ".btn-swiper_next_hotproducts",
            prevEl: ".btn-swiper_prev_hotproducts",
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
            nextEl: ".btn-swiper_next_relatedproducts",
            prevEl: ".btn-swiper_prev_relatedproducts",
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
            nextEl: ".btn-swiper_next_ads",
            prevEl: ".btn-swiper_prev_ads",
        },
    });

    $(document).on('click', '.btn-count_plus', function () {
        $('.input_count').val(parseInt($('.input_count').val()) + 1);
    });
    $(document).on('click', '.btn-count_minus', function () {
        $('.input_count').val(parseInt($('.input_count').val()) - 1);
        if ($('.input_count').val() == 0) {
            $('.input_count').val(1);
        }
    });

    var $radio_btn = $('#Product > .content > .options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }
}

function SortByPrice() {
    var $sort_icon = $(".btn-sort_price > i");
    if ($sort_icon.hasClass('fa-arrows-up-down')) {
        $sort_icon.toggleClass('fa-arrows-up-down');
        $sort_icon.toggleClass('fa-caret-down');

    } else if ($sort_icon.hasClass('fa-caret-down')) {
        $sort_icon.toggleClass('fa-caret-down');
        $sort_icon.toggleClass('fa-caret-up');

    } else {
        $sort_icon.toggleClass('fa-caret-up');
        $sort_icon.toggleClass('fa-arrows-up-down');
    }
}

function Typography() {
    $(".btn-typography > i").toggleClass('fa-table-list');
}