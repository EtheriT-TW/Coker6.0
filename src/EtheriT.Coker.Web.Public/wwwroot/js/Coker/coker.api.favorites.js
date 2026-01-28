(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Favorites: {

            /** 加入收藏 */
            Add: function (Pid) {
                return Coker.api.get("/api/Favorites/Add/", { Pid: Pid });
            },

            /** 取得收藏列表（前台顯示） */
            GetDisplay: function (page) {
                return Coker.api.get("/api/Favorites/GetDisplay/", { page: page });
            },

            /** 移除收藏 */
            Delete: function (Fid) {
                return Coker.api.get("/api/Favorites/Delete/", { Fid: Fid });
            },

            /** 檢查是否已收藏 */
            Check: function (Pid) {
                return Coker.api.get("/api/Favorites/CheckIsFavorites/", { Pid: Pid });
            }

        }
    });

})(window);