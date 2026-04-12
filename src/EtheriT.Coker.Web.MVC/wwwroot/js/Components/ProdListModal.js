import { ModuleSelectManager } from "./ModuleSelect.js";

let prodListModalInstance = null;

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
        name: "ProdListModal",
        modalSelector: "#ProdModal",
        gridSelector: "#devProdListModalGrid",
        saveButtonSelector: ".btn_prod_save",

        getRowKey: function (row) {
            return row?.Id ?? row?.id ?? null;
        },

        getRowText: function (row) {
            return row?.Title ?? row?.title ?? row?.Prod_Name ?? row?.prod_Name ?? "";
        },

        getStoredKey: function (item) {
            return item?.FK_ProdId
                ?? item?.fK_ProdId
                ?? item?.Id
                ?? item?.id
                ?? null;
        },

        createStoredItem: function (row) {
            return {
                Id: 0,
                FK_ProdId: row?.Id ?? row?.id ?? 0,
                Prod_Name: row?.Title ?? row?.title ?? row?.Prod_Name ?? row?.prod_Name ?? "",
                IsDeleted: false
            };
        },

        mapInitData: function (data) {
            return {
                Id: data?.id ?? data?.Id ?? 0,
                FK_ProdId:
                    data?.fK_ProdId
                    ?? data?.FK_ProdId
                    ?? data?.Id
                    ?? data?.id
                    ?? 0,
                Prod_Name:
                    data?.prod_Name
                    ?? data?.Prod_Name
                    ?? data?.title
                    ?? data?.Title
                    ?? "",
                IsDeleted: data?.IsDeleted === true
            };
        },

        buildDisplayText: function (rows) {
            if (!rows || !rows.length) return "無";

            return rows
                .map(function (x) {
                    return x.Prod_Name || x.Title || x.title || "";
                })
                .filter(Boolean)
                .join("、");
        },

        onAfterSave: function () {
            ProdListModalApi.refresh();
        }
    });
}

function getInstance() {
    if (prodListModalInstance) {
        return prodListModalInstance;
    }

    prodListModalInstance = createInstance();
    return prodListModalInstance;
}

function bindProdTarget(target) {
    const el = ensureElement(target);
    if (!el) return null;

    if (el.dataset.prodListModalBound === "true") {
        return el;
    }

    el.dataset.prodListModalBound = "true";

    el.addEventListener("click", function (evt) {
        evt.preventDefault();
        getInstance().open(el);
    });

    return el;
}

function bindAll(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(function (el) {
        bindProdTarget(el);
    });
    return elements;
}

const ProdListModalApi = {
    get instance() {
        return getInstance();
    },

    bind: function (target) {
        return bindProdTarget(target);
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
            const grid = window.jQuery("#devProdListModalGrid").dxDataGrid("instance");
            if (grid) {
                grid.refresh();
            }
        } catch (err) {
            console.error(err);
        }
    }
};

window.ProdListModalApi = ProdListModalApi;

export { ProdListModalApi };