(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryRenderer = (w.DirectoryRenderer = w.DirectoryRenderer || {});

    function isFn(fn) {
        return typeof fn === "function";
    }

    function isNullOrEmpty(value) {
        return value === null || value === undefined || value === "";
    }

    function getCatalog($item) {
        return $item.find(".catalog");
    }

    function getTemplateHtml($item) {
        return $item.data("temp") || $item.find(".templatecontent").html() || "";
    }

    function normalizeImagePath(orgName, imagePath) {
        let imglink = (imagePath || "").trim() || "/images/noImg.jpg";

        if (imglink.includes("noImg.jpg")) {
            const fallback =
                $("meta[property='og:image']").attr("content") ||
                $(".logo-img").attr("src") ||
                $("link[rel='icon']").attr("href");

            if (fallback && fallback.trim() !== "") {
                imglink = fallback;
            }
        }

        if (
            orgName != null &&
            (
                (typeof w.IsFaPage !== "undefined" && typeof w.OrgName !== "undefined" && !w.IsFaPage) ||
                (typeof w.OrgName !== "undefined" && w.OrgName !== orgName)
            )
        ) {
            imglink = imglink.replace("upload", `upload/${orgName}`);
        }

        return imglink;
    }

    function buildLinkPath($item, data) {
        const isSearch = $item.data("type") == "search";
        const dirPath = typeof $item.data("dirpath") === "undefined" ? "" : String($item.data("dirpath")).toLowerCase();

        let path;
        let target;

        const pathSegments = w.location.pathname.toLowerCase().split("/");
        const isSpecificPage = pathSegments.includes("search") || pathSegments.includes("techcert");

        if (isSearch || isSpecificPage) {
            const links = String(data.link || "").split("?filter=");
            data.link = links[0];
            const filter = links.length > 1 ? "?filter=" + links[1] : "";

            switch (data.type) {
                case 3:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/${data.link}`;
                    break;
                default:
                    path = `${data.orgName == null ? "" : `/${data.orgName}`}/search/${data.link}`;
                    break;
            }

            if (typeof $item.data("search-text") !== "undefined" && $item.data("search-text") !== "") {
                path = `${path}/${encodeURIComponent($item.data("search-text"))}${filter}`;
            }

            target = "_blank";

            if ((data.mainImage || "").indexOf("youtu") > 0) {
                let key = "";
                const rx = /^.*(?:(?:youtu.be\/|v\/|vi\/|u\/w\/|embed\/)|(?:(?:watch)??v(?:i)?=|&v(?:i)?=))([^#&?]*).*/;
                const r = data.mainImage.match(rx);
                if (r != null && r.length > 0) key = r[1];
                data.mainImage = "https://img.youtube.com/vi/" + key + "/mqdefault.jpg";
            }
        } else {
            path =
                w.location.pathname.indexOf(data.orgName) > 0 &&
                    w.location.pathname.toLowerCase().indexOf("home") < 0 &&
                    w.location.pathname.toLowerCase().indexOf(dirPath) >= 0
                    ? w.location.pathname
                    : `${data.orgName == null ? "" : `/${data.orgName}`}${dirPath == ""
                        ? data.orgName == null
                            ? w.location.pathname
                            : w.location.pathname.toLowerCase().replace(`${String(data.orgName).toLowerCase()}`, "")
                        : `/${dirPath}`
                    }`;

            if (typeof $item.data("pageto") !== "undefined" && $item.data("pageto") !== "") {
                const index = path.substring(1).indexOf("/") + 1;
                path = path.substring(0, index + 1) + $item.data("pageto");
            }

            path += data.link;
            target = "_self";
        }

        if (!/^http/.test(path)) {
            path = path.replace("//", "/");
        }

        return {
            path: path,
            target: target,
            title: target === "_blank"
                ? (isFn(w.cokerI18n) ? w.cokerI18n("LinkToAndBlank", data.title) : data.title)
                : (isFn(w.cokerI18n) ? w.cokerI18n("LinkTo", data.title) : data.title)
        };
    }

    function applyLinkToContent(content, linkData) {
        const $content = $(content);
        const attrs = {
            href: linkData.path,
            title: linkData.title,
            target: linkData.target,
            rel: "noopener noreferrer"
        };

        if ($content.length > 0) {
            if ($content[0].tagName === "A") {
                $content.attr(attrs);
            } else {
                $content.find("a").first().attr(attrs);
            }
        }
    }

    function applyBasicFields($item, content, data) {
        const $content = $(content);

        if ($item.hasClass("cross_graphics_frame") && data.mainImage == "") {
            $content.children("div:first").addClass("d-none");
        }

        $content.find(".dirname").removeClass("d-none").text(data.dirname || "");

        const imglink = normalizeImagePath(data.orgName, data.mainImage);
        $content.find("img").attr("src", imglink);
        $content.find("img").imgCheck().attr("alt", `${data.title}的主要圖片`);

        $content.find(".title").text(data.title || "");
        $content.find(".subtitle").text(data.subtitle || "");
        $content.find(".description").html(data.description || "");

        if ($item.hasClass("hover_display_details") && typeof w.OrgName !== "undefined") {
            $content.data("img_link", data.mainImage);
        }

        if ($content.is("a")) {
            if ($content.find("img").length && $content.find("h3,h4,h5,h6,span,p").length) {
                $content.find("img").imgCheck().attr("alt", " ");
            }
        } else {
            if ($content.find("a img").length && $content.find("a").find("h3,h4,h5,h6,span,p").length) {
                $content.find("img").imgCheck().attr("alt", " ");
            }
        }

        if ($content.find(".location").length > 0 && isNullOrEmpty(data.location)) {
            $content.find(".location").parents(".py-2").remove();
        } else {
            $content.find(".location").text(data.location || "");
        }

        if ($content.find(".address").length > 0 && isNullOrEmpty(data.address)) {
            $content.find(".address").parents(".py-2").remove();
        } else {
            $content.find(".address").text(data.address || "");
        }
    }

    function applyDateFields(content, data) {
        const $content = $(content);

        if (data.startTime != null && data.startTime !== "") {
            const startTime = new Date(data.startTime);
            $content.find(".startTime").text(
                `${startTime.getFullYear()}/${String(startTime.getMonth() + 1).padStart(2, "0")}/${String(startTime.getDate()).padStart(2, "0")}`
            );
        } else {
            $content.find(".startTime").each(function (i, e) {
                if (e.tagName.toLowerCase() === "span") $(e).parent().remove();
                else $(e).remove();
            });
        }

        if (data.nodeDate != null && data.nodeDate !== "") {
            const noteDate = new Date(data.nodeDate);
            const year = noteDate.getFullYear();
            const month = String(noteDate.getMonth() + 1).padStart(2, "0");
            const day = String(noteDate.getDate()).padStart(2, "0");

            $content.find(".date").text(`${year}/${month}/${day}`);
            $content.find(".date-month").text(`${month}月`);
            $content.find(".date-monthyear").text(`${month}/${year}`);
            $content.find(".date-day").text(`${day}`);
            $content.find(".date-month-number").text(`${month}`);
            $content.find(".date-year").text(`${year}`);
        } else {
            $content.find(".date,.date-month,date-monthyear,date-day").each(function (i, e) {
                if (e.tagName.toLowerCase() === "span") $(e).parent().remove();
                else $(e).remove();
            });
        }
    }

    function applyLoginSensitiveUi($item, result) {
        if (!(Array.isArray(result) && result.length > 0 && result[0].type == 1)) {
            if ($item.find(".btn_addToCar").length > 0) {
                $item.find(".btn_addToCar").addClass("d-none");
            }
            return $.Deferred().resolve().promise();
        }

        const dfd = $.Deferred();

        if (typeof w.islogin === "undefined" && typeof w.OrgName !== "undefined" && w.Coker && Coker.Token && isFn(Coker.Token.CheckToken)) {
            Coker.Token.CheckToken().done(function (token_result) {
                if (token_result.success) {
                    w.islogin = token_result.isLogin;
                    w.IsLogin = token_result.isLogin;
                }
                dfd.resolve();
            }).fail(function () {
                dfd.resolve();
            });
        } else {
            if (typeof w.IsLogin === "undefined" && typeof w.islogin !== "undefined") {
                w.IsLogin = w.islogin;
            }
            dfd.resolve();
        }

        return dfd.promise();
    }

    function clearCatalogItems($item) {
        const $catalog = getCatalog($item);
        $catalog.children().not(".templatecontent").not(".templatecontent-tag").remove();
    }

    function createCardContent($item) {
        const temp = getTemplateHtml($item);
        if (!temp) return $();
        return $(temp).clone();
    }

    function appendCard($item, content, data) {
        const $catalog = getCatalog($item);
        $catalog.append(content);
        $(content).attr({ "data-id": data.id });
    }

    function renderSingleCard($item, data) {
        const content = createCardContent($item);
        if (!content.length) return;

        const linkData = buildLinkPath($item, data);

        applyLinkToContent(content, linkData);
        applyBasicFields($item, content, data);
        applyDateFields(content, data);

        if (w.DirectoryPrice && isFn(w.DirectoryPrice.apply)) {
            w.DirectoryPrice.apply(content, data);
        }

        if (w.DirectoryParts) {
            if (isFn(w.DirectoryParts.applyStatus)) {
                w.DirectoryParts.applyStatus(content, data);
            }
            if (isFn(w.DirectoryParts.applyItemNo)) {
                w.DirectoryParts.applyItemNo(content, data);
            }
            if (isFn(w.DirectoryParts.applyTags)) {
                w.DirectoryParts.applyTags($item, content, data);
            }
            if (isFn(w.DirectoryParts.applyMarketingLabels)) {
                w.DirectoryParts.applyMarketingLabels(content, data);
            }
            if (isFn(w.DirectoryParts.applyShare)) {
                w.DirectoryParts.applyShare(content, linkData.path);
            }
            if (isFn(w.DirectoryParts.applyFavorite)) {
                w.DirectoryParts.applyFavorite(content, data);
            }
            if (isFn(w.DirectoryParts.applyBuyButton)) {
                w.DirectoryParts.applyBuyButton($item, content, data, linkData.path);
            }
        }

        appendCard($item, content, data);
    }

    function renderCatalogItems($item, result) {
        if (!Array.isArray(result)) return;

        if (w.DirectoryParts) {
            if (isFn(w.DirectoryParts.applyEmptyState)) {
                w.DirectoryParts.applyEmptyState($item, result);
            }
            if (isFn(w.DirectoryParts.applyHoverDetailsState)) {
                w.DirectoryParts.applyHoverDetailsState($item, result);
            }
        }

        result.forEach(function (data) {
            renderSingleCard($item, data);
        });

        if (w.DirectoryParts && isFn(w.DirectoryParts.afterRender)) {
            w.DirectoryParts.afterRender($item, result);
        }
    }

    function renderPager($item, option, result) {
        const dirLength = $(".catalog_frame").length;
        let page = parseInt(option.Page, 10);
        let loadPageRange = 2;

        $item.find(".page-item").each(function () {
            const $self = $(this);
            if (!$self.hasClass("btn_prev") && !$self.hasClass("btn_next")) {
                $self.remove();
            }
        });

        if (result.totalPage <= 1) $item.find(".page_btn").addClass("d-none");
        else $item.find(".page_btn").removeClass("d-none");

        if (page > result.totalPage) page = result.totalPage;
        else if (page < 1) page = 1;

        if (page == 1) $item.find(".btn_prev").addClass("d-none");
        else $item.find(".btn_prev").removeClass("d-none");

        if (page == result.totalPage) $item.find(".btn_next").addClass("d-none");
        else $item.find(".btn_next").removeClass("d-none");

        if (page == 1 || page == result.totalPage) loadPageRange = 4;

        for (let i = 1; i <= result.totalPage; i++) {
            if (i == 1) {
                if (i == page) {
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page="${i}">1</span></li>`);
                } else {
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><button class="btn_page page-link text-black" title="第一頁" data-page="${i}">1</button></li>`);
                    if (page - loadPageRange - 1 > 0) {
                        $item.find(".page_btn").children(".btn_next")
                            .before(`<li class="page-item px-2">...</li>`);
                    }
                }
            } else if (i == result.totalPage) {
                if (i == page) {
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page="${i}">${result.totalPage}</span></li>`);
                } else {
                    if (page + loadPageRange + 1 < result.totalPage) {
                        $item.find(".page_btn").children(".btn_next")
                            .before(`<li class="page-item px-2">...</li>`);
                    }
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><button class="btn_page page-link text-black" title="最後一頁" data-page="${i}">${result.totalPage}</button></li>`);
                }
            } else if (i >= page - loadPageRange && i <= page + loadPageRange) {
                if (i == page) {
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><span class="btn_page page-link text-black bg-secondary" data-page="${i}">${i}</span></li>`);
                } else {
                    $item.find(".page_btn").children(".btn_next")
                        .before(`<li class="page-item"><button class="btn_page page-link text-black" title="移動至第${i}頁" data-page="${i}">${i}</button></li>`);
                }
            }
        }

        $(".btn_page").off("click.directoryPager").on("click.directoryPager", function () {
            const $self = $(this);

            if (page != $self.data("page")) {
                page = $self.data("page");
                if (dirLength == 1 && w.location.hash != `#${page}`) w.location.hash = `#${page}`;
                else if (isFn(w.initElemntAndLoadDir)) w.initElemntAndLoadDir($item, page);
            }

            if (isFn($item.goTo)) $item.goTo();
        });

        if (!$item.data("init")) {
            $item.find(".btn_prev > button").off("click.directoryPager").on("click.directoryPager", function () {
                const $self = $(this);
                page = parseInt($item.data("page"), 10) - 1;
                $self.data("page", page);

                if (page >= 1) {
                    if (dirLength == 1 && w.location.hash != `#${page}`) w.location.hash = `#${page}`;
                    else if (isFn(w.initElemntAndLoadDir)) w.initElemntAndLoadDir($item, $self.data("page"));
                }

                if (isFn($item.goTo)) $item.goTo();
            });

            $item.find(".btn_next > button").off("click.directoryPager").on("click.directoryPager", function () {
                const $self = $(this);
                page = parseInt($item.data("page"), 10) + 1;
                $self.data("page", page);

                if (page <= result.totalPage) {
                    if (dirLength == 1 && w.location.hash != `#${page}`) w.location.hash = `#${page}`;
                    else if (isFn(w.initElemntAndLoadDir)) w.initElemntAndLoadDir($item, $self.data("page"));
                }

                if (isFn($item.goTo)) $item.goTo();
            });
        }

        $item.data("init", true);
    }

    function handleSwiperAfterRender($item) {
        if (!($item.hasClass("swiper") || $item.find(".swiper").length > 0 || $item.hasClass("swiper-wrapper"))) return;

        let c, b;

        if ($item.hasClass("swiper")) c = $item;
        else if ($item.find(".swiper").length > 0) c = $item.find(".swiper");
        else c = $item.parents(".swiper");

        b = $(c).parents(`[class*="_swiper"]`);

        if ($(b).length > 0) {
            if (typeof $(b).data("isInit") !== "undefined" && $(b).data("isInit")) {
                c[0].swiper.destroy(true, true);
            }
            b.data("isInit", false);
            if (isFn(w.SwiperInit)) w.SwiperInit({ autoplay: true });
        }
    }

    function resetType4CaptionHeights($item) {
        $item.removeClass("caption-equalized");
        $item.find(".type4-caption").css("min-height", "");
    }

    function getRowTop($el) {
        return Math.round($el.offset().top);
    }

    function equalizeType4Captions($item) {
        if (!$item || !$item.length) return;

        const $catalog = getCatalog($item);
        if (!$catalog.length || !$catalog.hasClass("type4")) {
            resetType4CaptionHeights($item);
            return;
        }

        const $cards = $catalog.find("> .template[data-id]");
        if ($cards.length <= 1) {
            resetType4CaptionHeights($item);
            return;
        }

        const $captions = $cards.find(".type4-caption:visible");
        if ($captions.length <= 1) {
            resetType4CaptionHeights($item);
            return;
        }

        resetType4CaptionHeights($item);

        const rows = new Map();

        $cards.each(function () {
            const $card = $(this);
            const $caption = $card.find(".type4-caption:visible").first();
            if (!$caption.length) return;

            const rowTop = getRowTop($card);
            if (!rows.has(rowTop)) rows.set(rowTop, []);
            rows.get(rowTop).push($caption);
        });

        rows.forEach(function (rowItems) {
            if (!rowItems || rowItems.length <= 1) return;

            let maxHeight = 0;
            rowItems.forEach(function ($caption) {
                const height = $caption.outerHeight();
                if (height > maxHeight) maxHeight = height;
            });

            rowItems.forEach(function ($caption) {
                $caption.css("min-height", `${maxHeight}px`);
            });
        });

        $item.addClass("caption-equalized");
    }

    function refreshType4CaptionEqualizer($item) {
        clearTimeout($item.data("captionEqualizeTimer"));
        const timer = setTimeout(function () {
            equalizeType4Captions($item);
        }, 0);
        $item.data("captionEqualizeTimer", timer);
    }

    function bindImageLoadEqualizer($item) {
        $item.find(".type4-image-frame img").off("load.captionEqualizer").on("load.captionEqualizer", function () {
            refreshType4CaptionEqualizer($item);
        });
    }

    DirectoryRenderer.renderItemsOnly = function ($item, releInfos) {
        if (!$item || !$item.length) return;

        clearCatalogItems($item);
        resetType4CaptionHeights($item);

        applyLoginSensitiveUi($item, releInfos).done(function () {
            renderCatalogItems($item, releInfos || []);
            handleSwiperAfterRender($item);
            bindImageLoadEqualizer($item);
            refreshType4CaptionEqualizer($item);
            $item.trigger("catalog:rendered", [{ reason: "DirectoryRenderer.renderItemsOnly" }]);
        });
    };

    /**
     * 給外部頁面（例如 Member Favorites）使用既有 template 進行渲染
     */
    DirectoryRenderer.renderItemsByExternalTemplate = function ($item, $container, templateHtml, dataList) {
        if (!$item || !$item.length || !$container || !$container.length || !templateHtml) return;

        $container.empty();
        resetType4CaptionHeights($item);

        applyLoginSensitiveUi($item, dataList).done(function () {
            if (!Array.isArray(dataList)) return;

            dataList.forEach(function (data) {
                const content = $(templateHtml).clone();
                if (!content.length) return;

                const linkData = buildLinkPath($item, data);

                applyLinkToContent(content, linkData);
                applyBasicFields($item, content, data);
                applyDateFields(content, data);

                if (w.DirectoryPrice && isFn(w.DirectoryPrice.apply)) {
                    w.DirectoryPrice.apply(content, data);
                }

                if (w.DirectoryParts) {
                    if (isFn(w.DirectoryParts.applyStatus)) {
                        w.DirectoryParts.applyStatus(content, data);
                    }
                    if (isFn(w.DirectoryParts.applyItemNo)) {
                        w.DirectoryParts.applyItemNo(content, data);
                    }
                    if (isFn(w.DirectoryParts.applyTags)) {
                        w.DirectoryParts.applyTags($item, content, data);
                    }
                    if (isFn(w.DirectoryParts.applyMarketingLabels)) {
                        w.DirectoryParts.applyMarketingLabels(content, data);
                    }
                    if (isFn(w.DirectoryParts.applyShare)) {
                        w.DirectoryParts.applyShare(content, linkData.path);
                    }
                    if (isFn(w.DirectoryParts.applyFavorite)) {
                        w.DirectoryParts.applyFavorite(content, data);
                    }
                    if (isFn(w.DirectoryParts.applyBuyButton)) {
                        w.DirectoryParts.applyBuyButton($item, content, data, linkData.path);
                    }
                }

                $container.append(content);
            });

            if (w.DirectoryParts && isFn(w.DirectoryParts.afterRender)) {
                w.DirectoryParts.afterRender($item, dataList);
            }

            handleSwiperAfterRender($item);
            bindImageLoadEqualizer($item);
            refreshType4CaptionEqualizer($item);
            $item.trigger("catalog:rendered", [{ reason: "DirectoryRenderer.renderItemsByExternalTemplate" }]);
        });
    };

    DirectoryRenderer.renderCatalogResult = function ($item, option, result) {
        if (!result) return;

        if (option.Type == "search") {
            $(".searchCount").text(result.totalCount);
        }

        renderPager($item, option, result);
        clearCatalogItems($item);
        resetType4CaptionHeights($item);

        applyLoginSensitiveUi($item, result.releInfos).done(function () {
            renderCatalogItems($item, result.releInfos);

            $item.data({
                filter: result.filter,
                directoryType: result.directoryType
            }).trigger("load");

            handleSwiperAfterRender($item);
            bindImageLoadEqualizer($item);
            refreshType4CaptionEqualizer($item);
            $item.trigger("catalog:rendered", [{ reason: "DirectoryRenderer.renderCatalogResult" }]);
        });
    };

    $(w).on("resize.directoryCaptionEqualizer", function () {
        clearTimeout(w.__directoryCaptionResizeTimer);
        w.__directoryCaptionResizeTimer = setTimeout(function () {
            $(".catalog_frame.type_change_frame").each(function () {
                const $item = $(this);
                refreshType4CaptionEqualizer($item);
            });
        }, 80);
    });

})(window, window.jQuery);