import { imageAssetAccept } from '../image/imageAssetTypes.js';

export const imageListComponentType = '相簿';
export const imageListEditorCommandId = 'coker:image-list:edit';
export const imageListUploadCommandId = 'coker:image-list:batch-upload';

const defaultItemHtml = '<a href="#" data-link-value="#" data-link-type="link" target="_self" class="imageItem align-items-center d-flex justify-content-center p-1 position-relative rounded templatecontent"><img src="/images/noImg.jpg" alt="" /><i class="material-symbols-outlined notranslate position-absolute">zoom_out_map</i></a>';
const uploadOnlyAssetType = 'coker-image-list-batch-upload';
const masonryPurposeCode = 'imglist-layout-change';
const masonryLayoutClass = 'layout-masonry';
const layoutTraitName = 'cokerLayout';
// 自動排序要兼顧的欄數（對應 Frame.css 的 4／3／2 欄斷點）
const masonryColumnCounts = [4, 3, 2];
// 每格除了圖片以外的高度（間距＋內距），以「一欄寬」為單位的估計值
const masonryItemGap = 0.06;
const masonryMaxRounds = 3;
// 張數太多時兩兩交換會卡住畫面，超過就不自動排序
const masonryMaxAutoArrangeItems = 150;
const masonryRatioTimeout = 3000;
const masonryConfirmMessage = '切換為瀑布式排版後，系統會依圖片比例自動調整圖片順序，讓各欄底部盡量對齊。\n目前的圖片順序將被取代，確定要切換嗎？';

// 往外層找最近的元件名稱（元件拖入時由 addBlockMetadata 蓋上），名稱不固定
function findBlockName(component) {
    let current = component;
    while (current) {
        const name = current.getAttributes?.()['data-block-name'];
        if (name) {
            return String(name);
        }
        current = current.parent?.();
    }
    return '';
}

// 找不到元件名稱或同名元件都沒有勾用途時，一律視為不可切換
function canSwitchToMasonry(editor, component) {
    const blockName = findBlockName(component);
    if (!blockName) {
        return false;
    }

    return editor.BlockManager.getAll().some(block => (
        String(block.get('label') || '') === blockName &&
        (block.get('purposeCodes') || []).includes(masonryPurposeCode)
    ));
}

// 實際排版由畫布內 Frame.js 負責；元件 class 更新後 DOM 才會同步，所以延到下一輪再通知
function refreshCanvasMasonry(editor, component) {
    globalThis.setTimeout(() => {
        const canvasWindow = editor.Canvas?.getWindow?.();
        const element = component.getEl?.();
        if (element && typeof canvasWindow?.refreshAlbumMasonry === 'function') {
            canvasWindow.refreshAlbumMasonry(element);
        }
    }, 0);
}

function syncLayoutTrait(editor, component) {
    const hasTrait = Boolean(component.getTrait?.(layoutTraitName));
    if (!canSwitchToMasonry(editor, component)) {
        if (hasTrait) {
            component.removeTrait(layoutTraitName);
        }
        return;
    }

    const isMasonry = (component.getClasses?.() || []).includes(masonryLayoutClass);
    component.set(layoutTraitName, isMasonry ? 'masonry' : 'grid', { silent: true });
    if (!hasTrait) {
        component.addTrait({
            type: 'select',
            name: layoutTraitName,
            label: '排版方式',
            changeProp: true,
            options: [
                { id: 'grid', label: '格列' },
                { id: 'masonry', label: '瀑布式' }
            ]
        }, { at: 0 });
    }
}
function normalizeAsset(asset) {
    return {
        src: asset?.get?.('src') || asset?.src || asset?.id || asset?.attributes?.src || '',
        name: asset?.get?.('name') || asset?.name || asset?.attributes?.name || '',
        type: asset?.get?.('type') || asset?.type || asset?.attributes?.type || '',
        mimeType: asset?.get?.('mimeType') || asset?.get?.('contentType') ||
            asset?.mimeType || asset?.contentType || asset?.attributes?.mimeType ||
            asset?.attributes?.contentType || '',
        fullSrc: asset?.get?.('fullSrc') || asset?.fullSrc || asset?.attributes?.fullSrc || '',
        mediumSrc: asset?.get?.('mediumSrc') || asset?.mediumSrc ||
            asset?.attributes?.mediumSrc || '',
        thumbnailSrc: asset?.get?.('thumbnailSrc') || asset?.thumbnailSrc ||
            asset?.attributes?.thumbnailSrc || '',
        guid: asset?.get?.('guid') || asset?.guid || asset?.attributes?.guid || '',
        mediumGuid: asset?.get?.('mediumGuid') || asset?.mediumGuid ||
            asset?.attributes?.mediumGuid || '',
        thumbnailGuid: asset?.get?.('thumbnailGuid') || asset?.thumbnailGuid ||
            asset?.attributes?.thumbnailGuid || ''
    };
}

