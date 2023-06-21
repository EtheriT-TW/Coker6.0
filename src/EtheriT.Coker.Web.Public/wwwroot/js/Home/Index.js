function PageReady() {
    ShoppingCarModalInit();

    if (typeof ($.cookie('EnterAd_Show')) == "undefined" && $("#EnterAdModal").length > 0) {
        var enterAdModal = new bootstrap.Modal($("#EnterAdModal"))
        enterAdModal.show();
    }

    $("#EnterAdModal button").on("click", function () {
        $.cookie('EnterAd_Show', true, { path: '/' });
    });

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

    $(".pro_link").on("click", function () {
        var $self = $(this);
        if ($self.parents(".frame").first().data("pid") != null) {
            ClickLog($self.parents(".frame").first().data("pid"));
        } else if ($self.parents("li").first().data("pid") != null) {
            ClickLog($self.parents("li").first().data("pid"));
        }
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
            el: "#BannerSwiper > .swiper_pagination_banner",
            clickable: true,
        },
        navigation: {
            nextEl: "#BannerSwiper > .swiper_button >.btn_swiper_next_banner",
            prevEl: "#BannerSwiper > .swiper_button >.btn_swiper_prev_banner",
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