import { PdfCatalogAdapter } from "./PdfCatalogAdapter.mjs";
import { TurnFlipEngine } from "./TurnFlipEngine.mjs";

let controller = null;

class FlipBookController {
    constructor(modalElement) {
        this.modalElement = modalElement;
        this.bookElement = modalElement.querySelector(".FlipBook");
        this.stageElement = modalElement.querySelector(".FlipBookStage");
        this.sidebarElement = modalElement.querySelector(".FlipBookSidebar");
        this.thumbnailListElement = modalElement.querySelector(".FlipBookThumbnailList");
        this.searchInputElement = modalElement.querySelector(".FlipBookSearchInput");
        this.searchStatusElement = modalElement.querySelector(".FlipBookSearchStatus");
        this.zoomStatusElement = modalElement.querySelector(".FlipBookZoomStatus");
        this.adapter = null;
        this.engine = null;
        this.pageElements = new Map();
        this.thumbnailElements = new Map();
        this.renderedPages = new Set();
        this.pageRenderPromises = new Map();
        this.searchResults = [];
        this.searchResultIndex = -1;
        this.searchRequestId = 0;
        this.activeSearchQuery = "";
        this.loadId = 0;
        this.resizeTimer = null;
        this.swipeStart = null;

        this.onShown = this.onShown.bind(this);
        this.onHidden = this.onHidden.bind(this);
        this.onKeyDown = this.onKeyDown.bind(this);
        this.onResize = this.onResize.bind(this);
        this.onPointerDown = this.onPointerDown.bind(this);
        this.onPointerUp = this.onPointerUp.bind(this);
        this.onPointerCancel = this.onPointerCancel.bind(this);

        modalElement.addEventListener("shown.bs.modal", this.onShown);
        modalElement.addEventListener("hidden.bs.modal", this.onHidden);
        modalElement.querySelector(".FlipBookPrevious").addEventListener("click", () => this.engine?.previous());
        modalElement.querySelector(".FlipBookNext").addEventListener("click", () => this.engine?.next());
        modalElement.querySelector(".FlipBookPageInput").addEventListener("change", (event) => {
            this.goToPage(event.currentTarget.value);
        });
        modalElement.querySelector(".FlipBookSidebarToggle").addEventListener("click", () => {
            this.setSidebarOpen(this.sidebarElement.hidden);
        });
        modalElement.querySelector(".FlipBookSidebarClose").addEventListener("click", () => {
            this.setSidebarOpen(false);
        });
        modalElement.querySelector(".FlipBookSearch").addEventListener("submit", (event) => {
            event.preventDefault();
            this.runSearch().catch((error) => {
                console.error("Unable to search PDF text.", error);
            });
        });
        modalElement.querySelector(".FlipBookSearchPrevious").addEventListener("click", () => {
            this.goToSearchResult(-1);
        });
        modalElement.querySelector(".FlipBookSearchNext").addEventListener("click", () => {
            this.goToSearchResult(1);
        });
        modalElement.querySelector(".FlipBookZoomOut").addEventListener("click", () => {
            this.setZoom((this.engine?.getZoom() || 1) - .25);
        });
        modalElement.querySelector(".FlipBookZoomIn").addEventListener("click", () => {
            this.setZoom((this.engine?.getZoom() || 1) + .25);
        });
        modalElement.querySelector(".FlipBookZoomReset").addEventListener("click", () => {
            this.setZoom(1);
        });
        this.stageElement.addEventListener("pointerdown", this.onPointerDown);
        this.stageElement.addEventListener("pointerup", this.onPointerUp);
        this.stageElement.addEventListener("pointercancel", this.onPointerCancel);

        this.resizeObserver = new ResizeObserver(this.onResize);
        this.resizeObserver.observe(this.stageElement);
        this.thumbnailObserver = new IntersectionObserver((entries) => {
            for (const entry of entries) {
                if (!entry.isIntersecting) continue;
                this.thumbnailObserver.unobserve(entry.target);
                this.renderThumbnail(entry.target).catch((error) => {
                    if (error?.name !== "RenderingCancelledException") {
                        console.warn("Unable to render PDF thumbnail.", error);
                    }
                });
            }
        }, {
            root: this.thumbnailListElement,
            rootMargin: "300px 0px"
        });
    }

