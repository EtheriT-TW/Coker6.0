var buy_step_swiper;
var gotop_switch = false, isCheckout = false;

var subtotal, ori_freight, low_con, disfreight, freight, total, paymentArr
var shipping = null, payment = null;

var ShippingForms, PaymentForms, OrdererForms, RecipientForms, InvoiceForms;
var OrdererOpen = false, RecipientOpen = false, InvoiceOpen = false;
var shipMethodsChosen = false, payMethodsChosen = false, OrdererFilled = true, RecipientFilled = true, InvoiceFilled = true;
var $Orderer_TWzipcode, $Recipient_TWzipcode, $Invoice_TWzipcode;
var $orderer_name, $orderer_sex, $orderer_email, $orderer_cellphone, $orderer_telphone_area, $orderer_telphone, $orderer_telphone_ext, $orderer_address_city, $orderer_address_town, $orderer_address;
var $recipient_name, $recipient_sex, $recipient_email, $recipient_cellphone, $recipient_telphone_area, $recipient_telphone, $recipient_telphone_ext, $recipient_address_city, $recipient_address_town, $recipient_address, $remark;
var $invoice_recipient, $invoice_title, $invoice_uniformid, $invoice_address_city, $invoice_address_town, $invoice_address;
var $ship_method, $pay_method;

