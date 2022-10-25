function ShoppingCarModalInit() {
    $(".btn_count_plus").on('click', function () {
        $('.input_count').val(parseInt($('.input_count').val()) + 1);
    });

    $(".btn_count_minus").on('click', function () {
        if ($('.input_count').val() > 1) {
            $('.input_count').val(parseInt($('.input_count').val()) - 1);
        }
    });

    var $radio_btn = $('.options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_addToCar").on("click", function () {
        $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + 1);
        CarItemAdd();
    });
}