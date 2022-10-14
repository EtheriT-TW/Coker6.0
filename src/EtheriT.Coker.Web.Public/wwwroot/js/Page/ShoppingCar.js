function PageReady() {

    var buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        keyboard: false,
        autoHeight: true,
        loop: false,
        simulateTouch: false,
        pagination: {
            el: ".swiper_pagination_buystep",
            clickable: true,
        },
        navigation: {
            nextEl: ".btn_swiper_next_buystep",
            prevEl: ".btn_swiper_prev_buystep",
        }
    });
}