function PageReady() {
    Coker.Order = {
        AddHeader: function (data) {
            return $.ajax({
                url: "/api/Order/AddHeader",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        AddDetails: function (data) {
            return $.ajax({
                url: "/api/Order/AddDetails",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetHeader: function (id) {
            return $.ajax({
                url: "/api/Order/GetHeaderOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { id: id },
            });
        },
        GetDetails: function (id) {
            return $.ajax({
                url: "/api/Order/GetOrderDetails/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { id: id },
            });
        },
        GetPaymentTypeEnum: function () {
            return $.ajax({
                url: "/api/Order/GetPaymentTypeEnum",
                type: "POST",
            });
        },
    };
    ElementInit();
    CartInit();

    Coker.Order.GetPaymentTypeEnum().done(function (result) {
        paymentArr = result;
    });

    /* Buy Swiper */
    buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        autoHeight: true,
        loop: false,
        enabled: false,
        pagination: {
            el: ".swiper_pagination > .swiper_pagination_buystep",
            clickable: true,
            renderBullet: function (index, className) {
                return `<span class="${className}">${index + 1}</span>`;
            },
        },
        navigation: {
            nextEl: ".btn_swiper_next_buystep",
            prevEl: ".btn_swiper_prev_buystep",
        }
    });

    buy_step_swiper.on('slideChangeTransitionEnd', function () {
        if (gotop_switch) {
            window.scrollTo(0, $(".swiper").offset().top - $("header").height());
        }
    });

    buy_step_swiper.on('slideChange', function () {
        switch (buy_step_swiper.activeIndex) {
            case 2:
                if (ShippingForms.find("input").length == 0) {
                    Coker.sweet.error("錯誤", "店家尚未設置運費方式，無法繼續", null, false);
                    buy_step_swiper.slideTo(1);
                } else if (PaymentForms.find("input").length == 0) {
                    Coker.sweet.error("錯誤", "店家尚未設置付款方式，無法繼續", null, false);
                    buy_step_swiper.slideTo(1);
                } else {
                    shipMethodsChosen = FormCheck(ShippingForms);
                    payMethodsChosen = FormCheck(PaymentForms);
                    if (!(shipMethodsChosen && payMethodsChosen)) {
                        Coker.sweet.error("請確實選擇運送及付款方式！", null, true);
                        setTimeout(function () {
                            buy_step_swiper.slideTo(1);
                        }, 1500);
                    }
                }
                break;
            case 3:
                if (!isCheckout) {
                    if (OrdererOpen) { OrdererFilled = FormCheck(OrdererForms) };
                    if (RecipientOpen) { RecipientFilled = FormCheck(RecipientForms) };
                    if (InvoiceOpen) { InvoiceFilled = FormCheck(InvoiceForms) };
                    Coker.sweet.error("未完成結帳流程！", "若資料已確實填寫完畢，請點選下方[確認付款]按鈕進入付款程序", null, false);
                    setTimeout(function () {
                        buy_step_swiper.slideTo(2);
                    }, 1500);
                }
                break;
        }
    });

    /* 根據畫面高度判斷切換Swiper是否滑動到上方 */
    top_position = $(".swiper").offset().top;

    $(window).scroll(function () {
        var topPosition = $(".swiper").offset().top - $("header").height();
        if (document.body.scrollTop > topPosition || document.documentElement.scrollTop > topPosition) {
            gotop_switch = true;
        } else {
            gotop_switch = false;
        }
    });

    /* 鍵盤輸入欄位檢測 */
    document.addEventListener("keyup", AutoSwapInput);

    /* 選按Input與label不拖動到swiper */
    $("input,label,button").mousedown(function (e) {
        buy_step_swiper.allowTouchMove = false;
    })

    $("input,label,button").mouseup(function (e) {
        buy_step_swiper.allowTouchMove = true;
    })

    /* Step2 Form 檢測 */
    ShippingForms = $('#RadioShipping');
    PaymentForms = $('#RadioPayment');

    $(".btn_step2_next").on("click", Step2Monitor);

    /* Step3 Form 檢測 */
    OrdererForms = $('#OrdererForm > form');
    RecipientForms = $('#RecipientForm > form');
    InvoiceForms = $('#InvoiceForm > form');

    $(".btn_checkout").on("click", Step3Monitor);

    /* Button */
    $(".btn_back_to_check").on("click", function () {
        buy_step_swiper.slideTo(0);
    });

    $(".btn_goprev").on("click", function () {
        buy_step_swiper.slidePrev();
    });

    $(".btn_edit_data").on("click", OrdererEdit);
    $(".btn_delete_recipient").on("click", DeleteRecipient);

    /* Radio Button */
    $('input[type=radio][name=RadioShipping]').change(RadioShipping);
    $('input[type=radio][name=RadioPayment]').change(RadioPayment);
    $('input[type=radio][name=RecipientRadio]').change(RecipientRadio);
    $('input[type=radio][name=InvoiceRadio]').change(InvoiceRadio);

}

/* 元素初始化 */
function ElementInit() {
    /* TWzipcode 初始化 */
    $Orderer_TWzipcode = $('#Orderer_TWzipcode');
    $Recipient_TWzipcode = $('#Recipient_TWzipcode');
    $Invoice_TWzipcode = $('#Invoice_TWzipcode');
    TWZipCodeInit();

    /* 寄件者資訊 */
    $orderer_name = $("#OrdererInputName");
    $orderer_sex = $("input[name=OrdererRadioGender]");
    $orderer_email = $("#OrdererInputMail");
    $orderer_cellphone = $("#OrdererInputCellPhone");
    $orderer_telphone_area = $("#OrdererInputTelPhoneArea");
    $orderer_telphone = $("#OrdererInputTelPhone");
    $orderer_telphone_ext = $("#OrdererInputTelPhoneExt");
    $orderer_address_city = $Orderer_TWzipcode.children('.county').children("select");
    $orderer_address_town = $Orderer_TWzipcode.children('.district').children("select");
    $orderer_address = $("#OrdererInputAddress");

    /* 收件者資訊 */
    $recipient_radio = $("input[name=RecipientRadio]");
    $recipient_name = $("#RecipientInputName");
    $recipient_sex = $("input[name=RecipientRadioGender]");
    $recipient_email = $("#RecipientInputMail");
    $recipient_cellphone = $("#RecipientInputCellPhone");
    $recipient_telphone_area = $("#RecipientInputTelPhoneArea");
    $recipient_telphone = $("#RecipientInputTelPhone");
    $recipient_telphone_ext = $("#RecipientInputTelPhoneExt");
    $recipient_address_city = $Recipient_TWzipcode.children('.county').children("select");
    $recipient_address_town = $Recipient_TWzipcode.children('.district').children("select");
    $recipient_address = $("#RecipientInputAddress");
    $remark = $("#TextareaRemark");

    /* 發票 */
    $invoice_recipient = $("input[name=InvoiceRadio]");
    $invoice_title = $("#InvoiceInputTitle");
    $invoice_uniformid = $("#InvoiceInputUniformId");
    $invoice_address_city = $Invoice_TWzipcode.children('.county').children("select");
    $invoice_address_town = $Invoice_TWzipcode.children('.district').children("select");
    $invoice_address = $("#InvoiceInputAddress");

    /* 運送、付款方式 */
    $ship_method = $("input[name=RadioShipping]");
    $ship_method.each(function () {
        if ($(this).is(":checked")) {
            shipping = $(this).val();
            ori_freight = $(this).data("freight");
            low_con = $(this).data("lowcon");
            disfreight = $(this).data("disfreight");
            freight = ori_freight
        } else {
            freight = 0;
        }
    })
    $pay_method = $("input[name=RadioPayment]");
}

function CartInit() {
    Product.GetAll.Cart().done(function (result) {
        if (result.length > 0) {
            $("#Step1 > .card-body").removeClass("d-none");
            for (var i = 0; i < result.length; i++) {
                CartAdd(result[i])
            }
            $("#Purchase_Null").addClass("d-none");
            buy_step_swiper.enable();

            var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
            var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
                return new bootstrap.Popover(popoverTriggerEl)
            })

            buy_step_swiper.update();
            TotalCount();
        }
    })
}

function CartAdd(result) {
    var item = $($("#Template_Cart_Details").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_instructions = item.find(".pro_instructions"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_total = item.find(".pro_subtotal"),
        item_btn_count_plus = item.find(".btn_count_plus"),
        item_btn_count_minus = item.find(".btn_count_minus"),
        item_btn_remove_pro = item.find(".btn_remove_pro"),
        item_btn_move_to_favorites = item.find(".btn_move_to_favorites");

    item.data("scid", result.scId);
    item_link.attr("href", `${OrgName}/Toilet/` + result.pId);
    item_link.on("click", function () {
        ClickLog(result.pId);
    });
    item_image.attr("src", result.imagePath);
    item_name.text(result.title);
    item_specification.append(result.s1Title == "" ? "" : '<span class="border px-1 me-1">' + result.s1Title + '</span>')
    item_specification.append(result.s2Title == "" ? "" : '<span class="border px-1">' + result.s2Title + '</span>')
    item_instructions.append(result.description.replaceAll("\n", "<br/>"))
    item_unit.text((result.price).toLocaleString('en-US'))
    item_quantity.val(result.quantity);
    item_total.data("subtotal", result.price * result.quantity)
    item_total.text(item_total.data("subtotal").toLocaleString('en-US'))

    item_btn_remove_pro.on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            CartDelete($self, $self.data("scid"), "成功移除商品", "移除商品發生未知錯誤")
        });
    });
    item_btn_move_to_favorites.on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品加入收藏？", "該商品將會加入收藏並從購物車中移除", "加入收藏", "取消", function () {
            CartDelete($self, $self.data("scid"), "成功移除商品", "移除商品發生未知錯誤")
        });
    });
    item_btn_count_plus.on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        $self_bro.val(parseInt($self_bro.val()) + 1)
        CartQuantityUpdate(item_total, result.price, item.data("scid"), $self_bro.val());
    });
    item_btn_count_minus.on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        if ($self_bro.val() > 1) {
            $self_bro.val(parseInt($self_bro.val()) - 1)
            CartQuantityUpdate(item_total, result.price, item.data("scid"), $self_bro.val());
        }
    });
    item_quantity.on("change", function () {
        var $self = $(this);
        if ($self.val() < 1) { $self.val(1); }
        CartQuantityUpdate(item_total, result.price, item.data("scid"), $self.val());
    });

    var item_list_ul = $("#Step1 > .card-body > .purchase_list");

    item_list_ul.append(item);
}

