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
    registerEmptyLayoutComponent(editor);
    stabilizeWebpageImportCommand(editor);

    editor.EtheriTCoker = {
        options,
        pluginOptions: options
    };

    editor.Commands.add('etherit:coker:test', {
        run() {
            const adapter = options.adapter;

            if (adapter?.ui?.success) {
                adapter.ui.success('EtheriT.Coker.GrapesJS plugin loaded.');
                return;
            }

            window.alert('EtheriT.Coker.GrapesJS plugin loaded.');
        }
    });
}
