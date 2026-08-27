import * as presetNewsletterModule from 'grapesjs-preset-newsletter';

export const newsletterCategory = Object.freeze({
    id: 'newsletter',
    label: '電子報',
    open: true
});

function resolvePlugin(moduleValue) {
    if (typeof moduleValue === 'function') {
        return moduleValue;
    }

    if (typeof moduleValue.default === 'function') {
        return moduleValue.default;
    }

    if (moduleValue.default && typeof moduleValue.default.default === 'function') {
        return moduleValue.default.default;
    }

    throw new Error('[EtheriT.Coker.GrapesJS] grapesjs-preset-newsletter export is not a function.');
}

const presetNewsletter = resolvePlugin(presetNewsletterModule);

export function createNewsletterPlugin(options = {}) {
    const category = {
        ...newsletterCategory,
        ...(options.category || {})
    };
    const customBlockOptions = typeof options.block === 'function'
        ? options.block
        : () => ({});

    const pluginOptions = {
        modalLabelExport: 'Copy the code and use it wherever you want',
        codeViewerTheme: 'material',
        cellStyle: {
            'font-size': '1rem',
            'font-weight': 300,
            'vertical-align': 'top',
            color: 'rgb(111, 119, 125)',
            margin: 0,
            padding: 0
        },
        ...options,
        block(blockId) {
            return {
                ...customBlockOptions(blockId),
                category
            };
        }
    };

    delete pluginOptions.category;

    return function cokerNewsletterPlugin(editor) {
        presetNewsletter(editor, pluginOptions);
    };
}
