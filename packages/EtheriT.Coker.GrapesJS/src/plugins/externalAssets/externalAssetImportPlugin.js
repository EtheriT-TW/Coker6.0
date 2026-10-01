const preferenceModes = new Set(['ask', 'small-only', 'all', 'skip']);
const defaultSmallImageMaxBytes = 300 * 1024;
const styleId = 'coker-external-asset-import-style';

export function externalAssetImportPlugin(editor, options = {}) {
    const hostWindow = typeof window === 'undefined' ? null : window;
    const hostDocument = hostWindow?.document;
    const canImportExternalImages = hostDocument
        ?.querySelector('meta[name="coker-can-import-external-canvas-images"]')
        ?.getAttribute('content') === 'true';
    if (!canImportExternalImages) return;
    const smallImageMaxBytes = Number(options.smallImageMaxBytes) || defaultSmallImageMaxBytes;
    const apiRoot = options.apiRoot || '/api/FileUpload';
    let isReady = false;
    let processing = Promise.resolve();
    let toolbarButton = null;
    let refreshTimer = null;
    const unavailablePaths = new Set();

    ensureStyles(hostDocument);

    const getOrgName = () => String(
        hostDocument?.querySelector('meta[name="coker-org-name"]')?.getAttribute('content')
        || editor?.Canvas?.getWindow?.()?.OrgName
        || hostWindow?.OrgName
        || ''
    ).replace(/^\/+|\/+$/g, '');

    const getPreferenceKey = () => {
        const userId = hostDocument
            ?.querySelector('meta[name="coker-user-id"]')
            ?.getAttribute('content') || 'anonymous';
        return `coker:grapes:external-assets:v1:${userId}`;
    };

    const loadPreference = () => {
        try {
            const value = JSON.parse(hostWindow.localStorage.getItem(getPreferenceKey()));
            return preferenceModes.has(value?.mode) ? value.mode : 'ask';
        } catch {
            return 'ask';
        }
    };

    const savePreference = mode => {
        if (!preferenceModes.has(mode)) return;
        try {
            hostWindow.localStorage.setItem(getPreferenceKey(), JSON.stringify({ mode }));
        } catch {
            // Browser storage may be disabled; the current operation can still continue.
        }
    };

    const request = async (action, paths) => {
        const websiteId = hostDocument
            ?.querySelector('meta[name="coker-website-id"]')
            ?.getAttribute('content');
        const response = await fetch(`${apiRoot}/${action}`, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json',
                ...(websiteId ? { 'X-Coker-Website-Id': websiteId } : {})
            },
            body: JSON.stringify({ paths })
        });
        if (!response.ok) throw new Error(`圖片匯入服務回傳 ${response.status}`);
        return response.json();
    };

    const collectForeignPaths = () => collectCanvasImagePaths(
        editor.getHtml?.() || '',
        editor.getCss?.() || '',
        getOrgName(),
        hostWindow?.location
    );

    const refreshPendingCount = () => {
        const count = collectForeignPaths().filter(path => !unavailablePaths.has(path)).length;
        if (!toolbarButton) return count;
        toolbarButton.set('label', count > 0
            ? `<i class="fa fa-image" aria-hidden="true"></i><span class="coker-asset-toolbar-count">${count}</span>`
            : '<i class="fa fa-image" aria-hidden="true"></i>');
        toolbarButton.set('attributes', {
            title: count > 0
                ? `外站圖片匯入設定（尚有 ${count} 張）`
                : '外站圖片匯入設定'
        });
        return count;
    };

    const schedulePendingRefresh = () => {
        if (refreshTimer) clearTimeout(refreshTimer);
        refreshTimer = setTimeout(refreshPendingCount, 250);
    };

    const importItems = async items => {
        if (!items.length) return { imported: 0, failedItems: [] };
        const loading = showLoading(hostDocument, items.length);
        const replacements = new Map();
        const importedAssets = [];
        const failedItems = [];
        try {
            for (let index = 0; index < items.length; index++) {
                const item = items[index];
                loading.update(
                    item.isInternal ? '正在複製圖片' : '正在下載圖片',
                    item.name || item.path,
                    index + 1
                );
                try {
                    const result = await request('ImportCanvasImages', [item.path]);
                    const imported = (result.files || result.Files || [])[0];
                    const newPath = imported?.path || imported?.Path;
                    if (newPath) {
                        replacements.set(item.path, newPath);
                        importedAssets.push({
                            src: newPath,
                            fullSrc: newPath,
                            thumbnailSrc: newPath,
                            name: imported?.name || imported?.Name || item.name || '',
                            guid: imported?.guid || imported?.Guid || '',
                            type: 'image'
                        });
                    }
                    else failedItems.push(item);
                } catch {
                    failedItems.push(item);
                }
            }
            loading.update('正在套用圖片路徑', '', items.length);
            applyReplacements(editor, replacements);
            addImportedAssets(editor, importedAssets);
        } finally {
            loading.close();
        }
        refreshPendingCount();
        return { imported: replacements.size, failedItems };
    };

    const scanAndHandle = async ({ forceAsk = false } = {}) => {
        const paths = collectForeignPaths();
        refreshPendingCount();
        if (!paths.length) {
            if (forceAsk) options.adapter?.ui?.success?.('目前畫布沒有需要匯入的外站圖片。');
            return;
        }

        let mode = forceAsk ? 'ask' : loadPreference();
        if (mode === 'skip') return;

        let items;
        const inspecting = showLoading(hostDocument, paths.length);
        inspecting.update('正在分析元件圖片', '', 0);
        try {
            const inspected = await request('InspectCanvasImages', paths);
            items = (inspected.items || inspected.Items || []).map(normalizeInspectItem);
            items.forEach(item => {
                if (item.canImport) unavailablePaths.delete(item.path);
                else unavailablePaths.add(item.path);
            });
        } finally {
            inspecting.close();
        }
        refreshPendingCount();
        const importable = items.filter(item => item.canImport);

        if (mode === 'ask') {
            const choice = await showImportPrompt(hostDocument, items, smallImageMaxBytes);
            if (!choice) return;
            mode = choice.mode;
            if (choice.remember) savePreference(mode);
            applyUnavailableActions(editor, choice.unavailableActions, hostDocument);
        }

        const selected = mode === 'all'
            ? importable
            : mode === 'small-only'
                ? importable.filter(item => item.size != null && item.size <= smallImageMaxBytes)
                : [];
        if (!selected.length) {
            refreshPendingCount();
            return;
        }

        const result = await importItems(selected);
        if (result.failedItems.length > 0) {
            const failedPaths = result.failedItems.map(item => item.path).join('\n');
            options.adapter?.ui?.alert?.(
                `已匯入 ${result.imported} 張圖片，${result.failedItems.length} 張匯入失敗並保留原始路徑：\n${failedPaths}`
            );
        } else {
            options.adapter?.ui?.success?.(`已匯入 ${result.imported} 張圖片。`);
        }
    };

    const openSettings = async () => {
        const pendingPaths = collectForeignPaths().filter(path => !unavailablePaths.has(path));
        const selected = await showSettings(
            hostDocument,
            loadPreference(),
            smallImageMaxBytes,
            pendingPaths
        );
        if (!selected) return;
        savePreference(selected.mode);
        if (selected.scan) {
            processing = processing
                .then(() => scanAndHandle())
                .catch(error => options.adapter?.ui?.error?.(error.message));
        }
    };

    editor.Commands.add('coker:external-assets-settings', { run: openSettings });
    const panel = editor.Panels?.getPanel?.('options');
    if (panel && !editor.Panels.getButton('options', 'cokerExternalAssets')) {
        const codeButton = editor.Panels.getButton('options', 'export-template');
        const codeButtonIndex = codeButton && panel.buttons
            ? panel.buttons.indexOf(codeButton)
            : -1;
        if (codeButton) {
            editor.Panels.removeButton('options', 'export-template');
        }
        const button = {
            id: 'cokerExternalAssets',
            label: '<i class="fa fa-image" aria-hidden="true"></i>',
            command: 'coker:external-assets-settings',
            attributes: { title: '外站圖片匯入設定' },
            active: false
        };
        if (codeButtonIndex >= 0 && panel.buttons) {
            toolbarButton = panel.buttons.add(button, { at: codeButtonIndex });
        } else {
            toolbarButton = editor.Panels.addButton('options', button);
        }
    }

    editor.on('load', () => {
        setTimeout(() => {
            isReady = true;
            refreshPendingCount();
        }, 0);
    });

    editor.on('component:add', component => {
        if (!isReady) return;
        const attributes = component?.getAttributes?.() || {};
        if (!attributes['data-block-name']) return;
        processing = processing
            .then(() => scanAndHandle())
            .catch(error => options.adapter?.ui?.error?.(error.message));
    });

    editor.on('component:remove', () => {
        if (isReady) schedulePendingRefresh();
    });
    editor.on('component:update:attributes', () => {
        if (isReady) schedulePendingRefresh();
    });
}

