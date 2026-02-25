(function (w) {
    "use strict";
    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Tag: {
            GetArticleDataAll: function (id) {
                return Coker.api.get("/api/Tags/GetArticleDataAll/", { AId: id });
            }
        }
    });

})(window);