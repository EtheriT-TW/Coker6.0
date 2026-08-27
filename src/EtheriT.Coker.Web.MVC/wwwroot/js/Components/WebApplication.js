var stationPageSize = 24;
var stationSearchDelay;
var stationSwitcherState = {
    loaded: false,
    loading: false,
    pageNow: 1,
    selectedId: null,
    pinnedIds: [],
    allWebs: [],
    filteredWebs: []
};

function SelectStationInit() {
    var $modal = $("#switchApp");
    $modal
        .off("shown.bs.modal.websiteSwitcher")
        .on("shown.bs.modal.websiteSwitcher", function () {
            if (!stationSwitcherState.loaded && !stationSwitcherState.loading) {
                StationLoadAll();
            }
        });
}

function StationLoadAll() {
    stationSwitcherState.loading = true;
    $(".website-load-error").addClass("d-none");
    $(".website-loading").removeClass("d-none");
    $(".app-switcher").empty();
    $(".page_btn").addClass("d-none").removeClass("d-flex");

    $.ajax({
        url: "/api/Website/GetSwitcherAll/",
        type: "GET",
        contentType: "application/json; charset=utf-8",
        headers: _c.Data.Header
    }).done(function (result) {
        stationSwitcherState.allWebs = Array.isArray(result) ? result : [];
        stationSwitcherState.allWebs.forEach(function (web, index) {
            web.stationOriginalOrder = index;
        });
        StationPinnedLoad();
        StationPinnedSort();
        stationSwitcherState.filteredWebs = stationSwitcherState.allWebs.slice();
        stationSwitcherState.pageNow = 1;
        stationSwitcherState.loaded = true;

        var selectedWeb = stationSwitcherState.allWebs.find(function (web) {
            return web.check;
        });
        if (selectedWeb) StationSetSelected(selectedWeb);
        else StationClearSelected();

        StationSearchInit();
        StationRender();
    }).fail(function () {
        stationSwitcherState.loaded = false;
        $(".website-load-error").removeClass("d-none");
        StationClearSelected();
    }).always(function () {
        stationSwitcherState.loading = false;
        $(".website-loading").addClass("d-none");
    });
}

function StationSearchInit() {
    var totalCount = stationSwitcherState.allWebs.length;
    var $searchPanel = $(".website-search-panel");

    if (totalCount <= 6) {
        $searchPanel.addClass("d-none");
        return;
    }

    $searchPanel.removeClass("d-none");
    var $searchBox = $("#websiteSearchBox");
    if (!$searchBox.hasClass("dx-textbox")) {
        $searchBox.dxTextBox({
            mode: "search",
            placeholder: "搜尋網站名稱、網站代碼、網址或公司名稱",
            showClearButton: true,
            valueChangeEvent: "input",
            onValueChanged: function (e) {
                clearTimeout(stationSearchDelay);
                stationSearchDelay = setTimeout(function () {
                    StationFilter(e.value);
                }, 250);
            }
        });
    }
}

function StationFilter(keyword) {
    var normalizedKeyword = StationNormalize(keyword);
    stationSwitcherState.filteredWebs = stationSwitcherState.allWebs.filter(function (web) {
        if (!normalizedKeyword) return true;

        var companyNames = Array.isArray(web.companyNames) ? web.companyNames.join(" ") : "";
        var searchableText = [
            web.name,
            web.defaultUrl,
            web.orgName,
            companyNames
        ].map(StationNormalize).join(" ");

        return searchableText.indexOf(normalizedKeyword) >= 0;
    });
    stationSwitcherState.pageNow = 1;
    StationRender();
}

function StationNormalize(value) {
    return String(value || "").trim().toLocaleLowerCase("zh-TW");
}

function StationPinnedStorageKey() {
    var account = StationNormalize($("#UserName").text());
    return "coker.websiteSwitcher.pinned." + encodeURIComponent(account);
}

