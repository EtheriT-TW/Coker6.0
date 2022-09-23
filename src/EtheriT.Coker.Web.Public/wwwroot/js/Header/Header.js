
function PageReady() {
    $(document).ready(function () {
        setInterval(function () {
            $('.news_box li:first-child').slideUp(function () {
                $(this).appendTo($('.news_box')).slideDown()
            })
        }, 3000)
    })
}

