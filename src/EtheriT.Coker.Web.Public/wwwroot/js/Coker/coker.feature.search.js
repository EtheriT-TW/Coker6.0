(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        feature: {
            search: {
                Init: function (id) {
                    var $e = $(id);
                    var $b = $e.find(".dropdown-menu button");
                    var $t = $e.find(".input_sear");
                    var $t2 = $("#SearchInput");

                    $e.data("sid", $b.first().data("id") ?? 0);

                    $b.on("click", function () {
                        $e.data("sid", $(this).data("id") ?? 0);
                        if (Coker.Directory.normalizeSearchKeyword($t.val()) !== "") w.location.href = "/" + OrgName + "/Search/Get/" + $e.data("sid") + "/" + Coker.Directory.encodeSearchKeyword(Coker.Directory.normalizeSearchKeyword($t.val()));
                    });

                    $e.find(".btn_sear").on("click", function () {
                        if (Coker.Directory.normalizeSearchKeyword($t.val()) === "") {
                            // Legacy sweet 相容（新位置也可用）
                            (Coker.sweet || Coker.ui.sweet).warning(local.Notice, local.PleaseEnterSearchText, function () {
                                setTimeout(function () { $t2.trigger("focus"); }, 300);
                            }, false);
                        } else {
                            w.location.href = "/" + OrgName + "/Search/Get/" + $e.data("sid") + "/" + Coker.Directory.encodeSearchKeyword(Coker.Directory.normalizeSearchKeyword($t.val()));
                        }
                        return false;
                    });

                    if ($t2.length != 0) {
                        $t2.on("keypress", function (e) {
                            if (e.which == 13 && Coker.Directory.normalizeSearchKeyword($t2.val())) {
                                w.location.href = "/" + OrgName + "/Search/Get/0/" + Coker.Directory.encodeSearchKeyword(Coker.Directory.normalizeSearchKeyword($t2.val()));
                            }
                        });
                    }
                }
            }
        }
    });

    // Legacy: Coker.Search.*
    Coker.Search = Coker.Search || {};
    Coker.Search.Init = Coker.Search.Init || Coker.feature.search.Init;

})(window);
