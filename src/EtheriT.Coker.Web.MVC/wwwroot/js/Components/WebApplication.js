var page_now = 1;
function SelectStationInit() {
    co.WebSite.getPageAll(null).done(function (result) {
        if (result != null && result.totalPage > 0) {
            page_now = result.pageNow;
            if (result.totalPage > 1) {
                StationPageBtnInit($(".page_btn"), result.totalPage)
                StationContentPageChage($(".page_btn"), page_now, result.totalPage);
            }
            StationPageSet(result);
        }
    });
}
function StationPageSet(data) {
    $(".app-switcher").empty();
    var webs = data.webs;
    webs.forEach(function (web) {
        var $frame = $($("#TemplateApp").html()).clone();
        if (web.check) {
            $frame.find("[data-key='Id']").addClass("active-app");
            $frame.find("[data-key='description']").after("<span class='material-icons app-selected md-16'>check</span>")
        }
        $frame.find("[data-key='Id']").data("id", web.id);
        $frame.find("[data-key='image']").attr({ src: web.images, alt: web.name })
        $frame.find("[data-key='name']").text(web.name);
        $frame.find("[data-key='description']").text(web.description);

        $(".app-switcher").append($frame);

        $frame.find($(".webitem")).on("click", function (e) {
            e.preventDefault();
            WebitemClick($frame);
        });
    });

}
function WebitemClick($frame) {
    $(".app-switcher .active-app").removeClass("active-app");
    $(".app-switcher .app-selected").remove();
    $frame.find(".card").addClass("active-app");
    $frame.find(".card-body").append(`<span class="material-icons app-selected md-16">check</span>`);
}
function StationPageBtnInit($btn_page, page_total) {
    $btn_page.addClass("d-flex");
    for (var i = 1; i <= page_total; i++) {
        var html = "";
        if (i == page_total && page_total > 7) {
            html += `<li class="page-item btn_page endhide">
                                    <button class="d-none" title="..." disabled='disabled'>...</button>
                                </li>`;
        }
        html += `<li class="page-item btn_page">
                                    <button class="d-none" data-page='${i}' title="切換至第${i}頁">${i}</button>
                                </li>`;
        if (i == 1 && page_total > 7) {
            html += `<li class="page-item btn_page starthide">
                                    <button class="d-none" title="..." disabled='disabled'>...</button>
                                </li>`;
        }
        $btn_page.find(".btn_next").before(html);
    }
    $btn_page.find(".btn_prev button").on("click", function () {
        var $btn = $(this);
        if (page_now > 1) {
            page_now -= 1;
            StationContentPageChage($btn.parent("li").parent("ul"), page_now, page_total);
            co.WebSite.getPageAll(page_now).done(function (result) {
                StationPageSet(result);
            });
        }
    })
    $btn_page.find(".btn_next button").on("click", function () {
        var $btn = $(this);
        if (page_now < page_total) {
            page_now += 1;
            StationContentPageChage($btn.parent("li").parent("ul"), page_now, page_total);
            co.WebSite.getPageAll(page_now).done(function (result) {
                StationPageSet(result);
            });
        }
    })
    $btn_page.find(".btn_page button").on("click", function () {
        var $btn = $(this);
        page_now = $btn.data("page");
        StationContentPageChage($btn.parent("li").parent("ul"), $btn.data("page"), page_total);
        co.WebSite.getPageAll(page_now).done(function (result) {
            StationPageSet(result);
        });
    })
}

function StationContentPageChage($self, page, page_total) {
    $self.find("li").each(function () {
        var $this_li = $(this);
        var $this_btn = $this_li.find("button");
        if ($this_btn.data("page") == page) {
            if (!$this_btn.hasClass("focus")) $this_btn.addClass("focus")
            if (typeof ($this_btn.attr("disabled")) == "undefined") $this_btn.attr("disabled", "disabled")
        } else {
            if ($this_btn.hasClass("focus")) $this_btn.removeClass("focus")
            if (typeof ($this_btn.attr("disabled")) != "undefined") $this_btn.removeAttr("disabled")
        }
    });

    if (page_total > 7) {
        if (page < 4) {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ($this_btn.data("page") <= 5 || $this_btn.data("page") == page_total) $this_btn.removeClass("d-none")
                else $this_btn.addClass("d-none")
            });
        } else if (page > page_total - 3) {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ($this_btn.data("page") >= page_total - 4 || $this_btn.data("page") == 1) $this_btn.removeClass("d-none")
                else $this_btn.addClass("d-none")
            });
        } else {
            $self.find("li.btn_page").each(function () {
                var $this_li = $(this);
                var $this_btn = $this_li.find("button");
                if ((parseInt(page) + 2 >= $this_btn.data("page") && $this_btn.data("page") >= parseInt(page) - 2) || $this_btn.data("page") == 1 || $this_btn.data("page") == page_total) $this_btn.removeClass("d-none")
                else $this_btn.addClass("d-none")
            });
        }
        if ($self.find(`li button[data-page=2]`).hasClass("d-none")) {
            if ($self.find("li.starthide button").hasClass("d-none")) $self.find("li.starthide button").removeClass("d-none");
        }
        if ($self.find(`li button[data-page=${page_total - 1}]`).hasClass("d-none")) {
            if ($self.find("li.endhide button").hasClass("d-none")) $self.find("li.endhide button").removeClass("d-none");
        }
    } else {
        $self.find("li").each(function () {
            var $this_li = $(this);
            var $this_btn = $this_li.find("button");
            $this_btn.removeClass("d-none")
        });
    }
}