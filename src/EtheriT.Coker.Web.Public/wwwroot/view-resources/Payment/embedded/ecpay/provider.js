// Shared ECPay SDK adapter.
(function (w, $) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-ecpay", function (C) {
        if (!C.Payment || !C.Payment.Core) {
            throw new Error("Coker.Payment.Core must be loaded before the ECPay provider.");
        }

        var activeApplePayProvider = null;
        var sdkLoadState = w.ECPay ? "loaded" : "idle";
        var sdkLoadCallbacks = [];

        function flushSdkLoadCallbacks(errorMessage) {
            var callbacks = sdkLoadCallbacks.slice();
            sdkLoadCallbacks = [];

            callbacks.forEach(function (callback) {
                callback(errorMessage || null);
            });
        }

        function ensureSdk(serverType, callback) {
            if (w.ECPay) {
                sdkLoadState = "loaded";
                callback(null);
                return;
            }

            sdkLoadCallbacks.push(callback);
            if (sdkLoadState === "loading") return;

            sdkLoadState = "loading";

            var isProduction = String(serverType || "").toLowerCase() === "prod";
            var script = w.document.createElement("script");
            script.src = (isProduction
                ? "https://ecpg.ecpay.com.tw"
                : "https://ecpg-stage.ecpay.com.tw") + "/Scripts/sdk-1.0.0.js?t=20210121100116";
            script.async = true;
            script.setAttribute("data-payment-provider", "ECPay");
            script.onload = function () {
                if (!w.ECPay) {
                    sdkLoadState = "failed";
                    flushSdkLoadCallbacks("綠界付款 SDK 載入完成，但找不到 ECPay API。");
                    return;
                }

                sdkLoadState = "loaded";
                flushSdkLoadCallbacks(null);
            };
            script.onerror = function () {
                sdkLoadState = "failed";
                flushSdkLoadCallbacks("綠界付款 SDK 載入失敗，請稍後重新嘗試。");
            };

            w.document.head.appendChild(script);
        }

        function isApplePayResultSuccess(resultData) {
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

            if (rtnCode === "1" && (tradeStatus === "1" || tradeStatus === "")) return true;
            return rtnMsg.toLowerCase().indexOf("success") >= 0;
        }

        function getApplePayErrorMessage(resultData, errMsg) {
            if (errMsg) return errMsg;
            if (resultData && (resultData.RtnMsg || resultData.rtnMsg)) {
                return resultData.RtnMsg || resultData.rtnMsg;
            }

            return "Apple Pay 付款未完成，請重新操作。";
        }

        function create(options) {
            var settings = $.extend({
                // ECPay SDK 固定以 #ECPayPayment 作為付款畫面掛載點。
                rootSelector: "#ECPayPayment",
                timeoutMs: 60000
            }, options || {});
            var timer = null;
            var waiting = false;
            var completed = false;
            var callbacks = null;

            function clearTimer() {
                if (timer == null) return;
                clearTimeout(timer);
                timer = null;
            }

            function resetApplePayState() {
                waiting = false;
                completed = true;
                callbacks = null;
                clearTimer();

                if (activeApplePayProvider === provider) {
                    activeApplePayProvider = null;
                }
            }

            function completeApplePay(resultData) {
                if (!waiting || completed) return;

                var activeCallbacks = callbacks;
                resetApplePayState();

                if (activeCallbacks && typeof activeCallbacks.onApplePaySuccess === "function") {
                    activeCallbacks.onApplePaySuccess(resultData);
                }
            }

            function failApplePay(message, rawData) {
                if (!waiting || completed) return;

                var activeCallbacks = callbacks;
                resetApplePayState();

                if (activeCallbacks && typeof activeCallbacks.onError === "function") {
                    activeCallbacks.onError(message, rawData, true);
                }
            }

            var provider = {
                code: "ECPay",
                mode: "embedded",

                isPhoneDevice: function () {
                    var device = C.util && C.util.device;

                    try {
                        return device && typeof device.isPhone === "function"
                            ? device.isPhone()
                            : /iPhone|iPod|Android.*Mobile|Windows Phone/i.test(w.navigator.userAgent || "");
                    } catch (ex) {
                        return /iPhone|iPod|Android.*Mobile|Windows Phone/i.test(w.navigator.userAgent || "");
                    }
                },

                showTwqrPayment: function (info, onClose) {
                    info = info || {};

                    var isPhone = provider.isPhoneDevice();
                    var qrText = String(info.QRcode || "").trim();
                    var redirectUrl = "";

                    try {
                        var url = new URL(String(info.RedirectURL || ""));

                        if (url.protocol === "https:") {
                            redirectUrl = url.href;
                        }
                    } catch (ex) {
                        // 由下方檢查並顯示錯誤。
                    }

                    if (isPhone && !redirectUrl) {
                        return w.Swal.fire({
                            icon: "error",
                            title: "無法取得 TWQR 付款連結",
                            text: "請保留訂單編號，聯絡客服確認，請勿重複下單。"
                        });
                    }

                    if (!isPhone &&
                        (!qrText || typeof w.QRCode !== "function")) {
                        return w.Swal.fire({
                            icon: "error",
                            title: "無法顯示 TWQR QR Code",
                            text: "付款資料或 QR Code 元件未完整載入，請勿重複下單。"
                        });
                    }

                    var content = w.document.createElement("div");
                    var description = w.document.createElement("p");

                    description.textContent = isPhone
                        ? "請點選下方按鈕，前往 TWQR 付款頁面。"
                        : "請使用支援 TWQR 的手機 App 掃描付款。";

                    content.appendChild(description);

                    var qrHost = null;

                    if (!isPhone) {
                        qrHost = w.document.createElement("div");
                        qrHost.className =
                            "d-flex justify-content-center bg-white p-3";

                        content.appendChild(qrHost);
                    }

                    if (redirectUrl) {
                        var link = w.document.createElement("a");

                        link.href = redirectUrl;
                        link.target = "_blank";
                        link.rel = "noopener noreferrer";
                        link.className = "btn btn-primary my-3";
                        link.textContent = "開啟 TWQR 付款頁面";

                        content.appendChild(link);
                    }

                    var notice = w.document.createElement("p");

                    notice.className = "small text-muted";
                    notice.textContent =
                        "付款資料有效期限為 10 分鐘；完成操作後，請確認訂單付款狀態。";

                    content.appendChild(notice);

                    return w.Swal.fire({
                        title: "TWQR 付款",
                        html: content,
                        confirmButtonText: "關閉付款資訊",
                        allowOutsideClick: false,
                        didOpen: function () {
                            if (!qrHost) return;

                            try {
                                new w.QRCode(qrHost, {
                                    text: qrText,
                                    width: 256,
                                    height: 256,
                                    correctLevel: w.QRCode.CorrectLevel.M
                                });
                            } catch (ex) {
                                qrHost.textContent =
                                    "QR Code 產生失敗，請聯絡客服確認。";
                            }
                        }
                    }).then(function () {
                        if (typeof onClose === "function") {
                            onClose();
                        }
                    });
                },

                canUseApplePay: function () {
                    // 桌機由綠界顯示 QR Code，不需要本機具備 ApplePaySession。
                    if (!provider.isPhoneDevice()) return true;

                    if (!w.ApplePaySession || typeof w.ApplePaySession.canMakePayments !== "function") {
                        return false;
                    }

                    try {
                        return w.ApplePaySession.canMakePayments() === true;
                    } catch (ex) {
                        return false;
                    }
                },

                getActivePaymentType: function () {
                    return $(settings.rootSelector + " .ecpay-pay-list-wrap .ecpay-pay-list > li.ecpay-pl-act").attr("id") || "";
                },

                isApplePaySelected: function () {
                    return provider.getActivePaymentType() === "ApplePay";
                },

                initialize: function (serverType, isLoading, callback) {
                    ensureSdk(serverType, function (loadError) {
                        if (loadError) {
                            callback(loadError);
                            return;
                        }

                        w.ECPay.initialize(serverType, isLoading, callback);
                    });
                },

                createPayment: function (token, language, callback, version) {
                    if (version == null) {
                        return w.ECPay.createPayment(token, language, callback);
                    }

                    return w.ECPay.createPayment(token, language, callback, version);
                },

                submit: function (submitOptions) {
                    var submitCallbacks = submitOptions || {};
                    var isApplePay = provider.isApplePaySelected();

                    if (isApplePay) {
                        if (activeApplePayProvider && activeApplePayProvider !== provider) {
                            activeApplePayProvider.cancelApplePay();
                        }

                        clearTimer();
                        waiting = true;
                        completed = false;
                        callbacks = submitCallbacks;
                        activeApplePayProvider = provider;

                        timer = setTimeout(function () {
                            if (!waiting || completed) return;

                            var activeCallbacks = callbacks;
                            resetApplePayState();

                            if (activeCallbacks && typeof activeCallbacks.onTimeout === "function") {
                                activeCallbacks.onTimeout();
                            }
                        }, Number(submitCallbacks.timeoutMs || settings.timeoutMs));
                    } else {
                        resetApplePayState();
                    }

                    try {
                        w.ECPay.getPayToken(function (paymentInfo, errMsg) {
                            if (errMsg != null) {
                                if (isApplePay) {
                                    failApplePay(errMsg, paymentInfo);
                                } else if (typeof submitCallbacks.onError === "function") {
                                    submitCallbacks.onError(errMsg, paymentInfo, false);
                                }
                                return;
                            }

                            // Apple Pay 不回傳 PayToken，後續由 getApplePayResultData 進入。
                            if (isApplePay) return;

                            if (typeof submitCallbacks.onPayToken === "function") {
                                submitCallbacks.onPayToken(paymentInfo);
                            }
                        });
                    } catch (ex) {
                        if (isApplePay) {
                            failApplePay(ex.message || String(ex), ex);
                        } else if (typeof submitCallbacks.onError === "function") {
                            submitCallbacks.onError(ex.message || String(ex), ex, false);
                        }
                    }
                },

                prepareCheckout: function (context, callback) {
                    provider.submit({
                        timeoutMs: context.timeoutMs,
                        onPayToken: function (paymentInfo) {
                            callback(true, paymentInfo);
                        },
                        onApplePaySuccess: function (resultData) {
                            if (typeof context.onApplePaySuccess === "function") {
                                context.onApplePaySuccess(resultData, provider);
                            }
                        },
                        onError: function (message, rawData, isApplePay) {
                            if (typeof context.onError === "function") {
                                context.onError(message, rawData, isApplePay, provider);
                            }
                        },
                        onTimeout: function () {
                            if (typeof context.onTimeout === "function") context.onTimeout(provider);
                        }
                    });
                },

                afterOrderCreated: function (context) {
                    if (typeof context.onEmbeddedComplete === "function") {
                        return context.onEmbeddedComplete(provider);
                    }
                },

                repay: function (context) {
                    return C.ThirdParty.Request(
                        context.orderId,
                        provider.code,
                        provider.canUseApplePay()
                    ).done(function (result) {
                        if (!result || !result.success) {
                            if (typeof context.onError === "function") {
                                context.onError(result && result.message, result, provider);
                            }
                            return;
                        }

                        if (typeof context.onEmbeddedReady === "function") {
                            context.onEmbeddedReady(result, provider);
                        }
                    }).fail(function (result) {
                        if (typeof context.onError === "function") {
                            context.onError("付款服務連線失敗，請稍後重新嘗試。", result, provider);
                        }
                    });
                },

                handleApplePayResult: function (resultData, errMsg) {
                    if (!waiting || completed) return;

                    if (errMsg != null) {
                        failApplePay(errMsg, resultData);
                        return;
                    }

                    if (!isApplePayResultSuccess(resultData)) {
                        failApplePay(getApplePayErrorMessage(resultData, errMsg), resultData);
                        return;
                    }

                    completeApplePay(resultData);
                },

                cancelApplePay: resetApplePayState,
                dispose: resetApplePayState
            };

            return provider;
        }

        var definition = C.Payment.Catalog.get("ECPay");

        C.Payment.Core.register({
            code: "ECPay",
            mode: "embedded",
            aliases: definition ? definition.aliases : [],
            create: create
        });

        // 綠界 SDK 固定尋找全域 callback，由共用 Provider 轉送給目前的付款流程。
        w.getApplePayResultData = function (resultData, errMsg) {
            if (activeApplePayProvider) {
                activeApplePayProvider.handleApplePayResult(resultData, errMsg);
            }
        };
        w.GetApplePayResultData = w.getApplePayResultData;
    });
})(window, window.jQuery);
