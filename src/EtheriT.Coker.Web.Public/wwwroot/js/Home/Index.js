function PageReady() {
    ShoppingCarModalInit();

    if ($("#EnterAdModal").length > 0) {
        var enterAdModal = new bootstrap.Modal($("#EnterAdModal"))
        enterAdModal.show();
    }

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

    GuessLikeGet();
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

function GuessLikeGet() {
    Product.Get.RandomProd(3).done(function (result) {
        var index = 0;
        result.forEach(function (id) {
            Product.GetOne.Prod(id).done(function (result) {
                GuessLikeAdd(index, result);
                index += 1;
            })
        })
    }).fail(function () {
        console.log("Fail")
    });
}

function GuessLikeAdd(index, result) {
    var item = $("#Guess_Like > div > div > div > .frame").eq(index);
    var item_link = item.find(".pro_link"),
        item_name = item.find(".pro_name"),
        item_tag = item.find(".pro_tag"),
        item_image = item.find(".pro_image"),
        item_price = item.find(".pro_price");

    item.data("pid", result.id);
    item_link.attr("href", "/Toilet/" + result.id);
    item_name.text(result.title);
    item_tag.append("<li class='pe-2 align-self-center'><button class='bg-transparent border-0 gray_text text-decoration-underline ps-0'>" + "TAG" + "</button></li>");
    item_image.attr("src", "../images/product/pro_0" + result.id + ".png");
    item_price.text((result.discount > 0 ? result.discount : result.price).toLocaleString('en-US'));

    var item_list_ul = $("#Guess_Like > div > div > div");
    item_list_ul.eq(index).children("span").remove();
}