function CartQuantityUpdate(self, price, scid, quantity) {
    Product.Update.Cart({
        Id: scid,
        FK_Tid: $.cookie("Token"),
        Quantity: quantity,
    }).done(function (result) {
        Product.GetOne.Cart(result.message).done(function (result) {
            self.data("subtotal", price * quantity)
            self.text(self.data("subtotal").toLocaleString('en-US'))
            TotalCount();
            CartDropReset(scid, quantity)
        });
    }).fail(function () {
        Coker.sweet.error("錯誤", "商品數量修改發生錯誤", null, true);
    });
}

function TotalCount() {
    subtotal = 0;
    $('.purchase_list').children("li").each(function () {
        subtotal += $(this).children(".content").children(".pro_subtotal").data("subtotal");
        if (disfreight > 0 && subtotal > low_con) {
            freight = disfreight;
        } else {
            freight = ori_freight;
        }
    })
    $(".subtotal").each(function () {
        $(this).text(subtotal.toLocaleString('en-US'))
    })
    $(".shipping_fee").each(function () {
        $(this).text(freight == null ? 0 : freight.toLocaleString('en-US'))
    })
    $(".total_amount").each(function () {
        total = freight == null ? subtotal : subtotal + freight;
        $(this).text(total.toLocaleString('en-us'))
    })
}

