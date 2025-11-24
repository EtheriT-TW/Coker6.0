var Product = {
    AddUp: {
        Cart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    Update: {
        Cart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/QuantityUpdate",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        MultiCart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/MultiQuantityUpdate",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    GetAll: {
        Cart: function (Tid) {
            return $.ajax({
                url: "/api/ShoppingCart/GetAll/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET",
                data: { Tid: Tid }
            });
        },
        History: function (page) {
            return $.ajax({
                url: "/api/Product/GetHistoryDisplay/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET",
                data: { page: page }
            });
        }
    },
    GetOne: {
        Cart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/GetDropOne/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET",
                data: { id: id }
            });
        },
        Prod: function (id) {
            return $.ajax({
                url: "/api/Product/GetDisplayOne/",
                type: "GET",
                data: { id: id }
            });
        },
        ProdMainDisplay: function (Id) {
            return $.ajax({
                url: "/api/Product/GetMainDisplayOne/",
                type: "GET",
                data: { Id: Id }
            });
        },
        ProdOne: function (id) {
            return $.ajax({
                url: "/api/Product/GetProdDataOne/",
                type: "GET",
                data: { id: id }
            });
        },
        Stock: function (id) {
            return $.ajax({
                url: "/api/Product/GetDisplayStock/",
                type: "GET",
                data: { id: id }
            });
        },
    },
    Delete: {
        Cart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/DeleteDrop/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET",
                data: { id: id }
            });
        }
    },
    Log: {
        Click: function (FK_Pid) {
            return $.ajax({
                url: "/api/Product/ClickLog",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { FK_Pid: FK_Pid },
            });
        }
    },
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
    var $template = $($("#Template_Car_Dropdown").html()).clone();
    if (!result.available) $template.addClass("unavailable");
    $template = HeaderDataInsert($template, result)
    $template.data("scid", result.scId);
    $template.find(".btn_cart_delete").on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            CartDropDelete($self, $self.data("scid"), "成功移除商品", "移除商品發生未知錯誤")
        });
    });

    $("#Car_Dropdown > ul").append($template);

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
function HeaderDataInsert($frame, data) {
    $frame.find("*").each(function () {
        var $self = $(this);
        if (typeof ($self.data("key")) != "undefined") {
            var key = $self.data("key");
            switch (key) {
                case "link":
                    if (data.available) {
                        $self.attr({
                            href: `/${OrgName}/home/product/${data['pId']}`,
                            title: `連結至：${data['title']}`
                        });
                    }
                    break;
                case "spec":
                    $self.append(data['s1Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s1Title']}</span>`)
                    $self.append(data['s2Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s2Title']}</span>`)
                    break;
                case "imagePath":
                    data[key] = data[key].replaceAll(`/${OrgName}/`, '/');
                    $self.attr({
                        src: data[key],
                        alt: `${data['title']}的圖片`
                    });
                    break;
                case "oldQuantity":
                    if (data.available) {
                        if (data[key] != data['quantity']) $self.removeClass("d-none");
                        $self.text(data[key]);
                    }
                    break;
                default:
                    $self.text(data[key]);
                    if (!data.available && (key == "price" || key == "quantity")) {
                        $self.text("");
                    }
                    break;
            }
            var type = $self.data("type");
            switch (type) {
                case "price":
                    if (data.available) {
                        if (data.bonus > 0) $self.text(`${parseInt($self.text()).toLocaleString()}+紅利${data.bonus}`)
                        else $self.text(parseInt($self.text()).toLocaleString())
                    }
                    break;
            };
        }
    });
    return $frame;
}