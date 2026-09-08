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
    const modalContainer = modal.getContainer?.() || modal.modal?.el || null;
    const modalDocument = modalContainer?.ownerDocument || document;
    const root = modalDocument.createElement('div');
    const description = modalDocument.createElement('p');
    const traitHost = modalDocument.createElement('div');
    const actions = modalDocument.createElement('div');
    const removeButton = modalDocument.createElement('button');

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
    description.textContent = '請先選擇類型，再輸入網址、電話、郵件或地址。所有變更會自動儲存；只有一般連結可從檔案庫選擇下載檔案。';
    traitHost.className = 'coker-link-traits__fields';
    actions.className = 'coker-link-traits__actions';
    removeButton.type = 'button';
    removeButton.className = 'coker-link-traits__remove';
    removeButton.textContent = '移除連結';
    actions.append(removeButton);
    root.append(description, traitHost, actions);

    let traitsElement;
    let traitsParent;
    let traitsNextSibling;
    let active = true;
    let suspended = false;
    let backdropGuardAttached = false;

    const preventBackdropClose = event => {
        if (event.target === modalContainer) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    };

    const closeOnEscape = event => {
        if (event.key !== 'Escape' || suspended || !active) {
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();
        modal.close();
    };

    const attachBackdropGuard = () => {
        if (!modalContainer || backdropGuardAttached) {
            return;
        }

        modalContainer.addEventListener('click', preventBackdropClose, true);
        modalDocument.addEventListener('keydown', closeOnEscape, true);
        backdropGuardAttached = true;
    };

    const detachBackdropGuard = () => {
        if (!modalContainer || !backdropGuardAttached) {
            return;
        }

        modalContainer.removeEventListener('click', preventBackdropClose, true);
        modalDocument.removeEventListener('keydown', closeOnEscape, true);
        backdropGuardAttached = false;
    };

    const detachTraits = () => {
        restoreElement(traitsElement, traitsParent, traitsNextSibling);
    };

    const handleClose = () => {
        if (suspended) {
            return;
        }

        active = false;
        detachBackdropGuard();
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
        attachBackdropGuard();
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
            detachBackdropGuard();
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

function inferLinkTypeFromText(value) {
    const text = String(value || '').trim();
    if (!text) {
        return 'link';
    }

    if (/^[^\s@]+@[^\s@]+\.[^\s@]+$/u.test(text)) {
        return 'email';
    }

    // Avoid treating common date formats as telephone numbers.
    const isDate = /^\d{4}[-/.]\d{1,2}[-/.]\d{1,2}$/.test(text);
    const isPhoneShape = /^\+?[\d\s().-]+(?:\s*(?:#|ext\.?|分機)\s*\d+)?$/iu.test(text);
    const digitCount = (text.match(/\d/g) || []).length;
    if (!isDate && isPhoneShape && digitCount >= 7 && digitCount <= 20) {
        return 'phone';
    }

    return 'link';
}

function getComponentTextContent(component) {
    const children = component.components?.().models || [];
    if (!children.length) {
        return String(component.get?.('content') || '');
    }

    return children
        .map(child => getComponentTextContent(child))
        .join('');
}

function initializeDisplayText(component) {
    const attributes = component.getAttributes();
    if (Object.prototype.hasOwnProperty.call(attributes, 'data-text')) {
        return;
    }

    const displayText = getComponentTextContent(component).trim();
    if (displayText) {
        component.addAttributes({ 'data-text': displayText });
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
    if (!href.trim()) return inferLinkTypeFromText(attributes['data-text']);
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
    if (type === 'phone') {
        return href
            ? href
                .replace(/^tel:/i, '')
                .replace(/;ext=(\d+)$/i, '#$1')
            : String(attributes['data-text'] || '').trim();
    }
    if (type === 'email') {
        return href
            ? href.replace(/^mailto:/i, '')
            : String(attributes['data-text'] || '').trim();
    }
    if (type === 'address') {
        const query = href.match(/[?&]query=([^&#]*)/i)?.[1];
        return query ? decodeValue(query) : '';
    }
    return href || (type !== 'link'
        ? String(attributes['data-text'] || '').trim()
        : '');
}

function getLinkDestinationValues(component) {
    if (!component.__cokerLinkDestinationValues) {
        component.__cokerLinkDestinationValues = {};
    }

    return component.__cokerLinkDestinationValues;
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
        case 'phone': {
            const extension = value.match(/\s*(?:#|ext\.?|分機)\s*(\d+)\s*$/iu);
            const number = (extension
                ? value.slice(0, extension.index)
                : value
            ).replace(/\s+/g, '');
            return `tel:${number}${extension ? `;ext=${extension[1]}` : ''}`;
        }
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

        const desiredValues = {
            'data-link-type': type,
            target,
            title: createGeneratedTitle(
                type,
                attributes['data-text'],
                target
            )
        };

        if (Object.prototype.hasOwnProperty.call(options, 'destinationValue')) {
            const destinationValue = String(options.destinationValue || '');
            desiredValues['data-link-value'] =
                destinationValue;

            desiredValues.href =
                createGeneratedHref(
                    type,
                    destinationValue
                );
        } else if (options.generateHref) {
            desiredValues.href =
                createGeneratedHref(
                    type,
                    attributes['data-link-value']
                );
        }

        /*
         * 只寫入真正有改變的 attribute。
         *
         * GrapesJS 的 addAttributes 即使資料邏輯相同，
         * 仍可能造成 attributes model 更新，
         * 進而觸發 change:attributes:* listener。
         */
        const changes = {};

        Object.entries(desiredValues).forEach(
            ([name, value]) => {
                const currentValue =
                    attributes[name] == null
                        ? ''
                        : String(attributes[name]);

                const nextValue =
                    value == null
                        ? ''
                        : String(value);

                if (currentValue !== nextValue) {
                    changes[name] = value;
                }
            }
        );

        if (Object.keys(changes).length > 0) {
            component.addAttributes(changes);
        }
    } finally {
        component.__cokerSynchronizingLink = false;
    }
}

function updateTargetSecurity(component) {
    if (component.__cokerUpdatingTargetSecurity) {
        return;
    }

    component.__cokerUpdatingTargetSecurity = true;

    try {
        const attributes = component.getAttributes();

        const currentRel = String(
            attributes.rel || ''
        )
            .split(/\s+/)
            .filter(Boolean);

        const rel = new Set(currentRel);

        if (attributes.target === '_blank') {
            rel.add('noopener');
        } else {
            rel.delete('noopener');
        }

        const newRel = Array
            .from(rel)
            .join(' ');

        const oldRel = String(
            attributes.rel || ''
        ).trim();

        // 沒改變就什麼都不要做
        if (newRel === oldRel) {
            return;
        }

        if (newRel) {
            component.addAttributes({
                rel: newRel
            });
        } else if (
            Object.prototype.hasOwnProperty.call(
                attributes,
                'rel'
            )
        ) {
            // 只有真的存在 rel 才 remove
            component.removeAttributes('rel');
        }
    } finally {
        component.__cokerUpdatingTargetSecurity = false;
    }
}

export function linkComponentPlugin(editor) {
    editor.DomComponents.addType(linkComponentType, {
        extend: 'link',
        extendView: 'text',
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
                initializeDisplayText(this);
                const initialAttributes = this.getAttributes();
                const initialType = inferLinkType(initialAttributes);
                const initialDestinationValue = initialType === 'address'
                    ? String(initialAttributes['data-text'] || '').trim()
                    : getInitialDestinationValue(initialType, initialAttributes);

                getLinkDestinationValues(this)[initialType] =
                    initialDestinationValue;

                if (initialType === 'address') {
                    this.addAttributes({
                        'data-link-value': initialDestinationValue
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
                    const attributes = component.getAttributes();
                    const type = inferLinkType(attributes);
                    getLinkDestinationValues(component)[type] =
                        String(attributes['data-link-value'] || '');

                    synchronizeLinkAttributes(component, { generateHref: true });
                });
                this.on('change:attributes:data-link-type', component => {
                    const attributes = component.getAttributes();
                    const previousAttributes = component.previous('attributes') || {};
                    const previousType = inferLinkType(previousAttributes);
                    const type = inferLinkType(attributes);
                    const destinationValues = getLinkDestinationValues(component);

                    destinationValues[previousType] =
                        getInitialDestinationValue(previousType, previousAttributes);

                    if (!Object.prototype.hasOwnProperty.call(destinationValues, type)) {
                        destinationValues[type] = type === 'address'
                            ? String(attributes['data-text'] || '').trim()
                            : '';
                    }

                    synchronizeLinkAttributes(component, {
                        destinationValue: destinationValues[type],
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
