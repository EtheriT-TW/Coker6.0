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

        ai: {
            generate: null
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
        ai: {
            ...adapter.ai,
            ...(options.ai || {})
        },
        content: {
            ...adapter.content,
            ...(options.content || {})
        }
    };
}
