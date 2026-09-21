//# sourceURL=LazyImage.js
(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    var defaultPlaceholder = "data:image/svg+xml," + encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 90"><rect width="160" height="90" fill="#f1f3f5"/><path d="M47 65l24-27 17 18 11-12 24 21H47z" fill="#adb5bd"/><circle cx="108" cy="29" r="8" fill="#adb5bd"/></svg>'
    );

    function toElements(elements) {
        if (!elements) return [];
        if (elements.jquery) return elements.toArray();
        if (elements.nodeType === 1) return [elements];
        return Array.from(elements).filter(function (element) {
            return element && element.nodeType === 1;
        });
    }

    function create(elements, options) {
        options = options || {};
        var images = toElements(elements);
        var states = new WeakMap();
        var fadeDuration = Number(options.fadeDuration);
        if (!Number.isFinite(fadeDuration)) fadeDuration = 250;

        function showPlaceholder(image) {
            var state = states.get(image);
            if (!state) return;

            image.style.transition = "none";
            image.style.opacity = "1";
            if (state.placeholder) {
                if (image.getAttribute("src") !== state.placeholder) {
                    image.setAttribute("src", state.placeholder);
                }
            } else {
                image.removeAttribute("src");
            }
        }

        function load(image) {
            var state = states.get(image);
            if (!state || !state.source || image.getAttribute("src") === state.source) return;

            image.style.transition = "none";
            image.style.opacity = "0";
            image.setAttribute("src", state.source);
        }

        function onLoad(event) {
            var image = event.currentTarget;
            var state = states.get(image);
            if (!state || image.getAttribute("src") !== state.source) return;

            image.style.transition = "opacity " + fadeDuration + "ms ease";
            w.requestAnimationFrame(function () {
                w.requestAnimationFrame(function () {
                    if (states.get(image) === state && image.getAttribute("src") === state.source) {
                        image.style.opacity = "1";
                    }
                });
            });
        }

        function onError(event) {
            var image = event.currentTarget;
            var state = states.get(image);
            if (state && image.getAttribute("src") === state.source) showPlaceholder(image);
        }

        images.forEach(function (image) {
            var declaredSource = image.getAttribute("data-src") || "";
            var currentSource = image.getAttribute("src") || "";
            var source = declaredSource || currentSource;
            var placeholder = image.getAttribute("data-placeholder-src") ||
                (declaredSource ? currentSource : (options.placeholder || defaultPlaceholder));
            states.set(image, { source: source, placeholder: placeholder });
            image.addEventListener("load", onLoad);
            image.addEventListener("error", onError);
            showPlaceholder(image);
        });

        var observer = typeof w.IntersectionObserver === "function"
            ? new w.IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        load(entry.target);
                    } else if (options.unloadOnExit === true ||
                        (options.unloadOnExit !== false && entry.target.getAttribute("data-coker-lazy") === "unload")) {
                        showPlaceholder(entry.target);
                    }
                });
            }, {
                root: options.root || null,
                rootMargin: options.rootMargin || "0px",
                threshold: options.threshold || 0
            })
            : null;

        function resume() {
            images.forEach(function (image) {
                if (observer) observer.observe(image);
                else load(image);
            });
        }

        function pause() {
            if (observer) observer.disconnect();
            if (options.unloadOnPause !== false) {
                images.forEach(showPlaceholder);
            }
        }

        function destroy() {
            if (observer) observer.disconnect();
            images.forEach(function (image) {
                image.removeEventListener("load", onLoad);
                image.removeEventListener("error", onError);
                states.delete(image);
            });
        }

        return {
            resume: resume,
            pause: pause,
            destroy: destroy,
            load: load,
            unload: showPlaceholder,
            getSource: function (image) {
                var state = states.get(image);
                return state ? state.source : "";
            }
        };
    }

    Coker.LazyImage = {
        create: create
    };
})(window);