function addImportedAssets(editor, assets) {
    const assetManager = editor?.AssetManager;
    if (!assetManager || !assets.length) return;
    const existing = new Set(
        assetManager.getAll().map(asset => asset.get('src'))
    );
    const additions = assets.filter(asset => {
        if (!asset.src || existing.has(asset.src)) return false;
        existing.add(asset.src);
        return true;
    });
    if (additions.length) assetManager.add(additions);
}

function normalizeInspectItem(item) {
    return {
        path: item.path ?? item.Path ?? '',
        name: item.name ?? item.Name ?? '',
        size: item.size ?? item.Size ?? null,
        isInternal: item.isInternal ?? item.IsInternal ?? false,
        canImport: item.canImport ?? item.CanImport ?? false,
        error: item.error ?? item.Error ?? ''
    };
}

function collectCanvasImagePaths(html, css, orgName, location) {
    const paths = new Set();
    const document = new DOMParser().parseFromString(html, 'text/html');
    const add = value => {
        let path = String(value || '').trim();
        if (path.startsWith('//')) path = `${location?.protocol || 'https:'}${path}`;
        if (isForeignImagePath(path, orgName, location)) paths.add(path);
    };
    document.querySelectorAll('img').forEach(node => {
        add(node.getAttribute('src'));
        add(node.getAttribute('data-src'));
        parseSrcset(node.getAttribute('srcset')).forEach(add);
    });
    document.querySelectorAll('source[srcset]').forEach(node => {
        parseSrcset(node.getAttribute('srcset')).forEach(add);
    });
    document.querySelectorAll('video[poster]').forEach(node => add(node.getAttribute('poster')));
    document.querySelectorAll('[data-full-src],[data-medium-src],[data-coker-poster]').forEach(node => {
        add(node.getAttribute('data-full-src'));
        add(node.getAttribute('data-medium-src'));
        add(node.getAttribute('data-coker-poster'));
    });
    document.querySelectorAll('[style]').forEach(node => {
        parseCssUrls(node.getAttribute('style')).forEach(add);
    });
    parseCssUrls(css).forEach(add);
    return [...paths];
}

