(function (window, document) {
    "use strict";

    let runtimePromise = null;

    function loadTurnJs() {
        if (window.jQuery && typeof window.jQuery.fn.turn === "function") {
            return Promise.resolve();
        }

        const url = "/lib/turnjs/turn.js";
        const existing = document.querySelector(`script[data-flipbook-runtime="${url}"]`);
        if (existing) {
            return new Promise(function (resolve, reject) {
                existing.addEventListener("load", resolve, { once: true });
                existing.addEventListener("error", reject, { once: true });
            });
        }

        return new Promise(function (resolve, reject) {
            const script = document.createElement("script");
            script.src = url;
            script.async = true;
            script.dataset.flipbookRuntime = url;
            script.addEventListener("load", resolve, { once: true });
            script.addEventListener("error", reject, { once: true });
            document.head.appendChild(script);
        });
    }

    window.FlipBookInit = function () {
        if (runtimePromise === null) {
            runtimePromise = loadTurnJs()
                .then(function () {
                    return import("/Shared/components/FlipBook/index.mjs");
                })
                .catch(function (error) {
                    runtimePromise = null;
                    throw error;
                });
        }

        return runtimePromise
            .then(function (module) {
                return module.FlipBookInit();
            })
            .catch(function (error) {
                console.error("FlipBook initialization failed.", error);
            });
    };
})(window, document);
