var $btn_expand, $btn_sear_all, $btn_sear_news;

var $modal_title, sear_text = "", $modal_body;

var $input_sear;

var $header, $img_ul;


function HeaderInit() {
    ElementInit();

    $btn_expand.on("click", function () {
        if ($btn_expand.hasClass("collapsed")) {
            $btn_expand.children("span").text("expand_more");
        } else {
            $btn_expand.children("span").text("close");
        }
    })
}

function ElementInit() {
    $btn_expand = $(".btn_expand");
    $btn_sear_all = $(".btn_sear_all");
    $btn_sear_news = $(".btn_sear_news");

    $header = $(".header");
    $img_ul = $(".img_ul");

    $modal_title = $(".modal-title");
    $modal_body = $(".modal-body");

    $input_sear = $(".input_sear");
}