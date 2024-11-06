function HeaderInit() {
    if ($('body.home').length) {
        setTimeout(function () {
            if ((window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop) == 0) {
                // 頁面加載完畢後滾動到目標元素
                $('html,body').animate({
                    scrollTop: $('.one_swiper').offset().top  // 滾動到目標元素的頂部
                }, 'smooth');  // 'smooth' 也可以替換成毫秒值，例如 1000 毫秒
            }
        }, 500);
    }

    var mega_menu_height = $("#Mega_Menu").css("height");
    $("body").css("padding-top", mega_menu_height);

    $(window).resize(function () {
        var mega_menu_height = $("nav").css("height");
        $("body").css("padding-top", mega_menu_height);
        MenuLiSize();
    });

    if ($("#Cart_Dropdown_Parent").length > 0) {
        CartDropInit();
    }
    MenuLiSize();

    const Cart_Dropdown = document.getElementById('Cart_Dropdown_Parent')
    if (Cart_Dropdown != null) {
        Cart_Dropdown.addEventListener('shown.bs.dropdown', event => {
            $("#btn_car_dropdown > i").addClass("open");
        })
        Cart_Dropdown.addEventListener('hidden.bs.dropdown', event => {
            $("#btn_car_dropdown > i").removeClass("open");
        })
    }

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

    /*
    $("#Offcanvas_Mega_Menu > ul > .title > .content > ul").each(function () {
        var $self = $(this);
        if ($self.children("li").length > 4) {
            $self.css("justify-content", "start")
        } else {
            $self.css("justify-content", "space-evenly")
        }
    });
    $("#Offcanvas_Mega_Menu > ul > .metaMenu > .content > ul > .subtitle ul").each(function () {
        var $self = $(this);
        if ($self.children("li").length >= 6) {
            var width = 100 / 6 * 2;
            var str_width = `${width}%`
            $self.parents(".subtitle").first().css("width", str_width);
            if ($self.children("li").length > 10) {
                var $parent = $self.prev("a").first();
                $self.children("li:eq(9)").html(`<a class="ps-2 nav-link text-black py-1 text-nowrap text-start fw-normal" href="${$parent.attr('href')}" title="${$parent.attr('title')}" target="${$parent.attr('target')}">更多...</a>`);
                $self.children("li:gt(9)").addClass("d-none")
            }
        }
        else {
            var width = 100 / 6;
            var str_width = `${width}%`
            $self.parents(".subtitle").first().css("width", str_width);
        }
    });*/
}

function MenuLiSize() {
    if ($(window).width() > 768) {
        $(".subtitle").removeClass("w-100")
        $(".subtitle li").removeClass("w-100")
    } else {
        $(".subtitle").addClass("w-100")
        $(".subtitle li").addClass("w-100")
    }
    if ($(window).width() > 992) {
        $(".offcanvas-body").removeClass("accordion")
        $(".offcanvas-body .collapse").addClass("show")
        $(`.offcanvas-body  .nav-link[data-bs-toggle]`).attr("data-bs-toggle", "");
    } else {
        $(".offcanvas-body").addClass("accordion")
        $(".offcanvas-body .collapse").removeClass("show")
        $(`.offcanvas-body  .nav-link[data-bs-toggle]`).each((i, item) => {
            if ($(item).next("ul").length > 0)
                $(item).attr("data-bs-toggle", "collapse");
            else
                $(item).find(".material-symbols-outlined").remove();
        });
    }
    $("#Offcanvas_Mega_Menu a").each(function () {
        const $item = $(this);
        if ($item.attr("href").match(PageKey) != null)
            $item.parents(".collapse").addClass("show");
    });
}

function CartDropInit() {
    Product.GetAll.Cart().done(function (result) {
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
        item_spec = item.find(".pro_spec"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_btn_delete = item.find(".btn_cart_delete");

    item.data("scid", result.scId);
    item_link.attr("href", `/${OrgName}/home/product/${result.pId}`);
    item_image.attr("src", result.imagePath);
    item_name.text(result.title);
    item_spec.append(result.s1Title == "" ? "" : `<span class="border px-1 me-1">${result.s1Title}</span>`)
    item_spec.append(result.s2Title == "" ? "" : `<span class="border px-1">${result.s2Title}</span>`)
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
        $("#Car_Dropdown > .btn_car_buy").removeAttr("disabled");
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
    $("#Car_Dropdown > .btn_car_buy").attr("disabled", "");
}