function CartDelete(self, id, success, error) {
    self.remove();
    Product.Delete.Cart(id).done(function () {
        Coker.sweet.success(success, null, true);
        CartDropReset(id, 0)
        TotalCount();
        if (parseInt($("#Car_Badge").text()) == 0) {
            DetailsClear();
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error, null, true);
    })
}

function Step2Monitor() {
    shipMethodsChosen = FormCheck(ShippingForms);
    payMethodsChosen = FormCheck(PaymentForms);

    if (!(shipMethodsChosen && payMethodsChosen)) {
        Coker.sweet.error("請確實選擇運送及付款方式！", null, true);
    } else {
        buy_step_swiper.slideNext();
    }

    buy_step_swiper.update();
}

function RadioShipping() {
    ori_freight = $(this).data("freight");
    low_con = $(this).data("lowcon");
    disfreight = $(this).data("disfreight");
    freight = ori_freight
    TotalCount();
}

function RadioPayment() {
    var $pay_text = $(".payment_method");
    $pay_text.addClass("fs-2 fw-bold px-3");
    $pay_method.each(function () {
        if ($(this).is(":checked")) {
            var val = $(this).val();
            if (val == 1) {
                $(".pay_byATM").removeClass("d-none");
            } else {
                $(".pay_byATM").addClass("d-none");
            }
            $(paymentArr).each(function () {
                var e = this;
                if (e.value == val) {
                    $pay_text.text(e.key);
                }
            });
        }
    })
    buy_step_swiper.update();
}

function Step3Monitor() {
    if (OrdererOpen) { OrdererFilled = FormCheck(OrdererForms) };
    if (RecipientOpen) { RecipientFilled = FormCheck(RecipientForms) };
    if (InvoiceOpen) { InvoiceFilled = FormCheck(InvoiceForms) };

    if (!(OrdererFilled && RecipientFilled && InvoiceFilled)) {
        Coker.sweet.error("請確實填寫資料！", null, true);
    } else {
        Coker.sweet.confirm("是否確定結帳？", "點選確認進入付款流程", "是，開始付款", "否", function () {
            OrderHeaderAdd();
        });
    }
    buy_step_swiper.update();
}

function OrdererEdit() {
    $("#OrdererForm > .default_data").toggleClass("d-none");
    $("#OrdererForm > form").toggleClass("d-none");
    OrdererOpen = !OrdererOpen;
    OrdererFilled = !OrdererOpen;
    buy_step_swiper.update();
}

function RecipientRadio() {
    var $self = $(this)
    if ($self.val() == "edit") {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").removeClass("d-none");
        RecipientOpen = true;
        RecipientFormClear();
    } else {
        $("#RecipientForm > .default_data").removeClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
    }
    buy_step_swiper.update();
}

function RecipientFormClear() {
    $recipient_name.val("");
    $recipient_sex.val("");
    $recipient_sex.each(function () {
        $(this).removeAttr("checked");
    })
    $recipient_email.val("");
    $recipient_cellphone.val("");
    $recipient_telphone_area.val("");
    $recipient_telphone.val("");
    $recipient_telphone_ext.val("");
    $recipient_address_city.val("");
    $recipient_address_town.val("");
    $recipient_address.val("");
    $remark.val("");
}

function RecipientFormSet(name, sex, email, cellphone, telphone_area, telphone, telphone_ext, address_city, address_town, address, remark) {
    $recipient_name.val(name);
    $recipient_sex.each(function () {
        if ($(this).val() == sex) {
            $(this).prop("checked", true);
        }
    })
    $recipient_email.val(email);
    $recipient_cellphone.val(cellphone);
    $recipient_telphone_area.val(telphone_area);
    $recipient_telphone.val(telphone);
    $recipient_telphone_ext.val(telphone_ext);
    $Recipient_TWzipcode.twzipcode('set', {
        'county': address_city,
        'district': address_town,
    });
    $recipient_address.val(address);
    $remark.val(remark);
}