    async onShown(event) {
        const targetPdf = event.relatedTarget?.getAttribute("data-pdf-url");
        if (!targetPdf) {
            this.showState("error", "找不到 PDF 檔案");
            return;
        }

        const loadId = ++this.loadId;
        this.resetViewer();
        this.modalElement.querySelector(".FlipBookOpenPdf").href = targetPdf;
        this.showState("download", "PDF 下載中", 0);

        try {
            this.adapter = new PdfCatalogAdapter();
            const pageCount = await this.adapter.load(targetPdf, ({ loaded, total }) => {
                if (loadId === this.loadId) {
                    this.updateProgress(loaded, total);
                }
            });
            if (loadId !== this.loadId) return;

            this.showState("processing", "正在建立電子書…");
            const firstPageSize = await this.adapter.getPageSize(1);
            if (loadId !== this.loadId) return;

            this.createPages(pageCount);
            this.createThumbnails(pageCount);
            this.engine = new TurnFlipEngine(this.bookElement, this.stageElement, {
                pageCount,
                pageRatio: firstPageSize.width / firstPageSize.height,
                onPageChange: (page) => this.updatePageStatus(page, pageCount),
                onPageRequest: (page, view) => {
                    this.renderNearbyPages(page, view).catch((error) => {
                        if (error?.name !== "RenderingCancelledException") {
                            console.error("Unable to render FlipBook pages.", error);
                        }
                    });
                }
            });
            this.engine.initialize();
            this.updateZoomControls();
            this.updatePageStatus(1, pageCount);
            await this.renderNearbyPages(1, [1, pageCount > 1 ? 2 : 0], true);

            if (loadId === this.loadId) {
                this.hideLoading();
            }
        } catch (error) {
            if (loadId !== this.loadId || error?.name === "RenderingCancelledException") return;
            this.showState("error", "PDF 載入失敗，請稍後再試");
            console.error("Unable to open FlipBook PDF.", error);
        }
    }

    async onHidden() {
        this.loadId++;
        this.hideLoading();
        this.resetViewer();
    }

    createPages(pageCount) {
        const fragment = document.createDocumentFragment();
        this.pageElements.clear();

        for (let pageNumber = 1; pageNumber <= pageCount; pageNumber++) {
            const pageElement = document.createElement("div");
            const canvas = document.createElement("canvas");
            const highlightLayer = document.createElement("div");
            const label = document.createElement("span");

            pageElement.className = "FlipBookPage";
            pageElement.dataset.pageNumber = String(pageNumber);
            canvas.setAttribute("aria-label", `第 ${pageNumber} 頁`);
            highlightLayer.className = "FlipBookHighlightLayer";
            highlightLayer.setAttribute("aria-hidden", "true");
            label.className = "FlipBookPagePlaceholder";
            label.textContent = String(pageNumber);

            pageElement.append(canvas, highlightLayer, label);
            fragment.appendChild(pageElement);
            this.pageElements.set(pageNumber, { pageElement, canvas, highlightLayer });
        }

        this.bookElement.replaceChildren(fragment);
    }

