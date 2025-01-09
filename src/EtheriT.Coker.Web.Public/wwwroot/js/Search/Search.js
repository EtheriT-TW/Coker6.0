function PageReady() {
    const $tabs = $(".search-category>.container>button");
    const $search = $(".search-category .catalog_frame");
    let timer = null;
    (async () => {
        const SearchDb = new LocalDb({
            apiUrl: "/api/Directory/GetSearchKeyList",
            dbName: "searchDB",
            storeName: "searchStore"
        });
        // 取得資料
        try {
            const data = await SearchDb.getData();
            // 監聽輸入框的鍵盤輸入事件
            $(".search-input").on("input", function () {
                const query = $(this).val().toLowerCase();
                const suggestions = data.filter(item => item.Key.toLowerCase().includes(query));

                const $suggestionsList = $(".search-suggestions");
                $suggestionsList.empty(); // 清空舊的建議項目

                if (suggestions.length > 0) {
                    suggestions.forEach(item => {
                        // 生成每個選項，添加 title 屬性
                        $suggestionsList.append(`<li><a href="#" title="搜尋：${item.Key}關鍵字">${item.Key}</a></li>`);
                    });
                    $suggestionsList.show(); // 顯示清單
                } else {
                    $suggestionsList.hide(); // 如果沒有符合的結果，隱藏清單
                }
            });

            // 當用戶選擇提示項目時
            $(document).on("click", ".search-suggestions a", function (event) {
                event.preventDefault(); // 關閉默認的超連結行為
                const selectedKey = $(this).text();
                $search.data("search-text", selectedKey);
                $(".search-input").val(selectedKey);
                $(".search-suggestions").hide(); // 隱藏提示清單
                $(".search-suggestions a").removeClass("highlighted");
            });

            // 鍵盤操作，支持上下鍵選擇和 Enter 鍵確認
            let highlightedIndex = -1; // 當前高亮的項目索引

            $(".search-input").on("keydown", function (e) {
                const $items = $(".search-suggestions a");
                if (!$items.length) return;

                const $currentHighlighted = $items.filter(".highlighted");
                let currentIndex = $items.index($currentHighlighted);

                switch (e.key) {
                    case "ArrowDown": // 向下鍵
                        e.preventDefault();
                        updateHighlightedItem((currentIndex + 1) % $items.length);
                        break;

                    case "ArrowUp": // 向上鍵
                        e.preventDefault();
                        updateHighlightedItem((currentIndex - 1 + $items.length) % $items.length);
                        break;

                    case "Enter": // 選擇項目
                        // 如果目前高亮的項目存在並且按下的是 Enter，停止事件冒泡
                        const $highlighted = $(".search-suggestions a.highlighted");
                        if (e.key === "Enter" && $highlighted.length > 0) {
                            e.preventDefault(); // 防止輸入框觸發多餘的行為
                            $highlighted.trigger("click"); // 觸發選項的點擊事件
                            return;
                        }

                        // 否則處理 Enter 鍵作為輸入框的觸發行為
                        if (e.key === "Enter") {
                            $(this).trigger("change");
                        }
                    case "Tab": // 支援 Tab 切換
                        if (currentIndex >= 0) {
                            $items.eq(currentIndex).focus();
                        }
                        break;
                }
            });

            // 更新高亮項目
            function updateHighlightedItem(newIndex) {
                const $items = $(".search-suggestions a");
                const $container = $(".search-suggestions"); // 包含滾動條的容器

                if (newIndex < 0 || newIndex >= $items.length) return;

                // 移除之前的高亮樣式
                $items.removeClass("highlighted");

                // 新的高亮項目
                const $newHighlighted = $items.eq(newIndex);
                $newHighlighted.addClass("highlighted");

                // 確保新的高亮項目位於容器中間
                const itemOffsetTop = $newHighlighted[0].offsetTop; // 項目距離容器頂部的距離
                const itemHeight = $newHighlighted.outerHeight(); // 項目的高度
                const containerScrollTop = $container.scrollTop(); // 容器的滾動位置
                const containerHeight = $container.innerHeight(); // 容器的高度

                // 計算新的滾動位置，確保高亮項目居中
                const newScrollTop =
                    itemOffsetTop - containerHeight / 2 + itemHeight / 2;

                // 更新容器滾動條位置
                $container.scrollTop(newScrollTop);
            }
        } catch (error) {
            console.error("取得資料時發生錯誤:", error);
        }
    })();
    $tabs.on("click", function () {
        //$tabs.removeClass("active");
        //$(this).addClass("active");
        //$search.data("searchID", $(this).data("id"));
        window.location.href = `/${OrgName}/Search/Get/${$(this).data("id")}/${$search.data("search-text")}`;
    });
    $(".search-category .catalog_frame [name='startDate']").on("change", function () {
        $search.data("startDate", $(this).val());
    });
    $(".search-category .catalog_frame [name='endDate']").on("change", function () {
        $search.data("endDate", $(this).val());
    });
    $(".search-category .search-input").on("change", function () {
        $search.data("search-text", $(this).val());
        window.location.href = `/${OrgName}/Search/Get/${$search.data("dirid")}/${$search.data("search-text")}`;
    });
    $(".search-category .catalog_frame").on("load", function () {
        const $fram = $(this);
        const temp = $("#filterTemp").html();
        const $traget = $("#filterList");
        const filter = $(this).data("filter");
        const dirTypeTemp = $("#filterDirTypeTemp").html();
        const $tragetDir = $("#filterBlock > .filterDirType > ul");
        $traget.empty();
        $tragetDir.empty();
        $fram.data("directoryType").unshift({ id: 0, name: "全部" })

        $("#filterBlock .fa-close").on("click", function () {
            $("#filterBlock").removeClass("active");
            $("body").removeClass("modal-open");
        });
        $(".btn_filter_grid").on("click", function () {
            $("#filterBlock").addClass("active");
            $("body").addClass("modal-open");
        });
        $($fram.data("directoryType")).each((i, e) => {
            const $item = $(dirTypeTemp);
            if (typeof ($fram.data("directoryTypeChecked")) != "undefined" && $fram.data("directoryTypeChecked") == e.id)
                $("#filterBlock > .filterDirType > a").text(e.name);
            $item.find("a").data(e).text(e.name).on("click", function () {
                $fram.data("directoryTypeChecked", $(this).data("id"));
                $("#filterBlock > .filterDirType > a").text($(this).data("name"));
                $fram.trigger("filter");
                return false;
            });
            $tragetDir.append($item);
        });
        $(filter).each((i, element) => {
            const $item = $(temp);
            const tid = `filterType${element.type}`;
            const $tagTarget = $item.find("ul");
            const tagTemp = $tagTarget.html();
            $tagTarget.empty();
            const $input = $item.find(".filterType>.form-check-input").data("type", element.type).attr("id", tid).on("change", function () {
                const $self = $(this);
                if ($self.prop("checked")) $self.parents("li").find("li .form-check-input").prop("checked", true).trigger("change");
                else $self.parents("li").find("li .form-check-input").prop("checked", false).trigger("change");
            });
            $item.find(".filterType>label").attr("for", tid)
            $(element.tags).each((ii, tag) => {
                const $tag = $(tagTemp);
                const tagId = `filterTag${element.type}-${tag.fK_TId}`;
                if (tag.count <= 0) return;
                const $tagInput = $tag.children(".form-check-input").data({ type: element.type, gid: element.id, id: tag.fK_TId }).attr("id", tagId).on("change", function () {
                    const $self = $(this);
                    clearTimeout(timer);
                    if (typeof ($fram.data("filtered")) == "undefined") $fram.data("filtered", []);
                    if ($self.prop("checked")) {
                        filteredPush($fram.data("filtered"), $self.data());
                    } else filteredPop($fram.data("filtered"), $self.data());
                    timer = setTimeout(function () {
                        $fram.trigger("filter");
                    }, 1000);
                });
                $tag.children("label").attr("for", tagId);
                $tag.find(".tagName").text(tag.tag_Name);
                $tag.find(".count").text(tag.count);
                $tagTarget.append($tag);
                if (typeof ($fram.data("filtered")) != "undefined") {
                    const filters = $fram.data("filtered").filter(e => e.type == element.type);
                    if (filters.length != 0) {
                        const group = filters[0].group.filter(e => e.id == element.id);
                        if (group.length != 0 && group[0].tags.includes(tag.fK_TId)) {
                            $tagInput.prop("checked", true).trigger("change");
                        }
                    }
                }
            })
            if (typeof ($fram.data("filtered")) != "undefined") {
                const filters = $fram.data("filtered").filter(e => e.type == element.type);
                if (filters.length != 0) {
                    const group = filters[0].group.filter(e => e.id == element.id);
                    if (group.length != 0 && group[0].tags.length == element.tags.length)
                        $input.prop("checked", true).trigger("change");
                }
            }
            $item.find(".typeName").text(element.name);
            if ($tagTarget.children("li").length > 0) $traget.append($item);
        });
        if ($(".search-category .active").data("id") != "3" || $(".searchText").text().trim() == "") $("#filterBlock").addClass("d-none");
        else $("#filterBlock").removeClass("d-none");
        clearTimeout(timer);
    })
    $(".searchbtn").on("click", function () {
        window.location.href = `/${OrgName}/Search/Get/${$search.data("dirid")}/${$search.data("search-text")}`;
    });
}
function filteredPush(data, item) {
    filteredInsertOrDeleted(data, item, true);
}
function filteredPop(data, item) {
    filteredInsertOrDeleted(data, item, false);
}
function filteredInsertOrDeleted(data, item, isAdd) {
    const filters = data.filter(e => e.type == item.type);
    let type = null;
    if (filters.length == 0) {
        type = {
            type: item.type,
            group: []
        };
        data.push(type);
    } else type = filters[0];
    const filterGroups = type.group.filter(e => e.id == item.gid);
    if (filterGroups.length == 0) {
        type.group.push({
            id: item.gid,
            tags: [item.id]
        });
    }
    else {
        const filterGroupTags = filterGroups[0].tags;
        if (filterGroupTags.includes(item.id)) {
            if (!isAdd) filterGroupTags.splice(filterGroupTags.indexOf(item.id), 1);
        } else if (isAdd) filterGroupTags.push(item.id);
    }
}