export const faqComponentType = 'QA元件';
export const faqLockedComponentType = 'QA元件鎖定版型';

const lockedFaqClasses = ['collapse', 'card', 'fas', 'qa-bg'];

function isLockedFaqElement(element) {
    if (!element?.classList || !element.parentElement?.closest('.qa')) {
        return false;
    }

    return lockedFaqClasses.some(className => element.classList.contains(className));
}

function updateFaqAttributes(component, viewElement) {
    const componentId = component.get('ccid') || component.getId();
    if (!componentId) {
        return;
    }

    const contentId = `${componentId}_content`;
    const trigger = component.find('a.qa-bg')[0];
    const collapse = component.find('div.collapse')[0];
    const answer = component.find('.card-body')[0];

    if (!trigger || !collapse) {
        return;
    }

    trigger.addAttributes({
        href: `#${contentId}`,
        title: '展開問答',
        role: 'button',
        'aria-controls': contentId,
        'aria-expanded': 'false',
        'data-bs-toggle': 'collapse',
        'data-coker-faq-question': ''
    });
    collapse.addAttributes({ id: contentId });
    answer?.addAttributes({ 'data-coker-faq-answer': '' });

    // Keep Bootstrap from collapsing content while it is being edited.
    viewElement?.querySelector('a.qa-bg')?.setAttribute('data-bs-toggle', '');
}

export function faqComponentPlugin(editor) {
    editor.DomComponents.addType(faqComponentType, {
        isComponent(element) {
            return element.classList?.contains('qa')
                ? { type: faqComponentType, name: faqComponentType }
                : undefined;
        },
        view: {
            init() {
                this.listenTo(
                    this.model,
                    'change:attributes:id change:ccid',
                    this.updateFaqAttributes
                );
            },
            onRender() {
                this.updateFaqAttributes();
            },
            updateFaqAttributes() {
                updateFaqAttributes(this.model, this.el);
            }
        }
    });

    editor.DomComponents.addType(faqLockedComponentType, {
        isComponent(element) {
            return isLockedFaqElement(element)
                ? { type: faqLockedComponentType }
                : undefined;
        },
        model: {
            defaults: {
                hoverable: false,
                selectable: false,
                droppable: false,
                copyable: false,
                removable: false,
                editable: false
            }
        }
    });
}