    createThumbnails(pageCount) {
        const fragment = document.createDocumentFragment();
        this.thumbnailObserver.disconnect();
        this.thumbnailElements.clear();

        for (let pageNumber = 1; pageNumber <= pageCount; pageNumber++) {
            const button = document.createElement("button");
            const canvas = document.createElement("canvas");
            const label = document.createElement("span");

            button.type = "button";
            button.className = "FlipBookThumbnail";
            button.dataset.pageNumber = String(pageNumber);
            button.setAttribute("aria-label", `前往第 ${pageNumber} 頁`);
            canvas.width = 1;
            canvas.height = 1;
            label.className = "FlipBookThumbnailLabel";
            label.textContent = `第 ${pageNumber} 頁`;

            button.append(canvas, label);
            button.addEventListener("click", () => this.goToPage(pageNumber));
            fragment.appendChild(button);
            this.thumbnailElements.set(pageNumber, button);
        }

        this.thumbnailListElement.replaceChildren(fragment);
        for (const button of this.thumbnailElements.values()) {
            this.thumbnailObserver.observe(button);
        }
    }

    async renderThumbnail(button) {
        if (!this.adapter || button.dataset.rendered === "true") return;

        const adapter = this.adapter;
        const pageNumber = Number.parseInt(button.dataset.pageNumber, 10);
        const canvas = button.querySelector("canvas");
        button.dataset.rendered = "loading";

        try {
            await adapter.renderThumbnail(pageNumber, canvas, {
                width: Math.max(120, this.thumbnailListElement.clientWidth - 32),
                height: 220
            });
            if (adapter === this.adapter) {
                button.dataset.rendered = "true";
            }
        } catch (error) {
            delete button.dataset.rendered;
            throw error;
        }
    }

    setSidebarOpen(open) {
        this.sidebarElement.hidden = !open;
        this.modalElement.querySelector(".FlipBookSidebarToggle")
            .setAttribute("aria-expanded", String(open));

        if (open) {
            const currentPage = this.engine?.getCurrentPage() || 1;
            this.thumbnailElements.get(currentPage)?.scrollIntoView({
                block: "nearest",
                inline: "nearest"
            });
        }
    }

    async renderNearbyPages(page, view = [], waitForVisible = false) {
        if (!this.adapter || !this.engine) return;

        const requested = new Set(
            [page, ...view]
                .filter(Number.isInteger)
                .filter((number) => this.pageElements.has(number))
        );

        for (let offset = -2; offset <= 3; offset++) {
            const pageNumber = page + offset;
            if (this.pageElements.has(pageNumber)) requested.add(pageNumber);
        }

        this.releaseDistantPages(requested);
        const targetSize = this.engine.getPageSize();
        const renders = [];

        for (const pageNumber of requested) {
            const existingRender = this.pageRenderPromises.get(pageNumber);
            if (existingRender) {
                if (waitForVisible) renders.push(existingRender);
                continue;
            }
            if (this.renderedPages.has(pageNumber) && !waitForVisible) continue;

            const entry = this.pageElements.get(pageNumber);
            entry.pageElement.classList.add("is-rendering");

            const render = this.adapter.renderPage(pageNumber, entry.canvas, targetSize)
                .then(async () => {
                    this.renderedPages.add(pageNumber);
                    entry.pageElement.classList.add("is-rendered");
                    await this.updatePageHighlights(pageNumber, targetSize);
                })
                .catch((error) => {
                    if (error?.name !== "RenderingCancelledException") throw error;
                })
                .finally(() => {
                    this.pageRenderPromises.delete(pageNumber);
                    entry.pageElement.classList.remove("is-rendering");
                });

            this.pageRenderPromises.set(pageNumber, render);
            renders.push(render);
        }

        if (waitForVisible) {
            await Promise.all(renders);
        }
    }

    async updatePageHighlights(pageNumber, targetSize = this.engine?.getPageSize()) {
        const entry = this.pageElements.get(pageNumber);
        if (!entry) return;

        entry.highlightLayer.replaceChildren();
        if (!this.activeSearchQuery || !this.adapter || !targetSize) return;

        const adapter = this.adapter;
        const query = this.activeSearchQuery;
        await adapter.renderSearchHighlights(
            pageNumber,
            query,
            entry.highlightLayer,
            targetSize
        );
        if (adapter !== this.adapter || query !== this.activeSearchQuery) {
            entry.highlightLayer.replaceChildren();
        }
    }

