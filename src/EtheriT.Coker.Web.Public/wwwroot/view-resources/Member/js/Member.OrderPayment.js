(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-order-payment", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});
        var activeAdapter = null;

        function createMemberAdapter(provider) {
            return C.Payment.Embedded.createAdapter(provider.code, "Member", {
                memberPage: MemberPage,
                provider: provider
            });
        }

        MemberPage.OrderPayment = {
            repay: function (datas) {
                MemberPage.OrderPayment.requestPayment(datas);
            },

            requestPayment: function (datas) {
                C.Payment.Flow.repay(datas.orderHeader.thirdParties, {
                    orderId: datas.orderHeader.id,
                    providerOptions: {
                        rootSelector: "#PaymentContainer"
                    },
                    onRedirect: function (url) {
                        localStorage.setItem("lastSaveTime", new Date().toISOString());
                        w.location.replace(url);
                    },
                    onEmbeddedReady: function (payResult, provider) {
                        activeAdapter = createMemberAdapter(provider);

                        if (!activeAdapter || typeof activeAdapter.open !== "function") {
                            C.sweet.error("重新付款發生錯誤", "此嵌入式付款方式尚未提供會員頁介面。", null, false);
                            return;
                        }

                        activeAdapter.open(datas, payResult);
                    },
                    onError: function (message) {
                        C.sweet.error("重新付款發生錯誤", message || "付款流程發生未知錯誤。", null, false);
                    }
                });
            },

            submitPayment: function () {
                if (!activeAdapter || typeof activeAdapter.submit !== "function") {
                    C.sweet.warning("付款模組尚未準備完成，請重新開啟付款流程。", "", null);
                    return;
                }

                activeAdapter.submit();
            },

            showPaymentInfo: function (orderHeader, ohid) {
                var provider;

                try {
                    provider = C.Payment.Core.create(orderHeader.thirdParties, {
                        rootSelector: "#PaymentContainer"
                    });
                } catch (ex) {
                    C.sweet.warning("取得付款資訊失敗", "此付款方式尚未註冊。", null);
                    return;
                }

                var adapter = createMemberAdapter(provider);

                if (!adapter || typeof adapter.showPaymentInfo !== "function") {
                    C.sweet.warning("取得付款資訊失敗", "此付款方式沒有可顯示的付款資訊。", null);
                    return;
                }

                adapter.showPaymentInfo(orderHeader, ohid);
            }
        };
    }
})(window);
