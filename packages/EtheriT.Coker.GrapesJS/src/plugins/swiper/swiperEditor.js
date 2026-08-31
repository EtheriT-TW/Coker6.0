import { createSlideId, normalizeSlide, swiperMediaTypes } from './swiperModel.js';
import {
    findSwiperThumbnailWrapperComponent,
    findSwiperWrapperComponent,
    parseSwiperSlides
} from './swiperParser.js';
import { renderSlides, renderThumbnailSlides } from './swiperRenderer.js';

export function openSwiperEditor(editor, component) {
    const controller = new SwiperEditorController(editor, component);
    controller.open();
    return controller;
}

class SwiperEditorController {
    constructor(editor, component) {
        this.editor = editor;
        this.component = component;
        this.slides = parseSwiperSlides(component);
        const templateSlide = this.slides.find(slide => slide.templateHtml);
        this.slideTemplateHtml = templateSlide?.templateHtml || '';
        this.slideTemplateTextFields = cloneTextFields(templateSlide?.textFields);
        this.slideTemplateThumbnailHtml = templateSlide?.thumbnailTemplateHtml || '';
        this.isVerticalSwiper = component.getClasses?.().includes('vertical_swiper_thumbs');
        this.duplicateSourceIds = new Map();
        this.dirtyTextPaths = new Map();
        this.mainSlideComponents = mapMainSlideComponents(component, this.slides);
        this.thumbnailSlideComponents = mapThumbnailSlideComponents(component, this.slides);
        this.supportsCaption = this.slides.some(slide => slide.hasCaption);
        this.initialState = serializeSlideState(this.slides);
        this.selectedIds = new Set();
        this.activeId = this.slides[0]?.id || null;
        this.draggedId = null;
        this.root = this.createRoot();
    }

    open() {
        this.editor.Modal.open({
            title: '輪播編輯',
            content: this.root,
            attributes: { class: 'coker-swiper-modal' }
        });
        this.render();
    }

    createRoot() {
        const root = document.createElement('div');
        root.className = 'coker-swiper-editor';
        root.innerHTML = `
            <div class="coker-swiper-toolbar">
                <div class="coker-swiper-actions">
                    <button type="button" data-action="add">新增</button>
                    <button type="button" data-action="select-all">全選</button>
                    <button type="button" data-action="open-bulk" disabled>批次設定（已選 0 筆）</button>
                    <button type="button" data-action="delete">刪除所選</button>
                </div>
                <span class="coker-swiper-batch-hint">勾選左側項目後，可進行批次設定或刪除。</span>
            </div>
            <div class="coker-swiper-workspace">
                <div class="coker-swiper-list" data-role="list"></div>
                <div class="coker-swiper-settings">
                    <div data-role="empty">請新增或選擇輪播項目。</div>
                    <form data-role="form" hidden>
                        <label>媒體類型
                            <select data-field="type">
                                <option value="image">圖片</option>
                                <option value="video">影片檔</option>
                                <option value="embed">嵌入影片</option>
                            </select>
                        </label>
                        <label data-media="image">圖片網址
                            <span class="coker-swiper-input-group">
                                <input type="text" data-field="src">
                                <button type="button" data-action="select-asset" data-asset-field="src" data-asset-type="image" title="選擇圖片" aria-label="選擇圖片">
                                    <span class="material-symbols-outlined">image_search</span>
                                </button>
                            </span>
                        </label>
                        <label data-media="video">影片網址／檔案
                            <span class="coker-swiper-input-group">
                                <input type="text" data-field="src">
                                <button type="button" data-action="select-asset" data-asset-field="src" data-asset-type="video" title="選擇影片檔" aria-label="選擇影片檔">
                                    <span class="material-symbols-outlined">video_file</span>
                                </button>
                            </span>
                        </label>
                        <label data-media="embed">嵌入影片網址
                            <input type="text" data-field="src">
                        </label>
                        <label data-media="video,embed">預覽圖
                            <span class="coker-swiper-input-group">
                                <input type="text" data-field="poster">
                                <button type="button" data-action="select-asset" data-asset-field="poster" data-asset-type="image" title="選擇預覽圖" aria-label="選擇預覽圖">
                                    <span class="material-symbols-outlined">image_search</span>
                                </button>
                            </span>
                        </label>
                        <label>替代文字／標題
                            <input type="text" data-field="title">
                        </label>
                        <label data-role="caption-field">說明文字
                            <textarea rows="3" data-field="caption"></textarea>
                        </label>
                        <label data-media="image">連結網址
                            <input type="text" data-field="link">
                        </label>
                        <label data-media="image">連結開啟方式
                            <select data-field="target">
                                <option value="_self">目前視窗</option>
                                <option value="_blank">另開視窗</option>
                            </select>
                        </label>
                        <div class="coker-swiper-field-row">
                            <label data-media="video,embed">開始秒數
                                <input type="number" min="0" step="1" data-field="startTime">
                            </label>
                            <label>停留秒數
                                <input type="number" min="0.1" step="0.1" data-field="duration">
                            </label>
                            <label data-media="video,embed">顯示比例
                                <select data-field="ratio">
                                    <option value="16x9">16:9</option>
                                    <option value="4x3">4:3</option>
                                    <option value="1x1">1:1</option>
                                    <option value="9x16">9:16</option>
                                </select>
                            </label>
                        </div>
                        <label class="coker-swiper-check"><input type="checkbox" data-field="hidden"> 僅後台顯示（前台隱藏）</label>
                        <details class="coker-swiper-advanced-text" data-role="advanced-text" hidden>
                            <summary>進階文字編輯 <span data-role="advanced-text-count"></span></summary>
                            <div class="coker-swiper-advanced-text-fields" data-role="advanced-text-fields"></div>
                        </details>
                    </form>
                </div>
            </div>
            <div class="coker-swiper-footer">
                <button type="button" data-action="cancel">取消</button>
                <button type="button" class="coker-swiper-primary" data-action="save">完成編輯</button>
            </div>
            <div class="coker-swiper-bulk-dialog" data-role="bulk-dialog" hidden>
                <div class="coker-swiper-bulk-backdrop" data-action="close-bulk"></div>
                <section class="coker-swiper-bulk-panel" role="dialog" aria-modal="true" aria-labelledby="coker-swiper-bulk-title">
                    <header>
                        <h3 id="coker-swiper-bulk-title">批次設定</h3>
                        <button type="button" data-action="close-bulk" aria-label="關閉">×</button>
                    </header>
                    <p data-role="bulk-summary"></p>
                    <div class="coker-swiper-bulk-fields">
                        <label>指定媒體類型
                            <select data-role="bulk-media-filter">
                                <option value="all">全部已勾選項目</option>
                                <option value="image">已勾選的圖片</option>
                                <option value="video">已勾選的影片檔</option>
                                <option value="embed">已勾選的嵌入影片</option>
                            </select>
                        </label>
                        <label data-bulk-media="image">連結開啟方式
                            <select data-bulk="target">
                                <option value="">不變更</option>
                                <option value="_self">目前視窗</option>
                                <option value="_blank">另開視窗</option>
                            </select>
                        </label>
                        <label>停留秒數
                            <input type="number" min="0.1" step="0.1" data-bulk="duration" placeholder="不變更">
                        </label>
                        <label data-bulk-media="video,embed">開始秒數
                            <input type="number" min="0" step="1" data-bulk="startTime" placeholder="不變更">
                        </label>
                        <label data-bulk-media="video,embed">顯示比例
                            <select data-bulk="ratio">
                                <option value="">不變更</option>
                                <option value="16x9">16:9</option>
                                <option value="4x3">4:3</option>
                                <option value="1x1">1:1</option>
                                <option value="9x16">9:16</option>
                            </select>
                        </label>
                        <label>顯示狀態
                            <select data-bulk="hidden">
                                <option value="">不變更</option>
                                <option value="false">前台顯示</option>
                                <option value="true">前台隱藏</option>
                            </select>
                        </label>
                    </div>
                    <footer>
                        <button type="button" data-action="close-bulk">取消</button>
                        <button type="button" class="coker-swiper-primary" data-action="apply-bulk">套用至符合條件的項目</button>
                    </footer>
                </section>
            </div>`;

        this.bindEvents(root);
        return root;
    }