    async refreshSearchHighlights() {
        if (!this.engine) return;
        const targetSize = this.engine.getPageSize();
        await Promise.all(
            [...this.renderedPages].map((pageNumber) =>
                this.updatePageHighlights(pageNumber, targetSize)
            )
        );
    }

    clearSearchHighlights() {
        for (const entry of this.pageElements.values()) {
            entry.highlightLayer.replaceChildren();
        }
    }

    releaseDistantPages(keepPages) {
        for (const pageNumber of this.pageRenderPromises.keys()) {
            if (!keepPages.has(pageNumber)) {
                this.adapter.cancelRender(pageNumber).catch((error) => {
                    if (error?.name !== "RenderingCancelledException") {
                        console.warn("Unable to cancel a PDF page render.", error);
                    }
                });
            }
        }

        for (const pageNumber of [...this.renderedPages]) {
            if (keepPages.has(pageNumber)) continue;

            const entry = this.pageElements.get(pageNumber);
            this.adapter.releasePage(pageNumber, entry.canvas);
            entry.highlightLayer.replaceChildren();
            entry.pageElement.classList.remove("is-rendered");
            this.renderedPages.delete(pageNumber);
        }
    }

    updatePageStatus(page, pageCount) {
        const input = this.modalElement.querySelector(".FlipBookPageInput");
        input.max = String(pageCount);
        input.value = String(page);
        this.modalElement.querySelector(".FlipBookPageTotal").textContent = String(pageCount);
        this.modalElement.querySelector(".FlipBookPrevious").disabled = page <= 1;
        this.modalElement.querySelector(".FlipBookNext").disabled = page >= pageCount;

        for (const [pageNumber, button] of this.thumbnailElements) {
            if (pageNumber === page) {
                button.setAttribute("aria-current", "page");
            } else {
                button.removeAttribute("aria-current");
            }
        }

        if (!this.sidebarElement.hidden) {
            this.thumbnailElements.get(page)?.scrollIntoView({
                block: "nearest",
                inline: "nearest"
            });
        }
    }

    goToPage(value) {
        if (!this.engine) return;
        const pageCount = this.pageElements.size;
        const page = Math.max(1, Math.min(pageCount, Number.parseInt(value, 10) || 1));
        this.engine.goTo(page);
    }

    async runSearch() {
        const query = this.searchInputElement.value.trim();
        const requestId = ++this.searchRequestId;
        const adapter = this.adapter;

        this.searchResults = [];
        this.searchResultIndex = -1;
        this.activeSearchQuery = "";
        this.clearSearchHighlights();
        this.setSearchButtonsDisabled(true);

        if (!query || !adapter) {
            this.searchStatusElement.textContent = "";
            this.searchStatusElement.removeAttribute("title");
            return;
        }

        this.searchStatusElement.textContent = "…";
        this.searchStatusElement.title = "搜尋中";

        try {
            const results = await adapter.search(query, (current, total) => {
                if (requestId === this.searchRequestId) {
                    this.searchStatusElement.textContent = `${current}/${total}`;
                    this.searchStatusElement.title = `搜尋中 ${current}/${total}`;
                }
            });

            if (requestId !== this.searchRequestId || adapter !== this.adapter) return;

            this.searchResults = results;
            if (results.length === 0) {
                this.searchStatusElement.textContent = "0頁";
                this.searchStatusElement.title = "找不到結果";
                return;
            }

            this.searchResultIndex = 0;
            this.activeSearchQuery = query;
            this.setSearchButtonsDisabled(false);
            this.showCurrentSearchResult();
        } catch (error) {
            if (requestId !== this.searchRequestId || adapter !== this.adapter) return;
            this.searchStatusElement.textContent = "錯誤";
            this.searchStatusElement.title = "搜尋失敗";
            throw error;
        }
    }

