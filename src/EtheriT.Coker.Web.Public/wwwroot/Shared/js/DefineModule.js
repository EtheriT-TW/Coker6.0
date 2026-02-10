//# sourceURL=DefineModule.js
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.defineModule("modules.facade", function (C) {
        function exists(selector) {
            try { return document.querySelector(selector) != null; }
            catch (e) { return false; }
        }

        function resolvePath(path, root) {
            if (!path) return null;
            var obj = root || w;
            var parts = ("" + path).split(".");
            for (var i = 0; i < parts.length; i++) {
                obj = obj ? obj[parts[i]] : null;
            }
            return obj;
        }

        function toArray(v) { return Array.isArray(v) ? v : (v ? [v] : []); }

        // module run cache: key -> Promise<boolean>
        var _runOnce = Object.create(null);

        // ----------------------------
        // Module definitions (declare here)
        // 你只要在這裡維護 deps/assets/init 即可
        // ----------------------------
        var _defs = {
            articleViewer: {
                key: "articleViewer",
                selector: ".article-viewer", // 或者改成你要的 hook
                deps: [],
                assets: {
                    css: [
                        "/Shared/articleViewer.min.css"
                    ],
                    js: [
                        "/Shared/articleViewer.min.js"
                    ]
                },
                initOnce: true
            },

            gallery3d: {
                key: "gallery3d",
                selector: ".js-gallery3d-page",
                deps: ["articleViewer"],
                assets: {
                    css: [
                        "/Shared/gallery3d.min.css"
                    ],
                    js: [
                        "/Shared/gallery3d.min.js"
                    ]
                },
                initGlobal: "Gallery3DShowcasePage.init",
                initOnce: true
            }
        };

        /**
 * @param {string} key
 * @param {boolean} [force] 當作相依 dep 被載入時，略過 selector gate
 */
        function run(key, force) {
            if (!key) return Promise.reject(new Error("[modules] missing key"));
            if (_runOnce[key]) return _runOnce[key];

            var m = _defs[key];
            if (!m) return Promise.reject(new Error("[modules] unknown module: " + key));

            var p = Promise.resolve()
                // 1) deps first (sequential) - deps 一律強制載入
                .then(function () {
                    var deps = toArray(m.deps);
                    var chain = Promise.resolve();
                    deps.forEach(function (dk) {
                        chain = chain.then(function () { return run(dk, true); });
                    });
                    return chain;
                })
                // 2) selector gate (if defined) - 只有「非強制」才 gate
                .then(function () {
                    if (!force && m.selector && !exists(m.selector)) return false;

                    // 3) assets
                    var assets = m.assets || {};
                    assets.css = toArray(assets.css);
                    assets.js = toArray(assets.js);

                    return C.loader.ensure(assets).then(function () {
                        // 4) init
                        if (!!!m.initGlobal) return true; // no init needed
                        var fn = resolvePath(m.initGlobal, w);
                        if (typeof fn !== "function") {
                            throw new Error("[modules] init not found: " + m.initGlobal);
                        }
                        fn();
                        return true;
                    });
                })
                .catch(function (err) {
                    delete _runOnce[key];
                    throw err;
                });

            if (m.initOnce !== false) _runOnce[key] = p;
            return p;
        }

        // ----------------------------
        // Public API: co.modules.xxx()
        // ----------------------------
        C.extend({
            modules: {
                gallery3d: function () { return run("gallery3d"); },
                articleViewer: function () { return run("articleViewer"); },

                // optional helpers (debug/extension)
                run: run,
                defs: _defs
            }
        }, { overwrite: false });

    });

})(window);