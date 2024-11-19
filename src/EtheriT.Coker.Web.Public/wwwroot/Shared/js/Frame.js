function FrameInit() {
    $(".masonry").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            const $grid = $(this).find(".grid")
            var grid = $grid.masonry({
                itemSelector: '.grid-item',
                columnWidth: '.grid-item'
            });
            $self.data("isInit", true);
        }
    })
    $(".YTmodal_frame").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("link")) != "undefined" && $self.attr("link") != "") {
                var vid = $self.attr('link').substring($self.attr("link").indexOf('v=') + 2);
                $self.attr("vid", vid);
                var img_link = `http://img.youtube.com/vi/${vid}/sddefault.jpg`;
                $self.find("img").attr("src", img_link)
            }
            if (typeof ($self.attr("yttitle")) != "undefined" && $self.attr("yttitle") != "") {
                $self.find("img").attr("alt", $self.attr("yttitle"))
            }
            if ($("body").find("#YTPreviewModal").length == 0) {
                var html = `<div class="modal fade" id="YTPreviewModal" tabindex="-1" aria-labelledby="YTPreviewModal" aria-hidden="true">
                                                  <div class="modal-dialog modal-xl">
                                                    <div class="modal-content bg-black">
                                                        <div class="modal-header">
                                                        <button type="button" data-bs-dismiss="modal" aria-label="Close" class="bg-light btn-close rounded-circle"</button>
                                                        </div>
                                                        <div class="modal-body"></div>
                                                    </div>
                                                  </div>
                                                </div>`
                $("body").prepend(html);

                document.getElementById('YTPreviewModal').addEventListener('hidden.bs.modal', function (event) {
                    $("#YTPreviewModal").find(".modal-body").empty();
                })
            }
            $self.find("button").on("click", function () {
                var temp_ytlink = "https://www.youtube-nocookie.com/embed/" + $self.attr("vid");
                $("#YTPreviewModal").find(".modal-body").append(`<iframe src="${temp_ytlink}" class="w-100 h-100" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>`)
            })
            $self.data("isInit", true);
        }
    })
}