var $display, $removedFromShelves, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count,
    $marks, $price, $subItemNo, $stock_number, $packingPoint_number, $alert_number, $min_number, $date, $picker, $permanent,
    $itemNo, $itemNo_count, $noStockManagement;
var $popularVisible;
var $popularValue;
var startDate, endDate, keyId, price_tid, temp_psid;
var specDescModal, $spec_desc_input, $currentSpecDescRow = null;
var productTagFilter = null;
var productTagOptions = [];
var productTagOptionsPromise = null;
var product_list, spec_num = 0, spec_price_num = 0, spec_remove_list = [], modal_price_list = [], spec_pick_list, suggest_price_list = []
var $price_modal, priceModal
var total_files = [];
var spec_media_map = {};
var specMediaModal;
let importProdPopup = null;
let productImportTemplateGrid = null;
let selectedProductImportTemplateId = null;
let pendingProductImportTaskId = null;
let pendingProductImportAnalysis = null;
let productImportAnalysisErrorGrid = null;
let productImportAnalysisErrorRows = [];
let pendingProductImportIgnoredRows = [];
let productImportConfirmInProgress = false;
var elementReady = false;
var pendingHashEdit = false;
var lastProductImportInfoLoaded = false;

async function loadLastProductImportInfo(forceReload) {
    if (lastProductImportInfoLoaded && !forceReload) return;
    lastProductImportInfoLoaded = true;
    try {
        const response = await fetch("/api/Product/GetLastProductImport", {
            method: "GET",
            headers: _c.Data.Header,
            credentials: "same-origin",
            cache: "no-store"
        });
        if (!response.ok) throw new Error("無法取得最後匯入時間");
        const result = await response.json();
        const $info = $("#LastProductImportInfo").removeClass("d-none");
        if (!result.hasImport || !result.completionTime) {
            $info.text("商品資料尚無成功匯入紀錄。");
            return;
        }
        const completionTime = new Date(result.completionTime);
        $info.text("最後成功匯入：" + completionTime.toLocaleString("zh-TW", { hour12: false }));
        if (result.message) $info.attr("title", result.message);
    } catch (error) {
        lastProductImportInfoLoaded = false;
        console.error(error);
    }
}

function getCurrentProductImportForm() {
    if (importProdPopup && typeof importProdPopup.content === "function") {
        const popupForm = $(importProdPopup.content()).find(`form[name="fileUploadForm"]`)[0];
        if (popupForm) return popupForm;
    }

    return $(`form[name="fileUploadForm"]:visible`)[0]
        || $(`form[name="fileUploadForm"]`)[0]
        || null;
}

function ImportProd() {
    const form = getCurrentProductImportForm();
    if (!form) {
        co.sweet.error("找不到目前的商品匯入表單，請關閉視窗後重新開啟。");
        return;
    }
    const fileInput = $(form).find(`[name="files"]`)[0];
    if (!selectedProductImportTemplateId) {
        co.sweet.error("請先選擇商品匯入版型。");
        return;
    }
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        co.sweet.error("請選擇欲匯入的 Excel 檔案。");
        return;
    }

    var formData = new FormData(form);
    formData.append("templateId", selectedProductImportTemplateId);
    const $submitButton = $("#btnStartProductImport").prop("disabled", true);
    importProdPopup.hide();
    Swal.fire({
        title: "正在掃描商品匯入檔",
        html: "<div style=\"margin-bottom:12px;\">正在上傳檔案並建立掃描任務…</div>",
        allowOutsideClick: false,
        allowEscapeKey: true,
        showCloseButton: true,
        showConfirmButton: false,
        didOpen: function () { Swal.showLoading(); }
    });
    co.Product.AddUp.Import(formData).done(async function (response) {
        try {
            const status = await waitForProductTask(response.taskId, "正在掃描商品匯入檔");
            Swal.close();
            pendingProductImportTaskId = response.taskId;
            pendingProductImportAnalysis = parseProductImportAnalysis(status.resultJson);
            renderProductImportAnalysis(pendingProductImportAnalysis);
            showProductImportStep(3);
            importProdPopup.show();
        } catch (error) {
            Swal.close();
            importProdPopup.show();
            if (error && Array.isArray(error.importErrors) && error.importErrors.length > 0) {
                showProductImportErrors(error.importErrors, true);
            } else {
                co.sweet.error(error && error.message ? error.message : "商品匯入失敗。");
            }
        }
    }).fail(function (xhr) {
        Swal.close();
        importProdPopup.show();
        var message = xhr.responseJSON && xhr.responseJSON.message
            ? xhr.responseJSON.message
            : "檔案格式錯誤，無法建立匯入任務。";
        co.sweet.error(message);
    }).always(function () {
        $submitButton.prop("disabled", false);
    });
}

function updateProductImportStartButton() {
    const form = getCurrentProductImportForm();
    const fileInput = form ? $(form).find(`[name="files"]`)[0] : null;
    const hasFile = !!(fileInput && fileInput.files && fileInput.files.length > 0);
    $("#btnStartProductImport").prop("disabled", !selectedProductImportTemplateId || !hasFile);
}

function showProductImportStep(step) {
    const selectingTemplate = step === 1;
    const selectingFile = step === 2;
    const confirming = step === 3;
    $("#productImportStepTemplate").toggleClass("d-none", !selectingTemplate);
    $("#productImportStepFile").toggleClass("d-none", !selectingFile);
    $("#productImportStepConfirm")
        .toggleClass("d-none", !confirming)
        .toggleClass("d-flex", confirming);
    if (importProdPopup) importProdPopup.option("height", selectingTemplate ? 620 : (confirming ? 720 : 420));
    if (selectingTemplate && productImportTemplateGrid) {
        window.setTimeout(function () {
            productImportTemplateGrid.updateDimensions();
        }, 0);
    }
}

function goToProductImportFileStep() {
    if (!selectedProductImportTemplateId) {
        co.sweet.error("請先選擇商品匯入版型。");
        return;
    }

    const selectedRows = productImportTemplateGrid
        ? productImportTemplateGrid.getSelectedRowsData()
        : [];
    $("#selectedProductImportTemplateName").text(
        selectedRows.length > 0 ? selectedRows[0].title : "已選擇版型"
    );
    showProductImportStep(2);
    updateProductImportStartButton();
}

function showProductImportImagePreview(title, imageUrl) {
    if (!imageUrl) return;

    const preview = $("<div>").css({
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        maxHeight: "70vh",
        overflow: "hidden"
    });
    $("<img>").attr({ src: imageUrl, alt: title || "版型預覽" }).css({
        maxWidth: "100%",
        maxHeight: "70vh",
        objectFit: "contain"
    }).appendTo(preview);

    Swal.fire({
        title: title || "版型預覽",
        html: preview[0],
        width: "min(90vw, 1000px)",
        showConfirmButton: false,
        showCloseButton: true
    });
}

function normalizeProductImportTemplate(item) {
    const icon = String(item.icon || item.Icon || "").trim();
    return {
        id: item.id !== undefined ? item.id : item.Id,
        title: item.title || item.Title || "未命名版型",
        imageUrl: item.img || item.Img || item.imgUrl || item.ImgUrl || item.imageUrl || item.ImageUrl || "",
        icon: icon.toLowerCase() === "empty" ? "" : icon
    };
}

function initializeProductImportTemplateGrid() {
    const $grid = $("#productImportTemplateGrid");
    if ($grid.length === 0) return;

    selectedProductImportTemplateId = null;
    pendingProductImportTaskId = null;
    pendingProductImportAnalysis = null;
    productImportAnalysisErrorRows = [];
    pendingProductImportIgnoredRows = [];
    productImportConfirmInProgress = false;
    showProductImportStep(1);
    $("#btnNextProductImportStep").prop("disabled", true);
    $("#btnStartProductImport").prop("disabled", true);
    const form = getCurrentProductImportForm();
    if (form) form.reset();
    $(form).find(`[name="files"]`)
        .off("change.productImport")
        .on("change.productImport", updateProductImportStartButton);

    if ($grid.hasClass("dx-widget")) {
        productImportTemplateGrid = $grid.dxDataGrid("instance");
        productImportTemplateGrid.clearSelection();
        productImportTemplateGrid.option("dataSource", []);
    } else {
        productImportTemplateGrid = $grid.dxDataGrid({
            dataSource: [],
            keyExpr: "id",
            height: 330,
            showBorders: true,
            rowAlternationEnabled: true,
            hoverStateEnabled: true,
            wordWrapEnabled: true,
            selection: { mode: "single" },
            focusedRowEnabled: true,
            searchPanel: {
                visible: true,
                width: 260,
                placeholder: "搜尋版型名稱"
            },
            paging: { pageSize: 8 },
            pager: {
                visible: true,
                showInfo: true,
                showNavigationButtons: true
            },
            noDataText: "尚無商品匯入版型，請先至元件目錄設定元件用途。",
            columns: [
                {
                    caption: "請選擇",
                    width: 82,
                    alignment: "center",
                    allowFiltering: false,
                    allowSorting: false,
                    cellTemplate: function (container, options) {
                        $("<input>").attr({
                            type: "radio",
                            name: "productImportTemplateChoice",
                            "aria-label": "選擇「" + options.data.title + "」版型"
                        }).prop("checked", options.component.isRowSelected(options.key))
                            .on("change", function () {
                                options.component.selectRows([options.key], false);
                            })
                            .appendTo(container);
                    }
                },
                {
                    caption: "預覽",
                    width: 110,
                    alignment: "center",
                    allowFiltering: false,
                    allowSorting: false,
                    cellTemplate: function (container, options) {
                        if (options.data.imageUrl) {
                            $("<button>").attr({
                                type: "button",
                                title: "點擊放大預覽"
                            }).addClass("btn btn-link border-0 p-0")
                                .on("click", function (event) {
                                    event.stopPropagation();
                                    showProductImportImagePreview(options.data.title, options.data.imageUrl);
                                })
                                .append(
                                    $("<img>").attr({ src: options.data.imageUrl, alt: options.data.title }).css({
                                        width: "78px",
                                        height: "52px",
                                        objectFit: "cover",
                                        borderRadius: "4px"
                                    })
                                ).appendTo(container);
                        } else if (options.data.icon) {
                            const icon = options.data.icon;
                            const $icon = $("<i>").addClass(icon);
                            const materialClass = icon.match(/\bmaterial-(?:symbols|icons)[\w-]*\b/);
                            if (materialClass) {
                                $icon.text(icon.replace(materialClass[0], "").trim());
                            }
                            $icon.css("font-size", "28px").appendTo(container);
                        } else {
                            $("<span>").text("無預覽").addClass("text-black-50 small").appendTo(container);
                        }
                    }
                },
                { dataField: "title", caption: "版型名稱", minWidth: 220 }
            ],
            onRowClick: function (e) {
                if (e.rowType === "data") e.component.selectRows([e.key], false);
            },
            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.css({ cursor: "pointer", height: "64px" });
                }
            },
            onCellPrepared: function (e) {
                if (e.rowType === "data") {
                    e.cellElement.css("vertical-align", "middle");
                }
            },
            onSelectionChanged: function (e) {
                const selected = e.selectedRowsData[0];
                selectedProductImportTemplateId = selected ? selected.id : null;
                $("#btnNextProductImportStep").prop("disabled", !selectedProductImportTemplateId);
                e.component.repaint();
            }
        }).dxDataGrid("instance");
    }

    co.HtmlContent.GetComponentsByPurpose("product-import-directory")
        .done(function (response) {
            if (!response.success) {
                co.sweet.error(response.error || "無法取得商品匯入版型。");
                return;
            }
            const templates = (response.list || response.List || []).map(normalizeProductImportTemplate);
            productImportTemplateGrid.option("dataSource", templates);
            if (templates.length > 0) {
                productImportTemplateGrid.selectRows([templates[0].id], false);
            }
        })
        .fail(function () {
            co.sweet.error("無法取得商品匯入版型。");
        });
}

function showImportProdPopup() {
    pendingProductImportTaskId = null;
    pendingProductImportAnalysis = null;
    pendingProductImportIgnoredRows = [];
    productImportAnalysisErrorGrid = null;
    productImportAnalysisErrorRows = [];
    productImportConfirmInProgress = false;
    importProdPopup = $("#importProdPopup").dxPopup("instance");
    importProdPopup.option("contentTemplate", $("#importProdPopup-template"));
    importProdPopup.option("title", "商品資料匯入");
    importProdPopup.option("onShown", function () {
        if (!pendingProductImportTaskId) initializeProductImportTemplateGrid();
    });
    importProdPopup.show();
}

function parseProductImportAnalysis(resultJson) {
    if (!resultJson) return { canImport: false, errors: [], differences: [], summary: null };
    const result = typeof resultJson === "string" ? JSON.parse(resultJson) : resultJson;
    return {
        canImport: result.CanImport !== undefined ? result.CanImport : result.canImport,
        errors: result.Errors || result.errors || [],
        differences: result.Differences || result.differences || [],
        summary: result.Summary || result.summary || null
    };
}

function getAnalysisValue(item, pascalName, camelName) {
    return item && item[pascalName] !== undefined ? item[pascalName] : (item ? item[camelName] : "");
}

