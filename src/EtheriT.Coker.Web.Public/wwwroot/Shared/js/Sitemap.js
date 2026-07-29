function SitemapInit($root) {
    var $scope = $root && $root.jquery ? $root : $(document);
    var $frames = $scope.is(".sitemap_hierarchical_frame")
        ? $scope
        : $scope.find(".sitemap_hierarchical_frame");

    if (!$frames.length) return;

    if (!window.Coker || !Coker.WebMenu || typeof Coker.WebMenu.getMainMenu !== "function") {
        console.error("[Sitemap] Coker.WebMenu.getMainMenu is required.");
        return;
    }

    Coker.WebMenu.getMainMenu()
        .done(function (result) {
            CreateSitemap(result && Array.isArray(result.maps) ? result.maps : [], $frames);
        })
        .fail(function (xhr, status, error) {
            console.error("[Sitemap] Failed to load menu data:", status, error);
        });
}

// 前台共用初始化器使用 SiteMapInit；保留 SitemapInit 供後台編輯器相容呼叫。
function SiteMapInit($root) {
    return SitemapInit($root);
}

function CreateSitemap(result, $frames) {
    var $targets = $frames && $frames.jquery
        ? $frames
        : $(".sitemap_hierarchical_frame");

    $targets.each(function () {
        var $frame = $(this);
        var $firstUl = $('<ul class=""></ul>');

        // HTML 樣板本身可能已有空的 ul，重新初始化時一併清除，避免內容重複。
        $frame.empty().append($firstUl);

        result.forEach(function (data, firstIndex) {
            var $first = cloneTemplate("#Hierarchical_First_Item");
            if (!$first.length) return;

            var firstNumber = firstIndex + 1;
            setMenuItem(
                $first.find(".first").first(),
                data,
                firstNumber + " " + (data.title || ""),
                "first d-block text-black px-3 rounded-3"
            );

            var children = getChildren(data);
            if (children.length) {
                $first.append('<ul class="ps-5 ps-md-0 col-md-8 py-1"></ul>');
                var $secondFrame = $first.children("ul").last();

                children.forEach(function (secondData, secondIndex) {
                    var $second = cloneTemplate("#Hierarchical_Second_Item");
                    if (!$second.length) return;

                    var secondNumber = firstNumber + "-" + (secondIndex + 1);
                    var $secondItem = $second.find(".second").first();
                    $secondItem = setMenuItem(
                        $secondItem,
                        secondData,
                        secondNumber + " " + (secondData.title || ""),
                        "second ps-4 ps-sm-3 position-relative align-items-center d-flex justify-content-between px-3 rounded-3 text-black",
                        true
                    );

                    var thirdChildren = getChildren(secondData);
                    if (thirdChildren.length) {
                        $secondItem.append('<span class="d-none d-md-flex material-symbols-outlined">navigate_next</span>');
                        $secondItem.removeClass("ps-4 ps-sm-3");
                        $second.append('<ul class="ps-5 ps-md-0 col-md-6"></ul>');
                        var $thirdFrame = $second.children("ul").last();

                        thirdChildren.forEach(function (thirdData, thirdIndex) {
                            var $third = cloneTemplate("#Hierarchical_Third_Item");
                            if (!$third.length) return;

                            var thirdNumber = secondNumber + "-" + (thirdIndex + 1);
                            setMenuItem(
                                $third.find(".third").first(),
                                thirdData,
                                thirdNumber + " " + (thirdData.title || ""),
                                "third d-block text-black px-3 rounded-3"
                            );
                            $thirdFrame.append($third);

                            var fourthChildren = getChildren(thirdData);
                            if ($("#Hierarchical_Fourth_Item").length && fourthChildren.length) {
                                $third.append('<ul class="ps-5 ps-md-5 col-md-6"></ul>');
                                var $fourthFrame = $third.children("ul").last();

                                fourthChildren.forEach(function (fourthData, fourthIndex) {
                                    var $fourth = cloneTemplate("#Hierarchical_Fourth_Item");
                                    if (!$fourth.length) return;

                                    setMenuItem(
                                        $fourth.find(".fourth").first(),
                                        fourthData,
                                        thirdNumber + "-" + (fourthIndex + 1) + " " + (fourthData.title || ""),
                                        "fourth"
                                    );
                                    $fourthFrame.append($fourth);
                                });
                            }
                        });
                    }

                    $secondFrame.append($second);
                });
            } else {
                $first.find(".material-symbols-outlined").remove();
            }

            if (firstIndex === result.length - 1) {
                $first.removeClass("border-bottom");
            }
            $firstUl.append($first);
        });
    });
}

function cloneTemplate(selector) {
    var html = $(selector).html();
    return html ? $(html.trim()).first().clone() : $();
}

function getChildren(data) {
    return data && Array.isArray(data.children) ? data.children : [];
}

function getMenuUrl(data) {
    if (!data) return "";

    var linkUrl = $.trim(data.linkUrl || "");
    if (linkUrl) return linkUrl;

    var routerName = $.trim(data.routerName || "");
    if (!data.hasContan || !routerName) return "";

    var orgName = $.trim(data.orgName || (typeof OrgName !== "undefined" ? OrgName : ""));
    return "/" + (orgName ? orgName + "/" : "") + routerName;
}

function setMenuItem($item, data, label, linkClass, showMobileExpandIcon) {
    if (!$item.length) return $item;

    var url = getMenuUrl(data);

    if (url) {
        var $link = $("<a></a>")
            .attr("href", url)
            .attr("title", (data && (data.text || data.title)) || "")
            .attr("class", linkClass);

        if (data && data.target) {
            $link.attr({ target: "_blank", rel: "noopener noreferrer" });
        }
        setMenuLabel($link, label, showMobileExpandIcon);
        $item.replaceWith($link);
        return $link;
    } else {
        var $text = $("<span></span>")
            .attr("class", linkClass)
            .removeClass("rounded-3");
        setMenuLabel($text, label, showMobileExpandIcon);
        $item.replaceWith($text);
        return $text;
    }
}

function setMenuLabel($element, label, showMobileExpandIcon) {
    if (!showMobileExpandIcon) {
        $element.text(label);
        return;
    }

    $element
        .append('<span class="d-flex d-md-none material-symbols-outlined position-absolute start-0">expand_more</span>')
        .append($("<span></span>").addClass("second p-sm-0 ps-2").text(label));
}
