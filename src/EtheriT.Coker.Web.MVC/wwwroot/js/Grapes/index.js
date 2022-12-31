/****************************************
 * obj.save 內容儲存
 * obj.import 內容發布
 ****************************************/
var grapesInit = function (options) {
    var settings = {
        save: function () { return false; },
        import: function () { return false; },
        getComponer: function () { return false; },
    };
    $.extend(true, settings, options);
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
    var panelManager = editor.Panels;
    const BlockManager = editor.BlockManager;
    var categories = editor.BlockManager.getCategories();
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
    const iconPickerOpt = { cols: 4, rows: 4, footer: false, iconset: "fontawesome5" };
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
            console.log(result);
            if (result.success) {
                appendBlock(blockId, {
                    category: category,
                    attributes: { custom_block_template: true },
                    label: `${name}`,
                    media: `<i class="${icon} fa-5x"></i>`,
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
        co.HtmlContent.Delete({ id: id }).done(function (result) {
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
    const iconPicker = $('#NewBlockicon').iconpicker(iconPickerOpt);
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

    settings.getComponer().done(function (result) {
        $(result).each(function () {
            const html = $("<div/>").html(this.html).text();
            const elementHtmlCss = `${html}<style>${this.css}</style>`;
            let blockId = 'customBlockTemplate_' + this.id;
            appendBlock(blockId, {
                category: this.typeName,
                attributes: { custom_block_template: true },
                label: `${this.title}`,
                media: `<i class="${this.icon} fa-5x"></i>`,
                content: elementHtmlCss,
            });
        });
    });

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

    panelManager.addButton('options', {
        id: 'panelImport',
        className: 'someClass',
        label: '<i title="發布" class="fa fa-cloud-arrow-up""></i>',
        command: function (editor) {
            console.log("in");
            settings.import(editor.getHtml(), editor.getCss()).done(function () {
                co.sweet.success("已儲存並發布");
            });
        },
        attributes: { title: 'save' },
        active: false,
    });

    //editor.addComponents('<div id="yui" class="cls">New component</div>');

    var checkLoadTimer = setInterval(function () {
        if (categories.models.length > 1) clearInterval(checkLoadTimer);
        blockControl();
    }, 500);

    $.fn.extend({
        removeAllElemntId: function () {
            $(this).removeAttr("id").removeClass("gjs-selected");
            $(this).children().each(function (index, element) {
                $(element).removeAllElemntId();
            });
        }
    });

    var linkCommandId = function () {
        myModal.show();
    }; // Id to use to create the button and anchor tag editor command
    var toolbarIcon = '<i class="fa fa-star"></i>'; // Icon used in the component toolbar

    /*add icon */
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

    return editor;
}