function getVisibleCharacter(character) {
    const visibleCharacters = {
        " ": { text: "␠", name: "半形空白", code: "U+0020" },
        "\u00a0": { text: "⍽", name: "不換行空白", code: "U+00A0" },
        "\u3000": { text: "□", name: "全形空白", code: "U+3000" },
        "\t": { text: "⇥", name: "Tab", code: "U+0009" },
        "\r": { text: "↵", name: "CR 換行", code: "U+000D" },
        "\n": { text: "↵", name: "LF 換行", code: "U+000A" },
        "\u200b": { text: "[ZWSP]", name: "零寬空白", code: "U+200B" },
        "\u200c": { text: "[ZWNJ]", name: "零寬非連字元", code: "U+200C" },
        "\u200d": { text: "[ZWJ]", name: "零寬連字元", code: "U+200D" },
        "\ufeff": { text: "[BOM]", name: "BOM／零寬不換行空白", code: "U+FEFF" }
    };
    if (visibleCharacters[character]) return visibleCharacters[character];
    const codePoint = character.codePointAt(0).toString(16).toUpperCase().padStart(4, "0");
    return { text: character, name: "字元", code: "U+" + codePoint };
}

function getCharacterDiff(leftValue, rightValue) {
    const left = Array.from(leftValue || "");
    const right = Array.from(rightValue || "");
    let prefixLength = 0;
    while (prefixLength < left.length
        && prefixLength < right.length
        && left[prefixLength] === right[prefixLength]) {
        prefixLength++;
    }
    let suffixLength = 0;
    while (suffixLength < left.length - prefixLength
        && suffixLength < right.length - prefixLength
        && left[left.length - 1 - suffixLength] === right[right.length - 1 - suffixLength]) {
        suffixLength++;
    }
    const leftChanged = left.map(function (_, index) {
        return index >= prefixLength && index < left.length - suffixLength;
    });
    const rightChanged = right.map(function (_, index) {
        return index >= prefixLength && index < right.length - suffixLength;
    });
    return { left: left, right: right, leftChanged: leftChanged, rightChanged: rightChanged };
}

function getTextSimilarity(leftValue, rightValue) {
    const left = Array.from((leftValue || "").toLocaleLowerCase().replace(/\s+/g, " ").trim());
    const right = Array.from((rightValue || "").toLocaleLowerCase().replace(/\s+/g, " ").trim());
    if (left.length === 0 || right.length === 0) return left.length === right.length ? 1 : 0;
    let previous = new Array(right.length + 1).fill(0);
    for (let leftIndex = 1; leftIndex <= left.length; leftIndex++) {
        const current = new Array(right.length + 1).fill(0);
        for (let rightIndex = 1; rightIndex <= right.length; rightIndex++) {
            current[rightIndex] = left[leftIndex - 1] === right[rightIndex - 1]
                ? previous[rightIndex - 1] + 1
                : Math.max(previous[rightIndex], current[rightIndex - 1]);
        }
        previous = current;
    }
    return (2 * previous[right.length]) / (left.length + right.length);
}

function appendMarkedCharacters($container, characters, changedCharacters) {
    let unchangedText = "";
    let changedText = "";
    let changedDetails = [];
    function flushUnchangedText() {
        if (!unchangedText) return;
        $container.append(document.createTextNode(unchangedText));
        unchangedText = "";
    }
    function flushChangedText() {
        if (!changedText) return;
        $("<mark>")
            .addClass("px-1 rounded bg-warning text-dark")
            .attr("title", "差異區段；包含：" + changedDetails.filter(function (item, index, items) {
                return items.indexOf(item) === index;
            }).join("、"))
            .text(changedText)
            .appendTo($container);
        changedText = "";
        changedDetails = [];
    }
    characters.forEach(function (character, index) {
        if (!changedCharacters[index]) {
            flushChangedText();
            unchangedText += character;
            return;
        }
        flushUnchangedText();
        const visible = getVisibleCharacter(character);
        changedText += visible.text;
        changedDetails.push(visible.name + "（" + visible.code + "）");
    });
    flushChangedText();
    flushUnchangedText();
    if (characters.length === 0) {
        $("<mark>").addClass("px-1 rounded bg-warning text-dark").text("[空字串]").appendTo($container);
    }
}

function appendCharacterComparison($container, leftLabel, leftValue, rightLabel, rightValue) {
    const shouldMarkDifference = getTextSimilarity(leftValue, rightValue) >= 0.55;
    const difference = shouldMarkDifference ? getCharacterDiff(leftValue, rightValue) : null;
    const $left = $("<div>").addClass("mb-1").appendTo($container);
    $("<span>").addClass("fw-bold me-1").text(leftLabel + "：").appendTo($left);
    if (shouldMarkDifference) {
        appendMarkedCharacters($left, difference.left, difference.leftChanged);
    } else {
        $left.append(document.createTextNode(leftValue || "[空字串]"));
    }
    const $right = $("<div>").appendTo($container);
    $("<span>").addClass("fw-bold me-1").text(rightLabel + "：").appendTo($right);
    if (shouldMarkDifference) {
        appendMarkedCharacters($right, difference.right, difference.rightChanged);
    } else {
        $right.append(document.createTextNode(rightValue || "[空字串]"));
        $("<div>")
            .addClass("text-muted mt-1")
            .text("兩側內容差異較大，直接顯示完整內容，不標示個別字元。")
            .appendTo($container);
    }
}

function appendExcelConflictComparisons($container, comparisonValues) {
    const groupedValues = [];
    (comparisonValues || []).forEach(function (item) {
        const value = getAnalysisValue(item, "Value", "value") || "";
        const label = getAnalysisValue(item, "Label", "label") || "內容";
        const rowNumber = Number(getAnalysisValue(item, "RowNumber", "rowNumber"));
        let group = groupedValues.find(function (entry) {
            return entry.value === value && entry.label === label;
        });
        if (!group) {
            group = { value: value, label: label, rowNumbers: [] };
            groupedValues.push(group);
        }
        if (rowNumber > 0 && group.rowNumbers.indexOf(rowNumber) < 0) group.rowNumbers.push(rowNumber);
    });
    if (groupedValues.length < 2) return;

    const reference = groupedValues[0];
    groupedValues.slice(1).forEach(function (other, index) {
        const $comparison = $("<div>")
            .addClass("mt-2 pt-2 border-top small")
            .appendTo($container);
        appendCharacterComparison(
            $comparison,
            "第 " + reference.rowNumbers.sort(function (a, b) { return a - b; }).join("、") + " 列 " + reference.label,
            reference.value,
            "第 " + other.rowNumbers.sort(function (a, b) { return a - b; }).join("、") + " 列 " + other.label,
            other.value);
    });
}

function renderProductImportAnalysis(analysis) {
    const $summary = $("#productImportAnalysisSummary").empty();
    const $content = $("#productImportAnalysisContent").empty();
    appendProductImportSummary($summary, analysis.summary);
    if (pendingProductImportIgnoredRows.length > 0) {
        $("<div>").addClass("alert alert-secondary py-2")
            .text("目前已選擇忽略 " + pendingProductImportIgnoredRows.length + " 個 Excel 資料列；這些列不會正式匯入。")
            .appendTo($summary);
    }

    const errors = analysis.errors || [];
    const differences = analysis.differences || [];
    if (errors.length > 0) {
        $("<div>").addClass("alert alert-danger py-2")
            .text("Excel 內有 " + errors.length + " 筆資料衝突。請修正後重新掃描；若確認不匯入衝突所涉及的整列資料，也可以選取『忽略』後繼續。")
            .appendTo($content);
        const gridData = errors.map(function (item, errorIndex) {
            const rowNumbers = getAnalysisValue(item, "RowNumbers", "rowNumbers") || [];
            const sortedRows = rowNumbers.slice().sort(function (a, b) { return a - b; });
            return {
                id: "error:" + errorIndex,
                sheet: getAnalysisValue(item, "Sheet", "sheet") || "-",
                rowNumbers: sortedRows,
                rowsText: sortedRows.length === 0
                    ? "-"
                    : sortedRows.join("、") + (sortedRows.length > 1 ? "（共 " + sortedRows.length + " 列）" : ""),
                name: getAnalysisValue(item, "Name", "name") || "資料衝突",
                description: getAnalysisValue(item, "Description", "description") || "-",
                comparisonValues: getAnalysisValue(item, "ComparisonValues", "comparisonValues") || [],
                canIgnore: !!getAnalysisValue(item, "CanIgnore", "canIgnore") && sortedRows.length > 0,
                ignore: false
            };
        });
        // 額外保存完整資料集合，避免 DevExpress 分頁後只取得目前載入的頁面。
        productImportAnalysisErrorRows = gridData;
        const $grid = $("<div>").addClass("mb-3").appendTo($content);
        productImportAnalysisErrorGrid = $grid.dxDataGrid({
            dataSource: gridData,
            keyExpr: "id",
            height: 330,
            showBorders: true,
            rowAlternationEnabled: true,
            wordWrapEnabled: true,
            filterRow: { visible: true },
            searchPanel: { visible: true, width: 240, placeholder: "搜尋工作表、列號或原因" },
            paging: { pageSize: 10 },
            pager: {
                visible: true,
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                showNavigationButtons: true
            },
            columns: [
                { dataField: "sheet", caption: "工作表", width: 100 },
                { dataField: "rowsText", caption: "Excel 列號", width: 120 },
                { dataField: "name", caption: "資料位置", width: 250 },
                {
                    dataField: "description",
                    caption: "衝突原因與字元差異",
                    cellTemplate: function (container, options) {
                        $("<div>").text(options.data.description).appendTo(container);
                        appendExcelConflictComparisons($(container), options.data.comparisonValues);
                    }
                },
                {
                    dataField: "ignore",
                    caption: "忽略此組",
                    width: 100,
                    alignment: "center",
                    allowFiltering: false,
                    cellTemplate: function (container, options) {
                        $("<div>").dxCheckBox({
                            value: options.data.ignore,
                            disabled: !options.data.canIgnore,
                            hint: options.data.canIgnore
                                ? "勾選後，此組衝突涉及的所有 Excel 資料列都不會匯入"
                                : "此結構錯誤不可忽略，必須修改 Excel",
                            onValueChanged: function (e) {
                                options.data.ignore = e.value;
                                updateProductImportConfirmButton();
                            }
                        }).appendTo(container);
                    }
                }
            ],
            onRowPrepared: function (e) {
                if (e.rowType === "data" && !e.data.canIgnore) e.rowElement.css("background", "#f8d7da");
            },
            onToolbarPreparing: function (e) {
                e.toolbarOptions.items.unshift({
                    location: "after",
                    widget: "dxButton",
                    options: {
                        text: "全部標記忽略",
                        hint: "將所有可忽略的 Excel 衝突列標記為不匯入",
                        onClick: function () {
                            productImportAnalysisErrorRows.forEach(function (row) {
                                if (row.canIgnore) row.ignore = true;
                            });
                            e.component.repaint();
                            updateProductImportConfirmButton();
                        }
                    }
                });
                e.toolbarOptions.items.unshift({
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "fa-solid fa-file-excel",
                        text: "下載衝突清單",
                        onClick: function () {
                            CokerDxGridExport({ component: e.component, format: "xlsx", cancel: false }, {
                                fileName: "ProductImportScanErrors",
                                worksheetName: "商品匯入掃描衝突"
                            });
                        }
                    }
                });
            }
            }).dxDataGrid("instance");
        $("<div>").addClass("form-text mb-3")
            .text("每筆代表一組衝突，Excel 列號會列出該組涉及的所有資料列；只有內容相近時才以黃色標示差異區段，內容差異較大時會直接顯示完整值。勾選「忽略此組」後，該組涉及的列不會寫入商品、目錄或技術證照。必須處理全部衝突後才能繼續。")
            .appendTo($content);
    } else {
        productImportAnalysisErrorRows = [];
    }

    if (differences.length > 0) {
        const $differenceAlert = $("<div>").addClass("alert alert-warning py-2")
            .appendTo($content);
        const optionDefinitions = [
            { code: "product-name", id: "confirmOverwriteProductNames", text: "允許以 Excel 更新既有商品名稱", showDetails: true },
            { code: "product-spec", id: "confirmOverwriteProductSpecs", text: "允許以 Excel 更新 SubItemNo 對應的既有規格", showDetails: true },
            { code: "product-price", id: "confirmOverwriteProductPrices", text: "允許以 Excel 更新既有商品價格", showDetails: true },
            { code: "technical-certificate", id: "confirmOverwriteTechnicalCertificates", text: "允許以 Excel 覆蓋既有技術證照內容", showDetails: true },
            { code: "duplicate-menu-title", id: "confirmAllowDuplicateMenuTitles", text: "RouterName 不同時，允許建立同名選單", showDetails: true },
            { code: "menu-parent", id: "confirmOverwriteMenuParents", text: "允許依 Excel 搬移既有選單父層", showDetails: true },
            {
                code: "directory-page",
                id: "confirmOverwriteDirectoryPages",
                text: "將版型重新套用到本次涉及的既有商品目錄頁",
                helpText: "未勾選時會完整保留既有目錄頁；只有勾選後才會重新套用版型。",
                countsAsUnresolved: false,
                showDetails: false
            }
        ];
        const renderedOptions = [];
        optionDefinitions.forEach(function (option) {
            const count = differences.filter(function (item) {
                return getAnalysisValue(item, "Code", "code") === option.code;
            }).length;
            if (count === 0) return;
            const $check = $("<div>").addClass("form-check mb-2").appendTo($content);
            const $input = $("<input>")
                .attr({ type: "checkbox", id: option.id })
                .addClass("form-check-input")
                .appendTo($check);
            $("<label>").attr("for", option.id).addClass("form-check-label fw-bold")
                .text(option.text + "（" + count + " 筆）")
                .appendTo($check);
            if (option.helpText) {
                $("<div>").addClass("form-text").text(option.helpText).appendTo($check);
            }
            renderedOptions.push({ definition: option, count: count, input: $input });
        });

        const differenceDisplayDefinitions = {
            "product-name": {
                type: "商品名稱不同",
                existingLabel: "資料庫商品名稱",
                excelLabel: "Excel 商品名稱",
                markCharacters: true
            },
            "product-spec": {
                type: "同一 SubItemNo 的規格不同",
                existingLabel: "資料庫規格",
                excelLabel: "Excel 規格",
                markCharacters: true
            },
            "product-price": {
                type: "商品價格異動",
                existingLabel: "目前價格",
                excelLabel: "匯入後價格",
                markCharacters: false
            },
            "technical-certificate": {
                type: "技術證照內容不同",
                existingLabel: "資料庫證照內容",
                excelLabel: "Excel 證照內容",
                markCharacters: true
            },
            "duplicate-menu-title": {
                type: "選單名稱相同，但 RouterName 不同",
                existingLabel: "現有 RouterName",
                excelLabel: "Excel RouterName",
                markCharacters: true
            },
            "menu-parent": {
                type: "選單所在父層不同",
                existingLabel: "目前父層",
                excelLabel: "Excel 指定父層",
                markCharacters: false
            },
            "directory-page": {
                type: "既有目錄頁已有內容，版型可能被重新套用",
                existingLabel: "目前目錄頁",
                excelLabel: "授權後的動作",
                markCharacters: false
            }
        };
        const $details = $("<div>").addClass("mt-3").appendTo($content);
        differences.forEach(function (item) {
            const code = getAnalysisValue(item, "Code", "code");
            const option = optionDefinitions.find(function (definition) { return definition.code === code; });
            if (!option || !option.showDetails) return;
            const display = differenceDisplayDefinitions[code] || {
                type: "資料內容不同",
                existingLabel: "現有資料",
                excelLabel: "Excel",
                markCharacters: false
            };
            const existingValue = getAnalysisValue(item, "ExistingValue", "existingValue") || "[空白]";
            const excelValue = getAnalysisValue(item, "ExcelValue", "excelValue") || "[空白]";
            const $row = $("<div>")
                .addClass("border rounded p-2 mb-2 product-import-difference-detail")
                .attr("data-difference-code", code)
                .appendTo($details);
            $("<div>").addClass("fw-bold")
                .text("[" + getAnalysisValue(item, "Sheet", "sheet") + "] " + getAnalysisValue(item, "Name", "name"))
                .appendTo($row);
            $("<div>")
                .addClass("text-danger fw-bold my-1")
                .text("差異類型：" + display.type)
                .appendTo($row);
            if (display.markCharacters) {
                appendCharacterComparison(
                    $row,
                    display.existingLabel,
                    existingValue,
                    display.excelLabel,
                    excelValue);
            } else {
                $("<div>").append(
                    $("<span>").addClass("fw-bold me-1").text(display.existingLabel + "："),
                    document.createTextNode(existingValue)
                ).appendTo($row);
                $("<div>").append(
                    $("<span>").addClass("fw-bold me-1").text(display.excelLabel + "："),
                    document.createTextNode(excelValue)
                ).appendTo($row);
            }
            $("<div>").addClass("text-muted small")
                .text(getAnalysisValue(item, "Description", "description") || "")
                .appendTo($row);
        });

        function updateDifferenceDisplay() {
            let unauthorizedCount = 0;
            let visibleDetailCount = 0;
            renderedOptions.forEach(function (option) {
                const authorized = option.input.is(":checked");
                if (!authorized && option.definition.countsAsUnresolved !== false)
                    unauthorizedCount += option.count;
                if (!authorized && option.definition.showDetails) visibleDetailCount += option.count;
                $details.find('[data-difference-code="' + option.definition.code + '"]')
                    .toggle(!authorized);
            });
            $differenceAlert
                .toggle(unauthorizedCount > 0)
                .text(unauthorizedCount > 0
                    ? "尚有 " + unauthorizedCount + " 筆更新未授權；未勾選的項目會保留資料庫現況。"
                    : "所有列出的更新皆已授權。正式匯入時會依勾選項目處理。");
            $details.toggle(visibleDetailCount > 0);
        }
        renderedOptions.forEach(function (option) {
            option.input.on("change", updateDifferenceDisplay);
        });
        updateDifferenceDisplay();
    } else if (errors.length === 0) {
        $("<div>").addClass("alert alert-success py-2")
            .text("匯入檔掃描完成，沒有 Excel 衝突或資料庫差異，可以直接匯入。")
            .appendTo($content);
    }

    updateProductImportConfirmButton();
}

