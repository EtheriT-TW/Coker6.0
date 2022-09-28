function HeaderInit() {
    var $myOffcanvas = $("#Mega_Menu>.offcanvas");
    $myOffcanvas.on('hidden.bs.offcanvas', function () {
        $("#menuButton").addClass("collapsed");
    });
    $myOffcanvas.on('shown.bs.offcanvas', function () {
        console.log($("#menuButton"));
        $("#menuButton").removeClass("collapsed");
    });
    setInterval(function () {
        $('.news_box li:first-child').slideUp(function () {
            $(this).appendTo($('.news_box')).slideDown()
        })
    }, 3000)
}