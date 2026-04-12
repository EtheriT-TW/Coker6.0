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

        $setDefault: null,
        $title: null,
        $preserve: null,
        $shipping: null,
        $freightStatusType: null,
        $freight: null,
        $lowCon: null,
        $dFreight: null,
        $freightType: null,
        $inputProd: null,
        $inputLogisticsBox: null,
        $logisticsBoxSection: null,
        $logisticsPriceSection: null,
        $logisticsBoxSelectedList: null,

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
            this.$freightType = $("input[name='FreightType']");
            this.$inputProd = $(this.prodInputSelector);

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

            co.Order.GetFreightStatusTypeEnum().done(function (result) {
                self.$freightStatusType.empty();
                $(result).each(function () {
                    self.$freightStatusType.append($("<option>").attr({ value: this.id }).text(this.name));
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
            this.$freightStatusType.val(1);
            this.$freightType.filter("[value='1']").prop("checked", true);

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
        },

        fillForm: function (result) {
            const self = this;

            _c.Form.insertData(result, "#" + this.formId);
            this.$setDefault.prop("checked", !!result.set_Default);

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
            });
        },

        getCurrentFreightType: function () {
            const $checked = this.$freightType.filter(":checked").first();
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
            const type = parseInt(this.$freightStatusType.val() || 0, 10);

            if (type === 2) this.$inputProd.removeAttr("disabled");
            else this.$inputProd.attr("disabled", "disabled");
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
            const freightType = this.getCurrentFreightType();

            if (freightType === 3 && !this.validateLogisticsBoxFees()) {
                return $.Deferred().reject().promise();
            }

            const payload = _c.Form.getJson(this.formId);
            payload.Id = this.keyId;
            payload.ProdIds = this.getFreightProdIds();
            payload.LogisticsBoxFees = this.getFreightLogisticsBoxFeesData();

            if (freightType === 3) {
                payload.Freight = 0;
                payload.Low_Con = 0;
                payload.Dis_Freight = 0;
            } else {
                payload.LogisticsBoxFees = [];
            }

            const self = this;

            return co.Freight.AddUp(payload)
                .done(function () {
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