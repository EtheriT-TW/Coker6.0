var $modal, $input_quantity
var $purchase_status, $quantity_controls
var modal_hass1 = false, modal_hass2 = false, modal_s1, modal_s2
var modal_price_list = []
var modal_s1_list = [], modal_s2_list = [], modal_price_list = []
var modal_stock_list = [], modal_price_id = null
var modal_product_data = null
var modal_quantity_step = 1
var modal_default_image = ""
var quickCartOptions = null;
var shoppingCarModalInitialized = false;
var quickCartDefaultsApplied = false;

function IsQuickCartEditMode() {
    return quickCartOptions && quickCartOptions.mode === "edit-cart";
}

function ShoppingCarModalInit() {
    if (shoppingCarModalInitialized || !document.getElementById('ShoppingCarModal')) return;
    shoppingCarModalInitialized = true;
    ModalElementInit();

    const myModal = document.getElementById('ShoppingCarModal')

    myModal.addEventListener('shown.bs.modal', () => {
    })

    myModal.addEventListener('hidden.bs.modal', () => {
        DataClear();
        quickCartOptions = null;
    })

    $modal.find(".btn_count_plus").on('click', function () {
        var max = Number($input_quantity.attr("max"));
        var next = Number($input_quantity.val() || 0) + modal_quantity_step;
        if (!$input_quantity.prop("disabled") && (!Number.isFinite(max) || next <= max)) {
            $input_quantity.val(next);
        }
    });

    $modal.find(".btn_count_minus").on('click', function () {
        if ($input_quantity.val() > modal_quantity_step) {
            $input_quantity.val(parseInt($input_quantity.val()) - modal_quantity_step);
        }
    });

    var $radio_btn = $modal.find('.options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $modal.find(".btn_addToCar").on("click", function () {
        if (!$(this).hasClass("close")) AddToCart();
    });

    $modal.find(".modal-left").on("click", function (event) {
        if ($(event.target).closest("a, button").length) return;
        var productUrl = $(this).attr("data-product-url");
        if (productUrl) window.location.href = productUrl;
    }).on("keydown", function (event) {
        if (event.key !== "Enter" && event.key !== " ") return;
        event.preventDefault();
        var productUrl = $(this).attr("data-product-url");
        if (productUrl) window.location.href = productUrl;
    });
}

function ModalElementInit() {
    $modal = $("#ShoppingCarModal > .Modal");
    $input_quantity = $modal.find('.input_pro_quantity');
    $content = $modal.find(".modal-content > .modal-body > .content")
    $pro_image = $content.find(".pro_image");
    $pro_name = $content.find(".name");
    $pro_introduction = $content.find(".introduction");
    $pro_price = $content.find(".ori_price");
    $pro_discount = $content.find(".discount");
    $purchase_status = $content.find(".quick-cart-unavailable");
    $quantity_controls = $content.find(".btn_count_minus, .input_pro_quantity, .btn_count_plus");

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
    $purchase_status.addClass("d-none").text("");
    $quantity_controls.removeClass("d-none");
    $content.find(".tag").empty().addClass("d-none");
    $options.children(".radio").remove();
    modal_hass1 = false;
    modal_hass2 = false;
    modal_s1 = null;
    modal_s2 = null;
    modal_s1_list = [];
    modal_s2_list = [];
    modal_price_list = [];
    modal_stock_list = [];
    modal_price_id = null;
    modal_product_data = null;
    modal_quantity_step = 1;
    modal_default_image = "";
    quickCartDefaultsApplied = false;
    $input_quantity.prop("disabled", false).removeAttr("max").val(1);
    $modal.find(".btn_count_minus, .btn_count_plus").prop("disabled", false);
    $modal.find(".btn_addToCar").removeClass("close").prop("disabled", false);
    $modal.find(".btn_addToCar > div").text("加入購物車");
}

function ModalSetLoading(isLoading, message) {
    if (!$modal || !$modal.length) return;
    $modal.toggleClass("is-loading", isLoading === true)
        .attr("aria-busy", isLoading === true ? "true" : "false");
    if (message) $modal.find(".quick-cart-loading-text").text(message);
}

