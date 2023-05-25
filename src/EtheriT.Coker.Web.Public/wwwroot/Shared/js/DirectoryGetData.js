function DirectoryGetDataInit() {
    var dirid, page, shownum;
    $(".catalog_frame").each(function () {
        var $self = $(this)
        if ($self.data("dirid") && $self.data("shownum")) {
            dirid = $self.data("dirid");
            page = 1;
            shownum = $self.data("shownum");
            DirectoryDataGet($self, dirid, page, shownum);
        }
    })

}

function DirectoryDataGet($item, dirid, page, shownum) {
    Coker.Directory.getDirectoryData({ Ids: [dirid], SiteId: SiteId, Page: page, ShowNum: shownum }).done(function (result) {
        if (!$item.data("init")) {
            for (var i = 1; i <= result.totalPage; i++) {
                $item.find(".page_btn").children(".btn_next").before(`<li class="page-item"><button class="btn_page page-link text-black" data-page=${i}>${i}</button></li>`)
            }

            $(`.btn_prev > button`).on("click", function () {
                console.log("Prev")
                if (page > 1) {
                    $item.find(".catalog").children().each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("templatecontent")) {
                            $self.remove();
                        }
                    })
                    page--;
                    DirectoryDataGet($item, page, shownum);
                }
            })

            $(`.btn_page`).on("click", function () {
                var $self = $(this);
                if (page != $self.data("page")) {
                    console.log($self);
                    $item.find(".catalog").children().each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("templatecontent")) {
                            $self.remove();
                        }
                    })
                    page = $self.data("page");
                    DirectoryDataGet($item, page, shownum);
                }
            })

            $(`.btn_next > button`).on("click", function () {
                console.log("Next")
                if (page < result.totalPage) {
                    $item.find(".catalog").children().each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("templatecontent")) {
                            $self.remove();
                        }
                    })
                    page++;
                    DirectoryDataGet($item, page, shownum);
                }
            })
        }

        $item.data("init", "true");

        DirectoryDataInsert($item, result.releInfos);
    })
}

function DirectoryDataInsert($item, result) {
    result.forEach(function (data) {
        console.log(data)
        var content = $($item.find(".templatecontent").html()).clone();
        content.find("a").attr("href", data.link);
        content.find("a").attr("alt", data.name);

        content.find("img").attr("src", data.mainImage);
        content.find("img").attr("alt", typeof (data.name) == "undefined" ? "" : `${data.name}的主要圖片`);

        content.find("a").attr("href", data.link);

        content.find(".title").text(data.title);
        content.find(".description").text(data.description);

        $item.find(".catalog").append(content);
    });

    HoverEffectInit();
}