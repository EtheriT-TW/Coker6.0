
var buy_step_swiper;
var gotop_switch = false;
var ShippingForms, PaymentForms, OrdererForms, RecipientForms, BillForms;
var OrdererOpen = false, RecipientOpen = false, BillOpen = false;
var shipMethodsChosen = false, payMethodsChosen = false, OrdererFilled = true, RecipientFilled = true, BillFilled = true;
var isCheckout = false;

function PageReady() {

    /* Swiper 畫面高度 */
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

    ReloadAllAmount();

    /* Popover */
    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    /* 欄位檢測 */
    document.addEventListener("keyup", AutoSwapInput);

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
                    if (BillOpen) { BillFilled = FormCheck(BillForms) };
                    Coker.sweet.error("未完成結帳流程！", "若資料已確實填寫完畢，請點選下方[確認付款]按鈕進入付款程序", null, false);
                    setTimeout(function () {
                        buy_step_swiper.slideTo(2);
                    }, 1500);
                }
                break;
        }
    });

    $("input, label").mousedown(function (e) {
        buy_step_swiper.allowTouchMove = false;
    })

    $("input, label").mouseup(function (e) {
        buy_step_swiper.allowTouchMove = true;
    })

    /* Step2 Form */
    ShippingForms = $('#ShippingRadio');
    PaymentForms = $('#PaymentRadio');

    $(".btn_step2_next").on("click", Step2Monitor);

    /* Step3 Form */
    OrdererForms = $('#OrdererForm > form');
    RecipientForms = $('#RecipientForm > form');
    BillForms = $('#BillForm > form');

    $(".btn_checkout").on("click", Step3Monitor);

    /* Button */
    $(".btn_gofirst").on("click", function () {
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
    $('input[type=radio][name=ShippingRadio]').change(ShippingRadio);
    $('input[type=radio][name=PaymentRadio]').change(PaymentRadio);
    $('input[type=radio][name=RecipientRadio]').change(RecipientRadio);
    $('input[type=radio][name=BillRadio]').change(BillRadio);

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

function ShippingRadio() {
    $.cookie('shipping_fee', this.value, { path: '/' });
    AllAmountChange();
}

function PaymentRadio() {
    $.cookie('payment_method', this.value, { path: '/' });
    var $payment = $(".payment_method");
    $payment.text($.cookie('payment_method'));
    $payment.addClass("fs-2 fw-bold px-3");
    if ($.cookie('payment_method') == 'ATM') {
        $(".pay_byATM").removeClass("d-none");
    } else {
        $(".pay_byATM").addClass("d-none");
    }
    buy_step_swiper.update();
}

function Step3Monitor() {

    if (OrdererOpen) { OrdererFilled = FormCheck(OrdererForms) };
    if (RecipientOpen) { RecipientFilled = FormCheck(RecipientForms) };
    if (BillOpen) { BillFilled = FormCheck(BillForms) };

    if (!(OrdererFilled && RecipientFilled && BillFilled)) {
        Coker.sweet.error("請確實填寫資料！", null, true);
    } else {
        Coker.sweet.confirm("是否確定結帳？", "點選確認進入付款流程", "是，開始付款", "否", function () {
            Coker.sweet.success("謝謝您的訂購！<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！", function () {
                setTimeout(function () {
                    isCheckout = true;
                    setTimeout(function () {
                        buy_step_swiper.slideNext();
                        buy_step_swiper.disable();
                    }, 300);
                }, 300);
            })
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
    if (this.value == 'edit') {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").removeClass("d-none");
        RecipientOpen = true;
    } else {
        $("#RecipientForm > .default_data").removeClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
    }
    buy_step_swiper.update();
}

function BillRadio() {
    if (this.value == 'company') {
        $("#BillForm > .default_data").addClass("d-none");
        $("#BillForm > form").removeClass("d-none");
        BillOpen = true;
    } else {
        $("#BillForm > .default_data").removeClass("d-none");
        $("#BillForm > form").addClass("d-none");
        BillOpen = false;
        BillFilled = true;
    }
    buy_step_swiper.update();
}

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

function AmountPlus() {
    var $self_unit = $(this).parents(".content").first().children(".pro_unit");
    var $self_subtotal = $(this).parents(".content").first().children(".pro_subtotal");
    var $self_input_count = $(this).parents(".counter_input").first().children("input");
    $self_input_count.val(parseInt($self_input_count.val()) + 1);
    var subtotal = parseInt($self_unit.data('unitprice')) * parseInt($self_input_count.val());
    $self_subtotal.text(subtotal.toLocaleString('en-US'));

    $.cookie('subtotal', parseInt($.cookie('subtotal')) + $self_unit.data('unitprice'), { path: '/' })
    AllAmountChange();
}

function AmountMinus() {
    var $self_unit = $(this).parents(".content").first().children(".pro_unit");
    var $self_subtotal = $(this).parents(".content").first().children(".pro_subtotal");
    var $self_input_count = $(this).parents(".counter_input").first().children("input");

    if (parseInt($self_input_count.val()) > 1) {
        $self_input_count.val(parseInt($self_input_count.val()) - 1);
        var subtotal = parseInt($self_unit.data('unitprice')) * parseInt($self_input_count.val());
        $self_subtotal.text(subtotal.toLocaleString('en-US'));

        $.cookie('subtotal', parseInt($.cookie('subtotal')) - $self_unit.data('unitprice'), { path: '/' })
        AllAmountChange();
    }
}

function AmountEnter() {
    var $self = $("#Step1 > .card-body > .purchase_list > .purchase_item > .content");
    var $self_unit = $self.children(".pro_unit");
    var $self_subtotal = $self.children(".pro_subtotal");
    var $self_input_count = $self.children(".counter_input").children(".pro_quantity");

    if ($self_input_count.val() <= 0) {
        $self_input_count.val(1);
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
}

function MoveToFavorites() {
    $selfparent = $(this).parents("li").first();
    Coker.sweet.confirm("確定將商品加入收藏？", "該商品將會加入收藏並從購物車中移除", "加入收藏", "取消", function () {
        $selfparent.remove();
        Coker.sweet.success("成功加入收藏！", null, true);
        ReloadAllAmount();
    });
}

function RemoveProduct() {
    $selfparent = $(this).parents("li").first();
    Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
        $selfparent.remove();
        Coker.sweet.success("成功移除商品", null, true);
        ReloadAllAmount();
    });
}

function ReloadAllAmount() {
    var total_price = 0;

    $(".purchase_item").each(function () {
        var $self_unit = $(this).children(".content").children(".pro_unit");
        var $self_subtotal = $(this).children(".content").children(".pro_subtotal");
        total_price = total_price + $self_unit.data('unitprice');
        $self_subtotal.text($self_unit.data('unitprice').toLocaleString('en-US'));
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

function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}