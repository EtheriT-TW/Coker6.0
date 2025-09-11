grapesjs.plugins.add('grapesJs-Coker6-Form', (editor, options) => {
    // ============ GrapesJS plugin: Dynamic Select with + button ============
    const bm = editor.BlockManager;
    const domc = editor.DomComponents;
    const tm = editor.TraitManager;

    // 工具：用 options 陣列同步 <option> 子節點
    const syncOptionsToChildren = (component) => {
        const opts = (component.get('options') || []).map(v => (v ?? '').toString());

        // 先清空子節點
        component.components().reset([]);

        const children = component.components();

        opts.forEach(txt => {
            // 新增 <option value="...">
            let optComp = children.add({ tagName: 'option', attributes: { value: txt } });
            // 某些版本回傳陣列，某些回傳單一 Component
            optComp = Array.isArray(optComp) ? optComp[0] : optComp;

            // 在 <option> 內放純文字（textnode）
            let textNode = optComp.components().add({ type: 'textnode', content: txt });
            textNode = Array.isArray(textNode) ? textNode[0] : textNode;
        });
    };
    function openOptionsModal(editor, component) {
        const modal = editor.Modal;

        // —— 容器（只掛 class，不寫行內樣式）——
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
        bulkLabel.textContent = '或貼上多行（每行一個選項，Ctrl + Enter 可快速加入）：';

        const bulkArea = document.createElement('textarea');
        bulkArea.className = 'ds-textarea';
        bulkArea.placeholder = '每一行會變成一個選項（會自動忽略空白行）';

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

        // —— 一列（含拖曳把手）——
        const makeRow = (text = '') => {
            const row = document.createElement('div');
            row.className = 'ds-row';

            const handle = document.createElement('span');
            handle.className = 'ds-handle';
            handle.textContent = '≡';
            handle.title = '拖曳以排序';

            const input = document.createElement('input');
            input.type = 'text';
            input.className = 'ds-input';
            input.value = text;
            input.placeholder = '輸入選項文字';

            const del = document.createElement('button');
            del.type = 'button';
            del.className = 'ds-del';
            del.setAttribute('aria-label', '刪除');
            del.textContent = '×';
            del.addEventListener('click', () => row.remove());

            row.appendChild(handle);
            row.appendChild(input);
            row.appendChild(del);
            return row;
        };

        // 依現有 options 畫出列表
        const opts = (component.get('options') || []).map(v => (v ?? '').toString());
        if (opts.length === 0) list.appendChild(makeRow('選項1'));
        else opts.forEach(v => list.appendChild(makeRow(v)));

        // 新增
        function importFromTextarea(bulkArea, list, makeRow) {
            const buf = (bulkArea.value || '').trim();
            if (!buf) return 0;
            const lines = buf.split(/\r?\n/).map(s => s.trim()).filter(Boolean);
            lines.forEach(v => list.appendChild(makeRow(v)));
            bulkArea.value = '';
            return lines.length;
        }
        // 「新增一列」：若 textarea 有內容，先匯入，不另外新增空白列
        addBtn.addEventListener('click', () => {
            const added = importFromTextarea(bulkArea, list, makeRow);
            if (added > 0) return;                 // 已匯入多行，就不再加空白
            list.appendChild(makeRow(''));         // 否則就加一列空白
        });

        // 可選：在 textarea 內 Ctrl+Enter / Cmd+Enter 直接匯入（不用移開焦點）
        bulkArea.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                importFromTextarea(bulkArea, list, makeRow);
            }
        });

        // 取消
        btnCancel.addEventListener('click', () => modal.close());

        // 儲存：回寫 options 並同步 <option>
        btnSave.addEventListener('click', () => {
            const inputs = Array.from(list.querySelectorAll('input.ds-input'));
            const values = inputs.map(i => (i.value || '').trim()).filter(Boolean);
            component.set('options', values);     // 觸發你的 change:options
            modal.close();
        });

        // 開啟 Modal
        modal.open({ title: '編輯下拉選項', content: wrap });

        // —— 啟用 jQuery UI Sortable（用把手拖曳）——
        const $ = window.jQuery;
        if ($ && $.fn && $.fn.sortable) {
            setTimeout(() => {
                try { $(list).sortable('destroy'); } catch { }
                $(list).sortable({
                    items: '.ds-row',
                    handle: '.ds-handle',
                    axis: 'y',
                    tolerance: 'pointer',
                    placeholder: 'ds-placeholder',
                    forcePlaceholderSize: true,
                    start: (e, ui) => ui.placeholder.height(ui.item.outerHeight()),
                });
                try { $(list).disableSelection && $(list).disableSelection(); } catch { }
            }, 0);
        }
    }

    // 從現有子節點抽取 <option> 的 value/文字 → 轉成 options 陣列
    const extractOptionsFromChildren = (component) => {
        const res = [];
        const children = component.components && component.components();
        if (!children || !children.length) return res;

        children.each(ch => {
            const tag = (ch.get && ch.get('tagName')) || '';
            if ((tag || '').toLowerCase() !== 'option') return;

            const attrs = (ch.getAttributes && ch.getAttributes()) || ch.get('attributes') || {};
            const val = attrs && attrs.value;

            // 找 textnode 內容當作顯示文字
            let txt = '';
            const tnode = ch.components && ch.components().models.find(m => {
                const t = m.get && (m.get('type') || m.get('tagName'));
                return t === 'textnode';
            });
            if (tnode) txt = tnode.get && tnode.get('content');

            res.push(((val != null ? val : txt) || '').toString());
        });

        return res;
    };

    // 定義 component type
    domc.addType('dynamic-select', {
        isComponent: el => el.tagName === 'SELECT',
        model: {
            defaults: {
                tagName: 'select',
                attributes: {
                    'data-dynamic-select': '1',
                },
                // 預設一個選項，value 與 text 同值
                options: [],
                // 讓 Style Manager 比較好用
                traits: [
                    { type: 'options-modal-launcher', label: '選項' } // 自訂 trait（見下）
                ],
                toolbar: [
                    { attributes: { class: 'fa fa-edit', title: '編輯選項 (E)' }, command: 'open-ds-editor' },
                    // 可以保留複製/刪除的預設按鈕
                ]
            },

            // 初始化時把 options 同步成 <option>
            init() {
                if (!this.get('_bootstrapped')) {
                    const extracted = extractOptionsFromChildren(this);
                    if (extracted.length) this.set('options', extracted, { silent: true });
                    this.set('_bootstrapped', true);
                }
                this.on('change:options', () => syncOptionsToChildren(this));
                // 第一次同步
                syncOptionsToChildren(this);
            },

            // 匯出 HTML 時確保 children 跟 options 一致
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

            // 攔截原生互動，同時強制選取這個 component
            intercept(e) {
                e.preventDefault(); // 不讓原生 select 展開
                const ed = this.em.get('Editor');
                if (ed) ed.select(this.model); // ← 讓它在 GrapesJS 視為「已選取」
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

    // Trait：只放一顆按鈕 → 按下開啟 Modal
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


    // Block：一鍵插入
    /*bm.add('dynamic-select', {
        label: '下拉選單',
        category: '表單',
        content: {
            type: 'dynamic-select',
            attributes: { class: 'gjs-dyn-select' },
            // 初始內文會被 options 覆蓋，不用放 children
        }
    });*/

    editor.Commands.add('open-ds-editor', {
        run(ed) {
            const sel = ed.getSelected();
            if (sel && sel.get('type') === 'dynamic-select') openOptionsModal(ed, sel);
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