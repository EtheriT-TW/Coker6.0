import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
    build: {
        minify: false,
        sourcemap: false,
        lib: {
            entry: resolve(__dirname, 'src/index.js'),
            name: 'EtheriTCokerGrapesJS',
            fileName: 'etherit-coker-grapesjs',
            formats: ['es']
        }
    }
});