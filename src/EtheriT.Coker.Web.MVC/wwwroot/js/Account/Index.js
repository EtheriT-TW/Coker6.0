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
        if (result.success && $("#loginBtn").length) {
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
            if (result.requiresAccountSetup) {
                co.sweet.error("首次啟用後台", result.error, function () {
                    location.href = "/Account/Forget";
                });
                return;
            }
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
    let requiresAccountSetup = false;
    let checkedAccount = null;
    let accountCheckTimer;
    let accountCheckRequest;
    let accountCheckVersion = 0;

    function showAccountStatus(message, available) {
        $("#accountAvailability").text(message)
            .toggleClass("text-success", available)
            .toggleClass("text-danger", !available);
    }

    function checkSetupAccount() {
        clearTimeout(accountCheckTimer);
        if (!requiresAccountSetup) return;
        const input = document.getElementById("setupAccount");
        const account = input.value.trim();
        const version = ++accountCheckVersion;
        if (accountCheckRequest) accountCheckRequest.abort();
        checkedAccount = null;
        $("#subnewpsw").prop("disabled", true);
        if (!/^[A-Za-z][A-Za-z0-9._-]{3,49}$/.test(account)) {
            showAccountStatus(account ? "帳號格式不符，請確認帳號規則。" : "請輸入登入帳號。", false);
            return;
        }
        showAccountStatus("檢查中…", false);
        accountCheckRequest = $.ajax({
            url: "/api/User/CheckAccountAvailability",
            method: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ ForgetID: forgetId, Account: account })
        }).done(function (result) {
            if (version !== accountCheckVersion || input.value.trim() !== account) return;
            checkedAccount = result.success ? account : null;
            showAccountStatus(result.success ? "✓ 帳號可使用" :
                (result.error || "此帳號不可使用。"), !!result.success);
            $("#subnewpsw").prop("disabled", !result.success);
        }).fail(function (_, status) {
            if (version !== accountCheckVersion || status === "abort") return;
            showAccountStatus("無法確認帳號，請離開欄位重試。", false);
        });
    }

    $("#setupAccount").on("input", function () {
        if (!requiresAccountSetup) return;
        ++accountCheckVersion;
        checkedAccount = null;
        clearTimeout(accountCheckTimer);
        if (accountCheckRequest) accountCheckRequest.abort();
        $("#subnewpsw").prop("disabled", true);
        showAccountStatus("等待檢查…", false);
        accountCheckTimer = setTimeout(checkSetupAccount, 500);
    }).on("blur", checkSetupAccount);
    if ($("#resetPasswordForm").length) {
        $("#subnewpsw").prop("disabled", true);
        if (!forgetId) {
            co.sweet.error("連結無效", "密碼重設連結無效或已逾期，請重新申請。", function () {
                location.href = "/Account/Forget";
            });
        } else {
            co.User.ValidatePasswordReset(forgetId).done(function (result) {
                if (result.success) {
                    requiresAccountSetup = result.message === "RequiresAccountSetup";
                    $("#accountSetupFields").prop("hidden", !requiresAccountSetup);
                    $("#setupAccount").prop("required", requiresAccountSetup);
                    if (requiresAccountSetup) $(".form-block h3").text("完成後台帳號設定");
                    $("#subnewpsw").prop("disabled", requiresAccountSetup);
                    if (requiresAccountSetup) checkSetupAccount();
                }
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
        if (requiresAccountSetup && !document.getElementById("setupAccount").reportValidity()) return;
        if (requiresAccountSetup && checkedAccount !== $("#setupAccount").val().trim()) {
            checkSetupAccount();
            return;
        }
        if (password !== passwordConfirm) {
            co.sweet.warn("提醒", "輸入的密碼不相符。");
            return;
        }

        co.sweet.loading("處理中", "正在重設密碼，請稍候...");
        co.User.ResetPassword({
            ForgetID: forgetId,
            Account: requiresAccountSetup ? $("#setupAccount").val().trim() : null,
            Password: password,
            PasswordConfirm: passwordConfirm
        }).done(function (result) {
            Swal.close();
            if (!result.success) {
                if (requiresAccountSetup) {
                    checkedAccount = null;
                    $("#subnewpsw").prop("disabled", true);
                    showAccountStatus("提交未完成，請離開帳號欄位重新檢查。", false);
                }
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

    function updatePasswordRules() {
        const input = document.getElementById("newpassword");
        if (!input) return;
        const value = input.value;
        const categories = {
            lowercase: /[a-z]/.test(value),
            uppercase: /[A-Z]/.test(value),
            number: /\p{Nd}/u.test(value),
            // Match .NET's Unicode-aware \W; underscore is not a symbol.
            symbol: /[^\p{L}\p{Mn}\p{Nd}\p{Pc}]/u.test(value)
        };
        Object.keys(categories).forEach(function (id) {
            $("#" + id).toggleClass("invalid", !categories[id]).toggleClass("valid", categories[id]);
        });
        const count = Object.values(categories).filter(Boolean).length;
        $("#length").toggleClass("invalid", value.length < 8 || value.length > 32)
            .toggleClass("valid", value.length >= 8 && value.length <= 32);
        $("#composition").text("四類中至少符合三類（目前 " + count + "／4）")
            .toggleClass("invalid", count < 3).toggleClass("valid", count >= 3);
        $("#rule").css("display", value.length ? "block" : "none");
        $("#short-rule").css("display", value.length ? "none" : "block");
    }

    function updatePasswordMatch() {
        const password = document.getElementById("newpassword");
        const confirmation = document.getElementById("agnewpassword");
        if (!password || !confirmation) return;
        const hasConfirmation = confirmation.value.length > 0;
        const matches = password.value.length > 0 && password.value === confirmation.value;
        $("#passwordMatchStatus").prop("hidden", !hasConfirmation)
            .text(matches ? "✓ 兩次密碼一致" : "兩次密碼不一致")
            .toggleClass("text-success", hasConfirmation && matches)
            .toggleClass("text-danger", hasConfirmation && !matches);
        if (hasConfirmation && !matches) confirmation.setAttribute("aria-invalid", "true");
        else confirmation.removeAttribute("aria-invalid");
    }

    $("#newpassword").on("input change", updatePasswordRules);
    $("#newpassword, #agnewpassword").on("input change", updatePasswordMatch);
    updatePasswordRules();
    updatePasswordMatch();
    // Re-evaluate restored/autofilled values without using focus as a visibility switch.
    $(window).on("pageshow", function () {
        updatePasswordRules();
        updatePasswordMatch();
    });
    $(document).on("change", "#agnewpassword", updatePasswordRules);

    $(document).on("click", ".toggle-password", function (e) {
        e.preventDefault();

        const $toggle = $(this);
        const $icon = $toggle.is("button") ? $toggle.find("i") : $toggle;
        const targetSelector = $toggle.data("target");
        const $input = $(targetSelector);

        if (!$input.length) return;

        const isPassword = $input.attr("type") === "password";

        $input.attr("type", isPassword ? "text" : "password");

        $icon.toggleClass("fa-eye", isPassword);
        $icon.toggleClass("fa-eye-slash", !isPassword);
        $toggle.attr({ "aria-pressed": String(isPassword), "aria-label": isPassword ? "隱藏密碼" : "顯示密碼", "title": isPassword ? "隱藏密碼" : "顯示密碼" });
    });
};
