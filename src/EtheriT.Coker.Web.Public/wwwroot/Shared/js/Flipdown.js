const FlipTimer = function () {
    let list = [];
    if (typeof window.FlipDown !== "function") {
        list.push($.LoadJs("/lib/flipdown/flipdown.min.js"));
        list.push($.loadCss("/lib/flipdown/flipdown.min.css"));
    }
    const wait = list.length > 0 ? $.when.apply(null, list) : $.Deferred().resolve();
    wait.done(e => {
        document.querySelectorAll('.flipdown[data-timer]').forEach((el, i) => {
            if (el.dataset.initialized === "1") return;
            const date = new Date(el.dataset.timer);
            if (isNaN(date.getTime())) return;
            const theme = el.dataset.theme || "dark";
            if (!el.id) {
                const id = `flipdown-${i}`;
                el.id = id;
            }
            const end = Math.floor(date.getTime() / 1000);
            new FlipDown(end, el.id, { theme: theme }).start();
            el.dataset.initialized = "1";
        });
    });
}