(function () {
    "use strict";

    Coker.extend({
        HashPage: {
            create: function (options) {
                const settings = $.extend(true, {
                    root: null,
                    defaultHash: "List",
                    listHash: "List",
                    newHash: "new",
                    contentPageKey: "Content",
                    listPageKey: "List",
                    titleSelector: "[data-hash-title]",
                    scrollTarget: null,
                    useStack: true,
                    onList: null,
                    onNew: null,
                    onEdit: null,
                    onChange: null
                }, options || {});

                const $root = $(settings.root);
                if ($root.length === 0) {
                    console.warn("HashPage root not found:", settings.root);
                    return null;
                }

                let stack = [];
                let ignorePush = false;
                let currentHash = "";

                function getHash() {
                    return (window.location.hash || "").replace(/^#/, "").trim();
                }

                function normalizeHash(hash) {
                    const val = String(hash || "").trim();
                    return val || settings.defaultHash;
                }

                function isNumericHash(hash) {
                    return /^\d+$/.test(String(hash || "").trim());
                }

                function isListHash(hash) {
                    return String(hash || "").toLowerCase() === String(settings.listHash).toLowerCase();
                }

                function isNewHash(hash) {
                    return String(hash || "").toLowerCase() === String(settings.newHash).toLowerCase();
                }

                function parseHash(hash) {
                    const val = normalizeHash(hash);

                    if (isListHash(val)) {
                        return {
                            raw: val,
                            mode: "list",
                            id: null,
                            pageKey: settings.listPageKey,
                            title: "列表"
                        };
                    }

                    if (isNewHash(val)) {
                        return {
                            raw: val,
                            mode: "new",
                            id: null,
                            pageKey: settings.contentPageKey,
                            title: "新增"
                        };
                    }

                    if (isNumericHash(val)) {
                        return {
                            raw: val,
                            mode: "edit",
                            id: parseInt(val, 10),
                            pageKey: settings.contentPageKey,
                            title: "編輯"
                        };
                    }

                    return {
                        raw: settings.listHash,
                        mode: "list",
                        id: null,
                        pageKey: settings.listPageKey,
                        title: "列表"
                    };
                }

                function setTitle(text) {
                    $root.find(settings.titleSelector).text(text || "");
                }

                function showPage(pageKey) {
                    $root.find("[data-hash-page]").addClass("d-none");
                    $root.find(`[data-hash-page="${pageKey}"]`).removeClass("d-none");
                }

                function moveToTarget() {
                    const $target = settings.scrollTarget
                        ? $root.find(settings.scrollTarget).first()
                        : $root.find(`[data-hash-page="${settings.contentPageKey}"]`).first();

                    if (!$target.length) return;

                    $("html, body").stop().animate({
                        scrollTop: Math.max($target.offset().top - 20, 0)
                    }, 200);
                }

                function pushStack(hash) {
                    if (!settings.useStack) return;
                    if (!hash) return;
                    if (stack.length > 0 && stack[stack.length - 1] === hash) return;
                    stack.push(hash);
                }

                function applyState(state, moveScroll) {
                    showPage(state.pageKey);
                    setTitle(state.title);

                    if (state.mode === "list") {
                        typeof settings.onList === "function" && settings.onList(state);
                    } else if (state.mode === "new") {
                        typeof settings.onNew === "function" && settings.onNew(state);
                        if (moveScroll !== false) moveToTarget();
                    } else if (state.mode === "edit") {
                        typeof settings.onEdit === "function" && settings.onEdit(state);
                        if (moveScroll !== false) moveToTarget();
                    }

                    typeof settings.onChange === "function" && settings.onChange(state);
                }

                function syncFromHash() {
                    const hash = normalizeHash(getHash());
                    const state = parseHash(hash);

                    if (!ignorePush) {
                        pushStack(state.raw);
                    } else {
                        ignorePush = false;
                    }

                    currentHash = state.raw;
                    applyState(state, true);
                }

                function setHash(hash, skipPush) {
                    const nextHash = normalizeHash(hash);

                    if (skipPush === true) {
                        ignorePush = true;
                    }

                    if (getHash() === nextHash) {
                        const state = parseHash(nextHash);
                        currentHash = state.raw;
                        if (!skipPush) pushStack(state.raw);
                        applyState(state, true);
                        return;
                    }

                    window.location.hash = nextHash;
                }

                function goList() {
                    setHash(settings.listHash);
                }

                function goNew() {
                    setHash(settings.newHash);
                }

                function goId(id) {
                    if (id === null || id === undefined || id === "") return;
                    setHash(String(id));
                }

                function back() {
                    if (settings.useStack && stack.length > 1) {
                        stack.pop();
                        const prev = stack.pop() || settings.listHash;
                        setHash(prev, true);
                        return;
                    }

                    if (window.history.length > 1) {
                        window.history.back();
                        return;
                    }

                    goList();
                }

                function bindButtons() {
                    $root.off("click.hashPageGo", "[data-hash-go]");
                    $root.on("click.hashPageGo", "[data-hash-go]", function (e) {
                        e.preventDefault();
                        const hash = $(this).attr("data-hash-go");
                        setHash(hash);
                    });

                    $root.off("click.hashPageBack", "[data-hash-back]");
                    $root.on("click.hashPageBack", "[data-hash-back]", function (e) {
                        e.preventDefault();
                        back();
                    });
                }

                function bindHashChange() {
                    window.addEventListener("hashchange", syncFromHash);
                }

                function init() {
                    bindButtons();
                    bindHashChange();

                    if (!getHash()) {
                        setHash(settings.defaultHash);
                        return;
                    }

                    syncFromHash();
                }

                init();

                return {
                    getHash: getHash,
                    parseHash: parseHash,
                    setHash: setHash,
                    goList: goList,
                    goNew: goNew,
                    goId: goId,
                    back: back,
                    refresh: syncFromHash
                };
            }
        }
    });
})();