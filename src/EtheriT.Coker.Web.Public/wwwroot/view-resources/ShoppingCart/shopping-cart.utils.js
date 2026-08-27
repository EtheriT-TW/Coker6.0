// wwwroot/view-resources/ShoppingCart/shopping-cart.utils.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Utils = cart.Utils || {};

function toNumberValue(value) {
    if (value == null || value === "") return 0;
    return Number(String(value).replaceAll(",", "")) || 0;
}
function getValueIgnoreCase(data, key) {
    if (!data || !key) return undefined;

    if (Object.prototype.hasOwnProperty.call(data, key)) {
        return data[key];
    }

    var lowerCamelKey = key.charAt(0).toLowerCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(data, lowerCamelKey)) {
        return data[lowerCamelKey];
    }

    var pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(data, pascalKey)) {
        return data[pascalKey];
    }

    return undefined;
}
function ShoppingCartDataInsert(data, $self) {
    cart.Utils.ShoppingCartDataClear($self);

    data = data || {};

    $self.find("[data-key]").each(function () {
        var $this = $(this);
        var key = $this.data("key");

        if (typeof key === "undefined" || !key) return;

        var value = cart.Utils.getValueIgnoreCase(data, key);

        if (value == null) {
            value = "";
        }

        if ($this.hasClass("price")) {
            var num = cart.Utils.toNumberValue(value);
            $this.text(num > 0 ? num.toLocaleString() : "");
        } else {
            $this.text(value);
        }
    });
}
function ShoppingCartDataClear($self) {
    $self.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof ($this.data("key")) != "undefined") {
            $this.text("");
        }
    });
}
function TemplateDataInsert($Frame, $CollapseFrame, $Template, datas, options) {
    $Frame.empty();
    $CollapseFrame.empty();
    options = options || {};

    $.each(datas || [], function (index, data) {
        var $html = $($Template.html()).clone();

        $html.find("[data-key]").each(function () {
            var $this = $(this);
            var key = $this.data("key");

            if (typeof key === "undefined" || !key) return;

            switch (key) {
                case "link":
                    $this.attr({
                        href: `/${OrgName}/search/product/${getValueIgnoreCase(data, "prodId")}`,
                        title: `連結至：${getValueIgnoreCase(data, "title") || ""}(另開新視窗)`
                    });
                    break;

                case "imagePath": {
                    var imagePath = String(cart.Utils.getValueIgnoreCase(data, "imagePath") || "/images/noImg.jpg");
                    imagePath = imagePath.replace(`/${OrgName}/`, "/");
                    $this.attr({
                        src: imagePath,
                        alt: cart.Utils.getValueIgnoreCase(data, "title") || ""
                    });
                    break;
                }

                case "spec": {
                    var s1 = cart.Utils.getValueIgnoreCase(data, "s1Title") || "";
                    var s2 = cart.Utils.getValueIgnoreCase(data, "s2Title") || "";

                    $this.empty();
                    if (s1 !== "") $this.append(`<span class="border px-1 me-1">${s1}</span>`);
                    if (s2 !== "") $this.append(`<span class="border px-1 me-1">${s2}</span>`);
                    break;
                }

                case "price": {
                    var price = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(data, "price"));
                    $this.text(price > 0 ? cart.Utils.formatMoney(price) : "");
                    break;
                }

                case "bonus": {
                    var bonus = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(data, "bonus"));
                    if (bonus > 0) {
                        $this.text(`紅利：${bonus.toLocaleString()}`);
                        $this.removeClass("d-none");
                    } else {
                        $this.text("");
                        $this.addClass("d-none");
                    }
                    break;
                }

                case "subtotal": {
                    var subtotal = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(data, "subtotal"));
                    $this.text(subtotal > 0 ? cart.Utils.formatMoney(subtotal) : "");
                    break;
                }

                case "subtotalBonus": {
                    var subtotalBonus = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(data, "subtotalBonus"));
                    if (subtotalBonus > 0) {
                        $this.text(`紅利：${subtotalBonus.toLocaleString()}`);
                        $this.removeClass("d-none");
                    } else {
                        $this.text("");
                        $this.addClass("d-none");
                    }
                    break;
                }

                case "quantity": {
                    var qty = cart.Utils.toNumberValue(cart.Utils.getValueIgnoreCase(data, "quantity"));
                    $this.text(`${qty.toLocaleString()}`);
                    break;
                }

                default: {
                    var value = cart.Utils.getValueIgnoreCase(data, key);
                    if (value == null) value = "";

                    if ($this.hasClass("price")) {
                        var num = cart.Utils.toNumberValue(value);
                        $this.text(num > 0 ? num.toLocaleString() : "");
                    } else {
                        $this.text(value);
                    }
                    break;
                }
            }
        });

        if (typeof options.decorateItem === "function") {
            options.decorateItem($html, data, index);
        }

        var $target = index === 0 ? $Frame : $CollapseFrame;
        if (typeof options.beforeItem === "function") {
            var $before = options.beforeItem(data, index);
            if ($before && $before.length) $target.append($before);
        }

        if (index === 0) {
            $target.append($html);
        } else {
            $(".btn_view_list").removeClass("d-none");
            $target.append($html);
        }
    });
}

    Object.assign(cart.Utils, {
        toNumberValue: toNumberValue,
        getValueIgnoreCase: getValueIgnoreCase,
        ShoppingCartDataInsert: ShoppingCartDataInsert,
        ShoppingCartDataClear: ShoppingCartDataClear,
        TemplateDataInsert: TemplateDataInsert
    });
})(window.ShoppingCart, window.jQuery);