function StationPinnedLoad() {
    var pinnedIds = [];
    try {
        var storedValue = JSON.parse(localStorage.getItem(StationPinnedStorageKey()) || "[]");
        if (Array.isArray(storedValue)) pinnedIds = storedValue;
    } catch {
        pinnedIds = [];
    }

    var accessibleIds = new Set(stationSwitcherState.allWebs.map(function (web) {
        return Number(web.id);
    }));
    stationSwitcherState.pinnedIds = pinnedIds
        .map(Number)
        .filter(function (websiteId, index, values) {
            return Number.isInteger(websiteId)
                && websiteId > 0
                && accessibleIds.has(websiteId)
                && values.indexOf(websiteId) === index;
        });
    StationPinnedSave();
}

function StationPinnedSave() {
    try {
        localStorage.setItem(
            StationPinnedStorageKey(),
            JSON.stringify(stationSwitcherState.pinnedIds)
        );
    } catch {
        // localStorage unavailable: keep the preference for the current page only.
    }
}

function StationPinnedSort() {
    var pinnedOrder = new Map(stationSwitcherState.pinnedIds.map(function (websiteId, index) {
        return [websiteId, index];
    }));
    stationSwitcherState.allWebs.sort(function (first, second) {
        var firstOrder = pinnedOrder.has(Number(first.id)) ? pinnedOrder.get(Number(first.id)) : Number.MAX_SAFE_INTEGER;
        var secondOrder = pinnedOrder.has(Number(second.id)) ? pinnedOrder.get(Number(second.id)) : Number.MAX_SAFE_INTEGER;
        if (firstOrder !== secondOrder) return firstOrder - secondOrder;
        return first.stationOriginalOrder - second.stationOriginalOrder;
    });
}

function StationPinnedToggle(websiteId) {
    websiteId = Number(websiteId);
    var pinnedIndex = stationSwitcherState.pinnedIds.indexOf(websiteId);
    if (pinnedIndex >= 0) stationSwitcherState.pinnedIds.splice(pinnedIndex, 1);
    else stationSwitcherState.pinnedIds.push(websiteId);

    StationPinnedSave();
    StationPinnedSort();

    var searchValue = $("#websiteSearchBox").hasClass("dx-textbox")
        ? $("#websiteSearchBox").dxTextBox("instance").option("value")
        : "";
    StationFilter(searchValue);
}

function StationPinnedVisualSet($frame, websiteId) {
    var isPinned = stationSwitcherState.pinnedIds.indexOf(Number(websiteId)) >= 0;
    var title = isPinned ? "取消置頂" : "置頂網站";
    $frame.find(".website-pin")
        .toggleClass("is-pinned", isPinned)
        .attr("title", title)
        .attr("aria-label", title)
        .attr("aria-pressed", String(isPinned));
}

function StationRender() {
    var filteredCount = stationSwitcherState.filteredWebs.length;
    var totalPages = Math.max(1, Math.ceil(filteredCount / stationPageSize));
    if (stationSwitcherState.pageNow > totalPages) stationSwitcherState.pageNow = totalPages;

    var startIndex = (stationSwitcherState.pageNow - 1) * stationPageSize;
    var pageWebs = stationSwitcherState.filteredWebs.slice(startIndex, startIndex + stationPageSize);

    StationPageSet(pageWebs);
    StationPageBtnSet(totalPages);
    StationResultCountSet(filteredCount);
    $(".website-no-result").toggleClass("d-none", filteredCount > 0);
}

