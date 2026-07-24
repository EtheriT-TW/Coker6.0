import blocksBasic from 'grapesjs-blocks-basic';

export function registerBasePlugins(grapesjs, options = {}) {
    if (!grapesjs || !grapesjs.plugins || typeof grapesjs.plugins.add !== 'function') {
        throw new Error('[EtheriT.Coker.GrapesJS] grapesjs instance is required.');
    }

    const pluginName = options.pluginName || 'etherit-coker-base-blocks';

    grapesjs.plugins.add(pluginName, (editor, pluginOptions = {}) => {
        blocksBasic(editor, pluginOptions);
    });

    return {
        pluginName
    };
}