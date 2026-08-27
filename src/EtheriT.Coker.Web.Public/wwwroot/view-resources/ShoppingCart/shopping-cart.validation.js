// Checkout data validation shared by every payment method.
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.CheckoutValidation = cart.CheckoutValidation || {};

    function GetFieldDisplayName($field) {
        var id = $field.attr("id");
        var label = id ? $('label[for="' + id + '"]').first().text() : "";

        if (!label) {
            label = $field.closest(".form-floating").find("label").first().text();
        }

        label = $.trim(label).replace(/[　：:*]/g, "");
        return label || $field.attr("placeholder") || $field.attr("name") || "必填欄位";
    }

    function AddInvalidFields(issues, selector, section) {
        // 舊版 jQuery/Sizzle 不支援 :invalid，改用瀏覽器原生 Constraint Validation API。
        $(selector).find(":input").each(function () {
            if (typeof this.checkValidity !== "function" || this.checkValidity()) return;

            issues.push({
                section: section,
                field: GetFieldDisplayName($(this)),
                target: this
            });
        });
    }

    function AddPhonePairIssue(issues, areaSelector, phoneSelector, section) {
        var $area = $(areaSelector);
        var $phone = $(phoneSelector);
        var hasArea = $.trim($area.val() || "") !== "";
        var hasPhone = $.trim($phone.val() || "") !== "";

        if (hasArea === hasPhone) return;

        var $missingField = hasArea ? $phone : $area;
        issues.push({
            section: section,
            field: GetFieldDisplayName($missingField),
            target: $missingField[0]
        });
    }

    function GetIssues() {
        var issues = [];

        if (!cart.Shipping.HasSelectedCvsStore()) {
            issues.push({
                code: "CvsStoreMissing",
                section: "運送方式",
                field: "取貨門市",
                target: cart.Shipping.GetCvsStoreSelectionTarget()
            });
        }

        AddInvalidFields(issues, "#Form_Orderer", "訂購人");
        AddPhonePairIssue(issues, "#OrdererInputTelPhoneArea", "#OrdererInputTelPhone", "訂購人");

        if ($('[name="RecipientRadio"]:checked').val() === "edit") {
            AddInvalidFields(issues, "#Form_Recipient", "收件人");
            AddPhonePairIssue(issues, "#RecipientInputTelPhoneArea", "#RecipientInputTelPhone", "收件人");
        }

        var invoiceType = $('[name="InvoiceType"]:checked').val();
        if (invoiceType === "company") {
            AddInvalidFields(issues, "#Form_Invoice", "發票");
        } else if (invoiceType === "personal" && $('[name="PersonalInvoiceMode"]:checked').val() === "mobile") {
            AddInvalidFields(issues, "#Form_InvoicePersonalType", "發票");
        }

        return issues.filter(function (issue, index, list) {
            return list.findIndex(function (item) {
                return item.target === issue.target;
            }) === index;
        });
    }

    function FocusIssue(target) {
        if (!target) return;

        var $target = $(target);
        if ($target.closest("#Form_Orderer").length && !S.OrdererOpen) {
            cart.Forms.OrdererEdit(true, true);
        }

        var menuHeight = $("#Mega_Menu").outerHeight() || 0;
        var top = Math.max(0, $target.offset().top - menuHeight - 30);
        $("html, body").stop(true).animate({ scrollTop: top }, 300, function () {
            $target.trigger("focus");
        });
    }

    function ShowWarning(issues, hasOrderDetails) {
        var $warning = $(".checkoutValidationWarning");
        $warning.empty().removeClass("d-none");

        if (!hasOrderDetails) {
            $warning.text("請先選擇要結帳的商品，才能選擇付款方式");
            return;
        }

        if (issues.length === 0) {
            $warning.text("請先完整填寫訂購、運送、收件與發票資料，才能選擇付款方式");
            return;
        }

        var names = issues.map(function (issue) {
            return issue.section + "－" + issue.field;
        });

        $("<span>").text("尚未完成：" + names.join("、") + "。").appendTo($warning);
        $("<button>", {
            type: "button",
            class: "btn btn-link text-danger text-decoration-underline p-0 ms-2 align-baseline",
            text: "前往填寫"
        }).on("click", function () {
            FocusIssue(issues[0] && issues[0].target);
        }).appendTo($warning);
    }

    function GetSelectedOrderDetails() {
        return cart.Items.getSelectedCartItems().filter(function (item) {
            return Number(item.Id || 0) > 0 && Number(item.Quantity || 0) > 0;
        });
    }

    function RefreshDisplay() {
        var details = GetSelectedOrderDetails();
        var dataReady = cart.Forms.AllDataGet(false, true);
        var issues = GetIssues();
        var paymentBlockingIssues = issues.filter(function (issue) {
            return issue.code !== "CvsStoreMissing";
        });
        var canChoosePayment = dataReady && details.length > 0 && paymentBlockingIssues.length === 0;
        var isValid = canChoosePayment && issues.length === 0;

        if (isValid) {
            $(".checkoutValidationWarning").addClass("d-none");
            return true;
        }

        if (canChoosePayment) {
            // 取貨門市是付款方式之後的下一個動作，不在使用者選金流時提前顯示錯誤。
            $(".checkoutValidationWarning").addClass("d-none").empty();
            return false;
        }

        $("#RadioPayment > .form-check").addClass("d-none");
        $(".noPaymentWarning, .ecpay_loading").addClass("d-none");
        $("#ECPayPayment").empty();
        ShowWarning(issues, details.length > 0);

        if (S.buy_step_swiper) S.buy_step_swiper.update();
        return false;
    }

    Object.assign(cart.CheckoutValidation, {
        GetIssues: GetIssues,
        RefreshDisplay: RefreshDisplay
    });
})(window.ShoppingCart, window.jQuery);
