
function PageReady() {
    HeaderInit();
    $("#btn_gonews").on("click", GoNews);
    $(".btn_favorites").on("click", AddFavorites);

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

function AddFavorites() {
    var toastLiveExample = document.getElementById('liveToast')
    $(this).toggleClass('fa-solid');
    $("#liveToast>.toast-body").empty();
    if ((this).classList.contains('fa-solid')) {
        $("#liveToast>.toast-body").append('<div>加入收藏成功</div>');
    } else {
        $("#liveToast>.toast-body").append('<div>移除收藏成功</div>');
    }
    var toast = new bootstrap.Toast(toastLiveExample)
    $('#Mask').toggleClass('show modal-backdrop');
    toast.show()
    setTimeout(function () {
        toast.hide();
        $('#Mask').toggleClass('show modal-backdrop');
    }, 1500);
}
