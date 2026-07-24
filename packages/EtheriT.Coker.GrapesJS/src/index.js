import { createEditorAdapter } from './core/createEditorAdapter.js';
import { registerCokerPlugins } from './plugins/registerCokerPlugins.js';

export { createEditorAdapter };
export { registerCokerPlugins };

export function createCokerGrapesLibrary(options = {}) {
    const adapter = options.adapter || createEditorAdapter(options);

    return {
        name: 'EtheriT.Coker.GrapesJS',
        version: '0.0.0',
        adapter,

        registerPlugins(grapesjs, pluginOptions = {}) {
            return registerCokerPlugins(grapesjs, {
                ...pluginOptions,
                adapter
            });
        }
    };
}