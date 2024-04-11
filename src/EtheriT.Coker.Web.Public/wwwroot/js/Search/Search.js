function PageReady() {
    const $tabs = $(".search-category>.container>button");
    const $search = $(".search-category .catalog_frame");
    let timer = null; 
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
        $traget.empty();
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
                        $input.prop("checked",true).trigger("change");
                }
            }
            $item.find(".typeName").text(element.name);
            $traget.append($item);
        });
        clearTimeout(timer);
    })
    $(".searchbtn").on("click", function () {
        window.location.href = `/${OrgName}/Search/Get/${$search.data("dirid") }/${$search.data("search-text")}`;
    });
    if ($(".search-category .active").data("id") != "3" || $(".searchText").text().trim()=="") $("#filterBlock").addClass("d-none");
    else $("#filterBlock").removeClass("d-none");
}
function filteredPush(data,item) {
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