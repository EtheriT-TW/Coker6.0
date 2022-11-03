function ready() {
    $.cookie('Member_Name', "會員一", { path: '/' });
    typeof $.cookie('Purchased_Type_Quantity') == "undefined" && $.cookie('Purchased_Type_Quantity', 0, { path: '/' })
    typeof $.cookie('Purchased_Item_Quantity') == "undefined" && $.cookie('Purchased_Item_Quantity', 0, { path: '/' })

    typeof (PageReady) === "function" && PageReady();
    HeaderInit();
    FooterInit();

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

    if ($self.hasClass("fa-solid")) {
        Coker.sweet.confirm("確定將商品從收藏中移除？", "該商品將會從收藏中移除，且不可復原。", "確認移除", "取消", function () {
            $self.toggleClass('fa-solid');
            if ($self.hasClass('fav_item')) {
                $self_parent.remove();
                Coker.sweet.success("成功移除商品", null, true);
            }
        });
    } else {
        $self.toggleClass('fa-solid');
        Coker.sweet.success("成功加入收藏", null, true);
    }
}

var Coker = {
    timeout: {
        time: 1500
    },
    sweet: {
        success: function (text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.timeout.time }

            Swal.fire({
                icon: 'success',
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        error: function (title, text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.timeout.time }

            Swal.fire({
                icon: 'error',
                title: title,
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        confirm: function (title, text, confirmtexet, cancanceltext, action) {
            Swal.fire({
                icon: 'info',
                title: title,
                html: text,
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: confirmtexet,
                cancelButtonText: cancanceltext
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        }
    }
}