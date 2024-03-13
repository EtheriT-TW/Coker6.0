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

function initElemntAndLoadDir($dir,page) {
    const $self = $dir || $(".catalog_frame,.menu_directory").first();
    const dirid = $self.attr("data-dirid") > 0 ? $self.attr("data-dirid") : 0;
    
    $self.data("prevdirid", dirid);
    const shownum = typeof ($self.data("shownum")) != "undefined" ? $self.data("shownum") : 12;
    const maxlen = typeof ($self.data("maxlen")) != "undefined" ? $self.data("maxlen") : 0;
    const hashPage = !!page? page.toString() : location.hash.replace("#", "");

    if (typeof ($self.data("page")) == "undefined" || $self.data("page") != hashPage) {
        if (isNaN(hashPage) || hashPage=="") page = "1";
        else page = hashPage;
        $self.find(".catalog>.template").remove();
        DirectoryDataGet($self,{
            Ids: [dirid],
            SiteId: typeof (SiteId) == "undefined" ? 0 : SiteId,
            Page: page,
            ShowNum: shownum,
            MaxLen: typeof (maxlen) == "undefined" ? 0 : maxlen
        });
        $self.data("page", page)
    }
    
}

function DirectoryGetDataInit() {
    const dirLength = $(".catalog_frame,.menu_directory").length;
    $(".catalog_frame").each(function () {
        const $self = $(this);
        const dirid = $self.attr("data-dirid") > 0 ? $self.attr("data-dirid") : 0;
        if (typeof ($self.data("prevdirid")) == "undefined" || dirid != $self.data("prevdirid")) initElemntAndLoadDir($(this));
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
                    const groupId = `#${$self.find(".accordion").attr("id")}`;
                    $.each(result.children, function (index, SecIItem) {
                        $self.find('.title').text(result.title);
                        if (SecIItem.children != null) {
                            var item = $($("#TemplateAccordionItem").html()).clone();
                            item.find(".sectitle").text(SecIItem.title);
                            item.find(".accordion-header").attr("id", `secheader${SecIItem.id}`);
                            const accordionCollapse= item.find(".accordion-collapse").attr({
                                "aria-labelledby": `secheader${SecIItem.id}`,
                                "id": `seccollapse${SecIItem.id}`,
                                "data-bs-parent": `${groupId}`
                            });
                            item.find(".accordion-button").attr({
                                "data-bs-target": `#seccollapse${SecIItem.id}`,
                                "aria-controls": `seccollapse${SecIItem.id}`,
                            });
                            var $body = item.find(".accordion-body");
                            $.each(SecIItem.children, function (index, ThirdIItem) {
                                if (PageKey.toLowerCase() == ThirdIItem.routerName.toLowerCase()) {
                                    $(accordionCollapse).collapse('show'); 
                                }
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
    if (dirLength == 1) {
        if ("onhashchange" in window) {
            window.onhashchange = hashChangeDirectory;
        } else {
            setInterval(hashChangeDirectory, 1000);
        }
    }
}
function hashChangeDirectory(e) {
    if (!!e) {
        initElemntAndLoadDir();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}

function DirectoryDataGet($item, option) {
    const dirLength = $(".catalog_frame,.menu_directory").length;
    let page = parseInt(option.Page);
    if ($item.data("type") == "search") {
        $.extend(true, option, {
            Type:"search",
            SearchText: $item.data("search-text").toString(),
            StartDate: $item.data("startDate"),
            EndDate: $item.data("endDate"),
        })
    }
    Directory.getDirectoryData(option).done(function (result) {
        let loadPageRange = 2;
        if (option.Type == "search") {
            $(".searchCount").text(result.totalCount);
        }
        $item.find(".page-item").each(function () {
            var $self = $(this);
            if (!$self.hasClass("btn_prev") && !$self.hasClass("btn_next")) $self.remove();
        });
        if (result.totalPage <= 1) $item.find(".page_btn").addClass("d-none")
        else $item.find(".page_btn").removeClass("d-none")
        if (page > result.totalPage) page = result.totalPage;
        else if (page < 1) page = 1;

        if (page == 1) $item.find(".btn_prev").addClass("d-none");
        else $item.find(".btn_prev").removeClass("d-none");

        if (page == result.totalPage) $item.find(".btn_next").addClass("d-none");
        else $item.find(".btn_next").removeClass("d-none");

        if (page == 1 || page == result.totalPage) loadPageRange = 4;

        for (var i = 1; i <= result.totalPage; i++) {
            if (i == 1) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page=${i}>1</span></li>`)
                else {
                    $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" title="第一頁" data-page=${i}>1</button></li>`);
                    if (page - loadPageRange - 1 > 0) {
                        $item.find(".page_btn").children(".btn_next").before(`<li class="page-item px-2">...</li>`);
                    }
                }
            } else if (i == result.totalPage) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page=${i}>${result.totalPage}</span></li>`)
                else {
                    if (page + loadPageRange + 1 < result.totalPage) {
                        $item.find(".page_btn").children(".btn_next").before(`<li class="page-item px-2">...</li>`);
                    }
                    $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" title="最後一頁" data-page=${i}>${result.totalPage}</button></li>`);
                }
            } else if (i >= page - loadPageRange && i <= page + loadPageRange) {
                if (i == page) $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page=${i}>${i}</span></li>`)
                else $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" title="移動至第${i}頁" data-page=${i}>${i}</button></li>`)
            }
        }
        $(`.btn_page`).off("click").on("click", function () {
            var $self = $(this);
            if (page != $self.data("page")) {
                page = $self.data("page");
                if (dirLength == 1 && window.location.hash != `#${page}`) window.location.hash = `#${page}`;
                else initElemntAndLoadDir($item, page);
            }
            $item.goTo();
        })
        if (!$item.data("init")) {
            $item.find(`.btn_prev > button`).on("click", function () {
                page = parseInt($item.data("page")) - 1;
                if (page >= 1) {
                    if (dirLength == 1 && window.location.hash != `#${page}`) window.location.hash = `#${page}`;
                    else initElemntAndLoadDir($item, $self.data("page"))
                }
                $item.goTo();
            })

            $item.find(`.btn_next > button`).on("click", function () {
                page = parseInt($item.data("page")) + 1;
                if (page <= result.totalPage) {
                    if (dirLength == 1 && window.location.hash != `#${page}`) window.location.hash = `#${page}`;
                    else initElemntAndLoadDir($item, $self.data("page"))
                }
                $item.goTo();
            })
        }

        $item.data("init", "true");
        DirectoryDataInsert($item, result.releInfos);
    })
}

function DirectoryDataInsert($item, result) {
    if (result == null) return;
    const temp = $item.find(".templatecontent").html();
    const temp_tag = $item.find(".templatecontent-tag").html();
    const isSearch = $item.data("type") == "search";
    if (result.length == 0) $item.find(".catalog").addClass("empty");
    else $item.find(".catalog").removeClass("empty");
    result != null && result.forEach(function (data) {
        var content = $(temp).clone();
        let path,target;
        if (isSearch) {
            switch (data.type) {
                case 3:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/${data.link}/${$item.data("search-text")}`;
                    break;
                default:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/search/${data.link}/${$item.data("search-text")}`;
                    break;
            }
           
            target = "_blank";
            if (data.mainImage.indexOf("youtu") > 0) {
                var key = "";
                var rx = /^.*(?:(?:youtu.be\/|v\/|vi\/|u\/w\/|embed\/)|(?:(?:watch)??v(?:i)?=|&v(?:i)?=))([^#&?]*).*/;
                var r = data.mainImage.match(rx);
                if (r != null && r.length > 0) key = r[1];
                data.mainImage = "https://img.youtube.com/vi/" + key + "/mqdefault.jpg";
            }
        } else {
            path = (window.location.pathname.indexOf(data.orgName) > 0 ? window.location.pathname : `${data.orgName == null ? "" : `/${data.orgName}`}${window.location.pathname}`) + data.link;
            target = "_self";
        }
        path = path.replace("//", "/");
        const linkData = {
            "href": path,
            "title": `連結至: ${data.title}${(target == "_blank" ? "(另開視窗)" : "")}`,
            "target": target
        }
        if (content[0].tagName == "A") {
            content.attr(linkData);
        } else {
            content.find("a").attr(linkData);
        }

        var imglink = data.mainImage || "/images/noImg.jpg";
        if (data.orgName!=null && ((typeof (IsFaPage) != "undefined" && typeof (OrgName) != "undefined" && IsFaPage != "True") || (typeof (OrgName) != "undefined" && OrgName != data.orgName))) {
            imglink = imglink.replace("upload", `upload/${data.orgName}`);
        }
        content.find("img").attr("src", imglink);
        content.find("img").attr("alt", `${data.title}的主要圖片`);
        content.find(".title").text(data.title);
        content.find(".description").html(data.description);
        if (content.find(".location").length > 0 && (data.location == null || data.location == "")) content.find(".location").parents(".py-2").remove();
        else content.find(".location").text(data.location);
        if (content.find(".address").length > 0 && (data.address == null || data.address == "")) content.find(".address").parents(".py-2").remove();
        else content.find(".address").text(data.address);
        if (data.startTime != null && data.startTime != "") {
            var startTime = new Date(data.startTime);
            content.find(".startTime").text(`${startTime.getFullYear()}/${startTime.getMonth() + 1}/${startTime.getDate()}`);
        }
        if (data.nodeDate != null && data.nodeDate != "") {
            var noteDate = new Date(data.nodeDate);
            content.find(".date").text(`${noteDate.getFullYear()}/${noteDate.getMonth() + 1}/${noteDate.getDate()}`);
            content.find(".date-month").text(`${noteDate.getMonth() + 1}月`);
            content.find(".date-monthyear").text(`${noteDate.getMonth() + 1}/${noteDate.getFullYear()}`);
            if (noteDate.getDate() < 10) {
                content.find(".date-day").text(`0${noteDate.getDate()}`);
            } else {
                content.find(".date-day").text(`${noteDate.getDate()}`);
            }
            if (noteDate.getMonth() < 9) {
                content.find(".date-month-number").text(`0${noteDate.getMonth() + 1}`);
            } else {
                content.find(".date-month-number").text(`${noteDate.getMonth() + 1}`);
            }
            content.find(".date-year").text(`${noteDate.getFullYear()}`);
        }
        
        if (data.price != null) {
            const convert_price = (price) => {
                if (price.includes("~")) {
                    [price_low, price_mid, price_high] = price.split(" ");
                    return price_high
                }
                else return price;
            }
            content.find(".normal-price").text(convert_price(data.price));
            content.find(".price-grid").text(convert_price(data.price));
            content.find(".itemNo").text(data.itemNo);
            $tags = content.find(".tags");
            $tags.empty();
            data.tags.slice(0, 2).forEach((tag) => {
                var badge = $(temp_tag).clone();
                badge.text(tag.tag_Name.slice(0, 4));
                $tags.append(badge);
            });
            data.tags.slice(2).forEach((tag) => {
                var badge = $(temp_tag).clone();
                badge.text(tag.tag_Name.slice(0, 4));
                badge.addClass("more-tag d-none");
                $tags.append(badge);
            });
            if (data.tags.length > 2) {
                var badge = $(temp_tag).clone();
                badge.text("...");
                badge.addClass("less-tag");
                $tags.append(badge);
            }
        }
        // Clear content of shareBlock and re-init
        // because content.find("a").attr(linkData); will replace the badly initialized share buttons
        content.find(".shareBlock > a").remove();
        content.find(".shareBlock").data("href", path);

        $item.find(".catalog").append(content);
    });

    HoverEffectInit();
    ShareBlockInit();
}