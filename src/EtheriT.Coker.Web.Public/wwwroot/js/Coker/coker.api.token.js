(function (w) {
    "use strict";
    var Coker = (w.Coker = w.Coker || {});

    // One initialization/renewal shared by bootstrap and all search requests.
    var pending = null;
    var current = null;
    function expiresSoon(token) {
        try {
            var payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            var expiry = JSON.parse(w.atob(payload)).exp;
            return !Number.isFinite(expiry) || expiry * 1000 <= Date.now() + 60000;
        } catch (_) { return true; }
    }
    function storeToken(token) {
        try { w.localStorage.setItem('token', token); } catch (_) { /* Cookies and in-memory token still work. */ }
    }

    Coker.extend({
        Token: {
            getAccessToken: function () { return current && current.token; },
            ResetToken: function () { current = null; },
            EnsureToken: function () {
                if (pending) return pending;
                var stored;
                var storageAvailable = false;
                try { stored = w.localStorage.getItem('token'); storageAvailable = true; } catch (_) { /* Storage may be blocked. */ }
                // A login/logout in another tab must invalidate the cached identity.
                if (current && storageAvailable && stored !== current.token) current = null;
                if (current && !expiresSoon(current.token)) return $.Deferred().resolve(current).promise();
                pending = Coker.Token.GetToken().then(function (result) {
                    if (!result || !result.success || !result.token || expiresSoon(result.token)) {
                        return $.Deferred().reject({ statusText: 'token-initialization-failed' }).promise();
                    }
                    current = result;
                    storeToken(result.token);
                    return result;
                });
                pending.fail(function () {
                    current = null;
                    Coker.api.clearAuth();
                });
                var request = pending;
                request.always(function () { pending = null; });
                return request;
            },
            GetToken: function () {
                return Coker.api.post("/api/Token/CreateToken", null, { auth: false });
            },
            CheckToken: function () {
                return Coker.api.get("/api/Token/CheckToken/");
            },
            AgreePrivacy: function () {
                return Coker.api.get("/api/Token/AgreePrivacy/");
            }
        }
    });

})(window);