function PageReady() {

    var $grid = $('.grid').masonry({
        itemSelector: '.grid-item',
        columnWidth: '.grid-item'
    });

    $(".grid-item > .content").on("click", function () {
        console.log($(this))
        console.log("Be Click")
    })
}