function isImageAsset(asset) {
    const type = String(asset.type || '').toLowerCase();
    const mimeType = String(asset.mimeType || '').toLowerCase();
    const path = String(asset.name || asset.src || '').split(/[?#]/)[0];

    return type === 'image' || mimeType.startsWith('image/') ||
        /\.(avif|bmp|gif|jpe?g|png|svg|webp)$/i.test(path);
}

function fileNameWithoutExtension(value) {
    const path = String(value || '').split(/[?#]/)[0].replace(/^.*[\\/]/, '');
    let fileName = path;
    try {
        fileName = decodeURIComponent(path);
    } catch (_) {
        // Keep the original value when the file name is not URI encoded.
    }

    return fileName.replace(/\.[^.]+$/, '');
}

function resolveBackendUploadPath(editor, value) {
    const path = String(value || '').trim();
    const orgName = String(
        editor?.Canvas?.getWindow?.()?.OrgName || globalThis.OrgName || ''
    ).trim().replace(/^\/+|\/+$/g, '');
    if (!orgName || !path.toLowerCase().startsWith('/upload/')) {
        return path;
    }

    const afterUpload = path.substring('/upload/'.length);
    if (afterUpload.toLowerCase().startsWith(`${orgName.toLowerCase()}/`)) {
        return path;
    }

    return `/upload/${orgName}/${afterUpload}`;
}

function findComponentById(component, id) {
    if (!component || !id) {
        return null;
    }

    if (component.getAttributes?.().id === id) {
        return component;
    }

    const children = component.components?.();
    if (!children) {
        return null;
    }

    for (const child of children.models || []) {
        const match = findComponentById(child, id);
        if (match) {
            return match;
        }
    }

    return null;
}

function resolveTemplate(editor, imageList) {
    const templateId = String(
        imageList.getAttributes?.()['data-edit-template'] || ''
    ).trim().replace(/^#/, '');

    if (templateId) {
        const template = findComponentById(
            editor.DomComponents?.getWrapper?.(),
            templateId
        );
        if (template && template !== imageList) {
            return { component: template, templateId };
        }
    }

    return {
        component: imageList.components?.().at?.(0) || null,
        templateId: ''
    };
}

function revealTemplateClone(component, templateId) {
    if (!component) {
        return;
    }

    if (templateId && component.getAttributes?.().id === templateId) {
        component.removeAttributes?.(['id']);
    }

    component.removeAttributes?.(['hidden', 'aria-hidden']);
    const classes = component.getClasses?.() || [];
    if (classes.includes('d-none') || classes.includes('backstageType')) {
        component.setClass(classes.filter(className => (
            className !== 'd-none' && className !== 'backstageType'
        )));
    }
}

function findImage(component) {
    if (String(component?.get?.('tagName') || '').toLowerCase() === 'img') {
        return component;
    }

    return component?.find?.('img')?.[0] || null;
}

function findAnchor(component) {
    if (String(component?.get?.('tagName') || '').toLowerCase() === 'a') {
        return component;
    }

    return component?.find?.('a')?.[0] || null;
}

function setOptionalAttribute(component, name, value) {
    if (!component) return;
    if (value) component.addAttributes?.({ [name]: value });
    else component.removeAttributes?.([name]);
}

function getDirectImageItems(editor, imageList) {
    const templateId = String(
        imageList.getAttributes?.()['data-edit-template'] || ''
    ).trim().replace(/^#/, '');
    const children = Array.from(
        imageList.components?.().models || imageList.components?.() || []
    );

    return children.map(component => {
        const image = findImage(component);
        if (!image || (templateId && component.getAttributes?.().id === templateId)) {
            return null;
        }

        const attributes = image.getAttributes?.() || {};
        const anchor = findAnchor(component);
        const anchorAttributes = anchor?.getAttributes?.() || {};
        const imageSrc = resolveBackendUploadPath(editor,
            attributes['data-src'] || image.get?.('src') ||
            attributes.src || '/images/noImg.jpg');
        const fullSrc = resolveBackendUploadPath(editor,
            anchorAttributes['data-full-src'] || imageSrc);
        const mediumSrc = resolveBackendUploadPath(editor,
            anchorAttributes['data-medium-src'] || '');
        return {
            component,
            image,
            anchor,
            name: String(attributes.alt || ''),
            src: imageSrc,
            fullSrc,
            mediumSrc,
            guid: anchorAttributes['data-image-guid'] || '',
            mediumGuid: anchorAttributes['data-medium-guid'] || '',
            thumbnailGuid: anchorAttributes['data-thumbnail-guid'] || '',
            visible: !(component.getClasses?.() || []).some(className => (
                className === 'backstageType' || className === 'd-none'
            )),
            needsProcessing: !anchorAttributes['data-full-src'] ||
                !attributes['data-src'] ||
                !anchorAttributes['data-image-guid'] ||
                !anchorAttributes['data-thumbnail-guid']
        };
    }).filter(Boolean);
}

// 讀圖片的高寬比；用獨立的 Image 載入，相簿編輯中剛換掉、還沒套用到畫布的圖也讀得到，失敗或逾時當成正方形
function loadImageRatio(src) {
    return new Promise(resolve => {
        const image = new Image();
        const finish = () => {
            globalThis.clearTimeout(timer);
            resolve(image.naturalWidth ? image.naturalHeight / image.naturalWidth : 1);
        };
        const timer = globalThis.setTimeout(finish, masonryRatioTimeout);
        image.addEventListener('load', finish, { once: true });
        image.addEventListener('error', finish, { once: true });
        image.src = src;
    });
}

// 模擬 masonry：依序把每張圖放進目前最矮的欄（同高取最左），回傳最高與最矮欄的差距
function measureMasonryImbalance(ratios, order, columnCount) {
    const heights = new Array(columnCount).fill(0);
    order.forEach(index => {
        const shortest = heights.indexOf(Math.min(...heights));
        heights[shortest] += ratios[index] + masonryItemGap;
    });
    return Math.max(...heights) - Math.min(...heights);
}

function measureMasonryCost(ratios, order) {
    return masonryColumnCounts.reduce(
        (sum, columnCount) => sum + measureMasonryImbalance(ratios, order, columnCount), 0);
}

// 從原本順序出發兩兩交換，變整齊才保留，盡量少動使用者排好的順序
function balanceMasonryOrder(ratios) {
    const order = ratios.map((_, index) => index);
    let bestCost = measureMasonryCost(ratios, order);
    let isImproved = true;
    for (let round = 0; isImproved && round < masonryMaxRounds; round += 1) {
        isImproved = false;
        for (let i = 0; i < order.length - 1; i += 1) {
            for (let j = i + 1; j < order.length; j += 1) {
                [order[i], order[j]] = [order[j], order[i]];
                const nextCost = measureMasonryCost(ratios, order);
                if (nextCost < bestCost - 0.0001) {
                    bestCost = nextCost;
                    isImproved = true;
                } else {
                    [order[i], order[j]] = [order[j], order[i]];
                }
            }
        }
    }
    return order;
}

// 回傳依瀑布式重排後的新陣列；只動顯示中的圖片，隱藏圖留在原本的位置
async function orderItemsForMasonry(items) {
    const visibleItems = items.filter(item => item.visible);
    const minColumnCount = Math.min(...masonryColumnCounts);
    if (visibleItems.length <= minColumnCount || visibleItems.length > masonryMaxAutoArrangeItems) {
        return items;
    }

    const ratios = await Promise.all(visibleItems.map(item => loadImageRatio(item.src)));
    const orderedVisibleItems = balanceMasonryOrder(ratios).map(index => visibleItems[index]);
    let nextIndex = 0;
    return items.map(item => (item.visible ? orderedVisibleItems[nextIndex++] : item));
}

// 元件設定切成瀑布式時，直接重排畫布內的相簿；樣板與非圖片子元件留在原位
async function arrangeMasonryOrder(editor, imageList) {
    const items = getDirectImageItems(editor, imageList);
    const orderedItems = await orderItemsForMasonry(items);
    // 等圖片載入期間使用者可能已切回格列，就不再動順序
    if (imageList.get(layoutTraitName) !== 'masonry' ||
        orderedItems.every((item, index) => item === items[index])) {
        return;
    }

    const collection = imageList.components?.();
    if (!collection) {
        return;
    }
    const slots = new Set(items.map(item => item.component));
    let nextIndex = 0;
    collection.reset(Array.from(collection.models || collection).map(component => (
        slots.has(component) ? orderedItems[nextIndex++].component : component
    )));
    editor.refresh?.();
}

function confirmMasonrySwitch(editor) {
    if (editor.AlertManager?.confirm) {
        return editor.AlertManager.confirm(masonryConfirmMessage);
    }

    return Promise.resolve(Boolean(globalThis.confirm?.(masonryConfirmMessage)));
}

function applyImageListChanges(editor, imageList, items, originalItemComponents) {
    const collection = imageList.components?.();
    if (!collection) {
        return;
    }

    items.forEach(item => {
        item.image.set({ src: item.src });
        item.image.addAttributes?.({
            alt: item.name.trim(),
            'data-src': item.src
        });
        item.anchor?.addAttributes?.({
            'data-full-src': item.fullSrc,
            'data-image-guid': item.guid,
            'data-thumbnail-guid': item.thumbnailGuid
        });
        setOptionalAttribute(item.anchor, 'data-medium-src', item.mediumSrc);
        setOptionalAttribute(item.anchor, 'data-medium-guid', item.mediumGuid);
        const classes = item.component.getClasses?.() || [];
        const normalizedClasses = classes.filter(className => (
            className !== 'd-none' && className !== 'backstageType'
        ));
        item.component.setClass(item.visible
            ? normalizedClasses
            : [...normalizedClasses, 'backstageType']);
    });

    const orderedComponents = items.map(item => item.component);
    let nextIndex = 0;
    const nextChildren = [];
    Array.from(collection.models || collection).forEach(component => {
        if (originalItemComponents.has(component)) {
            if (nextIndex < orderedComponents.length) {
                nextChildren.push(orderedComponents[nextIndex++]);
            }
            return;
        }

        nextChildren.push(component);
    });

    collection.reset(nextChildren);
    editor.select?.(imageList);
    editor.refresh?.();
}

async function ensureGalleryItems(items, onProgress) {
    const pendingGroups = new Map();
    items.filter(item => (
        item.needsProcessing && String(item.fullSrc || '').includes('/upload/')
    )).forEach(item => {
        const group = pendingGroups.get(item.fullSrc) || [];
        group.push(item);
        pendingGroups.set(item.fullSrc, group);
    });
    const pendingEntries = Array.from(pendingGroups.entries());
    if (!pendingEntries.length) {
        onProgress?.(0, 0);
        return;
    }

    onProgress?.(0, pendingEntries.length,
        `發現 ${pendingEntries.length} 組相簿圖片，準備檢查大圖、中圖與縮圖規格。`);
    for (let index = 0; index < pendingEntries.length; index += 1) {
        const [sourcePath, matchingItems] = pendingEntries[index];
        const itemName = matchingItems[0].name || `圖片 ${index + 1}`;
        onProgress?.(index, pendingEntries.length,
            `相簿圖片處理中：${index + 1} / ${pendingEntries.length}（${itemName}）`);
        const response = await fetch('/api/FileUpload/EnsureGalleryImages', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ paths: [sourcePath] })
        });
        if (!response.ok) {
            throw new Error(`圖片處理失敗（HTTP ${response.status}）`);
        }

        const result = await response.json();
        if (result.errorFiles?.length) {
            throw new Error(result.errorFiles.join('\n'));
        }
        const file = result.files?.[0];
        if (!file) {
            throw new Error(`圖片「${matchingItems[0].name || index + 1}」沒有處理結果。`);
        }

        matchingItems.forEach(item => {
            item.fullSrc = file.path;
            item.mediumSrc = file.mediumPath || '';
            item.src = file.thumbnailPath || file.path;
            item.guid = file.guid || '';
            item.mediumGuid = file.mediumGuid || '';
            item.thumbnailGuid = file.thumbnailGuid || '';
            item.needsProcessing = false;
        });
        onProgress?.(index + 1, pendingEntries.length,
            `已完成大圖、中圖與縮圖處理：${index + 1} / ${pendingEntries.length}`);
    }
}

export function openImageListEditor(editor, imageList) {
    if (!imageList) {
        editor.AlertManager?.alert?.('請先選擇相簿元件。');
        return;
    }

    const document = editor.Modal?.getContainer?.()?.ownerDocument || globalThis.document;
    if (!document) {
        return;
    }

    const items = getDirectImageItems(editor, imageList);
    const originalItemComponents = new Set(items.map(item => item.component));
    const canSwitchLayout = canSwitchToMasonry(editor, imageList);
    let layout = (imageList.getClasses?.() || []).includes(masonryLayoutClass) ? 'masonry' : 'grid';
    const layoutSwitchHtml = canSwitchLayout ? `
                <div class="coker-image-list-layout" role="group" aria-label="排版方式">
                    <button type="button" data-layout="grid">格列</button>
                    <button type="button" data-layout="masonry">瀑布式</button>
                </div>` : '';
    const root = document.createElement('div');
    root.className = 'coker-image-list-editor';
    root.innerHTML = `
        <div class="coker-image-list-toolbar">
            <div class="coker-image-list-toolbar-actions">
                <button type="button" data-action="upload">
                    <span class="material-symbols-outlined" aria-hidden="true">upload</span>
                    <span>批次上傳圖片</span>
                </button>
                <button type="button" data-action="sort-name" aria-label="依名稱順序排序">
                    <span>依名稱排序</span>
                    <span class="material-symbols-outlined" data-role="sort-name-icon" aria-hidden="true">swap_vert</span>
                </button>${layoutSwitchHtml}
            </div>
            <span>可拖曳或輸入排序號碼；點擊圖片可更換。</span>
        </div>
        <div class="coker-image-list-grid" data-role="grid"></div>
        <div class="coker-image-list-empty" data-role="empty" hidden>相簿目前沒有圖片，請先批次上傳。</div>
        <div class="coker-image-list-progress" data-role="progress" hidden>
            <div class="coker-image-list-progress-label" data-role="progress-label"></div>
            <progress data-role="progress-bar" value="0" max="1"></progress>
        </div>
        <div class="coker-image-list-actions">
            <button type="button" data-action="cancel">取消</button>
            <button type="button" data-action="save" class="coker-image-list-primary">完成</button>
        </div>
    `;

    const grid = root.querySelector('[data-role="grid"]');
    const empty = root.querySelector('[data-role="empty"]');
    const progress = root.querySelector('[data-role="progress"]');
    const progressLabel = root.querySelector('[data-role="progress-label"]');
    const progressBar = root.querySelector('[data-role="progress-bar"]');
    let draggedItem = null;
    let nameSortDirection = 'asc';

    const render = () => {
        grid.replaceChildren();
        empty.hidden = items.length > 0;
        grid.hidden = items.length === 0;

        items.forEach((item, index) => {
            const card = document.createElement('article');
            const handle = document.createElement('div');
            const preview = document.createElement('img');
            const cardHeader = document.createElement('div');
            const orderField = document.createElement('label');
            const orderLabel = document.createElement('span');
            const orderInput = document.createElement('input');
            const deleteButton = document.createElement('button');
            const visibilityButton = document.createElement('button');
            const field = document.createElement('label');
            const label = document.createElement('span');
            const input = document.createElement('input');

            card.className = 'coker-image-list-card';
            card.dataset.index = String(index);
            cardHeader.className = 'coker-image-list-card-header';
            orderField.className = 'coker-image-list-order';
            orderLabel.textContent = '排序';
            orderInput.type = 'number';
            orderInput.min = '1';
            orderInput.max = String(items.length);
            orderInput.value = String(index + 1);
            orderInput.title = '輸入目標順序後即可移動';
            orderInput.addEventListener('focus', () => orderInput.select());
            orderInput.addEventListener('change', () => {
                const targetIndex = Math.min(
                    items.length - 1,
                    Math.max(0, Number.parseInt(orderInput.value, 10) - 1)
                );
                if (!Number.isFinite(targetIndex) || targetIndex === index) {
                    orderInput.value = String(index + 1);
                    return;
                }

                items.splice(index, 1);
                items.splice(targetIndex, 0, item);
                render();
            });
            handle.className = 'coker-image-list-drag-handle';
            handle.draggable = true;
            handle.title = '拖曳調整順序';
            handle.innerHTML = '<span class="material-symbols-outlined" aria-hidden="true">drag_indicator</span>';
            deleteButton.type = 'button';
            deleteButton.className = 'coker-image-list-delete';
            deleteButton.title = '刪除這個項目';
            deleteButton.setAttribute('aria-label', `刪除圖片項目 ${index + 1}`);
            deleteButton.innerHTML = '<span class="material-symbols-outlined" aria-hidden="true">delete</span>';
            deleteButton.addEventListener('click', () => {
                items.splice(index, 1);
                render();
            });
            visibilityButton.type = 'button';
            visibilityButton.className = 'coker-image-list-visibility';
            visibilityButton.title = item.visible ? '設定為隱藏' : '設定為顯示';
            visibilityButton.setAttribute('aria-pressed', String(!item.visible));
            visibilityButton.innerHTML = `<span class="material-symbols-outlined" aria-hidden="true">${item.visible ? 'visibility' : 'visibility_off'}</span>`;
            visibilityButton.addEventListener('click', () => {
                item.visible = !item.visible;
                render();
            });
            preview.src = item.src;
            preview.alt = item.name;
            preview.draggable = false;
            preview.tabIndex = 0;
            preview.title = '點擊更換圖片';
            preview.setAttribute('role', 'button');
            field.className = 'coker-image-list-name';
            label.textContent = `圖片名稱 ${index + 1}`;
            input.type = 'text';
            input.value = item.name;
            input.placeholder = '請輸入圖片名稱';
            input.addEventListener('input', () => {
                item.name = input.value;
                preview.alt = input.value;
            });

            const selectReplacement = () => {
                editor.Modal.close();
                let reopened = false;
                const reopen = () => {
                    if (reopened) {
                        return;
                    }
                    reopened = true;
                    editor.off('asset:close', reopen);
                    globalThis.setTimeout(() => {
                        editor.Modal.open({
                            title: '相簿編輯',
                            content: root,
                            attributes: { class: 'coker-image-list-modal' }
                        });
                        render();
                    }, 0);
                };

                editor.on('asset:close', reopen);
                editor.AssetManager.open({
                    types: ['image'],
                    accept: imageAssetAccept,
                    select(asset) {
                        const replacement = normalizeAsset(asset);
                        if (replacement.src) {
                            item.fullSrc = replacement.fullSrc || replacement.src;
                            item.mediumSrc = replacement.mediumSrc || '';
                            item.src = replacement.thumbnailSrc || replacement.src;
                            item.guid = replacement.guid;
                            item.mediumGuid = replacement.mediumGuid;
                            item.thumbnailGuid = replacement.thumbnailGuid;
                            item.needsProcessing = !replacement.fullSrc ||
                                !replacement.thumbnailSrc ||
                                !replacement.guid ||
                                !replacement.thumbnailGuid;
                            if (!item.name.trim()) {
                                item.name = fileNameWithoutExtension(
                                    replacement.name || replacement.src
                                );
                            }
                        }
                        editor.AssetManager.close();
                    }
                });
            };
            preview.addEventListener('click', selectReplacement);
            preview.addEventListener('keydown', event => {
                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    selectReplacement();
                }
            });

            orderField.append(orderLabel, orderInput);
            cardHeader.append(orderField, visibilityButton, handle, deleteButton);
            field.append(label, input);
            card.append(cardHeader, preview, field);
            handle.addEventListener('dragstart', event => {
                draggedItem = item;
                card.classList.add('is-dragging');
                if (event.dataTransfer) {
                    event.dataTransfer.effectAllowed = 'move';
                    event.dataTransfer.setData('text/plain', String(index));
                }
            });
            handle.addEventListener('dragend', () => {
                draggedItem = null;
                card.classList.remove('is-dragging');
                grid.querySelectorAll('.is-drag-over').forEach(element => {
                    element.classList.remove('is-drag-over');
                });
            });
            card.addEventListener('dragover', event => {
                if (draggedItem && draggedItem !== item) {
                    event.preventDefault();
                    card.classList.add('is-drag-over');
                }
            });
            card.addEventListener('dragleave', () => card.classList.remove('is-drag-over'));
            card.addEventListener('drop', event => {
                event.preventDefault();
                card.classList.remove('is-drag-over');
                if (!draggedItem || draggedItem === item) {
                    return;
                }

                const fromIndex = items.indexOf(draggedItem);
                const toIndex = items.indexOf(item);
                items.splice(fromIndex, 1);
                items.splice(toIndex, 0, draggedItem);
                draggedItem = null;
                render();
            });
            grid.append(card);
        });
    };

    const nameSortButton = root.querySelector('[data-action="sort-name"]');
    const nameSortIcon = root.querySelector('[data-role="sort-name-icon"]');
    nameSortButton.addEventListener('click', () => {
        const direction = nameSortDirection;
        const collator = new Intl.Collator('zh-Hant', {
            numeric: true,
            sensitivity: 'base'
        });
        items.sort((left, right) => {
            const leftName = String(left.name || '').trim();
            const rightName = String(right.name || '').trim();
            if (!leftName && !rightName) return 0;
            if (!leftName) return 1;
            if (!rightName) return -1;
            const result = collator.compare(leftName, rightName);
            return direction === 'asc' ? result : -result;
        });
        const isAscending = direction === 'asc';
        nameSortIcon.textContent = isAscending ? 'arrow_upward' : 'arrow_downward';
        nameSortButton.title = isAscending
            ? '目前為名稱順序；再次點擊改為逆序'
            : '目前為名稱逆序；再次點擊改為順序';
        nameSortButton.setAttribute('aria-label', nameSortButton.title);
        nameSortDirection = isAscending ? 'desc' : 'asc';
        render();
    });
        const layoutButtons = Array.from(root.querySelectorAll('[data-layout]'));
    const renderLayoutButtons = () => {
        layoutButtons.forEach(button => {
            const isActive = button.dataset.layout === layout;
            button.classList.toggle('is-active', isActive);
            button.setAttribute('aria-pressed', String(isActive));
        });
    };
    // 切到瀑布式先確認，再把編輯視窗裡的圖片換成重排後的順序；按「完成」才會套用到畫布
    layoutButtons.forEach(button => {
        button.addEventListener('click', async () => {
            const nextLayout = button.dataset.layout;
            if (nextLayout === layout) {
                return;
            }
            if (nextLayout === 'grid') {
                layout = 'grid';
                renderLayoutButtons();
                return;
            }
            if (!await confirmMasonrySwitch(editor)) {
                return;
            }

            layoutButtons.forEach(element => { element.disabled = true; });
            try {
                const orderedItems = await orderItemsForMasonry(items);
                items.splice(0, items.length, ...orderedItems);
                layout = 'masonry';
                render();
            } catch (error) {
                editor.AlertManager?.error?.(error?.message || '瀑布式自動排序失敗。');
            } finally {
                layoutButtons.forEach(element => { element.disabled = false; });
                renderLayoutButtons();
            }
        });
    });
    renderLayoutButtons();
    // 相簿內容與排版一起套用；isArranged 讓元件不再跳確認、也不再重排一次
    const applyChanges = () => {
        applyImageListChanges(editor, imageList, items, originalItemComponents);
        if (canSwitchLayout) {
            imageList.set(layoutTraitName, layout, { isArranged: true });
        }
    };
    root.querySelector('[data-action="upload"]').addEventListener('click', () => {
        applyChanges();
        editor.Modal.close();
        openImageListBatchUpload(editor, imageList, {
            onComplete() {
                globalThis.setTimeout(() => openImageListEditor(editor, imageList), 0);
            }
        });
    });
    root.querySelector('[data-action="cancel"]').addEventListener('click', () => {
        editor.Modal.close();
    });
    root.querySelector('[data-action="save"]').addEventListener('click', async event => {
        const button = event.currentTarget;
        const originalText = button.textContent;
        const hasPendingImages = items.some(item => (
            item.needsProcessing && String(item.fullSrc || '').includes('/upload/')
        ));
        if (!hasPendingImages) {
            applyChanges();
            editor.Modal.close();
            return;
        }

        button.disabled = true;
        button.textContent = '圖片規格檢查／縮圖處理中…';
        progress.hidden = false;
        try {
            await ensureGalleryItems(items, (completed, total, message) => {
                progressBar.max = Math.max(total, 1);
                progressBar.value = completed;
                progressLabel.textContent = message || (total
                    ? `正在檢查並補齊相簿圖片：${completed} / ${total}`
                    : '圖片路徑與縮圖參數完整，不需要重新處理。');
            });
            applyChanges();
            editor.Modal.close();
        } catch (error) {
            editor.AlertManager?.error?.(error?.message || '圖片批次處理失敗。');
            button.disabled = false;
            button.textContent = originalText;
        }
    });

    editor.Modal.open({
        title: '相簿編輯',
        content: root,
        attributes: { class: 'coker-image-list-modal' }
    });
    render();
}

function appendImageItem(imageList, template, asset) {
    let item;
    if (template.component) {
        item = template.component.clone();
        revealTemplateClone(item, template.templateId);
        imageList.append(item);
    } else {
        item = imageList.append(defaultItemHtml)?.[0] || null;
    }

    const image = findImage(item);
    if (!image) {
        item?.remove?.();
        return false;
    }

    image.addAttributes?.({
        alt: fileNameWithoutExtension(asset.name || asset.src),
        'data-src': asset.thumbnailSrc || asset.src
    });
    const anchor = findAnchor(item);
    const fullSrc = asset.fullSrc || asset.src;
    const thumbnailSrc = asset.thumbnailSrc || asset.src;
    image.set({ src: thumbnailSrc });
    anchor?.addAttributes?.({
        'data-full-src': fullSrc,
        'data-image-guid': asset.guid,
        'data-thumbnail-guid': asset.thumbnailGuid
    });
    setOptionalAttribute(anchor, 'data-medium-src', asset.mediumSrc);
    setOptionalAttribute(anchor, 'data-medium-guid', asset.mediumGuid);
    return true;
}

function appendUploadedImages(editor, imageList, assets) {
    const template = resolveTemplate(editor, imageList);
    let addedCount = 0;

    assets.forEach(asset => {
        if (asset.src && appendImageItem(imageList, template, asset)) {
            addedCount += 1;
        }
    });

    if (addedCount) {
        editor.select?.(imageList);
        editor.refresh?.();
    }

    return addedCount;
}

function createUploadMessage(document) {
    const element = document?.createElement?.('div');
    if (!element) {
        return null;
    }

    element.className = 'coker-swiper-upload-empty coker-image-list-upload-message';
    element.setAttribute('role', 'status');
    element.setAttribute('aria-live', 'polite');
    element.textContent = '請從左側一次選取或拖曳多張圖片；上傳成功後會自動加入相簿。';
    return element;
}

export function openImageListBatchUpload(editor, imageList, options = {}) {
    if (!imageList) {
        editor.AlertManager?.alert?.('請先選擇相簿元件。');
        return;
    }

    const assetManager = editor.AssetManager;
    const collectedAssets = new Map();
    let closeTimer = null;
    let uploadInput = null;
    let message = null;
    let restored = false;

    const scheduleClose = () => {
        globalThis.clearTimeout(closeTimer);
        closeTimer = globalThis.setTimeout(() => {
            if (collectedAssets.size) {
                assetManager.close();
            }
        }, 80);
    };
    const setMessage = value => {
        if (message) {
            message.textContent = value;
        }
    };
    const collectAsset = value => {
        const asset = normalizeAsset(value);
        if (!asset.src || !isImageAsset(asset)) {
            return;
        }

        collectedAssets.set(asset.src, asset);
        setMessage(`已上傳 ${collectedAssets.size} 張圖片，正在建立相簿項目…`);
        scheduleClose();
    };
    const collectUploadResult = result => {
        const assets = result?.assets || result?.data || [];
        assets.forEach(collectAsset);
    };
    const handleFileSelection = event => {
        const count = event.target?.files?.length || 0;
        setMessage(count > 1
            ? `正在上傳 ${count} 張圖片，請稍候…`
            : '圖片上傳中，請稍候…');
    };
    const restoreEditor = () => {
        if (restored) {
            return;
        }

        restored = true;
        globalThis.clearTimeout(closeTimer);
        uploadInput?.removeEventListener('change', handleFileSelection);
        message?.remove();
        editor.off('asset:add', collectAsset);
        editor.off('asset:upload:end', collectUploadResult);
        editor.off('coker:asset-upload:complete', collectUploadResult);

        appendUploadedImages(
            editor,
            imageList,
            Array.from(collectedAssets.values())
        );
        options.onComplete?.();
    };

    editor.on('asset:add', collectAsset);
    editor.on('asset:upload:end', collectUploadResult);
    editor.on('coker:asset-upload:complete', collectUploadResult);
    editor.once('asset:close', restoreEditor);
    assetManager.open({
        // No persisted asset uses this type, so the dialog is dedicated to
        // uploading and does not mix the operation with asset selection.
        types: [uploadOnlyAssetType],
        accept: imageAssetAccept
    });

    globalThis.setTimeout(() => {
        const document = editor.Modal?.getContainer?.()?.ownerDocument || globalThis.document;
        uploadInput = document?.querySelector(
            '.gjs-mdl-content .gjs-am-file-uploader input[type="file"]'
        ) || null;
        uploadInput?.addEventListener('change', handleFileSelection);

        const assets = document?.querySelector('.gjs-mdl-content .gjs-am-assets');
        if (assets) {
            message = createUploadMessage(document);
            if (message) {
                assets.append(message);
            }
        }
    }, 0);
}

export function imageListPlugin(editor) {
    editor.Commands.add(imageListEditorCommandId, {
        run(currentEditor, sender, commandOptions = {}) {
            openImageListEditor(
                currentEditor,
                commandOptions.component || currentEditor.getSelected?.()
            );
        }
    });

    editor.Commands.add(imageListUploadCommandId, {
        run(currentEditor, sender, commandOptions = {}) {
            openImageListBatchUpload(
                currentEditor,
                commandOptions.component || currentEditor.getSelected?.()
            );
        }
    });

    editor.DomComponents.addType(imageListComponentType, {
        isComponent(element) {
            return element?.classList?.contains('imageList')
                ? { type: imageListComponentType, name: imageListComponentType }
                : undefined;
        },
        model: {
            defaults: {
                name: imageListComponentType,
                traits: [{
                    name: 'data-edit-template',
                    type: 'text',
                    label: '自訂樣板 ID',
                    placeholder: '例如：galleryItemTemplate'
                }, {
                    type: 'button',
                    text: '開啟相簿編輯',
                    command: imageListEditorCommandId
                }]
            },
            init() {
                this.on(`change:${layoutTraitName}`, this.handleLayoutChange);
            },
            async handleLayoutChange(model, value, options = {}) {
                if (this.get(layoutTraitName) !== 'masonry') {
                    this.removeClass(masonryLayoutClass);
                    refreshCanvasMasonry(editor, this);
                    return;
                }
                // 相簿編輯已確認並排好順序時，只需要套用 class
                if (!options.isArranged && !await confirmMasonrySwitch(editor)) {
                    this.set(layoutTraitName, 'grid');
                    return;
                }

                this.addClass(masonryLayoutClass);
                refreshCanvasMasonry(editor, this);
                if (!options.isArranged) {
                    arrangeMasonryOrder(editor, this).catch(error => {
                        console.error('瀑布式自動排序失敗', error);
                    });
                }
            }
        }
    });

        editor.on('component:selected', component => {
        if (component?.get?.('type') !== imageListComponentType) {
            return;
        }

        syncLayoutTrait(editor, component);

        const toolbar = component.get('toolbar') || [];
        if (toolbar.some(item => item.command === imageListEditorCommandId)) {
            return;
        }

        component.set('toolbar', [...toolbar, {
            attributes: {
                class: 'fa fa-images',
                title: '開啟相簿編輯'
            },
            command: imageListEditorCommandId
        }]);
    });
}
