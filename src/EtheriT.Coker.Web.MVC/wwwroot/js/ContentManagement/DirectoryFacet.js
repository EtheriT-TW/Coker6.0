// DirectoryFacet.js
// Purpose: co.DirectoryFacetModal.open(directoryId) open one shared Bootstrap 5 modal to edit facet ranges
// Requires: jQuery, Bootstrap 5, jQuery UI (sortable), co.sweet.*, co.Directory.* APIs returning ResponseMessageDto

(function (window, $) {
    'use strict';

    // Ensure global namespace
    var co = window.co = window.co || {};
    co.Directory = co.Directory || {};
    co.sweet = co.sweet || {};

    // -------------------------
    // Internal state
    // -------------------------
    var _inited = false;
    var _busy = false;
    var _currentDirectoryId = 0;

    // Cached DOM
    var $modal, $directoryId, $facetType;
    var $calendarAD, $calendarROC;
    var $tbody, $empty;
    var $btnAdd, $btnSortNatural, $btnSave;

    // NEW: wraps for hide/show
    var $calendarWrap, $rangesWrap;

    var rowTpl;

    // Bootstrap modal instance
    var _bsModal = null;

    // -------------------------
    // Helpers: sweet closing
    // -------------------------
    function closeLoading() {
        // Keep it compatible with your wrapper; if you have co.sweet.close use it.
        if (co.sweet.close && typeof co.sweet.close === 'function') {
            co.sweet.close();
            return;
        }
        if (window.Swal && typeof window.Swal.close === 'function') {
            window.Swal.close();
        }
    }

    function setBusy(b) {
        _busy = !!b;
        if (!_inited) return;

        $btnSave.prop('disabled', _busy);
        $btnAdd.prop('disabled', _busy);
        $btnSortNatural.prop('disabled', _busy);
        $facetType.prop('disabled', _busy);

        // calendar radios
        $modal.find('input[name="CalendarType"]').prop('disabled', _busy);

        // ranges inputs/buttons (保險：busy 時也一起鎖)
        if ($rangesWrap && $rangesWrap.length) {
            $rangesWrap.find('input,select,textarea,button').prop('disabled', _busy);
        }
    }

    // -------------------------
    // Helpers: calendar
    // -------------------------
    function rocToAd(rocYear) { return rocYear + 1911; }
    function adToRoc(adYear) { return adYear - 1911; }

    function getCalendarType() {
        return $calendarROC.prop('checked') ? 2 : 1;
    }

    function setCalendarType(t) {
        // accept 1/2 or "1"/"2"
        t = Number(t || 1);
        if (!Number.isFinite(t)) t = 1;
        $calendarROC.prop('checked', t === 2);
        $calendarAD.prop('checked', t !== 2);
    }

    function isNoFacetType(val) {
        var n = Number(val);
        return !Number.isFinite(n) || n === 0; // 0 當作不分類
    }

    function parseYearToAd(val, calendarType) {
        var n = parseInt(String(val || '').trim(), 10);
        if (isNaN(n)) return NaN;
        return (calendarType === 2) ? rocToAd(n) : n;
    }

    function formatAdYear(adYear, calendarType) {
        var n = parseInt(adYear, 10);
        if (isNaN(n)) return '';
        return (calendarType === 2) ? String(adToRoc(n)) : String(n);
    }

    // -------------------------
    // UI helpers
    // -------------------------
    function toggleEmpty() {
        $empty.toggleClass('d-none', $tbody.children('tr').length > 0);
    }

    function renumberSort() {
        $tbody.children('tr').each(function (idx) {
            var order = idx + 1;
            var $tr = $(this);
            $tr.find('[data-role="range-order"]').text(order);
            $tr.find('[data-field="Sort"]').val(order);
        });
    }

    function createRow(range, calendarType) {
        var frag = rowTpl.content.cloneNode(true);
        var $tr = $(frag).find('tr');

        var r = range || {};
        var id = parseInt(r.id ?? r.Id ?? 0, 10) || 0;
        var sort = parseInt(r.sort ?? r.Sort ?? 0, 10) || 0;
        var startAd = parseInt(r.start ?? r.Start ?? 0, 10) || 0;
        var endAd = parseInt(r.end ?? r.End ?? 0, 10) || 0;
        var enabled = (r.enabled != null) ? !!r.enabled : (r.Enabled != null ? !!r.Enabled : true);

        $tr.attr('data-range-id', id);
        $tr.find('[data-field="Id"]').val(id);
        $tr.find('[data-field="Sort"]').val(sort);
        $tr.find('[data-field="Start"]').val(formatAdYear(startAd, calendarType));
        $tr.find('[data-field="End"]').val(formatAdYear(endAd, calendarType));
        $tr.find('[data-field="Enabled"]').prop('checked', enabled);

        return $tr;
    }

    function initSortableOnce() {
        if ($tbody.data('sortable-inited')) return;
        if (!$.fn.sortable) return;

        $tbody.sortable({
            axis: 'y',
            handle: '[data-role="drag-handle"]',
            helper: function (e, tr) {
                var $tr = $(tr);
                var $helper = $tr.clone();
                $helper.children().each(function (i) {
                    $(this).width($tr.children().eq(i).width());
                });
                return $helper;
            },
            update: function () {
                renumberSort();
            }
        });

        $tbody.data('sortable-inited', true);
    }

    // NEW: facetType UI show/hide (0=不分類 => 隱藏年份相關)
    function applyFacetTypeUI() {
        var facetTypeVal = $facetType.val();
        var noFacet = isNoFacetType(facetTypeVal);

        // show/hide by Bootstrap util class
        if ($calendarWrap && $calendarWrap.length) {
            $calendarWrap.toggleClass('d-none', noFacet);
            $calendarWrap.find('input,select,textarea,button').prop('disabled', noFacet || _busy);
        }

        if ($rangesWrap && $rangesWrap.length) {
            $rangesWrap.toggleClass('d-none', noFacet);
            $rangesWrap.find('input,select,textarea,button').prop('disabled', noFacet || _busy);
        }

        // 你想要「不分類就清空區間」的話，打開下面三行
        // if (noFacet) {
        //     $tbody.empty(); toggleEmpty(); renumberSort();
        // }
    }

    function resetUIToEmpty(directoryId) {
        _currentDirectoryId = parseInt(directoryId, 10) || 0;
        $directoryId.val(_currentDirectoryId);

        $facetType.val('0');        // None
        setCalendarType(1);         // default AD
        $tbody.empty();
        toggleEmpty();
        renumberSort();

        applyFacetTypeUI();         // NEW: 套用隱藏/顯示
    }

    function renderConfig(cfg) {
        cfg = cfg || {};

        var dirId = cfg.directoryId ?? cfg.DirectoryId ?? _currentDirectoryId;
        _currentDirectoryId = parseInt(dirId, 10) || _currentDirectoryId || 0;
        $directoryId.val(_currentDirectoryId);

        var facetType = cfg.facetType ?? cfg.FacetType ?? 0;
        $facetType.val(String(parseInt(facetType, 10) || 0));

        // FIX: calendarType 欄位名
        var cal = cfg.calendarType ?? cfg.CalendarType ?? 1;
        setCalendarType(cal);

        var ranges = cfg.ranges ?? cfg.Ranges ?? [];
        if (!Array.isArray(ranges)) ranges = [];

        ranges = ranges
            .filter(function (x) { return x && (x.IsDeleted !== true) && (x.isDeleted !== true); })
            .sort(function (a, b) {
                var sa = parseInt(a.sort ?? a.Sort ?? 0, 10) || 0;
                var sb = parseInt(b.sort ?? b.Sort ?? 0, 10) || 0;
                return sa - sb;
            });

        $tbody.empty();
        var calendarType = getCalendarType();
        ranges.forEach(function (r) {
            $tbody.append(createRow(r, calendarType));
        });

        initSortableOnce();
        renumberSort();
        toggleEmpty();

        applyFacetTypeUI(); // NEW: render 後一定要再套用一次（關鍵）
    }

    function readRangesFromUI() {
        var cal = getCalendarType();
        var out = [];
        $tbody.children('tr').each(function () {
            var $tr = $(this);
            out.push({
                id: parseInt($tr.find('[data-field="Id"]').val(), 10) || 0,
                sort: parseInt($tr.find('[data-field="Sort"]').val(), 10) || 0,
                start: parseYearToAd($tr.find('[data-field="Start"]').val(), cal),
                end: parseYearToAd($tr.find('[data-field="End"]').val(), cal),
                enabled: $tr.find('[data-field="Enabled"]').prop('checked') === true
            });
        });
        return out;
    }

    function validateRanges(ranges) {
        for (var i = 0; i < ranges.length; i++) {
            if (isNaN(ranges[i].start) || isNaN(ranges[i].end)) return '起始/結束 必須輸入有效年份。';
            if (ranges[i].start > ranges[i].end) return '起始年份不可大於結束年份。';
        }

        var sorted = ranges.slice().sort(function (a, b) {
            if (a.start !== b.start) return a.start - b.start;
            return a.end - b.end;
        });

        for (var j = 1; j < sorted.length; j++) {
            if (sorted[j].start <= sorted[j - 1].end) return '分類區間不可重疊（含相交）。';
        }
        return null;
    }

    function buildPayload() {
        renumberSort();

        var facetType = parseInt($facetType.val(), 10) || 0;

        // 建議：不分類就不要送 ranges（避免後端誤以為要處理）
        var payload = {
            directoryId: _currentDirectoryId,
            facetType: facetType,
            calendarType: getCalendarType(),
            ranges: []
        };

        if (!isNoFacetType(facetType)) {
            payload.ranges = readRangesFromUI().map(function (r) {
                return { id: r.id, sort: r.sort, start: r.start, end: r.end, enabled: r.enabled };
            });
        } else {
            payload.calendarType = 1;
            payload.ranges = [];
        }

        return payload;
    }

    function addRange() {
        var thisYear = new Date().getFullYear();
        $tbody.append(createRow({ id: 0, sort: 0, start: thisYear, end: thisYear, enabled: true }, getCalendarType()));
        renumberSort();
        toggleEmpty();
        $tbody.children('tr:last').find('[data-field="Start"]').trigger('focus');
    }

    function sortNatural() {
        var cal = getCalendarType();

        var rows = [];
        $tbody.children('tr').each(function () {
            var $tr = $(this);
            rows.push({
                $tr: $tr,
                start: parseYearToAd($tr.find('[data-field="Start"]').val(), cal),
                end: parseYearToAd($tr.find('[data-field="End"]').val(), cal)
            });
        });

        rows.sort(function (a, b) {
            if (a.start !== b.start) return a.start - b.start;
            return a.end - b.end;
        });

        rows.forEach(function (x) { $tbody.append(x.$tr); });
        renumberSort();
    }

    function convertCalendarInputs(prevType, nextType) {
        $tbody.children('tr').each(function () {
            var $tr = $(this);
            var sAd = parseYearToAd($tr.find('[data-field="Start"]').val(), prevType);
            var eAd = parseYearToAd($tr.find('[data-field="End"]').val(), prevType);
            $tr.find('[data-field="Start"]').val(formatAdYear(sAd, nextType));
            $tr.find('[data-field="End"]').val(formatAdYear(eAd, nextType));
        });
    }

    // -------------------------
    // Bootstrap modal handling (lazy)
    // -------------------------
    function ensureModalInstance() {
        if (_bsModal) return _bsModal;

        var modalEl = document.getElementById('facetModal');
        if (!modalEl) return null;

        _bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
        return _bsModal;
    }

    function showModal() {
        var m = ensureModalInstance();
        if (!m) {
            co.sweet.error && co.sweet.error('錯誤', '找不到 facetModal，請確認 modal HTML 已輸出到頁面。');
            return;
        }
        m.show();
    }

    function hideModal() {
        var modalEl = document.getElementById('facetModal');
        var inst = modalEl ? bootstrap.Modal.getInstance(modalEl) : null;
        if (inst) inst.hide();
    }

    // -------------------------
    // Init / bind
    // -------------------------
    function init() {
        if (_inited) return true;

        $modal = $('#facetModal');
        if ($modal.length === 0) return false;

        $directoryId = $('#facetDirectoryId');
        $facetType = $('#facetType');

        $calendarAD = $('#calendarAD');
        $calendarROC = $('#calendarROC');

        $tbody = $('#facetRangesTbody');
        $empty = $('#facetRangesEmpty');

        $btnAdd = $('#facetBtnAddRange');
        $btnSortNatural = $('#facetBtnSortNatural');
        $btnSave = $('#facetBtnSave');

        // NEW: wraps
        $calendarWrap = $('#facetCalendarTypeWrap');
        $rangesWrap = $('#facetRangesWrap');

        rowTpl = document.getElementById('facetRangeRowTemplate');
        if (!rowTpl) {
            // Do not throw (avoid killing module); show sweet error once used.
            return false;
        }

        initSortableOnce();

        // bind once
        $tbody.on('click', '[data-action="remove-range"]', function () {
            $(this).closest('tr').remove();
            renumberSort();
            toggleEmpty();
        });

        $btnAdd.on('click', function (e) {
            e.preventDefault();
            if (_busy) return;
            addRange();
        });

        $btnSortNatural.on('click', function (e) {
            e.preventDefault();
            if (_busy) return;
            sortNatural();
        });

        $btnSave.on('click', function (e) {
            e.preventDefault();
            if (_busy) return;
            api.save();
        });

        // NEW: facetType change => hide/show
        $facetType.on('change', function () {
            applyFacetTypeUI();
        });

        $modal.on('change', 'input[name="CalendarType"]', function () {
            var next = getCalendarType();
            var prev = (next === 2) ? 1 : 2;
            convertCalendarInputs(prev, next);
        });

        _inited = true;

        // initial apply (in case modal html has default)
        applyFacetTypeUI();

        return true;
    }

    // -------------------------
    // Public API
    // -------------------------
    var api = {
        init: function () {
            return init();
        },

        open: function (directoryId) {
            // lazy init on first use
            if (!init()) {
                co.sweet.error && co.sweet.error('錯誤', 'Facet modal 初始化失敗：請確認 facetModal 與 facetRangeRowTemplate 已存在。');
                return;
            }

            if (!directoryId) {
                co.sweet.error && co.sweet.error('錯誤', 'DirectoryId 不正確');
                return;
            }

            resetUIToEmpty(directoryId);

            // show modal first (better UX), then load
            showModal();

            // loading
            co.sweet.loading && co.sweet.loading();
            setBusy(true);

            co.Directory.GetDirectoryFacetConfig(directoryId)
                .done(function (resp) {
                    closeLoading();
                    setBusy(false);

                    // ResponseMessageDto
                    if (!resp || resp.success !== true) {
                        co.sweet.error && co.sweet.error('讀取失敗', (resp && (resp.error || resp.message)) || '讀取目錄設定失敗');
                        // keep empty UI for editing
                        applyFacetTypeUI();
                        return;
                    }

                    // FIX: object/Object 兼容
                    var data = resp.object ?? resp.Object;
                    if (data) {
                        renderConfig(data);
                    } else {
                        // keep empty
                        applyFacetTypeUI();
                    }
                })
                .fail(function () {
                    closeLoading();
                    setBusy(false);
                    co.sweet.error && co.sweet.error('讀取失敗', '網路連線或伺服器錯誤');
                    applyFacetTypeUI();
                });
        },

        save: function () {
            if (!init()) {
                co.sweet.error && co.sweet.error('錯誤', 'Facet modal 初始化失敗：請確認 facetModal 與 facetRangeRowTemplate 已存在。');
                return;
            }

            var payload = buildPayload();

            // validate only when facetType == Year(1); if you only have None/Year, this is safe
            if ((payload.facetType | 0) === 1) {
                var err = validateRanges(payload.ranges);
                if (err) {
                    co.sweet.error && co.sweet.error('資料錯誤', err);
                    return;
                }
            }

            co.sweet.loading && co.sweet.loading();
            setBusy(true);

            co.Directory.SaveDirectoryFacetConfig(payload)
                .done(function (resp) {
                    closeLoading();
                    setBusy(false);

                    if (!resp || resp.success !== true) {
                        co.sweet.error && co.sweet.error('儲存失敗', (resp && (resp.error || resp.message)) || '儲存設定失敗');
                        applyFacetTypeUI();
                        return;
                    }

                    // success
                    co.sweet.success && co.sweet.success('儲存成功');

                    // if server returns latest Object, refresh UI (for new Ids)
                    var data = resp.object ?? resp.Object;
                    if (data) {
                        renderConfig(data);
                    }

                    hideModal();
                })
                .fail(function () {
                    closeLoading();
                    setBusy(false);
                    co.sweet.error && co.sweet.error('儲存失敗', '網路連線或伺服器錯誤');
                    applyFacetTypeUI();
                });
        },

        getPayload: function () {
            if (!init()) return null;
            return buildPayload();
        }
    };

    // Export to co
    co.DirectoryFacetModal = api;

})(window, jQuery);