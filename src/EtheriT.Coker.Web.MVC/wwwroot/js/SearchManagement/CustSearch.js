var custSearchState = { id: 0, system: false, ready: false, busy: false, revision: 0, grid: null, options: null, defaultId: null };

function custSearchPermission(name) {
    return document.getElementById('CustSearchSettings')?.getAttribute('data-' + name) === 'true';
}
function custSearchHeaders(write) {
    var headers = Object.assign({}, _c.Data.Header);
    var site = new URLSearchParams(location.search).get('_site');
    if (site) headers['X-Coker-Website-Id'] = site;
    if (write) headers['x-xsrf-token'] = $('#CustSearchForm [name="AntiforgeryField"]').val();
    return headers;
}
function custSearchGridBeforeSend(operation, ajaxOptions) {
    ajaxOptions.headers = Object.assign({}, ajaxOptions.headers, custSearchHeaders(false));
}
function custSearchApi(action, data, write) {
    var query = new URLSearchParams();
    var site = new URLSearchParams(location.search).get('_site');
    if (site) query.set('_site', site);
    var suffix = query.toString() ? '?' + query.toString() : '';
    var headers = custSearchHeaders(false);
    function send() {
        return $.ajax({ url: '/api/Search/' + action + suffix,
            type: write ? 'POST' : 'GET', contentType: 'application/json; charset=utf-8', headers: headers,
            data: write ? JSON.stringify(data) : data });
    }
    if (!write) return send();
    // The page token may no longer match a cookie refreshed by another tab or AJAX request.
    // Fetch a current pair before writing; do not retry the business POST after a failure.
    var pending = $.Deferred();
    $.ajax({ url: '/api/Search/GetAntiforgeryToken' + suffix, type: 'GET', headers: headers, cache: false })
        .done(function (result) {
            var token = result.token || result.Token;
            var headerName = result.headerName || result.HeaderName || 'x-xsrf-token';
            if (!token) {
                pending.reject({ status: 400, responseJSON: { message: '無法取得頁面驗證資料，請重新整理後再操作。' } });
                return;
            }
            Object.keys(headers).forEach(function (key) {
                if (key.toLowerCase() === 'x-xsrf-token' || key.toLowerCase() === headerName.toLowerCase()) delete headers[key];
            });
            headers[headerName] = token;
            $('input[name="AntiforgeryField"]').val(token);
            _c.Data.Header['x-xsrf-token'] = token;
            send().done(function () { pending.resolve.apply(pending, arguments); })
                  .fail(function () { pending.reject.apply(pending, arguments); });
        }).fail(function () { pending.reject.apply(pending, arguments); });
    return pending.promise();
}
function custSearchMessage(text, error) {
    $('#CustSearchMessage').toggleClass('d-none', !text).toggleClass('alert-danger', !!error).toggleClass('alert-info', !error).text(text || '');
}
function custSearchError(xhr) {
    if (xhr?.status === 400) {
        if (xhr.responseJSON?.message) return xhr.responseJSON.message;
        return xhr.responseJSON?.errors ? '提交資料格式不正確，請重新整理後再試（HTTP 400）。'
            : '頁面驗證未通過，請重新整理後再操作（HTTP 400）。';
    }
    return xhr?.status === 409 ? '網站已在其他分頁切換，請重新整理後再操作。' : xhr?.status === 403 ? '沒有操作此功能的權限。' : xhr?.status === 404 ? '搜尋項目不存在或已移除。' : '資料載入或儲存失敗，請重新整理後再試。';
}
function custSearchBox(id) { return custSearchState.options ? $(id).dxTagBox('instance') : null; }
function custSearchWritable() { return !custSearchState.system && custSearchPermission(custSearchState.id === 0 ? 'create' : 'update'); }
function custSearchTagIds(id) {
    return ($(id).data('tagList') || []).filter(function (x) { return !x.IsDeleted; }).map(function (x) { return x.FK_TId; });
}
function custSearchSetTags(id, keys, tags) {
    var names = keys.map(function (key) {
        var tag = tags.find(function (x) { return x.id === key; });
        return tag ? tag.title : '已失效（' + key + '）';
    });
    $(id).data({ tagList: keys.map(function (key) { return { Id: 0, FK_TId: key, IsDeleted: false }; }),
        tagCheckList: keys.slice(), tagText: names.join('、') }).val(names.length ? names.join('、') : '選擇標籤…');
    return keys.some(function (key) { return !tags.some(function (x) { return x.id === key; }); });
}
function custSearchSetControls() {
    var editable = custSearchState.ready && !custSearchState.busy && custSearchWritable();
    $('#CustSearchFields').prop('disabled', !editable);
    $('#CustSearchSave').prop('disabled', !editable);
    $('#CustSearchBack, #CustSearchAdd').prop('disabled', custSearchState.busy);
    var hasProducts = custSearchState.options?.hasProducts === true;
    $('#CustSearchProductScope').toggleClass('d-none', !hasProducts);
    $('#CustSearchAllProd').prop('disabled', !editable || !hasProducts);
    $('#CustSearchProductTags').prop('disabled', !editable || !hasProducts || $('#CustSearchAllProd').prop('checked'));
    var legacyProductScope = $('#CustSearchAllProd').prop('checked') || custSearchTagIds('#CustSearchProductTags').length > 0;
    $('#CustSearchInactiveProduct').toggleClass('d-none', hasProducts || !legacyProductScope);
    $('#CustSearchRemoveProductScope').prop('disabled', !editable);
    $('#CustSearchArticleTags').prop('disabled', !editable || $('#CustSearchAllArticle').prop('checked'));
    var menus = custSearchBox('#CustSearchMenus');
    if (menus) menus.option('disabled', !editable || $('#CustSearchAllMenu').prop('checked'));
    $('#CustSearchGrid input[name="cust-search-default"]').each(function () {
        $(this).prop('disabled', custSearchState.busy || !custSearchPermission('update') || !$(this).data('visible') || !$(this).data('available'));
    });
}
function custSearchFormData() {
    return {
        Id: custSearchState.id, Title: $('#CustSearchTitle').val(), Description: $('#CustSearchDescription').val(),
        Placeholder: $('#CustSearchPlaceholder').val(), SerNO: Number($('#CustSearchOrder').val()),
        Visible: $('#CustSearchVisible').prop('checked'),
        SearchAllProd: $('#CustSearchAllProd').prop('checked'), SearchAllArticle: $('#CustSearchAllArticle').prop('checked'),
        SearchAllMenu: $('#CustSearchAllMenu').prop('checked'),
        ProductTagIds: custSearchTagIds('#CustSearchProductTags'),
        ArticleTagIds: custSearchTagIds('#CustSearchArticleTags'),
        MenuIds: custSearchBox('#CustSearchMenus').option('value')
    };
}
function custSearchFill(data) {
    custSearchState.id = data.id;
    custSearchState.system = !!data.isSystem;
    $('#CustSearchEditorTitle').text(data.id === 0 ? '新增自訂搜尋' : data.isSystem ? '編輯系統搜尋' : '編輯自訂搜尋');
    $('#CustSearchTitle').val(data.title || '');
    $('#CustSearchDescription').val(data.description || '');
    $('#CustSearchPlaceholder').val(data.placeholder || '');
    $('#CustSearchOrder').val(data.serNO || 0);
    $('#CustSearchVisible').prop('checked', data.visible);
    $('#CustSearchAllProd').prop('checked', data.searchAllProd);
    $('#CustSearchAllArticle').prop('checked', data.searchAllArticle);
    $('#CustSearchAllMenu').prop('checked', data.searchAllMenu);
    var selectedTags = data.selectedTags || [];
    var productMissing = custSearchSetTags('#CustSearchProductTags', data.productTagIds || [], selectedTags);
    var articleMissing = custSearchSetTags('#CustSearchArticleTags', data.articleTagIds || [], selectedTags);
    var missing = productMissing || articleMissing;
    var menus = custSearchState.options.menus.slice();
    (data.menuIds || []).forEach(function (id) {
        if (!menus.some(function (x) { return x.id === id; })) {
            menus.push({ id: id, title: '已失效，請移除（' + id + '）', disabled: true });
            missing = true;
        }
    });
    custSearchBox('#CustSearchMenus').option({ dataSource: menus, value: data.menuIds || [] });
    custSearchMessage(missing ? '部分標籤或選單已失效，請移除或重新選擇後儲存。' : '', missing);
    custSearchState.ready = true;
    custSearchState.initialValues = JSON.stringify(custSearchFormData());
    custSearchSetControls();
}
function custSearchReadHash() {
    if (!custSearchState.options) return;
    var revision = ++custSearchState.revision;
    bootstrap.Modal.getInstance(document.getElementById('TagModal'))?.hide();
    var hash = location.hash.substring(1);
    $('#CustSearchAdd').toggleClass('d-none', !!hash);
    if (!hash) { $('#CustSearchList').removeClass('d-none'); $('#CustSearchEditor').addClass('d-none'); return; }
    if (!/^-?\d+$/.test(hash)) { location.hash = ''; return; }
    var id = Number(hash);
    if (!Number.isSafeInteger(id) || id < 0) { location.hash = ''; return; }
    custSearchState.id = id;
    custSearchState.ready = false;
    $('#CustSearchList').addClass('d-none'); $('#CustSearchEditor').removeClass('d-none');
    custSearchMessage('載入中…'); custSearchSetControls();
    if (id === 0) {
        custSearchFill({ id: 0, visible: true, isDefault: false });
        return;
    }
    custSearchApi('GetDataOne', { id: id }).done(function (data) {
        if (revision === custSearchState.revision) custSearchFill(data);
    }).fail(function (xhr) {
        if (revision === custSearchState.revision) custSearchMessage(custSearchError(xhr), true);
    });
}
function custSearchOrderCell(container, options) {
    // Fixed system entries have no user-editable order.
    if (!options.data.IsSystem) $(container).text(options.data.SerNO);
}
function custSearchGridReady(e) { custSearchState.grid = e.component; }
function custSearchCanEdit(e) { return !e.row.data.IsSystem; }
function custSearchEdit(e) { if (!e.row.data.IsSystem) location.hash = e.row.data.Id; }
function custSearchDefaultCell(container, options) {
    var row = options.data;
    if (row.IsDefault) custSearchState.defaultId = row.Id;
    $('<input type="radio" name="cust-search-default" class="form-check-input" />')
        .attr({ 'aria-label': '將' + row.Title + '設為預設搜尋', title: !row.Visible ? '請先啟用此搜尋項目' : row.IsAvailable === false ? '目前沒有可用的搜尋範圍' : '設為預設搜尋' })
        .val(row.Id).data({ visible: row.Visible, available: row.IsAvailable !== false }).prop('checked', row.IsDefault)
        .prop('disabled', !custSearchPermission('update') || !row.Visible || row.IsAvailable === false || custSearchState.busy)
        .on('click', function (event) {
            event.stopPropagation();
            // Clicking the automatic fallback must also persist it as the explicit default.
            if (custSearchState.busy || !this.checked) return;
            custSearchState.busy = true; custSearchSetControls();
            custSearchApi('SetDefault', { Id: row.Id }, true).done(function (result) {
                if (!result.success) { custSearchMessage(result.message || '預設搜尋變更失敗。', true); return; }
                custSearchState.defaultId = row.Id;
                custSearchMessage('已將「' + row.Title + '」設為預設搜尋。');
            }).fail(function (xhr) { custSearchMessage('設定預設失敗：' + custSearchError(xhr), true); })
              .always(function () {
                  $('#CustSearchGrid input[name="cust-search-default"]').each(function () {
                      $(this).prop('checked', Number(this.value) === custSearchState.defaultId);
                  });
                  custSearchState.busy = false; custSearchSetControls(); custSearchState.grid?.refresh();
              });
        }).appendTo(container);
}
function custSearchCanDelete(e) { return custSearchPermission('remove') && !e.row.data.IsSystem; }
function custSearchLoadError() { custSearchMessage('搜尋列表載入失敗，請重新整理。', true); }
function custSearchDelete(e) {
    var row = e.row.data;
    if (row.IsSystem || !custSearchPermission('remove') || custSearchState.busy) return;
    Coker.sweet.confirm('刪除搜尋', '確認刪除「' + row.Title + '」？若它是預設項目，將恢復站台自動選擇。', '刪除', '取消', function () {
        if (custSearchState.busy) return;
        custSearchState.busy = true; custSearchSetControls();
        custSearchApi('Delete', { Id: row.Id }, true).done(function (result) {
            if (!result.success) { Coker.sweet.error('刪除失敗', result.message || '請重新載入後再試。'); return; }
            custSearchState.grid?.refresh();
        }).fail(function (xhr) { Coker.sweet.error('刪除失敗', custSearchError(xhr)); })
          .always(function () { custSearchState.busy = false; custSearchSetControls(); });
    });
}
function PageReady() {
    $('#CustSearchAdd').on('click', function () {
        if (custSearchPermission('create') && !custSearchState.busy) { location.hash = '0'; custSearchReadHash(); }
    });
    $('#CustSearchBack').on('click', function () {
        if (custSearchState.busy) return;
        if (!custSearchState.ready || JSON.stringify(custSearchFormData()) === custSearchState.initialValues) { location.hash = ''; return; }
        Coker.sweet.confirm('返回列表', '尚未儲存的修改將不會保留。', '返回', '取消', function () { location.hash = ''; });
    });
    $('#CustSearchAllProd, #CustSearchAllArticle, #CustSearchAllMenu').on('change', custSearchSetControls);
    $('#CustSearchRemoveProductScope').on('click', function () {
        if (!custSearchWritable() || custSearchState.busy) return;
        $('#CustSearchAllProd').prop('checked', false);
        custSearchSetTags('#CustSearchProductTags', [], []);
        custSearchSetControls();
    });
    $(window).on('hashchange.custSearch', custSearchReadHash);
    custSearchApi('GetOptions').done(function (options) {
        custSearchState.options = options;
        ['#CustSearchProductTags', '#CustSearchArticleTags'].forEach(function (id) {
            $(id).data({ tagList: [], tagCheckList: [], tagText: '' }).TagListModalInit();
        });
        $('#CustSearchMenus').dxTagBox({ dataSource: options.menus, valueExpr: 'id', displayExpr: 'title', searchEnabled: true,
            showSelectionControls: true, applyValueMode: 'useButtons', placeholder: '選擇選單…',
            maxDisplayedTags: 2, showClearButton: true, dropDownOptions: { maxHeight: 320, width: Math.min(560, window.innerWidth - 32) } });
        custSearchReadHash();
    }).fail(function (xhr) {
        $('#CustSearchList').addClass('d-none'); $('#CustSearchEditor').removeClass('d-none');
        custSearchMessage(custSearchError(xhr), true);
    });
    $('#CustSearchForm').on('submit', function (event) {
        event.preventDefault();
        if (!custSearchState.ready || custSearchState.busy || !custSearchWritable() || !this.reportValidity()) return;
        var data = custSearchFormData();
        if (!data.SearchAllProd && !data.SearchAllArticle && !data.SearchAllMenu && !data.ProductTagIds.length && !data.ArticleTagIds.length && !data.MenuIds.length) {
            custSearchMessage('請至少設定一種搜尋範圍。', true); return;
        }
        var revision = custSearchState.revision;
        if (revision !== custSearchState.revision || custSearchState.busy) return;
        custSearchState.busy = true; custSearchSetControls();
        custSearchApi('AddUp', data, true).done(function (result) {
            if (!result.success) {
                if (revision === custSearchState.revision) custSearchMessage(result.message || '儲存失敗，請重新載入。', true);
                return;
            }
            custSearchState.grid?.refresh();
            if (revision === custSearchState.revision) { custSearchMessage('已儲存搜尋設定。'); location.hash = ''; }
        }).fail(function (xhr) {
            if (revision === custSearchState.revision) custSearchMessage(custSearchError(xhr), true);
        }).always(function () { custSearchState.busy = false; custSearchSetControls(); });
    });
}
