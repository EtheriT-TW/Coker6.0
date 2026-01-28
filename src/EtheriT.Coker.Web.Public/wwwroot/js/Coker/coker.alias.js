(function (w) {
    const Coker = (w.Coker = w.Coker || {});
    Coker.defineModule("alias", function (C) {
        // ✅ 對外保留 co
        // 用 defineProperty 避免被別人不小心覆蓋；仍可讀取 window.co
        if (!w.co) {
            Object.defineProperty(w, "co", {
                configurable: true,
                enumerable: true,
                get: function () { return C; }
            });
        }

        // ✅ 對外保留 _c
        if (!w._c) {
            Object.defineProperty(w, "_c", {
                configurable: true,
                enumerable: true,
                get: function () { return C; }
            });
        }
    });
})(window);
