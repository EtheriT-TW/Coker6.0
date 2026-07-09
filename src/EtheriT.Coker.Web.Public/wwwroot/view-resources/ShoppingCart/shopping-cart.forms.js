// wwwroot/view-resources/ShoppingCart/shopping-cart.forms.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Forms = cart.Forms || {};

    function ElementInit() {
        /* TWzipcode 初始化 */
        S.$Orderer_TWzipcode = $('#Orderer_TWzipcode');
        S.$Recipient_TWzipcode = $('#Recipient_TWzipcode');
        S.$Invoice_TWzipcode = $('#Invoice_TWzipcode');
        cart.Forms.TWZipCodeInit();

        /* 寄件者資訊 */
        S.$orderer_name = $("#OrdererInputName");
        S.$orderer_sex = $("input[name=OrdererRadioGender]");
        S.$orderer_email = $("#OrdererInputMail");
        S.$orderer_cellphone = $("#OrdererInputCellPhone");
        S.$orderer_telphone_area = $("#OrdererInputTelPhoneArea");
        S.$orderer_telphone = $("#OrdererInputTelPhone");
        S.$orderer_telphone_ext = $("#OrdererInputTelPhoneExt");
        S.$orderer_address_city = S.$Orderer_TWzipcode.children('.county').children("select");
        S.$orderer_address_town = S.$Orderer_TWzipcode.children('.district').children("select");
        S.$orderer_address = $("#OrdererInputAddress");

        /* 收件者資訊 */
        S.$recipient_radio = $("input[name=RecipientRadio]");
        S.$recipient_name = $("#RecipientInputName");
        S.$recipient_sex = $("input[name=RecipientRadioGender]");
        S.$recipient_email = $("#RecipientInputMail");
        S.$recipient_cellphone = $("#RecipientInputCellPhone");
        S.$recipient_telphone_area = $("#RecipientInputTelPhoneArea");
        S.$recipient_telphone = $("#RecipientInputTelPhone");
        S.$recipient_telphone_ext = $("#RecipientInputTelPhoneExt");
        S.$recipient_address_city = S.$Recipient_TWzipcode.children('.county').children("select");
        S.$recipient_address_town = S.$Recipient_TWzipcode.children('.district').children("select");
        S.$recipient_address = $("#RecipientInputAddress");
        S.$remark = $("#TextareaRemark");

        /* 發票 */
        S.$invoice_recipient = $("input[name=InvoiceRadio]");
        S.$invoice_title = $("#InvoiceInputTitle");
        S.$invoice_uniformid = $("#InvoiceInputUniformId");
        S.$invoice_address_city = S.$Invoice_TWzipcode.children('.county').children("select");
        S.$invoice_address_town = S.$Invoice_TWzipcode.children('.district').children("select");
        S.$invoice_address = $("#InvoiceInputAddress");

        /* 運送、付款方式 */
        S.$ship_method = $("input[name=RadioShipping]");
        S.$ship_method.each(function () {
            if ($(this).is(":checked")) {
                S.shipping = $(this).val();
                S.ori_freight = Number($(this).data("freight") || 0);
                S.low_con = Number($(this).data("lowcon") || 0);
                S.disfreight = Number($(this).data("disfreight") || 0);
                S.freightType = Number($(this).data("freight-type") || 0);

                var rawDiscountFreightType = $(this).attr("data-discount-freight-type");
                S.discountFreightType = rawDiscountFreightType === "" || rawDiscountFreightType == null
                    ? null
                    : Number(rawDiscountFreightType);

                var rawBoxFees = $(this).attr("data-boxfees");
                try {
                    S.boxFees = rawBoxFees ? JSON.parse(rawBoxFees) : [];
                } catch (err) {
                    console.error("init data-boxfees parse failed", err, rawBoxFees);
                    S.boxFees = [];
                }

                S.freight = S.ori_freight;
            } else {
                S.freight = 0;
            }
        });
        S.$pay_method = $("input[name=RadioPayment]");
    }
    function OrdererEdit(isopen, suppressECPayChange) {
        if (isopen == null) {
            isopen = !S.OrdererOpen;
        }

        if (!S.OrdererOpen && isopen) {
            $("#OrdererForm > .default_data").addClass("d-none");
            $("#OrdererForm > form#Form_Orderer").removeClass("d-none");
            S.OrdererOpen = true;
            S.OrdererFilled = false;
        } else if (S.OrdererOpen && !isopen) {
            S.OrdererFilled = cart.Forms.FormCheck(S.OrdererForms);
            if (!S.OrdererFilled) {
                Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
            } else {
                var data = co.Form.getJson($("#Form_Orderer").attr("id"), $("#OrdererForm .default_data"));
                data.ordererAddress = `${data.county}${data.district}${data.ordererAddress}`;
                cart.Utils.ShoppingCartDataInsert(data, $("#OrdererForm .default_data"));
                $("#OrdererForm > .default_data").removeClass("d-none");
                $("#OrdererForm > form#Form_Orderer").addClass("d-none");
                S.OrdererOpen = false;
            }
        }

        if (suppressECPayChange !== true) {
            cart.Payment.Core.onAmountChanged();
            cart.Payment.Core.reloadActiveEmbeddedProvider();
        }

        S.buy_step_swiper.update();
    }
    function RecipientRadio() {
        const value = $("[name='RecipientRadio']:checked").val();
        S.recipient_data = {};

        if (value == "edit") {
            $("#RecipientForm > .default_data").addClass("d-none");
            $("#RecipientForm > form").removeClass("d-none");
            S.RecipientOpen = true;
            S.RecipientFilled = false;
            cart.Forms.RecipientFormClear();
        } else if (value == "order") {
            $("#RecipientForm > .default_data").addClass("d-none");
            $("#RecipientForm > form").addClass("d-none");
            S.RecipientOpen = false;
            S.RecipientFilled = true;
            cart.Forms.RecipientSameOrderer();
        } else {
            $("#RecipientForm > .default_data").addClass("d-none");
            $("#RecipientForm > form").addClass("d-none");
            S.RecipientOpen = false;
            S.RecipientFilled = true;
        }
        cart.Payment.Core.onAmountChanged();
        cart.Payment.Core.reloadActiveEmbeddedProvider();
        S.buy_step_swiper.update();
    }
    function RecipientFormClear() {
        S.$recipient_name.val("");
        S.$recipient_sex.val("");
        S.$recipient_sex.each(function () {
            $(this).removeAttr("checked");
        })
        S.$recipient_email.val("");
        S.$recipient_cellphone.val("");
        S.$recipient_telphone_area.val("");
        S.$recipient_telphone.val("");
        S.$recipient_telphone_ext.val("");
        S.$recipient_address_city.val("");
        S.$recipient_address_town.val("");
        S.$recipient_address.val("");
    }
    function RecipientSameOrderer() {
        for (var key in S.order_data) {
            if (key.startsWith("orderer") > 0) {
                S.recipient_data[key.replace("orderer", "recipient")] = S.order_data[key];
            }
        }
    }
    function InvoiceRadio() {
        S.invoice_data = {}
        switch (this.value) {
            case "order":
                $("#InvoiceForm > .default_data").addClass("d-none");
                $("#InvoiceForm > form").addClass("d-none");
                S.InvoiceOpen = false;
                S.InvoiceFilled = true;
                break;
            case "recipient":
                $("#InvoiceForm > .default_data").addClass("d-none");
                $("#InvoiceForm > form").addClass("d-none");
                S.InvoiceOpen = false;
                S.InvoiceFilled = true;
                break;
        }
        cart.Payment.Core.onAmountChanged();
        cart.Payment.Core.reloadActiveEmbeddedProvider();
        S.buy_step_swiper.update();
    }
    function PersonalInvoiceMode() {
        $(`#invoiceType .invoice-row`).addClass("d-none");
        switch (this.value) {
            case "mobile":
                $(`.invoice-row[data-personal="mobile"]`).removeClass("d-none");
                $(S.InvoicePersonalTypeForms).find("input").prop("required", true)
                $("#InvoiceForm").addClass("d-none");
                break;
            case "paper":
                $(`.invoice-row[data-personal="paper"]`).removeClass("d-none");
                $("#InvoiceForm").removeClass("d-none");
                $(S.InvoicePersonalTypeForms).find("input").prop("required", false)
                break;
        }
    }
    function InvoiceTypeRadio() {
        const value = $("[name='InvoiceType']:checked").val();
        $(`#invoiceType .invoice-block`).addClass("d-none");
        switch (value) {
            case "personal":
                $("#InvoiceInputPersonal").removeClass("d-none");
                $("#InvoiceForm").removeClass("d-none");
                break
            case "company":
                $("#Form_Invoice").removeClass("d-none");
                $("#InvoiceForm").removeClass("d-none");
                break
            case "none":
                $("#InvoiceInputNone").removeClass("d-none");
                $("#InvoiceForm").addClass("d-none");
                break
        }
    }
    function InvoiceFormClear() {
        S.$invoice_title.val("");
        S.$invoice_uniformid.val("");
        S.$invoice_address_city.val("");
        S.$invoice_address_town.val("");
        S.$invoice_address.val("");
    }
    function InvoiceFormSet(title, uniformid, address_city, address_town, address) {
        S.$invoice_title.val(title);
        S.$invoice_uniformid.val(uniformid);
        S.$Invoice_TWzipcode.twzipcode('set', {
            'county': address_city,
            'district': address_town,
        });
        S.$invoice_address.val(address);
    }
    function FormCheck(Forms) {
        return Array.from(Forms).every(form => {
            const valid = form.checkValidity();
            form.classList.add('was-validated');
            return valid;
        });
    }
    function DetailsClear() {
        $("#Step1 > .card-body").addClass("d-none");
        $("#Purchase_Null").removeClass("d-none");
        S.buy_step_swiper.disable();
        cart.Items.refreshHasProds();
    }
    function DeleteRecipient() {
        var $this_parent = $(this).parents("tr");
        $this_parent.remove();
    }
    function OrderDataGet() {
        var shipping_radio = $(`[name="RadioShipping"]:checked`);
        S.order_header_data.shipping = shipping_radio.val();
        S.order_header_data.CVSStoreID = shipping_radio.attr("data-cvsstoreid") ?? null;
        S.order_header_data.CVSStoreName = shipping_radio.attr("data-cvsstorename") ?? null;
        S.order_header_data.CVSAddress = shipping_radio.attr("data-cvsaddress") ?? null;
        S.order_header_data.CVSTelephone = shipping_radio.attr("data-cvstelephone") ?? null;
        S.order_header_data.CVSOutSide = shipping_radio.attr("data-cvsoutside") ?? null;

        var paymentValue = cart.Payment.Core.getActivePaymentValue();

        if (paymentValue == null || paymentValue === "") {
            delete S.order_header_data.payment;
        } else {
            S.order_header_data.payment = Number(paymentValue);
        }

        S.order_header_data.state = 1;
        S.order_header_data.subtotal = S.subtotal;
        S.order_header_data.discount = 0;
        S.order_header_data.bonus = 0;
        S.order_header_data.couponId = 0;
        S.order_header_data.freight = S.freight == "" ? 0 : S.freight;
        S.order_header_data.Service_Charge = 0;
        S.order_header_data.OrderDetails = cart.Items.getSelectedCartItems();

        S.order_header_data.SupportApplePay = true;
    }
    function OrdererDataGet() {
        S.order_data = co.Form.getJson($("#Form_Orderer").attr("id"));
        S.order_data.ordererSex = S.order_data.ordererSex
            ? Number(S.order_data.ordererSex)
            : null;

        var country = S.order_data.county ? `${S.order_data.county} ` : "";
        var district = S.order_data.district ? `${S.order_data.district} ` : "";

        S.order_data.ordererZipCode = S.order_data.zipcode ? `${S.order_data.zipcode}` : "";
        S.order_data.ordererAddress = `${country}${district}${S.order_data.ordererAddress}`;

        if (S.order_data.ordererTelePhone != "" && S.order_data.zone != "") {
            S.order_data.ordererTelePhone =
                `${S.order_data.zone}-${S.order_data.ordererTelePhone}` +
                (S.order_data.ext == "" ? "" : `-${S.order_data.ext}`);
        }

        for (var key in S.order_data) {
            if (key.startsWith("orderer") > 0) {
                S.order_header_data[key] = S.order_data[key];
            }
        }

        if (!cart.Forms.FormCheck(S.OrdererForms)) return false;

        return true;
    }
    function RecipientDataGet() {
        var checkform = false;

        switch ($(`[name="RecipientRadio"]:checked`).val()) {
            case "order":
                cart.Forms.RecipientSameOrderer();
                break;
            case "edit":
                S.recipient_data = co.Form.getJson($("#Form_Recipient").attr("id"));
                S.recipient_data.recipientSex = S.recipient_data.recipientSex
                    ? Number(S.recipient_data.recipientSex)
                    : null;
                var country = S.recipient_data.county ? `${S.recipient_data.county} ` : "";
                var district = S.recipient_data.district ? `${S.recipient_data.district} ` : "";
                S.recipient_data.recipientZipCode = S.recipient_data.zipcode ? `${S.recipient_data.zipcode}` : "";
                S.recipient_data.recipientAddress = `${country}${district}${S.recipient_data.recipientAddress}`;
                S.recipient_data.recipientTelePhone = "";
                if (S.recipient_data.recipientTelePhone != "" && S.recipient_data.zone != "") {
                    S.recipient_data.recipientTelePhone = `${S.recipient_data.zone}-${S.recipient_data.recipientTelePhone}` + (S.recipient_data.ext == "" ? "" : `-${S.recipient_data.ext}`);
                }
                checkform = true;
                break;
        }

        for (var key in S.recipient_data) {
            if (key.startsWith("recipient") > 0) S.order_header_data[key] = S.recipient_data[key]
        }

        if (checkform && !cart.Forms.FormCheck(S.RecipientForms)) return false;
        return true;
    }
    function InvoiceDataGet() {
        var checkform = false;
        switch ($(`[name="InvoiceType"]:checked`).val()) {
            case "personal":
                if (S.InvoicePersonalTypeForms.length && !cart.Forms.FormCheck(S.InvoicePersonalTypeForms)) return false;
                S.order_header_data.invoiceType = 1;
                switch ($(`[name="PersonalInvoiceMode"]:checked`).val()) {
                    case "paper":
                        S.order_header_data.PersonalInvoiceType = 1;
                        break;
                    case "mobile":
                        S.order_header_data.PersonalInvoiceType = 2;
                        S.invoiceType_data = co.Form.getJson("Form_InvoicePersonalType");
                        S.order_header_data.Carrier = S.invoiceType_data["MobileCarrier"];
                        break;
                }
                S.invoiceType_data.PersonalInvoiceType = S.order_header_data.PersonalInvoiceType;
                break;
            case "company":
                checkform = true
                S.invoice_data = co.Form.getJson($("#Form_Invoice").attr("id"));
                var country = S.invoice_data.county ? `${S.invoice_data.county} ` : "";
                var district = S.invoice_data.district ? `${S.invoice_data.district} ` : "";
                S.invoice_data.invoiceAddress = `${country}${district}${S.invoice_data.invoiceAddress}`;
                S.order_header_data.invoiceType = 2;
                break;
        }
        switch ($(`[name="InvoiceRadio"]:checked`).val()) {
            case "order":
                S.invoice_data['invoiceRecipient'] = 1;
                break;
            case "recipient":
                S.invoice_data['invoiceRecipient'] = 2;
                break;
        }
        for (var key in S.invoice_data) {
            S.order_header_data[key] = S.invoice_data[key]
        }
        if (checkform && !cart.Forms.FormCheck(S.InvoiceForms)) return false;
        return true;
    }
    function AllDataGet(EnableWarning) {
        var checksuccess = true;

        cart.Payment.Core.RadioPayment();
        cart.Forms.OrderDataGet();

        if (!cart.Forms.OrdererDataGet()) {
            checksuccess = false;
            if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
        }
        else {
            if (S.order_data.zone == "" ^ S.order_data.ordererTelePhone == "") {
                if (EnableWarning) Coker.sweet.warning("資料填寫錯誤", "如要提供訂購人電話資訊，請確實填寫區碼與聯絡電話。", null);
                checksuccess = false;
            }
        }

        if (!cart.Forms.RecipientDataGet()) {
            checksuccess = false;
            if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫收件人資料！", null);
        }
        else {
            if ($(`[name="RecipientRadio"]:checked`).val() == "edit") {
                if (S.recipient_data.zone == "" ^ S.recipient_data.recipientTelePhone == "") {
                    if (EnableWarning) Coker.sweet.warning("資料填寫錯誤", "如要提供收件人電話資訊，請確實填寫區碼與聯絡電話。", null);
                    checksuccess = false;
                }
            }
        }

        if (!cart.Forms.InvoiceDataGet()) {
            checksuccess = false;
            if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫發票資料！", null);
        }

        return checksuccess;
    }
    function AutoSwapInput() {
        var target = event.target

        if (target.nodeName == "INPUT" && target.className.indexOf("pro_quantity") < 0) {
            if (target.value.length == target.maxLength) {
                var elements = $(target).parents("form").first().find("input");
                for (let i = 0; i < elements.length; i++) {
                    if (elements[i] == target) {
                        if (elements[i + 1]) {
                            elements[i + 1].focus();
                        }
                        return;
                    }
                }
            }
        }
    }
    function TWZipCodeInit() {
        //$Orderer_TWzipcode.twzipcode({
        //    'zipcodeIntoDistrict': true,
        //    'countySel': '高雄市',
        //    'districtSel': '前鎮區'
        //});
        S.$Orderer_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });
        S.$Recipient_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });
        S.$Invoice_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });

        var $county, $district;

        $county = S.$Orderer_TWzipcode.children('.county');
        $district = S.$Orderer_TWzipcode.children('.district');

        $county.children('select').attr({
            id: "OrdererSelectCity",
            class: "orderer_city form-select",
            required: "required"
        });
        $county.append("<label class='px-4 required' for='OrdererSelectCity'>縣市</label>");
        var $county_first_option = $county.children('select').children('option').first();
        $county_first_option.text("請選擇縣市");
        $county_first_option.attr('disabled', 'disabled');

        $district.children('select').attr({
            id: "OrdererSelectTown",
            class: "orderer_town form-select",
            required: "required"
        });
        $district.append("<label class='px-4 required' for='OrdererSelectCity'>鄉鎮</label>");
        var $district_first_option = $district.children('select').children('option').first();
        $district_first_option.text("請選擇鄉鎮");
        $district_first_option.attr('disabled', 'disabled');

        $county = S.$Recipient_TWzipcode.children('.county');
        $district = S.$Recipient_TWzipcode.children('.district');

        $county.children('select').attr({
            id: "RecipientSelectCity",
            class: "recipient_city form-select",
            required: "required"
        });
        $county.append("<label class='px-4 required' for='RecipientSelectCity'>縣市</label>");
        var $county_first_option = $county.children('select').children('option').first();
        $county_first_option.text("請選擇縣市");
        $county_first_option.attr('disabled', 'disabled');

        $district.children('select').attr({
            id: "RecipientSelectTown",
            class: "recipient_town form-select",
            required: "required"
        });
        $district.append("<label class='px-4 required' for='RecipientSelectCity'>鄉鎮</label>");
        var $district_first_option = $district.children('select').children('option').first();
        $district_first_option.text("請選擇鄉鎮");
        $district_first_option.attr('disabled', 'disabled');


        $county = S.$Invoice_TWzipcode.children('.county');
        $district = S.$Invoice_TWzipcode.children('.district');

        $county.children('select').attr({
            id: "InvoiceSelectCity",
            class: "bill_city form-select"
        });
        $county.append("<label class='px-4' for='InvoiceSelectCity'>縣市</label>");
        var $county_first_option = $county.children('select').children('option').first();
        $county_first_option.text("請選擇縣市");
        $county_first_option.attr('disabled', 'disabled');

        $district.children('select').attr({
            id: "InvoiceSelectTown",
            class: "bill_town form-select"
        });
        $district.append("<label class='px-4' for='InvoiceSelectCity'>鄉鎮</label>");
        var $district_first_option = $district.children('select').children('option').first();
        $district_first_option.text("請選擇鄉鎮");
        $district_first_option.attr('disabled', 'disabled');
    }
    function RecipientsList_ContentReady(e) {
        S.RecipientsList_dxData = $("#RecipientsList").dxDataGrid("instance");
        console.log("RecipientsList_dxData", S.RecipientsList_dxData)
    }
    function RecipientsList_SelectChange(selectedItems) {
        var data = selectedItems.selectedRowsData;

        console.log("Select", data)
    }
    function RecipientsList_DeleteButtonClicked(e) {
        co.sweet.confirm("刪除收件人", "確定刪除？資料刪除後不可復原", "確　定", "取　消", function () {
            //co.Tag.TagDelete(e.row.key).done(function () {
            //    RecipientsList_dxData.refresh();
            //})
        })
    }

    Object.assign(cart.Forms, {
        ElementInit: ElementInit,
        OrdererEdit: OrdererEdit,
        RecipientRadio: RecipientRadio,
        RecipientFormClear: RecipientFormClear,
        RecipientSameOrderer: RecipientSameOrderer,
        InvoiceRadio: InvoiceRadio,
        PersonalInvoiceMode: PersonalInvoiceMode,
        InvoiceTypeRadio: InvoiceTypeRadio,
        InvoiceFormClear: InvoiceFormClear,
        InvoiceFormSet: InvoiceFormSet,
        FormCheck: FormCheck,
        DetailsClear: DetailsClear,
        DeleteRecipient: DeleteRecipient,
        OrderDataGet: OrderDataGet,
        OrdererDataGet: OrdererDataGet,
        RecipientDataGet: RecipientDataGet,
        InvoiceDataGet: InvoiceDataGet,
        AllDataGet: AllDataGet,
        AutoSwapInput: AutoSwapInput,
        TWZipCodeInit: TWZipCodeInit,
        RecipientsList_ContentReady: RecipientsList_ContentReady,
        RecipientsList_SelectChange: RecipientsList_SelectChange,
        RecipientsList_DeleteButtonClicked: RecipientsList_DeleteButtonClicked
    });
})(window.ShoppingCart, window.jQuery);
