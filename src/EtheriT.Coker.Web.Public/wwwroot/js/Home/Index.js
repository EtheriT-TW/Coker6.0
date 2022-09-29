
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
        slidesPerView: jQuery(window).width() > 768 ? 2: 1,
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
    });
}

function GoNews() {
    $('html, body').animate({ scrollTop: $("#News").offset().top }, 0);
}
