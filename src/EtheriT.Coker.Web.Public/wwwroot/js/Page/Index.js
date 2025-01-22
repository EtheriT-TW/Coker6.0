function PageReady() {
    if ($('#SwitchPage').length > 0 && $('#SwitchPage').css('display') !== 'none') {
        var currentUrl = window.location.pathname + window.location.search;
        var catalog = currentUrl.substring(0, currentUrl.indexOf('/article'));
        var articleid = (currentUrl.split('/article/')[1]).split('?')[0].split('/')[0];
        var searchtext = decodeURIComponent((currentUrl.split('/article/')[1]).split('?')[0].split('/')[1]);
        var urlParams = new URLSearchParams(window.location.search);
        var dirid = urlParams.get('dirid');
        var diridList = dirid == null ? null : dirid.split(',').map(Number);

        var routername = catalog.substring(catalog.lastIndexOf("/") + 1)

        if (routername == "search" && sessionStorage.getItem(`article-${searchtext}`)) {
            $("#SwitchPage .btn_list").attr("href", `${catalog}/Get/0/${searchtext}`)
            var alist = JSON.parse(sessionStorage.getItem(`article-${searchtext}`));
            var index = alist.findIndex(a => a.key == `/article/${articleid}`);
            if (alist.length > 0) {
                var orgname = catalog.replace("/search", "")
                if (index > 0) {
                    $("#SwitchPage .btn_prev").removeClass("disabled")
                    var link = alist[index - 1].key.includes('/article/') ? `${orgname}/search${alist[index - 1].key}/${searchtext}` : `${orgname}${alist[index - 1].key}`;
                    $("#SwitchPage .btn_prev").attr({
                        href: link,
                        title: alist[index - 1].value
                    })
                    $("#SwitchPage .btn_prev").removeClass("disabled")
                }
                if (index < alist.length - 1) {
                    $("#SwitchPage .btn_next").removeClass("disabled")
                    var link = alist[index + 1].key.includes('/article/') ? `${orgname}/search${alist[index + 1].key}/${searchtext}` : `${orgname}${alist[index + 1].key}`;
                    $("#SwitchPage .btn_next").attr({
                        href: link,
                        title: alist[index + 1].value
                    })
                    $("#SwitchPage .btn_next").removeClass("disabled")
                }
            } else {
                $('#SwitchPage').remove();
            }
        } else {
            $("#SwitchPage .btn_list").attr("href", catalog)
            Directory.SwitchPage({ id: articleid, dirids: diridList, routername: routername, searchtext: searchtext, type: 2 }).done(function (result) {
                if (result.length > 0) {
                    if (routername == "search") {
                        var orgname = catalog.replace("/search", "")
                        sessionStorage.setItem(`article-${searchtext}`, JSON.stringify(result));
                        var index = result.findIndex(a => a.key == `/article/${articleid}`);
                        if (index > 0) {
                            var link = result[index - 1].key.includes('/article/') ? `${orgname}/search${result[index - 1].key}/${searchtext}` : `${orgname}${result[index - 1].key}`;
                            $("#SwitchPage .btn_prev").attr({
                                href: link,
                                title: result[index - 1].value
                            })
                            $("#SwitchPage .btn_prev").removeClass("disabled")
                        }
                        if (index < result.length - 1) {
                            var link = result[index + 1].key.includes('/article/') ? `${orgname}/search${result[index + 1].key}/${searchtext}` : `${orgname}${result[index + 1].key}`;
                            $("#SwitchPage .btn_next").attr({
                                href: link,
                                title: result[index + 1].value
                            })
                            $("#SwitchPage .btn_next").removeClass("disabled")
                        }
                    } else {
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
                } else {
                    $('#SwitchPage').remove();
                }
            });
        }
    }
}