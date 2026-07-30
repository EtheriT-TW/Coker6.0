function PageReady() {
    const $form = $('#globalSettings');

    function setLoading(isLoading) {
        $form.find('button[type="submit"]').prop('disabled', isLoading);
    }

    function loadSettings() {
        setLoading(true);
        co.Templates.getGlobalSettings()
            .then(result => {
                if (!result.success) {
                    throw new Error(result.error || '讀取全站設定失敗');
                }

                const visibility = result.object?.visibility || {};
                $('#showMarquee').prop('checked', visibility.showMarquee ?? true);
                $('#showPagePath').prop('checked', visibility.showPagePath ?? true);
                $('#showPopular').prop('checked', visibility.showPopular ?? false);
                $('#showPublishDate').prop('checked', visibility.showPublishDate ?? true);
            })
            .catch(error => {
                co.sweet.error('讀取失敗', error.message);
            })
            .finally(() => {
                setLoading(false);
            });
    }

    $form.on('submit', function (event) {
        event.preventDefault();
        setLoading(true);

        const data = {
            schemaVersion: 1,
            visibility: {
                showMarquee: $('#showMarquee').prop('checked'),
                showPagePath: $('#showPagePath').prop('checked'),
                showPopular: $('#showPopular').prop('checked'),
                showPublishDate: $('#showPublishDate').prop('checked')
            }
        };

        co.Templates.saveGlobalSettings(data)
            .then(result => {
                if (result.success) {
                    co.sweet.success('儲存成功');
                    return;
                }
                throw new Error(result.error || '儲存全站設定失敗');
            })
            .catch(error => {
                co.sweet.error('儲存失敗', error.message);
            })
            .finally(() => {
                setLoading(false);
            });
    });

    loadSettings();
}
