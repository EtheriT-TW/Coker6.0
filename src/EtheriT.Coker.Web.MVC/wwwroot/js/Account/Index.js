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
        const configuredDefaultUrl = co.Data.DefauleUrl || "/Welcome";
        const defaultUrl = configuredDefaultUrl === "/" ? "/Welcome" : configuredDefaultUrl;
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
    const hasExplicitReturnUrl = new URLSearchParams(window.location.search).has("returnUrl");

    function navigateAfterLogin() {
        const data = hasExplicitReturnUrl ? { returnUrl: returnUrl } : {};
        $.ajax({
            url: "/api/navigation/post-login-destination",
            method: "GET",
            data: data,
            dataType: "json"
        }).done(function (result) {
            const destination = result && (result.Url || result.url);
            location.href = typeof destination === "string" && destination ? destination : returnUrl;
        }).fail(function () {
            location.href = returnUrl;
        });
    }

    function getForgetId() {
        return new URLSearchParams(window.location.search).get("forgetId");
    }

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
            navigateAfterLogin();
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
            navigateAfterLogin();
        });
    });

    $("#forgetPasswordBtn").on("click", function (e) {
        e.preventDefault();
        const email = $("#email").val().trim();
        if (!email) {
            co.sweet.warn("提醒", "請輸入電子信箱。");
            return;
        }

        co.sweet.loading("寄送中", "正在寄送密碼重設信，請稍候...");
        co.User.RequestPasswordReset({ Email: email }).done(function (result) {
            Swal.close();
            if (!result.success) {
                co.sweet.error("寄送失敗", result.error || "密碼重設信寄送失敗，請稍後再試。");
                return;
            }
            co.sweet.success("申請完成", function () {
                location.href = "/Account/Index";
            }, false);
        }).fail(function () {
            Swal.close();
            co.sweet.error("寄送失敗", "伺服器暫時無法回應，請稍後再試。");
        });
    });

    const forgetId = getForgetId();
    if ($("#resetPasswordForm").length) {
        if (!forgetId) {
            co.sweet.error("連結無效", "密碼重設連結無效或已逾期，請重新申請。", function () {
                location.href = "/Account/Forget";
            });
        } else {
            co.User.ValidatePasswordReset(forgetId).done(function (result) {
                if (!result.success) {
                    co.sweet.error("連結無效", result.error || "密碼重設連結無效或已逾期，請重新申請。", function () {
                        location.href = "/Account/Forget";
                    });
                }
            });
        }
    }

    $("#subnewpsw").on("click", function (e) {
        e.preventDefault();
        const password = newpassword.value;
        const passwordConfirm = agnewpassword.value;
        if (password !== passwordConfirm) {
            co.sweet.warn("提醒", "輸入的密碼不相符。");
            return;
        }

        co.sweet.loading("處理中", "正在重設密碼，請稍候...");
        co.User.ResetPassword({
            ForgetID: forgetId,
            Password: password,
            PasswordConfirm: passwordConfirm
        }).done(function (result) {
            Swal.close();
            if (!result.success) {
                co.sweet.error("重設失敗", result.error || "無法重設密碼，請重新確認。");
                return;
            }
            co.sweet.success("密碼重設成功", function () {
                location.href = "/Account/Index";
            }, false);
        }).fail(function () {
            Swal.close();
            co.sweet.error("重設失敗", "伺服器暫時無法回應，請稍後再試。");
        });
    });

    $("#newpassword").on("input focus", function () {
        const value = this.value;
        $(lowercase).toggleClass("invalid", !/[a-z]/.test(value)).toggleClass("valid", /[a-z]/.test(value));
        $(uppercase).toggleClass("invalid", !/[A-Z]/.test(value)).toggleClass("valid", /[A-Z]/.test(value));
        $(number).toggleClass("invalid", !/\d/.test(value)).toggleClass("valid", /\d/.test(value));
        $(symbol).toggleClass("invalid", !/\W/.test(value)).toggleClass("valid", /\W/.test(value));
        $(length).toggleClass("invalid", value.length < 8 || value.length > 32).toggleClass("valid", value.length >= 8 && value.length <= 32);
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
