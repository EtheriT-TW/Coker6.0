// wwwroot/view-resources/ShoppingCart/shopping-cart.payment.ecpay.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Payment = cart.Payment || {};
    cart.Payment.ECPay = cart.Payment.ECPay || {};
    var ecpaySelectionObserver = null;
    var isClearingECPaySelection = false;

    function GetECPayEntryRadio() {
        return $('#RadioPayment input[name="RadioPayment"][data-third-party-id="' + S.ECPAY_THIRD_PARTY_ID + '"]').first();
    }

    function GetECPayEntryValue() {
        var $radio = GetECPayEntryRadio();
        return $radio.length ? $radio.val() : null;
    }

    function IsPaymentRadioECPay($radio) {
        return $radio &&
            $radio.length > 0 &&
            Number($radio.attr("data-third-party-id") || 0) === S.ECPAY_THIRD_PARTY_ID;
    }

    function IsECPaySelected() {
        var $checked = cart.Payment.Core.GetCheckedPaymentRadio();

        // 如果目前已經有明確選中的 RadioPayment，
        // 就以 RadioPayment 為準。
        // 避免綠界 SDK 自動加上的 .ecpay-pl-act 反過來搶走付款狀態。
        if ($checked.length > 0) {
            return S.HasECPay && IsPaymentRadioECPay($checked);
        }

        // 只有在沒有任何 RadioPayment 被選取時，
        // 才允許用綠界內部 active 狀態判斷。
        return S.HasECPay &&
            $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li.ecpay-pl-act").length > 0;
    }
    function BuildECPayOrderSnapshot() {
        var ids = cart.Items.getSelectedCartIds();

        var details = S.shopping_cart_data
            .filter(e => ids.includes(e.Id))
            .map(e => ({
                Id: Number(e.Id || 0),
                Price: Number(e.Price || 0),
                Quantity: Number(e.Quantity || 0),
                Bonus: Number(e.Bonus || 0)
            }))
            .sort((a, b) => a.Id - b.Id);

        return JSON.stringify({
            subtotal: Number(S.subtotal || 0),
            freight: Number(S.freight || 0),
            total: Number(S.total || 0),
            shipping: String(S.order_header_data.shipping || ""),
            cvsStoreId: String(S.order_header_data.CVSStoreID || ""),
            details: details
        });
    }
    function ECPaymentChange() {
        if (!S.ECPayMonitor || !S.HasECPay) return;

        var selectedPaymentBeforeSync = cart.Payment.Core.GetCheckedPaymentValue();
        var restorePaymentAfterSync = IsECPaySelected()
            ? GetECPayEntryValue()
            : selectedPaymentBeforeSync;

        cart.Pricing.TotalCount();

        var dataReady = cart.Forms.AllDataGet(false);
        cart.Payment.Core.Step3Monitor();

        if (!dataReady || S.SupportCashOnDelivery) {
            if (S.ECPayReady) {
                S.ECPayReady = false;
                S.ECPayOrderSnapshot = "";
                cart.Shipping.ConfigurePaymentOptions(null);
            }
            return;
        }

        var nextSnapshot = BuildECPayOrderSnapshot();

        if (S.ECPayChanging) {
            console.log("ECPaymentChange skipped: syncing");
            return;
        }

        if (S.ECPayReady && S.ECPayOrderSnapshot === nextSnapshot && typeof window.Pay !== "undefined" && $("#ECPayPayment").children().length > 0) {
            console.log("ECPaymentChange skipped: same snapshot");
            return;
        }

        S.ECPayChanging = true;
        S.ECPayReady = false;

        $(".ecpay_loading").removeClass("d-none").text("付款模組載入中...");
        $("#RadioPayment > .form-check").addClass("d-none");
        $("#ECPayPayment").empty();
        cart.Shipping.ConfigurePaymentOptions(null);

        var timeout = 0;
        var checkInterval = setInterval(function () {
            if (S.ECPayInit !== true) {
                timeout += 100;
                if (timeout >= 10000) {
                    clearInterval(checkInterval);
                    S.ECPayChanging = false;
                    $(".ecpay_loading").text("串接綠界發生錯誤(初始化失敗-逾時)");
                }
                return;
            }

            clearInterval(checkInterval);

            Coker.ThirdParty.ECPayGetToken(S.order_header_data)
                .done(function (result) {
                    if (!result.success) {
                        S.ECPayChanging = false;
                        S.ECPayReady = false;
                        $(".ecpay_loading").text("串接綠界發生錯誤，請稍後嘗試");
                        console.log(result.message);
                        return;
                    }

                    var message = result.message.split(",");
                    S.order_header_data.orderId = message[0];

                    ECPay.createPayment(message[1], ECPay.Language.zhTW, function (errMsg) {
                        if (errMsg != null) {
                            S.ECPayChanging = false;
                            S.ECPayReady = false;
                            $(".ecpay_loading").text(`串接綠界發生錯誤(${errMsg})`);
                            return;
                        }

                        S.ECPayReady = true;
                        S.ECPayChanging = false;
                        S.ECPayOrderSnapshot = nextSnapshot;

                        var currentPaymentValue = cart.Payment.Core.GetCheckedPaymentValue();
                        var ecpayEntryValue = GetECPayEntryValue();

                        var paymentValueToRestore =
                            currentPaymentValue && currentPaymentValue !== ecpayEntryValue
                                ? currentPaymentValue
                                : (restorePaymentAfterSync || ecpayEntryValue);

                        cart.Shipping.ConfigurePaymentOptions(paymentValueToRestore);
                        cart.Payment.Core.RadioPayment();

                        // 綠界 SDK 可能在 createPayment 後自動選取第一個付款項目。
                        // 先立刻清一次，再啟動 DOM 監聽，避免 SDK 稍後又補上 active。
                        ClearECPaySelectionIfNotActive();
                        WatchECPaySelectionAutoActive();

                        var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");
                        $ECPayList.removeClass("first last");
                        $ECPayList.first().next("li").addClass("first");
                        $ECPayList.last().addClass("last");

                        $("#ECPayPayment").off("click.ecpayPayment").on("click.ecpayPayment", function (e) {
                            const trusted = e.originalEvent?.isTrusted;
                            if (trusted !== true) return;

                            var $this_radio = GetECPayEntryRadio();
                            if ($this_radio.length === 0) return;

                            cart.Shipping.ConfigurePaymentOptions($this_radio.val());

                            var $parentFormCheck = $this_radio.closest(".form-check");
                            var $prevPayment = $parentFormCheck.prevAll(".form-check").first().find(".payment_display");

                            $("#RadioPayment .payment_display").removeClass("checked first last");
                            $this_radio.prop("checked", true);
                            $parentFormCheck.find(".payment_display").addClass("checked");
                            $("#RadioPayment .payment_display").first().addClass("first");
                            $prevPayment.addClass("last");
                            cart.Payment.Core.RadioPayment();

                            if ($(".ecpay_loading").hasClass("d-none")) {
                                $ECPayList.removeClass("first last");

                                var $activeLi = $ECPayList.filter(".ecpay-pl-act");

                                if ($activeLi.prev("li").length == 0) {
                                    if ($("#RadioPayment .payment_display").length > 1) $prevPayment.addClass("last");
                                } else {
                                    if ($("#RadioPayment .payment_display").length == 1) $ECPayList.first().addClass("first");
                                    $prevPayment.removeClass("last");
                                    $activeLi.prev("li").addClass("last");
                                }

                                $activeLi.addClass("first last");
                                $activeLi.next("li").addClass("first");
                                $ECPayList.last().addClass("last");
                            }

                            S.buy_step_swiper.update();
                        });

                        var checkPayExist = setInterval(function () {
                            if (typeof window.Pay !== "undefined") {
                                clearInterval(checkPayExist);

                                $(".ecpay_loading").addClass("d-none");

                                if (S.buy_step_swiper) {
                                    S.buy_step_swiper.update();
                                }
                            }
                        }, 100);
                    }, "V2");
                })
                .fail(function () {
                    S.ECPayChanging = false;
                    S.ECPayReady = false;
                    $(".ecpay_loading").text("串接綠界發生錯誤，請稍後嘗試");
                });
        }, 100);
    }
    function MarkECPayDirty() {
        if (!S.HasECPay) return;

        S.ECPayReady = false;
        S.ECPayOrderSnapshot = "";

        if (S.ECPayRefreshTimer != null) {
            clearTimeout(S.ECPayRefreshTimer);
            S.ECPayRefreshTimer = null;
        }
    }
    function IsCurrentPaymentECPay() {
        return IsPaymentRadioECPay(cart.Payment.Core.GetCheckedPaymentRadio());
    }

    function ClearECPaySelectionIfNotActive() {
        if (isClearingECPaySelection) return;

        // 目前外部 RadioPayment 是綠界時，不可以清掉綠界內部選取。
        if (IsCurrentPaymentECPay()) return;

        // 沒有綠界 active 時，不要重複整理 first / last，
        // 避免 MutationObserver 因為 class 變動被自己反覆觸發。
        var hasActive = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li.ecpay-pl-act").length > 0;
        if (!hasActive) return;

        ClearECPaySelection();
    }
    function ClearECPaySelection() {
        if (isClearingECPaySelection) return;

        isClearingECPaySelection = true;

        try {
            var $items = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");

            // 沒有 active 時，不需要再改 class。
            if ($items.filter(".ecpay-pl-act").length === 0) {
                return;
            }

            $items.removeClass("ecpay-pl-act first last");

            $items.first().addClass("first");
            $items.last().addClass("last");

            if (S.buy_step_swiper) {
                S.buy_step_swiper.update();
            }
        } finally {
            isClearingECPaySelection = false;
        }
    }
    function WatchECPaySelectionAutoActive() {
        var target = document.getElementById("ECPayPayment");

        if (!target) return;

        if (ecpaySelectionObserver) {
            ecpaySelectionObserver.disconnect();
            ecpaySelectionObserver = null;
        }

        ecpaySelectionObserver = new MutationObserver(function () {
            if (isClearingECPaySelection) return;

            window.requestAnimationFrame(function () {
                ClearECPaySelectionIfNotActive();
            });
        });

        ecpaySelectionObserver.observe(target, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ["class"]
        });
    }
    function GetECPayType() {
        var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");
        var $activeLi = $ECPayList.filter(".ecpay-pl-act");

        $("#Step4 .payment_method").text($activeLi.find(".ecpay-pl-intro .ecpay-pl-type").text());

        var payment = null;

        switch ($activeLi.attr("id")) {
            case "CreditCard":
                payment = 16;
                break;

            case "CreditInstallment":
                var stage = String($activeLi.find("select.ecpay-Installment").val() || "");

                switch (stage) {
                    case "3":
                        payment = 18;
                        $("#Step4 .payment_method").text("信用卡付款 (3期)");
                        break;

                    case "6":
                        payment = 19;
                        $("#Step4 .payment_method").text("信用卡付款 (6期)");
                        break;

                    case "12":
                        payment = 20;
                        $("#Step4 .payment_method").text("信用卡付款 (12期)");
                        break;

                    default:
                        payment = 16;
                        break;
                }
                break;

            case "UnionPay":
                payment = 17;
                break;

            case "ATM":
                payment = 21;
                break;

            case "CVS":
                payment = 23;
                break;

            case "Barcode":
                payment = 22;
                break;

            case "ApplePay":
                payment = 27;
                $("#Step4 .payment_method").text("Apple Pay");
                break;
        }

        if (payment != null) {
            S.order_header_data.payment = payment;
        }

        return payment;
    }
    function GetActiveECPayType() {
        return $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li.ecpay-pl-act").attr("id") || "";
    }

    function ClearApplePayWatch() {
        if (S.ECPayApplePayTimer) {
            clearTimeout(S.ECPayApplePayTimer);
            S.ECPayApplePayTimer = null;
        }
    }

    function IsApplePayResultSuccess(resultData) {
        if (resultData == null) return false;

        var rtnCode = String(
            resultData.RtnCode ??
            resultData.rtnCode ??
            resultData.RtnValue?.RtnCode ??
            resultData.rtnValue?.rtnCode ??
            ""
        );

        var rtnMsg = String(
            resultData.RtnMsg ??
            resultData.rtnMsg ??
            resultData.RtnValue?.RtnMsg ??
            resultData.rtnValue?.rtnMsg ??
            ""
        );

        var orderInfo = resultData.OrderInfo || resultData.orderInfo || {};
        var tradeStatus = String(orderInfo.TradeStatus ?? orderInfo.tradeStatus ?? "");

        // 有 TradeStatus 時，兩個都成立最安全
        if (rtnCode === "1" && tradeStatus === "1") return true;

        // 沒有 TradeStatus 時，只要 RtnCode = 1，先視為 ApplePay 前端流程成功
        // 最終付款狀態仍以後端 ReturnURL / QueryTrade 為準
        if (rtnCode === "1" && tradeStatus === "") return true;

        // 保留文字成功的容錯
        if (rtnMsg.toLowerCase().indexOf("success") >= 0) return true;

        return false;
    }

    function GetApplePayErrorMessage(resultData, errMsg) {
        if (errMsg) return errMsg;
        if (resultData && (resultData.RtnMsg || resultData.rtnMsg)) return resultData.RtnMsg || resultData.rtnMsg;
        return "Apple Pay 付款未完成，請重新操作。";
    }

    function CompleteApplePayOrder(resultData) {
        if (S.ECPayApplePayCompleted) return;

        S.ECPayApplePayCompleted = true;
        S.ECPayApplePayWaitingResult = false;
        ClearApplePayWatch();

        console.group("ECPay ApplePay success result");
        console.log("resultData:", resultData);
        console.groupEnd();

        // ApplePay 成功後一定要保險校正。
        S.order_header_data.payment = 27;
        $("#Step4 .payment_method").text("Apple Pay");

        cart.Order.AddHeader({
            IsApplePay: true,
            PaymentType: "ApplePay",
            ApplePayResultData: resultData,
            MerchantTradeNo:
                resultData?.OrderInfo?.MerchantTradeNo ||
                resultData?.orderInfo?.merchantTradeNo ||
                resultData?.MerchantTradeNo ||
                resultData?.merchantTradeNo ||
                null,
            TradeNo:
                resultData?.OrderInfo?.TradeNo ||
                resultData?.orderInfo?.tradeNo ||
                resultData?.TradeNo ||
                resultData?.tradeNo ||
                null
        });
    }

    function FailApplePayOrder(message, rawData) {
        if (S.ECPayApplePayCompleted) return;

        S.ECPayApplePayCompleted = true;
        S.ECPayApplePayWaitingResult = false;
        ClearApplePayWatch();
        Swal.close();

        console.group("ECPay ApplePay failed result");
        console.log("message:", message);
        console.log("rawData:", rawData);
        console.groupEnd();

        Coker.sweet.warning("Apple Pay 付款失敗", message || "付款未完成，請重新操作。", null);
    }

    function ValidateECPayPayment(callback) {
        if (!S.ECPayReady || S.ECPayChanging || typeof window.Pay === "undefined" || $("#ECPayPayment").children().length === 0) {
            callback(false, "綠界付款模組尚未載入完成，請稍候再試。");
            return;
        }

        var activePayment = GetActiveECPayType();
        var isApplePay = activePayment === "ApplePay";

        if (isApplePay) {
            S.ECPayApplePayCompleted = false;
            S.ECPayApplePayWaitingResult = true;

            ClearApplePayWatch();
            S.ECPayApplePayTimer = setTimeout(function () {
                if (!S.ECPayApplePayWaitingResult || S.ECPayApplePayCompleted) return;

                S.ECPayApplePayWaitingResult = false;
                Swal.close();

                console.error("ECPay ApplePay timeout: getApplePayResultData was not called.", {
                    activePayment: activePayment,
                    ECPayReady: S.ECPayReady,
                    ECPayChanging: S.ECPayChanging,
                    PayType: typeof window.Pay,
                    children: $("#ECPayPayment").children().length
                });

                callback(false, {
                    handled: true,
                    message: "Apple Pay 付款流程逾時，系統未收到綠界付款結果。若裝置已顯示付款成功，請先勿重複付款，請聯絡客服確認交易狀態。"
                });

                Coker.sweet.warning(
                    "Apple Pay 付款流程逾時",
                    "系統未收到綠界 Apple Pay 付款結果。若裝置已顯示付款成功，請先勿重複付款，請聯絡客服確認交易狀態。",
                    null
                );
            }, 60000);
        }

        try {
            ECPay.getPayToken(function (paymentInfo, errMsg) {
                console.group("ECPay getPayToken callback");
                console.log("activePayment:", activePayment);
                console.log("paymentInfo:", paymentInfo);
                console.log("errMsg:", errMsg);
                console.groupEnd();

                if (errMsg != null) {
                    if (isApplePay) {
                        FailApplePayOrder(errMsg, paymentInfo);
                        callback(false, { handled: true, message: errMsg });
                        return;
                    }

                    co.sweet.warning("請確實填寫付款資料", errMsg, null);
                    callback(false, errMsg);
                    return;
                }

                // 綠界文件說 Apple Pay 不會回 PayToken，
                // 付款結果會從 getApplePayResultData 回來。
                // 因此 Apple Pay 不在這裡 callback(true)，避免還沒取得 Apple Pay 結果就建單。
                if (isApplePay) return;

                callback(true, paymentInfo);
            });
        } catch (ex) {
            console.error("ECPay.getPayToken exception:", ex);

            if (isApplePay) {
                FailApplePayOrder("Apple Pay 付款流程發生例外，請重新操作。", ex);
                callback(false, { handled: true, message: ex.message || String(ex) });
                return;
            }

            callback(false, "綠界付款流程發生例外，請重新操作。");
        }
    }

    function getApplePayResultData(resultData, errMsg) {
        console.group("ECPay getApplePayResultData");
        console.log("resultData:", resultData);
        console.log("errMsg:", errMsg);
        console.groupEnd();

        if (errMsg != null) {
            FailApplePayOrder(errMsg, resultData);
            return;
        }

        if (!IsApplePayResultSuccess(resultData)) {
            FailApplePayOrder(GetApplePayErrorMessage(resultData, errMsg), resultData);
            return;
        }

        CompleteApplePayOrder(resultData);
    }

    // 綠界 SDK 會呼叫全域 getApplePayResultData；只掛在 cart.Payment.ECPay 可能接不到。
    window.getApplePayResultData = getApplePayResultData;
    window.GetApplePayResultData = getApplePayResultData;

    function afterOrderCreated(orderResult, context) {
        var paymentInfo = context ? context.paymentInfo : null;
        co.sweet.loading();

        var isApplePay =
            S.order_header_data.payment == 27 ||
            (paymentInfo && paymentInfo.IsApplePay === true) ||
            (paymentInfo && paymentInfo.PaymentType === "ApplePay");

        if (!isApplePay && paymentInfo != null) {
            co.ThirdParty.ECPayCreatePayment(paymentInfo).done(function (result) {
                Swal.close();

                if (result.success) {
                    var result_obj = JSON.parse(result.message);
                    var SwalClose = false;

                    switch (result_obj.OrderInfo.PaymentType) {
                        case null:
                        case "Credit":
                        case "UnionPay":
                            localStorage.setItem("lastSaveTime", new Date().toISOString());
                            localStorage.setItem("lastSaveToken", localStorage.getItem("token"));

                            var VerifyURL = result_obj.ThreeDInfo?.ThreeDURL ?? result_obj.UnionPayInfo?.UnionPayURL;

                            cart.CheckoutResult.setStatus("訂單已成立，即將進入驗證流程。");
                            cart.CheckoutResult.showThirdPayLink(VerifyURL, "連結至：驗證頁面(開新視窗)");

                            SwalClose = true;
                            window.open(VerifyURL, "_blank");
                            break;

                        case "ATM":
                            var ATMInfo = result_obj.ATMInfo;
                            cart.CheckoutResult.setStatus(`訂單已成立，請於${ATMInfo.ExpireDate}前完成付款。`);
                            co.sweet.confirm(
                                "訂單付款資訊",
                                `<div class="text-start">繳費銀行代碼：${ATMInfo.BankCode}<br>繳費虛擬帳號：${ATMInfo.vAccount}<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${ATMInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`,
                                "確定",
                                "",
                                null
                            );
                            break;

                        case "CVS":
                            var CVSInfo = result_obj.CVSInfo;
                            cart.CheckoutResult.setStatus(`訂單已成立，請於${CVSInfo.ExpireDate}前完成付款。`);
                            co.sweet.confirm(
                                "訂單付款資訊",
                                `<div class="text-start">繳費代碼：${CVSInfo.PaymentNo}<br>或點此<a class="fw-bold text-primary px-1" href="${CVSInfo.PaymentURL}" target="_blank" title="連結至：繳費條碼(開新分頁)">連結</a>取得繳費條碼<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${CVSInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`,
                                "確定",
                                "",
                                null
                            );
                            break;

                        case "BARCODE":
                        case "Barcode":
                            var BarcodeInfo = result_obj.BarcodeInfo;
                            cart.CheckoutResult.setStatus(`訂單已成立，請於${BarcodeInfo.ExpireDate}前完成付款。`);
                            co.sweet.confirm(
                                "訂單付款資訊",
                                `<div class="text-start"><svg id="barcode1" class="barcode_svg w-100"></svg><svg id="barcode2" class="barcode_svg w-100"></svg><svg id="barcode3" class="barcode_svg w-100"></svg><br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${BarcodeInfo.ExpireDate}</span>前完成付款，感謝您的訂購。<br><br>條碼載入需要一段時間，請耐心等候</div>`,
                                "確定",
                                "",
                                null
                            );

                            $.getScript("https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js", function () {
                                JsBarcode("#barcode1", BarcodeInfo.Barcode1, { format: "CODE39", displayValue: true });
                                JsBarcode("#barcode2", BarcodeInfo.Barcode2, { format: "CODE39", displayValue: true });
                                JsBarcode("#barcode3", BarcodeInfo.Barcode3, { format: "CODE39", displayValue: true });
                            });
                            break;

                        default:
                            co.sweet.confirm(`回傳的PaymentType：${result_obj.OrderInfo.PaymentType}`, "此為測試訊息", "確認", "", null);
                            break;
                    }

                    setTimeout(function () {
                        cart.CheckoutResult.goToResultPage();
                        if (SwalClose) Swal.close();
                    }, 300);
                } else {
                    cart.CheckoutResult.setStatus(`<div>付款流程發生錯誤，${result.message + "<br>"}請稍後重新嘗試，或直接聯繫客服人員。</div>`);
                    setTimeout(function () {
                        cart.CheckoutResult.goToResultPage();
                        Swal.close();
                    }, 300);
                }
            });

            return;
        }

        Swal.close();

        if (isApplePay) {
            cart.CheckoutResult.setStatus(
                "<div>Apple Pay 付款已完成，訂單付款狀態處理中。若畫面尚未更新，請稍後至會員中心查詢訂單狀態。</div>"
            );
        } else {
            cart.CheckoutResult.setStatus(
                "<div>付款資料驗證失敗，請返回上一頁重新確認付款資料。</div>"
            );
        }

        setTimeout(function () {
            cart.CheckoutResult.goToResultPage();
        }, 300);
    }


    Object.assign(cart.Payment.ECPay, {
        BuildECPayOrderSnapshot: BuildECPayOrderSnapshot,
        ECPaymentChange: ECPaymentChange,
        MarkECPayDirty: MarkECPayDirty,
        GetECPayType: GetECPayType,
        ValidateECPayPayment: ValidateECPayPayment,
        getApplePayResultData: getApplePayResultData,
        afterOrderCreated: afterOrderCreated
    });

    cart.Payment.Core.register({
        code: "ECPay",
        type: "embedded",
        thirdPartyId: S.ECPAY_THIRD_PARTY_ID,
        init: function () {
            if ($("#ECPayPayment").length === 0) return;

            S.HasECPay = true;
            S.ECPayMonitor = true;

            ECPay.initialize($("#ECPayPayment").data("server-type"), 1, function (errMsg) {
                if (errMsg != null) {
                    GetECPayEntryRadio().closest(".form-check").addClass("d-none");
                    console.log(`Initialize errMsg : ${errMsg}`);
                    co.sweet.error("串接綠界發生錯誤");
                    return;
                }

                S.ECPayInit = true;

                var $ecpayRadio = GetECPayEntryRadio();

                if ($ecpayRadio.length) {
                    $ecpayRadio.prop("checked", true);
                    $ecpayRadio.closest(".form-check").prevAll(".form-check").first().find(".payment_display").addClass("last");
                }

                $("#RadioPayment .payment_display").on("click.ecpayInit", function () {
                    var $thisRadioDisplay = $(this);
                    var $parentFormCheck = $thisRadioDisplay.closest(".form-check");
                    var $nextPaymentRadio = $parentFormCheck
                        .nextAll(".form-check")
                        .first()
                        .find('input[name="RadioPayment"]');

                    var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");

                    $ECPayList.removeClass("first");

                    if ($nextPaymentRadio.length && IsPaymentRadioECPay($nextPaymentRadio)) {
                        $ECPayList.first().addClass("first");
                    }

                    $ECPayList.removeClass("ecpay-pl-act");

                    if (S.buy_step_swiper) {
                        S.buy_step_swiper.update();
                    }
                });
            });

            GetECPayEntryRadio()
                .closest(".form-check")
                .addClass("d-none");
        },

        getEntryRadio: function () {
            return GetECPayEntryRadio();
        },

        getEntryValue: function () {
            return GetECPayEntryValue();
        },

        isMatchRadio: function ($radio) {
            return IsPaymentRadioECPay($radio);
        },

        isSelected: function () {
            return IsECPaySelected();
        },

        isReady: function () {
            return S.ECPayReady === true;
        },

        isLoaded: function () {
            return typeof window.Pay !== "undefined" && $("#ECPayPayment").children().length > 0;
        },

        getPaymentValue: function () {
            if (this.isSelected()) {
                var payment = GetECPayType();

                if (payment != null && payment !== "") {
                    return payment;
                }
            }

            return this.getEntryValue();
        },

        reload: function () {
            return ECPaymentChange();
        },

        markDirty: function () {
            MarkECPayDirty();
        },

        clear: function () {
            $("#ECPayPayment").empty();
        },

        setMonitor: function (enabled) {
            S.ECPayMonitor = enabled === true;
        },

        validatePayment: function (callback) {
            return ValidateECPayPayment(callback);
        },

        submitPayment: function (callback) {
            cart.Pricing.TotalCount();

            // 先同步共用訂單資料。
            // AllDataGet 會透過 Payment.Core.getActivePaymentValue()
            // 呼叫 ECPay provider.getPaymentValue()，因此會取得綠界內部實際付款方式。
            cart.Forms.AllDataGet(false);

            // 再校正一次 Step4 顯示文字與 S.order_header_data.payment。
            GetECPayType();

            var currentSnapshot = BuildECPayOrderSnapshot();

            if (!S.ECPayReady || !S.ECPayOrderSnapshot || currentSnapshot !== S.ECPayOrderSnapshot) {
                S.ECPayMonitor = true;
                ECPaymentChange();

                Coker.sweet.warning(
                    "付款資料已更新",
                    "訂單金額、運費或付款資料已有變更，已重新更新綠界付款模組，請重新確認付款資料後再送出訂單。",
                    null
                );

                callback(false, { handled: true });
                return;
            }

            ValidateECPayPayment(callback);
        },

        afterOrderCreated: afterOrderCreated,
        clearSelection: ClearECPaySelection,
    });
})(window.ShoppingCart, window.jQuery);
