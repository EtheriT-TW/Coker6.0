
function PageReady() {
    $(document).ready(function () {
        $("#btn_gonews").on("click", gonews);
    })
}

function gonews() {
    $('html, body').animate({ scrollTop: $("#News").offset().top }, 0);
}
