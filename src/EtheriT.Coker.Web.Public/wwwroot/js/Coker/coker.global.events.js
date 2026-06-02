(function (w, d) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    if (typeof Coker.defineModule === "function") {
        Coker.defineModule("global-events", factory);
    } else {
        factory(Coker);
    }

    function factory(C) {
        function closest(element, selector) {
            if (!element) return null;

            if (element.closest) {
                return element.closest(selector);
            }

            // very old browser fallback
            while (element && element.nodeType === 1) {
                if (matches(element, selector)) return element;
                element = element.parentElement;
            }

            return null;
        }

        function matches(element, selector) {
            var proto = Element.prototype;
            var fn =
                proto.matches ||
                proto.msMatchesSelector ||
                proto.webkitMatchesSelector;

            return fn ? fn.call(element, selector) : false;
        }

        function bindNoLinkEvents() {
            d.removeEventListener("click", handleNoLinkClick, false);
            d.addEventListener("click", handleNoLinkClick, false);
        }

        function handleNoLinkClick(event) {
            var link = closest(event.target, "a.is-no-link");

            if (!link) return;

            // 只阻止 href="#" 的預設跳轉，不阻止事件冒泡。
            // 這樣 Layout_8 手機版若有 collapse click，仍可繼續運作。
            event.preventDefault();
        }

        if (d.readyState === "loading") {
            d.addEventListener("DOMContentLoaded", bindNoLinkEvents, { once: true });
        } else {
            bindNoLinkEvents();
        }

        if (typeof C.extend === "function") {
            C.extend(
                {
                    globalEvents: {
                        bindNoLinkEvents: bindNoLinkEvents
                    }
                },
                { overwrite: false }
            );
        } else {
            C.globalEvents = C.globalEvents || {};
            C.globalEvents.bindNoLinkEvents = C.globalEvents.bindNoLinkEvents || bindNoLinkEvents;
        }
    }
})(window, document);