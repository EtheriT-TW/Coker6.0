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

    const externalPluginFunctions = [
        function legacyCoker6(editor) {
            window.CokerGrapesLegacyPlugins.coker6Plugin(editor, options);
        },

        function legacyCoker6Form(editor) {
            window.CokerGrapesLegacyPlugins.coker6FormPlugin(editor, {});
        },

        function legacySwiper(editor) {
            window.CokerGrapesLegacyPlugins.swiperPlugin(editor, {});
        }
    ];

    if (options.enableImageEditor !== false) {
        if (typeof window.CokerGrapesTuiImageEditorPlugin !== "function") {
            throw new Error("GrapesJS 圖片編輯模組尚未載入，無法建立畫布。");
        }

        externalPluginFunctions.push(window.CokerGrapesTuiImageEditorPlugin);
    }

    if (options.enableNewsletter) {
        if (typeof window.CokerGrapesNewsletterPlugin !== "function") {
            throw new Error("GrapesJS 電子報模組尚未載入，無法建立電子報畫布。");
        }

        externalPluginFunctions.push(window.CokerGrapesNewsletterPlugin);
    }

    const pendingCanvasProcessorClasses = new Set();
    let canvasProcessorDebounceTimer = null;

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

        externalPluginFunctions,
        officialPluginsOptions: {
            "grapesjs-table": {},
        },
        initOptions: {
            showOffsets: 1,
            noticeOnUnload: 0,
            protectedCss: "",

            selectorManager: {
                componentFirst: true,
            },

            assetManager: {
                custom: false,
                uploadFile: function (e, uploadDone) {
                    const sourceFiles = e.dataTransfer ? e.dataTransfer.files : e.target.files;
                    const files = Array.from(sourceFiles || []);
                    const maxFileSize = 10 * 1024 * 1024;
                    const extensionByMimeType = {
                        'image/gif': 'gif',
                        'image/jpeg': 'jpg',
                        'image/png': 'png',
                        'image/webp': 'webp'
                    };
                    const completeUpload = typeof uploadDone === "function"
                        ? uploadDone
                        : function () {};
                    const finishUpload = function (data, success) {
                        completeUpload({ data: data });
                        editor.trigger('coker:image-editor:upload:complete', {
                            success: success
                        });
                    };
                    const selectedComponent = typeof editor.getSelected === "function"
                        ? editor.getSelected()
                        : null;
                    const selectedSrc = selectedComponent && typeof selectedComponent.get === "function"
                        ? selectedComponent.get("src") || ""
                        : "";
                    const sourceAsset = editor.AssetManager.getAll().find(function (asset) {
                        return asset.get("src") === selectedSrc;
                    });
                    const sourceName = sourceAsset
                        ? sourceAsset.get("name") || ""
                        : "";

                    const getEditedFileStem = function () {
                        const sourcePath = (sourceName || selectedSrc || "image")
                            .split(/[?#]/)[0]
                            .replace(/\\/g, "/");
                        let fileName = sourcePath.split("/").pop() || "image";

                        try {
                            fileName = decodeURIComponent(fileName);
                        } catch (_) {
                            // Keep the original value when the path is not URI encoded.
                        }

                        return fileName
                            .replace(/\.[^.]+$/, "")
                            .replace(/-edited-\d+(?:-\d+)?$/, "") || "image";
                    };

                    if (!files.length) {
                        finishUpload([], false);
                        return;
                    }

                    if (files.some(file => file.size > maxFileSize)) {
                        co.sweet.error("錯誤", "圖片編輯結果不可超過 10 MB", null, false);
                        finishUpload([], false);
                        return;
                    }

                    var formData = new FormData();

                    files.forEach(function (file, index) {
                        const hasExtension = typeof file.name === "string" && /\.[a-z0-9]{1,10}$/i.test(file.name);
                        const contentType = file.type || 'image/png';
                        const extension = extensionByMimeType[contentType] || 'png';
                        const uploadFile = hasExtension
                            ? file
                            : new File(
                                [file],
                                `${getEditedFileStem()}-edited-${Date.now()}-${index}.${extension}`,
                                { type: contentType, lastModified: Date.now() }
                            );

                        formData.append('files', uploadFile, uploadFile.name);
                    });

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
                            finishUpload(myJSON, myJSON.length > 0);
                        } else {
                            finishUpload([], false);

                            if (result.errorFiles && result.errorFiles[0] == "Type Error") {
                                co.sweet.error("錯誤", "不支援的檔案格式", null, false);
                            } else {
                                co.sweet.error("錯誤", "圖片上傳失敗", null, false);
                            }
                        }
                    }).fail(function () {
                        finishUpload([], false);
                        co.sweet.error("錯誤", "圖片上傳失敗", null, false);
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
                        $(obj.classes).each(function () {
                            pendingCanvasProcessorClasses.add(this.toString());
                        });

                        clearTimeout(canvasProcessorDebounceTimer);
                        canvasProcessorDebounceTimer = setTimeout(function () {
                            const processorClasses = Array.from(pendingCanvasProcessorClasses);
                            pendingCanvasProcessorClasses.clear();

                        const isrun = false;
                        let timer = null;

                        const waitIframeReady = (cb, retryCount) => {
                            retryCount = retryCount || 0;

                            if (retryCount >= 50) {
                                console.warn('[GrapesJS] 畫布 iframe 等待逾時，略過本次元件初始化。');
                                return;
                            }

                            const iframeEl = editor &&
                                editor.Canvas &&
                                typeof editor.Canvas.getFrameEl === "function"
                                    ? editor.Canvas.getFrameEl()
                                    : null;

                            if (!iframeEl) {
                                return setTimeout(() => waitIframeReady(cb, retryCount + 1), 100);
                            }

                            const iframe = iframeEl.contentWindow;

                            if (iframe.document.readyState !== "complete") {
                                return setTimeout(() => waitIframeReady(cb, retryCount + 1), 100);
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

                            const init = function (retryCount) {
                                if (typeof (iframe.jqueryExtend) != "undefined" && typeof (iframe.local) != "undefined") {
                                    iframe.jqueryExtend();
                                } else if (retryCount < 50) {
                                    timer = setTimeout(function () {
                                        init(retryCount + 1);
                                    }, 100);
                                } else {
                                    console.warn('[GrapesJS] jqueryExtend 等待逾時，略過本次初始化。');
                                }
                            };

                            timer = setTimeout(function () {
                                init(0);
                            }, 100);

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

                            $(processorClasses).each(function () {
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

                            const checkEle = function (retryCount) {
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

                                if (!runAll && retryCount < 20) {
                                    setTimeout(function () {
                                        checkEle(retryCount + 1);
                                    }, 300);
                                } else if (!runAll) {
                                    console.warn('[GrapesJS] 畫布元件初始化等待逾時，停止本次重試。');
                                }
                            };

                            setTimeout(function () {
                                checkEle(0);
                            }, 300);
                        });
                        }, 0);
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