function updateProductImportConfirmButton() {
    if (!pendingProductImportAnalysis) return;
    if (productImportConfirmInProgress) {
        $("#btnConfirmProductImport").prop("disabled", true);
        return;
    }
    const errors = pendingProductImportAnalysis.errors || [];
    if (errors.length === 0) {
        $("#btnConfirmProductImport").prop("disabled", false);
        return;
    }
    const hasNonIgnorable = errors.some(function (item) {
        return !getAnalysisValue(item, "CanIgnore", "canIgnore");
    });
    const allIgnorableRowsHandled = productImportAnalysisErrorRows
        .filter(function (row) { return row.canIgnore; })
        .every(function (row) { return row.ignore; });
    $("#btnConfirmProductImport").prop("disabled", hasNonIgnorable || !allIgnorableRowsHandled);
}

async function confirmProductImport() {
    if (!pendingProductImportTaskId
        || !pendingProductImportAnalysis
        || productImportConfirmInProgress) return;
    productImportConfirmInProgress = true;
    const $button = $("#btnConfirmProductImport")
        .prop("disabled", true)
        .text("正在啟動匯入…");
    let confirmationPopupHidden = false;
    try {
        const selectedIgnoredRows = productImportAnalysisErrorRows
            .filter(function (error) { return error.canIgnore && error.ignore; })
            .reduce(function (rows, error) {
                error.rowNumbers.forEach(function (rowNumber) {
                    rows.push({ sheet: error.sheet, rowNumber: rowNumber });
                });
                return rows;
            }, []);
        const ignoredRows = pendingProductImportIgnoredRows.concat(selectedIgnoredRows)
            .filter(function (row, index, rows) {
                return rows.findIndex(function (item) {
                    return item.sheet === row.sheet && item.rowNumber === row.rowNumber;
                }) === index;
            });
        const response = await fetch("/api/Product/ConfirmProductImport", {
            method: "POST",
            headers: Object.assign({}, _c.Data.Header, { "Content-Type": "application/json" }),
            credentials: "same-origin",
            body: JSON.stringify({
                taskId: pendingProductImportTaskId,
                templateId: selectedProductImportTemplateId,
                overwriteExistingProductNames: $("#confirmOverwriteProductNames").is(":checked"),
                overwriteExistingSpecs: $("#confirmOverwriteProductSpecs").is(":checked"),
                overwriteExistingPrices: $("#confirmOverwriteProductPrices").is(":checked"),
                overwriteExistingTechnicalCertificates: $("#confirmOverwriteTechnicalCertificates").is(":checked"),
                allowDuplicateMenuTitles: $("#confirmAllowDuplicateMenuTitles").is(":checked"),
                overwriteExistingMenuParents: $("#confirmOverwriteMenuParents").is(":checked"),
                overwriteExistingDirectoryPages: $("#confirmOverwriteDirectoryPages").is(":checked"),
                ignoredRows: ignoredRows
            })
        });
        if (!response.ok) {
            const errorResult = await response.json().catch(function () { return {}; });
            if (errorResult.analysis) {
                pendingProductImportIgnoredRows = ignoredRows;
                pendingProductImportAnalysis = {
                    canImport: errorResult.analysis.CanImport !== undefined
                        ? errorResult.analysis.CanImport
                        : errorResult.analysis.canImport,
                    errors: errorResult.analysis.Errors || errorResult.analysis.errors || [],
                    differences: errorResult.analysis.Differences || errorResult.analysis.differences || [],
                    summary: errorResult.analysis.Summary || errorResult.analysis.summary || null
                };
                renderProductImportAnalysis(pendingProductImportAnalysis);
                $("#productImportAnalysisContent").scrollTop(0);
                co.sweet.warn(errorResult.message || "仍有 Excel 衝突，請繼續處理。");
                return;
            }
            throw new Error(errorResult.message || "無法確認商品匯入");
        }
        const responseData = await response.json();
        // DevExtreme Popup 的層級高於 SweetAlert；先關閉確認視窗，
        // 等隱藏動畫結束後再顯示正式匯入進度，避免進度被蓋住。
        importProdPopup.hide();
        confirmationPopupHidden = true;
        await new Promise(function (resolve) { window.setTimeout(resolve, 250); });
        const status = await waitForProductTask(responseData.taskId, "商品正式匯入中");
        Swal.close();
        const importResult = getProductImportResult(status.resultJson);
        showProductImportErrors(importResult.errors, false, importResult.summary);
        if (product_list != null) product_list.component.refresh();
        loadLastProductImportInfo(true);
    } catch (error) {
        Swal.close();
        if (!confirmationPopupHidden) importProdPopup.show();
        co.sweet.error(error && error.message ? error.message : "商品匯入失敗。");
    } finally {
        productImportConfirmInProgress = false;
        $button.text("確認並開始匯入");
        updateProductImportConfirmButton();
    }
}

function getProductImportErrors(resultJson) {
    return getProductImportResult(resultJson).errors;
}

function getProductImportResult(resultJson) {
    const emptyResult = { errors: [], summary: null };
    if (!resultJson) return emptyResult;

    try {
        const result = typeof resultJson === "string" ? JSON.parse(resultJson) : resultJson;
        const errors = result.Errors || result.errors || [];
        const summary = result.Summary || result.summary || null;
        if (!Array.isArray(errors)) return { errors: [], summary: summary };

        return {
            errors: errors.map(function (item, index) {
                return {
                    sequence: index + 1,
                    name: item.Name || item.name || "-",
                    description: item.Description || item.description || "-"
                };
            }),
            summary: summary
        };
    } catch (error) {
        console.error("Unable to parse product import result.", error);
        return emptyResult;
    }
}

function getImportSummaryValue(summary, pascalName, camelName) {
    if (!summary) return 0;
    return Number(summary[pascalName] !== undefined ? summary[pascalName] : summary[camelName]) || 0;
}

function appendProductImportSummary(contentElement, summary) {
    if (!summary) return;

    const detectedScopes = summary.DetectedUpdateScopes || summary.detectedUpdateScopes || [];

    const productText = "匯入檔 " + getImportSummaryValue(summary, "ProductRowCount", "productRowCount")
        + " 列，實際商品 " + getImportSummaryValue(summary, "ProductCount", "productCount")
        + " 隻；新增 " + getImportSummaryValue(summary, "ProductAddedCount", "productAddedCount")
        + "、更新 " + getImportSummaryValue(summary, "ProductUpdatedCount", "productUpdatedCount")
        + "；現有商品 " + getImportSummaryValue(summary, "ProductBeforeCount", "productBeforeCount")
        + " → " + getImportSummaryValue(summary, "ProductAfterCount", "productAfterCount") + " 隻";
    const menuText = "匯入檔 " + getImportSummaryValue(summary, "DirectoryRowCount", "directoryRowCount")
        + " 列目錄，涉及選單 " + getImportSummaryValue(summary, "MenuCount", "menuCount")
        + " 個；新增 " + getImportSummaryValue(summary, "MenuAddedCount", "menuAddedCount")
        + "、沿用既有 " + getImportSummaryValue(summary, "MenuExistingCount", "menuExistingCount")
        + "；現有選單 " + getImportSummaryValue(summary, "MenuBeforeCount", "menuBeforeCount")
        + " → " + getImportSummaryValue(summary, "MenuAfterCount", "menuAfterCount") + " 個";

    const summaryBox = $("<div>")
        .css({ padding: "12px 14px", marginBottom: "12px", background: "#f5f8fb", border: "1px solid #d9e2ec", borderRadius: "6px" })
        .appendTo(contentElement);
    $("<div>").css({ fontWeight: 600, marginBottom: "6px" }).text("匯入小總結").appendTo(summaryBox);
    $("<div>")
        .append(
            $("<span>").addClass("fw-bold").text("本次更新範圍："),
            document.createTextNode(detectedScopes.length > 0 ? detectedScopes.join("、") : "未偵測到可更新欄位")
        )
        .appendTo(summaryBox);
    $("<div>").css({ marginTop: "4px" }).text("商品：" + productText).appendTo(summaryBox);
    $("<div>").css({ marginTop: "4px" }).text("選單：" + menuText).appendTo(summaryBox);
}

