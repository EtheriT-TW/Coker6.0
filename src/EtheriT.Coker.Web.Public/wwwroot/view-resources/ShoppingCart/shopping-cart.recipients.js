// Saved recipient + delivery profile selection for signed-in checkout users.
(function (cart, $) {
    "use strict";

    var S = cart.State;
    var modalReturnFocus = "#btn_recipient_choose";
    cart.Recipients = cart.Recipients || {};

    function GetValue(data, pascalName, camelName) {
        if (!data) return null;
        return data[pascalName] != null ? data[pascalName] : data[camelName];
    }

    function Normalize(data) {
        return {
            recipientId: Number(GetValue(data, "Id", "id") || GetValue(data, "RecipientId", "recipientId") || 0),
            recipient: GetValue(data, "Name", "name") || GetValue(data, "Recipient", "recipient") || "",
            recipientSex: GetValue(data, "Sex", "sex") ?? GetValue(data, "RecipientSex", "recipientSex"),
            recipientEmail: GetValue(data, "Email", "email") || GetValue(data, "RecipientEmail", "recipientEmail") || "",
            recipientCellPhone: GetValue(data, "CellPhone", "cellPhone") || GetValue(data, "RecipientCellPhone", "recipientCellPhone") || "",
            recipientTelePhone: GetValue(data, "TelePhone", "telePhone") || GetValue(data, "RecipientTelePhone", "recipientTelePhone") || "",
            recipientZipCode: GetValue(data, "ZipCode", "zipCode") || GetValue(data, "RecipientZipCode", "recipientZipCode") || "",
            recipientAddress: GetValue(data, "Address", "address") || GetValue(data, "RecipientAddress", "recipientAddress") || "",
            logisticsType: Number(GetValue(data, "LogisticsType", "logisticsType") || 0),
            cvsStoreID: GetValue(data, "CVSStoreID", "cvsStoreID") || "",
            cvsStoreName: GetValue(data, "CVSStoreName", "cvsStoreName") || "",
            cvsAddress: GetValue(data, "CVSAddress", "cvsAddress") || "",
            cvsTelephone: GetValue(data, "CVSTelephone", "cvsTelephone") || "",
            cvsOutSide: GetValue(data, "CVSOutSide", "cvsOutSide") || ""
        };
    }

    function IsCvsType(logisticsType) {
        return [3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15].includes(Number(logisticsType));
    }

    function LogisticsLabel(recipient) {
        if (!recipient.logisticsType) return "尚未指定配送方式";
        if (IsCvsType(recipient.logisticsType)) {
            return recipient.cvsStoreName ? "超商取貨・" + recipient.cvsStoreName : "超商取貨";
        }
        return "宅配／貨運";
    }

    function GetSelectedLogisticsType() {
        return Number($("[name='RadioShipping']:checked").attr("data-logistics-type") || 0);
    }

    function FilterBySelectedShipping(items) {
        var logisticsType = GetSelectedLogisticsType();
        if (!logisticsType) return items;

        return items.filter(function (item) {
            return Normalize(item).logisticsType === logisticsType;
        });
    }

    function SetSourceSelected(value) {
        $(".recipient-source-option").removeClass("is-selected");

        if (value === "choose") {
            $("#btn_recipient_choose").addClass("is-selected");
        } else {
            $("[name='RecipientRadio'][value='" + value + "']")
                .closest(".recipient-source-option")
                .addClass("is-selected");
        }

        ArrangeActivePanel(value);
    }

    function GetSourceOption(value) {
        if (value === "choose") return $("#btn_recipient_choose");
        return $("[name='RecipientRadio'][value='" + value + "']").closest(".recipient-source-option");
    }

    function ArrangeActivePanel(value) {
        var $panel = $("#RecipientActivePanel");
        if (!$panel.length || !GetSourceOption(value).length) return;

        $panel.appendTo(".recipient-source-options");
        $panel.append($("#RecipientForm > .default_data, #RecipientActivePanel > .default_data"));
        $panel.append($(".recipient-copy-actions"));
        $panel.append($("#Form_Recipient"));
        $panel.append($("#ShippingSection"));
    }

    function PrepareActivePanel() {
        var value = $("[name='RecipientRadio']:checked").val() || "order";
        ArrangeActivePanel(value);
    }

    function SetCopyActionVisibility() {
        var source = $("[name='RecipientRadio']:checked").val();
        $(".recipient-copy-actions").toggleClass("d-none", source === "edit");
    }

    function ApplyShipping(recipient) {
        var $currentShipping = $("[name='RadioShipping']:checked");
        var currentLogisticsType = Number($currentShipping.attr("data-logistics-type") || 0);
        var $shipping = $currentShipping;

        // 常用清單已依目前配送方式過濾，套用收件人時應保留使用者目前的選擇。
        if (!$shipping.length || currentLogisticsType !== recipient.logisticsType) {
            var $matches = $("[name='RadioShipping'][data-logistics-type='" + recipient.logisticsType + "']");
            $shipping = $matches.filter(function () {
                return !$(this).closest(".shipping-option-row").hasClass("d-none");
            }).first();
            if (!$shipping.length) $shipping = $matches.first();
        }

        if (!$shipping.length) return false;

        $shipping.prop("checked", true);

        if (IsCvsType(recipient.logisticsType)) {
            $shipping.attr({
                "data-cvsstoreid": recipient.cvsStoreID,
                "data-cvsstorename": recipient.cvsStoreName,
                "data-cvsaddress": recipient.cvsAddress,
                "data-cvstelephone": recipient.cvsTelephone,
                "data-cvsoutside": recipient.cvsOutSide
            });
        }

        $shipping.trigger("change");
        cart.Shipping.UpdateCvsStoreSelectionDisplay();
        return true;
    }

    function RefreshDisplay() {
        var source = $("[name='RecipientRadio']:checked").val();
        if (source === "edit") return;

        var recipient = $.extend({}, S.recipient_data || {});
        if (!recipient.recipient) return;

        var $shipping = $("[name='RadioShipping']:checked");
        var isCvs = String($shipping.attr("data-is-cvs") || "").toLowerCase() === "true";
        var storeName = $.trim($shipping.attr("data-cvsstorename") || "");
        var storeAddress = $.trim($shipping.attr("data-cvsaddress") || "");

        if (isCvs) {
            recipient.recipientAddress = storeName
                ? storeName + (storeAddress ? "（" + storeAddress + "）" : "")
                : "尚未選擇取貨門市";
            $(".recipient-display-address-label").text("取貨門市");
        } else {
            $(".recipient-display-address-label").text("收件地址");
        }

        cart.Utils.ShoppingCartDataInsert(recipient, $("#RecipientActivePanel > .default_data"));
        $("#RecipientActivePanel > .default_data").removeClass("d-none");
    }

    function Apply(data, closeModal) {
        var recipient = Normalize(data);
        if (!recipient.recipientId || !recipient.recipient || !recipient.recipientCellPhone) return false;

        S.recipient_data = recipient;
        S.selectedRecipientId = recipient.recipientId;
        S.RecipientOpen = false;
        S.RecipientFilled = true;

        $("#radio_recipient_choose").prop("checked", true);
        $("#Form_Recipient").addClass("d-none");
        $("#RecipientActivePanel > .default_data").removeClass("d-none");
        SetSourceSelected("choose");
        SetCopyActionVisibility();
        ApplyShipping(recipient);
        RefreshDisplay();

        if (closeModal !== false && window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(document.getElementById("RecipientModal")).hide();
        }

        cart.Shipping.UpdateRecipientAddressRequirement();
        cart.Payment.Core.onAmountChanged();
        cart.Payment.Core.reloadActiveEmbeddedProvider();
        if (S.buy_step_swiper) S.buy_step_swiper.update();
        return true;
    }

    function CreateCard(data) {
        var recipient = Normalize(data);
        var address = IsCvsType(recipient.logisticsType)
            ? (recipient.cvsAddress || "尚未設定取貨門市")
            : (recipient.recipientAddress || "尚未設定宅配地址");
        var $details = $("<div>", { class: "recipient-card-details" });

        $("<div>", { class: "recipient-card-heading" })
            .append($("<strong>", { text: recipient.recipient }))
            .append($("<span>", { class: "recipient-logistics-badge", text: LogisticsLabel(recipient) }))
            .appendTo($details);
        $("<div>", { class: "small text-muted", text: recipient.recipientCellPhone }).appendTo($details);
        $("<div>", { class: "recipient-card-address small mt-1", text: address }).appendTo($details);

        var $button = $("<button>", {
            type: "button",
            class: "recipient-use-button align-self-center",
            text: "使用這筆資料"
        }).on("click", function () {
            Apply(data, true);
        });

        return $("<div>", { class: "recipient-card" }).append($details).append($button);
    }

    function Render(items) {
        var logisticsType = GetSelectedLogisticsType();
        items = FilterBySelectedShipping(items || []);
        var $list = $("#RecipientsCardList").empty();
        $("#RecipientsListLoading").addClass("d-none");
        $("#RecipientsListError").addClass("d-none");
        $("#RecipientsListEmpty").toggleClass("d-none", items.length > 0);
        $("#RecipientsListEmpty .recipient-empty-title").text(
            logisticsType ? "這個配送方式還沒有常用收件資訊" : "目前沒有常用收件資訊"
        );
        $("#RecipientsListEmpty .recipient-empty-description").text(
            logisticsType
                ? "可新增一筆符合目前配送方式的收件資訊。"
                : "先新增收件人與配送方式，下次結帳就能直接選用。"
        );

        items.forEach(function (item) {
            $list.append(CreateCard(item));
        });
    }

    function ShowLoadError(xhr) {
        var message = xhr && xhr.status === 401
            ? "登入狀態已失效，請重新登入後再讀取。"
            : "請重新讀取；也可以直接新增一筆收件資訊。";

        $("#RecipientsListLoading").addClass("d-none");
        $("#RecipientsListError").removeClass("d-none")
            .find(".recipient-list-error-detail").text(message);
    }

    function Load() {
        $("#RecipientsListLoading").removeClass("d-none");
        $("#RecipientsListEmpty, #RecipientsListError").addClass("d-none");
        $("#RecipientsCardList").empty();

        return $.ajax({
            url: "/api/Recipients/GetCheckoutList",
            method: "GET",
            dataType: "json"
        }).done(function (result) {
            Render(Array.isArray(result) ? result : []);
        }).fail(ShowLoadError);
    }

    function SetAddressFields(recipient) {
        var fullAddress = recipient.recipientAddress || "";
        co.Form.insertData(recipient, "#Form_Recipient");
        co.Zipcode.setData({
            el: $("#Recipient_TWzipcode"),
            addr: fullAddress
        });

        var parts = fullAddress.split(" ").filter(Boolean);
        $("#RecipientInputAddress").val(parts.length >= 3 ? parts.slice(2).join(" ") : fullAddress);
    }

    function CopyCurrentRecipient() {
        var recipient = $.extend({}, S.recipient_data || {});
        if (!recipient.recipient || !recipient.recipientCellPhone) return;

        S.selectedRecipientId = null;
        $("#radio_recipient_edit").prop("checked", true).trigger("change");
        SetAddressFields(recipient);
        cart.Shipping.UpdateRecipientAddressRequirement();

        var input = document.getElementById("RecipientInputName");
        if (input) input.focus();
    }

    function SelectNewRecipient() {
        modalReturnFocus = "#RecipientInputName";
        $("#radio_recipient_edit").prop("checked", true).trigger("change");
        setTimeout(function () {
            var panel = document.getElementById("RecipientActivePanel");
            if (panel) panel.scrollIntoView({ behavior: "smooth", block: "nearest" });
        }, 200);
    }

    function Init() {
        PrepareActivePanel();
        $("#RecipientModal")
            .on("show.bs.modal", function () {
                modalReturnFocus = "#btn_recipient_choose";
                Load();
            })
            .on("hide.bs.modal", function () {
                var activeElement = document.activeElement;
                if (activeElement && this.contains(activeElement)) activeElement.blur();
            })
            .on("hidden.bs.modal", function () {
                var target = document.querySelector(modalReturnFocus);
                if (target && target.offsetParent !== null) target.focus();
            });
        $(document).on("click", ".btn_recipient_retry", Load);
        $(document).on("click", ".btn_recipient_new", SelectNewRecipient);
        $(document).on("click", "#btn_copy_recipient", CopyCurrentRecipient);

        $(document).on("change", "[name='RecipientRadio']", function () {
            if (this.value !== "choose") {
                S.selectedRecipientId = null;
                SetSourceSelected(this.value);
            }
            SetCopyActionVisibility();
        });

        SetCopyActionVisibility();
    }

    Object.assign(cart.Recipients, {
        Init: Init,
        Load: Load,
        Apply: Apply,
        RefreshDisplay: RefreshDisplay,
        SetSourceSelected: SetSourceSelected
    });
})(window.ShoppingCart, window.jQuery);
