(function (window, $) {
    "use strict";

    const FreightPage = {
        formId: "FreightForm",
        pageRootSelector: "#FreightPageRoot",
        prodInputSelector: "#InputProd",
        logisticsBoxInputSelector: "#InputLogisticsBox",
        logisticsBoxSectionSelector: ".logisticsBoxSection",
        logisticsPriceSectionSelector: ".logisticsPriceSection",
        logisticsBoxListSelector: "#LogisticsBoxSelectedList",

        hashPage: null,
        freightListGridEvent: null,
        keyId: 0,
        isInitialized: false,
        paymentRestrictions: null,
        paymentRestrictionsDraft: null,
        paymentRestrictionRequestVersion: 0,
        pendingShippingType: null,

        $setDefault: null,
        $title: null,
        $preserve: null,
        $shipping: null,
        $freightStatusType: null,
        $freight: null,
        $lowCon: null,
        $dFreight: null,
        $discountFreightType: null,
        $discountFreightGroupSection: null,
        $freightType: null,
        $inputProd: null,
        $inputLogisticsBox: null,
        $logisticsBoxSection: null,
        $logisticsPriceSection: null,
        $logisticsBoxSelectedList: null,
        $paymentRestrictionModal: null,
        $paymentRestrictionRows: null,
        $paymentRestrictionSummary: null,
        $paymentRestrictionModalSummary: null,
        $showAllPaymentTypes: null,

        init: function () {
            if (this.isInitialized) return;
            this.isInitialized = true;

            this.cacheElements();
            this.initCommonForm();
            this.initModalSelectors();
            this.initHashPage();
            this.initStaticEvents();
            this.loadEnums();
        },

        cacheElements: function () {
            this.$setDefault = $("#CheckDefault");
            this.$title = $("#InputName");
            this.$preserve = $("#SelectPreserve");
            this.$shipping = $("#SelectShipping");
            this.$freightStatusType = $("#SelectStatus");
            this.$freight = $("#InputFreight");
            this.$lowCon = $("#InputLowCon");
            this.$dFreight = $("#InputDfreight");
            this.$discountFreightType = $("#SelectDiscountFreightType");
            this.$discountFreightGroupSection = $(".discountFreightGroupSection");
            this.$freightType = $("input[name='FreightType']");
            this.$inputProd = $(this.prodInputSelector);

            this.$inputLogisticsBox = $(this.logisticsBoxInputSelector);
            this.$logisticsBoxSection = $(this.logisticsBoxSectionSelector);
            this.$logisticsPriceSection = $(this.logisticsPriceSectionSelector);
            this.$logisticsBoxSelectedList = $(this.logisticsBoxListSelector);
            this.$paymentRestrictionModal = $("#PaymentRestrictionModal");
            this.$paymentRestrictionRows = $("#PaymentRestrictionRows");
            this.$paymentRestrictionSummary = $("#PaymentRestrictionSummary");
            this.$paymentRestrictionModalSummary = $("#PaymentRestrictionModalSummary");
            this.$showAllPaymentTypes = $("#ShowAllPaymentTypes");
        },

        initCommonForm: function () {
            const self = this;

            _c.Form.init(this.formId, function () {
                return self.submitForm();
            });
        },

        initModalSelectors: function () {
            const self = this;

            if (window.ProdListModalApi) {
                window.ProdListModalApi.bind(this.prodInputSelector, { setAsDefault: true });
            }

            if (window.LogisticsBoxModalApi) {
                window.LogisticsBoxModalApi.bind(this.logisticsBoxInputSelector, { setAsDefault: true });

                window.LogisticsBoxModalApi.setAfterSaveCallback(function () {
                    self.renderLogisticsBoxUI();
                });
            }
        },

        initHashPage: function () {
            const self = this;

            this.hashPage = Coker.HashPage.create({
                root: this.pageRootSelector,
                defaultHash: "List",
                listHash: "List",
                newHash: "new",
                listPageKey: "List",
                contentPageKey: "Content",
                titleSelector: "[data-hash-title]",
                scrollTarget: "[data-hash-page='Content']",
                useStack: true,

                onList: function () {
                    self.onEnterList();
                },

                onNew: function () {
                    self.onEnterNew();
                },

                onEdit: function (state) {
                    self.onEnterEdit(state.id);
                }
            });
        },

        initStaticEvents: function () {
            const self = this;

            $(".btn_back").off("click.freight").on("click.freight", function (e) {
                e.preventDefault();

                Coker.sweet.confirm("返回運費列表", "資料將不被保存", "確定", "取消", function () {
                    if (self.hashPage) {
                        self.hashPage.goList();
                    } else {
                        window.location.hash = "List";
                    }
                });
            });

            $(".btn_add").off("click.freight").on("click.freight", function (e) {
                e.preventDefault();
                if (self.hashPage) self.hashPage.goNew();
            });

            this.$freightStatusType.off("change.freight").on("change.freight", function () {
                self.applyFreightStatusUI();
            });

            this.$freightType.off("change.freight").on("change.freight", function () {
                self.applyFreightTypeUI();
            });

            this.$discountFreightType.off("change.freight").on("change.freight", function () {
                self.applyDiscountFreightTypeUI();
            });

            this.$shipping.off("change.freight").on("change.freight", function () {
                self.pendingShippingType = Number($(this).val() || 0) || null;
                self.loadPaymentRestrictions(true);
            });

            this.$lowCon.off("blur.freight").on("blur.freight", function () {
                self.handleLowConRule();
            });

            this.$freight.off("blur.freight").on("blur.freight", function () {
                self.validateDiscountFreightRelation(false);
            });

            this.$dFreight.off("blur.freight").on("blur.freight", function () {
                self.validateDiscountFreightRelation(true);
            });

            this.$paymentRestrictionModal
                .off("show.bs.modal.freight")
                .on("show.bs.modal.freight", function () {
                    self.openPaymentRestrictionEditor();
                })
                .off("hidden.bs.modal.freight")
                .on("hidden.bs.modal.freight", function () {
                    self.paymentRestrictionsDraft = null;
                });

            this.$showAllPaymentTypes.off("change.freight").on("change.freight", function () {
                self.renderPaymentRestrictionRows();
            });

            this.$paymentRestrictionRows
                .off("change.freight", ".payment-custom-switch")
                .on("change.freight", ".payment-custom-switch", function () {
                    var row = self.getPaymentRestrictionDraftRow($(this).closest("tr").data("payment-id"));
                    if (!row) return;

                    row.isCustomized = $(this).is(":checked");

                    if (!row.isCustomized) {
                        row.isEnabled = row.defaultIsEnabled;
                        row.overrideMinAmount = null;
                        row.overrideMaxAmount = null;
                    }

                    self.renderPaymentRestrictionRows();
                })
                .off("change.freight", ".payment-enabled-switch")
                .on("change.freight", ".payment-enabled-switch", function () {
                    var row = self.getPaymentRestrictionDraftRow($(this).closest("tr").data("payment-id"));
                    if (row) row.isEnabled = $(this).is(":checked");
                })
                .off("input.freight", ".payment-amount-input")
                .on("input.freight", ".payment-amount-input", function () {
                    var $input = $(this);
                    var row = self.getPaymentRestrictionDraftRow($input.closest("tr").data("payment-id"));
                    if (!row) return;

                    var value = $input.val();
                    row[$input.data("field")] = value === "" ? null : Number(value);
                });

            $("#ApplyPaymentRestrictions").off("click.freight").on("click.freight", function () {
                self.applyPaymentRestrictionDraft();
            });
        },

        loadEnums: function () {
            const self = this;

            co.Order.GetPreserveTypeEnum().done(function (result) {
                self.$preserve.empty();
                $(result).each(function () {
                    self.$preserve.append($("<option>").attr({ value: this.value }).text(this.key));
                });
            });

            co.Order.GetShippingTypeEnum().done(function (result) {
                var selectedValue = self.pendingShippingType || self.$shipping.val();
                self.$shipping.empty();
                $(result).each(function () {
                    self.$shipping.append($("<option>").attr({ value: this.value }).text(this.key));
                });

                if (selectedValue) self.$shipping.val(selectedValue);
                self.pendingShippingType = Number(self.$shipping.val() || 0) || null;
                self.loadPaymentRestrictions(false);
            });

            co.Order.GetFreightStatusTypeEnum().done(function (result) {
                self.$freightStatusType.empty();
                $(result).each(function () {
                    self.$freightStatusType.append($("<option>").attr({ value: this.id }).text(this.name));
                });

                self.applyFreightStatusUI();
            });

            co.Order.GetDiscountFreightTypeEnum().done(function (result) {
                self.$discountFreightType.empty();
                self.$discountFreightType.append(
                    $("<option>").attr({ value: "" }).text("不套用折抵運費")
                );

                $(result).each(function () {
                    self.$discountFreightType.append(
                        $("<option>").attr({ value: this.value ?? this.id }).text(this.key ?? this.name)
                    );
                });

                self.applyDiscountFreightTypeUI();
            });
        },

        onEnterList: function () {
            this.clearFormState();
        },

        onEnterNew: function () {
            this.keyId = 0;
            this.clearFormState();

            this.$preserve.val(1);
            this.$freightStatusType.val(1);
            this.$freightType.filter("[value='1']").prop("checked", true);

            this.applyFreightTypeUI();
            this.applyFreightStatusUI();
            this.applyDiscountFreightTypeUI();

            this.loadPaymentRestrictions(false);
        },

        onEnterEdit: function (id) {
            const self = this;

            if (!id) {
                if (this.hashPage) this.hashPage.goList();
                return;
            }

            co.Freight.Get(id).done(function (result) {
                if (!result) {
                    if (self.hashPage) self.hashPage.goList();
                    return;
                }

                self.keyId = result.id;
                self.fillForm(result);
            });
        },

        clearFormState: function () {
            this.keyId = 0;

            _c.Form.clear(this.formId);

            this.$setDefault.prop("checked", false);
            this.$inputProd.attr("disabled", "disabled");
            this.paymentRestrictions = null;
            this.paymentRestrictionsDraft = null;
            this.paymentRestrictionRequestVersion++;
            this.pendingShippingType = null;
            this.$showAllPaymentTypes.prop("checked", false);
            this.$paymentRestrictionRows.empty();
            this.$paymentRestrictionSummary.text("載入付款方式設定中…");

            this.$discountFreightType.val("").attr("disabled", "disabled");
            this.$discountFreightGroupSection.addClass("d-none");
            this.$dFreight.val("").attr("disabled", "disabled").attr("placeholder", "請先選擇折抵方式");

            if (window.ProdListModalApi) {
                window.ProdListModalApi.setActiveTarget(this.prodInputSelector);
                window.ProdListModalApi.clear();
            }

            if (window.LogisticsBoxModalApi) {
                window.LogisticsBoxModalApi.setActiveTarget(this.logisticsBoxInputSelector);
                window.LogisticsBoxModalApi.clear();
            }

            this.clearLogisticsBoxUI();
            this.applyFreightTypeUI();
            this.applyFreightStatusUI();
            this.applyDiscountFreightTypeUI();
        },

        fillForm: function (result) {
            const self = this;

            _c.Form.insertData(result, "#" + this.formId);
            this.pendingShippingType = Number(result.logisticsType ?? result.LogisticsType ?? 0) || null;
            if (this.pendingShippingType) this.$shipping.val(this.pendingShippingType);
            this.$setDefault.prop("checked", !!result.set_Default);

            if (!result.discountFreightType && !result.DiscountFreightType) {
                this.$discountFreightType.val("");
            }

            if (window.ProdListModalApi) {
                window.ProdListModalApi.setActiveTarget(this.prodInputSelector);
            }

            if (window.LogisticsBoxModalApi) {
                window.LogisticsBoxModalApi.setActiveTarget(this.logisticsBoxInputSelector);
            }

            const prodPromise = this.setFreightProdIds(result.prodIds || []);
            const logisticsPromise = this.setFreightLogisticsBoxFeesData(result.logisticsBoxFees || []);

            Promise.all([prodPromise, logisticsPromise]).finally(function () {
                self.applyFreightTypeUI();
                self.applyFreightStatusUI();
                self.applyDiscountFreightTypeUI();
                self.handleLowConRule();
            });

            this.loadPaymentRestrictions(false);
        },

        clonePaymentRestrictions: function (rows) {
            return JSON.parse(JSON.stringify(rows || []));
        },

        normalizePaymentRestriction: function (row) {
            return {
                paymentTypeId: row.paymentTypeId ?? row.PaymentTypeId,
                paymentTypeTitle: row.paymentTypeTitle ?? row.PaymentTypeTitle ?? "",
                paymentTypeCode: row.paymentTypeCode ?? row.PaymentTypeCode ?? "",
                websitePaymentEnabled: !!(row.websitePaymentEnabled ?? row.WebsitePaymentEnabled),
                paymentTypeMinAmount: Number(row.paymentTypeMinAmount ?? row.PaymentTypeMinAmount ?? 0),
                paymentTypeMaxAmount: row.paymentTypeMaxAmount ?? row.PaymentTypeMaxAmount ?? null,
                defaultIsEnabled: !!(row.defaultIsEnabled ?? row.DefaultIsEnabled),
                defaultMinAmount: Number(row.defaultMinAmount ?? row.DefaultMinAmount ?? 0),
                defaultMaxAmount: row.defaultMaxAmount ?? row.DefaultMaxAmount ?? null,
                isCustomized: !!(row.isCustomized ?? row.IsCustomized),
                isEnabled: !!(row.isEnabled ?? row.IsEnabled),
                overrideMinAmount: row.overrideMinAmount ?? row.OverrideMinAmount ?? null,
                overrideMaxAmount: row.overrideMaxAmount ?? row.OverrideMaxAmount ?? null,
                effectiveMinAmount: Number(row.effectiveMinAmount ?? row.EffectiveMinAmount ?? 0),
                effectiveMaxAmount: row.effectiveMaxAmount ?? row.EffectiveMaxAmount ?? null
            };
        },

        loadPaymentRestrictions: function (preserveCurrent) {
            var shippingType = this.pendingShippingType || Number(this.$shipping.val() || 0);
            if (!shippingType) {
                this.paymentRestrictions = [];
                this.$paymentRestrictionSummary.text("請先選擇物流型別");
                return $.Deferred().resolve([]).promise();
            }

            var self = this;
            var currentRows = preserveCurrent ? this.clonePaymentRestrictions(this.paymentRestrictions) : [];
            var requestVersion = ++this.paymentRestrictionRequestVersion;

            this.$paymentRestrictionSummary.text("載入付款方式設定中…");

            return co.Freight.GetPaymentRestrictions(this.keyId, shippingType)
                .done(function (result) {
                    if (requestVersion !== self.paymentRestrictionRequestVersion) return;

                    var rows = (result || []).map(function (row) {
                        return self.normalizePaymentRestriction(row);
                    });

                    if (currentRows.length) {
                        rows.forEach(function (row) {
                            var current = currentRows.find(function (x) {
                                return Number(x.paymentTypeId) === Number(row.paymentTypeId);
                            });

                            if (!current) return;
                            row.isCustomized = current.isCustomized;
                            row.isEnabled = current.isCustomized ? current.isEnabled : row.defaultIsEnabled;
                            row.overrideMinAmount = current.isCustomized ? current.overrideMinAmount : null;
                            row.overrideMaxAmount = current.isCustomized ? current.overrideMaxAmount : null;
                            self.updatePaymentRestrictionEffectiveValues(row);
                        });
                    }

                    self.paymentRestrictions = rows;
                    self.updatePaymentRestrictionSummary();

                    if (self.$paymentRestrictionModal.hasClass("show")) {
                        self.paymentRestrictionsDraft = self.clonePaymentRestrictions(rows);
                        self.renderPaymentRestrictionRows();
                    }
                })
                .fail(function () {
                    if (requestVersion !== self.paymentRestrictionRequestVersion) return;
                    self.$paymentRestrictionSummary.text("付款方式設定載入失敗");
                    self.$paymentRestrictionRows.html(
                        '<tr><td colspan="6" class="text-center text-danger py-4">無法載入付款方式設定</td></tr>'
                    );
                });
        },

        openPaymentRestrictionEditor: function () {
            if (this.paymentRestrictions == null) {
                this.$paymentRestrictionRows.html(
                    '<tr><td colspan="6" class="text-center text-muted py-4">載入中…</td></tr>'
                );
                this.loadPaymentRestrictions(false);
                return;
            }

            this.paymentRestrictionsDraft = this.clonePaymentRestrictions(this.paymentRestrictions);
            this.renderPaymentRestrictionRows();
        },

        getPaymentRestrictionDraftRow: function (paymentTypeId) {
            return (this.paymentRestrictionsDraft || []).find(function (row) {
                return Number(row.paymentTypeId) === Number(paymentTypeId);
            });
        },

        escapeHtml: function (value) {
            return $("<div>").text(value == null ? "" : String(value)).html();
        },

        formatPaymentAmount: function (value, noLimitText) {
            if (value == null) return noLimitText || "無上限";
            return "NT$ " + Number(value).toLocaleString("zh-TW");
        },

        formatPaymentRange: function (minAmount, maxAmount) {
            return this.formatPaymentAmount(minAmount, "NT$ 0")
                + " ～ "
                + this.formatPaymentAmount(maxAmount, "無上限");
        },

        renderPaymentRestrictionRows: function () {
            var self = this;
            var showAll = this.$showAllPaymentTypes.is(":checked");
            var rows = (this.paymentRestrictionsDraft || []).filter(function (row) {
                return showAll || row.websitePaymentEnabled;
            });

            if (!rows.length) {
                this.$paymentRestrictionRows.html(
                    '<tr><td colspan="6" class="text-center text-muted py-4">本站目前沒有啟用的付款方式，可開啟「顯示本站未啟用的付款方式」查看全部。</td></tr>'
                );
            } else {
                this.$paymentRestrictionRows.html(rows.map(function (row) {
                    var inactiveClass = row.websitePaymentEnabled ? "" : " payment-restriction-inactive";
                    var paymentStatus = row.websitePaymentEnabled
                        ? '<span class="badge bg-success ms-1">本站啟用</span>'
                        : '<span class="badge bg-secondary ms-1">本站未啟用</span>';
                    var defaultStatus = row.defaultIsEnabled
                        ? '<span class="badge bg-success">允許</span>'
                        : '<span class="badge bg-danger">停用</span>';
                    var disabled = row.isCustomized ? "" : " disabled";
                    var maxPlaceholder = row.paymentTypeMaxAmount == null
                        ? "沿用付款設定：無上限"
                        : "沿用付款設定：" + Number(row.paymentTypeMaxAmount).toLocaleString("zh-TW");

                    return `
                        <tr data-payment-id="${row.paymentTypeId}" class="${inactiveClass}">
                            <td>
                                <div class="fw-semibold">${self.escapeHtml(row.paymentTypeTitle)}</div>
                                <div class="small text-muted">${self.escapeHtml(row.paymentTypeCode)} ${paymentStatus}</div>
                            </td>
                            <td>
                                <div class="payment-restriction-default">${defaultStatus}</div>
                                <div class="small text-muted">${self.formatPaymentRange(row.defaultMinAmount, row.defaultMaxAmount)}</div>
                            </td>
                            <td class="text-center">
                                <div class="form-check form-switch d-inline-flex">
                                    <input class="form-check-input payment-custom-switch"
                                           type="checkbox"
                                           ${row.isCustomized ? "checked" : ""}>
                                </div>
                                <div class="small text-muted">${row.isCustomized ? "使用自訂" : "沿用預設"}</div>
                            </td>
                            <td class="text-center">
                                <div class="form-check form-switch d-inline-flex">
                                    <input class="form-check-input payment-enabled-switch"
                                           type="checkbox"
                                           ${row.isEnabled ? "checked" : ""}
                                           ${disabled}>
                                </div>
                            </td>
                            <td>
                                <input class="form-control form-control-sm payment-amount-input"
                                       type="number"
                                       min="0"
                                       step="0.01"
                                       data-field="overrideMinAmount"
                                       value="${row.overrideMinAmount ?? ""}"
                                       placeholder="沿用付款設定：${Number(row.paymentTypeMinAmount).toLocaleString("zh-TW")}"
                                       ${disabled}>
                            </td>
                            <td>
                                <input class="form-control form-control-sm payment-amount-input"
                                       type="number"
                                       min="0"
                                       step="0.01"
                                       data-field="overrideMaxAmount"
                                       value="${row.overrideMaxAmount ?? ""}"
                                       placeholder="${maxPlaceholder}"
                                       ${disabled}>
                            </td>
                        </tr>`;
                }).join(""));
            }

            var enabledCount = (this.paymentRestrictionsDraft || []).filter(function (row) {
                return row.websitePaymentEnabled;
            }).length;
            var hiddenCount = (this.paymentRestrictionsDraft || []).length - enabledCount;
            this.$paymentRestrictionModalSummary.text(
                "本站已啟用 " + enabledCount + " 項"
                + (hiddenCount ? "，另有 " + hiddenCount + " 項未啟用" : "")
            );
        },

        updatePaymentRestrictionEffectiveValues: function (row) {
            if (row.isCustomized) {
                row.effectiveMinAmount = row.overrideMinAmount ?? row.paymentTypeMinAmount;
                row.effectiveMaxAmount = row.overrideMaxAmount ?? row.paymentTypeMaxAmount;
            } else {
                row.isEnabled = row.defaultIsEnabled;
                row.effectiveMinAmount = row.defaultMinAmount;
                row.effectiveMaxAmount = row.defaultMaxAmount;
            }
        },

        validatePaymentRestrictionDraft: function () {
            var warnings = [];

            for (var row of (this.paymentRestrictionsDraft || [])) {
                if (!row.isCustomized) continue;

                this.updatePaymentRestrictionEffectiveValues(row);

                if (row.overrideMinAmount != null && row.overrideMinAmount < 0
                    || row.overrideMaxAmount != null && row.overrideMaxAmount < 0) {
                    Coker.sweet.error("錯誤", "付款金額限制不可小於 0。", null, true);
                    return null;
                }

                if (row.effectiveMaxAmount != null
                    && row.effectiveMinAmount > row.effectiveMaxAmount) {
                    Coker.sweet.error(
                        "錯誤",
                        row.paymentTypeTitle + "的最低金額不可大於最高金額。",
                        null,
                        true
                    );
                    return null;
                }

                if (row.overrideMinAmount != null
                    && row.overrideMinAmount < row.paymentTypeMinAmount) {
                    warnings.push(row.paymentTypeTitle + "最低金額低於付款方式預設");
                }

                if (row.overrideMaxAmount != null
                    && (row.paymentTypeMaxAmount == null
                        ? false
                        : row.overrideMaxAmount > row.paymentTypeMaxAmount)) {
                    warnings.push(row.paymentTypeTitle + "最高金額高於付款方式預設");
                }
            }

            return warnings;
        },

        applyPaymentRestrictionDraft: function () {
            var self = this;
            var warnings = this.validatePaymentRestrictionDraft();
            if (warnings == null) return;

            var commit = function () {
                self.paymentRestrictions = self.clonePaymentRestrictions(self.paymentRestrictionsDraft);
                self.updatePaymentRestrictionSummary();
                bootstrap.Modal
                    .getOrCreateInstance(document.getElementById("PaymentRestrictionModal"))
                    .hide();
            };

            if (warnings.length) {
                Coker.sweet.confirm(
                    "金額超出付款方式預設",
                    warnings.join("；") + "。請確認合約額度，仍要套用嗎？",
                    "仍要套用",
                    "返回調整",
                    commit
                );
                return;
            }

            commit();
        },

        updatePaymentRestrictionSummary: function () {
            var rows = this.paymentRestrictions || [];
            var websiteRows = rows.filter(function (row) {
                return row.websitePaymentEnabled;
            });
            var enabledCount = websiteRows.filter(function (row) {
                return row.isEnabled;
            }).length;
            var customizedCount = rows.filter(function (row) {
                return row.isCustomized;
            }).length;

            this.$paymentRestrictionSummary.text(
                "前台付款方式可用 " + enabledCount + " / " + websiteRows.length + " 項"
                + (customizedCount ? "，已自訂 " + customizedCount + " 項" : "，目前全部沿用系統預設")
            );
        },

        getPaymentRestrictionsPayload: function () {
            if (this.paymentRestrictions == null) return null;

            return this.paymentRestrictions.map(function (row) {
                return {
                    PaymentTypeId: row.paymentTypeId,
                    IsCustomized: row.isCustomized,
                    IsEnabled: row.isEnabled,
                    OverrideMinAmount: row.isCustomized ? row.overrideMinAmount : null,
                    OverrideMaxAmount: row.isCustomized ? row.overrideMaxAmount : null
                };
            });
        },

        getCurrentFreightType: function () {
            const $checked = this.$freightType.filter(":checked").first();
            return $checked.length ? parseInt($checked.val(), 10) : 0;
        },

        getDiscountFreightTypeValue: function () {
            const raw = this.$discountFreightType.val();
            return raw === "" || raw == null ? null : parseInt(raw, 10);
        },

        applyDiscountFreightTypeUI: function () {
            const type = this.getCurrentFreightType();
            const discountType = this.getDiscountFreightTypeValue();
            const isSinglePricing = type === 2;
            const isBoxPricing = type === 3;

            if (isSinglePricing || isBoxPricing) {
                this.$discountFreightGroupSection.removeClass("d-none");
                this.$discountFreightType.removeAttr("disabled");

                if (discountType != null) {
                    this.$dFreight.removeAttr("disabled");

                    if (discountType === 1) {
                        this.$dFreight.attr("placeholder", "請輸入達門檻後的最終運費");
                    } else if (discountType === 2) {
                        this.$dFreight.attr("placeholder", "請輸入要折抵的運費金額");
                    } else {
                        this.$dFreight.attr("placeholder", "");
                    }
                } else {
                    this.$dFreight.val("").attr("disabled", "disabled").attr("placeholder", "不套用折抵運費");
                }
                return;
            }

            this.$discountFreightGroupSection.addClass("d-none");
            this.$discountFreightType.val("").attr("disabled", "disabled");
            this.$dFreight.val("").attr("disabled", "disabled").attr("placeholder", "請先選擇折抵方式");
        },

        applyFreightTypeUI: function () {
            const type = this.getCurrentFreightType();
            const isBoxPricing = type === 3;
            const isSinglePricing = type === 2;

            if (isBoxPricing) {
                this.$logisticsBoxSection.removeClass("d-none");
                this.$logisticsPriceSection.removeClass("d-none");

                this.$freight.val("").attr("disabled", "disabled");
                this.$lowCon.removeAttr("disabled");

                this.applyDiscountFreightTypeUI();
                this.handleLowConRule();
                return;
            }

            if (isSinglePricing) {
                this.$logisticsBoxSection.addClass("d-none");
                this.$logisticsPriceSection.removeClass("d-none");

                this.$freight.removeAttr("disabled");
                this.$lowCon.removeAttr("disabled");

                this.applyDiscountFreightTypeUI();
                this.handleLowConRule();
                return;
            }

            this.$logisticsBoxSection.addClass("d-none");
            this.$discountFreightGroupSection.addClass("d-none");
            this.$discountFreightType.val("").attr("disabled", "disabled");
            this.$logisticsPriceSection.removeClass("d-none");

            this.$freight.val("").attr("disabled", "disabled");
            this.$lowCon.val("").attr("disabled", "disabled");
            this.$dFreight.val("").attr("disabled", "disabled").attr("placeholder", "");
        },

        applyFreightStatusUI: function () {
            const type = parseInt(this.$freightStatusType.val() || 0, 10);

            if (type === 2) this.$inputProd.removeAttr("disabled");
            else this.$inputProd.attr("disabled", "disabled");
        },

        handleLowConRule: function () {
            const freightType = this.getCurrentFreightType();
            const lowCon = Number(this.$lowCon.val() || 0);
            const discountType = this.getDiscountFreightTypeValue();

            if (freightType !== 2 && freightType !== 3) return;

            if (lowCon <= 0) {
                this.$discountFreightType.val("");
                this.$dFreight.val("");
                this.$dFreight.attr("disabled", "disabled");
                this.applyDiscountFreightTypeUI();
                return;
            }

            if (discountType == null) {
                this.$discountFreightType.val("1");
            }

            this.applyDiscountFreightTypeUI();
        },

        normalizeDiscountFreightPayload: function (payload) {
            const freightType = this.getCurrentFreightType();
            const lowCon = Number(payload.Low_Con || 0);
            const disFreight = Number(payload.Dis_Freight || 0);

            if (disFreight < 0) payload.Dis_Freight = 0;

            if (freightType !== 2 && freightType !== 3) {
                payload.DiscountFreightType = null;
                payload.Dis_Freight = 0;
                return payload;
            }

            if (lowCon <= 0) {
                payload.DiscountFreightType = null;
                payload.Dis_Freight = 0;
                return payload;
            }

            if (!payload.DiscountFreightType) {
                payload.DiscountFreightType = null;
                payload.Dis_Freight = 0;
                return payload;
            }

            if (Number(payload.DiscountFreightType) === 2 && Number(payload.Dis_Freight || 0) === 0) {
                payload.DiscountFreightType = null;
                payload.Dis_Freight = 0;
                return payload;
            }

            return payload;
        },

        validateDiscountFreightRelation: function (showMessage) {
            const freightType = this.getCurrentFreightType();
            const freight = Number(this.$freight.val() || 0);
            const disFreight = Number(this.$dFreight.val() || 0);
            const discountType = this.getDiscountFreightTypeValue();

            this.$freight.removeClass("is-invalid");
            this.$dFreight.removeClass("is-invalid");

            if (freightType !== 2 && freightType !== 3) {
                return true;
            }

            if (disFreight < 0) {
                this.$dFreight.addClass("is-invalid");
                if (showMessage) {
                    Coker.sweet.error("錯誤", "折抵運費不可小於 0。", null, true);
                }
                return false;
            }

            if (freightType === 2 && discountType === 2 && disFreight > freight) {
                this.$freight.addClass("is-invalid");
                this.$dFreight.addClass("is-invalid");
                if (showMessage) {
                    Coker.sweet.error("錯誤", "單筆計算時，折抵固定運費不可大於單筆運費。", null, true);
                }
                return false;
            }

            return true;
        },

        renderLogisticsBoxUI: function () {
            const container = this.$logisticsBoxSelectedList[0];
            if (!container) return;

            if (window.LogisticsBoxModalApi) {
                window.LogisticsBoxModalApi.setActiveTarget(this.logisticsBoxInputSelector);
            }

            const currentState = this.getLogisticsBoxState();
            const items = (currentState.items || []).filter(function (x) {
                return !x.IsDeleted;
            });

            container.innerHTML = "";

            if (!items.length) {
                container.innerHTML = '<div class="text-muted small">尚未選擇箱型</div>';
                return;
            }

            const self = this;

            items.forEach(function (item) {
                const row = document.createElement("div");
                row.className = "d-flex align-items-center gap-3 mb-2 logistics-box-row";
                row.dataset.id = item.FK_LogisticsBoxId;

                row.innerHTML = `
                    <button type="button" class="btn btn-link p-0 text-danger remove-btn">
                        <span class="material-symbols-outlined">close</span>
                    </button>

                    <div class="flex-grow-1 logistics-box-name fw-semibold">
                        ${item.Name || "未命名箱型"}
                    </div>

                    <div class="d-flex align-items-center gap-2 logistics-box-price">
                        <span class="text-muted small">運費</span>
                        <div class="input-group input-group-sm">
                            <span class="input-group-text">NT$</span>
                            <input type="number"
                                   name="Fee"
                                   class="form-control logistics-box-fee-input"
                                   value="${item.Fee ?? ""}"
                                   placeholder="0" />
                        </div>
                    </div>
                `;

                const $input = $(row).find(".logistics-box-fee-input");
                _c.Form.bindNumberFormatter($input);

                $input.off("blur.freightFee").on("blur.freightFee", function () {
                    const val = _c.Form.normalizeElementValue($input, $input.val());
                    item.Fee = val || 0;

                    if (!item.Fee || item.Fee <= 0) {
                        $input.addClass("is-invalid");
                    } else {
                        $input.removeClass("is-invalid");
                    }
                });

                const removeBtn = row.querySelector(".remove-btn");
                removeBtn.addEventListener("click", function () {
                    item.IsDeleted = true;
                    self.renderLogisticsBoxUI();
                });

                container.appendChild(row);
            });
        },

        clearLogisticsBoxUI: function () {
            this.$logisticsBoxSelectedList.empty();
        },

        getProdState: function () {
            if (!window.ProdListModalApi) {
                return { items: [], selectedKeys: [], selectedRows: [], text: "無" };
            }

            window.ProdListModalApi.setActiveTarget(this.prodInputSelector);
            return window.ProdListModalApi.getState();
        },

        getLogisticsBoxState: function () {
            if (!window.LogisticsBoxModalApi) {
                return { items: [], selectedKeys: [], selectedRows: [], text: "無" };
            }

            window.LogisticsBoxModalApi.setActiveTarget(this.logisticsBoxInputSelector);
            return window.LogisticsBoxModalApi.getState();
        },

        getFreightProdIds: function () {
            const state = this.getProdState();

            return (state.items || [])
                .filter(function (x) { return !x.IsDeleted; })
                .map(function (x) {
                    return {
                        Id: x.Id || 0,
                        FK_ProdId: x.FK_ProdId || 0,
                        IsDeleted: x.IsDeleted === true
                    };
                });
        },

        setFreightProdIds: function (value) {
            if (!window.ProdListModalApi) return Promise.resolve();

            window.ProdListModalApi.setActiveTarget(this.prodInputSelector);
            return window.ProdListModalApi.setData(value || []);
        },

        getFreightLogisticsBoxFeesData: function () {
            const state = this.getLogisticsBoxState();

            return (state.items || [])
                .filter(function (x) { return !x.IsDeleted; })
                .map(function (x) {
                    return {
                        Id: x.Id || 0,
                        FK_LogisticsBoxId: x.FK_LogisticsBoxId || 0,
                        Fee: x.Fee || 0
                    };
                });
        },

        setFreightLogisticsBoxFeesData: function (value) {
            if (!window.LogisticsBoxModalApi) return Promise.resolve();

            const rows = (value || []).map(function (x) {
                return {
                    Id: x.id ?? x.Id ?? 0,
                    FK_LogisticsBoxId: x.fK_LogisticsBoxId ?? x.FK_LogisticsBoxId ?? 0,
                    Name: x.logisticsBox_Name ?? x.LogisticsBox_Name ?? x.name ?? x.Name ?? "",
                    Fee: x.fee ?? x.Fee ?? 0,
                    IsDeleted: x.IsDeleted === true
                };
            });

            const self = this;

            window.LogisticsBoxModalApi.setActiveTarget(this.logisticsBoxInputSelector);
            return window.LogisticsBoxModalApi.setData(rows).then(function () {
                self.renderLogisticsBoxUI();
            });
        },

        validateLogisticsBoxFees: function () {
            const data = this.getFreightLogisticsBoxFeesData();

            if (!data.length) {
                Coker.sweet.error("錯誤", "請至少選擇一個箱型。", null, true);
                return false;
            }

            const invalid = data.find(function (x) {
                return !x.Fee || Number(x.Fee) <= 0;
            });

            if (invalid) {
                Coker.sweet.error("錯誤", "箱型運費不可為 0。", null, true);
                return false;
            }

            return true;
        },

        submitForm: function () {
            if (this.paymentRestrictions == null) {
                var page = this;
                return this.loadPaymentRestrictions(false).then(function () {
                    return page.submitForm();
                });
            }

            const freightType = this.getCurrentFreightType();

            if (freightType === 3 && !this.validateLogisticsBoxFees()) {
                return $.Deferred().reject().promise();
            }

            this.handleLowConRule();

            if (!this.validateDiscountFreightRelation(true)) {
                return $.Deferred().reject().promise();
            }

            let payload = _c.Form.getJson(this.formId);
            payload.Id = this.keyId;
            payload.ProdIds = this.getFreightProdIds();
            payload.LogisticsBoxFees = this.getFreightLogisticsBoxFeesData();
            payload.PaymentRestrictions = this.getPaymentRestrictionsPayload();
            payload = this.normalizeDiscountFreightPayload(payload);

            if (freightType === 3) {
                payload.Freight = 0;

                if (!payload.DiscountFreightType) {
                    payload.DiscountFreightType = null;
                    payload.Dis_Freight = 0;
                }
            } else {
                payload.LogisticsBoxFees = [];

                if (!payload.DiscountFreightType) {
                    payload.DiscountFreightType = null;
                }
            }

            const self = this;

            return co.Freight.AddUp(payload)
                .done(function (result) {
                    if (!result || result.success === false || result.Success === false) {
                        Coker.sweet.error(
                            "錯誤",
                            result?.error || result?.Error || "儲存運費設定發生錯誤",
                            null,
                            true
                        );
                        return;
                    }

                    Coker.sweet.success("運費設定儲存成功", null, true);

                    setTimeout(function () {
                        if (self.hashPage) self.hashPage.goList();
                        if (self.freightListGridEvent && self.freightListGridEvent.component) {
                            self.freightListGridEvent.component.refresh();
                        }
                    }, 300);
                })
                .fail(function () {
                    Coker.sweet.error("錯誤", "儲存運費設定發生錯誤", null, true);
                });
        },

        onGridContentReady: function (e) {
            this.freightListGridEvent = e;
        },

        onEditClick: function (e) {
            if (this.hashPage) this.hashPage.goId(e.row.key);
        },

        onDeleteClick: function (e) {
            Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Freight.Delete(e.row.key).done(function () {
                    Coker.sweet.success("刪除成功", null, true);
                    e.component.refresh();
                });
            });
        }
    };

    window.FreightSettingsPageReady = function () {
        FreightPage.init();
    };

    window.PageReady = window.FreightSettingsPageReady;

    window.contentReady = function (e) {
        FreightPage.onGridContentReady(e);
    };

    window.editButtonClicked = function (e) {
        FreightPage.onEditClick(e);
    };

    window.deleteButtonClicked = function (e) {
        FreightPage.onDeleteClick(e);
    };

})(window, window.jQuery);
