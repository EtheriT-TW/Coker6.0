export function createEditorAdapter(options = {}) {
    const adapter = {
        ui: {
            alert(message) {
                window.alert(message);
            },

            confirm(message) {
                return window.confirm(message);
            },

            success(message) {
                window.alert(message);
            },

            error(message) {
                window.alert(message);
            }
        },

        asset: {
            upload: null,
            delete: null
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
        asset: {
            ...adapter.asset,
            ...(options.asset || {})
        },
        content: {
            ...adapter.content,
            ...(options.content || {})
        }
    };
}