        async () => {
            const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
            const visible = el => el && el.getClientRects().length > 0 && getComputedStyle(el).visibility !== 'hidden'
                && !el.closest('template,[hidden],[aria-hidden="true"],.templatecontent');
            const results = [];
            const $ = window.jQuery;
            const scan = selector => [...document.querySelectorAll(selector)].filter(visible);
            async function check(title, elements, operation) {
                if (!elements.length) return;
                const row = {Title:title, Checked:0, Failed:0, Pending:Math.max(0,elements.length-10)};
                for (const element of elements.slice(0,10)) {
                    try {
                        const outcome = await operation(element);
                        if (outcome === null) row.Pending++;
                        else { row.Checked++; if (!outcome) row.Failed++; }
                    } catch (_) { row.Failed++; }
                }
                results.push(row);
            }
            // Wait only for these known instances; ongoing animations do not block readiness.
            for (let i=0; i<10; i++) {
                const waiting = scan('.swiper:not(.marqueeSwiper)').some(el=>!el.swiper)
                    || ($ && scan('.type_change_frame').some(el=>!$(el).data('isInit')));
                if (!waiting) break;
                await pause(250);
            }
            await check('輪播切換', scan('.swiper:not(.marqueeSwiper)'), async element => {
                const swiper = element.swiper;
                if (!swiper || swiper.destroyed || swiper.slides.length < 2 || swiper.isLocked) return null;
                const before = swiper.realIndex, active = swiper.activeIndex;
                try {
                    if(swiper.isEnd && !swiper.params.loop) swiper.slidePrev(0); else swiper.slideNext(0); await pause(100);
                    return swiper.realIndex !== before && swiper.activeIndex !== active;
                } finally {
                    if (swiper.params.loop) swiper.slideToLoop(before,0); else swiper.slideTo(active,0);
                }
            });
            await check('列表版型切換', scan('.type_change_frame'), async element => {
                if (!$ || !$(element).data('isInit')) return null;
                const buttons = [...element.querySelectorAll('.btn_grid,.btn_list,.btn_text,.btn_prod_grid,.btn_prod_list')].filter(visible);
                const content = element.querySelector('.content');
                if (buttons.length < 2 || !content) return null;
                const original = buttons.find(button=>$(button).data('activate'));
                const originalChildren = [...content.children];
                try {
                    $(buttons[0]).triggerHandler('click'); const first = content.className;
                    $(buttons[1]).triggerHandler('click'); const second = content.className;
                    return first !== second && $(buttons[1]).data('activate') === 1
                        && originalChildren.length === content.children.length
                        && originalChildren.every((child,index)=>content.children[index]===child);
                } finally { if (original) $(original).triggerHandler('click'); }
            });
            const navs = [...new Set(scan('.filter-btn').map(el=>el.closest('.nav-container')||el.parentElement))];
            await check('分類篩選', navs, async nav => {
                if (!$ || !$(nav).data('categoryFilterInited')) return null;
                const links = [...nav.querySelectorAll('.filter-btn')];
                const key = link => (link.getAttribute('data-filter') || (link.getAttribute('href')||'').split('#')[1] || '').trim().toLowerCase();
                const keys = [...new Set(links.map(key).filter(value=>value && value!=='all'))];
                if (!keys.length || keys.some(value=>!/^[a-z0-9_-]+$/i.test(value))) return null;
                const selector = keys.map(value=>'.cat-'+value).join(',');
                let scope = nav.parentElement;
                const explicit = nav.getAttribute('data-filter-target');
                let cards;
                if (explicit) cards = [...document.querySelectorAll(explicit)].flatMap(el=>[...el.querySelectorAll(selector)]);
                else { while(scope && !scope.querySelector(selector)) scope=scope.parentElement; cards=scope?[...scope.querySelectorAll(selector)]:[]; }
                const button=links.find(link=>key(link)!=='all' && key(link));
                if (!button || !cards.length) return null;
                const oldUrl=location.href, selected=key(button);
                try {
                    history.replaceState(history.state,'','#'+selected);
                    window.dispatchEvent(new HashChangeEvent('hashchange'));
                    return button.classList.contains('active') && cards.every(card => {
                        const matches=card.classList.contains('cat-'+selected);
                        return card.classList.contains(matches?'show':'hide') && !card.classList.contains(matches?'hide':'show');
                    });
                } finally { history.replaceState(history.state,'',oldUrl); window.dispatchEvent(new HashChangeEvent('hashchange')); }
            });
            return {Evidence:results};
        }
