/*
 * ============================================================
 * Component Insert Modal
 * ============================================================
 */

const modalClassName =
    'coker-component-insert-modal';

const modalTitle =
    '插入元件';

const uncategorizedId =
    '__coker_uncategorized__';

const uncategorizedLabel =
    '未分類';


/*
 * ============================================================
 * SVG Icons
 * ============================================================
 */

const icons = {
    search: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <circle cx="11" cy="11" r="7"></circle>
            <path d="m20 20-4-4"></path>
        </svg>
    `,

    back: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M19 12H5"></path>
            <path d="m12 19-7-7 7-7"></path>
        </svg>
    `,

    folder: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M3 6.5h6l2 2h10v10H3z"></path>
        </svg>
    `,

    chevronRight: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="m9 18 6-6-6-6"></path>
        </svg>
    `,

    add: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M12 5v14"></path>
            <path d="M5 12h14"></path>
        </svg>
    `,

    component: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <rect x="4" y="4" width="7" height="7" rx="1"></rect>
            <rect x="13" y="4" width="7" height="7" rx="1"></rect>
            <rect x="4" y="13" width="7" height="7" rx="1"></rect>
            <rect x="13" y="13" width="7" height="7" rx="1"></rect>
        </svg>
    `,

    empty: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <circle cx="11" cy="11" r="7"></circle>
            <path d="m20 20-4-4"></path>
            <path d="M8.5 11h5"></path>
        </svg>
    `,

    zoom: `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <circle cx="11" cy="11" r="6"></circle>
            <path d="m20 20-4.3-4.3"></path>
            <path d="M11 8v6"></path>
            <path d="M8 11h6"></path>
        </svg>
    `
};


/*
 * ============================================================
 * Generic Helpers
 * ============================================================
 */

function normalizeText(value) {
    if (
        value === null ||
        value === undefined
    ) {
        return '';
    }

    if (typeof value === 'string') {
        return value.trim();
    }

    return String(value).trim();
}


function createElement(
    documentRef,
    tagName,
    className = null
) {
    const element =
        documentRef.createElement(
            tagName
        );

    if (className) {
        element.className =
            className;
    }

    return element;
}


function createSvgIcon(
    documentRef,
    svg,
    className
) {
    const wrapper =
        createElement(
            documentRef,
            'span',
            className
        );

    wrapper.innerHTML =
        svg;

    return wrapper;
}


function setSafeMarkup(
    element,
    markup
) {
    const documentRef =
        element.ownerDocument;

    const template =
        documentRef.createElement(
            'template'
        );

    template.innerHTML =
        String(markup || '');

    template.content
        .querySelectorAll(
            'script, iframe, object, embed, link, meta, base, form'
        )
        .forEach(
            node => node.remove()
        );

    template.content
        .querySelectorAll('*')
        .forEach(
            node => {
                for (
                    const attribute of
                    Array.from(node.attributes)
                ) {
                    const name =
                        attribute.name
                            .toLowerCase();

                    const value =
                        attribute.value
                            .trim();

                    if (
                        name.startsWith('on') ||
                        name === 'srcdoc' ||
                        (
                            [
                                'href',
                                'src',
                                'xlink:href'
                            ].includes(name) &&
                            /^\s*(?:javascript|vbscript):/i
                                .test(value)
                        )
                    ) {
                        node.removeAttribute(
                            attribute.name
                        );
                    }
                }
            }
        );

    element.replaceChildren(
        template.content.cloneNode(
            true
        )
    );
}


/*
 * ============================================================
 * Block Helpers
 * ============================================================
 */

function getBlockId(block) {
    if (!block) {
        return '';
    }

    return normalizeText(
        block.get?.('id') ||
        block.id
    );
}


function getBlockLabel(block) {
    if (!block) {
        return '未命名元件';
    }

    const label =
        block.get?.('label');

    if (
        typeof label === 'string'
    ) {
        return (
            label.trim() ||
            getBlockId(block) ||
            '未命名元件'
        );
    }

    return (
        normalizeText(label) ||
        getBlockId(block) ||
        '未命名元件'
    );
}


function getBlockMedia(block) {
    if (!block) {
        return '';
    }

    return (
        block.get?.('media') ||
        ''
    );
}


function getBlockKeywords(block) {
    if (!block) {
        return '';
    }

    const keywords =
        block.get?.('keywords');

    if (Array.isArray(keywords)) {
        return keywords.join(' ');
    }

    return normalizeText(
        keywords
    );
}


/*
 * ============================================================
 * Category Helpers
 * ============================================================
 */

function getBlockCategoryRaw(block) {
    if (!block) {
        return null;
    }

    return (
        block.get?.('category') ||
        null
    );
}


function getBlockCategoryId(block) {
    const category =
        getBlockCategoryRaw(
            block
        );

    if (!category) {
        return uncategorizedId;
    }

    if (
        typeof category === 'string'
    ) {
        return (
            category.trim() ||
            uncategorizedId
        );
    }

    const id =
        category.get?.('id');

    if (id) {
        return normalizeText(id);
    }

    if (category.id) {
        return normalizeText(
            category.id
        );
    }

    const label =
        category.get?.('label');

    if (label) {
        return normalizeText(
            label
        );
    }

    return (
        normalizeText(category) ||
        uncategorizedId
    );
}


function getBlockCategoryLabel(block) {
    const category =
        getBlockCategoryRaw(
            block
        );

    if (!category) {
        return uncategorizedLabel;
    }

    if (
        typeof category === 'string'
    ) {
        return (
            category.trim() ||
            uncategorizedLabel
        );
    }

    const label =
        category.get?.('label');

    if (label) {
        return normalizeText(
            label
        );
    }

    const id =
        category.get?.('id');

    if (id) {
        return normalizeText(id);
    }

    if (category.id) {
        return normalizeText(
            category.id
        );
    }

    return (
        normalizeText(category) ||
        uncategorizedLabel
    );
}


/*
 * ============================================================
 * Search
 * ============================================================
 */

function getBlockSearchText(block) {
    return [
        getBlockLabel(block),
        getBlockId(block),
        getBlockCategoryLabel(block),
        getBlockKeywords(block)
    ]
        .filter(Boolean)
        .join(' ')
        .toLocaleLowerCase();
}


/*
 * ============================================================
 * Block Collection
 * ============================================================
 */

function getAllBlocks(editor) {
    const collection =
        editor.BlockManager
            ?.getAll?.();

    if (!collection) {
        return [];
    }

    if (
        Array.isArray(collection)
    ) {
        return collection.filter(
            block =>
                block?.get?.('disable') !==
                true
        );
    }

    if (
        Array.isArray(
            collection.models
        )
    ) {
        return collection.models.filter(
            block =>
                block?.get?.('disable') !==
                true
        );
    }

    if (
        typeof collection.toArray ===
        'function'
    ) {
        return collection.toArray().filter(
            block =>
                block?.get?.('disable') !==
                true
        );
    }

    return [];
}


/*
 * ============================================================
 * Category Collection
 * ============================================================
 */

function buildCategories(blocks) {
    const categoryMap =
        new Map();

    for (const block of blocks) {
        const id =
            getBlockCategoryId(
                block
            );

        const label =
            getBlockCategoryLabel(
                block
            );

        if (
            !categoryMap.has(id)
        ) {
            categoryMap.set(
                id,
                {
                    id,
                    label,
                    blocks: []
                }
            );
        }

        categoryMap
            .get(id)
            .blocks
            .push(block);
    }

    return Array.from(
        categoryMap.values()
    );
}


/*
 * ============================================================
 * Image Helpers
 * ============================================================
 */

function getBlockImageInfo(
    documentRef,
    block
) {
    const media =
        getBlockMedia(
            block
        );

    if (!media) {
        return null;
    }

    const wrapper =
        documentRef.createElement(
            'div'
        );

    setSafeMarkup(
        wrapper,
        media
    );

    const image =
        wrapper.querySelector(
            'img'
        );

    if (!image) {
        return null;
    }

    const src =
        image.getAttribute(
            'src'
        );

    if (!src) {
        return null;
    }

    return {
        src,

        alt:
            image.getAttribute(
                'alt'
            ) ||
            getBlockLabel(
                block
            )
    };
}


/*
 * ============================================================
 * Block Media
 * ============================================================
 */

function renderBlockMedia(
    documentRef,
    mediaContainer,
    block
) {
    const media =
        getBlockMedia(
            block
        );

    mediaContainer.innerHTML =
        '';

    mediaContainer.classList.remove(
        `${modalClassName}__block-media--custom`,
        `${modalClassName}__block-media--fallback`
    );

    if (media) {
        setSafeMarkup(
            mediaContainer,
            media
        );

        mediaContainer.classList.add(
            `${modalClassName}__block-media--custom`
        );

        return;
    }

    mediaContainer.classList.add(
        `${modalClassName}__block-media--fallback`
    );

    mediaContainer.append(
        createSvgIcon(
            documentRef,
            icons.component,
            `${modalClassName}__fallback-icon`
        )
    );
}


/*
 * ============================================================
 * Main API
 * ============================================================
 */

export function openComponentInsertModal(
    editor
) {
    return new Promise(
        resolve => {
            /*
             * =================================================
             * GrapesJS Modal
             * =================================================
             */

            const modal =
                editor.Modal;

            if (!modal) {
                console.error(
                    '[ComponentInsert] GrapesJS Modal 不存在。'
                );

                resolve(null);

                return;
            }

            const modalContainer =
                modal.getContainer?.();

            const documentRef =
                modalContainer
                    ?.ownerDocument ||
                window.document;

            /*
             * =================================================
             * Data
             * =================================================
             */

            let blocks =
                getAllBlocks(
                    editor
                );

            let categories =
                buildCategories(
                    blocks
                );

            /*
             * =================================================
             * State
             * =================================================
             */

            let currentCategoryId =
                null;

            let searchKeyword =
                '';

            let settled =
                false;

            let imageViewerElement =
                null;

            let blockRefreshQueued =
                false;

            /*
             * =================================================
             * Root
             * =================================================
             */

            const root =
                createElement(
                    documentRef,
                    'div',
                    modalClassName
                );

            /*
             * =================================================
             * Header
             * =================================================
             */

            const header =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__header`
                );

            /*
             * Back Button
             */

            const backButton =
                createElement(
                    documentRef,
                    'button',
                    `${modalClassName}__back`
                );

            backButton.type =
                'button';

            backButton.hidden =
                true;

            backButton.append(
                createSvgIcon(
                    documentRef,
                    icons.back,
                    `${modalClassName}__back-icon`
                )
            );

            const backLabel =
                createElement(
                    documentRef,
                    'span'
                );

            backLabel.textContent =
                '返回分類';

            backButton.append(
                backLabel
            );

            /*
             * Heading
             */

            const heading =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__heading`
                );

            const headingTitle =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__current-title`
                );

            const headingDescription =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__description`
                );

            heading.append(
                headingTitle,
                headingDescription
            );

            header.append(
                backButton,
                heading
            );

            /*
             * =================================================
             * Search
             * =================================================
             */

            const searchWrapper =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__search`
                );

            searchWrapper.append(
                createSvgIcon(
                    documentRef,
                    icons.search,
                    `${modalClassName}__search-icon`
                )
            );

            const searchInput =
                createElement(
                    documentRef,
                    'input',
                    `${modalClassName}__search-input`
                );

            searchInput.type =
                'search';

            searchInput.autocomplete =
                'off';

            searchInput.spellcheck =
                false;

            searchWrapper.append(
                searchInput
            );

            /*
             * =================================================
             * Summary
             * =================================================
             */

            const summary =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__summary`
                );

            /*
             * =================================================
             * Body
             * =================================================
             */

            const body =
                createElement(
                    documentRef,
                    'div',
                    `${modalClassName}__body`
                );

            root.append(
                header,
                searchWrapper,
                summary,
                body
            );

            /*
             * =================================================
             * Finish
             * =================================================
             */

            function finish(
                block
            ) {
                if (settled) {
                    return;
                }

                settled =
                    true;

                editor.off(
                    'block:add',
                    handleBlockCollectionChanged
                );

                editor.off(
                    'block:remove',
                    handleBlockCollectionChanged
                );

                editor.off(
                    'block:update',
                    handleBlockCollectionChanged
                );

                editor.off(
                    'modal:close',
                    handleModalClose
                );

                resolve(
                    block || null
                );
            }


            function handleBlockCollectionChanged() {
                if (
                    settled ||
                    blockRefreshQueued
                ) {
                    return;
                }

                blockRefreshQueued =
                    true;

                queueMicrotask(
                    () => {
                        blockRefreshQueued =
                            false;

                        if (settled) {
                            return;
                        }

                        blocks =
                            getAllBlocks(
                                editor
                            );

                        categories =
                            buildCategories(
                                blocks
                            );

                        render();
                    }
                );
            }


            function handleModalClose() {
                closeImageViewer();

                finish(null);
            }


            function selectBlock(
                block
            ) {
                if (!block) {
                    return;
                }

                finish(block);

                modal.close();
            }

            /*
             * =================================================
             * Image Viewer
             * =================================================
             */

            function closeImageViewer() {
                if (!imageViewerElement) {
                    return;
                }

                imageViewerElement.remove();

                imageViewerElement =
                    null;
            }


            function openImageViewer(
                imageInfo,
                block
            ) {
                if (!imageInfo) {
                    return;
                }

                closeImageViewer();

                const overlay =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__image-viewer`
                    );

                const panel =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__image-viewer-panel`
                    );

                /*
                 * Close
                 */

                const closeButton =
                    createElement(
                        documentRef,
                        'button',
                        `${modalClassName}__image-viewer-close`
                    );

                closeButton.type =
                    'button';

                closeButton.setAttribute(
                    'aria-label',
                    '關閉圖片預覽'
                );

                closeButton.innerHTML =
                    '&times;';

                /*
                 * Image Area
                 */

                const imageArea =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__image-viewer-image`
                    );

                const image =
                    documentRef.createElement(
                        'img'
                    );

                image.src =
                    imageInfo.src;

                image.alt =
                    imageInfo.alt;

                imageArea.append(
                    image
                );

                /*
                 * Title
                 */

                const title =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__image-viewer-title`
                    );

                title.textContent =
                    getBlockLabel(
                        block
                    );

                panel.append(
                    closeButton,
                    imageArea,
                    title
                );

                overlay.append(
                    panel
                );

                /*
                 * Events
                 */

                closeButton.addEventListener(
                    'click',
                    event => {
                        event.preventDefault();
                        event.stopPropagation();

                        closeImageViewer();
                    }
                );

                overlay.addEventListener(
                    'click',
                    event => {
                        if (
                            event.target ===
                            overlay
                        ) {
                            closeImageViewer();
                        }
                    }
                );

                panel.addEventListener(
                    'click',
                    event => {
                        event.stopPropagation();
                    }
                );

                root.append(
                    overlay
                );

                imageViewerElement =
                    overlay;
            }


            /*
             * =================================================
             * Empty
             * =================================================
             */

            function renderEmpty(
                titleText
            ) {
                body.innerHTML =
                    '';

                const empty =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__empty`
                    );

                empty.append(
                    createSvgIcon(
                        documentRef,
                        icons.empty,
                        `${modalClassName}__empty-icon`
                    )
                );

                const title =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__empty-title`
                    );

                title.textContent =
                    titleText;

                const description =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__empty-description`
                    );

                description.textContent =
                    '請調整搜尋條件後再試一次';

                empty.append(
                    title,
                    description
                );

                body.append(
                    empty
                );
            }


            /*
             * =================================================
             * Category Card
             * =================================================
             */

            function createCategoryCard(
                category,
                matchedCount = null
            ) {
                const card =
                    createElement(
                        documentRef,
                        'button',
                        `${modalClassName}__category-card`
                    );

                card.type =
                    'button';

                /*
                 * Icon
                 */

                const iconBox =
                    createElement(
                        documentRef,
                        'span',
                        `${modalClassName}__category-icon`
                    );

                iconBox.append(
                    createSvgIcon(
                        documentRef,
                        icons.folder,
                        `${modalClassName}__svg-icon`
                    )
                );

                /*
                 * Content
                 */

                const content =
                    createElement(
                        documentRef,
                        'span',
                        `${modalClassName}__category-content`
                    );

                const title =
                    createElement(
                        documentRef,
                        'span',
                        `${modalClassName}__category-title`
                    );

                title.textContent =
                    category.label;

                const count =
                    createElement(
                        documentRef,
                        'span',
                        `${modalClassName}__category-count`
                    );

                if (
                    matchedCount !==
                    null
                ) {
                    count.textContent =
                        `${matchedCount} / ${category.blocks.length} 個元件`;
                }
                else {
                    count.textContent =
                        `${category.blocks.length} 個元件`;
                }

                content.append(
                    title,
                    count
                );

                /*
                 * Arrow
                 */

                const arrow =
                    createSvgIcon(
                        documentRef,
                        icons.chevronRight,
                        `${modalClassName}__category-arrow`
                    );

                card.append(
                    iconBox,
                    content,
                    arrow
                );

                card.addEventListener(
                    'click',
                    () => {
                        currentCategoryId =
                            category.id;

                        /*
                         * 保留搜尋條件。
                         */

                        render();

                        queueMicrotask(
                            () => {
                                searchInput.focus();
                            }
                        );
                    }
                );

                return card;
            }


            /*
             * =================================================
             * Block Card
             * =================================================
             */

            function createBlockCard(
                block
            ) {
                const card =
                    createElement(
                        documentRef,
                        'article',
                        `${modalClassName}__block-card`
                    );

                /*
                 * =================================================
                 * Preview
                 * =================================================
                 */

                const preview =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-preview`
                    );

                const media =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-media`
                    );

                renderBlockMedia(
                    documentRef,
                    media,
                    block
                );

                preview.append(
                    media
                );

                /*
                 * =================================================
                 * Image Zoom
                 * =================================================
                 */

                const imageInfo =
                    getBlockImageInfo(
                        documentRef,
                        block
                    );

                if (imageInfo) {
                    const zoomButton =
                        createElement(
                            documentRef,
                            'button',
                            `${modalClassName}__block-zoom`
                        );

                    zoomButton.type =
                        'button';

                    zoomButton.title =
                        '放大查看圖片';

                    zoomButton.setAttribute(
                        'aria-label',
                        '放大查看圖片'
                    );

                    zoomButton.append(
                        createSvgIcon(
                            documentRef,
                            icons.zoom,
                            `${modalClassName}__block-zoom-icon`
                        )
                    );

                    zoomButton.addEventListener(
                        'click',
                        event => {
                            event.preventDefault();

                            event.stopPropagation();

                            openImageViewer(
                                imageInfo,
                                block
                            );
                        }
                    );

                    preview.append(
                        zoomButton
                    );
                }

                /*
                 * =================================================
                 * Content
                 * =================================================
                 */

                const content =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-content`
                    );

                const title =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-title`
                    );

                title.textContent =
                    getBlockLabel(
                        block
                    );

                const category =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-category`
                    );

                category.textContent =
                    getBlockCategoryLabel(
                        block
                    );

                /*
                 * =================================================
                 * Insert Button
                 * =================================================
                 */

                const insertButton =
                    createElement(
                        documentRef,
                        'button',
                        `${modalClassName}__block-insert`
                    );

                insertButton.type =
                    'button';

                insertButton.append(
                    createSvgIcon(
                        documentRef,
                        icons.add,
                        `${modalClassName}__block-insert-icon`
                    )
                );

                const insertLabel =
                    createElement(
                        documentRef,
                        'span'
                    );

                insertLabel.textContent =
                    '插入元件';

                insertButton.append(
                    insertLabel
                );

                insertButton.addEventListener(
                    'click',
                    event => {
                        event.preventDefault();

                        event.stopPropagation();

                        selectBlock(
                            block
                        );
                    }
                );

                content.append(
                    title,
                    category,
                    insertButton
                );

                card.append(
                    preview,
                    content
                );

                /*
                 * 整張卡片也可點擊插入。
                 *
                 * 放大鏡與 insert button
                 * 已經 stopPropagation。
                 */

                card.addEventListener(
                    'click',
                    () => {
                        selectBlock(
                            block
                        );
                    }
                );

                return card;
            }


            /*
             * =================================================
             * Filter Blocks
             * =================================================
             */

            function filterBlocks(
                sourceBlocks
            ) {
                if (!searchKeyword) {
                    return sourceBlocks;
                }

                const keyword =
                    searchKeyword
                        .toLocaleLowerCase();

                return sourceBlocks.filter(
                    block =>
                        getBlockSearchText(
                            block
                        ).includes(
                            keyword
                        )
                );
            }


            /*
             * =================================================
             * Render Categories
             * =================================================
             */

            function renderCategories() {
                body.innerHTML =
                    '';

                /*
                 * 沒搜尋。
                 */

                if (!searchKeyword) {
                    summary.textContent =
                        `${categories.length} 個分類・${blocks.length} 個元件`;

                    if (
                        categories.length ===
                        0
                    ) {
                        renderEmpty(
                            '目前沒有可用的元件分類'
                        );

                        return;
                    }

                    const grid =
                        createElement(
                            documentRef,
                            'div',
                            `${modalClassName}__category-grid`
                        );

                    for (
                        const category of
                        categories
                    ) {
                        grid.append(
                            createCategoryCard(
                                category,
                                null
                            )
                        );
                    }

                    body.append(
                        grid
                    );

                    return;
                }

                /*
                 * 有搜尋。
                 */

                const keyword =
                    searchKeyword
                        .toLocaleLowerCase();

                const matchedCategories =
                    [];

                let totalMatchedBlocks =
                    0;

                for (
                    const category of
                    categories
                ) {
                    const categoryMatched =
                        category.label
                            .toLocaleLowerCase()
                            .includes(
                                keyword
                            );

                    let matchedBlocks;

                    if (
                        categoryMatched
                    ) {
                        matchedBlocks =
                            category.blocks;
                    }
                    else {
                        matchedBlocks =
                            category.blocks.filter(
                                block =>
                                    getBlockSearchText(
                                        block
                                    ).includes(
                                        keyword
                                    )
                            );
                    }

                    if (
                        matchedBlocks.length ===
                        0
                    ) {
                        continue;
                    }

                    matchedCategories.push({
                        category,
                        matchedBlocks
                    });

                    totalMatchedBlocks +=
                        matchedBlocks.length;
                }

                summary.textContent =
                    `${matchedCategories.length} 個分類・${totalMatchedBlocks} 個符合元件`;

                if (
                    matchedCategories.length ===
                    0
                ) {
                    renderEmpty(
                        '找不到符合條件的分類或元件'
                    );

                    return;
                }

                const grid =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__category-grid`
                    );

                for (
                    const item of
                    matchedCategories
                ) {
                    grid.append(
                        createCategoryCard(
                            item.category,
                            item.matchedBlocks.length
                        )
                    );
                }

                body.append(
                    grid
                );
            }


            /*
             * =================================================
             * Render Blocks
             * =================================================
             */

            function renderBlocks(
                category
            ) {
                body.innerHTML =
                    '';

                const filteredBlocks =
                    filterBlocks(
                        category.blocks
                    );

                summary.textContent =
                    searchKeyword
                        ? `${filteredBlocks.length} / ${category.blocks.length} 個元件`
                        : `${category.blocks.length} 個元件`;

                if (
                    filteredBlocks.length ===
                    0
                ) {
                    renderEmpty(
                        '找不到符合條件的元件'
                    );

                    return;
                }

                const grid =
                    createElement(
                        documentRef,
                        'div',
                        `${modalClassName}__block-grid`
                    );

                for (
                    const block of
                    filteredBlocks
                ) {
                    grid.append(
                        createBlockCard(
                            block
                        )
                    );
                }

                body.append(
                    grid
                );
            }


            /*
             * =================================================
             * Render
             * =================================================
             */

            function render() {
                if (
                    currentCategoryId
                ) {
                    const category =
                        categories.find(
                            item =>
                                item.id ===
                                currentCategoryId
                        );

                    if (!category) {
                        currentCategoryId =
                            null;

                        render();

                        return;
                    }

                    backButton.hidden =
                        false;

                    headingTitle.textContent =
                        category.label;

                    headingDescription.textContent =
                        `選擇要加入頁面的 ${category.label} 元件`;

                    searchInput.placeholder =
                        `搜尋 ${category.label} 元件`;

                    renderBlocks(
                        category
                    );

                    return;
                }

                backButton.hidden =
                    true;

                headingTitle.textContent =
                    '選擇元件分類';

                headingDescription.textContent =
                    '先選擇分類，再挑選要加入頁面的元件';

                searchInput.placeholder =
                    '搜尋分類或元件';

                renderCategories();
            }


            /*
             * =================================================
             * Events
             * =================================================
             */

            backButton.addEventListener(
                'click',
                () => {
                    currentCategoryId =
                        null;

                    /*
                     * 保留搜尋條件。
                     */

                    render();

                    queueMicrotask(
                        () => {
                            searchInput.focus();
                        }
                    );
                }
            );


            searchInput.addEventListener(
                'input',
                () => {
                    searchKeyword =
                        searchInput.value
                            .trim();

                    render();
                }
            );


            /*
             * =================================================
             * Modal Close
             * =================================================
             */

            editor.on(
                'modal:close',
                handleModalClose
            );

            editor.on(
                'block:add',
                handleBlockCollectionChanged
            );

            editor.on(
                'block:remove',
                handleBlockCollectionChanged
            );

            editor.on(
                'block:update',
                handleBlockCollectionChanged
            );


            /*
             * =================================================
             * Open
             * =================================================
             */

            modal.setTitle(
                modalTitle
            );

            modal.setContent(
                root
            );

            modal.open({
                attributes: {
                    class:
                        'coker-component-insert-modal-wrapper'
                }
            });

            render();

            queueMicrotask(
                () => {
                    searchInput.focus();
                }
            );
        }
    );
}