function StationPageSet(webs) {
    var $switcher = $(".app-switcher").empty();

    webs.forEach(function (web) {
        var $frame = $($("#TemplateApp").html()).clone();
        var $card = $frame.find("[data-key='Id']");
        $card
            .data("id", web.id)
            .attr("data-id", web.id)
            .attr("title", web.description || web.name || "")
            .attr("aria-label", "選擇網站 " + (web.name || "未命名網站"))
            .on("click", function () {
                StationSetSelected(web);
            })
            .on("dblclick", function (e) {
                if ($(e.target).closest(".website-pin").length > 0) return;
                e.preventDefault();
                StationSetSelected(web);
                $("#switchApp .switch").trigger("click");
            })
            .on("keydown", function (e) {
                if ($(e.target).closest(".website-pin").length > 0) return;
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    StationSetSelected(web);
                }
            });
        $frame.find("[data-key='image']")
            .attr({ src: web.images || "/favicon.ico", alt: web.name || "網站圖示" })
            .on("error", function () { this.src = "/favicon.ico"; });
        $frame.find("[data-key='name']").text(web.name || "未命名網站");
        $frame.find("[data-key='description']").text(web.description || "");

        $frame.find(".website-pin")
            .on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
                StationPinnedToggle(web.id);
            })
            .on("dblclick", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
        StationPinnedVisualSet($frame, web.id);

        $switcher.append($frame);
    });

    StationSelectedVisualSet();
}

function StationSetSelected(web) {
    stationSwitcherState.selectedId = Number(web.id);
    $("#selectedWebsite")
        .addClass("active-app")
        .data("id", web.id)
        .attr("data-id", web.id)
        .find("[data-key='name']").text(web.name || "");
    $("#switchApp .switch").prop("disabled", false);
    StationSelectedVisualSet();
}

function StationClearSelected() {
    stationSwitcherState.selectedId = null;
    $("#selectedWebsite")
        .removeClass("active-app")
        .removeData("id")
        .attr("data-id", "")
        .find("[data-key='name']").text("");
    $("#switchApp .switch").prop("disabled", true);
    StationSelectedVisualSet();
}

function StationSelectedVisualSet() {
    $(".app-switcher .card").each(function () {
        var $card = $(this);
        var isSelected = Number($card.data("id")) === stationSwitcherState.selectedId;
        $card.toggleClass("active-app", isSelected);
        $card.find(".app-selected").remove();
        if (isSelected) {
            $card.find(".card-body").append(
                "<span class='material-symbols-outlined app-selected md-16'>check</span>"
            );
        }
    });
}

function StationPageBtnSet(totalPages) {
    var $pager = $(".page_btn").empty();
    if (totalPages <= 1) {
        $pager.addClass("d-none").removeClass("d-flex");
        return;
    }

    $pager.removeClass("d-none").addClass("d-flex");
    StationPageButtonAppend($pager, "上一頁", stationSwitcherState.pageNow - 1, "fa-solid fa-angle-left", stationSwitcherState.pageNow === 1);

    for (var page = 1; page <= totalPages; page++) {
        StationPageButtonAppend($pager, String(page), page, null, page === stationSwitcherState.pageNow, page === stationSwitcherState.pageNow);
    }

    StationPageButtonAppend($pager, "下一頁", stationSwitcherState.pageNow + 1, "fa-solid fa-angle-right", stationSwitcherState.pageNow === totalPages);
}

function StationPageButtonAppend($pager, title, page, iconClass, disabled, active) {
    var $item = $("<li>").addClass("page-item").toggleClass("active", !!active);
    var $button = $("<button>")
        .addClass("page-link")
        .attr("type", "button")
        .attr("title", title)
        .prop("disabled", disabled)
        .on("click", function () {
            stationSwitcherState.pageNow = page;
            StationRender();
        });

    if (iconClass) $button.append($("<i>").addClass(iconClass));
    else $button.text(title);

    $pager.append($item.append($button));
}

function StationResultCountSet(filteredCount) {
    if (stationSwitcherState.allWebs.length <= 6) return;

    var searchValue = $("#websiteSearchBox").hasClass("dx-textbox")
        ? $("#websiteSearchBox").dxTextBox("instance").option("value")
        : "";
    var text = StationNormalize(searchValue)
        ? "顯示 " + filteredCount + " / " + stationSwitcherState.allWebs.length + " 個網站"
        : "共 " + stationSwitcherState.allWebs.length + " 個網站";
    $(".website-result-count").text(text);
}
