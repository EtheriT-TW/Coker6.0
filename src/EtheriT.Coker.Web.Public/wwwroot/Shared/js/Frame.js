function FrameInit() {
    $(".masonry").each(function () {
        const $grid = $(this).find(".grid")
        var grid = $grid.masonry({
            itemSelector: '.grid-item',
            columnWidth: '.grid-item'
        });
    })
}