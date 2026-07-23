import {
    GlobalWorkerOptions,
    getDocument,
    TextLayer
} from "/lib/pdfjs-dist/build/pdf.min.mjs";

const assetRoot = new URL("/lib/pdfjs-dist/", import.meta.url);

GlobalWorkerOptions.workerSrc = new URL("build/pdf.worker.min.mjs", assetRoot).href;

function normalizeSearchText(value) {
    return String(value || "")
        .normalize("NFKC")
        .toLocaleLowerCase()
        .replace(/\s+/g, " ")
        .trim();
}

function countMatches(text, query) {
    let count = 0;
    let offset = 0;

    while (query && (offset = text.indexOf(query, offset)) !== -1) {
        count++;
        offset += Math.max(1, query.length);
    }

    return count;
}

function findHighlightSelections(pageText, normalizedQuery) {
    const selections = [];
    const seen = new Set();

    pageText.items.forEach((item, itemIndex) => {
        const itemText = normalizeSearchText(item.str);
        let offset = 0;

        while (itemText && (offset = itemText.indexOf(normalizedQuery, offset)) !== -1) {
            const key = `${itemIndex}:${offset}:${normalizedQuery.length}`;
            if (!seen.has(key)) {
                seen.add(key);
                selections.push({
                    itemIndex,
                    item,
                    startRatio: offset / itemText.length,
                    widthRatio: normalizedQuery.length / itemText.length
                });
            }
            offset += Math.max(1, normalizedQuery.length);
        }
    });

    if (selections.length > 0) return selections;

    const compactQuery = normalizedQuery.replace(/\s/g, "");
    const matchText = compactQuery.length > 1 ? pageText.compact : pageText.normalized;
    const query = compactQuery.length > 1 ? compactQuery : normalizedQuery;
    const segments = compactQuery.length > 1
        ? pageText.compactSegments
        : pageText.normalizedSegments;
    let matchOffset = 0;

    while (query && (matchOffset = matchText.indexOf(query, matchOffset)) !== -1) {
        const matchEnd = matchOffset + query.length;
        for (const segment of segments) {
            if (segment.end <= matchOffset || segment.start >= matchEnd) continue;
            const key = `${segment.itemIndex}:full`;
            if (seen.has(key)) continue;
            seen.add(key);
            selections.push({
                itemIndex: segment.itemIndex,
                item: pageText.items[segment.itemIndex],
                startRatio: 0,
                widthRatio: 1
            });
        }
        matchOffset = matchEnd;
    }

    return selections;
}

export class PdfCatalogAdapter {
    constructor() {
        this.loadingTask = null;
        this.document = null;
        this.renderTasks = new Map();
        this.pageTextPromises = new Map();
    }

    async load(url, onProgress) {
        await this.destroy();

        this.loadingTask = getDocument({
            url,
            cMapUrl: new URL("cmaps/", assetRoot).href,
            cMapPacked: true,
            iccUrl: new URL("iccs/", assetRoot).href,
            standardFontDataUrl: new URL("standard_fonts/", assetRoot).href,
            wasmUrl: new URL("wasm/", assetRoot).href
        });

        this.loadingTask.onProgress = function ({ loaded, total }) {
            if (typeof onProgress === "function") {
                onProgress({ loaded, total });
            }
        };

        this.document = await this.loadingTask.promise;
        return this.document.numPages;
    }

    async getPageSize(pageNumber) {
        this.ensureDocument();
        const page = await this.document.getPage(pageNumber);
        const viewport = page.getViewport({ scale: 1 });
        return { width: viewport.width, height: viewport.height };
    }

    async renderPage(pageNumber, canvas, targetSize) {
        return this.renderToCanvas(
            pageNumber,
            canvas,
            targetSize,
            `page:${pageNumber}`,
            Math.min(window.devicePixelRatio || 1, 2)
        );
    }

    async renderThumbnail(pageNumber, canvas, targetSize) {
        return this.renderToCanvas(
            pageNumber,
            canvas,
            targetSize,
            `thumbnail:${pageNumber}`,
            1
        );
    }

    async renderToCanvas(pageNumber, canvas, targetSize, taskKey, outputScale) {
        this.ensureDocument();
        await this.cancelTask(taskKey);

        const page = await this.document.getPage(pageNumber);
        const baseViewport = page.getViewport({ scale: 1 });
        const cssScale = Math.min(
            targetSize.width / baseViewport.width,
            targetSize.height / baseViewport.height
        );
        const viewport = page.getViewport({ scale: cssScale });
        const context = canvas.getContext("2d", { alpha: false });

        canvas.width = Math.max(1, Math.floor(viewport.width * outputScale));
        canvas.height = Math.max(1, Math.floor(viewport.height * outputScale));
        canvas.style.width = `${Math.floor(viewport.width)}px`;
        canvas.style.height = `${Math.floor(viewport.height)}px`;

        const renderTask = page.render({
            canvas,
            canvasContext: context,
            viewport,
            transform: outputScale === 1
                ? null
                : [outputScale, 0, 0, outputScale, 0, 0]
        });

        this.renderTasks.set(taskKey, renderTask);

        try {
            await renderTask.promise;
        } finally {
            if (this.renderTasks.get(taskKey) === renderTask) {
                this.renderTasks.delete(taskKey);
            }
        }
    }

