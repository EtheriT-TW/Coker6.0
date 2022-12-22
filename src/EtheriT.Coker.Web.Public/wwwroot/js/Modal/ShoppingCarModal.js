var $modal, $input_quantity
var modal_hass1 = false, modal_hass2 = false, s1, s2
var modal_price_list = []
var modal_s1_list = [], modal_s2_list = [], price_list = []

function ShoppingCarModalInit() {
    ModalElementInit();

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

function ModalElementInit() {
    $modal = $(".Modal");
    $input_quantity = $('.input_pro_quantity');
    $content = $(".Modal > .modal-content > .modal-body > .content")
    $pro_image = $content.find(".pro_image");
    $pro_name = $content.find(".name");
    $pro_introduction = $content.find(".introduction");
    $pro_price = $content.find(".ori_price");
    $pro_discount = $content.find(".discount");

    $options = $content.find(".options");
}

function DataClear() {
    $input_quantity.val(1);
    $pro_image.attr("src", "");
    $pro_name.text("");
    $pro_introduction.text("");
    $pro_price.addClass("d-none");
    $pro_price.text("");
    $pro_discount.text("");
    $options.children(".radio").remove();
}

function ModalDefaultSet() {
    Product.GetOne.Prod($modal.data("pid")).done(function (result) {
        console.log(result)
        $pro_image.attr("src", "../images/product/pro_0" + result.id + ".png");
        $pro_name.text(result.title);
        $pro_introduction.append("<div>．" + result.introduction.toString().replaceAll("\n", "<br />．") + "</div>")
        //if (result.discount > 0) {
        //    $pro_price.removeclass("d-none");
        //    $pro_price.append("<span class='text-decoration-line-through'>" + result.price.toLocaleString('en-us') + "</span>&ensp;折扣&ensp;");
        //    $pro_discount.text(result.discount.toLocaleString('en-us'));
        //} else {
        //    $pro_discount.text(result.price.toLocaleString('en-us'));
        //}
    });

    Product.GetOne.Stock($modal.data("pid")).done(function (result) {
        if (result.length > 1) {

            var obj = {};

            var item1 = $($("#Modal_Template_Spec_Radio").html()).clone(), item2 = $($("#Modal_Template_Spec_Radio").html()).clone();
            var item1_control = item1.find(".spec_control"),
                item1_title = item1.find(".spec_title"),
                item2_control = item2.find(".spec_control"),
                item2_title = item2.find(".spec_title");

            item1.data("stype", 1)
            item2.data("stype", 2)
            result.forEach(function (spec) {
                obj["s1id"] = spec.fK_S1id;
                obj["s2id"] = spec.fK_S2id;
                obj["price"] = spec.price;
                modal_price_list.push(obj);
                obj = {}

                if (spec.fK_S1id > 0) {
                    if (!modal_hass1) {
                        item1_title.text(spec.s1_Title);
                        modal_hass1 = true;
                    }
                    if (modal_s1_list.indexOf(spec.fK_S1id) < 0) {
                        item1_control.append('<input id="s1_' + spec.fK_S1id + '" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="' + spec.fK_S1id + '">');
                        item1_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s1_' + spec.fK_S1id + '">' + spec.s1_Name + '</label>');
                        modal_s1_list.push(spec.fK_S1id);
                    }
                } else {
                    if (!s1 >= 0) {
                        s1 = 0;
                    }
                }

                if (spec.fK_S2id > 0) {
                    if (!modal_hass2) {
                        item2_title.text(spec.s2_Title);
                        modal_hass2 = true;
                    }
                    if (modal_s2_list.indexOf(spec.fK_S2id) < 0) {
                        item2_control.append('<input id="s2_' + spec.fK_S2id + '" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="' + spec.fK_S2id + '">');
                        item2_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s2_' + spec.fK_S2id + '">' + spec.s2_Name + '</label>');
                        modal_s2_list.push(spec.fK_S2id);
                    }
                } else {
                    if (!s2 >= 0) {
                        s2 = 0;
                    }
                }
            })

            $options.prepend(item2);
            $options.prepend(item1);

            $radio = $("#Product > .content > .options > .radio");
            $radio.each(function () {
                $input = $(this).children(".spec_control").children("input")
                $input.each(function () {
                    $(this).on("click", SpecRadio)
                })
            })

            $pro_discount.text(result[0].price.toLocaleString('en-US') + " ~ " + result[result.length - 1].price.toLocaleString('en-US'));
        } else {
            s1 = result[0].fK_S1id;
            s2 = result[0].fK_S2id;
            $pro_discount.text(result[0].price.toLocaleString('en-US'));
        }
    })
}

function AddToCart() {
    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        Coker.sweet.error("錯誤", "若要進行商品選購，請先同意隱私權政策", null, false);
    } else {
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