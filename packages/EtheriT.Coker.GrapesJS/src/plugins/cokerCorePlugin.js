export function cokerCorePlugin(editor, options = {}) {
    editor.EtheriTCoker = {
        options,
        pluginOptions: options
    };

    editor.Commands.add('etherit:coker:test', {
        run() {
            const adapter = options.adapter;

            if (adapter?.ui?.success) {
                adapter.ui.success('EtheriT.Coker.GrapesJS plugin loaded.');
                return;
            }

            window.alert('EtheriT.Coker.GrapesJS plugin loaded.');
        }
    });
}