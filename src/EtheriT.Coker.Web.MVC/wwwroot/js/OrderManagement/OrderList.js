var keyId
var order_list
let $btn_reSend, $btn_save;
var oristate = 0, payment = "", transactionId = "", thirdparty = 0;
function PageReady() {
    OrderDataCollapse();
    $(window).resize(OrderDataCollapse);

    ElementInit();

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回訂單列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $btn_reSend.on("click", function () {
        co.sweet.confirm("重新發送通知信", "是否確認重發訂單通知信?", "確定", "取消", function () {
            co.Order.SendMail(keyId);
        });
    });
    $btn_save.on("click", function () {
        const status = $(".status_select > option:selected").text();
        var newstate = parseInt($(".status_select > option:selected").val());
        if (newstate != oristate && (payment == "LINEPay" || payment == "支付連")) {
            if (([1, 5, 6].includes(newstate) || (thirdparty == 3 && oristate == 1) || oristate == 6)) {
                co.sweet.error("訂單狀態錯誤", `不可將狀態變更為【${status}】`);
            } else updateOrder();
        } else updateOrder();
    });
    co.Order.GetOrderStatusLookup().done(function (result) {
        $(result).each(function () {
            $order_status.append(`<option value="${this.id}">${this.name}</option>`);
        });
    });
    $(".btn_confirm").on("click", function () {
        co.sweet.loading();
        Coker.ThirdParty.Line.Confirm(keyId).done(function (result) {
            if (result.success) {
                $(".confirm").addClass("d-none");
                $order_status.val(2);
                oristate = 4;
                OrderStateChange(oristate)
                co.sweet.success("付款程序已完成", function () {
                    updateOrder();
                    console.log(result.message);
                }, false);
            } else {
                co.sweet.error(result.error, result.message, null, false);
            }
        });
    });
    $(".btn_void").on("click", function () {
        co.sweet.confirm("取消授權", "確定取消消費者付款授權？", "確定", "取消", function () {
            co.sweet.loading();
            Coker.ThirdParty.Line.PayVoid(keyId).done(function (result) {
                if (result.success) {
                    $(".confirm").addClass("d-none");
                    $order_status.val(4);
                    oristate = 4;
                    OrderStateChange(oristate)
                    co.sweet.success("已取消付款授權", function () {
                        updateOrder();
                        console.log(result.message);
                    }, false);
                } else {
                    co.sweet.error(result.error, result.message, null, false);
                }
            });
        })
    });
    $(".btn_recheck").on("click", function () {
        co.sweet.loading();
        Coker.ThirdParty.CheckPaymentStatus(keyId, thirdparty).done(function (result) {
            if (result.success) {
                var state = result.message.split(",")[0];
                if (state != oristate) {
                    $(".btn_recheck").addClass("d-none");
                    $order_status.val(state);
                    oristate = state;
                    OrderStateChange(oristate)
                    co.sweet.success(`訂單狀態變更為【${$order_status.find("option:selected").text()}】`, function () {
                        updateOrder();
                        console.log(result.message.split(",")[1]);
                    });
                } else {
                    Swal.fire({
                        title: `訂單狀態`,
                        text: `訂單狀態未更動。`,
                    });
                }
            }
        });
    });
    $(".btn_refund").on("click", function () {
        co.sweet.confirm("退回貨款", "確定退回貨款?", "確定", "取消", function () {
            co.sweet.loading();
            Coker.ThirdParty.PayRefund(payment, keyId).done(function (result) {
                if (result.success) {
                    $(".btn_refund").addClass("d-none");
                    $order_status.val(4);
                    oristate = 4;
                    OrderStateChange(oristate)
                    updateOrder();
                } else {
                    co.sweet.error(result.error, result.message, null, false);
                }
            });
        });
    });

    $(".btn_checkrefund").on("click", function () {
        co.sweet.loading();
        Coker.ThirdParty.CheckRefund(payment, transactionId).done(function (result) {
            if (result.success) {
                Swal.fire({
                    title: `退款狀態查詢`,
                    text: result.message,
                });
            } else {
                co.sweet.error("錯誤", result.message, null, false);
            }
        });
    });
    $(".btn_failReason").on("click", function () {
        co.sweet.loading();
        Coker.ThirdParty.CheckPaymentStatus(keyId, thirdparty).done(function (result) {
            Swal.fire({
                title: `Code: ${result.error}`,
                text: result.message.split(",")[1],
            });
        });
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}
function updateOrder() {
    co.Order.UpdateStatus({ Id: keyId, Status: $order_status.val(), Memo: $memo_block.val() }).done(function (result) {
        if (result.success) {
            co.sweet.success("儲存成功");
            switch (parseInt($order_status.val())) {
                case 4:
                case 5:
                    $order_status.prop("disabled", true)
                    break;
            }
            //order_list.component.refresh();
        }
        else co.sweet.error("儲存失敗", result.error);
    });
}
function ElementInit() {

    /* Header */
    $order_number = $(".order_number")
    $order_date = $(".order_date")
    $order_subtotal = $(".order_subtotal")
    $order_freight = $(".order_freight")
    $order_total = $(".order_total")
    $order_payment = $(".order_payment")
    $order_shipping = $(".order_shipping")
    $order_status = $(".status_select")
    $order_notes = $(".order_notes")
    $memo_block = $(".memo_block");

    /* Recipient */
    $recipient_name = $(".recipient_name")
    $recipient_cellphone = $(".recipient_cellphone")
    $recipient_telphone = $(".recipient_telphone")
    $recipient_address = $(".recipient_address")

    /* Order */
    $orderer_name = $(".orderer_name")
    $orderer_cellphone = $(".orderer_cellphone")
    $orderer_telphone = $(".orderer_telphone")
    $orderer_address = $(".orderer_address")

    /*bottom*/
    $btn_reSend = $(".btn_reSend");
    $btn_save = $(".btn_save");
}
function FormDataClear() {
    keyId = 0;

    $order_number.text("");
    $order_date.text("")
    $order_subtotal.text("")
    $order_freight.text("")
    $order_total.text("")
    $order_payment.text("")
    $order_shipping.text("")
    $order_status.val(0);
    $order_status.prop("disabled", false);
    $order_notes.text("")

    $(".confirm").addClass("d-none");
    $(".btn_recheck").addClass("d-none");
    $(".btn_refund").addClass("d-none");
    $(".btn_checkrefund").addClass("d-none");
    $(".btn_failReason").addClass("d-none");

    $recipient_name.text("")
    $recipient_cellphone.text("")
    $recipient_telphone.text("")
    $recipient_address.text("")

    $orderer_name.text("")
    $orderer_cellphone.text("")
    $orderer_telphone.text("")
    $orderer_address.text("")
}
function contentReady(e) {
    order_list = e;
    var urlParams = new URLSearchParams(window.location.search);
    var filterValue = urlParams.get("mid");

    if (filterValue !== null) {
        $("#OrderListDx").dxDataGrid("instance").columnOption("MemberId", {
            selectedFilterOperation: "=",
            filterValue: filterValue
        });
        window.history.replaceState(null, "", "/OrderManagement");
    }
    HashDataEdit();
}
function onCellPrepared(e) {
    if (e.rowType === "data" && e.column.dataField === "State") {
        var $cell = $(e.cellElement);
        if (e.value == "已付款") {
            $cell.addClass("hasPay")
        }
    }
}
function onRowPrepared(e) {
    if (e.rowType === "data") {
        let $row = $(e.rowElement);
        if (e.data.State === "付款失敗") {
            $row.addClass("isFail");
        }
    }
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
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            var ohids = `${hash}`;
            co.Order.GetDisplay(ohids).done(function (result) {
                if (result.length > 0) {
                    var order_header = result[0].orderHeader;
                    //keyId = order_header.id;
                    HeaderDataInsert(order_header)

                    var order_details = result[0].orderDetails;
                    $.each(order_details, function (index, data) {
                        var frame = $($("#Template_Purchase_List").html()).clone();
                        frame = DataInsert(data, frame);
                        $("#OrderDetails > .card-body > .purchase_list").append(frame)
                    });

                    MoveToContent();
                } else {
                    window.location.hash = ""
                }
            });
            co.Order.GetHeaderOld(parseInt(hash)).done(function (result) {
                if (result != null) {
                    console.log(result);
                    FormDataClear();
                    keyId = result.id;
                    HeaderDataSet(result);
                } else {
                    //window.location.hash = ""
                }
            });
        }
    } else {
        BackToList();
    }
}
function editButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId
    MoveToContent();
}
function HeaderDataInsert(data) {
    DataInsert(data, $("#OrderDetails"));
    DataInsert(data, $("#OrderDetails .card-body > div"));
    DataInsert(data, $("#OrderDetails .card-body > .purchase_amount"));
    DataInsert(data, $("#OrderData"));

    if (data.ordererId != null) {
        $("#OrdererData .btn_orderer_data").css('display', 'flex');
        $("#OrdererData .btn_orderer_data").attr({
            href: `/MemberManagement/MemberList#${data.ordererId}`,
            title: `連結至：訂購人(${data.orderer})`
        })
        $("#OrdererData .btn_orderer_data").text((`000000000${data.ordererId}`).substring(data.ordererId.toString().length));
    }
}
function HeaderDataSet(result) {
    thirdparty = result.thirdParties;
    if (result.payment.indexOf("-") > 0) {
        payment = result.payment.substring(0, result.payment.indexOf("-"));
    } else {
        payment = result.payment
    }
    transactionId = result.transactionId;
    $order_status.val(result.state);
    oristate = parseInt(result.state);
    var status_lock = false;

    if ([4, 5].includes(oristate)) $order_status.prop("disabled", true)
    else if (oristate == 7) {
        var sevenDaysAgo = new Date();
        sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
        if (result.completedDate != null && new Date(result.completedDate) < sevenDaysAgo) {
            $order_status.prop("disabled", true);
            status_lock = true;
        }
    }

    if (result.refundTransactionId != null) $(".btn_checkrefund").removeClass("d-none");
    else if (result.transactionId != null && ![1, 5, 6].includes(oristate) && !status_lock) $(".btn_refund").removeClass("d-none");
    else OrderStateChange(oristate)

    $order_notes.text(result.remark)

    $recipient_name.text(result.recipient)
    $recipient_cellphone.text(result.recipientCellPhone)
    var re_telIndex = result.recipientTelephone.indexOf("-", 5)
    $recipient_telphone.text(result.recipientTelephone.substr(re_telIndex + 1).length > 0 ? result.recipientTelephone : result.recipientTelephone.subtotal(0, re_telIndex))
    $recipient_address.text(result.recipientAddress)

    $orderer_name.text(result.orderer)
    $orderer_cellphone.text(result.ordererCellPhone)
    var or_telIndex = result.ordererTelephone.indexOf("-", 5)
    $orderer_telphone.text(result.ordererTelephone.substr(or_telIndex + 1).length > 0 ? result.ordererTelephone : result.ordererTelephone.subtotal(0, or_telIndex))

    $memo_block.val(result.memo);

    $("#InvoiceData").find("*").each(function () {
        var $self = $(this);
        if (typeof ($self.data("key")) != "undefined") {
            var key = $self.data("key");
            switch (key) {
                case "invoiceRecipient":
                    switch (parseInt(result.invoiceRecipient)) {
                        case 1:
                            if ($self.parent("div").hasClass("d-none")) $self.parent("div").removeClass("d-none");
                            $self.siblings("div").text("同訂購人：");
                            $self.text(result["orderer"]);
                            break;
                        case 2:
                            if ($self.parent("div").hasClass("d-none")) $self.parent("div").removeClass("d-none");
                            $self.siblings("div").text("同收件人：");
                            $self.text(result["recipient"]);
                            break;
                        case 3:
                            if (!$self.parent("div").hasClass("d-none")) $self.parent("div").addClass("d-none");
                            break;
                    }
                    break;
                case "invoiceTitle":
                    if (result[key] != null) {
                        if ($self.parent("div").hasClass("d-none")) $self.parent("div").removeClass("d-none");
                        $self.text(result[key]);
                    } else $self.parent("div").addClass("d-none");
                    break;
                case "uniformId":
                    if (result[key] != null) {
                        if ($self.parent("div").hasClass("d-none")) $self.parent("div").removeClass("d-none");
                        $self.text(result[key]);
                    } else $self.parent("div").addClass("d-none");
                    break;
                case "invoiceAddress":
                    $self.text(result[key].replaceAll(" ", ""));
                    break;
                default:
                    $self.text(result[key]);
                    break;
            }
        }
    });
}
function DataInsert(data, frame) {
    frame.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof ($this.data("key")) != "undefined") {
            switch ($this.data("key")) {
                case "id":
                    $this.text((`000000000${data[key]}`).substring(data[key].toString().length));
                    break;
                case "imagePath":
                    $this.attr({
                        src: data[key],
                        alt: `${data['title']}的圖片`
                    })
                    break;
                case "spec":
                    var spec = data['s1Title'] + (data['s2Title'] == "" ? "" : ` ${data['s2Title']}`)
                    $this.text(spec);
                    break;
                default:
                    if ($this.data("key") != "invoiceRecipient") $this.text(data[key]);
                    break;
            }
        }
    });
    return frame;
}
function MoveToContent() {
    $("#OrderList").addClass("d-none");
    $("#OrderContent").removeClass("d-none");
    $btn_reSend.removeClass("d-none");
    $btn_save.removeClass("d-none");
}
function BackToList() {
    $("#OrderList").removeClass("d-none");
    $("#OrderContent").addClass("d-none");
    $btn_reSend.addClass("d-none");
    $btn_save.addClass("d-none");
    window.location.hash = ""

    $("#OrderDetails > .card-body > .purchase_list > .purchase_item").each(function () {
        $(this).remove();
    })
}
function OrderDataCollapse() {
    $this_body = $("body > .wrapper > .content-area > .content-wrapper");
    $OrderDetails = $("#OrderDetails");
    $OrderData = $("#OrderData");

    if ($this_body.width() >= 1024) {
        $("#Btn_Side_Collapse").addClass("d-none");
        $OrderDetails.removeClass("col-12");
        $OrderData.addClass("col-3");
        $OrderData.removeClass("offcanvas offcanvas-end visible");
        $OrderData.css('visibility', '');
    } else {
        $("#Btn_Side_Collapse").removeClass("d-none");
        $OrderDetails.addClass("col-12");
        $OrderData.addClass("offcanvas offcanvas-end visible");
        $OrderData.removeClass("col-3");
    }
}
function OrderStateChange(state) {

    $(".confirm").addClass("d-none");
    $(".btn_recheck").addClass("d-none");
    $(".btn_refund").addClass("d-none");
    $(".btn_checkrefund").addClass("d-none");
    $(".btn_failReason").addClass("d-none");

    switch (parseInt(state)) {
        case 1:
            if (thirdparty == 3) $(".btn_recheck").removeClass("d-none");
            break;
        case 5:
            if (thirdparty != 1) $(".btn_failReason").removeClass("d-none");
            break;
        case 6:
            switch (payment) {
                case "LINEPay":
                    $(".confirm").removeClass("d-none");
                    break;
                case "支付連":
                    $(".btn_recheck").removeClass("d-none");
                    break;
            }
            break;
    }
}