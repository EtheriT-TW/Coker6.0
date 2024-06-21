/****************************************
 * obj.save 內容儲存
 * obj.import 內容發布
 ****************************************/
var grapesInit = function (options) {
    const insertData = {
        css: [
            '/lib/bootstrap/dist/css/bootstrap.min.css',
            '/lib/swiper/swiper-bundle.min.css',
            '/lib/fortawesome/css/all.min.css',
            '/css/Grapes/GrapesCss.min.css',
            'https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200',
            '/Shared/shared.min.css',
            `/Layout/Default_Site.css`
        ],
        js: [
            '/lib/jquery/dist/jquery.min.js',
            '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
            '/lib/swiper/swiper-bundle.min.js',
            '/lib/masonry-layout/dist/masonry.pkgd.min.js',
            '/lib/jquery-plugin-c-share/dist/jquery.c-share.min.js',
            '/Shared/shared.min.js',
        ]
    }
    if (typeof (frameLevel) != "undefined" && frameLevel != null && frameLevel != 0) {
        insertData.css.push(`/Layout/Layout_${frameLevel}_Site.min.css`);
    }
    var editor = grapesjs.init({
        showOffsets: 1,
        noticeOnUnload: 0,
        container: '#gjs',
        height: '100vh',
        i18n: {
            locale: 'tw',
            localeFallback: 'tw',
        },
        selectorManager: {
            componentFirst: true,
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
                            myJSON.push({
                                path: this.path,
                                name: this.name,
                                guid: this.guid
                            });
                        });
                        var images = myJSON;
                        editor.AssetManager.add(images);
                    } else if (result.errorFiles[0] == "Type Error") {
                        co.sweet.error("錯誤", "不支援的檔案格式", null, false);
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
            //'grapesjs-tui-image-editor',
            'grapesjs-blocks-table',
            //'grapesjs-table',
            'grapesjs-parser-postcss',
            //'grapesjs-plugin-ckeditor',
            //'gjs-plugin-ckeditor5',
            //'grapesjs-rte-extensions',
            'grapesjs-Coker6'
        ],
        pluginsOpts: {
            'gjs-blocks-basic': { flexGrid: true },
            "grapesjs-table": {},
            'grapesjs-preset-webpage': {
                modalImportButton: '匯入',
                modalImportTitle: '匯入原始碼',
                modalImportLabel: '<div style="margin-bottom: 10px; font-size: 1rem;">請輸入您的原始碼</div>',
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
            'grapesjs-blocks-table': { containerId: '#gjs', componentCell:".test" },
            'grapesjs-Coker6': options,
            'grapesjs-preset-newsletter': {
                modalLabelExport: 'Copy the code and use it wherever you want',
                codeViewerTheme: 'material',
                cellStyle: {
                    'font-size': '1rem',
                    'font-weight': 300,
                    'vertical-align': 'top',
                    color: 'rgb(111, 119, 125)',
                    margin: 0,
                    padding: 0,
                }
            },
            'grapesjs-plugin-ckeditor': {
                onToolbar: el => {
                    el.style.minWidth = '350px';
                },
                ckeditor: "https://cdn.ckeditor.com/4.22.1/full-all/ckeditor.js",
                options: {
                    language: 'zh',
                    startupFocus: true,
                    extraAllowedContent: '*(*);*{*}', // Allows any class and any inline style
                    allowedContent: true, // Disable auto-formatting, class removing, etc.
                    enterMode: 2, // CKEDITOR.ENTER_BR,
                    extraPlugins: 'sharedspace,justify,colorbutton,panelbutton,font',
                    removePlugins:'exportpdf',
                    fontSize_sizes: '0.8rem;1rem;1.2rem;1.5rem;2rem;2.5rem;3rem;',
                    colorButton_enableMore: true,
                    toolbar: [
                        { name: 'styles', items: ['Font', 'FontSize'] },
                        ['Bold', 'Italic', 'Underline', 'Strike'],
                        { name: 'paragraph', items: ['NumberedList', 'BulletedList'] },
                        { name: 'links', items: ['Link', 'Unlink'] },
                        { name: 'colors', items: ['TextColor', 'BGColor'] },
                    ],
                }
            },
            'gjs-plugin-ckeditor5': {
                position: 'left',
                options: {
                    trackChanges: {},
                    toolbar: {
                        items: [
                            '|',
                            'fontColor',
                            'fontSize',
                            'fontFamily',
                            'fontBackgroundColor',
                            'alignment',
                            'bold',
                            'italic',
                            'underline',
                            'strikethrough',
                            'link',
                            'bulletedList',
                            'numberedList',
                            'horizontalLine',
                            '|',
                            'outdent',
                            'indent',
                            '|',
                            'blockQuote',
                            'insertTable',
                            '|',
                            'undo',
                            'redo'
                        ]
                    },
                    language: 'zh',
                    fontSize: {
                        options: ['0.8rem', '1rem', '1.2rem', '1.5rem', '2rem', '2.5rem', '3rem']
                    },
                    table: {
                        contentToolbar: [
                            'tableColumn',
                            'tableRow',
                            'mergeTableCells',
                            'tableCellProperties',
                            'tableProperties'
                        ]
                    },
                    htmlSupport: {
                        allow: [
                            {
                                name: /.*/,
                                attributes: true,
                                classes: true,
                                styles: true
                            }
                        ]
                    },
                    licenseKey: ''
                }
            },
            'grapesjs-rte-extensions': {
                // default options
                base: {
                    bold: true,
                    italic: true,
                    underline: true,
                    strikethrough: true,
                    link: true,
                },
                //fonts: {
                //  fontName: ['font1',...,'fontn'],
                //  fontSize: true,
                //  //An array of strings representing colors
                //  fontColor: ['#fff',...],
                //  //An array of strings representing colors
                //  hilite: ['#fff',...],
                //}
                fonts: {
                    fontColor: true,
                    hilite: true,
                },
                format: {
                    //heading1: true,
                    heading2: true,
                    heading3: true,
                    heading4: false,
                    //heading5: false,
                    //heading6: false,
                    paragraph: true,
                    //quote: false,
                    clearFormatting: true,
                },
                subscriptSuperscript: false,//|true
                indentOutdent: false,//|true
                list: false,//|true
                align: true,//|true
                //actions: {
                //  copy: true,
                //  cut: true,
                //  paste: true,
                //  delete: true,
                //},
                actions: false,//|true
                undoredo: false,//|true
                extra: false,//|true
                darkColorPicker: true,//|false
                maxWidth: '600px'
            }
        },
        canvas: {
            styles: insertData.css,
            scripts: insertData.js,
        },
        domComponents: {
            processor: (obj) => {
                if (!!obj.classes) {
                    const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
                    const isrun = false;
                    let timer = null;
                    if (typeof (iframe.local) == "undefined") {
                        co.i18.getAll().done(function (result) {
                            iframe.local = result;
                        });
                    }
                    const init = function () {
                        if (typeof (iframe.jqueryExtend) != "undefined" && typeof (iframe.local) != "undefined" ) iframe.jqueryExtend();
                        else timer = setTimeout(init, 100);
                    }
                    timer = setTimeout(init, 100);
                    let checkClass = [
                        { key: "SwiperInit", state: false, run: true, class: [], parameter: { autoplay: false } },
                        { key: "FrameInit", state: false, run: true, class: [], parameter: {} },
                        { key: "ViewTypeChangeInit", state: false, run: true, class: [], parameter: {} },
                        { key: "SitemapInit", state: false, run: true, class: [], parameter: {} },
                        { key: "HoverEffectInit", state: false, run: true, class: [], parameter: {} },
                        { key: "DirectoryGetDataInit", state: false, run: true, class: [], parameter: {} },
                        { key: "LinkWithIconInit", state: false, run: true, class: [], parameter: {} },
                        { key: "AnchorPointInit", state: false, run: true, class: [], parameter: {} },
                        { key: "ShareBlockInit", state: false, run: true, class: [], parameter: {} }
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
                            case "five_swiper":
                            case "six_swiper":
                                setConfig(0, s);
                                checkClass[0].parameter.autoplay = false;
                                break;
                            case "masonry":
                                setConfig(1, s);
                                break;
                            case "frame":
                            case "type_change_frame":
                                setConfig(2, s);
                                break;
                            case "sitemap_hierarchical_frame":
                                setConfig(3, s);
                                break;
                            case "hover_mask":
                                setConfig(4, s);
                                break;
                            case "catalog_frame":
                            case "menu_directory":
                                setConfig(5, s);
                                break;
                            case "link_with_icon":
                                setConfig(6, s);
                                break
                            case "anchor_directory":
                            case "anchor_title":
                                setConfig(7, s);
                                break;
                            case "shareBlock":
                                setConfig(8, s);
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
