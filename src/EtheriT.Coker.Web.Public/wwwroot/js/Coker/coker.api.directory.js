(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Directory: {
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
                return Coker.api.post("/api/Directory/GetReleInfo", data);
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
                return Coker.api.post("/api/Directory/SwitchPage", data);
            },
            getFacet: function (data) {
                return Coker.api.post("/api/Directory/GetFacet", data);
            }
        }
    });

})(window);
