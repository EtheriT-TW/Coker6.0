document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('LoginModal');
    const loginBaseUrl = `${BackstageUrl}api/OAuth/ExternalLogin`;
    const currentUrl = new URL(window.location.href);
          currentUrl.searchParams.set("siteId", SiteId);
    const redirectUrl = encodeURIComponent(currentUrl.toString());
    const displayMap = {
        line: {
            name: "LINE",
            provider: "Line",
            class: "btn_line"
        },
        google: {
            name: "Google",
            provider: "Google",
            class: "btn_google"
        },
        facebook: {
            name: "Facebook",
            provider: "Facebook",
            class: "btn_facebook"
        },
        apple: {
            name: "Apple",
            provider: "Apple",
            class: "btn_apple"
        }
    };
    if (modal != null) {
        fetch(`${BackstageUrl}api/OAuth/GetEnabledProviders`)
            .then(response => response.json())
            .then(data => {
                const container = modal.querySelector('.modal-body .otherLoginList');

                const enabledKeys = Object.entries(data)
                    .filter(([key, value]) => key.endsWith('Enabled') && value === true);

                if (enabledKeys.length === 0) {
                    document.querySelector('.btn_other_login')?.remove();
                } else {
                    enabledKeys.forEach(([key]) => {
                        const raw = key.replace('Enabled', '');
                        const entry = displayMap[raw];
                        if (entry) container.append(createButton(entry.name, entry.provider, entry.class));
                    });
                }
            })
    }
    let retryCount = 10;
    var t = setInterval(function () {
        if (typeof co !== "undefined" && typeof co.sweet !== "undefined") {
            clearInterval(t);
            if (OAuthError != "") co.sweet.error("登入失敗", OAuthError, null, false);
            else if (OAuthSuccess != "") co.sweet.success("登入成功", null, true);
        } else if (--retryCount <= 0) {
            clearInterval(t);
            console.warn("co.sweet 未在預期時間內載入，訊息顯示取消");
        }
    },1000);
    function createButton(text, provider, cssClass) {
        const a = document.createElement("a");
        a.href = `${loginBaseUrl}?provider=${provider}&redirect=${redirectUrl}`;
        a.className = `btn-login ${cssClass} d-inline-block text-center rounded border-0 text-white mt-3`;
        a.title = local.SignInWithAccount.format(text);
        a.textContent = text;
        return a;
    }
});