    bindEvents(root) {
        root.addEventListener('click', event => {
            const button = event.target.closest('[data-action]');
            if (!button) {
                return;
            }

            const actions = {
                add: () => this.addSlide(),
                'select-asset': button => this.selectAsset({
                    replaceId: this.activeId,
                    field: button.dataset.assetField,
                    assetType: button.dataset.assetType
                }),
                'select-all': () => this.selectAll(),
                'open-bulk': () => this.openBulkDialog(),
                'close-bulk': () => this.closeBulkDialog(),
                delete: () => this.deleteSelected(),
                'apply-bulk': () => this.applyBulk(),
                cancel: () => this.editor.Modal.close(),
                save: () => this.save()
            };

            actions[button.dataset.action]?.(button);
        });

        root.querySelector('[data-role="form"]').addEventListener('input', event => {
            if (event.target.matches('[data-text-path]')) {
                this.updateAdvancedText(event.target);
                return;
            }

            if (!event.target.matches('[data-field]')) {
                return;
            }

            this.updateActiveSlide(event.target);
        });

        root.querySelector('[data-role="bulk-media-filter"]').addEventListener('change', () => {
            this.renderBulkFields();
        });
    }

    render() {
        this.renderList();
        this.renderForm();
        const bulkButton = this.root.querySelector('[data-action="open-bulk"]');
        bulkButton.disabled = this.selectedIds.size === 0;
        bulkButton.textContent = `批次設定（已選 ${this.selectedIds.size} 筆）`;
    }

