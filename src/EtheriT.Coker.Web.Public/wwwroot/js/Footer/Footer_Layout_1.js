var menu_width

function FooterInit() {
    $menu_item = $("#Footer_Menu > ul > li > ul")
    $content = $("#Footer_Menu > ul > li > ul > li")
    MenuItemResize();
    toggleFooterMenu();
    $("#Collapse_Button").on("click", toggleFooterMenu);
    observeHtmlLangChange(() => {
        toggleFooterMenu();
        toggleFooterMenu();
    });
    $(window).on("resize",function () {
        MenuItemResize();
    })
}

function observeHtmlLangChange(callback) {
    const htmlElement = document.documentElement; // <html> 標籤

    const observer = new MutationObserver((mutationsList) => {
        for (let mutation of mutationsList) {
            if (mutation.type === "attributes" && mutation.attributeName === "lang") {
                callback(); // 執行回調
                break;
            }
        }
    });

    observer.observe(htmlElement, {
        attributes: true, // 監聽屬性變化
        attributeFilter: ["lang"] // 僅監聽 lang 屬性
    });
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
    if (footerMenu != null) {
        if (footerMenu.style.height === '0px' || footerMenu.style.height === '') {
            footerMenu.style.height = `${footerMenu.scrollHeight}px`;
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