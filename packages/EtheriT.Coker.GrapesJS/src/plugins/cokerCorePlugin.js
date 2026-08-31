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

const canvasEditorStyleId = 'etherit-coker-canvas-editor-styles';
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
`;

function registerCanvasEditorStyles(editor) {
    const injectStyles = frameEvent => {
        const document = frameEvent?.window?.document || editor.Canvas?.getDocument?.();
        if (!document?.head || document.getElementById(canvasEditorStyleId)) {
            return;
        }

        const style = document.createElement('style');
        style.id = canvasEditorStyleId;
        style.textContent = canvasEditorCss;
        document.head.append(style);
    };

    editor.on('canvas:frame:load', injectStyles);
    editor.on('load', injectStyles);
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

export function cokerCorePlugin(editor, options = {}) {
    const alertManager = attachAlertManager(editor, options.adapter);
    registerEmptyLayoutComponent(editor);
    registerCanvasEditorStyles(editor);
    stabilizeWebpageImportCommand(editor);

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
