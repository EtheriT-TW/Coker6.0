// Execution events, not model reasoning. Untrusted details are always plain text.
export function createProcess() {
    return { id: crypto.randomUUID(), started: Date.now(), ended: null, state: 'running', events: [] };
}

// Evict bulky request copies before responses and rejection reasons. This also
// repairs old records' presentation without pretending discarded text is recoverable.
export function limitProcessDetails(process, budget = 120000) {
    const entries = process.events || [];
    let excess = entries.reduce((sum, item) => sum + (item.detail?.length || 0), 0) - budget;
    const priority = item => item.stage === 'model-request' ? 0 : item.stage === 'model-response' ? 1 : 2;
    for (const entry of [...entries].sort((a, b) => priority(a) - priority(b))) {
        if (excess <= 0) break;
        const length = entry.detail?.length || 0;
        if (!length) continue;
        const remove = Math.min(excess, length);
        entry.detail = entry.detail.slice(0, length - remove);
        entry.clipped = true; excess -= remove;
    }
}

export function appendProcessEvent(process, event) {
    if (!process || process.state !== 'running') return;
    const elapsedMs = Math.max(0, Date.now() - process.started);
    const detail = typeof event?.detail === 'string' ? event.detail : '';
    const maxDetail = event?.stage === 'model-request' ? 20000 : 32000;
    const entry = {
        stage: String(event?.stage || '').slice(0, 60),
        message: String(event?.message || '處理中').slice(0, 1000),
        elapsedMs, detail: detail.slice(0, maxDetail),
        clipped: detail.length > maxDetail
    };
    if (entry.stage === 'model-response') {
        const request = [...process.events].reverse().find(item => item.stage === 'model-request');
        if (request) entry.durationMs = Math.max(0, elapsedMs - request.elapsedMs);
    }
    if (process.events.length < 60) process.events.push(entry);
    else { process.events[59] = entry; process.clipped = true; }
    limitProcessDetails(process);
}

export function finishProcess(process, state, message) {
    if (!process || process.state !== 'running') return;
    appendProcessEvent(process, { stage: state, message });
    process.state = state; process.ended = Date.now();
}

export function createProcessViewer(panel) {
    const dialog = document.createElement('dialog');
    dialog.className = 'coker-canvas-ai-settings coker-canvas-ai-process';
    dialog.setAttribute('aria-label', 'AI 處理流程');
    dialog.innerHTML = `<div class="coker-canvas-ai-header"><strong>處理流程</strong><button type="button" data-process-close aria-label="關閉處理流程">×</button></div>
        <p data-process-question></p><p data-process-state role="status"></p>
        <p class="coker-canvas-ai-note">顯示實際處理事件與提案，並非模型內部思考。詳細內容可能包含網站文字；僅保留於此瀏覽器。</p>
        <div data-process-events></div>`;
    panel.append(dialog);
    let selected = null, opener = null, timer = null, count = -1, tail = null;
    const events = dialog.querySelector('[data-process-events]');
    const labels = { running: '執行中', completed: '已完成', failed: '未完成', cancelled: '已取消', interrupted: '連線已中斷', clarification: '等待必要設定' };
    const seconds = value => `${(Math.max(0, value) / 1000).toFixed(1)} 秒`;
    function refresh() {
        const process = selected?.process;
        if (!dialog.open || !process) return;
        const last = process.events.at(-1);
        const elapsed = (process.ended || Date.now()) - process.started;
        dialog.querySelector('[data-process-state]').textContent = `${labels[process.state] || '紀錄'} · 總耗時 ${seconds(elapsed)}` +
            (process.state === 'running' && last ? ` · ${last.message}（已等待 ${seconds(elapsed - last.elapsedMs)}）` : '');
        if (count === process.events.length && tail === last) return;
        if (count > 0 && count === process.events.length) { events.lastElementChild?.remove(); count--; }
        // Append only new entries so expanded detail panels and scroll position survive updates.
        if (count < 0) { events.replaceChildren(); count = 0; }
        for (; count < process.events.length; count++) {
            const entry = process.events[count];
            const row = document.createElement('section');
            const title = document.createElement('p');
            title.textContent = `${seconds(entry.elapsedMs)} · ${entry.message}` +
                (Number.isFinite(entry.durationMs) ? `（本輪耗時 ${seconds(entry.durationMs)}）` : '');
            row.append(title);
            if (entry.detail || entry.clipped) {
                const detail = document.createElement('details');
                const summary = document.createElement('summary');
                summary.textContent = entry.stage === 'model-request' ? '查看本輪請求摘要／變更' :
                    ['validation', 'review'].includes(entry.stage) ? '查看驗證／修正要求' : '查看回傳內容';
                const pre = document.createElement('pre');
                let text = entry.detail || '';
                try {
                    const data = JSON.parse(text);
                    if (data && typeof data.context === 'string') {
                        try { data.context = JSON.parse(data.context); } catch { /* Keep actual string. */ }
                    }
                    text = JSON.stringify(data, null, 2);
                } catch { /* HTML and validation errors remain plain text. */ }
                if (text) { pre.textContent = text; detail.append(summary, pre); }
                else detail.append(summary);
                if (entry.clipped) {
                    const note = document.createElement('p'); note.textContent = text
                        ? '詳細內容超過保存上限，僅顯示部分。'
                        : '這段詳細內容未保留下來（已達保存上限）；不是模型回傳空白，無法從目前紀錄補回。'; detail.append(note);
                }
                row.append(detail);
            }
            events.append(row);
        }
        tail = last;
    }
    function close() { if (dialog.open) dialog.close(); }
    dialog.querySelector('[data-process-close]').onclick = close;
    dialog.addEventListener('close', () => {
        clearInterval(timer); timer = null;
        const button = opener?.isConnected ? opener : [...panel.querySelectorAll('[data-process]')].find(item => item.dataset.process === selected?.process?.id);
        selected = null;
        button?.focus();
    });
    return {
        open(message, button) {
            selected = message; opener = button; count = -1;
            dialog.querySelector('[data-process-question]').textContent = message.content;
            if (!dialog.open) dialog.showModal();
            refresh(); clearInterval(timer); timer = setInterval(refresh, 500);
        },
        refresh, close,
        destroy() { close(); clearInterval(timer); dialog.remove(); }
    };
}
