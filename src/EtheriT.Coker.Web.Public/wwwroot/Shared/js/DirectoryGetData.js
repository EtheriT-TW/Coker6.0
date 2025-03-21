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
    },
    getDirectoryAdvertiseData: function (data) {
        return $.ajax({
            url: "/api/Directory/GetReleAd",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            dataType: "json"
        });
    },
    SwitchPage: function (data) {
        return $.ajax({
            url: "/api/Directory/SwitchPage",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("token")
            },
            data: JSON.stringify(data),
            dataType: "json"
        });
    },
}
var Advertise = {
    ActivityClick: function (FK_Aid) {
        return $.ajax({
            url: "/api/Advertise/ActivityClick/",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            data: { FK_Aid: FK_Aid },
        });
    },
    ActivityExposure: function (FK_Aid) {
        return $.ajax({
            url: "/api/Advertise/ActivityExposure/",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            data: { FK_Aid: FK_Aid },
        });
    },
}
function initElemntAndLoadDir($dir, page) {
    const $self = $dir || $(".catalog_frame").first();
    var temp_siblings = $self.find(".templatecontent").siblings();
    if (temp_siblings.length > 0) {
        for (i = 0; i < temp_siblings.length; i++) {
            if (!$(temp_siblings[i]).hasClass("templatecontent-tag")) temp_siblings[i].remove();
        }
    }
    const dirid = typeof ($self.data("dirid")) != "undefined" ? typeof ($self.data("dirid")) == "string" ? $self.data("dirid").split(",") : [$self.data("dirid")] : 0;
    $self.data("prevdirid", dirid);
    const shownum = typeof ($self.data("shownum")) != "undefined" ? $self.data("shownum") : 12;
    const maxlen = typeof ($self.data("maxlen")) != "undefined" && $self.data("maxlen") != "" ? $self.data("maxlen") : 0;
    const hashPage = !!page ? page.toString() : location.hash.replace("#", "");
    const FindNearest = $self.hasClass("getlatlng");
    const Longitude = typeof ($self.data("longitude")) != "undefined" ? $self.data("longitude") : null;
    const Latitude = typeof ($self.data("latitude")) != "undefined" ? $self.data("latitude") : null;


    if (typeof ($self.data("page")) == "undefined" || $self.data("page") != hashPage) {
        if (isNaN(hashPage) || hashPage == "") page = "1";
        else page = hashPage;
        if (!(dirid.length == 1 && dirid[0] === "")) {
            const option = {
                Ids: dirid,
                SiteId: typeof (SiteId) == "undefined" ? 0 : SiteId,
                Page: page,
                ShowNum: shownum,
                MaxLen: typeof (maxlen) == "undefined" ? 0 : maxlen,
                Filters: $self.data("filtered"),
                directoryType: $self.data("directoryTypeChecked"),
                target: typeof ($self.data("target")) == "undefined" ? null : $self.data("target"),
                FindNearest: FindNearest,
                Longitude: Longitude,
                Latitude: Latitude,
            }
            $self.find(".catalog>.template").remove();
            DirectoryDataGet($self, option);

            $self.off("filter").on("filter", function () {
                $self.removeData("page");
                initElemntAndLoadDir($dir, page);
            });
            $self.data("page", page)
        }
    }
}
function DirectoryGetDataInit() {
    const dirLength = $(".catalog_frame").length;
    $(".catalog_frame").each(function () {
        const $self = $(this);
        const dirid = typeof ($self.data("dirid")) != "undefined" ? typeof ($self.data("dirid")) == "string" ? $self.data("dirid").split(",") : [$self.data("dirid")] : 0;
        if (typeof ($self.data("prevdirid")) == "undefined" || dirid != $self.data("prevdirid")) initElemntAndLoadDir($(this));
    })

    $(".menu_directory").each(function () {
        var $self = $(this);
        const dirid = typeof ($self.data("dirid")) != "undefined" ? typeof ($self.data("dirid")) == "string" ? $self.data("dirid").split(",") : [$self.data("dirid")] : 0;
        var showUnvisible = typeof ($self.attr("data-show-unvisible")) != "undefined" ? $self.attr("data-show-unvisible").toLowerCase() == "true" : false;

        if (typeof ($self.data("prevdirid")) == "undefined" || dirid != $self.data("prevdirid")) {
            $self.data("prevdirid", dirid);
            $self.find(".title").text("");
            $self.find(".accordion").empty();
            Directory.getDirectoryMenuData({
                Ids: dirid,
                WebsiteId: typeof (SiteId) != "undefined" ? SiteId : 0,
                showUnvisible: showUnvisible
            }).done(function (result) {
                if (typeof (result) != "undefined") {
                    $self.find(".title").text(result.title)
                    const groupId = `#${$self.find(".accordion").attr("id")}`;
                    $.each(result.children, function (index, SecIItem) {
                        $self.find('.title').text(result.title);
                        if (SecIItem.children != null) {
                            var item = $($("#TemplateAccordionItem").html()).clone();
                            item.find(".sectitle").text(SecIItem.title);
                            item.find(".accordion-header").attr("id", `secheader${SecIItem.id}`);
                            const accordionCollapse = item.find(".accordion-collapse").attr({
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
                                const _a = $(`<a href="${ThirdIItem.routerName}" title="連結至：${ThirdIItem.title}" class="list-group-item list-group-item-action border-0 py-3">${ThirdIItem.title}</a>`);
                                if (typeof (PageKey) != "undefined" && PageKey.toLowerCase() == ThirdIItem.routerName.toLowerCase()) {
                                    $(_a).addClass("active");
                                    $(accordionCollapse).collapse('show');
                                    item.find(".accordion-header").addClass("active");
                                }
                                $body.append(_a);
                            })
                            $self.find(".accordion").append(item);
                        } else {
                            const link = (SecIItem.routerName != null && SecIItem.routerName != "") ? SecIItem.routerName : SecIItem.linkUrl
                            var html = $(`<div class="accordion-item border-0 border-bottom px-1"><a href="${link}" title="連結至：${SecIItem.title}" class="list-group-item border-0 py-3 custom_h5 text-black"><span>${SecIItem.title}<span></a></div>`);
                            if (SecIItem.imgUrl != null && SecIItem.imgUrl != "") {
                                const $img = $(`<img alt=" " />`);
                                if (SecIItem.overImgUrl == null || (typeof (PageKey) != "undefined" && link.toLowerCase().indexOf(PageKey.toLowerCase()) >= 0)) {
                                    $img.attr("src", SecIItem.imgUrl);
                                } else {
                                    $img.attr("src", SecIItem.overImgUrl);
                                }
                                $(html).find("a").removeClass("py-3").addClass("p-0 imgMenu").append($img);
                            }
                            if (typeof (PageKey) != "undefined" && PageKey.toLowerCase() == SecIItem.routerName.toLowerCase()) {
                                $(html).find("a").addClass("active");
                            }
                            $self.find(".accordion").append(html);
                        }
                    })
                    $(".selectList").length > 0 && $(window).trigger("resize.selectList");

                }
            });
        }
    });

    $(".advertise_directory").each(function () {
        const $self = $(this);
        const dirid = typeof ($self.data("dirid")) != "undefined" ? typeof ($self.data("dirid")) == "string" ? $self.data("dirid").split(",") : [$self.data("dirid")] : 0;
        Directory.getDirectoryAdvertiseData({
            Ids: dirid,
            WebsiteId: typeof (SiteId) != "undefined" ? SiteId : 0,
            showUnvisible: true
        }).done(function (result) {
            DirectoryAdDataInsert($self, result)
        })
    });

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
    const dirLength = $(".catalog_frame").length;
    let page = parseInt(option.Page);
    if ($item.data("type") == "search") {
        $.extend(true, option, {
            Type: "search",
            SearchText: $item.data("search-text").toString(),
            StartDate: $item.data("startDate"),
            EndDate: $item.data("endDate"),
        })
    } else if ($item.data("type") != "undefined") {
        $.extend(true, option, {
            Type: $item.data("type")
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
                var $self = $(this);
                page = parseInt($item.data("page")) - 1;
                $self.data("page", page)
                if (page >= 1) {
                    if (dirLength == 1 && window.location.hash != `#${page}`) window.location.hash = `#${page}`;
                    else initElemntAndLoadDir($item, $self.data("page"))
                }
                $item.goTo();
            })

            $item.find(`.btn_next > button`).on("click", function () {
                var $self = $(this);
                page = parseInt($item.data("page")) + 1;
                $self.data("page", page)
                if (page <= result.totalPage) {
                    if (dirLength == 1 && window.location.hash != `#${page}`) window.location.hash = `#${page}`;
                    else initElemntAndLoadDir($item, $self.data("page"));
                }
                $item.goTo();
            })
        }

        $item.data("init", "true");

        if (result.releInfos.length > 0 && result.releInfos[0].type == 1) {
            var hasbuybtn = false;
            if (typeof ($item.data("hasbuybtn")) == "string") hasbuybtn = $item.data("hasbuybtn").toLowerCase() === "true";
            else hasbuybtn = $item.data("hasbuybtn");
            if (hasbuybtn && ($("#btn_car_dropdown").length > 0 || typeof (OrgName) == "undefined")) $item.addClass("hasBuyBtn");
            else $item.removeClass("hasBuyBtn");

            if (typeof (islogin) == "undefined" && typeof (OrgName) != "undefined") {
                Coker.Token.CheckToken().done(function (token_result) {
                    if (token_result.success) islogin = token_result.isLogin;
                    DirectoryDataInsert($item, result.releInfos);
                });
            } else DirectoryDataInsert($item, result.releInfos);
        } else {
            if ($item.find(".btn_addToCar").length > 0) $item.find(".btn_addToCar").addClass("d-none");
            DirectoryDataInsert($item, result.releInfos);
        }
        $item.data({ filter: result.filter, directoryType: result.directoryType }).trigger("load");

        if ($item.hasClass("swiper") || $item.find(".swiper").length > 0 || $item.hasClass("swiper-wrapper")) {
            let c, b;
            if ($item.hasClass("swiper")) c = $item;
            else if ($item.find(".swiper").length > 0) c = $item.find(".swiper");
            else c = $item.parents(".swiper");
            b = $(c).parents(`[class*="_swiper"]`);
            if ($(b).length > 0) {
                if (typeof ($(b).data("isInit")) != "undefined" && $(b).data("isInit"))
                    c[0].swiper.destroy(true, true);
                b.data("isInit", false);
                SwiperInit({ autoplay: true });
            }
        }
    })
}
function DirectoryDataInsert($item, result) {
    if (result == null) return;
    const temp = $item.find(".templatecontent").html();
    const temp_tag = $item.find(".templatecontent-tag").html();
    const isSearch = $item.data("type") == "search";
    const dirPath = typeof ($item.data("dirpath")) == "undefined" ? "" : $item.data("dirpath").toLowerCase();
    if (result.length == 0) $item.find(".catalog").addClass("empty");
    else $item.find(".catalog").removeClass("empty");

    if ($item.hasClass("hover_display_details") && typeof (OrgName) != "undefined") {
        if (result.length > 0) {
            if (result[0].mainImage != "") {
                $item.data("default_img_link", result[0].mainImage);
                $item.find(".details_display").attr("src", result[0].mainImage);
            }
            else $item.data("default_img_link", $item.find(".details_display").attr("src"));
        }
    }

    result != null && result.forEach(function (data) {
        var content = $(temp).clone();

        if ($item.hasClass("hover_display_details") && typeof (OrgName) != "undefined") content.data("img_link", data.mainImage);

        if ($item.hasClass("hasBuyBtn")) {
            if (content.find(".btn_addToCar").length > 0) content.find(".btn_addToCar").removeClass("d-none");
            else {
                //var btn_addToCarHtml = '<div class="btn_addToCar"><button>加入購物車</button></div>';
                var btn_addToCarHtml = '<div class="btn_addToCar"><button>瀏覽商品</button></div>';
                content.find("div").first().find("a").first().after(btn_addToCarHtml);
            }
        } else {
            if (content.find(".btn_addToCar").length > 0) content.find(".btn_addToCar").addClass("d-none");
        }

        let path, target;
        if (isSearch || window.location.pathname.toLowerCase().indexOf("search") > 0 || window.location.pathname.toLowerCase().indexOf("techcert") > 0) {
            var links = data.link.split("?filter=");
            data.link = links[0];
            var filter = links.length > 1 ? "?filter=" + links[1] : "";
            switch (data.type) {
                case 3:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/${data.link}`;
                    break;
                default:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/search/${data.link}`;
                    break;
            }
            if (typeof ($item.data("search-text")) != "undefined" && $item.data("search-text") != "") {
                path = `${path}/${encodeURIComponent($item.data("search-text"))}${filter}`;
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
            path = window.location.pathname.indexOf(data.orgName) > 0 && window.location.pathname.toLowerCase().indexOf("home") < 0 && window.location.pathname.toLowerCase().indexOf(dirPath) >= 0 ?
                window.location.pathname :
                `${data.orgName == null ? "" : `/${data.orgName}`}${dirPath == "" ? data.orgName == null ? window.location.pathname : window.location.pathname.toLowerCase().replace(`${data.orgName.toLowerCase()}`, "") : `/${dirPath}`}`;
            if (typeof ($item.data("pageto")) != "undefined" && $item.data("pageto") != "") {
                var index = path.substring(1).indexOf("/") + 1;
                path = path.substring(0, index + 1) + $item.data("pageto");
            }
            path += data.link;
            target = "_self";
        }
        if (data.type == 1 && data.status != 0) {
            content.find("a").append(`<span class="status status${data.status}">${data.statusName}</span>`);
        }
        if (!/^http/.test(path)) path = path.replace("//", "/");
        const linkData = {
            "href": path,
            "title": `連結至: ${data.title}${(target == "_blank" ? "(另開視窗)" : "")}`,
            "target": target
        }

        if (content.length > 0) {
            if (content[0].tagName == "A") {
                content.attr(linkData);
            } else {
                content.find("a").attr(linkData);
            }
        }

        if ($item.hasClass("cross_graphics_frame") && data.mainImage == "") {
            content.children("div:first").addClass("d-none");
        }

        if ($item.hasClass("getlatlng")) {
            content.find(".dirname").text(data.dirname);
            content.find(".dirname").removeClass("d-none");
        }

        var imglink = data.mainImage || "/images/noImg.jpg";
        if (data.orgName != null && ((typeof (IsFaPage) != "undefined" && typeof (OrgName) != "undefined" && !IsFaPage) || (typeof (OrgName) != "undefined" && OrgName != data.orgName))) {
            imglink = imglink.replace("upload", `upload/${data.orgName}`);
        }
        content.find("img").attr("src", imglink);
        content.find("img").imgCheck().attr("alt", `${data.title}的主要圖片`);
        content.find(".title").text(data.title);
        content.find(".subtitle").text(data.subtitle);
        content.find(".description").html(data.description);
        //符合無障礙規範，若a裡有圖片或文字則alt為空
        if (content.is('a')) {
            if (content.find("img").length && content.find("h3,h4,h5,h6,span,p").length) {
                content.find("img").imgCheck().attr("alt", " ");
            }
        } else {
            if (content.find("a img").length && content.find('a').find("h3,h4,h5,h6,span,p").length) {
                content.find("img").imgCheck().attr("alt", " ");
            }
        }
        if (content.find(".location").length > 0 && (data.location == null || data.location == "")) content.find(".location").parents(".py-2").remove();
        else content.find(".location").text(data.location);
        if (content.find(".address").length > 0 && (data.address == null || data.address == "")) content.find(".address").parents(".py-2").remove();
        else content.find(".address").text(data.address);
        if (data.startTime != null && data.startTime != "") {
            var startTime = new Date(data.startTime);
            content.find(".startTime").text(`${startTime.getFullYear()}/${startTime.getMonth() + 1}/${startTime.getDate()}`);
        } else {
            content.find(".startTime").each((i, e) => {
                if (e.tagName.toLowerCase() == "span") $(e).parent().remove();
                else $(e).remove();
            });
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
        } else {
            content.find(".date,.date-month,date-monthyear,date-day").each((i, e) => {
                if (e.tagName.toLowerCase() == "span") $(e).parent().remove();
                else $(e).remove();
            });
        }
        if (data.price != null) {
            console.log("data", data)
            if (data.oriPrice != 0 && data.oriPrice != null && data.price != data.oriPrice) {
                content.find(".price-grid").removeClass("price")
                content.find(".normal-price").removeClass("price")
                content.find(".normal-price").addClass("text-end");
                content.find(".price-grid").empty();
                content.find(".normal-price").empty();
                var price_text = `<div class="text-decoration-line-through">$${data.oriPrice}</div><div class="text-danger">會員價 $${data.price}</div>`;
                content.find(".price-grid").append(price_text);
                content.find(".normal-price").append(price_text);
            } else if (data.suggestPrice != null && data.suggestPrice != data.price) {
                content.find(".price-grid").removeClass("price")
                content.find(".normal-price").removeClass("price")
                content.find(".normal-price").addClass("text-end");
                content.find(".price-grid").empty();
                content.find(".normal-price").empty();
                var price_text = `<div class="text-body-tertiary text-decoration-line-through">建議售價$${data.suggestPrice}</div><div class="text-danger">折扣後 $${data.price}</div>`;
                content.find(".price-grid").append(price_text);
                content.find(".normal-price").append(price_text);
            }
            else {
                content.find(".normal-price").text(data.price);
                content.find(".price-grid").text(data.price);
            }
        } else {
            content.find(".price").addClass("notshow");
            content.find(".price").text("");
        }

        if (data.itemNo != null && data.itemNo != "") content.find(".itemNo").text(data.itemNo);
        else content.find(".itemNo").remove();

        $tags = content.find(".tags");
        $tags.empty();
        if (data.tags == null) data.tags = [];
        data.tags.slice(0, 2).forEach((tag) => {
            let badge = $(temp_tag).clone();
            badge.text(tag.tag_Name);
            badge.data("tagname", tag.tag_Name)
            $tags.append(badge);
        });
        data.tags.slice(2).forEach((tag) => {
            let badge = $(temp_tag).clone();
            badge.text(tag.tag_Name.slice(0, 4));
            badge.data("tagname", tag.tag_Name)
            badge.addClass("more-tag d-none");
            $tags.append(badge);
        });
        if (data.tags.length > 2) {
            let badge = $(temp_tag).clone();
            badge.text("...");
            badge.data("tagname", "...")
            badge.addClass("less-tag");
            $tags.append(badge);
        }
        $tags.children().each(function () {
            var $self = $(this);
            if (!$self.hasClass("less-tag")) {
                $self.on("click", function () {
                    var text = $self.data("tagname");
                    location.href = `/${OrgName}/Search/Get/3/${encodeURIComponent(text)}`;
                    return false;
                })
            }
        });
        // Clear content of shareBlock and re-init
        // because content.find("a").attr(linkData); will replace the badly initialized share buttons
        content.find(".shareBlock").data("init", false);
        content.find(".shareBlock > a").remove();
        content.find(".shareBlock").data("href", path);

        if (data.type == 1 && typeof (IsLogin) != "undefined" && IsLogin) ProdFavBtnSet(content, data)

        $item.find(".catalog").append(content);

        if (data.type == 1 && content.find(".btn_addToCar").length > 0) {
            content.find(".btn_addToCar").on("click", function () {
                window.location.href = path;
            });
        }

    });
    HoverEffectInit();
    ShareBlockInit();
    var pathname = window.location.pathname;
    if (pathname.indexOf("Search") > 0) {
        pathname = pathname.substring(0, pathname.indexOf("Search") + 6)
    } else {
        if (pathname.lastIndexOf("_") > 0) {
            var temp_text = pathname.substring(pathname.lastIndexOf("_") + 1);
            if ($.isNumeric(temp_text)) pathname = pathname.substring(0, pathname.lastIndexOf("_"))
        }
    }
    if (typeof (localStorage[`switchViewType-${pathname}`]) != "undefined" && isSearch) {
        var btnclass = localStorage[`switchViewType-${pathname}`];
        $(`button.${btnclass}`).trigger("click");
    }

    if ($item.hasClass("hover_display_details") && typeof (OrgName) != "undefined") {
        $item.find(".details_display_frame").css('height', $item.find(".details_display_frame").height());
        $(window).resize(function () {
            $item.find(".details_display_frame").css('height', $item.find(".details_display_frame").height());
            if (typeof ($item.find(".details_display_frame").css("display")) != "undefined" && $item.find(".details_display_frame").css("display") != "none") {

            }
        });
        if (typeof ($item.find(".details_display_frame").css("display")) != "undefined" && $item.find(".details_display_frame").css("display") != "none") {
            $item.find('.catalog > div:not(.templatecontent)').on('mouseenter', function () {
                var $this = $(this);
                var newImg = $this.data("img_link");
                var $img = $item.find(".details_display");
                var nowImg = $img.attr("src");

                if (newImg != nowImg && newImg != "") {
                    $img.stop(true, true).fadeOut(200, function () {
                        $img.attr("src", newImg).fadeIn(100);
                    });
                }
            }).on('mouseleave', function () {
                var defaultImg = $item.data("default_img_link");
                var $img = $item.find(".details_display");
                var nowImg = $img.attr("src");

                if (defaultImg != nowImg) {
                    $img.stop(true, true).fadeOut(200, function () {
                        $img.attr("src", defaultImg).fadeIn(100);
                    });
                }
            });
        }
    }

    if ($item.hasClass("swiper_display_details") && typeof (OrgName) != "undefined") {
        var $swiper = $item.find(".details_swiper_display_frame");
        if (typeof ($swiper.css("display")) != "undefined" && $swiper.css("display") != "none") {
            var $template_slide = $swiper.find(".template_slide");
            var swiper = $swiper.find(".swiper")[0].swiper;
            if (typeof (swiper) != "undefined") {
                swiper.autoplay.stop();
                swiper.removeAllSlides();
                result.forEach(function (data) {
                    if (data.mainImage != "") {
                        var newSlide = $($template_slide.html()).clone();
                        newSlide.find("img").attr({
                            "src": data.mainImage,
                            "alt": data.title
                        });
                        swiper.appendSlide(newSlide);
                    }
                });
                if (result.length > 1) swiper.autoplay.start();
            }
        }
    }

}
function ProdFavBtnSet(content, data) {
    var html = `<button data-pid="${data.id}" class="btn_fav"></button>`
    content.find(".shareBlock").after(html);
    if (content.find(".shareBlock").hasClass("d-none")) content.find(".btn_fav").addClass("d-none")
    else if (content.find(".shareBlock").hasClass("type5")) content.find(".btn_fav").addClass("type5")
    if (data.fId != null) {
        content.find(".btn_fav").data("fid", data.fId);
        content.find(".btn_fav").addClass("check")
        content.find(".btn_fav").attr("title", "移除收藏")
    }
    FavButtonInit(content.find(".btn_fav"))
}
function DirectoryAdDataInsert($item, result) {
    if ($item.find(".swiper").length > 0) {
        var $swiperwrapper = $item.find(".swiper-wrapper");
        for (var i = 0; i < result.length; i++) {
            var temp = $($swiperwrapper.find(".templatecontent").html()).clone();
            temp = InsertAdDatat(temp, result[i]);
            $swiperwrapper.append(temp);
        }
    } else {
        $item.find(".File_Frame").each(function (index) {
            var $frame = $(this);
            if (result.length > index) {
                InsertAdDatat($frame, result[index]);
            }
        });
    }
}
function InsertAdDatat($frame, result) {
    var isFront = typeof (OrgName) != "undefined";
    var result_File = result.fileLink[0];
    var filetype = result_File.fileType;
    var html;
    if (isFront || typeof ($frame.data("init")) == "undefined") {
        switch (parseInt(filetype)) {
            case 1:
                var $img_frame = $frame.find(".img_frame");
                $img_frame.find("img").attr("src", result_File.link);
                $img_frame.find("img").attr("alt", result.title);
                $img_frame.find("a").attr("href", result.link);
                $img_frame.find("a").attr("title", "連結至" + result.title + (result.target ? "(開新視窗)" : ""));
                $img_frame.find("a").attr("target", (result.target ? "_blank" : "_self"));
                if ($frame.find(".title").length > 0) {
                    $frame.find(".title").text(result.title);
                    $frame.find(".title").removeClass("d-none");
                }
                $img_frame.removeClass("d-none");
                $img_frame.parent().children().not(".img_frame").not(".keep").remove();
                break;
            case 2:
                var $video_frame = $frame.find(".video_frame");
                $video_frame.find("video").attr("src", result_File.link);
                $video_frame.find("video").attr("type", result_File.video_Type);
                $video_frame.removeClass("d-none");
                $video_frame.parent().children().not(".video_frame").remove();
                break;
            case 3:
                var $YT_frame = $frame.find(".YT_frame");
                $YT_frame.find("img").attr("src", "https://img.youtube.com/vi/" + result_File.name + "/maxresdefault.jpg");
                $YT_frame.find("img").attr("alt", result.title);
                $YT_frame.removeClass("d-none");
                $YT_frame.parent().find("div").not(".YT_frame").remove();
                if ($("body").find("#YTPreviewModal").length == 0) {
                    var html = `<div class="modal fade" id="YTPreviewModal" tabindex="-1" aria-labelledby="YTPreviewModal" aria-hidden="true">
                                                  <div class="modal-dialog modal-xl">
                                                    <div class="modal-content position-relative bg-black">
                                                        <div class="modal-header">
                                                        <button type="button" data-bs-dismiss="modal" aria-label="Close" class="bg-light btn-close rounded-circle"</button>
                                                        </div>
                                                        <div class="modal-body"></div>
                                                    </div>
                                                  </div>
                                                </div>`
                    $("body").prepend(html);
                    $("#YTPreviewModal").find(".modal-content").css("height", "90vh");

                    document.getElementById('YTPreviewModal').addEventListener('hidden.bs.modal', function (event) {
                        $("#YTPreviewModal").find(".modal-body").empty();
                    })
                }
                $YT_frame.on("click", function () {
                    var temp_ytlink = `https://www.youtube-nocookie.com/embed/${result_File.name}?&autoplay=1`;
                    $("#YTPreviewModal").find(".modal-body").append(`<iframe src="${temp_ytlink}" class="w-100 h-100" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>`)
                })
                $YT_frame.parent().children().not(".YT_frame").remove();
                break;
        }
        if (result.describe != null && $frame.find(".describe").length > 0) {
            var temp = (result.describe + "").split("\n");
            var describe;
            temp.forEach(function (v, i) {
                if (i == 0) {
                    describe = v;
                } else {
                    describe += `<br/>${v}`;
                }
            });
            $frame.find(".describe").append(describe);
        }
        if ($frame.find(".tag").length > 0) {
            var tags = "";
            for (var i = 0; i < result.tagDatas.length; i++) {
                var taglink = typeof (OrgName) == "undefined" ? "" : `/${OrgName}/Search/Get/${result.tagDatas[i].searchId}/${result.tagDatas[i].title}`;
                tags += `<a href="${taglink}" title="連結至：${result.tagDatas[i].title}" class="pe-2">#${result.tagDatas[i].title}</a>`;
            }
            $frame.find(".tag").append(tags);
        }

        if (isFront) {
            Advertise.ActivityExposure(result.id).done(function (result) {
                //console.log(result)
            })
        }

        $frame.find(".video_frame").find("video").on("play", function () {
            var $this = $(this);
            if (!$this.hasClass("playing")) {
                $(this).addClass("playing")
                Advertise.ActivityClick(result.id).done(function (result) {
                    //console.log(result)
                })
            }
        })

        $frame.find(".video_frame").find("video").on("ended", function () {
            var $this = $(this);
            if ($this.hasClass("playing")) {
                $(this).removeClass("playing")
            }
        })

        $frame.on("click", function () {
            if ($frame.find(".video_frame").length == 0) {
                Advertise.ActivityClick(result.id).done(function (result) {
                    //console.log(result)
                })
            }
        });

        $frame.find(".detectLang").each(function () {
            var $text = $(this);
            var text_html = $text.html();

            const cutting_text = text_html
                .replace(/([a-zA-Z]+)/g, '<span class="english">$1</span>');

            $text.html(cutting_text)
        })
        $frame.data("init", true)
    }

    return $frame;
}
function FavButtonInit($btn_favorites) {
    $btn_favorites.on("click", function () {
        $self = $(this);
        if (!$self.hasClass("check")) {
            Coker.Favorites.Add($btn_favorites.data("pid")).done(function (result) {
                if (result.success) {
                    $btn_favorites.data("fid", result.message);
                    $self.addClass("check")
                    $btn_favorites.attr("title", "移除收藏");
                    Coker.sweet.success("成功將商品加入收藏", null, true);
                } else {
                    console.log(result.message)
                }
            });
        } else {
            if (typeof ($btn_favorites.data("fid")) != "undefined" && typeof ($btn_favorites.data("fid")) != "") {
                Coker.Favorites.Delete($btn_favorites.data("fid")).done(function (result) {
                    if (result.success) {
                        $btn_favorites.data("fid", "");
                        $self.removeClass("check")
                        $btn_favorites.attr("title", "加入收藏");
                        Coker.sweet.success("已將商品從收藏中移除", null, true);
                    } else {
                        console.log(result.message)
                    }
                });
            }
        }
    })
}