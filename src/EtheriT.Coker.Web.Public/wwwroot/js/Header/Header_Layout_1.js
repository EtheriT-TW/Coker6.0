function HeaderInit() {
    if ($("#btnMenu.change-color").length) {
        $('#btnMenu').on('click', function () {
            $(this).find('#menuIcon').toggleClass('text-white');
        });
    }
    if (($('.navbar').hasClass('position-fixed') || $('.navbar').hasClass('fixed-top')) && !$(".full-banner").length > 0) {
        const syncFixedHeaderOffset = function () {
            $("body").css("padding-top", $("header > nav").first().css("height"));
        };

        syncFixedHeaderOffset();
        $(window).off("resize.fixedHeaderOffset").on("resize.fixedHeaderOffset", syncFixedHeaderOffset);
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
    $(window).off("resize.layout1Menu").on("resize.layout1Menu", MenuLiSize);

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

    // 加入購物車成功後自動打開購物車抽屜（只有把 #Car_Dropdown 改成 offcanvas
    // 的版型會作用，其餘版型維持原本的下拉，這裡直接 return）
    $(document)
        .off("productQuickCart:added.cartDrawer")
        .on("productQuickCart:added.cartDrawer", function () {
            var modalEl = document.getElementById("ShoppingCarModal");

            // 規格 modal 沒開著就直接開抽屜
            if (!modalEl || !modalEl.classList.contains("show")) {
                OpenCartDrawer();
                return;
            }

            // 等 modal 關完再開，避免它收尾時把 offcanvas 需要的捲動鎖一起清掉。
            // 但 hidden.bs.modal 不保證一定送得出來，所以加一道逾時保險，
            // 兩邊只會執行一次。
            var done = false;
            var run = function () {
                if (done) return;
                done = true;
                OpenCartDrawer();
            };

            $(modalEl).one("hidden.bs.modal", run);
            setTimeout(run, 600);
        });

}

function OpenCartDrawer() {
    var panel = document.getElementById("Car_Dropdown");
    if (!panel || !panel.classList.contains("offcanvas")) return;
    bootstrap.Offcanvas.getOrCreateInstance(panel).show();
}

function moveHiUserToMenu() {
    if (document.getElementById('MenuHiUser')) return;
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
    syncResponsiveHeaderMenu();
    $("#Offcanvas_Mega_Menu a").each(function () {
        const $item = $(this);
        if ($item.attr("href").match(PageKey) != null)
            $item.parents(".collapse").addClass("show");
    });
}