    goToSearchResult(direction) {
        if (this.searchResults.length === 0) return;

        this.searchResultIndex = (
            this.searchResultIndex + direction + this.searchResults.length
        ) % this.searchResults.length;
        this.showCurrentSearchResult();
    }

    showCurrentSearchResult() {
        const result = this.searchResults[this.searchResultIndex];
        if (!result) return;

        const totalMatches = this.searchResults.reduce((sum, item) => sum + item.count, 0);
        this.searchStatusElement.textContent =
            `${this.searchResultIndex + 1}/${this.searchResults.length}頁`;
        this.searchStatusElement.title = `共 ${totalMatches} 筆`;
        this.goToPage(result.pageNumber);
        this.refreshSearchHighlights().catch((error) => {
            console.warn("Unable to update PDF search highlights.", error);
        });
    }

    setSearchButtonsDisabled(disabled) {
        this.modalElement.querySelector(".FlipBookSearchPrevious").disabled = disabled;
        this.modalElement.querySelector(".FlipBookSearchNext").disabled = disabled;
    }

    setZoom(value) {
        if (!this.engine) return;

        const zoom = Math.round(Math.max(1, Math.min(3, value)) * 4) / 4;
        this.engine.setZoom(zoom);
        this.stageElement.classList.toggle("is-zoomed", zoom > 1);
        this.stageElement.scrollTo({ top: 0, left: 0 });
        this.updateZoomControls();
        this.rerenderVisiblePages();
    }

    updateZoomControls() {
        const zoom = this.engine?.getZoom() || 1;
        this.zoomStatusElement.textContent = `${Math.round(zoom * 100)}%`;
        this.zoomStatusElement.disabled = zoom <= 1;
        this.modalElement.querySelector(".FlipBookZoomOut").disabled = zoom <= 1;
        this.modalElement.querySelector(".FlipBookZoomIn").disabled = zoom >= 3;
    }

    rerenderVisiblePages() {
        if (!this.engine) return;

        const currentPage = this.engine.getCurrentPage() || 1;
        for (const pageNumber of this.renderedPages) {
            const entry = this.pageElements.get(pageNumber);
            this.adapter?.releasePage(pageNumber, entry.canvas);
            entry.highlightLayer.replaceChildren();
            entry.pageElement.classList.remove("is-rendered");
        }
        this.renderedPages.clear();
        this.renderNearbyPages(currentPage, [], true).catch((error) => {
            if (error?.name !== "RenderingCancelledException") {
                console.error("Unable to resize FlipBook pages.", error);
            }
        });
    }

    onPointerDown(event) {
        if (
            event.pointerType !== "touch" ||
            !event.isPrimary ||
            !this.engine ||
            this.engine.getZoom() > 1
        ) {
            this.swipeStart = null;
            return;
        }

        this.swipeStart = {
            pointerId: event.pointerId,
            x: event.clientX,
            y: event.clientY,
            time: performance.now(),
            page: this.engine.getCurrentPage()
        };
    }

    onPointerUp(event) {
        const start = this.swipeStart;
        this.swipeStart = null;
        if (!start || start.pointerId !== event.pointerId || !this.engine) return;

        const deltaX = event.clientX - start.x;
        const deltaY = event.clientY - start.y;
        const duration = performance.now() - start.time;
        if (
            duration > 800 ||
            Math.abs(deltaX) < 50 ||
            Math.abs(deltaX) <= Math.abs(deltaY) * 1.2
        ) {
            return;
        }

        window.setTimeout(() => {
            if (
                !this.engine ||
                this.engine.getZoom() > 1 ||
                this.engine.getCurrentPage() !== start.page
            ) {
                return;
            }

            if (deltaX < 0) {
                this.engine.next();
            } else {
                this.engine.previous();
            }
        }, 50);
    }

    onPointerCancel() {
        this.swipeStart = null;
    }