function showProductImportErrors(errors, importFailed, summary) {
    const popupElement = $("<div>").appendTo(document.body);
    let popupInstance = null;

    popupElement.dxPopup({
        title: importFailed ? "商品匯入失敗－請修正 Excel" : (errors.length > 0 ? "商品匯入完成－需留意資料" : "商品匯入完成"),
        width: function () { return Math.min($(window).width() * 0.92, 960); },
        height: function () { return Math.min($(window).height() * 0.88, 680); },
        minWidth: 320,
        minHeight: 360,
        showTitle: true,
        showCloseButton: true,
        dragEnabled: true,
        hideOnOutsideClick: false,
        contentTemplate: function (contentElement) {
            contentElement.css({
                display: "flex",
                flexDirection: "column",
                height: "100%"
            });

            if (!importFailed) appendProductImportSummary(contentElement, summary);

            $("<div>")
                .css({ marginBottom: "12px", color: importFailed ? "#a94442" : "#856404" })
                .text(importFailed
                    ? "匯入已停止，未寫入任何資料。共有 " + errors.length + " 筆錯誤，請修正原 Excel 後重新匯入。"
                    : (errors.length > 0
                        ? "匯入已完成，共有 " + errors.length + " 筆資料需要留意。請依下列原因檢查原 Excel 內容。"
                        : "匯入已完成，沒有需要留意的資料。"))
                .appendTo(contentElement);

            const gridElement = $("<div>")
                .css({ flex: "1 1 auto", minHeight: 0 })
                .appendTo(contentElement);

            if (errors.length === 0) {
                gridElement.hide();
                return;
            }

            gridElement.dxDataGrid({
                dataSource: errors,
                height: "100%",
                showBorders: true,
                rowAlternationEnabled: true,
                wordWrapEnabled: true,
                columnAutoWidth: false,
                columns: [
                    { dataField: "sequence", caption: "序號", width: 70, alignment: "center", allowFiltering: false },
                    { dataField: "name", caption: "資料位置", width: "32%" },
                    { dataField: "description", caption: "需留意原因" }
                ],
                searchPanel: {
                    visible: true,
                    width: 240,
                    placeholder: "搜尋資料位置或原因"
                },
                paging: { pageSize: 10 },
                pager: {
                    visible: true,
                    showPageSizeSelector: true,
                    allowedPageSizes: [10, 20, 50],
                    showInfo: true,
                    showNavigationButtons: true
                },
                onToolbarPreparing: function (e) {
                    e.toolbarOptions.items.unshift({
                        location: "after",
                        locateInMenu: "never",
                        widget: "dxButton",
                        options: {
                            icon: "fa-solid fa-file-excel",
                            text: importFailed
                                ? "下載錯誤清單"
                                : "下載注意清單",
                            hint: importFailed
                                ? "將目前商品匯入錯誤下載為 Excel"
                                : "將目前商品匯入注意事項下載為 Excel",
                            type: importFailed ? "danger" : "success",
                            stylingMode: "contained",
                            elementAttr: {
                                class: "product-import-error-download"
                            },
                            onClick: function () {
                                CokerDxGridExport({
                                    component: e.component,
                                    format: "xlsx",
                                    cancel: false
                                }, {
                                    fileName: importFailed
                                        ? "ProductImportErrors"
                                        : "ProductImportWarnings",
                                    worksheetName: importFailed
                                        ? "商品匯入錯誤清單"
                                        : "商品匯入注意清單"
                                });
                            }
                        }
                    });
                },
                noDataText: "沒有需留意的資料"
            });
        },
        toolbarItems: [
            {
                toolbar: "bottom",
                location: "after",
                widget: "dxButton",
                options: {
                    text: "關閉",
                    type: "normal",
                    stylingMode: "outlined",
                    onClick: function () { popupInstance.hide(); }
                }
            }
        ],
        onHidden: function (e) {
            e.component.dispose();
            popupElement.remove();
        }
    });

    popupInstance = popupElement.dxPopup("instance");
    popupInstance.show();
}

async function exportProd(e) {
    if (exportProd.isProcessing) return;

    const versionResult = await Swal.fire({
        title: "選擇商品匯出版本",
        text: "選擇版本後會立即開始製作匯出檔。",
        icon: "question",
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: "完整商品資料",
        denyButtonText: "價格／庫存簡易版",
        cancelButtonText: "取消",
        reverseButtons: true
    });
    if (!versionResult.isConfirmed && !versionResult.isDenied) return;
    const exportVersion = versionResult.isConfirmed ? "full" : "price";

    exportProd.isProcessing = true;
    const button = e && e.component ? e.component : null;
    if (button) button.option("disabled", true);

    try {
        const startResponse = await fetch(
            "/api/Product/StartProductExport?version=" + encodeURIComponent(exportVersion), {
            method: "POST",
            headers: _c.Data.Header,
            credentials: "same-origin"
            });
        if (!startResponse.ok) {
            const errorResult = await startResponse.json().catch(function () { return {}; });
            throw new Error(errorResult.message || "無法建立商品匯出任務");
        }

        const startResult = await startResponse.json();
        await waitForProductTask(startResult.taskId, "商品檔案製作中");
        window.location.href = "/api/Product/DownloadProductTask?taskId=" + encodeURIComponent(startResult.taskId);
        Swal.close();
    } catch (error) {
        Swal.close();
        co.sweet.error(error && error.message
            ? error.message
            : "商品匯出失敗，請稍後再試。");
    } finally {
        exportProd.isProcessing = false;
        if (button) button.option("disabled", false);
    }
}

async function waitForProductTask(taskId, title) {
    Swal.fire({
        title: title,
        html: [
            '<div id="productTaskMessage" style="margin-bottom:12px;">等待伺服器開始處理…</div>',
            '<div style="height:18px;background:#e9ecef;border-radius:9px;overflow:hidden;">',
            '<div id="productTaskProgressBar" style="height:100%;width:0%;background:#337ab7;transition:width .3s ease;"></div>',
            '</div>',
            '<div id="productTaskProgressText" style="margin-top:6px;font-weight:600;">0%</div>',
            '<div style="margin-top:10px;color:#777;font-size:13px;">關閉此視窗不會取消背景任務。</div>'
        ].join(""),
        allowOutsideClick: false,
        allowEscapeKey: true,
        showCloseButton: true,
        showConfirmButton: false
    });

    while (true) {
        await new Promise(function (resolve) { setTimeout(resolve, 1000); });
        const statusResponse = await fetch(
            "/api/Product/GetProductTaskStatus?taskId=" + encodeURIComponent(taskId),
            {
                method: "GET",
                headers: _c.Data.Header,
                credentials: "same-origin",
                cache: "no-store"
            });
        if (!statusResponse.ok) throw new Error("無法取得背景任務進度");

        const status = await statusResponse.json();
        const progress = Math.max(0, Math.min(100, Number(status.progress) || 0));
        const progressBar = document.getElementById("productTaskProgressBar");
        const progressText = document.getElementById("productTaskProgressText");
        const message = document.getElementById("productTaskMessage");
        if (progressBar) progressBar.style.width = progress + "%";
        if (progressText) progressText.textContent = progress + "%";
        if (message) message.textContent = status.message || "背景任務處理中…";

        if (status.status === "failed" || status.status === "expired") {
            const taskError = new Error(status.error || status.message || "背景任務失敗");
            const importResult = getProductImportResult(status.resultJson);
            taskError.importErrors = importResult.errors;
            taskError.importSummary = importResult.summary;
            throw taskError;
        }
        if (status.status === "succeeded" || status.status === "awaitingconfirmation")
            return status;
    }
}

function toolbarPreparing(e) {
    var dataGrid = e.component;

    e.toolbarOptions.items.unshift(
        {
            location: "before",
            widget: "dxButton",
            options: {
                icon: "plus",
                text: "新增商品",
                type: "default",
                stylingMode: "contained",
                onClick: function () {
                    window.location.hash = 0;
                }
            }
        },
        {
            location: "after",
            widget: "dxButton",
            options: {
                icon: "fa-solid fa-file-excel",
                text: "商品匯出",
                stylingMode: "outlined",
                onClick: exportProd
            }
        },
        {
            location: "after",
            widget: "dxButton",
            options: {
                icon: "fa-solid fa-file-arrow-up",
                text: "商品資料匯入",
                stylingMode: "outlined",
                onClick: showImportProdPopup
            }
        }
    );
}

