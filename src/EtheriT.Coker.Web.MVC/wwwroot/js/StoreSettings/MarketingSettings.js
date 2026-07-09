(function (window, $) {
    "use strict";

    const MarketingPage = {
        formId: "MarketingForm",
        pageRootSelector: "#MarketingPageRoot",

        hashPage: null,
        marketingListGridEvent: null,
        isGridLookupApplied: false,
        keyId: 0,
        isInitialized: false,
        options: null,

        $status: null,
        $ruleType: null,
        $neverEnd: null,
        $endTime: null,
        $endTimeSection: null,
        $rewardAmountSection: null,
        $rewardPercentSection: null,
        $discountAmount: null,
        $discountPercent: null,
        $maxDiscountAmount: null,
        $minAmount: null,
        $repeatable: null,
        $repeatableSection: null,
        ruleValueCache: {},
        lastRuleType:0,

        init: function () {
            if (this.isInitialized) return;
            this.isInitialized = true;

            this.cacheElements();
            this.initCommonForm();
            this.initHashPage();
            this.initStaticEvents();
            this.loadOptions();
        },

        cacheElements: function () {
            this.$status = $("#SelectStatus");
            this.$ruleType = $("input[name='RuleType']");
            this.$neverEnd = $("#CheckNeverEnd");
            this.$endTime = $("#InputEndTime");
            this.$endTimeSection = $(".endTimeSection");
            this.$rewardAmountSection = $(".rewardAmountSection");
            this.$rewardPercentSection = $(".rewardPercentSection");
            this.$discountAmount = $("#InputDiscountAmount");
            this.$discountPercent = $("#InputDiscountPercent");
            this.$maxDiscountAmount = $("#InputMaxDiscountAmount");
            this.$minAmount = $("#InputMinAmount");
            this.$repeatable = $("#CheckRepeatable");
            this.$repeatableSection = $(".repeatableSection");
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

            $(".btn_back").off("click.marketing").on("click.marketing", function (e) {
                e.preventDefault();

                Coker.sweet.confirm("返回行銷活動列表", "資料將不被保存", "確定", "取消", function () {
                    if (self.hashPage) self.hashPage.goList();
                    else window.location.hash = "List";
                });
            });

            $(".btn_add").off("click.marketing").on("click.marketing", function (e) {
                e.preventDefault();

                if (self.hashPage) self.hashPage.goNew();
            });

            this.$ruleType.off("change.marketing").on("change.marketing", function () {
                self.onRuleTypeChanged(true);
            });

            this.$neverEnd.off("change.marketing").on("change.marketing", function () {
                self.applyNeverEndUI();
            });

            this.$discountPercent.off("input.marketing").on("input.marketing", function () {
                let value = $(this).val();

                // 移除非數字，避免貼上小數點、負號、e 這類 number input 可能接受的字元
                value = String(value).replace(/[^\d]/g, "");

                // 最多兩位數
                if (value.length > 2) {
                    value = value.substring(0, 2);
                }

                $(this).val(value);
            });

            this.$discountPercent.off("blur.marketing").on("blur.marketing", function () {
                const normalizedValue = self.normalizeDiscountRateForView($(this).val());

                if (normalizedValue !== "") {
                    $(this).val(normalizedValue);
                }
            });

            this.$discountPercent.off("blur.marketing").on("blur.marketing", function () {
                const normalizedValue = self.normalizeDiscountRateForView($(this).val());

                if (normalizedValue !== "") {
                    $(this).val(normalizedValue);
                }
            });
        },

        loadOptions: function () {
            const self = this;

            return co.Marketing.GetOptions()
                .then(function (res) {
                    const data = res.object || res.Object || res;

                    self.options = self.normalizeOptions(data || {});
                    self.renderStatusOptions();
                    self.applyGridLookups();
                })
                .catch(function (err) {
                    self.handleRequestError(err, "讀取行銷活動選項失敗");
                });
        },

        normalizeOptions: function (options) {
            return {
                campaignTypes: options.campaignTypes || options.CampaignTypes || [],
                ruleTypes: options.ruleTypes || options.RuleTypes || [],
                displayStatuses: options.displayStatuses || options.DisplayStatuses || [],
                editableStatuses: options.editableStatuses || options.EditableStatuses || []
            };
        },

        normalizeLookupItems: function (items) {
            return (items || []).map(function (item) {
                return {
                    value: item.value ?? item.Value,
                    text: item.text ?? item.Text
                };
            });
        },

        renderStatusOptions: function () {
            const list = this.options ? this.options.editableStatuses : [];

            this.$status.empty();

            list.forEach(function (item) {
                $("<option>")
                    .attr("value", item.value ?? item.Value)
                    .text(item.text ?? item.Text)
                    .appendTo(this.$status);
            }, this);
        },

        applyGridLookups: function () {
            if (this.isGridLookupApplied) {
                return;
            }

            if (!this.marketingListGridEvent || !this.marketingListGridEvent.component || !this.options) {
                return;
            }

            const grid = this.marketingListGridEvent.component;

            grid.beginUpdate();

            try {
                grid.columnOption("CampaignType", "lookup", {
                    dataSource: this.normalizeLookupItems(this.options.campaignTypes),
                    valueExpr: "value",
                    displayExpr: "text"
                });

                grid.columnOption("RuleType", "lookup", {
                    dataSource: this.normalizeLookupItems(this.options.ruleTypes),
                    valueExpr: "value",
                    displayExpr: "text"
                });

                grid.columnOption("Status", "lookup", {
                    dataSource: this.normalizeLookupItems(this.options.displayStatuses),
                    valueExpr: "value",
                    displayExpr: "text"
                });

                this.isGridLookupApplied = true;
            }
            finally {
                grid.endUpdate();
            }
        },

        onEnterList: function () {
            this.clearFormState();
        },

        onEnterNew: function () {
            this.keyId = 0;
            this.clearFormState();

            $("#SelectStatus").val("0");
            $("#InputPriority").val("0");
            $("#RuleAmountDiscount").prop("checked", true);
            $("#CheckNeverEnd").prop("checked", false);
            $("#CheckCanStack").prop("checked", false);
            $("#CheckRepeatable").prop("checked", false);

            this.applyNeverEndUI();
            this.applyRuleTypeUI(false);
            this.resetRuleValueCache();
        },

        onEnterEdit: function (id) {
            const self = this;

            if (!id) {
                if (this.hashPage) this.hashPage.goList();
                return;
            }

            co.Marketing.Get(id)
                .then(function (res) {
                    const data = res.object || res.Object || res;

                    if (!data) {
                        Coker.sweet.error("錯誤", "讀取行銷活動資料失敗", null, true);
                        if (self.hashPage) self.hashPage.goList();
                        return;
                    }

                    self.keyId = data.id || data.Id || id;
                    self.fillForm(data);
                })
                .catch(function (err) {
                    self.handleRequestError(err, "讀取行銷活動資料失敗");

                    if (self.hashPage) self.hashPage.goList();
                });
        },

        clearFormState: function () {
            this.keyId = 0;
            _c.Form.clear(this.formId);

            $("#" + this.formId).removeClass("was-validated");
            $("input[name='Id']").val(0);

            this.$discountAmount.removeAttr("required").val("");
            this.$discountPercent.removeAttr("required").val("");
            this.$maxDiscountAmount.val("");

            this.applyNeverEndUI();
            this.applyRuleTypeUI(false);
            this.resetRuleValueCache();
        },

        fillForm: function (result) {
            const flat = this.flattenCampaign(result);

            _c.Form.insertData(flat, "#" + this.formId);
            $("input[name='Id']").val(this.keyId);

            this.applyNeverEndUI();
            this.applyRuleTypeUI(false);
            this.resetRuleValueCache();
        },

        flattenCampaign: function (result) {
            return {
                Id: result.id ?? result.Id ?? 0,
                Name: result.name ?? result.Name ?? "",
                Description: result.description ?? result.Description ?? "",
                CampaignType: result.campaignType ?? result.CampaignType ?? 0,
                Status: result.status ?? result.Status ?? 0,
                StartTime: this.toDateTimeLocal(result.startTime ?? result.StartTime),
                EndTime: this.toDateTimeLocal(result.endTime ?? result.EndTime),
                NeverEnd: result.neverEnd ?? result.NeverEnd ?? false,
                Priority: result.priority ?? result.Priority ?? 0,
                CanStack: result.canStack ?? result.CanStack ?? false,
                Repeatable: result.repeatable ?? result.Repeatable ?? false,

                RuleType: result.ruleType ?? result.RuleType ?? 1,
                MinAmount: result.minAmount ?? result.MinAmount ?? "",
                DiscountAmount: result.discountAmount ?? result.DiscountAmount ?? "",
                DiscountPercent: this.normalizeDiscountRateForView(result.discountPercent ?? result.DiscountPercent),
                MaxDiscountAmount: result.maxDiscountAmount ?? result.MaxDiscountAmount ?? ""
            };
        },

        toDateTimeLocal: function (value) {
            if (!value) return "";

            const date = new Date(value);
            if (isNaN(date.getTime())) return "";

            const pad = function (n) {
                return String(n).padStart(2, "0");
            };

            return date.getFullYear() + "-"
                + pad(date.getMonth() + 1) + "-"
                + pad(date.getDate()) + "T"
                + pad(date.getHours()) + ":"
                + pad(date.getMinutes());
        },

        normalizeDiscountRateForView: function (value) {
            if (value === null || value === undefined || value === "") return "";

            const percent = Number(value);

            if (!Number.isFinite(percent) || percent <= 0) return "";

            const intPercent = Math.floor(percent);

            // 10、20、30...90 顯示成 1、2、3...9
            // 85、95 這類八五折、九五折維持兩位數顯示
            if (intPercent >= 10 && intPercent <= 90 && intPercent % 10 === 0) {
                return intPercent / 10;
            }

            return intPercent;
        },

        normalizeDiscountPercentForSave: function (value) {
            if (value === null || value === undefined || value === "") return null;

            const rate = Number(value);

            if (!Number.isFinite(rate)) return null;

            const intRate = Math.floor(rate);

            // 1 ~ 9：使用者輸入幾折，轉成後端目前使用的百分比格式
            // 例如 9 => 90
            if (intRate >= 1 && intRate <= 9) {
                return intRate * 10;
            }

            // 10 ~ 99：視為後端格式，直接送出
            // 例如 90 => 90，85 => 85
            if (intRate >= 10 && intRate <= 99) {
                return intRate;
            }

            return null;
        },

        getCurrentRuleType: function () {
            const $checked = this.$ruleType.filter(":checked").first();

            return $checked.length ? parseInt($checked.val(), 10) : 0;
        },

        applyNeverEndUI: function () {
            if (this.$neverEnd.prop("checked")) {
                this.$endTime.val("").attr("disabled", "disabled").removeAttr("required");
                this.$endTimeSection.addClass("is-disabled");
                return;
            }

            this.$endTime.removeAttr("disabled");
            this.$endTimeSection.removeClass("is-disabled");
        },

        applyRuleTypeUI: function (clearValue) {
            const ruleType = this.getCurrentRuleType();

            this.$rewardAmountSection.addClass("d-none");
            this.$rewardPercentSection.addClass("d-none");

            this.$discountAmount.removeAttr("required");
            this.$discountPercent.removeAttr("required");

            // 預設顯示，固定金額折抵才有重複觸發的意義
            this.$repeatableSection.removeClass("d-none");

            if (clearValue) {
                this.$discountAmount.val("");
                this.$discountPercent.val("");
                this.$maxDiscountAmount.val("");
            }

            if (ruleType === 1) {
                this.$rewardAmountSection.removeClass("d-none");
                this.$discountAmount.attr("required", "required");
                return;
            }

            if (ruleType === 2) {
                this.$rewardPercentSection.removeClass("d-none");
                this.$discountPercent.attr("required", "required");

                // 百分比折扣沒有重複觸發的概念，避免送出 true 造成誤解
                this.$repeatable.prop("checked", false);
                this.$repeatableSection.addClass("d-none");
            }
        },

        onRuleTypeChanged: function () {
            // 切換前，先把目前類型已填入的值暫存起來
            this.stashRuleValues(this.lastRuleType);

            // 切換 UI 顯示（不清空，改由暫存/還原機制控制欄位值）
            this.applyRuleTypeUI(false);

            // 還原切換後類型先前填過的值
            const ruleType = this.getCurrentRuleType();
            this.restoreRuleValues(ruleType);

            this.lastRuleType = ruleType;
        },

        stashRuleValues: function (ruleType) {
            if (ruleType === 1) {
                this.ruleValueCache[1] = {
                    discountAmount: this.$discountAmount.val()
                };
            } else if (ruleType === 2) {
                this.ruleValueCache[2] = {
                    discountPercent: this.$discountPercent.val(),
                    maxDiscountAmount: this.$maxDiscountAmount.val()
                };
            }
        },

        restoreRuleValues: function (ruleType) {
            const cache = this.ruleValueCache[ruleType];

            if (ruleType === 1) {
                this.$discountAmount.val(cache ? (cache.discountAmount ?? "") : "");
            } else if (ruleType === 2) {
                this.$discountPercent.val(cache ? (cache.discountPercent ?? "") : "");
                this.$maxDiscountAmount.val(cache ? (cache.maxDiscountAmount ?? "") : "");
            }
        },

        resetRuleValueCache: function () {
            // 進入新增/編輯/列表時重置暫存，避免沿用上一筆的殘留值
            this.ruleValueCache = {};
            this.lastRuleType = this.getCurrentRuleType();
        },

        buildPayload: function () {
            const form = _c.Form.getJson(this.formId);

            const ruleType = parseInt(form.RuleType || 1, 10);
            const neverEnd = form.NeverEnd === true;

            return {
                Id: this.keyId,
                Name: form.Name,
                Description: form.Description || null,
                CampaignType: parseInt(form.CampaignType || 0, 10),
                Status: parseInt(form.Status || 0, 10),
                StartTime: form.StartTime || null,
                EndTime: neverEnd ? null : (form.EndTime || null),
                NeverEnd: neverEnd,
                Priority: parseInt(form.Priority || 0, 10),
                CanStack: form.CanStack === true,
                Repeatable: ruleType === 1 && form.Repeatable === true,

                RuleType: ruleType,
                MinAmount: Number(form.MinAmount || 0),
                DiscountAmount: ruleType === 1 ? Number(form.DiscountAmount || 0) : null,
                DiscountPercent: ruleType === 2 ? this.normalizeDiscountPercentForSave(form.DiscountPercent) : null,
                MaxDiscountAmount: ruleType === 2 && form.MaxDiscountAmount !== "" && form.MaxDiscountAmount != null
                    ? Number(form.MaxDiscountAmount || 0)
                    : null
            };
        },

        validatePayload: function (payload) {
            if (!payload.NeverEnd && payload.EndTime && payload.StartTime) {
                if (new Date(payload.EndTime) <= new Date(payload.StartTime)) {
                    Coker.sweet.error("錯誤", "結束時間必須晚於開始時間。", null, true);
                    return false;
                }
            }

            if (!payload.MinAmount || payload.MinAmount <= 0) {
                Coker.sweet.error("錯誤", "滿額門檻必須大於 0。", null, true);
                return false;
            }

            if (payload.RuleType === 1) {
                if (!payload.DiscountAmount || payload.DiscountAmount <= 0) {
                    Coker.sweet.error("錯誤", "折抵金額必須大於 0。", null, true);
                    return false;
                }

                if (payload.DiscountAmount > payload.MinAmount) {
                    Coker.sweet.error("錯誤", "折抵金額不可大於滿額門檻。", null, true);
                    return false;
                }
            }

            if (payload.RuleType === 2) {
                const inputValue = this.$discountPercent.val();
                const inputRate = Number(inputValue);

                if (!Number.isInteger(inputRate) || inputRate < 1 || inputRate > 99) {
                    Coker.sweet.error("錯誤", "折扣折數請輸入 1 到 99 的整數，例如 9 或 90 表示九折，85 表示八五折。", null, true);
                    return false;
                }

                if (!payload.DiscountPercent || payload.DiscountPercent <= 0 || payload.DiscountPercent >= 100) {
                    Coker.sweet.error("錯誤", "折扣折數格式錯誤。", null, true);
                    return false;
                }

                if (payload.MaxDiscountAmount != null && payload.MaxDiscountAmount < 0) {
                    Coker.sweet.error("錯誤", "最高折抵不可小於 0。", null, true);
                    return false;
                }
            }

            return true;
        },

        handleRequestError: function (err, defaultMessage) {
            const result = err && err.result ? err.result : {};
            const message =
                result.message ||
                result.Message ||
                err.message ||
                defaultMessage ||
                "操作失敗";

            Coker.sweet.error("錯誤", message, null, true);
        },

        reloadGrid: function () {
            if (!this.marketingListGridEvent || !this.marketingListGridEvent.component) {
                return;
            }

            const grid = this.marketingListGridEvent.component;
            const dataSource = grid.getDataSource();

            if (dataSource) {
                dataSource.reload();
                return;
            }

            grid.refresh();
        },

        submitForm: function () {
            const payload = this.buildPayload();

            if (!this.validatePayload(payload)) {
                return Promise.reject();
            }

            const self = this;

            return co.Marketing.AddUp(payload)
                .then(function (res) {
                    Coker.sweet.success((res && (res.message || res.Message)) || "行銷活動儲存成功", null, true);

                    setTimeout(function () {
                        if (self.hashPage) {
                            self.hashPage.goList();
                        }

                        self.reloadGrid();
                    }, 300);
                })
                .catch(function (err) {
                    self.handleRequestError(err, "儲存行銷活動發生錯誤");
                });
        },

        onGridContentReady: function (e) {
            this.marketingListGridEvent = e;
            this.applyGridLookups();
        },

        onEditClick: function (e) {
            if (this.hashPage) this.hashPage.goId(e.row.key);
        },

        onDeleteClick: function (e) {
            const self = this;

            Coker.sweet.confirm("刪除行銷活動", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Marketing.Delete(e.row.key)
                    .then(function (res) {
                        Coker.sweet.success((res && (res.message || res.Message)) || "刪除成功", null, true);
                        self.reloadGrid();
                    })
                    .catch(function (err) {
                        self.handleRequestError(err, "刪除行銷活動失敗");
                    });
            });
        }
    };

    function formatMarketingDiscountPercent(value) {
        const percent = Number(value || 0);

        if (!percent) return "";

        if (percent % 10 === 0) {
            return (percent / 10).toString();
        }

        return percent.toString();
    }

    window.MarketingSettingsPageReady = function () {
        MarketingPage.init();
    };

    window.PageReady = window.MarketingSettingsPageReady;

    window.marketingContentReady = function (e) {
        MarketingPage.onGridContentReady(e);
    };

    window.marketingEditButtonClicked = function (e) {
        MarketingPage.onEditClick(e);
    };

    window.marketingDeleteButtonClicked = function (e) {
        MarketingPage.onDeleteClick(e);
    };

    window.marketingRuleSummaryCellValue = function (rowData) {
        if (!rowData) return "";

        const ruleType = rowData.ruleType ?? rowData.RuleType;
        const minAmount = rowData.minAmount ?? rowData.MinAmount;
        const discountAmount = rowData.discountAmount ?? rowData.DiscountAmount;
        const discountPercent = rowData.discountPercent ?? rowData.DiscountPercent;

        const amountText = Number(minAmount || 0).toLocaleString();

        if (ruleType === 1) {
            return `滿 ${amountText} 折 ${Number(discountAmount || 0).toLocaleString()}`;
        }

        if (ruleType === 2) {
            return `滿 ${amountText} 打 ${formatMarketingDiscountPercent(discountPercent)} 折`;
        }

        return "尚未設定";
    };

})(window, window.jQuery);