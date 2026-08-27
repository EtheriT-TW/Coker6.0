// wwwroot/view-resources/ShoppingCart/shopping-cart.logistics.ecpay.js
// ECPay logistics / CVS map selection. This file handles logistics only, not ECPay payment.
(function (cart, $) {
    "use strict";

    var S = cart.State;

    cart.Logistics = cart.Logistics || {};
    cart.Logistics.ECPay = cart.Logistics.ECPay || {};

    function bindMapButton() {
        $(document)
            .off("click.shoppingCartEcpayLogistics", ".btn_getmap")
            .on("click.shoppingCartEcpayLogistics", ".btn_getmap", openMap);
    }

    function openMap() {
        var $radio = $('input[name="RadioShipping"]:checked');
        if (!$radio.length || !cart.Shipping.RequiresMerchantCvsStore()) {
            Coker.sweet.warning("請注意", "目前選擇的物流與付款方式不需在此選擇門市。", null);
            return;
        }

        saveOrderFormBeforeRedirect();

        var $form = $("form#ecpayLogisticsForm");
        var selectedCartIds = cart.Items.getSelectedCartIds();
        var selectedPaymentValue = cart.Payment.Core.getActivePaymentValue();
        var $selectedPayment = $('#RadioPayment input[name="RadioPayment"][value="' +
            selectedPaymentValue + '"]');
        var isCollection = String($selectedPayment.attr("data-code") || "")
            .toUpperCase() === "COD";

        $form.find('input[name="LogisticsSubType"]').val($radio.attr('data-logistics-subtype') || "");
        $form.find('input[name="SCIds"]').val(JSON.stringify(selectedCartIds));
        $form.find('input[name="IsCollection"]').val(isCollection ? "Y" : "N");

        // 注意：客戶綠界帳號 / 平台綠界帳號的判斷不在這支 JS 寫死。
        // 這裡沿用既有 ecpayLogisticsForm；表單 action / hidden 欄位由後端輸出時決定。
        $form.submit();
    }

    function saveOrderFormBeforeRedirect() {
        cart.Forms.AllDataGet(false);

        var paymentValue = cart.Payment.Core.getActivePaymentValue();

        if (paymentValue == null || paymentValue === "") {
            delete S.order_header_data.payment;
        } else {
            S.order_header_data.payment = Number(paymentValue);
        }

        var dataToSave = {
            formData: S.order_header_data,
            ShippingId: Number(S.order_header_data.shipping || 0),
            PaymentId: paymentValue == null || paymentValue === "" ? null : Number(paymentValue),
            SelectedCartIds: cart.Items.getSelectedCartIds(),
            RecipientType: $('[name="RecipientRadio"]:checked').val(),
            ScrollTop: window.pageYOffset || document.documentElement.scrollTop || 0,
            savedAt: Date.now()
        };

        sessionStorage.setItem("orderForm", JSON.stringify(dataToSave));
    }

    function restoreOrderForm() {
        var raw = sessionStorage.getItem("orderForm");
        if (!raw) return;

        var data;
        try {
            data = JSON.parse(raw);
        } catch (err) {
            sessionStorage.removeItem("orderForm");
            return;
        }

        var savedAt = Number(data.savedAt || 0);
        var diffMinutes = (Date.now() - savedAt) / 1000 / 60;

        if (!savedAt || diffMinutes >= 30) {
            sessionStorage.removeItem("orderForm");
            return;
        }

        var formData = data.formData || {};
        var selectedCartIds = data.SelectedCartIds || data.selectedCartIds || [];

        if (!Array.isArray(selectedCartIds) || selectedCartIds.length === 0) return;

        co.sweet.loading();

        var timer = setInterval(function () {
            if ($('input[name="buyItems"]').length === 0) return;

            clearInterval(timer);
            Swal.close();

            var $selectedGroup;

            $('input[name="buyItems"]').prop("checked", false);

            $('input[name="buyItems"]').each(function () {
                var $this = $(this);
                var value = Number($this.val());

                if (selectedCartIds.includes(value)) {
                    $this.prop("checked", true);
                    $selectedGroup = $this.closest('.purchase_group');
                    cart.Items.clearOtherGroupsExcept($selectedGroup);
                }
            });

            if (typeof ($selectedGroup) !== "undefined") {
                cart.Items.updateGroupSelectedSubtotal($selectedGroup);
            }

            cart.Pricing.TotalCount();
            cart.Pricing.updateNextStepByBonus();

            S.isRestoringECPayLogistics = true;

            S.buy_step_swiper.enable();
            S.buy_step_swiper.slideTo(1, 0);

            restoreOrdererForm(formData);
            restoreRecipientForm(data, formData);
            restoreInvoiceForm(formData);
            restoreShippingSelection(
                formData,
                selectedCartIds,
                data.ShippingId ?? data.shippingId);

            if (formData.invoiceRecipient == 2) {
                $('[name="InvoiceRadio"][value="order"]').prop("checked", true);
            }

            if (cart.Forms.FormCheck(S.OrdererForms)) {
                cart.Forms.OrdererEdit(false);
            }

            // 從綠界超商地圖返回後，這裡才是表單、物流、付款狀態都恢復完成的位置。
            // CVSStoreID / CVSStoreName / CVSAddress 會影響綠界付款 snapshot，
            // 所以必須在這裡明確重載嵌入式金流，而不是交給 RadioShipping 猜。
            cart.Shipping.RadioShipping();

            // RadioShipping 會排入一次延遲查詢；返回流程改由下方使用原付款 ID
            // 立即查詢，避免預設物流留下的舊請求稍後覆寫選擇。
            if (S.PaymentAvailabilityTimer != null) {
                clearTimeout(S.PaymentAvailabilityTimer);
                S.PaymentAvailabilityTimer = null;
            }

            var savedPaymentId = data.PaymentId ?? data.paymentId ?? formData.payment;
            var paymentRestore = restorePaymentSelection(savedPaymentId);
            var continueRestore = function () {
                cart.Payment.Core.RadioPayment();
                cart.Shipping.UpdateCvsStoreSelectionDisplay();

                var reloadPayment = cart.Payment.Core.reloadActiveEmbeddedProvider();
                var finishRestore = function () {
                    cart.Shipping.UpdateCvsStoreSelectionDisplay();
                    sessionStorage.removeItem("orderForm");
                    restoreScrollPosition(data.ScrollTop ?? data.scrollTop, function () {
                        S.isRestoringECPayLogistics = false;
                    });
                };

                if (reloadPayment && typeof reloadPayment.always === "function") {
                    reloadPayment.always(finishRestore);
                } else {
                    finishRestore();
                }
            };

            if (paymentRestore && typeof paymentRestore.always === "function") {
                paymentRestore.always(continueRestore);
            } else {
                continueRestore();
            }
        }, 50);
    }

    function restorePaymentSelection(savedPaymentId) {
        if (cart.Payment.Availability &&
            typeof cart.Payment.Availability.refresh === "function") {
            return cart.Payment.Availability.refresh(savedPaymentId);
        }

        cart.Shipping.ConfigurePaymentOptions(savedPaymentId);
        return null;
    }

    function restoreScrollPosition(savedScrollTop, completed) {
        var scrollTop = Number(savedScrollTop);
        if (!Number.isFinite(scrollTop) || scrollTop < 0) {
            if (typeof completed === "function") completed();
            return;
        }

        // 等付款模組與 Swiper 完成最後一次排版，再直接回到跳轉前的位置。
        setTimeout(function () {
            if (S.buy_step_swiper) S.buy_step_swiper.update();

            var root = document.documentElement;
            var body = document.body;
            var rootScrollBehavior = root.style.scrollBehavior;
            var bodyScrollBehavior = body.style.scrollBehavior;

            $("html, body").stop(true);
            root.style.scrollBehavior = "auto";
            body.style.scrollBehavior = "auto";
            window.scrollTo(0, scrollTop);

            window.requestAnimationFrame(function () {
                root.style.scrollBehavior = rootScrollBehavior;
                body.style.scrollBehavior = bodyScrollBehavior;
                if (typeof completed === "function") completed();
            });
        }, 100);
    }

    function restoreShippingSelection(formData, selectedCartIds, explicitShippingId) {
        var savedShippingId = String(explicitShippingId || formData.shipping || "");
        var $shipping = savedShippingId
            ? $('input[name="RadioShipping"][value="' + savedShippingId + '"]').first()
            : $();

        if (!$shipping.length) return;

        $shipping.prop("checked", true);

        var logisticsSubType = String($shipping.attr("data-logistics-subtype") || "").toUpperCase();
        var returnedStore = (S.shopping_cart_data || []).find(function (item) {
            return selectedCartIds.includes(Number(item.Id)) &&
                String(item.logisticsSubType || "").toUpperCase() === logisticsSubType &&
                $.trim(item.cvsStoreID || "") !== "";
        });

        if (!returnedStore) return;

        $shipping.attr({
            "data-cvsstoreid": returnedStore.cvsStoreID || "",
            "data-cvsstorename": returnedStore.cvsStoreName || "",
            "data-cvsaddress": returnedStore.cvsAddress || "",
            "data-cvstelephone": returnedStore.cvsTelephone || "",
            "data-cvsoutside": returnedStore.cvsOutSide || ""
        });
    }

    function restoreOrdererForm(formData) {
        cart.Forms.OrdererEdit(true);

        var ordererAddress = formData.ordererAddress || "";

        co.Form.insertData(formData, "#Form_Orderer");

        setAddressTail("#OrdererInputAddress", ordererAddress);

        co.Zipcode.setData({
            el: $("#Orderer_TWzipcode"),
            addr: ordererAddress
        });
    }

    function restoreRecipientForm(data, formData) {
        var recipientType = data.RecipientType || data.recipientType;

        if (recipientType === "choose" && cart.Recipients) {
            cart.Recipients.Apply(formData, false);
            return;
        }

        if (recipientType !== "edit") return;

        $('[name="RecipientRadio"][value="edit"]').prop("checked", true).trigger("change");

        var recipientAddress = formData.recipientAddress || "";

        co.Form.insertData(formData, "#Form_Recipient");
        setAddressTail("#RecipientInputAddress", recipientAddress);

        co.Zipcode.setData({
            el: $("#Recipient_TWzipcode"),
            addr: recipientAddress
        });
    }

    function restoreInvoiceForm(formData) {
        if (formData.invoiceType != 2) return;

        $('[name="InvoiceType"][value="company"]').prop("checked", true);
        cart.Forms.InvoiceTypeRadio();

        var invoiceAddress = formData.invoiceAddress || "";

        co.Form.insertData(formData, "#Form_Invoice");
        setAddressTail("#InvoiceInputAddress", invoiceAddress);

        co.Zipcode.setData({
            el: $("#Invoice_TWzipcode"),
            addr: invoiceAddress
        });
    }

    function setAddressTail(selector, fullAddress) {
        if (!fullAddress || fullAddress.indexOf(" ") < 0) {
            $(selector).val(fullAddress || "");
            return;
        }

        var firstSpace = fullAddress.indexOf(" ");
        var secondSpace = fullAddress.indexOf(" ", firstSpace + 1);

        if (secondSpace < 0) {
            $(selector).val("");
            return;
        }

        $(selector).val(fullAddress.substring(secondSpace).trim());
    }

    Object.assign(cart.Logistics.ECPay, {
        bindMapButton: bindMapButton,
        openMap: openMap,
        saveOrderFormBeforeRedirect: saveOrderFormBeforeRedirect,
        restoreOrderForm: restoreOrderForm
    });
})(window.ShoppingCart, window.jQuery);
