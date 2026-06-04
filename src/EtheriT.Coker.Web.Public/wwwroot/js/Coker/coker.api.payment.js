(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Payment: {

            /** 取得付款資訊 */
            GetPaymentInfo: function (paytypeid) {
                return Coker.api.get("/api/ShoppingCart/GetPaymentInfo/", { paytypeid: paytypeid });
            },

            /** 檢查訂單是否可付款 / 付款狀態 */
            CheckOrder: function (ohid) {
                return Coker.api.get("/api/Order/CheckOrder/", {
                    ohid: ohid
                });
            },

            /** 重新付款 */
            Repay: function (data) {
                return Coker.api.post("/api/Order/OrderRepay", data);
            }

        }
    });

})(window);