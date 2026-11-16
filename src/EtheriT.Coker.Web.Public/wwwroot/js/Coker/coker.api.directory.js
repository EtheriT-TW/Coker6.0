(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Directory: {
            normalizeSearchKeyword: function (value) {
                if (typeof value !== 'string') return '';
                const text = value.trim();
                return /^(undefined|null)$/i.test(text) ? '' : text;
            },
            encodeSearchKeyword: function (value) {
                const text = String(value ?? '');
                if (!text) return '';
                const bytes = new TextEncoder().encode(text);
                let binary = '';
                bytes.forEach(byte => { binary += String.fromCharCode(byte); });
                return '~b64~' + btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
            },
            decodeSearchKeyword: function (value) {
                const text = String(value ?? '');
                if (!text.startsWith('~b64~')) return text;
                const payload = text.substring(5);
                if (!/^[A-Za-z0-9_-]+$/.test(payload) || payload.length % 4 === 1) return text;
                try {
                    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
                    const binary = atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='));
                    const bytes = Uint8Array.from(binary, ch => ch.charCodeAt(0));
                    const decoded = new TextDecoder('utf-8', { fatal: true, ignoreBOM: true }).decode(bytes);
                    return Coker.Directory.encodeSearchKeyword(decoded) === text ? decoded : text;
                } catch (_) { return text; }
            },
            getNavigationContext: function () {
                const q = new URLSearchParams(w.location.search);
                return {
                    SearchSortBy: q.get('sort') || 'default',
                    SearchSortDirection: q.get('direction') === 'desc' ? 'desc' : 'asc',
                    ProductNameFilter: q.get('name') || '', filters: q.get('filter') || '',
                    Facet: q.get('facet') || '', DirectoryType: Number(q.get('directoryType')) || 0,
                    MaxLen: Number(q.get('maxLen')) || 0, ContentSource: q.get('source') || ''
                };
            },
            withNavigationContext: function (path, context) {
                let origin = w.location.origin;
                if (origin === 'null') {
                    try { origin = w.parent.location.origin; } catch (_) { return path; }
                }
                const url = new URL(path, origin);
                if (url.origin !== origin) return path;
                const q = url.searchParams;
                const sort = String(context.SearchSortBy || 'default').toLowerCase();
                q.delete('sort'); q.delete('direction');
                if (sort !== 'default') {
                    q.set('sort', sort);
                    q.set('direction', context.SearchSortDirection === 'desc' ? 'desc' : 'asc');
                }
                const set = (key, value) => {
                    q.delete(key);
                    if (value != null && String(value).trim() !== '') q.set(key, String(value));
                };
                set('name', context.ProductNameFilter);
                set('filter', context.filters);
                set('facet', context.Facet);
                set('directoryType', Number(context.DirectoryType) > 0 ? context.DirectoryType : '');
                set('maxLen', Number(context.MaxLen) > 0 ? context.MaxLen : '');
                set('source', context.ContentSource);
                if (context.dirids?.length) q.set('dirid', context.dirids.join(','));
                return url.pathname + url.search + url.hash;
            },
            getDirectoryData: function (data) {
                const request = Object.assign({}, data);
                request.SearchText = Coker.Directory.normalizeSearchKeyword(request.SearchText);
                const fullContent = /^(all-products|all-articles)$/.test(request.ContentSource || '');
                const keywordSearch = String(request.Type || request.directoryType || '').toLowerCase() === 'search' && !fullContent;
                if (keywordSearch && !request.SearchText) {
                    console.warn('[search] Missing or invalid keyword; request was not sent.');
                    return $.Deferred().reject({ statusText: 'invalid-search-keyword' }).promise();
                }
                if (!keywordSearch) return Coker.api.post("/api/Directory/GetReleInfo", request);
                return Coker.Token.EnsureToken().then(function () {
                    return Coker.api.post("/api/Directory/GetReleInfo", request);
                });
            },
            getDirectoryMenuData: function (data) {
                return Coker.api.post("/api/Directory/GetReleMenu", data);
            },
            getDirectoryAdvertiseData: function (data) {
                return Coker.api.post("/api/Directory/GetReleAd", data);
            },
            getDirectoryAdvertiseBatchData: function (data) {
                return Coker.api.post("/api/Directory/GetReleAdBatch", data);
            },
            SwitchPage: function (data) {
                const request = Object.assign({}, data);
                request.searchtext = Coker.Directory.normalizeSearchKeyword(request.searchtext);
                // Empty text is valid for canonical/all-content navigation; placeholders are not.
                if (data && data.searchtext != null && String(data.searchtext).trim() && !request.searchtext) {
                    console.warn('[search] Invalid navigation keyword; request was not sent.');
                    return $.Deferred().reject({ statusText: 'invalid-search-keyword' }).promise();
                }
                if (!request.searchtext) return Coker.api.post("/api/Directory/SwitchPage", request);
                return Coker.Token.EnsureToken().then(function () {
                    return Coker.api.post("/api/Directory/SwitchPage", request);
                });
            },
            getFacet: function (data) {
                return Coker.api.post("/api/Directory/GetFacet", data);
            }
        }
    });

})(window);
