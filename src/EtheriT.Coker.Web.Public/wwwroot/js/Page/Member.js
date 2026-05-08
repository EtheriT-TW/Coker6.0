var resetEmailModal, ResetEmailModal, $InputResetEmailVCode, $ResetEmailImgCaptcha, ResetEmailForms, reOrderAlertModal, ReOrderAlertModal
var old_email
var TabNow = "info", date_now = "";

function PageReady() {
    Coker.Member = {
        GetOrderHistory: function (page) {
            return $.ajax({
                url: "/api/Order/GetHistoryOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { page: page },
            });
        },
        Reorder: function (ohid) {
            return $.ajax({
                url: "/api/Order/Reorder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { ohid: ohid },
            });
        },
        CheckOrder: function (ohid) {
            return $.ajax({
                url: "/api/Order/CheckOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { ohid: ohid },
            });
        },
        OrderRepay: function (data) {
            return $.ajax({
                url: "/api/Order/OrderRepay",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        CancelOrder: function (ohid, payment) {
            return $.ajax({
                url: "/api/Order/CancelOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { ohid: ohid, payment: payment },
            });
        },
        GetECPayPaymentInfo: function (ohid) {
            return $.ajax({
                url: "/api/ThirdParty/ECPayGetPaymentInfo",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { ohid: ohid },
            });
        },
        GetBonusHistory: function (page) {
            return $.ajax({
                url: "/api/Bonus/GetFrontUserBonusHistory/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { page: page },
            });
        },
    };

    let addr = $("#TWzipcode .address").val();
    co.Zipcode.init("#TWzipcode");
    co.Zipcode.setData({
        el: $("#TWzipcode"),
        addr: addr
    });

    Coker.Token.CheckToken().done(function (resule) {
        if (!resule.isLogin) {
            co.sweet.warning(local.UserNotLoggedIn, local.ActionLoginRequiredRedirectHome, function () {
                location.href = "/";
            });
        } else {
            window.IsLogin = true;
            window.islogin = true;
            Member(resule);
        }
    });
}

function Member(data) {
    var now = new Date();
    var month = (now.getMonth() + 1).toString();
    if (month.length == 1) month = '0' + month;
    var day = now.getDate().toString();
    if (day.length == 1) day = '0' + day;
    date_now = `${now.getFullYear()}-${month}-${day}`;

    resetEmailModal = $("#ResetEmailModal").length > 0 ? new bootstrap.Modal($("#ResetEmailModal")) : null;
    reOrderAlertModal = $("#ReOrderAlertModal").length > 0 ? new bootstrap.Modal($("#ReOrderAlertModal")) : null;
    $InputResetEmailVCode = $("#InputNewMailVCode");
    $ResetEmailImgCaptcha = $('#NewMailImgCaptcha');
    ResetEmailForms = $('#ResetEmailForm');

    $("#ResetForm .reset_old_pass").removeClass("d-none");
    $("#ResetForm .reset_old_pass input").removeAttr("disabled");
    $("#ResetOldPassFeedBack").removeClass("d-none");
    $("#ResetModal .btn_resetforget").removeClass("d-none");

    SetMemberData();
    WebPageChange();

    $(".btn_logout").on("click", function () {
        co.User.Logout().done(function (result) {
            if (result.success) {
                co.sweet.success("登出成功");
                setTimeout(function () {
                    location.href = "/";
                }, 1000);
            }
        });
    });

    $(".btn_modifi").on("click", function () {
        var data = co.Form.getJson($("#UserDataForm").attr("id"));
        if (data.name == "") {
            co.sweet.warning("請注意", "姓名不可為空", null);
        } else if ($("#Email").val() == "") {
            co.sweet.warning("請注意", "電子郵件不可為空", null);
        } else if (data.zone == "" ^ data.telPhone == "") {
            co.sweet.warning("請注意", "如要填入電話資訊，請確實填寫區碼與聯絡電話", null);
        } else if (((data.county == "") ^ (data.address == ""))) {
            co.sweet.warning("請注意", "如要填入地址資訊，請確實填寫縣市、鄉鎮與地址", null);
        } else {
            var datacheck = true;
            if (data.birthday == "") data.birthday = null;
            data.address = `${data.county} ${data.district} ${data.address}`;
            if (data.cellPhone != "" && (!$.isNumeric(data.cellPhone) || data.cellPhone.length != 10)) {
                co.sweet.warning("請注意", "手機格式不正確，請重新輸入", null);
                datacheck = false;
            }
            if (data.telPhone != "") {
                if ($.isNumeric(data.zone) && $.isNumeric(data.telPhone) && ((data.ext != "" && $.isNumeric(data.ext)) || data.ext == "")) {
                    data.telPhone = `${data.zone}-${data.telPhone}-${data.ext}`;
                } else {
                    co.sweet.warning("請注意", "電話格式不正確，請重新輸入", null);
                    datacheck = false;
                }
            }
            if (datacheck) {
                co.User.UserEdit(data).done(function () {
                    co.sweet.success("資料修改完成！", null, true);
                });
            }
        }
    });

    $(".btn_resetPassword").on("click", function () {
        resetModal.show();
    });

    $(".btn_resetEmail").on("click", function (event) {
        event.preventDefault();
        resetEmailModal.show();
    });

    $("#ResetEmailModal .btn_resetforget").on("click", function () {
        $("#ResetModal .btn_resetforget").click();
    });

    $('#ResetEmailModal .btn_refresh').on('click', function (event) {
        event.preventDefault();
        NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
    });

    ResetEmailModal = document.getElementById('ResetEmailModal');
    if (ResetEmailModal != null) {
        ResetEmailModal.addEventListener('show.bs.modal', function () {
            NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
        });
        ResetEmailModal.addEventListener('hidden.bs.modal', function () {
            FormClear(ResetEmailForms, $InputResetEmailVCode);
        });
    }

    ReOrderAlertModal = document.getElementById('ReOrderAlertModal');
    if (ReOrderAlertModal != null) {
        ReOrderAlertModal.addEventListener('hidden.bs.modal', function () {
            $(".btn_repay").removeClass("d-none");
            $("#ReOrderAlertModal .orderlist ul li").remove();
        });
    }

    $("#ResetEmailForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#ResetEmailModal .btn_resetmail").click();
        }
    });

    $(".btn_resetmail").on("click", function () {
        if (SiteFormCheck(ResetEmailForms, $InputResetEmailVCode)) {
            CaptchaVerify($ResetEmailImgCaptcha, $InputResetEmailVCode, function () {
                ResetmailAction(data);
            });
        } else {
            $InputResetEmailVCode.addClass('is-invalid');
            $InputResetEmailVCode.siblings("div").addClass("me-4 pe-2");
            NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
            $InputResetEmailVCode.val("");
            Coker.sweet.warning("請注意", "請確實填寫資料", null, true);
        }
    });

    $(".btn_switchViewType button").on("click", function () {
        var $this = $(this);
        var $thisbro = $this.siblings("button");
        if (!$this.hasClass("focus")) $this.addClass("focus");
        if ($thisbro.hasClass("focus")) $thisbro.removeClass("focus");

        var $content = $this.parents(".tab-pane").find(".content");
        switch ($this.data("type")) {
            case "grid":
                localStorage.setItem(`switchViewType-Member${$content.data("storagename")}`, "grid");
                if (!$content.hasClass("type_grid")) $content.addClass("type_grid");
                if ($content.hasClass("type_list")) $content.removeClass("type_list");
                break;
            case "list":
                localStorage.setItem(`switchViewType-Member${$content.data("storagename")}`, "list");
                if (!$content.hasClass("type_list")) $content.addClass("type_list");
                if ($content.hasClass("type_grid")) $content.removeClass("type_grid");
                break;
        }
    });

    if (typeof (localStorage["switchViewType-MemberFavorite"]) != "undefined") {
        if (localStorage["switchViewType-MemberFavorite"] == "list") {
            $("#favorite-tab-pane .btn_switchViewType button[data-type='list']").click();
        }
    }
    if (typeof (localStorage["switchViewType-MemberBrowsing"]) != "undefined") {
        if (localStorage["switchViewType-MemberBrowsing"] == "list") {
            $("#history-tab-pane .btn_switchViewType button[data-type='list']").click();
        }
    }

    $("#ToolList > li button").on("click", function () {
        switch ($(this).attr("id")) {
            case "bonus-tab":
                window.location.hash = "#bonus";
                break;
            case "profile-tab":
                window.location.hash = "#order";
                break;
            case "favorite-tab":
                window.location.hash = "#favorites";
                break;
            case "history-tab":
                window.location.hash = "#browsing";
                break;
            default:
                window.location.hash = "#";
                break;
        }
    });

    $("#ReOrderAlertModal .btn_cancelrepay").on("click", function () {
        reOrderAlertModal.hide();
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function hashChange(e) {
    if (!!e) {
        e.preventDefault();
        WebPageChange();
    } else {
        console.log("HashChange錯誤");
    }
}

function WebPageChange() {
    const hash = window.location.hash || "";

    if (hash.startsWith("#bonus")) {
        if ($("#TabContent > div#bonus-tab-pane").length <= 0) {
            window.location.hash = "";
            return;
        }

        $("#TabContent > div").removeClass("active show");
        $("#ToolList > li button").removeClass("active");

        $("#TabContent > div#bonus-tab-pane").addClass("active show");
        $("#ToolList > li button#bonus-tab").addClass("active");

        if (hash.indexOf("-") > 0) {
            var pagenumber = hash.substring(hash.indexOf("-") + 1);
            if (!isNaN(Number(pagenumber))) {
                SetBonusPage(Number(pagenumber));
                TabNow = "bonus";
            } else {
                window.location.hash = "#bonus-1";
            }
        } else {
            window.location.hash = "#bonus-1";
        }

        return;
    }

    if (hash.startsWith("#order")) {
        if ($("#TabContent > div#profile-tab-pane").length <= 0) {
            window.location.hash = "";
            return;
        }

        $("#TabContent > div").removeClass("active show");
        $("#ToolList > li button").removeClass("active");

        $("#TabContent > div#profile-tab-pane").addClass("active show");
        $("#ToolList > li button#profile-tab").addClass("active");

        if (hash.indexOf("-") > 0) {
            var pagenumber = hash.substring(hash.indexOf("-") + 1);
            if (!isNaN(Number(pagenumber))) {
                SetHistoryOrderPage(Number(pagenumber));
                TabNow = "order";
            } else {
                window.location.hash = "#order-1";
            }
        } else {
            window.location.hash = "#order-1";
        }

        return;
    }

    if (hash.startsWith("#browsing")) {
        $("#TabContent > div").removeClass("active show");
        $("#ToolList > li button").removeClass("active");

        $("#TabContent > div#history-tab-pane").addClass("active show");
        $("#ToolList > li button#history-tab").addClass("active");

        if (hash.indexOf("-") > 0) {
            var pagenumber = hash.substring(hash.indexOf("-") + 1);
            if (!isNaN(Number(pagenumber))) {
                SetBrowsingHistoryPage(Number(pagenumber));
                TabNow = "browsing";
            } else {
                window.location.hash = "#browsing-1";
            }
        } else {
            window.location.hash = "#browsing-1";
        }

        return;
    }

    if (hash.startsWith("#favorites")) {
        $("#TabContent > div").removeClass("active show");
        $("#ToolList > li button").removeClass("active");

        $("#TabContent > div#favorite-tab-pane").addClass("active show");
        $("#ToolList > li button#favorite-tab").addClass("active");

        if (hash.indexOf("-") > 0) {
            var pagenumber = hash.substring(hash.indexOf("-") + 1);
            if (!isNaN(Number(pagenumber))) {
                SetFavoritesPage(Number(pagenumber));
                TabNow = "favorites";
            } else {
                window.location.hash = "#favorites-1";
            }
        } else {
            window.location.hash = "#favorites-1";
        }

        return;
    }

    $("#TabContent > div").removeClass("active show");
    $("#ToolList > li button").removeClass("active");

    $("#TabContent > div#info-tab-pane").addClass("active show");
    $("#ToolList > li button#info-tab").addClass("active");
    TabNow = "info";
}

function SetBonusPage(number) {
    const $pane = $("#bonus-tab-pane");
    const $content = $pane.find(".content");
    const $pageBtn = $pane.find(".page_btn");
    const $noData = $pane.find(".nodata");

    Coker.Member.GetBonusHistory(number).done(function (result) {
        const datas = result && Array.isArray(result.data) ? result.data : [];

        if (datas.length > 0) {
            $noData.addClass("d-none");

            if (result.page_Total > 1) {
                $pageBtn.removeClass("d-none");
                if (!$pageBtn.data("init")) {
                    PageButtonInit($pageBtn, result.page_Total, "bonus");
                }
                ContentPageChage($pageBtn, number, result.page_Total);
            } else {
                $pageBtn.addClass("d-none");
            }

            BonusTemplateDataInsert($content, datas);
        } else if (number != 1) {
            window.location.hash = "#bonus-1";
        } else {
            $pageBtn.addClass("d-none");
            $noData.removeClass("d-none");
            $content.empty();
        }
    });
}
function BonusTemplateDataInsert($content, datas) {
    $content.empty();

    $.each(datas, function (index, data) {
        const frame = $($("#Template_Bonus_List").html()).clone();

        frame.find(".bonus_start").text(Coker.util.string.dateText(data.startTime));
        frame.find(".bonus_end").text(Coker.util.string.dateText(data.endTime));
        frame.find(".bonus_add").text(Coker.util.string.thousandSign(data.addBonus));
        frame.find(".bonus_remain").text(Coker.util.string.thousandSign(data.remainBonus));

        if (!Coker.util.string.isNullOrEmpty(data.note)) {
            frame.find(".bonus_note").text(data.note);
        } else {
            frame.find(".bonus_note").text("未提供");
        }

        const collapseClass = `bonus_collapse_${data.id}`;
        frame.find(".bonus_logs").addClass(collapseClass);
        frame.find(".btn_bonus_collapse").attr("data-bs-target", `.${collapseClass}`);

        frame.find(".btn_bonus_collapse").on("click", function () {
            if ($(this).hasClass("collapsed")) $(this).text("查看使用紀錄");
            else $(this).text("關閉使用紀錄");
        });

        const $logList = frame.find(".bonus_log_list");
        const logs = Array.isArray(data.useLogs) ? data.useLogs : [];

        if (logs.length > 0) {
            $.each(logs, function (i, log) {
                const logFrame = $($("#Template_Bonus_Log_List").html()).clone();
                logFrame.find(".log_date").text(Coker.util.string.dateText(log.creationTime));
                logFrame.find(".log_reason").text(log.reason || "");
                logFrame.find(".log_use").text(Coker.util.string.thousandSign(log.useBonus));
                $logList.append(logFrame);
            });
        } else {
            frame.find(".bonus_log_empty").removeClass("d-none");
        }

        $content.append(frame);
    });
}
function SetMemberData() {
    Coker.User.GetUser().done(function (result) {
        if (result.success) {
            if (result.data.telPhone != null && result.data.telPhone != "") {
                result.data['zone'] = (result.data.telPhone).split('-')[0];
                result.data['telPhone'] = (result.data.telPhone).split('-')[1];
                if ((result.data.telPhone).split('-').length == 2) result.data['ext'] = (result.data.telPhone).split('-')[2];
            }

            co.Form.insertData(result.data, "#UserDataForm");

            old_email = result.data.email;

            co.Zipcode.setData({
                el: $("#TWzipcode"),
                addr: result.data.address
            });

            $("#ResetForm").data("Email", result.data.email);

            $("#Birthday").attr("max", date_now);
            $("#Birthday").on("keydown", function (e) {
                e.preventDefault();
            });
        }
    });
}

function SetHistoryOrderPage(number) {
    Coker.Member.GetOrderHistory(number).done(function (result) {
        if (result.success && result.orderData != null && result.orderData.length > 0) {
            if (result.page_Total > 1) {
                if (!$("#profile-tab-pane .page_btn").data("init")) {
                    PageButtonInit($("#profile-tab-pane .page_btn"), result.page_Total, "order");
                }
                ContentPageChage($("#profile-tab-pane .page_btn"), number, result.page_Total);
            }
            if ($("#profile-tab-pane .btn_switchViewType").hasClass("d-none")) $("#profile-tab-pane .btn_switchViewType").removeClass("d-none");
            HistoryTemplateDataInsert(result.orderData);
        } else if (number != 1) {
            window.location.hash = "#order-1";
        } else {
            if ($("#profile-tab-pane .nodata").hasClass("d-none")) $("#profile-tab-pane .nodata").removeClass("d-none");
        }
    });
}

function HistoryTemplateDataInsert(Datas) {
    $("#profile-tab-pane .content").empty();
    $.each(Datas, function (index, data) {
        var order_header = data.orderHeader;
        var order_details = data.orderDetails;
        var frame = $($("#Template_Order_List").html()).clone();

        frame.find(".number").text(("000000000" + order_header.id).substring(order_header.id.length));
        frame.find(".date").text(((order_header.creationTime).substr(0, 16).replaceAll("-", "/")));
        frame.find(".amount").text(`$${(order_header.total).toLocaleString()}`);

        if (typeof (order_header.paymentIcon) != "undefined" && order_header.paymentIcon != "") {
            frame.find(".payment").append(`<img src="${order_header.paymentIcon}"/><span>${order_header.payment}</span>`);
        } else {
            frame.find(".payment").text(order_header.payment);
        }

        const appendCancelButton = function () {
            frame.find(".state").append(`<button class="btn_cancelOrder bg-transparent border-0 text-decoration-underline ms-1" title="取消此筆訂單">取消訂單</button>`);
            frame.find(".state .btn_cancelOrder").data("ohid", order_header.id);
            frame.find(".state .btn_cancelOrder").on("click", function () {
                var $this = $(this);
                var $orderFrame = $this.closest(".order_frame");
                var confirm_text = "?";

                if ([2, 6].includes(order_header.state)) {
                    if (order_header.thirdParties === 1 ||
                        (order_header.thirdParties === 4 && [21, 22, 23].includes(order_header.paymentCode))
                    ) {
                        confirm_text += "<br><br><span class='fw-bold text-danger'>※退款事宜請聯繫客服處理※</span>";
                    } else {
                        confirm_text += "<br><br><span class='fw-bold'>※若已付款將退回款項※</span>";
                    }
                }

                Coker.sweet.confirm("取消訂單", `<div>確定要取消這筆訂單${confirm_text}</div>`, "確定", "取消", function () {
                    Coker.sweet.loading();
                    Coker.Member.CancelOrder($this.data("ohid"), order_header.thirdParties).done(function (result) {
                        if (result.success) {
                            $this.parent(".state").addClass("text-danger fw-bold");
                            $this.parent(".state").text("已取消");
                            if ($orderFrame.find(".btn_buyInfo").length > 0) $orderFrame.find(".btn_buyInfo").addClass("d-none");
                            Coker.sweet.success(result.message, null, false);
                        } else {
                            Coker.sweet.error("取消訂單失敗", "取消訂單時發生錯誤，請聯繫客服協助處理。");
                        }
                    });
                });
            });
        };

        const appendRepayButton = function () {
            frame.find(".state").append(`<button class="btn_payAgain text-danger bg-transparent border-0 text-decoration-underline ms-1" title="重新付款">重新付款</button>`);
            frame.find(".state .btn_payAgain").on("click", function () {
                Coker.sweet.confirm("確定要重新付款？", "", "確定", "取消", function () {
                    Coker.sweet.loading();
                    OrderRepay({ orderHeader: order_header });
                });
            });
        };

        frame.find(".state").prepend(`<span>${order_header.stateStr}</span>`);

        switch (order_header.action) {
            case "Cancel":
                appendCancelButton();
                break;

            case "Repay":
                appendRepayButton();
                break;

            case "RepayCancel":
                appendRepayButton();
                appendCancelButton();
                break;

            default:
                break;
        }

        switch (order_header.state) {
            case 1:
            case 6:
                frame.find(".state span").addClass("bg-warning text-black");
                break;
            case 5:
                frame.find(".state span").addClass("bg-danger text-white");
                break;
            default:
                if (order_header.state != 4) frame.find(".state span").addClass("bg-success text-white");
                break;
        }

        frame.find(".collapse").addClass(`collapse_${order_header.id}`);
        frame.find(".btn_collapse").attr("data-bs-target", `.collapse_${order_header.id}`);

        frame.find(".btn_collapse").on("click", function () {
            if ($(this).hasClass("collapsed")) $(this).text("查看訂單明細");
            else $(this).text("關閉訂單明細");
        });

        frame.find(".btn_buyAgain").data("ohid", order_header.id);
        frame.find(".btn_buyAgain").on("click", function () {
            var $this = $(this);
            Coker.sweet.confirm("確定要再次購買？", "", "確定", "取消", function () {
                Coker.sweet.loading();
                Coker.Member.Reorder($this.data("ohid")).done(function (result) {
                    if (result.success) {
                        var ohidstr = `000000000${result.message}`.substring(result.message.length);
                        window.location.href = `/${OrgName}/ShoppingCar?reorder${ohidstr}`;
                    } else {
                        Coker.sweet.warning("商品庫存不足", result.message, null);
                    }
                });
            });
        });

        if (order_header.state == 6 && [21, 22, 23].includes(order_header.paymentCode)) {
            frame.find(".btn_buyInfo").data("ohid", order_header.id);
            frame.find(".btn_buyInfo").removeClass("d-none");
            frame.find(".btn_buyInfo").on("click", function () {
                var $this = $(this);
                Coker.Member.GetECPayPaymentInfo($this.data("ohid")).done(function (result) {
                    if (result.success) {
                        if (order_header.paymentCode == 22) {
                            var message = result.message.split(",");
                            co.sweet.confirm("訂單付款資訊", message[0], "確定", "", null);
                            $.getScript("https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js", function () {
                                JsBarcode("#barcode1", message[1], { format: "CODE39", displayValue: true });
                                JsBarcode("#barcode2", message[2], { format: "CODE39", displayValue: true });
                                JsBarcode("#barcode3", message[3], { format: "CODE39", displayValue: true });
                            });
                        } else {
                            co.sweet.confirm("訂單付款資訊", result.message, "確定", "", null);
                        }
                    } else {
                        Coker.sweet.warning("取得付款資訊失敗", result.message, null);
                    }
                });
            });
        }

        $.each(order_details, function (index, detail) {
            if (detail != null) {
                var list_frame = $($("#Template_Order_Details_List").html()).clone();
                list_frame.find("a").attr("href", `/${OrgName}/Member/product/${detail.pId}`);
                list_frame.find("a").attr("title", `連結至：${detail.title}`);
                detail.imagePath = detail.imagePath.replaceAll(`/${OrgName}/`, `/`);
                list_frame.find("img").attr("src", detail.imagePath);
                list_frame.find("img").attr("alt", `${detail.title}的主要圖片`);
                list_frame.find(".title").text(detail.title);
                if (detail.s1Title != "") list_frame.find(".spec").append(`<span class="border px-1 me-1">${detail.s1Title}</span>`);
                if (detail.s2Title != "") list_frame.find(".spec").append(`<span class="border px-1 me-1">${detail.s2Title}</span>`);
                list_frame.find(".price").html(`${detail.price > 0 ?
                    detail.bonus > 0 ? `${parseInt(detail.price).toLocaleString()}<br />紅利：${(detail.bonus).toLocaleString()}` : `${parseInt(detail.price).toLocaleString()}` :
                    `紅利：${(detail.bonus).toLocaleString()}`
                    }`);
                list_frame.find(".quantity").text(Coker.util.string.thousandSign(detail.quantity));
                var detailPrice = parseInt(detail.price || 0);
                var detailQty = parseInt(detail.quantity || 0);
                var detailBonus = parseInt(detail.bonus || 0);
                var detailSubtotal = detailPrice * detailQty;
                var detailBonusSubtotal = detailBonus * detailQty;

                list_frame.find(".subtotal").html(`${detailPrice > 0 ?
                    detailBonus > 0
                        ? `${detailSubtotal.toLocaleString()}<br />紅利：${detailBonusSubtotal.toLocaleString()}`
                        : `${detailSubtotal.toLocaleString()}`
                    :
                    `紅利：${detailBonusSubtotal.toLocaleString()}`
                    }`);
                frame.find(".list-group").append(list_frame);
            }
        });

        // ===== 商品原金額（未折抵） =====
        var productAmount = 0;

        // ===== 商品紅利 =====
        var productBonus = 0;

        $.each(order_details, function (index, detail) {
            if (detail != null) {
                var price = parseInt(detail.price || 0);
                var qty = parseInt(detail.quantity || 0);
                var bonus = parseInt(detail.bonus || 0);

                productAmount += price * qty;
                productBonus += bonus * qty;
            }
        });

        // ===== 訂單紅利 =====
        var totalBonus = parseInt(order_header.bonus || 0);

        // ===== 紅利折抵 =====
        var redeemBonus = Math.max(totalBonus - productBonus, 0);

        // ===== 寫入畫面 =====
        frame.find(".collapse .header_subtotal").text(Coker.util.string.thousandSign(productAmount));
        frame.find(".collapse .header_freight").text(Coker.util.string.thousandSign(order_header.freight));
        frame.find(".collapse .header_total").text(Coker.util.string.thousandSign(order_header.total));

        frame.find(".collapse .header_invoiceTypeTitle").text(order_header.invoiceTypeTitle || "未提供");
        frame.find(".collapse .header_shipping").text(order_header.shipping || "未提供");
        frame.find(".collapse .header_remark").text(order_header.remark || "無");
        frame.find(".collapse .header_systemMemo").text(order_header.systemMemo || "無");

        frame.find(".collapse .header_recipient").text(order_header.recipient);
        frame.find(".collapse .header_recipientCellPhone").text(order_header.recipientCellPhone);
        frame.find(".collapse .header_recipientAddress").text(order_header.recipientAddress);

        if (Coker.util.string.isNullOrEmpty(order_header.carrier)) {
            frame.find(".collapse #carrier").addClass("d-none");
        } else {
            frame.find(".collapse #carrier").removeClass("d-none");
            frame.find(".collapse .header_carrier").text(order_header.carrier);
        }

        // ===== 紅利顯示 =====
        if (totalBonus <= 0) {
            frame.find(".collapse .bonus_summary_row").remove();
        } else {
            frame.find(".collapse .header_productBonus").text(`${Coker.util.string.thousandSign(productBonus)}點`);
            frame.find(".collapse .header_redeemBonus").text(`${Coker.util.string.thousandSign(redeemBonus)}點`);
            frame.find(".collapse .header_totalBonus").text(`${Coker.util.string.thousandSign(totalBonus)}點`);

            // 沒有商品紅利 → 隱藏
            if (productBonus <= 0) {
                frame.find(".collapse .header_productBonus").closest(".row").remove();
            }

            // 沒有折抵 → 隱藏
            if (redeemBonus <= 0) {
                frame.find(".collapse .header_redeemBonus").closest(".row").remove();
            }
        }

        $("#profile-tab-pane .content").append(frame);
    });
}

function OrderRepay(datas) {
    var data = {
        ohid: datas.orderHeader.id
    };

    Coker.Member.OrderRepay(data).done(function (checkResult) {
        if (!checkResult.success) {
            Coker.sweet.error("重新付款發生錯誤", checkResult.message);
            return;
        }

        var requestPayment = function (support) {
            Coker.ThirdParty.Request(datas.orderHeader.id, datas.orderHeader.thirdParties, support).done(function (payResult) {
                if (!payResult.success) {
                    Coker.sweet.error("重新付款發生錯誤", payResult.message, null, false);
                    return;
                }

                if (datas.orderHeader.thirdParties != 4) {
                    localStorage.setItem("lastSaveTime", new Date().toISOString());
                    window.location.replace(payResult.message);
                    return;
                }

                ECPayModal = $("#ECPayModal").length > 0 ? new bootstrap.Modal($("#ECPayModal")) : null;

                if (ECPayModal == null) {
                    Coker.sweet.error("重新付款發生錯誤", "找不到綠界付款視窗");
                    return;
                }

                console.log("ServerType", $("#ECPayModal").data("server-type"));

                ECPay.initialize($("#ECPayModal").data("server-type"), 1, function (errMsg) {
                    if (errMsg == null) {
                        ECPay.createPayment(payResult.message.split(",")[1], ECPay.Language.zhTW, function (errMsg) {
                            if (errMsg != null) {
                                console.log(`Create Payment errMsg : ${errMsg}`);
                                co.sweet.error("串接綠界發生錯誤");
                            }
                        }, "V2");
                    } else {
                        console.log(`Initialize errMsg : ${errMsg}`);
                        co.sweet.error("串接綠界發生錯誤");
                    }
                });

                ECPayModal.show();
                Swal.close();

                $("#ECPayModal .btn_pay").off("click").on("click", function () {
                    if (typeof window.Pay === "undefined") {
                        co.sweet.warning("付款模組尚未載入完成，請稍候再試。", "", null);
                        return;
                    }

                    ECPay.getPayToken(function (paymentInfo, errMsg) {
                        if (errMsg != null) {
                            co.sweet.warning("請確實填寫資料", errMsg, null);
                            return;
                        }

                        ECPayModal.hide();
                        co.sweet.loading();

                        co.ThirdParty.ECPayCreatePayment(paymentInfo).done(function (createResult) {
                            if (!createResult.success) {
                                co.sweet.confirm("付款發生未知錯誤。");
                                return;
                            }

                            var result_obj = JSON.parse(createResult.message);

                            switch (result_obj.OrderInfo.PaymentType) {
                                case null:
                                    localStorage.setItem("lastSaveTime", new Date().toISOString());
                                    localStorage.setItem("lastSaveToken", localStorage.getItem("token"));

                                    var VerifyURL = result_obj.ThreeDInfo?.ThreeDURL ?? result_obj.UnionPayInfo?.UnionPayURL;

                                    window.open(VerifyURL, "_blank");

                                    co.sweet.confirm("即將進入驗證流程", `<div class="text-start">如未自動跳轉，請點此<a class="fw-bold text-primary px-1" href="${VerifyURL}" target="_blank" title="連結至：驗證頁面(開新視窗)">連結</a>進行跳轉</div>`, "確定", "", function () {
                                        location.reload();
                                    });
                                    break;

                                case "ATM":
                                    var ATMInfo = result_obj.ATMInfo;
                                    co.sweet.confirm("訂單付款資訊", `<div class="text-start">繳費銀行代碼：${ATMInfo.BankCode}<br>繳費虛擬帳號：${ATMInfo.vAccount}<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${ATMInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`, "確定", "", function () {
                                        location.reload();
                                    });
                                    break;

                                case "CVS":
                                    var CVSInfo = result_obj.CVSInfo;
                                    co.sweet.confirm("訂單付款資訊", `<div class="text-start">繳費代碼：${CVSInfo.PaymentNo}<br>或點此<a class="fw-bold text-primary px-1" href="${CVSInfo.PaymentURL}" target="_blank" title="連結至：繳費條碼(開新分頁)">連結</a>取得繳費條碼<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${CVSInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`, "確定", "", function () {
                                        location.reload();
                                    });
                                    break;

                                case "BARCODE":
                                    var BarcodeInfo = result_obj.BarcodeInfo;
                                    co.sweet.confirm("訂單付款資訊", `<div class="text-start"><svg id="barcode1" class="w-100"></svg><svg id="barcode2" class="w-100"></svg><svg id="barcode3" class="w-100"></svg><br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${BarcodeInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。<br><br>條碼載入需要一段時間，請耐心等候</div>`, "確定", "", function () {
                                        location.reload();
                                    });

                                    $.getScript("https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js", function () {
                                        JsBarcode("#barcode1", BarcodeInfo.Barcode1, { format: "CODE39", displayValue: true });
                                        JsBarcode("#barcode2", BarcodeInfo.Barcode2, { format: "CODE39", displayValue: true });
                                        JsBarcode("#barcode3", BarcodeInfo.Barcode3, { format: "CODE39", displayValue: true });
                                    });
                                    break;

                                case "ApplePay":
                                    co.sweet.confirm("訂單已成立，謝謝您的訂購！", "", "確定", "", function () {
                                        location.reload();
                                    });
                                    break;
                            }
                        });
                    });
                });

                $("#ECPayModal .btn_cancel").off("click").on("click", function () {
                    co.sweet.custom("warning", "取消付款", "是否確認取消本筆訂單之付款？", "是", function () {
                        co.Member.CancelOrder(datas.orderHeader.id, 4).done(function (result) {
                            if (result.success) {
                                ECPayModal.hide();
                                co.sweet.confirm("訂單已取消", "", "確定", "", function () {
                                    location.reload();
                                });
                            }
                        });
                    }, "否", null);
                });
            });
        };

        if (window.ApplePaySession && typeof ApplePaySession.canMakePayments === "function") {
            ApplePaySession.canMakePayments().then(function (canPay) {
                requestPayment(!!canPay);
            }).catch(function () {
                requestPayment(false);
            });
        } else {
            requestPayment(false);
        }
    });
}

function SetFavoritesPage(number) {
    const $pane = $("#favorite-tab-pane");
    const $content = $pane.find(".content");
    const $pageBtn = $pane.find(".page_btn");
    const $noData = $pane.find(".nodata");
    const $switch = $pane.find(".btn_switchViewType");
    const templateHtml = $("#FavoriteTemplate").html();

    Coker.Favorites.GetDisplay(number).done(function (result) {
        const datas = result && Array.isArray(result.data) ? result.data : [];

        if (datas.length > 0) {
            $noData.addClass("d-none");

            if (result.page_Total > 1) {
                $pageBtn.removeClass("d-none");
                if (!$pageBtn.data("init")) {
                    PageButtonInit($pageBtn, result.page_Total, "favorites");
                }
                ContentPageChage($pageBtn, number, result.page_Total);
            } else {
                $pageBtn.addClass("d-none");
            }

            $switch.removeClass("d-none");

            if (window.DirectoryRenderer && typeof window.DirectoryRenderer.renderItemsByExternalTemplate === "function") {
                DirectoryRenderer.renderItemsByExternalTemplate(
                    $pane,
                    $content,
                    templateHtml,
                    datas
                );
            } else {
                // fallback：保留舊 render 方式，避免 renderer 尚未更新時整個壞掉
                MemberTemplateDataInsert($content, $("#FavoriteTemplate"), datas);
            }
        } else if (number != 1) {
            window.location.hash = "#favorites-1";
        } else {
            $pageBtn.addClass("d-none");
            $switch.addClass("d-none");
            $noData.removeClass("d-none");
            $content.empty();
        }
    });
}

function SetBrowsingHistoryPage(number) {
    const $pane = $("#history-tab-pane");
    const $content = $pane.find(".content");
    const $pageBtn = $pane.find(".page_btn");
    const $noData = $pane.find(".nodata");
    const $switch = $pane.find(".btn_switchViewType");
    const templateHtml = $("#FavoriteTemplate").html();

    Product.GetAll.History(number).done(function (result) {
        const datas = result && Array.isArray(result.data) ? result.data : [];

        if (result.success && datas.length > 0) {
            $noData.addClass("d-none");

            if (result.page_Total > 1) {
                $pageBtn.removeClass("d-none");
                if (!$pageBtn.data("init")) {
                    PageButtonInit($pageBtn, result.page_Total, "browsing");
                }
                ContentPageChage($pageBtn, number, result.page_Total);
            } else {
                $pageBtn.addClass("d-none");
            }

            $switch.removeClass("d-none");

            if (window.DirectoryRenderer && typeof window.DirectoryRenderer.renderItemsByExternalTemplate === "function") {
                DirectoryRenderer.renderItemsByExternalTemplate(
                    $pane,
                    $content,
                    templateHtml,
                    datas
                );

                BrowsingFavoriteButtonInit($content);
            } else {
                MemberTemplateDataInsert($content, $("#FavoriteTemplate"), datas);
                BrowsingFavoriteButtonInit($content);
            }
        } else if (number != 1) {
            window.location.hash = "#browsing-1";
        } else {
            $pageBtn.addClass("d-none");
            $switch.addClass("d-none");
            $noData.removeClass("d-none");
            $content.empty();
        }
    });
}

function PageButtonInit($self, page_total, thishash) {
    $self.removeClass("d-none");
    for (var i = 1; i <= page_total; i++) {
        var html = "";
        if (i == page_total && page_total > 7) {
            html += `<li class="page-item btn_page endhide">
                        <button class="d-none" title="..." disabled="disabled">...</button>
                     </li>`;
        }
        html += `<li class="page-item btn_page">
                    <button class="d-none" data-page="${i}" title="切換至第${i}頁">${i}</button>
                 </li>`;
        if (i == 1 && page_total > 7) {
            html += `<li class="page-item btn_page starthide">
                        <button class="d-none" title="..." disabled="disabled">...</button>
                     </li>`;
        }
        $self.find(".btn_next").before(html);
    }

    $self.data("init", true);

    $self.find(".btn_prev button").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
        var $btn = $(this);
        var page_now = window.location.hash.indexOf('-') < 0 ? 1 : parseInt(window.location.hash.substring(window.location.hash.indexOf('-') + 1));
        if (page_now > 1) page_now -= 1;
        ContentPageChage($btn.parent("li").parent("ul"), page_now, page_total);
        window.location.hash = `#${thishash}-${page_now}`;
    });

    $self.find(".btn_next button").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
        var $btn = $(this);
        var page_now = window.location.hash.indexOf('-') < 0 ? 1 : parseInt(window.location.hash.substring(window.location.hash.indexOf('-') + 1));
        if (page_now < page_total) page_now += 1;
        ContentPageChage($btn.parent("li").parent("ul"), page_now, page_total);
        window.location.hash = `#${thishash}-${page_now}`;
    });

    $self.find(".btn_page button").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
        var $btn = $(this);
        ContentPageChage($btn.parent("li").parent("ul"), $btn.data("page"), page_total);
        window.location.hash = `#${thishash}-${$btn.data("page")}`;
    });
}

