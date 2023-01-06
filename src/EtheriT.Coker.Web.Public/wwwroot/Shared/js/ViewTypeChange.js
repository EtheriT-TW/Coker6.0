function ViewTypeChangeInit() {
    $(".frame").each(function () {
        if (!!!$self.data("isInit")) {
            var $self = $(this)
            if ($self.data("action") == "TypeChange") {
                const $btn_grid = $self.find(".btn_grid");
                const $btn_list = $self.find(".btn_list");
                const $content = $self.find(".row").first();

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
        }
        $self.data("isInit", true);
    })
}

function typeChange($self, $brother, $content, type) {
    $self.data("activate", 1);
    $self.removeClass("text-black-50");
    $brother.data("activate", 0);
    $brother.addClass("text-black-50");
    if (type == "List") {
        $content.each(function () {
            var $self = $(this)
            $self.removeClass("row row-cols-2 row-cols-sm-4");
            $self.find("figure").removeClass("flex-column");
            $self.find("figcaption").addClass("flex-grow-1 p-3");
            $self.find(".title").removeClass("text-center");
            $self.find(".text").removeClass("d-none");
        })
    } else if (type == "Grid") {
        $content.each(function () {
            var $self = $(this)
            $self.addClass("row row-cols-2 row-cols-sm-4");
            $self.find("figure").addClass("flex-column");
            $self.find("figcaption").removeClass("flex-grow-1 p-3");
            $self.find(".title").addClass("text-center");
            $self.find(".text").addClass("d-none");

        })
    }
}