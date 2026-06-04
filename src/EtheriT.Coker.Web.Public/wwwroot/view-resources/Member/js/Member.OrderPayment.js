(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-order-payment", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        function renderBarcodes(barcode1, barcode2, barcode3) {
            MemberPage.Utils.loadBarcodeScript(function () {
                if (typeof w.JsBarcode !== "function") return;

                w.JsBarcode("#barcode1", barcode1, { format: "CODE39", displayValue: true });
                w.JsBarcode("#barcode2", barcode2, { format: "CODE39", displayValue: true });
                w.JsBarcode("#barcode3", barcode3, { format: "CODE39", displayValue: true });
            });
        }

        MemberPage.OrderPayment = {
            repay: function (datas) {
                var data = {
                    ohid: datas.orderHeader.id
                };

                C.Payment.Repay(data).done(function (checkResult) {
                    if (!checkResult.success) {
                        C.sweet.error("重新付款發生錯誤", checkResult.message);
                        return;
                    }

                    MemberPage.OrderPayment.requestPayment(datas);
                });
            },

            requestPayment: function (datas) {
                var requestPayment = function (support) {
                    C.ThirdParty.Request(datas.orderHeader.id, datas.orderHeader.thirdParties, support).done(function (payResult) {
                        if (!payResult.success) {
                            C.sweet.error("重新付款發生錯誤", payResult.message, null, false);
                            return;
                        }

                        if (datas.orderHeader.thirdParties != 4) {
                            localStorage.setItem("lastSaveTime", new Date().toISOString());
                            w.location.replace(payResult.message);
                            return;
                        }

                        MemberPage.OrderPayment.openECPay(datas, payResult);
                    });
                };

                if (w.ApplePaySession && typeof w.ApplePaySession.canMakePayments === "function") {
                    w.ApplePaySession.canMakePayments().then(function (canPay) {
                        requestPayment(!!canPay);
                    }).catch(function () {
                        requestPayment(false);
                    });
                } else {
                    requestPayment(false);
                }
            },

            openECPay: function (datas, payResult) {
                var state = MemberPage.State;
                var $modal = $(MemberPage.Selectors.ecPayModal);

                state.ecPayModal = $modal.length > 0 ? new bootstrap.Modal($modal) : null;

                if (state.ecPayModal == null) {
                    C.sweet.error("重新付款發生錯誤", "找不到綠界付款視窗");
                    return;
                }

                console.log("ServerType", $modal.data("server-type"));

                w.ECPay.initialize($modal.data("server-type"), 1, function (errMsg) {
                    if (errMsg == null) {
                        w.ECPay.createPayment(payResult.message.split(",")[1], w.ECPay.Language.zhTW, function (createErrMsg) {
                            if (createErrMsg != null) {
                                console.log("Create Payment errMsg : " + createErrMsg);
                                C.sweet.error("串接綠界發生錯誤");
                            }
                        }, "V2");
                    } else {
                        console.log("Initialize errMsg : " + errMsg);
                        C.sweet.error("串接綠界發生錯誤");
                    }
                });

                state.ecPayModal.show();
                Swal.close();

                $("#ECPayModal .btn_pay").off("click.memberOrderPayment").on("click.memberOrderPayment", function () {
                    MemberPage.OrderPayment.submitECPay();
                });

                $("#ECPayModal .btn_cancel").off("click.memberOrderPayment").on("click.memberOrderPayment", function () {
                    C.sweet.custom("warning", "取消付款", "是否確認取消本筆訂單之付款？", "是", function () {
                        C.Order.CancelOrder(datas.orderHeader.id, 4).done(function (result) {
                            if (result.success) {
                                state.ecPayModal.hide();
                                C.sweet.confirm("訂單已取消", "", "確定", "", function () {
                                    location.reload();
                                });
                            }
                        });
                    }, "否", null);
                });
            },

            submitECPay: function () {
                var state = MemberPage.State;

                if (typeof w.Pay === "undefined") {
                    C.sweet.warning("付款模組尚未載入完成，請稍候再試。", "", null);
                    return;
                }

                w.ECPay.getPayToken(function (paymentInfo, errMsg) {
                    if (errMsg != null) {
                        C.sweet.warning("請確實填寫資料", errMsg, null);
                        return;
                    }

                    state.ecPayModal.hide();
                    C.sweet.loading();

                    C.ThirdParty.ECPayCreatePayment(paymentInfo).done(function (createResult) {
                        if (!createResult.success) {
                            C.sweet.confirm("付款發生未知錯誤。");
                            return;
                        }

                        MemberPage.OrderPayment.handleECPayCreateResult(createResult);
                    });
                });
            },

            handleECPayCreateResult: function (createResult) {
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
            },

            showECPayPaymentInfo: function (orderHeader, ohid) {
                C.ThirdParty.GetECPayPaymentInfo(ohid).done(function (result) {
                    if (result.success) {
                        if (orderHeader.paymentCode == 22) {
                            var message = result.message.split(",");
                            C.sweet.confirm("訂單付款資訊", message[0], "確定", "", null);
                            renderBarcodes(message[1], message[2], message[3]);
                        } else {
                            C.sweet.confirm("訂單付款資訊", result.message, "確定", "", null);
                        }
                    } else {
                        C.sweet.warning("取得付款資訊失敗", result.message, null);
                    }
                });
            }
        };
    }
})(window);