function ContentPageChage($self, page, page_total) {
    $self.find("li").each(function () {
        var $this_li = $(this);
        var $this_btn = $this_li.find("button");
        if ($this_btn.data("page") == page) {
            if (!$this_btn.hasClass("focus")) $this_btn.addClass("focus");
            if (typeof ($this_btn.attr("disabled")) == "undefined") $this_btn.attr("disabled", "disabled");
        } else {
            if ($this_btn.hasClass("focus")) $this_btn.removeClass("focus");
            if (typeof ($this_btn.attr("disabled")) != "undefined") $this_btn.removeAttr("disabled");
        }
    });

    if (page_total > 7) {
        if (page < 4) {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ($this_btn.data("page") <= 5 || $this_btn.data("page") == page_total) {
                    if ($this_btn.hasClass("d-none")) $this_btn.removeClass("d-none");
                } else {
                    if (!$this_btn.hasClass("d-none")) $this_btn.addClass("d-none");
                }
            });
        } else if (page > page_total - 3) {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ($this_btn.data("page") >= page_total - 4 || $this_btn.data("page") == 1) {
                    if ($this_btn.hasClass("d-none")) $this_btn.removeClass("d-none");
                } else {
                    if (!$this_btn.hasClass("d-none")) $this_btn.addClass("d-none");
                }
            });
        } else {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ((parseInt(page) + 2 >= $this_btn.data("page") && $this_btn.data("page") >= parseInt(page) - 2) || $this_btn.data("page") == 1 || $this_btn.data("page") == page_total) {
                    if ($this_btn.hasClass("d-none")) $this_btn.removeClass("d-none");
                } else {
                    if (!$this_btn.hasClass("d-none")) $this_btn.addClass("d-none");
                }
            });
        }
        if ($self.find(`li button[data-page=2]`).hasClass("d-none")) {
            if ($self.find("li.starthide button").hasClass("d-none")) $self.find("li.starthide button").removeClass("d-none");
        }
        if ($self.find(`li button[data-page=${page_total - 1}]`).hasClass("d-none")) {
            if ($self.find("li.endhide button").hasClass("d-none")) $self.find("li.endhide button").removeClass("d-none");
        }
    } else {
        $self.find("li").each(function () {
            var $this_li = $(this);
            var $this_btn = $this_li.find("button");
            if ($this_btn.hasClass("d-none")) $this_btn.removeClass("d-none");
        });
    }
}

