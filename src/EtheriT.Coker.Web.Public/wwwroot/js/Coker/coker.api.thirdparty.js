(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        ThirdParty: {

            /** 發起第三方付款請求 */
            Request: function (ohid, paytype, support) {
                return Coker.api.get("/api/ThirdParty/PayRequest", {
                    ohid: ohid,
                    paytype: paytype,
                    support: support
                });
            },

            /** 取得綠界 Token */
            ECPayGetToken: function (data) {
                return Coker.api.post("/api/ThirdParty/ECPayGetToken");
            },

            /** 建立綠界付款 */
            ECPayCreatePayment: function (data) {
                return Coker.api.post("/api/ThirdParty/ECPayCreatePayment", data);
            },
            /** 取得綠界地圖 */
            LogisticsGetMap: function (scid, LogisticsSubType) {
                return Coker.api.get("/api/ThirdParty/ECPayLogisticsGetMap", { scid: scid, LogisticsSubType: LogisticsSubType });
            }

        }
    });

})(window);