function ViewTypeChangeInit() {
    $(".type_change_frame").each(function () {
        var $self = $(this)
        if (!!!$self.data("isInit")) {
            const $btn_grid = $self.find(".btn_grid");
            const $btn_list = $self.find(".btn_list");
            const $content = $self.find(".content").first();

            $btn_grid.on("click", function () {
                if (!$btn_grid.data("activate")) {
                    typeChange($btn_grid, $btn_list, $content, "Grid");
                }
            })

            $btn_list.on("click", function () {
                if (!$btn_list.data("activate")) {
                    typeChange($btn_list, $btn_grid, $content, "List");
                }
            })
        }
        $self.data("isInit", true);
    })
}

function typeChange($self, $brother, $content, type) {
    $self.data("activate", 1);
    $self.removeClass("text-black-50");
    $brother.data("activate", 0);
    $brother.addClass("text-black-50");
    switch (type) {
        case "List":
            $content.each(function () {
                var $self = $(this)
                $self.removeClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");
                $self.find("figure").removeClass("flex-column");
                $self.find(".image_frame").removeClass("w-100");
                $self.find(".image").addClass("px-0");
                $self.find("figcaption").addClass("flex-grow-1 p-3 py-0 py-md-3");
                $self.find(".title").removeClass("text-center");
                $self.find(".title").addClass("h-100");
                $self.find(".description").removeClass("d-none");
            })
            break;
        case "Grid":
            $content.each(function () {
                var $self = $(this)
                $self.addClass("row row-cols-2 row-cols-sm-4 row-cols-lg-6");
                $self.find("figure").addClass("flex-column");
                $self.find(".image_frame").addClass("w-100");
                $self.find(".image").removeClass("px-0");
                $self.find("figcaption").removeClass("flex-grow-1 p-3 py-0 py-md-3");
                $self.find(".title").addClass("text-center");
                $self.find(".title").removeClass("h-100");
                $self.find(".description").addClass("d-none");
            });
            break;
        case "Text":
            break;
    }
}