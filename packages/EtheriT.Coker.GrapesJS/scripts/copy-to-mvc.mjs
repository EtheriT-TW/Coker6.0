import { mkdirSync, copyFileSync, existsSync } from 'node:fs';
import { resolve } from 'node:path';

const packageRoot = resolve(import.meta.dirname, '..');

const sourceFile = resolve(
    packageRoot,
    'dist',
    'etherit-coker-grapesjs.min.js'
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

mkdirSync(targetDir, { recursive: true });
copyFileSync(sourceFile, targetFile);

console.log(`[copy-to-mvc] Copied to ${targetFile}`);