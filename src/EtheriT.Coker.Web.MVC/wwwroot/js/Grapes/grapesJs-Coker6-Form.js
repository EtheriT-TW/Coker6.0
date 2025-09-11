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
                    { type: 'options-builder', label: '選項' } // 自訂 trait（見下）
                ],
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
            onRender() {
                // 確保首次 render 也同步
                syncOptionsToChildren(this.model);
            }
        }
    });

    // 自訂 Trait：選項管理器（帶＋按鈕
    // 自訂 Trait：選項管理器（＋按鈕＋拖曳排序 with jQuery UI）
    tm.addType('options-builder', {
        createInput({ trait, component }) {
            const root = document.createElement('div');
            root.style.display = 'grid';
            root.style.gap = '8px';

            // 選項清單容器
            const listWrap = document.createElement('div');
            listWrap.style.display = 'grid';
            listWrap.style.gap = '6px';
            listWrap.setAttribute('data-optlist', '1');

            // ＋ 新增
            const addBtn = document.createElement('button');
            addBtn.type = 'button';
            addBtn.textContent = '＋ 新增選項';
            addBtn.style.padding = '6px 10px';
            addBtn.style.borderRadius = '6px';
            addBtn.style.border = '1px solid #ddd';
            addBtn.style.cursor = 'pointer';
            addBtn.style.background = '#f7f7f7';

            // 渲染單列（含拖曳把手）
            const renderRow = (v, i) => {
                const row = document.createElement('div');
                row.className = 'gjs-opt-row';
                row.style.display = 'grid';
                row.style.gridTemplateColumns = 'auto 1fr auto'; // 把手 / 輸入框 / 刪除
                row.style.alignItems = 'center';
                row.style.gap = '6px';

                // 拖曳把手
                const handle = document.createElement('span');
                handle.className = 'gjs-opt-handle';
                handle.textContent = '≡';
                handle.title = '拖曳以排序';
                handle.style.cursor = 'move';
                handle.style.userSelect = 'none';
                handle.style.padding = '0 6px';
                handle.style.color = '#666';
                handle.style.border = '1px dashed #ccc';
                handle.style.borderRadius = '4px';

                // 輸入框
                const input = document.createElement('input');
                input.type = 'text';
                input.value = v || '';
                input.placeholder = '輸入選項文字';
                input.style.padding = '6px 8px';
                input.style.border = '1px solid #ddd';
                input.style.borderRadius = '6px';
                input.addEventListener('input', () => {
                    const cur = [...(component.get('options') || [])];
                    cur[i] = input.value;
                    component.set('options', cur);
                });

                // 刪除
                const del = document.createElement('button');
                del.type = 'button';
                del.textContent = '刪除';
                del.style.padding = '6px 10px';
                del.style.borderRadius = '6px';
                del.style.border = '1px solid #ddd'; // ← 修正這行
                del.style.cursor = 'pointer';
                del.style.background = '#fff';
                del.addEventListener('click', () => {
                    const cur = [...(component.get('options') || [])];
                    cur.splice(i, 1);
                    component.set('options', cur);
                    redrawList();
                });

                row.appendChild(handle);
                row.appendChild(input);
                row.appendChild(del);
                return row;
            };

            // 套用/重設 jQuery UI sortable（延後到 DOM 掛載後）
            const initSortable = () => {
                const $ = window.jQuery;
                if (!$ || !$.fn || !$.fn.sortable) return; // 沒載 jQuery UI 就略過
                // 用 setTimeout 讓 root 已經插入 Traits 面板的 DOM 再初始化
                setTimeout(() => {
                    try { $(listWrap).sortable('destroy'); } catch { }
                    $(listWrap).sortable({
                        items: '.gjs-opt-row',
                        handle: '.gjs-opt-handle',
                        axis: 'y',
                        tolerance: 'pointer',
                        placeholder: 'gjs-opt-placeholder',
                        forcePlaceholderSize: true,
                        cancel: 'input,textarea,button,select',
                        start: function (e, ui) { ui.placeholder.height(ui.item.outerHeight()); },
                        update: function () {
                            // 依 DOM 順序回寫 options
                            const inputs = listWrap.querySelectorAll('input[type="text"]');
                            const values = Array.from(inputs).map(i => i.value);
                            component.set('options', values);
                        }
                    });
                    try { $(listWrap).disableSelection && $(listWrap).disableSelection(); } catch { }
                }, 0);
            };

            const redrawList = () => {
                listWrap.innerHTML = '';
                const opts = component.get('options') || [];
                opts.forEach((v, i) => listWrap.appendChild(renderRow(v, i)));
                initSortable();
            };

            addBtn.addEventListener('click', () => {
                const opts = [...(component.get('options') || [])];
                const base = '選項';
                let n = opts.length + 1;
                let candidate = `${base}${n}`;
                while (opts.includes(candidate)) { n += 1; candidate = `${base}${n}`; }
                opts.push(candidate);
                component.set('options', opts);
                redrawList();
            });

            // 初次渲染
            redrawList();

            root.appendChild(listWrap);
            root.appendChild(addBtn);
            return root; // 這個會被傳進 onUpdate 的 elInput
        },

        // 相容 elInput/el；重畫後重新綁 sortable（同樣用延後初始化）
        onUpdate(args) {
            const { component } = args;
            const root = args.elInput || args.el || null;
            if (!root) return;

            const listWrap = root.querySelector('[data-optlist]');
            if (!listWrap) return;

            // 讓 list 可拖曳排序（jQuery UI）
            const initSortable = () => {
                const $ = window.jQuery;
                if (!($ && $.fn && $.fn.sortable)) return;
                setTimeout(() => {
                    try { $(listWrap).sortable('destroy'); } catch { }
                    $(listWrap).sortable({
                        items: '.gjs-opt-row',
                        handle: '.gjs-opt-handle',
                        axis: 'y',
                        tolerance: 'pointer',
                        placeholder: 'gjs-opt-placeholder',
                        forcePlaceholderSize: true,
                        cancel: 'input,textarea,button,select',
                        start: function (e, ui) { ui.placeholder.height(ui.item.outerHeight()); },
                        update: () => {
                            // 依 DOM 順序回寫 options
                            const inputs = listWrap.querySelectorAll('input[type="text"]');
                            const values = Array.from(inputs).map(i => i.value);
                            component.set('options', values);
                        }
                    });
                    try { $(listWrap).disableSelection && $(listWrap).disableSelection(); } catch { }
                }, 0);
            };

            const renderRow = (v, i) => {
                const row = document.createElement('div');
                row.className = 'gjs-opt-row';
                row.style.display = 'grid';
                row.style.gridTemplateColumns = 'auto 1fr auto';
                row.style.alignItems = 'center';
                row.style.gap = '6px';

                // 把手
                const handle = document.createElement('span');
                handle.className = 'gjs-opt-handle';
                handle.textContent = '≡';
                handle.title = '拖曳以排序';
                handle.style.cursor = 'move';
                handle.style.userSelect = 'none';
                handle.style.padding = '0 6px';
                handle.style.color = '#666';
                handle.style.border = '1px dashed #ccc';
                handle.style.borderRadius = '4px';

                // 輸入框
                const input = document.createElement('input');
                input.type = 'text';
                input.value = v || '';
                input.placeholder = '輸入選項文字';
                input.style.padding = '6px 8px';
                input.style.border = '1px solid #ddd';
                input.style.borderRadius = '6px';
                input.addEventListener('input', () => {
                    const cur = [...(component.get('options') || [])];
                    cur[i] = input.value;
                    component.set('options', cur);
                });

                // 刪除（重點：刪完立即 redraw，不再等待 onUpdate）
                const del = document.createElement('button');
                del.type = 'button';
                del.textContent = '刪除';
                del.style.padding = '6px 10px';
                del.style.borderRadius = '6px';
                del.style.border = '1px solid #ddd';
                del.style.cursor = 'pointer';
                del.style.background = '#fff';
                del.addEventListener('click', () => {
                    const cur = [...(component.get('options') || [])];
                    cur.splice(i, 1);
                    component.set('options', cur);
                    redrawList(); // ← 立刻重畫，保證一開始就生效
                });

                row.appendChild(handle);
                row.appendChild(input);
                row.appendChild(del);
                return row;
            };

            const redrawList = () => {
                listWrap.innerHTML = '';
                const opts = component.get('options') || [];
                opts.forEach((v, i) => listWrap.appendChild(renderRow(v, i)));
                initSortable();
            };

            // 首次/每次 onUpdate 都完整重畫一次
            redrawList();
        }
    });

    // Block：一鍵插入
    bm.add('dynamic-select', {
        label: '下拉選單',
        category: '表單',
        content: {
            type: 'dynamic-select',
            attributes: { class: 'gjs-dyn-select' },
            // 初始內文會被 options 覆蓋，不用放 children
        }
    });
});