var keyId
var order_list
let $btn_reSend, $btn_save;
var OrderList_dxData;
var oristate = 0, payment = "";
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
        if (newstate != oristate) {
            switch (payment) {
                case "LINEPay":
                    if (newstate == 6) {
                        co.sweet.error("訂單狀態錯誤", `不可將狀態變更為【${status}】`);
                    } else if (oristate == 6) {
                        if ([1, 4, 5].includes(newstate)) {
                            co.sweet.confirm("變更訂單狀態", `訂單已授權付款，是否確認將訂單狀態變更為【${status}】並取消授權？`, "確定", "取消", function () {
                                Coker.ThirdParty.Line.PayVoid(keyId).done(function (result) {
                                    if (result.success) {
                                        updateOrder();
                                        console.log(result.message);
                                    } else {
                                        co.sweet.error(result.error, result.message, null, false);
                                    }
                                });
                            });
                        } else if (newstate == 2) {
                            co.sweet.confirm("變更訂單狀態", `是否確認將訂單狀態變更為【${status}】並執行付款流程？`, "確定", "取消", function () {
                                $(".btn_confirm").trigger("click");
                            });
                        } else {
                            co.sweet.error("訂單狀態錯誤", `訂單已授權付款，如要變更訂單狀態，請先完成付款程序。`);
                        }
                    } else if ([2, 3, 7].includes(oristate) && [1, 4, 5].includes(newstate)) {
                        co.sweet.confirm("變更訂單狀態", `訂單已完成付款，是否確認將訂單狀態變更為【${status}】？`, "確定", "取消", function () {
                            co.sweet.confirm("變更訂單狀態", "是否退回貨款?", "確定", "取消", function () {
                                Coker.ThirdParty.Line.PayRefund(keyId).done(function (result) {
                                    if (result.success) {
                                        updateOrder();
                                        console.log(result.message);
                                    } else {
                                        co.sweet.error(result.error, result.message, null, false);
                                    }
                                });
                            }, updateOrder);
                        });
                    } else updateOrder();
                    break;
                default:
                    co.sweet.confirm("變更訂單狀態", `是否確認將訂單狀態變更為【${status}】?`, "確定", "取消", function () {
                        updateOrder();
                    });
                    break;
            }
        } else updateOrder();
    });
    co.Order.GetOrderStatusLookup().done(function (result) {
        $(result).each(function () {
            $order_status.append(`<option value="${this.id}">${this.name}</option>`);
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
            OrderList_dxData.refresh();
        }
        else co.sweet.error("儲存失敗", result.error);
    });
}
function ElementInit() {

    OrderList_dxData = $("#OrderList").dxDataGrid("instance");

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
    if (!$(".btn_failReason").hasClass("d-none")) $(".btn_failReason").addClass("d-none");
    if (!$(".btn_confirm").hasClass("d-none")) $(".btn_confirm").addClass("d-none");

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
            co.Order.GetHeader(parseInt(hash)).done(function (result) {
                if (result != null) {
                    keyId = result.id;
                    HeaderDataSet(result);
                    co.Order.GetDetails(keyId).done(function (result) {
                        for (var i = 0; i < result.length; i++) {
                            DetailsDataSet(result[i])
                        }
                        MoveToContent();
                    });
                } else {
                    window.location.hash = ""
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
function HeaderDataSet(result) {
    payment = result.payment

    $order_number.text(("000000000" + result.id).substr(result.id.length));
    $order_date.text(result.creationTime)
    $order_subtotal.text(`$${result.subtotal.toLocaleString("en-US")}`)
    $order_freight.text(`$${result.freight.toLocaleString("en-US")}`)
    $order_total.text(`$${result.total.toLocaleString("en-US")}`)
    $order_payment.text(result.payment)
    $order_shipping.text(result.shipping)
    $order_status.val(result.state);
    oristate = result.state;
    switch (parseInt(result.state)) {
        case 4:
            $order_status.prop("disabled", true);
            break;
        case 5:
            $order_status.prop("disabled", true);
            switch (payment) {
                case "LINEPay":
                    if ($(".btn_failReason").hasClass("d-none")) $(".btn_failReason").removeClass("d-none");
                    break;
            }
            break;
        case 6:
            switch (payment) {
                case "LINEPay":
                    if ($(".btn_confirm").hasClass("d-none")) $(".btn_confirm").removeClass("d-none");
                    break;
            }
            break;
    }
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
                    }
                    break;
                case "uniformId":
                    if (result[key] != null) {
                        if ($self.parent("div").hasClass("d-none")) $self.parent("div").removeClass("d-none");
                        $self.text(result[key]);
                    }
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

    switch (result.payment) {
        case "LINEPay":
            $(".btn_failReason").on("click", function () {
                Coker.ThirdParty.Line.CheckPaymentStatus(keyId).done(function (result) {
                    Swal.fire({
                        title: `Code: ${result.returnCode}`,
                        text: result.returnMessage,
                    });
                });
            });
            $(".btn_confirm").on("click", function () {
                Coker.ThirdParty.Line.Confirm(keyId).done(function (result) {
                    if (result.success) {
                        co.sweet.success("付款程序已完成", function () {
                            location.reload();
                        }, false);
                    } else {
                        co.sweet.error(result.error, result.message, null, false);
                    }
                });
            });
            break;
    }
}
function DetailsDataSet(result) {
    var item = $($("#Templat_Purchase_List").html()).clone();
    var item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_instructions = item.find(".pro_instructions"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_subtotal = item.find(".pro_subtotal");

    item_image.attr("src", result.imagePath);
    item_name.text(result.title);
    item_specification.text(result.s1Title + " " + result.s2Title);
    item_instructions.text(result.description);
    item_unit.text(result.price.toLocaleString("en-US"))
    item_quantity.text(result.quantity);
    item_subtotal.text(`$${(result.price * result.quantity).toLocaleString("en-US")}`)

    var item_list_ul = $("#OrderDetails > .card-body > .purchase_list");
    item_list_ul.append(item);
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