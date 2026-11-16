function PageReady() {
    if ($('#SwitchPage').length > 0 && $('#SwitchPage').css('display') !== 'none') {
        var currentUrl = window.location.pathname + window.location.search;
        var catalog = currentUrl.substring(0, currentUrl.indexOf('/article'));
        var articleid = (currentUrl.split('/article/')[1]).split('?')[0].split('/')[0];
        var searchtext = (currentUrl.split('/article/')[1]).split('?')[0].split('/')[1] || '';
        try { searchtext = co.Directory.decodeSearchKeyword(decodeURIComponent(searchtext)); } catch (_) { /* Keep malformed legacy segments literal. */ }
        var urlParams = new URLSearchParams(window.location.search);
        var dirid = urlParams.get('dirid');
        var diridList = dirid == null ? null : dirid.split(',').map(Number);

        var navigationContext = co.Directory.getNavigationContext();
        var withContext = path => co.Directory.withNavigationContext(path, navigationContext);
        var searchCacheKey = `article-${searchtext}-${JSON.stringify(navigationContext)}`;
        var routername = catalog.substring(catalog.lastIndexOf("/") + 1).toLowerCase()

        if (routername == "search" && searchtext && sessionStorage.getItem(searchCacheKey)) {
            $("#SwitchPage .btn_list").attr("href", withContext(`${catalog}/Get/-1/${co.Directory.encodeSearchKeyword(searchtext)}`))
            var alist = JSON.parse(sessionStorage.getItem(searchCacheKey));
            var index = alist.findIndex(a => a.key == `/article/${articleid}`);
            if (alist.length > 0) {
                var orgname = catalog.replace(/\/search$/i, "")
                if (index > 0) {
                    $("#SwitchPage .btn_prev").removeClass("disabled")
                    var link = alist[index - 1].key.includes('/article/') ? `${orgname}/search${alist[index - 1].key}/${co.Directory.encodeSearchKeyword(searchtext)}` : `${orgname}${alist[index - 1].key}`;
                    $("#SwitchPage .btn_prev").attr({
                        href: withContext(link),
                        title: alist[index - 1].value
                    })
                    $("#SwitchPage .btn_prev").removeClass("disabled")
                }
                if (index < alist.length - 1) {
                    $("#SwitchPage .btn_next").removeClass("disabled")
                    var link = alist[index + 1].key.includes('/article/') ? `${orgname}/search${alist[index + 1].key}/${co.Directory.encodeSearchKeyword(searchtext)}` : `${orgname}${alist[index + 1].key}`;
                    $("#SwitchPage .btn_next").attr({
                        href: withContext(link),
                        title: alist[index + 1].value
                    })
                    $("#SwitchPage .btn_next").removeClass("disabled")
                }
            } else {
                $('#SwitchPage').remove();
            }
        } else {
            $("#SwitchPage .btn_list").attr("href", withContext(routername === "search" && searchtext
                ? `${catalog}/Get/-1/${co.Directory.encodeSearchKeyword(searchtext)}` : catalog))
            co.Directory.SwitchPage({ ...navigationContext, id: articleid, dirids: diridList, routername: routername, searchtext: searchtext, type: 2 }).done(function (result) {
                if (result.length > 0) {
                    if (routername == "search" && searchtext) {
                        var orgname = catalog.replace(/\/search$/i, "")
                        sessionStorage.setItem(searchCacheKey, JSON.stringify(result));
                        var index = result.findIndex(a => a.key == `/article/${articleid}`);
                        if (index > 0) {
                            var link = result[index - 1].key.includes('/article/') ? `${orgname}/search${result[index - 1].key}/${co.Directory.encodeSearchKeyword(searchtext)}` : `${orgname}${result[index - 1].key}`;
                            $("#SwitchPage .btn_prev").attr({
                                href: withContext(link),
                                title: result[index - 1].value
                            })
                            $("#SwitchPage .btn_prev").removeClass("disabled")
                        }
                        if (index < result.length - 1) {
                            var link = result[index + 1].key.includes('/article/') ? `${orgname}/search${result[index + 1].key}/${co.Directory.encodeSearchKeyword(searchtext)}` : `${orgname}${result[index + 1].key}`;
                            $("#SwitchPage .btn_next").attr({
                                href: withContext(link),
                                title: result[index + 1].value
                            })
                            $("#SwitchPage .btn_next").removeClass("disabled")
                        }
                    } else {
                        if (result[0]?.key != null) {
                            $("#SwitchPage .btn_prev").removeClass("disabled")
                            var link = `${catalog}/article/${result[0].key}`;
                            $("#SwitchPage .btn_prev").attr({
                                href: withContext(link),
                                title: result[0].value
                            })
                        }
                        if (result[1]?.key != null) {
                            $("#SwitchPage .btn_next").removeClass("disabled")
                            var link = `${catalog}/article/${result[1].key}`;
                            $("#SwitchPage .btn_next").attr({
                                href: withContext(link),
                                title: result[1].value
                            })
                        }
                    }
                } else {
                    $('#SwitchPage').remove();
                }
            });
        }
    }
}