var $btn_expand, $btn_sear_text, $btn_sear_firm, $btn_sear_all;

var $modal_title, sear_text = "", $modal_body;

var $sear_modal, $sear_target, $input_sear;

function HeaderInit() {
    ElementInit();

    MenuLiSize();
    $(window).resize(function () {
        MenuLiSize();
    });

    $btn_sear_text.on("click", function () {
        $input_sear.focus();
    })

    $btn_sear_firm.on("click", function () {
        $sear_target.text($btn_sear_firm.text());
        $input_sear.attr("placeholder", "請輸入廠商名稱或技術...");
    })

    $btn_sear_all.on("click", function () {
        $sear_target.text($btn_sear_all.text());
        $input_sear.attr("placeholder", "請輸入想尋找的關鍵字...");
    })
}

function MenuLiSize() {
    if ($(window).width() > 768) {
        $(".navbar-nav > .dropdown").hover(
            function () {
                var $selfa = $(this).children(".dropdown-toggle");
                $selfa.dropdown("show");
            },
            function () {
                var $selfa = $(this).children(".dropdown-toggle");
                $selfa.dropdown("hide");
            });
    } else {
        $btn_expand.on("click", function () {
            if ($btn_expand.hasClass("collapsed")) {
                $btn_expand.children("span").text("expand_more");
            } else {
                $btn_expand.children("span").text("close");
            }
        })
    }
}
function ElementInit() {
    $btn_expand = $(".btn_expand");
    $btn_sear_text = $(".btn_sear_text");
    $btn_sear_firm = $(".btn_sear_firm");
    $btn_sear_all = $(".btn_sear_all");

    $modal_title = $(".modal-title");
    $modal_body = $(".modal-body");

    $sear_modal = $(".sear_modal")
    $sear_target = $(".sear_target");

    $input_sear = $(".input_sear");
}