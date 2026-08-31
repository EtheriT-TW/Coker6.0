import { createEditorAdapter } from './core/createEditorAdapter.js';
import { AlertManager, attachAlertManager } from './core/createAlertManager.js';
import { registerCokerPlugins } from './plugins/registerCokerPlugins.js';
import { registerBasePlugins } from './core/registerBasePlugins.js';
import { createCokerGrapesEditor, grapesjs } from './core/createCokerGrapesEditor.js';
import { createOfficialPlugins, officialPluginIds } from './plugins/officialPlugins.js';
import 'grapesjs/dist/css/grapes.min.css';
import './styles/swiperEditor.css';

export { createEditorAdapter };
export { AlertManager, attachAlertManager };
export { registerCokerPlugins };
export { registerBasePlugins };
export { createCokerGrapesEditor };
export { createOfficialPlugins, officialPluginIds };
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
