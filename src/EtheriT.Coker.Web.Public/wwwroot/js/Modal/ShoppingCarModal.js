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
    $pro_name = $content.find(".name");
    $pro_introduction = $content.find(".introduction");
    $pro_price = $content.find(".ori_price");
    $pro_discount = $content.find(".discount");
}

function DataClear() {
    $input_quantity.val(1);
    $pro_image.attr("src", "");
    $pro_name.text("");
    $pro_introduction.text("");
    $pro_price.addClass("d-none");
    $pro_price.text("");
    $pro_discount.text("");
}

function ModalDefaultSet() {
    Product.GetOne.Prod($modal.data("pid")).done(function (result) {
        $pro_image.attr("src", "../images/product/pro_0" + result.id + ".png");
        $pro_name.text(result.title);
        $pro_introduction.append("<div>．" + result.introduction.replaceAll("\n", "<br />．") + "</div>")
        if (result.discount > 0) {
            $pro_price.removeClass("d-none");
            $pro_price.append("<span class='text-decoration-line-through'>" + result.price.toLocaleString('en-US') + "</span>&ensp;折扣&ensp;");
            $pro_discount.text(result.discount.toLocaleString('en-US'));
        } else {
            $pro_discount.text(result.price.toLocaleString('en-US'));
        }
    });
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