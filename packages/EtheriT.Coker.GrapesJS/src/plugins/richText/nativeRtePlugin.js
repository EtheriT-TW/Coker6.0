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

function getSelectionElement(rte) {
    const selection = rte.selection();
    let node = selection?.anchorNode;

    if (node?.nodeType === 3) {
        node = node.parentElement;
    }

    return node?.nodeType === 1 ? node : null;
}

function getCurrentLink(rte) {
    const link = getSelectionElement(rte)?.closest?.('a');
    return link && rte.el.contains(link) ? link : null;
}

function clearSelectedFormatting(rte) {
    const selection = rte.selection();
    if (!selection?.rangeCount || selection.getRangeAt(0).collapsed) {
        return;
    }

    rte.exec('removeFormat');
    rte.exec('unlink');
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

async function openLinkTraits(editor, rte) {
    const selection = rte.selection();
    if (!selection?.rangeCount) {
        return;
    }

    const range = selection.getRangeAt(0);
    const markerName = 'data-coker-link-editor';
    const markerValue = `link-${Date.now()}-${Math.random().toString(36).slice(2)}`;
    let link = getCurrentLink(rte);

    if (!link) {
        link = rte.doc.createElement('a');
        link.setAttribute('href', '');

        if (range.collapsed) {
            link.textContent = '連結文字';
            range.insertNode(link);
        } else {
            link.appendChild(range.extractContents());
            range.insertNode(link);
        }
    }

    link.setAttribute(markerName, markerValue);
    if (!link.hasAttribute('data-text')) {
        link.setAttribute('data-text', link.textContent?.trim() || '連結文字');
    }
    rte.el.dispatchEvent(new rte.doc.defaultView.Event('input', { bubbles: true }));

    const editingView = editor.getModel().get('editing');
    await editingView?.disableEditing?.();

    const component = findComponentByAttribute(
        editor.getWrapper(),
        markerName,
        markerValue
    );

    if (!component) {
        return;
    }

    component.removeAttributes(markerName);
    openLinkComponentEditor(editor, component);
}

function colorAction(property, title, label) {
    return {
        icon: `<label class="coker-native-rte-color" title="${title}">
            <span>${label}</span><input type="color" value="#000000">
        </label>`,
        event: 'input',
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
        attributes: { title: '清除選取文字的格式（需先反白文字）' },
        state: currentRte => {
            const selection = currentRte.selection();
            return !selection?.rangeCount || selection.getRangeAt(0).collapsed
                ? -1
                : 0;
        },
        result: currentRte => clearSelectedFormatting(currentRte)
    });
}

export function nativeRtePlugin(editor, options = {}) {
    let registered = false;

    editor.onReady(() => {
        if (registered) {
            return;
        }

        registered = true;
        registerNativeActions(editor, options);
    });
}

export { defaultFontSizes as nativeRteDefaultFontSizes };
