import {
    normalizeEmbedUrl,
    normalizeSlide,
    swiperMediaTypes
} from './swiperModel.js';

const defaultVideoPoster = '/images/defaultImage/video.jpg';

export function renderSlides(slides) {
    return slides.map(renderSlide).join('');
}

export function renderThumbnailSlides(slides) {
    return slides.map(renderThumbnailSlide).join('');
}

export function renderSlide(input) {
    const slide = normalizeSlide(input);

    if (slide.templateHtml) {
        return renderTemplateSlide(slide);
    }

    const classes = ['swiper-slide', 'coker-swiper-slide'];

    if (slide.hidden) {
        classes.push('backstageType');
    }

    const media = renderMedia(slide);
    const caption = renderCaption(slide);
    const autoplay = Math.round(slide.duration * 1000);

    return `<div class="${classes.join(' ')}" data-coker-slide-id="${escapeAttribute(slide.id)}" data-coker-media-type="${slide.type}" data-swiper-autoplay="${autoplay}">${media}${caption}</div>`;
}

function renderMedia(slide) {
    if (slide.type === swiperMediaTypes.video) {
        return `<video class="coker-swiper-media" src="${escapeAttribute(slide.src)}"${attribute('poster', slide.poster || defaultVideoPoster)}${attribute('title', slide.title)} controls preload="metadata" data-start_time="${slide.startTime}" data-keep_time="${slide.duration}"></video>`;
    }

    if (slide.type === swiperMediaTypes.embed) {
        const source = normalizeEmbedUrl(slide.src, slide.startTime);
        return `<div class="coker-swiper-embed ratio ratio-${escapeAttribute(slide.ratio)}"><iframe class="coker-swiper-media" src="${escapeAttribute(source)}"${attribute('title', slide.title)} width="100%" height="500" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen data-ratio="${escapeAttribute(slide.ratio)}" data-start_time="${slide.startTime}" data-keep_time="${slide.duration}"></iframe></div>`;
    }

    const image = `<img class="coker-swiper-media" src="${escapeAttribute(slide.src)}" alt="${escapeAttribute(slide.title)}" data-keep_time="${slide.duration}">`;

    if (!slide.link) {
        return image;
    }

    return `<a href="${escapeAttribute(slide.link)}"${attribute('title', slide.title)} target="${escapeAttribute(slide.target)}">${image}</a>`;
}

function renderCaption(slide) {
    if (!slide.hasCaption) {
        return '';
    }

    return `<div class="coker-swiper-caption"><h2 class="synopsis_title">${escapeHtml(slide.title)}</h2><div class="synopsis_caption">${escapeHtml(slide.caption)}</div></div>`;
}

function renderTemplateSlide(slide) {
    const parser = new DOMParser();
    const document = parser.parseFromString(slide.templateHtml, 'text/html');
    const element = document.body.firstElementChild;
    if (!element) {
        return renderSlide({ ...slide, templateHtml: '' });
    }

    element.classList.add('swiper-slide', 'coker-swiper-slide');
    element.classList.toggle('backstageType', slide.hidden);
    element.dataset.cokerSlideId = slide.id;
    element.dataset.cokerMediaType = slide.type;
    element.dataset.swiperAutoplay = String(Math.round(slide.duration * 1000));

    applyTemplateTextFields(element, slide.textFields);
    replaceTemplateMedia(element, slide, document);

    const title = element.querySelector('.synopsis_title, .title');
    if (title) {
        title.textContent = slide.title;
    }

    if (slide.hasCaption) {
        const caption = element.querySelector('.synopsis_caption');
        if (caption) {
            caption.textContent = slide.caption;
        }
    }

    return element.outerHTML;
}

function renderThumbnailSlide(input) {
    const slide = normalizeSlide(input);
    const parser = new DOMParser();
    const document = parser.parseFromString(slide.thumbnailTemplateHtml, 'text/html');
    const element = document.body.firstElementChild;

    if (!element) {
        return `<div class="swiper-slide${slide.hidden ? ' backstageType' : ''}"><div class="image"><img class="original" src="${escapeAttribute(slide.poster || slide.src)}" alt="${escapeAttribute(slide.title)}"></div></div>`;
    }

    element.classList.add('swiper-slide');
    element.classList.toggle('backstageType', slide.hidden);
    const image = element.querySelector('img.original') || element.querySelector('img');
    if (image) {
        image.setAttribute('src', slide.poster || slide.src);
        image.setAttribute('alt', slide.title);
    }
    return element.outerHTML;
}

function applyTemplateTextFields(slideElement, textFields) {
    textFields.forEach(field => {
        const target = findElementByPath(slideElement, field.path);
        if (target) {
            if (field.preserveLineBreaks) {
                replaceTextWithLineBreaks(target, field.value);
            } else {
                target.textContent = field.value;
            }
        }
    });
}

function replaceTextWithLineBreaks(element, value) {
    const document = element.ownerDocument;
    const fragment = document.createDocumentFragment();
    String(value || '').split(/\r?\n/).forEach((line, index) => {
        if (index) {
            fragment.append(document.createElement('br'));
        }
        fragment.append(document.createTextNode(line));
    });
    element.replaceChildren(fragment);
}

function findElementByPath(root, path) {
    if (!path) {
        return null;
    }

    return path.split('.').reduce((element, part) => {
        const index = Number(part);
        return Number.isInteger(index) ? element?.children?.[index] : null;
    }, root);
}

function replaceTemplateMedia(slideElement, slide, document) {
    const iframe = slideElement.querySelector('iframe');
    const video = slideElement.querySelector('video');
    const videoLink = slideElement.querySelector('a[data-link]');
    const image = slideElement.querySelector('img');
    const currentMedia = iframe || video || videoLink || image;
    const replacement = createElementFromHtml(document, renderMedia(slide));

    if (!replacement) {
        return;
    }

    if (!currentMedia) {
        slideElement.prepend(replacement);
        return;
    }

    let replaceTarget = currentMedia;
    if (iframe) {
        const ratio = iframe.closest('.ratio');
        if (ratio && ratio !== slideElement) {
            replaceTarget = ratio;
        }
    } else if (image) {
        const link = image.closest('a');
        if (link && link !== slideElement) {
            replaceTarget = link;
        }
    }

    replaceTarget.replaceWith(replacement);
}

function createElementFromHtml(document, html) {
    const template = document.createElement('template');
    template.innerHTML = html.trim();
    return template.content.firstElementChild;
}

function attribute(name, value) {
    return value ? ` ${name}="${escapeAttribute(value)}"` : '';
}

function escapeAttribute(value) {
    return escapeHtml(value).replace(/`/g, '&#96;');
}

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
