import { attachAlertManager } from '../core/createAlertManager.js';

const emptyLayoutTags = new Set([
    'ARTICLE',
    'ASIDE',
    'DIV',
    'FOOTER',
    'HEADER',
    'MAIN',
    'NAV',
    'SECTION'
]);

// Keep the text-editable HTML policy in one place. Tags backed by a special
// GrapesJS component type (eg. table cells) are listed separately below so we
// can retain their model/commands and only add the RTE behaviour they need.
const genericEditableTextTags = new Set([
    'ADDRESS',
    'BLOCKQUOTE',
    'BUTTON',
    'CAPTION',
    'DD',
    'DT',
    'FIGCAPTION',
    'H1',
    'H2',
    'H3',
    'H4',
    'H5',
    'H6',
    'LEGEND',
    'LI',
    'P',
    'PRE',
    'SUMMARY'
]);

const typedEditableTextComponents = Object.freeze([
    // GrapesJS built-in label component (preserves the `for` trait).
    'label',
    // GrapesJS built-in cells and grapesjs-blocks-table cell types.
    'cell',
    'tbl-cell',
    'th'
]);

const basicDesignTraitNames = new Set(['id', 'title']);

const canvasEditorStyleId = 'etherit-coker-canvas-editor-styles';
const compactBackstageClass = 'coker-backstage-compact';
const compactBackstageWidth = 220;
const canvasEditorCss = `
.backstageType {
    position: relative !important;
    isolation: isolate;
}

.backstageType::before {
    content: "前台不顯示";
    position: absolute !important;
    inset: 0 !important;
    z-index: 9999 !important;
    display: flex !important;
    align-items: center;
    justify-content: center;
    box-sizing: border-box;
    background: rgba(32, 32, 32, 0.72) !important;
    color: #fff !important;
    font-size: 2rem;
    font-weight: 900;
    line-height: 1.4;
    text-align: center;
    text-shadow: 0 1px 3px rgba(0, 0, 0, 0.9);
    opacity: 1 !important;
    visibility: visible !important;
    pointer-events: none;
}

.backstageType.${compactBackstageClass}::before {
    content: "visibility_off";
    font-family: "Material Symbols Outlined", sans-serif;
    font-size: 2rem;
    font-weight: 400;
    line-height: 1;
    letter-spacing: normal;
    white-space: nowrap;
    font-feature-settings: "liga";
    -webkit-font-feature-settings: "liga";
}
`;

function registerCanvasEditorStyles(editor) {
    let stopObserving = null;

    const injectStyles = frameEvent => {
        const document = frameEvent?.window?.document || editor.Canvas?.getDocument?.();
        if (!document?.head) {
            return;
        }

        if (!document.getElementById(canvasEditorStyleId)) {
            const style = document.createElement('style');
            style.id = canvasEditorStyleId;
            style.textContent = canvasEditorCss;
            document.head.append(style);
        }

        stopObserving?.();
        stopObserving = observeBackstageVisibility(document);
    };

    editor.on('canvas:frame:load', injectStyles);
    editor.on('load', injectStyles);
}

function observeBackstageVisibility(document) {
    const frameWindow = document.defaultView;
    if (!document.body || !frameWindow?.ResizeObserver || !frameWindow?.MutationObserver) {
        return () => {};
    }

    const observed = new Set();
    const updateCompactState = (element, width = element.getBoundingClientRect().width) => {
        element.classList.toggle(compactBackstageClass, width > 0 && width <= compactBackstageWidth);
    };
    const resizeObserver = new frameWindow.ResizeObserver(entries => {
        entries.forEach(entry => updateCompactState(entry.target, entry.contentRect.width));
    });

    const syncElements = () => {
        observed.forEach(element => {
            if (!element.isConnected || !element.classList.contains('backstageType')) {
                resizeObserver.unobserve(element);
                element.classList.remove(compactBackstageClass);
                observed.delete(element);
            }
        });

        document.querySelectorAll('.backstageType').forEach(element => {
            if (!observed.has(element)) {
                observed.add(element);
                resizeObserver.observe(element);
            }
            updateCompactState(element);
        });
    };

    const mutationObserver = new frameWindow.MutationObserver(syncElements);
    mutationObserver.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });
    syncElements();

    return () => {
        mutationObserver.disconnect();
        resizeObserver.disconnect();
        observed.forEach(element => element.classList.remove(compactBackstageClass));
        observed.clear();
    };
}

function isEmptyLayoutElement(element) {
    if (!element || !emptyLayoutTags.has(element.tagName)) {
        return false;
    }

    return Array.from(element.childNodes || []).every(node => {
        const isWhitespaceText = node.nodeType === 3 && !node.textContent?.trim();
        const isComment = node.nodeType === 8;

        return isWhitespaceText || isComment;
    });
}

