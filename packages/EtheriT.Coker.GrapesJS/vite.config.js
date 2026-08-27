import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
    build: {
        minify: false,
        cssMinify: true,
        sourcemap: false,
        lib: {
            entry: {
                main: resolve(__dirname, 'src/index.js'),
                newsletter: resolve(__dirname, 'src/newsletter.js'),
                imageEditor: resolve(__dirname, 'src/imageEditor.js')
            },
            name: 'EtheriTCokerGrapesJS',
            fileName: (format, entryName) => ({
                newsletter: 'etherit-coker-grapesjs-newsletter.js',
                imageEditor: 'etherit-coker-grapesjs-image-editor.js'
            })[entryName] || 'etherit-coker-grapesjs.js',
            cssFileName: 'etherit-coker-grapesjs',
            formats: ['es']
        }
    }
});
