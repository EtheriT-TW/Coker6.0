var total_price = 0;
var total_price_end = 0;
var subtotal_price = 0;

function PageReady() {

    ReloadAllAmount();

    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    $(".btn_move_to_favorites").on("click", function () {
        $(this).parents("li").first().remove();
        ReloadAllAmount();
    });
    $(".btn_remove_pro").on("click", function () {
        $(this).parents("li").first().remove();
        ReloadAllAmount();
    });
    $(".btn_count_plus").on("click", AmountPlus);
    $(".btn_count_minus").on("click", AmountMinus);
    $(".btn_edit_data").on("click", function () {
        $(this).parents(".orderer").children("form").children("div").children("div").toggleClass("show");
    });


    var buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
        slidesPerView: 1,
        spaceBetween: 15,
        keyboard: false,
        autoHeight: true,
        loop: false,
        simulateTouch: false,
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

    $(".btn_gofirst").on("click", function () {
        buy_step_swiper.slideTo(0);
    });

    $(".btn_goprev").on("click", function () {
        buy_step_swiper.slidePrev();
    });
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
    total_price = 0;
    total_price_end = 0;
    subtotal_price = 0;

    $(".purchase_list > li").each(function () {
        var $self_unit = $(this).children(".content").children(".unit");
        console.log($self_unit);
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
}

function RemoveProduct() {
    $(this).parents("li").first().remove();
}