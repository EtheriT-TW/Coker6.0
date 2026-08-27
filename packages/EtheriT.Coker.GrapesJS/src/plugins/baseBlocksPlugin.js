import * as blocksBasicModule from 'grapesjs-blocks-basic';

function resolvePlugin(moduleValue) {
    if (typeof moduleValue === 'function') {
        return moduleValue;
    }

    if (typeof moduleValue.default === 'function') {
        return moduleValue.default;
    }

    if (moduleValue.default && typeof moduleValue.default.default === 'function') {
        return moduleValue.default.default;
    }

    throw new Error('[EtheriT.Coker.GrapesJS] grapesjs-blocks-basic plugin export is not a function.');
}

const blocksBasic = resolvePlugin(blocksBasicModule);

export function baseBlocksPlugin(editor, options = {}) {
    blocksBasic(editor, options);
}