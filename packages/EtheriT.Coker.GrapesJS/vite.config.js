import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
    build: {
        lib: {
            entry: resolve(__dirname, 'src/index.js'),
            name: 'EtheriTCokerGrapesJS',
            fileName: 'etherit-coker-grapesjs.min',
            formats: ['es']
        }
    }
});