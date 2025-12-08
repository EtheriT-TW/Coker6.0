var $gjs_select = null;

grapesjs.plugins.add('grapesjs-Coker6', (editor, options) => {
    let settings = {
        save: function () { return false; },
        import: function () { return false; },
        getComponer: function () { return false; },
        asset: [],
        iconPickerOpt: { cols: 4, rows: 4, footer: false, iconset: "GoogleMaterialSymbolsOutlined" }
    };
    $.extend(true, settings, options);
    editor.settings = settings;
    const AssetManager = editor.AssetManager;
    const categories = editor.BlockManager.getCategories();
    const BlockManager = editor.BlockManager;
    const panelManager = editor.Panels;

    //設定UI文字
    editor.I18n.setMessages({ tw: tw });

    /*檔案管理*/
    AssetManager.addType('image', {
        view: {
            attributes: { 'Guid': 'custom-value' }
        }
    });

    // Wait for CKEDITOR load
    setInterval(() => {
        if (typeof (CKEDITOR) != "undefined") {
            CKEDITOR.dtd.$editable.a = 1;
            CKEDITOR.dtd.$editable.p = 1;
            CKEDITOR.dtd.$editable.span = 1;
            CKEDITOR.dtd.$editable.li = 1;
            CKEDITOR.dtd.$editable.strong = 1;
            CKEDITOR.dtd.$editable.div = 1;
            CKEDITOR.dtd.$removeEmpty.span = 0;
            CKEDITOR.dtd.$removeEmpty.i = 0;
        }
    }, 200);

    editor.on("run:modal-open:modalckeditor", function () {
        //console.log("run:modal-open:modalckeditor");
    });

    editor.on('asset:add', (option) => {
        //console.log("asset:add");
    });

    editor.on('run:open-assets', () => {
        //console.log("run:open-assets");
        const modal = editor.Modal;
        const modalBody = modal.getContentEl();
        const uploader = modalBody.querySelector('.gjs-am-file-uploader');
        const assetsHeader = modalBody.querySelector('.gjs-am-assets-header');
        const assetsBody = modalBody.querySelector('.gjs-am-assets-cont');
        const assetsFilter = modalBody.querySelector('.gjs-am-assets-filter');
        if (!!!assetsFilter) {
            const filter = $(`
                <div class="input-group mb-3 gjs-am-assets-filter">
                    <input type="text" class="form-control" placeholder="搜尋檔案名稱" aria-label="搜尋檔案名稱" aria-describedby="button-addon2">
                    <button class="btn btn-outline-secondary" type="button" id="button-addon2">
                        <span class="material-symbols-outlined">search</span>
                    </button>
                </div>
            `);
            filter.find(".btn").on("click", () => {
                const search = filter.children('[type="text"]').val();
                if (search == "") $(".gjs-am-asset-image").removeClass("d-none");
                else {
                    $(".gjs-am-asset-image").each(function () {
                        const $image = $(this);
                        const imageName = $image.find(".gjs-am-dimensions").text();
                        if (imageName.indexOf(search) < 0) $image.addClass("d-none");
                        else $image.removeClass("d-none");
                    });
                }
            });
            assetsHeader.style.display = 'none'
            assetsBody.insertBefore(filter[0], assetsHeader);
        }
        AssetManager.onSelect((result) => {
            //console.log("result", result)
            var name = result.attributes.name;
            $gjs_select.addAttributes({ alt: name.substring(0, name.lastIndexOf(".")) });
            $gjs_select = null;
        });
    });

    //檔案刪除
    editor.on("asset:remove", (asset) => {
        const guid = asset.get('guid');
        co.File.Delete(guid).done(function (result) {
            console.log(result);
        });
    });

    //元件參數設定
    /***********************************************************************
     * 注意事項
     * 元件名稱需為小寫，否則欄位會無法對應
     * ********************************************************************* */
    /*元件浮動控制項*/
    const commands = editor.Commands;
    commands.add('tlb-settime', editor => {
        myModal.show();
    });

    editor.on('component:selected', () => {
        //console.log("component:selected")
        // whenever a component is selected in the editor

        // set your command and icon here
        const commandToAdd = 'tlb-settime';
        const commandIcon = 'fa fa-star';

        // get the selected componnet and its default toolbar
        const selectedComponent = editor.getSelected();
        $gjs_select = editor.getSelected();
        const defaultToolbar = selectedComponent.get('toolbar');

        // check if this command already exists on this component toolbar
        const commandExists = defaultToolbar.some(item => item.command === commandToAdd);

        // if it doesn't already exist, add it
        if (!commandExists) {
            selectedComponent.set({
                toolbar: [...defaultToolbar, { attributes: { class: commandIcon }, command: commandToAdd }]
            });
        }
    });

    editor.DomComponents.addType('image', {
        isComponent: el => el.tagName == 'IMG',
        model: {
            defaults: {
                traits: [
                    { name: 'alt', type: 'text', label: '圖片名稱(Alt)', placeholder: '請輸入圖片名稱' }
                ]
            },
            init() { }
        }
    });

    editor.DomComponents.addType('計時器', {
        isComponent: el => el.classList?.contains('flipdown') && el.hasAttribute('data-timer'),
        model: {
            defaults: {
                tagName: 'div',
                classes: ['flipdown'],
                attributes: {
                    'data-timer': '2026-01-01T00:00:00',
                    'data-theme': 'dark'
                },
                traits: [
                    {
                        name: 'data-timer',
                        type: 'datetime-local',
                        label: '倒數結束時間'
                    },
                    {
                        name: 'data-theme',
                        type: 'select',
                        label: '主題',
                        options: [
                            { id: 'dark', name: '深色 (dark)' },
                            { id: 'light', name: '亮色 (light)' }
                        ]
                    }
                ],
                script: function () {
                    if (typeof FlipTimer === 'function') {
                        FlipTimer();
                    }
                },
                content: '倒數計時器區塊'
            },
            init() {
                this.on('change:attributes:data-timer', this.reInitTimer);
                this.on('change:attributes:data-theme', this.reInitTimer);
            },
            reInitTimer() {
                const iframeWin = this.view?.canvas?.getFrameEl()?.contentWindow;
                if (iframeWin?.FlipTimer) {
                    iframeWin.FlipTimer();
                }
            }
        },
    });

    editor.TraitManager.addType('datetime-local', {
        createInput({ trait }) {
            const el = document.createElement('input');
            el.type = 'datetime-local';
            el.value = this.target.getAttributes()[trait.name] || '';
            el.addEventListener('change', () => {
                this.target.addAttributes({ [trait.name]: el.value });
            });
            return el;
        },
        onEvent({ elInput, component, event }) {
            component.addAttributes({ [this.model.get('name')]: elInput.value });
        }
    });

    /*連結 */
    editor.DomComponents.addType('連結', {
        isComponent: el => el.tagName == 'A',
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'title', type: 'text', label: '名稱', placeholder: '請輸入連結名稱' },
                    { name: 'data-text', type: 'text', label: '顯示文字', placeholder: '請輸入顯示文字' },
                    { name: 'href', type: 'text', label: '超連結', placeholder: '請輸入連結位子' },
                    {
                        name: 'file', type: 'button',
                        text: "選擇檔案",
                        command: editor => {
                            AssetManager.open();
                            AssetManager.onSelect((resule) => {
                                editor.getSelected().set("attributes", { "href": resule.id });
                                AssetManager.close();
                            });
                        },
                    },
                    {
                        name: 'target', type: 'select', label: '開啟方式',
                        options: [
                            { id: '_self', name: '直接連結' },
                            { id: '_blank', name: '另開視窗', label: '另開視窗' }
                        ]
                    }
                ]
            }, init() {
                this.on('change:attributes:data-text', function (component) {
                    if (typeof (component.getEl()) != "undefined") {
                        if (component.find(".name").length > 0)
                            component.find(".name")[0].components(component.getAttributes()["data-text"]);
                        else
                            component.components(component.getAttributes()["data-text"]);
                    }
                });
            }
        },
    });
    
    editor.DomComponents.addType('電子書', {
        isComponent: el => el.classList?.contains('FlipBookItem'),
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'title', type: 'text', label: '檔案名稱', placeholder: '點選下方按鈕選擇檔案' },
                    { name: 'data-pdf-url', type: 'text', label: '連結', placeholder: '請輸入電子書路徑' },
                    {
                        name: 'file', type: 'button',
                        text: "選擇檔案",
                        command: editor => {
                            AssetManager.open({
                                types: ['pdf'],  // 允許 PDF 類型
                                accept: 'application/pdf', // 只允許上傳 PDF 檔案
                            });
                            AssetManager.onSelect((resule) => {
                                const selectedComponent = editor.getSelected();
                                const attributes = selectedComponent.getAttributes();
                                selectedComponent.set("attributes", {
                                    ...attributes, // 保留現有屬性
                                    "data-pdf-url": resule.id,
                                    "title": resule.attributes.name
                                });
                                AssetManager.close();
                            });
                        },
                    }
                ]
            }
        }, view: {
            // 這裡是關於元件的視圖
            onRender() {
                const el = this.el; // 這裡獲取渲染後的 DOM 元素
                $(el).removeAttr("data-bs-toggle"); // 移除屬性
            }
        }
    });
    /*檔案下載*/
    editor.DomComponents.addType('檔案下載', {
        isComponent: el => el.classList?.contains('link_with_icon'),
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'download', type: 'text', label: '檔案名稱', placeholder: '請輸入檔案名稱' },
                    {
                        name: 'file', type: 'button',
                        text: "選擇檔案",
                        command: editor => {
                            var OldName = "";
                            if (editor.getSelected().get("attributes").download != "" && editor.getSelected().get("attributes").download != "未命名") OldName = editor.getSelected().get("attributes").download;
                            AssetManager.open();
                            AssetManager.onSelect((result) => {
                                var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                                if (OldName == "") OldName = result.attributes.name;
                                editor.getSelected().set("attributes", {
                                    "href": result.id,
                                    "download": OldName,
                                });
                                LinkWithIconInit();
                                AssetManager.close();
                            });
                        },
                    }
                ],
            }, init() {
                this.on('change:attributes:download', function (component) {
                    if (typeof (component.getEl()) != "undefined") component.find(".name")[0].components(component.getAttributes().download);
                });
            }
        },
        view: {
            init() {
                this.setLink();
            }, setLink: function () {
                setTimeout(function () {
                    var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                    LinkWithIconInit();
                }, 100);
            }
        }
    });
    
    //QA元件
    editor.DomComponents.addType('QA元件', {
        isComponent: el => el.classList?.contains('qa'),

        view: {
            init() {
                // id 改變就重跑
                this.listenTo(this.model, 'change:attributes:id', this.updateQaAttrs);
            },

            onRender() {
                // 視圖每次渲染都會呼叫，等同你說的 finish
                this.updateQaAttrs();
            },

            updateQaAttrs() {
                const comp = this.model;
                const ccid = comp.get('ccid') || comp.getId();
                if (!ccid) return;

                const link = comp.find('a.qa-bg')[0];     // 請用實際存在的 selector
                const collapse = comp.find('div.collapse')[0];
                if (!link || !collapse) return;

                link.addAttributes({
                    href: `#${ccid}_content`,
                    title: '展開QA',
                    'data-bs-toggle': 'collapse'
                });

                collapse.addAttributes({
                    id: `${ccid}_content`
                });

                this.el.querySelector('a.qa-bg')?.setAttribute('data-bs-toggle', '');
            }
        }
    });

    editor.DomComponents.addType('QA元件鎖定版型', {
        isComponent: el => {
            const fa = $(el).parents(".qa");
            return $(fa).length > 0 && (el.classList?.contains('collapse') || el.classList?.contains('card') || el.classList?.contains('fas') || el.classList?.contains('qa-bg'));
        },
        model: {
            defaults: {
                hoverable: false,
                selectable: false,
                droppable: false,
                copyable: false,
                removable: false,
                editable: false,
            }
        }
    });
    //名片
    editor.DomComponents.addType('名片介紹', {
        isComponent: el => el.classList?.contains('frame_type_2'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {

            }
        },
    });
    //名片介紹
    editor.DomComponents.addType('名片介紹', {
        isComponent: el => el.classList?.contains('frame_type_2'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {

            }
        },
    });
    //活動
    editor.DomComponents.addType('活動列表', {
        isComponent: el => el.classList?.contains('articletype'),
        model: {
            defaults: {
                droppable: false,
                copyable: false,
                traits: [
                    { name: 'data-daterange', type: 'date-range', label: '起訖日期' },
                    { name: 'data-location', type: 'text', label: '地點', placeholder: '請輸入地點' },
                    { name: 'data-addr', type: 'text', label: '地址', placeholder: '請輸入地址' },
                    { name: 'data-link', type: 'text', label: '連結', placeholder: '請輸入連結' },
                    { name: 'data-organizer', type: 'text', label: '主辦單位', placeholder: '請輸入主辦單位' },
                    { name: 'data-a-organizer', type: 'text', label: '協辦單位', placeholder: '請輸入協辦單位' },
                    { name: 'data-r-organizer', type: 'text', label: '執行單位', placeholder: '請輸入執行單位' },
                    { name: 'data-tel', type: 'text', label: '電話', placeholder: '請輸入電話' }
                ]
            },
            init() {
                const self = this;
                const setting = function () {
                    setTimeout(() => {
                        var content = $(".gjs-frame")[0].contentWindow.date_input_change;
                        if (typeof (content) == "undefined") setting();
                        else {
                            const html = content(self.getId());
                            self.components(html);
                        }
                    }, 200);
                }
                self.on(`change:attributes`, setting);
                setting();
            }
        }
    });
    //日期區間型態
    editor.TraitManager.addType("date-range", {

        createInput({ trait }) {
            const self = this;
            const el = document.createElement('div');
            el.innerHTML = `
              <div class="date-range-inputs">
                <input type="text" class="date-range" pattern="[1-9]\d{3}/\d{2}/\d{2} \d{2}:\d{2} ~ [1-9]\d{3}/\d{2}/\d{2} \d{2}:\d{2}" />
              </div>
            `;
            const $d = $(el).find(".date-range");
            const timeStr = self.model.getValue();
            co.Picker.Init($d);
            try {
                const array = timeStr.split(" ~ ");
                $d.data('daterangepicker').setStartDate(array[0]);
                $d.data('daterangepicker').setEndDate(array[1]);
            } catch {
                $d.data('daterangepicker').setStartDate(Date.now);
            }
            $d.on("change", function () {
                editor.getSelected().addAttributes({ "data-daterange": this.value })
            });
            return el;
        }
    });
    //子頁內容區
    editor.DomComponents.addType('子頁內容', {
        isComponent: el => el.classList?.contains('subpage_content'),
        model: {
            defaults: {
                droppable: false,
                editable: false,
                copyable: false,
            }
        },
    });

    var PopupDirectory = null;
    //目錄
    editor.DomComponents.addType('目錄', {
        isComponent: el => el.classList?.contains('menu_directory') || el.classList?.contains('catalog_frame') || el.classList?.contains('advertise_directory'),
        model: {
            defaults: {
                droppable: false,
                editable: false,
                copyable: false,
                traits: [
                    { name: 'id', type: 'text', label: 'ID', placeholder: '元件ID名稱' },
                    { name: 'data-diridname', type: 'text', label: '目錄名稱', placeholder: '尚未關聯目錄' },
                    { name: 'data-dirpath', type: 'text', label: '路徑', placeholder: '設定連結路徑' },
                    {
                        name: 'data-dirid', type: 'button',
                        text: "設置目錄",
                        command: editor => {
                            var data = null;
                            if (!!!PopupDirectory) {
                                PopupDirectory = $("#PopupDirectory").dxPopup("instance");
                                PopupDirectory.option("contentTemplate", $("#PopupDirectory-template"));
                                PopupDirectory.option("title", "設置目錄");
                                window.DirectoryList_SelectChange = function (selectedItems) {
                                    data = selectedItems.selectedRowsData;
                                }
                                window.setTimeout(function () {
                                    var popup = $("#PopupDirectory").dxPopup("instance");
                                    var content = popup.content();
                                    $(content).find(".cancel").on("click", function () {
                                        PopupDirectory.hide();
                                    });
                                    $(content).find(".Sure").on("click", function (component) {
                                        var oldlist = editor.getSelected().getAttributes()["data-dirid"];
                                        editor.getSelected().set("attributes", {
                                            "data-dirid": data.map(function (item) {
                                                return item['Id'];
                                            }),
                                            "data-diridname": data.map(function (item) {
                                                return item['Title'];
                                            })
                                        });
                                        PopupDirectory.hide();
                                        var newlist = editor.getSelected().getAttributes()["data-dirid"].toString();
                                        if (oldlist != newlist) {
                                            $(".gjs-frame")[0].contentWindow.DirectoryGetDataInit();
                                        }
                                    });
                                }, 200);
                            }
                            var data_dirid = editor.getSelected().get("attributes")['data-dirid'];
                            var dirids = typeof (data_dirid) == "string" ? data_dirid.split(',').map(Number) : data_dirid;
                            PopupDirectory.option({
                                onShown: function () {
                                    var grid = $("#DirectoryList2").dxDataGrid("instance");
                                    if (grid) {
                                        grid.selectRows(dirids);
                                    }
                                }
                            });
                            PopupDirectory.show();
                        }
                    },
                    { name: 'data-shownum', type: 'text', label: '單頁筆數', placeholder: '一個分頁要抓幾筆資料' },
                    { name: 'data-maxlen', type: 'text', label: '最大筆數', placeholder: '該目錄僅抓幾筆資料' },
                    { name: 'data-hasbuybtn', type: 'checkbox', label: '購物按鈕(僅作用於商品目錄)', valueTrue: "true", valueFalse: "false" }
                ],
            },
            init() {
                var self = this;
                self.on(`change:attributes:data-shownum`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        fWindow.$(`#${component.getId()}`).data({
                            "prevdirid": attr["data-prevdirid"],
                            "shownum": attr["data-shownum"],
                            "hasbuybtn": attr["data-hasbuybtn"]
                        });
                        fWindow.DirectoryGetDataInit();
                    }, 200);
                });
                self.on(`change:attributes:data-maxlen`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        fWindow.$(`#${component.getId()}`).data({
                            "prevdirid": attr["data-prevdirid"],
                            "maxlen": attr["data-maxlen"],
                            "hasbuybtn": attr["data-hasbuybtn"]
                        });
                        fWindow.DirectoryGetDataInit();
                    }, 200);
                });
                self.on(`change:attributes:data-dirid`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        if (typeof (attr["data-dirid"]) != "undefined") {
                            fWindow.$(`#${component.getId()}`).data({
                                "dirid": attr["data-dirid"].toString(),
                            });
                            fWindow.DirectoryGetDataInit();
                        }
                    }, 200);
                });
                self.on(`change:attributes:data-hasbuybtn`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        fWindow.$(`#${component.getId()}`).data({
                            "prevdirid": attr["data-prevdirid"],
                            "maxlen": attr["data-maxlen"],
                            "hasbuybtn": attr["data-hasbuybtn"]
                        });
                        fWindow.DirectoryGetDataInit();
                    }, 200);
                });
            }
        },
    });
    //目錄切換控制
    editor.DomComponents.addType('格列切換控制', {
        isComponent: el => el.classList?.contains('switch_control'),
        model: {
            defaults: {
                droppable: false,
                copyable: false,
                traits: [
                    {
                        type: 'checkbox',
                        label: '文字',
                        name: 'btn_text',
                        valueTrue: "1",
                        valueFalse: "0"
                    }, {
                        type: 'checkbox',
                        label: '圖片',
                        name: 'btn_grid',
                        valueTrue: "1",
                        valueFalse: "0"
                    }, {
                        type: 'checkbox',
                        label: '圖文',
                        name: 'btn_list',
                        valueTrue: "1",
                        valueFalse: "0"
                    }
                ]
            },
            init() {

                var self = this;

                var list = ["btn_text", "btn_grid", "btn_list"];
                for (var i = 0; i < list.length; i++) {
                    const myClass = list[i];

                    self.on(`change:attributes:${myClass}`, () => {
                        self.components().models.forEach(function (item) {
                            if (item.getClasses().indexOf(myClass) >= 0) {

                                if (item.getClasses().indexOf('d-none') >= 0) {
                                    item.removeClass("d-none");
                                    setTimeout(() => {
                                        var content = $(".gjs-frame")[0].contentWindow.namecontrol;
                                        content(self.getId());
                                    }, 200);
                                }
                                else {
                                    item.addClass("d-none");
                                    setTimeout(() => {
                                        var content = $(".gjs-frame")[0].contentWindow.namecontrol;
                                        content(self.getId());
                                    }, 200);
                                }
                            }
                        });

                    });
                }
            }
        },
    });
    const textType = editor.DomComponents.getType("text");
    //文字
    editor.DomComponents.addType('span', {
        // Define the Model
        model: textType.model.extend({},
            {
                isComponent(el) {
                    if (el.tagName === 'SPAN') {
                        return {
                            type: 'text',
                            src: el.src,
                            tagName: el.tagName.toLowerCase(),
                            content: el.innerHTML,
                            editable: true

                        }
                    }
                }
            }),
        view: textType.view
    });
    //BGCanvas 動畫
    editor.DomComponents.addType('動畫背景', {
        isComponent: el => el.classList?.contains('.BGCanvas'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {
                const self = this;
                const ccid = self.ccid;
                const c = $(".gjs-frame")[0].contentWindow.$;

                window.setTimeout(function () {
                    console.log("in");
                    c(`#${ccid}`).BGCanvas();
                }, 200)
            }
        },
    });
    //關閉所有元件分類夾，僅開啟一個
    var blockControl = function () {
        $(categories.models).each(function (index, category) {
            category.set('open', index == 0).on('change:open', function (opened) {
                opened.get('open') && categories.each(category => {
                    if (category !== opened) {
                        category.set('open', false)
                    }
                })
            });
        });
    }
    //檢驗元件是否已載入，若載入成功則設定關閉控制
    editor.on('load', () => {
        blockControl();
    })

    //載入儲存的元件
    settings.getComponer().done(function (result) {
        $(result).each(function () {
            const html = co.Data.HtmlDecode(this.html);
            const htmlWithAttr = html.replace(
                /<([a-zA-Z0-9-]+)([^>]*)>/,
                `<$1$2 data-block-name="${this.title}">`
            );
            const elementHtmlCss = `${htmlWithAttr}<style>${this.css}</style>`;
            let blockId = 'customBlockTemplate_' + this.id;
            let iconText = (this.icon || "").replace("material-symbols-outlined", "").trim();
            let media = "";
            if (/^fa/.test(this.icon)) {
                media = `<i class="${this.icon} fa-5x"></i>`;
            } else if (/material-symbols-outlined/.test(this.icon)) {
                media = `<i class="material-symbols-outlined fa-5x">${iconText}</i>`;
            }
            appendBlock(blockId, {
                category: this.typeName,
                attributes: { custom_block_template: true },
                label: `${this.title}`,
                media: media,
                content: elementHtmlCss,
            });
        });
    });

    //產生元件
    const getCssRules = (selected, myRule, top) => {
        const id = selected.getId();
        const itemClass = selected.getClasses();
        const tagName = selected.get('tagName');
        const cssComposer = editor.CssComposer;
        const rules = cssComposer.getAll();
        const relatedIdRules = rules.filter(rule => {
            const ruleStr = rule.toCSS();
            const hasIdOrTagName =
                (itemClass.some(cls => ruleStr.includes(`.${cls}`)) && top.map(e => ruleStr.includes(e))) ||
                ruleStr.includes(`#${id}`) ||
                (ruleStr.includes(tagName) && top.map(e => ruleStr.includes(e)));
            return hasIdOrTagName;
        });
        co.Array.merge(myRule, relatedIdRules);
    }

    const findComponentStyles = function (selected, myRule, top) {
        if (selected) {
            const childModel = selected.components().models
            if (typeof (top) == "undefined") top = [];
            if (childModel) {
                top.push(...selected.get('tagName'));
                for (const model of childModel) {
                    findComponentStyles(model, myRule, top)
                }
            }
            getCssRules(selected, myRule, top);
        }
    }
    const createBlockTemplate = function (selected, name_blockId) {
        const blockId = name_blockId.blockId;
        const name = name_blockId.name;
        const relatedRules = [];
        let elementHTML = $(selected.toHTML()).removeClass("gjs-selected")[0].outerHTML;
        let first_partHtml = elementHTML.substring(0, elementHTML.indexOf(' '));
        let second_partHtml = elementHTML.substring(elementHTML.indexOf(' ') + 1);
        first_partHtml += ` custom_block_template=true block_id="${blockId}" `;
        let finalHtml = first_partHtml + second_partHtml;
        let icon = $('#NewBlockicon').val();
        findComponentStyles(selected, relatedRules);
        let blockCss = "";
        relatedRules.forEach(rule => {
            blockCss += rule.toCSS();
        });
        const css = `<style>${blockCss}</style>`;
        const elementHtmlCss = finalHtml + css;
        const category = $('#ComponerTypeList>option:selected').text();
        const object = {
            id: name_blockId.id,
            Title: name,
            icon: icon,
            type: $('#ComponerTypeList>option:selected').val(),
            Html: $('<div/>').text(finalHtml).html(),
            css: blockCss,
            disp_opt: true
        }
        co.HtmlContent.AddUp(object).done(function (result) {
            if (result.success) {
                let iconText = "";
                if (/material-symbols-outlined/.test(icon)) iconText = icon.replace("material-symbols-outlined", "").trim();
                appendBlock(blockId, {
                    category: category,
                    attributes: { custom_block_template: true },
                    label: `${name}`,
                    media: `<i class="${icon} fa-5x">${iconText}</i>`,
                    content: elementHtmlCss,
                })
            } else co.sweet.error(result.error);
        });

    }
    let actionBlockId = null;
    const ContextMenu = function (options) {
        // 唯一实例
        let instance;

        // 创建实例方法
        function createMenu() {
            // todo
            const ul = document.createElement("ul");
            ul.classList.add("custom-context-menu");
            const { menus } = options;
            if (menus && menus.length > 0) {
                for (let menu of menus) {
                    const li = document.createElement("li");
                    li.textContent = menu.name;
                    li.onclick = menu.onClick;
                    ul.appendChild(li);
                }
            }
            const body = document.querySelector("body");
            body.appendChild(ul);
            return ul;
        }

        return {
            // 获取实例的唯一方式
            getInstance: function () {
                if (!instance) {
                    instance = createMenu();
                }
                return instance;
            },
        };
    };
    const menuSinglton = ContextMenu({
        menus: [
            {
                name: "加入至頁面",
                onClick: function (e) {
                    const block = BlockManager.get(actionBlockId);
                    editor.addComponents(block.attributes.content);
                },
            },
            {
                name: "刪除元件",
                onClick: function (e) {
                    removeBlock();
                },
            }
        ],
    });
    function showMenu(e) {
        const menus = menuSinglton.getInstance();
        menus.style.top = `${e.clientY}px`;
        menus.style.left = `${e.clientX}px`;
        menus.style.display = "block";
    }
    function hideMenu(e) {
        const menus = menuSinglton.getInstance();
        menus.style.display = "none";
    }
    function removeBlock() {
        const l = actionBlockId.split("_");
        const id = l[l.length - 1];
        co.HtmlContent.Delete(id).done(function (result) {
            if (result.success) {
                BlockManager.remove(actionBlockId);
                co.sweet.success("成功", null, true);
            } else co.sweet.error(result.error);
        });
    }

    document.addEventListener("click", hideMenu);
    const appendBlock = function (id, obj) {
        const blockId = `customBlockTemplate_${id}`;
        var mySetting = {
            type: "default",
            render: ({ model, el }) => {
                el.addEventListener('dblclick', () => {
                    co.sweet.confirm('即將刪除', co.sweet.TitleHilight(`是否要確認將{0}刪除?`, obj.label), '確認', "取消", function () {
                        actionBlockId = blockId;
                        removeBlock();
                    });
                }), el.addEventListener('contextmenu', (e) => {
                    e.preventDefault();
                    actionBlockId = blockId;
                    showMenu(e);
                })
            }
        }
        $.extend(true, mySetting, obj);
        BlockManager.add(blockId, mySetting);
    }
    const createBlockTemplateConfirmation = function () {
        const selected = editor.getSelected();
        const self = this;
        let name = $("#NewBlockName").val() || "新增";

        co.HtmlContent.AddUp({
            Title: name,
            Type: $('#ComponerTypeList>option:selected').val()
        }).done(function (result) {
            if (result.success) {
                let blockId = 'customBlockTemplate_' + result.message.split(' ').join('_')
                let name_blockId = {
                    'id': result.message,
                    'name': name,
                    'blockId': blockId
                }
                createBlockTemplate(selected, name_blockId);
                document.getElementById("ComFrm").reset();
            } else co.sweet.error(result.error);
        });
    }
    $(`<div id="setComponents" class="modal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Modal title</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form id="ComFrm" class="form-horizontal needs-validation" novalidate>
                <div class="form-floating mb-3 input-group">
                    <input type="hidden" name="icon">
                    <input type="text" class="form-control" id="NewBlockName" placeholder="請輸入新元件的名稱">
                    <label for="floatingInput">元件名稱</label>
                    <div class="input-group-append">
                        <button type="button" id="NewBlockicon" class="btn btn-outline-secondary"></button>
                    </div>
                </div>
                <div class="d-none">
                    <select name="ComponerTypeList" id="ComponerTypeList" class="form-select item-menu" aria-label="類別">
                        <option value="" disabled="disabled">類別</option>
                    </select>
                    <div class="invalid-feedback">類別必填</div>
                </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary btn-save">Save changes</button>
          </div>
        </div>
      </div>
    </div>`).appendTo("body");
    const myModal = new bootstrap.Modal('#setComponents');
    const iconPicker = $('#NewBlockicon').iconpicker(settings.iconPickerOpt);
    co.HtmlContent.GetTypeList().done(function (result) {
        if (result.success) {
            var $s = $("#ComponerTypeList");
            $(result.type).each(function () {
                $s.append(`<option value="${this.value}">${this.key}</option>`);
            });
            if (result.type.length > 1) $s.parents(".d-none").removeClass("d-none");
        }
    });
    $('#setComponents').find(".btn-save").on("click", function () {
        createBlockTemplateConfirmation();
        myModal.hide();
        co.sweet.success("加入我的最愛");
    });
    iconPicker.on('change', function (e) {
        $('#NewBlockicon').val(e.icon);
    });


    //畫布內容儲存及發布
    if (settings.save != null) {
        panelManager.addButton('options', {
            id: 'panelSave',
            className: 'someClass',
            label: '<i title="儲存" class="fa fa-download"></i>',
            command: function (editor) {
                settings.save(editor.getHtml(), editor.getCss()).done(function () {
                    co.sweet.success("已儲存草稿");
                });
            },
            attributes: { title: 'save' },
            active: false,
        });
    }

    if (settings.import != null) {
        panelManager.addButton('options', {
            id: 'panelImport',
            className: 'someClass',
            label: '<i title="發布" class="fa fa-cloud-arrow-up""></i>',
            command: function (editor) {
                let t = editor.getHtml();
                const $html = $("<div>").append(t);
                //$html.find(`[data-bs-target]`).attr("data-bs-toggle", "modal");
                t = `<body>${$html.html()}</body>`;
                settings.import(t, editor.getCss()).done(function () {
                    co.sweet.success("已儲存並發布");
                });
            },
            attributes: { title: 'save' },
            active: false,
        });
    }

    // 複製事件監聽
    editor.on('component:clone', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();
        obj.setAttributes({ id: 'id', 'data-key': '' });

        if (classList.indexOf("anchor_title") > -1) {
            var cont = iframe.document.getElementsByClassName("anchor_title").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("anchor_title").length != cont) iframe.AnchorPointInit();
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        } else if (classList.indexOf("link_with_icon") > -1) {
            obj.set("attributes", {
                "href": "",
                "download": "未命名"
            });
        }
    });

    // 刪除事件監聽
    editor.on('component:remove', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();
        obj.setAttributes({ id: 'id', 'data-key': '' });

        if (classList.indexOf("anchor_title") > -1) {
            var cont = iframe.document.getElementsByClassName("anchor_title").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("anchor_title").length != cont) iframe.AnchorPointInit();
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        }
    });

    // 挪動事件監聽

    editor.on('component:drag:end', (obj) => {
        const classList = obj.target.getClasses();
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        if (classList.indexOf("anchor_title") > -1) iframe.AnchorPointInit();
    });

    /*editor.on('selector:add', selector => {
        selector.set({
            // Can't be seen by the style manager, therefore even by the user
            private: true,
        })
    });*/
    /**************
     * 指令參考
     * ***********/
    //editor.addComponents('<div id="yui" class="cls">New component</div>');
});
