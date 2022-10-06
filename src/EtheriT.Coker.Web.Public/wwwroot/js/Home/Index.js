function PageReady() {
    $("#btn_gonews").on("click", GoNews);

    var banner_swiper = new Swiper(".mySwiper", {
        slidesPerView: 1,
        spaceBetween: 30,
        loop: true,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
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