function registerEditableTextComponent(editor) {
    // Preserve special component types while giving them the same editing view
    // as GrapesJS text components. In particular, TD/TH must keep their table
    // types because the table plugin identifies them by type for row/column
    // operations.
    typedEditableTextComponents.forEach(type => {
        if (!editor.DomComponents.getType(type)) {
            return;
        }

        editor.DomComponents.addType(type, {
            extendView: 'text',
            model: {
                defaults: {
                    editable: true
                }
            }
        });
    });

    // GrapesJS only infers `text` for a limited set of child markup. Register a
    // single semantic text-container type so elements such as P with SPAN/BR,
    // LI and BUTTON remain editable regardless of their current children.
    editor.DomComponents.addType('coker-editable-text', {
        extend: 'text',
        model: {
            defaults: {
                name: '文字'
            }
        },
        isComponent(element) {
            if (!element || !genericEditableTextTags.has(element.tagName)) {
                return;
            }

            return {
                type: 'coker-editable-text',
                name: element.tagName.toLowerCase(),
                tagName: element.tagName.toLowerCase()
            };
        }
    });
}

function registerEmptyLayoutComponent(editor) {
    const defaultType = editor.DomComponents.getType('default');

    if (!defaultType) {
        throw new Error('[EtheriT.Coker.GrapesJS] GrapesJS default component type is unavailable.');
    }

    editor.DomComponents.addType('coker-empty-layout', {
        model: defaultType.model.extend({}, {
            isComponent(element) {
                if (!isEmptyLayoutElement(element)) {
                    return;
                }

                const originalName = element.getAttribute('data-block-name')?.trim();

                return {
                    type: 'coker-empty-layout',
                    name: originalName || element.tagName.toLowerCase(),
                    tagName: element.tagName.toLowerCase(),
                    droppable: true,
                    editable: false
                };
            }
        }),
        view: defaultType.view
    });
}

function stabilizeWebpageImportCommand(editor) {
    const commandId = 'gjs-open-import-webpage';
    const importButton = editor.Panels.getButton('options', commandId);

    if (!importButton || !editor.Commands.get(commandId)) {
        return;
    }

    importButton.set('command', currentEditor => {
        if (currentEditor.Commands.isActive(commandId)) {
            currentEditor.stopCommand(commandId, { force: true });
        }

        currentEditor.runCommand(commandId, { force: true });
    });
}

function keepComponentOutlinesEnabled(editor) {
    const commandId = 'core:component-outline';
    const buttonId = 'sw-visibility';

    const enableOutlines = () => {
        if (!editor.Commands.get(commandId)) {
            return;
        }

        const button = editor.Panels.getButton('options', buttonId);

        if (button && !button.get('active')) {
            button.set('active', true);
            return;
        }

        if (!editor.Commands.isActive(commandId)) {
            editor.runCommand(commandId);
        }
    };

    editor.on('load', enableOutlines);
    editor.on('canvas:frame:load', enableOutlines);
}

function hasComponentSettings(component) {
    const preference = component?.get?.('cokerOpenSettingsOnSelect');
    if (typeof preference === 'boolean') {
        return preference;
    }

    // Every ordinary GrapesJS component has id/title traits. Treat a component
    // as configurable only when it declares something beyond those basic
    // design fields. Custom components can force this behaviour on or off with
    // `cokerOpenSettingsOnSelect`.
    return component?.getTraits?.().some(trait => {
        const name = trait.getName?.() || trait.get?.('name') || '';
        return !basicDesignTraitNames.has(name);
    }) || false;
}

function openSettingsForConfigurableComponents(editor) {
    editor.on('component:selected', component => {
        if (!hasComponentSettings(component)) {
            return;
        }

        // Wait until GrapesJS has finished changing the TraitManager target.
        queueMicrotask(() => {
            if (editor.getSelected() !== component) {
                return;
            }

            // The link editor temporarily moves the TraitManager DOM into its
            // modal. Opening the sidebar at the same time would move those
            // fields back out and leave an empty modal.
            if (editor.__cokerLinkEditor?.component === component) {
                return;
            }

            const settingsButton = editor.Panels.getButton('views', 'open-tm');
            if (settingsButton && !settingsButton.get('active')) {
                settingsButton.set('active', true);
            }
        });
    });
}

export function cokerCorePlugin(editor, options = {}) {
    const alertManager = attachAlertManager(editor, options.adapter);
    registerEmptyLayoutComponent(editor);
    registerEditableTextComponent(editor);
    registerCanvasEditorStyles(editor);
    stabilizeWebpageImportCommand(editor);
    openSettingsForConfigurableComponents(editor);

    if (options.componentOutlinesOnLoad !== false) {
        keepComponentOutlinesEnabled(editor);
    }

    editor.EtheriTCoker = {
        ...(editor.EtheriTCoker || {}),
        options,
        pluginOptions: options
    };

    editor.Commands.add('etherit:coker:test', {
        run() {
            alertManager.success('EtheriT.Coker.GrapesJS plugin loaded.');
        }
    });
}
