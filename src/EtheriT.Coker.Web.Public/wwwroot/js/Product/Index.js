function ProductInit() {
    $(".btn_delecard").on("click", DeleteCard);

    var product_swiper = new Swiper(".ProductSwiper", {
        slidesPerView: jQuery(window).width() > 768 ? 4 : jQuery(window).width() > 576 ? 3: 2,
        spaceBetween: 15,
        loop: true,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        navigation: {
            nextEl: ".btn_swiper_next",
            prevEl: ".btn_swiper_prev",
        },
    });
}

function DeleteCard() {
}