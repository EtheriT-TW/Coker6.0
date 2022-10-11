function HeaderInit() {
    $(".btn_cart_delete").on("click", CartDelete);

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

function CartDelete() {
    var $self = $(this);
    var $cart_pro = $self.parents("li").first();
    $cart_pro.remove();
}