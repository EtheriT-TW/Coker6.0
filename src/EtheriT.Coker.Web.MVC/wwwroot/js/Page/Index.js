function PageReady() {
    var editor = grapesjs.init({
        showOffsets: 1,
        noticeOnUnload: 0,
        container: '#gjs',
        plugins: [
            'gjs-blocks-basic',
            'grapesjs-preset-webpage',
            "grapesjs-style-bg",
            'grapesjs-tabs',
            'grapesjs-custom-code',
            'grapesjs-tui-image-editor',
            'grapesjs-blocks-table'
        ],
        pluginsOpts: {
            'gjs-blocks-basic': { flexGrid: true },
            'grapesjs-preset-webpage': {
                modalImportTitle: 'Import Template',
                modalImportLabel: '<div style="margin-bottom: 10px; font-size: 13px;">Paste here your HTML/CSS and click Import</div>',
                modalImportContent: function (editor) {
                    return editor.getHtml() + '<style>' + editor.getCss() + '</style>'
                },
            },
            'grapesjs-tabs': {
                tabsBlock: { category: 'Extra' }
            },
            'grapesjs-tui-image-editor': {
                script: [
                    // 'https://cdnjs.cloudflare.com/ajax/libs/fabric.js/1.6.7/fabric.min.js',
                    'https://uicdn.toast.com/tui.code-snippet/v1.5.2/tui-code-snippet.min.js',
                    'https://uicdn.toast.com/tui-color-picker/v2.2.7/tui-color-picker.min.js',
                    'https://uicdn.toast.com/tui-image-editor/v3.15.2/tui-image-editor.min.js'
                ],
                style: [
                    'https://uicdn.toast.com/tui-color-picker/v2.2.7/tui-color-picker.min.css',
                    'https://uicdn.toast.com/tui-image-editor/v3.15.2/tui-image-editor.min.css',
                ],
            },
            'grapesjs-blocks-table': { containerId: '#gjs' }
        },
        canvas: {
            styles: [
                '/lib/bootstrap/dist/css/bootstrap.min.css',
                '/lib/swiper/swiper-bundle.min.css'
            ],
            scripts: [
                '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
                '/lib/swiper/swiper-bundle.min.js'
            ],
        },
        fromElement: true,
        storageManager: { autoload: 0 }
    });

    editor.BlockManager.add('testBlock', {
        category:"自定區",
        label: '歡迎區',
        attributes: { class: 'gjs-fonts gjs-f-b1', title: 'hello' },
        content: '<div style="text-align:center"><span>Hello World</span></div>'
    })
}