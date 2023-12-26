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

function toggleFooterMenu() {
    var footerMenu = document.getElementById('Footer_Menu');
    if (window.innerWidth < 1000) {
        if (footerMenu.style.height === '0px' || footerMenu.style.height === '') {
            footerMenu.style.height = '400px';
        } else {
            footerMenu.style.height = '0px';
        }
    } else {
        if (footerMenu.style.height === '0px' || footerMenu.style.height === '') {
            footerMenu.style.height = '230px';
        } else {
            footerMenu.style.height = '0px';
        }
    }
}

function toggleIconSearchVisibility() {
    var iconSearch = document.getElementById('icon-search');
    var screenWidth = window.innerWidth;
    if (!!iconSearch) {
        if (screenWidth < 1000) {
            iconSearch.style.display = 'block';
        } else {
            iconSearch.style.display = 'none';
        }
    }
}
toggleIconSearchVisibility();
window.addEventListener('resize', toggleIconSearchVisibility);