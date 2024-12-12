var guess_you_like_swiper

function PageReady() {
    ShoppingCarModalInit();

    $(".btn_sort_price").on("click", SortByPrice);

    PageDataSet();

    guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper > .swiper", {
        a11y: true,
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
        a11y: true,
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
        a11y: true,
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
        a11y: true,
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
        $("#ShoppingCarModal > .Modal").data("pid", 1);
        ModalDefaultSet();
    });

    $(".btn_view_type").on("click", GuessLikeTypeChange);

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

function PageDataSet() {
    /* 分類暫置資料 */
    var temp_tag_category = [
        {
            name: "智慧馬桶",
            link: "",
            total: 10
        }, {
            name: "單體馬桶",
            link: "",
            total: 30
        }, {
            name: "二件式馬桶",
            link: "",
            total: 17
        }, {
            name: "幼兒馬桶",
            link: "",
            total: 5
        }, {
            name: "奈米馬桶",
            link: "",
            total: 10
        }, {
            name: "蹲式馬桶",
            link: "",
            total: 10
        }, {
            name: "高背馬桶",
            link: "",
            total: 20
        }, {
            name: "馬桶蓋",
            link: "",
            total: 20
        }];
    temp_tag_category.forEach(function (tag) {
        $(".tag_category > ul").prepend(`<li class="col-auto p-0 mx-2"><a class="text-black" href="${tag.link}">${tag.name}(${tag.total})</a></li>`)
    })

    /* 展示用資料 */
    var temp_hot_products_data = [{
        link: "/lcb/product/toilet/2",
        img: "/upload/product/pro_pic_02.jpg",
        name: "上押一段省水馬桶",
        price: 11900,
        tags: [{ link: "", name: "一段省水" }, { link: "", name: "幼兒" }]
    },
    {
        link: "/lcb/product/toilet/3",
        img: "/upload/product/pro_pic_03.jpg",
        name: "C613NA 二段省水奈米高背單體馬桶",
        price: 26200,
        tags: [{ link: "", name: "二段省水" }, { link: "", name: "單體" }]
    },
    {
        link: "/lcb/product/toilet/4",
        img: "/upload/product/pro_pic_04.jpg",
        name: "C612NA 二段省水奈米高背單體馬桶",
        price: 25400,
        tags: [{ link: "", name: "二段省水" }, { link: "", name: "單體" }]
    },
    {
        link: "/lcb/product/toilet/5",
        img: "/upload/product/pro_pic_05.jpg",
        name: "C656NA 德瑞克Smart Ⅱ雙漩洗智慧馬桶",
        price: 105000,
        tags: [{ link: "", name: "一段省水" }, { link: "", name: "雙漩洗" }]
    }];
    SetProduct(temp_hot_products_data, $("#HotProductsSwiper > .swiper > .swiper-wrapper"));

    /* 展示用資料 */
    var temp_guesslike_data = [{
        link: "/lcb/product/toilet/1",
        img: "/upload/product/pro_pic_01.jpg",
        name: "CS230一段省水分離式幼兒馬桶",
        price: 9100,
        tags: [{ link: "", name: "一段省水" }, { link: "", name: "幼兒" }]
    },
    {
        link: "/lcb/product/toilet/2",
        img: "/upload/product/pro_pic_02.jpg",
        name: "上押一段省水馬桶",
        price: 11900,
        tags: [{ link: "", name: "一段省水" }]
    },
    {
        link: "/lcb/product/toilet/3",
        img: "/upload/product/pro_pic_03.jpg",
        name: "C613NA 二段省水奈米高背單體馬桶",
        price: 26200,
        tags: [{ link: "", name: "二段省水" }, { link: "", name: "單體" }]
    },
    {
        link: "/lcb/product/toilet/4",
        img: "/upload/product/pro_pic_04.jpg",
        name: "C612NA 二段省水奈米高背單體馬桶",
        price: 25400,
        tags: [{ link: "", name: "二段省水" }, { link: "", name: "單體" }]
    }];
    SetProduct(temp_guesslike_data, $("#GuessYouLikeSwiper > .swiper > .swiper-wrapper"));

    /* 展示用資料 */
    var temp_related_products_data = [{
        link: "/lcb/product/toilet/9",
        img: "/upload/product/pro_pic_09.jpg",
        name: "96657 單桿毛巾架",
        price: 2800,
        tags: []
    },
    {
        link: "/lcb/product/toilet/10",
        img: "/upload/product/pro_pic_10.jpg",
        name: "6602 衛生紙架",
        price: 1500,
        tags: []
    },
    {
        link: "/lcb/product/toilet/9",
        img: "/upload/product/pro_pic_09.jpg",
        name: "96657 單桿毛巾架",
        price: 2800,
        tags: []
    },
    {
        link: "/lcb/product/toilet/10",
        img: "/upload/product/pro_pic_10.jpg",
        name: "6602 衛生紙架",
        price: 1500,
        tags: []
    }];
    SetProduct(temp_related_products_data, $("#RelatedProductsSwiper > .swiper > .swiper-wrapper"));
}

// 資料放入Template
function SetProduct(datas, $place) {
    datas.forEach(function (data) {
        var prodcard = $($("#Template_ProdCard").html()).clone();
        prodcard.find("a").attr("href", data.link);
        prodcard.find("a").attr("alt", data.name);

        prodcard.find("img").attr("src", data.img);
        prodcard.find("img").attr("alt", data.name);

        if (data.tags.length > 0) {
            prodcard.find(".card-footer").prepend(`<ul class="pro_round_tag col p-0 m-0 d-flex flex-wrap overflow-hidden"></ul>`);
            data.tags.forEach(function (tag) {
                prodcard.find(".card-footer > ul").append(`<li class="btn_tag rounded-pill lh-base me-1 px-3 py-1"><a href="${tag.link}" alt="${tag.name}">${tag.name}</a></li>`)
            });
        }

        var temp_index = 3;
        var price = data.price.toString();
        while (temp_index < price.length) {
            price = price.substring(0, price.length - temp_index) + "," + price.substring(price.length - temp_index);
            temp_index += 4
        };

        prodcard.find(".price").text(price);

        prodcard.find(".card-text > a").text(data.name);

        $place.append(prodcard);
    });
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
        $frame_p.addClass("row row-cols-1 row-cols-sm-3 row-cols-md-4 m-0");
        $frame_p.children("div").each(function () {
            $(this).addClass("col")
            $(this).removeClass("swiper-slide")
        })
        $frame_btn.addClass("d-none")

    } else {
        $self.data("type", 1)
        $self.children("span").text("grid_view");
        guess_you_like_swiper.destroy(true, true);

        $frame_p.removeClass("row row-cols-1 row-cols-sm-3 row-cols-md-4 m-0");
        $frame_p.children("div").each(function () {
            $(this).removeClass("col")
            $(this).addClass("swiper-slide")
        })
        $frame_btn.removeClass("d-none")

        guess_you_like_swiper = new Swiper("#GuessYouLikeSwiper > .swiper", {
            a11y: true,
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