var OrgName = "Page", LayoutType = 0, SiteId = 0, IsFaPage = true, loginModal, otherLoginModal, registerModal, forgetModal, resetModal;

function ready() {
    const $conten = $("#main");
    const $parentConten = $("#ParentNode");
    const $PostCSS = $("#PostCSS");
    loginModal = $("#LoginModal").length > 0 ? new bootstrap.Modal($("#LoginModal")) : null;
    otherLoginModal = $("#OtherLoginModal").length > 0 ? new bootstrap.Modal($("#OtherLoginModal")) : null;
    registerModal = $("#RegisterModal").length > 0 ? new bootstrap.Modal($("#RegisterModal")) : null;
    forgetModal = $("#ForgetModal").length > 0 ? new bootstrap.Modal($("#ForgetModal")) : null;
    resetModal = $("#ResetModal").length > 0 ? new bootstrap.Modal($("#ResetModal")) : null;
    jqueryExtend();
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
    if (typeof (IsFaPage) == "string") IsFaPage = IsFaPage.toLowerCase() == "true";
    else IsFaPage = false;
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
        let s = $conten.text().indexOf("&amp;") >= 0 && $conten.text().indexOf("lt;") >= 0 ? Coker.stringManager.ReplaceAndSinge($conten.text()) : co.stringManager.htmlEncode($conten.html());
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
        if (location.pathname.toLowerCase().indexOf("/article/") >= 0) $conten.html($(`<div class="container">`).html(ele.textContent || ele.innerText));
        else if (location.pathname.toLowerCase().indexOf("/product/") >= 0) $conten.find("#ProductDescription > Content").html(ele.textContent || ele.innerText);
        else $conten.html(ele.textContent || ele.innerText);
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
    //swiper內的元素有一個以上就開啟自動輪播(autoplay:true)
    if ($(".one_swiper,.one_swiper_thumbs,.two_swiper,.three_swiper,.four_swiper,.five_swiper,.six_swiper,.picture-category").length > 0) SwiperInit({ autoplay: true });
    if ($(".marqueeSwiper").length > 0) MarqueeSwiper();
    if ($(".masonry").length > 0) FrameInit();
    if ($(".type_change_frame").length > 0) ViewTypeChangeInit();
    if ($(".hover_mask").length > 0) HoverEffectInit();
    if ($(".catalog_frame").length > 0 || $(".menu_directory").length > 0 || $(".advertise_directory").length > 0) DirectoryGetDataInit();
    if ($(".sitemap_hierarchical_frame").length > 0) SitemapInit();
    if ($(".link_with_icon").length > 0) LinkWithIconInit();
    if ($(".anchor_directory").length > 0 || $(".anchor_title").length > 0) AnchorPointInit();
    if ($(".shareBlock").length > 0) ShareBlockInit();
    if ($(".ContactForm").length > 0) {
        setContact();//From表單驗證碼
    }
    if ($(".BGCanvas").length > 0) setBGCanvas();
    if ($(".FlipBookItem").length > 0) FlipBookInit();
    if ($(".MapMessage").length > 0) MapMessage();
    if ($(".getlatlng").length > 0) GetLatLng();
    if ($("body").width() < 992) $("#lanBar").before($("#layout4 #NavbarContent"));
    if ($(".container .qa,.container-fluid .qa").length > 0) {
        $(".container,.container-fluid").each((i, e) => {
            var $c = $(e);
            if (typeof ($c.attr("id")) == "undefined" && $c.find("qa").length > 0) {
                $c.setRandenId();
            }
            $c.find(".qa .collapse").each((j, c) => {
                $(c).attr("data-bs-parent", `#${$c.attr("id")}`);
                //if (j != 0 || $c.find(".qa .collapse").length == 1) { //不隱藏第一個QA元素
                $(c).collapse("hide");
                //}
            });
        });
    }
    if (location.hash != "" && $(location.hash).length > 0) $(location.hash).goTo(45);
    if ($("video").length > 0) {
        $("video").each(function () {
            if (typeof (this.video) != "undefined") {
                this.video.pause();
                setTimeout(() => {
                    this.video.play().then((res) => {
                        console.log("playing start", res);
                    })
                        .catch((err) => {
                            console.log("error playing", err);
                        });
                }, 0);
            }
        });
    }
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
                const iframe = $(`<iframe allowfullscreen="allowfullscreen" rel="0" src="https://www.youtube-nocookie.com/embed/${key}?autohide=1" class="h-100"></iframe>`);
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
        CheckToken: function () {
            return $.ajax({
                url: "/api/Token/CheckToken/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET"
            });
        }
    };
    typeof (PageReady) === "function" && PageReady();
    typeof (HeaderInit) === "function" && HeaderInit();
    typeof (FooterInit) === "function" && FooterInit();
    SideFloatingInit();
    CreateToken();
    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') $("#Cookie").toggleClass("show");

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
        var adid = $("#EnterAdModal .modal-content").data("aid");
        if (adid != "undefined") {
            Advertise.ActivityExposure(adid).done(function (result) {
                //console.log(result)
            })
            $("#EnterAdModal img").on("click", function () {
                Advertise.ActivityClick(adid).done(function (result) {
                    //console.log(result)
                })
            });
        }
    }

    SiteElementInit();

    $('.btn_refresh').on('click', function (event) {
        event.preventDefault();
        NewCaptcha($LoginImgCaptcha, $InputLoginVCode);
        NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
        NewCaptcha($ForgetImgCaptcha, $InputForgetVCode);
        NewCaptcha($ResetImgCaptcha, $InputResetVCode);
    });

    var LoginModal = document.getElementById('LoginModal')
    if (LoginModal != null) {
        LoginModal.addEventListener('show.bs.modal', function (event) {
            NewCaptcha($LoginImgCaptcha, $InputLoginVCode);
        })
        LoginModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(LoginForms, $InputLoginVCode)
        })
    }

    var OtherLoginModal = document.getElementById('OtherLoginModal')
    if (OtherLoginModal != null) {
        OtherLoginModal.addEventListener('show.bs.modal', function (event) {
            loginModal.hide();
        })
        OtherLoginModal.addEventListener('hidden.bs.modal', function (event) {
        })
    }

    var ResetModal = document.getElementById('ResetModal')
    if (ResetModal != null) {
        ResetModal.addEventListener('show.bs.modal', function (event) {
            loginModal.hide();
            NewCaptcha($ResetImgCaptcha, $InputResetVCode);
        })
        ResetModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(ResetForms, $InputResetVCode)
        })
    }

    var RegisterModal = document.getElementById('RegisterModal')
    if (RegisterModal != null) {
        RegisterModal.addEventListener('show.bs.modal', function (event) {
            loginModal.hide();
            NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
        })
        RegisterModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(RegisterForms, $InputRegisterVCode)
        })
    }

    var ForgetModal = document.getElementById('ForgetModal')
    if (ForgetModal != null) {
        ForgetModal.addEventListener('show.bs.modal', function (event) {
            loginModal.hide();
            NewCaptcha($ForgetImgCaptcha, $InputForgetVCode);
        })
        ForgetModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(ForgetForms, $InputForgetVCode)
        })
    }
    $(".btn_login").on("click", function () {
        if (SiteFormCheck(LoginForms, $InputLoginVCode)) {
            CaptchaVerify($LoginImgCaptcha, $InputLoginVCode, LoginAction)
        } else {
            $InputLoginVCode.addClass('is-invalid');
            $InputLoginVCode.siblings("div").addClass("me-4 pe-2");
            NewCaptcha($LoginImgCaptcha, $InputLoginVCode)
            $InputLoginVCode.val("");
            Coker.sweet.error("錯誤", "請確實填寫登入資料", null, true);
        }
    })

    $(".btn_register").on("click", function () {
        var passcheck = PassCheck($("#InputRegisterNewPass"), $("#InputRegisterCheckPass"), $("#RegisterNewPassFeedBack"), $("#RegisterCheckPassFeedBack"))

        $("#InputRegisterNewPass").keyup(function () {
            PassCheck($("#InputRegisterNewPass"), $("#InputRegisterCheckPass"), $("#RegisterNewPassFeedBack"), $("#RegisterCheckPassFeedBack"));
        });

        $("#InputRegisterCheckPass").keyup(function () {
            PassCheck($("#InputRegisterNewPass"), $("#InputRegisterCheckPass"), $("#RegisterNewPassFeedBack"), $("#RegisterCheckPassFeedBack"));
        });

        var formcheck = SiteFormCheck(RegisterForms, $InputRegisterVCode)
        if (passcheck && formcheck) {
            if (!$RegisterAccept.prop("checked")) {
                NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
                Coker.sweet.error("錯誤", "請詳閱並同意會員條款", null, true);
            } else {
                CaptchaVerify($RegisterImgCaptcha, $InputRegisterVCode, RegisterAction)
            }
        } else {
            NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
            Coker.sweet.error("錯誤", "請確實填寫註冊資料", null, true);
        }
    })

    $(".btn_forget").on("click", function () {
        CaptchaVerify($ForgetImgCaptcha, $InputForgetVCode, ForgetAction)
    })

    $(".btn_backlogin").on("click", function () {
        loginModal.show();
        forgetModal.hide();
    })

    $(".btn_reset").on("click", function () {
        var passcheck = PassCheck($("#InputResetNewPass"), $("#InputResetCheckPass"), $("#ResetNewPassFeedBack"), $("#ResetCheckPassFeedBack"))

        $("#InputResetNewPass").keyup(function () {
            PassCheck($("#InputResetNewPass"), $("#InputResetCheckPass"), $("#ResetNewPassFeedBack"), $("#ResetCheckPassFeedBack"));
        });

        $("#InputResetCheckPass").keyup(function () {
            PassCheck($("#InputResetNewPass"), $("#InputResetCheckPass"), $("#ResetNewPassFeedBack"), $("#ResetCheckPassFeedBack"));
        });

        var formcheck = SiteFormCheck(ResetForms, $InputResetVCode)
        if (passcheck && formcheck) {
            CaptchaVerify($ResetImgCaptcha, $InputResetVCode, function () {
                ResetAction($(".btn_reset").data("forgetId"));
            })
        } else if (!passcheck) {
            NewCaptcha($ResetImgCaptcha, $InputResetVCode);
            Coker.sweet.error("錯誤", "密碼格式有誤", null, true);
        } else if (!formcheck) {
            NewCaptcha($ResetImgCaptcha, $InputResetVCode);
            Coker.sweet.error("錯誤", "驗證碼輸入錯誤", null, true);
        }
    })

    $("#ResetModal .btn_resetforget").on("click", function () {
        Coker.sweet.confirm(`寄送『密碼重設通知』至您的信箱："${$("#ResetForm").data("Email")}"?`, "", "是", "否", function () {
            Coker.sweet.loading();
            var data = {};
            data.Email = $("#ResetForm").data("Email");
            data.WebsiteId = SiteId;
            data.WebsiteLink = $(location).attr('origin');
            data.WebsiteName = $("meta[property='og:site_name'").attr("content");
            co.User.PasswordForget(data).done((result) => {
                if (result.success) {
                    Coker.sweet.success("系統將立即發送『密碼重設通知』信函至您所登錄之E-Mail中，此信函中包含您所設定之登入帳號(即E-mail)、密碼。請靜候密碼重設通知信。", null, false);
                    registerModal.hide();
                } else {
                    Coker.sweet.error(result.error, null, true);
                }
            })
        })
    })

    $(".btn_cookie_accept").on("click", cookie_accept);
    $(".btn_cookie_reject").on("click", cookie_reject);

    $("#Collapse_Button").on("click", function () {
        $("footer").toggleClass("footer_pack_up");
    });

    //$(".btn_favorites").on("click", AddFavorites);

    window.onscroll = function () {
        scrollFunction();
    };

    var insertdata_string = $(location).attr('search').substring(1, $(location).attr('search').length).split('&');
    var insertdata = {};
    $.each(insertdata_string, function (index, value) {
        insertdata[value.split('=')[0]] = value.split('=')[1];
    });
    if (typeof (insertdata["useraction"]) != "undefined") {
        switch (insertdata["useraction"]) {
            case "accountoping":
                if (typeof (insertdata["openid"]) != "undefined") {
                    Coker.sweet.loading();
                    co.User.AccountOpening(insertdata["openid"]).done(result => {
                        if (result.success) {
                            Coker.sweet.success("帳號成功開通，按下確定後即為您跳轉頁面！", function () {
                                window.location.href = $(location).attr('origin');
                            }, false);
                        } else {
                            if (result.message == "ReSendOrNot") {
                                var data = {};
                                data.OpenId = insertdata["openid"];
                                data.WebsiteId = SiteId;
                                data.WebsiteLink = $(location).attr('origin');
                                data.WebsiteName = $("meta[property='og:site_name'").attr("content");
                                co.sweet.confirm(result.error, "", "重新寄送", "取消", function () {
                                    Coker.sweet.loading();
                                    co.User.AccountReSendOpening(data).done(result => {
                                        if (result.success) {
                                            Coker.sweet.success("系統將立即重新發送『加入會員通知』信函至您所登錄之E-Mail中。請靜候開通帳號通知信。", null, false);
                                        } else {
                                            console.log(result.error);
                                            console.log(result.message);
                                        }
                                    });
                                });
                            } else {
                                co.sweet.error(result.error, "", null, false);
                            }
                        }
                    });
                }
                break;
            case "passwordforget":
                if (typeof (insertdata["forgetid"]) != "undefined") {
                    Coker.sweet.loading();
                    co.User.ForgetIdCheck(insertdata["forgetid"]).done(result => {
                        if (result.success) {
                            Swal.close();
                            resetModal.show();
                            $(".btn_reset").data("forgetId", insertdata["forgetid"])
                        } else {
                            co.sweet.error(result.error, "", null, false);
                        }
                    });
                }
                break;
        }
    }
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

    $InputForgetVCode = $("#InputForgetVCode");
    $ForgetImgCaptcha = $('#ForgetImgCaptcha');
    ForgetForms = $('#ForgetForm');

    $InputResetVCode = $("#InputResetVCode");
    $ResetImgCaptcha = $('#ResetImgCaptcha');
    ResetForms = $('#ResetForm');
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
}
function cookie_reject() {
    $.cookie('cookie', 'reject');
    $("#Cookie").toggleClass("show");
}
function CreateToken() {
    Coker.Token.GetToken().done(function (result) {
        localStorage.setItem("token", result.token);
        CheckToken();
    })
}
function CheckToken() {
    Coker.Token.CheckToken().done(function (result) {
        if (result.success) {
            if (result.isLogin && result.name != "") {
                $("#HiUser > .name").text(`${result.name} 您好!`);
            }
            console.log("userData:", result);
        }
    })
}

