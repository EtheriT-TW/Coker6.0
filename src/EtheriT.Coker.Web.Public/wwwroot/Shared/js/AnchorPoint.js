function AnchorPointInit() {
    $(".anchor_directory").each(function () {
        var $directory = $(this);
        $directory.children("ul").empty();
        $(".anchor_title").each(function () {
            var $self = $(this);
            var text = $self.text().indexOf('\n') > -1 ? $self.text().substr(0, $self.text().indexOf('\n')) : $self.text();
            $directory.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="#${$self.attr("id")}"><div class="p-2 px-4">${text}</div></a></li>`);
            $self.bind('DOMNodeInserted', function () {
                AnchorPointInit()
            });
        });

        $directory.children('a[href^="#"]').click(function (event) {
            var id = $(this).attr("href");
            var target = $(id).offset().top - 50;
            $('html, body').animate({ scrollTop: target }, 0);
            event.preventDefault();
        });
    });
}