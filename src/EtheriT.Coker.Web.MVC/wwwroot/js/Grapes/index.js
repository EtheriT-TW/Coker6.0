/****************************************
 * obj.save 內容儲存
 * obj.import 內容發布
 ****************************************/
var grapesInit = function (options) {
    var editor = grapesjs.init({
        showOffsets: 1,
        noticeOnUnload: 0,
        container: '#gjs',
        i18n: {
            locale: 'tw',
            localeFallback: 'tw',
        },
        assetManager: {
            custom: false,
            uploadFile: function (e) {
                var files = e.dataTransfer ? e.dataTransfer.files : e.target.files;
                var formData = new FormData();
                for (var i in files) {
                    formData.append('files', files[i]) //containing all the selected images from local
                }
                formData.append("type", 0);
                co.File.Upload(formData).done(function (result) {
                    if (result.success) {
                        var myJSON = [];
                        $(result.files).each(function (index) {
                            myJSON.push(this.path);
                        });
                        var images = myJSON;
                        editor.AssetManager.add(images);
                    }
                });
            }
        },
        plugins: [
            'gjs-blocks-basic',
            'grapesjs-preset-webpage',
            "grapesjs-style-bg",
            'grapesjs-tabs',
            'grapesjs-custom-code',
            'grapesjs-tui-image-editor',
            'grapesjs-blocks-table',
            'grapesjs-parser-postcss',
            'grapesjs-Coker6'
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
                    //'https://cdnjs.cloudflare.com/ajax/libs/fabric.js/1.6.7/fabric.min.js',
                    '/lib/tui-code/js/tui-code-snippet.min.js', //v1.5.2
                    '/lib/tui-code/js/tui-color-picker.min.js', //v2.2.7
                    '/lib/tui-code/js/tui-image-editor.min.js' //v3.15.2
                ],
                style: [
                    '/lib/tui-code/css/tui-color-picker.min.css', //v2.2.7
                    '/lib/tui-code/css/tui-image-editor.min.css', // v3.15.2
                ]
            },
            'grapesjs-blocks-table': { containerId: '#gjs' },
            'grapesjs-Coker6': options
        },
        canvas: {
            styles: [
                '/lib/bootstrap/dist/css/bootstrap.min.css',
                '/lib/swiper/swiper-bundle.min.css',
                '/css/Grapes/GrapesCss.min.css',
                'https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200',
                '/shared/css/Frame.min.css',
                '/shared/css/HoverEffect.min.css',
                '/shared/css/Swiper.min.css'
            ],
            scripts: [
                '/lib/jquery/dist/jquery.min.js',
                '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
                '/lib/swiper/swiper-bundle.min.js',
                '/lib/masonry-layout/dist/masonry.pkgd.min.js',
                '/shared/js/Frame.min.js',
                '/shared/js/Swiper.min.js',
                '/shared/js/ViewTypeChange.min.js',
                '/shared/js/Sitemap.min.js'
            ],
        },
        domComponents: {
            processor: (obj) => {                
                if (!!obj.classes) {
                    const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
                    let checkClass = [
                        { key: "SwiperInit", state: false, run: true, class: [], parameter: {} },
                        { key: "FrameInit", state: false, run: true, class: [], parameter: {} },
                        { key: "ViewTypeChangeInit", state: false, run: true, class: [], parameter: {} },
                        { key: "SitemapInit", state: false, run: true, class: [], parameter: {} }
                    ];
                    const setConfig = function (index, str) {
                        checkClass[index].state = true;
                        checkClass[index].run = false;
                        checkClass[index].class.push(`.${str}`);
                    }
                    $(obj.classes).each(function () {
                        var s = this.toString();
                        switch (s) {
                            case "one_swiper":
                            case "two_swiper":
                            case "four_swiper":
                                setConfig(0, s);
                                checkClass[0].parameter.autoplay = false;
                                break;
                            case "masonry":
                                setConfig(1, s);
                                break;
                            case "frame":
                                setConfig(2, s);
                                break;
                            case "sitemap_hierarchical_frame":
                                setConfig(3, s);
                                break;
                        }
                    });
                    const checkEle = function () {
                        var runAll = true;
                        $(checkClass).each(function () {
                            var item = this;
                            if (item.state) {
                                let c = true;
                                $(item.class).each(function () {
                                    var str = this;
                                    if (iframe.$(str).length == 0) c = false;
                                });
                                if (c) {
                                    iframe[item.key](item.parameter);
                                    item.run = true;
                                }
                            }
                            runAll = runAll && this.run
                        });
                        if (!runAll) setTimeout(checkEle, 300);
                    }
                    setTimeout(checkEle, 300);
                }
            }
        },
        fromElement: true,
        storageManager: { autoload: false }
    });

    return editor;
}