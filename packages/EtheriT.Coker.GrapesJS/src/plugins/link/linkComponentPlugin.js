export const linkComponentType = '連結';

const linkTypes = new Set(['link', 'phone', 'email', 'address']);
const destinationCategory = {
    id: 'coker-link-destination',
    label: '連結與檔案',
    open: true
};
const displayCategory = {
    id: 'coker-link-display',
    label: '顯示設定',
    open: true
};
const hintCategory = {
    id: 'coker-link-hint',
    label: '操作提示',
    open: true
};

export function getAssetSource(asset) {
    return asset?.getSrc?.() || asset?.get?.('src') || asset?.id || '';
}

export function openLinkAssetManager(editor, onSelect) {
    const assetManager = editor.AssetManager;

    assetManager.open({
        types: [],
        accept: 'image/*,video/*,audio/*,.pdf,.doc,.docx,.xls,.xlsx,.ods,.ppt,.pptx,.odp,.txt,.csv,.xml,.zip,.rar',
        cokerLinkAsset: true,
        select(asset) {
            const source = getAssetSource(asset);
            if (!source) {
                return;
            }

            onSelect(source, asset);
            assetManager.close();
        }
    });
}

function restoreElement(element, parent, nextSibling) {
    if (!element || !parent) {
        return;
    }

    if (nextSibling?.parentNode === parent) {
        parent.insertBefore(element, nextSibling);
    } else {
        parent.appendChild(element);
    }
}

/**
 * Opens the real GrapesJS TraitManager for a link component in a modal.
 * The trait views are moved temporarily, then returned to the right sidebar.
 */
export function openLinkComponentEditor(editor, component) {
    if (!component || component.get?.('type') !== linkComponentType) {
        return;
    }

    editor.__cokerLinkEditor?.close?.();

    const modal = editor.Modal;
    const traitManager = editor.TraitManager;
    const modalDocument = modal.getContainer?.()?.ownerDocument || document;
    const root = modalDocument.createElement('div');
    const description = modalDocument.createElement('p');
    const traitHost = modalDocument.createElement('div');
    const actions = modalDocument.createElement('div');
    const removeButton = modalDocument.createElement('button');
    const doneButton = modalDocument.createElement('button');

    root.className = 'coker-link-traits';
    const updateVisibleFields = () => {
        const type = inferLinkType(component.getAttributes());
        const isRegularLink = type === 'link';
        root.dataset.linkType = isRegularLink ? 'link' : 'generated';

        const destinationCategoryElement = traitHost.querySelector(
            '.gjs-trait-category'
        );
        const destinationTraits = destinationCategoryElement
            ?.querySelectorAll('.gjs-trt-trait');
        destinationTraits?.forEach((traitElement, index) => {
            traitElement.classList.toggle(
                'coker-link-trait--hidden',
                !isRegularLink && index > 1
            );
        });

        const destinationTrait = destinationTraits?.[1];
        const presentation = getDestinationPresentation(type);
        const label = destinationTrait?.querySelector('.gjs-label');
        const input = destinationTrait?.querySelector('input');
        if (label) label.textContent = presentation.label;
        if (input) {
            input.placeholder = presentation.placeholder;
            input.readOnly = type === 'address';
            input.setAttribute('aria-readonly', String(type === 'address'));
        }
    };
    updateVisibleFields();
    component.on('change:attributes:data-link-type', updateVisibleFields);
    description.className = 'coker-link-traits__description';
    description.textContent = '請先選擇類型，再輸入網址、電話、郵件或地址。只有一般連結可從檔案庫選擇下載檔案。';
    traitHost.className = 'coker-link-traits__fields';
    actions.className = 'coker-link-traits__actions';
    removeButton.type = 'button';
    removeButton.className = 'coker-link-traits__remove';
    removeButton.textContent = '移除連結';
    doneButton.type = 'button';
    doneButton.textContent = '完成';
    actions.append(removeButton, doneButton);
    root.append(description, traitHost, actions);

    let traitsElement;
    let traitsParent;
    let traitsNextSibling;
    let active = true;
    let suspended = false;

    const detachTraits = () => {
        restoreElement(traitsElement, traitsParent, traitsNextSibling);
    };

    const handleClose = () => {
        if (suspended) {
            return;
        }

        active = false;
        detachTraits();
        component.off('change:attributes:data-link-type', updateVisibleFields);
        editor.off('modal:close', handleClose);
        if (editor.__cokerLinkEditor === controller) {
            delete editor.__cokerLinkEditor;
        }
    };

    const show = () => {
        if (!active || !component.collection) {
            return;
        }

        suspended = false;
        editor.select(component);
        traitManager.select(component);
        traitsElement = traitManager.render();

        if (!traitsParent) {
            traitsParent = traitsElement.parentNode;
            traitsNextSibling = traitsElement.nextSibling;
        }

        traitHost.appendChild(traitsElement);
        updateVisibleFields();
        modal.setTitle('編輯連結');
        modal.setContent(root);
        modal.open({ attributes: { class: 'coker-link-traits-wrapper' } });
        editor.off('modal:close', handleClose);
        editor.on('modal:close', handleClose);
    };

    const controller = {
        component,
        close() {
            if (!active) {
                return;
            }
            modal.close();
        },
        suspend() {
            if (!active) {
                return;
            }
            suspended = true;
            editor.off('modal:close', handleClose);
            detachTraits();
        },
        resume() {
            if (active) {
                queueMicrotask(show);
            }
        }
    };

    removeButton.addEventListener('click', () => {
        const innerHtml = component.getInnerHTML();
        const parent = component.parent();
        modal.close();
        queueMicrotask(() => {
            const replacements = component.replaceWith(innerHtml);
            editor.select(replacements[0] || parent);
        });
    });
    doneButton.addEventListener('click', () => modal.close());
    editor.__cokerLinkEditor = controller;
    show();
}

