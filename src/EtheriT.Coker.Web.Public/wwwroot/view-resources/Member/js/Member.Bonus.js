(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-bonus", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Bonus = {
            loadPage: function (number) {
                var $pane = $(MemberPage.Selectors.bonusPane);
                var $content = $pane.find(".content");
                var $pageBtn = $pane.find(".page_btn");
                var $noData = $pane.find(".nodata");

                C.Bonus.GetFrontUserBonusHistory(number).done(function (result) {
                    var datas = result && Array.isArray(result.data) ? result.data : [];

                    if (datas.length > 0) {
                        $noData.addClass("d-none");

                        if (result.page_Total > 1) {
                            $pageBtn.removeClass("d-none");

                            if (!$pageBtn.data("init")) {
                                MemberPage.Pagination.init($pageBtn, result.page_Total, "bonus");
                            }

                            MemberPage.Pagination.change($pageBtn, number, result.page_Total);
                        } else {
                            $pageBtn.addClass("d-none");
                        }

                        MemberPage.Bonus.render($content, datas);
                    } else if (number != 1) {
                        w.location.hash = "#bonus-1";
                    } else {
                        $pageBtn.addClass("d-none");
                        $noData.removeClass("d-none");
                        $content.empty();
                    }
                });
            },

            renderBonusNote: function ($target, note) {
                note = !C.util.string.isNullOrEmpty(note) ? note : "未提供";

                var orderKeyword = "-訂單編號[";
                var orderIndex = note.lastIndexOf(orderKeyword);

                // 沒有訂單編號格式，就維持原本純文字
                if (orderIndex < 0) {
                    $target.text(note);
                    return;
                }

                var mainText = note.substring(0, orderIndex);
                var orderText = note.substring(orderIndex + 1); // 去掉前面的 -

                $target.empty()
                    .append($("<span>").addClass("bonus-note-main").text(mainText))
                    .append($("<span>").addClass("bonus-note-separator").text("-"))
                    .append($("<span>").addClass("bonus-note-order-no").text(orderText));
            },

            render: function ($content, datas) {
                $content.empty();

                $.each(datas, function (index, data) {
                    var frame = $($("#Template_Bonus_List").html()).clone();

                    frame.find(".bonus_start").text(C.util.string.dateText(data.startTime));
                    frame.find(".bonus_end").text(C.util.string.dateText(data.endTime));
                    frame.find(".bonus_add").text(C.util.string.thousandSign(data.addBonus));
                    frame.find(".bonus_remain").text(C.util.string.thousandSign(data.remainBonus));
                    MemberPage.Bonus.renderBonusNote(frame.find(".bonus_note"), data.note);

                    var collapseClass = "bonus_collapse_" + data.id;
                    frame.find(".bonus_logs").addClass(collapseClass);
                    frame.find(".btn_bonus_collapse").attr("data-bs-target", "." + collapseClass);

                    frame.find(".btn_bonus_collapse").on("click.memberBonus", function () {
                        if ($(this).hasClass("collapsed")) $(this).text("查看使用紀錄");
                        else $(this).text("關閉使用紀錄");
                    });

                    var $logList = frame.find(".bonus_log_list");
                    var logs = Array.isArray(data.useLogs) ? data.useLogs : [];

                    if (logs.length > 0) {
                        $.each(logs, function (i, log) {
                            var logFrame = $($("#Template_Bonus_Log_List").html()).clone();
                            logFrame.find(".log_date").text(C.util.string.dateText(log.creationTime));

                            var isRefund = log.isRefund === true;
                            var useBonus = Number(log.useBonus || 0);
                            var typeName = log.typeName || "";

                            var $badge = $("<span>")
                                .addClass("bonus-log-type-badge")
                                .toggleClass("is-refund", isRefund)
                                .toggleClass("is-redeem", !isRefund)
                                .text(typeName);

                            logFrame
                                .toggleClass("bonus-log-refund", isRefund)
                                .toggleClass("bonus-log-redeem", !isRefund);

                            logFrame.find(".log_reason")
                                .empty()
                                .append($badge)
                                .append(document.createTextNode(log.reason || ""));

                            logFrame.find(".log_use")
                                .toggleClass("bonus-point-refund", isRefund)
                                .toggleClass("bonus-point-redeem", !isRefund)
                                .text((useBonus > 0 ? "+" : "") + C.util.string.thousandSign(useBonus));
                            $logList.append(logFrame);
                        });
                    } else {
                        frame.find(".bonus_log_empty").removeClass("d-none");
                    }

                    $content.append(frame);
                });
            }
        };
    }
})(window);
