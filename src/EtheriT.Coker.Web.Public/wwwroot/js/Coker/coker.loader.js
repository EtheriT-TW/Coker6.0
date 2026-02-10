(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("loader", function (C) {

        // -----------------------------------------
        // Coker.loader (JS/CSS dynamic loader)
        // - CSP-safe
        // - load once per url
        // - Promise-based
        // -----------------------------------------

        var _loaded = Object.create(null); // url -> Promise

        function loadOnce(url, executor) {
            if (!url) return Promise.reject(new Error("[Coker.loader] empty url"));
            if (_loaded[url]) return _loaded[url];

            _loaded[url] = new Promise(executor).catch(function (err) {
                // allow retry after failure
                delete _loaded[url];
                throw err;
            });

            return _loaded[url];
        }

        function loadScriptOnce(url, options) {
            options = options || {};

            return loadOnce(url, function (resolve, reject) {
                var s = document.createElement("script");
                s.src = url;
                s.async = (options.async !== false); // default true
                s.defer = !!options.defer;

                if (options.crossOrigin) s.crossOrigin = options.crossOrigin;
                if (options.referrerPolicy) s.referrerPolicy = options.referrerPolicy;

                s.onload = function () { resolve(url); };
                s.onerror = function () { reject(new Error("[Coker.loader] failed to load script: " + url)); };

                document.head.appendChild(s);
            });
        }

        function loadCssOnce(url, options) {
            options = options || {};

            return loadOnce(url, function (resolve, reject) {
                var l = document.createElement("link");
                l.rel = "stylesheet";
                l.href = url;

                if (options.crossOrigin) l.crossOrigin = options.crossOrigin;
                if (options.referrerPolicy) l.referrerPolicy = options.referrerPolicy;

                l.onload = function () { resolve(url); };
                l.onerror = function () { reject(new Error("[Coker.loader] failed to load css: " + url)); };

                document.head.appendChild(l);
            });
        }

        /**
         * Ensure assets loaded.
         * @param {Object} assets
         * @param {string[]} [assets.css]
         * @param {string[]} [assets.js]
         * @param {Object} [assets.scriptOptions] options passed to loadScriptOnce
         * @param {Object} [assets.cssOptions] options passed to loadCssOnce
         */
        function ensure(assets) {
            assets = assets || {};
            var css = assets.css || [];
            var js = assets.js || [];

            var cssOpt = assets.cssOptions || null;
            var jsOpt = assets.scriptOptions || null;

            // CSS: parallel
            var cssTasks = css.map(function (u) { return loadCssOnce(u, cssOpt || undefined); });

            // JS: sequential (safer for dependencies)
            var jsChain = Promise.resolve();
            js.forEach(function (u) {
                jsChain = jsChain.then(function () { return loadScriptOnce(u, jsOpt || undefined); });
            });

            return Promise.all(cssTasks).then(function () { return jsChain; });
        }

        function isLoaded(url) {
            return !!_loaded[url];
        }

        // attach
        C.extend({
            loader: {
                loadScriptOnce: loadScriptOnce,
                loadCssOnce: loadCssOnce,
                ensure: ensure,
                isLoaded: isLoaded
            }
        }, { overwrite: false });

    });

})(window);