    renderList() {
        const list = this.root.querySelector('[data-role="list"]');
        list.replaceChildren();

        this.slides.forEach((slide, index) => {
            const row = document.createElement('div');
            row.className = `coker-swiper-item${slide.id === this.activeId ? ' is-active' : ''}`;
            row.dataset.id = slide.id;

            const dragHandle = document.createElement('span');
            dragHandle.className = 'coker-swiper-drag-handle';
            dragHandle.draggable = true;
            dragHandle.title = '拖曳調整順序';
            dragHandle.setAttribute('aria-label', '拖曳調整順序');
            dragHandle.textContent = '⠿';

            const check = document.createElement('input');
            check.type = 'checkbox';
            check.checked = this.selectedIds.has(slide.id);
            check.title = '勾選後可使用批次設定';
            check.setAttribute('aria-label', `批次選取第 ${index + 1} 筆`);
            check.addEventListener('change', () => {
                check.checked ? this.selectedIds.add(slide.id) : this.selectedIds.delete(slide.id);
                this.render();
            });

            const preview = this.createPreview(slide);
            const previewFrame = document.createElement('div');
            previewFrame.className = 'coker-swiper-preview-frame';
            previewFrame.append(preview);

            const description = document.createElement('button');
            description.type = 'button';
            description.className = 'coker-swiper-item-description';
            description.innerHTML = `<strong></strong><span></span>`;
            const displayTitle = slide.title || `輪播項目 ${index + 1}`;
            description.title = displayTitle;
            description.querySelector('strong').textContent = displayTitle;
            description.querySelector('strong').title = displayTitle;
            description.querySelector('span').textContent = mediaTypeLabel(slide.type);
            description.addEventListener('click', () => {
                this.activeId = slide.id;
                this.render();
            });

            const visibility = document.createElement('button');
            visibility.type = 'button';
            visibility.className = 'coker-swiper-visibility';
            visibility.title = slide.hidden ? '目前前台隱藏，點擊改為顯示' : '目前前台顯示，點擊改為隱藏';
            visibility.innerHTML = `<span class="material-symbols-outlined">${slide.hidden ? 'visibility_off' : 'visibility'}</span>`;
            visibility.addEventListener('click', () => {
                slide.hidden = !slide.hidden;
                this.render();
            });
            previewFrame.append(visibility);

            const duplicate = document.createElement('button');
            duplicate.type = 'button';
            duplicate.className = 'coker-swiper-duplicate';
            duplicate.title = '複製此輪播項目';
            duplicate.setAttribute('aria-label', `複製第 ${index + 1} 筆輪播項目`);
            duplicate.innerHTML = '<span class="material-symbols-outlined">content_copy</span>';
            duplicate.addEventListener('click', () => this.duplicateSlide(slide.id));
            previewFrame.append(duplicate);

            row.append(dragHandle, check, previewFrame, description);
            dragHandle.addEventListener('dragstart', event => {
                this.draggedId = slide.id;
                if (event.dataTransfer) {
                    event.dataTransfer.effectAllowed = 'move';
                    event.dataTransfer.setData('text/plain', slide.id);
                }
                row.classList.add('is-dragging');
            });
            dragHandle.addEventListener('dragend', () => {
                this.draggedId = null;
                row.classList.remove('is-dragging');
                this.clearDropIndicators();
            });
            row.addEventListener('dragover', event => {
                if (!this.draggedId || this.draggedId === slide.id) {
                    return;
                }

                event.preventDefault();
                if (event.dataTransfer) {
                    event.dataTransfer.dropEffect = 'move';
                }
                const bounds = row.getBoundingClientRect();
                const position = event.clientY < bounds.top + bounds.height / 2 ? 'before' : 'after';
                this.clearDropIndicators();
                row.classList.add(`is-drop-${position}`);
                row.dataset.dropPosition = position;
            });
            row.addEventListener('drop', event => {
                event.preventDefault();
                const position = row.dataset.dropPosition || 'before';
                this.moveSlide(this.draggedId, slide.id, position);
                this.clearDropIndicators();
            });
            list.append(row);
        });
    }

    createPreview(slide) {
        const previewSource = slide.type === swiperMediaTypes.image ? slide.src : slide.poster;
        if (previewSource) {
            const image = document.createElement('img');
            image.src = previewSource;
            image.alt = '';
            return image;
        }

        const preview = document.createElement('span');
        preview.className = 'coker-swiper-video-preview';
        preview.textContent = slide.type === swiperMediaTypes.embed ? '嵌入' : '影片';
        return preview;
    }

    renderForm() {
        const form = this.root.querySelector('[data-role="form"]');
        const empty = this.root.querySelector('[data-role="empty"]');
        const slide = this.getActiveSlide();

        form.hidden = !slide;
        empty.hidden = Boolean(slide);

        if (!slide) {
            return;
        }

        form.querySelectorAll('[data-field]').forEach(field => {
            field[field.type === 'checkbox' ? 'checked' : 'value'] = slide[field.dataset.field];
        });
        form.querySelectorAll('[data-media]').forEach(field => {
            field.hidden = !field.dataset.media.split(',').includes(slide.type);
        });
        form.querySelector('[data-role="caption-field"]').hidden = !this.supportsCaption;
        this.renderAdvancedTextFields(slide);
    }

    renderAdvancedTextFields(slide) {
        const section = this.root.querySelector('[data-role="advanced-text"]');
        const fieldsRoot = this.root.querySelector('[data-role="advanced-text-fields"]');
        const count = this.root.querySelector('[data-role="advanced-text-count"]');
        const textFields = slide.textFields || [];

        section.hidden = textFields.length === 0;
        count.textContent = `（${textFields.length} 項）`;
        fieldsRoot.replaceChildren();

        textFields.forEach(field => {
            const label = document.createElement('label');
            const caption = document.createElement('span');
            const input = document.createElement(field.multiline ? 'textarea' : 'input');

            caption.textContent = `${field.label} — ${field.value || '（空白）'}`;
            caption.title = field.value;
            if (field.multiline) {
                input.rows = 2;
            } else {
                input.type = 'text';
            }
            input.dataset.textPath = field.path;
            input.value = field.value;
            label.append(caption, input);
            fieldsRoot.append(label);
        });
    }

