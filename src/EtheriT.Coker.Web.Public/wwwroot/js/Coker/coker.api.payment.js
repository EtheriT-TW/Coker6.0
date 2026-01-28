(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Payment: {

            /** 取得付款資訊 */
            GetPaymentInfo: function (paytypeid) {
                return Coker.api.get("/api/ShoppingCart/GetPaymentInfo/", { paytypeid: paytypeid });
            }

        }
    });

})(window);