function ModalDefaultSet() {
    return Product.GetOne.ProdMainDisplay($modal.data("pid")).done(function (product) {
        if (!product) {
            $pro_discount.text("查無商品資料");
            $modal.find(".btn_addToCar").addClass("close");
            return;
        }
        $pro_name.text(product.title || $pro_name.text());
        var $tags = $content.find(".tag").empty();
        (Array.isArray(product.tagDatas) ? product.tagDatas : []).forEach(function (tag) {
            var name = tag.tag_Name || tag.tagName || "";
            if (name) $tags.append($("<li>").addClass("rounded-pill").text(name));
        });
        $tags.toggleClass("d-none", !$tags.children().length);

        modal_stock_list = Array.isArray(product.stocks) ? product.stocks : [];
        modal_product_data = product;
        if (!modal_stock_list.length) {
            ModalSetPriceDisplay(null, product);
            $input_quantity.val(0).attr("max", 0);
            ModalSetPurchaseAvailability(false, product.purchaseUnavailableReason || "已售完");
            return;
        }

        var initialStockId = IsQuickCartEditMode()
            ? Number(quickCartOptions.productStockId || 0)
            : 0;
        var first = modal_stock_list.find(function (stock) {
            return Number(stock.id || 0) === initialStockId;
        }) || modal_stock_list.find(function (stock) {
            return stock.canPurchase === true;
        }) || modal_stock_list[0];
        modal_s1 = Number(first.fK_S1id || 0);
        modal_s2 = Number(first.fK_S2id || 0);
        ModalBuildSpecGroup(1, modal_stock_list);
        ModalBuildSpecGroup(2, modal_stock_list);
        ModalRefreshSelection(product);
    }).fail(function () {
        $pro_discount.text("商品資料讀取失敗");
        $modal.find(".btn_addToCar").addClass("close");
    }).always(function () {
        ModalSetLoading(false);
    });
}

function ModalBuildSpecGroup(type, stocks) {
    var idKey = type === 1 ? "fK_S1id" : "fK_S2id";
    var optionKey = type === 1 ? "s1_Title" : "s2_Title";
    var groupKey = type === 1 ? "s1_Name" : "s2_Name";
    var values = [];
    stocks.forEach(function (stock) {
        var id = Number(stock[idKey] || 0);
        if (id > 0 && !values.some(function (value) { return value.id === id; })) {
            values.push({ id: id, name: stock[optionKey] || "未命名規格", title: stock[groupKey] || "規格" });
        }
    });
    if (!values.length) return;

    var $group = $($("#Modal_Template_Spec_Radio").html()).clone().attr("data-stype", type);
    $group.find(".spec_title").text(values[0].title);
    var $control = $group.find(".spec_control");
    values.forEach(function (value) {
        var inputId = "quick_cart_s" + type + "_" + value.id;
        var $input = $("<input>", { id: inputId, type: "radio", name: "QuickCartS" + type, value: value.id, class: "btn-check", autocomplete: "off" });
        var $label = $("<label>", { "for": inputId, class: "btn_radio me-2 my-1 px-3 py-1 align-self-center", text: value.name });
        $input.prop("checked", value.id === Number(type === 1 ? modal_s1 : modal_s2)).on("change", ModalSpecRadio);
        $control.append($input, $label);
    });

    $options.prepend($group);
}

function ModalFormatPrice(price) {
    var cash = Number(price && price.price || 0);
    var bonus = Number(price && price.bonus || 0);
    if (cash <= 0 && bonus > 0) return "紅利 " + bonus.toLocaleString("en-US") + " 點";
    var text = "NT$ " + cash.toLocaleString("en-US");
    return bonus > 0 ? text + " + 紅利 " + bonus.toLocaleString("en-US") + " 點" : text;
}

