function HeaderInit() {
    CarDropdownReset();

    $(".btn_cart_delete").on("click", CartDelete);

    var $myOffcanvas = $("#Mega_Menu>.offcanvas");
    $myOffcanvas.on('hidden.bs.offcanvas', function () {
        $("#menuButton").addClass("collapsed");
    });

    $myOffcanvas.on('shown.bs.offcanvas', function () {
        console.log($("#menuButton"));
        $("#menuButton").removeClass("collapsed");
    });

    $('.news_box').verticalLoop({
        delay: 3000,
        order: 'asc'
    });

    $("#btn_car_dropdown").on("click", function () {
        $("#btn_car_dropdown > i").toggleClass("open");
    })
}

function CartDelete() {
    var $self = $(this);
    var $cart_pro = $self.parents("li").first();
    Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
        $cart_pro.remove();
        $.cookie('Purchased_Type_Quantity', 0, { path: '/' });
        $.cookie('Purchased_Item_Quantity', 0, { path: '/' });
        CarDropdownReset();
        $("#btn_car_dropdown > i").removeClass("open");
        Coker.sweet.success("成功移除商品", null, true);
    });
}

function CarDropdownReset() {
    if ($.cookie('Purchased_Type_Quantity') > 0) {
        if ('content' in document.createElement('template')) {
            var item = document.querySelector("#Template_Car_Dropdown");
            var item_link = item.content.querySelector(".pro_link"),
                item_image = item.content.querySelector(".pro_image"),
                item_name = item.content.querySelector(".pro_name"),
                item_unit = item.content.querySelector(".pro_unit"),
                item_quantity = item.content.querySelector(".pro_quantity");

            item_link.href = "/Toilet/01";
            item_image.setAttribute("src", "../images/product/pro_pic_01.jpg");
            item_name.textContent = "CS230 一段省水分離式幼兒馬桶";
            item_unit.textContent = (9100).toLocaleString('en-US');
            item_quantity.textContent = $.cookie('Purchased_Item_Quantity');

            var item_list_ul = document.querySelector("#Car_Dropdown > ul");
            var clone = document.importNode(item.content, true);
            item_list_ul.appendChild(clone);
        }
        $("#Car_Badge").text($.cookie('Purchased_Type_Quantity'));
        $("#Car_Dropdown_Null").addClass("d-none");
        $(".btn_car_buy").removeAttr("disabled");
    } else {
        $("#Car_Badge").text("");
        $("#Car_Dropdown_Null").removeClass("d-none");
        $(".btn_car_buy").attr("disabled", "");
    }
}

function CarItemAdd() {
    console.log($("#Car_Dropdown > ul li > figure > a > figcaption > .number > .pro_quantity"))
    $("#Car_Dropdown > ul li > figure > a > figcaption > .number > .pro_quantity").text($.cookie('Purchased_Item_Quantity'));
}