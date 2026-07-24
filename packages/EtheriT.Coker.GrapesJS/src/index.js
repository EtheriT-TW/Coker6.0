import { createEditorAdapter } from './core/createEditorAdapter.js';

export { createEditorAdapter };

export function createCokerGrapesLibrary(options = {}) {
    const adapter = options.adapter || createEditorAdapter(options);

    return {
        name: 'EtheriT.Coker.GrapesJS',
        version: '0.0.0',
        adapter
    };
}