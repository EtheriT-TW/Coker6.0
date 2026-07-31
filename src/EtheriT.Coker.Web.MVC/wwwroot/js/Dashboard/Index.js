var PageReady = function () {
    "use strict";

    var trafficChart = null;
    var trafficHeatmap = null;
    var orderTrendChart = null;
    var trafficRequest = null;
    var heatmapRequest = null;
    var popularPagesRequest = null;
    var orderTrendRequest = null;
    var weekdayLabels = ["", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日"];

    function number(value) {
        return co.String.thousandSign(Number(value) || 0);
    }

    function money(value) {
        return "NT$ " + number(Math.round(Number(value) || 0));
    }

    function renderChange($target, current, previous) {
        current = Number(current) || 0;
        previous = Number(previous) || 0;
        $target.removeClass("is-up is-down");

        if (previous === 0) {
            if (current > 0) {
                $target.addClass("is-up").text("新增");
            } else {
                $target.text("持平");
            }
            return;
        }

        var change = ((current - previous) / previous) * 100;
        if (change > 0) $target.addClass("is-up");
        if (change < 0) $target.addClass("is-down");
        $target.text(
            (change > 0 ? "+" : "") + change.toFixed(1) + "%"
        );
    }

    function text(value, fallback) {
        return value == null || value === "" ? fallback : String(value);
    }

    function formatDate(value, includeTime) {
        var date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";

        var options = includeTime
            ? { month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit", hour12: false }
            : { month: "2-digit", day: "2-digit" };

        return new Intl.DateTimeFormat("zh-TW", options).format(date);
    }

    function toInputDate(date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, "0");
        var day = String(date.getDate()).padStart(2, "0");
        return year + "-" + month + "-" + day;
    }

    function formatInputDate(value) {
        return value ? value.replace(/-/g, "/") : "";
    }

    function formatHour(value) {
        var date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";
        return String(date.getHours()).padStart(2, "0") + ":00";
    }

    function setActiveButton($buttons, $active) {
        $buttons
            .removeClass("is-active")
            .attr("aria-pressed", "false");
        $active
            .addClass("is-active")
            .attr("aria-pressed", "true");
    }

    function showEmpty($target, message) {
        $("<div>")
            .addClass("dashboard-empty")
            .text(message)
            .appendTo($target.empty());
    }

    function showError($loading, message) {
        $loading
            .addClass("dashboard-error")
            .text(message)
            .show();
    }

    function refreshOnlineVisitors() {
        if (document.hidden) return;

        $.get("/api/Remote/GetOnlineVisitorCount")
            .done(function (result) {
                $(".online-visitor-count").text(number(result.onlineVisitors));
            });
    }

    function loadSystemOverview() {
        $.get("/api/Dashboard/GetSystemOverview")
            .done(function (result) {
                $(".storage-size").text(text(result.storageSize, "無法取得"));
                $(".storage-updated-at").text(text(result.storageUpdatedAt, "無法取得"));
                $(".month-flow-size").text(text(result.monthFlowSize, "無法取得"));
                $(".month-flow-range").text(text(result.monthRange, "無法取得"));
            })
            .fail(function () {
                $(".storage-size, .month-flow-size").text("載入失敗");
                $(".storage-updated-at, .month-flow-range").text("請稍後重新整理");
            });
    }

    function loadTraffic(parameters, title) {
        var $loading = $(".traffic-loading");
        $loading
            .removeClass("dashboard-error")
            .text("正在載入流量資料…")
            .show();
        $(".traffic-title").text(title);

        if (trafficRequest) trafficRequest.abort();
        trafficRequest = $.get("/api/Dashboard/GetTraffic", parameters)
            .done(function (result) {
                var items = result.items || [];
                var $chart = $("#dashboard-traffic-chart");
                if (!$chart.length) return;

                var chartData = items.map(function (item) {
                    return {
                        argument: result.granularity === "hour"
                            ? formatHour(item.date)
                            : formatDate(item.date, false),
                        pageViews: item.pageViews || 0,
                        visitors: item.visitors || 0
                    };
                });

                if (!trafficChart) {
                    trafficChart = $chart.dxChart({
                        dataSource: chartData,
                        commonSeriesSettings: {
                            argumentField: "argument",
                            type: "bar",
                            hoverMode: "allArgumentPoints"
                        },
                        series: [
                            {
                                valueField: "pageViews",
                                name: "有效瀏覽人次",
                                color: "#4285e7"
                            },
                            {
                                valueField: "visitors",
                                name: "不重複訪客",
                                color: "#7fc283"
                            }
                        ],
                        argumentAxis: {
                            grid: { visible: false },
                            label: { overlappingBehavior: "stagger" }
                        },
                        valueAxis: {
                            allowDecimals: false,
                            visualRange: { startValue: 0 },
                            grid: { opacity: 0.2 }
                        },
                        legend: {
                            horizontalAlignment: "center",
                            verticalAlignment: "bottom"
                        },
                        tooltip: {
                            enabled: true,
                            shared: true
                        },
                        adaptiveLayout: {
                            width: 420,
                            height: 240
                        }
                    }).dxChart("instance");
                } else {
                    trafficChart.option("dataSource", chartData);
                }

                $(".traffic-updated-at").text("更新：" + formatDate(result.updatedAt, true));
                $loading.hide();
            })
            .fail(function (_xhr, status) {
                if (status === "abort") return;
                showError($loading, "流量資料載入失敗");
            })
            .always(function (_result, status) {
                if (status !== "abort") trafficRequest = null;
            });
    }

    function loadPopularPages(period, periodTitle) {
        var $loading = $(".popular-pages-loading");
        var $list = $(".popular-page-list");
        $loading
            .removeClass("dashboard-error")
            .text("正在載入熱門頁面…")
            .show();
        $(".popular-pages-title").text(periodTitle + "熱門頁面");

        if (popularPagesRequest) popularPagesRequest.abort();
        popularPagesRequest = $.get("/api/Dashboard/GetPopularPages", { period: period, take: 5 })
            .done(function (items) {
                $list.empty();
                if (!items || items.length === 0) {
                    showEmpty($list, periodTitle + "尚無有效瀏覽資料");
                } else {
                    items.forEach(function (item, index) {
                        var $row = $("<div>").addClass("popular-page-item");
                        $("<div>").addClass("popular-page-rank").text(index + 1).appendTo($row);

                        var $content = $("<div>").addClass("popular-page-content").appendTo($row);
                        $("<div>").addClass("popular-page-title").text(text(item.title, "未命名頁面")).appendTo($content);
                        $("<div>")
                            .addClass("popular-page-meta")
                            .text(text(item.contentType, "頁面") + "・" + number(item.visitors) + " 位訪客")
                            .appendTo($content);

                        $("<div>")
                            .addClass("popular-page-views")
                            .text(number(item.views))
                            .attr("title", "有效瀏覽人次")
                            .appendTo($row);

                        $row.appendTo($list);
                    });
                }
                $loading.hide();
            })
            .fail(function (_xhr, status) {
                if (status === "abort") return;
                showError($loading, "熱門頁面載入失敗");
            })
            .always(function (_result, status) {
                if (status !== "abort") popularPagesRequest = null;
            });
    }

    function refreshOrderInsightsLayout() {
        var $section = $(".dashboard-order-trend-section");
        var $visibleColumns = $(".dashboard-order-insight-column").filter(function () {
            return !this.hidden;
        });

        $section
            .toggleClass("has-single-panel", $visibleColumns.length === 1)
            .prop("hidden", $visibleColumns.length === 0);
    }

    function loadOrderTrend(days) {
        var $trendColumn = $(".dashboard-order-trend-column");
        var $loading = $(".order-trend-loading");
        $loading
            .removeClass("dashboard-error")
            .text("正在載入訂單趨勢…")
            .show();

        if (orderTrendRequest) orderTrendRequest.abort();
        orderTrendRequest = $.get("/api/Dashboard/GetOrderTrend", { days: days })
            .done(function (result) {
                if (!result.isCommerceEnabled) {
                    $trendColumn.prop("hidden", true);
                    refreshOrderInsightsLayout();
                    return;
                }

                var chartData = (result.items || []).map(function (item) {
                    return {
                        argument: formatDate(item.date, false),
                        orderCount: item.orderCount || 0,
                        revenue: Number(item.revenue) || 0
                    };
                });
                var hasTrendData = chartData.some(function (item) {
                    return item.orderCount > 0;
                });

                if (!hasTrendData) {
                    $trendColumn.prop("hidden", true);
                    $loading.hide();
                    refreshOrderInsightsLayout();
                    return;
                }

                $trendColumn.prop("hidden", false);
                refreshOrderInsightsLayout();

                var $chart = $("#dashboard-order-trend-chart");
                if (!$chart.length) return;

                var options = {
                    dataSource: chartData,
                    commonSeriesSettings: {
                        argumentField: "argument",
                        hoverMode: "allArgumentPoints"
                    },
                    series: [
                        {
                            type: "line",
                            valueField: "orderCount",
                            name: "有效訂單數",
                            axis: "orderCount",
                            color: "#4285e7",
                            point: { visible: days === 7, size: 7 }
                        },
                        {
                            type: "line",
                            valueField: "revenue",
                            name: "有效訂單金額",
                            axis: "revenue",
                            color: "#43a047",
                            point: { visible: days === 7, size: 7 }
                        }
                    ],
                    argumentAxis: {
                        grid: { visible: false },
                        label: {
                            overlappingBehavior: days === 30 ? "stagger" : "none"
                        }
                    },
                    valueAxis: [
                        {
                            name: "orderCount",
                            position: "left",
                            allowDecimals: false,
                            visualRange: { startValue: 0 },
                            title: { text: "訂單數" },
                            grid: { opacity: 0.2 }
                        },
                        {
                            name: "revenue",
                            position: "right",
                            visualRange: { startValue: 0 },
                            title: { text: "訂單金額" },
                            label: {
                                customizeText: function () {
                                    return "NT$ " + number(this.value);
                                }
                            },
                            grid: { visible: false }
                        }
                    ],
                    legend: {
                        horizontalAlignment: "center",
                        verticalAlignment: "bottom"
                    },
                    tooltip: {
                        enabled: true,
                        shared: true
                    },
                    adaptiveLayout: {
                        width: 520,
                        height: 260
                    }
                };

                if (!orderTrendChart) {
                    orderTrendChart = $chart.dxChart(options).dxChart("instance");
                } else {
                    orderTrendChart.option(options);
                }

                $(".order-trend-title").text("近 " + days + " 天有效訂單趨勢");
                $(".order-trend-range").text(
                    formatDate(result.startDate, false) + "－" + formatDate(result.endDate, false)
                );
                $loading.hide();
            })
            .fail(function (_xhr, status) {
                if (status === "abort") return;
                $trendColumn.prop("hidden", false);
                refreshOrderInsightsLayout();
                showError($loading, "訂單趨勢載入失敗");
            })
            .always(function (_result, status) {
                if (status !== "abort") orderTrendRequest = null;
            });
    }

    function heatmapColor(value, maximum) {
        if (!maximum || value <= 0) return "#f5f8fc";

        var ratio = Math.min(value / maximum, 1);
        var alpha = 0.12 + (ratio * 0.82);
        return "rgba(26, 115, 232, " + alpha.toFixed(3) + ")";
    }

    function renderRecommendedSlots(items) {
        var $target = $(".dashboard-recommended-slots").empty();
        if (!items || items.length === 0) {
            $("<div>")
                .addClass("dashboard-heatmap-note")
                .text("目前尚無足夠資料可顯示熱門瀏覽時段")
                .appendTo($target);
            return;
        }

        var $list = $("<div>").addClass("dashboard-recommendation-list").appendTo($target);
        items.forEach(function (item, index) {
            var hour = String(item.hour).padStart(2, "0") + ":00";
            var nextHour = String((item.hour + 1) % 24).padStart(2, "0") + ":00";
            var $item = $("<div>").addClass("dashboard-recommendation-item").appendTo($list);
            $("<span>").addClass("dashboard-recommendation-rank").text(index + 1).appendTo($item);

            var $content = $("<span>").addClass("dashboard-recommendation-content").appendTo($item);
            $("<strong>")
                .text(weekdayLabels[item.dayOfWeek] + " " + hour + "－" + nextHour)
                .appendTo($content);
            $("<small>")
                .text("平均 " + Number(item.averageVisitors || 0).toFixed(1)
                    + " 位訪客・統計 " + item.sampleDays + " 天")
                .appendTo($content);
        });
    }

    function renderTrafficHeatmap(result) {
        var items = result.items || [];
        var maximum = items.reduce(function (current, item) {
            return Math.max(current, Number(item.averageVisitors) || 0);
        }, 0);
        var rows = [];

        for (var dayOfWeek = 1; dayOfWeek <= 7; dayOfWeek++) {
            var cells = {};
            items
                .filter(function (item) { return item.dayOfWeek === dayOfWeek; })
                .forEach(function (item) { cells[item.hour] = item; });
            rows.push({
                dayOfWeek: dayOfWeek,
                dayLabel: weekdayLabels[dayOfWeek],
                cells: cells
            });
        }

        var columns = [{
            dataField: "dayLabel",
            caption: "星期／小時",
            width: 82,
            fixed: true,
            allowSorting: false,
            cssClass: "dashboard-heatmap-weekday"
        }];

        for (var hour = 0; hour < 24; hour++) {
            (function (columnHour) {
                columns.push({
                    caption: String(columnHour).padStart(2, "0") + "時",
                    allowSorting: false,
                    calculateCellValue: function (row) {
                        var cell = row.cells[columnHour];
                        return cell ? cell.averageVisitors : 0;
                    },
                    cellTemplate: function (container, info) {
                        var cell = info.data.cells[columnHour] || {
                            averageVisitors: 0,
                            averageViews: 0,
                            totalVisitors: 0,
                            sampleDays: 0
                        };
                        var value = Number(cell.averageVisitors) || 0;
                        var ratio = maximum ? value / maximum : 0;
                        var label = cell.sampleDays > 0 ? value.toFixed(1) : "—";
                        var title = info.data.dayLabel + " "
                            + String(columnHour).padStart(2, "0") + ":00－"
                            + String((columnHour + 1) % 24).padStart(2, "0") + ":00"
                            + "\n平均訪客人數：" + value.toFixed(1)
                            + "\n平均瀏覽次數：" + Number(cell.averageViews || 0).toFixed(1)
                            + "\n訪客人數合計：" + number(cell.totalVisitors)
                            + "\n統計天數：" + cell.sampleDays;

                        $("<div>")
                            .addClass("dashboard-heatmap-cell")
                            .css({
                                backgroundColor: heatmapColor(value, maximum),
                                color: ratio >= 0.72 ? "#fff" : "#344767"
                            })
                            .attr("title", title)
                            .text(label)
                            .appendTo(container);
                    }
                });
            })(hour);
        }

        var options = {
            dataSource: rows,
            columns: columns,
            keyExpr: "dayOfWeek",
            showBorders: true,
            showColumnLines: true,
            showRowLines: true,
            rowAlternationEnabled: false,
            hoverStateEnabled: true,
            allowColumnResizing: false,
            columnAutoWidth: false,
            sorting: { mode: "none" },
            paging: { enabled: false },
            scrolling: {
                mode: "standard",
                showScrollbar: "always",
                useNative: true
            },
            loadPanel: { enabled: false },
            noDataText: "尚無小時統計資料",
            onCellPrepared: function (event) {
                if (event.rowType === "header" && event.columnIndex > 0) {
                    var hour = event.columnIndex - 1;
                    event.cellElement.attr(
                        "title",
                        String(hour).padStart(2, "0") + ":00－"
                            + String((hour + 1) % 24).padStart(2, "0") + ":00"
                    );
                }
            }
        };

        if (!trafficHeatmap) {
            trafficHeatmap = $("#dashboard-traffic-heatmap")
                .dxDataGrid(options)
                .dxDataGrid("instance");
        } else {
            trafficHeatmap.option(options);
        }

        renderRecommendedSlots(result.recommendedSlots);
    }

    function loadTrafficHeatmap(days) {
        var $loading = $(".heatmap-loading");
        var $content = $(".dashboard-heatmap-content");
        $loading
            .removeClass("dashboard-error")
            .text("正在載入訪客時段資料…")
            .show();
        $content.hide();

        if (heatmapRequest) heatmapRequest.abort();
        heatmapRequest = $.get("/api/Dashboard/GetTrafficHeatmap", { days: days })
            .done(function (result) {
                renderTrafficHeatmap(result);

                var range = formatDate(result.startDate, false) + "－" + formatDate(result.endDate, false);
                var summary = range + "・已統計 " + number(result.availableDays) + " 個完整日";
                if (result.updatedAt) summary += "・更新 " + formatDate(result.updatedAt, true);
                $(".heatmap-summary").text(summary);

                $content.show();
                $loading.hide();
            })
            .fail(function (_xhr, status) {
                if (status === "abort") return;
                showError($loading, "訪客時段資料載入失敗");
            })
            .always(function (_result, status) {
                if (status !== "abort") heatmapRequest = null;
            });
    }

    function initializeDashboardRanges() {
        var today = new Date();
        var sevenDaysAgo = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 6);
        var todayValue = toInputDate(today);
        var $trafficButtons = $(".traffic-range-button, .traffic-custom-toggle");
        var $startDate = $(".traffic-start-date");
        var $endDate = $(".traffic-end-date");

        $startDate.val(toInputDate(sevenDaysAgo)).attr("max", todayValue);
        $endDate.val(todayValue).attr("max", todayValue);
        $(".dashboard-filter-button").attr("aria-pressed", function () {
            return $(this).hasClass("is-active") ? "true" : "false";
        });

        $(".traffic-range-button").on("click", function () {
            var $button = $(this);
            var days = Number($button.data("days"));
            var title = days === 1 ? "今日瀏覽趨勢" : "最近 " + days + " 天瀏覽趨勢";

            setActiveButton($trafficButtons, $button);
            $(".dashboard-custom-range").prop("hidden", true);
            $(".traffic-range-error").text("");
            loadTraffic(
                {
                    days: days,
                    granularity: days === 1 ? "hour" : "day"
                },
                title
            );
        });

        $(".traffic-custom-toggle").on("click", function () {
            setActiveButton($trafficButtons, $(this));
            $(".dashboard-custom-range").prop("hidden", false);
            $(".traffic-range-error").text("");
        });

        $(".traffic-custom-apply").on("click", function () {
            var startDate = $startDate.val();
            var endDate = $endDate.val();
            var $error = $(".traffic-range-error");

            if (!startDate || !endDate) {
                $error.text("請選擇開始與結束日期");
                return;
            }

            var dayCount = Math.round(
                (new Date(endDate + "T00:00:00") - new Date(startDate + "T00:00:00"))
                / 86400000
            ) + 1;
            if (dayCount < 1) {
                $error.text("開始日期不可晚於結束日期");
                return;
            }
            if (dayCount > 366) {
                $error.text("自訂區間最多可查詢 366 天");
                return;
            }

            $error.text("");
            loadTraffic(
                { startDate: startDate, endDate: endDate },
                formatInputDate(startDate) + "－" + formatInputDate(endDate) + " 瀏覽趨勢"
            );
        });

        $(".popular-period-button").on("click", function () {
            var $button = $(this);
            setActiveButton($(".popular-period-button"), $button);
            loadPopularPages($button.data("period"), $button.data("title"));
        });

        $(".heatmap-range-button").on("click", function () {
            var $button = $(this);
            setActiveButton($(".heatmap-range-button"), $button);
            loadTrafficHeatmap(Number($button.data("days")));
        });

        $(".order-trend-range-button").on("click", function () {
            var $button = $(this);
            setActiveButton($(".order-trend-range-button"), $button);
            loadOrderTrend(Number($button.data("days")));
        });
    }

    function statusBadge(label, count, className) {
        return $("<span>")
            .addClass("contact-status-badge " + className)
            .text(label + " " + number(count));
    }

    function renderContactForms(forms) {
        var $summary = $(".contact-form-summary").empty();
        if (!forms || forms.length === 0) {
            showEmpty($summary, "目前尚無表單提交資料");
            return;
        }

        forms.forEach(function (form) {
            var $row = $("<div>").addClass("contact-form-row");
            $("<div>").addClass("contact-form-name").text(text(form.name, "未命名表單")).appendTo($row);

            var $statuses = $("<div>").addClass("contact-form-statuses").appendTo($row);
            statusBadge("未處理", form.pendingCount, "is-pending").appendTo($statuses);
            statusBadge("處理中", form.processingCount, "is-processing").appendTo($statuses);
            statusBadge("已回覆", form.repliedCount, "is-replied").appendTo($statuses);
            statusBadge("已完成", form.completedCount, "is-completed").appendTo($statuses);
            $row.appendTo($summary);
        });
    }

    function statusClass(status) {
        switch (status) {
            case "未處理": return "is-pending";
            case "處理中": return "is-processing";
            case "已回覆": return "is-replied";
            case "已完成": return "is-completed";
            default: return "is-muted";
        }
    }

    function renderRecentContacts(items) {
        var $list = $(".recent-contact-list").empty();
        if (!items || items.length === 0) {
            showEmpty($list, "目前尚無表單提交資料");
            return;
        }

        items.forEach(function (item) {
            var $link = $("<a>")
                .addClass("recent-contact-item text-reset text-decoration-none")
                .attr("href", "/ContentManagement/ContactUs#" + item.id);

            var $top = $("<div>").addClass("recent-contact-top").appendTo($link);
            $("<div>").addClass("recent-contact-form").text(text(item.formName, "未命名表單")).appendTo($top);
            $("<span>")
                .addClass("contact-status-badge " + statusClass(item.status))
                .text(text(item.status, "未知"))
                .appendTo($top);

            $("<div>")
                .addClass("recent-contact-meta")
                .text(text(item.userName, "未填寫姓名") + "・" + formatDate(item.creationTime, true))
                .appendTo($link);

            $link.appendTo($list);
        });
    }

    function loadContacts() {
        var $summaryLoading = $(".contact-summary-loading");
        var $recentLoading = $(".recent-contacts-loading");
        var $contactAreas = $(".dashboard-contact-kpi, .dashboard-contact-sections");

        $.get("/api/Dashboard/GetContacts", { take: 5 })
            .done(function (result) {
                if (!result.hasData) {
                    $contactAreas.prop("hidden", true);
                    return;
                }

                $contactAreas.prop("hidden", false);
                $(".pending-contact-count").text(number(result.pendingCount));
                renderContactForms(result.forms);
                renderRecentContacts(result.recent);
                $summaryLoading.hide();
                $recentLoading.hide();
            })
            .fail(function () {
                $contactAreas.prop("hidden", true);
            });
    }

    function loadCommerceOverview() {
        var $commerceKpis = $(".dashboard-commerce-kpis");
        var $revenueKpis = $(".dashboard-revenue-kpis");
        var $orderTrendSection = $(".dashboard-order-trend-section");
        var $orderKpis = $(".dashboard-order-kpi");
        var $orderInsightColumns = $(".dashboard-order-insight-column");

        $.get("/api/Dashboard/GetCommerceOverview")
            .done(function (result) {
                if (!result.isCommerceEnabled) {
                    if (orderTrendRequest) orderTrendRequest.abort();
                    $commerceKpis
                        .add($revenueKpis)
                        .add($orderTrendSection)
                        .prop("hidden", true);
                    $orderInsightColumns.prop("hidden", true);
                    $commerceKpis.removeClass("has-no-order-data");
                    return;
                }

                $(".pending-confirmation-order-count").text(number(result.pendingConfirmationCount));
                $(".pending-payment-order-count").text(number(result.pendingPaymentCount));
                $(".awaiting-shipment-order-count").text(number(result.awaitingShipmentCount));
                $(".shipping-order-count").text(number(result.shippingCount));
                $(".low-stock-count").text(number(result.lowStockCount));
                $(".sold-out-product-count").text(number(result.soldOutProductCount));
                $commerceKpis.prop("hidden", false);

                if (!result.hasOrderData) {
                    if (orderTrendRequest) orderTrendRequest.abort();
                    $commerceKpis.addClass("has-no-order-data");
                    $orderKpis
                        .add($revenueKpis)
                        .add($orderTrendSection)
                        .prop("hidden", true);
                    $orderInsightColumns.prop("hidden", true);
                    return;
                }

                $commerceKpis.removeClass("has-no-order-data");
                $orderKpis.add($revenueKpis).prop("hidden", false);
                $(".today-order-amount").text(money(result.todayOrderAmount));
                $(".yesterday-order-amount").text(money(result.yesterdayOrderAmount));
                $(".month-order-amount").text(money(result.monthOrderAmount));
                $(".previous-month-order-amount").text(money(result.previousMonthOrderAmount));
                $(".today-paid-order-count").text(number(result.todayPaidOrderCount));
                $(".month-paid-order-count").text(number(result.monthPaidOrderCount));
                renderChange(
                    $(".today-order-change"),
                    result.todayOrderAmount,
                    result.yesterdayOrderAmount
                );
                renderChange(
                    $(".month-order-change"),
                    result.monthOrderAmount,
                    result.previousMonthOrderAmount
                );
                renderRecentOrders(result.recentOrders);
                loadOrderTrend(30);
            })
            .fail(function () {
                $commerceKpis
                    .add($revenueKpis)
                    .add($orderTrendSection)
                    .prop("hidden", true);
                $orderInsightColumns.prop("hidden", true);
            });
    }

    function orderStatusClass(status) {
        switch (status) {
            case "待確認": return "is-pending";
            case "待付款": return "is-payment";
            case "已付款": return "is-shipment";
            case "已出貨": return "is-shipping";
            default: return "is-muted";
        }
    }

    function renderRecentOrders(items) {
        var $list = $(".dashboard-recent-order-list").empty();
        var $recentOrdersColumn = $(".dashboard-recent-orders-column");
        if (!items || items.length === 0) {
            $recentOrdersColumn.prop("hidden", true);
            refreshOrderInsightsLayout();
            return;
        }

        $recentOrdersColumn.prop("hidden", false);
        refreshOrderInsightsLayout();

        items.forEach(function (item) {
            var $link = $("<a>")
                .addClass("dashboard-recent-order-item text-reset text-decoration-none")
                .attr("href", "/OrderManagement#" + item.id);
            var $top = $("<div>").addClass("dashboard-recent-order-top").appendTo($link);
            $("<strong>")
                .addClass("dashboard-recent-order-number")
                .text("訂單 #" + item.id)
                .appendTo($top);
            $("<span>")
                .addClass("dashboard-order-status " + orderStatusClass(item.status))
                .text(text(item.status, "未知"))
                .appendTo($top);

            var $detail = $("<div>").addClass("dashboard-recent-order-detail").appendTo($link);
            $("<span>")
                .text(text(item.orderer, "未填寫訂購人") + "・" + formatDate(item.creationTime, true))
                .appendTo($detail);
            $("<strong>")
                .text(money(item.total))
                .appendTo($detail);
            $link.appendTo($list);
        });
    }

    loadSystemOverview();
    loadCommerceOverview();
    refreshOnlineVisitors();
    initializeDashboardRanges();
    loadTraffic({ days: 7 }, "最近 7 天瀏覽趨勢");
    loadTrafficHeatmap(30);
    loadPopularPages("today", "本日");
    loadContacts();
    window.setInterval(refreshOnlineVisitors, 30000);
};