async function PageReady() {
    ElementInit();

    ProductTagFilterInit();
    TechCertListModalInit();
    TagListModalInit();

    elementReady = true;

    if (pendingHashEdit) {
        pendingHashEdit = false;
        HashDataEdit();
    }
    try {
        const LogisticsBoxRequires = await co.LogisticsBox.Requires();
        if (!LogisticsBoxRequires.object) throw new Error("不需要物流箱");
    } catch (error) {
        $("#Spec_Frame").addClass("no-logistics-box");
    }

    // 啟動
    const editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.Product.Content.SaveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Product.Content.ImportConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });

    //設定html資料
    setPage = function (id) {
        co.Product.Content.GetConten({ Id: id }).done(function (result) {
            if (result.success) {
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                co.Grapes.setEditor(editor, html, result.conten.saveCss);
                co.Grapes.setFile(editor, id, 3);
            } else {
                co.sweet.error(result.error);
            }
        });
    }

    // 開啟規格描述 modal（.btn_spec_desc_edit 在樣板內，用 delegated）
    $(document).on("click", ".btn_spec_desc_edit", function (e) {
        e.preventDefault();
        $currentSpecDescRow = $(this).closest(".spec_list");
        $spec_desc_input.val($currentSpecDescRow.data("specdesc") || "");
        $("#SpecDescCount").text($spec_desc_input.val().length);
        specDescModal.show();
    });
    $spec_desc_input.on("input", function () {
        $("#SpecDescCount").text($(this).val().length);
    });
    $(".btn_spec_desc_save").on("click", function () {
        if ($currentSpecDescRow != null) {
            $currentSpecDescRow.data("specdesc", $spec_desc_input.val());
        }
        specDescModal.hide();
    });

    /* File Upload */
    co.File.ListFileInit();

    /* Spec List */
    co.Product.Spec.ListInit();

    /* 日期選擇 */
    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    $(document).on("wheel", "input[type=number]", function (event) {
        event.preventDefault();
    });

    $(document).on('blur', 'input[type="number"]', function () {
        var $self = $(this);
        var value = $self.val().trim();

        if (/^0+\d/.test(value)) value = value.replace(/^0+/, '');
        if ($self.attr("step") == "1" && value.includes(".")) value = value.substring(0, value.indexOf("."));
        if (parseFloat(value) > 100000000) value = "100000000";

        $self.val(value);
    });

    /*Form觸發*/
    const forms = $('#ProductForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    if (ISpecRepect()) {
                        co.sweet.error("錯誤", "商品規格不可重複", null, false);
                    } else {
                        var price_null = false;
                        var $null_input;
                        $(".input_price").each(function () {
                            if ($(this).val() == "") {
                                price_null = true;
                                $null_input = $(this);
                                return false;
                            }
                        })
                        if (price_null && window.priceOptional !== true) {
                            co.sweet.error("錯誤", "請確實填寫價格", function () {
                                setTimeout(function () {
                                    $('html, body').animate({ scrollTop: $null_input.offset().top - ($("header").height() * 2) }, 0);
                                }, 500)
                            }, false);
                        } else {
                            Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                                AddUp("已成功發布", "發布發生未知錯誤", "item");
                            });
                        }
                    }
                }
                form.classList.add('was-validated');
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回商品列表", "資料將不被保存", "確定", "取消", function () {
            BackToList(true);
        });
    })

    $(".btn_input_pic").on("click", function (event) {
        event.preventDefault();
        $(".input_pic").click();
    })

    $(".btn_expand_out").on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_less") {
            $self.children("span").text("expand_more")
        } else {
            $self.children("span").text("expand_less")
        }
    })
    $(".btn_spec_price_add").on("click", function () {
        SpecPriceAdd(null)
    });
    $(".btn_price_save").on("click", SpecPriceSave);
    $("#TimePrice").on("change", function () {
        if ($(this).prop("checked")) $(".priceSetting").addClass("d-none");
        else $(".priceSetting").removeClass("d-none");
    });

    $name.on('keyup', function () {
        $name_count.text($name.val().length);
    });
    $introduction.on('keyup', function () {
        $introduction_count.text($introduction.val().length);
    });
    $illustrate.on('keyup', function () {
        $illustrate_count.text($illustrate.val().length);
    });

    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $date.val('');
            $date.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
            $removedFromShelves.prop("checked", true);
        } else {
            $date.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) { window.onhashchange = hashChange; } else { setInterval(hashChange, 1000); }

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()

        Swal.fire({
            icon: 'info',
            title: "前往內容編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#888888',
            denyButtonColor: '#d33',
            confirmButtonText: "　是　",
            cancelButtonText: "　取消　",
            denyButtonText: "　否　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                Array.from(forms).forEach(form => {
                    if (form.checkValidity()) {
                        if (ISpecRepect()) {
                            if ($removedFromShelves.is(":checked")) {
                                $removedFromShelves.prop("checked", false);
                                AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                            } else {
                                AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                            }
                        } else {
                            var price_null = false;
                            var $null_input;
                            $(".input_price").each(function () {
                                if ($(this).val() == "") {
                                    price_null = true;
                                    $null_input = $(this);
                                    return false;
                                }
                            })
                            if (price_null && window.priceOptional !== true) {
                                if ($removedFromShelves.is(":checked")) {
                                    $removedFromShelves.prop("checked", false);
                                    AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                                } else {
                                    AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                                }
                            } else {
                                AddUp("已成功發布", "發布發生未知錯誤", "Canvas");
                            }
                        }
                    } else {
                        if ($removedFromShelves.is(":checked")) {
                            $removedFromShelves.prop("checked", false);
                            AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                        } else {
                            AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                        }
                    }
                });
            } else if (result.isDenied) {
                window.location.hash = `${keyId}-1`;
            }
        })
    })
}
function getProductTagFilterIds() {
    if (productTagFilter == null) return "";

    var values = productTagFilter.option("value") || [];
    return values.join(",");
}
function LoadProductTagOptions() {
    if (productTagOptions.length > 0) {
        return Promise.resolve(productTagOptions);
    }

    if (productTagOptionsPromise != null) {
        return productTagOptionsPromise;
    }

    productTagOptionsPromise = co.Product.Get.ProductListTags()
        .then(function (data) {
            productTagOptions = data || [];
            return productTagOptions;
        })
        .catch(function (error) {
            productTagOptionsPromise = null;
            productTagOptions = [];

            if (window.console) {
                console.error("商品標籤載入失敗", error);
            }

            return [];
        });

    return productTagOptionsPromise;
}
function ProductTagFilterInit() {
    var store = new DevExpress.data.CustomStore({
        key: "fK_TId",
        loadMode: "raw",
        load: function () {
            return LoadProductTagOptions();
        }
    });

    $("#ProductTagFilter").dxTagBox({
        dataSource: store,
        valueExpr: "fK_TId",
        displayExpr: "tag_Name",
        placeholder: "選擇商品標籤，可多選",
        showSelectionControls: true,
        applyValueMode: "useButtons",
        searchEnabled: true,
        multiline: false,
        maxDisplayedTags: 4,
        showMultiTagOnly: false,

        dropDownOptions: {
            width: "auto",
            minWidth: 520,
            maxWidth: 760,
            wrapperAttr: {
                class: "product-tag-filter-dropdown"
            }
        },

        itemTemplate: function (itemData) {
            return $("<div>")
                .addClass("product-tag-filter-item")
                .text(itemData.tag_Name);
        },

        onValueChanged: function () {
            if (product_list != null && product_list.component != null) {
                product_list.component.refresh();
            }
        }
    });

    productTagFilter = $("#ProductTagFilter").dxTagBox("instance");
    LoadProductTagOptions();

    $("#ProductTagFilterClear").on("click", function () {
        if (productTagFilter == null) return;

        productTagFilter.option("value", []);
    });

    $(document).on("click", ".product-tag-badge", function (event) {
        event.preventDefault();
        event.stopPropagation();

        var tagName = ($(this).data("tag-name") || "").toString().trim();

        if (tagName === "" || productTagFilter == null) return;

        LoadProductTagOptions().then(function (tagOptions) {
            var tagItem = tagOptions.find(function (item) {
                return (item.tag_Name || "").toString().trim() === tagName;
            });

            if (tagItem == null) return;

            var values = productTagFilter.option("value") || [];
            var tagId = tagItem.fK_TId;

            if (values.indexOf(tagId) === -1) {
                values.push(tagId);
                productTagFilter.option("value", values);
            }
        });
    });
}
function ElementInit() {
    $name = $("#InputName");
    $name_count = $("#ProductForm .name .name_count");
    $introduction = $("#InputIntroduction");
    $introduction_count = $("#ProductForm .introduction .introduction_count");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#ProductForm .illustrate .illustrate_count");
    $marks = $("#InputMarks");
    $spec_select = $(".spec_select")
    $price = $(".input_price");
    $subItemNo = $(".input_subItemNo");
    $stock_number = $(".input_stock_number");
    $packingPoint_number = $(".input_packingPoint_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
    $itemNo = $("#InputItemNo");
    $itemNo_count = $("#ProductForm .itemNo .itemNo_count");
    $display = $(`#ProductForm [name="Visible"]`);
    $popularVisible = $(`#ProductForm [name="PopularVisible"]`);
    $popularValue = $("#ProductPopularValue");
    $removedFromShelves = $(`#ProductForm [name="RemovedFromShelves"]`);
    $noStockManagement = $("#NoStockManagement");

    specDescModal = new bootstrap.Modal(document.getElementById('SpecDescModal'));
    $spec_desc_input = $("#InputSpecDesc");

    priceModal = new bootstrap.Modal(document.getElementById('PriceModal'));
    specMediaModal = new bootstrap.Modal(document.getElementById('SpecMediaModal'));
    document.getElementById('SpecMediaModal').addEventListener('hidden.bs.modal', function () {
        var $block = $("#SpecMedia");
        syncSpecMediaOrder($block);
        $block.find("ul > li.upload_list").remove();
        UploadPreviewFrameClear($block);
        var $row = $block.data("spec-row");
        if ($row && $row.length) refreshSpecThumb($row);
    });

    // 依畫面上的 li 順序，把 bucket 陣列「就地」重排（必須保留同一個陣列參照）
    function syncSpecMediaOrder($block) {
        var store = $block.data("files");
        if (!store) return;

        var ordered = [];
        $block.find("ul > li.upload_list").each(function () {
            var $li = $(this);
            var f;
            if (typeof $li.data("id") != "undefined") {
                f = store.find(x => x.Id == $li.data("id"));
            } else if (typeof $li.data("tempid") != "undefined") {
                f = store.find(x => x.TempId == $li.data("tempid"));
            }
            if (f && ordered.indexOf(f) === -1) ordered.push(f);
        });

        // 補回沒出現在畫面上的項目（例如已標記刪除的），避免存檔時漏掉刪除
        store.forEach(function (f) {
            if (ordered.indexOf(f) === -1) ordered.push(f);
        });

        // 就地替換內容，維持同一個陣列參照（spec_media_map[key] 與 $block.data("files") 是同一個陣列）
        store.length = 0;
        Array.prototype.push.apply(store, ordered);
    }

    $price_modal = $("#PriceModal >.modal-dialog > .modal-content > .modal-body > .priceSetting >.price_option");
    $("#SortCheck").on("change", function () {
        const $items = $(`[name="serNo"]`);
        if ($(this).prop("checked")) $items.removeAttr("disabled");
        else $items.attr({ disabled: "disabled" });
    });

    $("#NoStockManagement").on("change", function () {
        var disabled = $(this).prop("checked");
        var $targets = $(".input_stock_number, .input_alert_number");
        if (disabled) $targets.attr("disabled", "disabled");
        else $targets.removeAttr("disabled");
    });

    document.getElementById('PriceModal').addEventListener('hidden.bs.modal', function (event) {
        $price_modal.children(".frame").each(function () {
            $(this).remove();
            spec_price_num = 0;
        });

        $(".input_price").each(function () {
            var $self = $(this)
            var psid = $self.parents(".spec_list").data("psid")
            var temppsid = $self.parents(".spec_list").data("temppsid")
            var timePrice = $self.parents(".spec_list").data("timeprice")
            var count = $self.parents(".price").find(".count");
            var text = "";
            var filter = modal_price_list.filter(item => !item.IsDelete && (item["FK_PSId"] == psid || item["TempPSid"] == temppsid));
            $self.removeClass("multi-price");
            if (timePrice) {
                $self.val("時價");
                count.addClass("d-none")
            } else {
                if (filter.length > 1) {
                    count.removeClass("d-none").text(filter.length);
                    $self.addClass("multi-price");
                } else count.addClass("d-none");
                filter.forEach(item => {
                    if (text != "") text += "\n";
                    text += "現金：" + co.String.thousandSign(item["Price"]);
                    if (parseInt(item["Bonus"]) !== 0) text += " 紅利：" + co.String.thousandSign(item["Bonus"]);
                });
                $self.val(filter.length ? text : "");
            }
        })

        $(".alert_text").addClass("d-none");
    })
}
function FormDataClear() {
    TechCertDataClear();
    TagDataClear();
    $("#Spec_Frame .spec_list").each(function () {
        $(this).remove();
    })
    spec_num = 0;
    keyId = 0;
    $removedFromShelves.prop("checked", false);
    $display.prop("checked", false);
    $popularVisible.prop("checked", true);
    $popularValue.text("0");
    $name.val("");
    $name_count.text(0);
    $itemNo.val("");
    $itemNo_count.text(0);
    $introduction.val("");
    $introduction_count.text(0);
    $illustrate.val("");
    $illustrate_count.text(0);
    $marks.val("");
    $price.val("");
    $subItemNo.val("");
    $stock_number.val(0);
    $packingPoint_number.val(1);
    $alert_number.val("");
    $min_number.val(1);
    $permanent.prop("checked", false);
    $noStockManagement.prop("checked", false);
    $date.val("");
    $date.removeAttr("disabled");
    startDate = null;
    endDate = null;
    spec_remove_list = [];

    modal_price_list = [];
    suggest_price_list = [];
    price_tid = 0;
    temp_psid = 0;
    $(".data_upload").each(function () {
        UploadPreviewFrameClear($(this));
    });
    $(".data_upload > ul > .upload_list").remove();
    total_files = [];
    spec_media_map = {};
    $("#SpecMedia").data("files", null).data("spec-key", null);
}
function contentReady(e) {
    product_list = e;
    loadLastProductImportInfo(false);

    if (!elementReady) {
        pendingHashEdit = true;
        return;
    }

    HashDataEdit();
}
function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}
function HashDataEdit() {
    FormDataClear();
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) == 0) {
                if (hash.includes('-1')) {
                    MoveToCanvas();
                } else {
                    co.Spec.GetPickSpecList().done(function (pick_result) {
                        spec_pick_list = pick_result;
                        SpecAdd(null);
                        MoveToContent();
                    });
                }
            } else {
                if (hash.includes('-1')) {
                    keyId = parseInt(hash);
                    MoveToCanvas();
                } else {
                    co.Product.Get.ProdOne(parseInt(hash)).done(function (result) {
                        if (result != null) {
                            co.Spec.GetPickSpecList().done(function (pick_result) {
                                spec_pick_list = pick_result;
                                FormDataSet(result);
                                MoveToContent();
                            });
                        } else {
                            BackToList(false);
                        }
                    })
                }
            }
        }
    } else {
        BackToList(false);
    }
}
function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId
}
function paletteButtonClicked(e) {
    $("#gjs").data("id", e.row.key);
    setPage(e.row.key);
    keyId = e.row.key + "-1";
    window.location.hash = keyId;
}
function FormDataSet(result) {
    //console.log(result)
    //$("#ProductContent .card-header .titile").append(`編輯商品<span class="d-md-flex d-none">－${result.title}</span>`);

    TagDataSet(result.tagDatas);
    TechCertDataSet(result.techCertDatas);

    result.multimedia.forEach(media => {
        UploadListAdd(media, $("#ProdMedia"));
    })

    result.files.forEach(file => {
        UploadListAdd(file, $("#ProdFiles"));
    })
    $("#ProdMedia > ul > li:first-child").trigger("click");

    result.stocks.forEach(function (stock) {
        var suggest_price_obj = {};
        suggest_price_obj["FK_PSId"] = stock.id;
        suggest_price_obj["TempPSid"] = 0;
        suggest_price_obj["Price"] = stock.price
        suggest_price_list.push(suggest_price_obj);

        stock.prices.forEach(function (price) {
            var price_obj = {};
            price_obj["Id"] = price.id;
            price_obj["Tempid"] = price_tid;
            price_obj["FK_PSId"] = price.fK_PSId;
            price_obj["TempPSid"] = 0;
            price_obj["FK_RId"] = price.fK_RId
            price_obj["Price"] = price.price
            price_obj["Bonus"] = price.bonus
            price_obj["SubItemNo"] = price.subItemNo
            price_obj["IsDelete"] = false;
            price_tid += 1;
            modal_price_list.push(price_obj);
        });
        SpecAdd(stock);
    });

    startDate = result.startTime;
    endDate = result.endTime;
    keyId = result.id;
    disp_opt = result.disp_Opt;
    $removedFromShelves.prop("checked", !result.removedFromShelves);
    $display.prop("checked", result.visible);
    $popularVisible.prop("checked", result.popularVisible);
    $popularValue.text(Number(result.popular ?? result.Popular ?? 0).toLocaleString("zh-TW"));
    $noStockManagement.prop("checked", result.noStockManagement);
    $noStockManagement.prop("checked", result.noStockManagement);
    $noStockManagement.trigger("change");
    $name.val(result.title);
    $name_count.text($name.val().length);
    $itemNo.val(result.itemNo);
    $itemNo_count.text($itemNo.val().length);
    $introduction.val(result.introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);
    $(`[name="ProdStatus"] > option[value="${result.status}"]`).prop("selected", true);
    $date = $("#InputDate");
    $(".linkToF").attr("href", `${defaultUrl}/${OrgName}/search/product/${result.id}`);

    $("#SortCheck").prop("checked", result.ser_No != 500);
    $(`[name="serNo"]`).val(result.ser_No);
    $("#SortCheck").trigger("change");

    if (result.permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
    }
}
function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Product.Delete.Prod(e.row.key).done(function () {
            product_list.component.refresh();
        }).fail(function () {
            Coker.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
        });
    });
}
function IsGuestRoleId(roleId) {
    var id = parseInt(roleId || 0, 10);
    return id === 0 || id === 1;
}

