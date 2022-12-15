function PageReady() {
    ShoppingCarModalInit();

    $(".member_name").text($.cookie('Member_Name'));

    $(".btn_sort_price").on("click", SortByPrice);
    $(".btn_typography").on("click", Typography);
    $(".btn_remove_favorites ").on("click", RemoveFavorites);

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

    $(".pro_link").on("click", function () {
        var $self = $(this);
        if ($self.parents("figure").first().data("pid") != null) {
            ClickLog($self.parents("figure").first().data("pid"));
        }
    });
}

function RemoveFavorites() {
    var $thispro = $(this).parents("figure").parents("div").first();
    Coker.sweet.confirm("確定將商品從收藏移除？", "該商品將會從收藏中移除，且不可復原。", "確認移除", "取消", function () {
        $thispro.remove();
        Coker.sweet.success("成功移除商品", null, true);
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

function Typography() {
    var $btn_icon = $(".btn_typography > i");
    $btn_icon.toggleClass('fa-table-list');
    $btn_icon.toggleClass('fa-border-all');

    $("#Facorites_Result > div").toggleClass('row row-cols-2 row-cols-lg-4');

    var $fig_div = $("#Facorites_Result > div > div > figure > div");
    $fig_div.toggleClass('row g-0 showbycolumn');
    $fig_div.children('div').toggleClass('col-3');
    $fig_div.children('div').children('img').toggleClass('img-fluid w-75');
    $fig_div.children('figcaption').toggleClass('col');

    var $bottom_line = $("#Facorites_Result > div > div > figure > div > figcaption > .bottom_line");
    $bottom_line.children('.pro_tag').toggleClass('col col-md');
    $bottom_line.children('.priceframe ').toggleClass('col-auto col-md-auto');
}

function ProShare() {
    var $self = $(this);
    $self.toggleClass('show');
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