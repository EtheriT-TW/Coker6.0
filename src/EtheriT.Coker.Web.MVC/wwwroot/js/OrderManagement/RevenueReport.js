(function () {
    "use strict";

    var GRANULARITY_DAY = "Day";
    var GRANULARITY_MONTH = "Month";

    var granularity = GRANULARITY_DAY;
    var startDateBox;
    var endDateBox;
    var grid;

    function pad(value) {
        return value < 10 ? "0" + value : String(value);
    }

    function toQueryDate(date) {
        return date.getFullYear() + "-" + pad(date.getMonth() + 1) + "-" + pad(date.getDate());
    }

    function formatMoney(info) {
        return "$" + Number(info.value || 0).toLocaleString();
    }

    var summaryAmount = 0;
    var summaryOrderCount = 0;

    function calculateAverageOrderSummary(options){
                if (options.name !== "averageOrderValue") return;

        if (options.summaryProcess === "start") {
            summaryAmount = 0;
            summaryOrderCount = 0;
            return;
        }

        if (options.summaryProcess === "calculate") {
            summaryAmount += options.value.amount || 0;
            summaryOrderCount += options.value.orderCount || 0;
            return;
        }

        if (options.summaryProcess === "finalize") {
            options.totalValue = summaryOrderCount === 0 ? 0 : Math.round(summaryAmount / summaryOrderCount);
        }
    }

    function getDefaultRange(type) {
        var today = new Date();
        if (type === GRANULARITY_MONTH) {
            return {
                start: new Date(today.getFullYear(), 0, 1),
                end: new Date(today.getFullYear(), today.getMonth(), 1)
            };
        }
        return {
            start: new Date(today.getFullYear(), today.getMonth(), 1),
            end: today
        };
    }

    function getInitialRange(){
        var period = new URLSearchParams(window.location.search).get("period");
        if(period!=="lastMonth")
            return getDefaultRange(GRANULARITY_DAY);

        var today = new Date();
        return{
            start: new Date(today.getFullYear(), today.getMonth() -1, 1),
            end: new Date(today.getFullYear(), today.getMonth(), 0)
        }
    }

    function getDateBoxOptions(type) {
        if (type === GRANULARITY_MONTH) {
            return {
                displayFormat: "yyyy/MM",
                calendarOptions: { maxZoomLevel: "year", minZoomLevel: "century" }
            };
        }
        return {
            displayFormat: "yyyy/MM/dd",
            calendarOptions: { maxZoomLevel: "month", minZoomLevel: "century" }
        };
    }

    function loadReport() {
        var startDate = startDateBox.option("value");
        var endDate = endDateBox.option("value");
        if (!startDate || !endDate) {
            DevExpress.ui.notify("請選擇查詢區間", "warning", 2000);
            return;
        }

        grid.beginCustomLoading("載入中...");
        co.Order.GetRevenueReport(granularity, toQueryDate(startDate), toQueryDate(endDate))
            .done(function (result) {
                grid.option("dataSource", result.items || []);
            })
            .fail(function (xhr) {
                DevExpress.ui.notify(xhr.responseText || "營收報表載入失敗", "error", 3000);
            })
            .always(function () {
                grid.endCustomLoading();
            });
    }

    function applyGranularity(type) {
        granularity = type;
        var range = getDefaultRange(type);
        var options = getDateBoxOptions(type);
        startDateBox.option($.extend({ value: range.start }, options));
        endDateBox.option($.extend({ value: range.end }, options));
        grid.columnOption("periodLabel", "caption", type === GRANULARITY_MONTH ? "月份" : "日期");
        loadReport();
    }

    function createGrid() {
        return $("#RevenueReportGrid").dxDataGrid({
            dataSource: [],
            keyExpr: "periodLabel",
            showBorders: true,
            showRowLines: true,
            columnAutoWidth: true,
            columnHidingEnabled: true,
            noDataText: "請選擇查詢區間",
            paging: { pageSize: 31 },
            pager: {
                visible: true,
                showInfo: true,
                showPageSizeSelector: true,
                allowedPageSizes: [12, 31, 100]
            },
            columns: [
                { dataField: "periodLabel", caption: "日期", sortOrder: "asc" },
                { dataField: "orderCount", caption: "有效訂單筆數", dataType: "number", hidingPriority: 2 },
                { dataField: "amount", caption: "有效訂單金額", dataType: "number", customizeText: formatMoney },
                { dataField: "averageOrderValue", caption: "客單價", dataType: "number", customizeText: formatMoney, hidingPriority: 3 },
                { dataField: "invalidOrderCount", caption: "無效訂單筆數", dataType: "number", hidingPriority: 0 },
                { dataField: "invalidAmount", caption: "無效訂單金額", dataType: "number", customizeText: formatMoney, hidingPriority: 1 }
            ],
            summary: {
                totalItems: [
                    { column: "periodLabel", summaryType: "count", displayFormat: "共 {0} 期" },
                    { column: "orderCount", summaryType: "sum", displayFormat: "{0} 筆" },
                    { column: "amount", summaryType: "sum", customizeText: formatMoney },
                    { column: "invalidOrderCount", summaryType: "sum", displayFormat: "{0} 筆" },
                    { column: "invalidAmount", summaryType: "sum", customizeText: formatMoney },
                    { name: "averageOrderValue", showInColumn: "averageOrderValue", summaryType: "custom", customizeText: formatMoney }
                ],
                calculateCustomSummary: calculateAverageOrderSummary
            }
        }).dxDataGrid("instance");
    }

    $(function () {
        var range = getInitialRange();

        $("#RevenueGranularity").dxButtonGroup({
            items: [
                { text: "每日", granularity: GRANULARITY_DAY },
                { text: "每月", granularity: GRANULARITY_MONTH }
            ],
            keyExpr: "granularity",
            selectedItemKeys: [GRANULARITY_DAY],
            onItemClick: function (e) {
                if (e.itemData.granularity !== granularity) applyGranularity(e.itemData.granularity);
            }
        });

        startDateBox = $("#RevenueStartDate")
            .dxDateBox($.extend({ type: "date", value: range.start }, getDateBoxOptions(GRANULARITY_DAY)))
            .dxDateBox("instance");
        endDateBox = $("#RevenueEndDate")
            .dxDateBox($.extend({ type: "date", value: range.end }, getDateBoxOptions(GRANULARITY_DAY)))
            .dxDateBox("instance");

        $("#RevenueQuery").dxButton({ text: "查詢", type: "default", icon: "search", onClick: loadReport });

        grid = createGrid();
        loadReport();
    });
})();