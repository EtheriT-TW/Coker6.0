// 目錄管理「匯入資料」：#Import（選目錄、上傳、勾欄位）→ #Import_{批次ID}（AI 整理進度 → 預覽確認 → 結果）。
// API 呼叫在 CokerCore/Directory.js 的 co.ArticleImport。畫面一律用切換 class，不寫 style。

const ROW_STATUS = { pending: 0, ready: 1, failed: 2, imported: 3, excluded: 4 };
const TASK_STATUS = { queued: 0, running: 1, succeeded: 2, failed: 3, expired: 4, awaiting: 5, cancelRequested: 6, canceled: 7 };
const ROW_STATUS_VIEW = {
    0: { text: "處理中", css: "text-bg-secondary" },
    1: { text: "成功", css: "text-bg-success" },
    2: { text: "失敗", css: "text-bg-danger" },
    3: { text: "已匯入", css: "text-bg-primary" },
    4: { text: "已取消", css: "text-bg-secondary" }
};
const PAGE_SIZE = 30;
const POLL_MS = 3000;
// 與後端 ArticleImportFieldRules 一致
const MAX_LENGTH = { pagetitle: 150, string: 300, link: 500, textarea: 4000 };

let options = {};
let $root;
let sheetFile = null;
let state = null;      // { batch, included:Set, filter, page }
let pollTimer = null;
let editingRow = null;
let preselectDirectoryId = null; // 從「編輯目錄」按「匯入資料」帶進來的目錄 id
let importDirectory = null;      // 已驗證可匯入的目錄（上傳畫面使用）
let lastDirectoryId = null;      // 最近一次帶進來的目錄（preselect 被消耗後的後備）
let picker = null;               // 預覽列表上展開的選圖面板 { rowId, fieldKey }
let pickerBusy = false;          // 選圖存檔中，避免連點
let cancelReturnDirectoryId = null; // 剛按了取消匯入：確認已取消後回到第一步的目錄
let noticeDirectoryId = null;  // 使用者想進的目錄；若與進行中的批次不同目錄，載入批次後提示

function init(settings) {
    options = settings || {};
    $root = $("#ArticleImport");
    if ($root.length === 0) return;

    $root.on("click", "[data-import-action]", onAction);
    $root.on("click", "[data-import-filter]", function () {
        state.filter = $(this).data("import-filter");
        state.page = 1;
        renderPreview();
    });
    $root.on("change", "[data-row-include]", function () {
        const id = Number($(this).closest("[data-row-id]").data("row-id"));
        if (this.checked) state.included.add(id); else state.included.delete(id);
        renderPreview();
    });
    $root.on("change", "#ArticleImportFile", function () {
        sheetFile = this.files && this.files[0] ? this.files[0] : null;
        showFileName();
        $root.find("[data-import-columns]").addClass("d-none");
        setStep(1);
    });
    $root.on("change", "[data-column-check]", function () {
        $(this).closest(".article-import__column").toggleClass("is-checked", this.checked);
    });
    $("#ArticleImportRowModal").on("click", "[data-import-action='save-row']", SaveRow);
}

function parseRoute(hash) {
    const value = String(hash || "").trim();
    if (/^import$/i.test(value)) {
        return { raw: value, mode: "import", scope: "import", pageKey: "ArticleImport", title: "匯入資料" };
    }
    const match = /^import_(\d+)$/i.exec(value);
    if (match) {
        return { raw: value, mode: "import-batch", scope: "import", pageKey: "ArticleImport", title: "匯入資料", batchId: Number(match[1]) };
    }
    return null;
}

function enterRoute(route) {
    stopPolling();
    $("#TopLine").addClass("d-none");
    if (route.mode === "import-batch") {
        loadBatch(route.batchId);
        return;
    }
    co.ArticleImport.GetCurrent().done(function (current) {
        if (current && current.batchId) {
            // 同一個網站同時只能有一個匯入作業：記下使用者想進的目錄，載入進行中的批次後再提示。
            noticeDirectoryId = preselectDirectoryId;
            preselectDirectoryId = null;
            options.navigate("Import_" + current.batchId);
            return;
        }
        showUpload();
    }).fail(showError);
}

// ───── 步驟 1、2：選目錄、讀欄位、開始 ─────

