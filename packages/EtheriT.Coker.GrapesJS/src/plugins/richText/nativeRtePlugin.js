import {
    linkComponentType,
    openLinkComponentEditor
} from '../link/linkComponentPlugin.js';

const defaultFontSizes = [
    '0.75rem',
    '0.875rem',
    '1rem',
    '1.125rem',
    '1.25rem',
    '1.5rem',
    '2rem',
    '2.5rem',
    '3rem',
    '3.5rem',
    '4rem',
    '4.5rem',
    '5rem',
    '6rem'
];

const toolbarViewportPadding = 8;

function clampToolbarPositionToCanvas(position) {
    const {
        canvasOffsetLeft,
        canvasOffsetTop,
        canvasRect,
        targetHeight,
        targetWidth
    } = position || {};
    const canvasWidth = Number(canvasRect?.width);
    const canvasHeight = Number(canvasRect?.height);
    const offsetLeft = Number(canvasOffsetLeft);
    const offsetTop = Number(canvasOffsetTop);
    const toolbarWidth = Number(targetWidth);
    const toolbarHeight = Number(targetHeight);

    if (
        Number.isFinite(canvasWidth) &&
        Number.isFinite(offsetLeft) &&
        Number.isFinite(toolbarWidth)
    ) {
        const minLeft = toolbarViewportPadding - offsetLeft;
        const maxLeft = canvasWidth - toolbarViewportPadding - toolbarWidth - offsetLeft;

        position.left = maxLeft >= minLeft
            ? Math.min(Math.max(Number(position.left) || 0, minLeft), maxLeft)
            : minLeft;
    }

    if (
        Number.isFinite(canvasHeight) &&
        Number.isFinite(offsetTop) &&
        Number.isFinite(toolbarHeight)
    ) {
        const minTop = toolbarViewportPadding - offsetTop;
        const maxTop = canvasHeight - toolbarViewportPadding - toolbarHeight - offsetTop;

        position.top = maxTop >= minTop
            ? Math.min(Math.max(Number(position.top) || 0, minTop), maxTop)
            : minTop;
    }
}

function getSelectionElement(rte) {
    const selection = rte.selection();
    let node = selection?.anchorNode;

    if (node?.nodeType === 3) {
        node = node.parentElement;
    }

    return node?.nodeType === 1 ? node : null;
}

function isIconElement(element) {
    const tagName = String(element?.tagName || '').toLowerCase();
    const classNames = String(element?.className || '');

    return ['i', 'svg', 'use', 'path', 'img'].includes(tagName)
        || element?.getAttribute?.('aria-hidden') === 'true'
        || element?.getAttribute?.('role') === 'img'
        || element?.hasAttribute?.('data-icon')
        || /(^|\s)(?:fa[srlbd]?|fa-[\w-]+|material-icons(?:-outlined)?|material-symbols-[\w-]+|mdi(?:-[\w-]+)?|bi(?:-[\w-]+)?)(?=\s|$)/i.test(classNames);
}

function getElementDisplayText(element) {
    if (!element || isIconElement(element)) {
        return '';
    }

    const clone = element.cloneNode(true);
    clone.querySelectorAll('*').forEach(child => {
        if (isIconElement(child)) {
            child.remove();
        }
    });

    return clone.textContent?.trim() || '';
}

function getCurrentLink(rte) {
    const selection = rte.selection();
    const nodes = [selection?.anchorNode, selection?.focusNode];

    for (let node of nodes) {
        if (node?.nodeType === 3) {
            node = node.parentElement;
        }
        const link = node?.closest?.('a');
        if (link && rte.el.contains(link)) {
            return link;
        }
    }

    if (!selection?.rangeCount) {
        return null;
    }

    const range = selection.getRangeAt(0);
    return Array.from(rte.el.querySelectorAll('a')).find(link => {
        try {
            return range.intersectsNode(link);
        } catch {
            return false;
        }
    }) || null;
}

const inlineFormattingProperties = [
    'background-color',
    'color',
    'font-family',
    'font-size',
    'font-style',
    'font-weight',
    'letter-spacing',
    'line-height',
    'text-decoration',
    'text-transform'
];

const formattingTags = new Set([
    'B',
    'EM',
    'FONT',
    'I',
    'S',
    'STRIKE',
    'STRONG',
    'U'
]);

