import componentInsertCss
    from '../../styles/componentInsert/Index.css?inline';

import {
    openComponentInsertModal
} from './Modal.js';


const canvasStyleId =
    'etherit-coker-component-insert-styles';

const editorUiAttribute =
    'data-coker-editor-ui';

const editorUiValue =
    'component-insert';

const maxResolveDepth =
    10;


/*
 * ============================================================
 * 這些元件不適合直接塞完整 Block 到裡面
 * ============================================================
 */

const unsafeBlockParentTags =
    new Set([
        'span',
        'p',
        'a',
        'button',
        'label',

        'strong',
        'em',
        'b',
        'i',
        'u',
        'small',
        'mark',

        'abbr',
        'cite',
        'code',
        'q',
        's',
        'sub',
        'sup',

        'h1',
        'h2',
        'h3',
        'h4',
        'h5',
        'h6'
    ]);


/*
 * HTML 本身不可作為一般 Container。
 */
const nonContainerTags =
    new Set([
        'img',
        'input',
        'textarea',
        'select',
        'option',

        'br',
        'hr',

        'meta',
        'link',
        'source',
        'track',

        'area',
        'base',
        'embed',
        'param',
        'wbr'
    ]);


/*
 * ============================================================
 * Plugin
 * ============================================================
 */

