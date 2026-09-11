// ECPay implementation for the ShoppingCart payment host.
(function (w, $) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-embedded-ecpay-shopping-cart", function (C) {
        C.Payment.Embedded.registerAdapter("ECPay", "ShoppingCart", function (context) {
    var cart = context.cart;
    var S = cart.State;
    var stateDefaults = {
        HasECPay: false,
        ECPayInit: false,
        ECPayMonitor: false,
        ECPayReady: false,
        ECPayOrderSnapshot: "",
        ECPayRefreshTimer: null,
        ECPayChanging: false,
        ECPayAvailable: false,
        ECPayOperational: true,
        SupportApplePay: false
    };

    Object.keys(stateDefaults).forEach(function (key) {
        if (typeof S[key] === "undefined") S[key] = stateDefaults[key];
    });

    cart.Payment = cart.Payment || {};
    cart.Payment.ECPay = cart.Payment.ECPay || {};
    var ecpaySelectionObserver = null;
    var isClearingECPaySelection = false;
    var ecpayRequestVersion = 0;
    var ecpayProvider = Coker.Payment.Core.create("ECPay", {
        rootSelector: "#ECPayPayment"
    });

    function GetECPayEntryRadio() {
        return $('#RadioPayment input[name="RadioPayment"][data-provider-code="ECPay"]').first();
    }

    function GetECPayEntryValue() {
        var $radio = GetECPayEntryRadio();
        return $radio.length ? $radio.val() : null;
    }

    function IsPaymentRadioECPay($radio) {
        return $radio &&
            $radio.length > 0 &&
            String($radio.attr("data-provider-code") || "") === "ECPay";
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
    function GetSelectedOrderDetails() {
        return cart.Items.getSelectedCartItems()
            .filter(function (item) {
                return Number(item.Id || 0) > 0 && Number(item.Quantity || 0) > 0;
            });
    }
    function HandleECPayLoadFailure(stage, detail, preferredPaymentValue) {
        console.error("[ECPay] " + stage, detail || "串接綠界發生錯誤，請稍後嘗試");

        S.ECPayOperational = false;
        S.HasECPay = false;
        S.ECPayChanging = false;
        S.ECPayReady = false;
        S.ECPayOrderSnapshot = "";

        $(".payment_provider_loading").addClass("d-none").text("");
        $("#ECPayPayment").empty();
        GetECPayEntryRadio().prop("checked", false).closest(".form-check").addClass("d-none");

        if (S.PaymentAvailabilityLoaded &&
            cart.Payment.Availability &&
            typeof cart.Payment.Availability.apply === "function") {
            cart.Payment.Availability.apply(preferredPaymentValue);
        }
    }
    function ECPaymentChange() {
        if (!S.ECPayMonitor) {
            return;
        }

        cart.Pricing.TotalCount();

        // 取貨門市在付款方式之後選擇，載入付款項目時不應被門市必填檢核擋住。
        var dataReady = cart.Forms.AllDataGet(false, true);
        cart.Payment.Core.Step3Monitor();
        var selectedOrderDetails = GetSelectedOrderDetails();

        if (!dataReady || selectedOrderDetails.length === 0) {
            ecpayRequestVersion += 1;
            S.ECPayChanging = false;

            S.ECPayReady = false;
            S.ECPayOrderSnapshot = "";

            $("#RadioPayment > .form-check").addClass("d-none");
            $(".noPaymentWarning").addClass("d-none");
            $(".payment_provider_loading").addClass("d-none");
            $("#ECPayPayment").empty();
            cart.CheckoutValidation.RefreshDisplay();

            return;
        }

        // 缺漏提示必須先於綠界的可用性與初始化狀態判斷。
        // 否則資料不完整時，付款項目被隱藏了，提示也會一起沒有機會顯示。
        $(".checkoutValidationWarning").addClass("d-none");

        if (!S.HasECPay) {
            return;
        }

        if (!S.ECPayInit) {
            $(".payment_provider_loading").removeClass("d-none").text("付款模組載入中...");
            return;
        }

        var selectedPaymentBeforeSync = cart.Payment.Core.GetCheckedPaymentValue();
        var restorePaymentAfterSync = IsECPaySelected()
            ? GetECPayEntryValue()
            : selectedPaymentBeforeSync;

        S.order_header_data.OrderDetails = selectedOrderDetails;

        var nextSnapshot = BuildECPayOrderSnapshot();

        if (S.ECPayChanging) {
            return;
        }

        if (S.ECPayReady && S.ECPayOrderSnapshot === nextSnapshot && typeof window.Pay !== "undefined" && $("#ECPayPayment").children().length > 0) {
            return;
        }

        S.ECPayChanging = true;
        S.ECPayReady = false;
        var requestVersion = ++ecpayRequestVersion;

        $(".payment_provider_loading").removeClass("d-none").text("付款模組載入中...");
        $(".checkoutValidationWarning").addClass("d-none");
        $("#ECPayPayment").empty();

        var timeout = 0;
        var checkInterval = setInterval(function () {
            if (requestVersion !== ecpayRequestVersion) {
                clearInterval(checkInterval);
                return;
            }

            if (S.ECPayInit !== true) {
                timeout += 100;
                if (timeout >= 10000) {
                    clearInterval(checkInterval);
                    HandleECPayLoadFailure(
                        "初始化逾時",
                        "ECPay.initialize did not complete within 10 seconds.",
                        restorePaymentAfterSync
                    );
                }
                return;
            }

            clearInterval(checkInterval);
            S.order_header_data.SupportApplePay = CanUseApplePay();
            Coker.ThirdParty.ECPayGetToken(S.order_header_data)
                .done(function (result) {
                    if (requestVersion !== ecpayRequestVersion) return;

                    if (!result || !result.success) {
                        HandleECPayLoadFailure("取得 Token 失敗", result, restorePaymentAfterSync);
                        return;
                    }

                    var message = String(result.message || "").split(",");
                    if (message.length < 2 || !message[0] || !message[1]) {
                        HandleECPayLoadFailure("Token 回傳格式錯誤", result, restorePaymentAfterSync);
                        return;
                    }

                    S.order_header_data.orderId = message[0];
                    ecpayProvider.createPayment(message[1], ECPay.Language.zhTW, function (errMsg) {
                        if (requestVersion !== ecpayRequestVersion) return;

                        if (errMsg != null) {
                            HandleECPayLoadFailure("建立付款模組失敗", errMsg, restorePaymentAfterSync);
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
                        cart.Shipping.UpdateCvsStoreSelectionDisplay();

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
                            S.CvsStoreValidationRequested = false;
                            cart.Shipping.UpdateCvsStoreSelectionDisplay();

                            if ($(".payment_provider_loading").hasClass("d-none")) {
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

                                $(".payment_provider_loading").addClass("d-none");

                                if (S.buy_step_swiper) {
                                    S.buy_step_swiper.update();
                                }
                            }
                        }, 100);
                    }, "V2");
                })
                .fail(function (xhr, textStatus, errorThrown) {
                    if (requestVersion !== ecpayRequestVersion) return;

                    HandleECPayLoadFailure(
                        "取得 Token 請求失敗",
                        {
                            status: xhr && xhr.status,
                            textStatus: textStatus,
                            error: errorThrown,
                            response: xhr && xhr.responseJSON
                        },
                        restorePaymentAfterSync
                    );
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
        return ecpayProvider.getActivePaymentType();
    }
    function IsActiveApplePay() {
        return ecpayProvider.isApplePaySelected();
    }
    function CanUseApplePay() {
        return ecpayProvider.canUseApplePay();
    }

    function ConfirmApplePayThenValidate(callback) {
        // OrderHeaderAdd 進入付款驗證前已開啟 loading。
        // 若不先解除，SweetAlert 會把確認鈕留在轉圈圈狀態。
        if (typeof Swal.hideLoading === "function") {
            Swal.hideLoading();
        }

        Swal.fire({
            icon: "info",
            title: "準備開啟 Apple Pay",
            html: "請點選下方按鈕開啟 Apple Wallet 完成付款。",
            showConfirmButton: true,
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "開啟 Apple Pay",
            cancelButtonText: "取消",
            allowOutsideClick: false,
            didOpen: function () {
                if (typeof Swal.hideLoading === "function") {
                    Swal.hideLoading();
                }

                var confirmButton = Swal.getConfirmButton();
                if (!confirmButton) return;

                confirmButton.addEventListener("click", function (e) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    Swal.close();
                    ValidateECPayPayment(callback);
                }, { once: true, capture: true });
            }
        }).then(function (result) {
            if (result.dismiss === Swal.DismissReason.cancel) {
                callback(false, { handled: true });
            }
        });
    }

    function CompleteApplePayOrder(resultData) {
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
        Swal.close();
        Coker.sweet.warning("Apple Pay 付款失敗", message || "付款未完成，請重新操作。", null);
    }

    function ValidateECPayPayment(callback) {
        if (!S.ECPayReady || S.ECPayChanging || typeof window.Pay === "undefined" || $("#ECPayPayment").children().length === 0) {
            callback(false, "綠界付款模組尚未載入完成，請稍候再試。");
            return;
        }

        if (ecpayProvider.isApplePaySelected()) {
            // 交由綠界 SDK 根據裝置顯示桌機 QR Code 或手機 Apple Pay 確認。
            // 先關閉庫存檢查開啟的 loading，避免 SweetAlert 覆蓋綠界付款畫面。
            Coker.sweet.close();
        }

        Coker.Payment.Flow.prepareCheckout(ecpayProvider, {
            onApplePaySuccess: function (resultData) {
                CompleteApplePayOrder(resultData);
            },
            onError: function (message, rawData, isApplePay) {
                if (isApplePay) {
                    FailApplePayOrder(message, rawData);
                    callback(false, { handled: true, message: message });
                    return;
                }

                co.sweet.warning("請確實填寫付款資料", message, null);
                callback(false, message);
            },
            onTimeout: function () {
                Swal.close();
                callback(false, {
                    handled: true,
                    message: "Apple Pay 付款流程逾時，系統未收到綠界付款結果。若裝置已顯示付款成功，請先勿重複付款，請聯絡客服確認交易狀態。"
                });

                Coker.sweet.warning(
                    "Apple Pay 付款流程逾時",
                    "系統未收到綠界 Apple Pay 付款結果。若裝置已顯示付款成功，請先勿重複付款，請聯絡客服確認交易狀態。",
                    null
                );
            }
        }, callback);
    }

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
        CanUseApplePay: CanUseApplePay,
        ValidateECPayPayment: ValidateECPayPayment,
        afterOrderCreated: afterOrderCreated
    });

    cart.Payment.Core.register({
        code: "ECPay",
        type: "embedded",
        paymentProvider: ecpayProvider,
        isAvailable: function () {
            return S.ECPayAvailable === true;
        },
        getAvailabilityItems: function (items) {
            return (items || []).filter(function (item) {
                return String(item.providerCode || "") === "ECPay" &&
                    String(item.renderMode || "").toLowerCase() === "embedded";
            });
        },
        applyAvailability: function (items) {
            var providerItems = this.getAvailabilityItems(items);
            var availableItems = providerItems.filter(function (item) {
                return item.isAvailable === true;
            });
            var $entry = GetECPayEntryRadio();

            S.ECPayAvailable = availableItems.length > 0 && S.ECPayOperational !== false;
            S.HasECPay = S.ECPayAvailable && $entry.length > 0;

            if ($entry.length && S.ECPayAvailable) {
                var first = availableItems[0];
                $entry
                    .val(first.id)
                    .attr("data-availability-id", first.id)
                    .attr("data-code", first.code)
                    .attr("data-cvs-store-selection-mode", first.cvsStoreSelectionMode || 0)
                    .attr("data-minamount", first.minAmount)
                    .attr("data-maxamount", first.maxAmount == null ? "" : first.maxAmount);
            } else if ($entry.length) {
                $entry.prop("checked", false);
                this.clear();
                $(".payment_provider_loading").addClass("d-none");
            }

            return {
                provider: this,
                items: providerItems,
                availableItems: availableItems,
                available: S.ECPayAvailable,
                entry: $entry
            };
        },
        init: function () {
            if ($("#ECPayPayment").length === 0) {
                return;
            }

            S.HasECPay = false;
            S.ECPayOperational = true;
            S.ECPayMonitor = true;
            S.SupportApplePay = CanUseApplePay();
            ecpayProvider.initialize($("#ECPayPayment").data("server-type"), 1, function (errMsg) {
                if (errMsg != null) {
                    HandleECPayLoadFailure("SDK 初始化失敗", errMsg, null);
                    return;
                }

                S.ECPayInit = true;

                var $ecpayRadio = GetECPayEntryRadio();

                if ($ecpayRadio.length && S.ECPayAvailable) {
                    $ecpayRadio.prop("checked", true);
                    $ecpayRadio.closest(".form-check").prevAll(".form-check").first().find(".payment_display").addClass("last");
                }
                setTimeout(function () {
                    ECPaymentChange();
                }, 0);

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

            // 手機的 Apple Pay 需要保留一次明確的使用者點擊來開啟 Wallet。
            // 桌機則直接交由綠界 SDK 顯示 QR Code。
            if (IsActiveApplePay() && ecpayProvider.isPhoneDevice()) {
                ConfirmApplePayThenValidate(callback);
                return;
            }

            ValidateECPayPayment(callback);
        },

        afterOrderCreated: afterOrderCreated,
        clearSelection: ClearECPaySelection,
    });

            return cart.Payment.Core.getProvider("ECPay");
        });
    });
})(window, window.jQuery);
