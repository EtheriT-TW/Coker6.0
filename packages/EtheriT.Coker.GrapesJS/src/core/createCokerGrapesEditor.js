import grapesjs from 'grapesjs';
import { createEditorAdapter } from './createEditorAdapter.js';
import { baseBlocksPlugin } from '../plugins/baseBlocksPlugin.js';
import { cokerCorePlugin } from '../plugins/cokerCorePlugin.js';
import { createOfficialPlugins } from '../plugins/officialPlugins.js';
import { grapesZhTw } from '../locales/zhTw.js';

export function createCokerGrapesEditor(options = {}) {
    const adapter = options.adapter || createEditorAdapter(options);
    const initOptions = options.initOptions || {};
    const i18nOptions = initOptions.i18n || {};

    const externalPlugins = options.externalPlugins || [];
    const externalPluginFunctions = options.externalPluginFunctions || [];
    const officialPluginsOptions = options.officialPluginsOptions ||
        options.externalPluginsOpts ||
        {};
    const container = options.container || '#gjs';
    const officialPlugins = createOfficialPlugins(grapesjs, {
        container,
        pluginOptions: officialPluginsOptions
    });

    const editor = grapesjs.init({
        ...initOptions,

        container,
        height: options.height || '100vh',
        fromElement: options.fromElement ?? true,

        storageManager: options.storageManager || {
            autoload: false
        },

        i18n: {
            locale: 'tw',
            localeFallback: 'tw',
            ...i18nOptions,
            messages: {
                tw: grapesZhTw,
                ...(i18nOptions.messages || {})
            }
        },

        plugins: [
            grapesjs.usePlugin(baseBlocksPlugin, {
                flexGrid: true,
                ...(options.baseBlocksOptions || {})
            }),
            ...officialPlugins,
            ...externalPlugins,
            ...externalPluginFunctions,
            grapesjs.usePlugin(cokerCorePlugin, {
                adapter
            })
        ]
    });

    return editor;
}

export { grapesjs };
