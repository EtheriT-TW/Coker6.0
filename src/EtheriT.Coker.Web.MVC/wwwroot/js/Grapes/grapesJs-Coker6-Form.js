grapesjs.plugins.add('grapesJs-Coker6-Form', (editor, options) => {
    const bm = editor.BlockManager;
    const domc = editor.DomComponents;
    const tm = editor.TraitManager;

    function toStr(v) {
        return (v ?? '').toString();
    }

    function trimStr(v) {
        return toStr(v).trim();
    }

    function normalizeOptionItem(item) {
        if (item == null) {
            return {
                value: '',
                displayText: '',
                attributes: {}
            };
        }

        if (typeof item === 'string' || typeof item === 'number' || typeof item === 'boolean') {
            const s = toStr(item);
            return {
                value: s,
                displayText: s,
                attributes: { value: s }
            };
        }

        const attributes = Object.assign({}, item.attributes || {});

        const value = item.value != null
            ? toStr(item.value)
            : attributes.value != null
                ? toStr(attributes.value)
                : '';

        let displayText = item.displayText != null
            ? toStr(item.displayText)
            : '';

        if (!displayText && value) {
            displayText = value;
        }

        attributes.value = value;

        return {
            value,
            displayText,
            attributes
        };
    }

    function normalizeOptions(options) {
        if (!Array.isArray(options)) return [];
        return options
            .map(normalizeOptionItem)
            .filter(opt => trimStr(opt.displayText) !== '');
    }

    // 將 options 同步成 <option>，並保留原本 attributes
    const syncOptionsToChildren = (component) => {
        const opts = normalizeOptions(component.get('options') || []);

        component.components().reset([]);
        const children = component.components();

        opts.forEach(opt => {
            const attrs = Object.assign({}, opt.attributes || {});

            attrs.value = toStr(opt.value);

            let optComp = children.add({
                tagName: 'option',
                attributes: attrs
            });

            optComp = Array.isArray(optComp) ? optComp[0] : optComp;

            let textNode = optComp.components().add({
                type: 'textnode',
                content: toStr(opt.displayText)
            });

            textNode = Array.isArray(textNode) ? textNode[0] : textNode;
        });
    };

    // 從現有子節點抽取 <option> → 新格式
    const extractOptionsFromChildren = (component) => {
        const res = [];
        const children = component.components && component.components();

        if (!children || !children.length) {
            return res;
        }

        children.each(ch => {
            const tag = (ch.get && ch.get('tagName')) || '';

            if ((tag || '').toLowerCase() !== 'option') {
                return;
            }

            const rawAttrs =
                (ch.getAttributes && ch.getAttributes()) ||
                ch.get('attributes') ||
                {};

            const attributes = Object.assign({}, rawAttrs);

            const value = attributes.value != null
                ? toStr(attributes.value)
                : '';

            let displayText = '';

            const tnode = ch.components && ch.components().models.find(m => {
                const t = m.get && (m.get('type') || m.get('tagName'));
                return t === 'textnode';
            });

            if (tnode) {
                displayText = toStr(tnode.get && tnode.get('content'));
            }

            if (!displayText && value) {
                displayText = value;
            }

            if (trimStr(displayText) !== '') {
                attributes.value = value;

                res.push({
                    value,
                    displayText,
                    attributes
                });
            }
        });

        return res;
    };

    function makeOptionDraft(opt) {
        const normalized = normalizeOptionItem(opt);

        return {
            value: normalized.value,
            displayText: normalized.displayText,
            attributes: Object.assign({}, normalized.attributes || {})
        };
    }

    function openOptionsModal(editor, component) {
        const modal = editor.Modal;

        const wrap = document.createElement('div');
        wrap.className = 'ds-modal';

        const toolbar = document.createElement('div');
        toolbar.className = 'ds-toolbar';

        const addBtn = document.createElement('button');
        addBtn.type = 'button';
        addBtn.className = 'ds-btn ds-btn-primary';
        addBtn.textContent = '＋ 新增';

        const bulkBox = document.createElement('div');
        bulkBox.className = 'ds-bulk';

        const bulkLabel = document.createElement('label');
        bulkLabel.className = 'ds-label';
        bulkLabel.textContent = '或貼上多行（每行一個選項；若未指定 提交值，則 提交值 與顯示文字相同）：';

        const bulkArea = document.createElement('textarea');
        bulkArea.className = 'ds-textarea';
        bulkArea.placeholder = [
            '貼上格式支援：',
            '1. 單欄：台北市',
            '2. 雙欄：A001 | 台北市',
            '3. 提交值 可空：| 請選擇'
        ].join('\n');

        bulkBox.appendChild(bulkLabel);
        bulkBox.appendChild(bulkArea);

        toolbar.appendChild(bulkBox);
        toolbar.appendChild(addBtn);

        const list = document.createElement('div');
        list.className = 'ds-list';

        const actions = document.createElement('div');
        actions.className = 'ds-actions';

        const btnCancel = document.createElement('button');
        btnCancel.type = 'button';
        btnCancel.className = 'ds-btn';
        btnCancel.textContent = '取消';

        const btnSave = document.createElement('button');
        btnSave.type = 'button';
        btnSave.className = 'ds-btn ds-btn-primary';
        btnSave.textContent = '儲存';

        actions.appendChild(btnCancel);
        actions.appendChild(btnSave);

        wrap.appendChild(toolbar);
        wrap.appendChild(list);
        wrap.appendChild(actions);

        const ensureDisplayText = (row) => {
            const valueInput = row.querySelector('.ds-input-value');
            const textInput = row.querySelector('.ds-input-text');
            if (!valueInput || !textInput) return;

            const value = toStr(valueInput.value);
            const displayText = toStr(textInput.value);

            if (!trimStr(displayText) && trimStr(value)) {
                textInput.value = value;
            }
        };

        const makeRow = (item = { value: '', displayText: '', attributes: {} }) => {
            const rowData = makeOptionDraft(item);

            const row = document.createElement('div');
            row.className = 'ds-row';

            row._optionAttributes = Object.assign({}, rowData.attributes || {});

            const handle = document.createElement('span');
            handle.className = 'ds-handle';
            handle.textContent = '≡';
            handle.title = '拖曳以排序';

            const fields = document.createElement('div');
            fields.className = 'ds-fields';

            const valueWrap = document.createElement('div');
            valueWrap.className = 'ds-field ds-field-value';

            const valueLabel = document.createElement('label');
            valueLabel.className = 'ds-label';
            valueLabel.textContent = '提交值';

            const valueInput = document.createElement('input');
            valueInput.type = 'text';
            valueInput.className = 'ds-input ds-input-value';
            valueInput.value = rowData.value;
            valueInput.placeholder = '可留空';

            valueWrap.appendChild(valueLabel);
            valueWrap.appendChild(valueInput);

            const textWrap = document.createElement('div');
            textWrap.className = 'ds-field ds-field-text';

            const textLabel = document.createElement('label');
            textLabel.className = 'ds-label';
            textLabel.textContent = '顯示文字';

            const textInput = document.createElement('input');
            textInput.type = 'text';
            textInput.className = 'ds-input ds-input-text';
            textInput.value = rowData.displayText;
            textInput.placeholder = '不可留空';

            textWrap.appendChild(textLabel);
            textWrap.appendChild(textInput);

            fields.appendChild(valueWrap);
            fields.appendChild(textWrap);

            const del = document.createElement('button');
            del.type = 'button';
            del.className = 'ds-del';
            del.setAttribute('aria-label', '刪除');
            del.textContent = '×';
            del.addEventListener('click', () => row.remove());

            row.appendChild(handle);
            row.appendChild(fields);
            row.appendChild(del);

            // value 變更規則：
            // 1. 若 displayText 為空 → 自動帶入 value
            // 2. 若 displayText == 舊 value → 視為跟隨狀態，改成新 value
            valueInput.addEventListener('input', () => {
                const oldValue = toStr(valueInput.dataset.prevValue ?? '');
                const newValue = toStr(valueInput.value);
                const currentText = toStr(textInput.value);

                if (!trimStr(currentText)) {
                    textInput.value = newValue;
                } else if (currentText === oldValue) {
                    textInput.value = newValue;
                }

                valueInput.dataset.prevValue = newValue;
            });

            valueInput.addEventListener('blur', () => {
                ensureDisplayText(row);
                valueInput.dataset.prevValue = toStr(valueInput.value);
            });

            // displayText focus 規則：
            // 若 displayText == value，清空以方便輸入自訂文字
            textInput.addEventListener('focus', () => {
                const value = toStr(valueInput.value);
                const displayText = toStr(textInput.value);

                if (displayText === value) {
                    textInput.value = '';
                }
            });

            // displayText blur 規則：
            // 若空白，自動補回 value
            textInput.addEventListener('blur', () => {
                if (!trimStr(textInput.value)) {
                    textInput.value = toStr(valueInput.value);
                }
            });

            valueInput.dataset.prevValue = toStr(valueInput.value);

            return row;
        };

        const opts = normalizeOptions(component.get('options') || []);
        if (opts.length === 0) {
            list.appendChild(makeRow({ value: '', displayText: '選項1' }));
        } else {
            opts.forEach(opt => list.appendChild(makeRow(opt)));
        }

        function parseBulkLine(line) {
            const raw = toStr(line).trim();
            if (!raw) return null;

            // 支援：
            // A001 | 台北市
            // A001,台北市
            // A001\t台北市
            // 單欄：台北市 -> value/displayText 同值
            let parts = null;

            if (raw.includes('|')) {
                parts = raw.split('|');
            } else if (raw.includes('\t')) {
                parts = raw.split('\t');
            } else if (raw.includes(',')) {
                parts = raw.split(',');
            }

            if (parts && parts.length >= 2) {
                const value = toStr(parts[0]).trim();
                let displayText = toStr(parts.slice(1).join(parts === raw.split('|') ? '|' : parts === raw.split('\t') ? '\t' : ',')).trim();

                if (!displayText && value) {
                    displayText = value;
                }

                if (!displayText) return null;

                return { value, displayText };
            }

            return {
                value: raw,
                displayText: raw
            };
        }

        function importFromTextarea(bulkArea, list, makeRow) {
            const buf = toStr(bulkArea.value).trim();
            if (!buf) return 0;

            const lines = buf.split(/\r?\n/).map(s => s.trim()).filter(Boolean);
            let count = 0;

            lines.forEach(line => {
                const parsed = parseBulkLine(line);
                if (!parsed) return;
                list.appendChild(makeRow(parsed));
                count++;
            });

            bulkArea.value = '';
            return count;
        }

        addBtn.addEventListener('click', () => {
            const added = importFromTextarea(bulkArea, list, makeRow);
            if (added > 0) return;
            list.appendChild(makeRow({ value: '', displayText: '' }));
        });

        bulkArea.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                importFromTextarea(bulkArea, list, makeRow);
            }
        });

        btnCancel.addEventListener('click', () => modal.close());

        btnSave.addEventListener('click', () => {
            const rows = Array.from(list.querySelectorAll('.ds-row'));
            const values = [];
            let firstInvalidInput = null;

            rows.forEach(row => {
                const valueInput = row.querySelector('.ds-input-value');
                const textInput = row.querySelector('.ds-input-text');

                if (!valueInput || !textInput) return;

                let value = toStr(valueInput.value);
                let displayText = toStr(textInput.value);

                // blur 規則在儲存前再保底一次
                if (!trimStr(displayText)) {
                    displayText = value;
                    textInput.value = displayText;
                }

                value = toStr(value);
                displayText = toStr(displayText);

                // 若整列都空，直接忽略
                if (!trimStr(value) && !trimStr(displayText)) {
                    valueInput.classList.remove('is-invalid');
                    textInput.classList.remove('is-invalid');
                    return;
                }

                // displayText 不可空
                if (!trimStr(displayText)) {
                    textInput.classList.add('is-invalid');
                    if (!firstInvalidInput) firstInvalidInput = textInput;
                    return;
                }

                valueInput.classList.remove('is-invalid');
                textInput.classList.remove('is-invalid');

                const attributes = Object.assign({}, row._optionAttributes || {});

                attributes.value = value;

                values.push({
                    value,
                    displayText,
                    attributes
                });
            });

            if (firstInvalidInput) {
                firstInvalidInput.focus();
                return;
            }

            component.set('options', values);
            modal.close();
        });

        modal.open({ title: '編輯下拉選項', content: wrap });

        const $ = window.jQuery;
        if ($ && $.fn && $.fn.sortable) {
            setTimeout(() => {
                try { $(list).sortable('destroy'); } catch (_) { }
                $(list).sortable({
                    items: '.ds-row',
                    handle: '.ds-handle',
                    axis: 'y',
                    tolerance: 'pointer',
                    placeholder: 'ds-placeholder',
                    forcePlaceholderSize: true,
                    start: (e, ui) => ui.placeholder.height(ui.item.outerHeight()),
                });
                try { $(list).disableSelection && $(list).disableSelection(); } catch (_) { }
            }, 0);
        }
    }

    domc.addType('dynamic-select', {
        isComponent: el => el.tagName === 'SELECT',
        model: {
            defaults: {
                tagName: 'select',
                attributes: {
                    'data-dynamic-select': '1',
                },
                options: [],
                traits: [
                    { type: 'options-modal-launcher', label: '選項' }
                ],
                toolbar: [
                    { attributes: { class: 'fa fa-edit', title: '編輯選項 (E)' }, command: 'open-ds-editor' },
                ]
            },

            init() {
                if (!this.get('_bootstrapped')) {
                    const extracted = extractOptionsFromChildren(this);
                    if (extracted.length) {
                        this.set('options', extracted, { silent: true });
                    } else {
                        const normalized = normalizeOptions(this.get('options') || []);
                        if (normalized.length) {
                            this.set('options', normalized, { silent: true });
                        }
                    }
                    this.set('_bootstrapped', true);
                }

                this.on('change:options', () => {
                    const normalized = normalizeOptions(this.get('options') || []);
                    this.set('options', normalized, { silent: true });
                    syncOptionsToChildren(this);
                });

                syncOptionsToChildren(this);
            },

            toHTML(...args) {
                syncOptionsToChildren(this);
                return this.constructor.__super__.toHTML.apply(this, args);
            }
        },

        view: {
            events: {
                mousedown: 'intercept',
                click: 'intercept',
                dblclick: 'openEditor',
            },

            intercept(e) {
                e.preventDefault();
                const ed = this.em.get('Editor');
                if (ed) ed.select(this.model);
                return false;
            },

            openEditor() {
                const ed = this.em.get('Editor');
                if (ed) openOptionsModal(ed, this.model);
            },

            onRender() {
                syncOptionsToChildren(this.model);
            }
        }
    });

    tm.addType('options-modal-launcher', {
        createInput: ({ component }) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'ds-btn ds-btn-block';
            btn.textContent = '開啟選項編輯器';
            btn.addEventListener('click', () => openOptionsModal(editor, component));
            return btn;
        }
    });

    editor.Commands.add('open-ds-editor', {
        run(ed) {
            const sel = ed.getSelected();
            if (sel && sel.get('type') === 'dynamic-select') {
                openOptionsModal(ed, sel);
            }
        }
    });

    editor.Keymaps.add('open-ds-editor', 'e', (ed, ev) => {
        const sel = ed.getSelected();
        if (sel && sel.get('type') === 'dynamic-select') {
            ev.preventDefault();
            ed.runCommand('open-ds-editor');
        }
    });
});