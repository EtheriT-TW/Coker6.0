function HeaderInit() {

    if ($.cookie("Token") != null) {
        CartDropInit();
    }
    MenuLiSize();

    $(window).resize(function () {
        MenuLiSize();
    });

    const Cart_Dropdown = document.getElementById('Cart_Dropdown_Parent')
    Cart_Dropdown.addEventListener('shown.bs.dropdown', event => {
        $("#btn_car_dropdown > i").addClass("open");
    })
    Cart_Dropdown.addEventListener('hidden.bs.dropdown', event => {
        $("#btn_car_dropdown > i").removeClass("open");
    })

    var $myOffcanvas = $("#Mega_Menu>.offcanvas");
    $myOffcanvas.on('hidden.bs.offcanvas', function () {
        $("#menuButton").addClass("collapsed");
    });

    $myOffcanvas.on('shown.bs.offcanvas', function () {
        $("#menuButton").removeClass("collapsed");
    });

    $('#News_Marquee > .news_box').verticalLoop({
        delay: 3000,
        order: 'asc'
    });

    var $menu_content = $("#Offcanvas_Mega_Menu > ul > .title > .content > ul");

    $menu_content.each(function () {
        if ($(this).children('li').length < 4) {
            $(this).css("align-content", "center");
        } else {
            $(this).css("align-content", "start");
        }
    });
}

function MenuLiSize() {
    $(".sub_content").each(function () {
        var $self = $(this);
        if ($(window).width() > 768) {
            var content_width = $self.parents(".content").first().width();
            if ($self.children("li").length > 0) {
                if ($self.children("li").length > 7) {
                    var selfwidth = content_width / 6 * 2
                    $self.css("width", selfwidth)
                }
                $self.css("height", "100%")
                $self.parents("li").first().css("height", "100%");
                $self.parents("li").first().css("padding-bottom", "3rem");
            } else {
                $self.css("height", "fit-content")
                $self.parents("li").first().css("height", "fit-content");
            }

        } else {
            $self.css("width", "unset")
            $self.parents("li").first().css("padding-bottom", "0");
        }
    });
}

function CartDropInit() {
    Product.GetAll.Cart($.cookie("Token")).done(function (result) {
        if (result.length > 0) {
            for (var i = 0; i < result.length; i++) {
                CartDropAdd(result[i])
            }
        }
    })
}

function CartDropAdd(result) {
    var item = $($("#Template_Car_Dropdown").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_btn_delete = item.find(".btn_cart_delete");

    item.data("scid", result.scId);
    item_link.attr("href", "/Toilet/" + result.pId);
    item_image.attr("src", "../images/product/pro_0" + result.pId + ".png");
    item_name.text(result.title);
    item_unit.text((result.price + "").toLocaleString('en-US'));
    item_quantity.text(result.quantity);
    item_btn_delete.on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            CartDropDelete($self, $self.data("scid"), "成功移除商品", "移除商品發生未知錯誤")
        });
    });

    var item_list_ul = $("#Car_Dropdown > ul");

    item_list_ul.append(item);

    var car_num = $("#Car_Badge").text() == "" ? 1 : parseInt($("#Car_Badge").text()) + 1;
    $("#Car_Badge").text(car_num.toString());

    if (!$("#Car_Dropdown_Null").hasClass("d-none")) {
        $("#Car_Dropdown_Null").addClass("d-none");
        $(".btn_car_buy").removeAttr("disabled");
    }
}

function CartDropUpdate(result) {
    var $car_drop_li = $("#Car_Dropdown > ul > li");
    $car_drop_li.each(function () {
        var $self = $(this)
        if ($self.data("scid") == result.scId) {
            $self.find(".pro_quantity").text(result.quantity)
        }
    });
}

function CartDropReset(scid, quantity) {
    $("#Car_Dropdown > ul > li").each(function () {
        if ($(this).data("scid") == scid) {
            if (quantity == 0) {
                $(this).remove();
                $("#Car_Badge").text($("#Car_Badge").text() - 1)
            } else {
                $(this).find(".pro_quantity").text(quantity)
            }
        }
    });
}

function CartDropDelete(self, id, success, error) {
    self.remove();
    Product.Delete.Cart(id).done(function () {
        Coker.sweet.success(success, null, true);
        var car_num = parseInt($("#Car_Badge").text()) - 1;
        $("#Car_Badge").text(car_num.toString());
        if (parseInt($("#Car_Badge").text()) == 0) {
            CartClear();
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error, null, true);
    })
}

function CartClear() {
    $("#Car_Dropdown > ul > li").remove();
    $("#Car_Badge").text("");
    $("#Car_Dropdown_Null").removeClass("d-none");
    $(".btn_car_buy").attr("disabled", "");
}