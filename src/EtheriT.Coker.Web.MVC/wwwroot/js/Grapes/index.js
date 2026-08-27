/****************************************
 * obj.save 內容儲存
 * obj.import 內容發布
 ****************************************/
var grapesInit = function (options) {
    options = options || {};
    const container = options.container || '#gjs';

    if (!window.EtheriTCokerGrapesJS ||
        typeof window.EtheriTCokerGrapesJS.createEditor !== "function") {
        throw new Error("GrapesJS Vite 模組尚未載入，無法建立畫布。");
    }

    const getCurrentPageId = function () {
        if (typeof options.getPageId === "function") {
            return Number(options.getPageId() || 0);
        }

        return Number($(container).data("id") || 0);
    };

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
            '/Shared/Coker.min.js',
            '/Shared/shared.min.js',
        ]
    };

    if (typeof (frameLevel) != "undefined" && frameLevel != null && frameLevel != 0) {
        insertData.css.push(`/Layout/Layout_${frameLevel}_Site.min.css`);
    }
    var editor = window.EtheriTCokerGrapesJS.createEditor({
        container: container,
        height: '100vh',
        fromElement: true,

        storageManager: {
            autoload: false
        },

        baseBlocksOptions: {
            flexGrid: true
        },

        externalPlugins: [
            'grapesjs-preset-webpage',
            'grapesjs-style-bg',
            'grapesjs-tabs',
            'grapesjs-custom-code',
            //'grapesjs-tui-image-editor',
            'grapesjs-blocks-table',
            //'grapesjs-table',
            'grapesjs-parser-postcss',
            //'grapesjs-plugin-ckeditor',
            //'gjs-plugin-ckeditor5',
            //'grapesjs-rte-extensions'
        ],
        externalPluginFunctions: [
            function legacyCoker6(editor) {
                window.CokerGrapesLegacyPlugins.coker6Plugin(editor, options);
            },

            function legacyCoker6Form(editor) {
                window.CokerGrapesLegacyPlugins.coker6FormPlugin(editor, {});
            },

            function legacySwiper(editor) {
                window.CokerGrapesLegacyPlugins.swiperPlugin(editor, {});
            }
        ],
        externalPluginsOpts: {
            "grapesjs-table": {},

            'grapesjs-preset-webpage': {
                modalImportButton: '匯入',
                modalImportTitle: '匯入原始碼',
                modalImportLabel: '<div style="margin-bottom: 10px; font-size: 1rem;">請輸入您的原始碼</div>',
                modalImportContent: function (editor) {
                    return editor.getHtml() + '<style>' + editor.getCss() + '</style>';
                },
            },

            'grapesjs-tabs': {
                tabsBlock: { category: 'Extra' }
            },

            'grapesjs-tui-image-editor': {
                script: [
                    //'https://cdnjs.cloudflare.com/ajax/libs/fabric.js/1.6.7/fabric.min.js',
                    '/lib/tui-code/js/tui-code-snippet.min.js',
                    '/lib/tui-code/js/tui-color-picker.min.js',
                    '/lib/tui-code/js/tui-image-editor.min.js'
                ],
                style: [
                    '/lib/tui-code/css/tui-color-picker.min.css',
                    '/lib/tui-code/css/tui-image-editor.min.css',
                ]
            },
            'grapesjs-blocks-table': {
                containerId: container,
                componentCell: ".test"
            },
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
                    extraAllowedContent: '*(*);*{*}',
                    allowedContent: true,
                    enterMode: 2,
                    extraPlugins: 'sharedspace,justify,colorbutton,panelbutton,font',
                    removePlugins: 'exportpdf',
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
                base: {
                    bold: true,
                    italic: true,
                    underline: true,
                    strikethrough: true,
                    link: true,
                },
                fonts: {
                    fontColor: true,
                    hilite: true,
                },
                format: {
                    heading2: true,
                    heading3: true,
                    heading4: false,
                    paragraph: true,
                    clearFormatting: true,
                },
                subscriptSuperscript: false,
                indentOutdent: false,
                list: false,
                align: true,
                actions: false,
                undoredo: false,
                extra: false,
                darkColorPicker: true,
                maxWidth: '600px'
            }
        },
        initOptions: {
            showOffsets: 1,
            noticeOnUnload: 0,
            protectedCss: "",

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
                        formData.append('files', files[i]);
                    }

                    formData.append("type", 0);

                    co.File.Upload(formData).done(function (result) {
                        if (result.success) {
                            var myJSON = [];

                            $(result.files).each(function () {
                                myJSON.push({
                                    src: this.path,
                                    name: this.name,
                                    guid: this.guid
                                });
                            });

                            editor.AssetManager.add(myJSON);
                        } else if (result.errorFiles[0] == "Type Error") {
                            co.sweet.error("錯誤", "不支援的檔案格式", null, false);
                        }
                    });
                }
            },

            canvas: {
                styles: insertData.css,
                scripts: insertData.js,
            },

            domComponents: {
                processor: (obj) => {
                    if (!!obj.classes) {
                        const isrun = false;
                        let timer = null;

                        const waitIframeReady = (cb) => {
                            const iframeEl = editor &&
                                editor.Canvas &&
                                typeof editor.Canvas.getFrameEl === "function"
                                    ? editor.Canvas.getFrameEl()
                                    : null;

                            if (!iframeEl) {
                                return setTimeout(() => waitIframeReady(cb), 100);
                            }

                            const iframe = iframeEl.contentWindow;

                            if (iframe.document.readyState !== "complete") {
                                return setTimeout(() => waitIframeReady(cb), 100);
                            }

                            cb(iframe);
                        };

                        waitIframeReady((iframe) => {
                            // Shared page components also run inside the GrapesJS canvas.
                            // Mark the canvas explicitly so preview rendering never records
                            // front-site activity (for example advertisement exposure).
                            iframe.CokerEditorMode = true;

                            if (typeof (iframe.local) == "undefined") {
                                iframe.local = {};

                                co.i18.getAll().done(function (result) {
                                    iframe.local = result;
                                });
                            }

                            iframe.OrgName = typeof OrgName === "undefined" ? "" : OrgName;

                            const init = function () {
                                if (typeof (iframe.jqueryExtend) != "undefined" && typeof (iframe.local) != "undefined") {
                                    iframe.jqueryExtend();
                                } else {
                                    timer = setTimeout(init, 100);
                                }
                            };

                            timer = setTimeout(init, 100);

                            let checkClass = [
                                { key: "SwiperInit", state: false, run: true, class: [], parameter: { autoplay: false } },
                                { key: "FrameInit", state: false, run: true, class: [], parameter: {} },
                                { key: "ViewTypeChangeInit", state: false, run: true, class: [], parameter: {} },
                                { key: "SitemapInit", state: false, run: true, class: [], parameter: {} },
                                { key: "HoverEffectInit", state: false, run: true, class: [], parameter: {} },
                                { key: "DirectoryGetDataInit", state: false, run: true, class: [], parameter: null },
                                { key: "LinkWithIconInit", state: false, run: true, class: [], parameter: {} },
                                { key: "AnchorPointInit", state: false, run: true, class: [], parameter: {} },
                                { key: "ShareBlockInit", state: false, run: true, class: [], parameter: {} },
                                { key: "GetLatLng", state: false, run: true, class: [], parameter: {} },
                                { key: "ArticleTagsInit", state: false, run: true, class: [], parameter: {} },
                            ];

                            const setConfig = function (index, str) {
                                checkClass[index].state = true;
                                checkClass[index].run = false;
                                checkClass[index].class.push(`.${str}`);
                            };

                            $(obj.classes).each(function () {
                                var s = this.toString();

                                switch (s) {
                                    case "swiper_components":
                                    case "one_swiper":
                                    case "one_swiper_thumbs":
                                    case "two_swiper":
                                    case "three_swiper":
                                    case "four_swiper":
                                    case "five_swiper":
                                    case "six_swiper":
                                    case "three_two_grid_swiper":
                                    case "vertical_swiper_thumbs":
                                        setConfig(0, s);
                                        checkClass[0].parameter.autoplay = false;
                                        break;

                                    case "masonry":
                                    case "YTmodal_frame":
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
                                    case "advertise_directory":
                                        setConfig(5, s);
                                        break;

                                    case "link_with_icon":
                                        setConfig(6, s);
                                        break;

                                    case "anchor_directory":
                                    case "anchor_title":
                                        setConfig(7, s);
                                        break;

                                    case "shareBlock":
                                        setConfig(8, s);
                                        break;

                                    case "getlatlng":
                                        setConfig(9, s);
                                        break;

                                    case "article-tags":
                                        setConfig(10, s);
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

                                            if (iframe.$(str).length == 0) {
                                                c = false;
                                            }
                                        });

                                        if (c) {
                                            if (item.key === "ArticleTagsInit") {
                                                item.parameter = {
                                                    pageId: getCurrentPageId()
                                                };
                                            }

                                            if (typeof iframe[item.key] === "function") {
                                                iframe[item.key](item.parameter);
                                                item.run = true;
                                            } else {
                                                item.run = false;
                                            }
                                        }
                                    }

                                    runAll = runAll && this.run;
                                });

                                if (!runAll) {
                                    setTimeout(checkEle, 300);
                                }
                            };

                            setTimeout(checkEle, 300);
                        });
                    }
                }
            }
        }
    });

    return editor;
};

/**
 * 共用畫布實例管理器。
 * Article/Product/Menu 只提供各自的讀寫 callback，不重複 GrapesJS 初始化。
 */
var GrapesEditorManager = (function () {
    const instances = {};

    function create(key, options) {
        if (!key) throw new Error("建立畫布時必須提供唯一 key。");
        if (instances[key]) return instances[key];

        instances[key] = grapesInit(options || {});
        return instances[key];
    }

    function get(key) {
        return instances[key] || null;
    }

    function destroy(key) {
        const editor = instances[key];
        if (!editor) return;
        if (typeof editor.destroy === "function") editor.destroy();
        delete instances[key];
    }

    return {
        create: create,
        get: get,
        destroy: destroy
    };
})();