function InvoiceRadio() {
    if (this.value == 3) {
        $("#InvoiceForm > .default_data").addClass("d-none");
        $("#InvoiceForm > form").removeClass("d-none");
        InvoiceOpen = true;
        InvoiceFormClear();
    } else {
        $("#InvoiceForm > .default_data").removeClass("d-none");
        $("#InvoiceForm > form").addClass("d-none");
        InvoiceOpen = false;
        InvoiceFilled = true;
    }
    buy_step_swiper.update();
}

function InvoiceFormClear() {
    $invoice_title.val("");
    $invoice_uniformid.val("");
    $invoice_address_city.val("");
    $invoice_address_town.val("");
    $invoice_address.val("");
}

function InvoiceFormSet(title, uniformid, address_city, address_town, address) {
    $invoice_title.val(title);
    $invoice_uniformid.val(uniformid);
    $Invoice_TWzipcode.twzipcode('set', {
        'county': address_city,
        'district': address_town,
    });
    $invoice_address.val(address);
}

/* 表單驗證 */
function FormCheck(Forms) {
    var Check = false;
    Array.from(Forms).forEach(form => {
        if (form.checkValidity()) {
            Check = true;
        }
        form.classList.add('was-validated')
    })
    return Check;
}

function DetailsClear() {
    $("#Step1 > .card-body").addClass("d-none");
    $("#Purchase_Null").removeClass("d-none");
    buy_step_swiper.disable();
}

function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}

function OrderHeaderAdd() {
    var orderer_sex, orderer_telephone;
    $orderer_sex.each(function () {
        if ($(this).is(":checked")) { orderer_sex = $(this).val(); }
    })
    orderer_telephone = $orderer_telphone.val() != null ? ($orderer_telphone_area.val() + "-" + $orderer_telphone.val() + ($orderer_telphone_ext.val() != null ? "-" + $orderer_telphone_ext.val() : "")) : "";

    var recipient_radio, recipient_sex, recipient_telephone;
    $recipient_radio.each(function () {
        if ($(this).is(":checked")) { recipient_radio = $(this).val(); }
    })
    switch (recipient_radio) {
        case "order":
            RecipientFormSet($orderer_name.val(), orderer_sex, $orderer_email.val(), $orderer_cellphone.val(), $orderer_telphone_area.val(), $orderer_telphone.val(), $orderer_telphone_ext.val(), $orderer_address_city.val(), $orderer_address_town.val(), $orderer_address.val(), "")
            break;
        case "choose":
            RecipientFormSet($orderer_name.val(), orderer_sex, $orderer_email.val(), $orderer_cellphone.val(), $orderer_telphone_area.val(), $orderer_telphone.val(), $orderer_telphone_ext.val(), $orderer_address_city.val(), $orderer_address_town.val(), $orderer_address.val(), "")
            break;
    }
    $recipient_sex.each(function () {
        if ($(this).is(":checked")) { recipient_sex = $(this).val(); }
    })
    recipient_telephone = $recipient_telphone.val() != null ? ($recipient_telphone_area.val() + "-" + $recipient_telphone.val() + ($recipient_telphone_ext.val() != null ? "-" + $recipient_telphone_ext.val() : "")) : "";

    var invoice_recipient;
    $invoice_recipient.each(function () {
        if ($(this).is(":checked")) { invoice_recipient = $(this).val(); }
    })
    switch (invoice_recipient) {
        case "1":
            InvoiceFormSet("", "", $orderer_address_city.val(), $orderer_address_town.val(), $orderer_address.val())
            break;
        case "2":
            InvoiceFormSet("", "", $recipient_address_city.val(), $recipient_address_town.val(), $recipient_address.val())
            break;
    }

    $ship_method.each(function () {
        if ($(this).is(":checked")) { shipping = $(this).val(); }
    })
    $pay_method.each(function () {
        if ($(this).is(":checked")) { payment = $(this).val(); }
    })

    Coker.Order.AddHeader({
        Orderer: $orderer_name.val(),
        OrdererSex: orderer_sex,
        OrdererEmail: $orderer_email.val(),
        OrdererTelephone: orderer_telephone,
        OrdererCellPhone: $orderer_cellphone.val(),
        OrdererAddress: $orderer_address_city.val() + $orderer_address_town.val() + " " + $orderer_address.val(),
        Recipient: $recipient_name.val(),
        RecipientSex: recipient_sex,
        RecipientEmail: $recipient_email.val(),
        RecipientTelephone: recipient_telephone,
        RecipientCellPhone: $recipient_cellphone.val(),
        RecipientAddress: $recipient_address_city.val() + $recipient_address_town.val() + " " + $recipient_address.val(),
        Remark: $remark.val(),
        InvoiceRecipient: invoice_recipient,
        InvoiceTitle: $invoice_title.val(),
        UniformId: $invoice_uniformid.val(),
        InvoiceAddress: $invoice_address_city.val() + $invoice_address_town.val() + " " + $invoice_address.val(),
        Shipping: shipping,
        Payment: payment,
        State: 1,
        Subtotal: subtotal,
        Discount: 0,
        Bonus: 0,
        CouponId: 0,
        Freight: freight,
        Service_Charge: 0
    }).done(function (result) {
        Coker.Order.GetHeader(result.message).done(function (result) {
            OrderDetailsAdd(result.id, result)
            Coker.sweet.success("謝謝您的訂購！<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！", function () {
                setTimeout(function () {
                    isCheckout = true;
                    setTimeout(function () {
                        buy_step_swiper.slideNext();
                        buy_step_swiper.disable();
                    }, 300);
                }, 300);
            })
        })
    }).fail(function () {
        Coker.sweet.error("錯誤", "訂購商品發生未知錯誤", null, true);
    });
}

