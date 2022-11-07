function PageReady() {
    ShoppingCarModalInit();

    $(".btn_sort_price").on("click", SortByPrice);

    var guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper > .swiper", {
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

    var hot_products_swiper = new Swiper("#HotProductsSwiper > .swiper", {
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

    var related_products_swiper = new Swiper("#RelatedProductsSwiper > .swiper", {
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

    var ads_swiper = new Swiper("#AdsSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_ads",
            prevEl: ".btn_swiper_prev_ads",
        },
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        }
    });

    $('.btn_share').cShare({
        description: 'jQuery plugin - C Share buttons',
        showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
    });
    $(".btn_share").hover(ProShare);
}

function SortByPrice() {
    var $sort_icon = $(".btn_sort_price > i");
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

function ProShare() {
    var $self = $(this);
    $self.toggleClass('show');
}