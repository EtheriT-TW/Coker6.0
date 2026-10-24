// The host provides permissions, draft identity and transport through options/adapter.
// This plugin never knows MVC routes, antiforgery headers or Parallel credentials.
export function aiAssistantPlugin(editor, options = {}) {
    const adapter = options.adapter;
    if (options.enabled !== true || typeof adapter?.ai?.generate !== 'function' || !options.draftKey) return;
    const panel = document.createElement('aside');
    panel.className = 'coker-canvas-ai';
    panel.hidden = true;
    panel.setAttribute('aria-label', 'AI 輔助畫布');
    panel.innerHTML = `<header class="coker-canvas-ai-header"><strong>AI 輔助畫布</strong><button type="button" data-close aria-label="關閉 AI 輔助畫布" title="關閉">&times;</button></header>
        <div class="coker-canvas-ai-body">
        <p class="coker-canvas-ai-note">內容會傳送至 AI 服務。套用後請使用原有儲存功能。</p>
        <label>修改範圍<select data-scope><option value="selected">目前選取區塊</option><option value="canvas">整個畫布</option><option value="new">產生全新 layout</option></select></label>
        <label>修改方式<select data-edit-mode><option value="style">只調整樣式（保留原文）</option><option value="content">調整內容與排版</option></select></label>
        <div class="coker-canvas-ai-log" data-log aria-live="polite"></div>
        <label>需求<textarea data-prompt rows="4" maxlength="4000" placeholder="例如：選取區塊的標題放大一級，三欄改成四欄"></textarea></label>
        <div class="coker-canvas-ai-actions"><button type="button" data-send>產生並套用</button><button type="button" data-cancel disabled>取消</button><button type="button" data-clear>清除對話</button></div>
        <p class="coker-canvas-ai-status" data-status role="status"></p>
        <section class="coker-canvas-ai-history"><div class="coker-canvas-ai-section-title"><strong>本機版本</strong><span>最多 20 筆・尚未發布</span></div>
        <label>版本<select data-versions></select></label>
        <div class="coker-canvas-ai-actions"><button type="button" data-prev>← 上一版</button><button type="button" data-next>下一版 →</button><button type="button" data-checkpoint>保存草稿</button></div>
        <button type="button" data-delete>清除本機紀錄</button></section></div>`;
    const find = selector => panel.querySelector(selector);
    const status = message => { find('[data-status]').textContent = message; };
    let versions = [], conversation = [], activeKey = '', pending = null, restoring = false, destroyed = false;
    let serviceUnavailable = false;
    let recordedProject = null, currentVersionIndex = -1;
    let recordedContent = null, switching = false;
    const contentSignature = () => JSON.stringify({ html: editor.getHtml(), css: editor.getCss() });
    const updateNavigation = () => {
        find('[data-prev]').disabled = !!pending || switching || !versions.length || currentVersionIndex === 0;
        find('[data-next]').disabled = !!pending || switching || currentVersionIndex < 0 || currentVersionIndex >= versions.length - 1;
    };
    const getKey = () => {
        const identity = typeof options.draftKey === 'function' ? options.draftKey(editor) : options.draftKey;
        if (!identity) throw new Error('AI 草稿缺少內容識別資訊。');
        return `coker-ai-v1:${identity}`;
    };
    const confirm = message => Promise.resolve(adapter.ui?.confirm?.(message) ?? window.confirm(message));
    const render = () => {
        const select = find('[data-versions]');
        select.replaceChildren();
        versions.forEach((v, index) => select.add(new Option(`${v.date} ${v.label}`, String(index))));
        if (currentVersionIndex < 0) select.add(new Option('目前編輯內容', '-1'), 0);
        select.value = String(currentVersionIndex);
        find('[data-log]').textContent = conversation.map(m => `${m.role === 'user' ? '你' : 'AI'}：${m.content}`).join('\n');
        updateNavigation();
    };
    function sync() {
        const key = getKey();
        if (key === activeKey) return;
        activeKey = key;
        versions = []; conversation = []; recordedProject = null; recordedContent = null; currentVersionIndex = -1;
        try {
            const saved = JSON.parse(localStorage.getItem(activeKey) || 'null');
            if (saved && Array.isArray(saved.versions)) versions = saved.versions.slice(-20);
        } catch { status('本機紀錄無法讀取；畫布仍可編輯。'); }
        const currentProject = JSON.stringify(editor.getProjectData());
        currentVersionIndex = versions.findIndex(version => JSON.stringify(version.project) === currentProject);
        if (currentVersionIndex >= 0) { recordedProject = currentProject; recordedContent = contentSignature(); }
        render();
    }
    function checkpoint(label) {
        if (destroyed || restoring) return;
        // A changed resource must never receive another resource's snapshots.
        sync();
        const project = JSON.parse(JSON.stringify(editor.getProjectData()));
        const serializedProject = JSON.stringify(project);
        if (recordedProject === serializedProject) return;
        recordedProject = serializedProject;
        recordedContent = contentSignature();
        const existingIndex = versions.findIndex(version => JSON.stringify(version.project) === serializedProject);
        if (existingIndex >= 0) { currentVersionIndex = existingIndex; render(); return; }
        versions.push({ date: new Date().toLocaleString(), label, project });
        versions = versions.slice(-20);
        currentVersionIndex = versions.length - 1;
        try { localStorage.setItem(activeKey, JSON.stringify({ versions })); }
        catch { status('本機儲存空間不足或不可用；版本目前僅保留在此編輯器。'); }
        render();
    }
    function setBusy(busy) {
        panel.querySelectorAll('button, select, textarea').forEach(el => { el.disabled = busy; });
        find('[data-close]').disabled = false;
        find('[data-cancel]').disabled = !busy;
        if (serviceUnavailable) {
            find('[data-send]').disabled = true;
            find('[data-scope]').disabled = true;
            find('[data-prompt]').disabled = true;
        }
        updateNavigation();
    }
    find('[data-close]').onclick = () => { panel.hidden = true; };
    find('[data-cancel]').onclick = () => pending?.abort();
    find('[data-clear]').onclick = () => { conversation = []; render(); };
    find('[data-checkpoint]').onclick = () => { checkpoint('手動草稿'); status('已保存本機草稿，尚未儲存至伺服器。'); };
    find('[data-delete]').onclick = async () => {
        sync();
        const deleteKey = activeKey;
        if (!await confirm('清除此內容的所有本機版本？')) return;
        if (destroyed || deleteKey !== getKey()) return;
        try { localStorage.removeItem(activeKey); versions = []; currentVersionIndex = -1; render(); }
        catch { status('無法清除本機紀錄。'); }
    };
    async function switchVersion(index) {
        if (pending || switching || destroyed) return;
        sync();
        const version = versions[index];
        const restoreKey = activeKey;
        if (!version?.project) return;
        switching = true;
        updateNavigation();
        const dirty = recordedContent !== contentSignature();
        const approved = !dirty || await confirm('目前有其他修改尚未保存為版本，切換將取代這些修改。是否繼續？');
        switching = false;
        if (!approved) { render(); return; }
        if (destroyed || restoreKey !== getKey()) return;
        restoring = true;
        try { editor.loadProjectData(JSON.parse(JSON.stringify(version.project))); }
        catch { status('本機版本無法載入，請選擇其他版本。'); updateNavigation(); return; }
        finally { restoring = false; }
        // Restoring changes the current position, not the version history.
        // Also reset the comparison baseline so delayed updates/closing do not add a duplicate.
        recordedProject = JSON.stringify(editor.getProjectData());
        recordedContent = contentSignature();
        currentVersionIndex = versions.indexOf(version);
        render();
        status('已還原本機版本，尚未儲存至伺服器。');
    }
    find('[data-versions]').onchange = () => switchVersion(Number(find('[data-versions]').value));
    find('[data-prev]').onclick = () => switchVersion(currentVersionIndex < 0 ? versions.length - 1 : currentVersionIndex - 1);
    find('[data-next]').onclick = () => switchVersion(currentVersionIndex + 1);
    find('[data-send]').onclick = async () => {
        if (serviceUnavailable) return;
        sync();
        const prompt = find('[data-prompt]').value.trim();
        const scope = find('[data-scope]').value;
        const selected = editor.getSelected();
        if (!prompt) return status('請輸入需求。');
        if (scope === 'selected' && (!selected || selected === editor.getWrapper())) return status('請先選取要修改的區塊。');
        if (scope === 'new' && !await confirm('產生全新 layout 將取代畫布內容，是否繼續？')) return;
        if (destroyed || pending) return;
        const preserveContent = scope !== 'new' && find('[data-edit-mode]').value === 'style';
        const cleanBeforePreparingIds = recordedContent === contentSignature();
        const targets = scope === 'new' ? [] : scope === 'selected'
            ? [selected, ...selected.find('*')] : editor.getWrapper().find('*');
        // Explicit IDs let the AI patch CSS without rebuilding components or losing their styles.
        const styleContext = targets.slice(0, 40).map(component => {
            const id = component.getId();
            component.addAttributes({ id });
            const element = component.getEl();
            const computed = element?.ownerDocument.defaultView.getComputedStyle(element);
            return { selector: `#${id}`, properties: Object.fromEntries(
                ['font-size', 'font-weight', 'font-style', 'font-family', 'line-height', 'color', 'display']
                    .map(name => [name, computed?.getPropertyValue(name) || ''])) };
        });
        const originalProject = JSON.stringify(editor.getProjectData());
        // Adding target IDs is preparation by the plugin, not an unsaved user edit.
        if (cleanBeforePreparingIds) recordedContent = contentSignature();
        const requestKey = activeKey;
        const html = scope === 'new' ? '' : scope === 'selected' ? selected.toHTML() : editor.getHtml();
        if (!versions.length) checkpoint('初始版本');
        pending = new AbortController();
        setBusy(true); status('AI 產生中，可能需要數分鐘…');
        try {
            const result = await adapter.ai.generate({
                scope, prompt, html,
                preserveContent, styleContext: JSON.stringify(styleContext),
                css: scope === 'new' ? '' : editor.getCss(scope === 'selected' ? { component: selected } : {}),
                history: JSON.stringify(conversation).slice(-8000)
            }, { signal: pending.signal });
            // A host adapter may finish after cancellation even if it ignores AbortSignal.
            if (pending.signal.aborted) throw new DOMException('Aborted', 'AbortError');
            if (destroyed) return;
            if (requestKey !== getKey() || originalProject !== JSON.stringify(editor.getProjectData()) ||
                (scope === 'selected' && !selected.parent()))
                throw new Error('等待期間畫布已修改或切換，未套用 AI 回覆，請重新送出。');
            const replaceHtml = !preserveContent || result.htmlChanged === true;
            const safeHtml = replaceHtml ? validateHtml(result.html) : null;
            if (preserveContent && replaceHtml) {
                const parse = markup => new DOMParser().parseFromString(markup, 'text/html').body.textContent;
                if (parse(html) !== parse(safeHtml)) throw new Error('局部樣式修改不可改寫原文，未套用回覆。');
            }
            let rules = validateStyles(result.styles, replaceHtml ? safeHtml : html);
            // For an explicit one-level font request, enforce the direction against rendered CSS.
            // Keep all unrelated properties (bold, italic, family, etc.) untouched.
            const fontStep = preserveContent && !result.htmlChanged && /字|文字|字體|font/i.test(prompt) && /一級|一级|one level/i.test(prompt)
                ? /縮小|缩小|變小|变小|小一級|小一级|smaller/i.test(prompt) ? -1
                    : /放大|變大|变大|大一級|大一级|larger/i.test(prompt) ? 1 : 0 : 0;
            if (fontStep) {
                rules = rules.filter(rule => Object.hasOwn(rule.properties, 'font-size'));
                if (!rules.length) throw new Error('AI 未提供字級規則，已保留畫布。');
                for (const rule of rules) {
                    const baseline = styleContext.find(item => item.selector === rule.selector)?.properties['font-size'];
                    const size = parseFloat(baseline);
                    if (!Number.isFinite(size)) throw new Error('無法確認原始字級，已保留畫布。');
                    rule.properties = { 'font-size': `${Math.max(1, size + fontStep * 4)}px` };
                }
            }
            const beforeApply = JSON.parse(originalProject);
            const beforeContent = contentSignature();
            restoring = true;
            try {
                if (replaceHtml) {
                    if (scope === 'selected') selected.replaceWith(safeHtml);
                    else editor.setComponents(safeHtml);
                }
                for (const rule of rules) editor.Css.setRule(rule.selector, rule.properties, { addStyles: true });
                if (contentSignature() === beforeContent) throw new Error('回覆未產生任何 HTML 或 CSS 差異，畫布保持不變。');
            } catch (error) {
                editor.loadProjectData(beforeApply);
                throw error;
            } finally { restoring = false; }
            conversation.push({ role: 'user', content: prompt }, { role: 'assistant', content: result.message });
            conversation = conversation.slice(-8);
            checkpoint('AI：' + prompt.slice(0, 50));
            find('[data-prompt]').value = '';
            status('已套用，請檢查桌面／手機預覽並使用原有儲存功能。');
        } catch (error) {
            if (!destroyed && (error.serviceUnavailable === true || error instanceof TypeError || error instanceof SyntaxError)) {
                serviceUnavailable = true;
                editor.Panels.removeButton('options', 'coker-ai-assistant');
            }
            if (!destroyed) status(error.name === 'AbortError' ? '已取消，未套用 AI 回覆。' : error.message);
        } finally {
            pending = null;
            if (!destroyed) setBusy(false);
        }
    };
    // The browser parser is the final authority before GrapesJS parses untrusted markup.
    function validateHtml(html) {
        if (typeof html !== 'string' || !html.trim()) throw new Error('AI 未回傳有效 HTML。');
        const doc = new DOMParser().parseFromString(html, 'text/html');
        const tags = new Set('SECTION ARTICLE HEADER FOOTER NAV DIV SPAN P H1 H2 H3 H4 H5 H6 UL OL LI A IMG PICTURE SOURCE STRONG EM B I U SMALL BR HR BLOCKQUOTE FIGURE FIGCAPTION TABLE THEAD TBODY TFOOT TR TH TD DL DT DD BUTTON'.split(' '));
        const attrs = new Set('id class title role alt href src sizes width height loading decoding target rel type colspan rowspan scope'.split(' '));
        for (const el of doc.body.querySelectorAll('*')) {
            if (!tags.has(el.tagName)) throw new Error('AI HTML 包含不支援的元件。');
            for (const a of el.attributes) {
                if (!attrs.has(a.name) && !a.name.startsWith('aria-')) throw new Error('AI HTML 包含不支援的屬性。');
                if (['href', 'src'].includes(a.name)) {
                    const value = a.value.trim();
                    if (/[\x00-\x20\\]/.test(value) || value.startsWith('//') ||
                        !(value.startsWith('/') || value.startsWith('#') || /^https:\/\//i.test(value)))
                        throw new Error('AI HTML 包含不安全的連結。');
                }
            }
            if (el.tagName === 'A' && el.target === '_blank') el.rel = 'noopener noreferrer';
            if (el.tagName === 'BUTTON') el.type = 'button';
        }
        if (doc.head.children.length) throw new Error('AI HTML 不可包含外部樣式或頁面標頭。');
        return doc.body.innerHTML;
    }
    function validateStyles(rules, html) {
        if (!Array.isArray(rules) || rules.length > 100) throw new Error('AI 未提供有效 CSS 修改，請更新 MVC 與插件後重試。');
        const doc = new DOMParser().parseFromString(html, 'text/html');
        const ids = new Set([...doc.body.querySelectorAll('[id]')].map(el => el.id));
        const allowed = new Set('color background-color font-family font-size font-weight font-style line-height letter-spacing text-align text-decoration margin margin-top margin-bottom margin-left margin-right padding padding-top padding-bottom padding-left padding-right border border-radius width max-width min-width height max-height display gap grid-template-columns flex-direction flex-wrap justify-content align-items'.split(' '));
        for (const rule of rules) {
            if (!rule || !/^#[a-zA-Z_][a-zA-Z0-9_-]*$/.test(rule.selector) || !ids.has(rule.selector.slice(1)) || !rule.properties ||
                Object.entries(rule.properties).some(([name, value]) => !allowed.has(name) || typeof value !== 'string' || value.length > 300 ||
                    !/^[\p{L}0-9\s#%.,()'"/+-]+$/u.test(value) || /url|expression|javascript|import/i.test(value)))
                throw new Error('AI CSS 不符合安全或區塊範圍限制。');
        }
        return rules;
    }
    const mount = () => {
        if (destroyed) return;
        editor.getContainer().append(panel);
        editor.Panels.addButton('options', {
            id: 'coker-ai-assistant', label: 'AI', togglable: false, attributes: { title: 'AI 輔助畫布' },
            command: () => { panel.hidden = !panel.hidden; if (!panel.hidden) { sync(); if (!versions.length) checkpoint('初始版本'); } }
        });
    };
    editor.on('load', mount);
    editor.on('destroy', () => {
        destroyed = true; pending?.abort(); panel.remove();
    });
}
