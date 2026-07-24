import { mkdirSync, readFileSync, writeFileSync, existsSync } from 'node:fs';
import { resolve } from 'node:path';
import { minify } from 'terser';

const packageRoot = resolve(import.meta.dirname, '..');

const sourceFile = resolve(
    packageRoot,
    'dist',
    'etherit-coker-grapesjs.js'
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

if (!existsSync(sourceFile)) {
    throw new Error(`[copy-to-mvc] Source file not found: ${sourceFile}`);
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

if (!result.code) {
    throw new Error('[copy-to-mvc] Minify failed: output is empty.');
}

mkdirSync(targetDir, { recursive: true });
writeFileSync(targetFile, result.code, 'utf8');

console.log(`[copy-to-mvc] Minified and copied to ${targetFile}`);