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
    $(".searchbtn").on("click", function () {
        window.location.href = `/${OrgName}/Search/Get/${$search.data("dirid") }/${$search.data("search-text")}`;
    });
}