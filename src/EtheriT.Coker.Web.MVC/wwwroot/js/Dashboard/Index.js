var PageReady = function () {
    "use strict";

    var trafficChart = null;
    var trafficRequest = null;
    var popularPagesRequest = null;

    function number(value) {
        return co.String.thousandSign(Number(value) || 0);
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

        $.get("/api/Dashboard/GetContacts", { take: 5 })
            .done(function (result) {
                $(".pending-contact-count").text(number(result.pendingCount));
                renderContactForms(result.forms);
                renderRecentContacts(result.recent);
                $summaryLoading.hide();
                $recentLoading.hide();
            })
            .fail(function () {
                $(".pending-contact-count").text("--");
                showError($summaryLoading, "表單統計載入失敗");
                showError($recentLoading, "最近提交載入失敗");
            });
    }

    loadSystemOverview();
    refreshOnlineVisitors();
    initializeDashboardRanges();
    loadTraffic({ days: 7 }, "最近 7 天瀏覽趨勢");
    loadPopularPages("today", "本日");
    loadContacts();
    window.setInterval(refreshOnlineVisitors, 30000);
};
