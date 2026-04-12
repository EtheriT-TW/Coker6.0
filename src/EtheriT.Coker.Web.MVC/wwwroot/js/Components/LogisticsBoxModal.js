import { ModuleSelectManager } from "./ModuleSelect.js";

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
    return ModuleSelectManager.create({
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
                Fee: 0,
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
                    ?? data?.LogisticsBox_Name
                    ?? data?.name
                    ?? data?.Name
                    ?? "",
                Fee:
                    data?.fee
                    ?? data?.Fee
                    ?? 0,
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
            LogisticsBoxModalApi.refresh();
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
        LogisticsBoxModalApi.open(el);
    });

    return el;
}

function bindAll(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(function (el) {
        bindLogisticsBoxTarget(el);
    });
    return elements;
}

const LogisticsBoxModalApi = {
    defaultTarget: null,
    activeTarget: null,

    get instance() {
        return getInstance();
    },

    resolveElement: function (target) {
        return ensureElement(target);
    },

    setDefaultTarget: function (target) {
        const el = this.resolveElement(target);
        if (!el) return null;

        bindLogisticsBoxTarget(el);
        this.defaultTarget = el;
        return el;
    },

    getDefaultTarget: function () {
        return this.defaultTarget || null;
    },

    setActiveTarget: function (target) {
        const el = this.resolveElement(target);
        if (!el) return null;

        bindLogisticsBoxTarget(el);
        this.activeTarget = el;
        return el;
    },

    getActiveTarget: function () {
        return this.activeTarget || null;
    },

    getCurrentTarget: function () {
        return this.activeTarget || this.defaultTarget || null;
    },

    bind: function (target, options) {
        const el = bindLogisticsBoxTarget(target);
        if (!el) return null;

        const settings = Object.assign({
            setAsDefault: false
        }, options || {});

        if (!this.defaultTarget || settings.setAsDefault === true) {
            this.defaultTarget = el;
        }

        return el;
    },

    bindAll: function (selector, options) {
        const elements = bindAll(selector);
        const settings = Object.assign({
            setFirstAsDefault: true
        }, options || {});

        if (settings.setFirstAsDefault && !this.defaultTarget && elements.length > 0) {
            this.defaultTarget = elements[0];
        }

        return elements;
    },

    open: function (target) {
        const nextTarget = this.resolveElement(target) || this.defaultTarget;

        if (!nextTarget) {
            return Promise.resolve();
        }

        this.setActiveTarget(nextTarget);
        return getInstance().open(nextTarget);
    },

    clear: function () {
        const target = this.getCurrentTarget();
        if (!target) return Promise.resolve();

        return getInstance().clear(target);
    },

    setData: function (datas) {
        const target = this.getCurrentTarget();
        if (!target) return Promise.resolve();

        return getInstance().setData(target, datas || []);
    },

    getState: function () {
        const target = this.getCurrentTarget();
        if (!target) {
            return { items: [], selectedKeys: [], selectedRows: [], text: "無" };
        }

        return getInstance().getState(target);
    },

    getActiveKeysCsv: function () {
        const target = this.getCurrentTarget();
        if (!target) return "";

        return getInstance().getActiveKeysCsv(target);
    },

    setAfterSaveCallback: function (callback) {
        getInstance().onAfterSave = callback;
    },

    onClearButtonInit: function (e) {
        getInstance().onClearButtonInit(e);
    },

    onClearButtonClick: function () {
        return getInstance().onClearButtonClick();
    },

    onSelectionChanged: function (e) {
        getInstance().onSelectionChanged(e);
    },

    onGridContentReady: function (e) {
        getInstance().onGridContentReady(e);
    },

    refresh: function () {
        if (!window.jQuery) return;

        try {
            const grid = window.jQuery("#devLogisticsBoxListModalGrid").dxDataGrid("instance");
            if (grid) {
                grid.refresh();
            }
        } catch (err) {
            console.error(err);
        }
    },

    loadParams: {
        ids: function () {
            return window.LogisticsBoxModalApi.getActiveKeysCsv();
        }
    }
};

window.LogisticsBoxModalApi = LogisticsBoxModalApi;

export { LogisticsBoxModalApi };