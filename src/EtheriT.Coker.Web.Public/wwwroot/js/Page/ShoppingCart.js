
var buy_step_swiper;
var gotop_switch = false;
var isCheckout = false;

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
        Add: function (data) {
            return $.ajax({
                url: "/api/Order/Add",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    };

    /* Buy Swiper */
    buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        autoHeight: true,
        loop: false,
        pagination: {
            el: ".swiper_pagination > .swiper_pagination_buystep",
            clickable: true,
            renderBullet: function (index, className) {
                return '<span class="' + className + '">' + (index + 1) + "</span>";
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
                shipMethodsChosen = FormCheck(ShippingForms);
                payMethodsChosen = FormCheck(PaymentForms);
                if (!(shipMethodsChosen && payMethodsChosen)) {
                    Coker.sweet.error("請確實選擇運送及付款方式！", null, true);
                    setTimeout(function () {
                        buy_step_swiper.slideTo(1);
                    }, 1500);
                }
                break;
            case 3:
                if (isCheckout) {
                    $("#Pruchase_Content > .status_alert").text("訂單已成立，謝謝您的訂購！");
                } else {
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

    ElementInit();

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

    /* Cookie 資料初始設置 */
    $.cookie('subtotal', '', { path: '/' });
    $.cookie('shipping_fee', '', { path: '/' });
    $.cookie('total_amount', '', { path: '/' });
    $.cookie('payment_method', '', { path: '/' });

    HasProduct() && ReloadAllAmount();

    /* Popover Init */
    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    /* 鍵盤輸入欄位檢測 */
    document.addEventListener("keyup", AutoSwapInput);

    /* 選按Input與label不拖動到swiper */
    $("input, label").mousedown(function (e) {
        buy_step_swiper.allowTouchMove = false;
    })

    $("input, label").mouseup(function (e) {
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

    $(".btn_move_to_favorites").on("click", MoveToFavorites);
    $(".btn_remove_pro").on("click", RemoveProduct);

    $(".btn_count_plus").on("click", AmountPlus);
    $(".btn_count_minus").on("click", AmountMinus);

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
    $pay_method = $("input[name=RadioPayment]");
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
    $.cookie('shipping_fee', this.value, { path: '/' });
    AllAmountChange();
}

function RadioPayment() {
    $.cookie('payment_method', this.value, { path: '/' });
    var $payment = $(".payment_method");
    $payment.text($.cookie('payment_method'));
    $payment.addClass("fs-2 fw-bold px-3");
    if ($.cookie('payment_method') == '1') {
        $(".pay_byATM").removeClass("d-none");
    } else {
        $(".pay_byATM").addClass("d-none");
    }
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
            OrderAdd();
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

/* 數量修改 */
function AmountPlus() {
    var $self_unit = $(this).parents(".content").first().children(".pro_unit");
    var $self_subtotal = $(this).parents(".content").first().children(".pro_subtotal");
    var $self_input_count = $(this).parents(".counter_input").first().children("input");
    $self_input_count.val(parseInt($self_input_count.val()) + 1);
    $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + 1, { path: '/' })
    var subtotal = parseInt($self_unit.data('unitprice')) * parseInt($self_input_count.val());
    $self_subtotal.text(subtotal.toLocaleString('en-US'));

    $.cookie('subtotal', parseInt($.cookie('subtotal')) + $self_unit.data('unitprice'), { path: '/' })
    AllAmountChange();
    CarItemChange();
}

function AmountMinus() {
    var $self_unit = $(this).parents(".content").first().children(".pro_unit");
    var $self_subtotal = $(this).parents(".content").first().children(".pro_subtotal");
    var $self_input_count = $(this).parents(".counter_input").first().children("input");

    if (parseInt($.cookie('Purchased_Item_Quantity')) > 1) {
        $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) - 1, { path: '/' })
        $self_input_count.val(parseInt($self_input_count.val()) - 1);
        var subtotal = parseInt($self_unit.data('unitprice')) * parseInt($self_input_count.val());
        $self_subtotal.text(subtotal.toLocaleString('en-US'));

        $.cookie('subtotal', parseInt($.cookie('subtotal')) - $self_unit.data('unitprice'), { path: '/' })
        AllAmountChange();
        CarItemChange();
    }
}

function AmountEnter() {
    var $self = $("#Step1 > .card-body > .purchase_list > .purchase_item > .content");
    var $self_unit = $self.children(".pro_unit");
    var $self_subtotal = $self.children(".pro_subtotal");
    var $self_input_count = $self.children(".counter_input").children(".pro_quantity");

    if ($self_input_count.val() <= 0) {
        $self_input_count.val(1);
        $.cookie('Purchased_Item_Quantity', 1, { path: '/' })
    } else {
        $.cookie('Purchased_Item_Quantity', $self_input_count.val(), { path: '/' })
    }
    var subtotal = parseInt($self_unit.data('unitprice')) * parseInt($self_input_count.val());
    $self_subtotal.text(subtotal.toLocaleString('en-US'));

    var total_price = 0;

    $(".purchase_item").each(function () {
        var $self_unit = $(this).children(".content").children(".pro_unit");
        var $self_quantity = $(this).children(".content").children(".counter_input").children(".pro_quantity");
        total_price = total_price + ($self_unit.data('unitprice') * $self_quantity.val());
    });

    $.cookie('subtotal', total_price, { path: '/' })

    AllAmountChange();
    CarItemChange();
}

/* 商品移動至喜愛/移除 */
function MoveToFavorites() {
    $selfparent = $(this).parents("li").first();
    Coker.sweet.confirm("確定將商品加入收藏？", "該商品將會加入收藏並從購物車中移除", "加入收藏", "取消", function () {
        $selfparent.remove();
        Coker.sweet.success("成功加入收藏！", null, true);
        $.cookie("Purchased_Type_Quantity", 0, { path: '/' });
        $.cookie("Purchased_Item_Quantity", 0, { path: '/' });
        HasProduct() && ReloadAllAmount();
        buy_step_swiper.update();
        CarDropdownReset();
    });
}

function RemoveProduct() {
    $selfparent = $(this).parents("li").first();
    Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
        $selfparent.remove();
        Coker.sweet.success("成功移除商品", null, true);
        $.cookie("Purchased_Type_Quantity", 0, { path: '/' });
        $.cookie("Purchased_Item_Quantity", 0, { path: '/' });
        HasProduct() && ReloadAllAmount();
        buy_step_swiper.update();
        CarDropdownReset();
    });
}

/* 購物車無商品顯示的內容 */
function HasProduct() {
    if ($.cookie("Purchased_Type_Quantity") > 0) {
        $("#Purchase_Null").addClass('d-none');
        $("#Step1 > .card-body").removeClass('d-none');
        buy_step_swiper.enable();
        PurchaseListInit();
    } else {
        $("#Purchase_Null").removeClass('d-none');
        $("#Step1 > .card-body").addClass('d-none');
        buy_step_swiper.disable();
        return false;
    }
    return true;
}

/* 暫時用來輸入購買資料 */
function PurchaseListInit() {
    var item = $($("#Template_Purchase_Details").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_instructions = item.find(".pro_instructions"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity");

    item_link.attr("href", "/Toilet/01");
    item_image.attr("src", "../images/product/pro_pic_01.jpg");
    item_name.text("CS230 一段省水分離式幼兒馬桶");
    item_specification.text("白色");
    item_instructions.text("規格說明文字");
    item_unit.data('unitprice', 9100);
    item_quantity.val($.cookie('Purchased_Item_Quantity'));

    var item_list_ul = $("#Step1 > .card-body > .purchase_list");

    item_list_ul.append(item);
}

/* 刪除收件人 */
function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}

function OrderAdd() {
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

    var shipping, payment;
    $ship_method.each(function () {
        if ($(this).is(":checked")) { shipping = $(this).val(); }
    })
    $pay_method.each(function () {
        if ($(this).is(":checked")) { payment = $(this).val(); }
    })

    console.log("recipient_sex = " + recipient_sex)
    console.log("invoice_recipient = " + invoice_recipient)
    console.log("InvoiceAddress = " + $invoice_address_city.val() + $invoice_address_town.val() + $invoice_address.val())
    console.log("shipping = " + shipping)
    console.log("payment = " + payment)

    Coker.Order.Add({
        Orderer: $orderer_name.val(),
        OrdererSex: orderer_sex,
        OrdererEmail: $orderer_email.val(),
        OrdererTelephone: orderer_telephone,
        OrdererCellPhone: $orderer_cellphone.val(),
        OrdererAddress: $orderer_address_city.val() + $orderer_address_town.val() + $orderer_address.val(),
        Recipient: $recipient_name.val(),
        RecipientSex: recipient_sex,
        RecipientEmail: $recipient_email.val(),
        RecipientTelephone: recipient_telephone,
        RecipientCellPhone: $recipient_cellphone.val(),
        RecipientAddress: $recipient_address_city.val() + $recipient_address_town.val() + $recipient_address.val(),
        Remark: $remark.val(),
        InvoiceRecipient: invoice_recipient,
        InvoiceTitle: $invoice_title.val(),
        UniformId: $invoice_uniformid.val(),
        InvoiceAddress: $invoice_address_city.val() + $invoice_address_town.val() + $invoice_address.val(),
        Shipping: shipping,
        Payment: payment,
        State: 1,
        Total: 9100,
        Discount: 0,
        Bonus: 0,
        CouponId: 0,
        Freight: 0,
        Service_Charge: 0
    }).done(function () {
        Coker.sweet.success("謝謝您的訂購！<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！", function () {
            setTimeout(function () {
                isCheckout = true;
                setTimeout(function () {
                    $.cookie('Purchased_Type_Quantity', 0, { path: '/' });
                    $.cookie('Purchased_Item_Quantity', 0, { path: '/' });
                    CarDropdownReset();
                    buy_step_swiper.slideNext();
                    buy_step_swiper.disable();
                }, 300);
            }, 300);
        })
    }).fail(function () {
        Coker.sweet.error("錯誤", "訂購商品發生未知錯誤", null, true);
    });
}

/* 重新讀取全部訂單金額/小計 */
function ReloadAllAmount() {
    var total_price = 0;

    $(".purchase_item").each(function () {
        var $self_unit = $(this).children(".content").children(".pro_unit");
        var $self_quantity = $(this).children(".content").children(".counter_input").children(".pro_quantity");
        var $self_subtotal = $(this).children(".content").children(".pro_subtotal");
        $self_unit.text(($self_unit.data('unitprice')).toLocaleString('en-US'))
        total_price = total_price + $self_unit.data('unitprice');
        $self_subtotal.text((parseInt($self_unit.data('unitprice')) * $self_quantity.val()).toLocaleString('en-US'));
    });

    $.cookie('subtotal', total_price, { path: '/' })

    AllAmountChange();
}

function AllAmountChange() {

    $(".priceline > div > .subtotal").each(function () {
        $(this).text(parseInt($.cookie('subtotal')).toLocaleString('en-US'));
    });

    $(".priceline > div > .shipping_fee").each(function () {
        $(this).text(parseInt($.cookie('shipping_fee')).toLocaleString('en-US'));
    });

    $.cookie('total_amount', parseInt($.cookie('subtotal')) + parseInt($.cookie('shipping_fee')), { path: '/' });

    $(".total_amount").each(function () {
        $(this).text(parseInt($.cookie('total_amount')).toLocaleString('en-US'));
    });
}

/* Input輸入自動切換 */
function AutoSwapInput() {
    var target = event.target

    if (target.nodeName == "INPUT") {
        if (target.className.indexOf("pro_quantity") >= 0) {
            AmountEnter();
        } else {
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