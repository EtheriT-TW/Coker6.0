var OrgName = "Page", LayoutType = 0, IsFaPage = true, loginModal, otherLoginModal, registerModal, forgetModal, resetModal, privacyStatementModal, IsLogin;

function ready() {
    const $conten = $("#main");
    const $parentConten = $("#ParentNode");
    loginModal = $("#LoginModal").length > 0 ? new bootstrap.Modal($("#LoginModal")) : null;
    privacyStatementModal = $("#PrivacyStatementModal").length > 0 ? new bootstrap.Modal($("#PrivacyStatementModal")) : null;
    otherLoginModal = $("#OtherLoginModal").length > 0 ? new bootstrap.Modal($("#OtherLoginModal")) : null;
    registerModal = $("#RegisterModal").length > 0 ? new bootstrap.Modal($("#RegisterModal")) : null;
    forgetModal = $("#ForgetModal").length > 0 ? new bootstrap.Modal($("#ForgetModal")) : null;
    resetModal = $("#ResetModal").length > 0 ? new bootstrap.Modal($("#ResetModal")) : null;
    jqueryExtend();
    document.querySelector(`[data-rootid="${RootId}"]`)?.classList.add("active");
    $('.navbar-nav > .nav-item').each(function () {
        $this = $(this);
        if ($this.find('.subtitle').length == 0) {
            $this.find('.menu-item').addClass('no-arrow');
        }
    });

    $("link").each(function () {
        var $self = $(this);
        if ($self.data("orgname") != undefined) {
            OrgName = $self.data("orgname");
        }
        if ($self.data("layouttype") != undefined) {
            LayoutType = $self.data("layouttype");
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
        if (location.pathname.toLowerCase().indexOf("/article/") >= 0) $conten.html($(`<div class="container isArticle">`).html(ele.textContent || ele.innerText));
        else if (location.pathname.toLowerCase().indexOf("/product/") >= 0) $conten.find("#ProductDescription > Content").html(ele.textContent || ele.innerText);
        else $conten.html(ele.textContent || ele.innerText);
        $conten.find("[draggable]").removeAttr("draggable");
        if ($conten.find("#CustMain").length > 0) $("#jumpToCenter").attr("href", "#CustMain");
        $conten.removeClass("d-none");
    }
    $(".editTime,.popular").appendTo($conten);
    $(".backstageType").remove();
    if (typeof AOS !== 'undefined' && AOS && typeof AOS.init === 'function') AOS.init();
    if ($(".search-input").val() != "") {
        let encodedString = decodeURIComponent($(".search-input").val());
        const textArea = document.createElement('textarea');
        textArea.innerHTML = encodedString;
        const searchKey = textArea.value;

        $(".search-input").val(searchKey);
        $(".searchText").text($(".search-input").val());
        $("#Search_Result .catalog_frame").attr("data-search-text", $(".search-input").val());
    }
    if ($(".catalog_frame").length > 0 || $(".menu_directory").length > 0 || $(".advertise_directory").length > 0) DirectoryGetDataInit();
    //swiper內的元素有一個以上就開啟自動輪播(autoplay:true)
    if ($(".one_swiper,.one_swiper_thumbs,.two_swiper,.three_swiper,.four_swiper,.five_swiper,.six_swiper,.picture-category,.three_two_grid_swiper,.vertical_swiper_thumbs").length > 0) SwiperInit({ autoplay: true });
    if ($(".marqueeSwiper").length > 0) MarqueeSwiper(SiteId);
    if ($(".masonry").length > 0 || $(".YTmodal_frame").length > 0) FrameInit();
    if ($(".type_change_frame").length > 0) ViewTypeChangeInit();
    if ($(".hover_mask").length > 0) HoverEffectInit();
    if ($(".sitemap_hierarchical_frame").length > 0) SitemapInit();
    if ($(".link_with_icon").length > 0) LinkWithIconInit();
    if ($(".anchor_directory").length > 0 || $(".anchor_title").length > 0) AnchorPointInit();
    if ($(".shareBlock").length > 0) ShareBlockInit();
    if ($(".flipdown").length > 0) FlipTimer();
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
    if (location.hash == "#PrivacyStatement") privacyStatementModal.show();
    _c.Search.Init("#Search");
    $(".nav-link").on("focus", function () {
        $(this).trigger("mouseover");
    });
    $(".dropdown-toggle").on("focus", function () {
        new bootstrap.Dropdown($(this)[0], {}).show();
    });
    $(".accesskey[href]").on("click", function (e) {
        const $self = $(this);
        const $target = $($self.attr("href"));
        if ($target.length > 0) {
            $target.goTo();
            $target.attr('tabindex', '-1').trigger("focus");
            $target.on('blur', function () {
                $target.removeAttr('tabindex'); // 移除 tabindex
            });
        }
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
        $body.find(".fa-3x")

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

    CokerI18n.apply(document);

    typeof (PageReady) === "function" && PageReady();
    typeof (HeaderInit) === "function" && HeaderInit();
    typeof (FooterInit) === "function" && FooterInit();
    SideFloatingInit();
    CreateToken();
    let idleTimeout;
    sessionStorage.setItem('pageLoadTime', Date.now());
    //監測使用者是否離開畫面
    const sentTrackTime = function () {
        if (!sessionStorage.isNullOrEmpty("PageKey")) {
            // 將停留時間發送到伺服器
            let timeSpent = Date.now() - parseInt(sessionStorage.getItem('pageLoadTime'));
            const body = { PageKey: sessionStorage.getItem("PageKey"), TimeSpan: timeSpent };
            const headers = { type: 'application/json' };
            const blob = new Blob([JSON.stringify(body)], headers);
            navigator.sendBeacon("/api/UserStatistic/trackTime", blob);
            sessionStorage.setItem('pageLoadTime', Date.now());
        }
    }
    function resetIdleTimer() {
        clearTimeout(idleTimeout);
        idleTimeout = setTimeout(function () {
            // 可以選擇將閒置時間重置為零或停止計時
            sessionStorage.setItem('pageLoadTime', Date.now());
        }, 300000); // 設定為 5 分鐘
    }

    window.addEventListener("beforeunload", function () {
        if (!document.hidden) {
            sentTrackTime();
        }
    });
    // 監聽用戶的各種互動，例如滑鼠移動、鍵盤按壓等
    document.addEventListener('mousemove', resetIdleTimer);
    document.addEventListener('keydown', resetIdleTimer);
    document.addEventListener('scroll', resetIdleTimer);


    // 當用戶切換標籤或最小化時，使用 visibilitychange 事件來處理
    document.addEventListener("visibilitychange", function () {
        if (document.hidden) {
            sentTrackTime();
        } else {
            // 重新加載頁面時，重設加載時間
            sessionStorage.setItem('pageLoadTime', Date.now());
        }
    });


    if (typeof (SearchWord) !== "undefined" && SearchWord !== "") {
        // 取得 URL 關鍵字
        const searchText = decodeURIComponent(SearchWord);
        const highlightClass = "highlight";
        const mainContainer = document.querySelector("#main");
        function highlightText(node) {
            const regex = new RegExp(`(${searchText})`, "g");

            // 確保 node 是純文字節點，且未被標記過
            if (node.nodeType === Node.TEXT_NODE && regex.test(node.nodeValue)) {
                // 檢查父節點是否已包含 highlight 樣式，避免重複替換
                if (node.parentNode && node.parentNode.classList.contains(highlightClass)) {
                    return; // 已處理過則跳過
                }

                const textParts = node.nodeValue.split(regex);
                const fragment = document.createDocumentFragment();

                textParts.forEach((part) => {
                    if (part === searchText) {
                        const span = document.createElement("span");
                        span.className = highlightClass;
                        span.textContent = part;
                        fragment.appendChild(span);
                    } else {
                        fragment.appendChild(document.createTextNode(part));
                    }
                });

                !!node.parentNode && node.parentNode.replaceChild(fragment, node);
            }
        }

        function traverseAndHighlight(node) {
            if (node.nodeType === Node.TEXT_NODE) {
                highlightText(node);
            } else {
                for (let child of node.childNodes) {
                    traverseAndHighlight(child);
                }
            }
        }

        // 初次載入時執行高亮
        if (!!mainContainer) {
            traverseAndHighlight(mainContainer);

            // 監聽 DOM 變化 (適用於 AJAX 或動態內容)
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === Node.TEXT_NODE) {
                            highlightText(node);
                        } else if (node.nodeType === Node.ELEMENT_NODE) {
                            traverseAndHighlight(node);
                        }
                    });
                });
            });

            observer.observe(mainContainer, { childList: true, subtree: true });
        }
    }

    const enterAdModalEl = $('#EnterAdModal')
    var enteradid = enterAdModalEl.data("enteradid")

    if ($('#EnterAdModal').length > 0) {
        var adid = $("#EnterAdModal .modal-content").data("aid");
        if ((localStorage.getItem("EnterAd_Show") == null || localStorage.getItem(`AgreePrivacy`) == null) || localStorage.getItem("EnterAd_Show") != adid) {
            var enterAdModal = new bootstrap.Modal($("#EnterAdModal"))
            enterAdModal.show();
            enterAdModalEl.on('hidden.bs.modal', event => {
                localStorage.setItem("EnterAd_Show", adid);
            })
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
            $("#CheckRemember").prop("checked", true);
        })
        LoginModal.addEventListener('hidden.bs.modal', function (event) {
            localStorage.removeItem('gotoMember');
            FormClear(LoginForms, $InputLoginVCode);
        })
    } else {
        $("footer a[href*='Member']").closest("li").addClass("d-none");
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
    $("#LoginForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#LoginModal .btn_login").click();
        }
    });
    $(".btn_login").on("click", function () {
        if (SiteFormCheck(LoginForms, $InputLoginVCode)) {
            CaptchaVerify($LoginImgCaptcha, $InputLoginVCode, LoginAction)
        } else {
            $InputLoginVCode.addClass('is-invalid');
            $InputLoginVCode.siblings("div").addClass("me-4 pe-2");
            NewCaptcha($LoginImgCaptcha, $InputLoginVCode)
            $InputLoginVCode.val("");
            Coker.sweet.warning(local.AlertTitle, local.AlertCheckLoginInput, null);
        }
    })
    $("#RegisterForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#RegisterModal .btn_register").click();
        }
    });
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
                Coker.sweet.warning(local.AlertTitle, local.AlertAgreeMemberTerms, null, true);
            } else {
                CaptchaVerify($RegisterImgCaptcha, $InputRegisterVCode, RegisterAction)
            }
        } else {
            NewCaptcha($RegisterImgCaptcha, $InputRegisterVCode);
            Coker.sweet.warning(local.AlertTitle, local.AlertCheckRegisterInput, null);
        }
    })
    $("#ForgetForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#ForgetModal .btn_forget").click();
        }
    });
    $(".btn_forget").on("click", function () {
        CaptchaVerify($ForgetImgCaptcha, $InputForgetVCode, ForgetAction)
    })

    $(".btn_backlogin").on("click", function () {
        loginModal.show();
        forgetModal.hide();
    })
    $("#ResetForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#ResetModal .btn_reset").click();
        }
    });
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
            Coker.sweet.warning(local.AlertTitle, local.ErrorPasswordFormatInvalid, null);
        } else if (!formcheck) {
            NewCaptcha($ResetImgCaptcha, $InputResetVCode);
            Coker.sweet.warning(local.AlertTitle, local.ErrorCaptchaInvalid, null);
        }
    })

    $("#ResetModal .btn_resetforget").on("click", function () {
        Coker.sweet.confirm(local.ConfirmSendResetPasswordMailToEmail.format($("#ResetForm").data("Email")), "", local.Yes,local.No, function () {
            Coker.sweet.loading();
            var data = {};
            data.Email = $("#ResetForm").data("Email");
            data.WebsiteId = SiteId;
            data.WebsiteLink = $(location).attr('origin');
            data.WebsiteName = $("meta[property='og:site_name'").attr("content");
            co.User.PasswordForget(data).done((result) => {
                if (result.success) {
                    Coker.sweet.success(local.InfoResetPasswordMailWillBeSent, null, false);
                    registerModal.hide();
                } else {
                    Coker.sweet.error(result.error, null, true);
                }
            })
        })
    })

    $(".btn_cookie_accept").on("click", function () { cookie_accept(true) });
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
                            Coker.sweet.custom("success", "", local.ResultAccountActivated, local.ActionFillMemberProfile, function () {
                                window.location.href = `/${OrgName}/Member`;
                            }, local.ActionMaybeLater, function () {
                                window.history.replaceState({}, document.title, window.location.pathname);
                                location.reload();
                            })
                        } else {
                            if (result.message == "ReSendOrNot") {
                                var data = {};
                                data.OpenId = insertdata["openid"];
                                data.WebsiteId = SiteId;
                                data.WebsiteLink = $(location).attr('origin');
                                data.WebsiteName = $("meta[property='og:site_name'").attr("content");
                                co.sweet.confirm(result.error, "", local.ActionResend, local.Cancel, function () {
                                    Coker.sweet.loading();
                                    co.User.AccountReSendOpening(data).done(result => {
                                        if (result.success) {
                                            Coker.sweet.success(local.InfoResendMemberWelcomeMail, null, false);
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

    $(".btn_passtoggle").on("click", function (event) {
        event.preventDefault();
        var $this = $(this);
        switch ($this.text()) {
            case "visibility":
                $this.parent("div").siblings("input").attr("type", "password");
                $this.attr("title", local.ShowPassword);
                $this.text("visibility_off");
                break;
            case "visibility_off":
                $this.parent("div").siblings("input").attr("type", "text");
                $this.attr("title", local.HidePassword);
                $this.text("visibility");
                break;
        }
    });

    var string = $("#TermsModal .modal-body .content").text().toString();
    //console.log(string)
    string = $.trim(string);
    //console.log(string)
    $("#TermsModal .modal-body .content").html(string)

    if ($(".instagram-media").length > 0) {
        var ig_script = document.createElement('script');
        ig_script.src = "//www.instagram.com/embed.js";
        ig_script.async = true;
        document.head.appendChild(ig_script);
    }

    $("footer a[href*='Member']").on("click", function (e) {
        e.preventDefault();
        var $self = $(this);
        if (IsLogin) {
            window.location.href = $self.attr("href");
        } else {
            localStorage.setItem('gotoMember', 'true');
            loginModal.show();
        }
    });

    if ($(".noImageHide").length > 0) {
        $(".noImageHide").each(function () {
            var $self = $(this);
            $self.find("[src='/images/noImg.jpg']").addClass("custom_visibility_hidden");
            $self.find("[src='/images/UploadImg.png']").addClass("custom_visibility_hidden");
        });
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
        $("#btn_gotop").addClass("show");
        $("#Floating_Center").addClass("show");
    } else {
        $("#btn_gotop").removeClass("show");
        $("#Floating_Center").removeClass("show");
    }
}
function cookie_accept(isnew) {
    if (isnew) {
        Coker.Token.AgreePrivacy().done(function (result) {
            if (result.success) {
                localStorage.setItem(`AgreePrivacy`, true);
                localStorage.setItem("AgreeTime", result.message);
                $("#Cookie").removeClass("show");
            } else {
                console.log("AgreePrivacy Fail", result)
            }
        });
    } else {
        localStorage.setItem(`AgreePrivacy`, true);
        $("#Cookie").removeClass("show");
    }
}
function cookie_reject() {
    $("#Cookie").removeClass("show")
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
                IsLogin = true;
                $("#HiUser > .name").text(local.GreetingUser.format(result.name));
            }
            if ($("#Cart_Dropdown_Parent").length > 0) {
                CartDropInit();
            }
            if (window.location.pathname == `/${OrgName}/ShoppingCar`) {
                var search = window.location.search;
                if (search == "") CardDataGet();
                else if ($.isNumeric(search.substring(1)) || window.location.search.substring(1).startsWith("ECPayError")) {
                    if (localStorage.getItem("lastSaveToken") == result.token && localStorage.getItem("lastSaveTime") != null) {
                        var tokenSaveTime = new Date(localStorage.getItem('lastSaveTime'));
                        var fifteenMinutesAgo = new Date(Date.now() - 15 * 60 * 1000);

                        if (tokenSaveTime > fifteenMinutesAgo) {
                            Coker.Token.CheckToken(localStorage.getItem("lastSaveToken")).done(function (result) {
                                if (result.success) {
                                    if (result.isLogin && result.name != "") {
                                        IsLogin = true;
                                        $("#HiUser > .name").text(`${result.name} 您好!`);
                                        $("#HiUser").removeClass("d-none");
                                        $("#UserLogin").addClass("d-none");
                                        $("#memberLogin").addClass("d-none");
                                    }
                                    localStorage.setItem("token", result.token);
                                    localStorage.setItem("lastSaveTime", null);
                                }
                            });
                        }
                    }
                }
            }
            if (result.agreePrivacy) cookie_accept(false);
            else {
                if (localStorage.getItem('AgreeTime') != null) {
                    var agreeSaveTime = new Date(localStorage.getItem('AgreeTime'));
                    var oneYearAgo = new Date();
                    oneYearAgo.setFullYear(oneYearAgo.getFullYear() - 1);
                    if (agreeSaveTime > oneYearAgo) {
                        cookie_accept(false);
                    } else {
                        localStorage.removeItem("AgreePrivacy")
                        localStorage.removeItem("AgreeTime")
                        $("#Cookie").addClass("show")
                    }
                } else {
                    $("#Cookie").addClass("show")
                }
            }
        }
    })
}

function updateRightClickLock() {
    if (document.body.classList.contains("no-right-click")) {
        // 禁用右鍵
        document.addEventListener("contextmenu", disableContextMenu);
        // 禁用複製
        document.addEventListener("copy", disableCopy);
        // 禁用開發者工具
        document.addEventListener("keydown", disableDevTools);
    } else {
        // 解除所有限制
        document.removeEventListener("contextmenu", disableContextMenu);
        document.removeEventListener("copy", disableCopy);
        document.removeEventListener("keydown", disableDevTools);
    }
}

// 禁用右鍵
function disableContextMenu(event) {
    event.preventDefault();
}

// 禁用複製
function disableCopy(event) {
    event.preventDefault();
    alert(local.ErrorCopyNotAllowed);
}

// 禁用 F12、Ctrl+U、Ctrl+Shift+I
function disableDevTools(event) {
    if (event.key === "F12" ||
        (event.ctrlKey && event.shiftKey && event.key === "I") ||
        (event.ctrlKey && event.key === "U")) {
        event.preventDefault();
    }
}

// 頁面載入時檢查
document.addEventListener("DOMContentLoaded", updateRightClickLock);

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
                    Coker.sweet.warning(local.AlertTitle, local.ErrorCaptchaInvalid, null);
                }
            }
        })
    } else {
        $input.addClass('is-invalid');
        $input.siblings("div").addClass("me-4 pe-2");
        NewCaptcha($self, $input)
        $input.val("");
        Coker.sweet.warning(local.AlertTitle, local.AlertCheckCaptchaInput, null);
    }
}
function LoginAction() {
    Coker.sweet.loading();
    var gotoMember = localStorage.getItem('gotoMember');
    loginModal.hide();
    var data = co.Form.getJson($("#LoginForm").attr("id"));
    data.WebsiteId = SiteId
    data.Remember = $("#CheckRemember").prop("checked");
    co.User.Login(data).done((result) => {
        if (result.success) {
            Coker.sweet.success(local.WelcomeBack, function () {
                if (gotoMember == "true") {
                    window.location.href = `/${OrgName}/Member`;
                } else {
                    location.href = window.location.origin + window.location.pathname;
                }
            }, false);
        } else {
            switch (result.message) {
                case local.ResendActivationMail:
                    Coker.sweet.confirm(result.error, "", result.message, local.CloseWindow, function () {
                        Coker.sweet.loading();
                        data.WebsiteLink = $(location).attr('origin');
                        data.WebsiteName = $("meta[property='og:site_name'").attr("content");
                        co.User.AccountReSendOpening(data).done(result => {
                            if (result.success) {
                                Coker.sweet.success(local.SystemResendingActivationMail, null, false);
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
    co.User.AddUser(data).done((result) => {
        if (result.success) {
            Coker.sweet.success("<div>註冊成功</div><div>您將收到開通帳號的通知信</div><div>請至信箱確認以完成帳號開通</div>", null, false);
            registerModal.hide();
        } else {
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
            Coker.sweet.success(local.SystemSendingResetMail, null, false);
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
            Coker.sweet.success(local.PasswordResetSuccess, function () {
                resetModal.hide();
                loginModal.show();
            }, false);
        } else {
            switch (result.message) {
                case local.PasswordIncorrect:
                    Coker.sweet.confirm(result.error, "", `${local.ForgotPassword}?`, local.Confirm, function () {
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
                    $CheckPassFeedBack.text(local.ErrorPasswordNotMatch);
                }
            } else {
                $NewPassFeedBack.text(local.ErrorPasswordFormatInvalid);
                $CheckPassFeedBack.text(local.ErrorPasswordFormatInvalid);
            }
        } else {
            $NewPassFeedBack.text(local.AlertPasswordMax32);
            $CheckPassFeedBack.text(local.ErrorPasswordFormatInvalid);
        }
    } else {
        $NewPassFeedBack.text(local.AlertPasswordMin8);
        $CheckPassFeedBack.text(local.ErrorPasswordFormatInvalid);
    }
    return false;
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
    }
});