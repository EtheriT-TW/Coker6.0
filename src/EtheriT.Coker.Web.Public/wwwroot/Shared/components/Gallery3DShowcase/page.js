(function (global) {
    "use strict";

    /* -------------------- load-once helpers -------------------- */
    function loadCssOnce(href, id) {
        return new Promise((resolve, reject) => {
            if (!href) return resolve();
            if (id && document.getElementById(id)) return resolve();

            const already = Array.from(document.querySelectorAll('link[rel="stylesheet"]'))
                .some(l => (l.href || "").includes(href));
            if (already) return resolve();

            const link = document.createElement("link");
            link.rel = "stylesheet";
            link.href = href;
            if (id) link.id = id;
            link.onload = () => resolve();
            link.onerror = () => reject(new Error("Failed to load css: " + href));
            document.head.appendChild(link);
        });
    }

    function loadScriptOnce(src, id) {
        return new Promise((resolve, reject) => {
            if (!src) return resolve();
            if (id && document.getElementById(id)) return resolve();

            const already = Array.from(document.scripts || [])
                .some(s => (s.src || "").includes(src));
            if (already) return resolve();

            const s = document.createElement("script");
            s.src = src;
            if (id) s.id = id;

            // 保守：不 async，讓執行時序可控
            s.async = false;

            s.onload = () => resolve();
            s.onerror = () => reject(new Error("Failed to load script: " + src));
            document.head.appendChild(s);
        });
    }

    /* -------------------- offcanvas shell -------------------- */
    function ensureOffcanvasFramework(opt) {
        const id = opt.offcanvasId || "galleryCanvas";
        let el = document.getElementById(id);

        if (el) {
            el.setAttribute("data-bs-backdrop", "false");
            el.setAttribute("data-bs-scroll", "true");
            el.setAttribute("data-role", el.getAttribute("data-role") || "canvas");
            return el;
        }

        el = document.createElement("div");
        el.className = "offcanvas offcanvas-start gallery-offcanvas";
        el.tabIndex = -1;
        el.id = id;
        el.setAttribute("aria-hidden", "true");
        el.setAttribute("data-bs-backdrop", "false");
        el.setAttribute("data-bs-scroll", "true");
        el.setAttribute("data-role", "canvas");

        el.innerHTML = `
              <div class="offcanvas-header">
                <button type="button"
                        class="btn-close"
                        data-role="canvas-close"
                        aria-label="Close"></button>
              </div>

              <div class="offcanvas-body" data-role="canvas-body">
                <div class="article-viewer" data-role="article-viewer-host"></div>
              </div>

              <button type="button"
                      class="gallery-canvas-toggle"
                      data-role="canvas-toggle"
                      aria-label="展開內容">▶</button>
        `.trim();

        document.body.appendChild(el);
        return el;
    }

    function qs(root, sel) { return root.querySelector(sel); }
    function qsa(root, sel) { return Array.from(root.querySelectorAll(sel)); }

    /* -------------------- DirectoryFacet hook helpers -------------------- */
    function callDirectoryFacet(fnName, arg1, arg2) {
        const df = global.DirectoryFacet;
        if (!df) return;
        const fn = df[fnName];
        if (typeof fn === "function") {
            try { fn.call(df, arg1, arg2); }
            catch (e) { console.warn("[DirectoryFacet hook error]", fnName, e); }
        }
    }

    /* -------------------- page (正式版：不做 demo 相容) -------------------- */
    const Gallery3DShowcasePage = {
        async init(userOpt) {
            const self = Gallery3DShowcasePage;
            const opt = self._mergeDefaults(userOpt);
            self.opt = opt;

            // route guards
            this._ignoreNextRouteOnce = false;
            this._lastHandledHash = "";

            if (!global.bootstrap || !global.bootstrap.Offcanvas) {
                throw new Error("Bootstrap Offcanvas not found. Please load bootstrap.bundle.js first.");
            }

            // Optional: standalone demo mode asset loading (formal site should preload via ModuleLoader)
            if (opt.paths) {
                await loadCssOnce(opt.paths.pageCss, opt.ids.pageCssId);
                await loadCssOnce(opt.paths.galleryCss, opt.ids.galleryCssId);
                await loadCssOnce(opt.paths.viewerCss, opt.ids.viewerCssId);
                await loadScriptOnce(opt.paths.viewerJs, opt.ids.viewerJsId);
                await loadScriptOnce(opt.paths.galleryJs, opt.ids.galleryJsId);
            }
            if (!global.ArticleViewer) throw new Error("ArticleViewer not found: " + opt.paths.viewerJs);
            if (!global.Gallery3D) throw new Error("Gallery3D not found: " + opt.paths.galleryJs);

            self.els = {
                mainStage: document.querySelector(opt.selectors.mainStage),
                stage: document.querySelector(opt.selectors.stage),
            };
            if (!self.els.stage) throw new Error("stage not found: " + opt.selectors.stage);

            this.imgRatio = opt.defaults.imgRatio;

            // offcanvas
            this.canvasEl = ensureOffcanvasFramework({ offcanvasId: opt.ids.offcanvasId });
            this.canvas = global.bootstrap.Offcanvas.getOrCreateInstance(this.canvasEl);

            this.canvasRefs = {
                host: qs(this.canvasEl, '[data-role="article-viewer-host"]'),
                close: qs(this.canvasEl, '[data-role="canvas-close"]'),
                toggle: qs(this.canvasEl, '[data-role="canvas-toggle"]'),
            };
            if (!this.canvasRefs.host) throw new Error("viewer host not found.");

            // viewer：同容器高、溢出用 scroll（不做高度計算）
            this.viewer = new global.ArticleViewer({ autoHeight: false });
            this.viewer.mount(this.canvasRefs.host);
            this.viewer.setMode("narrow");

            this.isExpanded = false;
            this._bindOffcanvas();
            this._bindResize();

            // page.js：只負責監聽與分派；資料/查詢由 DirectoryFacet.js 處理
            this._bindMenuSelector();
            this._bindFacetSelector();

            // mount gallery
            this.gallery = global.Gallery3D.mount(self.els.stage, {
                visibleCount: opt.defaults.visibleCount,
                on: { itemClick: (payload) => this._onItemClick(payload) },
            });
            window.__g3dGallery = this.gallery;

            $(document).off("catalog:rendered.g3d").on("catalog:rendered", ".catalog_frame", function () {
                const g = window.__g3dGallery;
                if (!g || typeof g.refreshFromDom !== "function") return;
                g.refreshFromDom({ preserveIndex: true });
            });

            // 進站還原（如果網址帶 g3d:）
            this.restoreFromUrlState();
            this._bootInitialMenu();

            // 路由監聽：只處理 g3d: 狀態
            if (window.onhashchange === hashChangeDirectory) {
                window.onhashchange = null;
            }
            window.addEventListener("hashchange", () => this._onRouteChanged());
            window.addEventListener("popstate", () => this._onRouteChanged());
        },

        _mergeDefaults(o) {
            const d = {
                paths: null,
                selectors: {
                    mainStage: ".main-stage",
                    stage: ".gallery3d-stage",
                    menuItem: ".edge-link[data-dirid]",
                    facetItem: "[data-role='facet-item']",
                    facetContainer: "[data-role='facet-selector']",
                },
                ids: {
                    offcanvasId: "galleryCanvas",
                    pageCssId: "g3d-showcase-css",
                    galleryCssId: "g3d-css",
                    galleryJsId: "g3d-js",
                    viewerCssId: "article-viewer-css",
                    viewerJsId: "article-viewer-js",
                },
                defaults: {
                    imgRatio: 16 / 9,
                    visibleCount: 30,
                },

                // 文章 URL 由外部提供（不把 fetch 寫在 page.js）
                urlBuilder: ({ id }) => (id != null && id !== "") ? ("/embed/posts/" + id) : "",
            };

            o = o || {};
            return {
                paths: { ...d.paths, ...(o.paths || {}) },
                selectors: { ...d.selectors, ...(o.selectors || {}) },
                ids: { ...d.ids, ...(o.ids || {}) },
                defaults: { ...d.defaults, ...(o.defaults || {}) },
                urlBuilder: typeof o.urlBuilder === "function" ? o.urlBuilder : d.urlBuilder,
            };
        },

        /* -------------------- routing (正式版只處理 g3d:) -------------------- */
        _onRouteChanged() {
            // ✅ 這次 route 變化是我們自己 commit 造成的：忽略一次，避免 restore 重跑造成 Facet API 重載
            if (this._ignoreNextRouteOnce) {
                this._ignoreNextRouteOnce = false;
                return;
            }

            const raw = (location.hash || "").replace(/^#/, "");

            // 只處理 g3d: 狀態
            if (!raw.toLowerCase().startsWith("g3d:")) return;

            // 同一個 hash 不重複處理（防止某些瀏覽器/插件造成重入）
            if (raw === this._lastHandledHash) return;
            this._lastHandledHash = raw;

            this.restoreFromUrlState();
        },

        _parseG3dStateFromUrl() {
            const h0 = (location.hash || "").replace(/^#/, "");
            if (!h0.toLowerCase().startsWith("g3d:")) return null;

            const qs0 = h0.slice(4);
            const out = {};
            qs0.split("&").forEach((kv) => {
                if (!kv) return;
                const i = kv.indexOf("=");
                const k = i >= 0 ? kv.slice(0, i) : kv;
                const v = i >= 0 ? kv.slice(i + 1) : "";
                if (!k) return;
                out[decodeURIComponent(k)] = decodeURIComponent(v || "");
            });

            // ✅ 正式版僅接受：menu / facet / item / mode
            const allowed = {};
            ["menu", "facet", "item", "mode"].forEach(k => {
                if (out[k] != null) allowed[k] = String(out[k]);
            });
            return allowed;
        },

        _buildG3dHash(state) {
            const parts = [];
            for (const k of ["menu", "facet", "item", "mode"]) {
                if (state[k] == null || state[k] === "") continue;
                parts.push(encodeURIComponent(k) + "=" + encodeURIComponent(String(state[k])));
            }
            return "#g3d:" + parts.join("&");
        },

        _commitG3dState(state, { replace = false } = {}) {
            const hash = this._buildG3dHash(state);
            const url = location.pathname + location.search + hash;

            // ✅ 可能觸發 hashchange：先標記忽略一次，避免 restoreFromUrlState 又去打 onStateChanged
            this._ignoreNextRouteOnce = true;

            if (replace) history.replaceState(state, "", url);
            else history.pushState(state, "", url);
        },

        /* -------------------- UI state helpers (正式版：不猜) -------------------- */
        _getActiveFacetValue() {
            const active = document.querySelector("[data-role='facet-item'].active");
            return active ? String(active.dataset.facetValue || "") : "";
        },

        _syncFacetActiveByValue(facetValue) {
            const items = qsa(document, this.opt.selectors.facetItem);
            if (!items.length) return;

            const target = items.find(x => String(x.dataset.facetValue || "") === String(facetValue || ""));
            if (!target) return;

            const container = target.closest(this.opt.selectors.facetContainer) || target.parentElement;
            if (container) qsa(container, ".active").forEach(x => x.classList.remove("active"));
            target.classList.add("active");
        },

        _getActiveMenuKey() {
            return String(document.documentElement.getAttribute("data-menu") || "");
        },

        _setActiveMenuKey(menuKey) {
            document.documentElement.setAttribute("data-menu", String(menuKey || ""));
        },

        /* -------------------- offcanvas -------------------- */
        _bindOffcanvas() {
            const r = this.canvasRefs;

            r.close?.addEventListener("click", (e) => {
                e.preventDefault();
                this.canvas.hide();
            });

            r.toggle?.addEventListener("click", (e) => {
                e.preventDefault();
                this._setExpanded(!this.isExpanded);

                const st = this._parseG3dStateFromUrl();
                if (st && st.item) {
                    st.mode = this.isExpanded ? "wide" : "narrow";
                    this._commitG3dState(st, { replace: true });
                }
            });

            this.canvasEl.addEventListener("hidden.bs.offcanvas", () => {
                this._setExpanded(false);
            });
        },

        _setExpanded(expanded) {
            this.isExpanded = !!expanded;
            this.canvasEl.classList.toggle("expand", this.isExpanded);

            if (this.canvasRefs.toggle) {
                this.canvasRefs.toggle.textContent = this.isExpanded ? "◀" : "▶";
            }
            this.viewer.setMode(this.isExpanded ? "wide" : "narrow");
        },

        /* -------------------- main stage resize -------------------- */
        _bindResize() {
            const resize = () => {
                if (!this.els || !this.els.mainStage) return;

                let headerH_tem = document.querySelector("header")?.offsetHeight ?? 0;
                headerH_tem = headerH_tem == 0 ? document.querySelector("header .navbar")?.offsetHeight ?? 0 : headerH_tem;
                const headerH = headerH_tem;
                const footerH = document.querySelector(`[data-role="facet-selector"]`)?.offsetHeight ?? 0;

                const availH = window.innerHeight - headerH - footerH;
                const availW = window.innerWidth;

                const ratio = availW / availH;
                let w, h;
                if (ratio > this.imgRatio) { h = availH; w = h * this.imgRatio; }
                else { w = availW; h = w / this.imgRatio; }

                this.els.mainStage.style.width = w + "px";
                this.els.mainStage.style.height = h + "px";
            };

            window.addEventListener("resize", resize);
            resize();
        },

        /* -------------------- menu selector (menus -> dirId) -------------------- */
        _bindMenuSelector() {
            document.addEventListener("click", (e) => {
                const el = e.target.closest(this.opt.selectors.menuItem);
                if (!el) return;

                const menuKey =
                    el.dataset.dirId ||
                    el.dataset.dirid ||
                    el.dataset.menuId ||
                    el.dataset.menuid ||
                    "";

                if (!menuKey) return;

                this._setActiveMenuKey(menuKey);

                // ✅ 切換目錄：先清掉 iframe（停影片），再關閉 offcanvas
                if (this.viewer && typeof this.viewer.clear === "function") {
                    this.viewer.clear();
                }
                this._setExpanded(false);
                this.canvas.hide();

                const ctx = { source: "menu", el };
                callDirectoryFacet("onMenuChanged", String(menuKey), ctx);

                const st = this._parseG3dStateFromUrl();
                if (st && st.item) {
                    st.menu = String(menuKey);
                    st.facet = st.facet || this._getActiveFacetValue();
                    this._commitG3dState(st, { replace: true });

                    // ✅ menu click 本來就要換資料：這裡照舊，不靠 route restore
                    callDirectoryFacet("onStateChanged", { menu: st.menu, facet: st.facet }, ctx);
                }
            });
        },

        /* -------------------- facet selector (facet) -------------------- */
        _bindFacetSelector() {
            document.addEventListener("click", (e) => {
                const el = e.target.closest(this.opt.selectors.facetItem);
                if (!el) return;

                const facetValue = String(el.dataset.facetValue || "");
                if (!facetValue) return;

                const container = el.closest(this.opt.selectors.facetContainer) || el.parentElement;
                if (container) qsa(container, ".active").forEach(x => x.classList.remove("active"));
                el.classList.add("active");

                const ctx = { source: "facet", el };
                callDirectoryFacet("onFacetChanged", facetValue, ctx);

                const st = this._parseG3dStateFromUrl();
                if (st && st.item) {
                    st.facet = facetValue;
                    st.menu = st.menu || this._getActiveMenuKey();
                    this._commitG3dState(st, { replace: true });

                    // ✅ facet click 本來就要換資料：這裡照舊，不靠 route restore
                    callDirectoryFacet("onStateChanged", { menu: st.menu, facet: st.facet }, ctx);
                }
            });
        },

        /* -------------------- restore from url -------------------- */
        restoreFromUrlState() {
            const st = this._parseG3dStateFromUrl();
            if (!st || !st.item) return;

            // 先同步 UI (menu/facet/mode/canvas)
            if (st.menu) this._setActiveMenuKey(st.menu);
            if (st.facet) this._syncFacetActiveByValue(st.facet);

            this._setExpanded(String(st.mode || "").toLowerCase() === "wide");
            this.canvas.show();

            // 先載 viewer
            const url = this.opt.urlBuilder({
                id: st.item,
                item: st.item,
                menu: st.menu,
                facet: st.facet
            });
            if (url) this.viewer.loadUrl(url);

            // ✅ 關鍵：只有當「menu/facet 跟目前狀態不同」才呼叫 onStateChanged，避免 hashchange 造成 Facet API 重跑
            const currentMenu = this._getActiveMenuKey();
            const currentFacet = this._getActiveFacetValue();

            const needMenuSync = (st.menu || "") !== (currentMenu || "");
            const needFacetSync = (st.facet || "") !== (currentFacet || "");

            // 另外：如果目前 facet 按鈕區根本還沒 render（第一次進來），也需要讓 DirectoryFacet 跑一次把 facet 生出來
            const facetUiReady = document.querySelectorAll(this.opt.selectors.facetItem).length > 0;

            if (needMenuSync || needFacetSync || !facetUiReady) {
                callDirectoryFacet(
                    "onStateChanged",
                    { menu: st.menu || "", facet: st.facet || "" },
                    { source: "restore" }
                );
            }
        },

        /* -------------------- Gallery item click -------------------- */
        _onItemClick(payload) {
            const id =
                (payload && (payload.id ?? payload.articleId ?? payload.postId)) ||
                (payload && payload.card && payload.card.dataset && (payload.card.dataset.id || payload.card.dataset.articleId)) ||
                "";

            if (!id) return;

            // 先開 offcanvas
            this._setExpanded(false);
            this.canvas.show();

            const menu = this._getActiveMenuKey();
            const facet = this._getActiveFacetValue();
            const mode = this.isExpanded ? "wide" : "narrow";

            this._commitG3dState({ menu, facet, item: String(id), mode }, { replace: false });

            const url = this.opt.urlBuilder({ id: String(id), item: String(id), menu, facet });
            if (url) this.viewer.loadUrl(url);
        },

        _bootInitialMenu() {
            const st = this._parseG3dStateFromUrl();
            if (st && st.item) return;

            const raw = (location.hash || "").replace(/^#/, "").trim();
            let menuFromHash = "";

            if (raw.toLowerCase().startsWith("menu-")) {
                menuFromHash = raw.slice(5).trim();
            }

            if (menuFromHash) {
                this._setActiveMenuKey(menuFromHash);
                callDirectoryFacet("onMenuChanged", String(menuFromHash), { source: "boot-hash" });
                return;
            }

            const def =
                document.querySelector(".edge-menu [data-default='1'][data-dirid], .edge-menu [data-default='1'][data-dir-id]") ||
                document.querySelector(".edge-menu [data-dirid], .edge-menu [data-dir-id]");

            if (!def) return;

            const dirId = def.dataset.dirId || def.dataset.dirid || def.getAttribute("data-dirid") || def.getAttribute("data-dir-id") || "";
            if (!dirId) return;

            this._setActiveMenuKey(dirId);
            callDirectoryFacet("onMenuChanged", String(dirId), { source: "boot", el: def });
        },
    };

    global.Gallery3DShowcasePage = Gallery3DShowcasePage;
    // Bind init to avoid losing context when called as a standalone function
    global.Gallery3DShowcasePage.init = global.Gallery3DShowcasePage.init.bind(global.Gallery3DShowcasePage);
})(window);
