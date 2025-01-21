const newEnum = (descriptions) => {
    const result = {};
    Object.keys(descriptions).forEach((description) => {
        result[result[description] = descriptions[description]] = description;
    });
    return result;
};

const list_type = newEnum({
    grid: "Grid",
    list: "List",
    text: "Text",
    product_grid: "Product_Grid",
    product_list: "Product_List",
});

const display_configurations = {
    [list_type.list]: ["type1 bg-white", {
        ".card-border": "card-border-rd type3",
        ".catalog-number": "d-none",
        ".check_btn": "check_btn-type3",
        ".col": "p-1 type3 box-shadow",
        ".description": "type3-content",
        ".image": "px-0",
        ".image_frame": "h-100",
        ".imgh": "img-h",
        ".item-header": "d-none",
        ".like-and-share": "d-none",
        ".max-h": "max-hei",
        ".mergetag": "merge",
        ".more-btn": "d-none",
        ".more-tag": "d-none",
        ".price-grid": "d-none",
        ".purchase": "d-none",
        ".related-tag": "d-none",
        ".shareBlock": "d-none",
        ".btn_fav": "d-none",
        ".tags": "d-none",
        ".title": "type1-title fw-bold p-2",
        "figcaption": "flex-grow-1 p-3 py-0 py-md-3 pb1",
    }],
    [list_type.grid]: ["type2 row row-cols-sm-4 bg-white", {
        ".card-border": "card-border-rd type2",
        ".catalog-number": "d-none",
        ".check_btn": "w-100",
        ".col": "p-1 type2 box-shadow",
        ".date": "d-none",
        ".description": "d-none type2-content",
        ".image_frame": "w-100",
        ".item-header": "d-none",
        ".like-and-share": "d-none",
        ".more": "d-none",
        ".more-btn": "d-none",
        ".more-tag": "d-none",
        ".price-grid": "d-none",
        ".purchase": "d-none",
        ".related-tag": "d-none",
        ".shareBlock": "d-none",
        ".btn_fav": "d-none",
        ".tags": "d-none",
        ".title": "type1-title fw-bold p-2",
        "figcaption": "pb1",
        "figure": "flex-column",
    }],
    [list_type.text]: ["type3 row row-cols-sm-4 bg-white", {
        ".card-border": "pr3 type1",
        ".catalog-number": "d-none",
        ".check_btn": "w-100",
        ".col": "p-1 type1 text-border box-shadow",
        ".date": "d-none",
        ".description": "d-none",
        ".image": "d-none",
        ".image_frame": "d-none",
        ".item-header": "d-none",
        ".like-and-share": "d-none",
        ".more": "d-none",
        ".more-tag": "d-none",
        ".price-grid": "d-none",
        ".purchase": "d-none",
        ".related-tag": "d-none",
        ".shareBlock": "d-none",
        ".btn_fav": "d-none",
        ".search-more": "d-none",
        ".tags": "d-none",
        ".title": "type2-title pr3 h-100",
        "figure": "flex-column",
    }],
    [list_type.product_grid]: ["type4 row row-cols-lg-4 row-cols-md-2 row-cols-2  bg-light px-2", {
        ".card-border": "card-border-rd type2",
        ".catalog-number": "d-none",
        ".check_btn": "w-100",
        ".col": "p-1 type4 rounded-lg h-100",
        ".date": "d-none",
        ".description": "d-none type2-content",
        ".image_frame": "type4-image-frame w-100",
        ".item-header": "d-flex",
        ".like-and-share": "d-none",
        ".more": "d-none",
        ".more-btn": "d-none",
        ".more-tag": "d-none",
        ".purchase": "d-none",
        ".shareBlock": "type4",
        ".btn_fav": "type4",
        ".tags": "align-text-bottom",
        ".title": "type4-title d-inline fs-6",
        "figcaption": "pb1 type4-caption d-flex flex-column",
        "figure": "flex-column",
    }],
    [list_type.product_list]: ["type1 bg-light px-2", {
        ".bottom-row": "mt-auto",
        ".card-border": "card-border-rd type2",
        ".catalog-number": "type4-title d-inline",
        ".check_btn": "check_btn-type3",
        ".col": " type5 rounded-lg",
        ".description": "type3-content",
        ".image": "px-0",
        ".image_frame": "type5-image-frame",
        ".imgh": "img-h",
        ".item-header": "d-none",
        ".item-title": "d-lg-flex",
        ".less-tag": "d-none",
        ".like-and-share": "d-inline ms-auto fs-5 p-2",
        ".max-h": "max-hei",
        ".mergetag": "merge",
        ".more": "d-none",
        ".more-btn": "d-none",
        ".price-grid": "d-none",
        ".purchase": "ms-auto d-inline",
        ".related-tag": "d-none",
        ".shareBlock": "type5",
        ".btn_fav": "type5",
        ".tags": "mt-auto mb-2 align-text-bottom d-lg-flex",
        ".title": "type4-title d-inline fs-6 p-2",
        "figcaption": "flex-grow-1 d-flex flex-column",
    }],
}

