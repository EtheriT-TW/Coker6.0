/*!
 * Gallery3D (封裝版)
 * - 僅負責 main-stage（含）底下的 3D Carousel + Grid 模式 + 工具列
 * - 外部可透過 on.itemClick 接手點擊行為（例如開啟 offcanvas）
 */
(function (global) {
    'use strict';

    function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }
    function normalizeIndex(v, total) {
        let x = v;
        while (x < 0) x += total;
        while (x >= total) x -= total;
        return x;
    }

    const DEFAULT_I18N = {
        modeCarousel: '環形模式',
        modeGrid: '格狀模式',
        prev: '◀ 上一張',
        next: '下一張 ▶',
        pause: '暫停',
        play: '播放',
        reset: '回到第一個',
        indicator: (idx1, total) => `第 ${idx1} / ${total} 張`,
        gridIndicator: (total) => `共 ${total} 張`
    };

    class Gallery3D {
        /**
         * @param {HTMLElement|string} stageElOrSelector 例如：#gallery3d（.gallery3d-stage）
         * @param {object} options
         */
        constructor(stageElOrSelector, options) {
            this.stageEl = typeof stageElOrSelector === 'string'
                ? document.querySelector(stageElOrSelector)
                : stageElOrSelector;

            if (!this.stageEl) throw new Error('Gallery3D: stage element not found.');

            const opts = options || {};
            const i18n = Object.assign({}, DEFAULT_I18N, opts.i18n || {});
            this.opts = Object.assign({
                cardSelector: '.gallery3d-card',
                wrapperSelector: '.gallery3d-wrapper',
                holderSelector: '.gallery-holder',
                autoPlayInterval: 2600,
                autoPlayStartDelay: 5000,
                dragPixelsPerStep: 140,
                scaleMin: 0.80,
                scaleMax: 1.08,
                tiltX: -5,
                maxItems: null,
                items: Array.isArray(opts.items) ? opts.items : null,
                renderItem: (typeof opts.renderItem === 'function') ? opts.renderItem : null,
                visibleCount: 30,
                // items 會插入/搜尋的容器（預設：stage 內的 data-role=catalog-items）
                itemsHostSelector: '[data-role="catalog-items"]',
                // 少於此數量時強制 grid（預設 6；你可設成 visibleCount）
                gridThreshold: (typeof opts.gridThreshold === 'number' ? opts.gridThreshold : 6),
                // items 變更後是否自動依張數切換模式
                autoMode: (typeof opts.autoMode === 'boolean' ? opts.autoMode : true),
                toolbar: true,
                i18n,
                on: Object.assign({ itemClick: null }, (opts.on || {})),
            }, opts);

            // DOM refs
            this.wrapperEl = this.stageEl.closest(this.opts.wrapperSelector) || document.querySelector(this.opts.wrapperSelector);
            if (!this.wrapperEl) throw new Error('Gallery3D: wrapper element not found.');

            this.holderEl = this.wrapperEl.closest(this.opts.holderSelector) || document.querySelector(this.opts.holderSelector);

            // state
            // items host（真正插卡片的容器）
            this.itemsHostEl = this.stageEl.querySelector(this.opts.itemsHostSelector) || this.stageEl;
            // state（只抓 items host 內的 cards，避免 templates 被算進去）
            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            this.cards = [];
            this.extraCards = [];

            this.total = 0;
            this.displayMode = 'carousel';
            this.forceGridMode = false;

            // 使用者手動選擇模式（null=未選；'grid'/'carousel'）
            this.userSelectedMode = null;
            // 是否已完成一次完整初始化（toolbar/bind/layout）
            this._inited = false;

            this.indexFloat = 0;
            this.autoTimer = null;
            this.autoStartTimer = null;
            this._autoStartedOnce = false;
            this.userPaused = false;

            this.isDragging = false;
            this.dragStartX = 0;
            this.dragStartIndex = 0;
            this.lastIndexForVelocity = 0;
            this.lastTime = 0;
            this.velocity = 0;
            this.inertiaId = null;
            this.dragDistance = 0;

            // toolbar refs
            this.toolbarRowEl = null;
            this.btnMode = null;
            this.btnPause = null;
            this.btnReset = null;
            this.indicatorEl = null;
            this.carouselControlsEl = null;
        }

        mount() {
            if (Array.isArray(this.opts.items)) {
                this.setItems(this.opts.items, { silent: true });
            }
            // 若此時尚無卡片，也先完成 toolbar/事件初始化，等資料進來再 refresh/setItems
            if (!this._inited) {
                if (this.opts.toolbar) this._ensureToolbar();
                this._bindEvents();
                this._inited = true;
            }
            if (!this.allCards.length) {
                this.total = 0;
                this.forceGridMode = this._shouldForceGrid();
                this.displayMode = 'grid';
                if (this.toolbarRowEl) this.toolbarRowEl.classList.add('d-none');
                if (this.btnMode) this.btnMode.disabled = true;
                this._switchToGrid(true);
                return this;
            }

            const maxItems = (typeof this.opts.maxItems === 'number' && this.opts.maxItems > 0)
                ? this.opts.maxItems
                : null;

            // 先把「總顯示張數」裁掉（grid / carousel 都會一致）
            if (maxItems != null) {
                const keep = this.allCards.slice(0, maxItems);
                const drop = this.allCards.slice(maxItems);
                drop.forEach(el => el.remove());
                this.allCards = keep;
            }

            const limit = (typeof this.opts.visibleCount === 'number' ? this.opts.visibleCount : 30);
            this.cards = this.allCards.slice(0, limit);
            this.extraCards = this.allCards.slice(limit);

            this.total = this.cards.length;
            this.forceGridMode = this._shouldForceGrid();
            this.displayMode = this.forceGridMode ? 'grid' : 'carousel';

            // grid-only 標記（保持原本「格狀可看全部」的行為）
            this.extraCards.forEach(c => c.classList.add('grid-only'));

            // 套卡片寬度（與原本相同：依張數動態）
            this._applyCardWidth();

            // 建倒影（只做一次）
            this._ensureReflections();

            // 建工具列（你的選項：A，工具列屬於元件的一部分）
            // 建工具列 / 綁事件（只做一次）
            if (!this._inited) {
                if (this.opts.toolbar) this._ensureToolbar();
                this._bindEvents();
                this._inited = true;
            }
            // 初次 layout / 自動播放
            if (this.displayMode === 'grid') this._syncGridColumnsByTotal();
            if (this.forceGridMode) {
                this._switchToGrid(true);
            } else {
                this._layout();
                this._startAuto({ delay: this.opts.autoPlayStartDelay, force: true });
            }
            return this;
        }


        /** 內部：依目前設定判斷是否應強制 grid（少卡片） */
        _shouldForceGrid() {
            const w = window.innerWidth || document.documentElement.clientWidth || 0;
            const isPortrait = window.matchMedia && window.matchMedia('(orientation: portrait)').matches;

            // 你要的規則（自行調整門檻）
            const lockPhoneMax = 768;      // 手機
            const lockPortraitMax = 1024;  // 平板直/手機直

            if (w <= lockPhoneMax) return true;
            if (isPortrait && w <= lockPortraitMax) return true;

            const t = (typeof this.opts.gridThreshold === 'number' && this.opts.gridThreshold > 0)
                ? this.opts.gridThreshold
                : 6;
            const n = (typeof this.totalAll === "number") ? this.totalAll : this.total;
            return n < t;
        }

        destroy() {
            this._stopAuto();
            this._stopInertia();
            // 事件移除：此版本未做完整 event registry（若你需要 destroy 可再補齊）
            return this;
        }

        /**
 * 取得目前 items（由 DOM 反推）
 */
        getItems() {
            const cards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            return cards.map(card => this._extractItemFromCard(card));
        }

        /**
         * 從目前 DOM（itemsHost）重新抓 card，並依張數/模式重新 layout
         * - 外部若採用「先 append DOM 再觸發」可呼叫這個
         */
        refreshFromDom(opt) {
            const options = opt || {};
            const preserveIndex = options.preserveIndex !== false;
            const prevIndex = this.indexFloat;
            this._stopAuto();
            if (!this._inited) {
                if (this.opts.toolbar) this._ensureToolbar();
                this._bindEvents();
                this._inited = true;
            }
            this._stopInertia();
            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            this._refreshAfterItemsChanged({ preserveIndex, prevIndex });
        }

        /**
         * 整包重建 items（以現有 HTML 第一張 card 作 template）
         * @param {Array<object>} items
         * @param {{silent?:boolean, preserveIndex?:boolean}} opt
         */
        setItems(items, opt) {
            const options = opt || {};
            const preserveIndex = !!options.preserveIndex;
            const prevIndex = this.indexFloat;

            // 停掉播放/慣性，避免中途動 DOM 出現跳動
            this._stopAuto();
            this._stopInertia();

            // 先建立 template（優先用現有第一張 card）
            if (!this._inited) {
                if (this.opts.toolbar) this._ensureToolbar();
                this._bindEvents();
                this._inited = true;
            }

            const template = this._getCardTemplate();

            // 清空 stage
            this.itemsHostEl.innerHTML = '';

            // 重建 cards
            (items || []).forEach((it) => {
                const card = template.cloneNode(true);
                card.classList.add('gallery3d-card');
                card.classList.remove('grid-only', 'is-front');

                this._applyItemToCard(card, it);
                this.itemsHostEl.appendChild(card);
            });

            // 重新抓卡片集合
            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));

            // 重新切分 / 套狀態 / layout
            this._refreshAfterItemsChanged({ preserveIndex, prevIndex });

            if (!options.silent) {
                // 若你未來需要「完全重綁」才加，現在你的事件是綁在 stage/wrapper 上，
                // 用 event delegation + elementFromPoint，因此通常不需要重綁。
            }
        }
        /**
         * 從目前 DOM（itemsHostEl）重新同步 cards，並依據數量自動切換 grid / carousel，更新 toolbar。
         * 使用情境：外部（例如 DirectoryDataInsert）已經把 .gallery3d-card 插好了，只需要讓元件「重新算一次」。
         * @param {{preserveIndex?:boolean}} opt
         */
        syncFromDom(opt) {
            const options = opt || {};
            const prevIndex = this.indexFloat;

            this._stopAuto();
            this._stopInertia();

            if (!this._inited) {
                if (this.opts.toolbar) this._ensureToolbar();
                this._bindEvents();
                this._inited = true;
            }

            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));

            this._refreshAfterItemsChanged({
                preserveIndex: !!options.preserveIndex,
                prevIndex
            });
            return this;
        }



        /**
         * 新增 item（預設 append，亦可指定插入 index）
         */
        addItem(item, index) {
            const template = this._getCardTemplate();
            const card = template.cloneNode(true);
            card.classList.add('gallery3d-card');
            card.classList.remove('grid-only', 'is-front');

            this._applyItemToCard(card, item);

            const cards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            const insertAt = (typeof index === 'number') ? Math.max(0, Math.min(index, cards.length)) : cards.length;

            if (insertAt >= cards.length) this.itemsHostEl.appendChild(card);
            else this.itemsHostEl.insertBefore(card, cards[insertAt]);

            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            this._refreshAfterItemsChanged({ preserveIndex: true, prevIndex: this.indexFloat });
        }

        /**
         * 移除 item：可傳 index 或 predicate(card, index) => boolean
         */
        removeItem(indexOrPredicate) {
            const cards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            let removeIndex = -1;

            if (typeof indexOrPredicate === 'number') {
                removeIndex = indexOrPredicate;
            } else if (typeof indexOrPredicate === 'function') {
                removeIndex = cards.findIndex((c, i) => !!indexOrPredicate(c, i));
            }

            if (removeIndex < 0 || removeIndex >= cards.length) return false;

            cards[removeIndex].remove();

            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            this._refreshAfterItemsChanged({ preserveIndex: true, prevIndex: this.indexFloat });
            return true;
        }

        /**
         * 更新 item：以 index 找到卡片後套用 patch（部分更新）
         */
        updateItem(index, patch) {
            const cards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            if (index < 0 || index >= cards.length) return false;

            const card = cards[index];
            const current = this._extractItemFromCard(card);
            const next = Object.assign({}, current, patch || {});
            this._applyItemToCard(card, next);

            // 更新後一般不需要重切分，但圖片數量/限制沒變；為保守仍更新反射/布局
            this._ensureReflections();
            if (this.displayMode === 'carousel') this._layout();
            return true;
        }

        /** 內部：items 更新後重建狀態 */
        _refreshAfterItemsChanged(ctx) {
            const limit = (typeof this.opts.visibleCount === 'number' ? this.opts.visibleCount : 30);
            const maxItems = (typeof this.opts.maxItems === 'number' && this.opts.maxItems > 0)
                ? this.opts.maxItems
                : null;

            this.allCards = Array.from(this.itemsHostEl.querySelectorAll(this.opts.cardSelector));
            if (maxItems != null && this.allCards.length > maxItems) {
                this.allCards.slice(maxItems).forEach(el => el.remove());
                this.allCards = this.allCards.slice(0, maxItems);
            }

            this.cards = this.allCards.slice(0, limit);
            this.extraCards = this.allCards.slice(limit);

            const prevForced = this.forceGridMode;

            this.totalAll = this.allCards.length;
            this.total = this.cards.length;
            this.forceGridMode = this._shouldForceGrid();

            this.allCards.forEach(el => el.classList.remove('grid-only'));
            this.extraCards.forEach(el => el.classList.add('grid-only'));

            // ✅ 有卡片就一定要有 cardWidthRatio；首次資料進來尤其需要
            if (this.total > 0) {
                this._applyCardWidth();
            }

            // ✅ 外部 append 的卡片，倒影可能不存在
            this._ensureReflections();

            // toolbar / mode 按鈕狀態
            if (this.forceGridMode) {
                if (this.toolbarRowEl) this.toolbarRowEl.classList.add('d-none');
                if (this.btnMode) this.btnMode.disabled = true;
            } else {
                if (this.toolbarRowEl) this.toolbarRowEl.classList.remove('d-none');
                if (this.btnMode) this.btnMode.disabled = false;
            }

            if (this.forceGridMode) {
                // 少卡片：強制 grid（不讓使用者切換）
                this._switchToGrid(true);
                return;
            }

            // preserve index
            if (ctx && ctx.preserveIndex) {
                const max = Math.max(0, this.total - 1);
                const rounded = Math.round(ctx.prevIndex || 0);
                this.indexFloat = normalizeIndex(Math.min(rounded, max), this.total);
            } else {
                this.indexFloat = 0;
            }
            // 若上一輪是強制 grid（少卡），而現在已解除，且使用者沒有手動鎖定 grid，則自動切回 carousel
            if (this.opts.autoMode && prevForced && this.displayMode === 'grid' && this.userSelectedMode !== 'grid') {
                this._switchToCarousel();
                return;
            }

            if (this.displayMode === 'grid') {
                this._switchToGrid(true);
                this._syncGridColumnsByTotal();
                return;
            }

            // carousel
            this._layout();
            if (!this.userPaused) this._startAuto();
        }

        /** 內部：取得 card template（優先 clone 現有第一張） */
        _getCardTemplate() {
            // 若 stage 內本來就有 card，用第一張當 template
            const existing = this.itemsHostEl.querySelector(this.opts.cardSelector);
            if (existing) return existing.cloneNode(true);

            // fallback：如果完全沒有任何 card（純 items 初始化），用 minimal template
            const div = document.createElement('div');
            div.className = 'gallery3d-card';
            div.innerHTML = `
              <figure class="gallery3d-card-inner">
                <div class="card-main">
                  <img class="main" alt="">
                </div>
                <div class="card-reflect"></div>
                <figcaption></figcaption>
              </figure>
            `;
            return div;
        }

        /** 內部：把 item 套到 card（你可用 opts.renderItem 覆蓋） */
        _applyItemToCard(card, item) {
            if (this.opts.renderItem) {
                this.opts.renderItem(card, item);
                return;
            }
            const it = item || {};
            const img = card.querySelector('img.main');
            if (img && it.src) img.src = it.src;

            const cap = card.querySelector('figcaption');
            if (cap) cap.textContent = (it.title != null) ? String(it.title) : '';

            // ✅ id：統一寫到 card 的 data-id，方便外部/內部追蹤
            if (it.id != null && it.id !== '') card.setAttribute('data-id', String(it.id));
            else card.removeAttribute('data-id');

            // 你若有 link/btn 等延伸欄位，也可以在這裡套用
            // e.g. card.dataset.id = it.id;
        }

        /** 內部：從 card 反推出 item（用於 update 合併） */
        _extractItemFromCard(card) {
            const img = card.querySelector('img.main');
            const cap = card.querySelector('figcaption');

            const id =
                (card && card.dataset && (card.dataset.id || card.dataset.articleId || card.dataset.postId)) ||
                card.getAttribute("data-id") ||
                "";

            return {
                id: id,
                src: img ? img.src : '',
                title: cap ? cap.textContent.trim() : ''
            };
        }

        _applyCardWidth() {
            const total = this.total;
            const opts = this.opts;

            const maxWidthFewCards = 36;   // <=4
            const midWidthPercent = 36;    // ~10
            const minWidthPercent = 22;    // many
            const fewCardsThreshold = 4;
            const midCardsThreshold = 10;
            const maxCardsForCircle = opts.visibleCount || 30;

            const getCardWidthPercent = (t) => {
                if (t <= fewCardsThreshold) return maxWidthFewCards;
                if (t <= midCardsThreshold) {
                    const k = (t - fewCardsThreshold) / (midCardsThreshold - fewCardsThreshold);
                    return maxWidthFewCards + (midWidthPercent - maxWidthFewCards) * k;
                }
                const clampedTotal = Math.min(t, maxCardsForCircle);
                const k = (clampedTotal - midCardsThreshold) / (maxCardsForCircle - midCardsThreshold);
                return midWidthPercent + (minWidthPercent - midWidthPercent) * k;
            };

            this.baseCardWidthPercent = getCardWidthPercent(total);
            this.baseCardWidthRatio = (this.baseCardWidthPercent / 100) * opts.scaleMax;

            this.cardWidthPercent = this.baseCardWidthPercent;
            this.cardWidthRatio = this.baseCardWidthRatio;

            this.cards.forEach(card => {
                card.style.width = this.cardWidthPercent + '%';
            });
        }

        _ensureReflections() {
            this.cards.forEach(card => {
                const mainImg = card.querySelector('img.main');
                const reflectHost = card.querySelector('.card-reflect');
                if (mainImg && reflectHost && !reflectHost.querySelector('img.reflection')) {
                    const clone = mainImg.cloneNode(true);
                    clone.classList.add('reflection');
                    reflectHost.appendChild(clone);
                }
            });
        }

        _ensureToolbar() {
            // 插入位置：main-stage（以 holder 的 parent 當作 main-stage）
            const mainStage = this.holderEl ? this.holderEl.parentElement : null;
            if (!mainStage) return;

            // 避免重複建立
            if (mainStage.querySelector('.gallery-toolbar-row')) {
                this.toolbarRowEl = mainStage.querySelector('.gallery-toolbar-row');
                this.btnMode = this.toolbarRowEl.querySelector('[data-role="mode"]');
                this.btnPause = this.toolbarRowEl.querySelector('[data-role="pause"]');
                this.btnReset = this.toolbarRowEl.querySelector('[data-role="reset"]');
                this.indicatorEl = this.toolbarRowEl.querySelector('[data-role="indicator"]');
                this.carouselControlsEl = this.toolbarRowEl.querySelector('[data-role="controls"]');
                return;
            }

            const wrap = document.createElement('div');
            wrap.className = 'gallery-toolbar-wrap';

            const row = document.createElement('div');
            row.className = 'gallery-toolbar-row';

            // 模式
            const modeBox = document.createElement('div');
            modeBox.className = 'gallery-toolbar gallery-toolbar-mode';
            modeBox.innerHTML = `
            <div class="gallery-toolbar-inner">
                <button type="button" data-role="mode">${this.opts.i18n.modeCarousel}</button>
            </div>
            `;

            // 控制
            const ctrlBox = document.createElement('div');
            ctrlBox.className = 'gallery-toolbar gallery-toolbar-controls';
            ctrlBox.setAttribute('data-role', 'controls');
            ctrlBox.innerHTML = `
            <div class="gallery-toolbar-inner">
                <button type="button" data-role="reset" title="${this.opts.i18n.reset || 'Reset'}" aria-label="${this.opts.i18n.reset || 'Reset'}">⟳</button>
                <button type="button" data-dir="-1">${this.opts.i18n.prev}</button>
                <button type="button" data-dir="1">${this.opts.i18n.next}</button>
                <button type="button" data-role="pause">${this.opts.i18n.pause}</button>
                <span class="gallery-indicator" data-role="indicator"></span>
            </div>
            `;

            wrap.appendChild(row);
            row.appendChild(modeBox);
            row.appendChild(ctrlBox);

            // 放在 holder 之後（與原本結構一致）
            mainStage.appendChild(wrap);

            this.toolbarRowEl = row;
            this.btnMode = row.querySelector('[data-role="mode"]');
            this.btnPause = row.querySelector('[data-role="pause"]');
            this.btnReset = row.querySelector('[data-role="reset"]');
            this.indicatorEl = row.querySelector('[data-role="indicator"]');
            this.carouselControlsEl = row.querySelector('[data-role="controls"]');

            if (this.forceGridMode) {
                row.classList.add('d-none');
                if (this.btnMode) this.btnMode.disabled = true;
            }
        }

        _bindEvents() {
            const opts = this.opts;

            // prev/next
            if (this.toolbarRowEl) {
                this.toolbarRowEl.querySelectorAll('[data-dir]').forEach(btn => {
                    btn.addEventListener('click', () => {
                        if (this.displayMode !== 'carousel') return;
                        this._stopInertia();
                        this.indexFloat = normalizeIndex(this.indexFloat + parseInt(btn.dataset.dir, 10), this.total);
                        this._layout();
                        this._startAuto();
                    });
                });
            }

            // pause
            if (this.btnPause) {
                this.btnPause.addEventListener('click', () => {
                    this.userPaused = !this.userPaused;
                    if (this.userPaused) {
                        this.btnPause.textContent = this.opts.i18n.play;
                        this._stopAuto();
                        this._stopInertia();
                    } else {
                        this.btnPause.textContent = this.opts.i18n.pause;
                        this._startAuto();
                    }
                });
            }


            // reset to first + delay autoplay
            if (this.btnReset) {
                this.btnReset.addEventListener('click', () => {
                    if (this.displayMode !== 'carousel') return;

                    // reset always resumes autoplay after delay
                    this.userPaused = false;
                    if (this.btnPause) this.btnPause.textContent = this.opts.i18n.pause;

                    this._stopInertia();
                    this.indexFloat = 0;
                    this._layout();
                    this._startAuto({ delay: this.opts.autoPlayStartDelay, force: true });
                });
            }

            // mode
            if (this.btnMode) {
                this.btnMode.addEventListener('click', () => {
                    if (this.forceGridMode) return;
                    if (this.displayMode === 'carousel') {
                        this.userSelectedMode = 'grid';
                        this._switchToGrid(false);
                    } else {
                        this.userSelectedMode = 'carousel';
                        this._switchToCarousel();
                    }
                });
            }

            // hover pause/resume（保持原本行為）
            this.wrapperEl.addEventListener('mouseenter', () => {
                if (this.displayMode === 'carousel') this._stopAuto();
            });
            this.wrapperEl.addEventListener('mouseleave', () => {
                if (!this.userPaused && this.displayMode === 'carousel') this._startAuto();
            });

            // drag (pointer)
            this.stageEl.addEventListener('pointerdown', (e) => {
                if (this.displayMode !== 'carousel') return;

                this.isDragging = true;
                this.dragStartX = e.clientX;
                this.dragStartIndex = this.indexFloat;
                this.lastIndexForVelocity = this.indexFloat;
                this.lastTime = performance.now();
                this.dragDistance = 0;

                this._stopAuto();
                this._stopInertia();
                this.stageEl.setPointerCapture(e.pointerId);
            });

            this.stageEl.addEventListener('pointermove', (e) => {
                if (!this.isDragging || this.displayMode !== 'carousel') return;

                const dx = e.clientX - this.dragStartX;
                this.dragDistance = Math.max(this.dragDistance, Math.abs(dx));
                const steps = dx / opts.dragPixelsPerStep;
                this.indexFloat = this.dragStartIndex - steps;
                this._layout();

                const now = performance.now();
                const dt = now - this.lastTime;
                if (dt > 0) {
                    this.velocity = (this.indexFloat - this.lastIndexForVelocity) / dt;
                    this.lastIndexForVelocity = this.indexFloat;
                    this.lastTime = now;
                }
            });

            this.stageEl.addEventListener('dragstart', (e) => e.preventDefault());

            const endDrag = (e) => {
                if (!this.isDragging || this.displayMode !== 'carousel') return;
                this.isDragging = false;
                try { this.stageEl.releasePointerCapture(e.pointerId); } catch (_) { }

                // click 判斷（沿用你原本的 elementFromPoint 方式）
                if (this.dragDistance <= 5) {
                    const target = document.elementFromPoint(e.clientX, e.clientY);
                    const card = target && target.closest('.gallery3d-card');
                    if (card) this._emitItemClickFromCard(card);
                    this._startAuto();
                    return;
                }

                if (Math.abs(this.velocity) > 0.001) {
                    this._startInertia();
                } else {
                    this.indexFloat = normalizeIndex(Math.round(this.indexFloat), this.total);
                    this._layout();

                    // 原本的「補一格」節奏保持
                    this.indexFloat = normalizeIndex(this.indexFloat + 1, this.total);
                    this._layout();
                    this._startAuto();
                }
            };

            this.stageEl.addEventListener('pointerup', endDrag);
            this.stageEl.addEventListener('pointercancel', endDrag);

            // grid click（沿用原本 elementFromPoint）
            this.wrapperEl.addEventListener('click', (e) => {
                if (this.displayMode !== 'grid') return;
                const target = document.elementFromPoint(e.clientX, e.clientY);
                const card = target && target.closest('.gallery3d-card');
                if (!card) return;
                this._emitItemClickFromCard(card);
            });
        }

        _emitItemClickFromCard(card) {
            const img = card.querySelector('img.main');
            if (!img) return;
            const captionEl = card.querySelector('figcaption');
            const title = captionEl ? captionEl.textContent.trim() : '';
            const onClick = this.opts.on && this.opts.on.itemClick;

            if (typeof onClick === 'function') {
                const idx = this.allCards.indexOf(card);
                onClick({ src: img.src, title, card, index: idx });
            }
        }

        _layout() {
            if (this.displayMode !== 'carousel') return;

            const opts = this.opts;
            const total = this.total;
            const activeIndex = Math.round(normalizeIndex(this.indexFloat, total));
            const stepDeg = 360 / total;

            // 半徑等比縮放
            const wrapperRect = this.wrapperEl.getBoundingClientRect();

            // 先回到「基準卡片寬度」（避免前一次因為太擠而縮小後永久縮著）
            if (typeof this.baseCardWidthPercent === 'number' && typeof this.baseCardWidthRatio === 'number') {
                if (this.cardWidthPercent !== this.baseCardWidthPercent) {
                    this.cardWidthPercent = this.baseCardWidthPercent;
                    this.cardWidthRatio = this.baseCardWidthRatio;
                    this.cards.forEach(c => c.style.width = this.cardWidthPercent + '%');
                }
            }

            const spacingFactor = (typeof opts.spacingFactor === 'number' && opts.spacingFactor > 0)
                ? opts.spacingFactor
                : 1.10;

            const stepRad = 2 * Math.PI / total;

            // 半徑上/下限：不要再讓 maxRadius = 1.20 這麼大，會直接撞旁邊 UI
            const radiusMinRatio = (typeof opts.radiusMinRatio === 'number') ? opts.radiusMinRatio : 0.30;
            const radiusMaxRatio = (typeof opts.radiusMaxRatio === 'number') ? opts.radiusMaxRatio : 0.55;

            const minRadius = wrapperRect.width * radiusMinRatio;
            const maxRadius = wrapperRect.width * radiusMaxRatio;

            // 依目前卡片寬度推 chord 所需半徑
            let cardWidth = wrapperRect.width * this.cardWidthRatio;
            let minRadiusByChord = (cardWidth * spacingFactor) / (2 * Math.sin(stepRad / 2));

            // 先用 chord 推半徑，但一定被 cap 在 [minRadius, maxRadius]
            let radius = clamp(minRadiusByChord, minRadius, maxRadius);

            // ✅ 如果「理論所需半徑」比 maxRadius 還大：代表空間不夠，必須縮卡片寬度
            if (minRadiusByChord > maxRadius) {
                // 在既定 radius 下，最多允許的 cardWidth（避免互撞）
                const fitCardWidth = (2 * radius * Math.sin(stepRad / 2)) / spacingFactor;

                // 反推成百分比（以 wrapper 寬度 + scaleMax 為基準）
                const targetPercent = (fitCardWidth / (wrapperRect.width * opts.scaleMax)) * 100;

                const minCardWidthPercent = (typeof opts.minCardWidthPercent === 'number')
                    ? opts.minCardWidthPercent
                    : 14; // 你可調：過小會看不清楚，但可避免重疊

                const nextPercent = clamp(targetPercent, minCardWidthPercent, this.cardWidthPercent);

                // 套用縮小後的卡片寬度（等比縮圖，不靠硬縮半徑）
                if (Math.abs(nextPercent - this.cardWidthPercent) > 0.2) {
                    this.cardWidthPercent = nextPercent;
                    this.cardWidthRatio = (this.cardWidthPercent / 100) * opts.scaleMax;
                    this.cards.forEach(c => c.style.width = this.cardWidthPercent + '%');

                    // 更新 cardWidth（後面 ring 位置計算需要）
                    cardWidth = wrapperRect.width * this.cardWidthRatio;
                    minRadiusByChord = (cardWidth * spacingFactor) / (2 * Math.sin(stepRad / 2));
                    // radius 仍維持 cap 後值即可（我們的目標就是不突破 maxRadius）
                }
            }

            // 依張數動態調整整個圓環垂直位置
            let ringYOffset = 0;
            const baseCardsForY = 8;
            const midCardsForY = 16;
            const maxCardsForY = 32;

            if (total > baseCardsForY) {
                const h = wrapperRect.height;
                if (total <= midCardsForY) {
                    const t1 = (total - baseCardsForY) / (midCardsForY - baseCardsForY);
                    ringYOffset = t1 * (h * 0.25);
                } else {
                    const clampedTotal = Math.min(total, maxCardsForY);
                    const t2 = (clampedTotal - midCardsForY) / (maxCardsForY - midCardsForY);
                    ringYOffset = (h * 0.25) + t2 * ((h * 0.40) - (h * 0.25));
                }
            }

            this.cards.forEach((card, i) => {
                const tiltX = opts.tiltX || 0;
                const rawAngle = (i - this.indexFloat) * stepDeg;

                const normalized = ((rawAngle + 180) % 360) - 180;
                const rad = rawAngle * Math.PI / 180;

                const x = Math.sin(rad) * radius;
                const z = Math.cos(rad) * radius - radius;

                const dist = Math.min(Math.abs(normalized) / 180, 1);
                const scale = opts.scaleMax - (opts.scaleMax - opts.scaleMin) * dist;
                const opacity = 1 - dist * 0.60;

                const extraTilt = (i === activeIndex && tiltX !== 0) ? -tiltX : 0;

                card.style.transform =
                    `translate(-50%, -50%) translate3d(${x}px, ${ringYOffset}px, ${z}px) rotateY(${rawAngle}deg) rotateX(${extraTilt}deg) scale(${scale})`;
                card.style.opacity = opacity;
                card.style.zIndex = String(1000 - Math.round(dist * 200));
                card.classList.toggle('is-front', i === activeIndex);
            });

            this._updateToolbar();
            this.stageEl.style.transform = `rotateX(${opts.tiltX}deg)`;
        }

        _updateToolbar() {
            if (!this.indicatorEl) return;

            const i18n = this.opts.i18n || DEFAULT_I18N;

            // Grid：顯示總張數
            if (this.wrapperEl.classList.contains('mode-grid')) {
                const total = this.allCards.length;

                // 支援使用者自訂 i18n.gridIndicator，沒有就 fallback DEFAULT_I18N.gridIndicator
                const txt = (typeof i18n.gridIndicator === 'function')
                    ? i18n.gridIndicator(total)
                    : `共 ${total} 張`; // 最後保底（通常不會走到，除非你沒加 DEFAULT_I18N）

                this.indicatorEl.textContent = txt;
                return;
            }

            // Carousel：沿用你原本 indicator(i, total) 的 i18n
            const total = this.total;
            const idx1 = Math.round(normalizeIndex(this.indexFloat, total)) + 1;

            const txt = (typeof i18n.indicator === 'function')
                ? i18n.indicator(idx1, total)
                : DEFAULT_I18N.indicator(idx1, total);

            this.indicatorEl.textContent = txt;
            if (this.btnMode) this.btnMode.disabled = !!this.forceGridMode;
        }

        _startAuto(opt) {
            if (this.forceGridMode) return;
            if (this.userPaused || this.displayMode !== 'carousel') return;

            const o = opt || {};
            const delay = (typeof o.delay === 'number' && o.delay > 0) ? o.delay : 0;

            // 只在「初次」或 force 時做延遲；其他情況（例如使用者點上一張/下一張）維持立即開始
            const useDelay = (o.force === true) || (!this._autoStartedOnce && Math.round(normalizeIndex(this.indexFloat, this.total)) === 0);
            const delayMs = useDelay ? delay : 0;

            this._stopAuto();

            if (delayMs > 0) {
                this.autoStartTimer = setTimeout(() => {
                    // timeout 期間可能被切到 grid / 暫停 / destroy
                    if (this.forceGridMode) return;
                    if (this.userPaused || this.displayMode !== 'carousel') return;

                    this.autoTimer = setInterval(() => {
                        this.indexFloat = normalizeIndex(this.indexFloat + 1, this.total);
                        this._layout();
                    }, this.opts.autoPlayInterval);

                    this._autoStartedOnce = true;
                }, delayMs);
                return;
            }

            this.autoTimer = setInterval(() => {
                this.indexFloat = normalizeIndex(this.indexFloat + 1, this.total);
                this._layout();
            }, this.opts.autoPlayInterval);

            this._autoStartedOnce = true;
        }

        _stopAuto() {
            if (this.autoStartTimer) clearTimeout(this.autoStartTimer);
            this.autoStartTimer = null;

            if (this.autoTimer) clearInterval(this.autoTimer);
            this.autoTimer = null;
        }

        _stopInertia() {
            if (this.inertiaId) cancelAnimationFrame(this.inertiaId);
            this.inertiaId = null;
            this.velocity = 0;
        }

        _startInertia() {
            if (Math.abs(this.velocity) < 0.00018) {
                this.indexFloat = normalizeIndex(Math.round(this.indexFloat), this.total);
                this._layout();
                this._startAuto({ delay: this.opts.autoPlayStartDelay, force: true });
                return;
            }
            let prev = performance.now();
            const frame = (now) => {
                const dt = now - prev;
                prev = now;

                this.indexFloat = this.indexFloat + this.velocity * dt;
                this._layout();

                this.velocity *= 0.94;
                if (Math.abs(this.velocity) < 0.00018) {
                    this.inertiaId = null;
                    this.indexFloat = normalizeIndex(Math.round(this.indexFloat), this.total);
                    this._layout();
                    this._startAuto();
                    return;
                }
                this.inertiaId = requestAnimationFrame(frame);
            };
            this.inertiaId = requestAnimationFrame(frame);
        }

        _syncGridColumnsByTotal() {
            this.stageEl.classList.remove('grid-row-1');

            if (this._rafGridRowMeasure) cancelAnimationFrame(this._rafGridRowMeasure);
            this._rafGridRowMeasure = requestAnimationFrame(() => {
                const tops = new Set();

                for (const c of this.allCards) {
                    if (!c.isConnected) continue;
                    const r = c.getBoundingClientRect();
                    if (r.width <= 0 || r.height <= 0) continue;
                    tops.add(Math.round(r.top)); // 容忍 1px 誤差
                }

                if (tops.size === 1) {
                    this.stageEl.classList.add('grid-row-1');
                }
            });
        }

        _switchToGrid(initial) {
            this.displayMode = 'grid';
            this._stopAuto();
            this._stopInertia();

            this.allCards.forEach(c => c.style.opacity = 0);

            setTimeout(() => {
                this.wrapperEl.classList.add('mode-grid');
                if (this.holderEl) this.holderEl.classList.add('grid-mode');
                this._syncGridColumnsByTotal();
                this.allCards.forEach(c => {
                    c.style.opacity = '';
                    c.style.transform = '';
                    c.style.zIndex = '';
                });

                this._updateToolbar();
                if (this.btnMode) this.btnMode.textContent = this.opts.i18n.modeGrid;
            }, initial ? 0 : 180);
        }

        _switchToCarousel() {
            this.displayMode = 'carousel';
            this.wrapperEl.classList.remove('mode-grid');
            if (this.holderEl) this.holderEl.classList.remove('grid-mode');

            if (this.carouselControlsEl) this.carouselControlsEl.style.display = '';

            setTimeout(() => {
                this.indexFloat = normalizeIndex(Math.round(this.indexFloat), this.total);
                this._layout();
                if (this.btnMode) this.btnMode.textContent = this.opts.i18n.modeCarousel;
                if (!this.userPaused) this._startAuto();
            }, 80);
        }

        static mount(stageElOrSelector, options) {
            const inst = new Gallery3D(stageElOrSelector, options);
            return inst.mount();
        }
    }

    global.Gallery3D = Gallery3D;
})(window);