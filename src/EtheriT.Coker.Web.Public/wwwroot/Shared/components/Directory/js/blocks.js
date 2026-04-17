(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryBlocks = (w.DirectoryBlocks = w.DirectoryBlocks || {});

    function isFn(fn) {
        return typeof fn === "function";
    }

    function isNullOrEmpty(value) {
        return value === null || value === undefined || value === "";
    }

    function getLinkTitle(title, blank) {
        if (isFn(w.cokerI18n)) {
            return blank
                ? w.cokerI18n("LinkToAndBlank", title)
                : w.cokerI18n("LinkTo", title);
        }
        return title || "";
    }

    function renderMenuAccordionItem(groupId, secItem) {
        const item = $($("#TemplateAccordionItem").html()).clone();

        item.find(".sectitle").text(secItem.title);
        item.find(".accordion-header").attr("id", `secheader${secItem.id}`);

        const accordionCollapse = item.find(".accordion-collapse").attr({
            "aria-labelledby": `secheader${secItem.id}`,
            "id": `seccollapse${secItem.id}`,
            "data-bs-parent": `${groupId}`
        });

        item.find(".accordion-button").attr({
            "data-bs-target": `#seccollapse${secItem.id}`,
            "aria-controls": `seccollapse${secItem.id}`
        });

        const $body = item.find(".accordion-body");

        $.each(secItem.children || [], function (index, thirdItem) {
            const linkTitle = getLinkTitle(thirdItem.title, false);
            const $a = $(`<a href="${thirdItem.routerName}" title="${linkTitle}" class="list-group-item list-group-item-action border-0 py-3">${thirdItem.title}</a>`);

            if (
                typeof w.PageKey !== "undefined" &&
                thirdItem.routerName &&
                w.PageKey.toLowerCase() === thirdItem.routerName.toLowerCase()
            ) {
                $a.addClass("active");
                $(accordionCollapse).collapse("show");
                item.find(".accordion-header").addClass("active");
            }

            $body.append($a);
        });

        return item;
    }

    function renderMenuSingleLink(secItem) {
        const link = (!isNullOrEmpty(secItem.routerName)) ? secItem.routerName : secItem.linkUrl;
        const linkTitle = getLinkTitle(secItem.title, false);

        const html = $(
            `<div class="accordion-item border-0 border-bottom px-1">
                <a href="${link}" title="${linkTitle}" class="list-group-item border-0 py-3 custom_h5 text-black">
                    <span>${secItem.title}<span>
                </a>
            </div>`
        );

        if (!isNullOrEmpty(secItem.imgUrl)) {
            const $img = $(`<img alt=" " />`);

            if (
                secItem.overImgUrl == null ||
                (
                    typeof w.PageKey !== "undefined" &&
                    link &&
                    link.toLowerCase().indexOf(w.PageKey.toLowerCase()) >= 0
                )
            ) {
                $img.attr("src", secItem.imgUrl);
            } else {
                $img.attr("src", secItem.overImgUrl);
            }

            html.find("a")
                .removeClass("py-3")
                .addClass("p-0 imgMenu")
                .append($img);
        }

        if (
            typeof w.PageKey !== "undefined" &&
            secItem.routerName &&
            w.PageKey.toLowerCase() === secItem.routerName.toLowerCase()
        ) {
            html.find("a").addClass("active");
        }

        return html;
    }

    DirectoryBlocks.renderMenu = function ($self, result) {
        if (!result) return;

        $self.find(".title").text(result.title || "");

        const $accordion = $self.find(".accordion");
        const groupId = `#${$accordion.attr("id")}`;

        $.each(result.children || [], function (index, secItem) {
            $self.find(".title").text(result.title || "");

            if (secItem.children != null) {
                const item = renderMenuAccordionItem(groupId, secItem);
                $accordion.append(item);
            } else {
                const html = renderMenuSingleLink(secItem);
                $accordion.append(html);
            }
        });

        if ($(".selectList").length > 0) {
            $(window).trigger("resize.selectList");
        }
    };

    DirectoryBlocks.renderAdvertise = function ($item, result) {
        if (!Array.isArray(result)) return;

        if ($item.find(".swiper").length > 0) {
            const $swiperWrapper = $item.find(".swiper-wrapper");

            for (let i = 0; i < result.length; i++) {
                const temp = $($swiperWrapper.find(".templatecontent").html()).clone();
                const rendered = DirectoryBlocks.insertAdvertiseData(temp, result[i]);
                $swiperWrapper.append(rendered);
            }
        } else {
            $item.find(".File_Frame").each(function (index) {
                const $frame = $(this);
                if (result.length > index) {
                    DirectoryBlocks.insertAdvertiseData($frame, result[index]);
                }
            });
        }
    };

    DirectoryBlocks.insertAdvertiseData = function ($frame, result) {
        const isFront = typeof w.OrgName !== "undefined";
        const resultFile = result && result.fileLink ? result.fileLink[0] : null;

        if (resultFile == null) return $frame;

        const filetype = resultFile.fileType;

        if (isFront || typeof $frame.data("init") === "undefined") {
            switch (parseInt(filetype, 10)) {
                case 1:
                    renderImageAdvertise($frame, result, resultFile);
                    break;
                case 2:
                    renderVideoAdvertise($frame, resultFile);
                    break;
                case 3:
                    renderYoutubeAdvertise($frame, result, resultFile);
                    break;
            }

            applyAdvertiseDescribe($frame, result);
            applyAdvertiseTags($frame, result);
            bindAdvertiseExposure($frame, result, isFront);
            bindAdvertiseVideoTracking($frame, result);
            bindAdvertiseClickTracking($frame, result);
            applyAdvertiseDetectLang($frame);

            $frame.data("init", true);
        }

        return $frame;
    };

    function renderImageAdvertise($frame, result, resultFile) {
        const $imgFrame = $frame.find(".img_frame");

        $imgFrame.find("img").attr("src", resultFile.link);
        $imgFrame.find("img").attr("alt", result.title || "");

        $imgFrame.find("a").attr({
            href: result.link,
            title: getLinkTitle(result.title, !!result.target),
            target: (result.target ? "_blank" : "_self"),
            rel: "noopener noreferrer"
        });

        if ($frame.find(".title").length > 0) {
            $frame.find(".title").text(result.title || "").removeClass("d-none");
        }

        $imgFrame.removeClass("d-none");
        $imgFrame.parent().children().not(".img_frame").not(".keep").remove();
    }

    function renderVideoAdvertise($frame, resultFile) {
        const $videoFrame = $frame.find(".video_frame");

        $videoFrame.find("video").attr("src", resultFile.link);
        $videoFrame.find("video").attr("type", resultFile.video_Type);
        $videoFrame.removeClass("d-none");
        $videoFrame.parent().children().not(".video_frame").remove();
    }

    function ensureYoutubeModal() {
        if ($("body").find("#YTPreviewModal").length > 0) return;

        const html = `
            <div class="modal fade" id="YTPreviewModal" tabindex="-1" aria-labelledby="YTPreviewModal" aria-hidden="true">
                <div class="modal-dialog modal-xl">
                    <div class="modal-content position-relative bg-black">
                        <div class="modal-header">
                            <button type="button" data-bs-dismiss="modal" aria-label="Close" class="bg-light btn-close rounded-circle"></button>
                        </div>
                        <div class="modal-body"></div>
                    </div>
                </div>
            </div>`;

        $("body").prepend(html);
        $("#YTPreviewModal").find(".modal-content").css("height", "90vh");

        document.getElementById("YTPreviewModal").addEventListener("hidden.bs.modal", function () {
            $("#YTPreviewModal").find(".modal-body").empty();
        });
    }

    function renderYoutubeAdvertise($frame, result, resultFile) {
        const $ytFrame = $frame.find(".YT_frame");

        $ytFrame.find("img").attr("src", "https://img.youtube.com/vi/" + resultFile.name + "/maxresdefault.jpg");
        $ytFrame.find("img").attr("alt", result.title || "");
        $ytFrame.removeClass("d-none");
        $ytFrame.parent().children().not(".YT_frame").remove();

        ensureYoutubeModal();

        $ytFrame.off("click.directoryYT").on("click.directoryYT", function () {
            const tempYtLink = `https://www.youtube-nocookie.com/embed/${resultFile.name}?&autoplay=1`;
            $("#YTPreviewModal").find(".modal-body").append(
                `<iframe src="${tempYtLink}" class="w-100 h-100" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>`
            );
        });
    }

    function applyAdvertiseDescribe($frame, result) {
        if (result.describe != null && $frame.find(".describe").length > 0) {
            let describe = "";
            const temp = String(result.describe).split("\n");

            temp.forEach(function (v, i) {
                if (i === 0) describe = v;
                else describe += `<br/>${v}`;
            });

            $frame.find(".describe").append(describe);
        }
    }

    function applyAdvertiseTags($frame, result) {
        if ($frame.find(".tag").length === 0) return;

        let tags = "";

        for (let i = 0; i < (result.tagDatas || []).length; i++) {
            const tagData = result.tagDatas[i];
            const taglink = typeof w.OrgName === "undefined"
                ? ""
                : `/${w.OrgName}/Search/Get/${tagData.searchId}/${tagData.title}`;

            tags += `<a href="${taglink}" title="${getLinkTitle(tagData.title, false)}" class="pe-2">#${tagData.title}</a>`;
        }

        $frame.find(".tag").append(tags);
    }

    function bindAdvertiseExposure($frame, result, isFront) {
        if (!isFront) return;
        if (!w.co || !co.Activity || !isFn(co.Activity.Exposure)) return;

        co.Activity.Exposure(result.id).done(function () {
            // keep silent
        });
    }

    function bindAdvertiseVideoTracking($frame, result) {
        const $video = $frame.find(".video_frame").find("video");

        $video.off("play.directoryAd ended.directoryAd");

        $video.on("play.directoryAd", function () {
            const $this = $(this);

            if (!$this.hasClass("playing")) {
                $this.addClass("playing");

                if (w.co && co.Activity && isFn(co.Activity.Click)) {
                    co.Activity.Click(result.id).done(function () {
                        // keep silent
                    });
                }
            }
        });

        $video.on("ended.directoryAd", function () {
            const $this = $(this);
            if ($this.hasClass("playing")) {
                $this.removeClass("playing");
            }
        });
    }

    function bindAdvertiseClickTracking($frame, result) {
        $frame.off("click.directoryAd").on("click.directoryAd", function () {
            if ($frame.find(".video_frame").length === 0) {
                if (w.co && co.Activity && isFn(co.Activity.Click)) {
                    co.Activity.Click(result.id).done(function () {
                        // keep silent
                    });
                }
            }
        });
    }

    function applyAdvertiseDetectLang($frame) {
        $frame.find(".detectLang").each(function () {
            const $text = $(this);
            const textHtml = $text.html();

            const cuttingText = textHtml.replace(/([a-zA-Z]+)/g, '<span class="english">$1</span>');
            $text.html(cuttingText);
        });
    }

})(window, window.jQuery);