function ApplyGuestBonusControl($priceRow) {
    var $role = $priceRow.find(".select_role");
    var $bonus = $priceRow.find(".input_bonus");

    var isGuest = IsGuestRoleId($role.val());

    // 不清空欄位，只禁止編輯。
    // 真正回存時才把非會員紅利歸 0。
    $bonus
        .prop("disabled", isGuest)
        .toggleClass("bg-light text-muted", isGuest);
}
function GetFormattedNumberValue($input) {
    var value = co.Form.normalizeElementValue($input, $input.val());
    return value == null || value === "" || isNaN(value) ? 0 : Number(value);
}
function SetFormattedNumberValue($input, value) {
    co.Form.bindNumberFormatter($input);
    $input.val(co.Form.formatElementValue($input, value));
}
function SpecPriceAdd(result) {
    spec_price_num += 1;

    var item = $($("#ModalTemplatePrice").html()).clone();
    var item_role = item.find(".select_role"),
        item_cash = item.find(".input_cash"),
        item_bonus = item.find(".input_bonus"),
        item_btn_delete = item.find(".btn_price_delete");

    item.data("ppid", result == null ? 0 : result.Id);
    item.data("tempid", result == null ? -1 : result.Tempid);
    co.Form.bindNumberFormatter(item_cash);
    co.Form.bindNumberFormatter(item_bonus);

    if (result != null) {
        item_role.val(result.FK_RId);
        SetFormattedNumberValue(item_cash, result.Price);
        SetFormattedNumberValue(item_bonus, result.Bonus);
    } else {
        SetFormattedNumberValue(item_cash, item_cash.val());
        SetFormattedNumberValue(item_bonus, item_bonus.val());
    }

    ApplyGuestBonusControl(item);

    item_role.on("change", function () {
        ApplyGuestBonusControl(item);
    });

    item_btn_delete.on("click", function () {
        var $self_p = $(this).parents(".modal_price").first();
        if (spec_price_num == 1) {
            co.sweet.error("商品至少需有一種價格", null, false);
        } else {
            co.sweet.confirm("移除價格", "確定要移除此項價格嗎?", "　是　", "　否　", function () {
                if ($self_p.data("ppid") == 0) {
                    if ($self_p.data("tempid") > -1) {
                        var index = modal_price_list.findIndex(item => item["Tempid"] == $self_p.data("tempid"))
                        modal_price_list[index]["IsDelete"] = true;
                    }
                } else {
                    var index = modal_price_list.findIndex(item => item["Id"] == $self_p.data("ppid"))
                    modal_price_list[index]["IsDelete"] = true;
                }
                spec_price_num -= 1;
                $self_p.remove();
            })
        }
    })

    $("#PriceModal > .modal-dialog > .modal-content > .modal-body .price_option").append(item);

    $("input[type='number']").on("input", function () {
        var $self = $(this);
        var value = $self.val();
        if (value !== "" && parseFloat(value) < 0) $self.val("0");
    });
}
function SpecPriceSave() {
    var temp_list = []
    var save_success = true
    var psid = $price_modal.parents(".modal-body").first().data("psid");
    var temppsid = $price_modal.parents(".modal-body").first().data("temppsid");
    let index;
    if (psid != "") {
        index = suggest_price_list.findIndex(item => item["FK_PSId"] == psid)
    } else {
        index = suggest_price_list.findIndex(item => item["TempPSid"] == temppsid)
    }
    var $suggestPriceInput = $("#PriceModal .suggest_price input");
    suggest_price_list[index]["Price"] = GetFormattedNumberValue($suggestPriceInput);
    $(".spec_list").each(function () {
        if ($(this).data("psid") == psid || $(this).data("temppsid") == temppsid)
            $(this).data("timeprice", $("#TimePrice").prop("checked"));
    });
    if (!$("#TimePrice").prop("checked")) {
        $price_modal.children(".frame").each(function () {
            $self = $(this);
            const $cashInput = $self.find(".input_cash");
            const $bonusInput = $self.find(".input_bonus");
            var obj = {};
            obj["Id"] = $self.data("ppid");
            obj["Tempid"] = price_tid;
            obj["FK_PSId"] = psid;
            obj["TempPSid"] = temppsid;
            obj["FK_RId"] = $self.find(".select_role").val();
            obj["Price"] = GetFormattedNumberValue($cashInput);
            obj["Bonus"] = IsGuestRoleId(obj["FK_RId"])
                ? 0
                : GetFormattedNumberValue($bonusInput);
            obj["IsDelete"] = false;
            if (parseInt(obj["Price"] || 0, 10) === 0 && parseInt(obj["Bonus"] || 0, 10) === 0) {
                co.sweet.error("商品現金與紅利不可同時為空", null, true)
                $(".alert_text").text("商品現金與紅利不可同時為空")
                $(".alert_text").removeClass("d-none");
                save_success = false
            } else {
                if (temp_list.find(item => item["FK_RId"] == obj["FK_RId"] && (item["Price"] == obj["Price"] || item["Bonus"] == obj["Bonus"])) != null) {
                    co.sweet.error("商品現金或紅利不可重複", null, true)
                    $(".alert_text").removeClass("d-none");
                    $(".alert_text").text("同個會員等級下商品現金或紅利不可重複");
                    save_success = false
                } else {
                    temp_list.push(obj)
                    $(".alert_text").addClass("d-none");
                    if ($self.data("tempid") < 0) {
                        modal_price_list.push(obj)
                        price_tid += 1;
                    } else {
                        var index = modal_price_list.findIndex(item => item["Tempid"] == $self.data("tempid"))
                        modal_price_list[index]["FK_RId"] = obj["FK_RId"];
                        modal_price_list[index]["Price"] = obj["Price"];
                        modal_price_list[index]["Bonus"] = obj["Bonus"];
                    }
                }
            }
        })
    }
    if (save_success) {
        priceModal.hide();
    }
}
function SpecAdd(result) {
    spec_num += 1;
    $("#Spec_Frame").data("spec_num", spec_num)
    var item = $($("#TemplateSpecification").html()).clone();
    var item_select_input_1 = item.find(".input_spec").first(),
        item_select_input_2 = item.find(".input_spec").last(),
        item_select_list_1 = item.find("datalist").first(),
        item_select_list_2 = item.find("datalist").last(),
        item_price = item.find(".input_price"),
        item_price_count = item.find(".price > .count"),
        item_subItemNo = item.find(".input_subItemNo"),
        item_min = item.find(".input_min_number"),
        item_stock = item.find(".input_stock_number"),
        item_packingPoint = item.find(".input_packingPoint_number"),
        item_alert = item.find(".input_alert_number"),
        item_collapse = item.find(".collapse"),
        item_btn_expand = item.find(".btn_expand"),
        item_btn_delete = item.find(".btn_remove");

    if (result != null) {
        item.find(".ser_no").val(result.ser_No);
        item.data("serno", result.ser_No);
        item.data("timeprice", result.timePrice);
    } else {
        item.find(".ser_no").val(spec_num);
        item.data("serno", spec_num);
        item.data("timeprice", false);
    }

    item.find(".ser_no").on("blur", function () {
        var $self = $(this);
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $(".spec_list").length) {
            $self.val($(".spec_list").length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($(".spec_list"), "bigger", item.data("serno"), $self.val())
                $("#Spec_Frame > ul.specList").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($(".spec_list"), "smaller", $self.val(), item.data("serno"))
                $("#Spec_Frame > ul.specList").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })
    item.find(".spec").each(function () {
        var spectype = $($("#TemplateSpecType").html()).clone();
        $(this).prepend(spectype);
    })

    var item_select_1 = item.find(".spec_select").first(),
        item_select_2 = item.find(".spec_select").last();

    if (result != null && result.fK_ST1id != null) {
        item_select_1.val(result.fK_ST1id);
        if (result.fK_ST1id > 0) {
            var $spec1_bro = item_select_1.parents(".spec").first().siblings(".spec");

            $spec1_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                if (child.val() == item_select_1.val()) {
                    child.attr("disabled", "disabled");
                    child.addClass("bg-secondary-light25");
                }
            })
            item_select_input_1.removeAttr("disabled")

            var temp_spec_list = spec_pick_list.find(item => item.id == item_select_1.val());
            if (!!temp_spec_list && temp_spec_list.specs.length > 0) {
                temp_spec_list.specs.forEach(item => {
                    item_select_list_1.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    if (item.id == result.fK_S1id) {
                        item_select_input_1.val(item.title);
                        item_select_input_1.data("id", item.id);
                    }
                })
            }
        }
    }

    if (result != null && result.fK_ST2id != null) {
        item_select_2.val(result.fK_ST2id);
        if (result.fK_ST2id > 0) {
            var $spec2_bro = item_select_2.parents(".spec").first().siblings(".spec");

            $spec2_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                if (child.val() == item_select_2.val()) {
                    child.attr("disabled", "disabled");
                    child.addClass("bg-secondary-light25");
                }
            })
            item_select_input_2.removeAttr("disabled")

            var temp_spec_list = spec_pick_list.find(item => item.id == item_select_2.val())
            if (temp_spec_list && temp_spec_list.specs && temp_spec_list.specs.length > 0) {
                temp_spec_list.specs.forEach(item => {
                    item_select_list_2.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    if (item.id == result.fK_S2id) {
                        item_select_input_2.val(item.title);
                        item_select_input_2.data("id", item.id);
                    }
                })
            }
        }
    }
    if (result != null) {
        item.data("psid", result.id);
        item.data("oldstock", result.stock);
    } else {
        temp_psid += 1;
        item.data("temppsid", temp_psid);
        item.data("oldstock", null);

        var suggest_price_obj = {};
        suggest_price_obj["FK_PSId"] = "";
        suggest_price_obj["TempPSid"] = temp_psid;
        suggest_price_obj["Price"] = 0;
        suggest_price_list.push(suggest_price_obj);
    }

    var _specKey = result != null ? ("P" + result.id) : ("T" + temp_psid);
    spec_media_map[_specKey] = (result != null && result.multimedia)
        ? result.multimedia.map(convertSpecMedia)
        : [];
    item.find(".btn_spec_img_edit").on("click", function () {
        OpenSpecMediaModal(item);
    });
    refreshSpecThumb(item);

    if (item.data("timeprice")) {
        item_price.val("時價");
        item_price_count.addClass("d-none");
    } else {
        var text = "";
        var filter = modal_price_list.filter(mitem => !mitem.IsDelete && (mitem["FK_PSId"] == item.data("psid")));
        item_price.removeClass("multi-price");
        if (filter.length > 1) {
            item_price.addClass("multi-price");
            item_price_count.removeClass("d-none").text(filter.length);
        } else item_price_count.addClass("d-none");
        filter.map(item => {
            if (text != "") text += "\n";
            if (parseInt(item["Price"]) > 0) text += "現金：" + co.String.thousandSign(item["Price"]);
            if (parseInt(item["Bonus"]) > 0) text += " 紅利：" + co.String.thousandSign(item["Bonus"]);
        });
        if (filter.length > 0) {
            item_price.val(text);
        } else {
            item_price.val("");
        }
    }

    item.data("specdesc", result != null ? (result.specDescription || "") : "");
    item_subItemNo.val(result != null ? result.subItemNo : "");
    item_min.val(result != null ? result.min_Qty ?? 1 : 1);
    item_min.on("change", function () {
        var $self = $(this);
        if ($self.val() < 1 || $self.val() == "") $self.val(1);
    });
    item_stock.val(result != null ? (result.stock ?? 0) + result.orderStock : 0);
    item_packingPoint.val(result != null ? result.packingPoint ?? 1 : 1);
    item_alert.val("");
    item_alert.val(result != null ? result.alert_Qty : "");
    item_collapse.attr("id", "CollapseDetail" + spec_num);
    item_btn_expand.attr("data-bs-target", "#CollapseDetail" + spec_num);
    item_btn_expand.attr("aria-controls", "CollapseDetail" + spec_num);
    item_select_input_1.attr("list", "SpecListOpt" + spec_num + "-1");
    item_select_input_2.attr("list", "SpecListOpt" + spec_num + "-2");
    item_select_list_1.attr("id", "SpecListOpt" + spec_num + "-1");
    item_select_list_2.attr("id", "SpecListOpt" + spec_num + "-2");

    item_price.on("click", function () {
        var isnull = true;
        var $self = $(this)
        var psid = $self.parents(".spec_list").data("psid")
        var temppsid = $self.parents(".spec_list").data("temppsid")
        var timePrice = $self.parents(".spec_list").data("timeprice")
        $price_modal.parents(".modal-body").first().data("psid", psid != null ? psid : "")
        $price_modal.parents(".modal-body").first().data("temppsid", temppsid != null ? temppsid : "")
        $("#TimePrice").prop("checked", timePrice);
        $("#TimePrice").trigger("change");
        var $suggestPriceInput = $("#PriceModal .suggest_price input");
        co.Form.bindNumberFormatter($suggestPriceInput);

        if (!!psid) {
            var index = suggest_price_list.findIndex(item => item["FK_PSId"] == psid);
            SetFormattedNumberValue($suggestPriceInput, suggest_price_list[index]["Price"]);
        } else {
            var index = suggest_price_list.findIndex(item => item["TempPSid"] == temppsid);
            SetFormattedNumberValue($suggestPriceInput, suggest_price_list[index]["Price"]);
        }

        modal_price_list.forEach(function (item) {
            if (!item.IsDelete && (item.FK_PSId == psid || (item.TempPSid != null && item.TempPSid == temppsid))) {
                SpecPriceAdd(item)
                isnull = false;
            }
        })
        if (isnull) {
            SpecPriceAdd(null)
        }
        priceModal.show();
    })

    if (result != null) {
        item_btn_expand.children("span").text("expand_more");
        item_btn_expand.parents("div").first().prev().removeClass("show")
    }

    item_btn_expand.on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_less") {
            $self.children("span").text("expand_more")
        } else {
            $self.children("span").text("expand_less")
        }
    })

    item_btn_delete.on("click", function (e) {
        e.preventDefault();
        var $self = $(this);
        var $self_p = $self.parents('.spec_list');
        co.sweet.confirm("移除規格", "確定要移除此項規格嗎?", "　是　", "　否　", function () {
            spec_remove_list.push($self_p.data("psid"));
            spec_num -= 1;
            if (item.data("serno") < $("#Spec_Frame").data("spec_num")) { SortChange($(".spec_list"), "bigger", item.data("serno"), $("#Spec_Frame").data("spec_num")); }
            $self_p.remove();
            $("#Spec_Frame").data("spec_num", spec_num)
        })
    })

    item.find(".spec_select").each(function () {
        $self = $(this);
        var $spec_input = $self.siblings(".input_spec");
        $self.on("change", function () {
            var $spec_type = $(this);
            var $spec_bro = $spec_type.parents(".spec").first().siblings(".spec");
            var $spec_list = $spec_type.siblings("datalist");

            $spec_input.val("");
            $spec_list.children("option").each(function () {
                $(this).remove();
            })
            $spec_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                child.removeAttr("disabled");
                child.removeClass("bg-secondary-light25");
            })

            if ($spec_type.val() == 0) {
                $spec_input.attr("disabled", "disabled")
            } else {
                $spec_bro.children(".spec_select").children("option").each(function () {
                    var child = $(this)
                    if (child.val() == $spec_type.val()) {
                        child.attr("disabled", "disabled");
                        child.addClass("bg-secondary-light25");
                    }
                })
                $spec_input.removeAttr("disabled")
                var temp_spec_list = spec_pick_list.find(item => item.id == $spec_type.val())
                if (temp_spec_list.specs.length > 0) {
                    temp_spec_list.specs.forEach(item => {
                        $spec_list.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    })
                }
            }
        })
        $self.siblings(".input_spec").blur(function () {
            SpecBlurFunction($(this));
        })
    })

    $("#Spec_Frame ul .btn_spec_add").before(item);

    $price = $(".input_price");
    $stock_number = $(".input_stock_number");
    $packingPoint_number = $(".input_packingPoint_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");

    $alert_number = $(".input_alert_number");
    if ($noStockManagement && $noStockManagement.is(":checked")) {
        item.find(".input_stock_number, .input_alert_number").attr("disabled", "disabled");
    }

    $("input[type='number']").on("input", function () {
        var $self = $(this);
        var value = $self.val();
        if (value !== "" && parseFloat(value) < 0) $self.val("0");
    });
}
function SpecBlurFunction($spec) {
    var $option; var id;
    if ($spec.val() != "") {
        id = 0;
        $spec.each(function () {
            $self_input = $(this);
            $self_input.siblings("datalist").children("option").each(function () {
                $option = $(this);
                if ($option.val() == $self_input.val()) {
                    id = $option.data("sid");
                }
            })
        })
        if (id == 0) {
            co.Spec.SpecAddUp({ FK_Tid: $spec.prev("select").val(), Title: $spec.val() }).done(function (result) {
                if (result.success) {
                    co.Spec.GetPickSpecList().done(function (pick_result) {
                        spec_pick_list = pick_result;
                        $self_input.siblings("datalist").append(`<option value="${$spec.val()}" data-sid="${result.message}"></option>`)
                    });
                }
            });
        }
    }
}
function ISpecRepect() {
    var obj = []
    var temp_list = []
    var isRepect = false;
    $("#Spec_Frame .spec_list").each(function () {
        $self = $(this);
        $self.find(".input_spec").each(function () {
            obj.push($(this).val());
        })
        if (temp_list.find(item => item[0] == obj[0] && item[1] == obj[1]) != null) {
            isRepect = true;
        } else {
            temp_list.push(obj);
            obj = [];
        }
    })
    return isRepect;
}

