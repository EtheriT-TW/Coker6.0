function SideFloatingInit() {
    $("#btn_chat").on("click", function () {
        $("#Chatbot_Frame").toggleClass("show");
    });

    $("#btn_gotop").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    });
}