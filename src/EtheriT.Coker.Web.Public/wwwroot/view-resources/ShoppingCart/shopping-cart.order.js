// wwwroot/view-resources/ShoppingCart/shopping-cart.order.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Order = cart.Order || {};

    async function OrderHeaderAdd() {
        Coker.sweet.loading();

        var paymentInfo = null;

        cart.Pricing.TotalCount();

        var ids = cart.Items.getSelectedCartIds();
        var data = S.shopping_cart_data.filter(e => ids.includes(e.Id));

        if (ids.length === 0) {
            Coker.sweet.warning("請注意", "請先勾選要結帳的商品。", null);
            return;
        }

        if (!cart.Forms.AllDataGet(true)) {
            return;
        }

        Coker.Order.CheckStock(data).done(async function (result) {
            if (!result.success) {
                Coker.sweet.error("錯誤", result.message, null, false);
                $("#Step1 > .card-body > .purchase_list > li").remove();
                cart.Items.CardDataGet();
                S.buy_step_swiper.slideTo(0);
                return;
            }

            cart.Pricing.TotalCount();

            var checksuccess = cart.Forms.AllDataGet(true);

            var shipping_radio = $(`[name="RadioShipping"]:checked`);
            S.order_header_data.shipping = shipping_radio.val();
            S.order_header_data.CVSStoreID = shipping_radio.attr("data-cvsstoreid") ?? null;

            var hasBtnGetMap = shipping_radio.closest(".shipping-option-row").find(".btn_getmap").length > 0;
            if (hasBtnGetMap && S.order_header_data.CVSStoreID == null) {
                Coker.sweet.warning("請注意", "請選擇取貨門市！", null);
                return;
            }

            if (!checksuccess) {
                return;
            }

            S.order_header_data.OrderDetails = S.order_header_data.OrderDetails.filter(e => ids.includes(e.Id));
            S.order_header_data.remark = S.$remark.val();

            if (typeof (S.order_header_data.remark) == "undefined" || S.order_header_data.remark == "") {
                S.order_header_data.remark = "無";
            }

            cart.Payment.Core.submitActiveEmbeddedPayment(function (success, result) {
                if (!success) {
                    if (result && result.handled === true) {
                        return;
                    }

                    Coker.sweet.warning(
                        "付款資訊取得失敗",
                        result || "目前無法取得付款資訊，請重新操作一次；若問題持續發生，請聯絡客服協助。",
                        null
                    );
                    return;
                }

                cart.Order.AddHeader(result || null);
            });
        });
    }
    function AddHeader(paymentInfo) {
        var memberUpdateFailMessage = "";

        if ($(".memberUpdate").length > 0 ? $("#MemberUpdate").is(":checked") : false) {
            Coker.Order.FrontUserUpdate(S.order_header_data).done(function (result) {
                if (!result.success) {
                    memberUpdateFailMessage = `<br/>${result.message}`;
                }
            });
        }

        Coker.Order.AddHeader(S.order_header_data).done(function (result) {
            if (result.success) {
                Coker.sweet.success(
                    `謝謝您的訂購！${memberUpdateFailMessage}<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！`,
                    function () {
                        var orderResultLoading = cart.CheckoutResult.OrderSuccess(result);

                        $.when(orderResultLoading).always(function () {
                            S.isCheckout = true;

                            var message = result.message.split(",");
                            var paymenttype = message[0];
                            var orderId = message[1];

                            cart.Payment.Core.afterOrderCreated(result, {
                                paymentInfo: paymentInfo,
                                paymentType: paymenttype,
                                orderId: orderId
                            });
                        });
                    }
                );
            } else {
                Coker.sweet.error("錯誤", result.error, null, true);
            }
        }).fail(function (result) {
            Coker.sweet.error("錯誤", result.error, null, true);
        });
    }
    function PurchaseAdd(result, item_list_ul) {
        var item = $($("#Template_Purchase_Details").html()).clone();
        var item_link = item.find(".pro_link"),
            item_image = item.find(".pro_image"),
            item_name = item.find(".pro_name"),
            item_specification = item.find(".pro_specification"),
            item_unit = item.find(".pro_unit"),
            item_unitBonus = item.find(".pro_bonus"),
            item_quantity = item.find(".pro_quantity"),
            item_subtotal = item.find(`.pro_subtotal > [data-key="subtotal"]`),
            item_subtotal_bonus = item.find(`.pro_subtotal > [data-key="subtotalBonus"]`);

        item_link.attr("href", `/${OrgName}/Home/product/` + result.pId);
        item_link.attr("title", `連結至：${result.title}(另開新視窗)`);
        item_image.attr("src", result.imagePath.replace(`upload/${OrgName}/`, "upload/"));
        item_name.text(result.title);
        item_specification.append(result.s1Title == "" ? "" : '<span class="border px-1 me-1">' + result.s1Title + '</span>')
        item_specification.append(result.s2Title == "" ? "" : '<span class="border px-1">' + result.s2Title + '</span>')
        if (result.price > 0)
            item_unit.text(`$${(result.price).toLocaleString('en-US')}`)
        if (result.bonusPrice > 0)
            item_unitBonus.text(`紅利：${(result.bonusPrice).toLocaleString('en-US')}`)
        item_quantity.text(result.quantity);
        if (result.price > 0)
            item_subtotal.text((result.price * result.quantity).toLocaleString('en-US'))
        if (result.bonusPrice > 0)
            item_subtotal_bonus.text("紅利：" + (result.bonusPrice * result.quantity).toLocaleString('en-US'))

        item_list_ul.append(item);
    }
    function HiddenCode($self) {
        $self.find("*").each(function () {
            var $this = $(this);
            var key = $this.data("key");
            if (typeof (key) != "undefined") {
                switch ($this.data("type")) {
                    case "name":
                        var name = $this.text();
                        $this.text(`${name.substr(0, 1)}○${name.substr(name.length - 1)}`)
                        break;
                    case "email":
                        var email = $this.text();
                        $this.text(`${email.substr(0, 3)}**********`)
                        break;
                    case "phone":
                        var phone = $this.text();
                        $this.text(`${phone.substr(0, 3)}****${phone.substr(phone.length - 3)}`)
                        break;
                    case "address":
                        var address = $this.text();
                        address = address.split(' ')[0] + address.split(' ')[1] + address.split(' ')[2];
                        $this.text(`${address.substr(0, 9)}*****`)
                        break;
                    case "uniformId":
                        var uniformId = $this.text();
                        $this.text(`${uniformId.substr(0, 3)}*****`)
                        break;
                }
            }
        });
    }

    Object.assign(cart.Order, {
        OrderHeaderAdd: OrderHeaderAdd,
        AddHeader: AddHeader,
        PurchaseAdd: PurchaseAdd,
        HiddenCode: HiddenCode
    });
})(window.ShoppingCart, window.jQuery);