function unwrapElement(element) {
    const parent = element.parentNode;
    if (!parent) {
        return;
    }

    while (element.firstChild) {
        parent.insertBefore(element.firstChild, element);
    }
    element.remove();
}

function clearElementFormatting(element) {
    inlineFormattingProperties.forEach(property => {
        element.style?.removeProperty(property);
    });

    if (!element.getAttribute?.('style')?.trim()) {
        element.removeAttribute?.('style');
    }

    if (formattingTags.has(element.tagName)) {
        unwrapElement(element);
    }
}

function isFormattingOnlySpan(element) {
    if (element.tagName !== 'SPAN') {
        return false;
    }

    return Array.from(element.attributes).every(attribute =>
        ['class', 'id', 'style'].includes(attribute.name) ||
        attribute.name.startsWith('data-gjs-')
    );
}

function getInlineFormattingElements(rte) {
    const selection = rte.selection();
    if (!selection?.rangeCount) {
        return [];
    }

    const range = selection.getRangeAt(0);
    const elements = new Set();

    [selection.anchorNode, selection.focusNode].forEach(selectedNode => {
        let element = selectedNode?.nodeType === 1
            ? selectedNode
            : selectedNode?.parentElement;

        while (element && element !== rte.el) {
            elements.add(element);
            element = element.parentElement;
        }
    });

    if (!range.collapsed) {
        rte.el.querySelectorAll('*').forEach(element => {
            try {
                if (range.intersectsNode(element)) {
                    elements.add(element);
                }
            } catch {
                // Ignore nodes invalidated by the browser's removeFormat command.
            }
        });
    }

    return Array.from(elements).sort((left, right) => {
        if (left.contains(right)) {
            return 1;
        }
        if (right.contains(left)) {
            return -1;
        }
        return 0;
    });
}

function findComponentByElement(component, element) {
    if (component.getEl?.() === element) {
        return component;
    }

    const children = component.components?.().models || [];
    for (const child of children) {
        const match = findComponentByElement(child, element);
        if (match) {
            return match;
        }
    }

    return null;
}

function clearComponentFormatting(editor, element) {
    const component = findComponentByElement(editor.getWrapper(), element);
    if (!component) {
        return null;
    }

    const style = { ...component.getStyle() };
    let changed = false;

    inlineFormattingProperties.forEach(property => {
        if (Object.prototype.hasOwnProperty.call(style, property)) {
            delete style[property];
            changed = true;
        }
    });

    if (changed) {
        // With avoidInlineStyle enabled, this updates the component's #id CSS
        // rule instead of writing a style attribute back to the canvas element.
        component.setStyle(style);
    }

    return component;
}

function clearInlineFormatting(editor, elements) {
    elements.forEach(element => {
        const formattingOnlySpan = isFormattingOnlySpan(element);
        const component = clearComponentFormatting(editor, element);

        if (element.tagName === 'SPAN') {
            // Stop this occurrence from inheriting a shared class rule without
            // deleting that rule or changing any other component using it.
            component?.setClass([]);
            element.removeAttribute('class');

            if (formattingOnlySpan) {
                const idRule = component
                    ? editor.Css.getIdRule(component.getId())
                    : null;
                if (idRule) {
                    editor.Css.remove(idRule);
                }

                unwrapElement(element);
                return;
            }
        }

        clearElementFormatting(element);
    });
}

async function clearSelectedFormatting(editor, rte) {
    const currentLink = getCurrentLink(rte);
    const formattingElements = getInlineFormattingElements(rte);

    if (currentLink?.parentNode) {
        if (currentLink === rte.el) {
            formattingElements.push(currentLink);
            clearInlineFormatting(editor, formattingElements);
            rte.el.dispatchEvent(new rte.doc.defaultView.Event('input', { bubbles: true }));

            const editingView = editor.getModel().get('editing');
            const editingComponent = editingView?.model || editor.getEditing();
            const parentComponent = editingComponent?.parent?.();
            await editingView?.disableEditing?.();

            if (String(editingComponent?.get?.('tagName')).toLowerCase() === 'a') {
                const replacements = editingComponent.replaceWith(
                    editingComponent.getInnerHTML()
                );
                editor.select(replacements[0] || parentComponent);
            }
            return;
        }

        const parent = currentLink.parentNode;
        while (currentLink.firstChild) {
            parent.insertBefore(currentLink.firstChild, currentLink);
        }
        currentLink.remove();
    }

    const selection = rte.selection();
    if (!selection?.rangeCount) {
        return;
    }

    if (!selection.getRangeAt(0).collapsed) {
        rte.exec('removeFormat');
        rte.exec('unlink');
    }

    clearInlineFormatting(editor, formattingElements);
    rte.el.dispatchEvent(new rte.doc.defaultView.Event('input', { bubbles: true }));
}

