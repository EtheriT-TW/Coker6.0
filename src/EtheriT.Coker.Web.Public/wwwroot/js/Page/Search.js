function PageReady() {
    $(".btn_sort_price").on("click", SortByPrice);
    $(".btn_typography").on("click", Typography);

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

    $("#Search_Result > div").toggleClass('row row-cols-1 row-cols-md-4');

    var $fig_div = $("#Search_Result > div > div > figure > div");
    $fig_div.toggleClass('row g-0 showbycolumn');
    $fig_div.children('div').toggleClass('col-md-4');
    $fig_div.children('figcaption').toggleClass('col');
}