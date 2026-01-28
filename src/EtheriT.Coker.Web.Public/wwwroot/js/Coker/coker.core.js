(function (w) {
    "use strict";

    // Root
    var Coker = (w.Coker = w.Coker || {});

    // Meta (module guard + extend trace)
    Coker._meta = Coker._meta || {
        modules: {},
        extendLog: []
    };

    /**
     * Module guard (prevent double-loading).
     * Coker.defineModule("api-order", function(C){ ... });
     */
    Coker.defineModule = Coker.defineModule || function (name, factory) {
        if (!name || typeof factory !== "function") return;
        if (Coker._meta.modules[name]) return;

        Coker._meta.modules[name] = true;
        factory(Coker);
    };

    /**
     * Centralized extend entry.
     * Default:
     * - overwrite: false
     * - strict: true (namespace conflict -> throw)
     */
    Coker.extend = Coker.extend || function (source, options) {
        options = options || {};
        var overwrite = !!options.overwrite;
        var strict = (options.strict !== false); // default true

        if (!source || typeof source !== "object") {
            if (strict) throw new Error("[Coker.extend] source must be an object.");
            return;
        }

        Object.keys(source).forEach(function (key) {
            if (!key || typeof key !== "string") {
                if (strict) throw new Error("[Coker.extend] invalid key: " + String(key));
                return;
            }

            var value = source[key];

            // Primitive / function / null: direct assign (rare)
            if (value === null || typeof value !== "object") {
                if (typeof Coker[key] !== "undefined" && !overwrite) return;

                Coker[key] = value;
                Coker._meta.extendLog.push({ path: key, member: null, overwrite: overwrite, ts: Date.now() });
                return;
            }

            // Ensure namespace object exists
            var existing = Coker[key];
            if (typeof existing === "undefined") {
                Coker[key] = {};
            } else if (existing === null || typeof existing !== "object") {
                if (strict) throw new Error("[Coker.extend] namespace conflict: Coker." + key + " is not an object.");
                if (overwrite) Coker[key] = {};
                else return;
            }

            var target = Coker[key];

            Object.keys(value).forEach(function (prop) {
                if (!prop || typeof prop !== "string") {
                    if (strict) throw new Error("[Coker.extend] invalid member: " + String(prop) + " in " + key);
                    return;
                }

                if (!overwrite && Object.prototype.hasOwnProperty.call(target, prop)) return;

                target[prop] = value[prop];
                Coker._meta.extendLog.push({ path: key, member: prop, overwrite: overwrite, ts: Date.now() });
            });
        });
    };

    /**
     * Runtime inspector.
     * Coker.inspect()
     * Coker.inspect("Order")
     * Coker.inspect("util.money")
     */
    Coker.inspect = Coker.inspect || function (path) {
        var target = Coker;

        if (path) {
            var parts = String(path).split(".");
            for (var i = 0; i < parts.length; i++) {
                if (!target || typeof target !== "object" || !(parts[i] in target)) {
                    console.warn("[Coker.inspect] Path not found:", path);
                    return;
                }
                target = target[parts[i]];
            }
        }

        if (target === null || target === undefined) {
            console.log(target);
            return;
        }

        if (typeof target !== "object") {
            console.log(target);
            return;
        }

        var view = {};
        Object.keys(target).forEach(function (k) {
            var v = target[k];
            view[k] = (typeof v === "function") ? "ƒ()" : typeof v;
        });

        console.group("[Coker.inspect]" + (path ? " " + path : ""));
        console.table(view);
        console.groupEnd();
    };

    // -----------------------------
    // API Core (authHeader/get/post)
    // -----------------------------

    Coker.api = Coker.api || {};

    /**
     * Authorization header helper
     */
    Coker.api.authHeader = Coker.api.authHeader || function () {
        var token = w.localStorage ? w.localStorage.getItem("token") : null;
        return token ? { Authorization: "Bearer " + token } : {};
    };

    /**
     * GET wrapper
     * options:
     *   - auth: false => no Authorization header
     *   - headers: {} => extra headers (merged)
     *   - ajax: {} => passthrough override for $.ajax options
     */
    Coker.api.get = Coker.api.get || function (url, data, options) {
        options = options || {};
        var headers = {};

        if (options.auth !== false) {
            headers = Coker.api.authHeader();
        }

        if (options.headers && typeof options.headers === "object") {
            Object.assign(headers, options.headers);
        }

        var ajaxOpt = {
            url: url,
            type: "GET",
            contentType: "application/json; charset=utf-8",
            headers: headers,
            data: data
        };

        if (options.ajax && typeof options.ajax === "object") {
            Object.assign(ajaxOpt, options.ajax);
        }

        return $.ajax(ajaxOpt);
    };

    /**
     * POST wrapper (JSON)
     * options:
     *   - auth: false
     *   - headers: {}
     *   - ajax: {}
     */
    Coker.api.post = Coker.api.post || function (url, body, options) {
        options = options || {};
        var headers = {};

        if (options.auth !== false) {
            headers = Coker.api.authHeader();
        }

        if (options.headers && typeof options.headers === "object") {
            Object.assign(headers, options.headers);
        }

        var ajaxOpt = {
            url: url,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            headers: headers,
            data: body ? JSON.stringify(body) : null,
            dataType: "json"
        };

        if (options.ajax && typeof options.ajax === "object") {
            Object.assign(ajaxOpt, options.ajax);
        }

        return $.ajax(ajaxOpt);
    };

})(window);