    updateAdvancedText(input) {
        const slide = this.getActiveSlide();
        const field = slide?.textFields?.find(item => item.path === input.dataset.textPath);
        if (field) {
            field.value = input.value;
            if (!this.dirtyTextPaths.has(slide.id)) {
                this.dirtyTextPaths.set(slide.id, new Set());
            }
            this.dirtyTextPaths.get(slide.id).add(field.path);
        }
    }

    updateActiveSlide(field) {
        const slide = this.getActiveSlide();
        if (!slide) {
            return;
        }

        const key = field.dataset.field;
        let value = field.type === 'checkbox' ? field.checked : field.value;
        if (key === 'startTime' || key === 'duration') {
            value = Number(value);
        }

        const changes = { [key]: value };
        if (key === 'type' && slide.type === swiperMediaTypes.image && value !== swiperMediaTypes.image) {
            changes.poster = slide.src || slide.poster;
            changes.src = '';
        } else if (key === 'type' && slide.type !== swiperMediaTypes.image && value === swiperMediaTypes.image) {
            changes.src = slide.poster || slide.src;
        }

        if (key === 'src' && slide.type === swiperMediaTypes.image && value) {
            changes.title = fileNameWithoutExtension(value);
        }

        Object.assign(slide, normalizeSlide({ ...slide, ...changes }));
        this.renderList();

        if (key === 'type') {
            this.renderForm();
        } else if (changes.title !== undefined) {
            this.root.querySelector('[data-field="title"]').value = slide.title;
        }
    }

    selectAsset(options = {}) {
        const assetManager = this.editor.AssetManager;
        const field = options.field || 'src';
        const assetType = options.assetType === swiperMediaTypes.video
            ? swiperMediaTypes.video
            : swiperMediaTypes.image;
        const collectedAssets = new Map();
        let restored = false;
        const collectAsset = asset => {
            const normalized = normalizeAsset(asset);
            if (normalized.src) {
                collectedAssets.set(normalized.src, normalized);
            }
        };
        const applyCollectedAssets = () => {
            const assets = Array.from(collectedAssets.values());
            const replaceSlide = options.replaceId
                ? this.slides.find(slide => slide.id === options.replaceId)
                : null;

            if (replaceSlide && assets.length) {
                const replacement = assets.shift();
                replaceSlide[field] = replacement.src;
                if (field === 'src') {
                    replaceSlide.type = assetType;
                    replaceSlide.title = fileNameWithoutExtension(replacement.name || replacement.src);
                }
            }

            const additions = field === 'src'
                ? assets.map(asset => normalizeSlide({
                    id: createSlideId(),
                    type: assetType,
                    src: asset.src,
                    title: fileNameWithoutExtension(asset.name || asset.src),
                    hasCaption: this.supportsCaption,
                    textFields: cloneTextFields(this.slideTemplateTextFields),
                    thumbnailTemplateHtml: this.slideTemplateThumbnailHtml,
                    templateHtml: this.slideTemplateHtml
                }))
                : [];

            this.slides.push(...additions);
            this.activeId = replaceSlide?.id || additions.at(-1)?.id || this.activeId;
        };
        const restoreEditor = () => {
            if (restored) {
                return;
            }

            restored = true;
            this.editor.off('asset:add', collectAsset);
            applyCollectedAssets();
            globalThis.setTimeout(() => this.open(), 0);
        };

        this.editor.on('asset:add', collectAsset);
        this.editor.once('asset:close', restoreEditor);
        assetManager.open({
            types: [assetType],
            accept: assetType === swiperMediaTypes.video ? 'video/*' : 'image/*',
            select: asset => {
                collectAsset(asset);
                assetManager.close();
            }
        });
    }

    addSlide() {
        const slide = normalizeSlide({
            type: swiperMediaTypes.image,
            hasCaption: this.supportsCaption,
            textFields: cloneTextFields(this.slideTemplateTextFields),
            thumbnailTemplateHtml: this.slideTemplateThumbnailHtml,
            templateHtml: this.slideTemplateHtml
        });
        this.slides.push(slide);
        this.activeId = slide.id;
        this.render();
    }

    duplicateSlide(slideId) {
        const index = this.slides.findIndex(slide => slide.id === slideId);
        if (index < 0) {
            return;
        }

        const source = this.slides[index];
        const duplicate = normalizeSlide({
            ...source,
            id: createSlideId(),
            textFields: cloneTextFields(source.textFields)
        });
        this.duplicateSourceIds.set(duplicate.id, source.id);
        this.slides.splice(index + 1, 0, duplicate);
        this.activeId = duplicate.id;
        this.render();
    }

    openBulkDialog() {
        if (!this.selectedIds.size) {
            this.notify('alert', '請先勾選要批次設定的輪播項目。');
            return;
        }

        this.root.querySelectorAll('[data-bulk]').forEach(field => { field.value = ''; });
        this.root.querySelector('[data-role="bulk-media-filter"]').value = 'all';
        this.root.querySelector('[data-role="bulk-summary"]').textContent = `目前已勾選 ${this.selectedIds.size} 筆輪播項目。`;
        this.root.querySelector('[data-role="bulk-dialog"]').hidden = false;
        this.renderBulkFields();
    }

