import {
    copyFileSync,
    existsSync,
    mkdirSync,
    readFileSync,
    readdirSync,
    writeFileSync
} from 'node:fs';
import { resolve } from 'node:path';
import { minify } from 'terser';

const packageRoot = resolve(import.meta.dirname, '..');
const distDir = resolve(packageRoot, 'dist');

const sourceFile = resolve(
    distDir,
    'etherit-coker-grapesjs.js'
);
const sourceCssFile = resolve(
    distDir,
    'etherit-coker-grapesjs.css'
);
const sourceNewsletterFile = resolve(
    distDir,
    'etherit-coker-grapesjs-newsletter.js'
);
const sourceImageEditorFile = resolve(
    distDir,
    'etherit-coker-grapesjs-image-editor.js'
);
const sourceImageEditorCssFile = resolve(
    packageRoot,
    'node_modules',
    'tui-image-editor',
    'dist',
    'tui-image-editor.min.css'
);
const sourceImageEditorCustomCssFile = resolve(
    packageRoot,
    'src',
    'styles',
    'imageEditor.css'
);

const targetDir = resolve(
    packageRoot,
    '..',
    '..',
    'src',
    'EtheriT.Coker.Web.MVC',
    'wwwroot',
    'modules',
    'grapesjs'
);

const targetFile = resolve(
    targetDir,
    'etherit-coker-grapesjs.min.js'
);
const targetCssFile = resolve(
    targetDir,
    'etherit-coker-grapesjs.min.css'
);
const targetNewsletterFile = resolve(
    targetDir,
    'etherit-coker-grapesjs-newsletter.min.js'
);
const targetImageEditorFile = resolve(
    targetDir,
    'etherit-coker-grapesjs-image-editor.min.js'
);
const targetImageEditorCssFile = resolve(
    targetDir,
    'etherit-coker-grapesjs-image-editor.min.css'
);

if (!existsSync(sourceFile)) {
    throw new Error(`[copy-to-mvc] Source file not found: ${sourceFile}`);
}

if (!existsSync(sourceCssFile)) {
    throw new Error(`[copy-to-mvc] CSS source file not found: ${sourceCssFile}`);
}

if (!existsSync(sourceNewsletterFile)) {
    throw new Error(`[copy-to-mvc] Newsletter source file not found: ${sourceNewsletterFile}`);
}

if (!existsSync(sourceImageEditorFile)) {
    throw new Error(`[copy-to-mvc] Image editor source file not found: ${sourceImageEditorFile}`);
}

if (!existsSync(sourceImageEditorCssFile)) {
    throw new Error(`[copy-to-mvc] Image editor CSS source file not found: ${sourceImageEditorCssFile}`);
}

if (!existsSync(sourceImageEditorCustomCssFile)) {
    throw new Error(`[copy-to-mvc] Image editor custom CSS source file not found: ${sourceImageEditorCustomCssFile}`);
}

const sourceCode = readFileSync(sourceFile, 'utf8');

const result = await minify(sourceCode, {
    module: true,
    compress: true,
    mangle: true,
    format: {
        comments: false
    }
});

const newsletterResult = await minify(readFileSync(sourceNewsletterFile, 'utf8'), {
    module: true,
    compress: true,
    mangle: true,
    format: {
        comments: false
    }
});

const imageEditorResult = await minify(readFileSync(sourceImageEditorFile, 'utf8'), {
    module: true,
    compress: true,
    mangle: true,
    format: {
        comments: false
    }
});

if (!result.code) {
    throw new Error('[copy-to-mvc] Minify failed: output is empty.');
}

if (!newsletterResult.code) {
    throw new Error('[copy-to-mvc] Newsletter minify failed: output is empty.');
}

if (!imageEditorResult.code) {
    throw new Error('[copy-to-mvc] Image editor minify failed: output is empty.');
}

mkdirSync(targetDir, { recursive: true });
writeFileSync(targetFile, result.code, 'utf8');
writeFileSync(targetCssFile, readFileSync(sourceCssFile, 'utf8'), 'utf8');
writeFileSync(targetNewsletterFile, newsletterResult.code, 'utf8');
writeFileSync(targetImageEditorFile, imageEditorResult.code, 'utf8');
writeFileSync(
    targetImageEditorCssFile,
    `${readFileSync(sourceImageEditorCssFile, 'utf8')}\n${readFileSync(sourceImageEditorCustomCssFile, 'utf8')}`,
    'utf8'
);

const entryFileNames = new Set([
    'etherit-coker-grapesjs.js',
    'etherit-coker-grapesjs-newsletter.js',
    'etherit-coker-grapesjs-image-editor.js'
]);
const supportChunkNames = readdirSync(distDir)
    .filter(fileName => fileName.endsWith('.js') && !entryFileNames.has(fileName));

for (const fileName of supportChunkNames) {
    copyFileSync(resolve(distDir, fileName), resolve(targetDir, fileName));
    console.log(`[copy-to-mvc] Support chunk copied to ${resolve(targetDir, fileName)}`);
}

console.log(`[copy-to-mvc] Minified and copied to ${targetFile}`);
console.log(`[copy-to-mvc] Minified CSS copied to ${targetCssFile}`);
console.log(`[copy-to-mvc] Minified newsletter module copied to ${targetNewsletterFile}`);
console.log(`[copy-to-mvc] Minified image editor module copied to ${targetImageEditorFile}`);
console.log(`[copy-to-mvc] Minified image editor CSS copied to ${targetImageEditorCssFile}`);
