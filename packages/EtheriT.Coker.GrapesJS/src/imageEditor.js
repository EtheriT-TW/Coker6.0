import * as tuiImageEditorPluginModule from 'grapesjs-tui-image-editor';
import * as tuiImageEditorModule from 'tui-image-editor';
import { tuiImageEditorZhTw } from './locales/zhTw.js';

function resolveDefault(moduleValue, moduleName) {
    if (typeof moduleValue === 'function') {
        return moduleValue;
    }

    if (typeof moduleValue.default === 'function') {
        return moduleValue.default;
    }

    if (moduleValue.default && typeof moduleValue.default.default === 'function') {
        return moduleValue.default.default;
    }

    throw new Error(`[EtheriT.Coker.GrapesJS] ${moduleName} export is not a function.`);
}

const tuiImageEditorPlugin = resolveDefault(
    tuiImageEditorPluginModule,
    'grapesjs-tui-image-editor'
);
const TuiImageEditor = resolveDefault(tuiImageEditorModule, 'tui-image-editor');
const imageUploadCompleteEvent = 'coker:image-editor:upload:complete';

function setApplyButtonBusy(button, busy) {
    button.disabled = busy;
    button.classList.toggle('is-uploading', busy);
    button.setAttribute('aria-busy', busy ? 'true' : 'false');
    button.title = busy ? '圖片上傳中，請稍候' : '儲存編輯結果並上傳';
    button.setAttribute('aria-label', button.title);
    button.innerHTML = busy
        ? '<span class="material-symbols-outlined" aria-hidden="true">progress_activity</span>'
        : '<span class="material-symbols-outlined" aria-hidden="true">save</span>';
}

export function createTuiImageEditorPlugin(options = {}) {
    const customConfig = options.config || {};
    const customIncludeUI = customConfig.includeUI || {};
    const customApplyButton = options.onApplyButton;

    return function cokerTuiImageEditorPlugin(editor) {
        tuiImageEditorPlugin(editor, {
            labelImageEditor: '圖片編輯',
            labelApply: '儲存並上傳',
            ...options,
            config: {
                ...customConfig,
                includeUI: {
                    ...customIncludeUI,
                    locale: {
                        ...tuiImageEditorZhTw,
                        ...(customIncludeUI.locale || {})
                    }
                }
            },
            constructor: TuiImageEditor,
            upload: true,
            addToAssets: true,
            script: [],
            style: [],
            onApplyButton(button) {
                const applyChanges = button.onclick;

                button.removeAttribute('style');
                button.className = 'tui-image-editor__apply-btn coker-image-editor-save';
                button.type = 'button';
                setApplyButtonBusy(button, false);

                button.onclick = function onSaveImage(event) {
                    if (button.disabled) {
                        return;
                    }

                    setApplyButtonBusy(button, true);

                    const unlockButton = function () {
                        setApplyButtonBusy(button, false);
                    };

                    editor.once(imageUploadCompleteEvent, unlockButton);

                    try {
                        applyChanges.call(button, event);
                    } catch (error) {
                        editor.off(imageUploadCompleteEvent, unlockButton);
                        unlockButton();
                        throw error;
                    }
                };

                if (typeof customApplyButton === 'function') {
                    customApplyButton(button);
                }
            }
        });
    };
}
