function ready() {
    typeof (PageReady) === "function" && PageReady();
    $("#Collapse_Button > i").on("click", collapse);
    $(".btn_cookie_accept").on("click", cookie_accept);
    $(".btn_cookie_reject").on("click", cookie_reject);
    $("#Floating_Objects").on("click", function () {
        $('html,body').stop().animate({
            scrollTop: 0
        }, 0)
    });
    var swiper = new Swiper(".mySwiper", {
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
}

function collapse() {
    $("footer").toggleClass("footer_pack_up");
}

function cookie_accept() {
    $("#Cookie").toggleClass("cookie_close");
}

function cookie_reject() {
    $("#Cookie").toggleClass("cookie_close");
}