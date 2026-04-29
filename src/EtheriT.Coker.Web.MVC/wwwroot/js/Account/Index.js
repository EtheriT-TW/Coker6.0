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

    if (!!co.Cookie.Get("isLogin")) {
        co.User.Check().done(function (result) {
            if (result.success) {
                location.href = returnUrl;
                return;
            }
        });
    }

    $("#loginBtn").on("click", function (e) {
        e.preventDefault();

        co.User.Login({
            UserName: $("#username").val(),
            Password: $("#password").val()
        }).done(function (result) {
            if (!result.success) {
                alert(result.error);
                return;
            }

            const lastWebSite = co.Cookie.Get("LastWebSite");

            // LastWebSite 應該是網站 ID，不應該是 /Account/Index
            // 如果不是數字，就不要拿來 exchange，直接回 returnUrl
            if (!lastWebSite || isNaN(lastWebSite)) {
                location.href = returnUrl;
                return;
            }

            co.WebSite.exchange(lastWebSite).done(function (exchangeResult) {
                if (exchangeResult.success) {
                    location.href = returnUrl;
                } else {
                    location.href = co.Data.DefauleUrl || "/";
                }
            });
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
};