async input => {
    'use strict';
    const $ = window.jQuery, probe = window.__cokerProbe;
    const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
    const visible = el => el && el.getClientRects().length > 0 && getComputedStyle(el).visibility !== 'hidden'
        && !el.closest('template,[hidden],[aria-hidden="true"],.templatecontent');
    const scan = selector => [...document.querySelectorAll(selector)].filter(visible);
    const value = (obj, name) => obj?.[name] ?? obj?.[name[0].toUpperCase() + name.slice(1)];
    const requests = path => (probe?.requests || []).filter(item => item.path.replace(/\/$/,'').toLowerCase() === path.replace(/\/$/,'').toLowerCase());
    const last = array => array[array.length - 1];
    class Assertion extends Error { }
    const require = (condition, message) => { if (!condition) throw new Assertion(message); };
    class Restricted extends Error { }
    async function wait(test, message, timeout = 5000) {
        const until = performance.now() + timeout;
        do { const result = test(); if (result) return result; await pause(80); } while (performance.now() < until);
        throw new Restricted(message);
    }
    function click(element) {
        require(element, '缺少操作控制。');
        // Suppress a link's default navigation, preserving its actual bound handlers.
        const prevent = event => event.preventDefault();
        if(element.matches('a[href],button[type="submit"],input[type="submit"]'))element.addEventListener('click', prevent, { once: true });
        element.click();
    }
    async function response(path, after = 0, predicate = () => true) {
        const entry = await wait(() => last(requests(path).filter(item => item.started >= after && predicate(item.input) && !item.pending)), '資料請求未在期限內完成。');
        if (entry.status === 0 || [401,403,429].includes(entry.status)) throw new Restricted('請求受權限、CSP 或安全限制，未完成驗證。');
        require(entry.status >= 200 && entry.status < 300 && entry.data !== null, '資料回應錯誤或無法解析。');
        require(value(entry.data,'success') !== false, '資料回應表示操作失敗。');
        return entry.data;
    }
    function sameIds(a, b) { return JSON.stringify((a||[]).map(String).sort()) === JSON.stringify((b||[]).map(String).sort()); }
    function restoreData(element,key,value) { if(value===undefined) $(element).removeData(key); else $(element).data(key,value); }
    const metric = { Title: input.Title || '', Checked: 0, Failed: 0, Pending: 0, Reason: '' };
    function step(condition, message) { require(condition, message); metric.Checked++; }
    if (input.Phase === 'detect') {
        for (let i=0; i<25 && probe?.requests.some(request=>request.pending); i++) await pause(100);
        await pause(100);
        const definitions = input.Definitions.filter(item => {
            if (item.Id === 'remoteTracking') return !!window.RemoteTrackingToken;
            if (item.Id === 'mobileNavbarMove') return !!document.querySelector('#layout4 #NavbarContent') && !!document.querySelector('#lanBar');
            if (item.Id === 'sideFloating') return !!document.querySelector('#Floating_Center');
            return scan(item.Selector).length > 0;
        });
        probe.instances ||= new WeakMap(); probe.nextInstance ||= 0;
        return definitions.map(item=>{
            const elements=item.Id==='remoteTracking'?[document.body]:item.Id==='mobileNavbarMove'?[document.getElementById('NavbarContent')]:scan(item.Selector);
            const signature=elements.map(element=>{
                if(!element)return 'missing';
                if(!probe.instances.has(element))probe.instances.set(element,++probe.nextInstance);
                return probe.instances.get(element);
            }).join(',');
            return {Id:item.Id,Signature:signature};
        });
    }
    const tests = {};
    tests.directoryGetData = async frame => {
        if (!$) throw new Restricted('頁面程式未就緒。');
        if (frame.matches('.menu_directory')) {
            const ids=String(frame.dataset.dirid||'').split(',').filter(Boolean);
            const data=await response('/api/Directory/GetReleMenu',0,body=>sameIds(value(body,'ids'),ids));
            const links=[...frame.querySelectorAll('.accordion a[href]')];
            const flatten = node => [node,...(value(node,'children')||[]).flatMap(flatten)];
            const items=flatten(data).filter(item=>value(item,'linkUrl')||value(item,'routerName'));
            step(items.length===0 || links.length>0,'目錄選單資料未產生連結。');
            step(links.every(link=>link.textContent.trim() && !link.href.includes('undefined')),'目錄選單連結或文字錯誤。');
            const control=frame.querySelector('[data-bs-toggle="collapse"]');
            if(control){const selector=control.getAttribute('data-bs-target')||control.getAttribute('href');const panel=selector?.startsWith('#')?document.getElementById(selector.slice(1)):null;
                if(panel){const before=panel.classList.contains('show');click(control);await wait(()=>panel.classList.contains('show')!==before,'選單收合未完成。');metric.Checked++;click(control);}}
            return;
        }
        if(frame.matches('.advertise_directory')){
            const data=await wait(()=>last((probe?.requests||[]).filter(r=>/\/Directory\/GetReleAd(?:Batch)?$/i.test(r.path)&&!r.pending)), '廣告目錄資料未完成。');
            if(data.status===0)throw new Restricted('廣告資料受安全限制。');
            step(data.status===200 && data.data!==null,'廣告目錄回應錯誤。');
            step(!frame.querySelector('.coker-ad-loading'),'廣告載入畫面未結束。');
            const rendered=frame.querySelectorAll('a[href],img');
            if(Array.isArray(data.data)&&data.data.length===0)metric.Checked++;
            else step(rendered.length>0,'有廣告資料但沒有呈現內容。');
            return;
        }
        const ids=String(frame.dataset.dirid||'').split(',').filter(Boolean);
        const matches=body=>sameIds(value(body,'ids'),ids);
        const initial=await response('/api/Directory/GetReleInfo',0,matches);
        async function assertRender(data){
            const items=value(data,'releInfos');
            step(Array.isArray(items)&&Number.isFinite(Number(value(data,'totalCount'))),'目錄資料格式錯誤。');
            await wait(()=>!frame.querySelector('.directory-loading-item'),'目錄仍在載入。');
            const catalog=frame.querySelector('.catalog');require(catalog,'缺少目錄內容區。');
            const text=catalog.textContent||'';
            const cards=[...catalog.querySelectorAll(':scope > [data-id]')];
            step(cards.map(card=>String(card.dataset.id)).join(',')===items.map(item=>String(value(item,'id'))).join(','),'目錄畫面資料順序與回應不一致。');
            step(items.every(item=>!value(item,'title')||text.includes(String(value(item,'title')))), '目錄資料與畫面標題不一致。');
            step([...catalog.querySelectorAll('a[href]')].every(a=>!a.href.includes('undefined')&&!a.href.includes('/null')), '目錄明細連結格式錯誤。');
            if(items.length===0)step(!catalog.querySelector('a[href]:not([href="#"])'),'空資料仍保留舊商品連結。');
        }
        await assertRender(initial);
        const previous={sort:$(frame).data('searchSortBy'),direction:$(frame).data('searchSortDirection'),filter:$(frame).data('filtered'),page:$(frame).data('page'),facet:frame.getAttribute('data-facet')};
        const originalUrl=location.href;
        try{
            const control=frame.querySelector('.directory-sort-control [data-sort]:not([data-sort="default"])');
            if(control){const at=performance.now();click(control);const data=await response('/api/Directory/GetReleInfo',at,matches);await assertRender(data);
                step($(frame).data('searchSortBy')===control.dataset.sort,'排序操作未更新查詢條件。');
                const direction=frame.querySelector('.directory-sort-direction');if(direction){const start=performance.now();click(direction);const reversed=await response('/api/Directory/GetReleInfo',start,matches);await assertRender(reversed);
                    const field=control.dataset.sort==='price'?'price':null;
                    if(field){const numbers=(value(reversed,'releInfos')||[]).map(item=>Number(String(value(item,field)||'').replace(/[^0-9.+-]/g,''))).filter(Number.isFinite);if(numbers.length>1)step(numbers.every((number,index)=>index===0||numbers[index-1]>=number),'降序資料未依價格排序。');}
                    step($(frame).data('searchSortDirection')==='desc','排序方向未切換。');}}
            const filter=frame.querySelector('[data-role="facet-items"] [data-facet-value]:not(.active)') || (frame===document.querySelector('.catalog_frame')?document.querySelector('[data-role="facet-items"] [data-facet-value]:not(.active)'):null);
            if(filter){const oldFacet=frame.getAttribute('data-facet'),start=performance.now();click(filter);const data=await response('/api/Directory/GetReleInfo',start,matches);await assertRender(data);
                step(frame.getAttribute('data-facet')!==oldFacet&&filter.classList.contains('active'),'過濾控制未更新條件。');
                const years=String(frame.getAttribute('data-facet')||'').split(',').map(Number);
                if(years.length && years.every(year=>year>=1900&&year<=2200))step((value(data,'releInfos')||[]).every(item=>years.includes(new Date(value(item,'nodeDate')).getFullYear())),'篩選結果含範圍外年份。');}
            if(Number(value(initial,'totalPage'))>1){
                const next=frame.querySelector('.btn_next:not(.d-none)');
                if(!next)throw new Assertion('多頁資料沒有下一頁控制。');
                const at=performance.now();click(next);const data=await response('/api/Directory/GetReleInfo',at,body=>matches(body)&&Number(value(body,'page'))===2);await assertRender(data);metric.Checked++;
            }
        }finally{
            for(const [key,val] of [['searchSortBy',previous.sort],['searchSortDirection',previous.direction],['filtered',previous.filter],['page',previous.page]])restoreData(frame,key,val);
            if(previous.facet===null)frame.removeAttribute('data-facet');else frame.setAttribute('data-facet',previous.facet);
            history.replaceState(history.state,'',originalUrl);
            if(typeof window.initElemntAndLoadDir==='function')window.initElemntAndLoadDir($(frame),Number(previous.page)||1);
        }
    };
    tests.marqueeSwiper=async element=>{
        const data=await response('/api/Marquee/GetAll');step(Array.isArray(data),'跑馬燈資料格式錯誤。');
        await wait(()=>element.swiper,'跑馬燈尚未初始化。');
        step(data.every(item=>!value(item,'title')||element.textContent.includes(value(item,'title'))),'跑馬燈內容與回應不符。');
        const swiper=element.swiper;if(swiper.slides.length>1&&!swiper.isLocked){const before=swiper.activeIndex;if(swiper.isEnd&&!swiper.params.loop)swiper.slidePrev(0);else swiper.slideNext(0);await pause(100);step(swiper.activeIndex!==before,'跑馬燈未切換。');swiper.slideTo(before,0);}
    };
    tests.hoverMask=async element=>{
        const child=element.querySelector('.hover_protrude_scale');if(!child)throw new Restricted('目前版型沒有可驗證的滑入控制。');
        if(!$)throw new Restricted('頁面程式未就緒。');
        const siblings=[...child.parentElement.children].filter(el=>el!==child);if(!siblings.length)throw new Restricted('缺少可比對的相鄰內容。');
        try{$(child).triggerHandler('mouseenter');step(siblings.every(el=>el.classList.contains('opacity-50')),'滑入時相鄰內容未變化。');
            $(child).triggerHandler('mouseleave');step(siblings.every(el=>!el.classList.contains('opacity-50')),'離開後效果未還原。');}
        finally{$(child).triggerHandler('mouseleave');}
    };
    tests.linkWithIcon=async element=>{
        const href=element.getAttribute('href')||'';require(href,'檔案缺少來源。');
        const extension=(element.dataset.extension||href.split('?')[0].split('#')[0].split('.').pop()||'').toLowerCase();
        const groups={'fa-file-image':['jpg','jpeg','png','gif','avif'],'fa-file-pdf':['pdf'],'fa-file-word':['doc','docx','odt'],
            'fa-file-powerpoint':['ppt','pptx','odp'],'fa-file-excel':['xls','xlsx','ods'],'fa-file-zipper':['zip','rar']};
        const expected=Object.keys(groups).find(key=>groups[key].includes(extension))||'fa-file';
        step(!!element.querySelector('.icon .'+expected),'檔案類型與圖示不符。');
        step(extension==='pdf'?element.target==='_blank'&&element.rel.includes('noopener'):!!element.getAttribute('download'),'檔案開啟或下載設定錯誤。');
        if(typeof window.LinkWithIconInit!=='function')throw new Restricted('檔案處理函式尚未就緒。');
        const before=element.querySelectorAll('.icon i').length;window.LinkWithIconInit();step(element.querySelectorAll('.icon i').length===before,'重複初始化產生重複圖示。');
    };
    tests.anchorPoint=async element=>{
        if(!element.matches('.anchor_directory'))return;
        const titles=scan('.anchor_title'),links=[...element.querySelectorAll('a[href^="#"]')];
        step(titles.length>0&&titles.every(title=>title.id&&links.some(link=>link.hash==='#'+title.id)),'頁內導覽與標題不一致。');
        const link=links.find(link=>document.getElementById(link.hash.slice(1)));require(link,'頁內導覽沒有有效目標。');
        const before={x:scrollX,y:scrollY};const target=document.getElementById(link.hash.slice(1));
        try{click(link);await pause(100);const nav=document.querySelector('nav.position-fixed');const offset=50+(nav?(nav.getBoundingClientRect().height-10):0);
            const expected=Math.max(0,Math.min(document.documentElement.scrollHeight-innerHeight,target.getBoundingClientRect().top+scrollY-offset));
            step(Math.abs(scrollY-expected)<8,'頁內導覽沒有定位至預期位置。');}finally{scrollTo(before.x,before.y);}
    };
    tests.shareBlock=async element=>{
        await wait(()=>element.querySelector('a[href],button'),'分享控制尚未產生。');
        const links=[...element.querySelectorAll('a[href]')].filter(a=>/facebook|line\.me|twitter|plurk|mailto:/i.test(a.href));
        require(links.length>0,'分享選單沒有有效分享連結。');
        const expected=new URL(element.dataset.href||location.pathname,location.origin).href;
        const containsTarget=href=>{try{let decoded=href;for(let i=0;i<3;i++)decoded=decodeURIComponent(decoded);return decoded.includes(expected);}catch(_){return false;}};
        step(links.every(a=>containsTarget(a.href)),'分享網址未帶入目前頁面或指定網址。');
        const button=links.find(a=>!a.href.startsWith('mailto:'));if(button){const before=probe.opened.length;click(button);await pause(100);
            step(probe.opened.length>before&&containsTarget(last(probe.opened)),'分享操作未產生正確分享目的地。');metric.Reason='已核對分享資料與實際產生的目的地，外部視窗在檢測環境攔截，未向社群發文或寄信。';}
    };
    tests.flipTimer=async element=>{
        const end=new Date(element.dataset.timer).getTime();step(Number.isFinite(end),'倒數時間設定無效。');
        await wait(()=>element.dataset.initialized==='1'&&element.querySelector('.rotor'),'倒數元件尚未完成載入。');
        const before=[...element.querySelectorAll('.rotor-top')].map(el=>el.textContent).join('');await pause(1200);
        const after=[...element.querySelectorAll('.rotor-top')].map(el=>el.textContent).join('');
        step(end<=Date.now()?/^0+$/.test(after.replace(/\s/g,'')):before!==after,'倒數沒有更新或到期未歸零。');
    };
    tests.articleTags=async element=>{
        const data=await response('/api/Tags/GetArticleDataAll/');step(Array.isArray(data),'文章標籤資料格式錯誤。');
        await wait(()=>data.length===0||element.querySelectorAll('a').length>=data.length,'文章標籤未呈現。');
        const links=[...element.querySelectorAll('a')];step(data.every(item=>links.some(a=>a.textContent.trim()===value(item,'title'))),'文章標籤文字不一致。');
        step(links.every(a=>a.pathname.includes('/Search/Get/')&&a.rel.includes('noopener')),'標籤搜尋連結或開啟方式錯誤。');
    };
    tests.contactForm=async container=>{
        const form=container.matches('form')?container:container.querySelector('form');require(form,'表單元件缺少表單。');
        await wait(()=>window.Coker?.DynamicForm?.validation?.validate&&$(form).data('dynamic-form-inited'),'表單尚未完成初始化。');
        const fields=[...form.querySelectorAll('input:not([type="hidden"]),textarea,select')].filter(visible);
        const ids=fields.map(el=>el.id).filter(Boolean);step(ids.length===new Set(ids).size,'表單欄位識別重複。');
        const required=fields.find(el=>el.required&&el.type!=='checkbox'&&el.type!=='radio');
        if(required){const old=required.value;try{required.value='';step(!required.checkValidity(),'必填欄位允許空白。');
            if(required.type==='email'){required.value='invalid-email';step(!required.checkValidity(),'郵件欄位未拒絕無效格式。');}}
            finally{required.value=old;}}
        const section=form.querySelector('.checkbox_father_config[data-group-required="true"],.checkbox_father_config:has(.title.required)');
        if(section){const boxes=[...section.querySelectorAll('input[type="checkbox"]')].filter(visible);if(boxes.length>1){const old=boxes.map(box=>box.checked);
            try{boxes.forEach(box=>box.checked=false);step(!Coker.DynamicForm.validation.validate(form,false),'必選群組接受未勾選。');
                boxes[0].checked=true;Coker.DynamicForm.validation.validate(form,false);step(boxes[0].validity.valid,'已勾選群組仍無效。');}
            finally{boxes.forEach((box,index)=>box.checked=old[index]);Coker.DynamicForm.validation.validate(form,false);}}}
        const conditional=form.querySelector('select.filterCheck');
        if(conditional&&conditional.options.length>1){const old=conditional.value;
            try{for(const option of [...conditional.options].slice(0,2)){conditional.value=option.value;conditional.dispatchEvent(new Event('change',{bubbles:true}));
                const active=(option.dataset.filterid||'').split(',').map(value=>value.trim()).filter(Boolean);
                step([...form.querySelectorAll('[data-show-on]')].every(el=>{
                    const show=(el.dataset.showOn||'').split(',').some(value=>active.includes(value.trim()));
                    return el.classList.contains('d-none')!==show && (show||![...el.querySelectorAll('input,select,textarea')].some(field=>field.required));
                }),'條件欄位顯示或必填規則錯誤。');}}
            finally{conditional.value=old;conditional.dispatchEvent(new Event('change',{bubbles:true}));}}
        const captcha=form.querySelector('[name="captcha"]'),image=captcha?[...form.querySelectorAll('img.img-fluid')].pop():null;
        if(image){await wait(()=>image.complete,'驗證碼未載入。');step(image.naturalWidth>0,'驗證碼圖片無效。');const refresh=form.querySelector('.btn_refresh');if(refresh){const before=image.src;captcha.value='invalid';click(refresh);await wait(()=>image.src!==before&&image.complete,'驗證碼更新未完成。');step(!captcha.value&&image.naturalWidth>0,'驗證碼更新未清除輸入。');}metric.Reason='已驗證欄位規則與驗證碼更新；驗證碼圖片由隔離環境供應，未提交表單或驗證真實驗證碼服務。';}
        if(!required&&!section)step(fields.length>0,'表單沒有可用欄位。');
    };
    tests.remoteTracking=async()=>{
        const entry=await wait(()=>last(requests('/api/UserStatistic/Collect').filter(item=>item.input)), '尚未觀察到追蹤觸發。',4500);
        const data=entry.input;step(typeof value(data,'eventId')==='string'&&value(data,'eventId').length>0,'追蹤事件缺少識別。');
        step(typeof value(data,'hasInteraction')==='boolean'&&Number(value(data,'visibleSeconds'))>=2&&Number(value(data,'visibleSeconds'))<=300,'追蹤資料欄位或可見時間無效。');
        step(entry.status===0,'追蹤資料不應傳送到真實服務。');metric.Reason='已驗證實際建構的追蹤資料，請求在檢測環境攔截，未驗證伺服器收集結果。';
    };
    tests.bgCanvas=async element=>{
        const canvas=await wait(()=>element.matches('canvas')?element:element.querySelector('canvas'),'背景畫布尚未建立。');
        require(canvas.width>0&&canvas.height>0,'背景畫布尺寸無效。');
        const drawn=probe.canvasDraws.get(canvas)||0;await pause(250);
        step((probe.canvasDraws.get(canvas)||0)>drawn,'背景動畫沒有持續繪製。');
        const original=element.style.width,before=canvas.width;
        try{element.style.width=Math.max(40,element.getBoundingClientRect().width*.7)+'px';dispatchEvent(new Event('resize'));await pause(180);
            step(canvas.width!==before&&canvas.width>0,'背景畫布沒有回應尺寸變更。');const context=canvas.getContext('2d');step(!!context,'無法繪製背景畫布。');}
        finally{element.style.width=original;dispatchEvent(new Event('resize'));}
    };
    tests.flipBook=async trigger=>{
        const pdf=trigger.getAttribute('data-pdf-url');require(pdf,'電子書入口缺少 PDF 來源。');
        const target=trigger.getAttribute('data-bs-target')||'.FlipBookModal';const modal=document.querySelector(target);require(modal,'電子書缺少檢視器。');
        click(trigger);
        try{
            await wait(()=>modal.classList.contains('show')&&modal.querySelector('.FlipBook canvas'),'電子書首屏尚未完成或來源被限制。',12000);
            const total=Number(modal.querySelector('.FlipBookPageTotal')?.textContent),page=modal.querySelector('.FlipBookPageInput');
            step(total>=1&&Number(page?.value)>=1,'電子書頁數無效。');
            if(total>1){const before=Number(page.value);click(modal.querySelector('.FlipBookNext'));await wait(()=>Number(page.value)>before,'電子書翻頁未完成。');metric.Checked++;
                click(modal.querySelector('.FlipBookPrevious'));await wait(()=>Number(page.value)===before,'電子書無法回到前頁。');metric.Checked++;}
            const zoom=modal.querySelector('.FlipBookZoomStatus'),old=zoom.textContent;click(modal.querySelector('.FlipBookZoomIn'));
            await wait(()=>zoom.textContent!==old,'電子書縮放未更新。');metric.Checked++;click(modal.querySelector('.FlipBookZoomReset'));
            step(zoom.textContent==='100%','電子書縮放無法還原。');
            const toggle=modal.querySelector('.FlipBookSidebarToggle');click(toggle);step(!modal.querySelector('.FlipBookSidebar').hidden,'電子書頁面清單未展開。');
        }finally{const close=modal.querySelector('.FlipBookClose');if(close)click(close);await pause(350);}
        step(!modal.classList.contains('show'),'電子書未關閉。');
    };
    tests.mapMessage=async element=>{
        const controls=[...element.querySelectorAll('map>area,.mapData li')];require(controls.length,'互動圖區沒有操作入口。');
        for(const control of controls.slice(0,2)){const id=(control.getAttribute('href')||'#'+control.dataset.area).replace(/^#/,'');const target=document.getElementById(id);require(target,'互動圖區目標不存在。');
            if(!$)throw new Restricted('頁面程式未就緒。');$(control).trigger('mouseover');await pause(100);step(target.classList.contains('active'),'互動圖區沒有切換對應訊息。');}
    };
    tests.geolocation=async element=>{
        if(!$||typeof window.GetLatLng!=='function')throw new Restricted('定位處理程式未就緒。');
        const geo=navigator.geolocation;require(geo,'瀏覽器沒有定位介面。');const original=geo.getCurrentPosition;
        const old={lat:$(element).data('latitude'),lng:$(element).data('longitude')};let denied=false;
        try{
            geo.getCurrentPosition=success=>success({coords:{latitude:25.033,longitude:121.5654}});window.GetLatLng($(element));
            step($(element).data('latitude')===25.033&&$(element).data('longitude')===121.5654,'定位成功回呼未保存座標。');
            geo.getCurrentPosition=(_,failure)=>{denied=true;failure({code:1,PERMISSION_DENIED:1});};window.GetLatLng($(element));
            step(denied&&$(element).data('latitude')===25.033,'拒絕定位回呼破壞已知座標。');
        }finally{geo.getCurrentPosition=original;restoreData(element,'latitude',old.lat);restoreData(element,'longitude',old.lng);}
    };
    tests.mobileNavbarMove=async()=>{
        const navbar=document.querySelector('#layout4 #NavbarContent')||document.getElementById('NavbarContent'),bar=document.getElementById('lanBar');
        if(!navbar||!bar)throw new Restricted('目前版型沒有行動導覽。');require(innerWidth<992,'尚未切換行動視窗。');
        const parent=navbar.parentNode,next=navbar.nextSibling;
        try{if(!window.Coker?.page?.init)throw new Restricted('導覽初始化程式未就緒。');Coker.page.init(document,{only:['mobileNavbarMove'],force:true});
            step(bar.nextElementSibling===navbar,'行動導覽未移動至語言列後方。');
            const control=document.querySelector('[data-bs-target="#NavbarContent"]');if(control){const before=navbar.classList.contains('show');click(control);await wait(()=>navbar.classList.contains('show')!==before,'行動導覽未展開或收合。');metric.Checked++;click(control);}}
        finally{if(next?.parentNode===parent)parent.insertBefore(navbar,next);else parent.appendChild(navbar);}
    };
    tests.sideFloating=async root=>{
        if(!root.matches('#Floating_Center'))return;
        const y=scrollY;scrollTo(0,Math.min(450,document.documentElement.scrollHeight-innerHeight));dispatchEvent(new Event('scroll'));
        try{const controls=[...root.querySelectorAll('.side-floating-expand-trigger')];
            if(controls.length){const control=controls[0],panel=control.closest('.side-floating-item')?.querySelector('[data-side-floating-panel]');require(panel,'側邊工具沒有對應面板。');
                click(control);await wait(()=>control.getAttribute('aria-expanded')==='true'&&!panel.classList.contains('d-none'),'側邊面板未展開。');metric.Checked++;
                const close=panel.querySelector('.side-floating-close');if(close)click(close);else click(control);
                step(control.getAttribute('aria-expanded')==='false'&&panel.classList.contains('d-none'),'側邊面板未關閉。');}
            else {const top=document.querySelector('#btn_gotop');if(!top)throw new Restricted('目前側邊工具沒有可安全操作的控制。');click(top);await wait(()=>scrollY<5,'回到頁首未完成。');metric.Checked++;}}
        finally{scrollTo(0,y);}
    };
    tests.frameInit=async element=>{
        if(element.matches('.YTmodal_frame')){
            const target=element.getAttribute('data-bs-target')||element.getAttribute('data-target');
            if(!target?.startsWith('#'))throw new Restricted('此影片入口沒有可操作的視窗設定。');
            const modal=document.querySelector(target);require(modal,'影片視窗不存在。');click(element);
            try{await wait(()=>modal.classList.contains('show'),'影片視窗未開啟。');metric.Checked++;const frame=modal.querySelector('iframe,video');
                step(!!frame&&(frame.getAttribute('src')||frame.querySelector('source')?.src),'影片視窗未設定來源。');}
            finally{const close=modal.querySelector('[data-bs-dismiss="modal"]');if(close)click(close);}
            await wait(()=>!modal.classList.contains('show'),'影片視窗未關閉。');metric.Checked++;return;
        }
        const images=[...element.querySelectorAll('img')];await wait(()=>images.every(img=>img.complete),'排版圖片仍在載入。');
        const grid=element.querySelector('.grid')||element;
        const masonry=window.Masonry?.data?.(grid)||($ && $(grid).data('masonry'));if(!masonry)throw new Restricted('瀑布流排版實例尚未就緒。');
        const items=masonry.items.map(item=>item.element).filter(visible);if(!items.length)throw new Restricted('沒有可核對的排版內容。');
        const before=items.map(item=>item.getBoundingClientRect());masonry.layout();dispatchEvent(new Event('resize'));await pause(350);
        const rects=items.filter(item=>!item.classList.contains('masonry-sizer')).map(item=>item.getBoundingClientRect());
        step(rects.every((rect,index)=>rects.slice(index+1).every(other=>Math.min(rect.right,other.right)-Math.max(rect.left,other.left)<2||Math.min(rect.bottom,other.bottom)-Math.max(rect.top,other.top)<2)),'瀑布流內容互相重疊。');
        step(items.every(item=>item.getBoundingClientRect().width>0&&item.getBoundingClientRect().height>0),'圖片排版出現零尺寸內容。');
        step(before.every(rect=>Number.isFinite(rect.top)&&Number.isFinite(rect.left)),'圖片排版位置無效。');
    };
    tests.sitemap=async element=>{
        const data=await response('/api/WebMenu/GetMainMenu/');const maps=value(data,'maps');step(Array.isArray(maps),'網站地圖資料格式錯誤。');
        const flatten=items=>items.flatMap(item=>[item,...flatten(value(item,'children')||[])]);const nodes=flatten(maps);
        await wait(()=>nodes.length===0||element.querySelector('li'),'網站地圖尚未呈現。');
        step(nodes.every(node=>!value(node,'title')||element.textContent.includes(value(node,'title'))),'網站地圖缺少資料中的節點。');
        const links=[...element.querySelectorAll('a[href]')];step(links.every(link=>link.textContent.trim()&&!link.href.includes('undefined')),'網站地圖有無效連結或空白節點。');
        if(nodes.length===0)step(!links.length,'空網站地圖仍殘留舊連結。');
    };
    tests.noLink=async element=>{
        const before=location.href;let bubbled=false,cancelled=false;
        const observer=event=>{if(event.target===element){bubbled=true;cancelled=event.defaultPrevented;}};
        document.addEventListener('click',observer);
        try{element.dispatchEvent(new MouseEvent('click',{bubbles:true,cancelable:true}));step(bubbled&&cancelled&&location.href===before,'無連結控制未阻止跳轉或阻斷事件冒泡。');}
        finally{document.removeEventListener('click',observer);}
    };
    tests.search=async element=>{
        // Use the same catalog renderer with a deterministic no-match query. No
        // form is submitted and no member/session storage is reused.
        const frame=document.querySelector('.catalog_frame[data-type="search"]');
        if(!frame||!$||typeof window.initElemntAndLoadDir!=='function')throw new Restricted('目前只有搜尋入口；需在搜尋結果頁驗證資料流程。');
        const previous=$(frame).data('searchText'),keyword=input.Keyword||'CokerDiagnosticsNoMatch'+Date.now();
        const old=element.value;
        try{element.value=keyword;element.dispatchEvent(new Event('input',{bubbles:true}));
            $(frame).data('searchText',keyword).removeData('page');const start=performance.now();window.initElemntAndLoadDir($(frame),1);
            const data=await response('/api/Directory/GetReleInfo',start,body=>value(body,'searchText')===keyword);
            step(Array.isArray(value(data,'releInfos'))&&Number(value(data,'totalCount'))===0,'無結果搜尋回應不正確。');
            await wait(()=>!frame.querySelector('.directory-loading-item'),'搜尋結果未完成更新。');
            step(!frame.querySelector('.catalog a[href]:not([href="#"])'),'無結果搜尋仍顯示舊資料。');
            if(document.querySelector('.searchCount'))step(Number(document.querySelector('.searchCount').textContent)===0,'搜尋結果數未更新。');
        }finally{element.value=old;restoreData(frame,'searchText',previous);$(frame).removeData('page');window.initElemntAndLoadDir($(frame),1);}
    };
    tests.quickCartUi=async trigger=>{
        const modal=document.getElementById('ShoppingCarModal');require(modal,'快速購物車視窗不存在。');
        const start=performance.now();click(trigger);
        try{
            await wait(()=>modal.classList.contains('show'),'快速購物車未開啟。');metric.Checked++;
            const data=await response('/api/Product/GetMainDisplayOne',0);
            const expectedId=Number(trigger.getAttribute('data-quick-cart-product-id')||trigger.closest('.catalog > [data-id]')?.dataset.id||0);
            if(expectedId)step(last(requests('/api/Product/GetMainDisplayOne')).queryId===expectedId,'購買入口讀取了錯誤商品。');
            step(Array.isArray(value(data,'stocks'))&&value(data,'stocks').length>0,'隔離商品缺少規格資料。');
            await wait(()=>modal.querySelector('.name')?.textContent.includes(value(data,'title')),'快速購物車沒有呈現隔離商品。');metric.Checked++;
            const quantity=modal.querySelector('.input_pro_quantity');require(quantity,'快速購物車缺少數量欄位。');
            const initial=Number(quantity.value),stepSize=Number(quantity.step)||1,min=Number(quantity.min)||stepSize,max=Number(quantity.max);
            click(modal.querySelector('.btn_count_plus'));step(Number(quantity.value)===Math.min(initial+stepSize,max),'增加數量未依最小單位或庫存上限。');
            quantity.value=String(min);click(modal.querySelector('.btn_count_minus'));step(Number(quantity.value)===min,'減少數量低於最小購買量。');
            if(Number.isFinite(max)){quantity.value=String(max);click(modal.querySelector('.btn_count_plus'));step(Number(quantity.value)===max,'增加數量超過庫存上限。');}
            const choices=[...modal.querySelectorAll('.spec_control input[type="radio"]')];
            require(choices.length>1,'隔離商品的規格選項未呈現。');
            const candidate=choices.find(choice=>!choice.checked);require(candidate,'缺少第二個規格選項。');
            click(candidate);const selected=value(data,'stocks').find(stock=>Number(value(stock,'fK_S1id'))===Number(candidate.value));
            step(candidate.checked&&selected&&Number(quantity.min)===Number(value(selected,'min_Qty')),'規格選擇與購買單位沒有同步更新。');
            step(modal.querySelector('.btn_addToCar')?.disabled===false,'可購買的隔離商品被錯誤停用。');
            metric.Reason='已操作實際快速購物車的開關、規格與數量控制；商品資料由隔離服務供應，未送出真實加入購物車。';
        }finally{const close=modal.querySelector('[data-bs-dismiss="modal"]');if(close)click(close);await pause(350);}
        step(!modal.classList.contains('show'),'快速購物車沒有關閉。');
    };
    let elements=input.Id==='remoteTracking'?[document.body]:input.Id==='mobileNavbarMove'?[document.body]:scan(input.Selector);
    if(input.Id==='anchorPoint')elements=elements.filter(el=>el.matches('.anchor_directory'));
    if(input.Id==='sideFloating')elements=[document.getElementById('Floating_Center')].filter(Boolean);
    if(input.Id==='search')elements=elements.slice(0,1);
    const limit=input.Id==='flipBook'?3:10;
    metric.Pending+=Math.max(0,elements.length-limit);
    for(const element of elements.slice(0,limit)){
        try{if(!tests[input.Id])throw new Error('功能執行器缺失。');await tests[input.Id](element);}
        catch(error){if(error instanceof Restricted){metric.Pending++;if(!metric.Failed)metric.Reason=error.message;}else{metric.Failed++;metric.Reason=error instanceof Assertion?error.message:'操作時發生頁面程式錯誤。';}}
    }
    return metric;
}
