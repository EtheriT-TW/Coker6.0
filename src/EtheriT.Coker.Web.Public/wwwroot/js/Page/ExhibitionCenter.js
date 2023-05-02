function PageReady() {
    var $grid = $('.grid').masonry({
        itemSelector: '.grid-item',
        columnWidth: '.grid-item'
    });

    $(".grid-item > .content").on("click", function () {
    })
}
