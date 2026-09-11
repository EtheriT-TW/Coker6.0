export const ebookComponentType = '電子書';
export const fileDownloadComponentType = '檔案下載';
export const fileAssetType = 'file';
export const fileDownloadSelectCommandId = 'coker:file-download:select-file';

export const fileAssetAccept = [
    'image/*',
    'video/*',
    'audio/*',
    '.pdf',
    '.doc',
    '.docx',
    '.xls',
    '.xlsx',
    '.ods',
    '.ppt',
    '.pptx',
    '.odp',
    '.txt',
    '.csv',
    '.xml',
    '.zip',
    '.rar'
].join(',');

function getAssetValue(asset, name) {
    return asset?.get?.(name) ?? asset?.attributes?.[name] ?? '';
}

export function getFileAssetSource(asset) {
    return asset?.getSrc?.() || getAssetValue(asset, 'src') || asset?.id || '';
}

export function getFileAssetName(asset, source = '') {
    const assetName = getAssetValue(asset, 'name');
    if (assetName) {
        return String(assetName);
    }

    const path = String(source).split(/[?#]/)[0].replace(/\\/g, '/');
    const encodedName = path.split('/').pop() || '';

    try {
        return decodeURIComponent(encodedName);
    } catch (_) {
        return encodedName;
    }
}

function isPdfAsset(asset, source, name) {
    const mimeType = String(getAssetValue(asset, 'mimeType')).toLowerCase();
    return mimeType === 'application/pdf' || /\.pdf(?:$|[?#])/i.test(source) || /\.pdf$/i.test(name);
}

export function openFileAssetManager(editor, options = {}) {
    const assetManager = editor.AssetManager;

    assetManager.open({
        // Older files were stored as image assets. Show all types so those
        // records remain selectable while new non-media uploads use `file`.
        types: [],
        accept: options.accept || fileAssetAccept,
        cokerFileAsset: true,
        select(asset) {
            const source = getFileAssetSource(asset);
            const name = getFileAssetName(asset, source);

            if (!source || (options.validate && !options.validate(asset, source, name))) {
                return;
            }

            options.onSelect?.(asset, source, name);
            assetManager.close();
        }
    });
}

export function getDownloadFileExtension(href, explicitExtension = '') {
    const extension = String(explicitExtension || '').trim().replace(/^\./, '');
    if (extension) {
        return extension.toLowerCase();
    }

    const path = String(href || '').split(/[?#]/)[0];
    const fileName = path.split('/').pop() || '';
    const extensionIndex = fileName.lastIndexOf('.');
    return extensionIndex >= 0 ? fileName.substring(extensionIndex + 1).toLowerCase() : '';
}

export function getDownloadIconClass(extension) {
    if (['jpg', 'jpeg', 'png', 'gif', 'avif'].includes(extension)) return 'fa-file-image';
    if (extension === 'pdf') return 'fa-file-pdf';
    if (['doc', 'docx', 'odt'].includes(extension)) return 'fa-file-word';
    if (['ppt', 'pptx', 'odp'].includes(extension)) return 'fa-file-powerpoint';
    if (['xls', 'xlsx', 'ods'].includes(extension)) return 'fa-file-excel';
    if (['zip', 'rar'].includes(extension)) return 'fa-file-zipper';
    return 'fa-file';
}

function removeFileExtension(name, extension) {
    if (!extension) return name;
    const escapedExtension = extension.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return String(name).replace(new RegExp(`\\.${escapedExtension}$`, 'i'), '');
}

function getDownloadTitle(editor, name) {
    const titleTemplate = editor.Canvas?.getWindow?.()?.local?.LinkToAndBlank;
    return titleTemplate ? String(titleTemplate).replace('{0}', name) : name;
}

export function normalizeFileDownloadComponent(component, editor) {
    if (!component || component.__normalizingFileDownload) return;
    component.__normalizingFileDownload = true;

    try {
        const attributes = component.getAttributes();
        const originalHref = String(attributes.href || '');
        const versionIndex = originalHref.indexOf('?v=');
        const href = versionIndex >= 0 ? originalHref.substring(0, versionIndex) : originalHref;
        const extension = getDownloadFileExtension(href, attributes['data-extension']);
        const iconClass = getDownloadIconClass(extension);
        const storedName = String(attributes['data-file-name'] || attributes.download || '未命名');
        const shouldRename = !(component.getClasses?.() || []).includes('do_not_rename');
        const updates = {
            href,
            'data-file-name': storedName,
            'data-file-icon-extension': extension || 'file',
            'data-file-normalized-href': href,
            title: getDownloadTitle(editor, storedName)
        };
        const removals = [];

        if (shouldRename) {
            component.find('.name')[0]?.components(removeFileExtension(storedName, extension));

            if (extension === 'pdf') {
                updates.target = '_blank';
                updates.rel = 'noopener noreferrer';
                removals.push('download');
            } else {
                updates.download = extension && !new RegExp(`\\.${extension}$`, 'i').test(storedName)
                    ? `${storedName}.${extension}`
                    : storedName;
                if (attributes.target === '_blank') removals.push('target');
                if (attributes.rel === 'noopener noreferrer') removals.push('rel');
            }
        }

        const iconContainer = component.find('.icon')[0];
        if (iconContainer) {
            const currentIcon = iconContainer.find('i')[0];
            if (!currentIcon || !currentIcon.getClasses().includes(iconClass)) {
                iconContainer.components(`<i class="fa-solid ${iconClass}"></i>`);
            }
        }

        const changedAttributes = Object.fromEntries(
            Object.entries(updates).filter(([name, value]) => attributes[name] !== value)
        );
        if (Object.keys(changedAttributes).length) component.addAttributes(changedAttributes);
        if (removals.length) component.removeAttributes(removals);
    } finally {
        component.__normalizingFileDownload = false;
    }
}

function registerFileAssetType(editor) {
    editor.AssetManager.addType(fileAssetType, {
        view: {
            init() {
                const prefix = this.pfx || 'gjs-am-';
                // Keep the legacy image class because its search UI uses this
                // selector for every asset card.
                this.className += ` ${prefix}asset-image ${prefix}asset-file`;
            },

            getPreview() {
                return `<div class="${this.pfx}preview coker-file-asset-preview" aria-hidden="true">` +
                    '<span class="material-symbols-outlined">description</span>' +
                    '</div>';
            }
        }
    });
}

function registerEbookComponent(editor) {
    editor.DomComponents.addType(ebookComponentType, {
        isComponent(element) {
            return element?.classList?.contains('FlipBookItem')
                ? { type: ebookComponentType }
                : undefined;
        },
        model: {
            defaults: {
                name: ebookComponentType,
                traits: [
                    { name: 'title', type: 'text', label: '檔案名稱', placeholder: '點選下方按鈕選擇檔案' },
                    { name: 'data-pdf-url', type: 'text', label: '連結', placeholder: '請輸入電子書路徑' },
                    {
                        name: 'file',
                        type: 'button',
                        text: '選擇 PDF 檔案',
                        command(currentEditor) {
                            const component = currentEditor.getSelected();
                            if (!component) {
                                return;
                            }

                            openFileAssetManager(currentEditor, {
                                accept: 'application/pdf,.pdf',
                                validate(asset, source, name) {
                                    const valid = isPdfAsset(asset, source, name);
                                    if (!valid) {
                                        currentEditor.AlertManager?.alert('電子書僅能選擇 PDF 檔案。');
                                    }
                                    return valid;
                                },
                                onSelect(asset, source, name) {
                                    component.addAttributes({
                                        'data-pdf-url': source,
                                        title: name
                                    });
                                }
                            });
                        }
                    }
                ]
            }
        },
        view: {
            onRender() {
                this.el?.removeAttribute('data-bs-toggle');
            }
        }
    });
}

function registerFileDownloadComponent(editor) {
    const previousComponentType = editor.DomComponents.getType?.(fileDownloadComponentType) ||
        editor.DomComponents.getType?.('default');
    const serializeComponent = previousComponentType?.model?.prototype?.toHTML;

    editor.Commands.add(fileDownloadSelectCommandId, {
        run(currentEditor, sender, commandOptions = {}) {
            const component = commandOptions.component || currentEditor.getSelected();
            if (!component || component.get?.('type') !== fileDownloadComponentType) {
                return;
            }

            const attributes = component.getAttributes();
            const currentName = String(attributes['data-file-name'] || attributes.download || '').trim();
            openFileAssetManager(currentEditor, {
                onSelect(asset, source, name) {
                    component.addAttributes({
                        href: source,
                        'data-file-name': currentName && currentName !== '未命名' ? currentName : name
                    });
                    normalizeFileDownloadComponent(component, currentEditor);
                }
            });
        }
    });

    editor.DomComponents.addType(fileDownloadComponentType, {
        isComponent(element) {
            return element?.classList?.contains('link_with_icon')
                ? { type: fileDownloadComponentType }
                : undefined;
        },
        model: {
            defaults: {
                name: fileDownloadComponentType,
                // The legacy Coker6 double-click handler must not open traits;
                // this component's view opens the file picker directly.
                dblclickAction: 'none',
                traits: [
                    { name: 'data-file-name', type: 'text', label: '檔案名稱', placeholder: '請輸入檔案名稱' },
                    {
                        name: 'file',
                        type: 'button',
                        text: '選擇檔案',
                        command: fileDownloadSelectCommandId
                    }
                ]
            },

            init() {
                this.on('change:attributes', this.normalizeFileDownload);
                this.normalizeFileDownload();
            },

            normalizeFileDownload() {
                normalizeFileDownloadComponent(this, editor);
            },

            toHTML(...args) {
                this.normalizeFileDownload();
                return serializeComponent.apply(this, args);
            }
        },
        view: {
            events: {
                dblclick: 'openFileAssetManager'
            },

            openFileAssetManager(event) {
                event?.preventDefault?.();
                event?.stopPropagation?.();

                const currentEditor = this.em.get('Editor');
                currentEditor.select(this.model);
                currentEditor.runCommand(fileDownloadSelectCommandId, { component: this.model });
                return false;
            },

            onRender() {
                normalizeFileDownloadComponent(this.model, this.em.get('Editor'));
            }
        }
    });
}

export function fileComponentPlugin(editor) {
    registerFileAssetType(editor);
    registerEbookComponent(editor);
    registerFileDownloadComponent(editor);
}