function OrderDetailsAdd(ohid, oh_result) {
    var scarr = new Array();
    var first_scid
    $("#Step1").find(".purchase_item").each(function () {
        first_scid = first_scid == null ? $(this).data("scid") : first_scid;
        scarr.push($(this).data("scid"));
    })

    Coker.Order.AddDetails({
        FK_OHId: ohid,
        FK_SCId_Arr: scarr,
        FK_TId: $.cookie("Token"),
    }).done(function (result) {
        if (result.success) {
            OrderSuccess(oh_result);
        }
    });
}

function OrderSuccess(oh_result) {
    CartClear();

    $("#Step4 > .card-header > .order_number").text(("000000000" + oh_result.id).substr(oh_result.id.length));

    $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，謝謝您的訂購！");

    Coker.Order.GetDetails(oh_result.id).done(function (result) {
        if (result.length > 0) {
            if (result.length > 1) {
                $(".btn_view_list").removeClass("d-none")
            }
            for (var i = 0; i < result.length; i++) {
                if (i == 0) {
                    PurchaseAdd(result[i], $("#Step4 > .card-body > .pruchase_content > .purchase_list").first())
                } else {
                    PurchaseAdd(result[i], $("#Step4 > .card-body > .pruchase_content > .purchase_list.collapse"))
                }
            }
        }
    })

    var order_item = $("#Step4 > .card-body > .orderer_data");
    order_item.find(".name").text(HiddenCode(1, oh_result.orderer));
    order_item.find(".cellphone").text(HiddenCode(2, oh_result.ordererCellPhone));
    if (oh_result.ordererTelephone != null) {
        order_item.find(".telphone").parents("div").first().removeClass("d-none");
        order_item.find(".telphone").text(HiddenCode(3, oh_result.ordererTelephone));
    }

    var recipient_item = $("#Step4 >.card-body >  .recipient_data");
    recipient_item.find(".name").text(HiddenCode(1, oh_result.recipient));
    recipient_item.find(".cellphone").text(HiddenCode(2, oh_result.recipientCellPhone));
    if (oh_result.recipientTelephone != null) {
        recipient_item.find(".telphone").parents("div").first().removeClass("d-none");
        recipient_item.find(".telphone").text(HiddenCode(3, oh_result.recipientTelephone));
    }
    recipient_item.find(".address").text(HiddenCode(4, oh_result.recipientAddress));

    var invoice_item = $("#Step4 >.card-body >  .invoice_data");
    switch (oh_result.invoiceRecipient) {
        case 1:
            invoice_item.find(".person").removeClass("d-none");
            invoice_item.find(".name").text(order_item.find(".name").text());
            invoice_item.find(".cellphone").text(order_item.find(".cellphone").text());
            if (order_item.find(".telphone").text() != null) {
                invoice_item.find(".telphone").parents("div").first().removeClass("d-none");
                invoice_item.find(".telphone").text(order_item.find(".telphone").text());
            }
            break;
        case 2:
            invoice_item.find(".person").removeClass("d-none");
            invoice_item.find(".name").text(recipient_item.find(".name").text());
            invoice_item.find(".cellphone").text(recipient_item.find(".cellphone").text());
            if (recipient_item.find(".telphone").text() != null) {
                invoice_item.find(".telphone").parents("div").first().removeClass("d-none");
                invoice_item.find(".telphone").text(recipient_item.find(".telphone").text());
            }
            break;
        case 3:
            invoice_item.find(".company").removeClass("d-none");
            invoice_item.find(".invoice").text(oh_result.invoiceTitle);
            invoice_item.find(".unid").text(oh_result.uniformId);
            break;
    }
    invoice_item.find(".address").text(HiddenCode(4, oh_result.invoiceAddress));

}

