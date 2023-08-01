var Directory = {
    getDirectoryData: function (data) {
        return $.ajax({
            url: "/api/Directory/GetReleInfo",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            dataType: "json"
        });
    },
    getDirectoryMenuData: function (data) {
        return $.ajax({
            url: "/api/Directory/GetReleMenu",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            dataType: "json"
        });
    }
}

function DirectoryGetDataInit() {
    var dirid, page, shownum;
    $(".catalog_frame").each(function () {
        var $self = $(this)
        var dirid = $self.attr("data-dirid") > 0 ? $self.attr("data-dirid") : 0;
        if (typeof ($self.data("prevdirid")) == "undefined" || dirid != $self.data("prevdirid")) {
            $self.data("prevdirid", dirid);
            shownum = typeof ($self.data("shownum")) != "undefined" ? $self.data("shownum") : 12;
            page = 1;
            DirectoryDataGet($self, dirid, page, shownum);
        }
    })
    $(".menu_directory").each(function () {
        var $self = $(this);
        var dirid = $self.attr("data-dirid") > 0 ? $self.attr("data-dirid") : 0;
        if (typeof ($self.data("prevdirid")) == "undefined" || dirid != $self.data("prevdirid")) {
            $self.data("prevdirid", dirid);
            $self.find(".title").text("");
            $self.find(".accordion").empty();
            Directory.getDirectoryMenuData({ Id: dirid, WebsiteId: typeof (SiteId) != "undefined" ? SiteId : 0 }).done(function (result) {
                if (typeof (result) != "undefined") {
                    $self.find(".title").text(result.title)
                    $.each(result.children, function (index, SecIItem) {
                        $self.find('.title').text(result.title);
                        if (SecIItem.children != null) {
                            var item = $($("#TemplateAccordionItem").html()).clone();
                            item.find(".sectitle").text(SecIItem.title);
                            item.find(".accordion-header").attr("id", `secheader${SecIItem.Id}`);
                            item.find(".accordion-collapse").attr({
                                "aria-labelledby": `secheader${SecIItem.Id}`,
                                "id": `seccollapse${SecIItem.Id}`
                            });
                            item.find(".accordion-button").attr({
                                "data-bs-target": `#seccollapse${SecIItem.Id}`,
                                "aria-controls": `seccollapse${SecIItem.Id}`,
                            });
                            var $body = item.find(".accordion-body");
                            $.each(SecIItem.children, function (index, ThirdIItem) {
                                $body.append(`<a href="${ThirdIItem.routerName}" title="連結至：${ThirdIItem.title}" class="list-group-item list-group-item-action border-0 py-3">${ThirdIItem.title}</a>`)
                            })
                            $self.find(".accordion").append(item);
                        } else {
                            var html = `<div class="accordion-item border-0 border-bottom px-1"><a href="${SecIItem.routerName}" title="連結至：${SecIItem.title}" class="list-group-item border-0 py-3 custom_h5 text-black">${SecIItem.title}</a></div>`
                            $self.find(".accordion").append(html);
                        }
                    })
                }
            });
        }
    })
}

function DirectoryDataGet($item, dirid, page, shownum) {
    Directory.getDirectoryData({ Ids: [dirid], SiteId: typeof (SiteId) == "undefined" ? 0 : SiteId, Page: page, ShowNum: shownum }).done(function (result) {
        $item.find(".page-item").each(function () {
            var $self = $(this);
            if (!$self.hasClass("btn_prev") && !$self.hasClass("btn_next")) $self.remove();
        });
        if (result.totalPage <= 1) $item.find(".page_btn").addClass("d-none")
        for (var i = 1; i <= result.totalPage; i++) {
            if (i == 1) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black bg-secondary" data-page=${i}>第一頁</button></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>第一頁</button></li>`)
            } else if (i == result.totalPage) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black bg-secondary" data-page=${i}>最後一頁</button></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>最後一頁</button></li>`)
            } else if (page < 5 && i < 7) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black bg-secondary" data-page=${i}>${i}</button></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>${i}</button></li>`)
            } else if (page > result.totalPage - 4 && i > result.totalPage - 6) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black bg-secondary" data-page=${i}>${i}</button></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>${i}</button></li>`)
            } else if (i >= page - 2 && i <= page + 2) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black bg-secondary" data-page=${i}>${i}</button></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>${i}</button></li>`)
            }
        }

        $(`.btn_page`).on("click", function () {
            var $self = $(this);
            if (page != $self.data("page")) {
                $item.find(".catalog").children().each(function () {
                    var $self = $(this);
                    if (!$self.hasClass("templatecontent")) {
                        $self.remove();
                    }
                })
                page = $self.data("page");
                DirectoryDataGet($item, dirid, page, shownum);
            }
        })

        if (!$item.data("init")) {
            $(`.btn_prev > button`).on("click", function () {
                if (page > 1) {
                    $item.find(".catalog").children().each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("templatecontent")) {
                            $self.remove();
                        }
                    })
                    page--;
                    DirectoryDataGet($item, dirid, page, shownum);
                }
            })

            $(`.btn_next > button`).on("click", function () {
                if (page < result.totalPage) {
                    $item.find(".catalog").children().each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("templatecontent")) {
                            $self.remove();
                        }
                    })
                    page++;
                    DirectoryDataGet($item, dirid, page, shownum);
                }
            })
        }

        $item.data("init", "true");
        DirectoryDataInsert($item, result.releInfos);
    })
}

function DirectoryDataInsert($item, result) {
    result.forEach(function (data) {
        var content = $($item.find(".templatecontent").html()).clone();
        content.find("a").attr("href", window.location.pathname + data.link);
        content.find("a").attr("titile", `連結至: ${data.title}`);
        var imglink = data.mainImage;
        if (typeof (IsFaPage) != "undefined" && typeof (OrgName) != "undefined" && IsFaPage != "True") imglink = imglink.replace("upload", `upload/${OrgName}`);
        content.find("img").attr("src", imglink);
        content.find("img").attr("alt", `${data.title}的主要圖片`);
        content.find(".title").text(data.title);
        content.find(".date").text(data.nodeDate);
        content.find(".description").text(data.description);

        $item.find(".catalog").append(content);
    });

    HoverEffectInit();
}