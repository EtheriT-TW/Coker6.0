(function (window, $) {
    "use strict";

    const FreightPage = {
        formId: "FreightForm",
        pageRootSelector: "#FreightPageRoot",
        logisticsBoxInputSelector: "#InputLogisticsBox",
        logisticsBoxSectionSelector: ".logisticsBoxSection",
        logisticsPriceSectionSelector: ".logisticsPriceSection",
        logisticsBoxListSelector: "#LogisticsBoxSelectedList",

        hashPage: null,
        freightListGridEvent: null,
        keyId: 0,

        $setDefault: null,
        $title: null,
        $preserve: null,
        $shipping: null,
        $freigntStatusType: null,
        $freight: null,
        $lowCon: null,
        $dFreight: null,
        $freigntType: null,
        $inputProd: null,
        $inputLogisticsBox: null,
        $logisticsBoxSection: null,
        $logisticsPriceSection: null,
        $logisticsBoxSelectedList: null,

        init: function () {
            this.cacheElements();
            this.initCommonForm();
            this.initHashPage();
            this.initStaticEvents();
            this.initModalSelectors();
            this.loadEnums();
        },

        cacheElements: function () {
            this.$setDefault = $("#CheckDefault");
            this.$title = $("#InputName");
            this.$preserve = $("#SelectPreserve");
            this.$shipping = $("#SelectShipping");
            this.$freigntStatusType = $("#SelectStatus");
            this.$freight = $("#InputFreight");
            this.$lowCon = $("#InputLowCon");
            this.$dFreight = $("#InputDfreight");
            this.$freigntType = $("input[name='FreigntType']");
            this.$inputProd = $("#InputProd");

            this.$inputLogisticsBox = $(this.logisticsBoxInputSelector);
            this.$logisticsBoxSection = $(this.logisticsBoxSectionSelector);
            this.$logisticsPriceSection = $(this.logisticsPriceSectionSelector);
            this.$logisticsBoxSelectedList = $(this.logisticsBoxListSelector);
        },

        initCommonForm: function () {
            const self = this;
            _c.Form.init(this.formId, function () {
                return self.submitForm();
            });
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
                    if (self.hashPage) self.hashPage.back();
                    else history.back();
                });
            });

            $(".btn_add").off("click.freight").on("click.freight", function (e) {
                e.preventDefault();
                if (self.hashPage) self.hashPage.goNew();
            });

            this.$freigntStatusType.off("change.freight").on("change.freight", function () {
                self.applyFreightStatusUI();
            });

            this.$freigntType.off("change.freight").on("change.freight", function () {
                self.applyFreightTypeUI();
            });
        },

        initModalSelectors: function () {
            const self = this;

            if (typeof prodListModalInit === "function") {
                prodListModalInit();
            }

            if (window.LogisticsBoxModalApi) {
                LogisticsBoxModalApi.bind(document.querySelector(this.logisticsBoxInputSelector));

                if (typeof LogisticsBoxModalApi.setAfterSaveCallback === "function") {
                    LogisticsBoxModalApi.setAfterSaveCallback(function (target, state) {
                        self.renderLogisticsBoxUI(target, state);
                    });
                }
            }
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
                self.$shipping.empty();
                $(result).each(function () {
                    self.$shipping.append($("<option>").attr({ value: this.value }).text(this.key));
                });
            });

            co.Order.GetFreigntStatusTypEnum().done(function (result) {
                self.$freigntStatusType.empty();
                $(result).each(function () {
                    self.$freigntStatusType.append($("<option>").attr({ value: this.id }).text(this.name));
                });
                self.applyFreightStatusUI();
            });
        },

        onEnterList: function () {
            this.clearFormState();
        },

        onEnterNew: function () {
            this.keyId = 0;
            this.clearFormState();

            this.$preserve.val(1);
            this.$freigntStatusType.val(1);
            this.$freigntType.filter("[value='1']").prop("checked", true);

            this.applyFreightTypeUI();
            this.applyFreightStatusUI();
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

            if (typeof ProdDataClear === "function") {
                ProdDataClear();
            }

            if (window.LogisticsBoxModalApi) {
                LogisticsBoxModalApi.clear(document.querySelector(this.logisticsBoxInputSelector));
            }

            this.clearLogisticsBoxUI();
            this.applyFreightTypeUI();
            this.applyFreightStatusUI();
        },

        fillForm: function (result) {
            _c.Form.insertData(result, "#" + this.formId);

            this.$setDefault.prop("checked", !!result.set_Default);

            if (typeof window.setFreightProdIds === "function") {
                window.setFreightProdIds(result.prodIds || []);
            }

            if (typeof window.setFreightLogisticsBoxFeesData === "function") {
                window.setFreightLogisticsBoxFeesData(result.logisticsBoxFees || [], this.$logisticsBoxSelectedList, result);
            }

            this.applyFreightTypeUI();
            this.applyFreightStatusUI();
        },

        getCurrentFreightType: function () {
            const $checked = this.$freigntType.filter(":checked").first();
            return $checked.length ? parseInt($checked.val(), 10) : 0;
        },

        applyFreightTypeUI: function () {
            const type = this.getCurrentFreightType();
            const isBoxPricing = type === 3;

            if (isBoxPricing) {
                this.$logisticsBoxSection.removeClass("d-none");
                this.$logisticsPriceSection.addClass("d-none");

                this.$freight.val("").attr("disabled", "disabled");
                this.$lowCon.val("").attr("disabled", "disabled");
                this.$dFreight.val("").attr("disabled", "disabled");
                return;
            }

            this.$logisticsBoxSection.addClass("d-none");
            this.$logisticsPriceSection.removeClass("d-none");

            switch (type) {
                case 2:
                    this.$freight.removeAttr("disabled");
                    this.$lowCon.removeAttr("disabled");
                    this.$dFreight.removeAttr("disabled");
                    break;
                default:
                    this.$freight.val("").attr("disabled", "disabled");
                    this.$lowCon.val("").attr("disabled", "disabled");
                    this.$dFreight.val("").attr("disabled", "disabled");
                    break;
            }
        },

        applyFreightStatusUI: function () {
            const type = parseInt(this.$freigntStatusType.val() || 0, 10);

            if (type === 2) this.$inputProd.removeAttr("disabled");
            else this.$inputProd.attr("disabled", "disabled");
        },

        renderLogisticsBoxUI: function (target, state) {
            const container = this.$logisticsBoxSelectedList[0];
            if (!container) return;

            const currentState = state || LogisticsBoxModalApi.getState(target);
            const items = (currentState.items || []).filter(x => !x.IsDeleted);

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
                                   value="${item.Price ?? ""}"
                                   placeholder="0" />
                        </div>
                    </div>
                `;

                const $input = $(row).find(".logistics-box-fee-input");
                _c.Form.bindNumberFormatter($input);

                $input.off("blur.freightFee").on("blur.freightFee", function () {
                    const val = _c.Form.normalizeElementValue($input, $input.val());
                    item.Price = val || 0;

                    if (!item.Price || item.Price <= 0) {
                        $input.addClass("is-invalid");
                    } else {
                        $input.removeClass("is-invalid");
                    }
                });

                const removeBtn = row.querySelector(".remove-btn");
                removeBtn.addEventListener("click", function () {
                    item.IsDeleted = true;
                    self.renderLogisticsBoxUI(target, currentState);
                });

                container.appendChild(row);
            });
        },

        clearLogisticsBoxUI: function () {
            this.$logisticsBoxSelectedList.empty();
        },

        validateLogisticsBoxFees: function () {
            const data = window.getFreightLogisticsBoxFeesData
                ? window.getFreightLogisticsBoxFeesData(this.$logisticsBoxSelectedList)
                : [];

            if (!data.length) {
                Coker.sweet.error("錯誤", "請至少選擇一個箱型。", null, true);
                return false;
            }

            const invalid = data.find(x => !x.Fee || Number(x.Fee) <= 0);
            if (invalid) {
                Coker.sweet.error("錯誤", "箱型運費不可為 0。", null, true);
                return false;
            }

            return true;
        },

        submitForm: function () {
            const freightType = this.getCurrentFreightType();

            if (freightType === 3 && !this.validateLogisticsBoxFees()) {
                return $.Deferred().reject().promise();
            }

            const payload = _c.Form.getJson(this.formId);
            payload.Id = this.keyId;
            payload.ProdIds = typeof prod_list !== "undefined" ? prod_list : [];
            if (!Array.isArray(payload.LogisticsBoxFees)) {
                payload.LogisticsBoxFees = window.getFreightLogisticsBoxFeesData
                    ? window.getFreightLogisticsBoxFeesData()
                    : [];
            }

            if (freightType === 3) {
                payload.Freight = 0;
                payload.Low_Con = 0;
                payload.Dis_Freight = 0;
            } else {
                payload.LogisticsBoxFees = [];
            }

            return co.Freight.AddUp(payload)
                .done(() => {
                    Coker.sweet.success("運費設定儲存成功", null, true);

                    setTimeout(() => {
                        if (this.hashPage) this.hashPage.goList();
                        if (this.freightListGridEvent && this.freightListGridEvent.component) {
                            this.freightListGridEvent.component.refresh();
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

    window.getFreightProdIds = function () {
        return typeof prod_list !== "undefined" ? prod_list : [];
    };

    window.setFreightProdIds = function (value) {
        if (typeof ProdDataSet === "function") {
            ProdDataSet(value || []);
        }
    };

    window.getFreightLogisticsBoxFeesData = function () {
        const target = document.querySelector(FreightPage.logisticsBoxInputSelector);
        if (!window.LogisticsBoxModalApi || !target) return [];

        const state = LogisticsBoxModalApi.getState(target);

        return (state.items || [])
            .filter(x => !x.IsDeleted)
            .map(function (x) {
                return {
                    Id: x.Id || 0,
                    FK_LogisticsBoxId: x.FK_LogisticsBoxId,
                    Fee: x.Price || 0
                };
            });
    };

    window.setFreightLogisticsBoxFeesData = function (value) {
        if (!window.LogisticsBoxModalApi) return;

        const target = document.querySelector(FreightPage.logisticsBoxInputSelector);
        const rows = (value || []).map(function (x) {
            return {
                Id: x.id || 0,
                FK_LogisticsBoxId: x.fK_LogisticsBoxId,
                Name: x.logisticsBox_Name || x.name || "",
                Price: x.fee || 0,
                IsDeleted: false
            };
        });

        LogisticsBoxModalApi.setData(target, rows).then(function () {
            FreightPage.renderLogisticsBoxUI(target);
        });
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