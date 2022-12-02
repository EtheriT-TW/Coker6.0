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

    $(".btn_addToCar").on("click", AddToCart);
}

//function AddToCart() {
//    $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + parseInt($('.input_pro_quantity').val()), { path: '/' });
//    Coker.sweet.success("成功加入購物車！", null, true);
//    if ($.cookie('Purchased_Type_Quantity') > 0) {
//        CarItemChange();
//    } else {
//        $.cookie('Purchased_Type_Quantity', 1, { path: '/' });
//        CarDropdownReset();
//    }
//}

function AddToCart() {
    Product.Add.Cart({
        FK_Tid: $.cookie("Token"),
        FK_Pid: 1,
        FK_S1id: 1,
        FK_S2id: 4,
        Quantity: 1,
        Discont: 0,
        Bonus: 0,
        PriceType: 0,
        IsAdditional: false,
        Ser_No: 500,
    }).done(function () {
        Coker.sweet.success("商品已成功加入購物車", null, true);
    }).fail(function () {
        Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
    });
}