function ModalSetPriceDisplay(stock, product) {
    if (stock && stock.timePrice) {
        $pro_discount.text(product.priceDisplayText || "時價");
        return [];
    }

    var prices = stock && Array.isArray(stock.prices) ? stock.prices : [];
    if (prices.length) {
        $pro_discount.text(ModalFormatPrice(prices[0]));
        return prices;
    }

    if (product.priceDisplayText) {
        $pro_discount.text(product.priceDisplayText);
    } else if (product.price != null && product.price !== "") {
        $pro_discount.text("NT$ " + Number(product.price).toLocaleString("en-US"));
    } else {
        $pro_discount.text("目前無售價資訊");
    }
    return prices;
}

function ModalSetPurchaseAvailability(canPurchase, reason) {
    $quantity_controls.toggleClass("d-none", !canPurchase);
    $purchase_status
        .toggleClass("d-none", canPurchase)
        .text(canPurchase ? "" : (reason || "已售完"));
    $input_quantity.prop("disabled", !canPurchase);
    $modal.find(".btn_count_minus, .btn_count_plus").prop("disabled", !canPurchase);
    $modal.find(".btn_addToCar")
        .toggleClass("close", !canPurchase)
        .prop("disabled", !canPurchase);
}

function ModalRefreshImage(stock) {
    var media = stock && Array.isArray(stock.multimedia) ? stock.multimedia : [];
    var specImage = media.find(function (item) {
        return (Number(item.fileType) === 1 || Number(item.fileType) === 2) &&
            Array.isArray(item.link) && item.link[0];
    });
    $pro_image.attr("src", specImage ? specImage.link[0] : (modal_default_image || "/images/noImg.jpg"));
}

function ModalRefreshSelection(product) {
    var desiredQuantity = IsQuickCartEditMode()
        ? Number(quickCartDefaultsApplied
            ? $input_quantity.val()
            : quickCartOptions.quantity || 1)
        : 0;
    $options.children(".modal-price-options").remove();
    var stock = modal_stock_list.find(function (item) {
        return Number(item.fK_S1id || 0) === Number(modal_s1 || 0) && Number(item.fK_S2id || 0) === Number(modal_s2 || 0);
    });
    if (!stock) {
        modal_price_id = null;
        $pro_discount.text("請選擇完整規格");
        return;
    }

    modal_quantity_step = Math.max(Number(stock.min_Qty || 1), 1);
    $input_quantity.attr({ min: modal_quantity_step, step: modal_quantity_step }).val(modal_quantity_step);
    ModalRefreshImage(stock);

    if (stock.canPurchase !== true) {
        modal_price_id = null;
        ModalSetPriceDisplay(stock, product);
        $input_quantity.val(0).attr("max", 0);
        ModalSetPurchaseAvailability(false,
            stock.purchaseUnavailableReason || product.purchaseUnavailableReason || "已售完");
        return;
    }

    ModalSetPurchaseAvailability(true);
    if (stock.maxPurchaseQuantity == null) $input_quantity.removeAttr("max");
    else $input_quantity.attr("max", Number(stock.maxPurchaseQuantity));

    if (IsQuickCartEditMode()) {
        var normalizedQuantity = Math.max(
            modal_quantity_step,
            Math.ceil(Math.max(desiredQuantity, 1) / modal_quantity_step) * modal_quantity_step
        );
        if (stock.maxPurchaseQuantity != null) {
            normalizedQuantity = Math.min(normalizedQuantity, Number(stock.maxPurchaseQuantity));
        }
        $input_quantity.val(normalizedQuantity);
    }

    var prices = ModalSetPriceDisplay(stock, product);
    if (!prices.length) {
        modal_price_id = null;
        ModalSetPurchaseAvailability(false, stock.purchaseUnavailableReason || "目前無法購買");
        return;
    }

    var preferredPriceId = IsQuickCartEditMode() && !quickCartDefaultsApplied &&
        Number(stock.id || 0) === Number(quickCartOptions.productStockId || 0)
        ? Number(quickCartOptions.productPriceId || 0)
        : 0;
    var selectedPrice = prices.find(function (price) {
        return Number(price.id || 0) === preferredPriceId;
    }) || prices[0];

    modal_price_id = Number(selectedPrice.id || 0);
    $pro_discount.text(ModalFormatPrice(selectedPrice));
    $modal.find(".btn_addToCar").removeClass("close").prop("disabled", false);

    if (prices.length > 1) {
        var $group = $($("#Modal_Template_Spec_Radio").html()).clone().addClass("modal-price-options");
        $group.find(".spec_title").text("價格方案");
        var $control = $group.find(".spec_control");
        prices.forEach(function (price) {
            var inputId = "quick_cart_price_" + price.id;
            var $input = $("<input>", { id: inputId, type: "radio", name: "QuickCartPrice", value: price.id, class: "btn-check", autocomplete: "off" });
            var label = (price.roleName ? price.roleName + " " : "") + ModalFormatPrice(price);
            var $label = $("<label>", { "for": inputId, class: "btn_radio me-2 my-1 px-3 py-1 align-self-center", text: label });
            $input.prop("checked", Number(price.id || 0) === modal_price_id).on("change", function () {
                modal_price_id = Number(price.id || 0);
                $pro_discount.text(ModalFormatPrice(price));
            });
            $control.append($input, $label);
        });
        $group.insertBefore($options.find(".buy_line"));
    }

    quickCartDefaultsApplied = true;
}

