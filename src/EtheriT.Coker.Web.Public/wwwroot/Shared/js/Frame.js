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
        if (typeof ($self.data("isinit")) == "undefined") {
            // data-link / data-yt-title 是新版標準；舊屬性只保留過渡讀取，
            // 並立即正規化目前 DOM，避免其他程式繼續依賴 legacy attribute。
            var legacyLink = $self.attr("link");
            var videoLink = $self.attr("data-link") || legacyLink;
            var legacyTitle = $self.attr("yttitle");
            var videoTitle = $self.attr("data-yt-title") || legacyTitle;

            if (!videoLink) {
                $self.attr("data-isInit", true);
                return;
            }

            if (!$self.attr("data-link")) $self.attr("data-link", videoLink);
            if (!$self.attr("data-yt-title") && videoTitle) $self.attr("data-yt-title", videoTitle);
            $self.removeAttr("link yttitle");

            var vid = "";

            if (videoLink.includes("v=")) {
                var link = videoLink.substring("https://www.youtube.com/watch?".length);
                var vIndex = link.indexOf("v=") + 2;
                var ampersandIndex = link.indexOf("&", vIndex);
                vid = ampersandIndex >= 0 ? link.substring(vIndex, ampersandIndex) : link.substring(vIndex);
            } else {
                var link = videoLink.substring("https://www.youtube.com/embed/".length);
                var tagindex = link.indexOf("?");
                vid = tagindex >= 0 ? link.substring(0, tagindex) : link.substring(vIndex);
            }

            $self.attr("vid", vid);
            if (typeof ($self.attr("vid")) != "undefined") {
                vid = $self.attr("vid");
            } else {
                vid = videoLink.substring(videoLink.indexOf('v=') + 2);
                $self.attr("vid", vid);
            }
            if (videoLink.includes("youtu") && ($self.find("img").attr("src").startsWith("/images/") || $self.find("img").attr("src").startsWith("data:"))) {
                var img_link = `https://img.youtube.com/vi/${vid}/hqdefault.jpg`;
                $self.find("img").attr("src", img_link)
            }
            if (videoTitle) {
                $self.find("img").attr("alt", videoTitle)
            }
            $self.find("button").attr({ "data-bs-toggle": "modal", "data-bs-target": "#YTPreviewModal" })
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
                var temp_ytlink = "";
                if (!videoLink.includes("autoplay")) {
                    if (videoLink.includes("v=")) {
                        // 保留原有 embed link 取得方法
                        temp_ytlink = `https://www.youtube-nocookie.com/embed/${$self.attr("vid")}?&autoplay=1`;
                    } else {
                        temp_ytlink = `${videoLink}?&autoplay=1`;
                    }
                } else temp_ytlink = videoLink;
                console.log(temp_ytlink)
                $("#YTPreviewModal").find(".modal-body").append(`<iframe src="${temp_ytlink}" class="w-100 h-100" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>`)
            })
            $self.attr("data-isInit", true);
        }
    })
}
