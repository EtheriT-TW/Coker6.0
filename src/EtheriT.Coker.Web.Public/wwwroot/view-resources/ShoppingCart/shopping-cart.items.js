// wwwroot/view-resources/ShoppingCart/shopping-cart.items.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Items = cart.Items || {};

function syncHeaderCheckbox($group) {
    const $checks = $group.find('input[name="buyItems"]');
    const $validChecks = $checks.filter(':enabled');

    const total = $validChecks.length;
    const selected = $validChecks.filter(':checked').length;
    const $header = $group.find('.js-group-check');

    $header.prop('indeterminate', false);

    if (total === 0) {
        $header.prop({
            checked: false,
            indeterminate: false,
            disabled: true
        });
    } else if (selected === 0) {
        $header.prop({
            checked: false,
            indeterminate: false,
            disabled: false
        });
    } else if (selected === total) {
        $header.prop({
            checked: true,
            indeterminate: false,
            disabled: false
        });
    } else {
        $header.prop({
            checked: false,
            indeterminate: true,
            disabled: false
        });
    }

    $group.find('.js-selected-count').text(selected);
}
function refreshAllCartGroupSubtotal() {
    $('.purchase_group').each(function () {
        updateGroupSelectedSubtotal($(this));
    });

    cart.Pricing.TotalCount();
    cart.Pricing.updateNextStepByBonus();

    if (typeof S.buy_step_swiper !== "undefined" && S.buy_step_swiper) {
        S.buy_step_swiper.update();
    }
}
function updateGroupSelectedSubtotal($group) {
    let sum = 0;
    $group.find('li.purchase_item').each(function () {
        const $li = $(this);
        if ($li.find('input[name="buyItems"]').is(':checked')) {
            sum += Number($li.find('[data-key="subtotal"]').data('subtotal') || 0);
        }
    });
    $group.find('.js-group-subtotal').attr('data-subtotal', sum).text(`$${sum.toLocaleString()}`);
    cart.Items.syncHeaderCheckbox($group);
}
function syncAdditionalSelection($group) {
    const hasSelectedPrimary = $group
        .find('li.purchase_item:not(.cart-additional-item) input[name="buyItems"]:enabled:checked')
        .length > 0;

    $group.find('li.cart-additional-item input[name="buyItems"]:enabled')
        .prop('checked', hasSelectedPrimary);
    $group.toggleClass('has-selected-primary', hasSelectedPrimary);
}
function clearOtherGroupsExcept($group) {
    $('.purchase_group').not($group).each(function () {
        const $g = $(this);
        $g.find('.js-group-check').prop({ checked: false, indeterminate: false });
        $g.find('input[name="buyItems"]').prop('checked', false);
        $g.find('.js-group-subtotal').attr('data-subtotal', 0).text('$0');
        $g.find('.js-selected-count').text(0);
    });
}
function CardDataGet() {
    Product.GetAll.Cart().done(function (result) {
        if (result.length > 0) {
            cart.Items.CartInit(result)
        }
    });
}
function ReloadCartDisplay() {
    var deferred = $.Deferred();

    Product.GetAll.Cart().done(function (result) {
        var items = Array.isArray(result) ? result : [];
        S.shopping_cart_data = [];

        if (items.length > 0) {
            cart.Items.CartInit(items);
        } else {
            cart.Items.renderCartGroups([]);
            cart.Items.refreshHasProds();
            if (cart.Forms && typeof cart.Forms.DetailsClear === "function") {
                cart.Forms.DetailsClear();
            }
        }

        deferred.resolve(items);
    }).fail(function (error) {
        deferred.reject(error);
    });

    return deferred.promise();
}
function ReloadCartDropdown(items) {
    if (typeof window.CartDropAdd !== "function") return;

    $("#Car_Dropdown > ul").empty();
    $("#Car_Badge").text("");

    (items || []).forEach(function (item) {
        window.CartDropAdd(item);
    });

    if (!items || items.length === 0) {
        $("#Car_Dropdown_Null").removeClass("d-none");
        $("#Car_Dropdown > .btn_car_buy").attr("disabled", "");
    }
}
function groupByFreight(list) {
    return list.reduce((acc, it) => {
        const key = (it.freight && it.freight.id) ? it.freight.id : 0;
        (acc[key] ||= []).push(it);
        return acc;
    }, {});
}
function createGroupHeader(meta) {
    const $tpl = $($('#Template_Cart_GroupHeader').html().trim());
    $tpl.attr('data-group-id', meta.id);
    $tpl.find('[data-field="title"]').text(meta.title || '一般配送');
    if (meta.describe) $tpl.find('[data-field="describe"]').text(meta.describe).show();
    else $tpl.find('[data-field="describe"]').hide();
    $tpl.find('[data-field="count"]').text(`${meta.count} 件`);
    $tpl.find('.btn-checkout-group').attr('data-group-id', meta.id);
    return $tpl;
}
function renderCartGroups(result) {
    const $ul = $("#Step1 > .card-body > .purchase_list");
    // 只移除舊的群組，不要 .empty() 以免不小心砍到 template
    $ul.children("li.purchase_group").remove();

    // 分群
    const groups = cart.Items.groupByFreight(result);
    const orderedKeys = Object.keys(groups).sort((a, b) => groups[b].length - groups[a].length);

    orderedKeys.forEach(key => {
        const items = groups[key];
        const meta = {
            id: Number(key),
            title: items[0].freight?.title || '一般配送',
            describe: items[0].freight?.describe || '',
            count: items.length
        };

        // 建群組容器
        const $group = $($('#Template_Cart_Group').html().trim());
        $group.attr('data-group-id', meta.id);
        $group.find('[data-field="title"]').text(meta.title);
        $group.find('[data-field="count"]').text(`${meta.count} 件`);
        if (meta.describe) $group.find('[data-field="describe"]').text(meta.describe);
        else $group.find('[data-field="describe"]').remove();

        const $groupItems = $group.find('.group_items');

        // 塞入該組 items（沿用你原有 CartListAdd，但改讓它可以指定容器）
        // API 已依活動來源完成主從排序，不可再把所有優惠商品集中移到最後。
        const orderedItems = items.slice();
        const appendProductAdditionalSection = function () {
            $groupItems.append(
                '<li class="cart-additional-section-label js-cart-addon-group-action d-none" ' +
                'data-group-id="' + meta.id + '">' +

                '<span>' +
                '<i class="fa-solid fa-turn-down me-2"></i>' +
                '加價購／贈品' +
                '</span>' +

                '<span class="cart-additional-section-actions">' +
                '<small>會隨本組主要商品一併結帳</small>' +

                '<button type="button" ' +
                'class="btn btn-sm btn-outline-danger js-open-cart-addons">' +
                '<i class="fa-solid fa-plus me-1"></i>' +
                '補選／調整' +
                '</button>' +
                '</span>' +
                '</li>'
            );
        };

        const appendOrderAdditionalSection = function () {
            $groupItems.append(
                '<li class="cart-order-additional-section-label">' +
                '<span>' +
                '<strong>訂單加價購／贈品</strong>' +
                '<small>本筆購物車符合優惠活動條件</small>' +
                '</span>' +
                '</li>'
            );
        };

        let previousAdditionalType = null;

        orderedItems.forEach(row => {
            var isAdditional = row.isAdditional === true;

            if (!isAdditional) {
                previousAdditionalType = null;
            }
            else {
                var parentLabel = String(
                    cart.Utils.getValueIgnoreCase(row, "additionalParentLabel") || ""
                ).trim();

                var additionalType =
                    parentLabel === "本筆訂單"
                        ? "order"
                        : "product";

                if (additionalType !== previousAdditionalType) {
                    if (additionalType === "order") {
                        appendOrderAdditionalSection();
                    }
                    else {
                        appendProductAdditionalSection();
                    }
                }

                previousAdditionalType = additionalType;
            }

            var originalPrice =
                row.oldPrice != null ? row.oldPrice : row.price;

            var currentPrice =
                row.price != null ? row.price : 0;

            row.originalPriceInCart = originalPrice;
            row.priceChangeFlag = null;

            if (
                originalPrice > 0 &&
                currentPrice > 0 &&
                originalPrice !== currentPrice
            ) {
                row.priceChangeFlag =
                    currentPrice > originalPrice ? "up" : "down";
            }

            row.__groupId = meta.id;

            cart.Items.CartListAdd(row, $groupItems);
        });
        // 初始化本組已選小計/件數
        $group.find('.js-group-subtotal').attr('data-subtotal', 0).text('$0');
        $group.find('.js-group-check').prop('indeterminate', false);
        $group.find('.js-selected-count').text(0);

        $ul.append($group);
    });

    const hasItems = result.length > 0;
    $("#Step1 > .card-body").toggleClass("d-none", !hasItems);
    $("#Purchase_Null").toggleClass("d-none", hasItems);

    S.buy_step_swiper.update();
}
function updateOverallSubtotal() {
    let sum = 0;
    $('#Step1 .purchase_list .purchase_group_header [data-field="subtotal"]').each(function () {
        sum += Number($(this).attr('data-subtotal') || 0);
    });
    $('#Step1 [data-key="subtotal"].subtotal').text(`$${sum.toLocaleString()}`);
    $('#Step1 [data-key="total"].subtotal').text(`$${sum.toLocaleString()}`);
}
function CartInit(result) {
    $("#Step1 > .card-body").removeClass("d-none");
    cart.Items.renderCartGroups(result);

    $("#Purchase_Null").addClass("d-none");
    S.buy_step_swiper.enable();

    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    S.buy_step_swiper.update();
    cart.Pricing.TotalCount();
    cart.Pricing.updateNextStepByBonus();
    cart.Shipping.ConfigurePaymentOptions(null);

    cart.Items.ValidateCartOnInit();

    const $firstGroup = $('.purchase_group').first();
    if ($firstGroup.length) {
        const $validItems = $firstGroup
            .find('input[name="buyItems"]')
            .not(':disabled')
            .not('[data-requires-confirmation="true"]');

        if ($validItems.length > 0) {
            $firstGroup.find('.js-group-check').prop('checked', true);
            $validItems.prop('checked', true);
        } else {
            $firstGroup.find('.js-group-check').prop('checked', false);
        }

        cart.Items.syncAdditionalSelection($firstGroup);
        cart.Items.updateGroupSelectedSubtotal($firstGroup);
        cart.Pricing.TotalCount();
        cart.Pricing.updateNextStepByBonus();
        cart.Payment.Core.onAmountChanged();
        cart.Payment.Core.reloadActiveEmbeddedProvider();
        if (cart.Marketing && typeof cart.Marketing.loadCartMarketingCampaigns === 'function') {
            cart.Marketing.loadCartMarketingCampaigns();
        }
    }
}
function CartListAdd(data, $container) {
    if (data.quantity > 0) {
        var exists = S.shopping_cart_data.find(e => e.Id == data.scId);
        var marketingRewardItemId = cart.Utils.getValueIgnoreCase(data, "FK_MarketingRewardItemId");

        if (exists != null) {
            data.price = exists.Price;
            exists.PId = data.pId;
            exists.PSId = data.psId;
            exists.IsAdditional = data.isAdditional === true;
            exists.MarketingRewardItemId = marketingRewardItemId == null
                ? null
                : Number(marketingRewardItemId);
            exists.Quantity = data.quantity;
        } else {
            var obj = {};
            obj['Id'] = data.scId;
            obj['PId'] = data.pId;
            obj['PSId'] = data.psId;
            obj['IsAdditional'] = data.isAdditional === true;
            obj['MarketingRewardItemId'] = marketingRewardItemId == null
                ? null
                : Number(marketingRewardItemId);
            obj['Price'] = data.price;
            obj['OriginalPrice'] = data.originalPriceInCart ?? data.price;
            obj['Quantity'] = data.quantity;
            obj['Bonus'] = data.bonus;
            obj['PackingPoint'] = Number(data.packingPoint || data.PackingPoint || 0);
            obj['freight'] = data.freight;
            obj['cvsStoreID'] = data.cvsStoreID;
            obj['cvsStoreName'] = data.cvsStoreName;
            obj['cvsAddress'] = data.cvsAddress;
            obj['cvsTelephone'] = data.cvsTelephone;
            obj['cvsOutSide'] = data.cvsOutSide;
            obj['logisticsSubType'] = data.logisticsSubType;
            S.shopping_cart_data.push(obj);
            cart.Items.refreshHasProds();
        }
    }

    var validationCode = data.validationCode || data.ValidationCode || '';
    var isQuantityShortage = validationCode === "QuantityExceedsStock";
    var max_quantity = data.noStockManagement === true
        ? Infinity
        : isQuantityShortage
            ? Number(data.stock || 0)
            : data.quantity + data.stock;

    var item_list_ul = $container || $("#Step1 > .card-body > .purchase_list");
    var $template = $($("#Template_Cart_Details").html()).clone();
    var groupId = (data.freight && data.freight.id) ? data.freight.id : 0;

    $template.data("scId", data.scId);
    $template.attr('data-group-id', groupId);
    $template = cart.Items.CartListInsert($template, data);

    if (data.isAdditional === true) {
        var parentLabel = String(
            cart.Utils.getValueIgnoreCase(data, "additionalParentLabel") || ""
        ).trim();

        var isOrderLevel = parentLabel === "本筆訂單";

        $template.attr("data-additional", "true");

        if (isOrderLevel) {
            // 訂單滿額／滿件優惠
            // 不屬於任何單一主商品，所以不做縮排與 Parent 線
            $template
                .addClass("cart-order-additional-item")
                .removeClass("cart-additional-item");
        }
        else {
            // 指定商品型加價購／贈品
            $template
                .addClass("cart-additional-item")
                .removeClass("cart-order-additional-item");
        }

        $template.find('.pro_name').first().before(
            '<span class="cart-additional-badge">' +
            '<i class="fa-solid fa-link me-1"></i>' +
            (isOrderLevel ? '訂單優惠' : '附屬優惠品') +
            '</span>'
        );

        $template.find('input[name="buyItems"]')
            .addClass('cart-additional-check')
            .attr({
                tabindex: '-1',
                'aria-hidden': 'true'
            });

        $template.find('.pro_quantity')
            .prop('readonly', true)
            .attr('aria-label', '優惠商品數量');

        if (!isOrderLevel) {
            $template.find('.btn_count_plus')
                .attr('title', '補選或調整優惠商品');
        }
    }
    else {
        $template.addClass("cart-primary-item");

        var openVariantEditor = function (event) {
            event.preventDefault();

            if (!window.ProductQuickCart ||
                typeof window.ProductQuickCart.open !== "function") {
                return;
            }

            window.ProductQuickCart.open({
                mode: "edit-cart",
                cartId: Number(data.scId || 0),
                productId: Number(data.pId || 0),
                productStockId: Number(data.psId || 0),
                productPriceId: Number(data.ppId || 0),
                quantity: Number(data.quantity || 1),
                productName: data.title || "",
                imageUrl: data.imagePath || "/images/noImg.jpg",
                productUrl: `/${OrgName}/home/product/${data.pId}`,
                onUpdated: function () {
                    cart.Items.ReloadCartDisplay().done(function (items) {
                        ReloadCartDropdown(items);
                        S.productAddOnDrafts = null;
                        Coker.sweet.success("商品規格已更新", null, true);
                    }).fail(function () {
                        Coker.sweet.error(
                            "商品已更新",
                            "畫面更新失敗，請重新整理購物車確認。",
                            null,
                            true
                        );
                    });
                }
            });
        };

        $template.find("a.pro_link")
            .removeAttr("target rel")
            .attr({
                href: "#",
                title: `查看或修改：${data.title}`,
                role: "button"
            })
            .on("click", openVariantEditor);

        var $specDetail = $template.find(".content > a.pro_link .pro_detail").first();
        if (!$specDetail.length) {
            $specDetail = $template.find(".pro_detail").first();
        }
        $("<span>", {
            class: "cart-item-edit-variant border-0 bg-transparent p-0",
            text: "查看／修改"
        }).appendTo($specDetail);
    }

    var isUnavailable = Number(data.quantity || 0) <= 0 ||
        data.available === false ||
        data.available === "false";

    if (isQuantityShortage) {
        $template.addClass("cart-item-stock-shortage");
        $template.find('input[name="buyItems"]').prop({
            checked: false,
            disabled: true
        });
        $template.find('.btn_count_plus').prop('disabled', true);

        var $shortageContent = $template.find(".js-cart-item-message");
        if (!$shortageContent.length) $shortageContent = $template;
        $shortageContent.append(
            $('<div class="js-cart-stock-shortage text-danger small mt-1"></div>')
                .text(data.describe || "目前庫存不足，請先調整數量才可結帳。")
        );
    } else if (isUnavailable) {
        $template.addClass("cart-item-error");
        $template.find('input[name="buyItems"]').prop({
            checked: false,
            disabled: true
        });

        // 不可販售狀態由專用區塊顯示原因，不在商品標題下方重複顯示。
    } else if (["SpecTitleChanged", "ProductTitleChanged", "CartSnapshotChanged"].includes(validationCode)) {
        $template.addClass("cart-item-warning");
        $template.find('input[name="buyItems"]')
            .prop('checked', false)
            .attr('data-requires-confirmation', 'true');

        var warningMessage = data.describe || "商品資訊已調整，請確認後再勾選結帳。";
        var $warningContent = $template.find(".js-cart-item-message");
        if (!$warningContent.length) $warningContent = $template;

        $warningContent.append(
            $('<div class="js-cart-change-warning text-warning-emphasis small mt-1"></div>').text(warningMessage)
        );
    }

    $template.find(".btn_remove_pro").on("click", function () {
        var $self = $(this).parents("li").first();
        var $group = $self.closest("li.purchase_group");
        var mayRemoveAdditionalItems = !$self.hasClass("cart-additional-item") &&
            $group.find("li.cart-additional-item").length > 0;

        if (mayRemoveAdditionalItems) {
            Coker.sweet.confirm(
                "確定移除主要商品？",
                "移除後若不再符合活動資格，相關加價購／贈品也會一併移除。",
                "確認移除",
                "取消",
                function () {
                    cart.Items.CartDelete($self, $self.data("scId"), "成功移除商品", "移除商品時發生錯誤");
                }
            );
            return;
        }
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            cart.Items.CartDelete($self, $self.data("scId"), "成功移除商品", "移除商品發生未知錯誤");
        });
    });

    $template.find(".btn_count_plus").on("click", function () {
        if ($template.hasClass("cart-item-error") ||
            $template.hasClass("cart-item-stock-shortage")) return;
        if ($template.hasClass("cart-additional-item")) {
            $template.closest('.purchase_group').find('.js-open-cart-addons').first().trigger('click');
            return;
        }

        var $self_bro = $(this).siblings(".pro_quantity");
        const $group = $template.closest('.purchase_group');

        if ($self_bro.val() < max_quantity) {
            $self_bro.val(parseInt($self_bro.val()) + parseInt($self_bro.attr("step")));
            cart.Items.CartQuantityUpdate(
                $template.find(".pro_subtotal"),
                data.price,
                data.bonus,
                $template.data("scId"),
                Number($self_bro.val()),
                $group
            );
        }
    });

    $template.find(".btn_count_minus").on("click", function () {
        if ($template.hasClass("cart-item-error")) return;

        var $self_bro = $(this).siblings(".pro_quantity");
        const $group = $template.closest('.purchase_group');

        const currentQty = Number($self_bro.val() || 0);
        const step = Number($self_bro.attr("step") || 1);
        const nextQty = currentQty - step;
        if (nextQty < step) {
            $template.find(".btn_remove_pro").trigger("click");
            return;
        }

        $self_bro.val(nextQty);

        cart.Items.CartQuantityUpdate(
            $template.find(".pro_subtotal"),
            data.price,
            data.bonus,
            $template.data("scId"),
            nextQty,
            $group
        );
    });

    $template.find(".pro_quantity").on("change", function () {
        if ($template.hasClass("cart-item-error")) return;

        var $self = $(this);
        const $group = $template.closest('.purchase_group');

        if ($self.val() < parseInt($self.attr("step"))) {
            $self.val(parseInt($self.attr("step")));
        } else if ($self.val() > max_quantity) {
            $self.val(max_quantity - (max_quantity % parseInt($self.attr("step"))));
        } else {
            $self.val($self.val() - ($self.val() % parseInt($self.attr("step"))));
        }

        cart.Items.CartQuantityUpdate(
            $template.find(".pro_subtotal"),
            data.price,
            data.bonus,
            $template.data("scId"),
            Number($self.val()),
            $group
        );
    });

    if ($template.find(".btn_move_to_favorites").length > 0) {
        if (S.islogin) {
            var $btn_favorites = $template.find(".btn_move_to_favorites");
            if (data.quantity && data.available !== false && data.available !== "false") {
                $btn_favorites.parent("span").removeClass("d-none");
            }

            Coker.Favorites.Check(data.pId).done(function (check) {
                if (check.success) {
                    $btn_favorites.data("Fid", check.message);
                    $btn_favorites.find("i").addClass("fa-solid")
                    $btn_favorites.find("i").removeClass("fa-regular")
                }
            });

            $btn_favorites.on("click", function () {
                var $self = $(this).find("i");
                if ($self.hasClass("fa-regular")) {
                    Coker.Favorites.Add(data.pId).done(function (favorites) {
                        if (favorites.success) {
                            $btn_favorites.data("Fid", favorites.message);
                            $self.addClass("fa-solid")
                            $self.removeClass("fa-regular")
                            Coker.sweet.success("成功將商品加入收藏", null, true);
                        } else {
                            Coker.sweet.error("商品加入收藏發生錯誤", favorites.message, null, true);
                        }
                    });
                } else {
                    if (typeof ($btn_favorites.data("Fid")) != "undefined" && typeof ($btn_favorites.data("Fid")) != "") {
                        Coker.Favorites.Delete($btn_favorites.data("Fid")).done(function (favorites) {
                            if (favorites.success) {
                                $btn_favorites.data("Fid", "");
                                $self.addClass("fa-regular")
                                $self.removeClass("fa-solid")
                                Coker.sweet.success("已將商品從收藏中移除", null, true);
                            } else {
                                Coker.sweet.error("商品移除收藏發生錯誤", favorites.message, null, true);
                            }
                        });
                    }
                }
            })
        }
    }

    item_list_ul.append($template);
}
function refreshHasProds() {
    S.hasProds = S.shopping_cart_data && S.shopping_cart_data.length > 0;
}
function CartListInsert($frame, data) {
    var isUnavailable = Number(data.quantity || 0) <= 0 ||
        data.available === false ||
        data.available === "false";

    $frame.find("*").each(function () {
        var $self = $(this);
        if (typeof ($self.data("key")) != "undefined") {
            var key = $self.data("key");
            switch (key) {
                case "title":
                    if (data[key] != data['oldTitle'] && data['oldTitle'] != null) $self.addClass("text-danger");
                    $self.text(data[key]);
                    break;
                case "link":
                    $self.attr({
                        href: `/${OrgName}/home/product/${data['pId']}`,
                        title: `連結至：${data['title']}(另開新視窗)`
                    });
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
                    if (data[key] != data['quantity']) $self.removeClass("d-none");
                    $self.text(data[key]);
                    break;
                case "oldPrice": {
                    var original = Number(data.oldPrice || 0);
                    var current = Number(data.price || 0);

                    var oldBonus = Number(data.oldBonus || 0);
                    var bonus = Number(data.bonus || 0);

                    var $priceDiv = $self.siblings("div[data-key='price']");

                    // 先清乾淨，避免殘留上一筆狀態
                    $self.addClass("d-none").text("");
                    $priceDiv.removeClass("price-up price-down price-changed text-danger text-success red_text");
                    $priceDiv.removeData("price-display-mode");

                    var priceChanged = original > 0 && current > 0 && original !== current;
                    var bonusChanged = oldBonus !== bonus;

                    // 價格與紅利都沒變：
                    // 不顯示舊價，且目前價格改用 block 顯示
                    if (!priceChanged && !bonusChanged) {
                        $priceDiv.data("price-display-mode", "block");
                        break;
                    }

                    let oldText = "";

                    if (oldBonus > 0) {
                        if (original > 0) {
                            oldText = `$${original.toLocaleString()} + 紅利${oldBonus.toLocaleString()}`;
                        } else {
                            oldText = `紅利${oldBonus.toLocaleString()}`;
                        }
                    } else {
                        oldText = `$${original.toLocaleString()}`;
                    }

                    $self.removeClass("d-none");
                    $self.text(oldText);

                    if (priceChanged) {
                        if (current > original) {
                            $priceDiv.addClass("price-up text-danger red_text");
                        } else {
                            $priceDiv.addClass("price-down text-success");
                        }
                    }

                    if (bonusChanged) {
                        if (bonus > oldBonus) {
                            $priceDiv.addClass("price-up text-danger red_text");
                        } else {
                            $priceDiv.addClass("price-down text-success");
                        }
                    }

                    $priceDiv.addClass("price-changed");
                    break;
                }
                case "price": {
                    var unitPrice = Number(data.price || 0);
                    var bonus = Number(data.bonus || 0);

                    var cashText = unitPrice > 0
                        ? `$${unitPrice.toLocaleString()}`
                        : "";

                    if (data.priceLabel != null && cashText) {
                        cashText = `${data.priceLabel} ${cashText}`;
                    }

                    var mode = $self.data("price-display-mode") || "inline";

                    cart.Pricing.setCartPriceBlock($self, cashText, bonus, mode);
                    break;
                }
                case "subtotal": {
                    var unitPrice = Number(data.price || 0);
                    var qty = Number(data.quantity || 0);
                    var sub_price = unitPrice * qty;
                    var sub_bonus = Number(data.bonus || 0) * qty;

                    var cashText = sub_price > 0
                        ? `$${sub_price.toLocaleString()}`
                        : "";

                    cart.Pricing.setCartPriceBlock($self, cashText, sub_bonus, "block");
                    $self.removeClass("d-none");
                    $self.data("subtotal", sub_price);
                    $self.data("subtotal_bonus", sub_bonus);
                    break;
                }

                case "quantity":
                    $self.val(data[key]);
                    $self.attr({ step: data.step })
                    break;
                case "freight":
                    if (data[key] == null) $self.remove();
                    else {
                        $self.data("freight", data[key]);
                        $self.text(data[key].title);
                    }
                    break;
                default:
                    $self.text(data[key]);
                    break;
            }
            const item = $self.closest(".image");
            const checkItem = item.find(`[name="buyItems"]`).attr("id", `prod${data.scId}`).val(data.scId);
            checkItem.next("label").attr("for", `prod${data.scId}`);
            if (isUnavailable) {
                $frame.find(".nostock")
                    .removeClass("d-none")
                    .find(".js-unavailable-message")
                    .text(data.describe || "此商品目前無法購買，請調整或移除。");
                $frame.children(".content").addClass("d-none");
                $frame.find(".btn_side_icon .favorites").addClass("d-none");
            }
        }
    });
    return $frame;
}
function CartQuantityUpdate(self, price, bonus, scid, quantity, $group) {
    if (quantity <= 0) return;

    var entry = S.shopping_cart_data.find(function (e) { return e.Id == scid; });
    var oldQty = entry ? entry.Quantity : quantity;

    function updateSubtotalAndDisplay(qty) {
        var sub_price = Number(price || 0) * Number(qty || 0);
        var sub_bonus = Number(bonus || 0) * Number(qty || 0);

        self.data("subtotal", sub_price);
        self.data("subtotal_bonus", sub_bonus);

        var cashText = sub_price > 0
            ? `$${sub_price.toLocaleString()}`
            : "";

        cart.Pricing.setCartPriceBlock(self, cashText, sub_bonus);
    }

    function syncGroupAndTotal() {
        if ($group && $group.length) {
            cart.Items.updateGroupSelectedSubtotal($group);
        }
        cart.Pricing.TotalCount();
        cart.Pricing.updateNextStepByBonus();
    }

    function handleUpdateError(title, message, lockItem) {
        var $li = self.closest('li.purchase_item');
        var $qty = $li.find('.pro_quantity');

        // 1. 回復原本數量與小計
        $qty.val(oldQty);
        updateSubtotalAndDisplay(oldQty);

        var displayMessage = message || "商品數量修改發生錯誤，請稍後再試。";

        // 2. 彈出錯誤訊息
        if (title) {
            Coker.sweet.error(title, displayMessage, null, true);
        }

        // 3. 商品列紅字提示：不管有沒有鎖商品，都要顯示
        $li.find('.js-stock-error').remove();

        var $content = $li.find('.js-cart-item-message');
        if (!$content.length) $content = $li;

        var $msgDiv = $('<div class="js-stock-error text-danger small mt-1"></div>');
        $msgDiv.text(displayMessage);
        $content.append($msgDiv);

        // 4. 只有真正不可購買時才鎖住商品
        if (lockItem === true) {
            $li.addClass('cart-item-error');

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            if ($itemCheckbox.length) {
                $itemCheckbox.prop({
                    checked: false,
                    disabled: true
                });
            }
        }

        syncGroupAndTotal();
    }

    Product.Update.Cart({
        Id: scid,
        Quantity: quantity
    }).done(function (result) {
        if (result.success) {
            var $li = self.closest('li.purchase_item');
            var updateItem = result.object?.items?.[0] || result.Object?.Items?.[0];
            var updateSucceeded = updateItem == null ||
                updateItem.success === true || updateItem.Success === true;
            var updateError = updateItem?.error || updateItem?.Error || '';
            var updatedQuantity = Number(
                updateItem?.newQuantity ?? updateItem?.NewQuantity ?? quantity
            );

            if (entry) {
                entry.Price = price;
                entry.Bonus = bonus;
                entry.Quantity = updatedQuantity;
                entry.PackingPoint = Number(entry.PackingPoint || 0);
            }

            $li.removeClass('cart-item-error');
            $li.find('.js-stock-error').remove();

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            if ($itemCheckbox.length) {
                $itemCheckbox.prop('disabled', false);
            }

            if (!updateSucceeded && updateError === "StockNotEnough" && updatedQuantity < oldQty) {
                $li.addClass('cart-item-stock-shortage');
                $li.find('.pro_quantity').val(updatedQuantity);
                $li.find('.btn_count_plus').prop('disabled', true);
                $itemCheckbox.prop({ checked: false, disabled: true });

                var $shortageContent = $li.find('.js-cart-item-message');
                if (!$shortageContent.length) $shortageContent = $li;
                $li.find('.js-cart-stock-shortage').remove();
                $('<div class="js-cart-stock-shortage text-danger small mt-1"></div>')
                    .text(updateItem.message || updateItem.Message || "目前庫存不足，請繼續調整數量。")
                    .appendTo($shortageContent);

                updateSubtotalAndDisplay(updatedQuantity);
                syncGroupAndTotal();
                CartDropReset(scid, updatedQuantity);
                cart.Payment.Core.onAmountChanged();
                return;
            }

            if (!updateSucceeded) {
                handleUpdateError(
                    "商品數量無法更新",
                    updateItem.message || updateItem.Message || "庫存不足，已恢復為原本數量。",
                    false
                );
                return;
            }

            $li.removeClass('cart-item-stock-shortage');
            $li.find('.js-cart-stock-shortage').remove();
            $li.find('.btn_count_plus').prop('disabled', false);
            updateSubtotalAndDisplay(updatedQuantity);
            syncGroupAndTotal();
            CartDropReset(scid, updatedQuantity);
            cart.Payment.Core.onAmountChanged();
            if (entry && entry.IsAdditional !== true && cart.Marketing && typeof cart.Marketing.loadCartMarketingCampaigns === 'function') {
                cart.Marketing.loadCartMarketingCampaigns().always(function () {
                    if (typeof cart.Marketing.requireProductAddOnAdjustment === 'function') {
                        cart.Marketing.requireProductAddOnAdjustment();
                    }
                });
            }
            return;
        }

        var msg = result.message || "商品數量修改發生錯誤，請稍後再試。";
        handleUpdateError("商品更改數量發生錯誤", msg, false);

    }).fail(function () {
        handleUpdateError("錯誤", "商品數量修改發生錯誤，請稍後再試。", true);
    });
}
function CartDelete(self, id, success, error) {
    var $group = self.closest('li.purchase_group');

    Product.Delete.Cart(id).done(function (result) {
        if (!result || result.success !== true) {
            Coker.sweet.error("錯誤", (result && result.error) || error, null, true);
            return;
        }

        var responseData = result.object || result.Object || {};
        var removedAdditionalIds = responseData.removedCartIds || responseData.RemovedCartIds || [];
        if (removedAdditionalIds.length > 0) {
            var applyCascadeRemoval = function () {
                var removedIds = [Number(id)].concat(removedAdditionalIds.map(Number));
                var removedSet = new Set(removedIds);

                $("#Step1 li.purchase_item").filter(function () {
                    return removedSet.has(Number($(this).data("scId")));
                }).remove();

                S.shopping_cart_data = (S.shopping_cart_data || []).filter(function (item) {
                    return !removedSet.has(Number(item.Id));
                });
                removedIds.forEach(function (cartId) {
                    CartDropReset(cartId, 0);
                });

                $("#Step1 li.purchase_group").each(function () {
                    var $currentGroup = $(this);
                    var remainingCount = $currentGroup.find("li.purchase_item").length;
                    if (remainingCount === 0) {
                        $currentGroup.remove();
                        return;
                    }

                    $currentGroup.find('[data-field="count"]').text(remainingCount + " 件");
                    cart.Items.syncAdditionalSelection($currentGroup);
                    cart.Items.updateGroupSelectedSubtotal($currentGroup);
                });

                cart.Items.refreshHasProds();
                cart.Pricing.TotalCount();
                cart.Pricing.updateNextStepByBonus();
                cart.Payment.Core.onAmountChanged();
                if (cart.Marketing && typeof cart.Marketing.loadCartMarketingCampaigns === "function") {
                    cart.Marketing.loadCartMarketingCampaigns();
                }
                if (parseInt($("#Car_Badge").text()) === 0) {
                    cart.Forms.DetailsClear();
                }
            };

            Coker.sweet.success(
                result.message || ("主要商品已移除，並一併移除 " + removedAdditionalIds.length + " 項優惠商品。"),
                applyCascadeRemoval,
                false
            );
            return;
        }

        self.remove();
        Coker.sweet.success(success, null, true);

        var index = S.shopping_cart_data.findIndex(e => e.Id == id);
        if (index !== -1) {
            S.shopping_cart_data.splice(index, 1);
            cart.Items.refreshHasProds();
        }

        if ($group.length) {
            var remainingCount = $group.find('li.purchase_item').length;

            if (remainingCount === 0) {
                $group.remove();
            } else {
                $group.find('[data-field="count"]').text(`${remainingCount} 件`);
                cart.Items.syncAdditionalSelection($group);
                cart.Items.updateGroupSelectedSubtotal($group);
            }
        }

        CartDropReset(id, 0);
        cart.Pricing.TotalCount();
        cart.Pricing.updateNextStepByBonus();
        cart.Payment.Core.onAmountChanged();

        var isReorderPage = window.location.search.substring(1).startsWith("reorder");
        var reorderItemsRemain = $("#Step1 .purchase_group li.purchase_item").length > 0;
        if (isReorderPage && !reorderItemsRemain) {
            window.location.replace(`/${OrgName}/ShoppingCar`);
            return;
        }

        if (parseInt($("#Car_Badge").text()) == 0) {
            cart.Forms.DetailsClear();
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error, null, true);
    });
}
function getSelectedCartIds() {
    const ids = [];
    $('.purchase_group li.purchase_item input[name="buyItems"]:checked').each(function () {
        const $li = $(this).closest('li.purchase_item');
        const scid = $li.data('scId');            // ← 你已有：$template.data("scId", data.scId);
        if (scid != null) ids.push(Number(scid));
    });
    return ids;
}
function getSelectedCartItems() {
    var ids = cart.Items.getSelectedCartIds();
    return S.shopping_cart_data.filter(function (x) {
        return ids.includes(Number(x.Id));
    });
}
function ValidateCartOnInit() {
    if (!S.shopping_cart_data || S.shopping_cart_data.length === 0) return;

    var payload = S.shopping_cart_data
        .filter(function (x) { return x.Quantity > 0; })
        .map(function (x) {
            return { Id: x.Id, Quantity: x.Quantity };
        });

    if (payload.length === 0) return;

    Product.Update.MultiCart(payload).done(function (result) {
        var batch = result.object || result.Object;
        if (!batch) return;

        var items = batch.items || batch.Items || [];
        if (!items || !items.length) return;

        items.forEach(function (item) {
            var cartId = item.cartId || item.CartId;
            var success = (item.success === true || item.Success === true);
            var removed = (item.removed === true || item.Removed === true);
            var msg = item.message || item.Message || '';
            var errorCode = item.error || item.Error || '';

            if (!cartId) return;

            var $li = $('li.purchase_item').filter(function () {
                return $(this).data('scId') === cartId;
            });
            if (!$li.length) return;

            // 先清掉舊狀態
            $li.removeClass('cart-item-error');
            $li.find('.js-stock-error').remove();

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            // 預設先解鎖，避免舊狀態殘留
            $itemCheckbox.prop('disabled', false);

            if (errorCode === "StockNotEnough" &&
                $li.hasClass('cart-item-stock-shortage')) {
                $itemCheckbox.prop({ checked: false, disabled: true });
                $li.find('.btn_count_plus').prop('disabled', true);
                $li.find('.js-cart-stock-shortage')
                    .text(msg || '目前庫存不足，請先調整數量才可結帳。');
                return;
            }

            // ✅ 成功 & 未被後端標記移除 → 不處理
            if (success && !removed) return;

            // ❌ 有錯誤的品項：加上錯誤標記、取消勾選並禁用
            $li.addClass('cart-item-error');

            if ($itemCheckbox.length) {
                $itemCheckbox.prop('checked', false);
                $itemCheckbox.prop('disabled', true);
            }

            var $content = $li.find('.js-cart-item-message');
            if (!$content.length) $content = $li;
            var $msgDiv = $('<div class="js-stock-error text-danger small mt-1"></div>');
            $msgDiv.text(msg || '此商品目前無法購買，請調整或移除。');
            $content.append($msgDiv);
        });

        cart.Items.refreshAllCartGroupSubtotal();
    }).fail(function () {
        // 驗證失敗就當沒發生，不影響使用者操作
    });
}

    Object.assign(cart.Items, {
        syncHeaderCheckbox: syncHeaderCheckbox,
        refreshAllCartGroupSubtotal: refreshAllCartGroupSubtotal,
        updateGroupSelectedSubtotal: updateGroupSelectedSubtotal,
        clearOtherGroupsExcept: clearOtherGroupsExcept,
        CardDataGet: CardDataGet,
        ReloadCartDisplay: ReloadCartDisplay,
        groupByFreight: groupByFreight,
        createGroupHeader: createGroupHeader,
        renderCartGroups: renderCartGroups,
        updateOverallSubtotal: updateOverallSubtotal,
        CartInit: CartInit,
        CartListAdd: CartListAdd,
        refreshHasProds: refreshHasProds,
        CartListInsert: CartListInsert,
        syncAdditionalSelection: syncAdditionalSelection,
        CartQuantityUpdate: CartQuantityUpdate,
        CartDelete: CartDelete,
        getSelectedCartIds: getSelectedCartIds,
        getSelectedCartItems: getSelectedCartItems,
        ValidateCartOnInit: ValidateCartOnInit
    });
})(window.ShoppingCart, window.jQuery);
