/****************************************
 * obj.save 內容儲存
 * obj.import 內容發布
 ****************************************/
var grapesInit = function (options) {
    var settings = {
        save : function () { return false; },
        import : function () { return false; }
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
    const getCss = (editor, id) => {
        const style = editor.CssComposer.getRule(`#${id}`);
        const hoverStyle = editor.CssComposer.getRule(`#${id}:hover`);

        if (style) {
            if (hoverStyle) {
                return style.toCSS() + ' ' + hoverStyle.toCSS()
            }
            return style.toCSS()
        }
        else {
            return ''
        }
    }

    const findComponentStyles = function(selected){
        let css = ''
        if (selected) {
            const childModel = selected.components().models
            if (childModel) {
                for (const model of childModel) {
                    css = css + findComponentStyles(model)
                }
                return css + getCss(editor, selected.getId());
            }
            else {
                return getCss(editor, selected.getId());
            }
        }
    }
    const createBlockTemplate = function (selected, name_blockId) {
        const bm = editor.BlockManager
        const blockId = name_blockId.blockId;
        const name = name_blockId.name;

        let elementHTML = selected.getEl().outerHTML;
        let first_partHtml = elementHTML.substring(0, elementHTML.indexOf(' '));
        let second_partHtml = elementHTML.substring(elementHTML.indexOf(' ') + 1);
        first_partHtml += ` custom_block_template=true block_id="${blockId}" `
        let finalHtml = first_partHtml + second_partHtml
        let icon = $('#NewBlockicon').val();
        const blockCss = findComponentStyles(selected)
        const css = `<style>${blockCss}</style>`
        const elementHtmlCss = finalHtml + css

        bm.add(`customBlockTemplate_${blockId}`, {
            category: '自訂區',
            attributes: { custom_block_template: true },
            label: `${name}`,
            media: `<i class="${icon} fa-5x"></i>`,
            content: elementHtmlCss,
        })
    }
    const createBlockTemplateConfirmation = function() {
        const selected = editor.getSelected();
        //let name = this.blockTemplateForm.get('name')!.value
        let name = $("#NewBlockName").val() || "新增";
        let blockId = 'customBlockTemplate_' + name.split(' ').join('_')
        let name_blockId = {
            'name': name,
            'blockId': blockId
        }
        console.log($("#NewBlockicon").val());
        createBlockTemplate(selected, name_blockId)
        //this.blockTemplateForm.reset();
        //this.modalService.getModal('createBlockTemplate').close();
    }
    $(`<div id="setComponents" class="modal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Modal title</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="form-floating mb-3 input-group">
                <input type="hidden" name="icon">
                <input type="text" class="form-control" id="NewBlockName" placeholder="請輸入新元件的名稱">
                <label for="floatingInput">元件名稱</label>
                <div class="input-group-append">
                    <button type="button" id="NewBlockicon" class="btn btn-outline-secondary"></button>
                </div>
            </div>
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
    $('#setComponents').find(".btn-save").on("click", function(){
        createBlockTemplateConfirmation();
        myModal.hide();
        co.sweet.success("加入我的最愛");
    });
    iconPicker.on('change', function (e) {
        $('#NewBlockicon').val(e.icon);
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

    editor.BlockManager.add('testBlock', {
        category: "自訂區",
        label: '歡迎區',
        attributes: { class: 'gjs-fonts gjs-f-b1', title: 'hello' },
        content: '<div class="welecom" style="text-align:center"><span>Hello World</span></div>'
    });

    editor.addComponents('<div id="yui" class="cls">New component</div>');

    var checkLoadTimer = setInterval(function () {
        if (categories.models.length > 1) clearInterval(checkLoadTimer);
        blockControl();
    }, 500);

    $.fn.extend({
        removeAllElemntId : function () {
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