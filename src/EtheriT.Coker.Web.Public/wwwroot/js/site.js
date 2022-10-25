function ready() {
    typeof (PageReady) === "function" && PageReady();
    HeaderInit();
    FooterInit();

    $.cookie('Purchased_Type_Quantity', 1);
    $.cookie('Purchased_Item_Quantity', 2);
    console.log($.cookie('Purchased_Type_Quantity'));
    console.log($.cookie('Purchased_Item_Quantity'));

    var mega_menu_height = $("header").css("height");
    $("body").css("padding-top", mega_menu_height);

    $(window).resize(function () {
        var mega_menu_height = $("header").css("height");
        $("body").css("padding-top", mega_menu_height);
    });

    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        $("#Cookie").toggleClass("show");
    }
    $(".btn_cookie_accept").on("click", cookie_accept);
    $(".btn_cookie_reject").on("click", cookie_reject);

    $("#Collapse_Button > i").on("click", function () {
        $("footer").toggleClass("footer_pack_up");
    });

    $("#Floating_Objects").on("click", function () {
        $('html,body').stop().animate({
            scrollTop: 0
        }, 0)
    });

    $("#btn_chat").on("click", function () {
        $("#Chatbot_Frame").toggleClass("show");
    });

    $(".btn_favorites").on("click", AddFavorites);

    window.onscroll = function () {
        scrollFunction();
    };

    $("#btn_gotop").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    });
}

function scrollFunction() {
    if (document.body.scrollTop > 350 || document.documentElement.scrollTop > 350) {
        $("#btn_gotop").css('display', 'block');
    } else {
        $("#btn_gotop").css('display', 'none');
    }
}

function cookie_accept() {
    $.cookie('cookie', 'accept', { expires: 7 });
    $("#Cookie").toggleClass("show");
}

function cookie_reject() {
    $.cookie('cookie', 'reject');
    $("#Cookie").toggleClass("show");
}

function AddFavorites() {
    var $self = $(this).children('i');
    var $self_parent = $self.parents("li").first();
    var toastLiveExample = document.getElementById('liveToast')
    var $toastBody = $("#liveToast>.toast-body");

    $self.toggleClass('fa-solid');
    $toastBody.empty();

    if ($self.hasClass('fav_item')) {
        $self_parent.remove();
    }

    if ($self.hasClass("fa-solid")) {
        $toastBody.append('<div>加入收藏成功</div>');
    } else {
        $toastBody.append('<div>移除收藏成功</div>');
    }

    var toast = new bootstrap.Toast(toastLiveExample)
    $('#Mask').toggleClass('show modal-backdrop');
    toast.show()
    setTimeout(function () {
        toast.hide();
        $('#Mask').toggleClass('show modal-backdrop');
    }, 1500);
}

var Coker = {
    sweet: {
        success: function (text, isHteml, action) {
            if (isHteml) {
                Swal.fire({
                    icon: 'success',
                    html: text,
                    showCancelButton: false,
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: '確定'
                }).then((result) => {
                    if (result.isConfirmed) {
                        action();
                    }
                })
            } else {
                Swal.fire({
                    icon: 'success',
                    text: text,
                    showCancelButton: false,
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: '確定'
                }).then((result) => {
                    if (result.isConfirmed) {
                        action();
                    }
                })
            }
        },
        confirm: function (title, text, isHteml, confirmtexet, cancanceltext, action) {
            if (isHteml) {
                Swal.fire({
                    title: title,
                    html: text,
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: confirmtexet,
                    cancelButtonText: cancanceltext
                }).then((result) => {
                    if (result.isConfirmed) {
                        action();
                    }
                })
            } else {
                Swal.fire({
                    title: title,
                    text: text,
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: confirmtexet,
                    cancelButtonText: cancanceltext
                }).then((result) => {
                    if (result.isConfirmed) {
                        action();
                    }
                })
            }
        }
    }
}