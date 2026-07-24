import grapesjs from 'grapesjs';
import { createEditorAdapter } from './createEditorAdapter.js';
import { baseBlocksPlugin } from '../plugins/baseBlocksPlugin.js';
import { cokerCorePlugin } from '../plugins/cokerCorePlugin.js';

export function createCokerGrapesEditor(options = {}) {
    const adapter = options.adapter || createEditorAdapter(options);

    const externalPlugins = options.externalPlugins || [];
    const externalPluginsOpts = options.externalPluginsOpts || {};

    const editor = grapesjs.init({
        ...options.initOptions,

        container: options.container || '#gjs',
        height: options.height || '100vh',
        fromElement: options.fromElement ?? true,

        storageManager: options.storageManager || {
            autoload: false
        },

        plugins: [
            baseBlocksPlugin,
            ...externalPlugins,
            cokerCorePlugin
        ],

        pluginsOpts: {
            [baseBlocksPlugin]: {
                flexGrid: true,
                ...(options.baseBlocksOptions || {})
            },
            ...externalPluginsOpts,
            [cokerCorePlugin]: {
                adapter
            }
        }
    });

    return editor;
}

export { grapesjs };