function MemberTemplateDataInsert($content, $frame, datas) {
    $content.empty();
    $.each(datas, function (index, data) {
        var frame = $($frame.html()).clone();
        frame = MemberDataInsert(frame, data);
        if (frame.find(".btn_favorite").length > 0) FavoritesButtonInit(frame);
        if (frame.find(".shareBlock").length > 0) ShareButtonInit(frame.find(".shareBlock"));
        $content.append(frame);
    });
}

function MemberDataInsert(frame, data) {
    frame.data("Pid", data.pId);
    if (frame.find(".btn_favorite").length > 0 && typeof (data.fId) != "undefined") frame.find(".btn_favorite").data("Fid", data.fId);
    frame.find("*").each(function () {
        var $self = $(this);
        if (typeof ($self.data("key")) != "undefined") {
            var key = $self.data("key");
            switch (key) {
                case "link":
                    $self.attr("href", `/${OrgName}/Member${data[key]}`);
                    $self.attr("title", `連結至：${data['title']}`);
                    break;
                case "image":
                case "imagePath":
                    data[key] = data[key].replaceAll(`/${OrgName}/`, `/`);
                    $self.attr("src", data[key]);
                    $self.attr("alt", `${data['title']}的主要照片`);
                    break;
                case "price":
                    switch (typeof (data[key])) {
                        case "object":
                            var prices = data[key];
                            if (prices == null) {
                                $self.text("");
                            } else {
                                if (prices.length > 1 && prices[0] != prices[prices.length - 1]) $self.html(`$${prices[0].toLocaleString()}<wbr>~<wbr>$${[prices[prices.length - 1].toLocaleString()]}`);
                                else $self.text(`$${prices[0].toLocaleString()}`);
                            }
                            break;
                        default:
                            $self.text(data[key].toLocaleString());
                            break;
                    }
                    break;
                case "describe":
                    if (data[key] == "商品已下架") {
                        $self.removeClass("d-none");
                        $self.siblings().addClass("d-none");
                    }
                    $self.text(data[key]);
                    break;
                case "s1Title":
                case "s2Title":
                    if (data[key] != "") $self.removeClass("d-none");
                    $self.text(data[key]);
                    break;
                case "oldPrice":
                    if (data[key] != 0 && data[key] != data['price']) {
                        $self.removeClass("d-none");
                        $self.text(data[key].toLocaleString());
                        $self.siblings().addClass("red_text");
                    }
                    break;
                case "oldQuantity":
                    if (data[key] != 0 && data[key] != data['quantity']) {
                        $self.removeClass("d-none");
                        $self.text(data[key]);
                        $self.siblings().addClass("red_text");
                    }
                    break;
                case "subtotal_old":
                    var price = data['oldPrice'] != 0 ? data['oldPrice'] : data['price'];
                    var quantity = data['oldQuantity'] != 0 ? data['oldQuantity'] : data['quantity'];
                    $self.text((price * quantity).toLocaleString());
                    break;
                case "subtotal_new":
                    var sbutotal = data['price'] * data['quantity'];
                    $self.text(sbutotal.toLocaleString());
                    break;
                default:
                    $self.text(data[key]);
                    break;
            }
        }
    });
    return frame;
}

