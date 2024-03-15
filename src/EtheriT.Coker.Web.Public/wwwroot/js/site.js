var OrgName = "Page", LayoutType = 0, SiteId = 0, IsFaPage = true;

function ready() {

    const $conten = $("#main");
    const $parentConten = $("#ParentNode");
    const $PostCSS = $("#PostCSS");
    $("link").each(function () {
        var $self = $(this);
        if ($self.data("orgname") != undefined) {
            OrgName = $self.data("orgname");
        }
        if ($self.data("layouttype") != undefined) {
            LayoutType = $self.data("layouttype");
        }
        if ($self.data("siteid") != undefined) {
            SiteId = $self.data("siteid");
        }
        if ($self.data("isfapage") != undefined) {
            IsFaPage = $self.data("isfapage");
        }
    });
    const menuMouseover = function () {
        const item = $(this).find("img");
        if (!!$(item).data("mouseover"))
            $(item).attr("src", $(item).data("mouseover"));
    }
    const menuMouseout = function () {
        const item = $(this).find("img");
        if (!!$(item).data("mouseout"))
            $(item).attr("src", $(item).data("mouseout"));
    }
    $(".menu-item").on("mouseover", menuMouseover);
    $(".menu-item").on("mouseout", menuMouseout);
    $(".menu-item").on("focus", menuMouseover);
    $(".menu-item").on("blur", menuMouseout);
    if ($conten.length > 0) {
        let s = $conten.text().indexOf("&amp;") >= 0 ? Coker.stringManager.ReplaceAndSinge($conten.text()) : co.stringManager.htmlEncode($conten.html());
        let ele = document.createElement('span');
        ele.innerHTML = s;
        if ($parentConten.length > 0 && $parentConten.text().indexOf("subpage_content") >= 0) {
            let p = Coker.stringManager.ReplaceAndSinge($parentConten.text());
            let $pe = $('<div>');
            $pe[0].innerHTML = p;
            $pe.html($pe.text());
            $pe.find(".catalog_frame,.noInherit").remove();
            $pe.find(".subpage_content").replaceWith(ele.textContent || ele.innerText);
            ele.textContent = $pe.html();
        }
        $conten.html(ele.textContent || ele.innerText);
        $conten.find("[draggable]").removeAttr("draggable");
        $conten.removeClass("d-none");
    }
    if ($PostCSS.length > 0) {
        const $mainCss = $("#frameCss")
        let s = Coker.stringManager.ReplaceAndSinge($PostCSS.text());
        let ele = document.createElement('span');
        ele.innerHTML = s;
        $mainCss.text(ele.textContent || ele.innerText);
        $PostCSS.remove();
    }
    $(".editTime,.popular").appendTo($conten);
    $(".backstageType").remove();
    if ($(".one_swiper,.two_swiper,.four_swiper,.six_swiper,.picture-category").length > 0) SwiperInit({ autoplay: true });
    if ($(".masonry").length > 0) FrameInit();
    if ($(".type_change_frame").length > 0) ViewTypeChangeInit();
    if ($(".hover_mask").length > 0) HoverEffectInit();
    if ($(".catalog_frame").length > 0 || $(".menu_directory").length > 0) DirectoryGetDataInit();
    if ($(".sitemap_hierarchical_frame").length > 0) SitemapInit();
    if ($(".link_with_icon").length > 0) LinkWithIconInit();
    if ($(".anchor_directory").length > 0 || $(".anchor_title").length > 0) AnchorPointInit();
    if ($(".shareBlock").length > 0) ShareBlockInit();
    if ($(".ContactForm").length > 0) setContact();
    if ($(".BGCanvas").length > 0) setBGCanvas();
    if ($(".FlipBook").length > 0) FlipBookInit();
    if ($("body").width() < 992) $("#lanBar").before($("#layout4 #NavbarContent"));
    if ($(".container .qa").length > 0) {
        $(".container").each((i, e) => {
            var $c = $(e);
            if (typeof ($c.attr("id")) == "undefined" && $c.find("qa").length>0) {
                $c.setRandenId();
            }
            $c.find(".qa .collapse").each((j, c) => {
                $(c).attr("data-bs-parent", `#${$c.attr("id")}`);
                if (j != 0 || $c.find(".qa .collapse").length == 1) $(c).collapse("hide");
            });
        });
    }
    if (location.hash != "" && $(location.hash).length > 0) $(location.hash).goTo(45);
    _c.Search.Init("#Search");
    $(".nav-link").on("focus", function () {
        $(this).trigger("mouseover");
    });
    $(".dropdown-toggle").on("focus", function () {
        new bootstrap.Dropdown($(this)[0], {}).show();
    });
    $(".accesskey[href]").on("click", function (e) {
        const $self = $(this);
        $($self.attr("href")).goTo();
        return false;
    });
    $("#videoModal").on("hidden.bs.modal", function () {
        $(this).find("iframe").remove();
    });
    $(`[data-bs-target="#videoModal"]`).on("click", function () {
        const self = this;
        const $body = $("#videoModal .modal-body");
        const rx = /^.*(?:(?:youtu.be\/|v\/|vi\/|u\/w\/|embed\/)|(?:(?:watch)??v(?:i)?=|&v(?:i)?=))([^#&?]*).*/;
        let key = "";
        let url = $(self).attr("data-model-target");
        var r = url.match(rx);
        $body.find(".fa-duotone").removeClass("d-none");
        $body.find(".fa-3x").addClass("d-none");
        if (r != null && r.length > 0) key = r[1];
        if (key != "") {
            if ($body.find("iframe").length == 0) {
                const iframe = $(`<iframe allowfullscreen="allowfullscreen" rel="0" src="https://www.youtube.com/embed/${key}?autohide=1" class="h-100"></iframe>`);
                iframe.appendTo($body);
            }
            $body.find(".fa-duotone").addClass("d-none");
            $body.find(".fa-3x").addClass("d-none");
        } else {
            $body.find(".fa-duotone").addClass("d-none");
            $body.find(".fa-3x").addClass("d-none");
        }
    });

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

    $.cookie('Member_Name', "會員一", { path: '/' });

    typeof (PageReady) === "function" && PageReady();
    typeof (HeaderInit) === "function" && HeaderInit();
    typeof (FooterInit) === "function" && FooterInit();
    SideFloatingInit();

    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') $("#Cookie").toggleClass("show");
    else CheckToken();

    const enterAdModalEl = $('#EnterAdModal')
    var enteradid = enterAdModalEl.data("enteradid")
    var temp_idlist = typeof ($.cookie('EnterAd_Show')) == "undefined" ? [] : $.cookie('EnterAd_Show').split(",");
    if ($('#EnterAdModal').length > 0 && (typeof ($.cookie('EnterAd_Show')) == "undefined" || $.inArray(enteradid.toString(), temp_idlist) < 0)) {
        var enterAdModal = new bootstrap.Modal($("#EnterAdModal"))
        enterAdModal.show();
        enterAdModalEl.on('hidden.bs.modal', event => {
            temp_idlist.push(enteradid);
            $.cookie('EnterAd_Show', temp_idlist, { path: '/' });
        })
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
    if (RegisterModal != null) {
        RegisterModal.addEventListener('show.bs.modal', function (event) {
            NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
        })
        RegisterModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(RegisterForms, $InputRegisterVCode)
        })
    }
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
            $.cookie("Token", null, { path: '/' });
            CreateToken();
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
        $.ajax('/api/Captcha/Validate?id=' + $self.data("id") + '&code=' + code, {
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
}

function RegisterAction() {
    var registerModal = new bootstrap.Modal($("#RegisterModal"))
    registerModal.hide();
}

function NewCaptcha($self, $input, name = "") {
    if (!!!$self.data("id")) {
        $self.data("id", Math.floor(Math.random() * 10000));
        const $form = $self.parents("form")
        let captchaId = $form.find("[name='captchaId']");
        if (captchaId.length == 0) {
            captchaId = $(`<input type="hidden" name="captchaId" />`)
            $form.append(captchaId);
        }
        captchaId.val(name + $self.data("id"));
    }
    $self.attr('src', `/api/Captcha/index?id=${name}${$self.data("id")}&v=${Math.floor(Math.random() * 10000)}`);
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
            if (!!!str) return "";
            else {
                var s = str.replace(/&amp;/g, "&");
                if (s.indexOf("&amp;") > 0) return _c.stringManager.ReplaceAndSinge(s);
                else return s
            }
        },
        htmlEncode: function (text) {
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(text));
            return div.innerHTML;
        }
    }, Search: {
        Init: function (id) {
            const $e = $(id);
            const $b = $e.find(".dropdown-menu button");
            const $t = $e.find(".input_sear");
            const $t2 = $("#SearchInput");
            $e.data("sid", $b.first().data("id"));
            $b.on("click", function () {
                $e.data("sid", $(this).data("id"));
                if ($t.val() != "") window.location.href = `/${OrgName}/Search/Get/${$e.data("sid")}/${$t.val()}`;
            });
            $e.find(".btn_sear").on("click", function () {
                if ($t.val() == "") {
                    co.sweet.error("錯誤", "請輸入搜尋文字", null, false);
                } else {
                    window.location.href = `/${OrgName}/Search/Get/${$e.data("sid")}/${$t.val()}`;
                }
                return false;
            });
            if ($t2.length != 0) {
                $t2.on("keypress", function (e) {
                    if (e.which == 13) {
                        window.location.href = `/${OrgName}/Search/Get/0/${$t2.val()}`;
                    }
                });
            }
        }
    },
    isMobileDevice: function () {
        let mobileDevices = ['Android', 'webOS', 'iPhone', 'iPad', 'iPod', 'BlackBerry', 'Windows Phone']
        for (var i = 0; i < mobileDevices.length; i++) {
            if (navigator.userAgent.match(mobileDevices[i])) {
                //console.log("isMobileDevice: match " + mobileDevices[i]);
                return true;
            }
        }
        return false
    }
}
$.fn.extend({
    goTo: function (offset) {
        $('html, body').animate({ scrollTop: $(this).offset().top + (!!offset ? offset : 0) }, 0);
    },
    setRandenId: function(i) {
        const $self = $(this);
        let className = typeof ($self.attr('class')) != "undefined" && $self.attr('class') != "" ? $self.attr('class').split(/\s+/)[0]+"Id" : "";
        let order = !!i ? i : 0;
        if (className == "") className = "RandenId";
        let id = className + (order == 0 ? "" : order);
        if ($(`#${id}`).length == 0) $self.attr("id", id);
        else $self.setRandenId(order+1);
    },
    getFormJson: function () {
        const form = $(this);
        const formDataObject = $(form).serializeArray();
        $(formDataObject).each(function(){
            const obj = this;
            const field = $(form).find(`[name="${obj.name}"]`);
            switch (field.attr("name")) {
                case "authentiity_token":
                case "captcha":
                    break;
                default:
                    switch (field.get(0).tagName) {
                        case "SELECT":
                            obj.title = field.nextAll("label").text().trim();
                            obj.value = field.find(`option:selected`).text().trim();
                            break;
                        default:
                            switch (field.attr("type")) {
                                case "radio":
                                case "checkbox":
                                    obj.title = field.parents(".d-flex").prevAll(".title").text().trim();
                                    obj.value = "";
                                    $(form).find(`[name="${obj.name}"]:checked`).each(function () {
                                        obj.value += $(this).nextAll("label").text().trim()+" ,";
                                    });
                                    obj.value = obj.value.substring(0, obj.value.length-2);
                                    break;
                                default:
                                    obj.title = field.nextAll("label").text().trim();
                                    break;
                            }
                            break;
                    }
                    break
            }
        });
        return formDataObject;
    }
});
let _c = Coker;
let co = _c;