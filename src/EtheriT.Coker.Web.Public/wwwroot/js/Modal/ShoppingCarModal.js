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
    $content = $(".Modal > .modal-content > .modal-body > .content")
    $pro_image = $content.find(".pro_image");

}

function DataClear() {
    $input_quantity.val(1);
    $pro_image.attr("src", "");
}

function ModalDefaultSet() {

    Product.GetOne.Prod($modal.data("pid")).done(function (result) {
        console.log(result)
    });

    $pro_image.attr("src", "../images/product/pro_0" + $modal.data("pid") + ".png");

    /*
    item.data("scid", result.scId);
    item_link.attr("href", "/Toilet/" + result.pId);
    item_image.attr("src", "../images/product/pro_0" + result.pId + ".png");
    item_name.text(result.title);
    item_specification.text("白色");
    item_instructions.text(result.description);
    item_unit.text((result.price).toLocaleString('en-US'))
    item_quantity.val(result.quantity);
    item_total.data("subtotal", result.price * result.quantity)
    item_total.text(item_total.data("subtotal").toLocaleString('en-US'))
     */
}

function AddToCart() {
    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        Coker.sweet.error("錯誤", "若要進行商品選購，請先同意隱私權政策", null, false);
    } else {
        Product.AddUp.Cart({
            FK_Tid: $.cookie("Token"),
            FK_PSid: $modal.data("pid"),
            FK_S1id: 1,
            FK_S2id: 4,
            Quantity: $input_quantity.val(),
            Discont: 0,
            Bonus: 0,
            PriceType: 0,
            IsAdditional: false,
            Ser_No: 500,
        }).done(function (result) {
            if (result.success) {
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
            } else {
                Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
            }
        }).fail(function () {
            Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
        });
    }
}