function showUpload() {
    showPanel("upload");
    setStep(1);
    sheetFile = null;
    $root.find("#ArticleImportFile").val("");
    showFileName();
    $root.find("[data-import-columns]").addClass("d-none");
    importDirectory = null;
    const $directory = $root.find("#ArticleImportDirectory").val("");
    // 路由可能連續觸發兩次，第一次就把 preselect 清掉會讓第二次找不到目錄，所以改用 lastDirectoryId 當後備。
    const requestedId = preselectDirectoryId || lastDirectoryId;
    preselectDirectoryId = null;
    co.ArticleImport.GetDirectories().done(function (list) {
        const target = (list || []).find(function (item) { return item.id === requestedId; });
        if (!target) {
            co.sweet.warn("請從目錄進入", "請先在「編輯目錄」頁面按「匯入資料」。");
            options.backToList();
            return;
        }
        if (target.disabledReason) {
            co.sweet.warn("這個目錄還不能匯入", escapeHtml(target.title + "：" + target.disabledReason + "，請先到目錄設定後再匯入。"));
            options.backToList();
            return;
        }
        importDirectory = target;
        $directory.val(target.title + "｜樣板：" + target.templateName);
    }).fail(showError);
}

function showFileName() {
    $root.find("[data-import-file-name]")
        .text(sheetFile ? sheetFile.name : "沒有選擇檔案")
        .toggleClass("is-empty", !sheetFile);
}

// 從「編輯目錄」按「匯入資料」：帶入目前的目錄。
function open(directoryId) {
    preselectDirectoryId = Number(directoryId) || null;
    if (preselectDirectoryId) lastDirectoryId = preselectDirectoryId;
    options.navigate("Import");
}

function readColumns() {
    if (!importDirectory) return co.sweet.warn("請從目錄進入", "請先在「編輯目錄」頁面按「匯入資料」。");
    if (!sheetFile) return co.sweet.warn("請選擇檔案", "請選擇要匯入的 Excel 檔案（.xlsx）。");
    busy("read", true);
    co.ArticleImport.ReadColumns(sheetFile).done(function (result) {
        const $list = $root.find("[data-import-column-list]").empty();
        const template = document.getElementById("ArticleImportColumnTemplate");
        result.columns.forEach(function (column) {
            const $item = $(template.content.cloneNode(true)).children().first();
            $item.find("[data-column-check]").val(column.name).prop("checked", column.selected);
            $item.toggleClass("is-checked", column.selected);
            $item.find("[data-column-name]").text(column.name);
            $item.find("[data-column-sample]").text(column.sample ? "例：" + column.sample : "（前幾列沒有資料）");
            $list.append($item);
        });
        $root.find("[data-import-column-summary]").text(
            "「" + result.fileName + "」共 " + result.rowCount + " 筆資料、" + result.columns.length +
            " 個欄位；欄名含「不公開」的預設不勾選，不會送給 AI。");
        $root.find("[data-import-columns]").removeClass("d-none");
        setStep(2);
    }).fail(showError).always(function () { busy("read", false); });
}

function startImport() {
    const columns = $root.find("[data-column-check]:checked").map(function () { return this.value; }).get();
    if (columns.length === 0) return co.sweet.warn("請勾選欄位", "請至少勾選一個要匯入的欄位。");
    if (!importDirectory) return co.sweet.warn("請從目錄進入", "請先在「編輯目錄」頁面按「匯入資料」。");
    const directoryId = importDirectory.id;
    busy("start", true);
    co.ArticleImport.Start(directoryId, columns, sheetFile).done(function (result) {
        options.navigate("Import_" + result.batchId);
    }).fail(showError).always(function () { busy("start", false); });
}

// ───── 步驟 3：進度 ─────

function loadBatch(batchId) {
    co.ArticleImport.GetBatch(batchId).done(function (batch) {
        if (!isCurrentBatch(batchId)) return;
        if (noticeDirectoryId && noticeDirectoryId !== batch.directoryId) {
            co.sweet.warn("目前已有匯入作業", escapeHtml("「" + batch.directoryTitle + "」目錄正在匯入中，同一個網站同時只能有一個匯入作業。請先完成或取消後，再匯入其他目錄。"));
        }
        noticeDirectoryId = null;
        const confirmTask = batch.confirmTask;
        const scanTask = batch.task;
        if (confirmTask && isWorking(confirmTask.status)) return showProgress(batch, confirmTask, "正在建立文章", false);
        if (confirmTask) return showResult(batch);
        if (isWorking(scanTask.status) || scanTask.status === TASK_STATUS.cancelRequested)
            return showProgress(batch, scanTask, "AI 整理資料中", true);
        if (scanTask.status === TASK_STATUS.awaiting) return showPreview(batch);
        if (scanTask.status === TASK_STATUS.canceled && cancelReturnDirectoryId === batch.directoryId) {
            return returnToUploadAfterCancel();
        }
        showResult(batch);
    }).fail(function (xhr) {
        showError(xhr);
        options.backToList();
    });
}

