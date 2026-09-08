import { nativeRtePlugin } from './nativeRtePlugin.js';

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
        case 'tinymce':
            editor.log(
                `[EtheriT.Coker.GrapesJS] RTE provider ${provider} 未包含於目前的前端套件，改用 GrapesJS 內建 RTE。`,
                { ns: 'rich-text-provider', level: 'warning' }
            );
            return;

        default:
            editor.log(
                `[EtheriT.Coker.GrapesJS] 不支援的 RTE provider: ${provider}，改用 GrapesJS 內建 RTE。`,
                { ns: 'rich-text-provider', level: 'warning' }
            );
    }
}
