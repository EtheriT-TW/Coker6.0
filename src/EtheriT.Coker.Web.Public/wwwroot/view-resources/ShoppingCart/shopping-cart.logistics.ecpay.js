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
        saveOrderFormBeforeRedirect();

        var $btn = $(this);
        var $radio = $btn.prev('input[name="RadioShipping"]');
        $radio.prop('checked', true);

        var $form = $("form#ecpayLogisticsForm");
        var selectedCartIds = cart.Items.getSelectedCartIds();

        $form.find('input[name="LogisticsSubType"]').val($btn.data('subtype'));
        $form.find('input[name="SCIds"]').val(JSON.stringify(selectedCartIds));
        $form.find('input[name="IsCollection"]').val($btn.data("support-cash-on-delivery") == "True" ? "Y" : "N");

        // 注意：客戶綠界帳號 / 平台綠界帳號的判斷不在這支 JS 寫死。
        // 這裡沿用既有 ecpayLogisticsForm；表單 action / hidden 欄位由後端輸出時決定。
        $form.submit();
    }

    function saveOrderFormBeforeRedirect() {
        cart.Forms.AllDataGet(false);

        if (typeof (S.order_header_data.payment) == "undefined") {
            S.order_header_data.payment = $('[name="RadioPayment"]:checked').val();
        }

        var dataToSave = {
            formData: S.order_header_data,
            SelectedCartIds: cart.Items.getSelectedCartIds(),
            RecipientType: $('[name="RecipientRadio"]:checked').val(),
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
            console.error("orderForm parse failed", err);
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

            S.buy_step_swiper.enable();
            S.buy_step_swiper.slideTo(1);

            restoreOrdererForm(formData);
            restoreRecipientForm(data, formData);
            restoreInvoiceForm(formData);

            if (formData.invoiceRecipient == 2) {
                $('[name="InvoiceRadio"][value="order"]').prop("checked", true);
            }

            if (cart.Forms.FormCheck(S.OrdererForms)) {
                cart.Forms.OrdererEdit(false);
            }

            cart.Shipping.ConfigurePaymentOptions(formData.payment);

            sessionStorage.removeItem("orderForm");
        }, 50);
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

        if (recipientType !== "edit") return;

        $('[name="RecipientRadio"][value="edit"]').prop("checked", true);
        cart.Forms.RecipientRadio();

        var recipientAddress = formData.recipientAddress || "";

        co.Form.insertData(formData, "#RecipientForm");
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
