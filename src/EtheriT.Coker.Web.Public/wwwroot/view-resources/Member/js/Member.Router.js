(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-router", factory);
    else factory(Coker);

    function factory() {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Router = {
            init: function () {
                this.bindTabButtons();

                if ("onhashchange" in w) {
                    w.onhashchange = function (e) {
                        if (e) e.preventDefault();
                        MemberPage.Router.change();
                    };
                } else {
                    setInterval(function () {
                        MemberPage.Router.change();
                    }, 1000);
                }
            },

            bindTabButtons: function () {
                $(MemberPage.Selectors.toolList + " > li button")
                    .off("click.memberRouter")
                    .on("click.memberRouter", function () {
                        switch ($(this).attr("id")) {
                            case "bonus-tab":
                                w.location.hash = "#bonus";
                                break;
                            case "profile-tab":
                                w.location.hash = "#order";
                                break;
                            case "favorite-tab":
                                w.location.hash = "#favorites";
                                break;
                            case "history-tab":
                                w.location.hash = "#browsing";
                                break;
                            default:
                                w.location.hash = "#";
                                break;
                        }
                    });
            },

            change: function () {
                var hash = w.location.hash || "";

                if (hash.startsWith("#bonus")) return this.showBonus(hash);
                if (hash.startsWith("#order")) return this.showOrder(hash);
                if (hash.startsWith("#browsing")) return this.showBrowsing(hash);
                if (hash.startsWith("#favorites")) return this.showFavorites(hash);

                return this.showInfo();
            },

            showInfo: function () {
                MemberPage.Utils.activateTab(MemberPage.Selectors.infoPane, "#info-tab");
                MemberPage.Utils.setTabNow("info");
            },

            showBonus: function (hash) {
                if ($(MemberPage.Selectors.bonusPane).length <= 0) {
                    w.location.hash = "";
                    return;
                }

                MemberPage.Utils.activateTab(MemberPage.Selectors.bonusPane, "#bonus-tab");

                var page = MemberPage.Utils.getHashPage(hash);
                if (page != null) {
                    MemberPage.Bonus.loadPage(page);
                    MemberPage.Utils.setTabNow("bonus");
                } else {
                    w.location.hash = "#bonus-1";
                }
            },

            showOrder: function (hash) {
                if ($(MemberPage.Selectors.orderPane).length <= 0) {
                    w.location.hash = "";
                    return;
                }

                MemberPage.Utils.activateTab(MemberPage.Selectors.orderPane, "#profile-tab");

                var page = MemberPage.Utils.getHashPage(hash);
                if (page != null) {
                    MemberPage.Orders.loadPage(page);
                    MemberPage.Utils.setTabNow("order");
                } else {
                    w.location.hash = "#order-1";
                }
            },

            showBrowsing: function (hash) {
                MemberPage.Utils.activateTab(MemberPage.Selectors.historyPane, "#history-tab");

                var page = MemberPage.Utils.getHashPage(hash);
                if (page != null) {
                    MemberPage.Products.loadBrowsingHistoryPage(page);
                    MemberPage.Utils.setTabNow("browsing");
                } else {
                    w.location.hash = "#browsing-1";
                }
            },

            showFavorites: function (hash) {
                MemberPage.Utils.activateTab(MemberPage.Selectors.favoritePane, "#favorite-tab");

                var page = MemberPage.Utils.getHashPage(hash);
                if (page != null) {
                    MemberPage.Products.loadFavoritesPage(page);
                    MemberPage.Utils.setTabNow("favorites");
                } else {
                    w.location.hash = "#favorites-1";
                }
            }
        };
    }
})(window);
