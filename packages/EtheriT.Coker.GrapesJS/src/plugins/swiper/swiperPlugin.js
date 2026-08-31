import { isSwiperRootElement, normalizeEmbedUrl } from './swiperModel.js';
import { openSwiperEditor } from './swiperEditor.js';

const commandId = 'coker:swiper:edit';
const componentType = '輪播';
const embeddedVideoType = '外嵌影片放大檢視';
const selectThumbnailCommandId = 'coker:embedded-video:select-thumbnail';

export function swiperPlugin(editor, options = {}) {
    editor.Commands.add(commandId, {
        run(currentEditor, sender, commandOptions = {}) {
            const component = commandOptions.component || currentEditor.getSelected();
            if (!component) {
                currentEditor.AlertManager?.alert('請先選擇輪播元件。');
                return;
            }

            openSwiperEditor(currentEditor, component);
        }
    });

    editor.Commands.add(selectThumbnailCommandId, {
        run(currentEditor) {
            const component = currentEditor.getSelected();
            if (!component) {
                return;
            }

            currentEditor.AssetManager.open({
                types: ['image'],
                select(asset) {
                    const image = component.find('img')[0];
                    const source = asset?.getSrc?.() || asset?.get?.('src') || asset?.id || '';
                    if (image && source) {
                        image.set({ src: source });
                    }
                    currentEditor.AssetManager.close();
                }
            });
        }
    });

    editor.DomComponents.addType(componentType, {
        isComponent(element) {
            return isSwiperRootElement(element)
                ? { type: componentType, name: '輪播' }
                : undefined;
        },
        model: {
            defaults: {
                name: '輪播',
                traits: [{
                    type: 'button',
                    text: '開啟輪播編輯',
                    command: commandId
                }]
            }
        },
        view: {
            events: {
                dblclick: 'openSwiperEditor'
            },

            openSwiperEditor(event) {
                event?.preventDefault?.();
                this.em.runCommand(commandId, { component: this.model });
            }
        }
    });

    editor.DomComponents.addType(embeddedVideoType, {
        isComponent(element) {
            return element?.classList?.contains('YTmodal_frame')
                ? { type: embeddedVideoType }
                : undefined;
        },
        model: {
            defaults: {
                removable: true,
                editable: true,
                traits: [
                    { name: 'data-yt-title', type: 'text', label: '標題', placeholder: '請輸入影片標題' },
                    { name: 'data-link', type: 'text', label: '網址', placeholder: '請輸入 YouTube 或 Facebook 網址' },
                    {
                        name: 'thumb',
                        type: 'button',
                        text: '選擇預覽圖片',
                        command: selectThumbnailCommandId
                    }
                ]
            },

            init() {
                normalizeLegacyVideoAttributes(this);
                this.on('change:attributes:data-link', () => updateEmbeddedVideoLink(this));
                this.on('change:attributes:data-yt-title', () => updateEmbeddedVideoTitle(this));
            }
        }
    });

    bindSwiperDoubleClick(editor);
}

export { commandId as swiperEditorCommandId };

function normalizeLegacyVideoAttributes(component) {
    const attributes = component.getAttributes();
    const changes = {};

    if (!attributes['data-link'] && attributes.link) {
        changes['data-link'] = attributes.link;
    }
    if (!attributes['data-yt-title'] && attributes.yttitle) {
        changes['data-yt-title'] = attributes.yttitle;
    }
    if (Object.keys(changes).length) {
        component.addAttributes(changes);
    }
    component.removeAttributes(['link', 'yttitle']);
    updateEmbeddedVideoLink(component);
    updateEmbeddedVideoTitle(component);
}

function updateEmbeddedVideoLink(component) {
    const attributes = component.getAttributes();
    const source = attributes['data-link'];
    const normalizedSource = normalizeEmbedUrl(source);

    if (source && normalizedSource && source !== normalizedSource) {
        component.addAttributes({ 'data-link': normalizedSource });
    }
}

function updateEmbeddedVideoTitle(component) {
    const title = component.getAttributes()['data-yt-title'];
    const image = component.find('img')[0];

    if (image && title) {
        image.addAttributes({ alt: `${title}的圖片` });
    }
}

function bindSwiperDoubleClick(editor) {
    const boundDocuments = new WeakSet();
    const bindDocument = frameEvent => {
        const frameDocument = frameEvent?.window?.document || editor.Canvas?.getDocument?.();
        if (!frameDocument || boundDocuments.has(frameDocument)) {
            return;
        }

        boundDocuments.add(frameDocument);
        frameDocument.addEventListener('dblclick', event => {
            const rootElement = findSwiperRootElement(event.target, frameDocument.body);
            if (!rootElement) {
                return;
            }

            const component = findComponentByElement(editor, rootElement);
            if (!component) {
                return;
            }

            event.preventDefault();
            event.stopImmediatePropagation();
            editor.select(component);
            editor.runCommand(commandId, { component });
        }, true);
    };

    editor.on('canvas:frame:load', bindDocument);
    editor.on('load', bindDocument);
}

function findSwiperRootElement(target, boundary) {
    let element = target?.nodeType === 1 ? target : target?.parentElement;

    while (element && element !== boundary) {
        if (isSwiperRootElement(element)) {
            return element;
        }
        element = element.parentElement;
    }

    return isSwiperRootElement(boundary) ? boundary : null;
}

function findComponentByElement(editor, element) {
    let selected = editor.getSelected?.();
    while (selected) {
        if (selected.getEl?.() === element) {
            return selected;
        }
        selected = selected.parent?.();
    }

    const wrapper = editor.DomComponents?.getWrapper?.();
    const stack = wrapper ? [wrapper] : [];
    while (stack.length) {
        const component = stack.pop();
        if (component.getEl?.() === element) {
            return component;
        }
        component.components?.().forEach(child => stack.push(child));
    }

    return null;
}
