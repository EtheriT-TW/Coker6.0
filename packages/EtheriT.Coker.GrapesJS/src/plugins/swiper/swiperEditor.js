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
        this.slideTemplateImageFields = cloneFields(templateSlide?.imageFields);
        this.slideTemplateThumbnailHtml = templateSlide?.thumbnailTemplateHtml || '';
        this.isVerticalSwiper = component.getClasses?.().includes('vertical_swiper_thumbs');
        this.duplicateSourceIds = new Map();
        this.dirtyTextPaths = new Map();
        this.dirtyImagePaths = new Map();
        this.dirtyVisibilityPaths = new Map();
        this.dirtyGroupPaths = new Map();
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
                    <label class="coker-swiper-add-control" title="新增輪播項目">
                        <span class="material-symbols-outlined" aria-hidden="true">add</span>
                        <select class="coker-swiper-add-method" data-role="add-method" aria-label="新增輪播項目">
                            <option value="">新增</option>
                            <option value="upload">上傳媒體</option>
                            <option value="embed">嵌入影片</option>
                        </select>
                    </label>
                    <button type="button" data-action="select-all" title="全選或取消全選">
                        <span class="material-symbols-outlined" aria-hidden="true">select_all</span><span>全選</span>
                    </button>
                    <button type="button" data-action="open-bulk" title="批次設定" disabled>
                        <span class="material-symbols-outlined" aria-hidden="true">tune</span><span>批次</span><span data-role="bulk-count">0</span>
                    </button>
                    <button type="button" data-action="delete" title="刪除已勾選項目">
                        <span class="material-symbols-outlined" aria-hidden="true">delete</span><span>刪除</span>
                    </button>
                </div>
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
                        <button type="button" class="coker-swiper-advanced-trigger" data-action="open-advanced" data-role="advanced-trigger" hidden>
                            <span class="material-symbols-outlined">tune</span>
                            <span>進階內容編輯</span>
                            <span data-role="advanced-group-count"></span>
                            <span class="material-symbols-outlined">open_in_new</span>
                        </button>
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
                    <div class="coker-swiper-bulk-header">
                        <h3 id="coker-swiper-bulk-title">批次設定</h3>
                        <button type="button" data-action="close-bulk" aria-label="關閉">×</button>
                    </div>
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
                    <div class="coker-swiper-bulk-actions">
                        <button type="button" data-action="close-bulk">取消</button>
                        <button type="button" class="coker-swiper-primary" data-action="apply-bulk">套用至符合條件的項目</button>
                    </div>
                </section>
            </div>
            <div class="coker-swiper-image-preview-dialog" data-role="image-preview-dialog" hidden>
                <div class="coker-swiper-bulk-backdrop" data-action="close-image-preview"></div>
                <section class="coker-swiper-image-preview-panel" role="dialog" aria-modal="true" aria-label="圖片預覽">
                    <button type="button" data-action="close-image-preview" aria-label="關閉圖片預覽">×</button>
                    <img data-role="image-preview" alt="">
                </section>
            </div>
            <div class="coker-swiper-advanced-dialog" data-role="advanced-dialog" hidden>
                <div class="coker-swiper-bulk-backdrop" data-action="close-advanced"></div>
                <section class="coker-swiper-advanced-panel" role="dialog" aria-modal="true" aria-labelledby="coker-swiper-advanced-title">
                    <div class="coker-swiper-advanced-panel-header">
                        <div>
                            <h3 id="coker-swiper-advanced-title">進階內容編輯</h3>
                            <p data-role="advanced-summary"></p>
                        </div>
                        <button type="button" data-action="close-advanced" aria-label="關閉進階內容編輯">×</button>
                    </div>
                    <div class="coker-swiper-advanced-groups" data-role="advanced-group-fields"></div>
                    <div class="coker-swiper-advanced-panel-actions">
                        <button type="button" class="coker-swiper-primary" data-action="close-advanced">完成進階編輯</button>
                    </div>
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
                'select-asset': button => this.selectAsset({
                    replaceId: this.activeId,
                    field: button.dataset.assetField,
                    assetType: button.dataset.assetType,
                    imageKey: button.dataset.imageKey
                }),
                'preview-advanced-image': button => this.openAdvancedImagePreview(button.dataset.imageKey),
                'close-image-preview': () => this.closeAdvancedImagePreview(),
                'open-advanced': () => this.openAdvancedDialog(),
                'close-advanced': () => this.closeAdvancedDialog(),
                'toggle-advanced-visibility': button => this.toggleAdvancedVisibility(
                    button.dataset.advancedType,
                    button.dataset.advancedKey
                ),
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

        root.addEventListener('input', event => {
            if (event.target.matches('[data-text-path]')) {
                this.updateAdvancedText(event.target);
                return;
            }

            if (event.target.matches('[data-group-key]')) {
                this.updateAdvancedGroup(event.target);
                return;
            }

            if (event.target.matches('[data-image-key]')) {
                this.updateAdvancedImage(event.target);
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

        root.querySelector('[data-role="add-method"]').addEventListener('change', event => {
            const method = event.target.value;
            event.target.value = '';
            if (method === 'upload') {
                this.uploadSlides();
            } else if (method === 'embed') {
                this.addEmbedSlide();
            }
        });
    }

    render() {
        this.renderList();
        this.renderForm();
        const bulkButton = this.root.querySelector('[data-action="open-bulk"]');
        bulkButton.disabled = this.selectedIds.size === 0;
        bulkButton.querySelector('[data-role="bulk-count"]').textContent = this.selectedIds.size;
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
        this.renderAdvancedGroups(slide);
    }

    renderAdvancedGroups(slide) {
        const trigger = this.root.querySelector('[data-role="advanced-trigger"]');
        const fieldsRoot = this.root.querySelector('[data-role="advanced-group-fields"]');
        const count = this.root.querySelector('[data-role="advanced-group-count"]');
        const groups = collectAdvancedGroups(slide);

        trigger.hidden = groups.length === 0;
        count.textContent = `（${groups.length} 組）`;
        fieldsRoot.replaceChildren();

        groups.forEach(group => {
            const card = document.createElement('section');
            const header = document.createElement('div');
            const heading = document.createElement('strong');
            const firstItem = group.items[0];
            const visibility = createAdvancedVisibilityButton(firstItem.type, firstItem.field);

            card.className = 'coker-swiper-advanced-group';
            header.className = 'coker-swiper-advanced-group-header';
            heading.innerHTML = `<span class="material-symbols-outlined">${group.type === 'link' ? 'link' : 'view_in_ar'}</span><span></span>`;
            heading.querySelector('span:last-child').textContent = group.label;
            header.append(heading, visibility);
            card.append(header);

            if (group.type === 'link') {
                const linkFields = document.createElement('div');
                const hrefLabel = document.createElement('label');
                const linkControl = document.createElement('div');
                const href = document.createElement('input');
                const target = document.createElement('select');
                linkFields.className = 'coker-swiper-advanced-link-fields';
                hrefLabel.textContent = '連結';
                linkControl.className = 'coker-swiper-advanced-link-control';
                href.type = 'text';
                href.dataset.groupKey = group.key;
                href.dataset.groupField = 'groupHref';
                href.value = group.href;
                href.setAttribute('aria-label', '連結網址');
                target.dataset.groupKey = group.key;
                target.dataset.groupField = 'groupTarget';
                target.setAttribute('aria-label', '連結開啟方式');
                target.innerHTML = '<option value="_self">目前視窗</option><option value="_blank">另開視窗</option>';
                target.value = group.target;
                linkControl.append(href, target);
                hrefLabel.append(linkControl);
                linkFields.append(hrefLabel);
                card.append(linkFields);
            }

            const content = document.createElement('div');
            content.className = 'coker-swiper-advanced-group-content';
            group.items.forEach(item => {
                content.append(item.type === 'image'
                    ? createAdvancedImageField(item.field)
                    : createAdvancedTextField(item.field));
            });
            card.append(content);
            fieldsRoot.append(card);
        });
    }

    openAdvancedDialog() {
        const slide = this.getActiveSlide();
        if (!slide) {
            return;
        }
        this.root.querySelector('[data-role="advanced-summary"]').textContent =
            `目前項目：${slide.title || '未命名輪播項目'}`;
        this.root.querySelector('[data-role="advanced-dialog"]').hidden = false;
    }

    closeAdvancedDialog() {
        this.closeAdvancedImagePreview();
        this.root.querySelector('[data-role="advanced-dialog"]').hidden = true;
    }

    updateAdvancedGroup(input) {
        const slide = this.getActiveSlide();
        const key = input.dataset.groupKey;
        const property = input.dataset.groupField;
        if (!slide || !key || !property) {
            return;
        }

        let changed = false;
        [...slide.textFields, ...slide.imageFields].forEach(field => {
            if (getAdvancedGroupKey(field) === key) {
                field[property] = input.value;
                changed = true;
            }
        });
        if (changed) {
            markDirtyPath(this.dirtyGroupPaths, slide.id, key);
        }
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

    updateAdvancedImage(input) {
        const slide = this.getActiveSlide();
        const field = slide?.imageFields?.find(item => getAdvancedFieldKey(item) === input.dataset.imageKey);
        if (!field) {
            return;
        }

        field.src = input.value;
        markDirtyPath(this.dirtyImagePaths, slide.id, getAdvancedFieldKey(field));
    }

    toggleAdvancedVisibility(type, key) {
        const slide = this.getActiveSlide();
        const fields = type === 'text' ? slide?.textFields : slide?.imageFields;
        const field = fields?.find(item => getAdvancedFieldKey(item) === key);
        if (!slide || !field) {
            return;
        }

        const visibilityKey = getAdvancedVisibilityKey(field);
        const hidden = !field.hidden;
        [...slide.textFields, ...slide.imageFields].forEach(item => {
            if (getAdvancedVisibilityKey(item) === visibilityKey) {
                item.hidden = hidden;
            }
        });
        markDirtyPath(this.dirtyVisibilityPaths, slide.id, visibilityKey);
        this.renderAdvancedGroups(slide);
    }

    openAdvancedImagePreview(imageKey) {
        const slide = this.getActiveSlide();
        const field = slide?.imageFields?.find(item => getAdvancedFieldKey(item) === imageKey);
        if (!field) {
            return;
        }

        const dialog = this.root.querySelector('[data-role="image-preview-dialog"]');
        const image = dialog.querySelector('[data-role="image-preview"]');
        image.src = field.src;
        image.alt = field.alt || field.label;
        dialog.hidden = false;
    }

    closeAdvancedImagePreview() {
        const dialog = this.root.querySelector('[data-role="image-preview-dialog"]');
        dialog.hidden = true;
        dialog.querySelector('[data-role="image-preview"]').removeAttribute('src');
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
                const advancedImage = options.imageKey
                    ? replaceSlide.imageFields?.find(item => getAdvancedFieldKey(item) === options.imageKey)
                    : null;
                if (advancedImage) {
                    advancedImage.src = replacement.src;
                    markDirtyPath(this.dirtyImagePaths, replaceSlide.id, getAdvancedFieldKey(advancedImage));
                } else if (!options.imageKey) {
                    replaceSlide[field] = replacement.src;
                }
                if (!options.imageKey && field === 'src') {
                    replaceSlide.type = assetType;
                    replaceSlide.title = fileNameWithoutExtension(replacement.name || replacement.src);
                }
            }

            const additions = !options.imageKey && field === 'src'
                ? assets.map(asset => {
                    const id = createSlideId();
                    return normalizeSlide({
                        id,
                        type: assetType,
                        src: asset.src,
                        title: fileNameWithoutExtension(asset.name || asset.src),
                        hasCaption: this.supportsCaption,
                        textFields: cloneTextFields(this.slideTemplateTextFields),
                        imageFields: cloneFields(this.slideTemplateImageFields),
                        thumbnailTemplateHtml: cloneTemplateWithUniqueIds(this.slideTemplateThumbnailHtml, id),
                        templateHtml: cloneTemplateWithUniqueIds(this.slideTemplateHtml, id)
                    });
                })
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

    uploadSlides() {
        const assetManager = this.editor.AssetManager;
        const collectedAssets = new Map();
        const uploadStatus = createUploadStatus(this.root.ownerDocument || document);
        let closeTimer = null;
        let uploadInput = null;
        let emptyHint = null;
        let restored = false;

        const scheduleClose = () => {
            globalThis.clearTimeout(closeTimer);
            closeTimer = globalThis.setTimeout(() => {
                if (collectedAssets.size) {
                    assetManager.close();
                }
            }, 80);
        };

        const collectAsset = asset => {
            const normalized = normalizeAsset(asset);
            if (normalized.src) {
                collectedAssets.set(normalized.src, normalized);
                uploadStatus.show(`已上傳 ${collectedAssets.size} 個檔案，正在建立輪播項目…`);
                scheduleClose();
            }
        };
        const handleUploadStart = () => {
            uploadStatus.show('檔案上傳中，請稍候…');
        };
        const handleFileSelection = event => {
            const fileCount = event.target?.files?.length || 0;
            uploadStatus.show(fileCount > 1
                ? `正在上傳 ${fileCount} 個檔案，請稍候…`
                : '檔案上傳中，請稍候…');
        };
        const handleUploadEnd = result => {
            const assets = result?.data || result?.assets || [];
            assets.forEach(collectAsset);
            if (!collectedAssets.size) {
                uploadStatus.hide();
            }
        };
        const handleUploadError = () => {
            uploadStatus.hide();
        };
        const collectUploadResult = result => {
            const assets = result?.assets || result?.data || [];
            assets.forEach(collectAsset);

            if (result?.success && collectedAssets.size) {
                scheduleClose();
            } else if (!result?.success) {
                uploadStatus.hide();
            }
        };
        const restoreEditor = () => {
            if (restored) {
                return;
            }

            restored = true;
            globalThis.clearTimeout(closeTimer);
            uploadInput?.removeEventListener('change', handleFileSelection);
            emptyHint?.remove();
            this.editor.off('asset:add', collectAsset);
            this.editor.off('asset:upload:start', handleUploadStart);
            this.editor.off('asset:upload:end', handleUploadEnd);
            this.editor.off('asset:upload:error', handleUploadError);
            this.editor.off('coker:asset-upload:complete', collectUploadResult);
            uploadStatus.remove();

            const additions = Array.from(collectedAssets.values())
                .map(asset => {
                    const type = detectAssetMediaType(asset);
                    const id = createSlideId();
                    return type ? normalizeSlide({
                        id,
                        type,
                        src: asset.src,
                        title: fileNameWithoutExtension(asset.name || asset.src),
                        hasCaption: this.supportsCaption,
                        textFields: cloneTextFields(this.slideTemplateTextFields),
                        imageFields: cloneFields(this.slideTemplateImageFields),
                        thumbnailTemplateHtml: cloneTemplateWithUniqueIds(this.slideTemplateThumbnailHtml, id),
                        templateHtml: cloneTemplateWithUniqueIds(this.slideTemplateHtml, id)
                    }) : null;
                })
                .filter(Boolean);

            this.slides.push(...additions);
            this.activeId = additions.at(-1)?.id || this.activeId;
            globalThis.setTimeout(() => this.open(), 0);
        };

        this.editor.on('asset:add', collectAsset);
        this.editor.on('asset:upload:start', handleUploadStart);
        this.editor.on('asset:upload:end', handleUploadEnd);
        this.editor.on('asset:upload:error', handleUploadError);
        this.editor.on('coker:asset-upload:complete', collectUploadResult);
        this.editor.once('asset:close', restoreEditor);
        assetManager.open({
            // Use a dedicated type which no persisted asset has, so this dialog
            // is an upload-only surface instead of an asset picker.
            types: ['coker-swiper-batch-upload'],
            accept: 'image/*,video/*'
        });
        globalThis.setTimeout(() => {
            const document = this.root.ownerDocument;
            uploadInput = document?.querySelector('.gjs-mdl-content .gjs-am-file-uploader input[type="file"]') || null;
            uploadInput?.addEventListener('change', handleFileSelection);

            const assets = document?.querySelector('.gjs-mdl-content .gjs-am-assets');
            if (assets && !assets.querySelector('.coker-swiper-upload-empty')) {
                emptyHint = document.createElement('div');
                emptyHint.className = 'coker-swiper-upload-empty';
                emptyHint.textContent = '請從左側一次選取或拖曳多張圖片／影片；上傳成功後會自動建立輪播項目。';
                assets.append(emptyHint);
            }
        }, 0);
    }

    addEmbedSlide() {
        const id = createSlideId();
        const slide = normalizeSlide({
            id,
            type: swiperMediaTypes.embed,
            title: '嵌入式影片',
            hasCaption: this.supportsCaption,
            textFields: cloneTextFields(this.slideTemplateTextFields),
            imageFields: cloneFields(this.slideTemplateImageFields),
            thumbnailTemplateHtml: cloneTemplateWithUniqueIds(this.slideTemplateThumbnailHtml, id),
            templateHtml: cloneTemplateWithUniqueIds(this.slideTemplateHtml, id)
        });
        this.slides.push(slide);
        this.activeId = slide.id;
        this.render();
        globalThis.setTimeout(() => {
            this.root.querySelector('[data-media="embed"] [data-field="src"]')?.focus();
        }, 0);
    }

    duplicateSlide(slideId) {
        const index = this.slides.findIndex(slide => slide.id === slideId);
        if (index < 0) {
            return;
        }

        const source = this.slides[index];
        const id = createSlideId();
        const duplicate = normalizeSlide({
            ...source,
            id,
            thumbnailTemplateHtml: cloneTemplateWithUniqueIds(source.thumbnailTemplateHtml, id),
            templateHtml: cloneTemplateWithUniqueIds(source.templateHtml, id),
            textFields: cloneTextFields(source.textFields),
            imageFields: cloneFields(source.imageFields)
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

        const selectedSlides = this.slides.filter(slide => this.selectedIds.has(slide.id));
        const selectedMediaTypes = new Set(selectedSlides.map(slide => slide.type));
        const initialMediaFilter = selectedMediaTypes.size === 1
            ? selectedMediaTypes.values().next().value
            : 'all';
        this.root.querySelectorAll('[data-bulk]').forEach(field => { field.value = ''; });
        this.root.querySelector('[data-role="bulk-media-filter"]').value = initialMediaFilter;
        const mediaDescription = selectedMediaTypes.size === 1
            ? `，已自動切換為${mediaTypeLabel(initialMediaFilter)}模式`
            : '，包含混合媒體類型';
        this.root.querySelector('[data-role="bulk-summary"]').textContent =
            `目前已勾選 ${this.selectedIds.size} 筆輪播項目${mediaDescription}。`;
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

        let appliedCount = 0;
        this.slides = this.slides.map(slide => {
            if (!targetSlides.includes(slide)) {
                return slide;
            }

            const applicableValues = Object.fromEntries(
                Object.entries(values).filter(([key]) => isBulkSettingApplicable(key, slide.type))
            );
            if (!Object.keys(applicableValues).length) {
                return slide;
            }

            const updatedSlide = normalizeSlide({ ...slide, ...applicableValues });
            if (serializeSlideState([updatedSlide]) !== serializeSlideState([slide])) {
                appliedCount += 1;
            }
            return updatedSlide;
        });
        if (!appliedCount) {
            this.notify('alert', '目前填寫的設定不適用於所選媒體，或設定值與原資料相同。');
            return;
        }

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
                dirtyTextPaths: this.dirtyTextPaths,
                dirtyImagePaths: this.dirtyImagePaths,
                dirtyVisibilityPaths: this.dirtyVisibilityPaths,
                dirtyGroupPaths: this.dirtyGroupPaths
            })) {
                this.notify('error', '輪播內容更新失敗，已保留原始內容。');
                this.refreshSwiper(frameWindow);
                return;
            }

            this.editor.Modal.close();
            this.refreshSwiper(frameWindow);
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
            this.refreshSwiper(frameWindow);
            return;
        }
        if (thumbnailWrapper && !replaceSlideCollection(thumbnailWrapper, renderedThumbnails, this.slides.length)) {
            this.notify('error', '輪播縮圖更新失敗，請重新整理畫布後再嘗試。');
            this.refreshSwiper(frameWindow);
            return;
        }

        this.editor.Modal.close();
        this.refreshSwiper(frameWindow);
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

    refreshSwiper(frameWindow) {
        if (!frameWindow) {
            return;
        }

        frameWindow.requestAnimationFrame(() => {
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
        name: asset?.get?.('name') || asset?.name || asset?.attributes?.name || '',
        type: asset?.get?.('type') || asset?.type || asset?.attributes?.type || '',
        mimeType: asset?.get?.('mimeType') || asset?.get?.('contentType') ||
            asset?.mimeType || asset?.contentType || asset?.attributes?.mimeType ||
            asset?.attributes?.contentType || ''
    };
}

function createUploadStatus(document) {
    const element = document.createElement('div');
    const message = document.createElement('strong');
    element.className = 'coker-swiper-upload-status';
    element.hidden = true;
    element.setAttribute('role', 'status');
    element.setAttribute('aria-live', 'polite');
    element.innerHTML = '<span class="coker-swiper-upload-spinner" aria-hidden="true"></span>';
    element.append(message);
    document.body.append(element);

    return {
        show(value) {
            message.textContent = value;
            element.hidden = false;
        },
        hide() {
            element.hidden = true;
        },
        remove() {
            element.remove();
        }
    };
}

function detectAssetMediaType(asset) {
    const declaredType = String(asset?.type || '').toLowerCase();
    const mimeType = String(asset?.mimeType || '').toLowerCase();
    if (mimeType.startsWith('video/')) {
        return swiperMediaTypes.video;
    }
    if (mimeType.startsWith('image/')) {
        return swiperMediaTypes.image;
    }

    const path = String(asset?.name || asset?.src || '').split(/[?#]/)[0].toLowerCase();
    if (/\.(mp4|webm|ogg|ogv|mov|m4v)$/.test(path)) {
        return swiperMediaTypes.video;
    }
    if (/\.(avif|bmp|gif|jpe?g|png|svg|webp)$/.test(path)) {
        return swiperMediaTypes.image;
    }
    if (declaredType === swiperMediaTypes.video) {
        return swiperMediaTypes.video;
    }
    if (declaredType === swiperMediaTypes.image) {
        return swiperMediaTypes.image;
    }

    return null;
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
        dirtyTextPaths,
        dirtyImagePaths,
        dirtyVisibilityPaths,
        dirtyGroupPaths
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
            updateVerticalMainSlide(
                clone,
                slide,
                dirtyTextPaths.get(slide.id) || new Set(),
                dirtyImagePaths.get(slide.id) || new Set(),
                dirtyVisibilityPaths.get(slide.id) || new Set(),
                dirtyGroupPaths.get(slide.id) || new Set()
            );
            return clone;
        });
        const thumbnailComponents = thumbnailWrapper
            ? slides.map(slide => {
                const source = resolveSourceComponent(slide.id, thumbnailSlideComponents, duplicateSourceIds) || originalThumbnails[0];
                const clone = source?.clone?.();
                if (!clone) {
                    throw new Error(`Unable to clone thumbnail slide ${slide.id}`);
                }
                updateVerticalThumbnailSlide(
                    clone,
                    slide,
                    dirtyImagePaths.get(slide.id) || new Set(),
                    dirtyVisibilityPaths.get(slide.id) || new Set(),
                    dirtyGroupPaths.get(slide.id) || new Set()
                );
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

function updateVerticalMainSlide(component, slide, dirtyTextPaths, dirtyImagePaths, dirtyVisibilityPaths, dirtyGroupPaths) {
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
    updateImageComponent(image, slide.src, {
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

    slide.imageFields.filter(field => field.scope === 'slide').forEach(field => {
        if (!dirtyImagePaths.has(getAdvancedFieldKey(field))) {
            return;
        }
        const target = findComponentByPath(component, field.path);
        if (!target || getComponentTagName(target) !== 'img') {
            throw new Error(`Advanced image ${field.path} not found for slide ${slide.id}`);
        }
        updateImageComponent(target, field.src);
    });
    updateAdvancedVisibilityComponents(
        component,
        [...slide.textFields, ...slide.imageFields.filter(field => field.scope === 'slide')],
        dirtyVisibilityPaths,
        slide.id
    );
    updateAdvancedGroupComponents(
        component,
        [...slide.textFields, ...slide.imageFields.filter(field => field.scope === 'slide')],
        dirtyGroupPaths,
        slide.id
    );
}

function updateVerticalThumbnailSlide(component, slide, dirtyImagePaths, dirtyVisibilityPaths, dirtyGroupPaths) {
    setComponentStateClass(component, 'backstageType', slide.hidden);
    const image = findFirstComponent(component, model => (
        getComponentTagName(model) === 'img' && model.getClasses?.().includes('original')
    )) || findFirstComponent(component, model => getComponentTagName(model) === 'img');
    if (!image) {
        throw new Error(`Thumbnail image not found for slide ${slide.id}`);
    }
    updateImageComponent(image, slide.poster || slide.src, { alt: slide.title });
    slide.imageFields.filter(field => field.scope === 'thumbnail').forEach(field => {
        if (!dirtyImagePaths.has(getAdvancedFieldKey(field))) {
            return;
        }
        const target = findComponentByPath(component, field.path);
        if (!target || getComponentTagName(target) !== 'img') {
            throw new Error(`Thumbnail image ${field.path} not found for slide ${slide.id}`);
        }
        updateImageComponent(target, field.src);
    });
    updateAdvancedVisibilityComponents(
        component,
        slide.imageFields.filter(field => field.scope === 'thumbnail'),
        dirtyVisibilityPaths,
        slide.id
    );
    updateAdvancedGroupComponents(
        component,
        slide.imageFields.filter(field => field.scope === 'thumbnail'),
        dirtyGroupPaths,
        slide.id
    );
}

function updateAdvancedVisibilityComponents(component, fields, dirtyVisibilityPaths, slideId) {
    const applied = new Set();
    fields.forEach(field => {
        const key = getAdvancedVisibilityKey(field);
        if (applied.has(key) || !dirtyVisibilityPaths.has(key)) {
            return;
        }
        applied.add(key);
        const target = findComponentByPath(component, field.visibilityPath);
        if (!target) {
            throw new Error(`Visibility target ${field.visibilityPath} not found for slide ${slideId}`);
        }
        setComponentStateClass(target, 'backstageType', field.hidden);
    });
}

function updateAdvancedGroupComponents(component, fields, dirtyGroupPaths, slideId) {
    const applied = new Set();
    fields.forEach(field => {
        const key = getAdvancedGroupKey(field);
        if (applied.has(key) || !dirtyGroupPaths.has(key) || field.groupType !== 'link') {
            return;
        }
        applied.add(key);
        const target = findComponentByPath(component, field.groupPath);
        if (!target || getComponentTagName(target) !== 'a') {
            throw new Error(`Link group ${field.groupPath} not found for slide ${slideId}`);
        }
        const attributes = { target: field.groupTarget || '_self' };
        if (field.groupHref) {
            attributes.href = field.groupHref;
        }
        target.addAttributes?.(attributes);
        if (!field.groupHref) {
            target.removeAttributes?.(['href']);
        }
    });
}

function updateImageComponent(image, source, attributes = {}) {
    // GrapesJS Image serializes `model.src` instead of attributes.src.
    // Keep both values in sync so toHTML() does not restore the old image.
    image.addAttributes?.({ ...attributes, src: source });
    image.set?.({ src: source });
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

function findComponentByPath(root, path) {
    if (!path) {
        return null;
    }

    return path.split('.').reduce((component, part) => {
        const index = Number(part);
        const elementChildren = getChildComponents(component).filter(child => getComponentTagName(child));
        return Number.isInteger(index) ? elementChildren[index] : null;
    }, root);
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
    let parsedDefinitionCount = 0;

    try {
        const definitions = parseComponentDefinitions(collection, html);
        parsedDefinitionCount = definitions.length;
        if (parsedDefinitionCount !== expectedCount) {
            throw new Error(`Parsed ${parsedDefinitionCount} of ${expectedCount} slide definitions.`);
        }

        // components(html) tries to reuse globally registered models by their
        // element IDs. Slides cloned from one template can share those IDs and
        // collapse to an empty collection. Resetting parsed definitions creates
        // independent models for every sibling instead.
        collection.reset(definitions);
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
            parsedDefinitionCount,
            currentCount: currentSlideCount,
            slidesWithMedia
        });
        restoreWrapperComponents(wrapper, originalHtml);
    }

    return succeeded;
}

function restoreWrapperComponents(wrapper, html) {
    try {
        const collection = wrapper.components?.();
        if (!collection) {
            throw new Error('Wrapper component collection is unavailable.');
        }
        collection.reset(parseComponentDefinitions(collection, html));
    } catch (error) {
        console.error('[Coker Swiper] restoring original slide collection failed', error);
    }
}

function parseComponentDefinitions(collection, html) {
    const parsed = collection.parseString(html, {
        cloneRules: true,
        keepIds: collectComponentIds(collection)
    });
    if (Array.isArray(parsed)) {
        return parsed.filter(Boolean);
    }
    return parsed ? [parsed] : [];
}

function collectComponentIds(collection) {
    const ids = [];
    const stack = [...Array.from(collection.models || collection)];
    while (stack.length) {
        const component = stack.pop();
        const id = component?.getId?.() || component?.getAttributes?.().id;
        if (id) {
            ids.push(id);
        }
        stack.push(...getChildComponents(component));
    }
    return ids;
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
        textFields: slide.textFields,
        imageFields: slide.imageFields
    })));
}

function cloneTextFields(fields) {
    return cloneFields(fields);
}

function cloneFields(fields) {
    return Array.isArray(fields) ? fields.map(field => ({ ...field })) : [];
}

function getAdvancedFieldKey(field) {
    return `${field.scope || 'slide'}:${field.path}`;
}

function getAdvancedVisibilityKey(field) {
    return `${field.scope || 'slide'}:${field.visibilityPath || field.path}`;
}

function getAdvancedGroupKey(field) {
    return `${field.scope || 'slide'}:${field.groupPath || field.path}`;
}

function collectAdvancedGroups(slide) {
    const groups = new Map();
    const items = [
        ...slide.textFields.map(field => ({ type: 'text', field })),
        ...slide.imageFields.map(field => ({ type: 'image', field }))
    ].sort(compareAdvancedFieldPosition);
    items.forEach(item => {
        const key = getAdvancedGroupKey(item.field);
        if (!groups.has(key)) {
            groups.set(key, {
                key,
                type: item.field.groupType || 'content',
                label: item.field.groupLabel || item.field.label,
                href: item.field.groupHref || '',
                target: item.field.groupTarget || '_self',
                items: []
            });
        }
        groups.get(key).items.push(item);
    });
    return Array.from(groups.values()).map((group, index) => ({
        ...group,
        label: `${getNeutralAdvancedGroupLabel(group)} ${index + 1}`
    }));
}

function compareAdvancedFieldPosition(left, right) {
    const leftScope = left.field.scope === 'thumbnail' ? 1 : 0;
    const rightScope = right.field.scope === 'thumbnail' ? 1 : 0;
    if (leftScope !== rightScope) {
        return leftScope - rightScope;
    }

    const leftPath = String(left.field.path || '').split('.').map(Number);
    const rightPath = String(right.field.path || '').split('.').map(Number);
    const length = Math.max(leftPath.length, rightPath.length);
    for (let index = 0; index < length; index += 1) {
        const difference = (leftPath[index] ?? -1) - (rightPath[index] ?? -1);
        if (difference) {
            return difference;
        }
    }
    return 0;
}

function getNeutralAdvancedGroupLabel(group) {
    if (group.type === 'link') {
        return '連結區域';
    }
    if (group.items.length > 1) {
        return '內容區域';
    }
    return group.items[0]?.type === 'image' ? '圖片區域' : '文字區域';
}

function createAdvancedTextField(field) {
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
    return label;
}

function createAdvancedImageField(field) {
    const entry = document.createElement('div');
    const details = document.createElement('label');
    const caption = document.createElement('span');
    const inputGroup = document.createElement('div');
    const input = document.createElement('input');
    const preview = document.createElement('button');
    const select = document.createElement('button');
    const fieldKey = getAdvancedFieldKey(field);
    entry.className = 'coker-swiper-advanced-image-entry';
    caption.textContent = field.label;
    caption.title = field.src;
    inputGroup.className = 'coker-swiper-input-group';
    preview.type = 'button';
    preview.className = 'coker-swiper-advanced-image-preview';
    preview.dataset.action = 'preview-advanced-image';
    preview.dataset.imageKey = fieldKey;
    preview.title = `放大預覽${field.label}`;
    preview.setAttribute('aria-label', preview.title);
    const thumbnail = document.createElement('img');
    thumbnail.src = field.src;
    thumbnail.alt = field.alt || field.label;
    preview.append(thumbnail);
    input.type = 'text';
    input.dataset.imageKey = fieldKey;
    input.value = field.src;
    select.type = 'button';
    select.dataset.action = 'select-asset';
    select.dataset.assetType = swiperMediaTypes.image;
    select.dataset.imageKey = fieldKey;
    select.title = `選擇${field.label}`;
    select.setAttribute('aria-label', select.title);
    select.innerHTML = '<span class="material-symbols-outlined">image_search</span>';
    inputGroup.append(input, select);
    details.append(caption, inputGroup);
    entry.append(preview, details);
    return entry;
}

function createAdvancedVisibilityButton(type, field) {
    const button = document.createElement('button');
    const fieldKey = getAdvancedFieldKey(field);
    button.type = 'button';
    button.className = `coker-swiper-advanced-visibility${field.hidden ? ' is-hidden' : ''}`;
    button.dataset.action = 'toggle-advanced-visibility';
    button.dataset.advancedType = type;
    button.dataset.advancedKey = fieldKey;
    button.title = field.hidden ? '目前前台隱藏，點擊改為顯示' : '目前前台顯示，點擊改為隱藏';
    button.setAttribute('aria-label', button.title);
    button.innerHTML = `<span class="material-symbols-outlined">${field.hidden ? 'visibility_off' : 'visibility'}</span>`;
    return button;
}

function markDirtyPath(collection, slideId, path) {
    if (!collection.has(slideId)) {
        collection.set(slideId, new Set());
    }
    collection.get(slideId).add(path);
}

function cloneTemplateWithUniqueIds(html, slideId) {
    if (!html) {
        return '';
    }

    const parser = new DOMParser();
    const document = parser.parseFromString(html, 'text/html');
    const root = document.body.firstElementChild;
    if (!root) {
        return html;
    }

    const suffix = String(slideId || createSlideId()).replace(/[^a-z0-9_-]/gi, '');
    const idMap = new Map();
    const elementsWithIds = [root, ...root.querySelectorAll('[id]')].filter(element => element.id);
    elementsWithIds.forEach((element, index) => {
        const originalId = element.id;
        const uniqueId = `${originalId}--coker-${suffix}-${index + 1}`;
        idMap.set(originalId, uniqueId);
        element.id = uniqueId;
    });

    const singleIdAttributes = ['for', 'form', 'list', 'aria-activedescendant'];
    const multipleIdAttributes = ['aria-controls', 'aria-describedby', 'aria-labelledby', 'headers'];
    [root, ...root.querySelectorAll('*')].forEach(element => {
        singleIdAttributes.forEach(attribute => {
            const value = element.getAttribute(attribute);
            if (idMap.has(value)) {
                element.setAttribute(attribute, idMap.get(value));
            }
        });
        multipleIdAttributes.forEach(attribute => {
            const value = element.getAttribute(attribute);
            if (!value) {
                return;
            }
            element.setAttribute(attribute, value.split(/\s+/)
                .map(id => idMap.get(id) || id)
                .join(' '));
        });
        ['href', 'xlink:href', 'data-bs-target', 'data-target'].forEach(attribute => {
            const value = element.getAttribute(attribute);
            if (value?.startsWith('#') && idMap.has(value.slice(1))) {
                element.setAttribute(attribute, `#${idMap.get(value.slice(1))}`);
            }
        });
    });

    return root.outerHTML;
}
