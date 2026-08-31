export function createEditorAdapter(options = {}) {
    const hostWindow = typeof window === 'undefined' ? null : window;
    const nativeAlert = message => hostWindow?.alert?.(String(message ?? ''));

    const adapter = {
        ui: {
            alert(message) {
                nativeAlert(message);
            },

            confirm(message) {
                return hostWindow?.confirm?.(String(message ?? '')) ?? false;
            },

            success(message) {
                nativeAlert(message);
            },

            error(message) {
                nativeAlert(message);
            }
        },

        content: {
            save: null,
            import: null,
            loadComponents: null
        }
    };

    return {
        ...adapter,
        ...options,
        ui: {
            ...adapter.ui,
            ...(options.ui || {})
        },
        content: {
            ...adapter.content,
            ...(options.content || {})
        }
    };
}
