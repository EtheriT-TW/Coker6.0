
function ready() {
    const $conten = $("Content");
    Coker.Token = {
        GetToken: function () {
            return $.ajax({
                url: "/api/Token/CreateToken",
                type: "POST",
            });
        },
        CheckToken: function (id) {
            return $.ajax({
                url: "/api/Token/CheckToken/",
                type: "GET",
                data: { id: id }
            });
        }
    };

    if ($conten.length > 0) {
        let s = Coker.stringManager.ReplaceAndSinge($conten.text());
        let ele = document.createElement('span');
        ele.innerHTML = s;
        $conten.html(ele.textContent || ele.innerText);
        $conten.find("[draggable]").removeAttr("draggable");
        $conten.removeClass("d-none");
        if ($(".one_swiper,.two_swiper,.four_swiper").length > 0) SwiperInit({ autoplay:true});
        if ($(".masonry").length > 0) FrameInit();
        if ($(".frame").length > 0) ViewTypeChangeInit();
    }

    $.cookie('Member_Name', "會員一", { path: '/' });

    typeof (PageReady) === "function" && PageReady();
    HeaderInit();
    FooterInit();
    SideFloatingInit();

    var mega_menu_height = $("header").css("height");
    $("body").css("padding-top", mega_menu_height);

    $(window).resize(function () {
        var mega_menu_height = $("header").css("height");
        $("body").css("padding-top", mega_menu_height);
    });

    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        $("#Cookie").toggleClass("show");
    } else {
        CheckToken();
    }

    SiteElementInit();

    $('.btn_refresh').on('click', function (event) {
        event.preventDefault();
        NewCaptcha($LoginImgCaptcha, $InputLoginVCode);
        NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
    });

    var LoginModal = document.getElementById('LoginModal')
    LoginModal.addEventListener('show.bs.modal', function (event) {
        NewCaptcha($LoginImgCaptcha, $InputLoginVCode);
    })
    LoginModal.addEventListener('hidden.bs.modal', function (event) {
        FormClear(LoginForms, $InputLoginVCode)
    })

    var RegisterModal = document.getElementById('RegisterModal')
    RegisterModal.addEventListener('show.bs.modal', function (event) {
        NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
    })
    RegisterModal.addEventListener('hidden.bs.modal', function (event) {
        FormClear(RegisterForms, $InputRegisterVCode)
    })

    $(".btn_login").on("click", function () {
        if (SiteFormCheck(LoginForms, $InputLoginVCode)) {
            CaptchaVerify($LoginImgCaptcha, $InputLoginVCode, LoginAction)
        } else {
            Coker.sweet.error("錯誤", "請確實填寫登入資料", null, true);
        }
    })

    $(".btn_register").on("click", function () {
        var passcheck = PassCheck()
        var formcheck = SiteFormCheck(RegisterForms, $InputRegisterVCode)
        $NewPass.keyup(PassCheck);
        $CheckPass.keyup(PassCheck);
        if (passcheck && formcheck) {
            CaptchaVerify($RegisterImgCaptcha, $InputRegisterVCode, RegisterAction)
        } else {
            Coker.sweet.error("錯誤", "請確實填寫註冊資料", null, true);
        }
    })

    $(".btn_cookie_accept").on("click", cookie_accept);
    $(".btn_cookie_reject").on("click", cookie_reject);

    $("#Collapse_Button > i").on("click", function () {
        $("footer").toggleClass("footer_pack_up");
    });

    $(".btn_favorites").on("click", AddFavorites);

    window.onscroll = function () {
        scrollFunction();
    };
}

function SiteElementInit() {
    $InputLoginVCode = $("#InputLoginVCode");
    $LoginImgCaptcha = $('#LoginImgCaptcha');
    LoginForms = $('#LoginForm');
    $LoginMail = $("#InputLoginMail")
    $LoginPass = $("#InputLoginPass")
    $LoginRemember = $("#CheckRemember")

    $InputRegisterVCode = $("#InputRegisterVCode");
    $RegisterImgCaptcha = $('#RegisterImgCaptcha');
    RegisterForms = $('#RegisterForm');
    $RegisterMail = $("#InputRegisterMail")
    $RegisterName = $("#InputRegisterName")
    $RegisterAccept = $("#CheckAccept")

    $NewPass = $("#InputRegisterNewPass");
    $NewPassFeedBack = $("#NewPassFeedBack");
    $CheckPass = $("#InputRegisterCheckPass");
    $CheckPassFeedBack = $("#CheckPassFeedBack");
}

function scrollFunction() {
    if (document.body.scrollTop > 350 || document.documentElement.scrollTop > 350) {
        $("#btn_gotop").css('display', 'block');
    } else {
        $("#btn_gotop").css('display', 'none');
    }
}

function cookie_accept() {
    $.cookie('cookie', 'accept', { expires: 7, path: '/' });
    $("#Cookie").toggleClass("show");
    CreateToken();
}

