(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Product: {

            /** 取得會員瀏覽紀錄商品列表 */
            GetHistoryDisplay: function (page) {
                return Coker.api.get("/api/Product/GetHistoryDisplay/", {
                    page: page
                });
            }

        }
    });

})(window);