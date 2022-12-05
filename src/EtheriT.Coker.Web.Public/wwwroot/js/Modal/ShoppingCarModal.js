var $modal, $input_quantity

function ShoppingCarModalInit() {
    ElementInit();

    const myModal = document.getElementById('ShoppingCarModal')

    myModal.addEventListener('shown.bs.modal', () => {
        console.log($modal.data("pid"))
    })

    myModal.addEventListener('hidden.bs.modal', () => {
        DataClear();
    })

    $(".btn_count_plus").on('click', function () {
        $input_quantity.val(parseInt($input_quantity.val()) + 1);
    });

    $(".btn_count_minus").on('click', function () {
        if ($input_quantity.val() > 1) {
            $input_quantity.val(parseInt($input_quantity.val()) - 1);
        }
    });

    var $radio_btn = $('.options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_addToCar").on("click", AddToCart);
}

function ElementInit() {
    $modal = $(".Modal");
    $input_quantity = $('.input_pro_quantity');
}

function DataClear() {
    $input_quantity.val(1);
}

function AddToCart() {
    Product.AddUp.Cart({
        FK_Tid: $.cookie("Token"),
        FK_Pid: $modal.data("pid"),
        FK_S1id: 1,
        FK_S2id: 4,
        Quantity: $input_quantity.val(),
        Discont: 0,
        Bonus: 0,
        PriceType: 0,
        IsAdditional: false,
        Ser_No: 500,
    }).done(function (result) {
        Coker.sweet.success("商品已成功加入購物車", null, true);
        var type = (result.message).substr(0, 1);
        var id = (result.message).substr(1);
        Product.GetOne.Cart(id).done(function (result) {
            if (type == 'N') {
                CartDropAdd(result);
            } else {
                CartDropUpdate(result);
            }
        });
    }).fail(function () {
        Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
    });
}