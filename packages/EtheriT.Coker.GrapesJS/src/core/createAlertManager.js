export class AlertManager {
    constructor(ui = {}) {
        this.ui = ui;
        this.hostWindow = typeof window === 'undefined' ? null : window;
    }

    alert(message, options = {}) {
        return this.invoke('alert', message, options);
    }

    success(message, options = {}) {
        return this.invoke('success', message, options);
    }

    error(message, options = {}) {
        return this.invoke('error', message, options);
    }

    async confirm(message, options = {}) {
        const handler = this.ui?.confirm;
        const result = typeof handler === 'function'
            ? handler(message, options)
            : this.hostWindow?.confirm?.(String(message ?? ''));

        return Boolean(await Promise.resolve(result));
    }

    invoke(type, message, options) {
        const handler = this.ui?.[type];
        if (typeof handler === 'function') {
            return handler(message, options);
        }

        return this.hostWindow?.alert?.(String(message ?? ''));
    }
}

export function attachAlertManager(editor, adapter = {}) {
    const manager = new AlertManager(adapter.ui);

    editor.AlertManager = manager;
    editor.EtheriTCoker = {
        ...(editor.EtheriTCoker || {}),
        AlertManager: manager
    };

    return manager;
}
