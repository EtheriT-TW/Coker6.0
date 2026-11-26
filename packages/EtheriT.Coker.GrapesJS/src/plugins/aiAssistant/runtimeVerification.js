const owners = boundary => boundary ? [boundary, ...boundary.querySelectorAll('[id]')]
    .filter(el => el.id && [...el.children].some(child => child.classList.contains('swiper'))).slice(0, 8) : [];

export function captureRuntimeContext(boundary) {
    return owners(boundary).map(owner => {
        const el = [...owner.children].find(child => child.classList.contains('swiper'));
        const instance = el.swiper;
        return { targetId: owner.id, classes: [...owner.classList], viewportWidth: el.ownerDocument.defaultView.innerWidth,
            slidesPerView: instance?.params?.slidesPerView ?? null, breakpoint: instance?.currentBreakpoint ?? null,
            breakpoints: Object.fromEntries(Object.entries(instance?.originalParams?.breakpoints || {}).slice(0, 24)
                .map(([key, value]) => [key, { slidesPerView: value?.slidesPerView }])) };
    });
}

export function verifyRuntimeAssertions(boundary, assertions, originalOwners) {
    if (!originalOwners.length) return [];
    if (!Array.isArray(assertions) || !assertions.length || assertions.length > 8)
        throw new Error('未取得輪播執行驗證條件，已還原；不能只以 CSS 變動判定完成。');
    if (originalOwners.some(owner => !assertions.some(assertion => assertion.targetId === owner.targetId)))
        throw new Error('輪播執行驗證未涵蓋目前區塊的初始化元件，已還原。');
    const currentOwners = owners(boundary);
    return assertions.map(assertion => {
        const value = String(assertion.value ?? '');
        const expected = value === 'auto' ? 'auto' : Number(value);
        const minWidth = assertion.minWidth ?? 0;
        if (!originalOwners.some(owner => owner.targetId === assertion.targetId) ||
            !Number.isInteger(minWidth) || minWidth < 0 || minWidth > 20000 ||
            (expected !== 'auto' && (!Number.isFinite(expected) || expected <= 0 || expected > 100)))
            throw new Error('輪播執行驗證條件不正確，已還原。');
        const owner = currentOwners.find(el => el.id === assertion.targetId);
        const el = owner && [...owner.children].find(child => child.classList.contains('swiper'));
        const instance = el?.swiper;
        if (!instance || instance.destroyed) throw new Error('輪播尚未完成初始化，無法確認結果，已還原。');
        const same = actual => String(actual) === String(expected);
        const width = instance.params?.breakpointsBase === 'container' ? el.clientWidth : el.ownerDocument.defaultView.innerWidth;
        if (minWidth > 0 && !same(instance.originalParams?.breakpoints?.[minWidth]?.slidesPerView))
            throw new Error(`輪播斷點 ${minWidth} 的設定不符合核定欄數 ${expected}，已還原。`);
        if (width >= minWidth && !same(instance.params?.slidesPerView))
            throw new Error(`輪播目標為 ${expected} 欄，實際仍為 ${instance.params?.slidesPerView} 欄，已還原。`);
        const active = Number(instance.params?.slidesPerView);
        if (Number.isFinite(active) && active > 0) {
            const size = Number(instance.size) || el.clientWidth;
            const rawGap = instance.params.spaceBetween ?? 0;
            const gap = String(rawGap).endsWith('%') ? Number.parseFloat(rawGap) * size / 100 : Number(rawGap);
            const expectedWidth = (size - (active - 1) * gap) / active;
            const slides = [...(instance.slides || [])].map(slide => slide.getBoundingClientRect().width).filter(value => value > 0).slice(0, 12);
            if (!slides.length || !(expectedWidth > 0) || slides.some(actual => Math.abs(actual - expectedWidth) > Math.max(3, expectedWidth * .1)))
                throw new Error('輪播參數與實際卡片寬度不一致，尚未達到核定效果，已還原。');
        }
        return { targetId: owner.id, expected, minWidth, viewportWidth: width,
            actual: instance.params.slidesPerView, breakpoint: instance.currentBreakpoint };
    });
}