    closeBulkDialog() {
        this.root.querySelector('[data-role="bulk-dialog"]').hidden = true;
    }

    renderBulkFields() {
        const mediaFilter = this.root.querySelector('[data-role="bulk-media-filter"]').value;
        this.root.querySelectorAll('[data-bulk-media]').forEach(field => {
            field.hidden = mediaFilter !== 'all'
                && !field.dataset.bulkMedia.split(',').includes(mediaFilter);
        });
    }

    selectAll() {
        const allSelected = this.slides.length > 0 && this.selectedIds.size === this.slides.length;
        this.selectedIds = new Set(allSelected ? [] : this.slides.map(slide => slide.id));
        this.render();
    }

    async deleteSelected() {
        if (!this.selectedIds.size) {
            return;
        }

        const confirmed = await this.confirm(`確定刪除選取的 ${this.selectedIds.size} 筆輪播項目嗎？`);
        if (!confirmed) {
            return;
        }

        this.slides = this.slides.filter(slide => !this.selectedIds.has(slide.id));
        this.selectedIds.clear();
        if (!this.slides.some(slide => slide.id === this.activeId)) {
            this.activeId = this.slides[0]?.id || null;
        }
        this.render();
    }

    applyBulk() {
        const values = {};
        const mediaFilter = this.root.querySelector('[data-role="bulk-media-filter"]').value;
        this.root.querySelectorAll('[data-bulk]').forEach(field => {
            if (field.closest('[data-bulk-media]')?.hidden) {
                return;
            }

            if (field.value !== '') {
                values[field.dataset.bulk] = field.dataset.bulk === 'hidden'
                    ? field.value === 'true'
                    : field.dataset.bulk === 'duration' || field.dataset.bulk === 'startTime'
                        ? Number(field.value)
                        : field.value;
            }
        });

        if (!Object.keys(values).length) {
            this.notify('alert', '請先選擇要批次變更的設定。');
            return;
        }

        const targetSlides = this.slides.filter(slide => (
            this.selectedIds.has(slide.id) && (mediaFilter === 'all' || slide.type === mediaFilter)
        ));
        if (!targetSlides.length) {
            this.notify('alert', '已勾選項目中沒有符合指定媒體類型的資料。');
            return;
        }

        this.slides = this.slides.map(slide => {
            if (!targetSlides.includes(slide)) {
                return slide;
            }

            const applicableValues = Object.fromEntries(
                Object.entries(values).filter(([key]) => isBulkSettingApplicable(key, slide.type))
            );
            return normalizeSlide({ ...slide, ...applicableValues });
        });
        this.closeBulkDialog();
        this.render();
    }

    moveSlide(sourceId, targetId, position = 'before') {
        if (!sourceId || sourceId === targetId) {
            return;
        }

        const sourceIndex = this.slides.findIndex(slide => slide.id === sourceId);
        let targetIndex = this.slides.findIndex(slide => slide.id === targetId);
        if (sourceIndex < 0 || targetIndex < 0) {
            return;
        }

        const [slide] = this.slides.splice(sourceIndex, 1);
        targetIndex = this.slides.findIndex(item => item.id === targetId);
        if (position === 'after') {
            targetIndex += 1;
        }
        this.slides.splice(targetIndex, 0, slide);
        this.renderList();
    }

    clearDropIndicators() {
        this.root.querySelectorAll('.is-drop-before, .is-drop-after').forEach(row => {
            row.classList.remove('is-drop-before', 'is-drop-after');
            delete row.dataset.dropPosition;
        });
    }

    save() {
        if (serializeSlideState(this.slides) === this.initialState) {
            this.editor.Modal.close();
            return;
        }

        if (!this.slides.length) {
            this.notify('error', '輪播至少需要保留一筆項目。');
            return;
        }

        const invalidIndex = this.slides.findIndex(slide => !slide.src.trim());
        if (invalidIndex >= 0) {
            this.activeId = this.slides[invalidIndex].id;
            this.render();
            this.notify('error', `第 ${invalidIndex + 1} 筆輪播尚未設定圖片或影片網址。`);
            return;
        }

        const wrapper = findSwiperWrapperComponent(this.component);
        if (!wrapper) {
            this.notify('error', '找不到輪播的 swiper-wrapper，無法儲存。');
            return;
        }

        const renderedSlides = renderSlides(this.slides);
        const thumbnailWrapper = findSwiperThumbnailWrapperComponent(this.component);

        if (this.isVerticalSwiper) {
            const frameWindow = this.destroySwiperInstances();
            if (!replaceVerticalSwiperCollections({
                wrapper,
                thumbnailWrapper,
                slides: this.slides,
                mainSlideComponents: this.mainSlideComponents,
                thumbnailSlideComponents: this.thumbnailSlideComponents,
                duplicateSourceIds: this.duplicateSourceIds,
                dirtyTextPaths: this.dirtyTextPaths
            })) {
                this.notify('error', '輪播內容更新失敗，已保留原始內容。');
                this.refreshSwiper(frameWindow, wrapper);
                return;
            }

            this.editor.Modal.close();
            this.refreshSwiper(frameWindow, wrapper);
            return;
        }

        const renderedThumbnails = thumbnailWrapper ? renderThumbnailSlides(this.slides) : '';
        if (!hasExpectedSlideCount(renderedSlides, this.slides.length)) {
            this.notify('error', '輪播內容產生失敗，已保留原始內容。');
            return;
        }
        if (thumbnailWrapper && !hasExpectedSlideCount(renderedThumbnails, this.slides.length)) {
            this.notify('error', '輪播縮圖產生失敗，已保留原始內容。');
            return;
        }

        const frameWindow = this.destroySwiperInstances();
        if (!replaceSlideCollection(wrapper, renderedSlides, this.slides.length)) {
            this.notify('error', '輪播內容更新失敗，請重新整理畫布後再嘗試。');
            this.refreshSwiper(frameWindow, wrapper);
            return;
        }
        if (thumbnailWrapper && !replaceSlideCollection(thumbnailWrapper, renderedThumbnails, this.slides.length)) {
            this.notify('error', '輪播縮圖更新失敗，請重新整理畫布後再嘗試。');
            this.refreshSwiper(frameWindow, wrapper);
            return;
        }

        this.editor.Modal.close();
        this.refreshSwiper(frameWindow, wrapper);
    }

