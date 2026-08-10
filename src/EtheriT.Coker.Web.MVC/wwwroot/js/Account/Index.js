var PageReady = function () {
    var rule = document.getElementById("rule");
    var newpassword = document.getElementById("newpassword");
    var agnewpassword = document.getElementById("agnewpassword");
    var lowercase = document.getElementById("lowercase");
    var uppercase = document.getElementById("uppercase");
    var number = document.getElementById("number");
    var symbol = document.getElementById("symbol");
    var length = document.getElementById("length");

    function getSafeReturnUrl() {
        const defaultUrl = co.Data.DefauleUrl || "/";
        const params = new URLSearchParams(window.location.search);
        let url = params.get("returnUrl");

        if (!url || typeof url !== "string") return defaultUrl;

        try {
            url = decodeURIComponent(url);
        } catch {
            return defaultUrl;
        }

        url = url.trim();

        // 只允許站內相對路徑
        if (!url.startsWith("/")) return defaultUrl;

        // 避免 //evil.com 這種外部跳轉
        if (url.startsWith("//")) return defaultUrl;

        // 避免登入後又回到登入頁 / 註冊頁 / 忘記密碼頁
        if (/^\/Account(\/|$)/i.test(url)) return defaultUrl;

        return url;
    }

    const returnUrl = getSafeReturnUrl();

    function getPreferredWebsiteIds() {
        const storagePrefix = "coker.websiteSwitcher.pinned.";
        const preferredWebsiteIds = {};
        try {
            for (let index = 0; index < localStorage.length; index++) {
                const storageKey = localStorage.key(index);
                if (!storageKey || !storageKey.startsWith(storagePrefix)) continue;

                const account = decodeURIComponent(storageKey.substring(storagePrefix.length));
                const websiteIds = JSON.parse(localStorage.getItem(storageKey) || "[]");
                if (!account || !Array.isArray(websiteIds) || websiteIds.length === 0) continue;

                const websiteId = Number(websiteIds[0]);
                if (Number.isInteger(websiteId) && websiteId > 0) {
                    preferredWebsiteIds[account] = websiteId;
                }
            }
        } catch {
            return {};
        }
        return preferredWebsiteIds;
    }

    co.User.Check().done(function (result) {
        if (result.success) {
            location.href = returnUrl;
            return;
        }
    });

    $("#loginBtn").on("click", function (e) {
        e.preventDefault();

        const userName = $("#username").val();

        co.User.Login({
            UserName: userName,
            Password: $("#password").val(),
            PreferredWebsiteIds: getPreferredWebsiteIds()
        }).done(function (result) {
            if (!result.success) {
                co.sweet.error("登入失敗", result.error || "帳號或密碼不正確，請重新確認。");
                return;
            }

            // Login API 優先使用 HttpOnly LastWebSite Cookie，沒有有效 Cookie 時才使用第一個置頂網站。
            location.href = returnUrl;
        });
    });

    $("#verification-btn").on("click", function () {

    });

    $("#subnewpsw").on("click", function () {
        if (lowercase.innerHTML == '' && uppercase.innerHTML == '' &&
            number.innerHTML == '' && symbol.innerHTML == '' &&
            length.innerHTML == '' && agnewpassword.value == newpassword.value) {
            alert("成功");
        } else {
            alert("失敗");
        }
    });

    $("#newpassword").on("focus", function () {
        $("#rule").css("display", "block");
        $("#short-rule").css("display", "none");
    });

    $("#newpassword").on("blur", function () {
        $("#short-rule").css("display", "block");
        $("#rule").css("display", "none");
    });

    $(document).on("click", ".toggle-password", function (e) {
        e.preventDefault();

        const $icon = $(this);
        const targetSelector = $icon.data("target");
        const $input = $(targetSelector);

        if (!$input.length) return;

        const isPassword = $input.attr("type") === "password";

        $input.attr("type", isPassword ? "text" : "password");

        $icon.toggleClass("fa-eye", isPassword);
        $icon.toggleClass("fa-eye-slash", !isPassword);
    });
};
