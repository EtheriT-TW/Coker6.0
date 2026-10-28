(function () {
    function hasOwn(obj, key) {
        return Object.prototype.hasOwnProperty.call(obj, key);
    }

    function resolveRoot(root) {
        if (!root) return document;
        if (typeof root === 'string') return document.querySelector(root) || document;
        return root;
    }

    function getDict(dict) {
        return dict || (typeof local !== 'undefined' ? local : window.local) || {};
    }

    function getText(key, fallback, dict) {
        const d = getDict(dict);
        if (hasOwn(d, key) && d[key] != null && String(d[key]) !== '') {
            return String(d[key]);
        }
        return fallback == null ? '' : String(fallback);
    }

    function applyI18n(root, dict) {
        const r = resolveRoot(root);
        const d = getDict(dict);

        // 只掃描「尚未處理」的節點（資源節省的關鍵）
        r.querySelectorAll('[data-i18n]:not([data-i18n-done="1"])').forEach(function (el) {
            const key = (el.getAttribute('data-i18n') || '').trim();
            if (!key) return;

            // 記錄 fallback（只記一次）
            if (!el.hasAttribute('data-i18n-fallback')) {
                el.setAttribute('data-i18n-fallback', el.textContent || '');
            }

            // 套用翻譯
            const next = getText(key, '', d);
            if (next !== '') {
                if (el.textContent !== next) el.textContent = next;
            } else {
                // 沒翻譯：回退原始
                const fb = el.getAttribute('data-i18n-fallback') || '';
                if (fb && el.textContent !== fb) el.textContent = fb;
            }

            // 標記完成
            if (next !== '') el.setAttribute('data-i18n-done', '1');
        });

        r.querySelectorAll('[data-i18n-attr]:not([data-i18n-attr-done="1"])').forEach(function (el) {
            const raw = (el.getAttribute('data-i18n-attr') || '').trim();
            if (!raw) return;

            let allTranslated = true;
            raw.split(';').forEach(function (pair) {
                const p = pair.trim();
                if (!p) return;

                const idx = p.indexOf(':');
                if (idx <= 0) return;

                const attr = p.slice(0, idx).trim();
                const key = p.slice(idx + 1).trim();
                if (!attr || !key) return;

                const fbAttr = 'data-i18n-fallback-' + attr;
                if (!el.hasAttribute(fbAttr)) {
                    el.setAttribute(fbAttr, el.getAttribute(attr) || '');
                }

                const next = getText(key, '', d);
                if (next !== '') {
                    if (el.getAttribute(attr) !== next) el.setAttribute(attr, next);
                } else {
                    allTranslated = false;
                    const fb = el.getAttribute(fbAttr) || '';
                    if (el.getAttribute(attr) !== fb) el.setAttribute(attr, fb);
                }
            });

            if (allTranslated) el.setAttribute('data-i18n-attr-done', '1');
        });
    }

    // 提供 reset：如果你真的遇到「同 DOM 需要再翻一次」就先 reset 再 apply
    function resetI18n(root) {
        const r = resolveRoot(root);
        r.querySelectorAll('[data-i18n-done="1"]').forEach(function (el) {
            el.removeAttribute('data-i18n-done');
        });
        r.querySelectorAll('[data-i18n-attr-done="1"]').forEach(function (el) {
            el.removeAttribute('data-i18n-attr-done');
        });
    }

    window.CokerI18n = window.CokerI18n || {};
    window.CokerI18n.get = getText;
    window.CokerI18n.apply = applyI18n;
    window.CokerI18n.reset = resetI18n;
})();