    destroySwiperInstances() {
        const frameWindow = this.editor.Canvas?.getWindow?.();
        const root = this.component.getEl?.();
        if (!frameWindow || !root) {
            return null;
        }

        if (frameWindow.jQuery) {
            const $root = frameWindow.jQuery(root);
            $root.off('mouseover mouseout');
            $root.find('.swiper-slide a, .swiper-slide').off('focus');
            $root.find('a').off('blob');
            $root.data('isInit', false);
        }

        const swiperElements = [
            ...root.querySelectorAll('.swiper'),
            ...root.querySelectorAll('.swiper_thumbs:not(.swiper)')
        ];
        swiperElements.forEach(element => {
            element.swiper?.destroy?.(true, true);
        });

        return frameWindow;
    }

    refreshSwiper(frameWindow, wrapper) {
        if (!frameWindow) {
            return;
        }

        frameWindow.requestAnimationFrame(() => {
            const wrapperElement = wrapper?.getEl?.();
            const slideCount = wrapperElement
                ? Array.from(wrapperElement.children).filter(element => element.classList.contains('swiper-slide')).length
                : 0;
            if (!slideCount) {
                this.notify('error', '輪播更新後未偵測到 slide，已停止重新初始化。');
                return;
            }

            try {
                frameWindow.SwiperInit?.({ autoplay: false });
            } catch (error) {
                console.error('[Coker Swiper] 初始化失敗', error);
                this.notify('error', '輪播內容已更新，但重新初始化失敗，請重新整理畫布後再確認。');
            }
        });
    }

    getActiveSlide() {
        return this.slides.find(slide => slide.id === this.activeId) || null;
    }

    notify(type, message) {
        const handler = this.editor.AlertManager?.[type];
        if (typeof handler === 'function') {
            handler.call(this.editor.AlertManager, message);
            return;
        }

        globalThis.alert?.(message);
    }

    confirm(message) {
        const handler = this.editor.AlertManager?.confirm;
        return Promise.resolve(typeof handler === 'function'
            ? handler.call(this.editor.AlertManager, message, { title: '刪除輪播項目' })
            : globalThis.confirm?.(message));
    }
}

function normalizeAsset(asset) {
    return {
        src: asset?.get?.('src') || asset?.src || asset?.id || asset?.attributes?.src || '',
        name: asset?.get?.('name') || asset?.name || asset?.attributes?.name || ''
    };
}

