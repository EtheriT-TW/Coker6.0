function AnchorPointInit() {

    if ($(".anchor_point").hasClass("anchor_inpage")) {
        $(".anchor_point").each(function () {
            var $self = $(this);
            var anchorId = $self.data("anchorid").split(',');
            if (anchorId.length > 0) {
                var anchorName = [];
                anchorId.forEach(id => {
                    var item = $(id);
                    var name = item.text().substr(0, item.text().indexOf("\n"));
                    while (name == "") {
                        item = $(id).children().first();
                        name = item.text().substr(0, item.text().indexOf("\n"));
                    };
                    anchorName.push(name);
                });
                for (var i = 0; i < anchorId.length; i++) {
                    $self.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="${anchorId[i]}"><div class="p-2 px-4">${anchorName[i]}</div></a></li>`)
                }

                $self.children('a[href^="#"]').click(function (event) {
                    var id = $(this).attr("href");
                    console.log(id)
                    var target = $(id).offset().top - 50;
                    $('html, body').animate({ scrollTop: target }, 0);
                    event.preventDefault();
                });
            }
        })
    } else {
        console.log("OutPage")
    }
}