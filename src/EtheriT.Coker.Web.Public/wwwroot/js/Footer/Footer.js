function FooterInit() {
    window.onscroll = function () {
        scrollFunction();
    };

    function scrollFunction() {
        if (document.body.scrollTop > 350 || document.documentElement.scrollTop > 350) {
            $(".btn_gotop").css('display', 'block');
        } else {
            $(".btn_gotop").css('display', 'none');
        }
    }
}