function SpecRowKey($row) {
    var psid = $row.data("psid");
    if (psid != null && psid !== "" && typeof psid != "undefined") return "P" + psid;
    return "T" + $row.data("temppsid");
}

function convertSpecMedia(m) {
    var link = (m.link && m.link[0]) || "";
    return {
        Id: m.id,
        Name: m.name,
        File: m.fileType == 4 ? m.name : link,
        Type: m.fileType,
        Link: link,
        SerNo: m.serNo,
        IsDelete: false
    };
}

function OpenSpecMediaModal($row) {
    var key = SpecRowKey($row);
    var $block = $("#SpecMedia");
    var bucket = spec_media_map[key] || (spec_media_map[key] = []);

    // 容器指向該列的 bucket（co.File 之後讀寫都會落到這裡）
    $block.data("files", bucket);
    $block.data("spec-key", key);
    $block.data("file_num", 0);

    // 重建清單
    $block.find("ul > li.upload_list").remove();
    UploadPreviewFrameClear($block);
    bucket.filter(f => !f.IsDelete).forEach(function (f) {
        SpecMediaRowRender(f, $block);
    });

    // 預設顯示預覽：有圖就顯示第一張，沒有則顯示預設框
    var $default = $block.find(".preview_frame .default_frame");
    $block.find(".preview_frame .default_frame").addClass("d-none");

    var $items = $block.find("ul > li.upload_list");
    if ($items.length) {
        $items.first().trigger("click");                    // 有圖：顯示第一張，預設框保持隱藏
    }


    $block.data("spec-row", $row);
    specMediaModal.show();
}

// 從內部 obj 渲染一列（不 push，資料已在 bucket）
function SpecMediaRowRender(obj, $target) {
    var item = $($("#TemplateUploadList").html()).clone();
    var $ul = $target.children("ul");
    var file_num = $ul.find("li.upload_list").length + 1;

    item.data("uploadtype", obj.Type);
    item.data("edit", false);
    item.data("serno", file_num);
    item.find(".ser_no").val(file_num);
    if (typeof obj.Id != "undefined") item.data("id", obj.Id);
    else item.data("tempid", obj.TempId);
    item.find(".title").text(obj.Name || "");

    var file = obj.File;
    if (!!file) {
        switch (obj.Type) {
            case 2: item.find(".thumb_img").attr("src", "/images/defaultImage/360.jpg"); break;
            case 3: item.find(".thumb_img").attr("src", "/images/defaultImage/video.jpg"); break;
            case 4: item.find(".thumb_img").attr("src", `https://img.youtube.com/vi/${file}/hqdefault.jpg`); break;
            default: item.find(".thumb_img").attr("src", obj.Link || file); break;
        }
        var href = obj.Type == 4 ? `https://www.youtube.com/watch?v=${file}` : (obj.Link || file);
        item.find(".btn_link").removeClass("d-none").attr("href", href);
    } else item.find(".btn_link").addClass("d-none");

    item.on("click", function () { co.File.ListFile($(this)); });

    item.find(".ser_no").on("blur", function () {
        var $self = $(this);
        var $uploadList = $target.find(".upload_list");
        if ($self.val() < 1) $self.val(1);
        else if ($self.val() > $uploadList.length) $self.val($uploadList.length);
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($uploadList, "bigger", item.data("serno"), $self.val());
                $ul.children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($uploadList, "smaller", $self.val(), item.data("serno"));
                $ul.children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    });

    item.find(".btn_remove").on("click", function (e) {
        e.preventDefault();
        var $self = $(this).parents("li").first();
        var store = co.File.filesOf($target);
        if (typeof ($self.data("id")) != "undefined") {
            var s = store.find(f => f["Id"] == $self.data("id"));
            if (s) s["IsDelete"] = true;
        } else if (typeof ($self.data("tempid")) != "undefined") {
            var idx = store.findIndex(f => f["TempId"] == $self.data("tempid"));
            if (idx >= 0) store.splice(idx, 1);
        }
        UploadPreviewFrameClear($target);
        $self.remove();
    });

    $ul.children(".btn_upload_add").before(item);
    $target.data("file_num", file_num);
}

function UploadListAdd(result, $target) {
    var item = $($("#TemplateUploadList").html()).clone();
    var item_serno = item.find(".ser_no"),
        item_btn_remove = item.find(".btn_remove");
    var file_num = $target.find("ul > li").length - 1;
    var store = co.File.filesOf($target);
    var tempId = store.length;
    if (typeof (file_num) == "undefined") file_num = 0;
    if (result == null) {
        $target.find("ul > li").each(function () {
            var $self = $(this);
            if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
                $self.remove();
                file_num -= 1;
            }
        })

        file_num += 1;
        item.data("tempid", tempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        if ($target.find(".select_frame").length == 0 && typeof ($target.data("uploadtype")) != "undefined")
            item.data("uploadtype", $target.data("uploadtype"));
        else
            item.data("uploadtype", 0);
        item.data("edit", false);
        item.on("click", function () {
            co.File.ListFile($(this));
        })
    } else if (typeof (result.id) == "undefined") {
        item.data("tempid", result.TempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.Type);
        item.data("edit", false);
        item.find(".title").text(result.Name);
        if (!!result.Link) {
            item.find(".thumb_img").attr("src", result.Link);
        } else if (result.Type == 2)
            item.find(".thumb_img").attr("src", `/images/defaultImage/360.jpg`);
        else if (result.Type == 3)
            item.find(".thumb_img").attr("src", `/images/defaultImage/video.jpg`);
        item.on("click", function () {
            co.File.ListFile($(this));
        })
    } else {
        file_num += 1;

        item.data("id", result.id);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.fileType);
        item.data("edit", false);
        item.find(".title").text(result.name);

        var obj = {};
        obj["Id"] = result.id;
        obj["Name"] = result.name;
        var link = result.link[0];
        if (result.fileType == 4) {
            obj["File"] = result.name;
        } else {
            obj["File"] = link;
        }
        obj["Type"] = result.fileType;
        obj["IsDelete"] = false;
        if (!!obj["File"]) {
            switch (obj.Type) {
                case 2:
                    item.find(".thumb_img").attr("src", `/images/defaultImage/360.jpg`);
                    break;
                case 3:
                    item.find(".thumb_img").attr("src", `/images/defaultImage/video.jpg`);
                    break;
                case 4:
                    item.find(".thumb_img").attr("src", `https://img.youtube.com/vi/${obj["File"]}/hqdefault.jpg`);
                    break;
                default:
                    item.find(".thumb_img").attr("src", obj["File"]);
                    break;
            }
            item.find(".btn_link").removeClass("d-none").attr("href", obj["File"]);
        } else item.find(".btn_link").addClass("d-none");
        store.push(obj);

        item.on("click", function () {
            co.File.ListFile($(this));
        })
    }
    $target.data("file_num", file_num);
    item_serno.on("blur", function () {
        var $self = $(this);
        var $uploadList = $target.find(".upload_list");
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $uploadList.length) {
            $self.val($uploadList.length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($uploadList, "bigger", item.data("serno"), $self.val())
                $target.children("ul").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($uploadList, "smaller", $self.val(), item.data("serno"))
                $target.children("ul").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })

    item_btn_remove.on("click", function (e) {
        e.preventDefault();
        var $self = $(this).parents("li").first();
        var $uploadList = $target.find(".upload_list");
        if (item.data("serno") < $target.data("file_num")) {
            SortChange($uploadList, "bigger", item.data("serno"), $target.data("file_num"));
        }
        if (typeof ($self.data("id")) != "undefined") {
            store.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
        } else if (typeof ($self.data("tempid")) != "undefined") {
            var tempid = $self.data("tempid");
            var index = store.findIndex(item => item["TempId"] == tempid);
            if (index >= 0) {
                store.splice(index, 1);
                store.forEach(file => {
                    file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
                })
            }
        }
        UploadPreviewFrameClear($target);
        $self.remove();
        $target.data("file_num", $target.data("file_num") - 1);
    })

    $target.find("ul > .btn_upload_add").before(item);
    co.File.ListFile(item);
}
function UploadPreviewFrameClear($target) {
    var $self = $target.find(".preview_frame");
    $self.find(".default_frame").addClass("d-flex");
    $self.find(".upload_frame").addClass("d-none");
    $self.find(".media_frame").removeClass("d-flex");
    $self.find(".youtube_frame").removeClass("d-flex");
    $self.find(".select_frame").removeClass("d-flex");
    $self.find(".youtube_preview").empty();
    $self.find(".media_preview > div").empty();
}
/* ********** *****************
排序 沒有資料的情況下依舊可以拖動 需修改
***************************/
function SortChange($self, change, minindex, maxindex) {
    $self.each(function () {
        var $li_self = $(this)
        if (change == "bigger") {
            if ($li_self.data("serno") > minindex && $li_self.data("serno") <= maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) - 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        } else if (change == "smaller") {
            if ($li_self.data("serno") >= minindex && $li_self.data("serno") < maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) + 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        }
    })
}