    onKeyDown(event) {
        if (!this.modalElement.classList.contains("show")) return;
        if (event.target.matches("input")) return;

        if (event.key === "ArrowLeft" || event.key === "PageUp") {
            event.preventDefault();
            this.engine?.previous();
        } else if (event.key === "ArrowRight" || event.key === "PageDown") {
            event.preventDefault();
            this.engine?.next();
        }
    }

    onResize() {
        if (!this.engine) return;
        window.clearTimeout(this.resizeTimer);
        this.resizeTimer = window.setTimeout(() => {
            this.engine?.resize();
            this.rerenderVisiblePages();
        }, 150);
    }

    showState(state, message, percent) {
        const loading = this.modalElement.querySelector(".FlipBookLoading");
        const progress = loading.querySelector(".FlipBookLoadingProgress");

        loading.classList.remove("d-none", "is-processing", "is-error");
        loading.classList.toggle("is-processing", state === "processing");
        loading.classList.toggle("is-error", state === "error");
        loading.setAttribute("aria-hidden", "false");
        loading.querySelector(".FlipBookLoadingText").textContent = message;

        if (state === "download") {
            progress.classList.remove("is-indeterminate");
            this.setProgress(percent);
        }
    }

    updateProgress(loaded, total) {
        if (!Number.isFinite(total) || total <= 0) {
            const loading = this.modalElement.querySelector(".FlipBookLoading");
            const progress = loading.querySelector(".FlipBookLoadingProgress");
            progress.classList.add("is-indeterminate");
            progress.removeAttribute("aria-valuenow");
            loading.querySelector(".FlipBookLoadingPercent").textContent = "下載中…";
            return;
        }

        this.setProgress((loaded / total) * 100);
    }

    setProgress(value) {
        const percent = Math.max(0, Math.min(100, Math.round(Number(value) || 0)));
        const loading = this.modalElement.querySelector(".FlipBookLoading");
        const progress = loading.querySelector(".FlipBookLoadingProgress");

        progress.classList.remove("is-indeterminate");
        progress.setAttribute("aria-valuenow", String(percent));
        progress.querySelector(".FlipBookLoadingProgressBar").style.width = `${percent}%`;
        loading.querySelector(".FlipBookLoadingPercent").textContent = `${percent}%`;
    }

    hideLoading() {
        const loading = this.modalElement.querySelector(".FlipBookLoading");
        loading.classList.add("d-none");
        loading.setAttribute("aria-hidden", "true");
    }

    resetViewer() {
        window.clearTimeout(this.resizeTimer);
        this.searchRequestId++;
        this.swipeStart = null;
        this.engine?.destroy();
        this.engine = null;

        const adapter = this.adapter;
        this.adapter = null;
        if (adapter) {
            adapter.destroy().catch((error) => {
                console.warn("Unable to clean up the PDF worker.", error);
            });
        }

        this.pageElements.clear();
        this.thumbnailObserver.disconnect();
        this.thumbnailElements.clear();
        this.renderedPages.clear();
        this.pageRenderPromises.clear();
        this.searchResults = [];
        this.searchResultIndex = -1;
        this.activeSearchQuery = "";
        this.bookElement.replaceChildren();
        this.thumbnailListElement.replaceChildren();
        this.searchInputElement.value = "";
        this.searchStatusElement.textContent = "";
        this.searchStatusElement.removeAttribute("title");
        this.setSearchButtonsDisabled(true);
        this.setSidebarOpen(false);
        this.stageElement.classList.remove("is-zoomed");
        this.stageElement.scrollTo({ top: 0, left: 0 });
        this.updateZoomControls();
        document.removeEventListener("keydown", this.onKeyDown);
        document.addEventListener("keydown", this.onKeyDown);
    }
}

export function FlipBookInit() {
    const modalElement = document.querySelector(".FlipBookModal");
    if (!modalElement || !document.querySelector(".FlipBookItem")) {
        return Promise.resolve();
    }

    if (!controller) {
        controller = new FlipBookController(modalElement);
    }

    return Promise.resolve();
}
