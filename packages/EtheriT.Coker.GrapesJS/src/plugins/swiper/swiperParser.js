import {
    isVideoFileUrl,
    normalizeSlide,
    swiperMediaTypes
} from './swiperModel.js';

export function parseSwiperSlides(component) {
    const root = parseComponentHtml(component);
    const wrapper = findPrimaryWrapper(root);

    if (!wrapper) {
        return [];
    }

    const slides = Array.from(wrapper.children)
        .filter(element => element.classList.contains('swiper-slide'))
        .map(parseSlideElement);
    attachThumbnailTemplates(root, slides);
    return slides;
}

export function findSwiperWrapperComponent(component) {
    const wrappers = typeof component?.find === 'function'
        ? component.find('.swiper-wrapper')
        : [];

    if (component?.getClasses?.().includes('vertical_swiper_thumbs')) {
        return wrappers.find(wrapper => wrapper.parent?.()?.getClasses?.().includes('swiper_thumbs')) || null;
    }

    return wrappers.find(wrapper => (
        !hasAncestorClass(wrapper, 'template_slide') && !hasAncestorClass(wrapper, 'six_thumbs')
    )) || null;
}

export function findSwiperThumbnailWrapperComponent(component) {
    if (!component?.getClasses?.().includes('vertical_swiper_thumbs')) {
        return null;
    }

    const wrappers = typeof component.find === 'function'
        ? component.find('.swiper-wrapper')
        : [];
    return wrappers.find(wrapper => hasAncestorClass(wrapper, 'float')) || null;
}

function parseComponentHtml(component) {
    const parser = new DOMParser();
    const document = parser.parseFromString(component.toHTML(), 'text/html');
    return document.body.firstElementChild || document.body;
}

function findPrimaryWrapper(root) {
    return Array.from(root.querySelectorAll('.swiper-wrapper')).find(wrapper => (
        !wrapper.closest('.template_slide') && !wrapper.closest('.six_thumbs')
    )) || null;
}

function attachThumbnailTemplates(root, slides) {
    const thumbnails = Array.from(root.querySelectorAll('.float .swiper > .swiper-wrapper > .swiper-slide'));
    if (!thumbnails.length) {
        return;
    }

    const available = [...thumbnails];
    slides.forEach((slide, index) => {
        const matchedIndex = available.findIndex(element => getThumbnailSource(element) === slide.src);
        const thumbnail = matchedIndex >= 0
            ? available.splice(matchedIndex, 1)[0]
            : available[index] || thumbnails[index];
        slide.thumbnailTemplateHtml = thumbnail?.outerHTML || '';
    });
}

function getThumbnailSource(element) {
    return element.querySelector('img.original')?.getAttribute('src') ||
        element.querySelector('img')?.getAttribute('src') ||
        '';
}

function parseSlideElement(element) {
    const image = element.querySelector('img');
    const video = element.querySelector('video');
    const iframe = element.querySelector('iframe');
    const videoLink = element.querySelector('a[data-link]');
    const mediaLink = image?.closest('a');
    const ordinaryLink = mediaLink &&
        !mediaLink.hasAttribute('data-link') &&
        mediaLink.getAttribute('href') !== '#SwiperModal'
        ? mediaLink
        : null;
    const source = video?.getAttribute('src') ||
        iframe?.getAttribute('src') ||
        videoLink?.dataset.link ||
        image?.getAttribute('src') ||
        '';
    const type = iframe || videoLink
        ? swiperMediaTypes.embed
        : video || isVideoFileUrl(source)
            ? swiperMediaTypes.video
            : swiperMediaTypes.image;
    const textFields = extractEditableTextFields(element);

    return normalizeSlide({
        id: element.dataset.cokerSlideId,
        type,
        src: source,
        poster: video?.getAttribute('poster') || (type !== swiperMediaTypes.image ? image?.getAttribute('src') : '') || '',
        title: image?.getAttribute('alt') ||
            video?.getAttribute('title') ||
            iframe?.getAttribute('title') ||
            ordinaryLink?.getAttribute('title') ||
            element.querySelector('.synopsis_title, .title')?.textContent?.trim() ||
            '',
        caption: element.querySelector('.synopsis_caption')?.textContent?.trim() || '',
        link: ordinaryLink?.getAttribute('href') || '',
        target: ordinaryLink?.getAttribute('target') || '_self',
        startTime: firstDataValue(element, 'start_time', 'startTime'),
        duration: firstDataValue(element, 'keep_time', 'keepTime') ||
            Number(element.dataset.swiperAutoplay || 0) / 1000,
        ratio: videoLink?.dataset.ratio || element.querySelector('[data-ratio]')?.dataset.ratio || '16x9',
        hidden: element.classList.contains('backstageType'),
        hasCaption: Boolean(element.querySelector('.synopsis_caption')),
        textFields,
        templateHtml: element.outerHTML
    });
}

