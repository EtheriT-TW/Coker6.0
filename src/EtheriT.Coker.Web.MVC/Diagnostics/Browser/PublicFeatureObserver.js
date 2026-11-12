(() => {
    'use strict';
    const state = { requests: [], errors: [], csp: [], opened: [], scans: 0 };
    Object.defineProperty(window, '__cokerProbe', { value: state, configurable: false });
    const path = value => { try { return new URL(value, location.href).pathname; } catch (_) { return ''; } };
    const parse = value => { try { return JSON.parse(value); } catch (_) { return null; } };
    function begin(url, method, body) {
        const pathname = path(url);
        if (!pathname.startsWith('/api/')) return null;
        if (state.requests.length >= 150) return null;
        const entry = { path: pathname, method: String(method || 'GET').toUpperCase(), input: typeof body === 'string' ? parse(body) : null,
            pending: true, status: 0, data: null, started: performance.now() };
        if (pathname.toLowerCase().replace(/\/$/,'') === '/api/product/getmaindisplayone') {
            const query = [...new URL(url, location.href).searchParams].find(pair=>pair[0].toLowerCase()==='id');
            entry.queryId = Number(query?.[1] || 0);
        }
        state.requests.push(entry);
        return entry;
    }
    function finish(entry, status, text) {
        if (!entry) return;
        entry.pending = false; entry.status = status;
        entry.data = typeof text === 'string' && text.length <= 1024 * 1024 ? parse(text) : null;
    }
    // Count real drawing work without reading pixels or changing the animation.
    state.canvasDraws = new WeakMap();
    for(const name of ['clearRect','fillRect','stroke','fill','drawImage']) {
        const native=CanvasRenderingContext2D.prototype[name];
        CanvasRenderingContext2D.prototype[name]=function() {
            state.canvasDraws.set(this.canvas,(state.canvasDraws.get(this.canvas)||0)+1);
            return native.apply(this,arguments);
        };
    }
    document.addEventListener('submit',event=>event.preventDefault(),true);
    const nativeFetch = window.fetch;
    window.fetch = async function (resource, options) {
        const entry = begin(typeof resource === 'string' ? resource : resource.url,
            options?.method || resource?.method, options?.body);
        try {
            const response = await nativeFetch.apply(this, arguments);
            if (entry) {
                // API JSON only; do not duplicate binary or arbitrarily large bodies.
                const length = Number(response.headers.get('content-length') || 0);
                if (length > 1024 * 1024) finish(entry, response.status, '');
                else response.clone().text().then(text => finish(entry, response.status, text), () => finish(entry, response.status, ''));
            }
            return response;
        } catch (error) { finish(entry, 0, ''); throw error; }
    };
    const open = XMLHttpRequest.prototype.open, send = XMLHttpRequest.prototype.send;
    XMLHttpRequest.prototype.open = function (method, url) {
        this.__cokerRequest = { method, url }; return open.apply(this, arguments);
    };
    XMLHttpRequest.prototype.send = function (body) {
        const info = this.__cokerRequest;
        const entry = info && begin(info.url, info.method, body);
        if (entry) this.addEventListener('loadend', () => {
            try { finish(entry, this.status, this.responseType === 'json' ? JSON.stringify(this.response) : this.responseText); }
            catch (_) { finish(entry, this.status, ''); }
        }, { once: true });
        return send.apply(this, arguments);
    };
    // Tracking payloads are observed, never transmitted during diagnostics.
    navigator.sendBeacon = function (url, body) {
        const entry = begin(url, 'POST', typeof body === 'string' ? body : null);
        if (body instanceof Blob) body.text().then(text => { if (entry) entry.input = parse(text); finish(entry, 0, ''); });
        else finish(entry, 0, '');
        return true;
    };
    window.open = function (url) { state.opened.push(String(url || '').slice(0, 4096)); return null; };
    document.addEventListener('click', event => {
        const link = event.target?.closest?.('a[href]');
        if (link && /facebook|line\.me|twitter|plurk|mailto:/i.test(link.href)) {
            state.opened.push(link.href.slice(0, 4096));
            event.preventDefault();
        }
    }, true);
    const logError = console.error;
    console.error = function (...args) {
        if (state.errors.length < 100) state.errors.push(args.filter(value => typeof value === 'string').join(' ').slice(0, 200));
        return logError.apply(console, args);
    };
    window.addEventListener('error', event => { if (state.errors.length < 100) state.errors.push(String(event.message || 'resource error').slice(0, 200)); });
    window.addEventListener('unhandledrejection', () => { if (state.errors.length < 100) state.errors.push('unhandled rejection'); });
    window.addEventListener('securitypolicyviolation', event => {
        if (event.disposition === 'enforce' && state.csp.length < 100)
            state.csp.push({ directive: event.effectiveDirective, path: path(event.blockedURI) });
    });
})();
