function ShoppingCarModalInit() {
    $(".btn_count_plus").on('click', function () {
        $('.input_pro_quantity').val(parseInt($('.input_pro_quantity').val()) + 1);
    });

    $(".btn_count_minus").on('click', function () {
        if ($('.input_pro_quantity').val() > 1) {
            $('.input_pro_quantity').val(parseInt($('.input_pro_quantity').val()) - 1);
        }
    });

    var $radio_btn = $('.options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_addToCar").on("click", AddToCar);
}

function AddToCar() {
    $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + parseInt($('.input_pro_quantity').val()), { path: '/' });
    Coker.sweet.success("成功加入購物車！", null, true);
    if ($.cookie('Purchased_Type_Quantity') > 0) {
        CarItemChange();
    } else {
        $.cookie('Purchased_Type_Quantity', 1, { path: '/' });
        CarDropdownReset();
    }
}