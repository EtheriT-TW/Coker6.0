var guess_you_like_swiper

function PageReady() {
    ShoppingCarModalInit();

    $(".btn_sort_price").on("click", SortByPrice);

    guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper > .swiper", {
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

    $(".btn_cart").on("click", function () {
        //$frame = $(this).parents(".frame").first();
        $("#ShoppingCarModal > .Modal").data("pid", 1);
        ModalDefaultSet();
    });

    $(".btn_view_type").on("click", GuessLikeTypeChange);

    $(".pro_link").on("click", function () {
        var $self = $(this);
        if ($self.parents("figure").first().data("pid") != null) {
            ClickLog($self.parents("figure").first().data("pid"));
        }
    });
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

function GuessLikeTypeChange() {
    var $self = $(this);
    $frame = $("#GuessYouLikeSwiper");
    $frame_p = $frame.find(".swiper-wrapper");
    $frame_btn = $frame.children(".swiper_button");

    if (parseInt($self.data("type"))) {
        $self.data("type", 0)
        $self.children("span").text("view_carousel");
        guess_you_like_swiper.destroy(true, true);

        $frame.children("div").addClass("swiper-wrapper");
        $frame.children("div").children("div").addClass("swiper-slide");
        $frame_p.removeClass("swiper-wrapper")
        $frame_p.addClass("row row-cols-1 row-cols-sm-3 row-cols-md-4 m-0");
        $frame_p.children("div").each(function () {
            $(this).addClass("col")
            $(this).removeClass("swiper-slide")
        })
        $frame_btn.addClass("d-none")

        guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper", {
            direction: "vertical",
            slidesPerView: "auto",
            freeMode: true,
            scrollbar: {
                el: ".swiper-scrollbar",
            },
            mousewheel: true,
        });
    } else {
        $self.data("type", 1)
        $self.children("span").text("grid_view");
        guess_you_like_swiper.destroy(true, true);

        $frame.children("div").removeClass("swiper-wrapper");
        $frame.children("div").children("div").removeClass("swiper-slide");
        $frame_p.addClass("swiper-wrapper")
        $frame_p.removeClass("row row-cols-1 row-cols-sm-3 row-cols-md-4 m-0");
        $frame_p.children("div").each(function () {
            $(this).removeClass("col")
            $(this).addClass("swiper-slide")
        })
        $frame_btn.removeClass("d-none")

        guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper > .swiper", {
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
    }
}

function ClickLog(Pid) {
    if ($.cookie("Token") != null) {
        Product.Log.Click({
            FK_Pid: Pid,
            FK_Tid: $.cookie("Token"),
            Action: 2,
        });
    }
}