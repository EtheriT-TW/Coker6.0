import { attachAlertManager } from '../core/createAlertManager.js';

export function registerCokerPlugins(grapesjs, options = {}) {
    if (!grapesjs || !grapesjs.plugins || typeof grapesjs.plugins.add !== 'function') {
        throw new Error('[EtheriT.Coker.GrapesJS] grapesjs instance is required.');
    }

    const pluginName = options.pluginName || 'etherit-coker-grapesjs-core';

    grapesjs.plugins.add(pluginName, (editor, pluginOptions = {}) => {
        const alertManager = attachAlertManager(editor, options.adapter);
        editor.EtheriTCoker = {
            ...(editor.EtheriTCoker || {}),
            options,
            pluginOptions
        };

        editor.Commands.add('etherit:coker:test', {
            run(ed) {
                alertManager.success('EtheriT.Coker.GrapesJS plugin loaded.');
            }
        });
    });

    return {
        pluginName
    };
}