function fileNameWithoutExtension(value) {
    const path = String(value || '').split(/[?#]/)[0].replace(/^.*[\\/]/, '');
    let fileName = path;
    try {
        fileName = decodeURIComponent(path);
    } catch (_) {
        // Keep the original file name when it is not URI encoded.
    }
    return fileName.replace(/\.[^.]+$/, '');
}

function mediaTypeLabel(type) {
    return ({
        [swiperMediaTypes.image]: '圖片',
        [swiperMediaTypes.video]: '影片檔',
        [swiperMediaTypes.embed]: '嵌入影片'
    })[type] || '媒體';
}

function isBulkSettingApplicable(key, mediaType) {
    if (key === 'target') {
        return mediaType === swiperMediaTypes.image;
    }

    if (key === 'startTime' || key === 'ratio') {
        return mediaType === swiperMediaTypes.video || mediaType === swiperMediaTypes.embed;
    }

    return true;
}

function hasExpectedSlideCount(html, expectedCount) {
    const parser = new DOMParser();
    const document = parser.parseFromString(`<div data-render-root>${html}</div>`, 'text/html');
    const root = document.querySelector('[data-render-root]');
    return root && Array.from(root.children).filter(element => (
        element.classList.contains('swiper-slide')
    )).length === expectedCount;
}

function mapMainSlideComponents(component, slides) {
    const wrapper = findSwiperWrapperComponent(component);
    const components = getDirectSlideComponents(wrapper);
    return new Map(slides.map((slide, index) => [slide.id, components[index]]).filter(([, model]) => model));
}

function mapThumbnailSlideComponents(component, slides) {
    const wrapper = findSwiperThumbnailWrapperComponent(component);
    const components = getDirectSlideComponents(wrapper);
    const available = [...components];
    const result = new Map();

    slides.forEach(slide => {
        const index = available.findIndex(model => getComponentMediaSource(model) === slide.src);
        const model = index >= 0 ? available.splice(index, 1)[0] : available.shift();
        if (model) {
            result.set(slide.id, model);
        }
    });
    return result;
}

function replaceVerticalSwiperCollections(options) {
    const {
        wrapper,
        thumbnailWrapper,
        slides,
        mainSlideComponents,
        thumbnailSlideComponents,
        duplicateSourceIds,
        dirtyTextPaths
    } = options;
    const originalMain = getDirectSlideComponents(wrapper);
    const originalThumbnails = getDirectSlideComponents(thumbnailWrapper);

    try {
        const mainComponents = slides.map(slide => {
            const source = resolveSourceComponent(slide.id, mainSlideComponents, duplicateSourceIds) || originalMain[0];
            const clone = source?.clone?.();
            if (!clone) {
                throw new Error(`Unable to clone main slide ${slide.id}`);
            }
            updateVerticalMainSlide(clone, slide, dirtyTextPaths.get(slide.id) || new Set());
            return clone;
        });
        const thumbnailComponents = thumbnailWrapper
            ? slides.map(slide => {
                const source = resolveSourceComponent(slide.id, thumbnailSlideComponents, duplicateSourceIds) || originalThumbnails[0];
                const clone = source?.clone?.();
                if (!clone) {
                    throw new Error(`Unable to clone thumbnail slide ${slide.id}`);
                }
                updateVerticalThumbnailSlide(clone, slide);
                return clone;
            })
            : [];

        if (mainComponents.length !== slides.length || mainComponents.some(model => !hasSlideMedia(model))) {
            throw new Error('Generated vertical slides failed validation.');
        }
        if (thumbnailWrapper && (
            thumbnailComponents.length !== slides.length ||
            thumbnailComponents.some(model => !hasSlideMedia(model))
        )) {
            throw new Error('Generated vertical thumbnails failed validation.');
        }

        wrapper.components().reset(mainComponents);
        thumbnailWrapper?.components?.().reset(thumbnailComponents);
        return getDirectSlideComponents(wrapper).length === slides.length &&
            (!thumbnailWrapper || getDirectSlideComponents(thumbnailWrapper).length === slides.length);
    } catch (error) {
        console.error('[Coker Swiper] vertical swiper update failed', error);
        wrapper?.components?.().reset(originalMain);
        thumbnailWrapper?.components?.().reset(originalThumbnails);
        return false;
    }
}

function resolveSourceComponent(slideId, componentMap, duplicateSourceIds) {
    let sourceId = slideId;
    const visited = new Set();
    while (!componentMap.has(sourceId) && duplicateSourceIds.has(sourceId) && !visited.has(sourceId)) {
        visited.add(sourceId);
        sourceId = duplicateSourceIds.get(sourceId);
    }
    return componentMap.get(sourceId) || null;
}

function updateVerticalMainSlide(component, slide, dirtyTextPaths) {
    if (slide.type !== swiperMediaTypes.image) {
        throw new Error('vertical_swiper_thumbs currently requires image slides.');
    }

    setComponentStateClass(component, 'backstageType', slide.hidden);
    component.addAttributes?.({
        'data-coker-slide-id': slide.id,
        'data-coker-media-type': slide.type,
        'data-swiper-autoplay': String(Math.round(slide.duration * 1000))
    });

    const image = findFirstComponent(component, model => getComponentTagName(model) === 'img');
    if (!image) {
        throw new Error(`Main image not found for slide ${slide.id}`);
    }
    image.addAttributes?.({
        src: slide.src,
        alt: slide.title,
        'data-keep_time': String(slide.duration)
    });

    const title = findFirstComponent(component, model => model.getClasses?.().includes('title'));
    if (!title) {
        throw new Error(`Title component not found for slide ${slide.id}`);
    }
    setTextComponent(title, slide.title, false);

    const description = findFirstComponent(component, model => model.getClasses?.().includes('description'));
    const descriptionText = findFirstComponent(description, model => model.getClasses?.().includes('text'));
    const buttonGroup = findFirstComponent(component, model => model.getClasses?.().includes('button'));
    const textTargets = [
        ...(descriptionText ? [descriptionText] : []),
        ...findComponents(buttonGroup, model => getComponentTagName(model) === 'span')
    ];
    const changedTextFields = slide.textFields.filter(field => dirtyTextPaths.has(field.path));
    if (changedTextFields.length && textTargets.length < slide.textFields.length) {
        throw new Error(`Only ${textTargets.length} of ${slide.textFields.length} text fields were found for slide ${slide.id}`);
    }
    slide.textFields.forEach((field, index) => {
        if (!dirtyTextPaths.has(field.path)) {
            return;
        }
        const target = textTargets[index];
        setTextComponent(target, field.value, field.preserveLineBreaks);
        if (field.value && !getChildComponents(target).length) {
            throw new Error(`Text field ${index + 1} became empty for slide ${slide.id}`);
        }
    });
}

function updateVerticalThumbnailSlide(component, slide) {
    setComponentStateClass(component, 'backstageType', slide.hidden);
    const image = findFirstComponent(component, model => (
        getComponentTagName(model) === 'img' && model.getClasses?.().includes('original')
    )) || findFirstComponent(component, model => getComponentTagName(model) === 'img');
    if (!image) {
        throw new Error(`Thumbnail image not found for slide ${slide.id}`);
    }
    image.addAttributes?.({ src: slide.poster || slide.src, alt: slide.title });
}

function setComponentStateClass(component, className, enabled) {
    const classes = new Set(component.getClasses?.() || []);
    enabled ? classes.add(className) : classes.delete(className);
    component.setClass?.(Array.from(classes));
}

function setTextComponent(component, value, preserveLineBreaks) {
    const children = component?.components?.();
    if (!children) {
        throw new Error('Text component collection is unavailable.');
    }

    const lines = preserveLineBreaks
        ? String(value || '').split(/\r?\n/)
        : [String(value || '').replace(/\r?\n/g, ' ')];

    children.reset([]);
    lines.forEach((line, index) => {
        if (index) {
            children.add({
                type: 'default',
                tagName: 'br',
                void: true
            });
        }
        if (line) {
            children.add({
                type: 'textnode',
                content: line
            });
        }
    });
}

function getDirectSlideComponents(wrapper) {
    const collection = wrapper?.components?.();
    return collection
        ? Array.from(collection.models || collection).filter(isSlideComponent)
        : [];
}

function getComponentMediaSource(component) {
    const image = findFirstComponent(component, model => (
        getComponentTagName(model) === 'img' && model.getClasses?.().includes('original')
    )) || findFirstComponent(component, model => getComponentTagName(model) === 'img');
    return image?.getAttributes?.().src || '';
}

function findFirstComponent(component, predicate) {
    return findComponents(component, predicate, true)[0] || null;
}

function findComponents(component, predicate, stopAfterFirst = false) {
    if (!component) {
        return [];
    }

    const matches = [];
    const stack = [...getChildComponents(component)].reverse();
    while (stack.length) {
        const current = stack.pop();
        if (predicate(current)) {
            matches.push(current);
            if (stopAfterFirst) {
                break;
            }
        }
        stack.push(...getChildComponents(current).reverse());
    }
    return matches;
}

function getChildComponents(component) {
    const collection = component?.components?.();
    return collection ? Array.from(collection.models || collection) : [];
}

function getComponentTagName(component) {
    return String(component?.get?.('tagName') || '').toLowerCase();
}

function replaceSlideCollection(wrapper, html, expectedCount) {
    const collection = wrapper.components?.();
    if (!collection) {
        return false;
    }

    const renderedElements = parseRenderedSlideElements(html);
    if (renderedElements.length !== expectedCount) {
        return false;
    }

    const originalHtml = Array.from(collection.models || collection)
        .map(component => component.toHTML())
        .join('');

    try {
        // Parse the complete sibling set in one pass. Replacing each slide's
        // children separately can make GrapesJS move matching child models
        // between siblings, leaving all but the last slide empty.
        wrapper.components(html);
    } catch (error) {
        console.error('[Coker Swiper] slide collection update failed', error);
        restoreWrapperComponents(wrapper, originalHtml);
        return false;
    }

    const currentComponents = Array.from(wrapper.components().models || wrapper.components());
    const currentSlides = currentComponents.filter(isSlideComponent);
    const currentSlideCount = currentSlides.length;
    const slidesWithMedia = currentSlides.filter(hasSlideMedia).length;
    const succeeded = currentSlideCount === expectedCount && slidesWithMedia === expectedCount;

    if (!succeeded) {
        console.error('[Coker Swiper] slide count mismatch', {
            expectedCount,
            currentCount: currentSlideCount,
            slidesWithMedia
        });
        restoreWrapperComponents(wrapper, originalHtml);
    }

    return succeeded;
}

function restoreWrapperComponents(wrapper, html) {
    try {
        wrapper.components(html);
    } catch (error) {
        console.error('[Coker Swiper] restoring original slide collection failed', error);
    }
}

function parseRenderedSlideElements(html) {
    const parser = new DOMParser();
    const document = parser.parseFromString(`<div data-render-root>${html}</div>`, 'text/html');
    const root = document.querySelector('[data-render-root]');
    return root
        ? Array.from(root.children).filter(element => element.classList.contains('swiper-slide'))
        : [];
}

function isSlideComponent(component) {
    return component.getClasses?.().includes('swiper-slide');
}

function hasSlideMedia(component) {
    return Boolean(findFirstComponent(component, model => (
        ['img', 'video', 'iframe'].includes(getComponentTagName(model))
    )));
}

function serializeSlideState(slides) {
    return JSON.stringify(slides.map(slide => ({
        id: slide.id,
        type: slide.type,
        src: slide.src,
        poster: slide.poster,
        title: slide.title,
        caption: slide.caption,
        link: slide.link,
        target: slide.target,
        startTime: slide.startTime,
        duration: slide.duration,
        ratio: slide.ratio,
        hidden: slide.hidden,
        textFields: slide.textFields
    })));
}

function cloneTextFields(fields) {
    return Array.isArray(fields) ? fields.map(field => ({ ...field })) : [];
}