function showProgress(batch, task, title, canCancel) {
    showPanel("progress");
    setStep(canCancel ? 3 : 4);
    state = { batch: batch };
    $root.find("[data-import-progress-title]").text(title + "｜" + batch.directoryTitle + "｜" + batch.sourceFileName);
    $root.find("[data-import-progress]").val(task.progress);
    $root.find("[data-import-progress-message]").text(task.message);
    $root.find("[data-import-panel='progress'] [data-import-action='cancel']")
        .toggleClass("d-none", !canCancel)
        .prop("disabled", task.status === TASK_STATUS.cancelRequested);
    pollTimer = setTimeout(function () { loadBatch(batch.id); }, POLL_MS);
}

function cancelImport() {
    co.sweet.confirm("取消這批匯入", "已下載的圖片會移到資源回收桶，不會建立任何文章。", "取消匯入", "返回", function () {
        co.ArticleImport.Cancel(state.batch.id).done(function (result) {
            // 取消完成後直接回到第一步（同一個目錄）；背景還在停止時等輪詢看到「已取消」再回去
            cancelReturnDirectoryId = state.batch.directoryId;
            if (result.finished) {
                returnToUploadAfterCancel();
                return;
            }
            co.sweet.success("正在停止匯入，請稍候。", null, true);
            loadBatch(state.batch.id);
        }).fail(showError);
    });
}

function returnToUploadAfterCancel() {
    const directoryId = cancelReturnDirectoryId;
    cancelReturnDirectoryId = null;
    stopPolling();
    co.sweet.success("已取消匯入", null, true);
    open(directoryId);
}

// ───── 步驟 4：預覽確認 ─────

function showPreview(batch) {
    // 進度頁只有批次資料，首次進入預覽時要初始化勾選與分頁。
    const keep = state?.batch?.id === batch.id && state.included ? state : null;
    state = {
        batch: batch,
        included: keep ? keep.included : new Set(batch.rows.filter(isReady).map(function (r) { return r.id; })),
        filter: keep ? keep.filter : "all",
        page: keep ? keep.page : 1
    };
    showPanel("preview");
    setStep(4);
    $root.find("[data-import-preview-title]").text(
        "匯入到「" + batch.directoryTitle + "」｜樣板：" + batch.templateName + "｜檔案：" + batch.sourceFileName);
    renderPreview();
}

function renderPreview() {
    const rows = state.batch.rows;
    const counts = {
        all: rows.length,
        ready: rows.filter(isReady).length,
        update: rows.filter(function (r) { return isReady(r) && r.matchedArticleId; }).length,
        failed: rows.filter(function (r) { return r.status === ROW_STATUS.failed; }).length,
        excluded: rows.filter(function (r) { return isReady(r) && !state.included.has(r.id); }).length
    };
    const labels = { all: "全部", ready: "成功", update: "將更新", failed: "失敗", excluded: "未勾選" };
    $root.find("[data-import-filter]").each(function () {
        const key = $(this).data("import-filter");
        $(this).text(labels[key] + " " + counts[key]).toggleClass("active", state.filter === key);
    });

    const filtered = rows.filter(matchFilter);
    const pages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    state.page = Math.min(state.page, pages);
    const $list = $root.find("[data-import-rows]").empty();
    filtered.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE).forEach(function (row) {
        $list.append(buildRow(row, true));
    });
    if (filtered.length === 0) $list.append($("<li>").addClass("text-muted py-3 text-center").text("沒有符合的資料"));
    renderPager(pages);

    const selected = rows.filter(function (r) { return isReady(r) && state.included.has(r.id); });
    const updates = selected.filter(function (r) { return r.matchedArticleId; }).length;
    $root.find("[data-import-footer-text]").text(
        "將新增 " + (selected.length - updates) + " 篇、更新 " + updates + " 篇；" +
        counts.excluded + " 筆未勾選、" + counts.failed + " 筆失敗不會匯入。");
    $root.find("[data-import-action='confirm']").prop("disabled", selected.length === 0);
}

