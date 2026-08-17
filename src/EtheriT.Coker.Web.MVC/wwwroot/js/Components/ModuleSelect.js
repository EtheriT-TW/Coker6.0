function isFunction(fn) {
    return typeof fn === "function";
}

function isArray(value) {
    return Array.isArray(value);
}

function toArray(value) {
    return isArray(value) ? value : [];
}

function uniqueArray(arr) {
    return [...new Set((arr || []).filter(v => v !== null && v !== undefined && v !== ""))];
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

function setElementValue(el, value) {
    if (!el) return;

    if ("value" in el) {
        el.value = value;
    } else {
        el.textContent = value;
    }
}

function addClass(el, className) {
    if (!el) return;
    el.classList.add(className);
}

function removeClass(el, className) {
    if (!el) return;
    el.classList.remove(className);
}

function defaultBuildText(rows, getRowText) {
    if (!rows || !rows.length) return "無";
    return rows.map(r => getRowText(r)).filter(Boolean).join("、");
}

const registry = new Map();

export class ModuleSelect {
    constructor(options) {
        if (!options || !options.name) {
            throw new Error("ModuleSelect: options.name 為必填");
        }
        if (!options.modalSelector) {
            throw new Error("ModuleSelect: options.modalSelector 為必填");
        }
        if (!options.gridSelector) {
            throw new Error("ModuleSelect: options.gridSelector 為必填");
        }
        if (!isFunction(options.getRowKey)) {
            throw new Error("ModuleSelect: options.getRowKey 必須是 function");
        }
        if (!isFunction(options.getRowText)) {
            throw new Error("ModuleSelect: options.getRowText 必須是 function");
        }
        if (!isFunction(options.getStoredKey)) {
            throw new Error("ModuleSelect: options.getStoredKey 必須是 function");
        }
        if (!isFunction(options.createStoredItem)) {
            throw new Error("ModuleSelect: options.createStoredItem 必須是 function");
        }

        this.name = options.name;
        this.modalSelector = options.modalSelector;
        this.gridSelector = options.gridSelector;
        this.saveButtonSelector = options.saveButtonSelector || ".btn-save";

        this.getRowKey = options.getRowKey;
        this.getRowText = options.getRowText;
        this.getStoredKey = options.getStoredKey;
        this.createStoredItem = options.createStoredItem;

        this.mapInitData = isFunction(options.mapInitData)
            ? options.mapInitData
            : function (x) { return x; };

        this.buildDisplayText = isFunction(options.buildDisplayText)
            ? options.buildDisplayText
            : (rows) => defaultBuildText(rows, this.getRowText.bind(this));

        this.onAfterSave = isFunction(options.onAfterSave) ? options.onAfterSave : null;
        this.onAfterClear = isFunction(options.onAfterClear) ? options.onAfterClear : null;
        this.onContentReady = isFunction(options.onContentReady) ? options.onContentReady : null;

        this.targetElement = null;
        this.clearButton = null;
        this.dxGrid = null;
        this.stateMap = new WeakMap();

        this.modalElement = null;
        this.modal = null;
        this.isInitialized = false;
        this.isSynchronizingSelection = false;
        this.hasPendingUserSelectionAction = false;
    }

    _createDefaultState() {
        return {
            items: [],
            selectedKeys: [],
            selectedRows: [],
            pendingDeselectedKeys: [],
            text: "無"
        };
    }

    _ensureInitialized() {
        if (this.isInitialized) {
            return;
        }

        this.modalElement = document.querySelector(this.modalSelector);
        if (!this.modalElement) {
            throw new Error(`ModuleSelect: 找不到 modal 元素 ${this.modalSelector}`);
        }

        if (!window.bootstrap || !window.bootstrap.Modal) {
            throw new Error("ModuleSelect: 找不到 bootstrap.Modal，請先載入 Bootstrap");
        }

        this.modal = new window.bootstrap.Modal(this.modalElement);

        this._bindModalEvents();
        this._bindSaveButton();

        this.isInitialized = true;
    }

    _bindModalEvents() {
        const markUserSelectionAction = (event) => {
            const eventTarget = event?.target;
            if (!(eventTarget instanceof Element)) return;
            if (!eventTarget.closest(this.gridSelector)) return;
            if (!eventTarget.closest(".dx-command-select")) return;

            this.hasPendingUserSelectionAction = true;
        };

        this.modalElement.addEventListener("pointerdown", markUserSelectionAction, true);
        this.modalElement.addEventListener("keydown", (event) => {
            if (event.key === " " || event.key === "Enter") {
                markUserSelectionAction(event);
            }
        }, true);

        this.modalElement.addEventListener("hidden.bs.modal", () => {
            if (!this.targetElement) return;

            this.resetPendingSelection(this.targetElement);
            this.restoreSelectionFromState(this.targetElement).catch(function (err) {
                console.error(err);
            });
        });
    }

    getSelectionKey(row) {
        const rowKey = this.getRowKey(row);
        if (rowKey !== null && rowKey !== undefined && rowKey !== "") {
            return rowKey;
        }

        return this.getStoredKey(row);
    }

    resetPendingSelection(target) {
        const el = ensureElement(target);
        if (!el) return;

        const state = this.getState(el);
        const activeItems = toArray(state.items).filter(item => !item.IsDeleted);

        state.selectedKeys = uniqueArray(activeItems.map(item => this.getStoredKey(item)));
        state.selectedRows = activeItems.slice();
        state.pendingDeselectedKeys = [];
        state.text = this.buildDisplayText(activeItems) || "無";
    }

    _bindSaveButton() {
        const saveButton = this.modalElement.querySelector(this.saveButtonSelector);
        if (!saveButton) return;

        saveButton.addEventListener("click", () => {
            if (!this.targetElement) return;

            const state = this.getState(this.targetElement);
            const selectedKeys = uniqueArray(state.selectedKeys);
            const selectedRows = toArray(state.selectedRows);
            const currentItems = toArray(state.items);

            currentItems.forEach((item) => {
                const exists = selectedKeys.includes(this.getStoredKey(item));
                item.IsDeleted = !exists;
            });

            selectedRows.forEach((row) => {
                const key = this.getSelectionKey(row);
                const exists = currentItems.some(item => this.getStoredKey(item) === key);

                if (!exists) {
                    currentItems.push(this.createStoredItem(row));
                }
            });

            state.items = currentItems;
            state.text = state.text || "無";

            this.refreshTargetDisplay(this.targetElement);

            if (this.onAfterSave) {
                this.onAfterSave(this.targetElement, state, this);
            }

            this.modal.hide();
        });
    }

    getState(target) {
        const el = ensureElement(target);
        if (!el) {
            return this._createDefaultState();
        }

        let state = this.stateMap.get(el);
        if (!state) {
            state = this._createDefaultState();
            this.stateMap.set(el, state);
        }

        return state;
    }

    setTarget(target) {
        this.targetElement = ensureElement(target);
    }

    getTarget() {
        return this.targetElement;
    }

    async waitGridInstance(timeoutMs = 10000, intervalMs = 100) {
        const start = Date.now();

        while (Date.now() - start < timeoutMs) {
            const gridEl = document.querySelector(this.gridSelector);

            if (gridEl && gridEl.classList.contains("isReady")) {
                if (window.jQuery) {
                    const instance = window.jQuery(this.gridSelector).dxDataGrid("instance");
                    if (instance) {
                        this.dxGrid = instance;
                        return instance;
                    }
                }
            }

            await new Promise(resolve => setTimeout(resolve, intervalMs));
        }

        throw new Error(`ModuleSelect: 等待 DataGrid instance 逾時 ${this.gridSelector}`);
    }

    refreshTargetDisplay(target) {
        const el = ensureElement(target);
        if (!el) return;

        const state = this.getState(el);
        const text = state.text || "無";

        setElementValue(el, text);

        removeClass(el, "multiple");

        if ("scrollHeight" in el && "offsetHeight" in el) {
            if (el.scrollHeight > el.offsetHeight) {
                addClass(el, "multiple");
            }
        }
    }

    async restoreSelectionFromState(target) {
        const el = ensureElement(target);
        if (!el) return;

        const state = this.getState(el);
        const selectedKeys = toArray(state.items)
            .filter(item => !item.IsDeleted)
            .map(item => this.getStoredKey(item));

        const keys = uniqueArray(selectedKeys);

        await this.applySelectionKeys(keys);
    }

    async restorePendingSelection(target) {
        const el = ensureElement(target);
        if (!el) return;

        const state = this.getState(el);
        await this.applySelectionKeys(uniqueArray(state.selectedKeys));
    }

    async applySelectionKeys(keys) {
        const grid = await this.waitGridInstance();
        const selectionKeys = uniqueArray(keys);

        this.isSynchronizingSelection = true;
        try {
            if (selectionKeys.length > 0) {
                await Promise.resolve(grid.selectRows(selectionKeys, false));
            } else {
                await Promise.resolve(grid.clearSelection());
            }
        } finally {
            this.isSynchronizingSelection = false;
        }
    }

    async open(target) {
        const el = ensureElement(target);
        if (!el) return;

        this._ensureInitialized();
        this.setTarget(el);
        this.resetPendingSelection(el);

        try {
            const grid = await this.waitGridInstance();
            this.isSynchronizingSelection = true;
            try {
                await Promise.resolve(grid.refresh());
            } finally {
                this.isSynchronizingSelection = false;
            }
            await this.restoreSelectionFromState(el);
        } catch (err) {
            console.error(err);
        }

        this.modal.show();
    }

    async clear(target) {
        const el = ensureElement(target);
        if (!el) return;

        this._ensureInitialized();

        const state = this.getState(el);
        state.items = [];
        state.selectedKeys = [];
        state.selectedRows = [];
        state.pendingDeselectedKeys = [];
        state.text = "無";

        this.refreshTargetDisplay(el);

        try {
            const grid = await this.waitGridInstance();
            grid.clearSelection();
        } catch (err) {
            console.error(err);
        }

        if (this.clearButton) {
            this.clearButton.option("disabled", true);
        }

        if (this.onAfterClear) {
            this.onAfterClear(el, state, this);
        }
    }

    async setData(target, datas) {
        const el = ensureElement(target);
        if (!el) return;

        this._ensureInitialized();

        const rows = toArray(datas).map(this.mapInitData);
        const state = this.getState(el);

        const items = [];
        const selectedKeys = [];

        rows.forEach((row) => {
            const key = this.getStoredKey(row);
            if (key === null || key === undefined || key === "") return;

            items.push({
                ...row,
                IsDeleted: false
            });

            selectedKeys.push(key);
        });

        state.items = items;
        state.selectedKeys = uniqueArray(selectedKeys);
        state.selectedRows = rows;
        state.pendingDeselectedKeys = [];
        state.text = this.buildDisplayText(rows) || "無";

        this.refreshTargetDisplay(el);

        if (el === this.getTarget() && this.modalElement?.classList.contains("show")) {
            try {
                await this.restoreSelectionFromState(el);
            } catch (err) {
                console.error(err);
            }
        }
    }

    getActiveKeys(target) {
        const el = ensureElement(target);
        if (!el) return [];

        const state = this.getState(el);

        return toArray(state.items)
            .filter(item => !item.IsDeleted)
            .map(item => this.getStoredKey(item))
            .filter(v => v !== null && v !== undefined && v !== "");
    }

    getActiveKeysCsv(target) {
        return this.getActiveKeys(target).join(",");
    }

    onSelectionChanged(e) {
        const target = this.getTarget();
        if (!target || this.isSynchronizingSelection) return;

        const state = this.getState(target);
        const pendingDeselectedKeys = new Set(toArray(state.pendingDeselectedKeys));
        const selectedKeys = new Set(toArray(state.selectedKeys));
        const currentSelectedKeys = toArray(e?.currentSelectedRowKeys);
        const currentDeselectedKeys = toArray(e?.currentDeselectedRowKeys);
        const isUserAction = Boolean(e?.event) || this.hasPendingUserSelectionAction;
        this.hasPendingUserSelectionAction = false;

        // Remote filtering can temporarily report selected rows as deselected because
        // they are no longer part of the loaded page. Keep committed selections unless
        // the deselection came from an actual user interaction.
        toArray(state.items).filter(item => !item.IsDeleted).forEach(item => {
            const key = this.getStoredKey(item);
            if (!pendingDeselectedKeys.has(key)) selectedKeys.add(key);
        });

        // When checking a row after a remote filter, DevExtreme may report the hidden
        // committed rows in currentDeselectedRowKeys together with the newly selected
        // row. In that case the action is an addition, not an explicit removal.
        if (isUserAction && currentSelectedKeys.length === 0) {
            currentDeselectedKeys.forEach(key => {
                selectedKeys.delete(key);
                pendingDeselectedKeys.add(key);
            });
        }

        currentSelectedKeys.forEach(key => {
            selectedKeys.add(key);
            pendingDeselectedKeys.delete(key);
        });

        const rowsByKey = new Map();
        toArray(state.selectedRows).forEach(row => {
            const key = this.getSelectionKey(row);
            if (selectedKeys.has(key)) rowsByKey.set(key, row);
        });
        toArray(state.items).filter(item => !item.IsDeleted).forEach(item => {
            const key = this.getStoredKey(item);
            if (selectedKeys.has(key) && !rowsByKey.has(key)) rowsByKey.set(key, item);
        });
        toArray(e?.selectedRowsData).forEach(row => {
            const key = this.getSelectionKey(row);
            if (selectedKeys.has(key)) rowsByKey.set(key, row);
        });

        const rows = Array.from(selectedKeys)
            .map(key => rowsByKey.get(key))
            .filter(Boolean);

        state.selectedRows = rows;
        state.selectedKeys = Array.from(selectedKeys);
        state.pendingDeselectedKeys = Array.from(pendingDeselectedKeys);
        state.text = this.buildDisplayText(rows) || "無";

        if (this.clearButton) {
            this.clearButton.option("disabled", state.selectedKeys.length === 0);
        }
    }

    onClearButtonInit(e) {
        this.clearButton = e?.component || null;
    }

    async onClearButtonClick() {
        const target = this.getTarget();
        if (!target) return;

        const state = this.getState(target);

        toArray(state.items).forEach(item => {
            item.IsDeleted = true;
        });

        state.selectedKeys = [];
        state.selectedRows = [];
        state.pendingDeselectedKeys = uniqueArray(
            toArray(state.items).map(item => this.getStoredKey(item))
        );
        state.text = "無";

        this.refreshTargetDisplay(target);

        try {
            const grid = await this.waitGridInstance();
            grid.clearSelection();
        } catch (err) {
            console.error(err);
        }

        if (this.clearButton) {
            this.clearButton.option("disabled", true);
        }
    }

    onGridContentReady(e) {
        const gridEl = document.querySelector(this.gridSelector);
        if (gridEl) {
            gridEl.classList.add("isReady");
        }

        if (window.jQuery) {
            try {
                this.dxGrid = window.jQuery(this.gridSelector).dxDataGrid("instance");
            } catch (err) {
                console.error(err);
            }
        }

        if (this.onContentReady) {
            this.onContentReady(e, this);
        }

        if (this.targetElement && this.modalElement?.classList.contains("show")) {
            this.restorePendingSelection(this.targetElement).catch(function (err) {
                console.error(err);
            });
        }
    }
}

export const ModuleSelectManager = {
    create(options) {
        if (!options || !options.name) {
            throw new Error("ModuleSelectManager.create: options.name 為必填");
        }

        if (registry.has(options.name)) {
            return registry.get(options.name);
        }

        const instance = new ModuleSelect(options);
        registry.set(options.name, instance);
        return instance;
    },

    get(name) {
        return registry.get(name) || null;
    }
};