function extractEditableTextFields(slideElement) {
    const selector = 'h1,h2,h3,h4,h5,h6,p,span,a,button,small,strong,em,li,figcaption,blockquote,div';

    return Array.from(slideElement.querySelectorAll(selector))
        .filter(element => isEditableTextElement(element, slideElement))
        .map((element, index) => ({
            path: getElementPath(element, slideElement),
            label: createTextFieldLabel(element, index),
            value: getEditableTextValue(element),
            multiline: ['P', 'DIV', 'LI', 'FIGCAPTION', 'BLOCKQUOTE'].includes(element.tagName),
            preserveLineBreaks: Array.from(element.children).some(child => child.tagName === 'BR')
        }));
}

function isEditableTextElement(element, slideElement) {
    if (!element.textContent?.trim() || Array.from(element.children).some(child => child.tagName !== 'BR')) {
        return false;
    }

    if (element.closest('.synopsis_title, .synopsis_caption, .swiper-pagination, .swiper-button-next, .swiper-button-prev')) {
        return false;
    }

    if (element.matches('.title')) {
        return false;
    }

    if (element.closest('script, style, noscript, svg') || element === slideElement) {
        return false;
    }

    const classes = Array.from(element.classList || []);
    return !classes.some(className => (
        className === 'material-symbols-outlined' ||
        className.startsWith('fa-') ||
        className === 'fa' ||
        className === 'fas' ||
        className === 'far' ||
        className === 'fab'
    ));
}

function getEditableTextValue(element) {
    const lines = [''];

    const readNode = node => {
        if (node.nodeType === 1 && node.tagName === 'BR') {
            lines.push('');
            return;
        }

        if (node.nodeType === 1) {
            Array.from(node.childNodes).forEach(readNode);
            return;
        }

        if (node.nodeType !== 3) return;

        // Source formatting commonly places indentation and a newline around
        // each <br>. Those characters are not visual line breaks in HTML and
        // must not be counted in addition to the actual <br> element.
        const sourceText = String(node.textContent || '');
        const hasFormattingBreak = /[\t\r\n]/.test(sourceText);
        const text = hasFormattingBreak
            ? sourceText.replace(/\s+/g, ' ').trim()
            : sourceText.replace(/ {2,}/g, ' ');
        if (text) {
            lines[lines.length - 1] += text;
        }
    };

    Array.from(element.childNodes).forEach(readNode);

    while (lines[0] === '') {
        lines.shift();
    }
    while (lines.at(-1) === '') {
        lines.pop();
    }
    return lines.join('\n');
}

function getElementPath(element, root) {
    const indexes = [];
    let current = element;

    while (current && current !== root) {
        const parent = current.parentElement;
        if (!parent) {
            return '';
        }
        indexes.unshift(Array.from(parent.children).indexOf(current));
        current = parent;
    }

    return indexes.join('.');
}

function createTextFieldLabel(element, index) {
    const labels = {
        A: '連結文字',
        BUTTON: '按鈕文字',
        FIGCAPTION: '圖片說明',
        BLOCKQUOTE: '引言'
    };
    const baseLabel = /^H[1-6]$/.test(element.tagName)
        ? '標題'
        : labels[element.tagName] || (['P', 'DIV', 'LI'].includes(element.tagName) ? '段落文字' : '文字');

    return `${baseLabel} ${index + 1}`;
}

function firstDataValue(element, attributeName, datasetName) {
    const target = element.querySelector(`[data-${attributeName}]`);
    return target?.dataset?.[datasetName] || target?.getAttribute(`data-${attributeName}`) || '';
}

function hasAncestorClass(component, className) {
    let parent = component?.parent?.();

    while (parent && parent !== component) {
        if (parent.getClasses?.().includes(className)) {
            return true;
        }

        parent = parent.parent?.();
    }

    return false;
}
