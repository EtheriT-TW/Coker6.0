function PageReady() {
    $("#btn_gonews").on("click", GoNews);

    var banner_swiper = new Swiper("#BannerSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        autoplay: {
            delay: 2500,
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

    var new_swiper = new Swiper(".NewsSwiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        breakpoints: {
            769: {
                slidesPerView: 2,
            }
        }
    });
}

function GoNews() {
    $('html, body').animate({ scrollTop: $("#News").offset().top }, 0);
}
