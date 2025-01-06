function PageReady() {
    if ($('#SwitchPage').length > 0 && $('#SwitchPage').css('display') !== 'none') {
        var currentUrl = window.location.pathname + window.location.search;
        var catalog = currentUrl.substring(0, currentUrl.indexOf('/article'));
        var articleid = (currentUrl.split('/article/')[1]).split('?')[0].split('/')[0];
        var urlParams = new URLSearchParams(window.location.search);
        var dirid = urlParams.get('dirid');
        var diridList = dirid == null ? null : dirid.split(',').map(Number);

        $("#SwitchPage .btn_list").attr("href", catalog)

        Directory.SwitchPage({ id: articleid, dirids: diridList, routername: catalog.substring(catalog.lastIndexOf("/") + 1), type: 2 }).done(function (result) {
            if (result.length > 0) {
                if (result[0].key != null) {
                    $("#SwitchPage .btn_prev").removeClass("disabled")
                    var link = `${catalog}/article/${result[0].key}`;
                    $("#SwitchPage .btn_prev").attr({
                        href: link,
                        title: result[0].value
                    })
                }
                if (result[1].key != null) {
                    $("#SwitchPage .btn_next").removeClass("disabled")
                    var link = `${catalog}/article/${result[1].key}`;
                    $("#SwitchPage .btn_next").attr({
                        href: link,
                        title: result[1].value
                    })
                }
            }
        });
    }
}