function cookie_reject() {
    $.cookie('cookie', 'reject');
    $("#Cookie").toggleClass("show");
}

function CreateToken() {
    Coker.Token.GetToken().done(function (result) {
        $.cookie("Token", result.token, { expires: 30, path: "/" })
    })
}

function CheckToken() {
    Coker.Token.CheckToken($.cookie("Token")).done(function (result) {
        if (!result.success) {
            CheckToken();
        }
    })
}

function AddFavorites() {
    var $self = $(this).children('i');
    var $self_parent = $self.parents("li").first();

    if ($self.hasClass("fa-solid")) {
        Coker.sweet.confirm("確定將商品從收藏中移除？", "該商品將會從收藏中移除，且不可復原。", "確認移除", "取消", function () {
            $self.removeClass('fa-solid');
            if ($self.hasClass('fav_item')) {
                $self_parent.remove();
                Coker.sweet.success("成功移除商品", null, true);
            }
        });
    } else {
        $self.addClass('fa-solid');
        Coker.sweet.success("成功加入收藏", null, true);
    }
}

function SiteFormCheck(Forms, $input) {
    $input.addClass('is-invalid');
    var Check = false;
    Array.from(Forms).forEach(form => {
        if (form.checkValidity()) {
            Check = true;
        }
        form.classList.add('was-validated');
    })
    return Check;
}

function CaptchaVerify($self, $input, SuccessAction) {
    var code = $input.val();
    if (code != "") {
        $.ajax('/api/Captcha/Validate?id=' + id + '&code=' + code, {
            dataType: "JSON",
            success: function (result) {
                if (result.success) {
                    $input.removeClass('is-invalid');
                    $input.addClass('is-valid');
                    SuccessAction();
                } else {
                    $input.addClass('is-invalid');
                    $input.siblings("div").addClass("me-4 pe-2");
                    NewCaptcha($self, $input)
                    $input.val("");
                    Coker.sweet.error("錯誤", "驗證碼錯誤", null, true);
                }
            }
        })
    } else {
        $input.addClass('is-invalid');
        $input.siblings("div").addClass("me-4 pe-2");
        NewCaptcha($self, $input)
        $input.val("");
        Coker.sweet.error("錯誤", "請確實填寫驗證碼", null, true);
    }
}

function LoginAction() {
    var loginModal = new bootstrap.Modal($("#LoginModal"))
    loginModal.hide();
    /*console.log("Login")*/
}

function RegisterAction() {
    var registerModal = new bootstrap.Modal($("#RegisterModal"))
    registerModal.hide();
    /*console.log("Register")*/
}

function NewCaptcha($self, $input) {
    id = Math.floor(Math.random() * 10000);
    $self.attr('src', '/api/Captcha/index?id=' + id);
    $input.val("");
}

function FormClear(form, $input) {
    form.removeClass('was-validated')
    $input.siblings("div").removeClass("me-4 pe-2")
    $input.removeClass('is-invalid');
    $LoginMail.val("");
    $LoginPass.val("");
    $LoginRemember.prop('checked', false);
    $RegisterMail.val("");
    $RegisterName.val("");
    $NewPass.val("");
    $NewPassFeedBack.val("");
    $CheckPass.val("");
    $CheckPassFeedBack.val("");
    $RegisterAccept.prop('checked', false);
    $input.val("");
}

function PassCheck() {
    var hasNum = /\d+/, hasLetter = /[a-zA-Z]+/, hasSpesym = /[^\a-\z\A-\Z0-9]/g;
    $NewPass.addClass("is-invalid");
    $CheckPass.addClass("is-invalid");
    if ($NewPass.val().length >= 6) {
        if (hasNum.test($NewPass.val()) && hasLetter.test($NewPass.val()) && !(hasSpesym.test($NewPass.val()))) {
            $NewPass.removeClass("is-invalid");
            $NewPass.addClass("is-valid");
            $NewPassFeedBack.text("　");
            if ($CheckPass.val() == $NewPass.val()) {
                $CheckPass.removeClass("is-invalid");
                $CheckPass.addClass("is-valid");
                $CheckPassFeedBack.text("　");
                return true;
            } else {
                $CheckPassFeedBack.text("密碼不相符");
            }
        } else {
            $NewPassFeedBack.text("密碼格式有誤");
            $CheckPassFeedBack.text("密碼格式有誤");
        }
    } else {
        $NewPassFeedBack.text("請輸入6個以上的字元");
        $CheckPassFeedBack.text("密碼格式有誤");
    }
    return false;
}

function ClickLog(Pid) {
    if ($.cookie("Token") != null) {
        Product.Log.Click({
            FK_Pid: Pid,
            FK_Tid: $.cookie("Token"),
            Action: 2,
        }).done(function () {
            ProdHistorySet();
        });

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
    },
    stringManager: {
        ReplaceAndSinge: function (str) {
            var s = str.replace(/&amp;/g, "&");
            if (s.indexOf("&amp;") > 0) return _c.Data.ReplaceAndSinge(s);
            else return s
        }
    }
}