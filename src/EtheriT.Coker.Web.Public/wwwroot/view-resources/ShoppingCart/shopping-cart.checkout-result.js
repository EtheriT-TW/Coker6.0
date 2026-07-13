// wwwroot/view-resources/ShoppingCart/shopping-cart.checkout-result.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.CheckoutResult = cart.CheckoutResult || {};

    function hashChange(e) {
        if (!!e) {
            e.preventDefault();
            cart.CheckoutResult.GetOrderPage();
        } else {
        }
    }
    function GetOrderPage() {
        if ($.isNumeric(window.location.search.substring(1))) {
            S.isCheckout = true;
            var ohid = parseInt(window.location.search.substring(1));
            Coker.Order.GetAllData(ohid, true).done(function (results) {
                if (results.length > 0) {
                    var result = results[0];
                    $("#Step4 > .card-header > .order_number").text(window.location.search.substring(1));
                    $("#Step4 > .card-body .pruchase_content .order_time").text(`訂單成立時間：${result.orderHeader.creationTime}`);
                    switch (result.orderHeader.stateStr) {
                        case "待確認":
                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，謝謝您的訂購！");
                            break;
                        case "已付款":
                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立並完成付款，謝謝您的訂購！");
                            break;
                        case "已取消":
                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已取消。");
                            break;
                        case "付款失敗":
                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單付款失敗！");
                            if ($('.buyagain_text').length > 0 && !IsLogin) {
                                $('.buyagain_text').removeClass("d-none");
                                $('.buyagain_text span').on("click", function () {
                                    var ohid = parseInt($("#Step4 .card-header .order_number").text());
                                    Coker.Order.Reorder(ohid).done(function (result) {
                                        if (result.success) {
                                            var ohidstr = `000000000${result.message}`.substring(result.message.length);
                                            window.location.href = `/${OrgName}/ShoppingCar?reorder${ohidstr}`;
                                        } else {
                                            Coker.sweet.error("錯誤", result.message)
                                        }
                                    });
                                });
                            }
                            break;
                        case "待付款":
                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，待商家確認付款資訊，謝謝您的訂購！");
                            break;
                    }
                    cart.CheckoutResult.SuccessPageDataInsert(result);
                } else {
                    if (S.islogin) {
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("查無訂單資訊或期限已過，請至會員管理歷史訂單中確認");
                    } else {
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("查無訂單資訊");
                    }
                }
                S.buy_step_swiper.enable();
                S.buy_step_swiper.slideTo(4);
                S.buy_step_swiper.disable();
            });
        } else if (window.location.search.substring(1).startsWith("reorder")) {
            var ohid = parseInt(window.location.search.substring("reorder".length + 1));
            Coker.Order.GetReorder(ohid).done(function (result) {
                if (result.success && result.object != null) {
                    $("#Step1 > .card-body").removeClass("d-none");
                    S.buy_step_swiper.enable();
                    $("#Purchase_Null").addClass("d-none");
                    cart.Items.CartInit(result.object)
                } else {
                    window.location.href = `/${OrgName}/ShoppingCar`;
                }
            })
        } else if (window.location.search.substring(1).startsWith("ECPayError")) {
            co.sweet.confirm("訂單付款發生錯誤", "", "確認", "", null);
        }
    }
    function SuccessPageDataInsert(data) {
        var header = data.orderHeader || {};
        var details = data.orderDetails || [];

        cart.Utils.ShoppingCartDataInsert(header, $("#Step4 .card-body"));

        cart.CheckoutResult.toggleStep4EndlineDisplay(header);
        cart.CheckoutResult.toggleStep4InvoiceDisplay(header);
        cart.CheckoutResult.toggleStep4PaymentDisplay(header);

        cart.Utils.TemplateDataInsert($("#Purchase"), $("#CollapsePurchase"), $("#Template_Purchase_Details"), details);

        S.buy_step_swiper.update();
    }
    function toggleStep4EndlineDisplay(header) {
        header = header || {};

        var productBonus = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(header, "productBonus"));
        var redeemBonus = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(header, "redeemBonus"));
        var totalBonus = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(header, "bonus"));
        var discount = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(header, "discount"));

        $("#Step4 .productBonusLine").toggleClass("d-none", productBonus <= 0);
        $("#Step4 .bonusDiscionLine").toggleClass("d-none", redeemBonus <= 0);
        $("#Step4 .bonusUseTotalLine").toggleClass("d-none", totalBonus <= 0);

        $("#Step4 .step4MarketingDiscountLine").toggleClass("d-none", discount <= 0);
        $("#Step4 .step4MarketingDiscount").text(discount > 0 ? discount.toLocaleString() : "");

        // Step4 運費是費用列，0 元也要顯示
        var freightText = String(cart.Utils.getValueIgnoreCase(header, "freight") ?? "");
        if (freightText !== "") {
            $("#Step4 .shipping_fee").text(cart.Utils.toNumberValue(freightText).toLocaleString());
        }
    }
    function toggleStep4InvoiceDisplay(header) {
        header = header || {};

        var invoiceRecipient = Number(cart.Utils.getValueIgnoreCase(header, "invoiceRecipient") || 0);

        $("#Step4 .invoice_type .person").addClass("d-none");
        $("#Step4 .invoice_type .company").addClass("d-none");
        $("#Step4 .invoice_type .mobileCarrier").addClass("d-none");

        $("#Step4 .invoice_data .orderer").addClass("d-none");
        $("#Step4 .invoice_data .recipient").addClass("d-none");

        var invoiceTypeTitle = String(cart.Utils.getValueIgnoreCase(header, "invoiceTypeTitle") || "");
        var personalInvoiceTypeTitle = String(cart.Utils.getValueIgnoreCase(header, "personalInvoiceTypeTitle") || "");
        var carrier = String(cart.Utils.getValueIgnoreCase(header, "carrier") || "");

        if (invoiceTypeTitle.indexOf("公司") >= 0) {
            $("#Step4 .invoice_type .company").removeClass("d-none");
        } else {
            $("#Step4 .invoice_type .person").removeClass("d-none");

            if (carrier !== "" || personalInvoiceTypeTitle.indexOf("載具") >= 0 || personalInvoiceTypeTitle.indexOf("手機") >= 0) {
                $("#Step4 .invoice_type .mobileCarrier").removeClass("d-none");
            }
        }

        switch (invoiceRecipient) {
            case 1:
                $("#Step4 .invoice_data .orderer").removeClass("d-none");
                break;
            case 2:
                $("#Step4 .invoice_data .recipient").removeClass("d-none");
                break;
        }
    }
    function toggleStep4PaymentDisplay(header) {
        header = header || {};

        var payment = String(cart.Utils.getValueIgnoreCase(header, "payment") || "");
        var total = String(cart.Utils.getValueIgnoreCase(header, "total") || "");
        var memo = String(cart.Utils.getValueIgnoreCase(header, "memo") || "");

        if (payment !== "") {
            $("#PaymentData").removeClass("d-none");
            $("#PaymentData .payment_method").text(payment);
        } else {
            $("#PaymentData").addClass("d-none");
        }

        if (total !== "") {
            $("#PaymentData .pay_info .total_amount").text(total);
        }

        $("#PaymentData .pay_info .paid_date").empty();

        if (S.step4PaidDateHtml !== "") {
            $("#PaymentData .pay_info .paid_date").append(S.step4PaidDateHtml);
            $("#PaymentData .pay_info").removeClass("d-none");
        } else if (memo !== "") {
            $("#PaymentData .pay_info .paid_date").html(memo);
            $("#PaymentData .pay_info").removeClass("d-none");
        }
    }
    function OrderSuccess(result) {
        var noticeDeferred = $.Deferred();

        var message = result.message.split(",");
        var order_header_id = message[1];
        S.step4PaidDateHtml = message.length > 3 ? (message[3] || "") : "";

        CartClear();

        $("#Step4 > .card-header > .order_number")
            .text(("000000000" + order_header_id).substring(order_header_id.length));

        $("#Step4 > .card-body .pruchase_content .order_time")
            .text(`訂單成立時間：${message[2]}`);

        $("#Step4 > .card-body > .pruchase_content > .status_alert")
            .text("訂單已成立，訂單明細載入中，請稍候...");

        S.buy_step_swiper.enable();
        S.buy_step_swiper.slideNext();
        S.buy_step_swiper.update();
        S.buy_step_swiper.disable();

        Coker.sweet.processing(
            "訂單成立中",
            "正在載入訂單明細，請稍候..."
        );

        Coker.Order.GetAllData(order_header_id, true)
            .done(function (results) {
                if (results && results.length > 0) {
                    cart.CheckoutResult.SuccessPageDataInsert(results[0]);

                    $("#Step4 > .card-body > .pruchase_content > .status_alert")
                        .text("訂單已成立，謝謝您的訂購！");
                } else {
                    $("#Step4 > .card-body > .pruchase_content > .status_alert")
                        .text("訂單已成立，但查無訂單明細，請至會員管理歷史訂單中確認。");
                }
            })
            .fail(function () {
                $("#Step4 > .card-body > .pruchase_content > .status_alert")
                    .text("訂單已成立，但訂單明細載入失敗，請至會員管理歷史訂單中確認。");
            })
            .always(function () {
                Coker.sweet.close();

                cart.CheckoutResult.ShowOrderSuccessNotice(result, function () {
                    noticeDeferred.resolve();
                });
            });

        return noticeDeferred.promise();
    }
    function ShowOrderSuccessNotice(result, callback) {
        var storeMemoText = $.trim($(".storememo").text() || "");

        function done() {
            if (typeof callback === "function") {
                callback();
            }
        }

        function showMailErrorIfNeed() {
            if (result.error == null) {
                done();
                return;
            }

            if (!S.islogin) {
                Coker.sweet.warning(
                    "信件發送失敗",
                    "訂購信件發送失敗，請註冊會員以查看詳細訂單，或將訂單完成頁面截圖。",
                    done
                );
            } else {
                Coker.sweet.warning(
                    "信件發送失敗",
                    "訂購信件發送失敗，訂單詳細可於會員管理歷史訂單中查看。",
                    done
                );
            }
        }

        if (storeMemoText !== "") {
            Coker.sweet.notice(
                "小提醒",
                storeMemoText.replaceAll("\n", "<br/>"),
                showMailErrorIfNeed
            );
        } else {
            showMailErrorIfNeed();
        }
    }

    function setStatus(message) {
        var $target = $("#Step4 > .card-body > .pruchase_content > .status_alert");
        if (String(message || "").indexOf("<") >= 0) $target.html(message);
        else $target.text(message || "");
    }
    function showThirdPayLink(url, title) {
        if (!url) return;
        $("#Step4 > .card-body .thirdpay_link a").attr({
            href: url,
            title: title || "連結至：付款頁面(開新視窗)"
        });
        $("#Step4 > .card-body .thirdpay_link").removeClass("d-none");
    }
    function goToResultPage() {
        S.buy_step_swiper.slideNext();
        S.buy_step_swiper.update();
        S.buy_step_swiper.disable();
    }


    Object.assign(cart.CheckoutResult, {
        hashChange: hashChange,
        GetOrderPage: GetOrderPage,
        SuccessPageDataInsert: SuccessPageDataInsert,
        toggleStep4EndlineDisplay: toggleStep4EndlineDisplay,
        toggleStep4InvoiceDisplay: toggleStep4InvoiceDisplay,
        toggleStep4PaymentDisplay: toggleStep4PaymentDisplay,
        OrderSuccess: OrderSuccess,
        ShowOrderSuccessNotice: ShowOrderSuccessNotice,
        setStatus: setStatus,
        showThirdPayLink: showThirdPayLink,
        goToResultPage: goToResultPage
    });
})(window.ShoppingCart, window.jQuery);
