(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-orders", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Orders = {
            loadPage: function (number) {
                C.Order.GetHistoryOrder(number).done(function (result) {
                    var $pane = $(MemberPage.Selectors.orderPane);
                    var $pageBtn = $pane.find(".page_btn");
                    var $noData = $pane.find(".nodata");

                    if (result.success && result.orderData != null && result.orderData.length > 0) {
                        if (result.page_Total > 1) {
                            if (!$pageBtn.data("init")) {
                                MemberPage.Pagination.init($pageBtn, result.page_Total, "order");
                            }

                            MemberPage.Pagination.change($pageBtn, number, result.page_Total);
                        }

                        MemberPage.Orders.render(result.orderData);
                    } else if (number != 1) {
                        w.location.hash = "#order-1";
                    } else {
                        $noData.removeClass("d-none");
                    }
                });
            },

            render: function (datas) {
                $(MemberPage.Selectors.orderPane + " .content").empty();

                $.each(datas, function (index, data) {
                    var orderHeader = data.orderHeader;
                    var orderDetails = data.orderDetails;
                    var frame = $($("#Template_Order_List").html()).clone();

                    frame.find(".number").text(("000000000" + orderHeader.id).substring(String(orderHeader.id).length));
                    frame.find(".date").text((orderHeader.creationTime).substr(0, 16).replaceAll("-", "/"));
                    frame.find(".amount").text("$" + orderHeader.total.toLocaleString());

                    if (typeof orderHeader.paymentIcon != "undefined" && orderHeader.paymentIcon != "") {
                        frame.find(".payment").append("<img src='" + orderHeader.paymentIcon + "'/><span>" + orderHeader.payment + "</span>");
                    } else {
                        frame.find(".payment").text(orderHeader.payment);
                    }

                    MemberPage.Orders.appendActions(frame, orderHeader);
                    MemberPage.Orders.applyStateStyle(frame, orderHeader);
                    MemberPage.Orders.bindCollapse(frame, orderHeader);
                    MemberPage.Orders.bindBuyAgain(frame, orderHeader);
                    MemberPage.Orders.bindPaymentInfo(frame, orderHeader);
                    MemberPage.Orders.renderDetails(frame, orderDetails);
                    MemberPage.Orders.renderSummary(frame, orderHeader, orderDetails);

                    $(MemberPage.Selectors.orderPane + " .content").append(frame);
                });
            },

            appendActions: function (frame, orderHeader) {
                frame.find(".state").prepend("<span>" + orderHeader.stateStr + "</span>");

                switch (orderHeader.action) {
                    case "Cancel":
                        this.appendCancelButton(frame, orderHeader);
                        break;
                    case "Repay":
                        this.appendRepayButton(frame, orderHeader);
                        break;
                    case "RepayCancel":
                        this.appendRepayButton(frame, orderHeader);
                        this.appendCancelButton(frame, orderHeader);
                        break;
                    default:
                        this.appendRepayCountdown(frame, orderHeader);
                        break;
                }
            },

            appendCancelButton: function (frame, orderHeader) {
                frame.find(".state").append("<button class='btn_cancelOrder bg-transparent border-0 text-decoration-underline ms-1' title='取消此筆訂單'>取消訂單</button>");
                frame.find(".state .btn_cancelOrder").data("ohid", orderHeader.id);

                frame.find(".state .btn_cancelOrder").on("click.memberOrders", function () {
                    var $this = $(this);
                    var $orderFrame = $this.closest(".order_frame");
                    var confirmText = "?";

                    if ([2, 6].includes(orderHeader.state)) {
                        if (orderHeader.thirdParties === 1 ||
                            (orderHeader.thirdParties === 4 && [21, 22, 23].includes(orderHeader.paymentCode))) {
                            confirmText += "<br><br><span class='fw-bold text-danger'>※退款事宜請聯繫客服處理※</span>";
                        } else {
                            confirmText += "<br><br><span class='fw-bold'>※若已付款將退回款項※</span>";
                        }
                    }

                    C.sweet.confirm("取消訂單", "<div>確定要取消這筆訂單" + confirmText + "</div>", "確定", "取消", function () {
                        C.sweet.loading();

                        C.Order.CancelOrder($this.data("ohid"), orderHeader.thirdParties).done(function (result) {
                            if (result.success) {
                                $this.parent(".state").addClass("text-danger fw-bold").text("已取消");
                                if ($orderFrame.find(".btn_buyInfo").length > 0) $orderFrame.find(".btn_buyInfo").addClass("d-none");
                                C.sweet.success(result.message, null, false);
                            } else {
                                C.sweet.error("取消訂單失敗", "取消訂單時發生錯誤，請聯繫客服協助處理。");
                            }
                        });
                    });
                });
            },

            appendRepayButton: function (frame, orderHeader) {
                frame.find(".state").append("<button class='btn_payAgain text-danger bg-transparent border-0 text-decoration-underline ms-1' title='重新付款'>重新付款</button>");

                frame.find(".state .btn_payAgain").on("click.memberOrders", function () {
                    C.sweet.confirm("確定要重新付款？", "", "確定", "取消", function () {
                        C.sweet.loading();
                        MemberPage.OrderPayment.repay({ orderHeader: orderHeader });
                    });
                });
            },

            appendRepayCountdown: function (frame, orderHeader) {
                var seconds = parseInt(orderHeader.repayRemainingSeconds || 0, 10);
                var self = this;

                if (seconds <= 0) return;

                var $countdown = $("<span class='repay-countdown text-muted small ms-1'>重新付款倒數 <span class='repay-countdown-time'></span></span>");
                frame.find(".state").append($countdown);

                var $time = $countdown.find(".repay-countdown-time");

                var render = function () {
                    if (seconds <= 0) {
                        $countdown.remove();
                        self.appendRepayButton(frame, orderHeader);
                        return false;
                    }

                    var minutes = Math.floor(seconds / 60);
                    var remainSeconds = seconds % 60;

                    $time.text(minutes + ":" + String(remainSeconds).padStart(2, "0"));
                    seconds -= 1;
                    return true;
                };

                render();

                var timer = setInterval(function () {
                    var keepGoing = render();
                    if (!keepGoing) clearInterval(timer);
                }, 1000);
            },

            applyStateStyle: function (frame, orderHeader) {
                switch (orderHeader.state) {
                    case 1:
                    case 6:
                        frame.find(".state span").addClass("bg-warning text-black");
                        break;
                    case 5:
                        frame.find(".state span").addClass("bg-danger text-white");
                        break;
                    default:
                        if (orderHeader.state != 4) frame.find(".state span").addClass("bg-success text-white");
                        break;
                }
            },

            bindCollapse: function (frame, orderHeader) {
                frame.find(".collapse").addClass("collapse_" + orderHeader.id);
                frame.find(".btn_collapse").attr("data-bs-target", ".collapse_" + orderHeader.id);

                frame.find(".btn_collapse").on("click.memberOrders", function () {
                    if ($(this).hasClass("collapsed")) $(this).text("查看訂單明細");
                    else $(this).text("關閉訂單明細");
                });
            },

            bindBuyAgain: function (frame, orderHeader) {
                frame.find(".btn_buyAgain").data("ohid", orderHeader.id);

                frame.find(".btn_buyAgain").on("click.memberOrders", function () {
                    var $this = $(this);

                    C.sweet.confirm("確定要再次購買？", "", "確定", "取消", function () {
                        C.sweet.loading();

                        C.Order.Reorder($this.data("ohid")).done(function (result) {
                            if (result.success) {
                                var ohidStr = ("000000000" + result.message).substring(String(result.message).length);
                                w.location.href = "/" + w.OrgName + "/ShoppingCar?reorder" + ohidStr;
                            } else {
                                C.sweet.warning("商品庫存不足", result.message, null);
                            }
                        });
                    });
                });
            },

            bindPaymentInfo: function (frame, orderHeader) {
                if (orderHeader.state == 6 && [21, 22, 23].includes(orderHeader.paymentCode)) {
                    frame.find(".btn_buyInfo").data("ohid", orderHeader.id).removeClass("d-none");

                    frame.find(".btn_buyInfo").on("click.memberOrders", function () {
                        MemberPage.OrderPayment.showECPayPaymentInfo(orderHeader, $(this).data("ohid"));
                    });
                }
            },

            renderDetails: function (frame, orderDetails) {
                $.each(orderDetails, function (index, detail) {
                    if (detail == null) return;

                    var listFrame = $($("#Template_Order_Details_List").html()).clone();

                    listFrame.find("a").attr({
                        href: "/" + w.OrgName + "/Member/product/" + detail.pId,
                        title: "連結至：" + detail.title
                    });

                    detail.imagePath = (detail.imagePath || "/images/RemovedProd.png")
                        .replaceAll("/" + w.OrgName + "/", "/");

                    listFrame.find("img").attr({
                        src: detail.imagePath,
                        alt: detail.title + "的主要圖片"
                    }).one("error", function () {
                        $(this).attr("src", "/images/RemovedProd.png");
                    });

                    listFrame.find(".title").text(detail.title);
                    if (detail.s1Title != "") listFrame.find(".spec").append("<span class='border px-1 me-1'>" + detail.s1Title + "</span>");
                    if (detail.s2Title != "") listFrame.find(".spec").append("<span class='border px-1 me-1'>" + detail.s2Title + "</span>");

                    listFrame.find(".price").html(detail.price > 0
                        ? (detail.bonus > 0 ? parseInt(detail.price, 10).toLocaleString() + "<br />紅利：" + detail.bonus.toLocaleString() : parseInt(detail.price, 10).toLocaleString())
                        : "紅利：" + detail.bonus.toLocaleString());

                    listFrame.find(".quantity").text(C.util.string.thousandSign(detail.quantity));

                    var detailPrice = parseInt(detail.price || 0, 10);
                    var detailQty = parseInt(detail.quantity || 0, 10);
                    var detailBonus = parseInt(detail.bonus || 0, 10);
                    var detailSubtotal = detailPrice * detailQty;
                    var detailBonusSubtotal = detailBonus * detailQty;

                    listFrame.find(".subtotal").html(detailPrice > 0
                        ? (detailBonus > 0 ? detailSubtotal.toLocaleString() + "<br />紅利：" + detailBonusSubtotal.toLocaleString() : detailSubtotal.toLocaleString())
                        : "紅利：" + detailBonusSubtotal.toLocaleString());

                    frame.find(".list-group").append(listFrame);
                });
            },

            renderSummary: function (frame, orderHeader, orderDetails) {
                var productAmount = 0;
                var productBonus = 0;

                $.each(orderDetails, function (index, detail) {
                    if (detail == null) return;

                    var price = parseInt(detail.price || 0, 10);
                    var qty = parseInt(detail.quantity || 0, 10);
                    var bonus = parseInt(detail.bonus || 0, 10);

                    productAmount += price * qty;
                    productBonus += bonus * qty;
                });

                var totalBonus = parseInt(orderHeader.bonus || 0, 10);
                var redeemBonus = Math.max(totalBonus - productBonus, 0);
                var discount = parseInt(orderHeader.discount || 0, 10);

                frame.find(".collapse .header_subtotal").text(C.util.string.thousandSign(productAmount));
                frame.find(".collapse .header_freight").text(C.util.string.thousandSign(orderHeader.freight));
                frame.find(".collapse .header_total").text(C.util.string.thousandSign(orderHeader.total));

                if (discount > 0) {
                    frame.find(".collapse .header_discount").text(C.util.string.thousandSign(discount));
                } else {
                    frame.find(".collapse .discount_summary_row").remove();
                }

                frame.find(".collapse .header_invoiceTypeTitle").text(orderHeader.invoiceTypeTitle || "未提供");
                frame.find(".collapse .header_shipping").text(orderHeader.shipping || "未提供");
                frame.find(".collapse .header_remark").text(orderHeader.remark || "無");
                frame.find(".collapse .header_systemMemo").text(orderHeader.systemMemo || "無");

                frame.find(".collapse .header_recipient").text(orderHeader.recipient);
                frame.find(".collapse .header_recipientCellPhone").text(orderHeader.recipientCellPhone);
                frame.find(".collapse .header_recipientAddress").text(orderHeader.recipientAddress);

                if (C.util.string.isNullOrEmpty(orderHeader.carrier)) {
                    frame.find(".collapse #carrier").addClass("d-none");
                } else {
                    frame.find(".collapse #carrier").removeClass("d-none");
                    frame.find(".collapse .header_carrier").text(orderHeader.carrier);
                }

                if (totalBonus <= 0) {
                    frame.find(".collapse .bonus_summary_row").remove();
                } else {
                    frame.find(".collapse .header_productBonus").text(C.util.string.thousandSign(productBonus) + "點");
                    frame.find(".collapse .header_redeemBonus").text(C.util.string.thousandSign(redeemBonus) + "點");
                    frame.find(".collapse .header_totalBonus").text(C.util.string.thousandSign(totalBonus) + "點");

                    if (productBonus <= 0) frame.find(".collapse .header_productBonus").closest(".row").remove();
                    if (redeemBonus <= 0) frame.find(".collapse .header_redeemBonus").closest(".row").remove();
                }

                if (C.util.string.isNullOrEmpty(orderHeader.trackingNumber)) {
                    frame.find(".logistics_info").addClass("d-none");
                } else {
                    frame.find(".logistics_info").removeClass("d-none");
                    frame.find(".logistics_info .logistics_number").text(orderHeader.trackingNumber);
                    MemberPage.Orders.bindCopyTracking(frame);
                }
            },

            bindCopyTracking: function (frame) {
                var $btn = frame.find(".logistics_info .copy-btn");
                var $number = frame.find(".logistics_info .logistics_number");

                $btn.on("click.memberOrders", function () {
                    var text = $number.text();
                    if (C.util.string.isNullOrEmpty(text)) return;

                    if (navigator.clipboard && navigator.clipboard.writeText) {
                        navigator.clipboard.writeText(text).then(function () {
                            C.sweet.success("已複製物流編號", null, true);
                        }).catch(function () {
                            C.sweet.warning("複製失敗", "請手動複製", null);
                        });
                    } else {
                        C.sweet.warning("複製失敗", "瀏覽器不支援自動複製", null);
                    }
                });
            }
        };
    }
})(window);
