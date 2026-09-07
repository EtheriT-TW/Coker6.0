import { joditRtePlugin } from './joditRtePlugin.js';
import { nativeRtePlugin } from './nativeRtePlugin.js';
import { tinyMceRtePlugin } from './tinyMceRtePlugin.js';

export function richTextProviderPlugin(editor, options = {}) {
    const provider = String(options.provider || 'builtin').toLowerCase();

    switch (provider) {
        case 'builtin':
        case 'grapesjs':
        case 'none':
            return;

        case 'native':
        case 'coker':
            nativeRtePlugin(editor, options.native || {});
            return;

        case 'jodit':
            joditRtePlugin(editor, options.jodit || {});
            return;

        case 'tinymce':
            tinyMceRtePlugin(editor, options.tinymce || {});
            return;

        default:
            editor.log(
                `[EtheriT.Coker.GrapesJS] 不支援的 RTE provider: ${provider}，改用 GrapesJS 內建 RTE。`,
                { ns: 'rich-text-provider', level: 'warning' }
            );
    }
}
