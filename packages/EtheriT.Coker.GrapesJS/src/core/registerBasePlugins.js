import * as blocksBasicModule from 'grapesjs-blocks-basic';

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

    throw new Error('[EtheriT.Coker.GrapesJS] grapesjs-blocks-basic plugin export is not a function.');
}

export function registerBasePlugins(grapesjs, options = {}) {
    if (!grapesjs || !grapesjs.plugins || typeof grapesjs.plugins.add !== 'function') {
        throw new Error('[EtheriT.Coker.GrapesJS] grapesjs instance is required.');
    }

    const pluginName = options.pluginName || 'etherit-coker-base-blocks';
    const blocksBasic = resolvePlugin(blocksBasicModule);

    grapesjs.plugins.add(pluginName, (editor, pluginOptions = {}) => {
        blocksBasic(editor, pluginOptions);
    });

    return {
        pluginName
    };
}