function refreshSpecThumb($row) {
    var key = SpecRowKey($row);
    var bucket = spec_media_map[key] || [];
    var first = bucket.find(f => !f.IsDelete);
    var $thumb = $row.find(".spec_thumb");
    var $icon = $row.find(".spec_img_icon");
    if (first) {
        var src = first.Type == 4 ? `https://img.youtube.com/vi/${first.File}/hqdefault.jpg`
            : first.Type == 3 ? "/images/defaultImage/video.jpg"
                : (first.Link || first.File);
        $thumb.attr("src", src).removeClass("d-none");
        $icon.addClass("d-none");
    } else {
        $thumb.addClass("d-none");
        $icon.removeClass("d-none");
    }
}

function AddUp(success_text, error_text, target) {
    var stock_addup_list = []
    var status = parseInt($(`[name="ProdStatus"] > option:selected`).val() || 0);
    var updateStock = false;
    $("#Spec_Frame ul li.spec_list").each(function () {
        var $self = $(this);
        var obj = {};
        var fk_sid = [];
        $self.find(".input_spec").each(function () {
            var id = 0;
            $self_input = $(this);
            $self_input.siblings("datalist").children("option").each(function () {
                var $option = $(this);
                if ($option.val() == $self_input.val()) {
                    id = $option.data("sid");
                }
            })
            fk_sid.push(id)
        })
        obj["Id"] = $self.data("psid") == "" || typeof ($self.data("psid")) == "undefined" ? 0 : $self.data("psid");
        if (obj["Id"] != 0) {
            var index = suggest_price_list.findIndex(item => item["FK_PSId"] == obj["Id"])
            obj["Price"] = suggest_price_list[index]["Price"];
        } else {
            var index = suggest_price_list.findIndex(item => item["TempPSid"] == $self.data("temppsid"))
            obj["Price"] = suggest_price_list[index]["Price"];
        }
        obj["TimePrice"] = $self.data("timeprice");
        obj["Price"] = isNaN(obj["Price"]) ? 0 : Number(obj["Price"]);
        obj["FK_S1id"] = fk_sid[0];
        obj["FK_S2id"] = fk_sid[1];
        obj["PackingPoint"] = $self.find(".input_packingPoint_number").val() ?? 1;
        obj["Stock"] = $self.find(".input_stock_number").val();
        obj["Alert_Qty"] = $self.find(".input_alert_number").val();
        obj["Min_Qty"] = $self.find(".input_min_number").val();
        obj["Ser_No"] = $self.find(".ser_no").val();
        obj['OldStock'] = $self.data("oldstock");
        obj["TempPSid"] = $self.data("temppsid") || 0;
        obj['SubItemNo'] = $self.find(".input_subItemNo").val();
        obj["SpecDescription"] = $self.data("specdesc") || "";
        updateStock = updateStock || parseInt(obj["Stock"] || 0) > parseInt(obj['OldStock'] || 0);
        var price_list = [];
        modal_price_list.forEach(function (item) {
            if (item.FK_PSId == $self.data("psid") || item.TempPSid == $self.data("temppsid")) {
                var price_object = {};
                price_object["Id"] = item.Id;
                price_object["FK_PSId"] = item.FK_PSId;
                price_object["FK_RId"] = item.FK_RId;
                price_object["Price"] = item.Price;
                price_object["Bonus"] = item.Bonus;
                price_object["IsDelete"] = item.IsDelete;
                price_list.push(price_object)
            }
        })
        obj["Prices"] = price_list;
        stock_addup_list.push(obj);
    })

    const update = function () {
        co.Product.AddUp.Product({
            Id: keyId,
            Title: $name.val(),
            ItemNo: $itemNo.val(),
            Visible: $display.is(":checked"),
            PopularVisible: $popularVisible.is(":checked"),
            RemovedFromShelves: !$removedFromShelves.is(":checked"),
            NoStockManagement: $noStockManagement.is(":checked"),
            Ser_No: $("#SortCheck").is(":checked") ? $(`[name="serNo"]`).val() : 500,
            Introduction: $introduction.val(),
            Description: $illustrate.val(),
            StartTime: startDate,
            EndTime: endDate,
            Permanent: $permanent.is(":checked"),
            TagSelected: tag_list,
            TechCertSelected: techcert_list,
            Stocks: stock_addup_list,
            Status: status
        }).done(function (result) {
            var pid = parseInt(result.message);
            if (result.success) {
                Coker.sweet.success(success_text, null, true);
                var fileListSave = [];
                if (total_files.length > 0) {
                    $("#ProductForm .data_upload > ul > li").each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("btn_upload_add")) {
                            var data = [];
                            total_files.forEach(file => {
                                if ((typeof (file["Id"]) != "undefined" && file["Id"] == $self.data("id")) || (typeof (file["TempId"]) != "undefined" && file["TempId"] == $self.data("tempid"))) {
                                    data.push(file);
                                }
                            })
                            if (data.length > 0) {
                                switch (data[0]["Type"]) {
                                    case 1:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                Sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("type", 1);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            data.forEach(item => {
                                                for (var i = 0; i < item["File"].length; i++) {
                                                    formData.append("files", item["File"][i]);
                                                }
                                                fileListSave.push(
                                                    co.File.Upload(formData).done(function (result) {
                                                        var _dfr = $.Deferred()
                                                        if (result.success) {
                                                            for (let n = 0; n < data.length; n++) {
                                                                data[n].Id = result.files[n].id;
                                                                data[n].File = result.files[n].path;
                                                            }
                                                            return _dfr.resolve();
                                                        } else return _dfr.reject();
                                                        return _dfr.promise();
                                                    })
                                                );
                                                formData.delete('files');
                                            })
                                        }
                                        break;
                                    /* ********** *****************
                                  360 上傳資料庫，須重打
                                   ***************************/
                                    case 2:
                                        var formData = new FormData();
                                        formData.append("type", 1);
                                        formData.append("sid", pid);
                                        formData.append("serno", $self.find(".ser_no").val());
                                        for (var i = 0; i < data.length; i += 3) {
                                            for (var j = i; j < i + 3; j++) {
                                                formData.append('files', data[j]);
                                            }
                                            formData.delete('files');
                                        }
                                        break;
                                    /* ********** *****************
                                       影片上傳資料庫，不確定錯誤是否在這
                                        ***************************/
                                    case 3:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("files", data[0]["File"]);
                                            formData.append("type", 1);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            fileListSave.push(
                                                co.File.Upload(formData).done(function (result) {
                                                    if (result.success) {
                                                        data[0].Id = result.files[0].id;
                                                        data[0].File = result.files[0].path;
                                                    }
                                                })
                                            );
                                        }
                                        break;
                                    case 4:
                                        var Id = typeof (data[0]["Id"]) == "undefined" ? 0 : data[0]["Id"];
                                        fileListSave.push(
                                            co.File.UploadYTLink({
                                                Id: Id,
                                                File: data[0]["File"] + "",
                                                SId: pid,
                                                Type: 1,
                                                SerNo: $self.find(".ser_no").val(),
                                            }).done(function (result) {
                                                var _dfr = $.Deferred();
                                                if (result.success && typeof (result.files) != "undefined") {
                                                    data[0].Id = result.files[0].id;
                                                    return _dfr.resolve();
                                                } else return _dfr.reject();
                                                return _dfr.promise();
                                            })
                                        );
                                        break;
                                    case 5:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("files", data[0]["File"]);
                                            formData.append("type", 8);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            fileListSave.push(
                                                co.File.Upload(formData).done(function (result) {
                                                    var _dfr = $.Deferred();
                                                    if (result.success) {
                                                        data[0].Id = result.files[0].id;
                                                        data[0].File = result.files[0].path;
                                                        return _dfr.resolve();
                                                    } else return _dfr.reject();
                                                    return _dfr.promise();
                                                })
                                            );
                                        }
                                }
                            }
                        }
                    })

                    total_files.forEach(file => {
                        if (typeof (file["IsDelete"]) != "undefined" && file["IsDelete"] == true) {
                            switch (file["Type"]) {
                                /* ********** *****************
                               360檔案刪除未處理
                                ***************************/
                                case 2:
                                    break;
                                case 1:
                                case 3:
                                case 4:
                                case 5:
                                    if (typeof (file["Id"]) != "undefined") {
                                        var deleteid_list = [];
                                        deleteid_list.push(file["Id"]);
                                        fileListSave.push(
                                            co.File.DeleteFileById({
                                                Sid: parseInt(result.message),
                                                Type: (file["Type"] == 5 ? 8 : 1),
                                                Fid: deleteid_list,
                                            })
                                        );
                                    }
                                    break;
                            }
                        }
                    });

                    switch (target) {
                        case "List":
                            setTimeout(function () {
                                BackToList(true);
                            }, 1000);
                            break;
                        case "Canvas":
                            setTimeout(function () {
                                window.location.hash = `${pid}-1`;
                            }, 1000);
                            break;
                    }
                } else {
                    switch (target) {
                        case "List":
                            setTimeout(function () {
                                BackToList(true);
                            }, 1000);
                            break;
                        case "Canvas":
                            setTimeout(function () {
                                window.location.hash = `${pid}-1`;
                            }, 1000);
                            break;
                    }
                }

                // ===== 規格圖上傳/刪除 =====
                var stockIdMap = {};
                (result.object || []).forEach(function (m) {
                    stockIdMap["T" + m.tempPSid] = m.id;   // 新規格：temppsid -> 真實 id
                });
                Object.keys(spec_media_map).forEach(function (key) {
                    var stockId = key.charAt(0) === "P" ? parseInt(key.substring(1)) : stockIdMap[key];
                    if (!stockId) return;
                    var serno = 0;
                    spec_media_map[key].forEach(function (f) {
                        // 刪除既有
                        if (f.IsDelete) {
                            if (typeof f.Id != "undefined") {
                                fileListSave.push(co.File.DeleteFileById({
                                    Sid: stockId,
                                    Type: 16,               // 產品規格圖
                                    Fid: [f.Id]
                                }));
                            }
                            return;
                        }
                        serno += 1;
                        // 已存在（有 Id 且 File 是字串網址）→ 僅排序
                        if (typeof f.Id != "undefined" && typeof f.File == "string") {
                            fileListSave.push(co.File.fileSortChange({ Id: f.Id, Sid: stockId, SerNo: serno }));
                            return;
                        }
                        // 新增
                        switch (f.Type) {
                            case 1: {   // 圖片（f.File 是 [原圖, 壓縮, 縮圖] 陣列）
                                var fd = new FormData();
                                fd.append("type", 16);
                                fd.append("sid", stockId);
                                fd.append("serno", serno);
                                for (var i = 0; i < f.File.length; i++) fd.append("files", f.File[i]);
                                fileListSave.push(co.File.Upload(fd).done(function (r) {
                                    if (r.success) { f.Id = r.files[0].id; f.File = r.files[0].path; }
                                }));
                                break;
                            }
                            case 3: {   // 影片
                                var fd = new FormData();
                                fd.append("files", f.File);
                                fd.append("type", 16);
                                fd.append("sid", stockId);
                                fd.append("serno", serno);
                                fileListSave.push(co.File.Upload(fd).done(function (r) {
                                    if (r.success) { f.Id = r.files[0].id; f.File = r.files[0].path; }
                                }));
                                break;
                            }
                            case 4: {   // Youtube
                                fileListSave.push(co.File.UploadYTLink({
                                    Id: typeof f.Id == "undefined" ? 0 : f.Id,
                                    File: f.File + "",
                                    SId: stockId,
                                    Type: 16,
                                    SerNo: serno
                                }).done(function (r) {
                                    if (r.success && typeof r.files != "undefined") f.Id = r.files[0].id;
                                }));
                                break;
                            }
                            // case 2 (360) 比照商品圖目前未實作，先略
                        }
                    });
                });
                // ===== 規格圖結束 =====

                $.when.apply(null, fileListSave).done(function () {
                    HashDataEdit();
                });
            } else {
                Coker.sweet.error("錯誤", error_text, null, true);
            }
        }).fail(function () {
            Coker.sweet.error("錯誤", error_text, null, true);
        });
    }
    if (status == 2 && updateStock) {
        co.sweet.confirm("商品狀態即將變更", "您加大了商品可銷售量，商品售完狀態將被變更！", "確認", "取消", update);
    } else update()
    if (spec_remove_list.length > 0) {
        spec_remove_list.forEach(function (item) {
            co.Product.Delete.Stock(item);
        })
    }
}
function setTotalFile(obj) {
    total_files.forEach((index, item) => {
        obj.data.forEach((index2, item2) => {
            if (typeof (item.TempId) != "") {

            }
        });
    });
}
function MoveToContent() {
    if (keyId == 0) $("#ProductContent .card-header .titile").text("新增商品")
    else $("#ProductContent .card-header .titile").text("編輯商品")
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
    tagContentRefresh();
}
function MoveToCanvas() {
    $("#gjs").data("id", keyId);
    setPage(keyId);
    $("#TopLine > a").removeClass("d-none");
    $("#ProductList").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    $("#ProductCanvas").removeClass("d-none");
}
function BackToList(refresh) {
    $("#TopLine > a").addClass("d-none");
    $("#ProductList").removeClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    if (refresh) {
        window.location.hash = "";
        product_list.component.refresh();
    }
}
