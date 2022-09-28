function ready() {
    typeof (PageReady) === "function" && PageReady();
    typeof (ProductInit) === "function" && ProductInit();
    $("#Collapse_Button > i").on("click", collapse);
    $(".btn_cookie_accept").on("click", cookie_accept);
    $(".btn_cookie_reject").on("click", cookie_reject);
    $("#Floating_Objects").on("click", function () {
        $('html,body').stop().animate({
            scrollTop: 0
        }, 0)
    });
    $(".btn_favorites").on("click", AddFavorites);
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