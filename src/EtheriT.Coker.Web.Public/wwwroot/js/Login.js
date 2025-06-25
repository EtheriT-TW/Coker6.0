document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('OtherLoginModal');
    const loginBaseUrl = `${BackstageUrl}api/OAuth/ExternalLogin`;
    const currentUrl = new URL(window.location.href);
          currentUrl.searchParams.set("siteId", SiteId);
    const redirectUrl = encodeURIComponent(currentUrl.toString());
    if (modal != null) {
        fetch(`${BackstageUrl}api/OAuth/GetEnabledProviders`)
            .then(response => response.json())
            .then(data => {
                const container = modal.querySelector('.modal-body>div');
                if (data.lineEnabled) {
                    container.after(createButton("LINE", "Line", "btn_line"));
                }
                if (data.googleEnabled) {
                    container.after(createButton("Google", "Google", "btn_google"));
                }
                if (data.facebookEnabled) {
                    container.after(createButton("Facebook", "Facebook", "btn_facebook"));
                }
                if (data.appleEnabled) {
                    container.after(createButton("Apple", "Apple", "btn_apple"));
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
        a.className = `btn-login ${cssClass} d-inline-block text-center w-100 rounded border-0 text-white py-2 fs-5 my-3`;
        a.textContent = provider;
        return a;
    }
});