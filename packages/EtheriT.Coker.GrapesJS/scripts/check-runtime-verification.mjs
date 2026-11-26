// Isolated DOM-shaped fixture, no browser, server, model or database.
import assert from 'node:assert/strict';
import { captureRuntimeContext, verifyRuntimeAssertions } from '../src/plugins/aiAssistant/runtimeVerification.js';
const classes = values => Object.assign(values, { contains: value => values.includes(value) });
const view = { innerWidth: 1400 };
let slideWidth = 200;
const instance = { destroyed: false, currentBreakpoint: '1024', size: 1200,
    params: { slidesPerView: 6, spaceBetween: 0 },
    originalParams: { breakpoints: { 992: { slidesPerView: 4 }, 1024: { slidesPerView: 6 } } },
    slides: Array.from({ length: 6 }, () => ({ getBoundingClientRect: () => ({ width: slideWidth }) })) };
const viewport = { classList: classes(['swiper']), swiper: instance, ownerDocument: { defaultView: view }, clientWidth: 1200 };
const owner = { id: 'carousel', children: [viewport], classList: classes(['six_swiper']), querySelectorAll: () => [] };
const original = captureRuntimeContext(owner);
const desired = [{ targetId: 'carousel', value: '4', minWidth: 0 }];
assert.throws(() => verifyRuntimeAssertions(owner, [], original));
assert.throws(() => verifyRuntimeAssertions(owner, desired, original), /實際仍為 6/);
instance.params.slidesPerView = 4;
assert.throws(() => verifyRuntimeAssertions(owner, desired, original), /卡片寬度/);
slideWidth = 300;
assert.equal(verifyRuntimeAssertions(owner, desired, original)[0].actual, 4);
instance.params.slidesPerView = 6; slideWidth = 200;
assert.throws(() => verifyRuntimeAssertions(owner, [{ targetId: 'carousel', value: '4', minWidth: 992 }], original), /實際仍為 6/);
view.innerWidth = 500; instance.params.slidesPerView = 1; slideWidth = 1200;
assert.equal(verifyRuntimeAssertions(owner, [{ targetId: 'carousel', value: '4', minWidth: 992 }], original)[0].actual, 1);
assert.throws(() => verifyRuntimeAssertions(owner, [{ targetId: 'other', value: '4', minWidth: 0 }], original));
console.log('Isolated runtime postcondition checks passed; not browser/model/SQL verification.');
