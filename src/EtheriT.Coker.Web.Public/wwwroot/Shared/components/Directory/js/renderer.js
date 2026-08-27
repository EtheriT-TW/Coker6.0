(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryRenderer = (w.DirectoryRenderer = w.DirectoryRenderer || {});

    const MONTH_NAMES_EN = [
        "Jan.", "Feb.", "Mar.", "Apr.", "May", "Jun.",
        "Jul.", "Aug.", "Sep.", "Oct.", "Nov.", "Dec."]

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

    function normalizePathSegment(value) {
        return String(value || "")
            .trim()
            .replace(/^\/+/, "")
            .replace(/\/+$/, "");
    }

    function buildDirectoryBasePath(orgName, dirPath) {
        const org = normalizePathSegment(orgName).toLowerCase();
        const dir = normalizePathSegment(dirPath);

        if (dir === "") {
            return org === "" ? "" : `/${org}`;
        }

        const dirLower = dir.toLowerCase();

        // dirPath 已經包含 orgName，例如 mjmw/product，不再補 /mjmw
        if (org !== "" && (dirLower === org || dirLower.startsWith(`${org}/`))) {
            return `/${dir}`;
        }

        // dirPath 不含 orgName，例如 product，才補 /orgName
        if (org !== "") {
            return `/${org}/${dir}`;
        }

        return `/${dir}`;
    }

    function buildLinkPath($item, data) {
        const isSearch = $item.data("type") == "search";
        const dirPath = typeof $item.data("dirpath") === "undefined"
            ? ""
            : normalizePathSegment($item.data("dirpath")).toLowerCase();

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
            const orgName = normalizePathSegment(data.orgName);
            const currentPathRaw = w.location.pathname;
            const currentPath = currentPathRaw.toLowerCase();
            const orgLower = orgName.toLowerCase();

            if (
                orgName !== "" &&
                dirPath !== "" &&
                currentPath.indexOf(orgLower) > 0 &&
                currentPath.indexOf("home") < 0 &&
                currentPath.indexOf(dirPath) >= 0
            ) {
                path = currentPathRaw;
            } else if (dirPath !== "") {
                path = buildDirectoryBasePath(orgName, dirPath);
            } else if (orgName !== "") {
                const currentClean = normalizePathSegment(currentPathRaw);
                const currentCleanLower = currentClean.toLowerCase();

                if (currentCleanLower === orgLower || currentCleanLower.startsWith(`${orgLower}/`)) {
                    path = `/${currentClean}`;
                } else {
                    path = `/${orgName}${currentClean === "" ? "" : `/${currentClean}`}`;
                }
            } else {
                path = currentPathRaw;
            }

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
            if ($content.attr("data-directory-clickable-card") === "true") {
                $content.attr({ role: "link", tabindex: "0", "data-directory-href": linkData.path });
                $content.off("click.directoryCard keydown.directoryCard")
                    .on("click.directoryCard", function (event) {
                        if ($(event.target).closest("button, .btn_addToCar, .btn_fav, .shareBlock, .tags, a").length) return;
                        w.location.href = linkData.path;
                    })
                    .on("keydown.directoryCard", function (event) {
                        if (event.key !== "Enter" && event.key !== " ") return;
                        if ($(event.target).closest("button, .btn_addToCar, .btn_fav, .shareBlock, .tags, a").length) return;
                        event.preventDefault();
                        w.location.href = linkData.path;
                    });
                return;
            }

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
        $content.find(".description").html(data.introduction || data.description || "");

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
            const monthEN = MONTH_NAMES_EN[noteDate.getMonth()] || "";
            const day = String(noteDate.getDate()).padStart(2, "0");

            $content.find(".date").text(`${year}/${month}/${day}`);
            $content.find(".date-month").text(`${month}月`);
            $content.find(".date-monthyear").text(`${month}/${year}`);
            $content.find(".date-day").text(`${day}`);
            $content.find(".date-month-number").text(`${month}`);
            $content.find(".date-month-number-en").text(`${monthEN}`);
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

    function getLoadingItemCount($catalog) {
        return $catalog.hasClass("type1") ? 3 : 4;
    }

    function createLoadingCard($item, loadingText) {
        const content = createCardContent($item);
        if (!content.length) return $();

        const $content = $(content);

        $content
            .addClass("directory-loading-item")
            .attr("aria-hidden", "true")
            .removeAttr("data-id data-key");

        $content.find("[id]").removeAttr("id");
        $content.find("[data-key]").removeAttr("data-key");
        $content.find("a").removeAttr("href title target rel").attr("tabindex", "-1");
        $content.find("button, input, select, textarea").prop("disabled", true).attr("tabindex", "-1");

        $content.find(".title").text(loadingText);
        $content.find(".subtitle, .description, .itemNo, .catalog-number, .date, .more").empty();
        $content.find(".shareBlock, .btn_fav, .like-and-share, .btn_addToCar, .more-btn, .purchase, .price-grid, .tags, .marketing-labels")
            .addClass("d-none");

        $content.find(".image_frame")
            .css("background-image", "url('/images/directory-loading.svg')");
        $content.find(".image_frame img")
            .attr({
                src: "/images/directory-loading.svg",
                alt: ""
            });

        return $content;
    }

    function createCardContent($item) {
        const temp = getTemplateHtml($item);
        if (!temp) return $();
        const $content = $(temp).clone();

        // 商品圖卡的舊模板可能以 <a> 作為根節點，並把購買按鈕包在裡面。
        // 這是無效的互動元素巢狀，瀏覽器會優先執行連結而吃掉按鈕事件。
        if ($content.length === 1 && $content.is("a") && $content.find(".btn_addToCar, button").length) {
            const $card = $("<div>").attr("data-directory-clickable-card", "true");
            Array.from($content[0].attributes || []).forEach(function (attribute) {
                if (!["href", "target", "rel"].includes(attribute.name.toLowerCase())) {
                    $card.attr(attribute.name, attribute.value);
                }
            });
            $card.append($content.contents());
            return $card;
        }

        return $content;
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

    function getPagerUrl(page) {
        if (w.DirectoryBoot && isFn(w.DirectoryBoot.getPageUrl)) {
            return w.DirectoryBoot.getPageUrl(page);
        }

        const url = new URL(w.location.href);
        if (page === 1) url.searchParams.delete("Page");
        else url.searchParams.set("Page", page);
        return `${url.pathname}${url.search}${url.hash}`;
    }

    function createPagerItem(page, currentPage, title) {
        const $item = $("<li>", { class: "page-item" });

        if (page === currentPage) {
            return $item.append($("<span>", {
                class: "btn_page page-link text-black bg-secondary",
                "data-page": page,
                "aria-current": "page"
            }).text(page));
        }

        return $item.append($("<a>", {
            class: "btn_page page-link text-black",
            href: getPagerUrl(page),
            title: title,
            "data-page": page
        }).text(page));
    }

    function configureDirectionLink($container, page, rel) {
        let $control = $container.children("a, button").first();

        if ($control.is("button")) {
            const $link = $("<a>", {
                class: $control.attr("class"),
                title: $control.attr("title"),
                "aria-label": $control.attr("aria-label")
            }).html($control.html());
            $control.replaceWith($link);
            $control = $link;
        }

        $control
            .addClass("btn_page")
            .attr({ href: getPagerUrl(page), "data-page": page, rel: rel });
    }

    function renderPager($item, option, result) {
        let page = parseInt(option.Page, 10);
        let loadPageRange = 2;
        const totalPage = Math.max(parseInt(result.totalPage, 10) || 0, 0);
        const $pager = $item.find(".page_btn");
        const $nextItem = $pager.children(".btn_next");

        $pager.children(".page-item").not(".btn_prev, .btn_next").remove();
        $pager.toggleClass("d-none", totalPage <= 1);

        if (page > totalPage) page = totalPage;
        else if (page < 1) page = 1;

        $item.find(".btn_prev").toggleClass("d-none", page <= 1);
        $item.find(".btn_next").toggleClass("d-none", page >= totalPage);

        configureDirectionLink($item.find(".btn_prev"), Math.max(page - 1, 1), "prev");
        configureDirectionLink($item.find(".btn_next"), Math.min(page + 1, totalPage), "next");

        if (page === 1 || page === totalPage) loadPageRange = 4;

        for (let i = 1; i <= totalPage; i++) {
            if (i === 1) {
                createPagerItem(i, page, "第一頁").insertBefore($nextItem);
                if (i !== page && page - loadPageRange - 1 > 0) {
                    $("<li>", { class: "page-item px-2", "aria-hidden": "true" }).text("...").insertBefore($nextItem);
                }
            } else if (i === totalPage) {
                if (i !== page && page + loadPageRange + 1 < totalPage) {
                    $("<li>", { class: "page-item px-2", "aria-hidden": "true" }).text("...").insertBefore($nextItem);
                }
                createPagerItem(i, page, "最後一頁").insertBefore($nextItem);
            } else if (i >= page - loadPageRange && i <= page + loadPageRange) {
                createPagerItem(i, page, `移動至第${i}頁`).insertBefore($nextItem);
            }
        }

        $pager.off("click.directoryPager").on("click.directoryPager", "a.btn_page", function (event) {
            // 保留開新分頁、另存連結等瀏覽器原生操作。
            if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

            event.preventDefault();
            const targetPage = parseInt($(this).attr("data-page"), 10);

            if (targetPage !== page) {
                // 等新資料完成渲染後再捲動，避免依照舊版面的位置計算。
                $item.data("directoryScrollAfterRender", true);

                if (w.DirectoryBoot && isFn(w.DirectoryBoot.navigateToPage)) {
                    w.DirectoryBoot.navigateToPage($item, targetPage);
                } else if (isFn(w.initElemntAndLoadDir)) {
                    w.initElemntAndLoadDir($item, targetPage);
                }
            }
        });
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

    function bindViewTypeChangeEqualizer($item) {
        $item.off("viewtype:changed.captionEqualizer")
            .on("viewtype:changed.captionEqualizer", function () {
                refreshType4CaptionEqualizer($item);
            });
    }

    function refreshViewTypeIfNeeded($item) {
        if (isFn(w.ViewTypeChangeRefresh)) {
            w.ViewTypeChangeRefresh($item);
        }
    }

    function afterCatalogRendered($item, reason) {
        refreshViewTypeIfNeeded($item);
        handleSwiperAfterRender($item);
        bindImageLoadEqualizer($item);
        bindViewTypeChangeEqualizer($item);
        refreshType4CaptionEqualizer($item);
        $item.trigger("catalog:rendered", [{ reason: reason }]);
        scrollToCatalogAfterRender($item);
    }

    function getFixedTopOffset() {
        let offset = 0;

        $("header, nav, .fixed-top, .sticky-top, [data-sticky]").each(function () {
            const style = w.getComputedStyle(this);
            if (style.position !== "fixed" && style.position !== "sticky") return;

            const rect = this.getBoundingClientRect();
            if (rect.height <= 0 || rect.bottom <= 0 || rect.top >= w.innerHeight / 2) return;

            offset = Math.max(offset, rect.bottom);
        });

        return Math.max(offset, 0);
    }

    function getCatalogScrollTarget($item) {
        const $main = $item.closest("#main, main").first();

        // 主內容只有一個目錄時，從 main 開始顯示，保留 breadcrumb 與頁面標題。
        if ($main.length && $main.find(".catalog_frame").length === 1) {
            return { $element: $main, spacing: 16 };
        }

        // 同頁有多個目錄時不拉回整頁頂端，改在該目錄上方預留 4rem。
        const rootFontSize = parseFloat(w.getComputedStyle(document.documentElement).fontSize) || 16;
        return { $element: $item, spacing: rootFontSize * 4 };
    }

    function scrollToCatalogAfterRender($item) {
        const shouldScroll = $item.data("directoryScrollAfterRender") === true;
        $item.removeData("directoryScrollAfterRender");
        if (!shouldScroll) return;

        // 連續兩個 frame：讓卡片、檢視模式與等高處理先完成版面計算。
        w.requestAnimationFrame(function () {
            w.requestAnimationFrame(function () {
                if (!$item.length || !$.contains(document, $item[0])) return;

                const scrollTarget = getCatalogScrollTarget($item);
                const topSpacing = getFixedTopOffset() + scrollTarget.spacing;
                const targetTop = Math.max(0, Math.round(scrollTarget.$element.offset().top - topSpacing));

                $("html, body").stop(true).animate({ scrollTop: targetTop }, 200);
            });
        });
    }

    DirectoryRenderer.renderItemsOnly = function ($item, releInfos) {
        if (!$item || !$item.length) return;

        clearCatalogItems($item);
        resetType4CaptionHeights($item);

        applyLoginSensitiveUi($item, releInfos).done(function () {
            renderCatalogItems($item, releInfos || []);
            afterCatalogRendered($item, "DirectoryRenderer.renderItemsOnly");
        });
    };

    DirectoryRenderer.showLoading = function ($item) {
        if (!$item || !$item.length) return;

        const $catalog = getCatalog($item);
        const loadingText = "載入中…";

        clearCatalogItems($item);
        resetType4CaptionHeights($item);
        $catalog.removeClass("empty").attr("aria-busy", "true");

        const itemCount = getLoadingItemCount($catalog);
        for (let i = 0; i < itemCount; i++) {
            const $loadingCard = createLoadingCard($item, loadingText);
            if (!$loadingCard.length) break;
            $catalog.append($loadingCard);
        }
    };

    DirectoryRenderer.hideLoading = function ($item) {
        if (!$item || !$item.length) return;

        getCatalog($item)
            .removeAttr("aria-busy")
            .children(".directory-loading-item")
            .remove();
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

                $(content).attr({ "data-id": data.id });
                $container.append(content);
            });

            if (w.DirectoryParts && isFn(w.DirectoryParts.afterRender)) {
                w.DirectoryParts.afterRender($item, dataList);
            }

            afterCatalogRendered($item, "DirectoryRenderer.renderItemsByExternalTemplate");
        });
    };

    DirectoryRenderer.renderCatalogResult = function ($item, option, result, requestId) {
        if (requestId && $item.data("directoryRequestId") !== requestId) return;

        if (!result) {
            DirectoryRenderer.hideLoading($item);
            return;
        }

        if (option.Type == "search") {
            $(".searchCount").text(result.totalCount);
        }

        renderPager($item, option, result);
        resetType4CaptionHeights($item);

        applyLoginSensitiveUi($item, result.releInfos).done(function () {
            if (requestId && $item.data("directoryRequestId") !== requestId) return;

            clearCatalogItems($item);
            getCatalog($item).removeAttr("aria-busy");
            renderCatalogItems($item, result.releInfos);

            $item.data({
                filter: result.filter,
                directoryType: result.directoryType
            }).trigger("load");

            afterCatalogRendered($item, "DirectoryRenderer.renderCatalogResult");
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
