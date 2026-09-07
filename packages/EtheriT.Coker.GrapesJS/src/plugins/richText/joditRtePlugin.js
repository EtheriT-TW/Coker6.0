const defaultFontSizes = [
    '0.75rem',
    '0.875rem',
    '1rem',
    '1.125rem',
    '1.25rem',
    '1.5rem',
    '2rem',
    '2.5rem',
    '3rem'
];

const defaultButtons = [
    'cokerFontSize',
    'bold',
    'italic',
    'underline',
    'strikethrough',
    '|',
    'brush',
    '|',
    'ul',
    'ol',
    'outdent',
    'indent',
    '|',
    'cokerLink',
    'unlink',
    '|',
    'eraser',
    'undo',
    'redo'
];

const joditStyleId = 'coker-jodit-styles';
let joditModulesPromise;

function ensureStyle(document, css) {
    if (!document?.head || document.getElementById(joditStyleId)) {
        return;
    }

    const style = document.createElement('style');
    style.id = joditStyleId;
    style.textContent = css;
    document.head.appendChild(style);
}

function loadJodit() {
    if (!joditModulesPromise) {
        joditModulesPromise = Promise.all([
            import('jodit'),
            import('jodit/es2021/jodit.min.css?inline')
        ]).then(([joditModule, cssModule]) => ({
            Jodit: joditModule.Jodit || joditModule.default,
            css: cssModule.default
        }));
    }

    return joditModulesPromise;
}

function notifyInput(element) {
    const EventConstructor = element.ownerDocument?.defaultView?.Event || Event;
    element.dispatchEvent(new EventConstructor('input', {
        bubbles: true
    }));
}

function getCurrentFontSize(jodit) {
    let current = jodit.s.current();

    if (current?.nodeType === 3) {
        current = current.parentElement;
    }

    const element = current?.closest?.('[style*="font-size"]') || current;
    if (!element || element === jodit.editor.parentElement) {
        return '';
    }

    const fontSize = Number.parseFloat(
        jodit.ew.getComputedStyle(element).fontSize
    );
    const rootSize = Number.parseFloat(
        jodit.ew.getComputedStyle(jodit.ed.documentElement).fontSize
    ) || 16;

    if (!Number.isFinite(fontSize)) {
        return '';
    }

    return `${Number((fontSize / rootSize).toFixed(3))}rem`;
}

function findCurrentLink(jodit) {
    let current = jodit.s.current();

    if (current?.nodeType === 3) {
        current = current.parentElement;
    }

    const link = current?.closest?.('a');
    return link && jodit.editor.contains(link) ? link : null;
}

