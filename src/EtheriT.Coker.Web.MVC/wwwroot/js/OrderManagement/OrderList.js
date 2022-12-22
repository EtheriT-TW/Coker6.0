var keyId
var order_list

function PageReady() {
    co.Order = {
        GetHeader: function (id) {
            return $.ajax({
                url: "/api/Order/GetHeaderOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        GetDetails: function (id) {
            return $.ajax({
                url: "/api/Order/GetOrderDetails/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { id: id },
            });
        },
    };

    OrderDataCollapse();
    $(window).resize(OrderDataCollapse);

    ElementInit();

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回訂單列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }

    $(".status_select").on("change", function () {
    })
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
    $order_notes.text("")

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
    $order_number.text(("000000000" + result.id).substr(result.id.length));
    $order_date.text(result.creationTime)
    $order_subtotal.text(result.subtotal.toLocaleString("en-US"))
    $order_freight.text(result.freight.toLocaleString("en-US"))
    $order_total.text(result.total.toLocaleString("en-US"))
    $order_payment.text(result.payment)
    $order_shipping.text(result.shipping)
    $order_status.val(result.state);
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

    item_image.attr("src", "../images/product/pro_0" + result.pId + ".png");
    item_name.text(result.title);
    item_specification.text(result.s1Title + " " + result.s2Title);
    item_instructions.text(result.description);
    item_unit.text(result.price.toLocaleString("en-US"))
    item_quantity.text(result.quantity);
    item_subtotal.text((result.price * result.quantity).toLocaleString("en-US"))

    var item_list_ul = $("#OrderDetails > .card-body > .purchase_list");
    item_list_ul.append(item);
}

function MoveToContent() {
    $("#OrderList").addClass("d-none");
    $("#OrderContent").removeClass("d-none");
}

function BackToList() {
    $("#OrderList").removeClass("d-none");
    $("#OrderContent").addClass("d-none");
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