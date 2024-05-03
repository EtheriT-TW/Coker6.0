function AnchorPointInit() {
    $(".anchor_directory:not(.selectList)").addClass("selectList").each(function () {
        var $directory = $(this);
        $directory.children("ul").empty();
        $(".anchor_title").each(function () {
            var $self = $(this);
            var text = $self.text().indexOf('\n') > -1 ? $self.text().replace(/\n/g,"") : $self.text();
            $directory.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="#${$self.attr("id")}"><div class="p-2 px-4">${text}</div></a></li>`);
            $self.on('DOMNodeInserted', function () {
                AnchorPointInit()
            });
        });
        $directory.children('a[href^="#"]').on("click",function (event) {
            var id = $(this).attr("href");
            var target = $(id).offset().top - 50;
            $('html, body').animate({ scrollTop: target }, 0);
            event.preventDefault();
        });
    });
    $(window).off("resize.selectList").on("resize.selectList", (event) => {
        $(".selectList").each((i, e) => {
            const $self = $(e);
            if ($self.find("select").length == 0 || $self.find("select").children().length == 1) {
                const $list = $self.find("a");
                const $select = $("<select>");
                $self.find("select").remove();
                $select.append($("<option>").val("").text("請選擇將前往的標籤"))
                $list.each((j, a) => {
                    $select.append(
                        $("<option>").data("trigger", a).val(a.href).text($(a).text())
                    );
                });
                $select.on("change", function () {
                    const $o = $select.children(":selected");
                    if (/^#/.test($o.val())) $o.data("trigger").trigger("click");
                    else location.href = $o.val();
                });
                $self.append($select);
            }
            if ($(window).width() > 576) {
                $self.children().first().removeClass("d-none");
                $self.children("select").addClass("d-none");
            } else {
                $self.children().first().addClass("d-none");
                $self.children("select").removeClass("d-none");
            }
        });
    });
    $(window).trigger("resize.selectList");
}