function PageReady() {
    const $tabs = $(".search-category>.container>button");
    const $search = $(".search-category .catalog_frame");
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
        const temp = $("#filterTemp").html();
        const $traget = $("#filterList");
        const filter = $(this).data("filter"); 
        $(filter).each((i, element) => {
            const $item = $(temp);
            const tid = `filterType${element.type}`;
            const $tagTarget = $item.find("ul");
            const tagTemp = $tagTarget.html();
            $tagTarget.empty();
            $item.children(".form-check-input").attr("id", tid)
            $item.children("label").attr("for", tid)
            $(element.tags).each((ii, tag) => {
                const $tag = $(tagTemp);
                const tagId = `filterTag${tag.fK_TId}`;
                $tag.children(".form-check-input").attr("id", tagId);
                $tag.children("label").attr("for", tagId);
                $tag.find(".tagName").text(tag.tag_Name);
                $tag.find(".count").text(tag.count);
                $tagTarget.append($tag);
            })
            $item.find(".typeName").text(element.name);
            $traget.append($item);
        });
    })
    $(".searchbtn").on("click", function () {
        window.location.href = `/${OrgName}/Search/Get/${$search.data("dirid") }/${$search.data("search-text")}`;
    });
    if ($(".search-category .active").data("id") != "3") $("#filterBlock").addClass("d-none");
    else $("#filterBlock").removeClass("d-none");
}