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
});

const display_configurations = {
    [list_type.list]: { ".box-shadow": "p-1 type3",
        ".card-border": "card-border-rd type3",
        ".check_btn":"check_btn-type3",
        ".description": "type3-content",
        ".image":"px-0",
        ".image_frame":"h-100",
        ".imgh":"img-h",
        ".max-h":"max-hei",
        ".mergetag":"merge",
        ".more-btn":"d-none",
        ".title":"type1-title fw-bold p-2",
        "figcaption":"flex-grow-1 p-3 py-0 py-md-3 pb1",
    },
    [list_type.grid]: {
        ".box-shadow": "p-1 type2",
        ".card-border": "card-border-rd type2",
        ".check_btn": "w-100",
        ".date": "d-none",
        ".description": "d-none type2-content",
        ".image_frame": "w-100",
        ".more": "d-none",
        ".more-btn": "d-none",
        ".title": "type1-title fw-bold p-2",
        "figcaption": "pb1",
        "figure": "flex-column",
    },
    [list_type.text]: {
        ".box-shadow": "p-1 type1 text-border",
        ".card-border": "pr3 type1",
        ".check_btn": "w-100",
        ".date": "d-none",
        ".description": "d-none",
        ".image": "d-none",
        ".image_frame": "d-none",
        ".more": "d-none",
        ".related-tag": "d-none",
        ".search-more": "d-none",
        ".title": "type2-title pr3 h-100",
        "figure": "flex-column",
    },
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
                    typeChange($btn_grid, $btn_list, $btn_text, $content, list_type.grid);
                }
            })

            $btn_list.on("click", function () {
                if (!$btn_list.data("activate")) {
                    typeChange($btn_list, $btn_grid,  $btn_text , $content, list_type.list);
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
        for (const [selector, classes] of Object.entries(config)) {
            $self.find(selector).removeClass(classes);
        }
    }
    for (const [selector, classes] of Object.entries(configuration)) {
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
    switch (type) {
        case list_type.list:
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type3").removeClass("type1 type2");
                $self.removeClass("row row-cols-sm-4");
                /*$self.removeClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");*/
                updateStyle($self, display_configurations[list_type.list])
                
            })
            break;
        case list_type.grid:
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type2").removeClass("type1 type3");
                $self.addClass("row row-cols-sm-4");
                /* $self.addClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");*/
                updateStyle($self, display_configurations[list_type.grid])
            });
            break;
        case list_type.text:
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type1").removeClass("type2 type3");
                $self.addClass("row row-cols-sm-4");
                updateStyle($self, display_configurations[list_type.text])
            });
            break;
    }
}