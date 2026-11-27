(function () {
    "use strict";

    const DAY = "Day";
    const MONTH = "Month";
    const DAY_MS = 86400000;
    const number = new Intl.NumberFormat("zh-TW");
    const percent = new Intl.NumberFormat("zh-TW", { style: "percent", maximumFractionDigits: 1 });
    const signedPercent = new Intl.NumberFormat("zh-TW", {
        style: "percent", maximumFractionDigits: 1, signDisplay: "exceptZero"
    });

    const metrics = {
        amount: { text: "有效訂單金額", fields: ["orderCount", "amount"] },
        orderCount: { text: "有效訂單筆數", fields: ["orderCount"] },
        averageOrderValue: { text: "客單價", fields: ["averageOrderValue"] },
        invalid: { text: "無效訂單", fields: ["invalidOrderCount", "invalidAmount"] }
    };
    const trendSeries = {
        orderCount: { name: "有效訂單筆數", axis: "count", type: "bar", color: "#4285e7" },
        amount: { name: "有效訂單金額", axis: "money", type: "area", color: "#2e7d32" },
        averageOrderValue: { name: "客單價", axis: "money", type: "line", color: "#ad1457" },
        invalidOrderCount: { name: "無效訂單筆數", axis: "count", type: "bar", color: "#fb8c00" },
        invalidAmount: { name: "無效訂單金額", axis: "money", type: "area", color: "#c62828" }
    };

    let granularity = MONTH;
    let trendMetric = "amount";
    let startDateBox;
    let endDateBox;
    let grid;
    let chart;
    let reportRequest = 0;
    let maxima = {};
    let summaryAmount = 0;
    let summaryCount = 0;

    function toQueryDate(date) {
        return [
            String(date.getFullYear()).padStart(4, "0"),
            String(date.getMonth() + 1).padStart(2, "0"),
            String(date.getDate()).padStart(2, "0")
        ].join("-");
    }

    function rangeLabel(start, end, type) {
        const length = type === MONTH ? 7 : 10;
        return [start, end].map(date => toQueryDate(date).slice(0, length).replace(/-/g, "/")).join(" ～ ");
    }

    function money(value) {
        return "$" + number.format(value);
    }

    function countText(value) {
        return number.format(value) + " 筆";
    }

    function average(amount, count) {
        return count === 0 ? 0 : Math.sign(amount) * Math.round(Math.abs(amount) / count);
    }

    function calculateAverageOrderSummary(options) {
        if (options.name !== "averageOrderValue") return;

        if (options.summaryProcess === "start") {
            summaryAmount = 0;
            summaryCount = 0;
        } else if (options.summaryProcess === "calculate") {
            summaryAmount += options.value.amount;
            summaryCount += options.value.orderCount;
        } else if (options.summaryProcess === "finalize") {
            options.totalValue = average(summaryAmount, summaryCount);
        }
    }

    function getDefaultRange(type) {
        const today = new Date();
        const year = today.getFullYear();
        const month = today.getMonth();
        return type === MONTH
            ? { start: new Date(year, 0, 1), end: new Date(year, month, 1) }
            : { start: new Date(year, month, 1), end: today };
    }

    function shiftPeriod(date, offset, type) {
        const shifted = new Date(date);
        if (type === MONTH) {
            shifted.setDate(1);
            shifted.setMonth(shifted.getMonth() + offset);
        } else {
            shifted.setDate(shifted.getDate() + offset);
        }
        return shifted;
    }

    function periodCount(start, end, type) {
        if (type === MONTH) {
            return (end.getFullYear() - start.getFullYear()) * 12 + end.getMonth() - start.getMonth() + 1;
        }
        return (Date.parse(toQueryDate(end)) - Date.parse(toQueryDate(start))) / DAY_MS + 1;
    }

    function getDateBoxOptions(type) {
        return type === MONTH
            ? { displayFormat: "yyyy/MM", calendarOptions: { maxZoomLevel: "year", minZoomLevel: "century" } }
            : { displayFormat: "yyyy/MM/dd", calendarOptions: { maxZoomLevel: "month", minZoomLevel: "century" } };
    }

    function totalReport(items) {
        const totals = { amount: 0, orderCount: 0, invalidAmount: 0, invalidOrderCount: 0 };
        items.forEach(function (item) {
            totals.amount += item.amount;
            totals.orderCount += item.orderCount;
            totals.invalidAmount += item.invalidAmount;
            totals.invalidOrderCount += item.invalidOrderCount;
        });
        totals.average = average(totals.amount, totals.orderCount);
        return totals;
    }

    function renderSummary(totals) {
        $("#RevenueTotalAmount").text(totals ? money(totals.amount) : "—");
        $("#RevenueTotalCount").text(totals ? countText(totals.orderCount) : "—");
        $("#RevenueTotalAverage").text(totals ? money(totals.average) : "—");
        $("#RevenueTotalInvalidAmount").text(totals ? money(totals.invalidAmount) : "—");
        $("#RevenueTotalInvalidCount").text(totals ? countText(totals.invalidOrderCount) : "—");
    }

    function renderComparison(totals, previous) {
        const fields = {
            RevenueAmountChange: "amount",
            RevenueCountChange: "orderCount",
            RevenueAverageChange: "average"
        };
        Object.entries(fields).forEach(function ([id, field]) {
            const baseline = previous && previous[field];
            const change = baseline ? (totals[field] - baseline) / Math.abs(baseline) : null;
            $("#" + id)
                .text(change === null ? "—" : signedPercent.format(change))
                .toggleClass("revenue-change-up", change > 0)
                .toggleClass("revenue-change-down", change < 0)
                .attr("title", baseline === 0 ? "前期為 0，無法計算變動率" : "");
        });
    }

    function renderOrderStatus(items, totals) {
        const totalCount = totals ? totals.orderCount + totals.invalidOrderCount : 0;
        $("#RevenueValidShare").text(totalCount ? percent.format(totals.orderCount / totalCount) : "—");
        $("#RevenueInvalidShare").text(totalCount ? percent.format(totals.invalidOrderCount / totalCount) : "—");

        const list = $("#RevenueOrderStatusBars").empty().prop("hidden", !totalCount);
        $("#RevenueOrderStatusEmpty").text(totals && !totalCount ? "此區間沒有有效或無效訂單" : "");
        if (!totalCount) return;

        const maxCount = Math.max(...items.map(item => item.orderCount + item.invalidOrderCount));
        const bars = document.createDocumentFragment();
        items.forEach(function (item) {
            const description = "有效 " + countText(item.orderCount) + "，無效 " + countText(item.invalidOrderCount);
            const row = $("<li>").attr("title", item.periodLabel + "｜" + description);
            $("<span>").text(item.periodLabel).appendTo(row);
            $("<span>").addClass("visually-hidden").text(description).appendTo(row);
            const track = $("<span>").addClass("revenue-report-status-track").attr("aria-hidden", "true").appendTo(row);
            $("<span>").addClass("revenue-report-valid-bar").css("width", (item.orderCount / maxCount * 100) + "%").appendTo(track);
            $("<span>").addClass("revenue-report-invalid-bar").css("width", (item.invalidOrderCount / maxCount * 100) + "%").appendTo(track);
            bars.appendChild(row[0]);
        });
        list.append(bars);
    }

    function setTrendMetric(metric) {
        trendMetric = metric;
        const fields = metrics[metric].fields;
        const series = fields.map(function (field) {
            const options = trendSeries[field];
            const isMoney = options.axis === "money";
            return {
                ...options,
                valueField: field,
                maxLabelCount: 12,
                label: {
                    visible: isMoney || fields.length === 1,
                    backgroundColor: "transparent",
                    font: isMoney ? { color: options.color, size: 12, weight: 700 } : { color: "#344767", size: 11 },
                    customizeText: info => isMoney ? money(info.value) : number.format(info.value)
                }
            };
        });
        const moneySeries = series.find(item => item.axis === "money");
        const hasCount = series.some(item => item.axis === "count");
        const axes = [];
        if (moneySeries) {
            axes.push({
                name: "money", position: "left", title: "金額 (NT$)", showZero: true,
                label: { font: { color: moneySeries.color }, customizeText: info => money(info.value) },
                grid: { visible: true, opacity: 0.2 }
            });
        }
        if (hasCount) {
            axes.push({
                name: "count", position: moneySeries ? "right" : "left", title: "訂單筆數",
                showZero: true, allowDecimals: false, grid: { visible: !moneySeries, opacity: 0.2 }
            });
        }
        chart.option({ series: series, valueAxis: axes, "legend.visible": series.length > 1 });
    }

    function createTrendChart() {
        return $("#RevenueTrendChart").dxChart({
            dataSource: [],
            commonSeriesSettings: {
                argumentField: "periodLabel",
                bar: { maxBarWidth: 34 },
                area: {
                    opacity: 0.17,
                    border: { visible: true, width: 3 },
                    hoverStyle: { border: { visible: true, width: 3 } },
                    point: { visible: true, size: 7 }
                },
                line: { width: 3, point: { visible: true, size: 7 } }
            },
            argumentAxis: { type: "discrete", label: { overlappingBehavior: "hide" } },
            resolveLabelOverlapping: "hide",
            legend: { visible: false, horizontalAlignment: "center", verticalAlignment: "bottom" },
            zoomAndPan: { argumentAxis: "both", allowMouseWheel: false, dragToZoom: true },
            scrollBar: { visible: false },
            tooltip: {
                enabled: true,
                shared: true,
                customizeTooltip: function (info) {
                    const lines = (info.points || [info]).map(function (point) {
                        const series = Object.values(trendSeries).find(item => item.name === point.seriesName);
                        const value = series.axis === "money" ? money(point.value) : countText(point.value);
                        return series.name + "：" + value;
                    });
                    return { text: [info.argumentText, ...lines].join("\n") };
                }
            }
        }).dxChart("instance");
    }

    function renderReport(items, totals, type) {
        ["orderCount", "averageOrderValue", "invalidOrderCount"].forEach(function (field) {
            maxima[field] = Math.max(0, ...items.map(item => Math.abs(item[field])));
        });
        grid.columnOption("periodLabel", "caption", type === MONTH ? "月份" : "日期");
        grid.option("dataSource", items);
        grid.pageIndex(0);
        chart.resetVisualRange();
        chart.option("dataSource", items);
        renderSummary(totals);
        renderOrderStatus(items, totals);
    }

    function loadReport() {
        let start = startDateBox.option("value");
        let end = endDateBox.option("value");
        const type = granularity;
        if (!startDateBox.option("isValid") || !endDateBox.option("isValid")
            || !(start instanceof Date) || !(end instanceof Date) || isNaN(start) || isNaN(end)) {
            DevExpress.ui.notify("請選擇有效的查詢日期", "warning", 2000);
            return;
        }

        if (type === MONTH) {
            start = shiftPeriod(start, 0, MONTH);
            end = shiftPeriod(end, 0, MONTH);
        }
        const count = periodCount(start, end, type);
        const limit = type === MONTH ? 60 : 366;
        if (count < 1 || count > limit) {
            const message = count > limit ? "查詢區間最多 " + limit + (type === MONTH ? " 個月" : " 天")
                : "開始日期不可晚於結束日期";
            DevExpress.ui.notify(message, "warning", 2500);
            return;
        }

        const request = ++reportRequest;
        const results = $("#RevenueReportResults").attr("aria-busy", "true");
        const status = $("#RevenueReportStatus").text("正在載入營收報表…");
        const label = rangeLabel(start, end, type);
        renderComparison(null, null);
        grid.beginCustomLoading("載入中…");

        co.Order.GetRevenueReport(type, toQueryDate(start), toQueryDate(end))
            .done(function (result) {
                if (request !== reportRequest) return;

                const items = result.items || [];
                const totals = totalReport(items);
                renderReport(items, totals, type);
                status.text(label);

                const previousStart = shiftPeriod(start, -count, type);
                const previousEnd = shiftPeriod(start, -1, type);
                if (previousStart.getFullYear() < 1) return;

                co.Order.GetRevenueReport(type, toQueryDate(previousStart), toQueryDate(previousEnd))
                    .done(function (comparison) {
                        if (request !== reportRequest) return;

                        renderComparison(totals, totalReport(comparison.items || []));
                        status.text(label + "｜前期 " + rangeLabel(previousStart, previousEnd, type));
                    })
                    .fail(function () {
                        if (request === reportRequest) status.text(label + "｜前期資料載入失敗");
                    });
            })
            .fail(function () {
                if (request !== reportRequest) return;

                renderReport([], null, type);
                status.text("營收報表載入失敗，請重新查詢。");
            })
            .always(function () {
                if (request !== reportRequest) return;

                grid.endCustomLoading();
                results.attr("aria-busy", "false");
            });
    }

    function applyGranularity(type) {
        granularity = type;
        const range = getDefaultRange(type);
        const options = getDateBoxOptions(type);
        startDateBox.option({ ...options, value: range.start });
        endDateBox.option({ ...options, value: range.end });
        loadReport();
    }

    function renderBarCell(container, info) {
        const max = maxima[info.column.dataField];
        const width = max ? Math.abs(info.value) / max * 100 : 0;
        $("<div>").text(info.text).appendTo(container);
        const track = $("<div>").addClass("revenue-report-cell-track").attr("aria-hidden", "true").appendTo(container);
        $("<span>").css("width", width + "%").appendTo(track);
    }

    function moneyText(info) {
        return info.value == null ? "" : money(info.value);
    }

    function createGrid() {
        return $("#RevenueReportGrid").dxDataGrid({
            dataSource: [],
            keyExpr: "periodLabel",
            showBorders: true,
            showRowLines: true,
            columnAutoWidth: true,
            columnHidingEnabled: true,
            noDataText: "沒有可顯示的資料",
            paging: { pageSize: 31 },
            pager: {
                visible: true,
                showInfo: true,
                showPageSizeSelector: true,
                allowedPageSizes: [12, 31, 100]
            },
            columns: [
                { dataField: "periodLabel", caption: "日期", sortOrder: "asc" },
                { dataField: "orderCount", caption: "有效訂單筆數", dataType: "number", hidingPriority: 2,
                    cellTemplate: renderBarCell },
                { dataField: "amount", caption: "有效訂單金額", dataType: "number", customizeText: moneyText },
                { dataField: "averageOrderValue", caption: "客單價", dataType: "number", customizeText: moneyText,
                    hidingPriority: 3, cssClass: "revenue-report-cell-average", cellTemplate: renderBarCell },
                { dataField: "invalidOrderCount", caption: "無效訂單筆數", dataType: "number",
                    hidingPriority: 0, cssClass: "revenue-report-cell-invalid", cellTemplate: renderBarCell },
                { dataField: "invalidAmount", caption: "無效訂單金額", dataType: "number",
                    customizeText: moneyText, hidingPriority: 1 }
            ],
            summary: {
                totalItems: [
                    { column: "periodLabel", summaryType: "count", displayFormat: "共 {0} 期" },
                    { column: "orderCount", summaryType: "sum", displayFormat: "{0} 筆" },
                    { column: "amount", summaryType: "sum", customizeText: moneyText },
                    { column: "invalidOrderCount", summaryType: "sum", displayFormat: "{0} 筆" },
                    { column: "invalidAmount", summaryType: "sum", customizeText: moneyText },
                    { name: "averageOrderValue", showInColumn: "averageOrderValue", summaryType: "custom",
                        customizeText: moneyText }
                ],
                calculateCustomSummary: calculateAverageOrderSummary
            }
        }).dxDataGrid("instance");
    }

    $(function () {
        const period = new URLSearchParams(window.location.search).get("period");
        granularity = period === "thisMonth" || period === "lastMonth" ? DAY : MONTH;
        const range = getDefaultRange(granularity);
        if (period === "lastMonth") {
            range.end = shiftPeriod(range.start, -1, DAY);
            range.start = shiftPeriod(range.start, -1, MONTH);
        }

        $("#RevenueGranularity").dxButtonGroup({
            items: [{ text: "每日", granularity: DAY }, { text: "每月", granularity: MONTH }],
            keyExpr: "granularity",
            selectedItemKeys: [granularity],
            onItemClick: function (e) {
                if (e.itemData.granularity !== granularity) applyGranularity(e.itemData.granularity);
            }
        });

        const dateOptions = getDateBoxOptions(granularity);
        startDateBox = $("#RevenueStartDate")
            .dxDateBox({ ...dateOptions, type: "date", value: range.start, inputAttr: { "aria-label": "開始日期" } })
            .dxDateBox("instance");
        endDateBox = $("#RevenueEndDate")
            .dxDateBox({ ...dateOptions, type: "date", value: range.end, inputAttr: { "aria-label": "結束日期" } })
            .dxDateBox("instance");

        $("#RevenueQuery").dxButton({ text: "查詢", type: "default", icon: "search", onClick: loadReport });
        grid = createGrid();
        chart = createTrendChart();
        setTrendMetric(trendMetric);
        $("#RevenueTrendMetric").dxButtonGroup({
            items: Object.entries(metrics).map(([metric, options]) => ({ metric: metric, text: options.text })),
            keyExpr: "metric",
            selectedItemKeys: [trendMetric],
            stylingMode: "outlined",
            onItemClick: function (e) {
                if (e.itemData.metric !== trendMetric) setTrendMetric(e.itemData.metric);
            }
        });
        loadReport();
    });
})();