function normalizeLinkUrl(value) {
    const url = String(value || '').trim();

    if (!url || /^\s*(?:javascript|vbscript|data):/i.test(url)) {
        return '';
    }

    if (/^(?:https?:|mailto:|tel:|ftp:|#|\/|\.\.?(?:\/|$))/i.test(url)) {
        return url;
    }

    if (/^(?:www\.)?[a-z0-9-]+(?:\.[a-z0-9-]+)+(?:[/?#].*)?$/i.test(url)) {
        return `https://${url}`;
    }

    return '';
}

function applyLink(jodit, currentLink, values) {
    jodit.s.restore();

    let links = currentLink?.isConnected ? [currentLink] : [];
    if (!links.length && !jodit.s.isCollapsed()) {
        links = jodit.s.wrapInTag('a');
    }

    if (!links.length) {
        const link = jodit.createInside.element('a');
        link.textContent = values.text || values.url;
        jodit.s.insertNode(link, false, false);
        links = [link];
    }

    links.forEach(link => {
        link.setAttribute('href', values.url);

        if (values.targetBlank) {
            link.setAttribute('target', '_blank');
        } else {
            link.removeAttribute('target');
        }

        const rel = new Set(
            (link.getAttribute('rel') || '').split(/\s+/).filter(Boolean)
        );
        values.noFollow ? rel.add('nofollow') : rel.delete('nofollow');
        values.targetBlank ? rel.add('noopener') : rel.delete('noopener');
        rel.size
            ? link.setAttribute('rel', Array.from(rel).join(' '))
            : link.removeAttribute('rel');

        values.ariaLabel
            ? link.setAttribute('aria-label', values.ariaLabel)
            : link.removeAttribute('aria-label');
    });

    if (links.length === 1 && values.text && values.textChanged) {
        links[0].textContent = values.text;
    }

    jodit.synchronizeValues();
}

function openLinkModal(grapesEditor, jodit, toolbarContainer) {
    const modal = grapesEditor.Modal;
    const modalContainer = modal.getContainer?.();
    const documentRef = modalContainer?.ownerDocument || document;
    const currentLink = findCurrentLink(jodit);
    const selectedText = currentLink?.textContent || jodit.s.range?.toString() || '';
    const root = documentRef.createElement('form');
    root.className = 'coker-rte-link-modal';
    root.innerHTML = `
        <label class="coker-rte-link-modal__field">
            <span>連結網址</span>
            <input name="url" type="text" inputmode="url" autocomplete="url" required>
            <small>可輸入 https://、mailto:、tel:、站內路徑或 #錨點</small>
        </label>
        <label class="coker-rte-link-modal__field">
            <span>顯示文字</span>
            <input name="text" type="text">
        </label>
        <label class="coker-rte-link-modal__field">
            <span>無障礙標籤（選填）</span>
            <input name="ariaLabel" type="text">
        </label>
        <div class="coker-rte-link-modal__options">
            <label><input name="targetBlank" type="checkbox"> 在新分頁開啟</label>
            <label><input name="noFollow" type="checkbox"> 搜尋引擎不追蹤（nofollow）</label>
        </div>
        <div class="coker-rte-link-modal__error" role="alert" hidden></div>
        <div class="coker-rte-link-modal__actions">
            <button type="button" data-action="cancel">取消</button>
            <button type="submit" class="coker-rte-link-modal__submit">套用連結</button>
        </div>
    `;

    const urlInput = root.elements.namedItem('url');
    const textInput = root.elements.namedItem('text');
    const ariaLabelInput = root.elements.namedItem('ariaLabel');
    const targetBlankInput = root.elements.namedItem('targetBlank');
    const noFollowInput = root.elements.namedItem('noFollow');
    const errorElement = root.querySelector('.coker-rte-link-modal__error');
    const cancelButton = root.querySelector('[data-action="cancel"]');
    let applied = false;

    urlInput.value = currentLink?.getAttribute('href') || '';
    textInput.value = selectedText;
    ariaLabelInput.value = currentLink?.getAttribute('aria-label') || '';
    targetBlankInput.checked = currentLink?.getAttribute('target') === '_blank';
    noFollowInput.checked = (currentLink?.getAttribute('rel') || '')
        .split(/\s+/)
        .includes('nofollow');

    jodit.s.save();
    toolbarContainer.classList.add('coker-rte-toolbar--modal-open');

    const handleClose = () => {
        grapesEditor.off('modal:close', handleClose);
        toolbarContainer.classList.remove('coker-rte-toolbar--modal-open');

        if (!applied && !jodit.isDestructed) {
            jodit.s.restore();
            jodit.focus();
        }
    };

    grapesEditor.on('modal:close', handleClose);
    cancelButton.addEventListener('click', () => modal.close());
    root.addEventListener('submit', event => {
        event.preventDefault();
        const url = normalizeLinkUrl(urlInput.value);

        if (!url) {
            errorElement.textContent = '請輸入有效且安全的連結網址。';
            errorElement.hidden = false;
            urlInput.focus();
            return;
        }

        applied = true;
        applyLink(jodit, currentLink, {
            url,
            text: textInput.value.trim(),
            textChanged: textInput.value !== selectedText,
            ariaLabel: ariaLabelInput.value.trim(),
            targetBlank: targetBlankInput.checked,
            noFollow: noFollowInput.checked
        });
        modal.close();
        jodit.focus();
    });

    modal.setTitle(currentLink ? '編輯連結' : '新增連結');
    modal.setContent(root);
    modal.open({
        attributes: {
            class: 'coker-rte-link-modal-wrapper'
        }
    });
    queueMicrotask(() => urlInput.focus());
}

function createControls(Jodit, fontSizes, openLink) {
    const fontSizeControl = Jodit.defaultOptions.controls.fontsize;

    return {
        cokerFontSize: {
            ...fontSizeControl,
            name: 'cokerFontSize',
            command: 'fontsize',
            icon: '',
            tooltip: '字體大小（rem）',
            list: Object.fromEntries(fontSizes.map(size => [size, size])),
            value: getCurrentFontSize,
            textTemplate: (_jodit, value) => value || '字級',
            childTemplate: (_jodit, key, value) => value || key
        },
        cokerLink: {
            icon: 'link',
            tooltip: '設定連結',
            exec(jodit) {
                openLink(jodit);
                return false;
            }
        }
    };
}

export function joditRtePlugin(editor, options = {}) {
    if (options.enabled === false) {
        return;
    }

    const instances = new Set();
    const customConfig = options.config || {};
    const {
        controls: customControls = {},
        events: customEvents = {},
        link: customLink = {},
        ...restConfig
    } = customConfig;
    const fontSizes = options.fontSizes || defaultFontSizes;
    const buttons = options.buttons || defaultButtons;

    editor.setCustomRte({
        // Lists and links must remain HTML owned by the RTE while editing.
        parseContent: false,

        async enable(element, rte) {
            if (rte && !rte.isDestructed) {
                rte.setReadOnly(false);
                rte.focus();
                return rte;
            }

            const toolbarContainer = editor.RichTextEditor.getToolbarEl();
            const initialHeight = element.getBoundingClientRect().height;
            const originalStyle = element.getAttribute('style');
            const { Jodit, css } = await loadJodit();
            ensureStyle(toolbarContainer.ownerDocument, css);
            ensureStyle(element.ownerDocument, css);

            const instance = Jodit.make(element, {
                inline: true,
                toolbar: toolbarContainer,
                toolbarInline: false,
                toolbarAdaptive: false,
                language: 'zh_tw',
                statusbar: false,
                showCharsCounter: false,
                showWordsCounter: false,
                showXPathInStatusbar: false,
                askBeforePasteHTML: false,
                askBeforePasteFromWord: false,
                processPasteHTML: true,
                browserSpellcheck: true,
                buttons,
                ...restConfig,
                controls: {
                    ...createControls(
                        Jodit,
                        fontSizes,
                        jodit => openLinkModal(editor, jodit, toolbarContainer)
                    ),
                    ...customControls
                },
                link: {
                    noFollowCheckbox: true,
                    openInNewTabCheckbox: true,
                    ariaLabelInput: true,
                    ...customLink
                },
                ownerDocument: element.ownerDocument,
                ownerWindow: element.ownerDocument.defaultView,
                events: {
                    ...customEvents,
                    change(value, previousValue) {
                        notifyInput(element);
                        customEvents.change?.call(this, value, previousValue);
                    }
                }
            });

            instance.__cokerOriginalStyle = originalStyle;
            if (initialHeight > 0) {
                element.style.setProperty('height', `${initialHeight}px`, 'important');
            }
            instances.add(instance);
            instance.focus();
            return instance;
        },

        disable(element, rte) {
            if (!rte || rte.isDestructed) {
                return;
            }

            // Inline mode temporarily adds Jodit workplace markup inside the
            // component. Destructing restores the component's clean HTML.
            instances.delete(rte);
            rte.destruct();
            if (rte.__cokerOriginalStyle === null) {
                element.removeAttribute('style');
            } else {
                element.setAttribute('style', rte.__cokerOriginalStyle);
            }
            element.contentEditable = 'false';
        },

        getContent(element, rte) {
            if (!rte || rte.isDestructed) {
                return element.innerHTML;
            }

            return rte.value;
        },

        destroy() {
            instances.forEach(instance => {
                if (!instance.isDestructed) {
                    instance.destruct();
                }
            });
            instances.clear();
        }
    });
}

export { defaultFontSizes as joditDefaultFontSizes };