function ModalSpecRadio() {
    var type = Number($(this).closest(".radio").attr("data-stype"));
    if (type === 1) modal_s1 = Number($(this).val());
    if (type === 2) modal_s2 = Number($(this).val());

    if (type === 1) {
        var validS2 = modal_stock_list.filter(function (stock) {
            return Number(stock.fK_S1id || 0) === modal_s1;
        });
        var currentValid = validS2.some(function (stock) { return Number(stock.fK_S2id || 0) === Number(modal_s2 || 0); });
        if (!currentValid && validS2.length) {
            modal_s2 = Number(validS2[0].fK_S2id || 0);
            $options.find('[data-stype="2"] input[value="' + modal_s2 + '"]').prop("checked", true);
        }
        $options.find('[data-stype="2"] input').each(function () {
            var id = Number($(this).val());
            $(this).prop("disabled", !validS2.some(function (stock) { return Number(stock.fK_S2id || 0) === id; }));
        });
    }
    ModalRefreshSelection(modal_product_data || {});
}

function AddToCart() {
    if (IsQuickCartEditMode()) {
        UpdateCartVariant();
        return;
    }

    if (!localStorage.getItem("AgreePrivacy")) {
        Coker.sweet.error("請注意", "若要進行商品選購，請先同意隱私權政策", null);
    } else {
        var selectedStock = modal_stock_list.find(function (stock) {
            return Number(stock.fK_S1id || 0) === Number(modal_s1 || 0)
                && Number(stock.fK_S2id || 0) === Number(modal_s2 || 0);
        });
        if (!modal_product_data || modal_product_data.canPurchase === false || !selectedStock || selectedStock.canPurchase !== true) {
            Coker.sweet.error("請注意", (selectedStock && selectedStock.purchaseUnavailableReason)
                || (modal_product_data && modal_product_data.purchaseUnavailableReason)
                || "此商品目前無法購買", null, false);
            return;
        }
        if (modal_s1 != null && modal_s2 != null) {
            Product.AddUp.Cart({
                FK_Pid: $modal.data("pid"),
                FK_PriceId: modal_price_id,
                FK_S1id: modal_s1,
                FK_S2id: modal_s2,
                Quantity: $input_quantity.val(),
            }).done(function (result) {
                if (result.success) {
                    Coker.sweet.success("商品已成功加入購物車", null, true);
                    var addedDetail = {
                        productId: Number($modal.data("pid")),
                        result: result
                    };
                    $(document).trigger("productQuickCart:added", [addedDetail]);
                    if (quickCartOptions && typeof quickCartOptions.onAdded === "function") {
                        quickCartOptions.onAdded(addedDetail);
                    }
                    bootstrap.Modal.getOrCreateInstance(document.getElementById("ShoppingCarModal")).hide();
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
                    if (result.error === "庫存不足") {
                        Coker.sweet.error(result.error, result.message, null, false);
                    } else {
                        Coker.sweet.error("商品加入購物車發生錯誤", result.message, null, true);
                    }
                }
            }).fail(function () {
                Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
            });
        } else {
            Coker.sweet.error("請注意", "請確實選擇規格", null, false);
        }
    }
}

