function HeaderInit() {
    var backgroundimgurl = "";

    if ($('body').hasClass('home')) {
        $(".titleimage").removeClass("d-none");
        $(".headernews").removeClass("d-none");
        $("link").each(function (index) {
            if (typeof ($(this).attr("data-orgname")) == "string") {
                backgroundimgurl = `url("/upload/${$(this).attr("data-orgname")}/backgroundtop.jpg")`;
                $("body").css("background-image", backgroundimgurl);
            }
        });
    }
}

function ElementInit() {
}