function getCurrentRem(rte) {
    const element = getSelectionElement(rte);
    if (!element) {
        return '';
    }

    const view = rte.doc.defaultView;
    const fontSize = Number.parseFloat(view.getComputedStyle(element).fontSize);
    const rootSize = Number.parseFloat(
        view.getComputedStyle(rte.doc.documentElement).fontSize
    ) || 16;

    if (!Number.isFinite(fontSize)) {
        return '';
    }

    return `${Number((fontSize / rootSize).toFixed(3))}rem`;
}

function syncFontSizeSelect(select, currentValue, fontSizes) {
    select.querySelector('option[data-current]')?.remove();

    if (currentValue && !fontSizes.includes(currentValue)) {
        const option = select.ownerDocument.createElement('option');
        option.value = currentValue;
        option.textContent = currentValue;
        option.dataset.current = 'true';
        select.appendChild(option);
    }

    select.value = currentValue || '';
    select.title = currentValue
        ? `字體大小：${currentValue}`
        : '字體大小';
}

function getComputedColor(rte, property) {
    const element = getSelectionElement(rte);
    if (!element) {
        return null;
    }

    const value = rte.doc.defaultView.getComputedStyle(element)[property];
    const channels = value?.match(/[\d.]+/g)?.map(Number) || [];
    if (channels.length < 3) {
        return null;
    }

    const hex = channels.slice(0, 3)
        .map(channel => Math.max(0, Math.min(255, Math.round(channel)))
            .toString(16)
            .padStart(2, '0'))
        .join('');

    return {
        hex: `#${hex}`,
        transparent: channels.length > 3 && channels[3] === 0
    };
}

function applyInlineStyle(rte, property, value) {
    const selection = rte.selection();
    if (!selection?.rangeCount) {
        return;
    }

    const range = selection.getRangeAt(0);
    const span = rte.doc.createElement('span');
    span.style[property] = value;

    if (range.collapsed) {
        span.appendChild(rte.doc.createTextNode('\u200b'));
        range.insertNode(span);
        range.setStart(span.firstChild, 1);
        range.collapse(true);
    } else {
        span.appendChild(range.extractContents());
        range.insertNode(span);
        range.selectNodeContents(span);
    }

    selection.removeAllRanges();
    selection.addRange(range);
    rte.el.dispatchEvent(new rte.doc.defaultView.Event('input', { bubbles: true }));
}

function findComponentByAttribute(component, name, value) {
    if (component.getAttributes?.()[name] === value) {
        return component;
    }

    const children = component.components?.().models || [];
    for (const child of children) {
        const match = findComponentByAttribute(child, name, value);
        if (match) {
            return match;
        }
    }

    return null;
}

async function replaceEditingSpanWithLink(editor, rte, range) {
    if (!range.collapsed) {
        return false;
    }

    const selectionElement = getSelectionElement(rte);
    const spanElement = selectionElement?.closest?.('span');
    if (!spanElement || !rte.el?.contains?.(spanElement)) {
        return false;
    }

    // Only replace a SPAN represented by a real GrapesJS component. This
    // avoids converting temporary/internal RTE markup.
    const currentSpanComponent = findComponentByElement(
        editor.getWrapper(),
        spanElement
    );
    if (currentSpanComponent?.get?.('tagName') !== 'span') {
        return false;
    }

    const editingView = editor.getModel().get('editing');
    const markerName = 'data-coker-span-to-link';
    const markerValue = `span-${Date.now()}-${Math.random()
        .toString(36)
        .slice(2)}`;
    const displayText = getElementDisplayText(spanElement) || '連結文字';

    // The edited RTE root may be a parent DIV/P rather than the SPAN itself.
    // Mark the exact DOM node so it can be found again after RTE synchronization.
    spanElement.setAttribute(markerName, markerValue);
    rte.el.dispatchEvent(new rte.doc.defaultView.Event('input', { bubbles: true }));

    // Finish the current RTE session before replacing a component inside it.
    await editingView?.disableEditing?.();

    const spanComponent = findComponentByAttribute(
        editor.getWrapper(),
        markerName,
        markerValue
    );
    if (!spanComponent) {
        spanElement.removeAttribute(markerName);
        return false;
    }

    const attributes = { ...spanComponent.getAttributes() };
    delete attributes[markerName];
    const innerHtml = spanComponent.getInnerHTML();

    const replacements = spanComponent.replaceWith({
        type: linkComponentType,
        name: linkComponentType,
        tagName: 'a',
        attributes: {
            ...attributes,
            'data-text': displayText
        },
        components: innerHtml
    });
    const linkComponent = replacements[0];

    if (linkComponent) {
        editor.select(linkComponent);
        openLinkComponentEditor(editor, linkComponent);
    }

    return true;
}