//function AddFavorites() {
//    var $self = $(this).children('i');
//    var $self_parent = $self.parents("li").first();

//    if ($self.hasClass("fa-solid")) {
//        Coker.sweet.confirm("確定將商品從收藏中移除？", "該商品將會從收藏中移除，且不可復原。", "確認移除", "取消", function () {
//            $self.removeClass('fa-solid');
//            if ($self.hasClass('fav_item')) {
//                $self_parent.remove();
//                Coker.sweet.success("成功移除商品", null, true);
//            }
//        });
//    } else {
//        $self.addClass('fa-solid');
//        Coker.sweet.success("成功加入收藏", null, true);
//    }
//}
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
    Coker.sweet.loading();
    loginModal.hide();
    var data = co.Form.getJson($("#LoginForm").attr("id"));
    data.WebsiteId = SiteId
    co.User.Login(data).done((result) => {
        if (result.success) {
            Coker.sweet.success("歡迎回來！", function () {
                location.reload()
            }, false);
        } else {
            switch (result.message) {
                case "重新寄送通知信":
                    Coker.sweet.confirm(result.error, "", result.message, "關閉視窗", function () {
                        Coker.sweet.loading();
                        data.WebsiteLink = $(location).attr('origin');
                        data.WebsiteName = $("meta[property='og:site_name'").attr("content");
                        data.RoleId = 2;
                        co.User.AccountReSendOpening(data).done(result => {
                            if (result.success) {
                                Coker.sweet.success("系統將重新發送『加入會員通知』信函至您所登錄之E-Mail中。請靜候開通帳號通知信。", null, false);
                            } else {
                                console.log(result.error);
                                console.log(result.message);
                            }
                        });
                    });
                    break;
                default:
                    Coker.sweet.error(result.error, null, false);
                    break;
            }
            console.log(result)
            NewCaptcha($LoginImgCaptcha, $InputLoginVCode);
        }
    })
}
function RegisterAction() {
    Coker.sweet.loading();
    var data = co.Form.getJson($("#RegisterForm").attr("id"));
    data.WebsiteId = SiteId;
    data.WebsiteLink = $(location).attr('origin');
    data.WebsiteName = $("meta[property='og:site_name'").attr("content");
    data.RoleId = 2;
    co.User.AddUser(data).done((result) => {
        if (result.success) {
            Coker.sweet.success("註冊成功，系統將立即發送『加入會員通知』信函至您所登錄之E-Mail中，您必須完成帳號開通程序後，才能登入網站與使用會員功能，此信函中包含您所設定之登入帳號(即E-mail)、密碼。請靜候開通帳號通知信。", null, false);
            registerModal.hide();
        } else {
            console.log(result)
            console.log(result.message)
            switch (result.message) {
                case "重新寄送通知信":
                    Coker.sweet.confirm(result.error, "", result.message, "關閉視窗", function () {
                        Coker.sweet.loading();
                        co.User.AccountReSendOpening(data).done(result => {
                            if (result.success) {
                                Coker.sweet.success("系統將重新發送『加入會員通知』信函至您所登錄之E-Mail中。請靜候開通帳號通知信。", null, false);
                            } else {
                                Coker.sweet.error("發生未知錯誤", "", null, false);
                                console.log(result.error);
                                console.log(result.message);
                            }
                        });
                    });
                    break;
                case "郵箱已存在":
                    Coker.sweet.info(result.error, null);
                    break;
                default:
                    Coker.sweet.error(result.error, null, true);
                    break;
            }
            NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
        }
    })
}
function ForgetAction() {
    Coker.sweet.loading();
    var data = co.Form.getJson($("#ForgetForm").attr("id"));
    data.WebsiteId = SiteId;
    data.WebsiteLink = $(location).attr('origin');
    data.WebsiteName = $("meta[property='og:site_name'").attr("content");
    co.User.PasswordForget(data).done((result) => {
        if (result.success) {
            Coker.sweet.success("系統將立即發送『密碼重設通知』信函至您所登錄之E-Mail中，此信函中包含您所設定之登入帳號(即E-mail)、密碼。請靜候密碼重設通知信。", null, false);
            registerModal.hide();
        } else {
            Coker.sweet.error(result.error, null, true);
            NewCaptcha($ForgetImgCaptcha, $InputForgetVCode);
        }
    })
}
function ResetAction(forgetid) {
    var data = co.Form.getJson($("#ResetForm").attr("id"));
    data.ForgetId = forgetid;
    data.WebsiteId = SiteId;
    co.User.PasswordChange(data).done((result) => {
        if (result.success) {
            Coker.sweet.success("密碼重置成功。", function () {
                window.location.href = $(location).attr('origin');
            }, false);
            registerModal.hide();
        } else {
            switch (result.message) {
                case "密碼錯誤":
                    Coker.sweet.confirm(result.error, "", "忘記密碼?", "確定", function () {
                        $("#ResetModal .btn_resetforget").click();
                    })
                    NewCaptcha($ResetImgCaptcha, $InputResetVCode);
                    break;
                default:
                    Coker.sweet.error(result.error, null, true);
                    NewCaptcha($ResetImgCaptcha, $InputResetVCode);
                    break;
            }
        }
    })
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

    $("#InputRegisterNewPass").val("");
    $("#RegisterNewPassFeedBack").val("");
    $("#InputRegisterCheckPass").val("");
    $("#RegisterCheckPassFeedBack").val("");

    $("#InputResetNewPass").val("");
    $("#ResetNewPassFeedBack").val("");
    $("#InputResetCheckPass").val("");
    $("#ResetCheckPassFeedBack").val("");

    $RegisterAccept.prop('checked', false);
    $input.val("");
}
function PassCheck($NewPass, $CheckPass, $NewPassFeedBack, $CheckPassFeedBack) {

    var hasNum = /[0-9]/, hasUpper = /[A-Z]/, hasLower = /[a-z]/, hasSpesym = /[^\a-\z\A-\Z0-9]/g;

    $NewPass.addClass("is-invalid");
    $CheckPass.addClass("is-invalid");

    var password = $NewPass.val();
    var check = 0;

    if (password.length >= 8) {
        if (password.length <= 32) {
            if (hasNum.test(password)) check += 1;
            if (hasUpper.test(password)) check += 1;
            if (hasLower.test(password)) check += 1;
            if (hasSpesym.test(password)) check += 1;

            if (check >= 3) {
                $NewPass.removeClass("is-invalid");
                $NewPass.addClass("is-valid");
                $NewPassFeedBack.text("　");
                if ($CheckPass.val() == password) {
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
            $NewPassFeedBack.text("請輸入32個以下的字元");
            $CheckPassFeedBack.text("密碼格式有誤");
        }
    } else {
        $NewPassFeedBack.text("請輸入8個以上的字元");
        $CheckPassFeedBack.text("密碼格式有誤");
    }
    return false;
}

var Coker = {
    timeout: {
        time: 1500
    },
    User: {
        AddUser: function (data) {
            return $.ajax({
                url: "/api/User/AddUser",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        AccountOpening: function (OpenId) {
            return $.ajax({
                url: "/api/User/AccountOpening/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { OpenId: OpenId },
            });
        },
        AccountReSendOpening: function (data) {
            return $.ajax({
                url: "/api/User/ReSendOpening",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        PasswordForget: function (data) {
            return $.ajax({
                url: "/api/User/PasswordForget",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        ForgetIdCheck: function (ForgetId) {
            return $.ajax({
                url: "/api/User/ForgetIdCheck/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { ForgetId: ForgetId },
            });
        },
        PasswordChange: function (data) {
            return $.ajax({
                url: "/api/User/PasswordChage",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        EmailChange: function (data) {
            return $.ajax({
                url: "/api/User/EmailChage",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Login: function (data) {
            return $.ajax({
                url: "/api/User/Login",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetUser: function (refreshToken) {
            return $.ajax({
                url: "/api/User/GetUserData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
            });
        },
        UserEdit: function (data) {
            return $.ajax({
                url: "/api/User/FrontUserEdit",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Logout: function () {
            return $.ajax({
                url: "/api/User/Logout",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                dataType: "json"
            });
        },
    },
    Favorites: {
        Add: function (Pid) {
            return $.ajax({
                url: "/api/Favorites/Add/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { Pid: Pid },
            });
        },
        GetDisplay: function () {
            return $.ajax({
                url: "/api/Favorites/GetDisplay/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
            });
        },
        Delete: function (Fid) {
            return $.ajax({
                url: "/api/Favorites/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { Fid: Fid },
            });
        },
        Check: function (Pid) {
            return $.ajax({
                url: "/api/Favorites/CheckIsFavorites/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { Pid: Pid },
            });
        },
    },
    Form: {
        getJson: function (id, isArrayType) {
            let form = document.getElementById(id);
            let formFields = new FormData(form);
            let isArray = typeof (isArrayType) == "undefined" ? false : isArrayType;
            let formDataObject = Object.fromEntries(Array.from(formFields.keys(), key => {
                const val = formFields.getAll(key)
                return [key, (isArray || val.length > 1) ? val : val.pop()]
            }));
            return formDataObject;
        },
        insertData: function (obj, $self) {
            if (typeof ($self) == "undefined" || $self == null) $self = $("form").first();
            else if (typeof ($self) == "string") $self = $($self);
            const formTypeSet = (type, $e, value) => {
                switch (type) {
                    case "zipcode":
                        co.Zipcode.setData({
                            el: $e,
                            addr: value
                        });
                        break;
                    case "date_range":
                        if (!!!$e.data('daterangepicker')) _c.Picker.Init($e);
                        if (!!obj[$e.data("start")] || !!obj[$e.data("end")]) {
                            $e.data('daterangepicker').setStartDate(obj[$e.data("start")]);
                            $e.data('daterangepicker').setEndDate(obj[$e.data("end")]);
                        } else $e.val("");
                        break;
                    case "date":
                        if (!!!$e.data('daterangepicker'))
                            _c.Picker.Init($e, { singleDatePicker: true, timePicker: false, locale: { format: 'YYYY/MM/DD' } });
                        $e.data('daterangepicker').setStartDate(value || Date.now());
                        break;
                    case "disabled":
                        $e.on("change", function () {
                            const checked = $(this).data("direct") == "reverse" ? !$(this).prop("checked") : $(this).prop("checked");
                            if (checked) $(`#${$(this).data("target")}`).attr("disabled", "disabled").val("");
                            else $(`#${$(this).data("target")}`).removeAttr("disabled")
                        });

                        if (!!$e.data("value")) {
                            let _v = $(`#${$e.data("target")}`).val();
                            if (typeof ($e.data("value")) == "number") _v = parseInt(_v);
                            else if (typeof ($e.data("value")) == "string") _v = _v.toString();
                            value = $e.data("direct") == "reverse" ? !($e.data("value") == _v) : $e.data("value") == _v;
                        }
                        $e.prop("checked", value);
                        $e.trigger("change");
                        break;
                    case "images":
                        if (!!!$e.data("init")) {
                            $e.ImageUploadModalClear();
                            $e.data("init", true)
                        }
                        co.File.getImgFile({ Sid: obj[$e.data("target")], Type: $e.data("image-type"), Size: $e.data("image-size") }).done(function (file) {
                            if (file.length > 0)
                                ImageUploadModalDataInsert($e, file[0].id, file[0].link, file[0].name)
                        });
                        break;
                    case "html":
                        $e.empty().html($("<div>").html(value).html());
                        break;
                    default:
                        $e.val(value);
                        break;
                }
            }
            for (const key in obj) {
                const $e = $self.find(`[name="${key}"]`);
                if ($e.length > 0) {
                    if (!!$e.data("form-type")) formTypeSet($e.data("form-type"), $e, obj[key])
                    else {
                        switch ($e[0].tagName) {
                            case "INPUT":
                                switch ($e.attr("type").toLowerCase()) {
                                    case "radio":
                                        $self.find(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                        break;
                                    case "checkbox":
                                        if (Array.isArray(obj[key])) {
                                            $(obj[key]).each(function (index, element) {
                                                $self.find(`[name="${key}"][value="${element}"]`).prop("checked", true);
                                            });
                                        } else $self.find(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                        break;
                                    case "datetime-local":
                                        $e.val(co.Date.GetDateTimeStr(obj[key]));
                                        break;
                                    default:
                                        $e.val(obj[key]);
                                        break;
                                }
                                break;
                            default:
                                $e.val(obj[key]);
                                break;
                        }
                    }
                }// else console.log(key);
            }
        },
    },
    sweet: {
        loading: function () {
            Swal.fire({
                title: "資料處理中",
                html: "已收到您填寫的資料，請稍後。",
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                },
                willClose: () => {
                }
            }).then((result) => {
            });
        },
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
                timer: closetime,
                allowOutsideClick: false,
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
                timer: closetime,
                allowOutsideClick: false,
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
                cancelButtonText: cancanceltext,
                allowOutsideClick: false,
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        info: function (title, action) {
            Swal.fire({
                icon: 'info',
                title: title,
                showConfirmButton: true,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: false,
                allowOutsideClick: false,
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        warning: function (title, text, action) {
            Swal.fire({
                title: title,
                text: text,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                confirmButtonText: "確定"
            }).then((result) => {
                if (typeof (action) == "function") action();
            });
            if (typeof (action) == "function")
                setTimeout(action, 3000);
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
    String: {
        generateRandomString: function (num) {
            const characters =
                'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
            let result1 = ' ';
            const charactersLength = characters.length;
            for (let i = 0; i < num; i++) {
                result1 +=
                    characters.charAt(Math.floor(Math.random() * charactersLength));
            }

            return result1;
        },
        isNullOrEmpty: function (str) {
            if (typeof (str) == "undefined" || str == null || str.trim() == "") return true;
            else return false;
        },
        getWeekNumber: function (i) {
            const characters = "一二三四五六日";
            return characters.charAt(i - 1);
        },
        thousandSign: function (str) {
            let num = parseFloat(str);
            return isNaN(num) ? "0" : num.toLocaleString();
        }
    },
    Zipcode: {
        init: function (id) {
            const reandomStr = co.String.generateRandomString(5);
            $TWzipcode = $(id);

            $TWzipcode.twzipcode({
                'zipcodeIntoDistrict': true,
            });

            var $county, $district;

            $address = $TWzipcode.find('.address');
            $county = $TWzipcode.children('.county');
            $district = $TWzipcode.children('.district');

            $county.children('select').attr({
                id: `SelectCity_${reandomStr}`,
                class: "city form-select"
            }).prop("required", $address.prop("required"));
            $county.append(`<label class='px-4 required' for='SelectCity_${reandomStr}'>縣市</label>`);
            var $county_first_option = $county.children('select').children('option').first();
            $county_first_option.text("請選擇縣市");
            $county_first_option.attr('disabled', 'disabled');

            $district.children('select').attr({
                id: `SelectTown_${reandomStr}`,
                class: "town form-select"
            }).prop("required", $address.prop("required"));
            $district.append(`<label class='required' for='SelectTown_${reandomStr}'>鄉鎮</label>`);
            var $district_first_option = $district.children('select').children('option').first();
            $district_first_option.text("請選擇鄉鎮");
            $district_first_option.attr('disabled', 'disabled');
        },
        setData: function (obj) {
            const $addr = obj.el.find(".address");
            if (co.String.isNullOrEmpty(obj.addr)) {
                obj.el.twzipcode('reset');
                obj.el.find(".address").val("");
            } else {
                var address_split = obj.addr.split(" ");
                obj.el.twzipcode('set', {
                    'county': address_split[0],
                    'district': address_split[1],
                });
                $addr.val(address_split[2]);
            }
        },
        getData: function ($e) {
            return $e.find(".county>select").val() + " " + $e.find(".district>select").val() + " " + $e.find(".address").val()
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
    setRandenId: function (i) {
        const $self = $(this);
        let className = typeof ($self.attr('class')) != "undefined" && $self.attr('class') != "" ? $self.attr('class').split(/\s+/)[0] + "Id" : "";
        let order = !!i ? i : 0;
        if (className == "") className = "RandenId";
        let id = className + (order == 0 ? "" : order);
        if ($(`#${id}`).length == 0) $self.attr("id", id);
        else $self.setRandenId(order + 1);
    },
    getFormJson: function () {
        const form = $(this);
        const formDataObject = $(form).serializeArray();
        $(formDataObject).each(function () {
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
                                        obj.value += $(this).nextAll("label").text().trim() + " ,";
                                    });
                                    obj.value = obj.value.substring(0, obj.value.length - 2);
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