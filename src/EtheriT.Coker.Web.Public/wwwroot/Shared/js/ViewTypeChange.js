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
});

const display_configurations = {
    [list_type.list]: ["type1 bg-white", {
        ".card-border": "card-border-rd type3",
        ".catalog-number":"d-none",
        ".check_btn":"check_btn-type3",
        ".col": "p-1 type3 box-shadow",
        ".description": "type3-content",
        ".image":"px-0",
        ".image_frame":"h-100",
        ".imgh":"img-h",
        ".item-header": "d-none",
        ".like-and-share": "d-none",
        ".max-h":"max-hei",
        ".mergetag":"merge",
        ".more-btn": "d-none",
        ".purchase": "d-none",
        ".related-tag": "d-none",
        ".tags": "d-none",
        ".title":"type1-title fw-bold p-2",
        "figcaption":"flex-grow-1 p-3 py-0 py-md-3 pb1",
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
        ".purchase": "d-none",
        ".related-tag": "d-none",
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
        ".purchase": "d-none",
        ".related-tag": "d-none",
        ".search-more": "d-none",
        ".tags": "d-none",
        ".title": "type2-title pr3 h-100",
        "figure": "flex-column",
    }],
    [list_type.product_grid]: ["type4 row row-cols-lg-4 bg-light px-2", {
        ".card-border": "card-border-rd type2",
        ".catalog-number": "type4-title d-inline",
        ".check_btn": "w-100",
        ".col": "p-1 type4 rounded-lg h-100",
        ".date": "d-none",
        ".description": "d-none type2-content",
        ".image_frame": "type4-image-frame w-100",
        ".item-header": "d-flex",
        ".like-and-share": "d-none",
        ".more": "d-none",
        ".more-btn": "d-none",
        ".purchase":"d-none",
        ".tags": "mt-auto align-text-bottom",
        ".title": "type4-title d-inline fs-6",
        "figcaption": "pb1 type4-caption d-flex flex-column",
        "figure": "flex-column",
    }],
    [list_type.product_list]: ["type1 bg-light px-2", {
        ".card-border": "card-border-rd type2",
        ".catalog-number": "type4-title d-inline",
        ".check_btn": "check_btn-type3",
        ".col": " type5 rounded-lg",
        ".description": "d-none",
        ".image": "px-0",
        ".image_frame": "h-100 w-25",
        ".imgh": "img-h",
        ".item-header": "d-none",
        ".item-title": "d-flex",
        ".like-and-share":"d-inline ms-auto fs-5 p-2",
        ".max-h": "max-hei",
        ".mergetag": "merge",
        ".more":"d-none",
        ".more-btn": "d-none",
        ".purchase": "ms-auto d-inline",
        ".related-tag": "d-none",
        ".tags": "mt-auto mb-2 align-text-bottom d-flex",
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
            const $content = $self.find(".content").first();

            $btn_grid.on("click", function () {
                if (!$btn_grid.data("activate")) {
                    typeChange($btn_grid, $btn_list, $btn_text, $content, list_type.product_grid);
                }
            })

            $btn_list.on("click", function () {
                if (!$btn_list.data("activate")) {
                    typeChange($btn_list, $btn_grid,  $btn_text , $content, list_type.product_list);
                }
            })

            $btn_text.on("click", function () {
                if (!$btn_text.data("activate")) {
                    typeChange($btn_text ,$btn_list, $btn_grid, $content, list_type.text);
                }
            })
            if ($btn_grid.hasClass("d-none") && !$btn_list.hasClass("d-none")) $btn_list.trigger("click");
            if ($btn_grid.hasClass("d-none") && $btn_list.hasClass("d-none") && !$btn_text.hasClass("d-none")) $btn_text.trigger("click");
            if ($self.find(".btn_grid.d-none,.btn_list.d-none,.btn_text.d-none").length >= 2) $self.find(".switch_control").addClass("d-none");
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
function typeChange($self, $brother ,$brother2, $content, type) {
    $self.data("activate", 1);
    $self.removeClass("text-black-50");
    $brother.data("activate", 0);
    $brother.addClass("text-black-50");
    $brother2.data("activate", 0);
    $brother2.addClass("text-black-50");
    $content.each(function () {
        var $self = $(this)
        updateStyle($self, display_configurations[type])
    })
}