function PageReady() {
    ShoppingCarModalInit();
    $(".btn_sort_price").on("click", SortByPrice);
    $(".btn_typography").on("click", Typography);

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
        },
        on: {
            init: function () {
                // 初始化時處理無障礙屬性
                updateAccessibleSlides(this);
            }
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

    $("#Search_Result > div").toggleClass('row row-cols-2 row-cols-lg-4');

    var $fig_div = $("#Search_Result > div > div > figure > div");
    $fig_div.toggleClass('row g-0 showbycolumn');
    $fig_div.children('div').toggleClass('col-3');
    $fig_div.children('div').children('img').toggleClass('img-fluid w-75');
    $fig_div.children('figcaption').toggleClass('col');

    var $bottom_line = $("#Search_Result > div > div > figure > div > figcaption > .bottom_line");
    $bottom_line.children('.pro_tag').toggleClass('col col-md');
    $bottom_line.children('.priceframe ').toggleClass('col-auto col-md-auto');

}

function ProShare() {
    var $self = $(this);
    $self.toggleClass('show');
}