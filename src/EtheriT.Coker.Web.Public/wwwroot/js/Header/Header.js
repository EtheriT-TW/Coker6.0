function HeaderInit() {
    var $myOffcanvas = $("#Mega_Menu>.offcanvas");
    $myOffcanvas.on('hidden.bs.offcanvas', function () {
        $("#menuButton").addClass("collapsed");
    });

    $myOffcanvas.on('shown.bs.offcanvas', function () {
        console.log($("#menuButton"));
        $("#menuButton").removeClass("collapsed");
    });

    $('.news_box').verticalLoop({
        delay: 3000,
        order: 'asc'
    });
}