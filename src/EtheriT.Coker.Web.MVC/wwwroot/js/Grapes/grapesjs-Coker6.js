grapesjs.plugins.add('grapesjs-Coker6', (editor, options) => {
    let settings = {
        save: function () { return false; },
        import: function () { return false; },
        getComponer: function () { return false; },
        asset: [],
        iconPickerOpt: { cols: 4, rows: 4, footer: false, iconset: "GoogleMaterialSymbolsOutlined" }
    };
    $.extend(true, settings, options);
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

    editor.on('asset:add', (option) => {
        //console.log(option);
    });

    editor.on('run:open-assets', () => {
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
    });

    //檔案刪除
    editor.on("asset:remove", (asset) => {
        const guid = asset.get('guid');
        co.File.Delete(guid).done(function (result) {
            console.log(result);
        });
    });

    //載入所有檔案
    co.File.getFileList(0).done(function (result) {
        if (result.success) {
            var myJSON = [];
            settings.asset = result.files;
            $(result.files).each(function (index) {
                myJSON.push({
                    src: this.path,
                    name: `${this.name}`,
                    guid: this.guid
                });
            });
            var images = myJSON;
            editor.AssetManager.add(images);
        }
    });

    //元件參數設定
    editor.DomComponents.addType('link', {
        isComponent: el => el.tagName == 'A',
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'title', type: 'text', label: '名稱', placeholder: '請輸入連結名稱' },
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
            }
        },
    });

    editor.DomComponents.addType('linkWithIcon', {
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
                            AssetManager.open();
                            AssetManager.onSelect((resule) => {
                                var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                                editor.getSelected().set("attributes", { "href": resule.id });
                                LinkWithIconInit();
                                AssetManager.close();
                            });
                        },
                    }
                ],
            },
            init() {
                this.on('change:attributes:download', function () {
                    setTimeout(() => {
                        var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                        LinkWithIconInit();
                    }, "100");
                });
            }
        },
    });

    editor.DomComponents.addType('輪播', {
        isComponent: el => el.classList?.contains('one_swiper') || el.classList?.contains('two_swiper') || el.classList?.contains('four_swiper') || el.classList?.contains('six_swiper'),
        model: {
            defaults: {
                traits: [
                    {
                        name: 'swiper-slide', type: 'button',
                        text: "新增一欄",
                        command: editor => {
                            var $selected = editor.getSelected();
                            var swiper = $selected.find(".swiper")[0].getEl().swiper;
                            var new_slide = $("<div>").append($($selected.find(".template_slide")[0].find(".swiper-slide")[0].toHTML())).html();
                            $selected.find(".swiper-wrapper")[0].append(new_slide);
                            swiper.update();
                        },
                    }
                ],
            }
        },
    });

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

    editor.DomComponents.addType('輪播容器', {
        isComponent: el => el.classList?.contains('image_link_slide'),
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
    var checkLoadTimer = setInterval(function () {
        if (categories.models.length > 1) {
            clearInterval(checkLoadTimer);
            blockControl();
        }
    }, 500);

    /*元件浮動控制項*/
    var linkCommandId = function () {
        myModal.show();
    }; // Id to use to create the button and anchor tag editor command
    var toolbarIcon = '<i class="fa fa-star"></i>'; // Icon used in the component toolbar
    var setType = "default";
    var linkType = editor.DomComponents.getType(setType);
    var linkTypeModel = editor.DomComponents.getType(setType).model;
    editor.DomComponents.addType(setType, {
        model: {
            initToolbar() {
                linkTypeModel.prototype.initToolbar.apply(this, arguments);
                const tb = this.get('toolbar');
                const tbExists = tb.some(item => item.command === linkCommandId);
                // Add link icon in case user click on anchor tag
                if (!tbExists) {
                    tb.unshift({
                        command: linkCommandId,
                        label: toolbarIcon,
                    });
                    this.set('toolbar', tb);
                }
            }
        },
        // Double click on link open link editor
        view: linkType.view.extend({
            events: {
                "dblclick": "editLink"
            },
            editLink: function (e) {
                e.stopImmediatePropagation(); // prevent the RTE from opening
                editor.runCommand(linkCommandId, {});
            }
        })
    });

    //載入儲存的元件
    settings.getComponer().done(function (result) {
        $(result).each(function () {
            const html = co.Data.HtmlDecode(this.html);
            const elementHtmlCss = `${html}<style>${this.css}</style>`;
            let blockId = 'customBlockTemplate_' + this.id;
            let iconText = this.icon.replace("material-symbols-outlined", "").trim();
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
    const getCss = (selected) => {
        const el = selected.getEl();
        const id = selected.getId();
        const itemClass = el.classList;
        const style = editor.CssComposer.getRule(`#${id}`);
        const hoverStyle = editor.CssComposer.getRule(`#${id}:hover`);
        let classCssStr = "";
        $(itemClass).each(function () {
            if (!!this) {
                const myClass = this.toString();
                const cStyle = editor.CssComposer.getRule(`.${myClass}`);
                const cHoverStyle = editor.CssComposer.getRule(`.${myClass}:hover`);
                if (cStyle) {
                    classCssStr = `${classCssStr} ${cStyle.toCSS()}`;
                }
                if (cHoverStyle) classCssStr = `${classCssStr} ${cHoverStyle.toCSS()}`;
            }
        });
        if (style) {
            if (hoverStyle) {
                classCssStr = `${style.toCSS()} ${hoverStyle.toCSS()} ${classCssStr}`;
            }
            return classCssStr = `${style.toCSS()} ${classCssStr}`;
        }
        else {
            return classCssStr;
        }
    }

    const findComponentStyles = function (selected) {
        let css = ''
        if (selected) {
            const childModel = selected.components().models
            if (childModel) {
                for (const model of childModel) {
                    css = css + findComponentStyles(model)
                }
                return css + getCss(selected);
            }
            else {
                return getCss(selected);
            }
        }
    }
    const createBlockTemplate = function (selected, name_blockId) {
        const blockId = name_blockId.blockId;
        const name = name_blockId.name;

        let elementHTML = $(selected.getEl().outerHTML).removeClass("gjs-selected")[0].outerHTML;
        let first_partHtml = elementHTML.substring(0, elementHTML.indexOf(' '));
        let second_partHtml = elementHTML.substring(elementHTML.indexOf(' ') + 1);
        first_partHtml += ` custom_block_template=true block_id="${blockId}" `;
        let finalHtml = first_partHtml + second_partHtml;
        let icon = $('#NewBlockicon').val();
        const blockCss = findComponentStyles(selected);
        const css = `<style>${blockCss}</style>`;
        const elementHtmlCss = finalHtml + css;
        const category = $('#ComponerTypeList>option:selected').text();
        const object = {
            id: name_blockId.id,
            Title: name,
            icon: icon,
            type: $('#ComponerTypeList>option:selected').val(),
            Html: $('<div/>').text(finalHtml).html(),
            css: blockCss
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
        console.log(id);
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
                settings.import(editor.getHtml(), editor.getCss()).done(function () {
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
        } else if (classList.indexOf("swiper-slide") > -1) {
            var swiper = editor.getSelected().parent().parent().getEl().swiper;
            if (typeof (swiper) != "undefined") {
                var cont = iframe.document.getElementsByClassName("swiper-slide").length;
                const timmer = function () {
                    if (iframe.document.getElementsByClassName("swiper-slide").length != cont) swiper.update();
                    else setTimeout(timmer, 100);
                }
                setTimeout(timmer, 100);
            }
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1) {
            var cont = iframe.document.getElementsByClassName("swiper").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("swiper").length != cont) iframe.SwiperInit({ autoplay: false });
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
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
        } else if (classList.indexOf("swiper-slide") > -1) {
            if (typeof (editor.getSelected()) != "undefined") {
                var swiper = editor.getSelected().parent().parent().getEl().swiper;
                if (typeof (swiper) != "undefined") {
                    var cont = iframe.document.getElementsByClassName("swiper-slide").length;
                    const timmer = function () {
                        if (iframe.document.getElementsByClassName("swiper-slide").length != cont) swiper.update();
                        else setTimeout(timmer, 100);
                    }
                    setTimeout(timmer, 100);
                }
            }
        }
    });

    // 挪動事件監聽
    editor.on('component:drag:end', (obj) => {
        const classList = obj.target.getClasses();
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        if (classList.indexOf("anchor_title") > -1) iframe.AnchorPointInit();
        else if (classList.indexOf("swiper-slide") > -1) {
            var swiper = obj.target.parent().parent().getEl().swiper;
            swiper.update();
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1) iframe.SwiperInit({ autoplay: false });
    });
    /**************
     * 指令參考
     * ***********/
    //editor.addComponents('<div id="yui" class="cls">New component</div>');
});