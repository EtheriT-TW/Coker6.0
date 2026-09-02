export const swiperComponentClasses = new Set([
    'swiper_components',
    'one_swiper',
    'one_swiper_thumbs',
    'two_swiper',
    'three_swiper',
    'four_swiper',
    'five_swiper',
    'six_swiper',
    'three_two_grid_swiper',
    'vertical_swiper_thumbs'
]);

export const swiperMediaTypes = Object.freeze({
    image: 'image',
    video: 'video',
    embed: 'embed'
});

const validRatios = new Set(['16x9', '4x3', '1x1', '9x16']);

export function createSlideId() {
    if (globalThis.crypto?.randomUUID) {
        return globalThis.crypto.randomUUID();
    }

    return `slide-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

export function normalizeSlide(slide = {}) {
    const type = Object.values(swiperMediaTypes).includes(slide.type)
        ? slide.type
        : swiperMediaTypes.image;

    return {
        id: String(slide.id || createSlideId()),
        type,
        src: String(slide.src || ''),
        poster: String(slide.poster || ''),
        title: String(slide.title || ''),
        caption: String(slide.caption || ''),
        link: String(slide.link || ''),
        target: slide.target === '_blank' ? '_blank' : '_self',
        startTime: toNonNegativeNumber(slide.startTime, 0),
        duration: toPositiveNumber(slide.duration, 5),
        ratio: validRatios.has(slide.ratio) ? slide.ratio : '16x9',
        hidden: Boolean(slide.hidden),
        hasCaption: Boolean(slide.hasCaption),
        textFields: normalizeTextFields(slide.textFields),
        imageFields: normalizeImageFields(slide.imageFields),
        thumbnailTemplateHtml: String(slide.thumbnailTemplateHtml || ''),
        templateHtml: String(slide.templateHtml || '')
    };
}

function normalizeImageFields(fields) {
    if (!Array.isArray(fields)) {
        return [];
    }

    return fields
        .filter(field => field && typeof field.path === 'string')
        .map(field => ({
            path: field.path,
            label: String(field.label || '圖片'),
            src: String(field.src || ''),
            alt: String(field.alt || ''),
            scope: field.scope === 'thumbnail' ? 'thumbnail' : 'slide',
            visibilityPath: String(field.visibilityPath || field.path),
            hidden: Boolean(field.hidden),
            groupPath: String(field.groupPath || field.path),
            groupCollectionPath: String(field.groupCollectionPath || ''),
            groupVisibilityPath: String(field.groupVisibilityPath || field.groupPath || field.path),
            groupHidden: Boolean(field.groupHidden),
            groupType: String(field.groupType || 'content'),
            groupLabel: String(field.groupLabel || field.label || '內容'),
            groupHref: String(field.groupHref || ''),
            groupTarget: field.groupTarget === '_blank' ? '_blank' : '_self'
        }));
}

function normalizeTextFields(fields) {
    if (!Array.isArray(fields)) {
        return [];
    }

    return fields
        .filter(field => field && typeof field.path === 'string')
        .map(field => ({
            path: field.path,
            label: String(field.label || '文字'),
            value: String(field.value || ''),
            multiline: Boolean(field.multiline),
            preserveLineBreaks: Boolean(field.preserveLineBreaks),
            scope: 'slide',
            visibilityPath: String(field.visibilityPath || field.path),
            hidden: Boolean(field.hidden),
            groupPath: String(field.groupPath || field.path),
            groupCollectionPath: String(field.groupCollectionPath || ''),
            groupVisibilityPath: String(field.groupVisibilityPath || field.groupPath || field.path),
            groupHidden: Boolean(field.groupHidden),
            groupType: String(field.groupType || 'content'),
            groupLabel: String(field.groupLabel || field.label || '內容'),
            groupHref: String(field.groupHref || ''),
            groupTarget: field.groupTarget === '_blank' ? '_blank' : '_self'
        }));
}

export function isSwiperRootElement(element) {
    if (!element?.classList) {
        return false;
    }

    return Array.from(swiperComponentClasses).some(className => element.classList.contains(className));
}

export function isVideoFileUrl(value) {
    const path = String(value || '').split(/[?#]/)[0].toLowerCase();
    return ['.mp4', '.webm', '.ogg', '.mov', '.m4v'].some(extension => path.endsWith(extension));
}

export function normalizeEmbedUrl(value, startTime = 0) {
    const source = String(value || '').trim();

    if (!source) {
        return '';
    }

    if (/youtube(?:-nocookie)?\.com\/embed\//i.test(source)) {
        return appendQueryParameter(source, 'start', toNonNegativeNumber(startTime, 0));
    }

    const youtubeMatch = source.match(
        /(?:youtube\.com\/(?:watch\?[^#]*v=|shorts\/|live\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})/i
    );

    if (youtubeMatch?.[1]) {
        const url = `https://www.youtube.com/embed/${youtubeMatch[1]}`;
        return appendQueryParameter(url, 'start', toNonNegativeNumber(startTime, 0));
    }

    if (/facebook\.com\/plugins\/video\.php/i.test(source)) {
        return source;
    }

    if (/facebook\.com/i.test(source) && !/fb\.watch/i.test(source)) {
        const cleanSource = source.replace(/[?&]t=\d+/i, '');
        const url = `https://www.facebook.com/plugins/video.php?href=${encodeURIComponent(cleanSource)}&show_text=false`;
        return appendQueryParameter(url, 'start_time', toNonNegativeNumber(startTime, 0));
    }

    return source;
}

function appendQueryParameter(value, key, parameterValue) {
    if (!parameterValue || new RegExp(`[?&]${key}=`).test(value)) {
        return value;
    }

    return `${value}${value.includes('?') ? '&' : '?'}${key}=${parameterValue}`;
}

function toNonNegativeNumber(value, fallback) {
    const number = Number(value);
    return Number.isFinite(number) && number >= 0 ? number : fallback;
}

function toPositiveNumber(value, fallback) {
    const number = Number(value);
    return Number.isFinite(number) && number > 0 ? number : fallback;
}
