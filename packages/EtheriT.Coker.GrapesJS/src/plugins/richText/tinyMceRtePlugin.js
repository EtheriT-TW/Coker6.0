const defaultFontSizes = [
    '0.75rem',
    '0.875rem',
    '1rem',
    '1.125rem',
    '1.25rem',
    '1.5rem',
    '2rem',
    '2.5rem',
    '3rem'
];

const defaultToolbar = [
    'fontsize',
    'bold italic underline strikethrough',
    'forecolor backcolor',
    'bullist numlist',
    'outdent indent',
    'link unlink',
    'removeformat'
].join(' | ');

const tinyMceStyleId = 'coker-tinymce-oxide-skin';
let tinyMceModulesPromise;

function ensureStyle(document, id, css) {
    if (!document?.head || document.getElementById(id)) {
        return;
    }

    const style = document.createElement('style');
    style.id = id;
    style.textContent = css;
    document.head.appendChild(style);
}

function loadTinyMce() {
    if (!tinyMceModulesPromise) {
        tinyMceModulesPromise = (async () => {
            const [{ default: tinymce }, { default: skinCss }, { default: contentCss }] =
                await Promise.all([
                    import('tinymce/tinymce'),
                    import('tinymce/skins/ui/oxide/skin.min.css?inline'),
                    import('tinymce/skins/ui/oxide/content.inline.min.css?inline')
                ]);

            // These modules register themselves against the TinyMCE singleton,
            // so load them only after the core module is available.
            await Promise.all([
                import('tinymce/icons/default'),
                import('tinymce/models/dom'),
                import('tinymce/themes/silver'),
                import('tinymce/plugins/autolink'),
                import('tinymce/plugins/link'),
                import('tinymce/plugins/lists')
            ]);

            return { tinymce, skinCss, contentCss };
        })();
    }

    return tinyMceModulesPromise;
}

function normalizeLicenseKey(value) {
    return typeof value === 'string' ? value.trim() : '';
}

function notifyInput(element) {
    const EventConstructor = element.ownerDocument?.defaultView?.Event || Event;
    element.dispatchEvent(new EventConstructor('input', {
        bubbles: true
    }));
}

function focusEditor(element, rte) {
    element.contentEditable = 'true';
    rte.setEditableRoot?.(true);
    rte.mode?.set?.('design');
    rte.show?.();
    rte.focus?.();
}

function disableEditor(element, rte) {
    rte.setEditableRoot?.(false);
    rte.mode?.set?.('readonly');
    element.contentEditable = 'false';
}

export function tinyMceRtePlugin(editor, options = {}) {
    const customConfig = options.config || {};
    const licenseKey = normalizeLicenseKey(
        options.licenseKey || customConfig.license_key
    );

    if (options.enabled === false) {
        return;
    }

    if (!licenseKey) {
        editor.log(
            '[EtheriT.Coker.GrapesJS] TinyMCE RTE 未啟用：請透過 tinyMceRteOptions.licenseKey 明確提供 trial、商業授權 key，或在確認 GPL 條款後傳入 "gpl"。',
            { ns: 'tiny-mce-rte', level: 'warning' }
        );
        return;
    }

    const instances = new Set();
    const customSetup = customConfig.setup;
    const customInitCallback = customConfig.init_instance_callback;
    const toolbar = options.toolbar || defaultToolbar;
    const fontSizes = options.fontSizes || defaultFontSizes;

    editor.setCustomRte({
        // TinyMCE owns the markup inside a text component. Keeping this disabled
        // avoids GrapesJS reparsing list/link markup into nested components.
        parseContent: false,

        async enable(element, rte) {
            if (rte && !rte.removed) {
                focusEditor(element, rte);
                return rte;
            }

            const toolbarContainer = editor.RichTextEditor.getToolbarEl();
            const { tinymce, skinCss, contentCss } = await loadTinyMce();
            ensureStyle(toolbarContainer.ownerDocument, tinyMceStyleId, skinCss);
            const initialized = await tinymce.init({
                menubar: false,
                branding: false,
                promotion: false,
                statusbar: false,
                inline: true,
                toolbar_persist: true,
                toolbar_mode: 'wrap',
                highlight_on_focus: false,
                object_resizing: false,
                browser_spellcheck: true,
                plugins: 'autolink link lists',
                toolbar,
                font_size_formats: fontSizes.join(' '),
                link_title: true,
                link_context_toolbar: true,
                link_default_protocol: 'https',
                allow_unsafe_link_target: false,
                target_list: [
                    { title: '目前視窗', value: '' },
                    { title: '另開視窗', value: '_blank' }
                ],
                rel_list: [
                    { title: '無', value: '' },
                    { title: 'nofollow', value: 'nofollow' },
                    { title: 'noopener', value: 'noopener' },
                    { title: 'noreferrer', value: 'noreferrer' }
                ],
                convert_urls: false,
                skin: false,
                content_css: false,
                content_style: contentCss,
                ...customConfig,
                target: element,
                inline: true,
                fixed_toolbar_container_target: toolbarContainer,
                license_key: licenseKey,
                setup(tinyEditor) {
                    instances.add(tinyEditor);
                    tinyEditor.on('input change undo redo', () => notifyInput(element));
                    customSetup?.(tinyEditor);
                },
                init_instance_callback(tinyEditor) {
                    focusEditor(element, tinyEditor);
                    customInitCallback?.(tinyEditor);
                }
            });

            const instance = initialized?.[0];
            if (!instance) {
                throw new Error('[EtheriT.Coker.GrapesJS] TinyMCE 初始化失敗。');
            }

            return instance;
        },

        disable(element, rte) {
            if (!rte || rte.removed) {
                return;
            }

            disableEditor(element, rte);
        },

        getContent(element, rte) {
            if (!rte || rte.removed) {
                return element.innerHTML;
            }

            return rte.getContent({ format: 'html' });
        },

        destroy() {
            instances.forEach(instance => {
                if (!instance.removed) {
                    instance.remove();
                }
            });
            instances.clear();
        }
    });
}

export { defaultFontSizes as tinyMceDefaultFontSizes };