    releasePage(pageNumber, canvas) {
        this.cancelRender(pageNumber);
        canvas.width = 1;
        canvas.height = 1;
        canvas.style.removeProperty("width");
        canvas.style.removeProperty("height");
    }

    async cancelRender(pageNumber) {
        return this.cancelTask(`page:${pageNumber}`);
    }

    async cancelTask(taskKey) {
        const task = this.renderTasks.get(taskKey);
        if (!task) return;

        task.cancel();
        this.renderTasks.delete(taskKey);

        try {
            await task.promise;
        } catch (error) {
            if (error?.name !== "RenderingCancelledException") throw error;
        }
    }

    async renderSearchHighlights(pageNumber, query, container, targetSize) {
        this.ensureDocument();
        const normalizedQuery = normalizeSearchText(query);
        container.replaceChildren();
        if (!normalizedQuery) return;

        const [page, pageText] = await Promise.all([
            this.document.getPage(pageNumber),
            this.getPageText(pageNumber)
        ]);
        const baseViewport = page.getViewport({ scale: 1 });
        const cssScale = Math.min(
            targetSize.width / baseViewport.width,
            targetSize.height / baseViewport.height
        );
        const viewport = page.getViewport({ scale: cssScale });
        const selections = findHighlightSelections(pageText, normalizedQuery);
        const selectedItemIndexes = new Set(
            selections.map(({ itemIndex }) => itemIndex)
        );

        container.style.setProperty("--total-scale-factor", String(viewport.scale));
        const textLayer = new TextLayer({
            textContentSource: {
                items: pageText.items,
                styles: pageText.styles,
                lang: pageText.lang
            },
            container,
            viewport
        });
        await textLayer.render();

        container.style.width = `${viewport.width}px`;
        container.style.height = `${viewport.height}px`;

        const textSpans = container.querySelectorAll("span[role='presentation']");
        let spanIndex = 0;
        pageText.items.forEach((item, itemIndex) => {
            if (item.str === "") return;
            const span = textSpans[spanIndex++];
            if (span && selectedItemIndexes.has(itemIndex)) {
                span.classList.add("FlipBookSearchHighlight");
            }
        });
    }

    async search(query, onProgress) {
        this.ensureDocument();
        const normalizedQuery = normalizeSearchText(query);
        if (!normalizedQuery) return [];

        const compactQuery = normalizedQuery.replace(/\s/g, "");
        const results = [];

        for (let pageNumber = 1; pageNumber <= this.document.numPages; pageNumber++) {
            const pageText = await this.getPageText(pageNumber);
            let count = countMatches(pageText.normalized, normalizedQuery);

            if (count === 0 && compactQuery.length > 1) {
                count = countMatches(pageText.compact, compactQuery);
            }

            if (count > 0) {
                results.push({ pageNumber, count });
            }

            if (typeof onProgress === "function") {
                onProgress(pageNumber, this.document.numPages);
            }
        }

        return results;
    }

    async getPageText(pageNumber) {
        let pageTextPromise = this.pageTextPromises.get(pageNumber);
        if (!pageTextPromise) {
            pageTextPromise = this.document.getPage(pageNumber)
                .then((page) => page.getTextContent())
                .then(({ items, styles, lang }) => {
                    const textItems = items.filter((item) => typeof item.str === "string");
                    const normalizedSegments = [];
                    const compactSegments = [];
                    let normalized = "";
                    let compact = "";

                    textItems.forEach((item, itemIndex) => {
                        const itemText = normalizeSearchText(item.str);
                        if (!itemText) return;

                        if (normalized) normalized += " ";
                        const normalizedStart = normalized.length;
                        normalized += itemText;
                        normalizedSegments.push({
                            itemIndex,
                            start: normalizedStart,
                            end: normalized.length
                        });

                        const compactItemText = itemText.replace(/\s/g, "");
                        const compactStart = compact.length;
                        compact += compactItemText;
                        compactSegments.push({
                            itemIndex,
                            start: compactStart,
                            end: compact.length
                        });
                    });

                    return {
                        items: textItems,
                        styles,
                        lang,
                        normalized,
                        compact,
                        normalizedSegments,
                        compactSegments
                    };
                });
            this.pageTextPromises.set(pageNumber, pageTextPromise);
        }

        return pageTextPromise;
    }

    async destroy() {
        for (const task of this.renderTasks.values()) {
            task.cancel();
        }
        this.renderTasks.clear();
        this.pageTextPromises.clear();

        const loadingTask = this.loadingTask;
        this.loadingTask = null;
        this.document = null;

        if (loadingTask) {
            await loadingTask.destroy();
        }
    }

    ensureDocument() {
        if (!this.document) {
            throw new Error("PDF document has not been loaded.");
        }
    }
}