function isForeignImagePath(path, orgName, location) {
    if (!path || /^(data:|blob:|#|javascript:)/i.test(path)) return false;
    if (/\.(woff2?|ttf|otf|eot)(?:[?#]|$)/i.test(path)) return false;
    const ownPrefix = orgName ? `/upload/${orgName}/`.toLowerCase() : '';
    if (path.startsWith('/')) {
        return path.toLowerCase().startsWith('/upload/')
            && (!ownPrefix || !path.toLowerCase().startsWith(ownPrefix));
    }
    try {
        const url = new URL(path, location?.href || 'http://localhost');
        if (!/^https?:$/.test(url.protocol)) return false;
        return !ownPrefix || !url.pathname.toLowerCase().startsWith(ownPrefix);
    } catch {
        return false;
    }
}

function parseCssUrls(value) {
    const result = [];
    const pattern = /url\(\s*(['"]?)(.*?)\1\s*\)/gi;
    let match;
    while ((match = pattern.exec(String(value || ''))) !== null) result.push(match[2]);
    return result;
}

function parseSrcset(value) {
    return String(value || '').split(',').map(item => item.trim().split(/\s+/)[0]).filter(Boolean);
}

function replaceText(value, replacements) {
    let result = String(value ?? '');
    [...replacements.entries()]
        .sort((left, right) => right[0].length - left[0].length)
        .forEach(([source, target]) => { result = result.split(source).join(target); });
    return result;
}

function applyReplacements(editor, replacements) {
    if (!replacements.size) return;
    editor.getWrapper?.()?.onAll?.(component => {
        const attributes = component.getAttributes?.() || {};
        const updated = {};
        let changed = false;
        Object.entries(attributes).forEach(([name, value]) => {
            if (typeof value !== 'string') return;
            const next = replaceText(value, replacements);
            if (next !== value) {
                updated[name] = next;
                changed = true;
            }
        });
        if (changed) component.addAttributes(updated);
        ['src', 'poster'].forEach(property => {
            const value = component.get?.(property);
            if (typeof value !== 'string') return;
            const next = replaceText(value, replacements);
            if (next !== value) component.set(property, next);
        });
        const content = component.get?.('content');
        if (typeof content === 'string') {
            const next = replaceText(content, replacements);
            if (next !== content) component.set('content', next);
        }
    });
    editor.CssComposer?.getAll?.().forEach(rule => {
        const style = rule.getStyle?.() || {};
        let changed = false;
        const next = { ...style };
        Object.entries(style).forEach(([name, value]) => {
            if (typeof value !== 'string') return;
            const replaced = replaceText(value, replacements);
            if (replaced !== value) {
                next[name] = replaced;
                changed = true;
            }
        });
        if (changed) rule.setStyle?.(next);
    });
}

function applyUnavailableActions(editor, actions, document) {
    const selected = new Map(
        (actions || [])
            .filter(action => action?.path && action.mode !== 'skip')
            .map(action => [action.path, action.mode])
    );
    if (!selected.size) return;

    const removedComponents = new Set();
    editor.getWrapper?.()?.onAll?.(component => {
        if (removedComponents.has(component)) return;
        const matchedEntries = [...selected.entries()].filter(([path]) => componentReferencesPath(component, path));
        if (!matchedEntries.length) return;

        if (matchedEntries.some(([path, mode]) => mode === 'delete' && isImageComponentReference(component, path))) {
            removedComponents.add(component);
            component.remove?.();
            return;
        }

        removeComponentAssetSettings(component, matchedEntries.map(([path]) => path), document);
    });

    editor.CssComposer?.getAll?.().forEach(rule => {
        const style = rule.getStyle?.() || {};
        const next = removeMatchingStyleProperties(style, selected.keys());
        if (next.changed) rule.setStyle?.(next.style);
    });
}

function componentReferencesPath(component, path) {
    const attributes = component.getAttributes?.() || {};
    if (Object.values(attributes).some(value => typeof value === 'string' && value.includes(path))) return true;
    if (['src', 'poster'].some(property => String(component.get?.(property) || '').includes(path))) return true;
    if (Object.values(component.getStyle?.() || {}).some(value => cssValueReferencesPath(value, path))) return true;
    return String(component.get?.('content') || '').includes(path);
}

function isImageComponentReference(component, path) {
    const tagName = String(component.get?.('tagName') || '').toLowerCase();
    const type = String(component.get?.('type') || '').toLowerCase();
    if (tagName !== 'img' && tagName !== 'source' && type !== 'image') return false;
    const attributes = component.getAttributes?.() || {};
    return ['src', 'data-src', 'srcset', 'data-full-src', 'data-medium-src']
        .some(name => String(attributes[name] || component.get?.(name) || '').includes(path));
}

function removeComponentAssetSettings(component, paths, document) {
    const pathSet = new Set(paths);
    const attributes = component.getAttributes?.() || {};
    const updated = {};
    const removals = [];

    Object.entries(attributes).forEach(([name, value]) => {
        if (typeof value !== 'string') return;
        if (name === 'style') {
            const next = removeMatchingCssDeclarations(value, pathSet, document);
            if (next !== value) {
                if (next) updated[name] = next;
                else removals.push(name);
            }
            return;
        }
        if (!paths.some(path => value.includes(path))) return;
        if (name === 'srcset') {
            const next = removeSrcsetPaths(value, pathSet);
            if (next) updated[name] = next;
            else removals.push(name);
        } else {
            removals.push(name);
        }
    });

    if (Object.keys(updated).length) component.addAttributes?.(updated);
    if (removals.length) component.removeAttributes?.([...new Set(removals)]);

    ['src', 'poster'].forEach(property => {
        const value = component.get?.(property);
        if (typeof value === 'string' && paths.some(path => value.includes(path))) component.set?.(property, '');
    });

    const componentStyle = removeMatchingStyleProperties(component.getStyle?.() || {}, pathSet);
    if (componentStyle.changed) component.setStyle?.(componentStyle.style);
}

function removeMatchingStyleProperties(style, paths) {
    const pathList = [...paths];
    const next = { ...style };
    let changed = false;
    Object.entries(style).forEach(([name, value]) => {
        if (!pathList.some(path => cssValueReferencesPath(value, path))) return;
        delete next[name];
        changed = true;
    });
    return { style: next, changed };
}

function removeMatchingCssDeclarations(value, paths, document) {
    if (!document?.createElement) return value;
    const element = document.createElement('span');
    element.style.cssText = value;
    Array.from(element.style).forEach(name => {
        const propertyValue = element.style.getPropertyValue(name);
        if ([...paths].some(path => cssValueReferencesPath(propertyValue, path))) {
            element.style.removeProperty(name);
        }
    });
    return element.style.cssText;
}

function cssValueReferencesPath(value, path) {
    return parseCssUrls(value).some(url => url === path);
}

function removeSrcsetPaths(value, paths) {
    return String(value || '')
        .split(',')
        .map(item => item.trim())
        .filter(item => item && !paths.has(item.split(/\s+/)[0]))
        .join(', ');
}

function ensureStyles(document) {
    if (!document || document.getElementById(styleId)) return;
    const style = document.createElement('style');
    style.id = styleId;
    style.textContent = `
        .coker-asset-modal{position:fixed;inset:0;z-index:2147483000;background:rgba(15,23,42,.55);display:flex;align-items:center;justify-content:center;padding:20px}
        .coker-asset-dialog{width:min(560px,100%);max-height:calc(100vh - 40px);overflow:auto;background:#fff;color:#1f2937;border-radius:12px;box-shadow:0 24px 70px rgba(0,0,0,.3);padding:24px}
        .coker-asset-dialog h3{font-size:20px;margin:0 0 12px}.coker-asset-dialog p{line-height:1.6;margin:8px 0}
        .coker-asset-summary{background:#f8fafc;border-radius:8px;padding:12px;margin:14px 0}.coker-asset-actions{display:flex;gap:8px;justify-content:flex-end;flex-wrap:wrap;margin-top:18px}
        .coker-asset-actions button{border:1px solid #cbd5e1;border-radius:7px;background:#fff;padding:8px 14px;cursor:pointer}.coker-asset-actions button.primary{background:#2563eb;color:#fff;border-color:#2563eb}
        .coker-asset-choice{display:block;padding:8px 0}.coker-asset-loading{text-align:center}.coker-asset-spinner{width:36px;height:36px;border:4px solid #dbeafe;border-top-color:#2563eb;border-radius:50%;animation:coker-asset-spin .8s linear infinite;margin:0 auto 14px}
        .coker-asset-pending{margin-top:14px}.coker-asset-pending ul{max-height:150px;overflow:auto;margin:8px 0 0;padding-left:22px}.coker-asset-pending li{overflow-wrap:anywhere;margin:5px 0;font-size:12px}
        .coker-asset-problems{margin-top:14px}.coker-asset-problem{border:1px solid #fecaca;background:#fff7f7;border-radius:8px;padding:10px;margin-top:8px}.coker-asset-problem-path{font-size:12px;overflow-wrap:anywhere}.coker-asset-problem-error{font-size:12px;color:#b91c1c;margin-top:4px}.coker-asset-problem-actions{display:flex;gap:12px;flex-wrap:wrap;margin-top:8px;font-size:13px}
        .coker-asset-toolbar-count{display:inline-block;margin-left:2px;color:#fbbf24;font-size:9px;line-height:1;vertical-align:top}
        @keyframes coker-asset-spin{to{transform:rotate(360deg)}}
    `;
    document.head.append(style);
}

function createModal(document, content) {
    const overlay = document.createElement('div');
    overlay.className = 'coker-asset-modal';
    const dialog = document.createElement('div');
    dialog.className = 'coker-asset-dialog';
    dialog.innerHTML = content;
    overlay.append(dialog);
    document.body.append(overlay);
    return { overlay, dialog, close: () => overlay.remove() };
}

function showImportPrompt(document, items, smallMaxBytes) {
    const available = items.filter(item => item.canImport);
    const unavailableItems = items.filter(item => !item.canImport);
    const small = available.filter(item => item.size != null && item.size <= smallMaxBytes).length;
    const large = available.filter(item => item.size != null && item.size > smallMaxBytes).length;
    const unknown = available.filter(item => item.size == null).length;
    const unavailable = items.length - available.length;
    return new Promise(resolve => {
        const modal = createModal(document, `
            <h3>發現非本站圖片</h3>
            <p>此元件包含非本站上傳圖片。若不匯入網站空間，前台可能因來源圖片被刪除或路徑不同而出現掉圖。</p>
            <div class="coker-asset-summary">小型圖片（${formatBytes(smallMaxBytes)} 以下）：${small} 張<br>大型圖片：${large} 張<br>大小未知：${unknown} 張${unavailable ? `<br>無法匯入或來源不存在：${unavailable} 張` : ''}</div>
            ${unavailableItems.length ? `
                <div class="coker-asset-problems">
                    <strong>有問題的圖片來源</strong>
                    <p>請選擇如何處理。若來源位於 CSS，選擇移除或刪除時會移除完整 CSS 設定。</p>
                    ${unavailableItems.map((item, index) => `
                        <div class="coker-asset-problem" data-problem-index="${index}">
                            <div class="coker-asset-problem-path">${escapeHtml(item.path)}</div>
                            ${item.error ? `<div class="coker-asset-problem-error">${escapeHtml(item.error)}</div>` : ''}
                            <div class="coker-asset-problem-actions">
                                <label><input type="radio" name="coker-problem-${index}" value="remove"> 移除圖片設定</label>
                                <label><input type="radio" name="coker-problem-${index}" value="delete"> 刪除圖片元件</label>
                                <label><input type="radio" name="coker-problem-${index}" value="skip" checked> 暫時略過</label>
                            </div>
                        </div>
                    `).join('')}
                </div>
            ` : ''}
            ${available.length ? '<label><input type="checkbox" data-remember> 記住我的匯入選擇</label>' : ''}
            <div class="coker-asset-actions">
                ${available.length
                    ? '<button data-mode="skip">暫時略過可匯入圖片</button><button data-mode="all">全部匯入</button><button class="primary" data-mode="small-only">僅匯入小型圖片</button>'
                    : '<button data-cancel>取消</button><button class="primary" data-mode="skip">套用問題圖片處理</button>'}
            </div>
        `);
        modal.dialog.querySelectorAll('[data-mode]').forEach(button => {
            button.addEventListener('click', () => {
                const result = {
                    mode: button.dataset.mode,
                    remember: modal.dialog.querySelector('[data-remember]')?.checked || false,
                    unavailableActions: unavailableItems.map((item, index) => ({
                        path: item.path,
                        mode: modal.dialog.querySelector(`input[name="coker-problem-${index}"]:checked`)?.value || 'skip'
                    }))
                };
                modal.close();
                resolve(result);
            });
        });
        modal.dialog.querySelector('[data-cancel]')?.addEventListener('click', () => {
            modal.close();
            resolve(null);
        });
        modal.overlay.addEventListener('click', event => {
            if (event.target === modal.overlay) { modal.close(); resolve(null); }
        });
    });
}

function showSettings(document, currentMode, smallMaxBytes, pendingPaths = []) {
    return new Promise(resolve => {
        const choices = [
            ['ask', '每次詢問'],
            ['small-only', `自動匯入小型圖片（${formatBytes(smallMaxBytes)} 以下）`],
            ['all', '自動匯入全部圖片'],
            ['skip', '不自動匯入']
        ];
        const modal = createModal(document, `
            <h3>外站圖片匯入設定</h3>
            <p>插入含有非本站圖片的元件時：</p>
            ${choices.map(([value, label]) => `<label class="coker-asset-choice"><input type="radio" name="coker-asset-mode" value="${value}" ${value === currentMode ? 'checked' : ''}> ${label}</label>`).join('')}
            <p>未匯入的圖片可能因來源網站變更、圖片刪除或路徑不同，導致前台出現掉圖。</p>
            ${pendingPaths.length ? `
                <details class="coker-asset-pending">
                    <summary>尚有 ${pendingPaths.length} 張圖片待處理</summary>
                    <ul>${pendingPaths.map(path => `<li title="${escapeHtml(path)}">${escapeHtml(path)}</li>`).join('')}</ul>
                </details>
            ` : '<p>目前沒有待處理的圖片。</p>'}
            <div class="coker-asset-actions"><button data-cancel>取消</button><button data-save>儲存設定</button><button class="primary" data-scan>儲存並掃描目前畫布</button></div>
        `);
        const finish = scan => {
            const mode = modal.dialog.querySelector('input[name="coker-asset-mode"]:checked')?.value || 'ask';
            modal.close();
            resolve({ mode, scan });
        };
        modal.dialog.querySelector('[data-cancel]').addEventListener('click', () => { modal.close(); resolve(null); });
        modal.dialog.querySelector('[data-save]').addEventListener('click', () => finish(false));
        modal.dialog.querySelector('[data-scan]').addEventListener('click', () => finish(true));
    });
}

function showLoading(document, total) {
    const modal = createModal(document, `
        <div class="coker-asset-loading"><div class="coker-asset-spinner"></div><h3 data-title>正在處理圖片</h3><p data-name></p><p data-progress>0 / ${total}</p></div>
    `);
    return {
        update(title, name, current) {
            modal.dialog.querySelector('[data-title]').textContent = title;
            modal.dialog.querySelector('[data-name]').textContent = name || '';
            modal.dialog.querySelector('[data-progress]').textContent = `${current} / ${total}`;
        },
        close: modal.close
    };
}

function formatBytes(bytes) {
    return bytes >= 1024 * 1024
        ? `${Math.round(bytes / 1024 / 1024 * 10) / 10} MB`
        : `${Math.round(bytes / 1024)} KB`;
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
