(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Marketing: {

            /**
             * 取得購物車可用行銷活動。
             * 目前第一階段回傳滿額優惠規則，給購物車前端試算顯示用。
             */
            GetCartMarketingCampaigns: function () {
                return Coker.api.post("/api/Marketing/GetCartMarketingCampaigns", null);
            }

        }
    });

})(window);