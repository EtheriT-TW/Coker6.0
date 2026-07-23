export class TurnFlipEngine {
    constructor(bookElement, stageElement, options = {}) {
        this.bookElement = bookElement;
        this.stageElement = stageElement;
        this.pageCount = options.pageCount;
        this.pageRatio = options.pageRatio;
        this.onPageChange = options.onPageChange;
        this.onPageRequest = options.onPageRequest;
        this.$book = window.jQuery(bookElement);
        this.display = null;
        this.zoom = 1;
    }

    initialize() {
        this.resize();

        this.$book.turn({
            width: this.bookElement.clientWidth,
            height: this.bookElement.clientHeight,
            pages: this.pageCount,
            page: 1,
            display: this.display,
            autoCenter: false,
            elevation: 50,
            gradients: true,
            duration: 650,
            when: {
                turning: (_event, page, view) => {
                    this.onPageRequest?.(page, view);
                },
                turned: (_event, page, view) => {
                    this.onPageChange?.(page, view);
                    this.onPageRequest?.(page, view);
                }
            }
        });
    }

    resize() {
        const stageWidth = Math.max(1, this.stageElement.clientWidth - 32);
        const stageHeight = Math.max(1, this.stageElement.clientHeight - 32);
        const nextDisplay = stageWidth < 768 ? "single" : "double";
        const spread = nextDisplay === "double" ? 2 : 1;
        const basePageWidth = Math.min(stageWidth / spread, stageHeight * this.pageRatio);
        const pageWidth = basePageWidth * this.zoom;
        const pageHeight = pageWidth / this.pageRatio;
        const bookWidth = pageWidth * spread;

        this.display = nextDisplay;
        this.bookElement.style.width = `${bookWidth}px`;
        this.bookElement.style.height = `${pageHeight}px`;

        if (this.isInitialized()) {
            this.$book.turn("display", nextDisplay);
            this.$book.turn("size", bookWidth, pageHeight);
        }
    }

    getPageSize() {
        const size = this.isInitialized()
            ? this.$book.turn("size")
            : {
                width: this.bookElement.clientWidth,
                height: this.bookElement.clientHeight
            };

        return {
            width: this.display === "double" ? size.width / 2 : size.width,
            height: size.height
        };
    }

    getCurrentPage() {
        return this.isInitialized() ? this.$book.turn("page") : 1;
    }

    setZoom(value) {
        this.zoom = Math.max(1, Math.min(3, Number(value) || 1));
        this.resize();
        if (this.isInitialized()) {
            this.$book.turn("disable", this.zoom > 1);
        }
    }

    getZoom() {
        return this.zoom;
    }

    goTo(pageNumber) {
        if (this.isInitialized()) {
            this.$book.turn("page", pageNumber);
        }
    }

    next() {
        if (this.isInitialized()) this.$book.turn("next");
    }

    previous() {
        if (this.isInitialized()) this.$book.turn("previous");
    }

    destroy() {
        if (this.isInitialized()) {
            this.$book.turn("destroy");
        }
        this.bookElement.replaceChildren();
        this.bookElement.removeAttribute("style");
        this.zoom = 1;
    }

    isInitialized() {
        return this.$book.turn("is");
    }
}