function ShareButtonInit($ShareBlock) {
    $ShareBlock.hover(function () {
        $(this).addClass("show");
    }, function () {
        $(this).removeClass("show");
    });

    $ShareBlock.cShare({
        description: 'jQuery plugin - C Share buttons',
        showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
    });
}

function FavoritesButtonInit(frame) {
    var $btn_favorites = frame.find(".btn_favorite");
    if (typeof ($btn_favorites.data("Fid")) == "undefined") {
        Coker.Favorites.Check(frame.data("Pid")).done(function (check) {
            if (check.success) {
                $btn_favorites.data("Fid", check.message);
                $btn_favorites.find("i").addClass("fa-solid");
                $btn_favorites.find("i").removeClass("fa-regular");
                $btn_favorites.attr("title", "移除收藏");
            }
        });
    }
    $btn_favorites.on("click", function () {
        $self = $(this).find("i");
        if ($self.hasClass("fa-regular")) {
            Coker.Favorites.Add(frame.data("Pid")).done(function (result) {
                if (result.success) {
                    $btn_favorites.data("Fid", result.message);
                    $self.addClass("fa-solid");
                    $self.removeClass("fa-regular");
                    $btn_favorites.attr("title", "移除收藏");
                    Coker.sweet.success("成功將商品加入收藏", null, true);
                } else {
                    console.log(result.message);
                }
            });
        } else {
            if (typeof ($btn_favorites.data("Fid")) != "undefined" && typeof ($btn_favorites.data("Fid")) != "") {
                Coker.Favorites.Delete($btn_favorites.data("Fid")).done(function (result) {
                    if (result.success) {
                        $btn_favorites.data("Fid", "");
                        $self.addClass("fa-regular");
                        $self.removeClass("fa-solid");
                        $btn_favorites.attr("title", "加入收藏");
                        Coker.sweet.success("已將商品從收藏中移除", function () {
                            var pagenumber = window.location.hash.substring(window.location.hash.indexOf("-") + 1);
                            if (!isNaN(Number(pagenumber))) SetFavoritesPage(pagenumber);
                        }, false);
                    } else {
                        console.log(result.message);
                    }
                });
            }
        }
    });
}
function BrowsingFavoriteButtonInit($content) {
    $content.find(".type_change_frame").each(function () {
        const $frame = $(this);
        const $btnFavorite = $frame.find(".btn_favorite");
        const $icon = $btnFavorite.find("i");

        if ($btnFavorite.length <= 0 || $icon.length <= 0) return;

        const pid =
            $frame.data("Pid") ||
            $frame.data("pid") ||
            $btnFavorite.data("Pid") ||
            $btnFavorite.data("pid");

        if (!pid) return;

        $frame.data("Pid", pid);

        $btnFavorite.data("Fid", "");
        $btnFavorite.attr("title", "加入收藏");
        $icon.removeClass("fa-solid").addClass("fa-regular");

        FavoritesButtonInit($frame);
    });
}
function ResetmailAction(data) {
    var input_data = co.Form.getJson($("#ResetEmailForm").attr("id"));
    if (input_data.email == old_email) {
        Coker.sweet.info(local.InfoEmailSameNoChange, null);
        resetEmailModal.hide();
    } else {
        Coker.sweet.loading();
        Coker.User.EmailChange(input_data).done(function (result) {
            if (result.success) {
                Coker.sweet.success(local.ResultEmailChangeSuccess, function () {
                    location.reload();
                }, false);
            } else {
                NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
                switch (result.error) {
                    case local.PasswordIncorrect:
                        Coker.sweet.confirm(local.PasswordIncorrect, result.message, `${local.ForgotPassword}?`, local.Confirm, function () {
                            $("#ResetModal .btn_resetforget").click();
                        });
                        break;
                    case local.PasswordErrorThreeTimesTitle:
                        Coker.sweet.error(result.error, result.message, null, false);
                        break;
                    default:
                        Coker.sweet.error(result.error, result.message, null, false);
                        break;
                }
            }
        });
    }
}