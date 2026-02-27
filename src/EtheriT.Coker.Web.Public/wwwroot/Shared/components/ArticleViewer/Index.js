(function (global) {
    "use strict";

    /**
     * ArticleViewer (CSP-friendly-ish)
     * - 盡量不使用 element.style.*（主題變數改用 <style> 注入）
     * - autoHeight=true：量測內容高度後，寫 iframe height attribute（px 數字）
     * - autoHeight=false：iframe 填滿容器高度、由 iframe 自己滾（offcanvas-body 會 overflow:hidden）
     */
    class ArticleViewer {
        /**
         * @param {object} [opt]
         * @param {string} [opt.className] iframe class
         * @param {boolean} [opt.sandbox]
         * @param {string} [opt.sandboxValue]
         * @param {string} [opt.referrerPolicy]
         * @param {number} [opt.minHeight]
         * @param {number} [opt.extraHeight]
         * @param {number} [opt.remeasureDelayMs]
         * @param {number} [opt.remeasureTimes]
         * @param {boolean} [opt.autoHeight] default true
         * @param {function} [opt.onLoad]
         * @param {function} [opt.onError]
         * @param {function} [opt.onHeight]
         */
        constructor(opt) {
            this.opt = Object.assign(
                {
                    className: "article-viewer-frame",
                    sandbox: false,
                    sandboxValue: "allow-same-origin allow-scripts allow-forms allow-popups allow-top-navigation-by-user-activation",
                    referrerPolicy: "no-referrer-when-downgrade",
                    minHeight: 180,
                    extraHeight: 8,
                    remeasureDelayMs: 80,
                    remeasureTimes: 6,
                    nonce: document.querySelector('meta[name="csp-nonce"]').getAttribute('content'),
                    autoHeight: true,
                    onLoad: null,
                    onError: null,
                    onHeight: null,
                },
                opt || {}
            );

            this.container = null;
            this.iframe = null;
            this.mode = "narrow";
            this._disposed = false;

            this.loadingEl = null;

            this._lastHeight = 0;
            this._pendingRaf = 0;
            this._onMessageBound = (e) => this._onMessage(e);
            this._onResizeBound = () => this.remeasure();
        }

        /**
         * Mount viewer to a container element
         * @param {HTMLElement} container
         */
        mount(container) {
            if (!container || !(container instanceof HTMLElement)) {
                throw new Error("ArticleViewer.mount(container): container must be an HTMLElement.");
            }

            this.container = container;

            // container class (for CSS)
            this.container.classList.add("article-viewer");
            this.container.classList.toggle("article-viewer--autoheight", this.opt.autoHeight !== false);
            this.container.classList.toggle("article-viewer--fill", this.opt.autoHeight === false);

            if (!this.iframe) {
                this.iframe = document.createElement("iframe");
                this.iframe.className = this.opt.className;
                this.iframe.setAttribute("data-role", "article-viewer-iframe");
                this.iframe.setAttribute("frameborder", "0");
                this.iframe.referrerPolicy = this.opt.referrerPolicy;

                // scrolling strategy
                if (this.opt.autoHeight !== false) {
                    // autoHeight：外層不滾，iframe 高度會被算到剛好
                    this.iframe.setAttribute("scrolling", "no");
                    this.iframe.setAttribute("height", String(this.opt.minHeight));
                } else {
                    // fill：iframe 撐滿容器，自己滾
                    this.iframe.setAttribute("scrolling", "yes");
                    this.iframe.removeAttribute("height");
                }

                if (this.opt.sandbox) {
                    this.iframe.setAttribute("sandbox", this.opt.sandboxValue);
                }

                this.iframe.addEventListener("load", () => this._onLoad());
                this.iframe.addEventListener("error", (err) => this._onError(err));

                // 預設：mount 就是 iframe 模式（你的 page.js 目前就是這樣使用）
                this._setIframeMode(true);
            }

            // build loading overlay
            if (!this.loadingEl) {
                this.loadingEl = document.createElement("div");
                this.loadingEl.className = "article-viewer-loading";
                this.loadingEl.setAttribute("data-role", "article-viewer-loading");
                this.loadingEl.setAttribute("aria-hidden", "true");
                this.loadingEl.innerHTML = `
                    <div class="spinner" aria-hidden="true"></div>
                    <div class="text">載入中</div>
                `.trim();
            }

            // ensure container has overlay + iframe (order matters)
            if (!this.container.contains(this.loadingEl) || !this.container.contains(this.iframe)) {
                this.container.innerHTML = "";
                this.container.appendChild(this.loadingEl);
                this.container.appendChild(this.iframe);
            }

            // listeners
            window.addEventListener("message", this._onMessageBound);
            window.addEventListener("resize", this._onResizeBound);

            // apply mode class to container
            this.setMode(this.mode);

            return this;
        }

        /**
         * Set mode: 'narrow' or 'wide'
         * @param {'narrow'|'wide'} mode
         */
        setMode(mode) {
            this.mode = mode === "wide" ? "wide" : "narrow";
            if (!this.container) return;

            this.container.classList.toggle("article-viewer--wide", this.mode === "wide");
            this.container.classList.toggle("article-viewer--narrow", this.mode !== "wide");

            this.remeasure();
        }

        /**
         * clear iframe
         * @returns
         */
        clear() {
            if (!this.iframe) return;

            // 關掉 loading，避免 UI 殘留
            this._markLoading(false);

            // 直接卸載 iframe 內容（停止影片/音訊/網路請求）
            this.iframe.onload = null; // 可選：避免 about:blank 觸發舊 onload 流程
            this.iframe.srcdoc = "";
            this.iframe.src = "about:blank";
        }

        /**
         * Load article by URL
         * @param {string} url
         */
        loadUrl(url) {
            if (!this.iframe) throw new Error("ArticleViewer.loadUrl: call mount(container) first.");
            this._ensureNotDisposed();

            // ★重點：切第二篇時，先把舊內容關掉（避免新 loading 疊舊畫面）
            this._markLoading(true);
            this._stopCurrentLoad();

            this.iframe.removeAttribute("srcdoc");
            this.iframe.src = url;
        }

        /**
         * Load article by raw HTML (srcdoc)
         * @param {string} html
         */
        loadHtml(html) {
            if (!this.iframe) throw new Error("ArticleViewer.loadHtml: call mount(container) first.");
            this._ensureNotDisposed();

            this._markLoading(true);
            this._stopCurrentLoad();

            // srcdoc often becomes opaque origin; same-origin measurement may fail
            this.iframe.src = "about:blank";
            this.iframe.srcdoc = String(html || "");
        }

        /**
         * Force remeasure iframe height (only when autoHeight)
         */
        remeasure() {
            if (!this.iframe || this._disposed) return;
            if (this.opt.autoHeight === false) return;

            if (this._pendingRaf) cancelAnimationFrame(this._pendingRaf);
            this._pendingRaf = requestAnimationFrame(() => {
                this._pendingRaf = 0;
                this._tryMeasureSameOrigin();
            });
        }

        /**
         * Clear current content (keeps iframe)
         */
        reset() {
            if (!this.iframe) return;
            this._markLoading(false);
            this._stopCurrentLoad();
            if (this.opt.autoHeight !== false) {
                this._setHeight(this.opt.minHeight);
            }
        }

        /**
         * Destroy viewer and detach listeners
         */
        destroy() {
            this._disposed = true;

            window.removeEventListener("message", this._onMessageBound);
            window.removeEventListener("resize", this._onResizeBound);

            if (this._pendingRaf) cancelAnimationFrame(this._pendingRaf);
            this._pendingRaf = 0;

            if (this.iframe) {
                this.iframe.srcdoc = "";
                this.iframe.src = "about:blank";
            }
        }

        /* -------------------- internals -------------------- */

        _ensureNotDisposed() {
            if (this._disposed) throw new Error("ArticleViewer: instance has been destroyed.");
        }

        _markLoading(isLoading = true) {
            if (!this.container) return;
            this.container.classList.toggle("article-viewer--loading", !!isLoading);
        }

        _stopCurrentLoad() {
            if (!this.iframe) return;
            // about:blank 可中止前一個 request + 清畫面（避免疊在一起）
            this.iframe.srcdoc = "";
            this.iframe.src = "about:blank";
        }

        _onLoad() {
            // iframe load 完：先把主題 + 捲軸 CSS 注入，再收起 loading
            this._syncThemeVarsToIframe();
            this._injectScrollbarCssToIframe();

            this._markLoading(false);

            if (typeof this.opt.onLoad === "function") {
                try { this.opt.onLoad(); } catch { /* ignore */ }
            }

            if (this.opt.autoHeight === false) return;

            // initial measure + multiple delayed measures (images/fonts)
            this.remeasure();

            const times = Math.max(0, Number(this.opt.remeasureTimes) || 0);
            const delay = Math.max(0, Number(this.opt.remeasureDelayMs) || 0);

            for (let i = 1; i <= times; i++) {
                setTimeout(() => this.remeasure(), delay * i);
            }
        }

        _onError(err) {
            this._markLoading(false);
            if (typeof this.opt.onError === "function") {
                try { this.opt.onError(err); } catch { /* ignore */ }
            }
        }

        _tryMeasureSameOrigin() {
            if (!this.iframe) return;

            try {
                const doc = this.iframe.contentDocument;
                const win = this.iframe.contentWindow;
                if (!doc || !win) return;

                const de = doc.documentElement;
                const body = doc.body;

                const h1 = de ? de.scrollHeight : 0;
                const h2 = body ? body.scrollHeight : 0;
                const h3 = de ? de.offsetHeight : 0;
                const h4 = body ? body.offsetHeight : 0;

                let height = Math.max(h1, h2, h3, h4, this.opt.minHeight) + (this.opt.extraHeight || 0);
                if (!height || height < this.opt.minHeight) height = this.opt.minHeight;

                this._setHeight(height);
            } catch {
                // cross-origin or opaque origin -> ignore, keep current
            }
        }

        _setHeight(height) {
            if (!this.iframe) return;
            if (this.opt.autoHeight === false) return;

            const h = Math.max(this.opt.minHeight, Math.ceil(Number(height) || 0));
            if (h === this._lastHeight) return;

            this._lastHeight = h;

            // CSP-friendly: use attribute instead of inline style
            this.iframe.setAttribute("height", String(h));

            if (typeof this.opt.onHeight === "function") {
                try { this.opt.onHeight(h); } catch { /* ignore */ }
            }
        }

        _onMessage(e) {
            if (!this.iframe || !e || !e.data) return;
            if (this.opt.autoHeight === false) return;

            if (this.iframe.contentWindow && e.source !== this.iframe.contentWindow) return;

            const data = e.data;
            if (data && data.type === "ARTICLE_VIEWER_HEIGHT" && typeof data.height === "number") {
                this._setHeight(data.height + (this.opt.extraHeight || 0));
            }
        }

        _setIframeMode(isIframe) {
            if (!this.container) return;

            const offcanvasBody = this.container.closest(".offcanvas-body");
            if (!offcanvasBody) return;

            // ★你的需求：只有「真的用 iframe」時才加 use-iframe
            offcanvasBody.classList.toggle("use-iframe", !!isIframe);
        }

        /**
         * （保留未來 DOM 模式可能性）
         * 你如果之後要回 DOM 插入，就呼叫這個，會自動把 use-iframe 拿掉
         */
        loadDom(nodeOrHtml) {
            this._setIframeMode(false);
            // TODO: 你未來要 DOM mode 時再補
        }

        _syncThemeVarsToIframe() {
            const doc = this.iframe && this.iframe.contentDocument;
            if (!doc) return;

            const id = "av-theme-vars";
            const src = getComputedStyle(document.documentElement);

            const keys = [
                "--gallery-scroll-track",
                "--gallery-scroll-thumb",
                "--gallery-scroll-thumb-hover",
                "--gallery-accent",
                "--gallery-accent-hover",
                "--gallery-accent-contrast",
                "--gallery-accent-border",
                "--gallery-accent-contaner-background"
            ];

            let css = ":root{";
            keys.forEach(k => {
                const v = src.getPropertyValue(k).trim();
                if (v) css += `${k}:${v};`;
            });
            css += "}";

            let style = doc.getElementById(id);
            if (!style) {
                style = doc.createElement("style");
                style.id = id;
                style.setAttribute('nonce', this.opt.nonce);
                doc.body.appendChild(style);
            }
            style.textContent = css;
        }

        _injectScrollbarCssToIframe() {
            const doc = this.iframe && this.iframe.contentDocument;
            if (!doc) return;

            const id = "av-scrollbar-style";
            let style = doc.getElementById(id);
            if (!style) {
                style = doc.createElement("style");
                style.id = id;
                style.setAttribute('nonce', this.opt.nonce);
                doc.body.appendChild(style);
            }

            style.textContent = `
                html, body {
                    scrollbar-width: thin;
                    scrollbar-color: var(--gallery-scroll-thumb) var(--gallery-scroll-track);
                }
                body::-webkit-scrollbar { width: 6px; }
                body::-webkit-scrollbar-track { background: var(--gallery-scroll-track); border-radius: 999px; }
                body::-webkit-scrollbar-thumb { background: var(--gallery-scroll-thumb); border-radius: 999px; border: 1px solid var(--gallery-scroll-track); }
                body::-webkit-scrollbar-thumb:hover { background: var(--gallery-scroll-thumb-hover); }
            `.trim();
        }
    }

    global.ArticleViewer = ArticleViewer;
})(window);