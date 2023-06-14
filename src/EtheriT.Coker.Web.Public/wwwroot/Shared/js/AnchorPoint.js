function AnchorPointInit() {
    console.log("AnchorPointInit")
    $(".anchor_point").each(function () {
        var $self = $(this);
        $self.children("ul").empty();
        console.log($self.data("anchorid"));
        if ($self.data("anchorid") != "" && typeof ($self.data("anchorid")) != "undefined") {
            var anchorId = $self.data("anchorid").split(',');
            if (anchorId.length > 0) {
                var anchorName = [];
                anchorId.forEach(id => {
                    var name = $(`#${id}`).text().substr(0, $(`#${id}`).text().indexOf("\n"));
                    anchorName.push(name);
                });
                for (var i = 0; i < anchorId.length; i++) {
                    $self.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="#${anchorId[i]}"><div class="p-2 px-4">${anchorName[i]}</div></a></li>`)
                }

                $self.children('a[href^="#"]').click(function (event) {
                    var id = $(this).attr("href");
                    console.log(id)
                    var target = $(id).offset().top - 50;
                    $('html, body').animate({ scrollTop: target }, 0);
                    event.preventDefault();
                });
            }
        }
    })
}