function ViewTypeChangeInit() {
    $(".type_change_frame").each(function () {
        var $self = $(this)
        if (!!!$self.data("isInit")) {
            const $btn_grid = $self.find(".btn_grid");
            const $btn_list = $self.find(".btn_list");
            const $btn_text = $self.find(".btn_text");
            const $btn_prod_grid = $self.find(".btn_prod_grid");
            const $btn_prod_list = $self.find(".btn_prod_list");
            const $content = $self.find(".content").first();
            const $btns = {
                [list_type.grid]: $btn_grid,
                [list_type.list]: $btn_list,
                [list_type.text]: $btn_text,
                [list_type.product_grid]: $btn_prod_grid,
                [list_type.product_list]: $btn_prod_list,
            }

            var pathname = window.location.pathname;
            if (pathname.indexOf("Search") > 0) {
                pathname = pathname.substring(0, pathname.indexOf("Search") + 6)
            } else {
                if (pathname.lastIndexOf("_") > 0) {
                    var temp_text = pathname.substring(pathname.lastIndexOf("_") + 1);
                    if ($.isNumeric(temp_text)) pathname = pathname.substring(0, pathname.lastIndexOf("_"))
                }
            }

            for (const [config, $btn] of Object.entries($btns)) {
                $btn.on("click", function () {
                    if (!$btn.data("activate")) {
                        var btn_list = $btn.attr("class").split(" ");
                        var btn_list_str = btn_list.filter(item => item.startsWith("btn_")).toString().replaceAll(",", ".");
                        localStorage.setItem(`switchViewType-${pathname}`, btn_list_str);
                        typeChange($btn, $btns, $content, config);
                    }
                })
            }
            var TypeButtonInit = false;
            if (typeof (localStorage[`switchViewType-${pathname}`]) != "undefined") {
                if (pathname.indexOf("Search") < 0) {
                    var btnclass = localStorage[`switchViewType-${pathname}`];
                    $(`button.${btnclass}`).trigger("click");
                }
                TypeButtonInit = true;
            }
            if (!TypeButtonInit) {
                if ($btn_grid.length > 0 && !$btn_grid.hasClass("d-none")) {
                    $btn_grid.trigger("click");
                }
                else if ($btn_list.length > 0 && !$btn_list.hasClass("d-none")) {
                    $btn_list.trigger("click");
                }
                else if ($btn_prod_grid.length > 0 && !$btn_prod_grid.hasClass("d-none")) {
                    $btn_prod_grid.trigger("click");
                }
                else if ($btn_prod_list.length > 0 && !$btn_prod_list.hasClass("d-none")) {
                    $btn_prod_list.trigger("click");
                }
                else if ($btn_text.length > 0 && !$btn_text.hasClass("d-none")) {
                    $btn_text.trigger("click");
                }
            }

            var visible_btns = 0;
            for (const [config, $btn] of Object.entries($btns)) {
                if ($btn.length === 0 || $btn.hasClass("d-none")) visible_btns += 1;
            }

            if (visible_btns >= 4) $self.find(".switch_control").addClass("d-none");
            else $self.find(".switch_control").removeClass("d-none");
        }
        $self.data("isInit", true);
    })
}

function updateStyle($self, configuration) {
    for (const [_, config] of Object.entries(display_configurations)) {
        $self.removeClass(config[0]);
        for (const [selector, classes] of Object.entries(config[1])) {
            $self.find(selector).removeClass(classes);
        }
    }
    $self.addClass(configuration[0]);
    for (const [selector, classes] of Object.entries(configuration[1])) {
        $self.find(selector).addClass(classes);
    }
}
function typeChange($self, $btns, $content, type) {
    for (const [config, $btn] of Object.entries($btns)) {
        $btn.data("activate", 0);
        $btn.addClass("text-black-50");
    }

    $self.data("activate", 1);
    $self.removeClass("text-black-50");

    $content.each(function () {
        var $self = $(this)
        updateStyle($self, display_configurations[type])
    })
}