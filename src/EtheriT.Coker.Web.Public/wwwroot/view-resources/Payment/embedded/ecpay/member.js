(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("payment-embedded-ecpay-member", function (C) {
        C.Payment.Embedded.registerAdapter("ECPay", "Member", function (context) {
            var MemberPage = context.memberPage;
            var provider = context.provider;
            var currentOrder = null;

            function renderBarcodes(barcode1, barcode2, barcode3) {
                MemberPage.Utils.loadBarcodeScript(function () {
                    if (typeof w.JsBarcode !== "function") return;

                    w.JsBarcode("#barcode1", barcode1, { format: "CODE39", displayValue: true });
                    w.JsBarcode("#barcode2", barcode2, { format: "CODE39", displayValue: true });
                    w.JsBarcode("#barcode3", barcode3, { format: "CODE39", displayValue: true });
                });
            }

            function handleCreateResult(createResult) {
                var resultObj = JSON.parse(createResult.message);

                switch (resultObj.OrderInfo.PaymentType) {
                    case null:
                        localStorage.setItem("lastSaveTime", new Date().toISOString());
                        localStorage.setItem("lastSaveToken", localStorage.getItem("token"));

                        var verifyUrl = resultObj.ThreeDInfo?.ThreeDURL ?? resultObj.UnionPayInfo?.UnionPayURL;

                        w.open(verifyUrl, "_blank");
                        C.sweet.confirm("即將進入驗證流程", "<div class='text-start'>如未自動跳轉，請點此<a class='fw-bold text-primary px-1' href='" + verifyUrl + "' target='_blank' title='連結至：驗證頁面(開新視窗)'>連結</a>進行跳轉</div>", "確定", "", function () {
                            location.reload();
                        });
                        break;

                    case "ATM":
                        var atmInfo = resultObj.ATMInfo;
                        C.sweet.confirm("訂單付款資訊", "<div class='text-start'>繳費銀行代碼：" + atmInfo.BankCode + "<br>繳費虛擬帳號：" + atmInfo.vAccount + "<br><br>請將此付款資訊截圖保存，並於繳費期限<span class='text-danger fw-bold'>" + atmInfo.ExpireDate + "</span>前完成繳費，感謝您的訂購。</div>", "確定", "", function () {
                            location.reload();
                        });
                        break;

                    case "CVS":
                        var cvsInfo = resultObj.CVSInfo;
                        C.sweet.confirm("訂單付款資訊", "<div class='text-start'>繳費代碼：" + cvsInfo.PaymentNo + "<br>或點此<a class='fw-bold text-primary px-1' href='" + cvsInfo.PaymentURL + "' target='_blank' title='連結至：繳費條碼(開新分頁)'>連結</a>取得繳費條碼<br><br>請將此付款資訊截圖保存，並於繳費期限<span class='text-danger fw-bold'>" + cvsInfo.ExpireDate + "</span>前完成繳費，感謝您的訂購。</div>", "確定", "", function () {
                            location.reload();
                        });
                        break;

                    case "BARCODE":
                    case "Barcode":
                        var barcodeInfo = resultObj.BarcodeInfo;
                        C.sweet.confirm("訂單付款資訊", "<div class='text-start'><svg id='barcode1' class='w-100'></svg><svg id='barcode2' class='w-100'></svg><svg id='barcode3' class='w-100'></svg><br><br>請將此付款資訊截圖保存，並於繳費期限<span class='text-danger fw-bold'>" + barcodeInfo.ExpireDate + "</span>前完成繳費，感謝您的訂購。<br><br>條碼載入需要一段時間，請耐心等候</div>", "確定", "", function () {
                            location.reload();
                        });
                        renderBarcodes(barcodeInfo.Barcode1, barcodeInfo.Barcode2, barcodeInfo.Barcode3);
                        break;

                    case "ApplePay":
                        C.sweet.confirm("訂單已成立，謝謝您的訂購！", "", "確定", "", function () {
                            location.reload();
                        });
                        break;
                }
            }

            function submit() {
                var state = MemberPage.State;

                if (typeof w.Pay === "undefined") {
                    C.sweet.warning("付款模組尚未載入完成，請稍候再試。", "", null);
                    return;
                }

                C.Payment.Flow.prepareCheckout(provider, {
                    onApplePaySuccess: function () {
                        state.paymentModal.hide();
                        C.sweet.confirm("付款已完成，謝謝您的訂購！", "", "確定", "", function () {
                            location.reload();
                        });
                    },
                    onError: function (message, rawData, isApplePay) {
                        C.sweet.warning(
                            isApplePay ? "Apple Pay 付款失敗" : "請確實填寫資料",
                            message || "綠界付款流程發生例外，請重新操作。",
                            null
                        );
                    },
                    onTimeout: function () {
                        C.sweet.warning(
                            "Apple Pay 付款流程逾時",
                            "系統未收到綠界 Apple Pay 付款結果。若裝置已顯示付款成功，請先勿重複付款，並聯絡客服確認交易狀態。",
                            null
                        );
                    }
                }, function (success, paymentInfo) {
                    if (!success) return;

                    state.paymentModal.hide();
                    C.sweet.loading();

                    C.ThirdParty.ECPayCreatePayment(paymentInfo).done(function (createResult) {
                        if (!createResult.success) {
                            C.sweet.confirm("付款發生未知錯誤。");
                            return;
                        }

                        handleCreateResult(createResult);
                    });
                });
            }

            function open(datas, payResult) {
                var state = MemberPage.State;
                var $modal = $(MemberPage.Selectors.paymentModal);
                currentOrder = datas.orderHeader;
                state.paymentModal = $modal.length > 0 ? new bootstrap.Modal($modal) : null;

                if (state.paymentModal == null) {
                    C.sweet.error("重新付款發生錯誤", "找不到付款視窗");
                    return;
                }

                provider.initialize($modal.data("server-type"), 1, function (errMsg) {
                    if (errMsg != null) {
                        console.log("Initialize errMsg : " + errMsg);
                        C.sweet.error("串接付款服務發生錯誤");
                        return;
                    }

                    provider.createPayment(payResult.message.split(",")[1], w.ECPay.Language.zhTW, function (createErrMsg) {
                        if (createErrMsg != null) {
                            console.log("Create Payment errMsg : " + createErrMsg);
                            C.sweet.error("串接付款服務發生錯誤");
                        }
                    }, "V2");
                });

                state.paymentModal.show();
                Swal.close();

                $("#PaymentModal .btn_pay").off("click.memberOrderPayment").on("click.memberOrderPayment", submit);
                $("#PaymentModal .btn_cancel").off("click.memberOrderPayment").on("click.memberOrderPayment", function () {
                    provider.cancelApplePay();

                    C.sweet.custom("warning", "取消付款", "是否確認取消本筆訂單之付款？", "是", function () {
                        C.Order.CancelOrder(currentOrder.id, currentOrder.thirdParties).done(function (result) {
                            if (!result.success) return;

                            state.paymentModal.hide();
                            C.sweet.confirm("訂單已取消", "", "確定", "", function () {
                                location.reload();
                            });
                        });
                    }, "否", null);
                });
            }

            function showPaymentInfo(orderHeader, ohid) {
                C.ThirdParty.GetECPayPaymentInfo(ohid).done(function (result) {
                    if (!result.success) {
                        C.sweet.warning("取得付款資訊失敗", result.message, null);
                        return;
                    }

                    if (orderHeader.paymentCode == 22) {
                        var message = result.message.split(",");
                        C.sweet.confirm("訂單付款資訊", message[0], "確定", "", null);
                        renderBarcodes(message[1], message[2], message[3]);
                    } else {
                        C.sweet.confirm("訂單付款資訊", result.message, "確定", "", null);
                    }
                });
            }

            return {
                code: provider.code,
                open: open,
                submit: submit,
                showPaymentInfo: showPaymentInfo,
                dispose: provider.dispose
            };
        });
    });
})(window);
