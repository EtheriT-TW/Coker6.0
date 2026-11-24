// The host provides permissions, draft identity and transport through options/adapter.
// This plugin never knows MVC routes, antiforgery headers or Parallel credentials.
export function aiAssistantPlugin(editor, options = {}) {
    const adapter = options.adapter;
    if (options.enabled !== true || typeof adapter?.ai?.generate !== 'function' || !options.draftKey) return;
    const panel = document.createElement('aside');
    panel.className = 'coker-canvas-ai';
    panel.hidden = true;
    panel.setAttribute('aria-label', 'AI 輔助畫布');
    panel.innerHTML = `<div class="coker-canvas-ai-header" role="group" aria-label="AI 助理工具列"><strong>AI 輔助畫布</strong><div class="coker-canvas-ai-actions"><button type="button" data-undo title="還原上次 AI 修改前" aria-label="還原上次 AI 修改前">↶</button><button type="button" data-settings aria-label="設定與版本紀錄" title="設定與版本紀錄" aria-haspopup="dialog" aria-controls="coker-ai-settings">&#9881;</button><button type="button" data-close aria-label="關閉 AI 輔助畫布" title="關閉">&times;</button></div></div>
        <div class="coker-canvas-ai-body">
        <dialog class="coker-canvas-ai-settings" id="coker-ai-settings" data-settings-panel aria-labelledby="coker-ai-settings-title">
        <div class="coker-canvas-ai-header"><strong id="coker-ai-settings-title">設定與版本紀錄</strong><button type="button" data-settings-close aria-label="關閉設定" title="關閉">&times;</button></div>
        <p data-current-version></p><p>版本只保留在此瀏覽器；還原後仍需儲存頁面。</p>
        <label>選擇還原版本<select data-versions></select></label>
        <button type="button" data-version-restore>還原選取版本</button>
        <button type="button" data-delete>清除本機內容</button></dialog>
        <div class="coker-canvas-ai-log" data-log role="log" aria-label="AI 對話" aria-live="polite"></div>
        <div class="coker-canvas-ai-composer"><p class="coker-canvas-ai-selection" data-selection></p>
        <label>需求<textarea data-prompt rows="3" maxlength="4000" placeholder="告訴 AI，要如何調整選取的區塊…"></textarea></label>
        <div class="coker-canvas-ai-actions"><button type="button" data-send>送出</button><button type="button" data-cancel disabled>取消</button></div>
        <p class="coker-canvas-ai-status" data-status role="status"></p>
        <p class="coker-canvas-ai-note">內容會傳送至 AI 服務，修改後請儲存頁面。</p></div></div>`;
    const find = selector => panel.querySelector(selector);
    const status = message => { find('[data-status]').textContent = message; };
    let versions = [], conversation = [], activeKey = '', pending = null, restoring = false, destroyed = false;
    let serviceUnavailable = false;
    let recordedProject = null, currentVersionIndex = -1;
    let recordedContent = null, switching = false;
    let replyLocale = '', undoVersionId = '';
    const settingCommands = { link: 'coker-settings-link', directory: 'coker-settings-directory' };
    const settingLabels = { link: '設定連結', directory: '設定目錄' };
    const contentSignature = () => JSON.stringify({ html: editor.getHtml(), css: editor.getCss() });
    const updateNavigation = () => {
        panel.querySelectorAll('[data-restore], [data-versions], [data-delete], [data-version-restore]').forEach(el => { el.disabled = !!pending || switching; });
        find('[data-undo]').disabled = !!pending || switching || !versions.some(v => v.id === undoVersionId);
        find('[data-version-restore]').disabled = !!pending || switching || Number(find('[data-versions]').value) < 0 || !versions.length;
    };
    const updateSelection = () => {
        const selected = editor.getSelected();
        find('[data-selection]').textContent = selected && selected !== editor.getWrapper()
            ? `目前選取：${selected.getName()}` : '請先選取要調整的區塊';
    };
    const getKey = () => {
        const identity = typeof options.draftKey === 'function' ? options.draftKey(editor) : options.draftKey;
        if (!identity) throw new Error('AI 草稿缺少內容識別資訊。');
        return `coker-ai-v1:${identity}`;
    };
    const confirm = message => Promise.resolve(adapter.ui?.confirm?.(message) ?? window.confirm(message));
    const persist = () => {
        try { localStorage.setItem(activeKey, JSON.stringify({ versions, conversation, replyLocale, undoVersionId })); }
        catch { status('本機儲存空間不足或不可用；紀錄目前僅保留在此編輯器。'); }
    };
    const render = () => {
        if (recordedContent !== contentSignature()) currentVersionIndex = -1;
        const select = find('[data-versions]');
        select.replaceChildren();
        versions.forEach((v, index) => select.add(new Option(`${v.date} ${v.label}`, String(index))));
        if (currentVersionIndex < 0) select.add(new Option('目前編輯內容', '-1'), 0);
        select.value = String(currentVersionIndex);
        find('[data-current-version]').textContent = currentVersionIndex >= 0
            ? `目前版本：${versions[currentVersionIndex].date} ${versions[currentVersionIndex].label}`
            : '目前畫布與本機歷史版本不同；下次修改前會自動保留還原點。';
        const log = find('[data-log]');
        log.replaceChildren();
        for (const message of conversation) {
            const bubble = document.createElement('article');
            bubble.className = `coker-canvas-ai-message ${message.role === 'user' ? 'is-user' : 'is-assistant'}`;
            const who = document.createElement('strong');
            who.textContent = message.role === 'user' ? '你' : message.role === 'assistant' ? 'AI' : '紀錄';
            const text = document.createElement('p');
            text.textContent = message.content;
            bubble.append(who, text);
            const index = versions.findIndex(v => v.id && v.id === message.versionId);
            if (index >= 0) {
                const restore = document.createElement('button');
                restore.type = 'button'; restore.dataset.restore = '';
                restore.textContent = '還原此版';
                restore.onclick = () => switchVersion(index);
                bubble.append(restore);
            }
            const beforeIndex = versions.findIndex(v => v.id && v.id === message.beforeVersionId);
            if (beforeIndex >= 0) {
                const undo = document.createElement('button');
                undo.type = 'button'; undo.dataset.restore = '';
                undo.textContent = '還原這次修改前';
                undo.onclick = () => switchVersion(beforeIndex);
                bubble.append(undo);
            }
            for (const action of Array.isArray(message.settingsActions) ? message.settingsActions.slice(0, 1) : []) {
                if (!Object.hasOwn(settingCommands, action?.kind)) continue;
                const configure = document.createElement('button');
                configure.type = 'button'; configure.dataset.restore = '';
                configure.textContent = settingLabels[action.kind];
                configure.onclick = () => openSettings(action);
                bubble.append(configure);
            }
            log.append(bubble);
        }
        if (!conversation.length) log.textContent = '選取畫布上的區塊，告訴 AI 你想怎麼調整。';
        log.scrollTop = log.scrollHeight;
        updateSelection();
        updateNavigation();
    };
    function sync() {
        const key = getKey();
        if (key === activeKey) return;
        activeKey = key;
        versions = []; conversation = []; recordedProject = null; recordedContent = null; currentVersionIndex = -1;
        replyLocale = ''; undoVersionId = '';
        try {
            const saved = JSON.parse(localStorage.getItem(activeKey) || 'null');
            replyLocale = typeof saved?.replyLocale === 'string' ? saved.replyLocale : '';
            undoVersionId = typeof saved?.undoVersionId === 'string' ? saved.undoVersionId : '';
            if (saved && Array.isArray(saved.versions)) versions = saved.versions
                .filter(v => v && v.project && typeof v.label === 'string').slice(-20)
                .map(v => ({ ...v, id: v.id || crypto.randomUUID() }));
            if (saved && Array.isArray(saved.conversation)) conversation = saved.conversation
                .filter(m => m && ['user', 'assistant', 'system'].includes(m.role) && typeof m.content === 'string').slice(-200);
        } catch { status('本機紀錄無法讀取；畫布仍可編輯。'); }
        const currentProject = JSON.stringify(editor.getProjectData());
        currentVersionIndex = versions.findIndex(version => JSON.stringify(version.project) === currentProject);
        if (currentVersionIndex >= 0) { recordedProject = currentProject; recordedContent = contentSignature(); }
        // Loading chat history must never imply that the latest AI version is on the canvas.
        render();
    }
    function checkpoint(label) {
        if (destroyed || restoring) return;
        // A changed resource must never receive another resource's snapshots.
        sync();
        const project = JSON.parse(JSON.stringify(editor.getProjectData()));
        const serializedProject = JSON.stringify(project);
        if (recordedProject === serializedProject && currentVersionIndex >= 0) return versions[currentVersionIndex]?.id;
        recordedProject = serializedProject;
        recordedContent = contentSignature();
        const existingIndex = versions.findIndex(version => JSON.stringify(version.project) === serializedProject);
        if (existingIndex >= 0) { currentVersionIndex = existingIndex; render(); return versions[existingIndex].id; }
        const id = crypto.randomUUID();
        versions.push({ id, date: new Date().toLocaleString(), label, project });
        versions = versions.slice(-20);
        currentVersionIndex = versions.length - 1;
        persist();
        render();
        return id;
    }
    function setBusy(busy) {
        panel.querySelectorAll('button, select, textarea').forEach(el => { el.disabled = busy; });
        find('[data-close]').disabled = false;
        find('[data-cancel]').disabled = !pending;
        if (serviceUnavailable) {
            find('[data-send]').disabled = true;
            find('[data-prompt]').disabled = true;
        }
        updateNavigation();
    }
    find('[data-close]').onclick = () => { panel.hidden = true; };
    find('[data-cancel]').onclick = () => pending?.abort();
    find('[data-settings]').onclick = () => {
        sync(); render();
        find('[data-settings-panel]').showModal();
    };
    find('[data-settings-close]').onclick = () => find('[data-settings-panel]').close();
    find('[data-version-restore]').onclick = () => switchVersion(Number(find('[data-versions]').value));
    find('[data-versions]').onchange = updateNavigation;
    find('[data-undo]').onclick = () => switchVersion(versions.findIndex(v => v.id === undoVersionId));
    function openSettings(action) {
        if (pending || switching || destroyed || !action || !Object.hasOwn(settingCommands, action.kind) ||
            typeof action.targetId !== 'string' || !/^[a-zA-Z_][a-zA-Z0-9_-]*$/.test(action.targetId)) return;
        sync();
        const components = editor.getWrapper().find(`#${action.targetId}`);
        const component = components.length === 1 ? components[0] : null;
        const command = settingCommands[action.kind];
        if (!component?.parent() || !editor.Commands.get(command)) {
            status('原設定元件已變更或設定功能不可用，請重新選取元件。'); return;
        }
        const key = activeKey;
        const before = contentSignature();
        const beforeVersionId = checkpoint('設定前：' + settingLabels[action.kind]);
        const wasOpen = !panel.hidden;
        switching = true; setBusy(true);
        find('[data-settings-panel]').close();
        panel.hidden = true;
        let finished = false;
        let openError = '';
        const onClose = () => setTimeout(() => {
            if (finished) return;
            finished = true;
            switching = false;
            if (destroyed) return;
            setBusy(false);
            if (key !== getKey()) return;
            panel.hidden = !wasOpen;
            if (contentSignature() !== before) {
                const versionId = checkpoint('設定後：' + settingLabels[action.kind]);
                undoVersionId = beforeVersionId || '';
                conversation.push({ role: 'system', content: '已返回 AI 助理，設定變更已保留，請儲存頁面。', versionId, beforeVersionId });
                conversation = conversation.slice(-200);
                persist(); render();
            }
            status(openError || '已返回 AI 助理；接下來會依目前設定繼續調整。');
            find('[data-prompt]').focus();
        }, 0);
        try {
            let opened = false;
            editor.runCommand(command, { component, onClose, onOpened: () => { opened = true; }, force: true });
            if (!opened) {
                openError = '此元件無法開啟設定，請重新選取元件。'; onClose();
            }
        } catch {
            openError = '設定視窗無法開啟，請使用元件原有設定功能。'; onClose();
        }
    }
    find('[data-delete]').onclick = async () => {
        if (pending || switching || destroyed) return;
        sync();
        const deleteKey = activeKey;
        switching = true; setBusy(true);
        const approved = await confirm('清除此頁面的本機對話與版本紀錄？目前畫布及已儲存的頁面不受影響。');
        switching = false; setBusy(false);
        if (!approved || destroyed || deleteKey !== getKey()) return;
        try { localStorage.removeItem(activeKey); versions = []; conversation = []; replyLocale = ''; undoVersionId = ''; recordedProject = null; recordedContent = null; currentVersionIndex = -1; render(); status('已清除本機內容。'); }
        catch { status('無法清除本機紀錄。'); }
    };
    async function switchVersion(index) {
        if (pending || switching || destroyed) return;
        sync();
        const version = versions[index];
        const restoreKey = activeKey;
        if (!version?.project) return;
        switching = true;
        setBusy(true);
        const dirty = recordedContent !== contentSignature();
        const approved = !dirty || await confirm('目前有其他修改尚未保存為版本，切換將取代這些修改。是否繼續？');
        switching = false;
        setBusy(false);
        if (!approved) { render(); return; }
        if (destroyed || restoreKey !== getKey()) return;
        // Keep an escape route even when restoring over manual edits or a newly loaded page.
        const previousId = checkpoint('還原操作前的畫布');
        if (!versions.includes(version)) { versions.unshift(version); versions.splice(1, Math.max(0, versions.length - 20)); }
        restoring = true;
        try { editor.loadProjectData(JSON.parse(JSON.stringify(version.project))); }
        catch { status('本機版本無法載入，請選擇其他版本。'); updateNavigation(); return; }
        finally { restoring = false; }
        // Restoring changes the current position, not the version history.
        // Also reset the comparison baseline so delayed updates/closing do not add a duplicate.
        recordedProject = JSON.stringify(editor.getProjectData());
        recordedContent = contentSignature();
        currentVersionIndex = versions.indexOf(version);
        undoVersionId = previousId || '';
        conversation.push({ role: 'system', content: `已還原至 ${version.date} ${version.label}` });
        conversation = conversation.slice(-200);
        persist();
        render();
        status('已還原本機版本，尚未儲存至伺服器。');
        find('[data-settings-panel]').close();
    }
    function contextTarget(selected) {
        const isLayout = component => {
            const el = component.getEl();
            if (!el || el.children.length < 2) return false;
            if (![...el.children].every(child => child.tagName === el.children[0].tagName)) return false;
            const computed = el.ownerDocument.defaultView.getComputedStyle(el);
            return el.classList.contains('row') || ['grid', 'inline-grid'].includes(computed.display) ||
                (['flex', 'inline-flex'].includes(computed.display) && computed.flexDirection.startsWith('row'));
        };
        if ([selected, ...selected.find('*')].some(isLayout)) return selected;
        let candidate = selected.parent();
        for (let depth = 0; candidate && candidate !== editor.getWrapper() && depth < 4; depth++, candidate = candidate.parent()) {
            if (isLayout(candidate)) return candidate;
        }
        return selected;
    }
    find('[data-send]').onclick = async () => {
        if (serviceUnavailable || pending || switching || destroyed) return;
        sync();
        const prompt = find('[data-prompt]').value.trim();
        const scope = 'selected';
        let selected = editor.getSelected();
        if (!prompt) return status('請輸入需求。');
        if (scope === 'selected' && (!selected || selected === editor.getWrapper())) return status('請先選取要修改的區塊。');
        if (destroyed || pending) return;
        const selectedId = selected.getId();
        const target = contextTarget(selected);
        const expanded = target !== selected;
        selected = target;
        if (expanded) editor.select(selected);
        const autoDetect = true;
        let preserveContent = autoDetect;
        const cleanBeforePreparingIds = recordedContent === contentSignature();
        const descendants = selected.find('*');
        // Give direct layout children a chance before deeply nested text consumes the budget.
        const targets = [selected, ...descendants.filter(c => c.parent() === selected), ...descendants.filter(c => c.parent() !== selected)];
        // Explicit IDs let the AI patch CSS without rebuilding components or losing their styles.
        // ID coverage is independent of the optional 40-component computed-style budget.
        for (const component of targets) component.addAttributes({ id: component.getId() });
        const styleContext = [];
        let styleContextLength = 2;
        for (const component of targets.slice(0, 40)) {
            const id = component.getId();
            const element = component.getEl();
            const computed = element?.ownerDocument.defaultView.getComputedStyle(element);
            const context = { selector: `#${id}`, componentType: component.get('type'),
                parentSelector: component.parent() ? `#${component.parent().getId()}` : null,
                properties: Object.fromEntries(
                ['display', 'width', 'max-width', 'min-width', 'grid-template-columns', 'flex-direction', 'flex-wrap',
                    'flex-basis', 'flex-grow', 'flex-shrink', 'gap', 'row-gap', 'column-gap', 'justify-content', 'align-items',
                    'margin-left', 'margin-right', 'margin-top', 'margin-bottom', 'padding-left', 'padding-right',
                    'box-sizing', 'object-fit', 'aspect-ratio',
                    'font-size', 'font-weight', 'font-style', 'font-family', 'line-height', 'color',
                    'background-color', 'text-shadow', '-webkit-text-stroke-width', '-webkit-text-stroke-color', 'paint-order']
                    .map(name => [name, computed?.getPropertyValue(name) || ''])
                    .filter(([, value]) => value && value.length <= 300)) };
            const length = JSON.stringify(context).length + (styleContext.length ? 1 : 0);
            // Computed styles are optional reference data; never overflow the API's 16K budget.
            if (styleContextLength + length > 16000) break;
            styleContext.push(context);
            styleContextLength += length;
        }
        const originalProject = JSON.stringify(editor.getProjectData());
        // Adding target IDs is preparation by the plugin, not an unsaved user edit.
        if (cleanBeforePreparingIds) recordedContent = contentSignature();
        const requestKey = activeKey;
        const html = selected.toHTML();
        const css = editor.getCss({ component: selected });
        if (html.length > 64000) return status('選取區塊的內容太長，請選取較小的區塊後重試。');
        if (css.length > 32000) return status('選取區塊的樣式太多，請選取較小的區塊後重試。');
        const beforeVersionId = checkpoint('修改前：' + prompt.slice(0, 50));
        pending = new AbortController();
        const recent = conversation.filter(m => ['user', 'assistant'].includes(m.role))
            .slice(-8).map(({ role, content }) => ({ role, content }));
        while (JSON.stringify(recent).length > 8000) recent.shift();
        const history = JSON.stringify(recent);
        conversation.push({ role: 'user', content: prompt });
        conversation = conversation.slice(-200);
        persist(); render();
        find('[data-prompt]').value = '';
        setBusy(true); status(expanded ? '已定位包含此元件的排列區塊，AI 產生中…' : 'AI 產生中，可能需要數分鐘…');
        let nextSettings = null;
        try {
            const result = await adapter.ai.generate({
                scope, prompt, html, selectedId,
                browserLocale: navigator.languages?.[0] || navigator.language || '',
                conversationLocale: replyLocale,
                availableSettings: Object.keys(settingCommands).filter(kind => editor.Commands.get(settingCommands[kind])),
                preserveContent, autoDetect, styleContext: JSON.stringify(styleContext),
                css,
                history
            }, { signal: pending.signal });
            // A host adapter may finish after cancellation even if it ignores AbortSignal.
            if (pending.signal.aborted) throw new DOMException('Aborted', 'AbortError');
            if (destroyed) return;
            if (requestKey !== getKey() || originalProject !== JSON.stringify(editor.getProjectData()) ||
                (scope === 'selected' && !selected.parent()))
                throw new Error('等待期間畫布已修改或切換，未套用 AI 回覆，請重新送出。');
            if (autoDetect) {
                if (typeof result.preserveContent !== 'boolean') throw new Error('未取得自動判斷結果，未修改畫布。');
                preserveContent = result.preserveContent;
            }
            const replaceHtml = !preserveContent || result.htmlChanged === true;
            const safeHtml = replaceHtml ? validateHtml(result.html) : null;
            let classChanges = null;
            if (replaceHtml) {
                const parse = markup => new DOMParser().parseFromString(markup, 'text/html').body;
                const before = parse(html), after = parse(safeHtml);
                const images = root => [...root.querySelectorAll('img,source')]
                    .map(el => JSON.stringify([el.tagName, el.getAttribute('src') || '', el.getAttribute('srcset') || '']));
                const remaining = images(after);
                for (const image of images(before)) {
                    const index = remaining.indexOf(image);
                    if (index < 0) throw new Error('修改回覆遺失原有圖片，已保留原區塊。');
                    remaining.splice(index, 1);
                }
                const signature = node => node.nodeType === 3 ? node.nodeValue : {
                    tag: node.nodeName,
                    attrs: [...(node.attributes || [])].filter(a => a.name !== 'class')
                        .map(a => [a.name, a.value]).sort((a, b) => a[0].localeCompare(b[0])),
                    children: [...node.childNodes].filter(n => n.nodeType !== 3 || n.nodeValue.trim()).map(signature)
                };
                if (JSON.stringify(signature(before)) === JSON.stringify(signature(after))) {
                    const models = new Map([selected, ...selected.find('*')].map(c => [c.getId(), c]));
                    const originals = [...before.querySelectorAll('*')];
                    classChanges = [];
                    [...after.querySelectorAll('*')].forEach((el, index) => {
                        if (el.className === originals[index].className) return;
                        const component = models.get(el.id);
                        if (!component) throw new Error('無法定位要調整的排列元件，已保留原區塊。');
                        classChanges.push({ component, classes: [...el.classList] });
                    });
                }
            }

            if (preserveContent && replaceHtml) {
                const parse = markup => new DOMParser().parseFromString(markup, 'text/html').body.textContent;
                if (parse(html) !== parse(safeHtml)) throw new Error('局部樣式修改不可改寫原文，未套用回覆。');
            }
            let rules = validateStyles(result.styles, replaceHtml ? safeHtml : html);
            const beforeApply = JSON.parse(originalProject);
            const beforeContent = contentSignature();
            const originalImages = [...new DOMParser().parseFromString(editor.getHtml(), 'text/html').querySelectorAll('img')]
                .map(el => el.getAttribute('src') || '');
            restoring = true;
            let appliedTarget = selected;
            try {
                if (classChanges) {
                    // Layout edits keep the existing GrapesJS tree, custom types and children.
                    for (const change of classChanges) change.component.setClass(change.classes);
                } else if (replaceHtml) {
                    if (scope === 'selected') {
                        const parent = selected.parent();
                        const at = parent.components().indexOf(selected);
                        const replacement = new DOMParser().parseFromString(safeHtml, 'text/html');
                        const replacementIds = new Set([...replacement.body.querySelectorAll('[id]')].map(el => el.id));
                        const keepIds = [selected, ...selected.find('*')]
                            .map(component => component.getId()).filter(id => replacementIds.has(id));
                        // replaceWith removes ID rules before adding the replacement.
                        // Preserve rules for surviving elements, including nested text spans.
                        selected.remove({ keepIds });
                        if (selected.parent()) throw new Error('無法替換選取區塊，已保留畫布。');
                        const inserted = parent.append(safeHtml, { at });
                        if (inserted.length) { appliedTarget = inserted[0]; editor.select(inserted[0]); }
                    }
                    else editor.setComponents(safeHtml);
                }
                for (const rule of rules) {
                    const count = rule.properties['--coker-swiper-columns'];
                    if (count) {
                        const component = [appliedTarget, ...appliedTarget.find('*')].find(c => `#${c.getId()}` === rule.selector);
                        const layouts = ['one_swiper', 'two_swiper', 'three_swiper', 'four_swiper', 'five_swiper', 'six_swiper'];
                        const root = component?.getEl();
                        const frameWindow = root?.ownerDocument.defaultView;
                        if (!component || !layouts.some(name => component.getClasses().includes(name)) ||
                            !frameWindow?.jQuery || typeof frameWindow.SwiperInit !== 'function')
                            throw new Error('此輪播目前無法調整欄數，已保留原設定。');
                        for (const element of root.querySelectorAll('.swiper')) element.swiper?.destroy?.(true, true);
                        component.setClass([...component.getClasses().filter(name => !layouts.includes(name)), layouts[Number(count) - 1]]);
                        frameWindow.jQuery(component.getEl()).data('isInit', false);
                        frameWindow.SwiperInit({ autoplay: false });
                        if (!component.getEl().querySelector('.swiper')?.swiper)
                            throw new Error('輪播重新初始化未完成，已還原修改前內容。');
                    }
                    editor.Css.setRule(rule.selector, rule.properties, { addStyles: true });
                }
                const appliedModels = new Map([appliedTarget, ...appliedTarget.find('*')]
                    .map(component => [component.getId(), component]));
                for (const rule of rules) {
                    const tracks = rule.properties['grid-template-columns'];
                    if (!tracks || ['none', 'initial', 'unset', 'inherit', 'revert'].includes(tracks)) continue;
                    const element = appliedModels.get(rule.selector.slice(1))?.getEl();
                    const display = element && element.ownerDocument.defaultView.getComputedStyle(element).display;
                    if (!['grid', 'inline-grid'].includes(display))
                        throw new Error('排列樣式未在畫布生效，已還原修改前內容。');
                }
                // Validate the actual runtime directory cards, not just the hidden model template.
                const directory = appliedTarget.getEl();
                for (const list of directory ? [directory, ...directory.querySelectorAll('.catalog')] : []) {
                    if (!list.matches('.catalog')) continue;
                    const computed = list.ownerDocument.defaultView.getComputedStyle(list);
                    if (!['grid', 'inline-grid'].includes(computed.display)) continue;
                    const tracks = computed.gridTemplateColumns.split(' ').map(Number.parseFloat).filter(Number.isFinite);
                    const cards = [...list.children].filter(el => el.matches('.template') && el.getBoundingClientRect().width > 0);
                    if (tracks.length && cards.some(el => el.getBoundingClientRect().width < Math.min(...tracks) / 2))
                        throw new Error('目錄卡片寬度未正確套用，已還原修改前內容。');
                }
                const appliedImages = [...new DOMParser().parseFromString(editor.getHtml(), 'text/html').querySelectorAll('img')]
                    .map(el => el.getAttribute('src') || '');
                for (const src of originalImages) {
                    const index = appliedImages.indexOf(src);
                    if (index < 0) throw new Error('套用後圖片不完整，已還原修改前內容。');
                    appliedImages.splice(index, 1);
                }
                if (contentSignature() === beforeContent) throw new Error('回覆未產生任何 HTML 或 CSS 差異，畫布保持不變。');
            } catch (error) {
                editor.loadProjectData(beforeApply);
                throw error;
            } finally { restoring = false; }
            const versionId = checkpoint('AI：' + prompt.slice(0, 50));
            undoVersionId = beforeVersionId || '';
            if (typeof result.replyLocale === 'string') replyLocale = result.replyLocale;
            conversation.push({ role: 'assistant', content: result.message || '已完成調整。', versionId, beforeVersionId, settingsActions: result.settingsActions });
            nextSettings = result.settingsActions?.[0];
            conversation = conversation.slice(-200);
            persist(); render();
            find('[data-prompt]').value = '';
            status('已套用，請檢查桌面／手機預覽並使用原有儲存功能。');
        } catch (error) {
            if (!destroyed && requestKey === getKey()) {
                conversation.push({ role: error.needsClarification === true ? 'assistant' : 'system',
                    content: error.name === 'AbortError' ? '已取消，未套用 AI 回覆。' : error.message,
                    settingsActions: error.needsClarification === true ? error.settingsActions : [] });
                if (error.needsClarification === true) nextSettings = error.settingsActions?.[0];
                if (error.needsClarification && typeof error.replyLocale === 'string') replyLocale = error.replyLocale;
                conversation = conversation.slice(-200);
                persist();
                render();
                find('[data-prompt]').value = '';
            }
            if (!destroyed && (error.serviceUnavailable === true || error instanceof TypeError || error instanceof SyntaxError)) {
                serviceUnavailable = true;
                editor.Panels.removeButton('options', 'coker-ai-assistant');
            }
            if (!destroyed) status(error.name === 'AbortError' ? '已取消，未套用 AI 回覆。' : error.message);
        } finally {
            pending = null;
            if (!destroyed) setBusy(false);
            if (!destroyed && requestKey === getKey() && nextSettings) openSettings(nextSettings);
        }
    };
    // The browser parser is the final authority before GrapesJS parses untrusted markup.
    function validateHtml(html) {
        if (typeof html !== 'string' || !html.trim()) throw new Error('AI 未回傳有效 HTML。');
        const doc = new DOMParser().parseFromString(html, 'text/html');
        const tags = new Set('SECTION ARTICLE HEADER FOOTER NAV DIV SPAN P H1 H2 H3 H4 H5 H6 UL OL LI A IMG PICTURE SOURCE STRONG EM B I U SMALL BR HR BLOCKQUOTE FIGURE FIGCAPTION TABLE THEAD TBODY TFOOT TR TH TD DL DT DD BUTTON'.split(' '));
        const attrs = new Set('id class title role alt href src sizes width height loading decoding target rel type colspan rowspan scope data-text'.split(' '));
        for (const el of doc.body.querySelectorAll('*')) {
            if (!tags.has(el.tagName)) throw new Error('AI HTML 包含不支援的元件。');
            for (const a of el.attributes) {
                if (el.tagName === 'A' && a.name === 'data-link-type' && a.value === 'link') continue;
                if (el.tagName === 'A' && a.name === 'data-link-value' && (a.value === '' ||
                    (!/[\x00-\x20\\]/.test(a.value) && !a.value.startsWith('//') && /^(?:\/(?!\/)|#|https:\/\/)/i.test(a.value)))) continue;
                if (!attrs.has(a.name) && !a.name.startsWith('aria-')) throw new Error('AI HTML 包含不支援的屬性。');
                if (['href', 'src'].includes(a.name)) {
                    const value = a.value.trim();
                    if (el.tagName === 'A' && a.name === 'href' && value === '') { el.removeAttribute('href'); continue; }
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
        const allowed = new Set('color background-color font-family font-size font-weight font-style line-height letter-spacing text-align text-decoration text-shadow -webkit-text-stroke -webkit-text-stroke-width -webkit-text-stroke-color paint-order margin margin-top margin-bottom margin-left margin-right padding padding-top padding-bottom padding-left padding-right border border-radius width max-width min-width height max-height display gap row-gap column-gap grid-template-columns grid-template-rows grid-auto-flow grid-column grid-row flex-direction flex-wrap flex-basis flex-grow flex-shrink justify-content align-items align-self justify-self order box-sizing object-fit object-position aspect-ratio --coker-swiper-columns'.split(' '));
        for (const rule of rules) {
            if (rule?.properties?.['--coker-swiper-columns']) {
                const root = doc.getElementById(rule.selector?.slice(1));
                if (!/^[1-6]$/.test(rule.properties['--coker-swiper-columns']) ||
                    !root?.matches('.one_swiper,.two_swiper,.three_swiper,.four_swiper,.five_swiper,.six_swiper'))
                    throw new Error('輪播欄數設定與目標元件不相容。');
            }
            const directoryMatch = rule && /^#([a-zA-Z_][a-zA-Z0-9_-]*) > \.template$/.exec(rule.selector);
            const directoryList = directoryMatch && doc.getElementById(directoryMatch[1]);
            const directoryItems = directoryList?.matches('.catalog') && directoryList.closest('.catalog_frame');
            if (!rule || (!directoryItems && (!/^#[a-zA-Z_][a-zA-Z0-9_-]*$/.test(rule.selector) || !ids.has(rule.selector.slice(1)))) || !rule.properties ||
                Object.entries(rule.properties).some(([name, value]) => !allowed.has(name) || typeof value !== 'string' || value.length > 300 ||
                    !/^[\p{L}0-9\s#%.,()'"/*+-]+$/u.test(value) || /url|expression|javascript|import/i.test(value)))
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
    editor.on('component:selected component:deselected', updateSelection);
    editor.on('destroy', () => {
        editor.off('component:selected component:deselected', updateSelection);
        destroyed = true; pending?.abort(); panel.remove();
    });
}
