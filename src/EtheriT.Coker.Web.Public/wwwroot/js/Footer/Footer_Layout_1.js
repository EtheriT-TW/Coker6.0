var menu_width

function FooterInit() {
    $menu_item = $("#Footer_Menu > ul > li > ul")
    $content = $("#Footer_Menu > ul > li > ul > li")
    MenuItemResize();

    $(window).resize(function () {
        MenuItemResize();
    })
}

function MenuItemResize() {
    menu_width = $("#Footer_Menu").width()

    $menu_item.each(function () {
        var $self = $(this)
        if ($self.children("li").length > 7) {
        } else {
        }
    })
}
