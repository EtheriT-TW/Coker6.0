var $modal, $input_quantity
var modal_hass1 = false, modal_hass2 = false, modal_s1, modal_s2
var modal_price_list = []
var modal_s1_list = [], modal_s2_list = [], modal_price_list = []

function ShoppingCarModalInit() {
    ModalElementInit();

    const myModal = document.getElementById('ShoppingCarModal')

    myModal.addEventListener('shown.bs.modal', () => {
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

    $(".btn_addToCar").on("click", function () {
        if (!$(".btn_addToCar").hasClass("close")) AddToCart();
    });
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
    modal_hass1 = false;
    modal_hass2 = false;
    modal_s1 = null;
    modal_s2 = null;
    modal_s1_list = [];
    modal_s2_list = [];
    modal_price_list = [];
}

function ModalDefaultSet() {
    Product.GetOne.Prod($modal.data("pid")).done(function (result) {
        $pro_image.attr("src", "../upload/product/pro_0" + result.id + ".png");
        $pro_name.text(result.title);
        $pro_introduction.append("<div>．" + result.introduction.toString().replaceAll("\n", "<br />．") + "</div>")
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
                    if (!modal_s1 >= 0) {
                        modal_s1 = 0;
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
                    if (!modal_s2 >= 0) {
                        modal_s2 = 0;
                    }
                }
            })

            $options.prepend(item2);
            $options.prepend(item1);

            $radio = $(".Modal > .modal-content > .modal-body > .content > .options > .radio");
            $radio.each(function () {
                $input = $(this).children(".spec_control").children("input")
                $input.each(function () {
                    $(this).on("click", ModalSpecRadio)
                })
            })

            $pro_discount.text(result[0].price.toLocaleString('en-US') + " ~ " + result[result.length - 1].price.toLocaleString('en-US'));
        } else {
            modal_s1 = result[0].fK_S1id;
            modal_s2 = result[0].fK_S2id;
            $pro_discount.text(result[0].price.toLocaleString('en-US'));
        }
    })
}

function ModalSpecRadio() {
    $self = $(this);
    $self_p = $self.parents(".radio").first();
    $self_s = $self_p.siblings(".radio");

    $self_p.find("input").each(function () {
        $radio = $(this)
        $radio.removeAttr("disabled");
    })
    $self_s.find("input").each(function () {
        $radio = $(this)
        $radio.removeAttr("disabled");
    })

    switch ($self_p.data("stype")) {
        case 1:
            modal_s1 = $self.val()
            var temp_list = []
            modal_price_list.forEach(function (item) {
                if (item.s1id == modal_s1) {
                    temp_list.push(item.s2id)
                }
            })
            $self_s.find("input").attr("disabled", "disabled");
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1) {
                    $radio.removeAttr("disabled");
                }
            })
            if ($self_s.find("input[value='" + parseInt(modal_s2) + "']").attr("disabled") == "disabled") {
                modal_s2 = null;
            }
            break;
        case 2:
            modal_s2 = $self.val()
            var temp_list = []
            modal_price_list.forEach(function (item) {
                if (item.s2id == modal_s2) {
                    temp_list.push(item.s1id)
                }
            })
            $self_s.find("input").attr("disabled", "disabled");
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1) {
                    $radio.removeAttr("disabled");
                }
            })
            if ($self_s.find("input[value='" + parseInt(modal_s2) + "']").attr("disabled") == "disabled") {
                modal_s1 = null;
            }
            break;
    }

    if (modal_s1 != null && modal_s2 != null) {
        modal_price_list.forEach(function (item) {
            if (item.s1id == modal_s1 && item.s2id == modal_s2) {
                $pro_discount.text(item.price.toLocaleString('en-US'));
            }
        })
    }
}

function AddToCart() {

    if ($.cookie('cookie') == null) {
        Coker.sweet.error("錯誤", "若要進行商品選購，請先同意隱私權政策", null, false);
    } else {
        if (modal_s1 != null && modal_s2 != null) {
            Product.AddUp.Cart({
                FK_Tid: $.cookie("Token"),
                FK_Pid: $modal.data("pid"),
                FK_S1id: modal_s1,
                FK_S2id: modal_s2,
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
        } else {
            Coker.sweet.error("錯誤", "請確實選擇規格", null, false);
        }
    }
}