function matchFilter(row) {
    switch (state.filter) {
        case "ready": return isReady(row);
        case "update": return isReady(row) && !!row.matchedArticleId;
        case "failed": return row.status === ROW_STATUS.failed;
        case "excluded": return isReady(row) && !state.included.has(row.id);
        default: return true;
    }
}

function renderPager(pages) {
    const $pager = $root.find("[data-import-pager]").empty();
    if (pages <= 1) return;
    for (let page = 1; page <= pages; page++) {
        $("<button type='button' class='btn btn-sm'>")
            .addClass(page === state.page ? "btn-primary" : "btn-outline-secondary")
            .attr("aria-current", page === state.page ? "page" : null)
            .text(page)
            .on("click", function () { state.page = page; renderPreview(); })
            .appendTo($pager);
    }
}

// editable：預覽頁可勾選與編輯；結果頁只顯示。
function buildRow(row, editable) {
    const template = document.getElementById("ArticleImportRowTemplate");
    const $row = $(template.content.cloneNode(true)).children().first().attr("data-row-id", row.id);
    const status = ROW_STATUS_VIEW[row.status] || ROW_STATUS_VIEW[0];
    const included = isReady(row) && state.included && state.included.has(row.id);

    $row.toggleClass("is-failed", row.status === ROW_STATUS.failed)
        .toggleClass("is-excluded", editable ? isReady(row) && !included : row.status === ROW_STATUS.excluded);
    $row.find("[data-row-include]")
        .prop("checked", included)
        .prop("disabled", !isReady(row))
        .attr("aria-label", "匯入第 " + row.rowNumber + " 列");
    $row.find(".article-import__row-check").toggleClass("d-none", !editable);
    $row.find("[data-row-number]").text("第 " + row.rowNumber + " 列");
    $row.find("[data-row-title]").text(row.title || "（沒有標題）");
    $row.find("[data-row-status]").addClass(status.css).text(status.text);
    $row.find("[data-row-update]").toggleClass("d-none", !(editable && row.matchedArticleId));
    $row.find("[data-row-duplicate]").toggleClass("d-none", !(editable && row.duplicateInBatch));
    $row.find("[data-row-summary]").text(summaryText(row));

    const $errors = $row.find("[data-row-errors]");
    if (row.error) $("<li>").text(row.error).appendTo($errors);
    if (editable && row.matchedArticleId && !row.matchedCanEdit) $("<li>").text("同標題文章您沒有編輯權限，確認後會失敗。").appendTo($errors);
    row.images.filter(function (i) { return i.error; }).forEach(function (image) {
        $("<li>").text("圖片（" + image.column + "）：" + image.error).appendTo($errors);
    });

    // 圖片依用途分組顯示（官網橫幅、公司 Logo＋品牌 Logo）；成功的列點群組可直接換圖。
    const canPick = editable && isReady(row) && rowImages(row).length > 0;
    const $media = $row.find("[data-row-images]").addClass("article-import__media-list");
    imageFields().forEach(function (field) {
        $media.append(buildMediaGroup(row, field, canPick));
    });
    if (canPick && picker && picker.rowId === row.id) {
        const field = imageFields().find(function (f) { return f.key === picker.fieldKey; });
        if (field) $row.append(buildPicker(row, field));
    }
    $row.find(".article-import__row-actions").toggleClass("d-none", !(editable && isEditable(row)));
    return $row;
}

// ───── 圖片：分組顯示與列表上直接換圖 ─────

function imageFields() {
    return state.batch.fields.filter(function (f) { return f.type === "image" || f.type === "gallery"; });
}

// 這列下載成功的圖片（依 Excel 順序）。
function rowImages(row) {
    return (row.images || []).filter(function (i) { return i.path; });
}

