(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    if (typeof Coker.defineModule === "function") {
        Coker.defineModule("member-core", factory);
    } else {
        factory(Coker);
    }

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.State = MemberPage.State || {
            tabNow: "info",
            dateNow: "",
            oldEmail: "",
            loginData: null,
            resetEmailModal: null,
            resetEmailModalElement: null,
            resetEmailCaptchaInput: null,
            resetEmailCaptchaImage: null,
            resetEmailForm: null,
            reOrderAlertModal: null,
            reOrderAlertModalElement: null,
            ecPayModal: null
        };

        MemberPage.Selectors = MemberPage.Selectors || {
            twZipcode: "#TWzipcode",
            userDataForm: "#UserDataForm",
            toolList: "#ToolList",
            tabContent: "#TabContent",
            infoPane: "#info-tab-pane",
            orderPane: "#profile-tab-pane",
            favoritePane: "#favorite-tab-pane",
            historyPane: "#history-tab-pane",
            bonusPane: "#bonus-tab-pane",
            resetEmailModal: "#ResetEmailModal",
            resetEmailForm: "#ResetEmailForm",
            resetEmailCaptchaInput: "#InputNewMailVCode",
            resetEmailCaptchaImage: "#NewMailImgCaptcha",
            reOrderAlertModal: "#ReOrderAlertModal",
            ecPayModal: "#ECPayModal"
        };

        MemberPage.Utils = MemberPage.Utils || {
            todayText: function () {
                var now = new Date();
                var month = String(now.getMonth() + 1).padStart(2, "0");
                var day = String(now.getDate()).padStart(2, "0");
                return now.getFullYear() + "-" + month + "-" + day;
            },

            getHashPage: function (hash) {
                hash = hash || w.location.hash || "";
                if (hash.indexOf("-") < 0) return null;

                var page = Number(hash.substring(hash.indexOf("-") + 1));
                return isNaN(page) ? null : page;
            },

            activateTab: function (paneSelector, tabSelector) {
                $(MemberPage.Selectors.tabContent + " > div").removeClass("active show");
                $(MemberPage.Selectors.toolList + " > li button").removeClass("active");

                $(paneSelector).addClass("active show");
                $(tabSelector).addClass("active");

                MemberPage.Utils.scrollActiveTabIntoView();
            },

            setTabNow: function (tabName) {
                MemberPage.State.tabNow = tabName;
            },

            scrollActiveTabIntoView: function () {
                var $toolList = $(MemberPage.Selectors.toolList);
                var $activeTab = $toolList.find(".nav-link.active").first();

                if ($toolList.length <= 0 || $activeTab.length <= 0) return;

                var toolList = $toolList.get(0);
                var activeTab = $activeTab.closest("li").get(0) || $activeTab.get(0);

                if (!toolList || !activeTab) return;

                // 只有在手機 / 平板橫向 tab 並且真的有 overflow 時才處理
                if (!w.matchMedia || !w.matchMedia("(max-width: 992px)").matches) return;
                if (toolList.scrollWidth <= toolList.clientWidth) return;

                setTimeout(function () {
                    if (typeof activeTab.scrollIntoView === "function") {
                        activeTab.scrollIntoView({
                            behavior: "auto",
                            block: "nearest",
                            inline: "center"
                        });
                        return;
                    }

                    // fallback
                    var targetLeft = activeTab.offsetLeft - (toolList.clientWidth - activeTab.offsetWidth) / 2;
                    toolList.scrollLeft = Math.max(targetLeft, 0);
                }, 0);
            },

            requireRenderer: function ($content) {
                if (!w.DirectoryRenderer || typeof w.DirectoryRenderer.renderItemsByExternalTemplate !== "function") {
                    console.error("DirectoryRenderer.renderItemsByExternalTemplate 尚未載入");
                    if ($content && $content.length) $content.empty();
                    return false;
                }
                return true;
            },

            loadBarcodeScript: function (callback) {
                var src = "https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js";

                if (typeof w.JsBarcode === "function") {
                    callback && callback();
                    return;
                }

                if (C.loader && typeof C.loader.loadScriptOnce === "function") {
                    C.loader.loadScriptOnce(src).then(function () {
                        callback && callback();
                    }).catch(function () {
                        console.error("JsBarcode 載入失敗：" + src);
                    });
                    return;
                }

                $.getScript(src, function () {
                    callback && callback();
                });
            }
        };
    }
})(window);
