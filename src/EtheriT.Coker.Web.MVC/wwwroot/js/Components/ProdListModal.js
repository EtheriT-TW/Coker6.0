import { ModuleSelectManager } from "./ModuleSelect.js";

let prodListModalInstance = null;
let prodModalTagFilter = null;
let prodModalTagOptionsPromise = null;

function refreshModalGrid() {
    if (!window.jQuery) return;

    try {
        const grid = window.jQuery("#devProdListModalGrid").dxDataGrid("instance");
        if (grid) grid.refresh();
    } catch (err) {
        console.error(err);
    }
}

function loadModalTagOptions() {
    if (prodModalTagOptionsPromise) return prodModalTagOptionsPromise;

    prodModalTagOptionsPromise = co.Product.Get.ProductListTags()
        .then(function (data) { return data || []; })
        .catch(function (err) {
            prodModalTagOptionsPromise = null;
            console.error("商品標籤載入失敗", err);
            return [];
        });

    return prodModalTagOptionsPromise;
}

function initModalFilters() {
    if (!window.jQuery || !document.querySelector("#ProdModalTagFilter")) return;

    if (!prodModalTagFilter) {
        const store = new DevExpress.data.CustomStore({
            key: "fK_TId",
            loadMode: "raw",
            load: loadModalTagOptions
        });

        prodModalTagFilter = window.jQuery("#ProdModalTagFilter").dxTagBox({
            dataSource: store,
            valueExpr: "fK_TId",
            displayExpr: "tag_Name",
            placeholder: "選擇商品標籤，可多選",
            showSelectionControls: true,
            applyValueMode: "useButtons",
            searchEnabled: true,
            multiline: false,
            maxDisplayedTags: 3,
            showMultiTagOnly: false,
            dropDownOptions: {
                minWidth: 420,
                wrapperAttr: { class: "prod-modal-tag-dropdown" }
            },
            onValueChanged: refreshModalGrid
        }).dxTagBox("instance");
    }

    window.jQuery("#ProdModalExcludeUnavailable")
        .off("change.prodModalFilter")
        .on("change.prodModalFilter", refreshModalGrid);

    window.jQuery("#ProdModalFilterClear")
        .off("click.prodModalFilter")
        .on("click.prodModalFilter", function () {
            if (prodModalTagFilter) prodModalTagFilter.option("value", []);
            window.jQuery("#ProdModalExcludeUnavailable").prop("checked", false);
            refreshModalGrid();
        });

    window.jQuery(document)
        .off("click.prodModalTagBadge", "#ProdModal .prod-modal-tag-badge")
        .on("click.prodModalTagBadge", "#ProdModal .prod-modal-tag-badge", function (event) {
            event.preventDefault();
            event.stopPropagation();

            const tagName = String(window.jQuery(this).attr("data-tag-name") || "").trim();
            if (!tagName || !prodModalTagFilter) return;

            loadModalTagOptions().then(function (tagOptions) {
                const tag = tagOptions.find(function (item) {
                    return String(item.tag_Name ?? item.Tag_Name ?? "").trim() === tagName;
                });
                if (!tag) return;

                const tagId = tag.fK_TId ?? tag.FK_TId;
                const values = (prodModalTagFilter.option("value") || []).slice();
                if (!values.includes(tagId)) {
                    values.push(tagId);
                    prodModalTagFilter.option("value", values);
                }
            });
        });
}

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
    initModalFilters();

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
                MinsizeImage: row?.MinsizeImage ?? row?.minsizeImage ?? "/images/noImg.jpg",
                ItemNo: row?.ItemNo ?? row?.itemNo ?? "",
                Price: row?.Price ?? row?.price ?? "",
                ProductStatus: row?.ProductStatus ?? row?.productStatus ?? 0,
                ProductStatusName: row?.ProductStatusName ?? row?.productStatusName ?? "",
                Visible: row?.Visible ?? row?.visible ?? false,
                Available: row?.Available ?? row?.available ?? false,
                NoStockManagement: row?.NoStockManagement ?? row?.noStockManagement ?? false,
                StockQuantity: row?.StockQuantity ?? row?.stockQuantity ?? null,
                AlertQuantity: row?.AlertQuantity ?? row?.alertQuantity ?? null,
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
                MinsizeImage: data?.minsizeImage ?? data?.MinsizeImage ?? "/images/noImg.jpg",
                ItemNo: data?.itemNo ?? data?.ItemNo ?? "",
                Price: data?.price ?? data?.Price ?? "",
                ProductStatus: data?.productStatus ?? data?.ProductStatus ?? 0,
                ProductStatusName: data?.productStatusName ?? data?.ProductStatusName ?? "",
                Visible: data?.visible ?? data?.Visible ?? false,
                Available: data?.available ?? data?.Available ?? false,
                NoStockManagement: data?.noStockManagement ?? data?.NoStockManagement ?? false,
                StockQuantity: data?.stockQuantity ?? data?.StockQuantity ?? null,
                AlertQuantity: data?.alertQuantity ?? data?.AlertQuantity ?? null,
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
        ProdListModalApi.open(el);
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

        bindProdTarget(el);
        this.defaultTarget = el;
        return el;
    },

    getDefaultTarget: function () {
        return this.defaultTarget || null;
    },

    setActiveTarget: function (target) {
        const el = this.resolveElement(target);
        if (!el) return null;

        bindProdTarget(el);
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
        const el = bindProdTarget(target);
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
            const grid = window.jQuery("#devProdListModalGrid").dxDataGrid("instance");
            if (grid) {
                grid.refresh();
            }
        } catch (err) {
            console.error(err);
        }
    },

    loadParams: {
        pids: function () {
            return window.ProdListModalApi.getActiveKeysCsv();
        },

        tagIds: function () {
            if (!prodModalTagFilter) return "";
            return (prodModalTagFilter.option("value") || []).join(",");
        },

        excludeUnavailable: function () {
            return document.querySelector("#ProdModalExcludeUnavailable")?.checked === true;
        }
    }
};

window.ProdListModalApi = ProdListModalApi;

export { ProdListModalApi };