// 欄位名稱去掉「（800×800，最多 5 張）」這類說明。
function shortLabel(field) {
    const cut = field.label.search(/[（(]/);
    return (cut > 0 ? field.label.slice(0, cut) : field.label).trim();
}

function buildMediaGroup(row, field, canPick) {
    const values = row.fields[field.key] || [];
    const images = values.map(function (path) {
        return rowImages(row).find(function (i) { return i.path === path; });
    }).filter(Boolean);
    const isOpen = !!picker && picker.rowId === row.id && picker.fieldKey === field.key;
    const $group = canPick
        ? $("<button type='button' class='article-import__media is-pickable' data-import-action='pick-image'>")
            .attr("data-field-key", field.key)
            .attr("aria-expanded", isOpen ? "true" : "false")
            .attr("title", "點選更換" + shortLabel(field))
        : $("<div class='article-import__media'>");
    $group.addClass(field.type === "image" ? "is-image" : "is-gallery").toggleClass("is-open", isOpen);

    let label = shortLabel(field);
    if (field.type === "gallery" && field.maxCount) label += " " + images.length + "/" + field.maxCount;
    $("<span class='article-import__media-label'>").text(label).appendTo($group);

    const $thumbs = $("<span class='article-import__media-thumbs'>").appendTo($group);
    if (images.length === 0) {
        $("<span class='article-import__media-empty'>")
            .toggleClass("is-warning", field.type === "image")
            .text(field.type === "image" ? "未設定" : "無")
            .appendTo($thumbs);
    }
    images.forEach(function (image) {
        $("<img class='article-import__thumb' loading='lazy'>")
            .attr("src", image.path).attr("alt", image.fileName).attr("title", image.fileName)
            .appendTo($thumbs);
    });
    return $group;
}

function buildPicker(row, field) {
    const values = row.fields[field.key] || [];
    const $picker = $("<div class='article-import__picker'>");
    const hint = field.type === "gallery"
        ? "可複選" + (field.maxCount ? "，最多 " + field.maxCount + " 張" : "")
        : "點一張圖片設為" + shortLabel(field);
    const $head = $("<div class='article-import__picker-head'>").appendTo($picker);
    $("<span class='small'>").text("選擇" + shortLabel(field) + "（" + hint + "）").appendTo($head);
    $("<button type='button' class='btn btn-sm btn-outline-secondary' data-import-action='close-picker'>").text("完成").appendTo($head);

    const $items = $("<div class='article-import__picker-items'>").appendTo($picker);
    rowImages(row).forEach(function (image) {
        const $item = $("<button type='button' class='article-import__picker-item' data-import-action='toggle-image'>")
            .attr("data-path", image.path)
            .attr("title", image.fileName)
            .attr("aria-pressed", values.indexOf(image.path) >= 0 ? "true" : "false")
            .toggleClass("is-checked", values.indexOf(image.path) >= 0)
            .prop("disabled", pickerBusy);
        $("<img loading='lazy'>").attr("src", image.path).attr("alt", "").appendTo($item);
        $("<span class='article-import__image-name'>").text(image.fileName).appendTo($item);
        $items.append($item);
    });
    return $picker;
}

function togglePicker(rowId, fieldKey) {
    picker = picker && picker.rowId === rowId && picker.fieldKey === fieldKey ? null : { rowId: rowId, fieldKey: fieldKey };
    renderPreview();
}

// 點選圖片後立即存檔（沿用 UpdateRow，後端會再套一次圖片規則）。
function toggleImage(rowId, path) {
    if (pickerBusy || !picker) return;
    const row = state.batch.rows.find(function (r) { return r.id === rowId; });
    const field = row && imageFields().find(function (f) { return f.key === picker.fieldKey; });
    if (!field) return;

    const fields = {};
    Object.keys(row.fields || {}).forEach(function (key) { fields[key] = (row.fields[key] || []).slice(); });
    const current = fields[field.key] || [];
    if (field.type === "image") {
        if (current[0] === path) return;
        fields[field.key] = [path];
    } else if (current.indexOf(path) >= 0) {
        fields[field.key] = current.filter(function (p) { return p !== path; });
    } else {
        if (field.maxCount && current.length >= field.maxCount) {
            co.sweet.warn("已達上限", escapeHtml(shortLabel(field) + "最多 " + field.maxCount + " 張，請先取消其他圖片。"));
            return;
        }
        fields[field.key] = current.concat([path]);
    }

    pickerBusy = true;
    renderPreview();
    co.ArticleImport.UpdateRow(row.id, fields).done(function (detail) {
        const index = state.batch.rows.findIndex(function (r) { return r.id === detail.row.id; });
        state.batch.rows[index] = detail.row;
    }).fail(showError).always(function () {
        pickerBusy = false;
        renderPreview();
    });
}

function summaryText(row) {
    const fields = state.batch.fields.filter(function (f) {
        return f.type !== "pagetitle" && f.type !== "image" && f.type !== "gallery";
    });
    return fields.map(function (f) { return (row.fields[f.key] || []).join("、"); })
        .filter(function (text) { return text; })
        .slice(0, 3)
        .join("｜");
}

function confirmImport() {
    const rows = state.batch.rows;
    const excluded = rows.filter(function (r) { return isReady(r) && !state.included.has(r.id); }).map(function (r) { return r.id; });
    const count = rows.filter(isReady).length - excluded.length;
    co.sweet.confirm("確認匯入",
        "即將在「" + escapeHtml(state.batch.directoryTitle) + "」建立或更新 <b>" + count + "</b> 篇文章，完成後會顯示在前台。",
        "確認匯入", "再看看", function () {
            busy("confirm", true);
            co.ArticleImport.Confirm(state.batch.id, excluded).done(function () {
                loadBatch(state.batch.id);
            }).fail(showError).always(function () { busy("confirm", false); });
        });
}

// ───── 編輯一列 ─────

function openRowEditor(rowId) {
    co.ArticleImport.GetRow(rowId).done(function (detail) {
        editingRow = detail.row;
        const $modal = $("#ArticleImportRowModal");
        $modal.find(".modal-title").text("編輯第 " + detail.row.rowNumber + " 列");
        $modal.find("[data-import-row-error]").addClass("d-none").text("");
        const $form = $modal.find("[data-import-row-form]").empty();
        // 圖片（官網橫幅、Logo）改在預覽列表上直接選，彈窗只編輯文字欄位
        state.batch.fields.filter(function (field) { return field.type !== "image" && field.type !== "gallery"; })
            .forEach(function (field) { $form.append(buildField(field, detail.row)); });
        const $source = $modal.find("[data-import-row-source]").empty();
        Object.keys(detail.source).forEach(function (column) {
            $("<dt>").text(column).appendTo($source);
            $("<dd>").text(detail.source[column]).appendTo($source);
        });
        $modal.modal("show");
    }).fail(showError);
}

function buildField(field, row) {
    const id = "ArticleImportField_" + field.key.replace(/[^A-Za-z0-9_-]/g, "_");
    const values = row.fields[field.key] || [];
    const $group = $("<div class='mb-3'>").attr("data-field-key", field.key).attr("data-field-type", field.type);
    const label = (field.section ? field.section + "｜" : "") + field.label + (field.required || field.type === "pagetitle" ? "（必填）" : "");
    $("<label class='form-label'>").attr("for", id).text(label).appendTo($group);
    const multiline = field.type === "textarea" || field.type === "list";
    const $input = $(multiline ? "<textarea class='form-control'>" : "<input type='text' class='form-control'>")
        .attr("id", id)
        .attr("data-field-input", "")
        .val(field.type === "list" ? values.join("\n") : (values[0] || ""));
    if (multiline) $input.attr("rows", field.type === "list" ? 3 : 5);
    if (MAX_LENGTH[field.type]) $input.attr("maxlength", MAX_LENGTH[field.type]);
    $group.append($input);
    if (field.type === "list") $("<div class='form-text'>").text("一行一個項目").appendTo($group);
    return $group;
}

function SaveRow() {
    const $modal = $("#ArticleImportRowModal");
    // 彈窗沒有圖片欄位，沿用這列目前選的圖片
    const fields = {};
    imageFields().forEach(function (field) {
        fields[field.key] = (editingRow.fields[field.key] || []).slice();
    });
    $modal.find("[data-field-key]").each(function () {
        const $group = $(this);
        const type = $group.data("field-type");
        const text = String($group.find("[data-field-input]").val() || "");
        const values = type === "list" ? text.split(/\r?\n/) : [text];
        fields[$group.data("field-key")] = values.map(function (v) { return v.trim(); }).filter(function (v) { return v; });
    });

    const $save = $modal.find("[data-import-action='save-row']").prop("disabled", true);
    co.ArticleImport.UpdateRow(editingRow.id, fields).done(function (detail) {
        const index = state.batch.rows.findIndex(function (r) { return r.id === detail.row.id; });
        const wasReady = isReady(state.batch.rows[index]);
        state.batch.rows[index] = detail.row;
        if (!wasReady) state.included.add(detail.row.id);
        $modal.modal("hide");
        renderPreview();
    }).fail(function (xhr) {
        $modal.find("[data-import-row-error]").removeClass("d-none").text(errorMessage(xhr));
    }).always(function () { $save.prop("disabled", false); });
}

// ───── 結果 ─────

function showResult(batch) {
    showPanel("result");
    setStep(4);
    state = { batch: batch, included: new Set() };
    const task = batch.confirmTask || batch.task;
    const ok = task.status === TASK_STATUS.succeeded && !!batch.confirmTask;
    const canceled = task.status === TASK_STATUS.canceled;
    const text = task.status === TASK_STATUS.expired ? "這批匯入資料已超過保存期限，請重新匯入。"
        : task.error ? task.message + "：" + task.error
        : task.message;
    $root.find("[data-import-result-alert]")
        .removeClass("alert-success alert-danger alert-secondary")
        .addClass(ok ? "alert-success" : canceled ? "alert-secondary" : "alert-danger")
        .text(text);
    const $list = $root.find("[data-import-result-rows]").empty();
    batch.rows.forEach(function (row) { $list.append(buildRow(row, false)); });
    if (ok && options.refreshDirectories) options.refreshDirectories();
}

// ───── 共用 ─────

function onAction(event) {
    const action = $(event.currentTarget).data("import-action");
    switch (action) {
        case "back": stopPolling(); options.backToList(); break;
        case "read": readColumns(); break;
        case "start": startImport(); break;
        case "select-all": toggleColumns(true); break;
        case "select-none": toggleColumns(false); break;
        case "cancel": cancelImport(); break;
        case "confirm": confirmImport(); break;
        case "edit-row": openRowEditor(Number($(event.currentTarget).closest("[data-row-id]").data("row-id"))); break;
        case "pick-image":
            togglePicker(Number($(event.currentTarget).closest("[data-row-id]").data("row-id")), $(event.currentTarget).attr("data-field-key"));
            break;
        case "toggle-image":
            toggleImage(Number($(event.currentTarget).closest("[data-row-id]").data("row-id")), $(event.currentTarget).attr("data-path"));
            break;
        case "close-picker": picker = null; renderPreview(); break;
        case "new": open(state && state.batch ? state.batch.directoryId : null); break;
    }
}

function toggleColumns(checked) {
    $root.find("[data-column-check]").prop("checked", checked)
        .closest(".article-import__column").toggleClass("is-checked", checked);
}

function showPanel(name) {
    $root.find("[data-import-panel]").addClass("d-none");
    $root.find("[data-import-panel='" + name + "']").removeClass("d-none");
}

function setStep(step) {
    $root.find("[data-import-step]").each(function () {
        const current = Number($(this).data("import-step"));
        $(this).toggleClass("is-active", current === step).toggleClass("is-done", current < step)
            .attr("aria-current", current === step ? "step" : null);
    });
}

function busy(action, isBusy) {
    $root.find("[data-import-action='" + action + "']").prop("disabled", isBusy);
}

function stopPolling() {
    if (pollTimer) clearTimeout(pollTimer);
    pollTimer = null;
}

// 使用者已經離開這批的頁面，就不再更新畫面或輪詢。
function isCurrentBatch(batchId) {
    return String(window.location.hash || "").toLowerCase() === "#import_" + batchId;
}

function isWorking(status) {
    return status === TASK_STATUS.queued || status === TASK_STATUS.running;
}

function isReady(row) {
    return !!row && row.status === ROW_STATUS.ready;
}

function isEditable(row) {
    return row.status === ROW_STATUS.ready || row.status === ROW_STATUS.failed;
}

function errorMessage(xhr) {
    return (xhr && xhr.responseJSON && xhr.responseJSON.message) || "發生未知錯誤，請稍後再試。";
}

function escapeHtml(text) {
    return $("<div>").text(text == null ? "" : String(text)).html();
}

function showError(xhr) {
    co.sweet.error("無法完成", escapeHtml(errorMessage(xhr)));
}

export const ArticleImport = {
    init: init,
    open: open,
    parseRoute: parseRoute,
    enterRoute: enterRoute,
    leave: stopPolling
};
