(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Bonus: {

            /** 取得前台會員紅利異動紀錄 */
            GetFrontUserBonusHistory: function (page) {
                return Coker.api.get("/api/Bonus/GetFrontUserBonusHistory", {
                    page: page
                });
            }

        }
    });

})(window);