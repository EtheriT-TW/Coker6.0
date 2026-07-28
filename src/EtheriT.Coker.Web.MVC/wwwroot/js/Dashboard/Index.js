var PageReady = function () {
    "use strict";

    var trafficChart = null;

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

    function loadTraffic() {
        var $loading = $(".traffic-loading");

        $.get("/api/Dashboard/GetTraffic", { days: 7 })
            .done(function (result) {
                var items = result.items || [];
                var canvas = document.getElementById("dashboard-traffic-chart");
                if (!canvas) return;

                if (trafficChart) trafficChart.destroy();
                trafficChart = new Chart(canvas.getContext("2d"), {
                    type: "bar",
                    data: {
                        labels: items.map(function (item) { return formatDate(item.date, false); }),
                        datasets: [
                            {
                                label: "有效瀏覽人次",
                                data: items.map(function (item) { return item.pageViews || 0; }),
                                backgroundColor: "rgba(26, 115, 232, 0.82)",
                                borderRadius: 5,
                                maxBarThickness: 34
                            },
                            {
                                label: "不重複訪客",
                                data: items.map(function (item) { return item.visitors || 0; }),
                                backgroundColor: "rgba(102, 187, 106, 0.82)",
                                borderRadius: 5,
                                maxBarThickness: 28
                            }
                        ]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        interaction: { intersect: false, mode: "index" },
                        plugins: {
                            legend: { position: "bottom" }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: { precision: 0 },
                                grid: { color: "rgba(0, 0, 0, 0.06)" }
                            },
                            x: {
                                grid: { display: false }
                            }
                        }
                    }
                });

                $(".traffic-updated-at").text("更新：" + formatDate(result.updatedAt, true));
                $loading.hide();
            })
            .fail(function () {
                showError($loading, "流量資料載入失敗");
            });
    }

    function loadPopularPages() {
        var $loading = $(".popular-pages-loading");
        var $list = $(".popular-page-list");

        $.get("/api/Dashboard/GetPopularPages", { take: 5 })
            .done(function (items) {
                $list.empty();
                if (!items || items.length === 0) {
                    showEmpty($list, "今天尚無有效瀏覽資料");
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
            .fail(function () {
                showError($loading, "熱門頁面載入失敗");
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
    loadTraffic();
    loadPopularPages();
    loadContacts();
    window.setInterval(refreshOnlineVisitors, 30000);
};
