var total_price = 0;
var total_price_end = 0;
var subtotal_price = 0;
var buy_step_swiper;

function PageReady() {

    $.cookie('subtotal', '');
    $.cookie('delivery_fee', '');
    $.cookie('total_amount', '');
    $.cookie('payment_method', '');

    ReloadAllAmount();

    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    var phone_front_box = document.getElementsByClassName("phone_front");
    var phone_back_box = document.getElementsByClassName("phone_back");
    var phone_ext_box = document.getElementsByClassName("phone_ext");
    document.addEventListener("keyup", tabForward);

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

    buy_step_swiper.on('activeIndexChange', function () {
        $('html, body').animate({ scrollTop: $(".swiper").offset().top - $("header").height() }, 0);
    });

    /* Swiper Button*/
    $(".btn_gofirst").on("click", function () {
        buy_step_swiper.slideTo(0);
    });

    $(".btn_goprev").on("click", function () {
        buy_step_swiper.slidePrev();
    });

    /* Normal Button */
    $(".btn_move_to_favorites").on("click", MoveToFavorites);
    $(".btn_remove_pro").on("click", RemoveProduct);
    $(".btn_count_plus").on("click", AmountPlus);
    $(".btn_count_minus").on("click", AmountMinus);
    $(".btn_edit_data").on("click", function () {
        $(this).parents("#OrdererForm").children("form").children("div").toggleClass("show");
        buy_step_swiper.update();
    });
    $(".btn_delete_recipient").on("click", DeleteRecipient);

    $(".btn_checkout").on("click", function () {
        Coker.sweet.confirm("是否確定結帳？", "點選確認進入付款流程", "是，開始付款", "否", function () {
            Coker.sweet.success("謝謝您的訂購！<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！", function () {
                setTimeout(function () {
                    buy_step_swiper.slideNext();
                }, 300);
            })
        });
    });

    /* Radio Button */
    $('input[type=radio][name=PaymentRadio]').change(PaymentRadio);
    $('input[type=radio][name=RecipientRadio]').change(RecipientRadio);
    $('input[type=radio][name=BillRadio]').change(BillRadio);

}

function PaymentRadio() {
    $.cookie('payment_method', this.value);
    var $payment = $(".payment_method");
    $payment.text($.cookie('payment_method'));
    $payment.addClass("fs-2 fw-bold");
    if ($.cookie('payment_method') == 'ATM') {
        $(".pay_byATM").removeClass("d-none");
    } else {
        $(".pay_byATM").addClass("d-none");
    }
    buy_step_swiper.update();
}

function RecipientRadio() {
    if (this.value == 'edit') {
        $("#RecipientForm > form > div").addClass("show")
    } else {
        $("#RecipientForm > form > div").removeClass("show");;
    }
    buy_step_swiper.update();
}

function BillRadio() {
    if (this.value == 'company') {
        $("#BillForm > .default_data").removeClass("show");
        $("#BillForm > form").addClass("show");
    } else {
        $("#BillForm > .default_data").addClass("show");
        $("#BillForm > form").removeClass("show");
    }
    buy_step_swiper.update();
}

function AmountPlus() {
    var $self_unit = $(this).parents("li").first().children(".content").children(".unit");
    var $self_subtotal = $(this).parents("li").first().children(".content").children(".subtotal");
    var $self_input_count = $(this).parents("li").first().children(".content").children(".counter_input").children(".input_count");
    $self_input_count.val(parseInt($self_input_count.val()) + 1);
    var subtotal = parseInt($self_unit.data('unittotal')) * parseInt($self_input_count.val());
    $self_subtotal.text(subtotal.toLocaleString('en-US'));

    total_price = total_price + $self_unit.data('unittotal');
    AllAmountChange();
}

function AmountMinus() {
    var $self_unit = $(this).parents("li").first().children(".content").children(".unit");
    var $self_subtotal = $(this).parents("li").first().children(".content").children(".subtotal");
    var $self_input_count = $(this).parents("li").first().children(".content").children(".counter_input").children(".input_count");
    if ($self_input_count.val() > 1) {
        $self_input_count.val(parseInt($self_input_count.val()) - 1);
        var subtotal = parseInt($self_unit.data('unittotal')) * parseInt($self_input_count.val());
        $self_subtotal.text(subtotal.toLocaleString('en-US'));
        total_price = total_price - $self_unit.data('unittotal');
        AllAmountChange();
    }
}

function ReloadAllAmount() {
    $.cookie('subtotal', '');
    total_price = 0;
    total_price_end = 0;
    subtotal_price = 0;

    $(".purchase_list > li").each(function () {
        var $self_unit = $(this).children(".content").children(".unit");
        var $self_subtotal = $(this).children(".content").children(".subtotal");
        total_price = total_price + $self_unit.data('unittotal');
        $self_subtotal.text($self_unit.data('unittotal').toLocaleString('en-US'));
    });
    AllAmountChange();
}

function AllAmountChange() {
    $("#Totalprice").text(total_price.toLocaleString('en-US'));
    $("#Subtotal").text(total_price.toLocaleString('en-US'));
    total_price_end = total_price;
    $("#EndTotalprice").text(total_price_end.toLocaleString('en-US'));
    subtotal_price = total_price + $("#Freight").data('freight');
    $("#Freight").text($("#Freight").data('freight').toLocaleString('en-US'));
    $("#EndSubtotal").text(subtotal_price.toLocaleString('en-US'));
    $("#TotalSpend").text(subtotal_price.toLocaleString('en-US'));

    $(".subtotal").text = $.cookie('subtotal');
}

function MoveToFavorites() {
    Coker.sweet.confirm("確定將商品加入收藏？", "該商品將會加入收藏並從購物車中移除", "加入收藏", "取消", function () {
        $(this).parents("li").first().remove();
        ReloadAllAmount();
    });
}

function RemoveProduct() {
    Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
        $(this).parents("li").first().remove();
        ReloadAllAmount();
    });
}

function tabForward() {
    var target = event.target
    if (target.value.length == target.maxLength) {
        var elements = document.getElementById("OrdererForm").getElementsByTagName("input")
        for (let i = 0; i < elements.length; i++) {
            if (elements[i] == target) {
                if (elements[i + 1]) {
                    elements[i + 1].focus();
                }
                return;
            }
        }
        elements = document.getElementById("RecipientForm").getElementsByTagName("input")
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

function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}