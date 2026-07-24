export function registerCokerPlugins(grapesjs, options = {}) {
    if (!grapesjs || !grapesjs.plugins || typeof grapesjs.plugins.add !== 'function') {
        throw new Error('[EtheriT.Coker.GrapesJS] grapesjs instance is required.');
    }

    const pluginName = options.pluginName || 'etherit-coker-grapesjs-core';

    grapesjs.plugins.add(pluginName, (editor, pluginOptions = {}) => {
        editor.EtheriTCoker = {
            options,
            pluginOptions
        };
    });

    return {
        pluginName
    };
}