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
                    typeChange($btn_grid, $btn_list, $btn_text, $content, "Grid");
                }
            })

            $btn_list.on("click", function () {
                if (!$btn_list.data("activate")) {
                    typeChange($btn_list, $btn_grid,  $btn_text , $content, "List");
                }
            })

            $btn_text.on("click", function () {
                if (!$btn_text.data("activate")) {
                    typeChange($btn_text ,$btn_list, $btn_grid, $content, "Text");
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

function typeChange($self, $brother ,$brother2, $content, type) {
    $self.data("activate", 1);
    $self.removeClass("text-black-50");
    $brother.data("activate", 0);
    $brother.addClass("text-black-50");
    $brother2.data("activate", 0);
    $brother2.addClass("text-black-50");
    switch (type) {
        case "List":
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type3").removeClass("type1 type2");
                $self.removeClass("row row-cols-sm-4");
                /*$self.removeClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");*/
                $self.find(".box-shadow").addClass("p-1").removeClass("p-3");
                $self.find(".box-shadow").addClass("type3").removeClass("type2 type1");
                $self.find(".box-shadow").removeClass("h_100");
                $self.find(".box-shadow").removeClass("text-border");
                $self.find(".card-border").addClass("card-border-rd");
                $self.find(".card-border").addClass("type3").removeClass("type2 type1");
                $self.find(".card-border").removeClass("h_100");    
                $self.find(".card-border").addClass("p-3").removeClass("pr3");
                $self.find(".check_btn").addClass("check_btn-type3").removeClass("w-100");
                $self.find(".date").removeClass("d-none");
                $self.find(".description").addClass("type3-content").removeClass("type2-content");
                $self.find(".description").removeClass("d-none");
                $self.find(".hover_protrude_scale").removeClass("text-border");
                $self.find(".image").addClass("px-0");
                $self.find(".image").removeClass("d-none");
                $self.find(".image_frame").addClass("h-100");
                $self.find(".image_frame").removeClass("d-none");
                $self.find(".image_frame").removeClass("w-100");
                $self.find(".imgh").addClass("img-h");
                $self.find(".max-h").addClass("max-hei");
                $self.find(".mergetag").addClass("merge");
                $self.find(".more").removeClass("d-none");
                $self.find(".more-btn").addClass("d-none");
                $self.find(".related-tag").removeClass("d-none");
                $self.find(".search-more").removeClass("d-none"); 
                $self.find(".title").addClass("type1-title fw-bold").removeClass("type2-title pr3 text-center h-100");
                $self.find("figcaption").addClass("flex-grow-1 p-3 py-0 py-md-3");
                $self.find("figcaption").addClass("pb1");
                $self.find("figure").removeClass("flex-column");
            })
            break;
        case "Grid":
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type2").removeClass("type1 type3");
                /* $self.addClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");*/
                $self.addClass("row row-cols-sm-4");
                $self.find(".box-shadow").addClass("p-1").removeClass("p-3");
                $self.find(".box-shadow").addClass("type2").removeClass("type1 type3");
                $self.find(".box-shadow").removeClass("h_100");
                $self.find(".box-shadow").removeClass("text-border");
                $self.find(".card-border").addClass("card-border-rd");
                $self.find(".card-border").addClass("type2").removeClass("type1 type3");
                $self.find(".card-border").removeClass("h_100");
                $self.find(".card-border").addClass("p-3").removeClass("pr3");
                $self.find(".check_btn").addClass("w-100").removeClass("check_btn-type3");
                $self.find(".date").addClass("d-none");
                $self.find(".description").addClass("d-none");
                $self.find(".description").addClass("type2-content").removeClass("type3-content");
                $self.find(".image").removeClass("d-none");
                $self.find(".image").removeClass("px-0");
                $self.find(".image_frame").addClass("w-100");
                $self.find(".image_frame").removeClass("d-none");
                $self.find(".image_frame").removeClass("h-100");
                $self.find(".imgh").removeClass("img-h");
                $self.find(".mergetag").removeClass("merge");
                $self.find(".more").addClass("d-none");
                $self.find(".more-btn").addClass("d-none");
                $self.find(".related-tag").removeClass("d-none");
                $self.find(".search-more").removeClass("d-none"); 
                $self.find(".title").addClass("type1-title fw-bold p-2").removeClass("h-100 text-center type2-title");
                $self.find(".title").removeClass("p-2 pr3");
                $self.find("figcaption").addClass("pb1");
                $self.find("figcaption").removeClass("flex-grow-1 p-3 py-0 py-md-3");
                $self.find("figure").addClass("flex-column");
            });
            break;
        case "Text":
            $content.each(function () {
                var $self = $(this)
                $self.addClass("type2").removeClass("type2 type3");
                $self.addClass("row row-cols-sm-4");
                $self.find(".box-shadow").addClass("p-1").removeClass("p-3");
                $self.find(".box-shadow").addClass("text-border");
                $self.find(".box-shadow").addClass("type1").removeClass("type2 type3");
                $self.find(".box-shadow").removeClass("h_100");
                $self.find(".card-border").addClass("type1").removeClass("type2 type3");
                $self.find(".card-border").removeClass("card-border-rd");
                $self.find(".card-border").removeClass("h_100");
                $self.find(".card-border").addClass("pr3").removeClass("p-3");
                $self.find(".check_btn").addClass("w-100").removeClass("check_btn-type3");
                $self.find(".date").addClass("d-none");
                $self.find(".description").addClass("d-none");
                $self.find(".image").addClass("d-none");
                $self.find(".image").removeClass("px-0");
                $self.find(".image_frame").addClass("d-none");
                $self.find(".image_frame").removeClass("h-100");
                $self.find(".image_frame").removeClass("w-100 ");
                $self.find(".imgh").removeClass("img-h");
                $self.find(".max-h").removeClass("max-hei");
                $self.find(".mergetag").removeClass("merge ");
                $self.find(".more").addClass("d-none");
                $self.find(".more-btn").removeClass("d-none");
                $self.find(".related-tag").addClass("d-none");
                $self.find(".search-more").addClass("d-none");
                $self.find(".title").addClass("pr3").removeClass("p-2");
                $self.find(".title").addClass("type2-title h-100").removeClass("type1-title fw-bold text-center p-2");
                $self.find("figcaption").removeClass("flex-grow-1 p-3 py-0 py-md-3");
                $self.find("figcaption").removeClass("pb1");
                $self.find("figure").addClass("flex-column");
            });
            break;
    }
}