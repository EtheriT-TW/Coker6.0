(function (window) {
    "use strict";

    if (!window.ModuleSelectManager) {
        throw new Error("請先載入 ModuleSelect.js");
    }

    let logisticsBoxModalInstance = null;

    function ensureElement(target) {
        if (!target) return null;

        if (typeof target === "string") {
            return document.querySelector(target);
        }

        if (target instanceof Element) {
            return target;
        }

        if (window.jQuery && target instanceof window.jQuery && target.length > 0) {
            return target[0];
        }

        if (target[0] instanceof Element) {
            return target[0];
        }

        return null;
    }

    function createInstance() {
        return window.ModuleSelectManager.create({
            name: "LogisticsBoxModal",
            modalSelector: "#LogisticsBoxModal",
            gridSelector: "#devLogisticsBoxListModalGrid",
            saveButtonSelector: ".btn_logisticsBox_save",

            getRowKey: function (row) {
                return row?.Id ?? row?.id ?? null;
            },

            getRowText: function (row) {
                return row?.Name ?? row?.name ?? "";
            },

            getStoredKey: function (item) {
                return item?.FK_LogisticsBoxId
                    ?? item?.fK_LogisticsBoxId
                    ?? item?.Id
                    ?? item?.id
                    ?? null;
            },

            createStoredItem: function (row) {
                return {
                    Id: 0,
                    FK_LogisticsBoxId: row?.Id ?? row?.id ?? 0,
                    Name: row?.Name ?? row?.name ?? "",
                    IsDeleted: false
                };
            },

            mapInitData: function (data) {
                return {
                    Id: data?.id ?? data?.Id ?? 0,
                    FK_LogisticsBoxId:
                        data?.fK_LogisticsBoxId
                        ?? data?.FK_LogisticsBoxId
                        ?? data?.Id
                        ?? data?.id
                        ?? 0,
                    Name:
                        data?.logisticsBox_Name
                        ?? data?.name
                        ?? data?.Name
                        ?? "",
                    IsDeleted: data?.IsDeleted === true
                };
            },

            buildDisplayText: function (rows) {
                if (!rows || !rows.length) return "無";

                return rows
                    .map(function (x) { return x.Name; })
                    .filter(Boolean)
                    .join("、");
            },

            onAfterSave: function () {
                if (typeof window.LogisticsBoxContentRefresh === "function") {
                    window.LogisticsBoxContentRefresh();
                }
            }
        });
    }

    function getInstance() {
        if (logisticsBoxModalInstance) {
            return logisticsBoxModalInstance;
        }

        logisticsBoxModalInstance = createInstance();
        return logisticsBoxModalInstance;
    }

    function bindLogisticsBoxTarget(target) {
        const el = ensureElement(target);
        if (!el) return null;

        if (el.dataset.logisticsBoxModalBound === "true") {
            return el;
        }

        el.dataset.logisticsBoxModalBound = "true";

        el.addEventListener("click", function (evt) {
            evt.preventDefault();
            getInstance().open(el);
        });

        el.LogisticsBoxDataSet = function (datas) {
            return getInstance().setData(el, datas);
        };

        el.LogisticsBoxDataClear = function () {
            return getInstance().clear(el);
        };

        el.GetLogisticsBoxState = function () {
            return getInstance().getState(el);
        };

        el.GetLogisticsBoxSelectSort = function () {
            return getInstance().getActiveKeysCsv(el);
        };

        return el;
    }

    function bindAll(selector) {
        const elements = document.querySelectorAll(selector);
        elements.forEach(function (el) {
            bindLogisticsBoxTarget(el);
        });
        return elements;
    }

    window.LogisticsBoxModalApi = {
        get instance() {
            return getInstance();
        },

        bind: function (target) {
            return bindLogisticsBoxTarget(target);
        },

        bindAll: function (selector) {
            return bindAll(selector);
        },

        open: function (target) {
            return getInstance().open(target);
        },

        clear: function (target) {
            return getInstance().clear(target);
        },

        setData: function (target, datas) {
            return getInstance().setData(target, datas);
        },

        getState: function (target) {
            return getInstance().getState(target);
        },

        getActiveKeysCsv: function (target) {
            return getInstance().getActiveKeysCsv(target);
        },

        setAfterSaveCallback: function (callback) {
            getInstance().onAfterSave = callback;
        }
    };

    window.LogisticsBox_ClearBtnInit = function (e) {
        getInstance().onClearButtonInit(e);
    };

    window.LogisticsBox_ClearBtnClick = function () {
        return getInstance().onClearButtonClick();
    };

    window.LogisticsBox_SelectChange = function (e) {
        getInstance().onSelectionChanged(e);
    };

    window.LogisticsBoxContentReady = function (e) {
        getInstance().onGridContentReady(e);
    };

    window.getLogisticsBoxSelectSort = function () {
        const instance = getInstance();
        const target = instance.getTarget();
        if (!target) return "";
        return instance.getActiveKeysCsv(target);
    };

    if (typeof window.LogisticsBoxContentRefresh !== "function") {
        window.LogisticsBoxContentRefresh = function () {
            if (!window.jQuery) return;

            try {
                const grid = window.jQuery("#devLogisticsBoxListModalGrid").dxDataGrid("instance");
                if (grid) {
                    grid.refresh();
                }
            } catch (err) {
                console.error(err);
            }
        };
    }

    if (window.jQuery) {
        window.jQuery.fn.logisticsBoxModalInit = function () {
            return this.each(function () {
                bindLogisticsBoxTarget(this);
            });
        };
    }

})(window);