const path = require('path');
const { spawnSync } = require('child_process');

const grapesJsDirectory = path.resolve(__dirname, '..');
const repositoryDirectory = path.resolve(grapesJsDirectory, '..', '..');
const mvcDirectory = path.join(
    repositoryDirectory,
    'src',
    'EtheriT.Coker.Web.MVC'
);
const npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm';

function assertSupportedNodeVersion() {
    const [major, minor] = process.versions.node.split('.').map(Number);
    const isSupported =
        (major === 20 && minor >= 19) ||
        (major === 22 && minor >= 12) ||
        major > 22;

    if (!isSupported) {
        throw new Error(
            `目前 Node.js 為 ${process.version}，Vite 8 需要 Node.js 20.19+ 或 22.12+。`
        );
    }
}

function runNpm(directory, args, description) {
    console.log(`\n=== ${description} ===`);
    console.log(`目錄：${directory}`);
    console.log(`指令：npm ${args.join(' ')}`);

    const result = spawnSync(npmCommand, args, {
        cwd: directory,
        env: {
            ...process.env,
            NODE_ENV: ''
        },
        stdio: 'inherit'
    });

    if (result.error) {
        throw result.error;
    }

    if (result.status !== 0) {
        throw new Error(`${description}失敗，npm 結束代碼：${result.status}`);
    }
}

function main() {
    assertSupportedNodeVersion();

    runNpm(
        grapesJsDirectory,
        ['ci', '--include=dev'],
        '安裝 GrapesJS／Vite 依賴'
    );
    runNpm(
        grapesJsDirectory,
        ['run', 'build:mvc'],
        '建置 GrapesJS 並發布至 MVC'
    );
    runNpm(
        mvcDirectory,
        ['ci', '--include=dev'],
        '安裝 MVC 前端依賴'
    );
    runNpm(
        mvcDirectory,
        ['run', 'create-bundles'],
        '產生 MVC bundles'
    );

    console.log('\nGrapesJS 初始化與 MVC 前端發布完成。');
}

try {
    main();
} catch (error) {
    console.error(`\n初始化失敗：${error.message}`);
    process.exitCode = 1;
}
