import { createEditorAdapter } from './core/createEditorAdapter.js';
import { registerCokerPlugins } from './plugins/registerCokerPlugins.js';
import { registerBasePlugins } from './core/registerBasePlugins.js';
import { createCokerGrapesEditor, grapesjs } from './core/createCokerGrapesEditor.js';

export { createEditorAdapter };
export { registerCokerPlugins };
export { registerBasePlugins };
export { createCokerGrapesEditor };
export { grapesjs };


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