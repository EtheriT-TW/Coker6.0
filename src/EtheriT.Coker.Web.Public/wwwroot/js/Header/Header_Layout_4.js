var $btn_expand, $btn_sear_all, $btn_sear_news;

var $modal_title, sear_text = "", $modal_body;

var $sear_modal, $input_sear;

var $header, $img_ul;


function HeaderInit() {
    console.log("Layout4")

    ElementInit();

    if ($(window).width() <= 768) {
        $img_ul.css("top", $header.height() - 26);
    } else if ($(window).width() <= 576) {
        $img_ul.css("top", $header.height() - 20);
    } else {
        $img_ul.css("top", "87px");
    }

    $(window).resize(function () {
        if ($(window).width() <= 768) {
            $img_ul.css("top", $header.height() - 26);
        } else if ($(window).width() <= 576) {
            $img_ul.css("top", $header.height() - 20);
        } else {
            $img_ul.css("top", "87px");
        }
    });

    $btn_expand.on("click", function () {
        if ($btn_expand.hasClass("collapsed")) {
            $btn_expand.children("span").text("expand_more");
            $img_ul.children("li").css("height", 0);
        } else {
            $btn_expand.children("span").text("close");
            $img_ul.children("li").css("height", "145px");
        }
    })

    $btn_sear_news.on("click", Search)

    $btn_sear_all.on("click", Search)

}

function ElementInit() {
    $btn_expand = $(".btn_expand");
    $btn_sear_all = $(".btn_sear_all");
    $btn_sear_news = $(".btn_sear_news");

    $header = $(".header");
    $img_ul = $(".img_ul");

    $modal_title = $(".modal-title");
    $modal_body = $(".modal-body");

    $sear_modal = $(".sear_modal")

    $input_sear = $(".input_sear");
}

function Search() {
    var sear_target = $(this).text();
    if ($input_sear.val() == "") {
        Coker.sweet.error("錯誤", "請輸入搜尋字元", null, false);
    } else {
        $modal_title.text();
        $sear_modal.modal("show");
        sear_text = $input_sear.val();
        $modal_title.text(`搜尋-${sear_target}`)
        $modal_body.text(`搜尋值：${sear_text}`)
    }
}