function updateDisplayText(component) {
    const text = component.getAttributes()['data-text'];
    if (typeof text !== 'string') {
        return;
    }

    const nameComponent = component.find('.name')[0];
    if (nameComponent) {
        nameComponent.components(text);
    } else {
        component.components(text);
    }
}

function inferLinkType(attributes) {
    if (linkTypes.has(attributes['data-link-type'])) {
        return attributes['data-link-type'];
    }

    const href = String(attributes.href || '');
    if (/^tel:/i.test(href)) return 'phone';
    if (/^mailto:/i.test(href)) return 'email';
    if (/google\.[^/]+\/maps\/search|google\.com\/maps\/search/i.test(href)) return 'address';
    return 'link';
}

function decodeValue(value) {
    try {
        return decodeURIComponent(String(value || '').replace(/\+/g, ' '));
    } catch {
        return String(value || '');
    }
}

function getInitialDestinationValue(type, attributes) {
    if (Object.prototype.hasOwnProperty.call(attributes, 'data-link-value')) {
        return attributes['data-link-value'];
    }

    const href = String(attributes.href || '');
    if (type === 'phone') return href.replace(/^tel:/i, '');
    if (type === 'email') return href.replace(/^mailto:/i, '');
    if (type === 'address') {
        const query = href.match(/[?&]query=([^&#]*)/i)?.[1];
        return query ? decodeValue(query) : '';
    }
    return href;
}

function getDestinationPresentation(type) {
    return {
        phone: {
            label: '電話號碼',
            placeholder: '例如：07-123-4567'
        },
        email: {
            label: '電子郵件',
            placeholder: '例如：service@example.com'
        },
        address: {
            label: 'Google 地圖搜尋地址（自動產生）',
            placeholder: '會自動使用下方的顯示文字'
        },
        link: {
            label: '連結網址',
            placeholder: '例如：https://example.com 或 /news'
        }
    }[type] || {
        label: '連結網址',
        placeholder: '請輸入連結'
    };
}

function createGeneratedHref(type, destinationValue) {
    const value = String(destinationValue || '').trim();
    if (!value) return '';

    switch (type) {
        case 'phone':
            return `tel:${value.replace(/\s+/g, '')}`;
        case 'email':
            return `mailto:${value}`;
        case 'address':
            return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(value)}`;
        default:
            return value;
    }
}

function createGeneratedTitle(type, displayText, target) {
    const text = String(displayText || '').trim();
    if (!text) return '';

    const prefix = {
        phone: '撥打電話：',
        email: '寄送郵件至：',
        address: '在 Google 地圖搜尋：',
        link: '連結至：'
    }[type] || '連結至：';
    const targetHint = target === '_blank' ? '(另開新視窗)' : '';
    return `${prefix}${text}${targetHint}`;
}

function getDefaultTarget(type) {
    return type === 'address' ? '_blank' : '_self';
}

function synchronizeLinkAttributes(component, options = {}) {
    if (component.__cokerSynchronizingLink) {
        return;
    }

    component.__cokerSynchronizingLink = true;
    try {
        const attributes = component.getAttributes();
        const type = inferLinkType(attributes);
        const target = options.applyDefaultTarget
            ? getDefaultTarget(type)
            : attributes.target || getDefaultTarget(type);
        const changes = {
            'data-link-type': type,
            target,
            title: createGeneratedTitle(
                type,
                attributes['data-text'],
                target
            )
        };
        const generatedHref = options.generateHref
            ? createGeneratedHref(type, attributes['data-link-value'])
            : null;

        if (options.resetDestination) {
            const destinationValue = type === 'address'
                ? String(attributes['data-text'] || '').trim()
                : '';
            changes['data-link-value'] = destinationValue;
            changes.href = createGeneratedHref(type, destinationValue);
        } else if (generatedHref !== null) {
            changes.href = generatedHref;
        }

        component.addAttributes(changes);
    } finally {
        component.__cokerSynchronizingLink = false;
    }
}

function updateTargetSecurity(component) {
    const attributes = component.getAttributes();
    const rel = new Set(String(attributes.rel || '').split(/\s+/).filter(Boolean));

    if (attributes.target === '_blank') {
        rel.add('noopener');
    } else {
        rel.delete('noopener');
    }

    if (rel.size) {
        component.addAttributes({ rel: Array.from(rel).join(' ') });
    } else {
        component.removeAttributes('rel');
    }
}

export function linkComponentPlugin(editor) {
    editor.DomComponents.addType(linkComponentType, {
        isComponent(element) {
            return element.tagName === 'A'
                ? { type: linkComponentType, name: linkComponentType }
                : undefined;
        },
        model: {
            defaults: {
                dblclickAction: 'traits',
                traits: [
                    {
                        name: 'data-link-type',
                        type: 'select',
                        label: '類型',
                        category: destinationCategory,
                        options: [
                            { id: 'link', name: '連結' },
                            { id: 'phone', name: '電話' },
                            { id: 'email', name: '郵件' },
                            { id: 'address', name: '地址（Google 地圖）' }
                        ]
                    },
                    {
                        name: 'data-link-value',
                        type: 'text',
                        label: '連結網址',
                        placeholder: '例如：https://example.com 或 /news',
                        category: destinationCategory
                    },
                    {
                        name: 'file',
                        type: 'button',
                        text: '選擇檔案',
                        category: destinationCategory,
                        attributes: { 'data-coker-link-only': 'true' },
                        command(currentEditor) {
                            const component = currentEditor.getSelected();
                            if (!component) {
                                return;
                            }

                            const linkEditor = currentEditor.__cokerLinkEditor;
                            linkEditor?.suspend?.();
                            currentEditor.once('asset:close', () => linkEditor?.resume?.());

                            openLinkAssetManager(currentEditor, source => {
                                component.addAttributes({ 'data-link-type': 'link' });
                                component.addAttributes({ 'data-link-value': source });
                            });
                        }
                    },
                    {
                        name: 'data-text',
                        type: 'text',
                        label: '顯示文字',
                        placeholder: '請輸入顯示文字',
                        category: displayCategory
                    },
                    {
                        name: 'target',
                        type: 'select',
                        label: '開啟方式',
                        category: displayCategory,
                        options: [
                            { id: '_self', name: '直接連結' },
                            { id: '_blank', name: '另開視窗' }
                        ]
                    },
                    {
                        name: 'title',
                        type: 'text',
                        label: '提示文字（自動產生）',
                        category: hintCategory,
                        attributes: {
                            readonly: 'readonly',
                            'aria-readonly': 'true'
                        }
                    }
                ]
            },
            init() {
                const initialAttributes = this.getAttributes();
                const initialType = inferLinkType(initialAttributes);
                if (initialType === 'address') {
                    this.addAttributes({
                        'data-link-value': String(
                            initialAttributes['data-text'] || ''
                        ).trim()
                    });
                } else if (!Object.prototype.hasOwnProperty.call(
                    initialAttributes,
                    'data-link-value'
                )) {
                    this.addAttributes({
                        'data-link-value': getInitialDestinationValue(
                            initialType,
                            initialAttributes
                        )
                    });
                }

                this.on('change:attributes:data-text', component => {
                    updateDisplayText(component);
                    if (inferLinkType(component.getAttributes()) === 'address') {
                        component.addAttributes({
                            'data-link-value': String(
                                component.getAttributes()['data-text'] || ''
                            ).trim()
                        });
                    }
                    synchronizeLinkAttributes(component);
                });
                this.on('change:attributes:data-link-value', component => {
                    synchronizeLinkAttributes(component, { generateHref: true });
                });
                this.on('change:attributes:data-link-type', component => {
                    synchronizeLinkAttributes(component, {
                        resetDestination: true,
                        applyDefaultTarget: true
                    });
                });
                this.on('change:attributes:target', component => {
                    updateTargetSecurity(component);
                    synchronizeLinkAttributes(component);
                });
                synchronizeLinkAttributes(this, {
                    generateHref: true
                });
                updateTargetSecurity(this);
            }
        }
    });
}