async function openLinkTraits(editor, rte) {
    const selection = rte.selection();

    if (!selection?.rangeCount) {
        return;
    }

    const range = selection.getRangeAt(0);
    let link = getCurrentLink(rte);

    // A short standalone value (phone, mail, etc.) is commonly wrapped in a
    // SPAN. Requiring users to drag-select it is difficult because GrapesJS may
    // start component dragging first. With only a caret, convert that complete
    // SPAN component into a link while preserving its id/classes and content.
    if (!link && await replaceEditingSpanWithLink(editor, rte, range)) {
        return;
    }

    /*
     * 情況 1：
     * 原本 HTML 就是一個 <a> Component。
     *
     * 此時 RTE 的根節點 rte.el 本身就是 <a>，
     * 不要再透過 DOM marker + input 反查 Component，
     * 直接取得目前正在 Editing 的 GrapesJS Component。
     */
    if (link && link === rte.el) {
        const editingView =
            editor.getModel().get('editing');

        const editingComponent =
            editingView?.model ||
            editor.getEditing();

        if (!editingComponent) {
            return;
        }

        // 舊資料可能沒有 data-text，補成目前顯示文字
        const attributes =
            editingComponent.getAttributes?.() || {};

        if (
            !Object.prototype.hasOwnProperty.call(
                attributes,
                'data-text'
            )
        ) {
            editingComponent.addAttributes({
                'data-text':
                    getElementDisplayText(link) ||
                    '連結文字'
            });
        }

        /*
         * 先結束 RTE。
         * 此時不要 dispatch input，
         * 因為 editingComponent 就是我們要的 Component。
         */
        await editingView?.disableEditing?.();

        openLinkComponentEditor(
            editor,
            editingComponent
        );

        return;
    }

    /*
     * 情況 2：
     * 使用者在一般文字 Component 的 RTE 裡
     * 新增了一個 <a>。
     *
     * 這時 <a> 還只是 rte.el 裡面的 DOM，
     * 必須使用 marker 讓 GrapesJS 同步後再找到 Component。
     */
    const markerName =
        'data-coker-link-editor';

    const markerValue =
        `link-${Date.now()}-${Math.random()
            .toString(36)
            .slice(2)}`;

    if (!link) {
        link = rte.doc.createElement('a');
        link.setAttribute('href', '');

        if (range.collapsed) {
            link.textContent = '連結文字';
            range.insertNode(link);
        } else {
            link.appendChild(
                range.extractContents()
            );

            range.insertNode(link);
        }
    }

    link.setAttribute(
        markerName,
        markerValue
    );

    if (!link.hasAttribute('data-text')) {
        link.setAttribute(
            'data-text',
            getElementDisplayText(link) ||
            '連結文字'
        );
    }

    rte.el.dispatchEvent(
        new rte.doc.defaultView.Event(
            'input',
            {
                bubbles: true
            }
        )
    );

    const editingView =
        editor.getModel().get('editing');

    await editingView?.disableEditing?.();

    const component =
        findComponentByAttribute(
            editor.getWrapper(),
            markerName,
            markerValue
        );

    if (!component) {
        return;
    }

    component.removeAttributes(
        markerName
    );

    openLinkComponentEditor(
        editor,
        component
    );
}

