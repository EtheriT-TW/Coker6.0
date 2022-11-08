function HeaderInit() {
    CarDropdownReset();

    const Cart_Dropdown = document.getElementById('Cart_Dropdown_Parent')
    Cart_Dropdown.addEventListener('shown.bs.dropdown', event => {
        $("#btn_car_dropdown > i").addClass("open");
    })
    Cart_Dropdown.addEventListener('hidden.bs.dropdown', event => {
        $("#btn_car_dropdown > i").removeClass("open");
    })

    $(".btn_cart_delete").on("click", CartDelete);

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
}

function CartDelete() {
    var $self = $(this);
    var $cart_pro = $self.parents("li").first();
    Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
        $cart_pro.remove();
        $.cookie('Purchased_Type_Quantity', 0, { path: '/' });
        $.cookie('Purchased_Item_Quantity', 0, { path: '/' });
        CarDropdownReset();
        Coker.sweet.success("成功移除商品", null, true);
    });
}

function CarDropdownReset() {
    if ($.cookie('Purchased_Type_Quantity') > 0) {
        var item = $($("#Template_Car_Dropdown").html()).clone();
        var item_link = item.find(".pro_link"),
            item_image = item.find(".pro_image"),
            item_name = item.find(".pro_name"),
            item_unit = item.find(".pro_unit"),
            item_quantity = item.find(".pro_quantity");

        item_link.attr("href", "/Toilet/01");
        item_image.attr("src", "../images/product/pro_pic_01.jpg");
        item_name.text("CS230 一段省水分離式幼兒馬桶");
        item_unit.text((9100).toLocaleString('en-US'));
        item_quantity.text($.cookie('Purchased_Item_Quantity'));

        var item_list_ul = $("#Car_Dropdown > ul");

        item_list_ul.append(item);

        $("#Car_Badge").text($.cookie('Purchased_Type_Quantity'));
        $("#Car_Dropdown_Null").addClass("d-none");
        $(".btn_car_buy").removeAttr("disabled");
    } else {
        $("#Car_Dropdown > ul > li").remove();
        $("#Car_Badge").text("");
        $("#Car_Dropdown_Null").removeClass("d-none");
        $(".btn_car_buy").attr("disabled", "");
    }
}

function CarItemChange() {
    $("#Car_Dropdown > ul li > figure > a > figcaption > .number > .pro_quantity").text($.cookie('Purchased_Item_Quantity'));
}