(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-init", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Init = {
            pageReady: function () {
                this.initZipcode();

                C.Token.CheckToken().done(function (result) {
                    if (!result.isLogin) {
                        C.sweet.warning(local.UserNotLoggedIn, local.ActionLoginRequiredRedirectHome, function () {
                            w.location.href = "/";
                        });
                        return;
                    }

                    w.IsLogin = true;
                    w.islogin = true;

                    MemberPage.Init.start(result);
                });
            },

            initZipcode: function () {
                var $twZipcode = $(MemberPage.Selectors.twZipcode);
                var addr = $twZipcode.find(".address").val();

                C.Zipcode.init(MemberPage.Selectors.twZipcode);
                C.Zipcode.setData({
                    el: $twZipcode,
                    addr: addr
                });
            },

            start: function (loginData) {
                MemberPage.State.dateNow = MemberPage.Utils.todayText();

                MemberPage.Modals.init(loginData);
                MemberPage.Profile.init();
                MemberPage.Router.init();
                MemberPage.Router.change();
            }
        };

        w.PageReady = function () {
            MemberPage.Init.pageReady();
        };
    }
})(window);