function colorAction(property, title, label) {
    return {
        icon: `<label class="coker-native-rte-color" title="${title}">
            <span>${label}</span><input type="color" value="#000000">
        </label>`,
        // A color input fires `input` continuously while its picker is dragged.
        // Applying on each event repeatedly reparses the RTE and nests SPANs,
        // which quickly becomes expensive. Commit once when the picker closes.
        event: 'change',
        result(rte, action) {
            applyInlineStyle(rte, property, action.btn.querySelector('input').value);
        },
        update(rte, action) {
            const control = action.btn.querySelector('.coker-native-rte-color');
            const input = control.querySelector('input');
            const currentColor = getComputedColor(rte, property);
            const fallback = property === 'backgroundColor' ? '#ffffff' : '#000000';
            const value = currentColor?.transparent ? fallback : currentColor?.hex || fallback;

            input.value = value;
            control.style.setProperty('--coker-rte-current-color', value);
            control.classList.toggle(
                'coker-native-rte-color--transparent',
                Boolean(currentColor?.transparent)
            );
            control.title = currentColor?.transparent
                ? `${title}：透明`
                : `${title}：${value}`;
        }
    };
}

function registerNativeActions(editor, options) {
    const rte = editor.RichTextEditor;
    const fontSizes = options.fontSizes || defaultFontSizes;

    rte.remove('link');
    rte.remove('unlink');
    rte.remove('wrap');

    rte.add('fontSizeRem', {
        icon: `<select class="coker-native-rte-font-size" title="字體大小">
            <option value="">字級</option>
            ${fontSizes.map(size => `<option value="${size}">${size}</option>`).join('')}
        </select>`,
        event: 'change',
        result(currentRte, action) {
            const select = action.btn.querySelector('select');
            if (select.value) {
                applyInlineStyle(currentRte, 'fontSize', select.value);
            }
        },
        update(currentRte, action) {
            const select = action.btn.querySelector('select');
            const currentValue = getCurrentRem(currentRte);
            syncFontSizeSelect(select, currentValue, fontSizes);
        }
    });

    rte.add('unorderedList', {
        icon: '<span class="coker-native-rte-list-icon" aria-hidden="true"><b>&#8226;</b><i>&#8801;</i></span>',
        attributes: { title: '項目符號清單' },
        state: (currentRte, doc) => doc.queryCommandState('insertUnorderedList') ? 1 : 0,
        result: currentRte => currentRte.exec('insertUnorderedList')
    });
    rte.add('orderedList', {
        icon: '<span class="coker-native-rte-list-icon" aria-hidden="true"><b>1.</b><i>&#8801;</i></span>',
        attributes: { title: '編號清單' },
        state: (currentRte, doc) => doc.queryCommandState('insertOrderedList') ? 1 : 0,
        result: currentRte => currentRte.exec('insertOrderedList')
    });
    rte.add('outdent', {
        icon: '&#8676;',
        attributes: { title: '減少縮排' },
        result: currentRte => currentRte.exec('outdent')
    });
    rte.add('indent', {
        icon: '&#8677;',
        attributes: { title: '增加縮排' },
        result: currentRte => currentRte.exec('indent')
    });
    rte.add('foreColor', colorAction('color', '文字顏色', 'A'));
    rte.add('backColor', colorAction('backgroundColor', '背景顏色', '▰'));
    rte.add('cokerLink', {
        icon: '&#128279;',
        attributes: { title: '新增或編輯連結' },
        state: currentRte => getCurrentLink(currentRte) ? 1 : 0,
        result: currentRte => openLinkTraits(editor, currentRte)
    });
    rte.add('clearFormat', {
        icon: '&#8856;',
        attributes: {
            title: '清除文字格式與連結；未反白時清除游標所在的行內格式'
        },
        state: () => 0,
        result: currentRte => clearSelectedFormatting(editor, currentRte)
    });
}

export function nativeRtePlugin(editor, options = {}) {
    let registered = false;

    // GrapesJS aligns the RTE to the edited component. When the toolbar is
    // wider than that component, its own element-width constraint can produce
    // a negative left position. Clamp the final coordinates to the visible
    // canvas on every enable, scroll and resize-driven position update.
    editor.on('rteToolbarPosUpdate', clampToolbarPositionToCanvas);

    editor.onReady(() => {
        if (registered) {
            return;
        }

        registered = true;
        registerNativeActions(editor, options);
    });
}

export { defaultFontSizes as nativeRteDefaultFontSizes };
