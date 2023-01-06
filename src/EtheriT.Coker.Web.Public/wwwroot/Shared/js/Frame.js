function FrameInit() {
    $(".masonry").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            const $grid = $(this).find(".grid")
            var grid = $grid.masonry({
                itemSelector: '.grid-item',
                columnWidth: '.grid-item'
            });
            $self.data("isInit", true);
        }
    })
}