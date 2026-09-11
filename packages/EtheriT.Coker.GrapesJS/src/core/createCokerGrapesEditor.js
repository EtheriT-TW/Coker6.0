import grapesjs from 'grapesjs';
import { createEditorAdapter } from './createEditorAdapter.js';
import { baseBlocksPlugin } from '../plugins/baseBlocksPlugin.js';
import { cokerCorePlugin } from '../plugins/cokerCorePlugin.js';
import { swiperPlugin } from '../plugins/swiper/swiperPlugin.js';
import { componentInsertPlugin } from '../plugins/componentInsert/Index.js';
import { faqComponentPlugin } from '../plugins/faq/faqComponentPlugin.js';
import { linkComponentPlugin } from '../plugins/link/linkComponentPlugin.js';
import { fileComponentPlugin } from '../plugins/file/fileComponentPlugin.js';
import { richTextProviderPlugin } from '../plugins/richText/Index.js';
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
        // Frontend output must not depend on style attributes. GrapesJS stores
        // component styles as #id CSS rules and getHtml() omits inline styles.
        avoidInlineStyle: true,

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
            grapesjs.usePlugin(cokerCorePlugin, {
                adapter,
                componentOutlinesOnLoad: true,
                ...(options.cokerCoreOptions || {})
            }),
            grapesjs.usePlugin(faqComponentPlugin),
            grapesjs.usePlugin(linkComponentPlugin),
            grapesjs.usePlugin(richTextProviderPlugin, {
                ...(options.richTextOptions || {}),
                jodit: {
                    ...(options.joditRteOptions || {}),
                    ...(options.richTextOptions?.jodit || {})
                },
                tinymce: {
                    ...(options.tinyMceRteOptions || {}),
                    ...(options.richTextOptions?.tinymce || {})
                }
            }),
            ...externalPlugins,
            ...externalPluginFunctions,
            // Register after legacy plugins while the migration is in progress,
            // so Vite owns the generic file components even with an old Coker6
            // bundle still present on the page.
            grapesjs.usePlugin(fileComponentPlugin),
            grapesjs.usePlugin(swiperPlugin, {
                adapter
            }),
            grapesjs.usePlugin(componentInsertPlugin, {
                adapter
            })
        ]
    });

    return editor;
}

export { grapesjs };