export function componentInsertPlugin(
    editor,
    options = {}
) {
    /*
     * =========================================================
     * State
     * =========================================================
     */

    let insertTarget =
        null;

    let hoveredComponent =
        null;

    let hoverClearTimer =
        null;

    let isInsertLineInteracting =
        false;

    let selectedComponent =
        null;


    /*
     * Hover After Line
     */
    let insertLineElement =
        null;


    /*
     * Large Selected UI
     */
    let selectedBeforeElement =
        null;

    let selectedInsideElement =
        null;

    let selectedAfterElement =
        null;


    /*
     * Compact Selected UI
     */
    let selectedCompactTriggerElement =
        null;

    let selectedCompactMenuElement =
        null;


    /*
     * Body Bottom
     */
    let bottomInsertElement =
        null;


    /*
     * RAF
     */
    let insertLineFrame =
        null;

    let selectedUiFrame =
        null;


    /*
     * Selected UI Retry
     */
    let selectedUiRetryCount = 0;
    const maxSelectedUiRetries = 5;

    /*
     * Canvas
     */
    let boundCanvasWindow =  null;
    let boundCanvasDocument = null;


    /*
     * Editing State
     */
    let isTextEditing = false;

    let isInsertModalOpen = false;


    /*
     * =========================================================
     * Basic Helpers
     * =========================================================
     */

    function getCanvasDocument() {
        return (
            editor.Canvas
                .getDocument?.() ||
            null
        );
    }


    function getCanvasWindow() {
        return (
            editor.Canvas
                .getWindow?.() ||
            null
        );
    }


    function getWrapperComponent() {
        return (
            editor.getWrapper?.() ||
            null
        );
    }


    function getComponentElement(
        component
    ) {
        if (!component) {
            return null;
        }

        return (
            component.getEl?.() ||
            component.getView?.()?.el ||
            null
        );
    }


    function getComponentTagName(
        component
    ) {
        if (!component) {
            return '';
        }

        const tagName =
            component.get?.(
                'tagName'
            );

        if (tagName) {
            return String(
                tagName
            ).toLowerCase();
        }

        return (
            getComponentElement(
                component
            )
                ?.tagName
                ?.toLowerCase() ||
            ''
        );
    }


    function isEditorUiElement(
        element
    ) {
        if (!element) {
            return false;
        }

        return Boolean(
            element.closest?.(
                `[${editorUiAttribute}="${editorUiValue}"]`
            )
        );
    }


    /*
     * =========================================================
     * Canvas CSS
     * =========================================================
     */

    function injectCanvasStyles() {
        const canvasDocument =
            getCanvasDocument();

        if (!canvasDocument?.head) {
            return;
        }

        let style =
            canvasDocument.getElementById(
                canvasStyleId
            );

        if (!style) {
            style =
                canvasDocument
                    .createElement(
                        'style'
                    );

            style.id =
                canvasStyleId;

            canvasDocument.head.append(
                style
            );
        }

        if (
            style.textContent !==
            componentInsertCss
        ) {
            style.textContent =
                componentInsertCss;
        }
    }


    /*
     * =========================================================
     * Hover Target Resolve
     * =========================================================
     */

    function shouldPromoteToParent(
        current,
        parent
    ) {
        if (
            !current ||
            !parent
        ) {
            return false;
        }

        const canvasWindow =
            getCanvasWindow();

        const parentElement =
            getComponentElement(
                parent
            );

        if (
            !canvasWindow ||
            !parentElement
        ) {
            return false;
        }

        const style =
            canvasWindow.getComputedStyle(
                parentElement
            );

        const display =
            style.display || '';

        const parentTag =
            getComponentTagName(
                parent
            );


        if (
            unsafeBlockParentTags.has(
                parentTag
            )
        ) {
            return true;
        }


        if (
            display === 'inline' ||
            display === 'inline-block' ||
            display === 'inline-flex' ||
            display === 'inline-grid'
        ) {
            return true;
        }


        return false;
    }


    function resolveInsertTarget(
        component
    ) {
        if (!component) {
            return null;
        }

        const wrapper =
            getWrapperComponent();

        if (
            component === wrapper
        ) {
            return null;
        }

        let current =
            component;

        let depth =
            0;

        while (
            current &&
            depth < maxResolveDepth
        ) {
            depth += 1;

            const parent =
                current.parent?.();

            if (!parent) {
                break;
            }

            if (
                parent === wrapper
            ) {
                break;
            }

            if (
                !shouldPromoteToParent(
                    current,
                    parent
                )
            ) {
                break;
            }

            current =
                parent;
        }

        return current;
    }


    /*
     * =========================================================
     * Can Contain Children
     * =========================================================
     */

    function canContainChildren(
        component
    ) {
        if (!component) {
            return false;
        }


        if (
            component ===
            getWrapperComponent()
        ) {
            return false;
        }


        const tagName =
            getComponentTagName(
                component
            );


        /*
         * 文字元件雖然 DOM 可以有 child，
         * 但不應讓使用者塞完整 GrapesJS Block。
         */
        if (
            unsafeBlockParentTags.has(
                tagName
            )
        ) {
            return false;
        }


        if (
            nonContainerTags.has(
                tagName
            )
        ) {
            return false;
        }


        /*
         * GrapesJS 明確禁止 Drop。
         */
        if (
            component.get?.(
                'droppable'
            ) === false
        ) {
            return false;
        }


        return Boolean(
            component.components?.()
        );
    }


    function canInsertInsideEmptyComponent(
        component
    ) {
        if (
            !canContainChildren(
                component
            )
        ) {
            return false;
        }

        const children =
            component.components?.();

        return Boolean(
            children &&
            children.length === 0
        );
    }


    /*
     * =========================================================
     * Compact / Large
     * =========================================================
     */

    function shouldUseCompactSelectedUi(
        rect
    ) {
        /*
         * H3、文字列、按鈕、小型元件
         * 不適合塞三顆按鈕。
         */
        return (
            rect.height < 110 ||
            rect.width < 260
        );
    }


    /*
     * =========================================================
     * Insert Position
     * =========================================================
     */

    function getInsertPosition(
        target,
        mode = 'after'
    ) {
        if (!target) {
            return null;
        }


        /*
         * Inside
         */
        if (
            mode === 'inside'
        ) {
            if (
                !canInsertInsideEmptyComponent(
                    target
                )
            ) {
                return null;
            }

            const children =
                target.components?.();

            if (!children) {
                return null;
            }

            return {
                mode:
                    'inside',

                parent:
                    target,

                at:
                    children.length,

                referenceComponent:
                    target
            };
        }


        /*
         * Before / After
         */
        const parent =
            target.parent?.();

        if (!parent) {
            return null;
        }

        const collection =
            parent.components?.();

        if (!collection) {
            return null;
        }

        const targetIndex =
            collection.indexOf?.(
                target
            );

        if (
            targetIndex === undefined ||
            targetIndex < 0
        ) {
            return null;
        }

        return {
            mode,

            parent,

            at:
                mode === 'before'
                    ? targetIndex
                    : targetIndex + 1,

            referenceComponent:
                target
        };
    }


    function getBottomInsertPosition() {
        const wrapper =
            getWrapperComponent();

        if (!wrapper) {
            return null;
        }

        const components =
            wrapper.components?.();

        if (!components) {
            return null;
        }

        const length =
            components.length;

        return {
            mode:
                'append',

            parent:
                wrapper,

            at:
                length,

            referenceComponent:
                length > 0
                    ? components.at(
                        length - 1
                    )
                    : null
        };
    }


    function refreshInsertPosition(
        position
    ) {
        if (!position) {
            return null;
        }

        if (
            position.mode === 'append'
        ) {
            return getBottomInsertPosition();
        }

        const referenceComponent =
            position.referenceComponent;

        if (!referenceComponent) {
            return null;
        }

        return getInsertPosition(
            referenceComponent,
            position.mode
        );
    }


    /*
     * =========================================================
     * Block Insert
     * =========================================================
     */

    function getBlockContent(
        block
    ) {
        if (!block) {
            return null;
        }

        return block.get?.(
            'content'
        );
    }


    function insertSelectedBlock(
        block,
        position
    ) {
        if (
            !block ||
            !position
        ) {
            return null;
        }

        if (
            block.get?.('disable') ===
            true
        ) {
            return null;
        }

        const currentPosition =
            refreshInsertPosition(
                position
            );

        if (!currentPosition) {
            console.warn(
                '[ComponentInsert] 插入位置已失效。'
            );

            return null;
        }

        const onClick =
            block.get?.('onClick');

        if (
            typeof onClick ===
            'function'
        ) {
            return onClick(
                block,
                editor,
                {
                    event: null,
                    position: currentPosition
                }
            );
        }

        const content =
            getBlockContent(
                block
            );

        if (
            content === null ||
            content === undefined
        ) {
            console.warn(
                '[ComponentInsert] Block 沒有 content:',
                block
            );

            return null;
        }

        const componentManager =
            editor.Components ||
            editor.DomComponents;

        const canMove =
            componentManager
                ?.canMove?.(
                    currentPosition.parent,
                    content,
                    currentPosition.at
                );

        if (
            canMove &&
            !canMove.result
        ) {
            console.warn(
                '[ComponentInsert] 這個元件不能插入到指定位置。',
                canMove
            );

            return null;
        }

        const inserted =
            currentPosition.parent
                ?.append?.(
                    content,
                    {
                        at:
                            currentPosition.at
                    }
                );


        let componentToSelect =
            inserted;

        if (
            Array.isArray(
                inserted
            )
        ) {
            componentToSelect =
                inserted[0] ||
                null;
        }


        if (
            componentToSelect
        ) {
            const shouldResetId =
                block.get?.('resetId') ===
                true;

            const shouldActivate =
                block.get?.('activate') ===
                true;

            const shouldSelect =
                block.get?.('select') !==
                false;

            if (shouldResetId) {
                componentToSelect.onAll?.(
                    component => {
                        component.resetId?.();
                    }
                );
            }

            queueMicrotask(
                () => {
                    try {
                        if (shouldSelect) {
                            editor.select(
                                componentToSelect,
                                {
                                    activate:
                                        shouldActivate,

                                    scroll:
                                        true
                                }
                            );
                        }
                        else if (shouldActivate) {
                            componentToSelect.trigger?.(
                                'active'
                            );
                        }
                    }
                    catch {
                        /*
                         * 不影響插入。
                         */
                    }
                }
            );
        }

        return inserted;
    }


    /*
     * =========================================================
     * Modal Flow
     * =========================================================
     */

    async function openInsertUi(
        position
    ) {
        if (
            !position ||
            isInsertModalOpen
        ) {
            return;
        }

        isInsertModalOpen =
            true;


        hideInsertLine();

        hideSelectedUi();

        closeCompactMenu();


        try {
            const selectedBlock =
                await openComponentInsertModal(
                    editor
                );

            if (!selectedBlock) {
                return;
            }

            insertSelectedBlock(
                selectedBlock,
                position
            );
        }
        finally {
            isInsertModalOpen =
                false;


            if (
                !isTextEditing &&
                selectedComponent
            ) {
                requestSelectedUiPosition();
            }
            else if (
                !isTextEditing &&
                insertTarget
            ) {
                requestInsertLinePosition();
            }


            ensureBottomInsert();
        }
    }


    /*
     * =========================================================
     * Hover Insert Line
     * =========================================================
     */

    function cancelHoverClear() {
        if (!hoverClearTimer) {
            return;
        }

        clearTimeout(
            hoverClearTimer
        );

        hoverClearTimer =
            null;
    }


    function clearHoverTarget() {
        cancelHoverClear();

        if (
            hoveredComponent ||
            isInsertLineInteracting
        ) {
            return;
        }

        insertTarget =
            null;

        hideInsertLine();
    }


    function scheduleHoverClear() {
        cancelHoverClear();

        hoverClearTimer =
            setTimeout(
                clearHoverTarget,
                100
            );
    }

    function createInsertLine() {
        const canvasDocument =
            getCanvasDocument();

        if (!canvasDocument?.body) {
            return null;
        }


        if (
            insertLineElement &&
            insertLineElement.isConnected
        ) {
            return insertLineElement;
        }


        const line =
            canvasDocument.createElement(
                'span'
            );

        line.className =
            'coker-component-insert-line';

        line.setAttribute(
            editorUiAttribute,
            editorUiValue
        );

        line.hidden =
            true;


        const leftLine =
            canvasDocument.createElement(
                'span'
            );

        leftLine.className =
            'coker-component-insert-line__line';


        const button =
            canvasDocument.createElement(
                'button'
            );

        button.type =
            'button';

        button.className =
            'coker-component-insert-line__button';

        button.title =
            '插入到此元件後方';


        const icon =
            canvasDocument.createElement(
                'span'
            );

        icon.className =
            'material-symbols-outlined';

        icon.textContent =
            'add';


        button.append(
            icon
        );


        const rightLine =
            canvasDocument.createElement(
                'span'
            );

        rightLine.className =
            'coker-component-insert-line__line';


        line.append(
            leftLine,
            button,
            rightLine
        );


        button.addEventListener(
            'pointerdown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'mousedown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'click',
            event => {
                event.preventDefault();
                event.stopPropagation();

                if (
                    isTextEditing ||
                    isInsertModalOpen
                ) {
                    return;
                }

                openInsertUi(
                    getInsertPosition(
                        insertTarget,
                        'after'
                    )
                );
            }
        );


        line.addEventListener(
            'pointerenter',
            () => {
                isInsertLineInteracting =
                    true;

                cancelHoverClear();
            }
        );


        line.addEventListener(
            'pointerleave',
            () => {
                isInsertLineInteracting =
                    false;

                scheduleHoverClear();
            }
        );


        canvasDocument.body.append(
            line
        );

        insertLineElement =
            line;

        return line;
    }


    function hideInsertLine() {
        if (!insertLineElement) {
            return;
        }

        insertLineElement.hidden =
            true;
    }


    function updateInsertLinePosition() {
        insertLineFrame =
            null;


        /*
         * Selected 模式優先。
         */
        if (
            selectedComponent ||
            isTextEditing ||
            isInsertModalOpen ||
            !insertTarget
        ) {
            hideInsertLine();

            return;
        }


        const targetElement =
            getComponentElement(
                insertTarget
            );

        if (
            !targetElement ||
            !targetElement.isConnected
        ) {
            hideInsertLine();

            return;
        }


        const rect =
            targetElement
                .getBoundingClientRect();

        if (
            rect.width <= 0 ||
            rect.height <= 0
        ) {
            hideInsertLine();

            return;
        }


        const line =
            createInsertLine();

        if (!line) {
            return;
        }


        const canvasWindow =
            getCanvasWindow();

        const viewportWidth =
            canvasWindow?.innerWidth ||
            0;


        let width =
            Math.max(
                80,
                rect.width
            );


        if (
            viewportWidth > 0
        ) {
            width =
                Math.min(
                    width,
                    viewportWidth - 16
                );
        }


        let left =
            rect.left;


        if (
            viewportWidth > 0 &&
            left + width >
            viewportWidth - 8
        ) {
            left =
                viewportWidth -
                width -
                8;
        }


        left =
            Math.max(
                8,
                left
            );


        const top =
            rect.bottom + 8;


        line.style.left =
            `${left}px`;

        line.style.top =
            `${top}px`;

        line.style.width =
            `${width}px`;

        line.hidden =
            false;
    }


    function requestInsertLinePosition() {
        if (
            selectedComponent ||
            isTextEditing ||
            isInsertModalOpen
        ) {
            hideInsertLine();

            return;
        }


        if (insertLineFrame) {
            return;
        }


        const canvasWindow =
            getCanvasWindow();

        if (!canvasWindow) {
            return;
        }


        insertLineFrame =
            canvasWindow
                .requestAnimationFrame(
                    updateInsertLinePosition
                );
    }


    function cancelInsertLineFrame() {
        if (!insertLineFrame) {
            return;
        }


        getCanvasWindow()
            ?.cancelAnimationFrame(
                insertLineFrame
            );


        insertLineFrame =
            null;
    }


    /*
     * =========================================================
     * Hover
     * =========================================================
     */

    function handleComponentHovered(
        component
    ) {
        cancelHoverClear();

        if (
            selectedComponent ||
            isTextEditing ||
            isInsertModalOpen ||
            !component
        ) {
            return;
        }


        const element =
            getComponentElement(
                component
            );


        if (
            element &&
            isEditorUiElement(
                element
            )
        ) {
            return;
        }


        const resolved =
            resolveInsertTarget(
                component
            );


        if (!resolved) {
            hoveredComponent =
                null;

            scheduleHoverClear();

            return;
        }

        hoveredComponent =
            component;


        if (
            insertTarget ===
            resolved
        ) {
            return;
        }


        insertTarget =
            resolved;


        requestInsertLinePosition();
    }


    function handleComponentUnhovered(
        component
    ) {
        if (
            component &&
            hoveredComponent !==
            component
        ) {
            return;
        }

        hoveredComponent =
            null;

        scheduleHoverClear();
    }


    /*
     * =========================================================
     * Selected Common Action
     * =========================================================
     */

    function createSelectedActionButton(
        className,
        label,
        mode
    ) {
        const canvasDocument =
            getCanvasDocument();

        if (!canvasDocument) {
            return null;
        }


        const button =
            canvasDocument
                .createElement(
                    'button'
                );


        button.type =
            'button';

        button.className =
            className;

        button.setAttribute(
            editorUiAttribute,
            editorUiValue
        );


        const icon =
            canvasDocument
                .createElement(
                    'span'
                );

        icon.className =
            'coker-component-insert-selected__icon';

        icon.textContent =
            '+';


        const text =
            canvasDocument
                .createElement(
                    'span'
                );

        text.className =
            'coker-component-insert-selected__text';

        text.textContent =
            label;


        button.append(
            icon,
            text
        );


        button.addEventListener(
            'pointerdown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'mousedown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'click',
            event => {
                event.preventDefault();
                event.stopPropagation();

                if (
                    !selectedComponent ||
                    isTextEditing ||
                    isInsertModalOpen
                ) {
                    return;
                }


                const position =
                    getInsertPosition(
                        selectedComponent,
                        mode
                    );


                if (!position) {
                    return;
                }


                openInsertUi(
                    position
                );
            }
        );


        return button;
    }


    /*
     * =========================================================
     * Large Selected UI
     * =========================================================
     */

    function createLargeSelectedUi() {
        const canvasDocument =
            getCanvasDocument();

        if (!canvasDocument?.body) {
            return;
        }


        if (
            !selectedBeforeElement ||
            !selectedBeforeElement.isConnected
        ) {
            selectedBeforeElement =
                createSelectedActionButton(
                    'coker-component-insert-selected coker-component-insert-selected--before',
                    '插入到前面',
                    'before'
                );

            canvasDocument.body.append(
                selectedBeforeElement
            );
        }


        if (
            !selectedInsideElement ||
            !selectedInsideElement.isConnected
        ) {
            selectedInsideElement =
                createSelectedActionButton(
                    'coker-component-insert-selected coker-component-insert-selected--inside',
                    '插入元件到裡面',
                    'inside'
                );

            canvasDocument.body.append(
                selectedInsideElement
            );
        }


        if (
            !selectedAfterElement ||
            !selectedAfterElement.isConnected
        ) {
            selectedAfterElement =
                createSelectedActionButton(
                    'coker-component-insert-selected coker-component-insert-selected--after',
                    '插入到後面',
                    'after'
                );

            canvasDocument.body.append(
                selectedAfterElement
            );
        }
    }


    function hideLargeSelectedUi() {
        if (selectedBeforeElement) {
            selectedBeforeElement.hidden =
                true;
        }

        if (selectedInsideElement) {
            selectedInsideElement.hidden =
                true;
        }

        if (selectedAfterElement) {
            selectedAfterElement.hidden =
                true;
        }
    }


    /*
     * =========================================================
     * Compact UI
     * =========================================================
     */

    function createCompactTrigger() {
        const canvasDocument =
            getCanvasDocument();

        if (!canvasDocument?.body) {
            return null;
        }


        if (
            selectedCompactTriggerElement &&
            selectedCompactTriggerElement.isConnected
        ) {
            return selectedCompactTriggerElement;
        }


        const button =
            canvasDocument.createElement(
                'button'
            );


        button.type =
            'button';

        button.className =
            'coker-component-insert-compact-trigger';

        button.setAttribute(
            editorUiAttribute,
            editorUiValue
        );


        const icon =
            canvasDocument.createElement(
                'span'
            );

        icon.className =
            'coker-component-insert-compact-trigger__icon';

        icon.textContent =
            '+';


        const text =
            canvasDocument.createElement(
                'span'
            );

        text.className =
            'coker-component-insert-compact-trigger__text';

        text.textContent =
            '插入';


        button.append(
            icon,
            text
        );


        button.addEventListener(
            'pointerdown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'mousedown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'click',
            event => {
                event.preventDefault();
                event.stopPropagation();

                if (
                    selectedCompactMenuElement &&
                    !selectedCompactMenuElement.hidden
                ) {
                    closeCompactMenu();

                    return;
                }


                openCompactMenu();
            }
        );


        canvasDocument.body.append(
            button
        );


        selectedCompactTriggerElement =
            button;


        return button;
    }


    function createCompactMenuItem(
        label,
        description,
        mode
    ) {
        const canvasDocument =
            getCanvasDocument();

        const button =
            canvasDocument.createElement(
                'button'
            );


        button.type =
            'button';

        button.className =
            'coker-component-insert-compact-menu__item';


        const title =
            canvasDocument.createElement(
                'span'
            );

        title.className =
            'coker-component-insert-compact-menu__item-title';

        title.textContent =
            label;


        const desc =
            canvasDocument.createElement(
                'span'
            );

        desc.className =
            'coker-component-insert-compact-menu__item-description';

        desc.textContent =
            description;


        button.append(
            title,
            desc
        );


        button.addEventListener(
            'pointerdown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'click',
            event => {
                event.preventDefault();
                event.stopPropagation();


                const position =
                    getInsertPosition(
                        selectedComponent,
                        mode
                    );


                if (!position) {
                    return;
                }


                closeCompactMenu();

                openInsertUi(
                    position
                );
            }
        );


        return button;
    }


    function openCompactMenu() {
        if (
            !selectedComponent ||
            !selectedCompactTriggerElement
        ) {
            return;
        }


        const canvasDocument =
            getCanvasDocument();

        const canvasWindow =
            getCanvasWindow();


        if (
            !canvasDocument?.body ||
            !canvasWindow
        ) {
            return;
        }


        closeCompactMenu();


        const menu =
            canvasDocument.createElement(
                'div'
            );


        menu.className =
            'coker-component-insert-compact-menu';

        menu.setAttribute(
            editorUiAttribute,
            editorUiValue
        );


        menu.append(
            createCompactMenuItem(
                '插入到前面',
                '與目前元件同層，放在目前元件之前',
                'before'
            )
        );


        if (
            canInsertInsideEmptyComponent(
                selectedComponent
            )
        ) {
            menu.append(
                createCompactMenuItem(
                    '插入到裡面',
                    '成為目前元件的最後一個子元件',
                    'inside'
                )
            );
        }


        menu.append(
            createCompactMenuItem(
                '插入到後面',
                '與目前元件同層，放在目前元件之後',
                'after'
            )
        );


        canvasDocument.body.append(
            menu
        );


        selectedCompactMenuElement =
            menu;


        /*
         * 取得 Trigger 位置。
         */
        const triggerRect =
            selectedCompactTriggerElement
                .getBoundingClientRect();


        const menuRect =
            menu.getBoundingClientRect();


        let left =
            triggerRect.right -
            menuRect.width;


        let top =
            triggerRect.bottom + 6;


        /*
         * 右側超出。
         */
        if (
            left +
            menuRect.width >
            canvasWindow.innerWidth - 8
        ) {
            left =
                canvasWindow.innerWidth -
                menuRect.width -
                8;
        }


        /*
         * 下方超出 → 改開在上面。
         */
        if (
            top +
            menuRect.height >
            canvasWindow.innerHeight - 8
        ) {
            top =
                triggerRect.top -
                menuRect.height -
                6;
        }


        left =
            Math.max(
                8,
                left
            );


        top =
            Math.max(
                8,
                top
            );


        menu.style.left =
            `${left}px`;

        menu.style.top =
            `${top}px`;
    }


    function closeCompactMenu() {
        selectedCompactMenuElement
            ?.remove();


        selectedCompactMenuElement =
            null;
    }


    function hideCompactUi() {
        if (
            selectedCompactTriggerElement
        ) {
            selectedCompactTriggerElement.hidden =
                true;
        }


        closeCompactMenu();
    }


    /*
     * =========================================================
     * Selected UI
     * =========================================================
     */

    function hideSelectedUi() {
        hideLargeSelectedUi();

        hideCompactUi();
    }


    function removeSelectedUi() {
        selectedBeforeElement
            ?.remove();

        selectedInsideElement
            ?.remove();

        selectedAfterElement
            ?.remove();

        selectedCompactTriggerElement
            ?.remove();


        closeCompactMenu();


        selectedBeforeElement =
            null;

        selectedInsideElement =
            null;

        selectedAfterElement =
            null;

        selectedCompactTriggerElement =
            null;
    }


    function updateSelectedUiPosition() {
        /*
         * RAF 已經執行，
         * 先清掉 reference，
         * 這樣必要時可以再次 request。
         */
        selectedUiFrame =
            null;

        /*
         * =====================================================
         * 基本狀態檢查
         * =====================================================
         */

        if (
            !selectedComponent ||
            isTextEditing ||
            isInsertModalOpen
        ) {
            hideSelectedUi();

            return;
        }

        if (
            selectedComponent ===
            getWrapperComponent()
        ) {
            hideSelectedUi();

            return;
        }

        const canvasWindow =
            getCanvasWindow();

        if (!canvasWindow) {
            hideSelectedUi();

            return;
        }

        const element =
            getComponentElement(
                selectedComponent
            );

        /*
         * 某些 GrapesJS Component 在 selected 當下，
         * view / DOM 可能尚未 ready。
         *
         * 不直接永久放棄，
         * 最多 retry 幾個 frame。
         */
        if (
            !element ||
            !element.isConnected
        ) {
            hideSelectedUi();

            if (
                selectedUiRetryCount <
                maxSelectedUiRetries
            ) {
                selectedUiRetryCount += 1;

                requestSelectedUiPosition();
            }

            return;
        }

        const rect =
            element.getBoundingClientRect();

        /*
         * DOM 已存在，
         * 但 layout 尚未完成時，
         * rect 有可能暫時是 0。
         */
        if (
            rect.width <= 0 ||
            rect.height <= 0
        ) {
            hideSelectedUi();

            if (
                selectedUiRetryCount <
                maxSelectedUiRetries
            ) {
                selectedUiRetryCount += 1;

                requestSelectedUiPosition();
            }

            return;
        }

        /*
         * 已成功取得有效位置，
         * retry counter 歸零。
         */
        selectedUiRetryCount =
            0;

        /*
         * =====================================================
         * Compact 模式
         * =====================================================
         *
         * 小型 / 矮元件：
         *
         * 不顯示 Before / Inside / After 三顆，
         * 避免像 H3 那樣全部疊在一起。
         *
         * 只顯示一顆：
         *
         *      + 插入
         *
         * 放在 selected 元件外側。
         */

        if (
            shouldUseCompactSelectedUi(
                rect
            )
        ) {
            /*
             * Large UI 全部關閉。
             */
            hideLargeSelectedUi();

            const trigger =
                createCompactTrigger();

            if (!trigger) {
                return;
            }

            trigger.hidden =
                false;

            /*
             * Compact Trigger 尺寸。
             *
             * 要跟 CSS 大致一致，
             * 這裡主要用於 viewport clamp。
             */
            const triggerWidth =
                76;

            const triggerHeight =
                30;

            const gap =
                8;

            /*
             * 預設放在元件下方外側中央。
             */
            let left =
                rect.left +
                rect.width / 2;

            let top =
                rect.bottom +
                gap +
                triggerHeight / 2;

            /*
             * 如果下方空間不足，
             * 改放到元件上方外側。
             */
            if (
                top +
                triggerHeight / 2 >
                canvasWindow.innerHeight - 8
            ) {
                top =
                    rect.top -
                    gap -
                    triggerHeight / 2;
            }

            /*
             * 避免左右超出 Canvas viewport。
             */
            left =
                Math.max(
                    triggerWidth / 2 + 8,
                    Math.min(
                        left,
                        canvasWindow.innerWidth -
                        triggerWidth / 2 -
                        8
                    )
                );

            trigger.style.left =
                `${left}px`;

            trigger.style.top =
                `${top}px`;

            return;
        }

        /*
         * =====================================================
         * Large 模式
         * =====================================================
         *
         * 大型 Container / Cell / Section：
         *
         *     + 前面
         *
         * ┌────────────────────┐
         * │                    │
         * │   + 插入到裡面     │
         * │                    │
         * └────────────────────┘
         *
         *     + 後面
         */

        hideCompactUi();

        createLargeSelectedUi();

        /*
         * createLargeSelectedUi()
         * 理論上會建立這三個 element。
         */
        if (
            !selectedBeforeElement ||
            !selectedInsideElement ||
            !selectedAfterElement
        ) {
            return;
        }

        /*
         * =====================================================
         * Before
         * =====================================================
         */

        selectedBeforeElement.hidden =
            false;

        selectedBeforeElement.style.left =
            `${rect.left + rect.width / 2}px`;

        selectedBeforeElement.style.top =
            `${rect.top + 18}px`;

        /*
         * =====================================================
         * After
         * =====================================================
         */

        selectedAfterElement.hidden =
            false;

        selectedAfterElement.style.left =
            `${rect.left + rect.width / 2}px`;

        selectedAfterElement.style.top =
            `${rect.bottom - 18}px`;

        /*
         * =====================================================
         * Inside
         * =====================================================
         */

        const allowInside =
            canInsertInsideEmptyComponent(
                selectedComponent
            );

        selectedInsideElement.hidden =
            !allowInside;

        if (allowInside) {
            selectedInsideElement.style.left =
                `${rect.left + rect.width / 2}px`;

            selectedInsideElement.style.top =
                `${rect.top + rect.height / 2}px`;
        }
    }


    function requestSelectedUiPosition() {
        if (
            !selectedComponent ||
            isTextEditing ||
            isInsertModalOpen
        ) {
            hideSelectedUi();

            return;
        }

        /*
         * 已經有一個 RAF 在等待，
         * 不重複排程。
         */
        if (selectedUiFrame) {
            return;
        }

        const canvasWindow =
            getCanvasWindow();

        if (!canvasWindow) {
            return;
        }

        selectedUiFrame =
            canvasWindow.requestAnimationFrame(
                updateSelectedUiPosition
            );
    }


    function cancelSelectedUiFrame() {
        if (!selectedUiFrame) {
            return;
        }


        getCanvasWindow()
            ?.cancelAnimationFrame(
                selectedUiFrame
            );


        selectedUiFrame =
            null;
    }


    /*
     * =========================================================
     * Selected Events
     * =========================================================
     */

    function handleComponentSelected(
        component
    ) {
        /*
         * Wrapper 不顯示 Selected Insert UI。
         */
        if (
            !component ||
            component ===
            getWrapperComponent()
        ) {
            selectedComponent =
                null;

            selectedUiRetryCount =
                0;

            hideSelectedUi();

            return;
        }

        selectedComponent =
            component;

        /*
         * 新的 Component Selection，
         * retry 計數重新開始。
         */
        selectedUiRetryCount =
            0;

        /*
         * Selected 模式優先於 Hover After Line。
         */
        hideInsertLine();

        /*
         * 如果上一個 Compact Menu 還開著，
         * 先關閉。
         */
        closeCompactMenu();

        /*
         * 第一次定位。
         */
        requestSelectedUiPosition();

        /*
         * GrapesJS 某些 Component：
         *
         * - Cell
         * - Row
         * - Table 類
         * - 剛 mount 的元件
         *
         * selected event 發生後，
         * View/Layout 有可能晚一兩個 frame 才穩定。
         *
         * 因此額外補兩個 frame。
         */
        const canvasWindow =
            getCanvasWindow();

        if (!canvasWindow) {
            return;
        }

        canvasWindow.requestAnimationFrame(
            () => {
                /*
                 * Selection 若已改變，
                 * 不再替舊 component 更新。
                 */
                if (
                    selectedComponent !==
                    component
                ) {
                    return;
                }

                requestSelectedUiPosition();

                canvasWindow.requestAnimationFrame(
                    () => {
                        if (
                            selectedComponent !==
                            component
                        ) {
                            return;
                        }

                        requestSelectedUiPosition();
                    }
                );
            }
        );
    }


    function handleComponentDeselected(
        component
    ) {
        if (!selectedComponent) {
            return;
        }

        /*
         * 某些 GrapesJS 版本
         * deselected callback 可能沒有 component。
         */
        if (
            component &&
            component !==
            selectedComponent
        ) {
            return;
        }

        selectedComponent =
            null;

        selectedUiRetryCount =
            0;

        cancelSelectedUiFrame();

        hideSelectedUi();

        /*
         * Selected 結束，
         * 恢復原本 Hover After 模式。
         */
        if (
            insertTarget &&
            !isTextEditing &&
            !isInsertModalOpen
        ) {
            requestInsertLinePosition();
        }
    }


    /*
     * =========================================================
     * Text Editing
     * =========================================================
     */

    function handleTextEditingStart() {
        if (isTextEditing) {
            return;
        }


        isTextEditing =
            true;


        cancelInsertLineFrame();

        cancelSelectedUiFrame();


        hideInsertLine();

        hideSelectedUi();
    }


    function handleTextEditingEnd() {
        if (!isTextEditing) {
            return;
        }


        isTextEditing =
            false;


        if (
            selectedComponent &&
            !isInsertModalOpen
        ) {
            requestSelectedUiPosition();

            return;
        }


        if (
            insertTarget &&
            !isInsertModalOpen
        ) {
            requestInsertLinePosition();
        }
    }


    /*
     * =========================================================
     * Bottom Insert
     * =========================================================
     */

    function createBottomInsert() {
        const canvasDocument =
            getCanvasDocument();


        if (!canvasDocument?.body) {
            return null;
        }


        if (
            bottomInsertElement &&
            bottomInsertElement.isConnected
        ) {
            if (
                bottomInsertElement !==
                canvasDocument.body
                    .lastElementChild
            ) {
                canvasDocument.body.append(
                    bottomInsertElement
                );
            }


            return bottomInsertElement;
        }


        const container =
            canvasDocument.createElement(
                'div'
            );


        container.className =
            'coker-component-insert-bottom';


        container.setAttribute(
            editorUiAttribute,
            editorUiValue
        );


        const button =
            canvasDocument.createElement(
                'button'
            );


        button.type =
            'button';


        button.className =
            'coker-component-insert-bottom__button';


        button.title =
            '插入元件到頁面最後';


        const iconWrapper =
            canvasDocument.createElement(
                'span'
            );


        iconWrapper.className =
            'coker-component-insert-bottom__icon';


        const icon =
            canvasDocument.createElement(
                'span'
            );


        icon.className =
            'material-symbols-outlined';


        icon.textContent =
            'add';


        iconWrapper.append(
            icon
        );


        const textWrapper =
            canvasDocument.createElement(
                'span'
            );


        textWrapper.className =
            'coker-component-insert-bottom__text';


        const title =
            canvasDocument.createElement(
                'span'
            );


        title.className =
            'coker-component-insert-bottom__title';


        title.textContent =
            '插入元件到最後';


        const description =
            canvasDocument.createElement(
                'span'
            );


        description.className =
            'coker-component-insert-bottom__description';


        description.textContent =
            '在目前頁面內容的最下方新增元件';


        textWrapper.append(
            title,
            description
        );


        button.append(
            iconWrapper,
            textWrapper
        );


        button.addEventListener(
            'pointerdown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'mousedown',
            event => {
                event.preventDefault();
                event.stopPropagation();
            }
        );


        button.addEventListener(
            'click',
            event => {
                event.preventDefault();
                event.stopPropagation();


                if (
                    isInsertModalOpen
                ) {
                    return;
                }


                openInsertUi(
                    getBottomInsertPosition()
                );
            }
        );


        container.append(
            button
        );


        canvasDocument.body.append(
            container
        );


        bottomInsertElement =
            container;


        return container;
    }


    function ensureBottomInsert() {
        createBottomInsert();
    }


    /*
     * =========================================================
     * Canvas Global Pointer
     * =========================================================
     */

    function handleCanvasPointerDown(
        event
    ) {
        /*
         * Compact Menu 外點擊即關閉。
         */
        if (
            selectedCompactMenuElement &&
            !event.target
                ?.closest?.(
                    '.coker-component-insert-compact-menu'
                ) &&
            !event.target
                ?.closest?.(
                    '.coker-component-insert-compact-trigger'
                )
        ) {
            closeCompactMenu();
        }
    }


    /*
     * =========================================================
     * Canvas Scroll / Resize
     * =========================================================
     */

    function handleCanvasScroll() {
        closeCompactMenu();


        if (
            isTextEditing ||
            isInsertModalOpen
        ) {
            return;
        }


        if (
            selectedComponent
        ) {
            requestSelectedUiPosition();
        }
        else if (
            insertTarget
        ) {
            requestInsertLinePosition();
        }
    }


    function handleCanvasResize() {
        closeCompactMenu();


        if (
            isTextEditing ||
            isInsertModalOpen
        ) {
            return;
        }


        if (
            selectedComponent
        ) {
            requestSelectedUiPosition();
        }
        else if (
            insertTarget
        ) {
            requestInsertLinePosition();
        }
    }


    function unbindCanvasEvents() {
        if (
            boundCanvasWindow
        ) {
            boundCanvasWindow
                .removeEventListener(
                    'scroll',
                    handleCanvasScroll
                );


            boundCanvasWindow
                .removeEventListener(
                    'resize',
                    handleCanvasResize
                );
        }


        if (
            boundCanvasDocument
        ) {
            boundCanvasDocument
                .removeEventListener(
                    'pointerdown',
                    handleCanvasPointerDown,
                    true
                );
        }


        boundCanvasWindow =
            null;


        boundCanvasDocument =
            null;
    }


    function bindCanvasEvents() {
        unbindCanvasEvents();


        const canvasWindow =
            getCanvasWindow();


        const canvasDocument =
            getCanvasDocument();


        if (
            !canvasWindow ||
            !canvasDocument
        ) {
            return;
        }


        boundCanvasWindow =
            canvasWindow;


        boundCanvasDocument =
            canvasDocument;


        canvasWindow.addEventListener(
            'scroll',
            handleCanvasScroll,
            {
                passive:
                    true
            }
        );


        canvasWindow.addEventListener(
            'resize',
            handleCanvasResize,
            {
                passive:
                    true
            }
        );


        canvasDocument.addEventListener(
            'pointerdown',
            handleCanvasPointerDown,
            true
        );
    }


    /*
     * =========================================================
     * Reset
     * =========================================================
     */

    function resetCanvasUi() {
        cancelHoverClear();

        cancelInsertLineFrame();

        cancelSelectedUiFrame();

        unbindCanvasEvents();


        insertTarget =
            null;

        hoveredComponent =
            null;

        isInsertLineInteracting =
            false;

        selectedComponent =
            null;


        isTextEditing =
            false;

        isInsertModalOpen =
            false;


        insertLineElement =
            null;

        bottomInsertElement =
            null;


        selectedBeforeElement =
            null;

        selectedInsideElement =
            null;

        selectedAfterElement =
            null;


        selectedCompactTriggerElement =
            null;

        selectedCompactMenuElement =
            null;


        injectCanvasStyles();

        bindCanvasEvents();

        ensureBottomInsert();
    }


    /*
     * =========================================================
     * Init
     * =========================================================
     */

    injectCanvasStyles();

    bindCanvasEvents();


    /*
     * =========================================================
     * GrapesJS Events
     * =========================================================
     */

    editor.on(
        'load',
        () => {
            injectCanvasStyles();

            bindCanvasEvents();

            ensureBottomInsert();
        }
    );


    editor.on(
        'canvas:frame:load',
        () => {
            resetCanvasUi();
        }
    );


    /*
     * Hover
     */
    editor.on(
        'component:hovered',
        component => {
            handleComponentHovered(
                component
            );
        }
    );


    editor.on(
        'component:unhovered',
        component => {
            handleComponentUnhovered(
                component
            );
        }
    );


    /*
     * Selected
     */
    editor.on(
        'component:selected',
        component => {
            handleComponentSelected(
                component
            );
        }
    );


    editor.on(
        'component:deselected',
        component => {
            handleComponentDeselected(
                component
            );
        }
    );


    /*
     * RTE
     */
    editor.on(
        'rte:enable',
        () => {
            handleTextEditingStart();
        }
    );


    editor.on(
        'rte:disable',
        () => {
            handleTextEditingEnd();
        }
    );


    /*
     * Component Lifecycle
     */
    editor.on(
        'component:mount',
        () => {
            queueMicrotask(
                ensureBottomInsert
            );
        }
    );


    editor.on(
        'component:add',
        () => {
            queueMicrotask(
                ensureBottomInsert
            );


            if (
                selectedComponent
            ) {
                queueMicrotask(
                    requestSelectedUiPosition
                );
            }
        }
    );


    editor.on(
        'component:update',
        component => {
            if (
                component ===
                selectedComponent
            ) {
                requestSelectedUiPosition();
            }
        }
    );


    editor.on(
        'component:remove',
        component => {
            if (
                component ===
                hoveredComponent
            ) {
                hoveredComponent =
                    null;

                scheduleHoverClear();
            }

            if (
                component ===
                insertTarget
            ) {
                insertTarget =
                    null;

                hideInsertLine();
            }


            if (
                component ===
                selectedComponent
            ) {
                selectedComponent =
                    null;

                hideSelectedUi();
            }


            queueMicrotask(
                ensureBottomInsert
            );
        }
    );


    /*
     * Device
     */
    editor.on(
        'change:device',
        () => {
            closeCompactMenu();


            if (
                selectedComponent &&
                !isTextEditing &&
                !isInsertModalOpen
            ) {
                requestSelectedUiPosition();
            }
            else if (
                insertTarget &&
                !isTextEditing &&
                !isInsertModalOpen
            ) {
                requestInsertLinePosition();
            }


            ensureBottomInsert();
        }
    );


    /*
     * Destroy
     */
    editor.on(
        'destroy',
        () => {
            cancelHoverClear();

            cancelInsertLineFrame();

            cancelSelectedUiFrame();

            unbindCanvasEvents();


            insertLineElement
                ?.remove();


            bottomInsertElement
                ?.remove();


            removeSelectedUi();


            insertLineElement =
                null;

            bottomInsertElement =
                null;


            insertTarget =
                null;

            hoveredComponent =
                null;

            isInsertLineInteracting =
                false;

            selectedComponent =
                null;


            isTextEditing =
                false;

            isInsertModalOpen =
                false;
        }
    );
}