function UpdateCartVariant() {
    var selectedStock = modal_stock_list.find(function (stock) {
        return Number(stock.fK_S1id || 0) === Number(modal_s1 || 0)
            && Number(stock.fK_S2id || 0) === Number(modal_s2 || 0);
    });

    if (!selectedStock || selectedStock.canPurchase !== true || !modal_price_id) {
        Coker.sweet.error(
            "無法修改商品規格",
            (selectedStock && selectedStock.purchaseUnavailableReason) || "請選擇可購買的規格與價格方案。",
            null,
            false
        );
        return;
    }

    var $saveButton = $modal.find(".btn_addToCar");
    $saveButton.prop("disabled", true);

    Product.Update.CartVariant({
        CartId: Number(quickCartOptions.cartId || 0),
        ProductStockId: Number(selectedStock.id || 0),
        ProductPriceId: Number(modal_price_id || 0),
        Quantity: Number($input_quantity.val() || 0)
    }).done(function (result) {
        if (!result || result.success !== true) {
            Coker.sweet.error(
                "無法修改商品規格",
                (result && result.message) || "請重新整理購物車後再試。",
                null,
                true
            );
            $saveButton.prop("disabled", false);
            return;
        }

        var detail = {
            cartId: Number(quickCartOptions.cartId || 0),
            productId: Number($modal.data("pid") || 0),
            result: result
        };

        $(document).trigger("productQuickCart:updated", [detail]);
        if (quickCartOptions && typeof quickCartOptions.onUpdated === "function") {
            quickCartOptions.onUpdated(detail);
        }

        bootstrap.Modal.getOrCreateInstance(document.getElementById("ShoppingCarModal")).hide();
    }).fail(function () {
        Coker.sweet.error("無法修改商品規格", "請稍後再試。", null, true);
        $saveButton.prop("disabled", false);
    });
}

window.ProductQuickCart = window.ProductQuickCart || {
    open: function (options) {
        options = typeof options === "object" ? options : { productId: options };
        var productId = Number(options.productId || 0);
        if (!productId || !document.getElementById("ShoppingCarModal")) return false;

        ShoppingCarModalInit();
        quickCartOptions = options;
        DataClear();
        ModalSetLoading(true, "正在讀取商品資料…");
        $modal.data("pid", productId);
        $pro_name.text(options.productName || "");
        modal_default_image = options.imageUrl || "/images/noImg.jpg";
        $pro_image.attr("src", modal_default_image);
        var orgPrefix = window.OrgName ? "/" + String(window.OrgName).replace(/^\/+|\/+$/g, "") : "";
        var productUrl = options.productUrl || orgPrefix + "/search/product/" + productId;
        $modal.find(".modal-product-link, .btn_learnMore").attr("href", productUrl);
        $modal.find(".modal-left").attr({ "data-product-url": productUrl, role: "link", tabindex: "0" });
        $modal.find(".btn_addToCar").removeClass("close");
        if (IsQuickCartEditMode()) {
            $modal.find(".btn_addToCar > div").text("確認修改");
        }
        ModalDefaultSet();

        var modalElement = document.getElementById("ShoppingCarModal");
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
        return true;
    }
};

$(function () {
    ShoppingCarModalInit();
    $(document).off("click.productQuickCart", "[data-quick-cart-product-id]")
        .on("click.productQuickCart", "[data-quick-cart-product-id]", function (event) {
            event.preventDefault();
            window.ProductQuickCart.open({
                productId: $(this).attr("data-quick-cart-product-id"),
                productName: $(this).attr("data-quick-cart-product-name") || "",
                imageUrl: $(this).attr("data-quick-cart-image-url") || ""
            });
        });
});
