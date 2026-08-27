import * as presetWebpageModule from 'grapesjs-preset-webpage';
import * as styleBgModule from 'grapesjs-style-bg';
import * as tabsModule from 'grapesjs-tabs';
import * as customCodeModule from 'grapesjs-custom-code';
import * as blocksTableModule from 'grapesjs-blocks-table';
import * as parserPostcssModule from 'grapesjs-parser-postcss';

export const officialPluginIds = Object.freeze({
    presetWebpage: 'grapesjs-preset-webpage',
    styleBg: 'grapesjs-style-bg',
    tabs: 'grapesjs-tabs',
    customCode: 'grapesjs-custom-code',
    blocksTable: 'grapesjs-blocks-table',
    parserPostcss: 'grapesjs-parser-postcss'
});

function resolvePlugin(moduleValue, pluginId) {
    if (typeof moduleValue === 'function') {
        return moduleValue;
    }

    if (typeof moduleValue.default === 'function') {
        return moduleValue.default;
    }

    if (moduleValue.default && typeof moduleValue.default.default === 'function') {
        return moduleValue.default.default;
    }

    throw new Error(`[EtheriT.Coker.GrapesJS] ${pluginId} export is not a function.`);
}

const officialPlugins = [
    {
        id: officialPluginIds.presetWebpage,
        plugin: resolvePlugin(presetWebpageModule, officialPluginIds.presetWebpage),
        defaults: {
            modalImportButton: '匯入',
            modalImportTitle: '匯入原始碼',
            modalImportLabel: '<div style="margin-bottom: 10px; font-size: 1rem;">請輸入您的原始碼</div>',
            modalImportContent(editor) {
                return editor.getHtml() + '<style>' + editor.getCss() + '</style>';
            }
        }
    },
    {
        id: officialPluginIds.styleBg,
        plugin: resolvePlugin(styleBgModule, officialPluginIds.styleBg),
        defaults: {}
    },
    {
        id: officialPluginIds.tabs,
        plugin: resolvePlugin(tabsModule, officialPluginIds.tabs),
        defaults: {
            tabsBlock: { category: 'Extra' }
        }
    },
    {
        id: officialPluginIds.customCode,
        plugin: resolvePlugin(customCodeModule, officialPluginIds.customCode),
        defaults: {}
    },
    {
        id: officialPluginIds.blocksTable,
        plugin: resolvePlugin(blocksTableModule, officialPluginIds.blocksTable),
        defaults: {}
    },
    {
        id: officialPluginIds.parserPostcss,
        plugin: resolvePlugin(parserPostcssModule, officialPluginIds.parserPostcss),
        defaults: {}
    }
];

export function createOfficialPlugins(grapesjs, options = {}) {
    const container = options.container || '#gjs';
    const pluginOptions = options.pluginOptions || {};

    return officialPlugins.map(({ id, plugin, defaults }) => {
        const contextualDefaults = id === officialPluginIds.blocksTable
            ? { containerId: container, componentCell: '.test' }
            : {};

        return grapesjs.usePlugin(plugin, {
            ...defaults,
            ...contextualDefaults,
            ...(pluginOptions[id] || {})
        });
    });
}
