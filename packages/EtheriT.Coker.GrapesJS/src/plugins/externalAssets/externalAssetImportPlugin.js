const styleId = 'coker-external-asset-import-style';

export function externalAssetImportPlugin(editor, options = {}) {
    const hostWindow = typeof window === 'undefined' ? null : window;
    const hostDocument = hostWindow?.document;
    const canImportExternalImages = hostDocument
        ?.querySelector('meta[name="coker-can-import-external-canvas-images"]')
        ?.getAttribute('content') === 'true';
    if (!canImportExternalImages) return;
    const apiRoot = options.apiRoot || '/api/FileUpload';
    let isReady = false;
    let processing = Promise.resolve();
    let toolbarButton = null;
    let refreshTimer = null;
    let scanTimer = null;

    ensureStyles(hostDocument);

    const getOrgName = () => String(
        hostDocument?.querySelector('meta[name="coker-org-name"]')?.getAttribute('content')
        || editor?.Canvas?.getWindow?.()?.OrgName
        || hostWindow?.OrgName
        || ''
    ).replace(/^\/+|\/+$/g, '');

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
        const count = collectForeignPaths().length;
        if (!toolbarButton) return count;
        toolbarButton.set('label', count > 0
            ? `<i class="fa fa-image" aria-hidden="true"></i><span class="coker-asset-toolbar-count">${count}</span>`
            : '<i class="fa fa-image" aria-hidden="true"></i>');
        toolbarButton.set('attributes', {
            title: count > 0
                ? `檢查外部圖片（尚有 ${count} 張）`
                : '檢查外部圖片'
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
        try {
            loading.update(`正在匯入 ${items.length} 張圖片`, '', 0);
            const result = await request('ImportCanvasImages', items.map(item => item.path));
            const importedFiles = result.files || result.Files || [];
            importedFiles.forEach(imported => {
                const sourcePath = imported?.sourcePath || imported?.SourcePath;
                const newPath = imported?.path || imported?.Path;
                if (!sourcePath || !newPath) return;
                const item = items.find(candidate => candidate.path === sourcePath);
                replacements.set(sourcePath, newPath);
                importedAssets.push({
                    src: newPath,
                    fullSrc: newPath,
                    thumbnailSrc: imported?.thumbnailPath || imported?.ThumbnailPath || newPath,
                    name: imported?.name || imported?.Name || item?.name || '',
                    guid: imported?.guid || imported?.Guid || '',
                    type: 'image'
                });
            });
            loading.update('正在套用圖片路徑', '', items.length);
            applyReplacements(editor, replacements);
            addImportedAssets(editor, importedAssets);
        } finally {
            loading.close();
        }
        refreshPendingCount();
        const failedItems = items.filter(item => !replacements.has(item.path));
        return { imported: replacements.size, failedItems };
    };

    const scanAndHandle = async ({ forceAsk = false } = {}) => {
        const paths = collectForeignPaths();
        refreshPendingCount();
        if (!paths.length) {
            if (forceAsk) options.adapter?.ui?.success?.('目前畫布沒有需要匯入的外站圖片。');
            return;
        }

        let items;
        const inspecting = showLoading(hostDocument, paths.length);
        inspecting.update('正在分析元件圖片', '', 0);
        try {
            const inspected = await request('InspectCanvasImages', paths);
            items = (inspected.items || inspected.Items || [])
                .map(normalizeInspectItem)
                .map(item => ({ ...item, usages: describeAssetUsages(editor, item.path) }));
        } finally {
            inspecting.close();
        }
        refreshPendingCount();
        const choice = await showImportPrompt(hostDocument, items);
        if (!choice) return;
        applyUnavailableActions(editor, choice.unavailableActions, hostDocument);

        const selectedPaths = new Set(choice.importPaths);
        const selected = items.filter(item => item.canImport && selectedPaths.has(item.path));
        if (!selected.length) {
            refreshPendingCount();
            return;
        }

        const result = await importItems(selected);
        if (result.failedItems.length > 0) {
            options.adapter?.ui?.alert?.(
                `已匯入 ${result.imported} 張圖片，另有 ${result.failedItems.length} 張未成功，已保留原圖。`
            );
        } else {
            options.adapter?.ui?.success?.(`已匯入 ${result.imported} 張圖片。`);
        }
    };

    const openImageList = () => {
        processing = processing
            .then(() => scanAndHandle({ forceAsk: true }))
            .catch(error => options.adapter?.ui?.error?.(error.message));
    };

    const scheduleImageList = () => {
        if (scanTimer) clearTimeout(scanTimer);
        scanTimer = setTimeout(() => {
            scanTimer = null;
            processing = processing
                .then(() => scanAndHandle())
                .catch(error => options.adapter?.ui?.error?.(error.message));
        }, 300);
    };

    editor.Commands.add('coker:external-assets-settings', { run: openImageList });
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
            attributes: { title: '檢查外部圖片' },
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
        scheduleImageList();
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

function describeAssetUsages(editor, path) {
    const usages = [];
    const addUsage = (type, nearbyText = '') => {
        const key = `${type}|${nearbyText}`;
        if (!usages.some(usage => usage.key === key)) usages.push({ key, type, nearbyText });
    };

    editor.getWrapper?.()?.onAll?.(component => {
        const attributes = component.getAttributes?.() || {};
        const tagName = String(component.get?.('tagName') || '').toLowerCase();
        const componentType = String(component.get?.('type') || '').toLowerCase();
        const isImage = tagName === 'img' || tagName === 'source' || componentType === 'image';
        const nearbyText = getComponentNearbyText(component, !isImage);

        Object.entries(attributes).forEach(([name, value]) => {
            if (typeof value !== 'string' || !referenceContainsPath(value, path)) return;
            if (name === 'style') addUsage(getInlineStyleUsageType(value, path), nearbyText);
            else if (name === 'poster' || name === 'data-coker-poster') addUsage('影片封面', nearbyText);
            else if (isImage || ['src', 'data-src', 'srcset', 'data-full-src', 'data-medium-src'].includes(name)) addUsage('圖片', nearbyText);
            else addUsage('圖片設定', nearbyText);
        });

        ['src', 'poster'].forEach(property => {
            const value = component.get?.(property);
            if (!referenceContainsPath(value, path)) return;
            addUsage(property === 'poster' ? '影片封面' : '圖片', nearbyText);
        });

        Object.entries(component.getStyle?.() || {}).forEach(([property, value]) => {
            if (cssValueReferencesPath(value, path)) addUsage(getStyleUsageType(property), nearbyText);
        });
    });

    editor.CssComposer?.getAll?.().forEach(rule => {
        Object.entries(rule.getStyle?.() || {}).forEach(([property, value]) => {
            if (!cssValueReferencesPath(value, path)) return;
            const selector = rule.getSelectorsString?.() || '';
            const element = findCanvasElement(editor, selector);
            addUsage(getStyleUsageType(property), getElementNearbyText(element, true));
        });
    });

    return usages.slice(0, 3).map(({ type, nearbyText }) => ({ type, nearbyText }));
}

function getInlineStyleUsageType(value, path) {
    const declaration = String(value || '')
        .split(';')
        .find(item => referenceContainsPath(item, path)) || '';
    return getStyleUsageType(declaration.split(':')[0]);
}

function getStyleUsageType(property) {
    const name = String(property || '').toLowerCase();
    if (name.includes('background')) return '背景圖';
    if (name.includes('poster')) return '影片封面';
    return '樣式圖片';
}

function referenceContainsPath(value, path) {
    const text = String(value || '');
    if (text.includes(path)) return true;
    const protocolRelativePath = String(path || '').replace(/^https?:/i, '');
    return protocolRelativePath !== path && text.includes(protocolRelativePath);
}

function getComponentNearbyText(component, includeOwnText) {
    const elementText = getElementNearbyText(component.getEl?.(), includeOwnText);
    if (elementText) return elementText;

    let current = component;
    for (let depth = 0; current && depth < 4; depth++) {
        const attributes = current.getAttributes?.() || {};
        const candidates = [
            attributes.alt,
            attributes.title,
            attributes['aria-label'],
            attributes['data-block-name'],
            includeOwnText || depth > 0 ? current.get?.('content') : ''
        ];
        const text = candidates.map(cleanNearbyText).find(Boolean);
        if (text) return text;
        current = current.parent?.();
    }
    return '';
}

function getElementNearbyText(element, includeOwnText) {
    if (!element) return '';
    const attributeText = ['alt', 'title', 'aria-label']
        .map(name => cleanNearbyText(element.getAttribute?.(name)))
        .find(Boolean);
    if (includeOwnText) {
        const ownText = getPreferredContextText(element);
        if (ownText) return ownText;
    }

    let current = element;
    for (let depth = 0; current?.parentElement && depth < 4; depth++) {
        const siblings = [...current.parentElement.children];
        const currentIndex = siblings.indexOf(current);
        for (let distance = 1; distance <= 2; distance++) {
            const text = [siblings[currentIndex - distance], siblings[currentIndex + distance]]
                .map(getPreferredContextText)
                .find(Boolean);
            if (text) return text;
        }
        current = current.parentElement;
    }
    return attributeText || '';
}

function getPreferredContextText(element) {
    if (!element) return '';
    const preferredSelectors = [
        '.cht',
        '[lang^="zh"]',
        'h1, h2, h3, h4, h5, h6',
        '.name',
        '.title',
        '[class*="-title"]',
        '[class*="_title"]'
    ];
    for (const selector of preferredSelectors) {
        const matched = element.matches?.(selector)
            ? element
            : element.querySelector?.(selector);
        const text = getUsefulContextText(matched?.textContent);
        if (text) return text;
    }
    return getUsefulContextText(element.textContent);
}

function getUsefulContextText(value) {
    const text = cleanNearbyText(value);
    if (!text) return '';

    // Animated counters and telephone numbers are often split into one span
    // per character. A nearby "0", "-", or similarly short fragment does not
    // identify the image location, so keep searching for the section label.
    const digitCount = (text.match(/\d/g) || []).length;
    const hasWords = /[A-Za-z\u3400-\u9fff]/u.test(text);
    if (!hasWords && digitCount <= 2) return '';
    return text;
}

function cleanNearbyText(value) {
    const text = String(value || '')
        .replace(/<[^>]*>/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
    if (!text || /^(?:https?:)?\/\//i.test(text)) return '';
    return text.length > 48 ? `${text.slice(0, 48)}…` : text;
}

function findCanvasElement(editor, selector) {
    if (!selector) return null;
    try {
        return editor.Canvas?.getDocument?.()?.querySelector(selector) || null;
    } catch {
        return null;
    }
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
        .forEach(([source, target]) => {
            const variants = [source];
            const protocolRelative = source.replace(/^https?:/i, '');
            if (protocolRelative !== source) variants.push(protocolRelative);
            variants.forEach(variant => { result = result.split(variant).join(target); });
        });
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
    if (Object.values(attributes).some(value => typeof value === 'string' && referenceContainsPath(value, path))) return true;
    if (['src', 'poster'].some(property => referenceContainsPath(component.get?.(property), path))) return true;
    if (Object.values(component.getStyle?.() || {}).some(value => cssValueReferencesPath(value, path))) return true;
    return String(component.get?.('content') || '').includes(path);
}

function isImageComponentReference(component, path) {
    const tagName = String(component.get?.('tagName') || '').toLowerCase();
    const type = String(component.get?.('type') || '').toLowerCase();
    if (tagName !== 'img' && tagName !== 'source' && type !== 'image') return false;
    const attributes = component.getAttributes?.() || {};
    return ['src', 'data-src', 'srcset', 'data-full-src', 'data-medium-src']
        .some(name => referenceContainsPath(attributes[name] || component.get?.(name), path));
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
        if (!paths.some(path => referenceContainsPath(value, path))) return;
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
        if (typeof value === 'string' && paths.some(path => referenceContainsPath(value, path))) component.set?.(property, '');
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
    return parseCssUrls(value).some(url => referenceContainsPath(url, path));
}

function removeSrcsetPaths(value, paths) {
    return String(value || '')
        .split(',')
        .map(item => item.trim())
        .filter(item => item && ![...paths].some(path => referenceContainsPath(item.split(/\s+/)[0], path)))
        .join(', ');
}

function ensureStyles(document) {
    if (!document || document.getElementById(styleId)) return;
    const style = document.createElement('style');
    style.id = styleId;
    style.textContent = `
        .coker-asset-modal{position:fixed;inset:0;z-index:2147483000;background:rgba(15,23,42,.55);display:flex;align-items:center;justify-content:center;padding:20px}
        .coker-asset-dialog{width:min(720px,100%);max-height:calc(100vh - 40px);overflow:auto;background:#fff;color:#1f2937;border-radius:12px;box-shadow:0 24px 70px rgba(0,0,0,.3);padding:24px}
        .coker-asset-dialog h3{font-size:20px;margin:0 0 12px}.coker-asset-dialog p{line-height:1.6;margin:8px 0}
        .coker-asset-actions{display:flex;gap:8px;justify-content:flex-end;flex-wrap:wrap;margin-top:18px}
        .coker-asset-actions button{border:1px solid #cbd5e1;border-radius:7px;background:#fff;padding:8px 14px;cursor:pointer}.coker-asset-actions button.primary{background:#2563eb;color:#fff;border-color:#2563eb}
        .coker-asset-loading{text-align:center}.coker-asset-spinner{width:36px;height:36px;border:4px solid #dbeafe;border-top-color:#2563eb;border-radius:50%;animation:coker-asset-spin .8s linear infinite;margin:0 auto 14px}
        .coker-asset-list{display:grid;gap:10px;margin-top:16px}.coker-asset-item{display:grid;grid-template-columns:96px minmax(0,1fr);gap:14px;align-items:center;border:1px solid #e2e8f0;border-radius:10px;padding:10px}.coker-asset-item.problem{border-color:#fecaca;background:#fffafa}
        .coker-asset-preview{position:relative;height:72px;border-radius:7px;background:#f1f5f9;display:flex;align-items:center;justify-content:center;overflow:hidden;color:#94a3b8}.coker-asset-preview img{position:absolute;inset:0;width:100%;height:100%;object-fit:contain;background:#fff}.coker-asset-preview i{font-size:24px}
        .coker-asset-name{font-weight:600;overflow-wrap:anywhere}.coker-asset-meta{color:#64748b;font-size:13px;margin-top:4px}.coker-asset-source{font-size:12px;margin-top:5px}.coker-asset-source summary{cursor:pointer;color:#64748b}.coker-asset-path{margin-top:5px;overflow-wrap:anywhere;color:#64748b}
        .coker-asset-usages{display:grid;gap:3px;margin-top:5px}.coker-asset-usage{font-size:13px;color:#475569}.coker-asset-usage-type{display:inline-block;border-radius:999px;background:#e2e8f0;color:#334155;padding:1px 7px;margin-right:5px}.coker-asset-item.problem .coker-asset-usage-type{background:#fee2e2;color:#991b1b}
        .coker-asset-select{display:inline-flex;align-items:center;gap:6px;margin-top:9px;font-size:14px}.coker-asset-error{color:#b91c1c;font-size:13px;margin-top:5px}.coker-asset-item-actions{display:flex;gap:12px;flex-wrap:wrap;margin-top:9px;font-size:13px}
        .coker-asset-toolbar-count{display:inline-block;margin-left:2px;color:#fbbf24;font-size:9px;line-height:1;vertical-align:top}
        @media (max-width:600px){.coker-asset-item{grid-template-columns:72px minmax(0,1fr)}.coker-asset-preview{height:60px}}
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

function showImportPrompt(document, items) {
    return new Promise(resolve => {
        const modal = createModal(document, `
            <h3>外部圖片</h3>
            <p>下列 ${items.length} 張圖片來自其他網站或站台，請確認要如何處理。</p>
            <div class="coker-asset-list">
                ${items.map((item, index) => `
                    <div class="coker-asset-item ${item.canImport ? '' : 'problem'}">
                        <div class="coker-asset-preview">
                            <i class="fa fa-image" aria-hidden="true"></i>
                            <img data-preview src="${escapeHtml(item.path)}" alt="" loading="lazy">
                        </div>
                        <div>
                            <div class="coker-asset-name">${escapeHtml(getImageDisplayName(item, index))}</div>
                            <div class="coker-asset-meta">圖片大小：${item.size == null ? '無法取得' : formatBytes(item.size)}</div>
                            <div class="coker-asset-usages">
                                ${(item.usages || []).length
                                    ? item.usages.map(usage => `
                                        <div class="coker-asset-usage"><span class="coker-asset-usage-type">${escapeHtml(usage.type)}</span>${usage.nearbyText ? `位於「${escapeHtml(usage.nearbyText)}」附近` : '畫布中'}</div>
                                    `).join('')
                                    : '<div class="coker-asset-usage"><span class="coker-asset-usage-type">圖片設定</span>無法辨識附近文字</div>'}
                            </div>
                            <details class="coker-asset-source">
                                <summary>查看圖片來源</summary>
                                <div class="coker-asset-path">${escapeHtml(item.path)}</div>
                            </details>
                            ${item.canImport
                                ? `<label class="coker-asset-select"><input type="checkbox" data-import-index="${index}" checked> 匯入本站</label>`
                                : `
                                    <div class="coker-asset-error">目前無法匯入這張圖片</div>
                                    ${item.error ? `<details class="coker-asset-source"><summary>查看原因</summary><div class="coker-asset-path">${escapeHtml(item.error)}</div></details>` : ''}
                                    <div class="coker-asset-item-actions">
                                        <label><input type="radio" name="coker-problem-${index}" value="skip" checked> 保留原狀</label>
                                        <label><input type="radio" name="coker-problem-${index}" value="remove"> 移除圖片設定</label>
                                        <label><input type="radio" name="coker-problem-${index}" value="delete"> 刪除圖片</label>
                                    </div>
                                `}
                        </div>
                    </div>
                `).join('')}
            </div>
            <div class="coker-asset-actions"><button data-cancel>取消</button><button class="primary" data-apply>套用選擇</button></div>
        `);
        modal.dialog.querySelectorAll('[data-preview]').forEach(image => {
            image.addEventListener('error', () => { image.hidden = true; });
        });
        modal.dialog.querySelector('[data-apply]').addEventListener('click', () => {
            const result = {
                importPaths: items
                    .filter((item, index) => item.canImport && modal.dialog.querySelector(`[data-import-index="${index}"]`)?.checked)
                    .map(item => item.path),
                unavailableActions: items
                    .map((item, index) => item.canImport ? null : ({
                        path: item.path,
                        mode: modal.dialog.querySelector(`input[name="coker-problem-${index}"]:checked`)?.value || 'skip'
                    }))
                    .filter(Boolean)
            };
            modal.close();
            resolve(result);
        });
        modal.dialog.querySelector('[data-cancel]').addEventListener('click', () => {
            modal.close();
            resolve(null);
        });
        modal.overlay.addEventListener('click', event => {
            if (event.target === modal.overlay) { modal.close(); resolve(null); }
        });
    });
}

function getImageDisplayName(item, index) {
    if (item.name) return item.name;
    try {
        const pathName = new URL(item.path, 'http://localhost').pathname;
        return decodeURIComponent(pathName.split('/').filter(Boolean).pop() || `圖片 ${index + 1}`);
    } catch {
        return `圖片 ${index + 1}`;
    }
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
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024 * 10) / 10} KB`;
    return `${Math.round(bytes / 1024 / 1024 * 10) / 10} MB`;
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