function PurchaseAdd(result, item_list_ul) {
    var item = $($("#Template_Purchase_Details").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_instructions = item.find(".pro_instructions"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_subtotal = item.find(".pro_subtotal");

    item_link.attr("href", "${OrgName}/Toilet/" + result.pId);
    item_link.on("click", function () {
        ClickLog(result.pId);
    });
    item_image.attr("src", result.imagePath);
    item_name.text(result.title);
    item_specification.append(result.s1Title == "" ? "" : '<span class="border px-1 me-1">' + result.s1Title + '</span>')
    item_specification.append(result.s2Title == "" ? "" : '<span class="border px-1">' + result.s2Title + '</span>')
    item_instructions.text(result.description);
    item_unit.text((result.price).toLocaleString('en-US'))
    item_quantity.text(result.quantity);
    item_subtotal.text((result.price * result.quantity).toLocaleString('en-US'))

    item_list_ul.append(item);
}

function HiddenCode(type, data) {
    switch (type) {
        case 1:
            if (data.length > 2) {
                return (data.substr(0, 1) + "○" + (data.substr(data.length - 1)));
            } else {
                return (data.substr(0, 1) + "○");
            }
            break;
        case 2:
            return (data.substr(0, 3) + "****" + data.substr(7));
            break;
        case 3:
            var index1 = data.indexOf('-');
            var index2 = data.indexOf('-', index1);
            var new_data = (data.substr(index1 + 1, 2) + "***" + data.substr(index1 + 6, 2));
            var ext = data.length > (index2 + 1) ? "分機" + data.substr(index2 + 1, 1) + "***" : "";
            return new_data + ext;
            break;
        case 4:
            return (data.substr(0, data.indexOf(" ")) + "***");
            break;
    }
}

/* Input輸入自動切換 */
function AutoSwapInput() {
    var target = event.target

    if (target.nodeName == "INPUT" && target.className.indexOf("pro_quantity") < 0) {
        if (target.value.length == target.maxLength) {
            var elements = $(target).parents("form").first().find("input");
            for (let i = 0; i < elements.length; i++) {
                if (elements[i] == target) {
                    if (elements[i + 1]) {
                        elements[i + 1].focus();
                    }
                    return;
                }
            }
        }
    }
}

/* 地址選單初始化 */
function TWZipCodeInit() {
    $Orderer_TWzipcode.twzipcode({
        'zipcodeIntoDistrict': true,
        'countySel': '高雄市',
        'districtSel': '前鎮區'
    });
    $Recipient_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });
    $Invoice_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });

    var $county, $district;

    $county = $Orderer_TWzipcode.children('.county');
    $district = $Orderer_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "OrdererSelectCity",
        class: "orderer_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='OrdererSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "OrdererSelectTown",
        class: "orderer_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='OrdererSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');


    $county = $Recipient_TWzipcode.children('.county');
    $district = $Recipient_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "RecipientSelectCity",
        class: "recipient_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='RecipientSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "RecipientSelectTown",
        class: "recipient_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='RecipientSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');


    $county = $Invoice_TWzipcode.children('.county');
    $district = $Invoice_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "InvoiceSelectCity",
        class: "bill_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='InvoiceSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "InvoiceSelectTown",
        class: "bill_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='InvoiceSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');
}