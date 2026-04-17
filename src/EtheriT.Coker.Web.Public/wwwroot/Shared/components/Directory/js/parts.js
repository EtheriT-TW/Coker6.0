(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryParts = (w.DirectoryParts = w.DirectoryParts || {});

    function isFn(fn) {
        return typeof fn === "function";
    }

    function isNullOrEmpty(value) {
        return value === null || value === undefined || value === "";
    }

    function getTempTagHtml($item) {
        const html = $item.find(".templatecontent-tag").html();
        return html || "";
    }

    function getCatalog($item) {
        return $item.find(".catalog");
    }

    // =========================
    // Status（加入 slot 支援）
    // =========================
    DirectoryParts.applyStatus = function (content, data) {
        const $content = $(content);

        if (data.type == 1 && data.status != 0) {
            const text = isNullOrEmpty(data.statusName) ? "" : data.statusName;
            const html = `<span class="status status${data.status}">${text}</span>`;

            const $slot = $content.find(".status-slot");

            if ($slot.length) {
                $slot.append(html);
            } else {
                $content.find("a").first().append(html);
            }
        }
    };

    // =========================
    // Buy Button（完全不動）
    // =========================
    DirectoryParts.applyBuyButton = function ($item, content, data, path) {
        const $content = $(content);
        let hasBuyBtn = false;

        if (typeof $item.data("hasbuybtn") === "string") {
            hasBuyBtn = $item.data("hasbuybtn").toLowerCase() === "true";
        } else {
            hasBuyBtn = !!$item.data("hasbuybtn");
        }

        if (hasBuyBtn && ($("#btn_car_dropdown").length > 0 || typeof w.OrgName === "undefined")) {
            $item.addClass("hasBuyBtn");
        } else {
            $item.removeClass("hasBuyBtn");
        }

        if (!$item.hasClass("hasBuyBtn")) {
            if ($content.find(".btn_addToCar").length > 0) {
                $content.find(".btn_addToCar").addClass("d-none");
            }
            return;
        }

        if ($content.find(".btn_addToCar").length > 0) {
            $content.find(".btn_addToCar").removeClass("d-none");
        } else {
            const html = '<div class="btn_addToCar"><button type="button">瀏覽商品</button></div>';
            $content.find("div").first().find("a").first().after(html);
        }

        if (data.type == 1 && $content.find(".btn_addToCar").length > 0) {
            $content.find(".btn_addToCar").off("click").on("click", function (e) {
                e.preventDefault();
                w.location.href = path;
            });
        }
    };

    DirectoryParts.applyItemNo = function (content, data) {
        const $content = $(content);

        if (!isNullOrEmpty(data.itemNo)) {
            $content.find(".itemNo").text(data.itemNo);
        } else {
            $content.find(".itemNo").remove();
        }
    };

    // =========================
    // Tags（加入 fallback）
    // =========================
    DirectoryParts.applyTags = function ($item, content, data) {
        const $content = $(content);
        const $catalog = getCatalog($item);
        const tempTag = getTempTagHtml($item);

        const $tags = $content.find(".tags");
        if ($tags.length === 0) return;

        $tags.empty();

        const tags = Array.isArray(data.tags) ? data.tags : [];

        // fallback（新）
        if (!tempTag) {
            tags.forEach(function (tag) {
                const name = tag.tag_Name || tag;
                $tags.append(`<span class="badge">${name}</span>`);
            });
            return;
        }

        tags.slice(0, 2).forEach(function (tag) {
            const badge = $(tempTag).clone();
            badge.text(tag.tag_Name);
            badge.data("tagname", tag.tag_Name);
            $tags.append(badge);
        });

        tags.slice(2).forEach(function (tag) {
            const badge = $(tempTag).clone();
            badge.text((tag.tag_Name || "").slice(0, 4));
            badge.data("tagname", tag.tag_Name);

            if ($catalog.hasClass("type1")) {
                badge.addClass("more-tag");
            } else {
                badge.addClass("more-tag d-none");
            }

            $tags.append(badge);
        });

        if (tags.length > 2) {
            const badge = $(tempTag).clone();
            badge.text("...");
            badge.data("tagname", "...");

            if ($catalog.hasClass("type1")) {
                badge.addClass("less-tag d-none");
            } else {
                badge.addClass("less-tag");
            }

            $tags.append(badge);
        }

        $tags.children().each(function () {
            const $self = $(this);

            if ($self.hasClass("less-tag")) return;

            $self.off("click").on("click", function () {
                const text = $self.data("tagname");
                if (typeof w.OrgName === "undefined") return false;

                location.href = `/${w.OrgName}/Search/Get/3/${encodeURIComponent(text)}`;
                return false;
            });
        });
    };

    DirectoryParts.applyShare = function (content, path) {
        const $content = $(content);
        const $shareBlock = $content.find(".shareBlock");

        if ($shareBlock.length === 0) return;

        $shareBlock.data("init", false);
        $shareBlock.find("> a").remove();
        $shareBlock.data("href", path);
    };

    // =========================
    // Favorite（重點改這裡）
    // =========================
    DirectoryParts.applyFavorite = function (content, data) {
        const $content = $(content);

        const isLogin = typeof w.islogin !== "undefined"
            ? w.islogin
            : (typeof w.IsLogin !== "undefined" ? w.IsLogin : false);

        // ✅ 保留原條件（完全不動）
        if (!(data.type == 1 && isLogin)) {
            return;
        }

        // ✅ 優先：你 Member 自己的按鈕
        if ($content.find(".btn_favorite").length > 0) {
            const $btn = $content.find(".btn_favorite");

            if (data.fId != null) {
                $btn.addClass("check");
            } else {
                $btn.removeClass("check");
            }

            DirectoryParts.bindFavoriteButton($btn);
            return;
        }

        // ✅ slot 支援
        let $slot = $content.find("[data-slot='favorite'], .favorite-slot");

        if ($slot.length) {
            const html = `<button type="button" data-pid="${data.id}" class="btn_fav"></button>`;
            $slot.html(html);
            DirectoryParts.bindFavoriteButton($slot.find(".btn_fav"));
            return;
        }

        // ✅ fallback（舊版）
        DirectoryParts.renderFavoriteButton($content, data);
    };

    // ===== 原本 function 完整保留 =====
    DirectoryParts.renderFavoriteButton = function (content, data) {
        const $content = $(content);

        if ($content.find(".btn_fav").length === 0) {
            const html = `<button type="button" data-pid="${data.id}" class="btn_fav"></button>`;
            $content.find(".shareBlock").after(html);
        }

        const $btn = $content.find(".btn_fav");

        if ($content.find(".shareBlock").hasClass("d-none")) {
            $btn.addClass("d-none");
        } else if ($content.find(".shareBlock").hasClass("type5")) {
            $btn.addClass("type5");
        }

        if (data.fId != null) {
            $btn.data("fid", data.fId);
            $btn.addClass("check");
            $btn.attr("title", "移除收藏");
        } else {
            $btn.removeClass("check");
            $btn.attr("title", "加入收藏");
        }

        DirectoryParts.bindFavoriteButton($btn);
    };

    DirectoryParts.bindFavoriteButton = function ($btn) {
        $btn.off("click.directoryFav").on("click.directoryFav", function () {
            const $self = $(this);

            if (!$self.hasClass("check")) {
                if (!w.Coker || !Coker.Favorites || !isFn(Coker.Favorites.Add)) return;

                Coker.Favorites.Add($self.data("pid")).done(function (result) {
                    if (result.success) {
                        $self.data("fid", result.message);
                        $self.addClass("check");
                        $self.attr("title", "移除收藏");
                    }
                });

                return;
            }

            const fid = $self.data("fid");
            if (!fid) return;

            if (!w.Coker || !Coker.Favorites || !isFn(Coker.Favorites.Delete)) return;

            Coker.Favorites.Delete(fid).done(function (result) {
                if (result.success) {
                    $self.data("fid", "");
                    $self.removeClass("check");
                    $self.attr("title", "加入收藏");
                }
            });
        });
    };

    // ===== 其他完全不動 =====
    DirectoryParts.applyEmptyState = function ($item, result) {
        const $catalog = getCatalog($item);

        if (!Array.isArray(result) || result.length === 0) {
            $catalog.addClass("empty");
        } else {
            $catalog.removeClass("empty");
        }
    };

    DirectoryParts.applyHoverDetailsState = function ($item, result) {
        if (!$item.hasClass("hover_display_details") || typeof w.OrgName === "undefined") return;
        if (!Array.isArray(result) || result.length === 0) return;

        if (result[0].mainImage != "") {
            $item.data("default_img_link", result[0].mainImage);
            $item.find(".details_display").attr("src", result[0].mainImage);
        } else {
            $item.data("default_img_link", $item.find(".details_display").attr("src"));
        }
    };

    DirectoryParts.bindHoverDetails = function ($item) {
        if (!$item.hasClass("hover_display_details") || typeof w.OrgName === "undefined") return;

        $item.find(".details_display_frame").css("height", $item.find(".details_display_frame").height());

        $(w).off("resize.directoryHoverDetails").on("resize.directoryHoverDetails", function () {
            $item.find(".details_display_frame").css("height", $item.find(".details_display_frame").height());
        });

        if ($item.find(".details_display_frame").css("display") === "none") return;

        $item.find(".catalog > div:not(.templatecontent)")
            .off("mouseenter.directoryHoverDetails mouseleave.directoryHoverDetails")
            .on("mouseenter.directoryHoverDetails", function () {
                const $this = $(this);
                const newImg = $this.data("img_link");
                const $img = $item.find(".details_display");
                const nowImg = $img.attr("src");

                if (newImg !== nowImg && newImg !== "") {
                    $img.stop(true, true).fadeOut(200, function () {
                        $img.attr("src", newImg).fadeIn(100);
                    });
                }
            })
            .on("mouseleave.directoryHoverDetails", function () {
                const defaultImg = $item.data("default_img_link");
                const $img = $item.find(".details_display");
                const nowImg = $img.attr("src");

                if (defaultImg !== nowImg) {
                    $img.stop(true, true).fadeOut(200, function () {
                        $img.attr("src", defaultImg).fadeIn(100);
                    });
                }
            });
    };

    DirectoryParts.bindSwitchViewType = function ($item) {
        let pathname = w.location.pathname;

        if (pathname.indexOf("Search") > 0) {
            pathname = pathname.substring(0, pathname.indexOf("Search") + 6);
        } else {
            if (pathname.lastIndexOf("_") > 0) {
                const tempText = pathname.substring(pathname.lastIndexOf("_") + 1);
                if ($.isNumeric(tempText)) {
                    pathname = pathname.substring(0, pathname.lastIndexOf("_"));
                }
            }
        }

        if (typeof localStorage[`switchViewType-${pathname}`] !== "undefined" && $item.data("type") == "search") {
            const btnclass = localStorage[`switchViewType-${pathname}`];
            $(`button.${btnclass}`).trigger("click");
        }
    };

    DirectoryParts.afterRender = function ($item, result) {
        if (isFn(w.HoverEffectInit)) w.HoverEffectInit();
        if (isFn(w.ShareBlockInit)) w.ShareBlockInit();

        DirectoryParts.bindSwitchViewType($item);
        DirectoryParts.bindHoverDetails($item);
    };

})(window, window.jQuery);