function HeaderInit() {
    if ($("#btnMenu.change-color").length) {
        $('#btnMenu').on('click', function () {
            $(this).find('#menuIcon').toggleClass('text-white');
        });
    }
    if (($('.navbar').hasClass('position-fixed') || $('.navbar').hasClass('fixed-top')) && !$(".full-banner").length > 0) {
        var mega_menu_height = $("nav").css("height");
        $("body").css("padding-top", mega_menu_height);
        $(window).resize(function () {
            var mega_menu_height = $("nav").css("height");
            $("body").css("padding-top", mega_menu_height);
            MenuLiSize();
        });
        /*setTimeout(function () {
            if ($('.full-banner').length>0 && (window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop) == 0) {
                // 頁面加載完畢後滾動到目標元素
                $('html,body').animate({
                    scrollTop: $('.full-banner').offset().top - 78  // 滾動到目標元素的頂部
                }, 'smooth');  // 'smooth' 也可以替換成毫秒值，例如 1000 毫秒
            }
        }, 500);*/
    }

    const showNav = document.querySelectorAll('.full-banner');
    if (showNav.length) {
        $("header").addClass("overlap-banner");
        function hoverOff() {
            $('header>nav').off('mouseleave');
            $('header>nav').off('mouseover');
        }
        function hoverOn() {
            $('header>nav').on('mouseover', () => {
                $('header>nav').removeClass('hide-menu');
            });
            $('header>nav').on('mouseleave', () => {
                $('header>nav').addClass('hide-menu');
            });
        }
        if ($('header>nav').hasClass('position-fixed')) {
            $('header>nav').addClass('hide-menu');
            hoverOn();
        }
        window.addEventListener('scroll', () => {
            let scrollPosition = window.scrollY;
            showNav.forEach(showNav => {
                // 使用 getBoundingClientRect 獲取區塊的位置
                const sectionTop = showNav.getBoundingClientRect().top + window.scrollY;
                const sectionHeight = showNav.offsetHeight;

                // 檢查當前滾動位置是否在這個區塊範圍內
                if (scrollPosition >= sectionTop && scrollPosition < sectionTop + sectionHeight) {
                    $('header>nav').removeClass('show-menu').addClass('hide-menu');
                    hoverOn();
                } else {
                    $('header>nav').addClass('show-menu').removeClass('hide-menu');
                    hoverOff();
                }
            });
        })
    }

    MenuLiSize();

    moveHiUserToMenu();

    if ($(window).width() > 767) {
        const Cart_Dropdown = document.getElementById('Cart_Dropdown_Parent')
        if (Cart_Dropdown != null) {
            Cart_Dropdown.addEventListener('shown.bs.dropdown', event => {
                $("#btn_car_dropdown > i").addClass("open");
            })
            Cart_Dropdown.addEventListener('hidden.bs.dropdown', event => {
                $("#btn_car_dropdown > i").removeClass("open");
            })
        }
    } else {
        $("#btn_car_dropdown").attr("data-bs-toggle", "");
        $("#btn_car_dropdown").on("click", function (even) {
            var newUrl = window.location.origin + `/${OrgName}/ShoppingCar`;
            window.location.href = newUrl;
        });
    }

    var $myOffcanvas = $("#Mega_Menu>.offcanvas");
    $myOffcanvas.on('hidden.bs.offcanvas', function () {
        $("#menuButton").addClass("collapsed");
    });

    $myOffcanvas.on('shown.bs.offcanvas', function () {
        $("#menuButton").removeClass("collapsed");
    });

    if ($('#News_Marquee > .news_box').length > 0) {
        $('#News_Marquee > .news_box').verticalLoop({
            delay: 3000,
            order: 'asc'
        });
    }

    /*
    $("#Offcanvas_Mega_Menu > ul > .title > .content > ul").each(function () {
        var $self = $(this);
        if ($self.children("li").length > 4) {
            $self.css("justify-content", "start")
        } else {
            $self.css("justify-content", "space-evenly")
        }
    });
    $("#Offcanvas_Mega_Menu > ul > .metaMenu > .content > ul > .subtitle ul").each(function () {
        var $self = $(this);
        if ($self.children("li").length >= 6) {
            var width = 100 / 6 * 2;
            var str_width = `${width}%`
            $self.parents(".subtitle").first().css("width", str_width);
            if ($self.children("li").length > 10) {
                var $parent = $self.prev("a").first();
                $self.children("li:eq(9)").html(`<a class="ps-2 nav-link text-black py-1 text-nowrap text-start fw-normal" href="${$parent.attr('href')}" title="${$parent.attr('title')}" target="${$parent.attr('target')}">更多...</a>`);
                $self.children("li:gt(9)").addClass("d-none")
            }
        }
        else {
            var width = 100 / 6;
            var str_width = `${width}%`
            $self.parents(".subtitle").first().css("width", str_width);
        }
    });*/

    $('#Mega_Menu .btn_menu').on("click", function () {
        var $self = $(this);
        if ($self.hasClass("show")) $self.removeClass("show");
        else $self.addClass("show");
    })
}
function moveHiUserToMenu() {
    const hiUser = document.getElementById('HiUser');
    const login = document.querySelector('.login');
    const hamburgerMenu = document.querySelector('.offcanvas-header');
    const iconBlock = document.querySelector('.icon-block');
    if (window.innerWidth <= 576) { // 如果屏幕宽度 <= 576px
        if (hiUser) {
            if (!hamburgerMenu.contains(hiUser)) { // 避免重复移动
                hamburgerMenu.appendChild(hiUser); // 移动 HiUser 到汉堡菜单
            }
        } else if (login) {
            if (!hamburgerMenu.contains(login)) { // 避免重复移动
                hamburgerMenu.appendChild(login); // 移动 login 到汉堡菜单

                // 添加 "會員登入" 文本
                if (!login.querySelector('p')) { // 避免重复添加文本
                    const loginText = document.createElement('p');
                    loginText.textContent = '會員登入';
                    login.appendChild(loginText);
                }
            }
        }
    } else {
        // 否则, 移回原来的位置
        if ($('#HiUser').length > 0) {
            if (!iconBlock.contains(hiUser)) { // 避免重复移动
                iconBlock.appendChild(hiUser); // 移动 HiUser 回原来的位置
            }
        } else if (login) {
            if (!iconBlock.contains(login)) { // 避免重复移动
                iconBlock.appendChild(login); // 移动 login 回原来的位置
            }
        }
    }
}
function MenuLiSize() {
    if ($(window).width() > 768) {
        $(".subtitle").removeClass("w-100")
        $(".subtitle li").removeClass("w-100")
    } else {
        $(".subtitle").addClass("w-100")
        $(".subtitle li").addClass("w-100")
    }
    if ($(window).width() > 992) {
        $(".offcanvas-body").removeClass("accordion")
        $(".offcanvas-body .collapse").addClass("show")
        $(`.offcanvas-body  .nav-link[data-bs-toggle]`).attr("data-bs-toggle", "");
    } else {
        $(".offcanvas-body").addClass("accordion")
        $(".offcanvas-body .collapse").removeClass("show")
        $(`.offcanvas-body  .nav-link[data-bs-toggle]`).each((i, item) => {
            if ($(item).next("ul").length > 0)
                $(item).attr("data-bs-toggle", "collapse");
            else
                $(item).find(".material-symbols-outlined").remove();
        });
    }
    $("#Offcanvas_Mega_Menu a").each(function () {
        const $item = $(this);
        if ($item.attr("href").match(PageKey) != null)
            $item.parents(".collapse").addClass("show");
    });
}
